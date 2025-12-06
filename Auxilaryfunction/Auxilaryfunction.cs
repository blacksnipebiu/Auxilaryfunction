using Auxilaryfunction.Models;
using Auxilaryfunction.Patch;
using Auxilaryfunction.Services;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using static Auxilaryfunction.AuxConfig;
using static Auxilaryfunction.Services.GUIDraw;

namespace Auxilaryfunction;

[BepInPlugin(GUID, NAME, VERSION)]
public class Auxilaryfunction : BaseUnityPlugin
{
    private SynchronizationContext _mainContext;
    public static bool autobuildgetitem;
    public static int automovetoPrebuildSecondElapseCounter;
    public static bool autoRemoveRuin;
    public static int autoRemoveRuinId = -1;
    public static bool GameDataImported;
    public static GUIDraw guidraw;
    public static bool HideDarkFogAssaultTip;
    public static bool HideDarkFogMonitor;
    public static int maxCount = 0;
    public static Player player;
    public static List<int> readyresearch;
    public static bool StartAutoMovetoDarkfog;
    public static bool StartAutoMovetounbuilt;
    public static int[,] stationcopyItem = new int[5, 6];
    public static GameObject stationTipRoot;
    public static Light SunLight;
    public static GameObject tipPrefab;
    public static GameObject TrashCanButton;
    public static StorageComponent TrashStorage;
    public static GameObject TrashStorageWindow;
    public static UIStorageWindow uiTrashStorageWindow;
    public bool firstStart;


    public void Start()
    {
        _mainContext = SynchronizationContext.Current;
        harmony = new Harmony(GUID);
        harmony.PatchAll(typeof(SkipRenderPatch));
        harmony.PatchAll(typeof(AuxilaryfunctionPatch));
        AuxilaryTranslate.regallTranslate();
        BindAll(Config);
        if (closeplayerflyaudio.Value)
        {
            PlayerAudioMutePatch.Enable = true;
        }
        if (CloseMilestone.Value)
        {
            originUnlockVolumn = LDB.audios["unlock-1"].Volume;
            LDB.audios["unlock-1"].Volume = 0;
        }
        //里程碑声音关闭
        CloseMilestone.SettingChanged += (t, e) =>
        {
            LDB.audios["unlock-1"].Volume = CloseMilestone.Value ? 0 : originUnlockVolumn;
            Debug.Log(LDB.audios["unlock-1"].Volume);
        };
        closeplayerflyaudio.SettingChanged += (t, e) =>
        {
            PlayerAudioMutePatch.Enable = closeplayerflyaudio.Value;
        };
        if (autonavigation_bool.Value)
        {
            PlayerOperationPatch.Enable = true;
        }
        autonavigation_bool.SettingChanged += (t, e) =>
        {
            PlayerOperationPatch.Enable = autonavigation_bool.Value;
        };
        if (SunLightOpen.Value)
        {
            SunLightPatch.Enable = SunLightOpen.Value;
        }
        SunLightOpen.SettingChanged += (t, e) =>
        {
            SunLightPatch.Enable = SunLightOpen.Value;
        };
        scrollPosition[0] = 0;
        dysonBluePrintscrollPosition[0] = 0;

        if (QuickKey.Value.MainKey == KeyCode.Alpha2 && QuickKey.Value.Modifiers.Count() == 1 && QuickKey.Value.Modifiers.ElementAt(0) == KeyCode.LeftAlt)
        {
            QuickKey.Value = new KeyboardShortcut(KeyCode.LeftAlt, KeyCode.Alpha2);
        }

        readyresearch = [];
        var AuxilaryPanel = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Auxilaryfunction.Resources.auxilarypanel")).LoadAsset<GameObject>("AuxilaryPanel");
        var ui_AuxilaryPanelPanel = Instantiate(AuxilaryPanel, UIRoot.instance.overlayCanvas.transform);
        guidraw = new GUIDraw(Math.Max(5, Math.Min(scale.Value, 35)), ui_AuxilaryPanelPanel);
        DysonBluePrintDataService.LoadDysonBluePrintData();
        ThreadPool.QueueUserWorkItem(_ => SecondEvent());
    }

