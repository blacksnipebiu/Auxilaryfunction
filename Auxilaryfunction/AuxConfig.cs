using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Auxilaryfunction;

public static class AuxConfig
{
    public const string GUID = "cn.blacksnipe.dsp.Auxilaryfunction";
    public const string NAME = "Auxilaryfunction";
    public const string VERSION = "3.0.4";
    public static string ErrorTitle = "辅助面板错误提示";
    public static Harmony harmony;
    public static bool ChangingBluePrintQuickKey;
    public static bool ChangingQuickKey;
    public static bool ChangeBluePrintQuickKey;
    public static bool ChangeQuickKey;
    public static KeyboardShortcut tempBluePrintShowWindow;
    public static KeyboardShortcut tempShowWindow;
    public static bool simulatorrender;
    public static bool simulatorchanging;
    public static PlanetData LocalPlanet => GameMain.localPlanet;
    public static float originUnlockVolumn;

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
    public static ConfigEntry<int> scale;

    #endregion 配置菜单


    public static void BindAll(ConfigFile config)
    {
        QuickKey = config.Bind("打开窗口快捷键", "Key", new KeyboardShortcut(KeyCode.LeftAlt, KeyCode.Alpha2));
        BluePrintShowWindow = config.Bind("打开蓝图窗口快捷键", "BluePrintQuickKey", new KeyboardShortcut(KeyCode.LeftControl, KeyCode.F));
        tempShowWindow = QuickKey.Value;
        tempBluePrintShowWindow = BluePrintShowWindow.Value;

        auto_supply_station = config.Bind("自动配置新运输站", "auto_supply_station", false);
        autosetstationconfig = config.Bind("自动配置新运输站参数显示", "autosetstationconfig", true);
        auto_supply_Courier = config.Bind("自动填充配送机", "auto_supply_Courier", 10);
        auto_supply_drone = config.Bind("自动填充飞机数量", "auto_supply_drone", 10);
        auto_supply_ship = config.Bind("自动填充飞船数量", "auto_supply_ship", 5);
        stationmaxpowerpertick = config.Bind("自动设置最大充电功率", "autowarpdistance", 30f);
        stationwarpdist = config.Bind("自动设置物流站使用翘曲器距离", "stationwarpdist", 12.0);
        DroneStartCarry = config.Bind("自动设置物流站小飞机起送量", "DroneStartCarry", 0.1f);
        ShipStartCarry = config.Bind("自动设置物流站运输船起送量", "ShipStartCarry", 1f);
        veincollectorspeed = config.Bind("大型采矿机采矿速度", "veincollectorspeed", 10);
        auto_supply_warp = config.Bind("自动设置物流站翘曲器数量", "auto_supply_warp", 0);
        stationdronedist = config.Bind("自动设置物流站运输机最远距离", "stationdronedist", 180);
        stationshipdist = config.Bind("自动设置物流站运输船最远距离", "stationshipdist", 61);
        scale = config.Bind("大小适配", "scale", 16);
        SunLightOpen = config.Bind("夜灯", "SunLight", false);

        closeplayerflyaudio = config.Bind("关闭玩家飞行声音", "closeplayerflyaudio", false);
        BluePrintDelete = config.Bind("蓝图删除", "BluePrintDelete", false);
        BluePrintSelectAll = config.Bind("蓝图全选", "BluePrintSelectAll", false);
        stationcopyItem_bool = config.Bind("物流站复制物品配方", "stationcopyItem_bool", false);

        autocleartrash_bool = config.Bind("30s间隔自动清除垃圾", "autocleartrash_bool", false);
        autoabsorttrash_bool = config.Bind("30s间隔自动吸收垃圾", "autoabsorttrash_bool", false);
        onlygetbuildings = config.Bind("只回收建筑", "onlygetbuildings", false);

        autowarpcommand = config.Bind("自动导航使用翘曲", "autowarpcommand", false);
        CloseUIRandomTip = config.Bind("关闭建筑栏提示", "CloseUIRandomTip", false);
        CloseUITutorialTip = config.Bind("关闭教学提示", "CloseUITutorialTip", false);
        CloseUIAdvisor = config.Bind("关闭顾问", "CloseUIAdvisor", false);
        CloseMilestone = config.Bind("关闭里程碑", "CloseMilestone", false);
        CloseUIGeneralTip = config.Bind("关闭科技解锁提示", "CloseUIGeneralTip", false);

        autoAddwarp = config.Bind("自动添加翘曲器", "autoAddwarp", false);
        autonavigation_bool = config.Bind("自动导航", "autonavigation_bool", false);
        autoAddPlayerVel = config.Bind("自动加速", "autoAddPlayerVel", false);
        autowarpdistance = config.Bind("自动使用翘曲器距离", "autowarpdistance", 0f);
        autowarpdistanceEnergyPercent = config.Bind("自动曲速能量阈值", "autowarpdistanceEnergyPercent", 0f);
        autoaddtech_bool = config.Bind("自动添加科技队列", "autoaddtech_bool", false);
        auto_add_techid = config.Bind("自动添加科技队列科技ID", "auto_add_techid", 0);
        auto_add_techmaxLevel = config.Bind("自动添加科技队列科技等级上限", "auto_add_techmaxLevel", 500);
        ShowStationInfo = config.Bind("展示物流站信息", "ShowStationInfo", false);
        ShowStationInfoMode = config.Bind("展示物流站信息模式", "ShowStationInfoMode", false);
        SaveLastOpenBluePrintBrowserPathConfig = config.Bind("记录上次蓝图路径", "SaveLastOpenBluePrintBrowserPathConfig", false);

        noscaleuitech_bool = config.Bind("科技页面不缩放", "noscaleuitech_bool", false);
        norender_shipdrone_bool = config.Bind("不渲染飞机飞船", "norender_shipdrone_bool", false);
        norender_lab_bool = config.Bind("不渲染研究室", "norender_lab_bool", false);
        norender_beltitem = config.Bind("不渲染传送带货物", "norender_beltitem", false);
        norender_dysonshell_bool = config.Bind("不渲染戴森壳", "norender_dysonshell_bool", false);
        norender_dysonswarm_bool = config.Bind("不渲染戴森云", "norender_dysonswarm_bool", false);
        norender_DarkFog = config.Bind("不渲染黑雾", "norender_DarkFog", false);
        norender_entity_bool = config.Bind("不渲染实体", "norender_entity_bool", false);
        norender_powerdisk_bool = config.Bind("不渲染电网覆盖", "norender_powerdisk_bool", false);
        Quickstop_bool = config.Bind("ctrl+空格开启暂停工厂和戴森球", "Quickstop_bool", false);
        automovetounbuilt = config.Bind("自动走向未完成建筑", "automovetounbuilt", false);
        automovetoPrebuildSecondElapse = config.Bind("自动走向未完成建筑时间间隔", "automovetoPrebuildSecondElapse", 1f);
        automovetodarkfog = config.Bind("自动飞向地面黑雾基地", "automovetodarkfog", false);
        upsquickset = config.Bind("快速设置逻辑帧倍数", "upsquickset", false);
        autosetSomevalue_bool = config.Bind("自动配置建筑", "autosetSomevalue_bool", false);
        autosetCourier_bool = config.Bind("自动填充配送运输机", "autosetCourier_bool", false);
        auto_supply_starfuel = config.Bind("人造恒星自动填充燃料数量", "auto_supply_starfuel", 4);
        auto_supply_starfuelID = config.Bind("人造恒星自动填充燃料ID", "auto_supply_starfuelID", 1803);
        autosavetimechange = config.Bind("自动保存", "autosavetimechange", false);
        KeepBeltHeight = config.Bind("保持传送带高度", "KeepBeltHeight", false);
        autosavetime = config.Bind("自动保存时间", "autosavetime", 25);
        DysonPanelSingleLayer = config.Bind("单层壳列表", "DysonPanelSingleLayer", true);
        DysonPanelLayers = config.Bind("多层壳列表", "DysonPanelLayers", true);
        DysonPanelSwarm = config.Bind("戴森云列表", "DysonPanelSwarm", true);
        DysonPanelDysonSphere = config.Bind("戴森壳列表", "DysonPanelDysonSphere", true);
        TrashStorageWindow_bool = config.Bind("背包垃圾桶", "TrashStorageWindow_bool", false);
        window_height = config.Bind("窗口高度", "window_height", 660f);
        window_width = config.Bind("窗口宽度", "window_width", 830f);
        LastOpenBluePrintBrowserPathConfig = config.Bind("上次打开蓝图路径", "LastOpenBluePrintBrowserPathConfig", "");
        AutoNavigateToDarkFogHive = config.Bind("自动导航至黑雾巢穴", "AutoNavigateToDarkFogHive", false);
        AutoNavigateToDarkFogHiveKeepDistance = config.Bind("自动导航至黑雾巢穴跟随距离", "AutoNavigateToDarkFogHiveKeepDistance", 400);
    }
}

