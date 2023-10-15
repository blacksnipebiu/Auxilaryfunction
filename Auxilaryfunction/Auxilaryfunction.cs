using Auxilaryfunction.Services;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using static Auxilaryfunction.Constant;
using static Auxilaryfunction.Services.GUIDraw;

namespace Auxilaryfunction
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Auxilaryfunction : BaseUnityPlugin
    {
        public const string GUID = "cn.blacksnipe.dsp.Auxilaryfunction";
        public const string NAME = "Auxilaryfunction";
        public const string VERSION = "2.1.0";
        public static string ErrorTitle = "辅助面板错误提示";
        public static GUIDraw guidraw;
        public static int stationindex = 4;
        public static int locallogic;
        public static int remotelogic = 2;
        public static int pointlayerid;
        public static int pointsignalid;
        public static List<int> assemblerpools = new List<int>();
        public static List<int> labpools = new List<int>();
        public static List<int> beltpools = new List<int>();
        public static List<int> monitorpools = new List<int>();
        public static List<int> ejectorpools = new List<int>();
        public static List<int> powergenGammapools = new List<int>();
        public static List<int> pointlayeridlist = new List<int>();
        public static List<int> stationpools = new List<int>();
        public static List<int> readyresearch = new List<int>();
        public float slowconstructspeed = 1;
        private float trashfunctiontime;
        public float autobuildtime;
        public static float batchnum = 1;
        public static float upsfix = 1;
        public static bool temp;
        public static bool simulatorrender;
        public static bool simulatorchanging;
        public static bool ready;
        public bool DysonBluePrint;
        public bool constructframe;
        public bool constructingshell;
        public bool constructshell;
        public bool slowconstruct = true;
        public bool onlysinglelayer = true;
        public bool firstStart = false;
        public bool startdrawdyson;
        public static bool ChangingQuickKey;
        public bool auto_setejector_start;
        public bool autoAddwarp_start;
        public bool autoAddFuel_start;
        public static bool GameDataImported;
        public static Dictionary<int, List<int>> EjectorDictionary;
        public static Player player => GameMain.mainPlayer;
        public static PlanetData LocalPlanet => GameMain.localPlanet;
        public static BuildingParameters buildingParameters;
        public static int[,] stationcopyItem = new int[5, 6];
        public Dictionary<int, List<List<int>>> shellnodeidlayer = new Dictionary<int, List<List<int>>>();
        public Dictionary<int, List<int[]>> framenodeidlayer = new Dictionary<int, List<int[]>>();
        public List<string> DysonSphereBluePrintNamelist = new List<string>();
        public List<int[]> framenodeid = new List<int[]>();
        public List<List<int>> shellnodeid = new List<List<int>>();
        public List<DysonSphereLayer> constructinglayer = new List<DysonSphereLayer>();

        public static ERecipeType pointeRecipetype = ERecipeType.None;
        public static bool closecollider;
        public static bool stopfactory;
        public static bool changeups;
        public static bool autobuildgetitem;
        public static readonly FieldInfo _uiGridSplit_sliderImg = AccessTools.Field(typeof(UIGridSplit), "sliderImg");
        public static readonly FieldInfo _uiGridSplit_valueText = AccessTools.Field(typeof(UIGridSplit), "valueText");

        #region 配置菜单
        public static ConfigEntry<bool> autoClearEmptyDyson;
        public static ConfigEntry<bool> closeplayerflyaudio;
        public static ConfigEntry<bool> autosetSomevalue_bool;
        public static ConfigEntry<bool> noscaleuitech_bool;
        public static ConfigEntry<bool> close_alltip_bool;
        public static ConfigEntry<bool> autowarpcommand;
        public static ConfigEntry<bool> autonavigation_bool;
        public static ConfigEntry<bool> autocleartrash_bool;
        public static ConfigEntry<bool> autoabsorttrash_bool;
        public static ConfigEntry<bool> stationcopyItem_bool;
        public static ConfigEntry<bool> BluePrintDelete;
        public static ConfigEntry<bool> BluePrintRevoke;
        public static ConfigEntry<bool> BluePrintSetRecipe;
        public static ConfigEntry<bool> BluePrintSelectAll;
        public static ConfigEntry<bool> norender_shipdrone_bool;
        public static ConfigEntry<bool> norender_lab_bool;
        public static ConfigEntry<bool> norender_beltitem;
        public static ConfigEntry<bool> norender_dysonshell_bool;
        public static ConfigEntry<bool> norender_dysonswarm_bool;
        public static ConfigEntry<bool> norender_entity_bool;
        public static ConfigEntry<bool> norender_powerdisk_bool;
        public static ConfigEntry<bool> autoaddtech_bool;
        public static ConfigEntry<bool> Quickstop_bool;
        public static ConfigEntry<bool> automovetounbuilt;
        public static ConfigEntry<bool> upsquickset;
        public static ConfigEntry<bool> onlygetbuildings;
        public static ConfigEntry<bool> autosavetimechange;
        public static ConfigEntry<bool> auto_supply_station;
        public static ConfigEntry<bool> auto_setejector_bool;
        public static ConfigEntry<bool> ShowStationInfo;
        public static ConfigEntry<bool> KeepBeltHeight;
        public static ConfigEntry<bool> autoAddFuel;
        public static ConfigEntry<bool> autoAddwarp;
        public static ConfigEntry<double> stationwarpdist;
        public static ConfigEntry<bool> DysonPanelSingleLayer;
        public static ConfigEntry<bool> DysonPanelLayers;
        public static ConfigEntry<bool> DysonPanelSwarm;
        public static ConfigEntry<bool> DysonPanelDysonSphere;
        public static ConfigEntry<KeyboardShortcut> QuickKey;
        public static ConfigEntry<int> auto_add_techid;
        public static ConfigEntry<int> auto_add_techmaxLevel;
        public static ConfigEntry<int> auto_supply_Courier;
        public static ConfigEntry<int> stationdronedist;
        public static ConfigEntry<int> autosavetime;
        public static ConfigEntry<int> stationshipdist;
        public static ConfigEntry<int> auto_supply_starfuel;
        public static ConfigEntry<int> auto_supply_drone;
        public static ConfigEntry<int> auto_supply_ship;
        public static ConfigEntry<int> auto_supply_warp;
        public static ConfigEntry<int> veincollectorspeed;
        public static ConfigEntry<bool> SaveLastOpenBluePrintBrowserPathConfig;
        public static ConfigEntry<float> window_width;
        public static ConfigEntry<float> window_height;
        public static ConfigEntry<float> stationmaxpowerpertick;
        public static ConfigEntry<float> autowarpdistance;
        public static ConfigEntry<float> DroneStartCarry;
        public static ConfigEntry<float> ShipStartCarry;
        public static ConfigEntry<string> FuelFilterConfig;
        public static ConfigEntry<string> LastOpenBluePrintBrowserPathConfig;
        #endregion


        public static Thread autobuildThread;

        public static int maxCount = 0;
        public static GameObject[] tip = new GameObject[maxCount];
        public static GameObject stationTip;
        public static GameObject tipPrefab;
        public static GameObject beltWindow;
        public static GameObject SpeakerPanel;

        void Start()
        {
            var harmony = new Harmony(GUID);
            harmony.PatchAll(typeof(PlayerOperation));
            harmony.PatchAll(typeof(AuxilaryfunctionPatch));
            AuxilaryTranslate.regallTranslate();
            trashfunctiontime = Time.time;

            {
                QuickKey = Config.Bind("打开窗口快捷键", "Key", new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftAlt));
                tempShowWindow = QuickKey.Value;
                auto_setejector_bool = Config.Bind("自动配置太阳帆弹射器", "auto_setejector_bool", false);

                auto_supply_station = Config.Bind("自动配置新运输站", "auto_supply_station", false);
                auto_supply_Courier = Config.Bind("自动填充配送机", "auto_supply_Courier", 10);
                auto_supply_drone = Config.Bind("自动填充飞机数量", "auto_supply_drone", 10);
                auto_supply_ship = Config.Bind("自动填充飞船数量", "auto_supply_ship", 5);
                stationmaxpowerpertick = Config.Bind("自动设置最大充电功率", "autowarpdistance", 30f);
                stationwarpdist = Config.Bind("自动设置物流站使用翘曲器距离", "stationwarpdist", 12.0);
                DroneStartCarry = Config.Bind("自动设置物流站小飞机起送量", "DroneStartCarry", 0.1f);
                ShipStartCarry = Config.Bind("自动设置物流站运输船起送量", "ShipStartCarry", 1f);
                veincollectorspeed = Config.Bind("大型采矿机采矿速度", "veincollectorspeed", 10);
                auto_supply_warp = Config.Bind("自动设置物流站翘曲器数量", "auto_supply_warp", 0);
                stationdronedist = Config.Bind("自动设置物流站运输机最远距离", "stationdronedist", 180);
                stationshipdist = Config.Bind("自动设置物流站运输船最远距离", "stationshipdist", 61);
                scale = Config.Bind("大小适配", "scale", 16);

                closeplayerflyaudio = Config.Bind("关闭玩家飞行声音", "closeplayerflyaudio", false);
                BluePrintDelete = Config.Bind("蓝图删除", "BluePrintDelete", false);
                BluePrintRevoke = Config.Bind("蓝图撤销", "BluePrintRevoke", false);
                BluePrintSelectAll = Config.Bind("蓝图全选", "BluePrintSelectAll", false);
                BluePrintSetRecipe = Config.Bind("蓝图配方", "BluePrintSetRecipe", false);
                stationcopyItem_bool = Config.Bind("物流站复制物品配方", "stationcopyItem_bool", false);

                autocleartrash_bool = Config.Bind("30s间隔自动清除垃圾", "autocleartrash_bool", false);
                autoabsorttrash_bool = Config.Bind("30s间隔自动吸收垃圾", "autoabsorttrash_bool", false);
                onlygetbuildings = Config.Bind("只回收建筑", "onlygetbuildings", false);

                autowarpcommand = Config.Bind("自动导航使用翘曲", "autowarpcommand", false);
                close_alltip_bool = Config.Bind("关掉所有提示", "close_alltip_bool", false);

                autoAddwarp = Config.Bind("自动添加翘曲器", "autoAddwarp", false);
                autoAddFuel = Config.Bind("自动添加燃料", "autoAddFuel", false);
                autonavigation_bool = Config.Bind("自动导航", "autonavigation_bool", false);
                autowarpdistance = Config.Bind("自动使用翘曲器距离", "autowarpdistance", 0f);
                autoaddtech_bool = Config.Bind("自动添加科技队列", "autoaddtech_bool", false);
                auto_add_techid = Config.Bind("自动添加科技队列科技ID", "auto_add_techid", 0);
                auto_add_techmaxLevel = Config.Bind("自动添加科技队列科技等级上限", "auto_add_techmaxLevel", 500);
                ShowStationInfo = Config.Bind("展示物流站信息", "ShowStationInfo", false);
                SaveLastOpenBluePrintBrowserPathConfig = Config.Bind("记录上次蓝图路径", "SaveLastOpenBluePrintBrowserPathConfig", false);

                noscaleuitech_bool = Config.Bind("科技页面不缩放", "noscaleuitech_bool", false);
                norender_shipdrone_bool = Config.Bind("不渲染飞机飞船", "norender_shipdrone_bool", false);
                norender_lab_bool = Config.Bind("不渲染研究室", "norender_lab_bool", false);
                norender_beltitem = Config.Bind("不渲染传送带货物", "norender_beltitem", false);
                norender_dysonshell_bool = Config.Bind("不渲染戴森壳", "norender_dysonshell_bool", false);
                norender_dysonswarm_bool = Config.Bind("不渲染戴森云", "norender_dysonswarm_bool", false);
                norender_entity_bool = Config.Bind("不渲染实体", "norender_entity_bool", false);
                norender_powerdisk_bool = Config.Bind("不渲染电网覆盖", "norender_powerdisk_bool", false);
                Quickstop_bool = Config.Bind("ctrl+空格开启暂停工厂和戴森球", "Quickstop_bool", false);
                automovetounbuilt = Config.Bind("自动走向未完成建筑", "automovetounbuilt", false);
                upsquickset = Config.Bind("快速设置逻辑帧倍数", "upsquickset", false);
                autosetSomevalue_bool = Config.Bind("自动配置建筑", "autosetSomevalue_bool", false);
                auto_supply_starfuel = Config.Bind("人造恒星自动填充燃料数量", "auto_supply_starfuel", 4);
                autosavetimechange = Config.Bind("自动保存", "autosavetimechange", false);
                KeepBeltHeight = Config.Bind("保持传送带高度", "KeepBeltHeight", false);
                autosavetime = Config.Bind("自动保存时间", "autosavetime", 25);
                FuelFilterConfig = Config.Bind("自动填充燃料", "FuelFilterConfig", "");
                autoClearEmptyDyson = Config.Bind("自动清除空戴森球", "autoClearEmptyDyson", false);
                DysonPanelSingleLayer = Config.Bind("单层壳列表", "DysonPanelSingleLayer", true);
                DysonPanelLayers = Config.Bind("多层壳列表", "DysonPanelLayers", true);
                DysonPanelSwarm = Config.Bind("戴森云列表", "DysonPanelSwarm", true);
                DysonPanelDysonSphere = Config.Bind("戴森壳列表", "DysonPanelDysonSphere", true);
                window_height = Config.Bind("窗口高度", "window_height", 660f);
                window_width = Config.Bind("窗口宽度", "window_width", 830f);
                LastOpenBluePrintBrowserPathConfig = Config.Bind("上次打开蓝图路径", "LastOpenBluePrintBrowserPathConfig", "");
            }


            scrollPosition[0] = 0;
            dysonBluePrintscrollPosition[0] = 0;
            foreach (var item in LDB.items.dataArray)
            {
                if (item.HeatValue > 0)
                {
                    fuelItems.Add(item.ID);
                    FuelFilter.Add(item.ID, false);
                }
            }
            var AuxilaryPanel = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Auxilaryfunction.auxilarypanel")).LoadAsset<GameObject>("AuxilaryPanel");
            var ui_AuxilaryPanelPanel = UnityEngine.Object.Instantiate(AuxilaryPanel, UIRoot.instance.overlayCanvas.transform);
            guidraw = new GUIDraw(Math.Max(5, Math.Min(scale.Value, 35)), ui_AuxilaryPanelPanel);
            DysonBluePrintDataService.LoadDysonBluePrintData();
            ThreadPool.QueueUserWorkItem(_ => SecondEvent());
        }

        void Update()
        {
            AutoSaveTimeChange();
            GameUpdate();
            ChangeQuickKeyMethod();
            guidraw.GUIUpdate();
            if (upsquickset.Value)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetKey(KeyCode.KeypadPlus)) upsfix += 0.01f;
                    if (Input.GetKey(KeyCode.KeypadMinus)) upsfix -= 0.01f;
                    if (upsfix < 0.01) upsfix = 0.01f;
                    else if (upsfix > 10) upsfix = 10;
                }
            }
        }

        private void OnGUI()
        {
            guidraw.Draw();
        }

        private void SecondEvent()
        {
            while (true)
            {
                if (!GameDataImported)
                    continue;
                if (auto_setejector_bool.Value)
                {
                    ResetEjector();
                }
                if (autoAddwarp.Value)
                {
                    AutoAddwarp();
                }
                if (autoAddFuel.Value)
                {
                    AutoAddFuel();
                }
                if (autoaddtech_bool.Value && auto_add_techid.Value > 0 && GameMain.history != null && GameMain.history.techQueueLength == 0)
                {
                    TechState techstate = GameMain.history.techStates[auto_add_techid.Value];
                    if (techstate.curLevel == techstate.maxLevel)
                    {
                        auto_add_techid.Value = 0;
                    }
                    else if (techstate.curLevel < auto_add_techmaxLevel.Value)
                    {
                        GameMain.history.EnqueueTech(auto_add_techid.Value);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void AutoSaveTimeChange()
        {
            if (autosavetimechange.Value && UIAutoSave.autoSaveTime != autosavetime.Value)
            {
                DSPGame.globalOption.autoSaveTime = autosavetime.Value;
                DSPGame.globalOption.Apply();
                UIAutoSave.autoSaveTime = autosavetime.Value;
            }
        }

        private void GameUpdate()
        {
            if (!GameDataImported)
            {
                firstStart = true;
                closecollider = true;
                StopAutoBuildThread();
                readyresearch = new List<int>();
                upsfix = 1;
                auto_add_techid.Value = 0;
                blueprintopen = false;
                simulatorrender = false;
            }
            else
            {
                if (firstStart)
                {
                    firstStart = false;
                    ready = true;
                    trashfunctiontime = Time.time;
                    ReLoadEjectorDic();
                }
                else
                {
                    StartAndStopGame();
                    BluePrintoptimize();
                    TrashFunction();
                    EnqueueTech();
                }

            }
        }

        /// <summary>
        /// 将文字科技树入列
        /// </summary>
        private void EnqueueTech()
        {
            for (int i = 0; i < readyresearch.Count && GameMain.history.techQueueLength < 7; i++)
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

        /// <summary>
        /// 每次加载存档，重载发射器
        /// </summary>
        private void ReLoadEjectorDic()
        {
            EjectorDictionary = new Dictionary<int, List<int>>();
            if (GameMain.galaxy != null && GameMain.galaxy.stars != null)
            {
                foreach (StarData sd in GameMain.galaxy.stars)
                {
                    foreach (PlanetData pd in sd.planets)
                    {
                        if (pd.factory == null || pd.factory.factorySystem == null) continue;
                        FactorySystem fs = pd.factory.factorySystem;
                        List<int> tempec = new List<int>();
                        int index = 0;
                        foreach (EjectorComponent ec in fs.ejectorPool)
                        {
                            if (ec.id > 0 && ec.entityId > 0)
                            {
                                int protoId = fs.factory.entityPool[ec.entityId].protoId;
                                tempec.Add(index);
                            }
                            index++;
                        }
                        if (tempec.Count > 0)
                            EjectorDictionary[pd.id] = tempec;
                    }
                }
            }
        }

        /// <summary>
        /// 添加飞机飞船翘曲器
        /// </summary>
        public static void AddDroneShipToStation()
        {
            if (LocalPlanet == null || LocalPlanet.type == EPlanetType.Gas) return;
            int autoCourierValue = auto_supply_Courier.Value;
            int autoShipValue = auto_supply_ship.Value;
            int autoDroneValue = auto_supply_drone.Value;
            int autoWrapValue = auto_supply_warp.Value;
            foreach (var dc in LocalPlanet.factory.transport.dispenserPool)
            {
                if (dc == null) continue;

                int needCourierNum = autoCourierValue - dc.idleCourierCount - dc.workCourierCount;
                if (needCourierNum > 0)
                    dc.idleCourierCount += player.package.TakeItem(5003, needCourierNum, out _);
            }
            foreach (StationComponent sc in LocalPlanet.factory.transport.stationPool)
            {
                if (sc == null || sc.isVeinCollector) continue;

                int dronelimit = sc.isStellar ? 100 : 50;
                int tempdrone = autoDroneValue > dronelimit ? dronelimit : autoDroneValue;
                int needDroneNum = tempdrone - sc.workDroneCount - sc.idleDroneCount;
                if (needDroneNum > 0)
                    sc.idleDroneCount += player.package.TakeItem(5001, needDroneNum, out _);

                int needShipNum = autoShipValue - sc.idleShipCount - sc.workShipCount;
                if (sc.isStellar && needShipNum > 0)
                    sc.idleShipCount += player.package.TakeItem(5002, needShipNum, out _);

                int needWrapNum = autoWrapValue - sc.warperCount;
                if (needWrapNum > 0)
                    sc.warperCount += player.package.TakeItem(1210, needWrapNum, out _);
            }
        }

        /// <summary>
        /// 设置所有大型采矿机配置
        /// </summary>
        public static void ChangeAllVeinCollectorSpeedConfig()
        {
            if (LocalPlanet == null || LocalPlanet.type == EPlanetType.Gas) return;

            foreach (StationComponent sc in LocalPlanet.factory.transport.stationPool)
            {
                if (sc == null || !sc.isVeinCollector) continue;
                LocalPlanet.factory.factorySystem.minerPool[sc.minerId].speed = veincollectorspeed.Value * 1000;
                //LocalPlanet.factory.powerSystem.consumerPool[sc.pcId].workEnergyPerTick = (long)((double)LDB.items.Select((int)LocalPlanet.factory.entityPool[sc.entityId].protoId).prefabDesc.workEnergyPerTick * (veincollectorspeed.Value / 10.0) * (veincollectorspeed.Value / 10.0));
            }
        }

        /// <summary>
        /// 设置所有物流站参数
        /// </summary>
        public static void ChangeAllStationConfig()
        {
            if (LocalPlanet == null || LocalPlanet.factory == null || LocalPlanet.factory.transport == null || LocalPlanet.factory.transport.stationPool == null) return;
            foreach (StationComponent sc in LocalPlanet.factory.transport.stationPool)
            {
                if (sc == null || sc.isVeinCollector) continue;

                sc.tripRangeDrones = Math.Cos(stationdronedist.Value * Math.PI / 180);
                LocalPlanet.factory.powerSystem.consumerPool[sc.pcId].workEnergyPerTick = (long)stationmaxpowerpertick.Value * 16667;
                if (stationmaxpowerpertick.Value > 60 && !sc.isStellar)
                {
                    LocalPlanet.factory.powerSystem.consumerPool[sc.pcId].workEnergyPerTick = (long)60 * 16667;
                }
                sc.deliveryDrones = (int)(DroneStartCarry.Value * 10) * 10;

                if (sc.isStellar)
                {
                    sc.warpEnableDist = stationwarpdist.Value * GalaxyData.AU;
                    sc.deliveryShips = (int)(ShipStartCarry.Value * 10) * 10;
                    sc.tripRangeShips = stationshipdist.Value > 60 ? 24000000000 : stationshipdist.Value * 2400000;
                }
            }
        }

        /// <summary>
        /// 铺40个轨道采集器
        /// </summary>
        public static void SetGasStation()
        {
            if (LocalPlanet == null || LocalPlanet.type != EPlanetType.Gas)
                return;
            PlanetFactory pf = LocalPlanet.factory;
            if (pf != null)
            {
                foreach (StationComponent sc in pf.transport.stationPool)
                {
                    if (sc != null && sc.isCollector)
                    {
                        UIMessageBox.Show("铺满轨道采集器失败", "当前星球存在采集器，请先清空所有建筑", "确定", 3);
                        return;
                    }
                }
            }
            StorageComponent package = player.package;
            int sum = 0;
            for (int index = 0; index < package.size; ++index)
            {
                if (package.grids[index].itemId == 2105)
                {
                    sum += package.grids[index].count;
                    if (sum >= 40)
                        break;
                }
            }
            if (sum >= 40) package.TakeItem(2105, 40, out _);
            else
            {
                UIMessageBox.Show("铺满轨道采集器失败", "背包里没有40个采集器，请重新准备", "确定", 3);
                return;
            }
            for (int i = 0; i < 40; i++)
            {
                Vector3 pos;
                double posx = 0;
                double posz = 0;
                if (i % 10 == 0)
                {
                    if (i < 20)
                    {
                        posx = (i == 0 ? posxz[0, 0] : -posxz[0, 0]);
                        posz = (i == 0 ? posxz[0, 1] : -posxz[0, 1]);
                    }
                    else
                    {
                        posx = (i == 20 ? posxz[0, 1] : -posxz[0, 1]);
                        posz = (i == 20 ? posxz[0, 0] : -posxz[0, 0]);
                    }
                }
                else if (0 < i && i < 10)
                {
                    posx = posxz[i, 0];
                    posz = posxz[i, 1];
                }
                else if (i > 10 && i < 20)
                {
                    posx = -posxz[i - 10, 0];
                    posz = posxz[i - 10, 1];
                }
                else if (i > 20 && i < 30)
                {
                    posx = posxz[i - 20, 0];
                    posz = -posxz[i - 20, 1];
                }
                else if (i > 30)
                {
                    posx = -posxz[i - 30, 0];
                    posz = -posxz[i - 30, 1];
                }
                pos = new Vector3((float)(posx * player.planetData.realRadius), 0, (float)(posz * player.planetData.realRadius));
                Vector3 vector3_3 = 0.025f * pos.normalized * player.planetData.realRadius;
                Quaternion quaternion3 = Maths.SphericalRotation(pos, player.controller.actionBuild.clickTool.yaw);
                pos += vector3_3;
                PrebuildData prebuild = new PrebuildData
                {
                    protoId = 2105,
                    modelIndex = 117,
                    pos = pos + quaternion3 * Vector3.zero,
                    pos2 = pos + quaternion3 * Vector3.zero,
                    rot = quaternion3 * Quaternion.identity,
                    rot2 = quaternion3 * Quaternion.identity,
                    pickOffset = 0,
                    insertOffset = 0,
                    recipeId = 0,
                    filterId = 0,
                    paramCount = 0
                };
                prebuild.InitParametersArray(0);
                player.controller.actionBuild.clickTool.factory.AddPrebuildDataWithComponents(prebuild);
            }
        }

        /// <summary>
        /// 自动添加燃料到人造恒星
        /// </summary>
        public static void AddFuelToAllStar()
        {
            if (LocalPlanet == null) return;
            PlanetFactory fs = LocalPlanet.factory;
            foreach (PowerGeneratorComponent pgc in fs.powerSystem.genPool)
            {
                int inc;
                if (pgc.fuelMask == 4 && pgc.fuelCount < auto_supply_starfuel.Value)
                    fs.powerSystem.genPool[pgc.id].SetNewFuel(1803, (short)(player.package.TakeItem(1803, auto_supply_starfuel.Value - pgc.fuelCount, out inc) + pgc.fuelCount), (short)inc);
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
                if (BluePrintRevoke.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
                {
                    blue_copy.ClearSelection();
                    blue_copy.ClearPreSelection();
                    blue_copy.RefreshBlueprintData();
                    for (int i = 0; i < LocalPlanet.factory.prebuildPool.Length; i++)
                    {
                        int itemId = LocalPlanet.factory.prebuildPool[i].protoId;
                        if (LocalPlanet.factory.prebuildPool[i].itemRequired == 0)
                            player.TryAddItemToPackage(itemId, 1, 0, true);
                        LocalPlanet.factory.RemovePrebuildWithComponents(i);
                    }
                    pointeRecipetype = ERecipeType.None;
                }
                if (BluePrintSetRecipe.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
                {
                    recipewindowx = (int)Input.mousePosition.x;
                    recipewindowy = (int)Input.mousePosition.y;
                    var assemblerPreviewpools = new List<int>();

                    if (stationpools.Count + labpools.Count + assemblerpools.Count + beltpools.Count + monitorpools.Count + ejectorpools.Count + powergenGammapools.Count > 0)
                    {
                        InitBluePrintData();
                        return;
                    }
                    //检索全部需要设置配方的建筑
                    foreach (BuildPreview bp in blue_copy.bpPool)
                    {
                        if (bp != null && bp.item != null && bp.objId > 0)
                        {
                            var prefabDesc = bp.item.prefabDesc;
                            if (prefabDesc.isStation)
                            {
                                if (build.factory.entityPool[bp.objId].minerId > 0) continue;
                                stationpools.Add(build.factory.entityPool[bp.objId].stationId);
                            }
                            else if (prefabDesc.isAssembler)
                            {
                                assemblerPreviewpools.Add(bp.previewIndex);
                            }
                            else if (prefabDesc.isLab)
                            {
                                labpools.Add(build.factory.entityPool[bp.objId].labId);
                            }
                            else if (prefabDesc.isMonitor && build.factory.entityPool[bp.objId].monitorId > 0)
                            {
                                monitorpools.Add(build.factory.entityPool[bp.objId].monitorId);
                            }
                            else if (prefabDesc.isBelt && LocalPlanet.factory.entitySignPool[bp.objId].iconId0 > 0)
                            {
                                beltpools.Add(build.factory.entityPool[bp.objId].id);
                            }
                            else if (prefabDesc.isEjector)
                            {
                                ejectorpools.Add(build.factory.entityPool[bp.objId].ejectorId);
                            }
                            else if (prefabDesc.isPowerGen)
                            {
                                powergenGammapools.Add(build.factory.entityPool[bp.objId].powerGenId);
                            }
                        }
                    }
                    //组装机优先，其他次之，传送带再次之，检测器最后
                    if (assemblerPreviewpools.Count > 0)
                    {
                        foreach (var previewIndex in assemblerPreviewpools)
                        {
                            var bp = blue_copy.bpPool[previewIndex];
                            if (bp.item.prefabDesc.assemblerRecipeType != ERecipeType.None)
                            {
                                if (pointeRecipetype == ERecipeType.None) pointeRecipetype = bp.item.prefabDesc.assemblerRecipeType;
                                if (pointeRecipetype != bp.item.prefabDesc.assemblerRecipeType)
                                {
                                    pointeRecipetype = ERecipeType.None;
                                    assemblerpools = new List<int>();
                                    break;
                                }
                                assemblerpools.Add(build.factory.entityPool[bp.objId].assemblerId);
                            }
                        }
                    }
                    else if (labpools.Count > 0 || ejectorpools.Count > 0 || stationpools.Count > 0 || powergenGammapools.Count > 0)
                    {
                        //无需操作
                    }
                    else if (beltpools.Count > 0)
                    {
                        if (!beltWindow.gameObject.activeSelf)
                        {
                            int tempsignaliconid = (int)LocalPlanet.factory.entitySignPool[beltpools[0]].iconId0;
                            int tempnumberinput = (int)LocalPlanet.factory.entitySignPool[beltpools[0]].count0;
                            beltWindow.gameObject.SetActive(true);
                            pointsignalid = tempsignaliconid;
                            beltWindow.transform.Find("item-sign").GetComponent<Image>().sprite = LDB.signals.IconSprite(tempsignaliconid);
                            beltWindow.transform.Find("number-input").GetComponent<InputField>().text = tempnumberinput + "";
                        }
                        else
                        {
                            beltpools.Clear();
                            beltWindow.gameObject.SetActive(false);
                            if (UISignalPicker.isOpened)
                                UISignalPicker.Close();
                        }
                    }
                    else if (monitorpools.Count > 0)
                    {
                        if (!SpeakerPanel.gameObject.activeSelf)
                        {
                            int speakerId = LocalPlanet.factory.cargoTraffic.monitorPool[monitorpools[0]].speakerId;
                            SpeakerComponent speakerComponent = LocalPlanet.factory.digitalSystem.speakerPool[speakerId];
                            SpeakerPanel.transform.Find("speaker-panel").GetComponent<UISpeakerPanel>().pitchSlider.value = speakerComponent.pitch;
                            SpeakerPanel.transform.Find("speaker-panel").GetComponent<UISpeakerPanel>().volumeSlider.value = speakerComponent.volume;
                            SpeakerPanel.transform.Find("speaker-panel").GetComponent<UISpeakerPanel>().toneCombo.itemIndex = speakerComponent.tone > 0 ? SpeakerPanel.transform.Find("speaker-panel").GetComponent<UISpeakerPanel>().toneCombo.ItemsData.IndexOf(speakerComponent.tone) : 0;

                            SpeakerPanel.gameObject.SetActive(true);
                            SpeakerPanel.transform.Find("speaker-panel").gameObject.SetActive(true);
                        }
                        else
                        {
                            monitorpools.Clear();
                            SpeakerPanel.gameObject.SetActive(false);
                            SpeakerPanel.transform.Find("speaker-panel").gameObject.SetActive(false);
                        }
                    }
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
            else if (stationpools.Count + labpools.Count + assemblerpools.Count + beltpools.Count + monitorpools.Count + ejectorpools.Count + powergenGammapools.Count > 0)
            {
                InitBluePrintData();
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
        /// 停止自动寻找建筑的进程
        /// </summary>
        public static void StopAutoBuildThread()
        {
            closecollider = !closecollider;
            autobuildgetitem = false;
            if (autobuildThread != null)
            {
                if (autobuildThread.ThreadState == ThreadState.WaitSleepJoin)
                    autobuildThread.Interrupt();
                else
                    autobuildThread.Abort();
                autobuildThread = null;
            }
        }

        public static void InitBluePrintData()
        {
            pointeRecipetype = ERecipeType.None;
            stationpools = new List<int>();
            assemblerpools = new List<int>();
            labpools = new List<int>();
            beltpools = new List<int>();
            monitorpools = new List<int>();
            powergenGammapools = new List<int>();
            ejectorpools = new List<int>();
            beltWindow.gameObject.SetActive(false);
            SpeakerPanel.gameObject.SetActive(false);
            SpeakerPanel.transform.Find("speaker-panel").gameObject.SetActive(false);
            if (UISignalPicker.isOpened)
                UISignalPicker.Close();
        }

        /// <summary>
        /// 修改快捷键
        /// </summary>
        private void ChangeQuickKeyMethod()
        {
            if (ChangeQuickKey)
            {
                setQuickKey();
                ChangingQuickKey = true;
            }
            else if (!ChangeQuickKey && ChangingQuickKey)
            {
                QuickKey.Value = tempShowWindow;
                ChangingQuickKey = false;
            }
        }

        /// <summary>
        /// 开始或者暂停游戏
        /// </summary>
        private void StartAndStopGame()
        {
            if (Quickstop_bool.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Space))
            {
                stopfactory = !stopfactory;
            }
        }

        /// <summary>
        /// 垃圾处理相关函数
        /// </summary>
        private void TrashFunction()
        {
            if (Time.time - trashfunctiontime < 30)
                return;
            trashfunctiontime = Time.time;
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
        /// 自动添加燃料
        /// </summary>
        private void AutoAddFuel()
        {
            if (player?.mecha != null && player.mecha.reactorStorage.isEmpty)
            {
                foreach (var itemID in fuelItems)
                {
                    if (!FuelFilter[itemID] || player.package.GetItemCount(itemID) == 0)
                    {
                        continue;
                    }
                    var item = LDB.items.Select(itemID);
                    int tempitemID = itemID;
                    int count = item.StackSize;
                    while (!player.mecha.reactorStorage.isFull)
                    {
                        player.package.TakeTailItems(ref tempitemID, ref count, out int inc);
                        if (tempitemID <= 0 || count <= 0)
                        {
                            break;
                        }
                        player.mecha.reactorStorage.AddItem(itemID, count, inc, out _);
                        if (player.mecha.reactorStorage.isFull || player.mecha.reactorStorage.GetItemCount(itemID) > 100000) return;
                    }
                }
            }
        }

        /// <summary>
        /// 自动设置太阳帆弹射器
        /// </summary>
        private void ResetEjector()
        {
            if (GameMain.galaxy == null || GameMain.data == null || GameMain.data.dysonSpheres == null || EjectorDictionary == null || EjectorDictionary.Count == 0) return;

            foreach (int pdid in EjectorDictionary.Keys)
            {
                PlanetData pd = GameMain.galaxy.PlanetById(pdid);
                if (pd == null || pd.factory == null || pd.factory.factorySystem == null || GameMain.data.dysonSpheres[pd.star.index] == null || GameMain.data.dysonSpheres[pd.star.index].swarm == null || GameMain.data.dysonSpheres[pd.star.index].swarm.orbits.Length <= 0) continue;

                DysonSwarm swarm = GameMain.data.dysonSpheres[pd.star.index].swarm;
                int swarmorbitcursor = swarm.orbitCursor;
                List<int> temp = EjectorDictionary[pdid];
                FactorySystem fs = pd.factory.factorySystem;
                foreach (int ec in temp)
                {
                    if (fs.ejectorPool != null && fs.ejectorPool.Length > ec && fs.ejectorPool[ec].id != 0 && fs.ejectorPool[ec].targetState != EjectorComponent.ETargetState.OK && swarmorbitcursor > 1)
                    {
                        int pointorbitid = fs.ejectorPool[ec].orbitId + 1;
                        while (!swarm.OrbitEnabled(pointorbitid))
                        {
                            pointorbitid++;
                            if (pointorbitid >= swarmorbitcursor) pointorbitid = 1;
                        }
                        fs.ejectorPool[ec].SetOrbit(pointorbitid);
                    }
                }
            }
        }

        private void setQuickKey()
        {
            bool left = true;
            int[] result = new int[2];
            if (Input.GetKey(KeyCode.LeftShift) && left)
            {
                left = false;
                result[0] = 304;
            }
            if (Input.GetKey(KeyCode.LeftControl) && left)
            {
                left = false;
                result[0] = 306;
            }
            if (Input.GetKey(KeyCode.LeftAlt) && left)
            {
                left = false;
                result[0] = 308;
            }
            bool right = true;
            for (int i = (int)KeyCode.Alpha0; i <= (int)KeyCode.Alpha9 && right; i++)
            {
                if (Input.GetKey((KeyCode)i))
                {
                    result[1] = i;
                    right = false;
                    break;
                }
            }
            for (int i = (int)KeyCode.A; i <= (int)KeyCode.Z && right; i++)
            {
                if (Input.GetKey((KeyCode)i))
                {
                    result[1] = i;
                    right = false;
                    break;
                }
            }
            for (int i = (int)KeyCode.F1; i <= (int)KeyCode.F10 && right; i++)
            {
                if (Input.GetKey((KeyCode)i))
                {
                    result[1] = i;
                    right = false;
                    break;
                }
            }
            if (left && right) { }
            else if (!left && !right) tempShowWindow = new KeyboardShortcut((KeyCode)result[1], (KeyCode)result[0]);
            else
            {
                tempShowWindow = new KeyboardShortcut((KeyCode)Math.Max(result[0], result[1]));
            }
        }
    }


}