    private static void FindItemAndMove(int itemId, int itemCount)
    {
        if (LocalPlanet == null || LocalPlanet.factory == null || LocalPlanet.gasItems != null) return;
        if (LDB.items == null || LDB.items.Select(itemId) == null) return;
        int packageGridLen = player.package.grids.Length;
        int stackMax;
        int stackSize = 0;
        if (player.package.grids[packageGridLen - 1].count != 0)
        {
            player.package.Sort();
        }
        for (int i = packageGridLen - 1; i >= 0; i--, stackSize++)
        {
            if (player.package.grids[i].count != 0) break;
        }
        stackMax = LDB.items.Select(itemId).StackSize * stackSize;
        itemCount = stackMax > itemCount ? itemCount : stackMax;
        PlanetFactory pf = LocalPlanet.factory;

        if (pf.transport != null && pf.transport.stationPool != null)
        {
            int result = 0;
            int resultinc = 0;
            foreach (StationComponent sc in pf.transport.stationPool)
            {
                if (itemCount == 0) break;
                if (sc == null || sc.storage == null) continue;
                int tempItemCount = itemCount;
                int tempItemId = itemId;
                sc.TakeItem(ref tempItemId, ref tempItemCount, out int inc);
                result += tempItemCount;
                itemCount -= tempItemCount;
                resultinc += inc;
            }
            player.TryAddItemToPackage(itemId, result, resultinc, false);
        }

        if (pf.factoryStorage != null && pf.factoryStorage.storagePool != null)
        {
            if (pf.factoryStorage.storagePool != null)
            {
                foreach (StorageComponent sc in pf.factoryStorage.storagePool)
                {
                    if (itemCount == 0) break;
                    if (sc != null)
                    {
                        int temp = sc.TakeItem(itemId, itemCount, out int inc);
                        itemCount -= temp;
                        if (temp > 0)
                        {
                            player.TryAddItemToPackage(itemId, temp, inc, false);
                        }
                    }
                }
            }
            if (pf.factoryStorage.tankPool != null)
            {
                foreach (TankComponent tc in pf.factoryStorage.tankPool)
                {
                    if (itemCount == 0) break;
                    if (tc.id > 0 && tc.fluidId > 0 && tc.fluidCount > 0 && itemId == tc.fluidId)
                    {
                        int tempItemCount = tc.fluidCount > itemCount ? itemCount : tc.fluidCount;

                        int inc = (int)(tc.fluidInc / tc.fluidCount + 0.5) * tempItemCount;
                        pf.factoryStorage.tankPool[tc.id].fluidCount -= tempItemCount;
                        pf.factoryStorage.tankPool[tc.id].fluidInc -= inc;

                        itemCount -= tempItemCount;
                        if (tempItemCount > 0)
                        {
                            player.TryAddItemToPackage(itemId, tempItemCount, inc, false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 自动添加翘曲器
    /// </summary>
    private void AutoAddwarp()
    {
        if (player?.mecha != null && player.mecha.thrusterLevel >= 3 && !player.mecha.HasWarper())
        {
            int itemID = 1210;
            int count = 20;
            player.package.TakeTailItems(ref itemID, ref count, out int inc);
            if (itemID <= 0 || count <= 0)
            {
                return;
            }
            player.mecha.warpStorage.AddItem(itemID, count, inc, out _);
        }
    }

    /// <summary>
    /// 自动移动到黑雾单位
    /// </summary>
    private void AutoMoveToDarkfog()
    {
        autoRemoveRuinId = -1;
        if (GameMain.localPlanet == null || GameMain.mainPlayer == null || GameMain.mainPlayer.movementState != EMovementState.Fly)
        {
            return;
        }
        if (!StartAutoMovetoDarkfog)
        {
            return;
        }
        EnemyDFGroundSystem enemySystem = GameMain.localPlanet.factory.enemySystem;
        float mindistance = 100000000;
        DFGBaseComponent baseComponent = null;
        for (int i = 1; i < enemySystem.bases.cursor; i++)
        {
            if (enemySystem.bases[i] != null && enemySystem.bases[i].id == i && (autoRemoveRuin || enemySystem.CheckBaseCanRemoved(i) != 0))
            {
                Vector3 pos = GameMain.localPlanet.factory.enemyPool[enemySystem.bases[i].enemyId].pos;
                if (baseComponent == null || mindistance > (pos - player.position).magnitude)
                {
                    baseComponent = enemySystem.bases[i];
                    mindistance = (pos - player.position).magnitude;
                }
            }
        }
        if (baseComponent == null)
        {
            return;
        }
        Vector3 targetPos = GameMain.localPlanet.factory.enemyPool[baseComponent.enemyId].pos;

        _mainContext.Post(_ =>
        {
            GameMain.mainPlayer.Order(OrderNode.MoveTo(targetPos.normalized * GameMain.localPlanet.realRadius), false);
        }, null);
        if (autoRemoveRuin && enemySystem.CheckBaseCanRemoved(baseComponent.id) == 0)
        {
            autoRemoveRuinId = baseComponent.ruinId;
        }
    }

    private void AutoMovetounbuilt()
    {
        if (!StartAutoMovetounbuilt)
        {
            return;
        }
        if (GameMain.localPlanet == null || GameMain.mainPlayer == null || GameMain.mainPlayer.movementState != EMovementState.Fly)
        {
            return;
        }
        automovetoPrebuildSecondElapseCounter++;
        if (automovetoPrebuildSecondElapseCounter < automovetoPrebuildSecondElapse.Value)
        {
            return;
        }
        automovetoPrebuildSecondElapseCounter -= (int)automovetoPrebuildSecondElapse.Value;
        //找到最近的未完成建筑
        float mindistance = 3000;
        int lasthasitempd = -1;
        foreach (PrebuildData pd in GameMain.localPlanet.factory.prebuildPool)
        {
            if (pd.id == 0 || pd.itemRequired > 0) continue;
            if (lasthasitempd == -1 || mindistance > (pd.pos - GameMain.mainPlayer.position).magnitude)
            {
                lasthasitempd = pd.id;
                mindistance = (pd.pos - GameMain.mainPlayer.position).magnitude;
            }
        }
        if (lasthasitempd == -1)
        {
            bool getitem = true;
            foreach (PrebuildData pd in GameMain.localPlanet.factory.prebuildPool)
            {
                if (pd.id == 0 || pd.protoId == 0) continue;
                if (GameMain.mainPlayer.package.GetItemCount(pd.protoId) > 0)
                {
                    lasthasitempd = pd.id;
                    getitem = false;
                    _mainContext.Post(_ =>
                    {
                        GameMain.mainPlayer.Order(OrderNode.MoveTo(GameMain.localPlanet.factory.prebuildPool[lasthasitempd].pos.normalized * GameMain.localPlanet.realRadius), false);

                    }, null);
                    break;
                }
            }
            if (getitem && autobuildgetitem)
            {
                int[] warningCounts = GameMain.data.warningSystem.warningCounts;
                WarningData[] warningpools = GameMain.data.warningSystem.warningPool;
                List<int> getItem = [];
                int stackSize = 0;
                int packageGridLen = player.package.grids.Length;
                for (int j = packageGridLen - 1; j >= 0 && player.package.grids[j].count == 0; j--, stackSize++) { }
                for (int i = 1; i < GameMain.data.warningSystem.warningCursor && stackSize > 0; i++)
                {
                    if (getItem.Contains(warningpools[i].detailId1)) continue;
                    if (player.package.GetItemCount(warningpools[i].detailId1) > 0) break;
                    getItem.Add(warningpools[i].detailId1);
                    FindItemAndMove(warningpools[i].detailId1, warningCounts[warningpools[i].signalId]);
                }
            }
        }
        else if (lasthasitempd != -1 && lasthasitempd != 0 && lasthasitempd == GameMain.localPlanet.factory.prebuildPool[lasthasitempd].id)
        {
            _mainContext.Post(_ =>
            {
                GameMain.mainPlayer.Order(OrderNode.MoveTo(GameMain.localPlanet.factory.prebuildPool[lasthasitempd].pos.normalized * GameMain.localPlanet.realRadius), false);

            }, null);
        }
    }

    private void AutoSaveTimeChange()
    {
        if (autosavetimechange.Value && UIAutoSave.autoSaveTime != autosavetime.Value)
        {
            Debug.Log("AutoSaveTimeChange");
            DSPGame.globalOption.autoSaveTime = autosavetime.Value;
            DSPGame.globalOption.Apply();
            UIAutoSave.autoSaveTime = autosavetime.Value;
        }
    }

    /// <summary>
    /// 蓝图操作优化
    /// </summary>
    private void BluePrintoptimize()
    {
        blueprintopen = false;
        //蓝图复制操作优化
        if (player?.controller?.actionBuild == null)
        {
            return;
        }
        PlayerAction_Build build = player.controller.actionBuild;
        if (build.blueprintCopyTool != null && build.blueprintCopyTool.active && build.blueprintCopyTool.bpPool != null)
        {
            blueprintopen = true;
            BuildTool_BlueprintCopy blue_copy = build.blueprintCopyTool;
            if (BluePrintSelectAll.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
            {
                foreach (var et in LocalPlanet.factory.entityPool)
                {
                    if (et.id == 0) continue;
                    blue_copy.preSelectObjIds.Add(et.id);
                    blue_copy.selectedObjIds.Add(et.id);
                }
                blue_copy.RefreshBlueprintData();
                blue_copy.DeterminePreviews();
            }
            if (BluePrintDelete.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
            {
                foreach (BuildPreview bp in blue_copy.bpPool)
                {
                    if (bp != null && bp.item != null && bp.objId > 0)
                    {
                        int stationId = LocalPlanet.factory.entityPool[bp.objId].stationId;
                        if (stationId > 0)
                        {
                            StationComponent sc = LocalPlanet.factory.transport.stationPool[stationId];
                            for (int i = 0; i < sc.storage.Length; i++)
                            {
                                int package = player.TryAddItemToPackage(sc.storage[i].itemId, sc.storage[i].count, 0, true, bp.objId);
                                UIItemup.Up(sc.storage[i].itemId, package);
                            }
                            sc.storage = new StationStore[sc.storage.Length];
                            sc.needs = new int[sc.needs.Length];
                        }
                        build.DoDismantleObject(bp.objId);
                    }
                }
                blue_copy.ClearSelection();
                blue_copy.ClearPreSelection();
                blue_copy.ResetBlueprint();
                blue_copy.ResetBuildPreviews();
                blue_copy.RefreshBlueprintData();
            }
        }
        //蓝图粘贴操作优化
        if (build.blueprintPasteTool != null && build.blueprintPasteTool.active && build.blueprintPasteTool.bpPool != null)
        {
            BuildTool_BlueprintPaste blue_paste = build.blueprintPasteTool;
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
            {
                blue_paste.ResetBuildPreviews();
                blue_paste.ResetStatesOnClose();
                blue_paste.RefreshBlueprintUI();
                player.controller.OpenBlueprintCopyMode();
            }
        }
    }

    /// <summary>
    /// 将文字科技树入列
    /// </summary>
    private void  EnqueueTech()
    {
        int len = readyresearch.Count;
        if (len > 0)
        {
            try
            {
                for (int i = 0; i < len && GameMain.history.techQueueLength < 7; i++)
                {
                    if (GameMain.history.TechUnlocked(readyresearch[i])) readyresearch.RemoveAt(i);
                    else if (!GameMain.history.TechInQueue(readyresearch[i]))
                    {
                        if (!GameMain.history.CanEnqueueTech(readyresearch[i]))
                        {
                            readyresearch.RemoveAt(i);
                        }
                        else GameMain.history.EnqueueTech(readyresearch[i]);
                    }
                }
            }
            catch { }
        }
    }

    private void GameUpdate()
    {
        if (!GameDataImported)
        {
            player = null;
            firstStart = true;
            readyresearch.Clear();
            SpeedUpPatch.SpeedMultiple = 1;
            blueprintopen = false;
            simulatorrender = false;
            PlayerOperationPatch.FlyStatus = false;
            PlayerOperationPatch.Enable = false;
        }
        else
        {
            if (GameMain.mainPlayer != null && player == null)
            {
                player = GameMain.mainPlayer;
                player.controller.actionInspect.onInspecteeChange += (_, b) =>
                {
                    if (b > 0)
                    {
                        guidraw.CloseTrashStorageWindow();
                    }
                };
                if (autonavigation_bool.Value)
                {
                    PlayerOperationPatch.Enable = true;
                }
            }
            if (firstStart)
            {
                firstStart = false;
                PlayerOperationPatch.ClearFollow();
                BluePrintBatchModifyBuild.Init();
            }
            else
            {
                StartAndStopGame();
                BluePrintoptimize();
                SunLightSet();

                if (autoRemoveRuin && autoRemoveRuinId >= 0 && player?.controller?.actionBuild?.reformTool != null)
                {
                    player.controller.actionBuild.reformTool.RemoveBasePit(autoRemoveRuinId);
                }
                autoRemoveRuinId = -1;
            }
        }
    }

    private void OnGUI()
    {
        guidraw.Draw();
    }

    private void SecondEvent()
    {
        int counter = 0;
        int autoaddtechid;
        while (true)
        {
            if (!GameDataImported)
            {
                Thread.Sleep(500);
                continue;
            }
            if (autoAddwarp.Value)
            {
                AutoAddwarp();
            }
            autoaddtechid = auto_add_techid.Value;
            if (autoaddtech_bool.Value && autoaddtechid > 0 && GameMain.history != null && GameMain.history.techQueueLength == 0)
            {
                TechState techstate = GameMain.history.techStates[autoaddtechid];
                if (techstate.curLevel == techstate.maxLevel)
                {
                    autoaddtechid = 0;
                    auto_add_techid.Value = 0;
                }
                else if (techstate.curLevel < auto_add_techmaxLevel.Value)
                {
                    if (guidraw.AutoUnlockTechInSandBox)
                    {
                        GameMain.history.UnlockTechUnlimited(autoaddtechid, true);
                    }
                    else
                    {
                        GameMain.history.EnqueueTech(autoaddtechid);
                    }
                }
            }
            if (automovetounbuilt.Value)
            {
                try
                {
                    AutoMovetounbuilt();
                }
                catch (Exception ex)
                {
                    UIMessageBox.Show(ErrorTitle.getTranslate(), ex.ToString(), "确定".Translate(), 3, null);
                }
            }
            if (automovetodarkfog.Value)
            {
                try
                {
                    AutoMoveToDarkfog();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            if (counter >= 30)
            {
                try { TrashFunction(); } catch { }
                counter = 0;
            }
            counter++;
            EnqueueTech();
            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 开始或者暂停游戏
    /// </summary>
    private void StartAndStopGame()
    {
        if (Quickstop_bool.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Space))
        {
            GameTickPatch.Enable = !GameTickPatch.Enable;
        }
    }

    private void SunLightSet()
    {
        if (!SunLightOpen.Value || GameMain.localPlanet == null || GameMain.universeSimulator?.LocalStarSimulator()?.sunLight == null)
        {
            if (SunLight != null)
            {
                Destroy(SunLight.gameObject);
                SunLight = null;
            }
            return;
        }
        if (SunLight == null)
        {
            SunLight = Instantiate(GameMain.universeSimulator.LocalStarSimulator().sunLight, GameMain.mainPlayer.transform);
            SunLight.enabled = true;
            SunLight.transform.position = GameMain.mainPlayer.position + GameMain.mainPlayer.transform.up * 5;
            SunLight.transform.rotation = Quaternion.LookRotation(-GameMain.mainPlayer.transform.up);
            SunLight.transform.localPosition = new Vector3(0, 5, 0);
            SunLight.name = "PlayerLight";
            SunLight.intensity = 1f;
        }
        if (FactoryModel.whiteMode0)
        {
            SunLight.enabled = false;
        }
        else
        {
            SunLight.enabled = true;
        }
    }

    /// <summary>
    /// 垃圾处理相关函数
    /// </summary>
    private void TrashFunction()
    {
        if (GameMain.data?.trashSystem == null)
        {
            return;
        }
        if (autoabsorttrash_bool.Value) //回收函数
        {
            var container = GameMain.data.trashSystem.container;
            for (int i = 0; i < container.trashCursor; i++)
            {
                int itemId = container.trashObjPool[i].item;
                if (itemId > 0 && container.trashObjPool[i].expire < 0)
                {
                    //如果是建筑则回收，不是则清除
                    if (onlygetbuildings.Value && !(LDB.items.Select(itemId)?.CanBuild ?? false))
                        container.RemoveTrash(i);
                    else
                        container.trashObjPool[i].expire = 35;
                }
            }
        }
        if (autocleartrash_bool.Value) //清除函数
            GameMain.data.trashSystem.ClearAllTrash();
    }

    void Update()
    {
        AutoSaveTimeChange();
        GameUpdate();
        HotkeyService.HandleQuickKeyChange();
        guidraw.GUIUpdate();
    }
}
