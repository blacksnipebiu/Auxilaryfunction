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
using System.Text;
using System.Threading;
using UnityEngine;
using static Auxilaryfunction.Constant;
using static Auxilaryfunction.Services.GUIDraw;

namespace Auxilaryfunction
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Auxilaryfunction : BaseUnityPlugin
    {
        public const string GUID = "cn.blacksnipe.dsp.Auxilaryfunction";
        public const string NAME = "Auxilaryfunction";
        public const string VERSION = "3.0.3";
        private SynchronizationContext _mainContext;
        public static int autoaddtechid;
        public static bool autobuildgetitem;
        public static Thread autobuildThread;
        public static int automovetoPrebuildSecondElapseCounter;
        public static bool autoRemoveRuin;
        public static int autoRemoveRuinId = -1;
        public static float batchnum = 1;
        public static BuildingParameters buildingParameters;
        public static bool ChangingBluePrintQuickKey;
        public static bool ChangingQuickKey;
        public static string ErrorTitle = "辅助面板错误提示";
        public static bool GameDataImported;
        public static GUIDraw guidraw;
        public static Harmony harmony;
        public static bool HideDarkFogAssaultTip;
        public static bool HideDarkFogMonitor;
        public static int locallogic;
        public static int maxCount = 0;
        public static Player player;
        public static List<int> pointlayeridlist;
        public static int pointsignalid;
        public static List<int> readyresearch;
        public static int remotelogic = 2;
        public static bool simulatorchanging;
        public static bool simulatorrender;
        public static bool StartAutoMovetoDarkfog;
        public static bool StartAutoMovetounbuilt;
        public static int[,] stationcopyItem = new int[5, 6];
        public static int stationindex = 4;
        public static RectTransform stationTipRectTransform;
        public static GameObject stationTipRoot;
        public static StationTip[] stationtips = new StationTip[maxCount];
        public static GameObject StationWindow;
        public static Light SunLight;
        public static bool temp;
        public static GameObject tipPrefab;
        public static GameObject TrashCanButton;
        public static StorageComponent TrashStorage;
        public static GameObject TrashStorageWindow;
        public static UIStationWindow uiStationWindow;
        public static UIStorageWindow uiTrashStorageWindow;
        public bool firstStart;
        private float trashfunctiontime;
        public static PlanetData LocalPlanet => GameMain.localPlanet;

        #region 配置菜单

        public static ConfigEntry<int> auto_add_techid;
        public static ConfigEntry<int> auto_add_techmaxLevel;
        public static ConfigEntry<int> auto_supply_Courier;
        public static ConfigEntry<int> auto_supply_drone;
        public static ConfigEntry<int> auto_supply_ship;
        public static ConfigEntry<int> auto_supply_starfuel;
        public static ConfigEntry<int> auto_supply_starfuelID;
        public static ConfigEntry<bool> auto_supply_station;
        public static ConfigEntry<int> auto_supply_warp;
        public static ConfigEntry<bool> autoabsorttrash_bool;
        public static ConfigEntry<bool> autoAddPlayerVel;
        public static ConfigEntry<bool> autoaddtech_bool;
        public static ConfigEntry<bool> autoAddwarp;
        public static ConfigEntry<bool> autocleartrash_bool;
        public static ConfigEntry<bool> automovetodarkfog;
        public static ConfigEntry<float> automovetoPrebuildSecondElapse;
        public static ConfigEntry<bool> automovetounbuilt;
        public static ConfigEntry<bool> AutoNavigateToDarkFogHive;
        public static ConfigEntry<int> AutoNavigateToDarkFogHiveKeepDistance;
        public static ConfigEntry<bool> autonavigation_bool;
        public static ConfigEntry<int> autosavetime;
        public static ConfigEntry<bool> autosavetimechange;
        public static ConfigEntry<bool> autosetCourier_bool;
        public static ConfigEntry<bool> autosetSomevalue_bool;
        public static ConfigEntry<bool> autosetstationconfig;
        public static ConfigEntry<bool> autowarpcommand;
        public static ConfigEntry<float> autowarpdistance;
        public static ConfigEntry<float> autowarpdistanceEnergyPercent;
        public static ConfigEntry<bool> BluePrintDelete;
        public static ConfigEntry<bool> BluePrintSelectAll;
        public static ConfigEntry<KeyboardShortcut> BluePrintShowWindow;
        public static ConfigEntry<bool> CloseMilestone;
        public static ConfigEntry<bool> closeplayerflyaudio;
        public static ConfigEntry<bool> CloseUIAdvisor;
        public static ConfigEntry<bool> CloseUIRandomTip;
        public static ConfigEntry<bool> CloseUITutorialTip;
        public static ConfigEntry<bool> CloseUIGeneralTip;
        public static ConfigEntry<float> DroneStartCarry;
        public static ConfigEntry<bool> DysonPanelDysonSphere;
        public static ConfigEntry<bool> DysonPanelLayers;
        public static ConfigEntry<bool> DysonPanelSingleLayer;
        public static ConfigEntry<bool> DysonPanelSwarm;
        public static ConfigEntry<bool> KeepBeltHeight;
        public static ConfigEntry<string> LastOpenBluePrintBrowserPathConfig;
        public static ConfigEntry<bool> norender_beltitem;
        public static ConfigEntry<bool> norender_DarkFog;
        public static ConfigEntry<bool> norender_dysonshell_bool;
        public static ConfigEntry<bool> norender_dysonswarm_bool;
        public static ConfigEntry<bool> norender_entity_bool;
        public static ConfigEntry<bool> norender_lab_bool;
        public static ConfigEntry<bool> norender_powerdisk_bool;
        public static ConfigEntry<bool> norender_shipdrone_bool;
        public static ConfigEntry<bool> noscaleuitech_bool;
        public static ConfigEntry<bool> onlygetbuildings;
        public static ConfigEntry<KeyboardShortcut> QuickKey;
        public static ConfigEntry<bool> Quickstop_bool;
        public static ConfigEntry<bool> SaveLastOpenBluePrintBrowserPathConfig;
        public static ConfigEntry<float> ShipStartCarry;
        public static ConfigEntry<bool> ShowStationInfo;
        public static ConfigEntry<bool> ShowStationInfoMode;
        public static ConfigEntry<bool> stationcopyItem_bool;
        public static ConfigEntry<int> stationdronedist;
        public static ConfigEntry<float> stationmaxpowerpertick;
        public static ConfigEntry<int> stationshipdist;
        public static ConfigEntry<double> stationwarpdist;
        public static ConfigEntry<bool> SunLightOpen;
        public static ConfigEntry<bool> TrashStorageWindow_bool;
        public static ConfigEntry<bool> upsquickset;
        public static ConfigEntry<int> veincollectorspeed;
        public static ConfigEntry<float> window_height;
        public static ConfigEntry<float> window_width;

        #endregion 配置菜单

        /// <summary>
        /// 自动添加配送运输机
        /// </summary>
        public static void AddCourierToAllStar()
        {
            if (LocalPlanet == null) return;
            int autoCourierValue = auto_supply_Courier.Value;
            foreach (var dc in LocalPlanet.factory.transport.dispenserPool)
            {
                if (dc == null) continue;

                int needCourierNum = autoCourierValue - dc.idleCourierCount - dc.workCourierCount;
                if (needCourierNum > 0)
                    dc.idleCourierCount += player.package.TakeItem(5003, needCourierNum, out _);
            }
        }

        /// <summary>
        /// 添加飞机飞船翘曲器
        /// </summary>
        public static void AddDroneShipToStation()
        {
            if (LocalPlanet == null || LocalPlanet.type == EPlanetType.Gas) return;
            int autoShipValue = auto_supply_ship.Value;
            int autoDroneValue = auto_supply_drone.Value;
            int autoWrapValue = auto_supply_warp.Value;
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
        /// 自动添加燃料到人造恒星
        /// </summary>
        public static void AddFuelToAllStar()
        {
            if (LocalPlanet == null) return;
            PlanetFactory fs = LocalPlanet.factory;
            int fuelId = auto_supply_starfuelID.Value;
            if (fuelId < 1803 || fuelId > 1804)
            {
                return;
            }
            foreach (PowerGeneratorComponent pgc in fs.powerSystem.genPool)
            {
                if (pgc.fuelMask == 4 && pgc.fuelCount < auto_supply_starfuel.Value)
                {
                    int needNum = auto_supply_starfuel.Value - pgc.fuelCount;
                    short fuelcount = ((short)player.package.TakeItem(fuelId, needNum, out int inc));
                    if (fuelcount == 0)
                    {
                        return;
                    }
                    if (pgc.fuelCount == 0)
                    {
                        fs.powerSystem.genPool[pgc.id].SetNewFuel(fuelId, fuelcount, (short)inc);
                    }
                    else if (pgc.fuelCount < auto_supply_starfuel.Value && fuelId == pgc.fuelId)
                    {
                        fs.powerSystem.genPool[pgc.id].fuelCount += fuelcount;
                        fs.powerSystem.genPool[pgc.id].fuelInc += (short)inc;
                    }
                }
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
                sc.deliveryDrones = DroneStartCarry.Value == 0 ? 1 : (int)(DroneStartCarry.Value * 10) * 10;

                if (sc.isStellar)
                {
                    sc.warpEnableDist = stationwarpdist.Value * GalaxyData.AU;
                    sc.deliveryShips = ShipStartCarry.Value == 0 ? 1 : (int)(ShipStartCarry.Value * 10) * 10;
                    sc.tripRangeShips = stationshipdist.Value > 60 ? 24000000000 : stationshipdist.Value * 2400000;
                }
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
                if (pf.prebuildCount > 0)
                {
                    UIMessageBox.Show("铺满轨道采集器失败", "当前星球存在采集器，请先清空所有建筑", "确定", 3);
                    return;
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

        public void Start()
        {
            _mainContext = SynchronizationContext.Current;
            harmony = new Harmony(GUID);
            harmony.PatchAll(typeof(AuxilaryfunctionPatch));
            AuxilaryTranslate.regallTranslate();
            trashfunctiontime = Time.time;

            {
                QuickKey = Config.Bind("打开窗口快捷键", "Key", new KeyboardShortcut(KeyCode.LeftAlt, KeyCode.Alpha2));
                BluePrintShowWindow = Config.Bind("打开蓝图窗口快捷键", "BluePrintQuickKey", new KeyboardShortcut(KeyCode.LeftControl, KeyCode.F));
                tempShowWindow = QuickKey.Value;
                tempBluePrintShowWindow = BluePrintShowWindow.Value;

                auto_supply_station = Config.Bind("自动配置新运输站", "auto_supply_station", false);
                autosetstationconfig = Config.Bind("自动配置新运输站参数显示", "autosetstationconfig", true);
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
                SunLightOpen = Config.Bind("夜灯", "SunLight", false);

                closeplayerflyaudio = Config.Bind("关闭玩家飞行声音", "closeplayerflyaudio", false);
                BluePrintDelete = Config.Bind("蓝图删除", "BluePrintDelete", false);
                BluePrintSelectAll = Config.Bind("蓝图全选", "BluePrintSelectAll", false);
                stationcopyItem_bool = Config.Bind("物流站复制物品配方", "stationcopyItem_bool", false);

                autocleartrash_bool = Config.Bind("30s间隔自动清除垃圾", "autocleartrash_bool", false);
                autoabsorttrash_bool = Config.Bind("30s间隔自动吸收垃圾", "autoabsorttrash_bool", false);
                onlygetbuildings = Config.Bind("只回收建筑", "onlygetbuildings", false);

                autowarpcommand = Config.Bind("自动导航使用翘曲", "autowarpcommand", false);
                CloseUIRandomTip = Config.Bind("关闭建筑栏提示", "CloseUIRandomTip", false);
                CloseUITutorialTip = Config.Bind("关闭教学提示", "CloseUITutorialTip", false);
                CloseUIAdvisor = Config.Bind("关闭顾问", "CloseUIAdvisor", false);
                CloseMilestone = Config.Bind("关闭里程碑", "CloseMilestone", false);
                CloseUIGeneralTip = Config.Bind("关闭科技解锁提示", "CloseUIGeneralTip", false);

                autoAddwarp = Config.Bind("自动添加翘曲器", "autoAddwarp", false);
                autonavigation_bool = Config.Bind("自动导航", "autonavigation_bool", false);
                autoAddPlayerVel = Config.Bind("自动加速", "autoAddPlayerVel", false);
                autowarpdistance = Config.Bind("自动使用翘曲器距离", "autowarpdistance", 0f);
                autowarpdistanceEnergyPercent = Config.Bind("自动曲速能量阈值", "autowarpdistanceEnergyPercent", 0f);
                autoaddtech_bool = Config.Bind("自动添加科技队列", "autoaddtech_bool", false);
                auto_add_techid = Config.Bind("自动添加科技队列科技ID", "auto_add_techid", 0);
                auto_add_techmaxLevel = Config.Bind("自动添加科技队列科技等级上限", "auto_add_techmaxLevel", 500);
                ShowStationInfo = Config.Bind("展示物流站信息", "ShowStationInfo", false);
                ShowStationInfoMode = Config.Bind("展示物流站信息模式", "ShowStationInfoMode", false);
                SaveLastOpenBluePrintBrowserPathConfig = Config.Bind("记录上次蓝图路径", "SaveLastOpenBluePrintBrowserPathConfig", false);

                noscaleuitech_bool = Config.Bind("科技页面不缩放", "noscaleuitech_bool", false);
                norender_shipdrone_bool = Config.Bind("不渲染飞机飞船", "norender_shipdrone_bool", false);
                norender_lab_bool = Config.Bind("不渲染研究室", "norender_lab_bool", false);
                norender_beltitem = Config.Bind("不渲染传送带货物", "norender_beltitem", false);
                norender_dysonshell_bool = Config.Bind("不渲染戴森壳", "norender_dysonshell_bool", false);
                norender_dysonswarm_bool = Config.Bind("不渲染戴森云", "norender_dysonswarm_bool", false);
                norender_DarkFog = Config.Bind("不渲染黑雾", "norender_DarkFog", false);
                norender_entity_bool = Config.Bind("不渲染实体", "norender_entity_bool", false);
                norender_powerdisk_bool = Config.Bind("不渲染电网覆盖", "norender_powerdisk_bool", false);
                Quickstop_bool = Config.Bind("ctrl+空格开启暂停工厂和戴森球", "Quickstop_bool", false);
                automovetounbuilt = Config.Bind("自动走向未完成建筑", "automovetounbuilt", false);
                automovetoPrebuildSecondElapse = Config.Bind("自动走向未完成建筑时间间隔", "automovetoPrebuildSecondElapse", 1f);
                automovetodarkfog = Config.Bind("自动飞向地面黑雾基地", "automovetodarkfog", false);
                upsquickset = Config.Bind("快速设置逻辑帧倍数", "upsquickset", false);
                autosetSomevalue_bool = Config.Bind("自动配置建筑", "autosetSomevalue_bool", false);
                autosetCourier_bool = Config.Bind("自动填充配送运输机", "autosetCourier_bool", false);
                auto_supply_starfuel = Config.Bind("人造恒星自动填充燃料数量", "auto_supply_starfuel", 4);
                auto_supply_starfuelID = Config.Bind("人造恒星自动填充燃料ID", "auto_supply_starfuelID", 1803);
                autosavetimechange = Config.Bind("自动保存", "autosavetimechange", false);
                KeepBeltHeight = Config.Bind("保持传送带高度", "KeepBeltHeight", false);
                autosavetime = Config.Bind("自动保存时间", "autosavetime", 25);
                DysonPanelSingleLayer = Config.Bind("单层壳列表", "DysonPanelSingleLayer", true);
                DysonPanelLayers = Config.Bind("多层壳列表", "DysonPanelLayers", true);
                DysonPanelSwarm = Config.Bind("戴森云列表", "DysonPanelSwarm", true);
                DysonPanelDysonSphere = Config.Bind("戴森壳列表", "DysonPanelDysonSphere", true);
                TrashStorageWindow_bool = Config.Bind("背包垃圾桶", "TrashStorageWindow_bool", false);
                window_height = Config.Bind("窗口高度", "window_height", 660f);
                window_width = Config.Bind("窗口宽度", "window_width", 830f);
                LastOpenBluePrintBrowserPathConfig = Config.Bind("上次打开蓝图路径", "LastOpenBluePrintBrowserPathConfig", "");
                AutoNavigateToDarkFogHive = Config.Bind("自动导航至黑雾巢穴", "AutoNavigateToDarkFogHive", false);
                AutoNavigateToDarkFogHiveKeepDistance = Config.Bind("自动导航至黑雾巢穴跟随距离", "AutoNavigateToDarkFogHiveKeepDistance", 400);
            }
            if (closeplayerflyaudio.Value)
            {
                PlayerAudioMutePatch.Enable = true;
            }
            if (CloseMilestone.Value)
            {
                LDB.audios["unlock-1"].Volume = 0;
            }
            //里程碑声音关闭
            CloseMilestone.SettingChanged += (t, e) =>
            {
                LDB.audios["unlock-1"].Volume = CloseMilestone.Value ? 0: AuxilaryfunctionPatch.originUnlockVolumn;
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

            pointlayeridlist = new List<int>();
            readyresearch = new List<int>();
            var AuxilaryPanel = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Auxilaryfunction.auxilarypanel")).LoadAsset<GameObject>("AuxilaryPanel");
            var ui_AuxilaryPanelPanel = Instantiate(AuxilaryPanel, UIRoot.instance.overlayCanvas.transform);
            guidraw = new GUIDraw(Math.Max(5, Math.Min(scale.Value, 35)), ui_AuxilaryPanelPanel);
            DysonBluePrintDataService.LoadDysonBluePrintData();
            ThreadPool.QueueUserWorkItem(_ => SecondEvent());
            autoaddtechid = auto_add_techid.Value;
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
                    List<int> getItem = new List<int>();
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
        /// 修改快捷键
        /// </summary>
        private void ChangeQuickKeyMethod()
        {
            if (ChangingQuickKey)
            {
                ChangeBluePrintQuickKey = false;
                ChangingBluePrintQuickKey = false;
            }
            else if (ChangingBluePrintQuickKey)
            {
                ChangeQuickKey = false;
                ChangingQuickKey = false;
            }
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

            if (ChangeBluePrintQuickKey)
            {
                setBluePrintQuickKey();
                ChangingBluePrintQuickKey = true;
            }
            else if (!ChangeBluePrintQuickKey && ChangingBluePrintQuickKey)
            {
                BluePrintShowWindow.Value = tempBluePrintShowWindow;
                ChangingBluePrintQuickKey = false;
            }
        }

        /// <summary>
        /// 将文字科技树入列
        /// </summary>
        private void EnqueueTech()
        {
            if (readyresearch.Count > 0)
            {
                try
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
                autoaddtechid = 0;
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
                    trashfunctiontime = Time.time;
                    BluePrintBatchModifyBuild.Init();
                }
                else
                {
                    StartAndStopGame();
                    BluePrintoptimize();
                    TrashFunction();
                    EnqueueTech();
                    autoaddtechid = auto_add_techid.Value;
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
            while (true)
            {
                if (!GameDataImported)
                    continue;
                if (autoAddwarp.Value)
                {
                    AutoAddwarp();
                }
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
                Thread.Sleep(1000);
            }
        }

        private void setBluePrintQuickKey()
        {
            bool left = true;
            int[] result = new int[2];
            if (Input.GetKey(KeyCode.LeftShift) && left)
            {
                left = false;
                result[0] = (int)KeyCode.LeftShift;
            }
            if (Input.GetKey(KeyCode.LeftControl) && left)
            {
                left = false;
                result[0] = (int)KeyCode.LeftControl;
            }
            if (Input.GetKey(KeyCode.LeftAlt) && left)
            {
                left = false;
                result[0] = (int)KeyCode.LeftAlt;
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
            else if (!left && !right) tempBluePrintShowWindow = new KeyboardShortcut((KeyCode)result[0], (KeyCode)result[1]);
            else
            {
                tempBluePrintShowWindow = new KeyboardShortcut((KeyCode)Math.Max(result[0], result[1]));
            }
        }

        private void setQuickKey()
        {
            bool left = true;
            int[] result = new int[2];
            if (Input.GetKey(KeyCode.LeftShift) && left)
            {
                left = false;
                result[0] = (int)KeyCode.LeftShift;
            }
            if (Input.GetKey(KeyCode.LeftControl) && left)
            {
                left = false;
                result[0] = (int)KeyCode.LeftControl;
            }
            if (Input.GetKey(KeyCode.LeftAlt) && left)
            {
                left = false;
                result[0] = (int)KeyCode.LeftAlt;
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
            else if (!left && !right) tempShowWindow = new KeyboardShortcut((KeyCode)result[0], (KeyCode)result[1]);
            else
            {
                tempShowWindow = new KeyboardShortcut((KeyCode)Math.Max(result[0], result[1]));
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
            if (Time.time - trashfunctiontime < 30)
                return;
            if (GameMain.data?.trashSystem == null)
            {
                return;
            }
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
                    if (Input.GetKey(KeyCode.KeypadPlus)) SpeedUpPatch.SpeedMultiple += 0.01f;
                    if (Input.GetKey(KeyCode.KeypadMinus)) SpeedUpPatch.SpeedMultiple -= 0.01f;
                    if (SpeedUpPatch.SpeedMultiple < 0.01) SpeedUpPatch.SpeedMultiple = 0.01f;
                    else if (SpeedUpPatch.SpeedMultiple > 10) SpeedUpPatch.SpeedMultiple = 10;
                }
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                test = !test;
            }

            if (test)
            {
            }
        }

        private bool test;
    }
}