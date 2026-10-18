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
    // readyresearch 同时被后台线程（SecondEvent → EnqueueTech/AutoOrderAddTech）和 UI 线程访问，用它串行化
    public static readonly object ReadyResearchLock = new object();
    /// <summary>
    /// 文字科技树的“自动顺序添加”开关。仅供本次运行使用，刻意不做成配置项：
    /// 每次进游戏都是关闭状态，需要时在面板上勾一下。
    /// </summary>
    public static bool autoorderadd;
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
        try
        {
            // readyresearch 会被后台线程（本方法、AutoOrderAddTech）和 UI 线程（文字科技树点击、快照）同时访问，
            // 统一用 ReadyResearchLock 串行化，避免“集合已修改”之类的异常打断 OnGUI
            lock (ReadyResearchLock)
            {
                // 等级上限超过文字科技树范围（TechService.TechTreeMaxLevelLimit）的科技不再呈现，
                // 也就不该继续占着研究队列：旧版本留下的这种条目直接剔除。
                readyresearch.RemoveAll(TechService.IsBeyondTechTreeLimit);
                // 注意：移除元素后不能 i++，否则会跳过刚补位的那一项；用 i 只在“未移除”时前进
                for (int i = 0; i < readyresearch.Count && GameMain.history.techQueueLength < 7;)
                {
                    int techId = readyresearch[i];
                    if (GameMain.history.TechUnlocked(techId))
                    {
                        readyresearch.RemoveAt(i);
                        continue;
                    }
                    if (GameMain.history.TechInQueue(techId))
                    {
                        i++;
                        continue;
                    }
                    if (!GameMain.history.CanEnqueueTech(techId))
                    {
                        readyresearch.RemoveAt(i);
                        continue;
                    }
                    GameMain.history.EnqueueTech(techId);
                    i++;
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// “自动顺序添加”：把当前可以研究（前置科技全部解锁）且还没进列表的科技加入“准备研究”，
    /// 随后由 EnqueueTech() 依次入队。入列位置由 CompareTechPriority 决定（见那里）。
    /// </summary>
    private void AutoOrderAddTech()
    {
        if (GameMain.history == null) return;
        foreach (int techId in TechService.CollectResearchableTechIds(false, limitmaterial))
        {
            AddTechToReady(techId);
        }
        foreach (int techId in TechService.CollectResearchableTechIds(true, limitmaterial))
        {
            AddTechToReady(techId);
        }
    }

    /// <summary>
    /// 把科技补进“准备研究”：按优先级插到合适位置，而不是简单追加
    /// （列表只增不改序，直接追加会让先入列的贵科技一直挡着后面新出现的便宜科技）。
    /// </summary>
    private void AddTechToReady(int techId)
    {
        if (GameMain.history.TechInQueue(techId)) return;
        lock (ReadyResearchLock)
        {
            if (readyresearch.Contains(techId)) return;
            readyresearch.Insert(InsertIndexFor(techId), techId);
        }
    }

    /// <summary>
    /// 新科技插在“第一个优先级比它低的项”之前；优先级相同的排在后面（保持先来后到）。
    /// </summary>
    private static int InsertIndexFor(int techId)
    {
        for (int i = 0; i < readyresearch.Count; i++)
        {
            if (CompareTechPriority(readyresearch[i], techId) > 0) return i;
        }
        return readyresearch.Count;
    }

    /// <summary>
    /// 自动顺序添加的优先级，返回 &lt;0 表示 a 先入列（先研究）：
    /// 1) 多级/无限升级科技（MaxLevel≥20）垫底 —— 一个上万级的科技排在前面会把研究队列一直堵住；
    /// 2) 所需矩阵档次低的先入列 —— 只要 6001 的排在要 6004 的前面；
    /// 3) 档次相同时，需要矩阵种类少的先入列 —— 只要 6004 的排在要 6001~6004 的前面；
    /// 4) 矩阵要求一致时，普通科技（ID≤2000）优先于升级科技（ID&gt;2000）；
    /// 5) 再比矩阵消耗总量，少的先入列；
    /// 6) 仍然并列就保持原有先后。
    /// </summary>
    private static int CompareTechPriority(int a, int b)
    {
        int ua = IsUnboundedLevelTech(a) ? 1 : 0;
        int ub = IsUnboundedLevelTech(b) ? 1 : 0;
        if (ua != ub) return ua - ub;

        TechProto pa = LDB.techs.Select(a);
        TechProto pb = LDB.techs.Select(b);
        int tierA = TechService.MatrixTier(pa);
        int tierB = TechService.MatrixTier(pb);
        if (tierA != tierB) return tierA - tierB;

        int kindA = TechService.MatrixTypeCount(pa);
        int kindB = TechService.MatrixTypeCount(pb);
        if (kindA != kindB) return kindA - kindB;

        // 矩阵要求一致：先“科技”，后“升级”
        int upgradeA = pa != null && pa.ID > 2000 ? 1 : 0;
        int upgradeB = pb != null && pb.ID > 2000 ? 1 : 0;
        if (upgradeA != upgradeB) return upgradeA - upgradeB;

        long costA = TechService.MatrixCost(pa, GameMain.history.techStates[a].curLevel);
        long costB = TechService.MatrixCost(pb, GameMain.history.techStates[b].curLevel);
        if (costA != costB) return costA < costB ? -1 : 1;
        return 0;
    }

    /// <summary>多级/无限升级科技（MaxLevel ≥ 分界值），这类必须排最后</summary>
    private static bool IsUnboundedLevelTech(int techId)
    {
        TechProto tp = LDB.techs.Select(techId);
        return tp != null && tp.MaxLevel >= TechService.FiniteUpgradeMaxLevel;
    }

    private void GameUpdate()
    {
        if (!GameDataImported)
        {
            player = null;
            firstStart = true;
            lock (ReadyResearchLock) { readyresearch.Clear(); }
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
            if (autoorderadd)
            {
                try { AutoOrderAddTech(); } catch { }
            }
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
