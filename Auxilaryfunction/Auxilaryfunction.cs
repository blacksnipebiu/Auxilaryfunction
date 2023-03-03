using BepInEx;
using HarmonyLib;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.Events;
using static Auxilaryfunction.Constant;
using System.Reflection;
using System.Threading;
using System.IO;
using static Auxilaryfunction.AuxilaryfunctionPatch;
using System.Text;

namespace Auxilaryfunction
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Auxilaryfunction : BaseUnityPlugin
    {
        public const long AU = 40000;
        public const string ErrorTitle = "辅助面板错误提示";
        public const string GUID = "cn.blacksnipe.dsp.Auxilaryfunction";
        public const string NAME = "Auxilaryfunction";
        public const string VERSION = "2.0.2";
        public int stationindex = 4;
        public int locallogic;
        public int remotelogic = 2;
        public int[] locallogics = new int[5];
        public int[] remotelogics = new int[5];
        public int autoaddtechid;
        public int maxheight => Screen.height;
        public int recipewindowx;
        public int recipewindowy;
        public int pointlayerid;
        public int pointsignalid;
        private int DysonPanelBluePrintNum;
        private TechProto techProto;
        private TempDysonBlueprintData selectDysonBlueprintData = new TempDysonBlueprintData();
        public List<string> ConfigNames = new List<string>();
        public List<float[]> boundaries = new List<float[]>();
        public List<int> fuelItems = new List<int>();
        public List<int> assemblerpools = new List<int>();
        public List<int> labpools = new List<int>();
        public List<int> beltpools = new List<int>();
        public List<int> monitorpools = new List<int>();
        public List<int> ejectorpools = new List<int>();
        public List<int> powergenGammapools = new List<int>();
        public List<int> pointlayeridlist = new List<int>();
        public List<int> stationpools = new List<int>();
        public List<int> readyresearch = new List<int>();
        private List<TempDysonBlueprintData> tempDysonBlueprintData = new List<TempDysonBlueprintData>();
        private static Dictionary<int, bool> FuelFilter = new Dictionary<int, bool>();
        public string[] stationname = new string[6] { "星球矿机", "垃圾站", "星球无限供货机", "星球量子传输站", "星系量子传输站", "设置翘曲需求" };
        public float slowconstructspeed = 1;
        private float trashfunctiontime;
        public float autobuildtime;
        public float window_x_move = 200;
        public float window_y_move = 200;
        public float temp_window_x = 10;
        public float temp_window_y = 200;
        public float window_x = 300;
        public float window_y = 200;
        public float batchnum = 1;
        public float window_width = 830;
        public float window_height = 0;
        public float max_window_height = 710;
        public static float upsfix = 1;
        public static bool temp;
        public static bool simulatorrender;
        public static bool simulatorchanging;
        public static bool ready;
        public bool firstopen = true;
        public bool TextTech;
        public bool DysonBluePrint;
        public bool changescale;
        public bool selectautoaddtechid;
        public bool constructframe;
        public bool constructingshell;
        public bool constructshell;
        public bool slowconstruct = true;
        public bool onlysinglelayer = true;
        public bool limitmaterial;
        public bool blueprintopen;
        public bool autosetstationconfig = true;
        public bool firstStart = false;
        public bool moving;
        public bool leftscaling;
        public bool startdrawdyson;
        public bool rightscaling;
        public bool topscaling;
        public bool bottomscaling;
        public bool showwindow;
        public bool ChangeQuickKey;
        public bool ChangingQuickKey;
        public bool auto_setejector_start;
        public bool autoAddwarp_start;
        public bool autoAddFuel_start;
        public static bool GameDataImported;
        private bool DysonPanel;
        private bool DeleteDysonLayer;
        public static Dictionary<int, List<int>> EjectorDictionary;
        public static Player player => GameMain.mainPlayer;
        public static PlanetData LocalPlanet => GameMain.localPlanet;
        public Texture2D mytexture;
        public static BuildingParameters buildingParameters;
        public static int[,] stationcopyItem = new int[5, 6];
        public Dictionary<int, List<List<int>>> shellnodeidlayer = new Dictionary<int, List<List<int>>>();
        public Dictionary<int, List<int[]>> framenodeidlayer = new Dictionary<int, List<int[]>>();
        public List<string> DysonSphereBluePrintNamelist = new List<string>();
        public List<int[]> framenodeid = new List<int[]>();
        public List<List<int>> shellnodeid = new List<List<int>>();
        public List<DysonSphereLayer> constructinglayer = new List<DysonSphereLayer>();
        public KeyboardShortcut tempShowWindow;
        public Vector2 scrollPosition;
        public Vector2 pdselectscrollPosition;
        public GUIStyle styleblue = new GUIStyle();
        public GUIStyle styleyellow = new GUIStyle();
        public GUIStyle styleitemname = null;
        public GUIStyle buttonstyleyellow = null;
        public GUIStyle buttonstyleblue = null;

        public ERecipeType pointeRecipetype = ERecipeType.None;
        public static bool closecollider;
        public static bool stopfactory;
        public static bool changeups;
        public static bool autobuildgetitem;
        public static readonly FieldInfo _uiGridSplit_sliderImg = AccessTools.Field(typeof(UIGridSplit), "sliderImg");
        public static readonly FieldInfo _uiGridSplit_valueText = AccessTools.Field(typeof(UIGridSplit), "valueText");
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
        public static ConfigEntry<bool> CloseUIpanel;
        public static ConfigEntry<bool> KeepBeltHeight;
        public static ConfigEntry<bool> autoAddFuel;
        public static ConfigEntry<bool> autoAddwarp;
        public static ConfigEntry<double> stationwarpdist;
        public static ConfigEntry<bool> DysonPanelSingleLayer;
        public static ConfigEntry<bool> DysonPanelLayers;
        public static ConfigEntry<bool> DysonPanelSwarm;
        public static ConfigEntry<bool> DysonPanelDysonSphere;
        public static ConfigEntry<KeyboardShortcut> QuickKey;
        public static ConfigEntry<int> auto_supply_Courier;
        public static ConfigEntry<int> stationdronedist;
        public static ConfigEntry<int> autosavetime;
        public static ConfigEntry<int> scale;
        public static ConfigEntry<int> stationshipdist;
        public static ConfigEntry<int> auto_supply_starfuel;
        public static ConfigEntry<int> auto_supply_drone;
        public static ConfigEntry<int> auto_supply_ship;
        public static ConfigEntry<int> auto_supply_warp;
        public static ConfigEntry<int> veincollectorspeed;
        public static ConfigEntry<float> stationmaxpowerpertick;
        public static ConfigEntry<float> autowarpdistance;
        public static ConfigEntry<float> DroneStartCarry;
        public static ConfigEntry<float> ShipStartCarry;
        public static ConfigEntry<string> FuelFilterConfig;
        public static Thread autobuildThread;

        public static int maxCount = 0;
        public static GameObject[] tip = new GameObject[maxCount];
        public static GameObject stationTip;
        public static GameObject tipPrefab;
        public static GameObject beltWindow;
        public static GameObject SpeakerPanel;
        public static int count;
        private GameObject AuxilaryPanel;
        private GameObject ui_AuxilaryPanelPanel;

        void Start()
        {
            var harmony = new Harmony(GUID);
            harmony.PatchAll(typeof(PlayerOperation));
            harmony.PatchAll(typeof(AuxilaryfunctionPatch));
            AuxilaryTranslate.regallTranslate();
            AuxilaryPanel = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Auxilaryfunction.auxilarypanel")).LoadAsset<GameObject>("AuxilaryPanel");
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
                ShowStationInfo = Config.Bind("展示物流站信息", "ShowStationInfo", false);

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
                CloseUIpanel = Config.Bind("关闭面板", "CloseUIpanel", true);
                FuelFilterConfig = Config.Bind("自动填充燃料", "FuelFilterConfig", "");
                autoClearEmptyDyson = Config.Bind("自动清除空戴森球", "autoClearEmptyDyson", false);
                DysonPanelSingleLayer = Config.Bind("单层壳列表", "DysonPanelSingleLayer", true);
                DysonPanelLayers = Config.Bind("多层壳列表", "DysonPanelLayers", true);
                DysonPanelSwarm = Config.Bind("戴森云列表", "DysonPanelSwarm", true);
                DysonPanelDysonSphere = Config.Bind("戴森壳列表", "DysonPanelDysonSphere", true);
            }
            
            scrollPosition[0] = 0;
            pdselectscrollPosition[0] = 0;
            mytexture = new Texture2D(10, 10);
            for (int i = 0; i < mytexture.width; i++)
                for (int j = 0; j < mytexture.height; j++)
                    mytexture.SetPixel(i, j, new Color(0, 0, 0, 1));

            ConfigNames.Add("填充配送机数量");
            boundaries.Add(new float[] { 0, 10 });
            ConfigNames.Add("填充飞机数量");
            boundaries.Add(new float[] { 0, 100 });
            ConfigNames.Add("填充飞船数量");
            boundaries.Add(new float[] { 0, 10 });
            ConfigNames.Add("最大充电功率");
            boundaries.Add(new float[] { 30, 300 });
            ConfigNames.Add("运输机最远路程");
            boundaries.Add(new float[] { 20, 180 });
            ConfigNames.Add("运输船最远路程");
            boundaries.Add(new float[] { 1, 61 });
            ConfigNames.Add("曲速启用路程");
            boundaries.Add(new float[] { 0.5f, 60 });
            ConfigNames.Add("运输机起送量");
            boundaries.Add(new float[] { 0.01f, 1 });
            ConfigNames.Add("运输船起送量");
            boundaries.Add(new float[] { 0.1f, 1 });
            ConfigNames.Add("翘曲填充数量");
            boundaries.Add(new float[] { 0, 50 });
            ConfigNames.Add("大型采矿机采矿速率");
            boundaries.Add(new float[] { 10, 30 });
            foreach (var item in LDB.items.dataArray)
            {
                if (item.HeatValue > 0)
                {
                    fuelItems.Add(item.ID);
                    FuelFilter.Add(item.ID, false);
                }
            }
            if (!string.IsNullOrEmpty(FuelFilterConfig.Value))
            {
                string[] temp = FuelFilterConfig.Value.Split(',');
                foreach (var str in temp)
                {
                    if (str.Length > 3 && int.TryParse(str, out int itemID))
                    {
                        if (FuelFilter.ContainsKey(itemID))
                        {
                            FuelFilter[itemID] = true;
                        }
                    }
                }
            }
            mytexture.Apply();

            styleblue.fontStyle = FontStyle.Bold;
            styleblue.fontSize = 20;
            styleblue.normal.textColor = new Color32(167, 255, 255, 255);
            styleyellow.fontStyle = FontStyle.Bold;
            styleyellow.fontSize = 20;
            styleyellow.normal.textColor = new Color32(240, 191, 103, 255);
            BeltMonitorWindowOpen();
            LoadDysonBluePrintData();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                player.package.Sort();
            }
            if (autosavetimechange.Value && UIAutoSave.autoSaveTime != autosavetime.Value)
            {
                DSPGame.globalOption.autoSaveTime = autosavetime.Value;
                DSPGame.globalOption.Apply();
                UIAutoSave.autoSaveTime = autosavetime.Value;
            }
            if (!GameDataImported)
            {
                firstStart = true;
                closecollider = true;
                StopAutoBuildThread();
                readyresearch = new List<int>();
                upsfix = 1;
                autoaddtechid = 0;
            }
            else
            {
                if (firstStart)
                {
                    firstStart = false;
                    ready = true;
                    EjectorDictionary = new Dictionary<int, List<int>>();
                    trashfunctiontime = Time.time;
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
                else
                {
                    if (auto_setejector_bool.Value && !auto_setejector_start)
                    {
                        InvokeRepeating("ResetEjector", 1, 1);
                        auto_setejector_start = true;
                    }
                    else if (!auto_setejector_bool.Value && auto_setejector_start)
                    {
                        CancelInvoke("ResetEjector");
                        auto_setejector_start = false;
                    }
                    if (autoAddwarp.Value && !autoAddwarp_start)
                    {
                        InvokeRepeating("AutoAddwarp", 1, 1);
                        autoAddwarp_start = true;
                    }
                    else if (!autoAddwarp.Value && autoAddwarp_start)
                    {
                        CancelInvoke("AutoAddwarp");
                        autoAddwarp_start = false;
                    }
                    if (autoAddFuel.Value && !autoAddFuel_start)
                    {
                        InvokeRepeating("AutoAddFuel", 1, 1);
                        autoAddFuel_start = true;
                    }
                    else if (!autoAddFuel.Value && autoAddFuel_start)
                    {
                        CancelInvoke("AutoAddFuel");
                        autoAddFuel_start = false;
                    }
                    if (autoaddtechid > 0 && GameMain.history.techQueueLength == 0 && autoaddtech_bool.Value)
                    {
                        GameMain.history.EnqueueTech(autoaddtechid);
                    }
                    StartAndStopGame();
                    BluePrintoptimize();
                    StationInfoWindowUpdate();
                    TrashFunction();
                }

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

            ChangeQuickKeyMethod();
            if (QuickKey.Value.IsDown() && !ChangingQuickKey && ready)
            {
                showwindow = !showwindow;
                if (ui_AuxilaryPanelPanel == null)
                    ui_AuxilaryPanelPanel = Instantiate(AuxilaryPanel, UIRoot.instance.overlayCanvas.transform);
                ui_AuxilaryPanelPanel.SetActive(showwindow && !CloseUIpanel.Value);
            }
            if (showwindow && Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) { scale.Value++; changescale = true; window_height = 52 * scale.Value; }
                if (Input.GetKeyDown(KeyCode.DownArrow)) { scale.Value--; changescale = true; window_height = 52 * scale.Value; }
                if (scale.Value < 5) scale.Value = 5;
                if (scale.Value > 35) scale.Value = 35;
            }
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
            if (changescale || firstopen)
            {
                changescale = false;
                firstopen = false;
                GUI.skin.label.fontSize = scale.Value;
                GUI.skin.button.fontSize = scale.Value;
                GUI.skin.toggle.fontSize = scale.Value;
                GUI.skin.textField.fontSize = scale.Value;
                GUI.skin.textArea.fontSize = scale.Value;
            }
            else if (!changescale && GUI.skin.toggle.fontSize != scale.Value)
            {
                scale.Value = GUI.skin.toggle.fontSize;
            }
            if (styleitemname == null)
            {
                styleitemname = new GUIStyle(GUI.skin.label);
                styleitemname.normal.textColor = Color.white;
                buttonstyleblue = new GUIStyle(GUI.skin.button);
                buttonstyleblue.normal.textColor = styleblue.normal.textColor;
                buttonstyleyellow = new GUIStyle(GUI.skin.button);
                buttonstyleyellow.normal.textColor = styleyellow.normal.textColor;
            }
            if (showwindow)
            {
                var rt = ui_AuxilaryPanelPanel.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(window_width, window_height);
                rt.localPosition = new Vector2(-Screen.width / 2 + window_x, Screen.height / 2 - window_y - window_height);
                //ui_StarMapToolsBasePanel.transform. = new Vector3(window_width, window_height , 1);
                Rect window = new Rect(window_x, window_y, window_width, window_height);
                GUI.DrawTexture(window, mytexture);
                if (leftscaling || rightscaling || topscaling || bottomscaling) { }
                else
                    moveWindow_xl_first(ref window_x, ref window_y, ref window_x_move, ref window_y_move, ref moving, ref temp_window_x, ref temp_window_y, window_width);
                scaling_window(window_width, window_height, ref window_x, ref window_y);
                window = GUI.Window(20210827, window, DoMyWindow1, "辅助面板".getTranslate() + "(" + VERSION + ")" + "ps:ctrl+↑↓");
                int window2width = Localization.language != Language.zhCN ? 15 * scale.Value : 15 * scale.Value / 2;
                Rect switchwindow = new Rect(window_x - window2width, window_y, window2width, 25 * scale.Value);
                if (leftscaling || rightscaling || topscaling || bottomscaling) { }
                else
                    moveWindow_xl_first(ref window_x, ref window_y, ref window_x_move, ref window_y_move, ref moving, ref temp_window_x, ref temp_window_y, window_width);
                scaling_window(window_width, window_height, ref window_x, ref window_y);
                switchwindow = GUI.Window(202108228, switchwindow, DoMyWindow2, "");
                GUI.DrawTexture(switchwindow, mytexture);
            }
            if (player != null && player.navigation != null && player.navigation._indicatorAstroId != 0)
            {
                if (GUI.Button(new Rect(10, 250, 150, 60), PlayerOperation.fly ? "停止导航".getTranslate() : "继续导航".getTranslate()))
                {
                    PlayerOperation.fly = !PlayerOperation.fly;
                }
                if (GUI.Button(new Rect(10, 300, 150, 60), "取消方向指示".getTranslate()))
                {
                    player.navigation._indicatorAstroId = 0;
                }
            }
            if (automovetounbuilt.Value && player != null && LocalPlanet != null && LocalPlanet.factory != null && LocalPlanet.factory.prebuildCount > 0 && player.movementState == EMovementState.Fly)
            {
                if (GUI.Button(new Rect(10, 360, 150, 60), closecollider ? "停止寻找未完成建筑".getTranslate() : "开始寻找未完成建筑".getTranslate()))
                {
                    StopAutoBuildThread();
                    player.gameObject.GetComponent<SphereCollider>().enabled = !closecollider;
                    player.gameObject.GetComponent<CapsuleCollider>().enabled = !closecollider;
                }
            }
            else if (closecollider)
            {
                StopAutoBuildThread();
                player.gameObject.GetComponent<SphereCollider>().enabled = true;
                player.gameObject.GetComponent<CapsuleCollider>().enabled = true;
            }
            if (closecollider && LocalPlanet.gasItems == null && GUI.Button(new Rect(10, 420, 150, 60), autobuildgetitem ? "停止自动补充材料".getTranslate() : "开始自动补充材料".getTranslate()))
            {
                autobuildgetitem = !autobuildgetitem;
            }
            if (changeups)
            {
                GUI.Label(new Rect(Screen.width / 2, 0, 200, 50), string.Format("{0:N2}", upsfix) + "x");
            }
            if (blueprintopen)
            {
                int tempwidth = 0;
                int tempheight = 0;
                if (pointeRecipetype != ERecipeType.None)
                {
                    List<RecipeProto> showrecipe = new List<RecipeProto>();
                    foreach (RecipeProto rp in LDB.recipes.dataArray)
                    {
                        if (rp.Type != pointeRecipetype) continue;
                        showrecipe.Add(rp);
                    }
                    foreach (RecipeProto rp in showrecipe)
                    {
                        if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 50, maxheight - recipewindowy + tempheight * 50, 50, 50), rp.iconSprite.texture))
                        {
                            for (int j = 0; j < assemblerpools.Count; j++)
                            {
                                LocalPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].SetRecipe(rp.ID, LocalPlanet.factory.entitySignPool);
                            }
                        }
                        if (tempwidth % 10 == 0)
                        {
                            tempwidth = 0;
                            tempheight++;
                        }
                    }
                    if (showrecipe.Count > 0)
                    {
                        if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 50, maxheight - recipewindowy + tempheight++ * 50, 50, 50), "无".getTranslate()))
                        {
                            for (int j = 0; j < assemblerpools.Count; j++)
                            {
                                LocalPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].SetRecipe(0, LocalPlanet.factory.entitySignPool);
                            }
                        }
                        if (GUI.Button(new Rect(recipewindowx, maxheight - recipewindowy + tempheight * 50, 200, 50), "额外产出".getTranslate()))
                        {
                            for (int j = 0; j < assemblerpools.Count; j++)
                            {
                                if (LocalPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].productive)
                                    LocalPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].forceAccMode = false;
                            }
                        }
                        if (GUI.Button(new Rect(recipewindowx + 200, maxheight - recipewindowy + tempheight * 50, 200, 50), "生产加速".getTranslate()))
                        {
                            for (int j = 0; j < assemblerpools.Count; j++)
                            {
                                if (LocalPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].productive)
                                    LocalPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].forceAccMode = true;
                            }
                        }
                    }
                }
                else if (labpools.Count > 0)
                {
                    for (int i = 0; i <= 5; i++)
                        if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 50, maxheight - recipewindowy + tempheight * 50, 50, 50), LDB.items.Select(LabComponent.matrixIds[i]).iconSprite.texture))
                        {
                            for (int j = 0; j < labpools.Count; j++)
                            {
                                LocalPlanet.factory.factorySystem.labPool[labpools[j]].SetFunction(false, LDB.items.Select(LabComponent.matrixIds[i]).maincraft.ID, 0, LocalPlanet.factory.entitySignPool);
                            }
                        }
                    if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 50, maxheight - recipewindowy + tempheight++ * 50, 50, 50), "无".getTranslate()))
                    {
                        for (int j = 0; j < labpools.Count; j++)
                        {
                            LocalPlanet.factory.factorySystem.labPool[labpools[j]].SetFunction(false, 0, 0, LocalPlanet.factory.entitySignPool);
                        }
                    }
                    if (GUI.Button(new Rect(recipewindowx, maxheight - recipewindowy + tempheight++ * 50, 200, 50), "科研模式".getTranslate()))
                    {
                        for (int j = 0; j < labpools.Count; j++)
                        {
                            LocalPlanet.factory.factorySystem.labPool[labpools[j]].SetFunction(true, 0, GameMain.history.currentTech, LocalPlanet.factory.entitySignPool);
                        }
                    }
                    if (GUI.Button(new Rect(recipewindowx, maxheight - recipewindowy + tempheight * 50, 200, 50), "额外产出".getTranslate()))
                    {
                        for (int j = 0; j < labpools.Count; j++)
                        {
                            if (LocalPlanet.factory.factorySystem.labPool[labpools[j]].productive)
                                LocalPlanet.factory.factorySystem.labPool[labpools[j]].forceAccMode = false;
                        }
                    }
                    if (GUI.Button(new Rect(recipewindowx + 200, maxheight - recipewindowy + tempheight * 50, 200, 50), "生产加速".getTranslate()))
                    {
                        for (int j = 0; j < labpools.Count; j++)
                        {
                            if (LocalPlanet.factory.factorySystem.labPool[labpools[j]].productive)
                                LocalPlanet.factory.factorySystem.labPool[labpools[j]].forceAccMode = true;
                        }
                    }
                }
                else if (ejectorpools.Count > 0 && GameMain.data.dysonSpheres[GameMain.localStar.index] != null)
                {
                    DysonSwarm ds = GameMain.data.dysonSpheres[GameMain.localStar.index].swarm;
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            int orbitid = i * 5 + j + 1;
                            if (ds.OrbitExist(orbitid) && GUI.Button(new Rect(recipewindowx + j * 50, maxheight - recipewindowy + tempheight * 50, 50, 50), orbitid.ToString()))
                            {
                                for (int k = 0; k < ejectorpools.Count; k++)
                                {
                                    LocalPlanet.factory.factorySystem.ejectorPool[ejectorpools[k]].SetOrbit(orbitid);
                                }
                            }
                        }
                        tempheight++;
                    }
                }
                else if (stationpools.Count > 0)
                {
                    if (tempheight + tempwidth > 0) tempheight++;
                    tempwidth = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 130, maxheight - recipewindowy + tempheight * 50, 130, 50), stationname[i]))
                        {

                            for (int j = 0; j < stationpools.Count; j++)
                            {
                                StationComponent sc = LocalPlanet.factory.transport.stationPool[stationpools[j]];
                                if (i == 5)
                                {
                                    if (sc.storage[4].count > 0 && sc.storage[4].itemId != 1210)
                                        player.TryAddItemToPackage(sc.storage[4].itemId, sc.storage[4].count, 0, false);
                                    LocalPlanet.factory.transport.SetStationStorage(stationpools[j], stationindex, 1210, (int)batchnum * 100, (ELogisticStorage)locallogic, (ELogisticStorage)remotelogic, player);
                                }
                                else sc.name = stationname[i];
                            }
                            stationpools.Clear();
                            break;
                        }
                        if (i == 4)
                        {
                            tempheight++;
                            tempwidth = 0;
                        }
                    }
                    int tempx = recipewindowx + tempwidth * 130;
                    int tempy = maxheight - recipewindowy + tempheight++ * 50;
                    batchnum = (int)GUI.HorizontalSlider(new Rect(tempx, tempy, 150, 30), batchnum, 0, 100);
                    GUI.Label(new Rect(tempx, tempy + 30, 100, 30), "上限".getTranslate() + ":" + batchnum * 100);
                    if (GUI.Button(new Rect(tempx + 150, tempy, 100, 30), "第".getTranslate() + (stationindex + 1) + "格".getTranslate()))
                    {
                        stationindex++;
                        stationindex %= 5;
                    }
                    if (GUI.Button(new Rect(tempx + 250, tempy, 100, 30), "本地".getTranslate() + GetStationlogic(locallogic)))
                    {
                        locallogic++;
                        locallogic %= 3;
                    }
                    if (GUI.Button(new Rect(tempx + 350, tempy, 100, 30), "星际".getTranslate() + GetStationlogic(remotelogic)))
                    {
                        remotelogic++;
                        remotelogic %= 3;
                    }
                    if (GUI.Button(new Rect(recipewindowx, maxheight - recipewindowy + tempheight++ * 50, 130, 50), "粘贴物流站配方".getTranslate()))
                    {
                        PlanetFactory factory = LocalPlanet.factory;
                        for (int j = 0; j < stationpools.Count; j++)
                        {
                            StationComponent sc = factory.transport.stationPool[stationpools[j]];
                            for (int i = 0; i < sc.storage.Length && i < 5; i++)
                            {
                                if (stationcopyItem[i, 0] > 0)
                                {
                                    if (sc.storage[i].count > 0 && sc.storage[i].itemId != stationcopyItem[i, 0])
                                        player.TryAddItemToPackage(sc.storage[i].itemId, sc.storage[i].count, 0, false);
                                    factory.transport.SetStationStorage(stationpools[j], i, stationcopyItem[i, 0], stationcopyItem[i, 1], (ELogisticStorage)stationcopyItem[i, 2]
                                        , (ELogisticStorage)stationcopyItem[i, 3], player);
                                }
                                else
                                    factory.transport.SetStationStorage(stationpools[j], i, 0, 0, ELogisticStorage.None, ELogisticStorage.None, player);
                            }
                        }
                        stationpools.Clear();
                    }
                    int heightdis = scale.Value * 2;
                    GUILayout.BeginArea(new Rect(recipewindowx, maxheight - recipewindowy + tempheight++ * 50, heightdis * 15, heightdis * 10));
                    {
                        GUILayout.BeginVertical();
                        GUILayout.BeginHorizontal();
                        for (int i = 0; i < 5; i++)
                        {
                            GUILayout.BeginVertical();
                            if (GUILayout.Button("本地".getTranslate() + GetStationlogic(locallogics[i])))
                            {
                                locallogics[i]++;
                                locallogics[i] %= 3;
                            }
                            if (GUILayout.Button("星际".getTranslate() + GetStationlogic(remotelogics[i])))
                            {
                                remotelogics[i]++;
                                remotelogics[i] %= 3;
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();
                        if (GUILayout.Button("设置物流站逻辑"))
                        {
                            PlanetFactory factory = LocalPlanet.factory;
                            for (int j = 0; j < stationpools.Count; j++)
                            {
                                StationComponent sc = factory.transport.stationPool[stationpools[j]];
                                for (int i = 0; i < sc.storage.Length && i < 5; i++)
                                {
                                    if (sc.storage[i].itemId > 0)
                                    {
                                        sc.storage[i].localLogic = (ELogisticStorage)locallogics[i];
                                        sc.storage[i].remoteLogic = (ELogisticStorage)remotelogics[i];
                                    }
                                }
                            }
                            InitBluePrintData();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndArea();
                }
                else if (powergenGammapools.Count > 0)
                {
                    PlanetFactory factory = LocalPlanet.factory;
                    tempwidth = 0;
                    GUILayout.BeginArea(new Rect(recipewindowx, maxheight - recipewindowy, scale.Value * 10, scale.Value * 10));
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("直接发电".getTranslate()))
                    {
                        for (int j = 0; j < powergenGammapools.Count; j++)
                        {
                            var pgc = factory.powerSystem.genPool[powergenGammapools[j]];
                            int generatorId = powergenGammapools[j];
                            if (pgc.gamma)
                            {
                                PowerGeneratorComponent powerGeneratorComponent = factory.powerSystem.genPool[generatorId];

                                int productId = powerGeneratorComponent.productId;
                                int num = (int)powerGeneratorComponent.productCount;
                                if (productId != 0 && num > 0)
                                {
                                    int upCount = player.TryAddItemToPackage(productId, num, 0, true, 0);
                                    UIItemup.Up(productId, upCount);
                                }
                                factory.powerSystem.genPool[generatorId].productId = 0;
                                factory.powerSystem.genPool[generatorId].productCount = 0;
                            }
                        }
                    }
                    if (GUILayout.Button("光子生成".getTranslate()))
                    {
                        for (int j = 0; j < powergenGammapools.Count; j++)
                        {
                            var pgc = factory.powerSystem.genPool[powergenGammapools[j]];
                            int generatorId = powergenGammapools[j];
                            if (pgc.gamma)
                            {
                                PowerGeneratorComponent powerGeneratorComponent = factory.powerSystem.genPool[generatorId];

                                ItemProto itemProto = LDB.items.Select(factory.entityPool[powerGeneratorComponent.entityId].protoId);
                                if (itemProto == null)
                                {
                                    return;
                                }
                                GameHistoryData history = GameMain.history;
                                if (LDB.items.Select(itemProto.prefabDesc.powerProductId) == null || !history.ItemUnlocked(itemProto.prefabDesc.powerProductId))
                                {
                                    factory.powerSystem.genPool[generatorId].productId = 0;
                                    return;
                                }
                                factory.powerSystem.genPool[generatorId].productId = itemProto.prefabDesc.powerProductId;
                            }
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }
            }
        }

        public void DoMyWindow2(int winId)
        {
            int heightdis = scale.Value * 2;
            int widthlen2 = Localization.language != Language.zhCN ? 15 * scale.Value : 9 * scale.Value;
            GUILayout.BeginArea(new Rect(10, 20, widthlen2, 400));
            if (TextTech != GUI.Toggle(new Rect(0, 10, widthlen2, heightdis), TextTech, "文字科技树".getTranslate()))
            {
                TextTech = !TextTech;
                if (TextTech) DysonPanel = false;
            }
            if (limitmaterial != GUI.Toggle(new Rect(heightdis / 2, 10 + heightdis, widthlen2, heightdis), limitmaterial, "限制材料".getTranslate()))
            {
                limitmaterial = !limitmaterial;
                if (limitmaterial) TextTech = true;
            }
            if (DysonPanel != GUI.Toggle(new Rect(0, 10 + heightdis * 2, widthlen2, heightdis), DysonPanel, "戴森球面板".getTranslate()))
            {
                DysonPanel = !DysonPanel;
                if (DysonPanel) TextTech = false;
            }

            GUILayout.EndArea();

        }
        public void DoMyWindow1(int winId)
        {
            int heightdis = scale.Value * 2;
            if (window_height == 0) window_height = 26 * heightdis;
            if (TextTech)
            {
                TextTechPanelGUI(heightdis);
            }
            else if (DysonPanel)
            {
                DysonPanelGUI(heightdis);
            }
            else
            {
                GUILayout.BeginArea(new Rect(10, 20, window_width, window_height));
                GUILayout.Space(20);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, new[] { GUILayout.Width(window_width), GUILayout.Height(window_height) });
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                GUILayoutOption[] HorizontalSlideroptions = new[] { GUILayout.ExpandWidth(false), GUILayout.Height(heightdis / 2), GUILayout.Width(heightdis * 5) };
                GUILayoutOption[] buttonoptions = new[] { GUILayout.Height(heightdis), GUILayout.ExpandWidth(false) };
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                    auto_supply_station.Value = GUILayout.Toggle(auto_supply_station.Value, "自动配置新运输站".getTranslate(), buttonoptions);
                    autosetstationconfig = GUILayout.Toggle(autosetstationconfig, "配置参数".getTranslate(), buttonoptions);
                    GUILayout.EndHorizontal();
                    if (autosetstationconfig && auto_supply_station.Value)
                    {
                        for (int i = 0; i < ConfigNames.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            string showinfo = "";
                            switch (i)
                            {
                                case 0:
                                    auto_supply_Courier.Value = (int)GUILayout.HorizontalSlider(auto_supply_Courier.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    showinfo = auto_supply_Courier.Value + " ";
                                    break;
                                case 1:
                                    auto_supply_drone.Value = (int)GUILayout.HorizontalSlider(auto_supply_drone.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    showinfo = auto_supply_drone.Value + " ";
                                    break;
                                case 2:
                                    auto_supply_ship.Value = (int)GUILayout.HorizontalSlider(auto_supply_ship.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    showinfo = auto_supply_ship.Value + " ";
                                    break;
                                case 3:
                                    stationmaxpowerpertick.Value = (int)GUILayout.HorizontalSlider(stationmaxpowerpertick.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    showinfo = (int)stationmaxpowerpertick.Value + "MW ";
                                    break;
                                case 4:
                                    stationdronedist.Value = (int)GUILayout.HorizontalSlider(stationdronedist.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    showinfo = stationdronedist.Value + "° ";
                                    break;
                                case 5:
                                    stationshipdist.Value = (int)GUILayout.HorizontalSlider(stationshipdist.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    showinfo = (stationshipdist.Value == 61 ? "∞ " : stationshipdist.Value + "ly ");
                                    break;
                                case 6:
                                    stationwarpdist.Value = (int)GUILayout.HorizontalSlider((float)stationwarpdist.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    if (stationwarpdist.Value == 0) stationwarpdist.Value = 0.5;
                                    showinfo = stationwarpdist.Value + "AU ";
                                    break;
                                case 7:
                                    DroneStartCarry.Value = GUILayout.HorizontalSlider(DroneStartCarry.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    DroneStartCarry.Value = DroneStartCarry.Value == 0 ? 0.01f : DroneStartCarry.Value;
                                    showinfo = ((int)(DroneStartCarry.Value * 10) * 10 == 0 ? "1" : "" + (int)(DroneStartCarry.Value * 10) * 10) + "% ";
                                    break;
                                case 8:
                                    ShipStartCarry.Value = GUILayout.HorizontalSlider(ShipStartCarry.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    showinfo = (int)(ShipStartCarry.Value * 10) * 10 + "% ";
                                    break;
                                case 9:
                                    auto_supply_warp.Value = (int)GUILayout.HorizontalSlider(auto_supply_warp.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    showinfo = auto_supply_warp.Value + " ";
                                    break;
                                case 10:
                                    veincollectorspeed.Value = (int)GUILayout.HorizontalSlider(veincollectorspeed.Value, boundaries[i][0], boundaries[i][1], HorizontalSlideroptions);
                                    showinfo = veincollectorspeed.Value / 10.0f + " ";
                                    break;
                            }
                            GUIStyle labelstyle = new GUIStyle(GUI.skin.label);
                            labelstyle.fontSize = scale.Value - 3;
                            labelstyle.normal.textColor = GUI.skin.toggle.normal.textColor;
                            GUILayout.Label(showinfo + ConfigNames[i].getTranslate(), labelstyle, buttonoptions);
                            GUILayout.EndHorizontal();
                        }
                    }
                    if (GUILayout.Button("铺满轨道采集器".getTranslate(), buttonoptions)) SetGasStation();
                    if (GUILayout.Button("批量配置当前星球物流站".getTranslate(), buttonoptions)) ChangeAllStationConfig();
                    if (GUILayout.Button("填充当前星球配送机飞机飞船、翘曲器".getTranslate(), buttonoptions)) AddDroneShipToStation();
                    if (GUILayout.Button("批量配置当前星球大型采矿机采矿速率".getTranslate(), buttonoptions)) ChangeAllVeinCollectorSpeedConfig();
                    norender_dysonshell_bool.Value = GUILayout.Toggle(norender_dysonshell_bool.Value, "不渲染戴森壳".getTranslate());
                    norender_dysonswarm_bool.Value = GUILayout.Toggle(norender_dysonswarm_bool.Value, "不渲染戴森云".getTranslate());
                    norender_lab_bool.Value = GUILayout.Toggle(norender_lab_bool.Value, "不渲染研究站".getTranslate());
                    norender_beltitem.Value = GUILayout.Toggle(norender_beltitem.Value, "不渲染传送带货物".getTranslate());
                    norender_shipdrone_bool.Value = GUILayout.Toggle(norender_shipdrone_bool.Value, "不渲染运输船和飞机".getTranslate());
                    norender_entity_bool.Value = GUILayout.Toggle(norender_entity_bool.Value, "不渲染实体".getTranslate());
                    if (simulatorrender != GUILayout.Toggle(simulatorrender, "不渲染全部".getTranslate()))
                    {
                        simulatorrender = !simulatorrender;
                        simulatorchanging = true;
                    }
                    norender_powerdisk_bool.Value = GUILayout.Toggle(norender_powerdisk_bool.Value, "不渲染电网覆盖".getTranslate());
                    closeplayerflyaudio.Value = GUILayout.Toggle(closeplayerflyaudio.Value, "关闭玩家走路飞行声音".getTranslate());
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    if (autoaddtech_bool.Value != GUILayout.Toggle(autoaddtech_bool.Value, "自动添加科技队列".getTranslate(), buttonoptions))
                    {
                        autoaddtech_bool.Value = !autoaddtech_bool.Value;
                        if (!autoaddtech_bool.Value) autoaddtechid = 0;
                    }
                    if (autoaddtech_bool.Value)
                    {
                        if (GUILayout.Button(LDB.techs.Select(autoaddtechid) == null ? "未选择".getTranslate() : LDB.techs.Select(autoaddtechid).name, buttonoptions))
                        {
                            selectautoaddtechid = !selectautoaddtechid;
                        }
                        if (LDB.techs.dataArray != null && selectautoaddtechid)
                        {
                            for (int i = 0; i < LDB.techs.dataArray.Length; i++)
                            {
                                TechState techstate = GameMain.history.techStates[LDB.techs.dataArray[i].ID];
                                if (techstate.curLevel < techstate.maxLevel && techstate.maxLevel > 10)
                                {
                                    if (GUILayout.Button(LDB.techs.dataArray[i].name + " " + techstate.curLevel + " " + techstate.maxLevel, buttonoptions))
                                    {
                                        autoaddtechid = LDB.techs.dataArray[i].ID;
                                    }
                                }
                            }
                        }
                    }
                    autoAddwarp.Value = GUILayout.Toggle(autoAddwarp.Value, "自动添加翘曲器".getTranslate(), buttonoptions);
                    autoAddFuel.Value = GUILayout.Toggle(autoAddFuel.Value, "自动添加燃料".getTranslate(), buttonoptions);
                    if (autoAddFuel.Value)
                    {
                        int rownum = fuelItems.Count / 6;
                        rownum = fuelItems.Count % 6 > 0 ? rownum + 1 : rownum;
                        int index = 0;
                        for (int i = 0; i < rownum; i++)
                        {
                            GUILayout.BeginHorizontal();
                            for (int j = 0; j < 6 && index < fuelItems.Count; j++, index++)
                            {
                                int itemID = fuelItems[index];
                                GUIStyle style = new GUIStyle();
                                if (FuelFilter[itemID])
                                    style.normal.background = Texture2D.whiteTexture;
                                if (GUILayout.Button(LDB.items.Select(itemID).iconSprite.texture, style, new[] { GUILayout.Height(heightdis), GUILayout.Width(heightdis) }))
                                {
                                    FuelFilter[itemID] = !FuelFilter[itemID];
                                    string result = "";
                                    foreach (var item in FuelFilter)
                                    {
                                        if (item.Value)
                                        {
                                            result += item.Key + ",";
                                        }
                                    }
                                    FuelFilterConfig.Value = result;
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    auto_setejector_bool.Value = GUILayout.Toggle(auto_setejector_bool.Value, "自动配置太阳帆弹射器".getTranslate(), buttonoptions);
                    automovetounbuilt.Value = GUILayout.Toggle(automovetounbuilt.Value, "自动飞向未完成建筑".getTranslate(), buttonoptions);
                    close_alltip_bool.Value = GUILayout.Toggle(close_alltip_bool.Value, "一键闭嘴".getTranslate(), buttonoptions);
                    noscaleuitech_bool.Value = GUILayout.Toggle(noscaleuitech_bool.Value, "科技面板选中不缩放".getTranslate(), buttonoptions);
                    BluePrintSelectAll.Value = GUILayout.Toggle(BluePrintSelectAll.Value, "蓝图全选".getTranslate() + "(ctrl+A）", buttonoptions);
                    BluePrintDelete.Value = GUILayout.Toggle(BluePrintDelete.Value, "蓝图删除".getTranslate() + "(ctrl+X）", buttonoptions);
                    BluePrintRevoke.Value = GUILayout.Toggle(BluePrintRevoke.Value, "蓝图撤销".getTranslate() + "(ctrl+Z)", buttonoptions);
                    BluePrintSetRecipe.Value = GUILayout.Toggle(BluePrintSetRecipe.Value, "蓝图设置配方".getTranslate() + "(ctrl+F)", buttonoptions);
                    bool temp = GUILayout.Toggle(ShowStationInfo.Value, "物流站信息显示".getTranslate(), buttonoptions);
                    stationcopyItem_bool.Value = GUILayout.Toggle(stationcopyItem_bool.Value, "物流站物品设置复制粘贴".getTranslate(), buttonoptions);
                    if (temp != ShowStationInfo.Value)
                    {
                        ShowStationInfo.Value = temp;
                        if (!temp)
                            for (int index = 0; index < maxCount; ++index)
                                tip[index].SetActive(false);
                    }
                    if (autoabsorttrash_bool.Value != GUILayout.Toggle(autoabsorttrash_bool.Value, "30s间隔自动吸收垃圾".getTranslate(), buttonoptions))
                    {
                        autoabsorttrash_bool.Value = !autoabsorttrash_bool.Value;
                        if (autoabsorttrash_bool.Value)
                        {
                            autocleartrash_bool.Value = false;
                        }
                    }
                    if (autoabsorttrash_bool.Value)
                    {
                        onlygetbuildings.Value = GUILayout.Toggle(onlygetbuildings.Value, "只回收建筑".getTranslate(), buttonoptions);
                    }
                    if (autocleartrash_bool.Value != GUILayout.Toggle(autocleartrash_bool.Value, "30s间隔自动清除垃圾".getTranslate(), buttonoptions))
                    {
                        autocleartrash_bool.Value = !autocleartrash_bool.Value;
                        if (autocleartrash_bool.Value)
                        {
                            autoabsorttrash_bool.Value = false;
                            onlygetbuildings.Value = false;
                        }
                    }
                    ChangeQuickKey = GUILayout.Toggle(ChangeQuickKey, !ChangeQuickKey ? "改变窗口快捷键".getTranslate() : "点击确认".getTranslate(), buttonoptions);
                    if (ChangeQuickKey)
                    {
                        GUILayout.TextArea(tempShowWindow.ToString(), new[] { GUILayout.Height(heightdis), GUILayout.Width(6 * heightdis) });
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    changeups = GUILayout.Toggle(changeups, "启动时间流速修改".getTranslate(), buttonoptions);
                    GUILayout.Label("流速倍率".getTranslate() + ":" + string.Format("{0:N2}", upsfix), buttonoptions);
                    string tempupsfixstr = GUILayout.TextField(string.Format("{0:N2}", upsfix), new[] { GUILayout.Height(heightdis), GUILayout.Width(5 * heightdis) });

                    if (tempupsfixstr != string.Format("{0:N2}", upsfix) && float.TryParse(tempupsfixstr, out float tempupsfix))
                    {
                        if (tempupsfix < 0.01) tempupsfix = 0.01f;
                        if (tempupsfix > 10) tempupsfix = 10;
                        upsfix = tempupsfix;
                    }
                    upsfix = GUILayout.HorizontalSlider(upsfix, 0.01f, 10, HorizontalSlideroptions);
                    upsquickset.Value = GUILayout.Toggle(upsquickset.Value, "加速减速".getTranslate() + "(shift,'+''-')", buttonoptions);

                    autosetSomevalue_bool.Value = GUILayout.Toggle(autosetSomevalue_bool.Value, "自动配置建筑".getTranslate(), buttonoptions);
                    GUILayout.Label("人造恒星燃料数量".getTranslate() + "：" + auto_supply_starfuel.Value, buttonoptions);
                    auto_supply_starfuel.Value = (int)GUILayout.HorizontalSlider(auto_supply_starfuel.Value, 4, 100, HorizontalSlideroptions);
                    if (GUILayout.Button("填充当前星球人造恒星".getTranslate(), buttonoptions)) AddFuelToAllStar();

                    autosavetimechange.Value = GUILayout.Toggle(autosavetimechange.Value, "自动保存".getTranslate(), buttonoptions);
                    if (autosavetimechange.Value)
                    {
                        GUILayout.Label("自动保存时间".getTranslate() + "/min：", buttonoptions);
                        int tempint = autosavetime.Value / 60;
                        if (int.TryParse(Regex.Replace(GUILayout.TextField(tempint + "", new[] { GUILayout.Height(heightdis), GUILayout.Width(5 * heightdis) }), @"[^0-9]", ""), out tempint))
                        {
                            if (tempint < 1) tempint = 1;
                            if (tempint > 10000) tempint = 10000;
                            autosavetime.Value = tempint * 60;
                        }
                    }
                    if (CloseUIpanel.Value != GUILayout.Toggle(CloseUIpanel.Value, "关闭白色面板".getTranslate(), buttonoptions))
                    {
                        CloseUIpanel.Value = !CloseUIpanel.Value;
                        ui_AuxilaryPanelPanel.SetActive(!CloseUIpanel.Value);
                    }
                    KeepBeltHeight.Value = GUILayout.Toggle(KeepBeltHeight.Value, "保持传送带高度(shift)".getTranslate(), buttonoptions);
                    Quickstop_bool.Value = GUILayout.Toggle(Quickstop_bool.Value, "ctrl+空格快速开关".getTranslate(), buttonoptions);
                    stopfactory = GUILayout.Toggle(stopfactory, "     停止工厂".getTranslate(), buttonoptions);
                    autonavigation_bool.Value = GUILayout.Toggle(autonavigation_bool.Value, "自动导航".getTranslate(), buttonoptions);
                    if (autonavigation_bool.Value)
                    {
                        autowarpcommand.Value = GUILayout.Toggle(autowarpcommand.Value, "自动导航使用曲速".getTranslate(), buttonoptions);
                        GUILayout.Label("自动使用翘曲器距离".getTranslate() + ":", buttonoptions);
                        autowarpdistance.Value = GUILayout.HorizontalSlider(autowarpdistance.Value, 0, 30, HorizontalSlideroptions);
                        GUILayout.Label(string.Format("{0:N2}", autowarpdistance.Value) + "光年".getTranslate() + "\n", buttonoptions);
                    }
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUI.EndScrollView();
                GUILayout.EndArea();

            }
        }
        private void TextTechPanelGUI(int heightdis)
        {
            int tempheight = 0;
            scrollPosition = GUI.BeginScrollView(new Rect(10, 20, window_width - 20, window_height - 30), scrollPosition, new Rect(0, 0, window_width - 20, max_window_height));

            int buttonwidth = heightdis * 5;
            int colnum = (int)window_width / buttonwidth;
            var propertyicon = UIRoot.instance.uiGame.techTree.nodePrefab.buyoutButton.transform.Find("icon").GetComponent<Image>().mainTexture;
            GUI.Label(new Rect(0, 0, heightdis * 10, heightdis), "准备研究".getTranslate());
            int i = 0;
            tempheight += heightdis;
            for (; i < readyresearch.Count; i++)
            {
                TechProto tp = LDB.techs.Select(readyresearch[i]);
                if (i != 0 && i % colnum == 0) tempheight += heightdis * 4;
                if (GUI.Button(new Rect(i % colnum * buttonwidth, tempheight, buttonwidth, heightdis * 2), tp.ID < 2000 ? tp.name : (tp.name + tp.Level)))
                {
                    if (GameMain.history.TechInQueue(readyresearch[i]))
                    {
                        for (int j = 0; j < GameMain.history.techQueue.Length; j++)
                        {
                            if (GameMain.history.techQueue[j] != readyresearch[i]) continue;
                            GameMain.history.RemoveTechInQueue(j);
                            break;
                        }
                    }
                    readyresearch.RemoveAt(i);
                }
                int k = 0;
                foreach (ItemProto ip in tp.itemArray)
                {
                    GUI.Button(new Rect(i % colnum * buttonwidth + k++ * heightdis, heightdis * 2 + tempheight, heightdis, heightdis), ip.iconSprite.texture);
                }
                k = 0;
                foreach (RecipeProto rp in tp.unlockRecipeArray)
                {
                    GUI.Button(new Rect(i % colnum * buttonwidth + k++ * heightdis, heightdis * 3 + tempheight, heightdis, heightdis), rp.iconSprite.texture);
                }
                if (GUI.Button(new Rect(i % colnum * buttonwidth + 4 * heightdis, heightdis * 3 + tempheight, heightdis, heightdis), propertyicon))
                {
                    BuyoutTech(tp);
                }
            }
            tempheight += heightdis * 4;
            GUI.Label(new Rect(0, tempheight, heightdis * 10, heightdis), "科技".getTranslate());
            tempheight += heightdis;
            i = 0;
            foreach (TechProto tp in LDB.techs.dataArray)
            {
                if (tp.ID > 2000) break;
                if (readyresearch.Contains(tp.ID) || !GameMain.history.CanEnqueueTech(tp.ID) || tp.MaxLevel > 20 || GameMain.history.TechUnlocked(tp.ID)) continue;
                if (limitmaterial)
                {
                    bool condition = true;
                    foreach (int ip in tp.Items)
                    {
                        if (GameMain.history.ItemUnlocked(ip)) continue;
                        condition = false;
                        break;
                    }
                    if (!condition) continue;
                }
                if (i != 0 && i % colnum == 0) tempheight += heightdis * 4;
                if (GUI.Button(new Rect(i % colnum * buttonwidth, tempheight, buttonwidth, heightdis * 2), tp.name))
                {
                    readyresearch.Add(tp.ID);
                }
                int k = 0;
                foreach (ItemProto ip in tp.itemArray)
                {
                    GUI.Button(new Rect(i % colnum * buttonwidth + k++ * heightdis, heightdis * 2 + tempheight, heightdis, heightdis), ip.iconSprite.texture);
                }
                k = 0;
                foreach (RecipeProto rp in tp.unlockRecipeArray)
                {
                    GUI.Button(new Rect(i % colnum * buttonwidth + k++ * heightdis, heightdis * 3 + tempheight, heightdis, heightdis), rp.iconSprite.texture);
                }
                if (GUI.Button(new Rect(i % colnum * buttonwidth + 4 * heightdis, heightdis * 3 + tempheight, heightdis, heightdis), UIRoot.instance.uiGame.techTree.nodePrefab.buyoutButton.transform.Find("icon").GetComponent<Image>().mainTexture))
                {
                    BuyoutTech(tp);
                }
                i++;
            }
            tempheight += heightdis * 4;
            GUI.Label(new Rect(0, tempheight, heightdis * 10, heightdis), "升级".getTranslate());
            i = 0;
            tempheight += heightdis;
            foreach (TechProto tp in LDB.techs.dataArray)
            {
                if (tp.ID < 2000 || readyresearch.Contains(tp.ID) || !GameMain.history.CanEnqueueTech(tp.ID) || tp.MaxLevel > 20 || tp.MaxLevel > 100 || GameMain.history.TechUnlocked(tp.ID)) continue;
                if (limitmaterial)
                {
                    bool condition = true;
                    foreach (int ip in tp.Items)
                    {
                        if (GameMain.history.ItemUnlocked(ip)) continue;
                        condition = false;
                        break;
                    }
                    if (!condition) continue;
                }
                if (i != 0 && i % colnum == 0) tempheight += heightdis * 4;
                if (GUI.Button(new Rect(i % colnum * buttonwidth, tempheight, buttonwidth, heightdis * 2), tp.name + tp.Level))
                {
                    readyresearch.Add(tp.ID);
                }
                int k = 0;
                foreach (ItemProto ip in tp.itemArray)
                {
                    GUI.Button(new Rect(i % colnum * buttonwidth + k++ * heightdis, heightdis * 2 + tempheight, heightdis, heightdis), ip.iconSprite.texture);
                }
                if (GUI.Button(new Rect(i % colnum * buttonwidth + 4 * heightdis, heightdis * 3 + tempheight, heightdis, heightdis), UIRoot.instance.uiGame.techTree.nodePrefab.buyoutButton.transform.Find("icon").GetComponent<Image>().mainTexture))
                {
                    BuyoutTech(tp);
                }
                i++;
            }
            max_window_height = heightdis * 5 + tempheight;
            GUI.EndScrollView();
        }
        private void DysonPanelGUI(int heightdis)
        {
            bool[] dysonlayers = new bool[11];
            var dyson = UIRoot.instance?.uiGame?.dysonEditor?.selection?.viewDysonSphere;
            if (dyson != null)
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (dyson.layersIdBased[i] != null && dyson.layersIdBased[i].id != 0 && dyson.layersIdBased[i].nodeCount == 0)
                    {
                        dysonlayers[dyson.layersIdBased[i].id] = true;
                    }
                }
            }

            GUILayout.BeginArea(new Rect(10, 20, window_width, window_height));
            {
                #region 左侧面板
                GUILayout.BeginArea(new Rect(10, 0, window_width / 2, window_height));
                GUILayout.BeginVertical();
                GUILayout.Label("选择一个蓝图后，点击右侧的层级可以自动导入".getTranslate());
                if (GUILayout.Button("打开戴森球蓝图文件夹".getTranslate(), new[] { GUILayout.Height(heightdis), GUILayout.Width(heightdis * 10) }))
                {
                    string path = new StringBuilder(GameConfig.overrideDocumentFolder).Append(GameConfig.gameName).Append("/DysonBluePrint/").ToString();
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    Application.OpenURL(path);
                }
                if (GUILayout.Button("刷新文件".getTranslate(), new[] { GUILayout.Height(heightdis), GUILayout.Width(heightdis * 10) }))
                {
                    selectDysonBlueprintData.path = "";
                    LoadDysonBluePrintData();
                }
                GUILayout.BeginArea(new Rect(0, 3 * heightdis, window_width / 2, window_height));
                scrollPosition = GUI.BeginScrollView(new Rect(0, 0, window_width/2 -20 , window_height - 4 * heightdis), scrollPosition, new Rect(0, 0, window_width / 2-40, Math.Max((4+ DysonPanelBluePrintNum) *(heightdis+4), window_height - 4 * heightdis)));

                GUILayout.BeginVertical();
                DysonPanelBluePrintNum = 0;
                if (tempDysonBlueprintData.Exists(o => o.type == EDysonBlueprintType.SingleLayer))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("单层壳".getTranslate());
                    GUILayout.FlexibleSpace();
                    DysonPanelSingleLayer.Value = GUILayout.Toggle(DysonPanelSingleLayer.Value, "", new[] { GUILayout.Width(2 * heightdis) });
                    GUILayout.Space(heightdis);
                    GUILayout.EndHorizontal();
                    if (DysonPanelSingleLayer.Value)
                    {
                        var templist = tempDysonBlueprintData.Where(x => x.type == EDysonBlueprintType.SingleLayer).ToList();
                        for (int i = 0; i < templist.Count; i++)
                        {
                            bool temp = GUILayout.Toggle(templist[i].path == selectDysonBlueprintData.path, templist[i].name, new[] { GUILayout.Height(heightdis) });
                            if (temp != (templist[i].path == selectDysonBlueprintData.path))
                            {
                                selectDysonBlueprintData = templist[i];
                            }
                        }
                        DysonPanelBluePrintNum += templist.Count;
                    }
                }
                if (tempDysonBlueprintData.Exists(o => o.type == EDysonBlueprintType.Layers))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("多层壳".getTranslate(), new[] { GUILayout.Height(heightdis) });
                    GUILayout.FlexibleSpace();
                    DysonPanelLayers.Value=GUILayout.Toggle(DysonPanelLayers.Value, "", new[] { GUILayout.Width(2 * heightdis) });
                    GUILayout.Space(heightdis);
                    GUILayout.EndHorizontal();
                    if (DysonPanelLayers.Value)
                    {
                        var templist = tempDysonBlueprintData.Where(x => x.type == EDysonBlueprintType.Layers).ToList();
                        for (int i = 0; i < templist.Count; i++)
                        {
                            bool temp = GUILayout.Toggle(templist[i].path == selectDysonBlueprintData.path, templist[i].name, new[] { GUILayout.Height(heightdis) });
                            if (temp != (templist[i].path == selectDysonBlueprintData.path))
                            {
                                selectDysonBlueprintData = templist[i];
                            }
                        }
                        DysonPanelBluePrintNum += templist.Count;
                    }
                }
                if (tempDysonBlueprintData.Exists(o => o.type == EDysonBlueprintType.SwarmOrbits))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("戴森云".getTranslate());
                    GUILayout.FlexibleSpace();
                    DysonPanelSwarm.Value=GUILayout.Toggle(DysonPanelSwarm.Value, "", new[] { GUILayout.Width(2 * heightdis)});
                    GUILayout.Space(heightdis);
                    GUILayout.EndHorizontal();
                    if (DysonPanelSwarm.Value)
                    {
                        var templist = tempDysonBlueprintData.Where(x => x.type == EDysonBlueprintType.SwarmOrbits).ToList();
                        for (int i = 0; i < templist.Count; i++)
                        {
                            bool temp = GUILayout.Toggle(templist[i].path == selectDysonBlueprintData.path, templist[i].name, new[] { GUILayout.Height(heightdis) });
                            if (temp != (templist[i].path == selectDysonBlueprintData.path))
                            {
                                selectDysonBlueprintData = templist[i];
                            }
                        }
                        DysonPanelBluePrintNum += templist.Count;
                    }
                }
                if (tempDysonBlueprintData.Exists(o => o.type == EDysonBlueprintType.DysonSphere))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("戴森球(包括壳、云)".getTranslate());
                    GUILayout.FlexibleSpace();
                    DysonPanelDysonSphere.Value=GUILayout.Toggle(DysonPanelDysonSphere.Value, "",new[] {GUILayout.Width(2*heightdis) });
                    GUILayout.Space(heightdis);
                    GUILayout.EndHorizontal();
                    if (DysonPanelDysonSphere.Value)
                    {
                        var templist = tempDysonBlueprintData.Where(x => x.type == EDysonBlueprintType.DysonSphere).ToList();
                        for (int i = 0; i < templist.Count; i++)
                        {
                            bool temp = GUILayout.Toggle(templist[i].path == selectDysonBlueprintData.path, templist[i].name, new[] {GUILayout.Height(heightdis)});
                            if (temp != (templist[i].path == selectDysonBlueprintData.path))
                            {
                                selectDysonBlueprintData = templist[i];
                            }
                        }
                        DysonPanelBluePrintNum += templist.Count;
                    }
                }
                GUILayout.EndVertical();
                GUI.EndScrollView();
                GUILayout.EndArea();
                GUILayout.EndVertical();
                GUILayout.EndArea();
                #endregion
                #region 右侧面板
                GUILayout.BeginArea(new Rect(10 + window_width / 2, 0, window_width / 2, window_height));
                int lineIndex = 0;

                if (GUI.Button(new Rect(0, lineIndex++ * heightdis, heightdis * 10, heightdis), "复制选中文件代码".getTranslate()))
                {
                    string data = ReaddataFromFile(selectDysonBlueprintData.path);
                    GUIUtility.systemCopyBuffer = data;
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        Thread.Sleep(10000);
                        GUIUtility.systemCopyBuffer = "";
                    });
                }
                if (GUI.Button(new Rect(0, lineIndex++ * heightdis, heightdis * 10, heightdis), "清除剪贴板".getTranslate()))
                {
                    GUIUtility.systemCopyBuffer = "";
                }
                if (GUI.Button(new Rect(0, lineIndex++ * heightdis, heightdis * 10, heightdis), "应用蓝图".getTranslate()) && dyson != null)
                {
                    string data = ReaddataFromFile(selectDysonBlueprintData.path);
                    ApplyDysonBlueprintManage(data, dyson, selectDysonBlueprintData.type);
                }
                if (GUI.Button(new Rect(0, lineIndex++ * heightdis, heightdis * 10, heightdis), "自动生成最大半径戴森壳".getTranslate()) && dyson != null)
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        float radius = dyson.maxOrbitRadius;
                        while (radius > dyson.minOrbitRadius)
                        {
                            if (dyson.QueryLayerRadius(ref radius, out float orbitAngularSpeed))
                            {
                                dyson.AddLayer(radius, Quaternion.identity, orbitAngularSpeed);
                                break;
                            }
                            radius -= 1;
                        }
                        if (dyson.layerCount == 10) break;
                    }
                }
                if (GUI.Button(new Rect(0, lineIndex++ * heightdis, heightdis * 10, heightdis), "删除全部空壳".getTranslate()) && dyson != null)
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        if (dyson.layersIdBased[i] != null && dyson.layersIdBased[i].nodeCount == 0)
                        {
                            dyson.RemoveLayer(dyson.layersIdBased[i]);
                        }
                    }
                }
                if (autoClearEmptyDyson.Value != GUI.Toggle(new Rect(0, lineIndex++ * heightdis, heightdis * 8, heightdis), autoClearEmptyDyson.Value, "自动清除空戴森球".getTranslate()))
                {
                    UIMessageBox.Show(ErrorTitle.getTranslate(), "每次打开戴森球面板都会自动进行清理".getTranslate(), "确定".Translate(), 3, null);
                }


                GUI.Label(new Rect(0, lineIndex++ * heightdis, heightdis * 12, heightdis), "当前选中".getTranslate() + ":" +
                    UIRoot.instance?.uiGame?.dysonEditor?.selection?.viewDysonSphere?.starData.name ?? "");
                GUI.Label(new Rect(0, lineIndex++ * heightdis, heightdis * 5, heightdis), "可用戴森壳层级:".getTranslate());
                for (int i = 1; i <= 10; i++)
                {
                    if (GUI.Button(new Rect((i - 1) % 5 * heightdis, lineIndex * heightdis, heightdis, heightdis), dysonlayers[i] ? i.ToString() : ""))
                    {
                        if (dysonlayers[i])
                        {
                            string data = ReaddataFromFile(selectDysonBlueprintData.path);
                            ApplyDysonBlueprintManage(data, dyson, EDysonBlueprintType.SingleLayer, i);
                        }
                    }
                    if (i % 5 == 0)
                    {
                        lineIndex++;
                    }
                }
                DeleteDysonLayer = GUI.Toggle(new Rect(heightdis * 5, lineIndex * heightdis, heightdis * 8, heightdis), DeleteDysonLayer, "勾选即可点击删除".getTranslate());
                GUI.Label(new Rect(0, lineIndex++ * heightdis, heightdis * 5, heightdis), "不可用戴森壳层级:".getTranslate());
                for (int i = 1; i <= 10; i++)
                {
                    if (GUI.Button(new Rect((i - 1) % 5 * heightdis, lineIndex * heightdis, heightdis, heightdis), !dysonlayers[i] ? i.ToString() : ""))
                    {
                        if (DeleteDysonLayer)
                        {
                            RemoveLayerById(i);
                        }
                    }
                    if (i % 5 == 0)
                    {
                        lineIndex++;
                    }
                }
                GUILayout.EndArea();
                #endregion
            }
            GUILayout.EndArea();
        }

        void BeltMonitorWindowOpen()
        {
            # region BeltWindow
            beltWindow = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Belt Window"), GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Blueprint Copy Mode").transform);
            beltWindow.GetComponent<RectTransform>().position = new Vector3(7.5f, 50, 20);
            beltWindow.name = "test";
            Vector3 item_sign_localposition = beltWindow.transform.Find("item-sign").GetComponent<RectTransform>().localPosition;
            beltWindow.transform.Find("item-sign").GetComponent<RectTransform>().localPosition = item_sign_localposition - new Vector3(item_sign_localposition.x, -30, 0);
            Vector3 number_input_localposition = beltWindow.transform.Find("number-input").GetComponent<RectTransform>().localPosition;
            beltWindow.transform.Find("number-input").GetComponent<RectTransform>().localPosition = number_input_localposition - new Vector3(number_input_localposition.x, -30, 0);
            Destroy(beltWindow.transform.Find("state").gameObject);
            Destroy(beltWindow.transform.Find("item-display").gameObject);
            Destroy(beltWindow.transform.Find("panel-bg").Find("title-text").gameObject);
            beltWindow.transform.Find("item-sign").GetComponent<Button>().onClick.AddListener(() =>
            {
                if (UISignalPicker.isOpened)
                    UISignalPicker.Close();
                else
                    UISignalPicker.Popup(new Vector2(-300, Screen.height / 3), new Action<int>(SetSignalId));
            });
            beltWindow.transform.Find("number-input").GetComponent<InputField>().onEndEdit.AddListener((string str) =>
            {
                float result = 0.0f;
                if (!float.TryParse(str, out result))
                    return;
                if (beltpools.Count > 0)
                {
                    foreach (int i in beltpools)
                    {
                        LocalPlanet.factory.cargoTraffic.SetBeltSignalIcon(i, pointsignalid);
                        LocalPlanet.factory.cargoTraffic.SetBeltSignalNumber(i, result);
                    }
                }
            });
            beltWindow.gameObject.SetActive(false);
            #endregion
            #region MonitorWindow
            SpeakerPanel = Instantiate<GameObject>(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Monitor Window"), GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Blueprint Copy Mode").transform);
            SpeakerPanel.GetComponent<RectTransform>().position = new Vector3(7.5f, 50, 20);
            SpeakerPanel.name = "test1";
            SpeakerPanel.gameObject.SetActive(false);
            SpeakerPanel.transform.Find("speaker-panel").gameObject.SetActive(false);
            SpeakerPanel.transform.Find("speaker-panel").GetComponent<RectTransform>().position = new Vector3(8, 47, 20);
            Destroy(SpeakerPanel.transform.Find("flow-statistics").gameObject);
            Destroy(SpeakerPanel.transform.Find("alarm-settings").gameObject);
            Destroy(SpeakerPanel.transform.Find("monitor-settings").gameObject);
            Destroy(SpeakerPanel.transform.Find("sep-line").gameObject);
            Destroy(SpeakerPanel.transform.Find("sep-line").gameObject);
            GameObject speaker_panel = SpeakerPanel.transform.Find("speaker-panel").gameObject;
            GameObject pitch = speaker_panel.transform.Find("pitch").gameObject;
            GameObject volume = speaker_panel.transform.Find("volume").gameObject;
            speaker_panel.GetComponent<UISpeakerPanel>().toneCombo.onItemIndexChange.AddListener(new UnityAction(() =>
            {
                if (monitorpools != null && monitorpools.Count > 0)
                {
                    UIComboBox toneCombo = speaker_panel.GetComponent<UISpeakerPanel>().toneCombo;
                    foreach (int i in monitorpools)
                    {
                        int speakerId = LocalPlanet.factory.cargoTraffic.monitorPool[i].speakerId;
                        LocalPlanet.factory.digitalSystem.speakerPool[speakerId].SetTone(toneCombo.ItemsData[toneCombo.itemIndex]);
                        LocalPlanet.factory.digitalSystem.speakerPool[speakerId].Play(ESpeakerPlaybackOrigin.Current);
                    }
                }
            }));
            pitch.transform.Find("slider").GetComponent<Slider>().onValueChanged.AddListener((float f) =>
            {
                string str = PitchLetter[((int)f - 1) % 12] + (((int)f - 1) / 12).ToString();
                speaker_panel.GetComponent<UISpeakerPanel>().pitchText.text = str;

                if (monitorpools != null && monitorpools.Count > 0)
                {
                    UIComboBox toneCombo = speaker_panel.GetComponent<UISpeakerPanel>().toneCombo;
                    foreach (int i in monitorpools)
                    {
                        int speakerId = LocalPlanet.factory.cargoTraffic.monitorPool[i].speakerId;
                        LocalPlanet.factory.digitalSystem.speakerPool[speakerId].SetPitch((int)f);
                        LocalPlanet.factory.digitalSystem.speakerPool[speakerId].Play(ESpeakerPlaybackOrigin.Current);
                    }
                }
            });
            volume.transform.Find("slider").GetComponent<Slider>().onValueChanged.AddListener((float f) =>
            {
                speaker_panel.GetComponent<UISpeakerPanel>().volumeText.text = ((int)f).ToString();

                if (monitorpools != null && monitorpools.Count > 0)
                {
                    UIComboBox toneCombo = speaker_panel.GetComponent<UISpeakerPanel>().toneCombo;
                    foreach (int i in monitorpools)
                    {
                        int speakerId = LocalPlanet.factory.cargoTraffic.monitorPool[i].speakerId;
                        LocalPlanet.factory.digitalSystem.speakerPool[speakerId].SetVolume((int)speaker_panel.GetComponent<UISpeakerPanel>().volumeSlider.value);
                        LocalPlanet.factory.digitalSystem.speakerPool[speakerId].Play(ESpeakerPlaybackOrigin.Current);
                    }
                }
            });
            speaker_panel.GetComponent<UISpeakerPanel>().testPlayBtn.GetComponent<UIButton>().onClick += new Action<int>((int str) =>
              {
                  if (monitorpools != null && monitorpools.Count > 0)
                  {
                      UIComboBox toneCombo = speaker_panel.GetComponent<UISpeakerPanel>().toneCombo;
                      foreach (int i in monitorpools)
                      {
                          int speakerId = LocalPlanet.factory.cargoTraffic.monitorPool[i].speakerId;
                          LocalPlanet.factory.digitalSystem.speakerPool[speakerId].SetPitch((int)speaker_panel.GetComponent<UISpeakerPanel>().pitchSlider.value);
                          LocalPlanet.factory.digitalSystem.speakerPool[speakerId].SetVolume((int)speaker_panel.GetComponent<UISpeakerPanel>().volumeSlider.value);
                          LocalPlanet.factory.digitalSystem.speakerPool[speakerId].SetTone(toneCombo.ItemsData[toneCombo.itemIndex]);
                          LocalPlanet.factory.digitalSystem.speakerPool[speakerId].Play(ESpeakerPlaybackOrigin.Current);
                      }
                  }
              });
            #endregion

            stationTip = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Scene UIs/Vein Marks"), GameObject.Find("UI Root/Overlay Canvas/In Game/Scene UIs").transform);
            stationTip.name = "stationTip";
            Destroy(stationTip.GetComponent<UIVeinDetail>());
            tipPrefab = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Scene UIs/Vein Marks/vein-tip-prefab"), stationTip.transform);
            tipPrefab.name = "tipPrefab";
            tipPrefab.GetComponent<Image>().sprite = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Key Tips/tip-prefab").GetComponent<Image>().sprite;
            tipPrefab.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            tipPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 160f);
            tipPrefab.GetComponent<Image>().enabled = true;
            tipPrefab.transform.localPosition = new Vector3(200f, 800f, 0);
            tipPrefab.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            Destroy(tipPrefab.GetComponent<UIVeinDetailNode>());
            tipPrefab.SetActive(false);
            for (int index = 0; index < 6; ++index)
            {
                GameObject gameObject1 = Instantiate<GameObject>(tipPrefab.transform.Find("info-text").gameObject, new Vector3(0, 0, 0), Quaternion.identity, tipPrefab.transform);
                gameObject1.name = "countText" + index;
                gameObject1.GetComponent<Text>().fontSize = index == 5 ? 15 : 19;
                gameObject1.GetComponent<Text>().text = "99999";
                gameObject1.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                gameObject1.GetComponent<RectTransform>().sizeDelta = new Vector2(95, 30);
                gameObject1.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                gameObject1.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                gameObject1.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                gameObject1.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, (-5 - 30 * index), 0);
                Destroy(gameObject1.GetComponent<Shadow>());
                GameObject gameObject2 = Instantiate(tipPrefab.transform.Find("icon").gameObject, new Vector3(0, 0, 0), Quaternion.identity, tipPrefab.transform);
                gameObject2.name = "icon" + index;
                gameObject2.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                gameObject2.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                gameObject2.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                gameObject2.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, (-5 - 30 * index), 0);
                //UIIconCountInc uiiconCountInc = Instantiate<UIIconCountInc>(this.icons[0], this.icons[0].transform.parent);
                //uiiconCountInc.SetTransformIdentity();
                //uiiconCountInc.visible = false;
            }
            for (int i = 0; i < 3; i++)
            {
                GameObject icontext = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Top Tips/Entity Briefs/brief-info-top/brief-info/content/icons/icon"), new Vector3(0, 0, 0), Quaternion.identity, tipPrefab.transform);
                icontext.name = "icontext" + i;
                icontext.GetComponent<RectTransform>().localScale = new Vector3(0.7f, 0.7f, 1);
                icontext.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                icontext.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                icontext.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                icontext.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(i * 30, -180, 0);
                GameObject gameObject1 = Instantiate(tipPrefab.transform.Find("info-text").gameObject, new Vector3(0, 0, 0), Quaternion.identity, icontext.transform);
                gameObject1.name = "countText";
                gameObject1.GetComponent<Text>().fontSize = 22;
                gameObject1.GetComponent<Text>().text = "100";
                gameObject1.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                gameObject1.GetComponent<RectTransform>().sizeDelta = new Vector2(95, 30);
                gameObject1.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                gameObject1.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                gameObject1.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                gameObject1.GetComponent<RectTransform>().localPosition = new Vector3(-50, -20, 0);
                Destroy(gameObject1.GetComponent<Shadow>());
                if (i != 2)
                {
                    GameObject gameObject2 = Instantiate(tipPrefab.transform.Find("info-text").gameObject, new Vector3(0, 0, 0), Quaternion.identity, icontext.transform);
                    gameObject2.name = "countText2";
                    gameObject2.GetComponent<Text>().fontSize = 22;
                    gameObject2.GetComponent<Text>().text = "100";
                    gameObject2.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                    gameObject2.GetComponent<RectTransform>().sizeDelta = new Vector2(95, 30);
                    gameObject2.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                    gameObject2.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                    gameObject2.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                    gameObject2.GetComponent<RectTransform>().localPosition = new Vector3(-50, 10, 0);
                    Destroy(gameObject2.GetComponent<Shadow>());
                }
                Destroy(icontext.transform.Find("count-text").gameObject);
                Destroy(icontext.transform.Find("bg").gameObject);
                Destroy(icontext.transform.Find("inc").gameObject);
                Destroy(icontext.GetComponent<UIIconCountInc>());
            }
            tipPrefab.transform.Find("info-text").gameObject.SetActive(false);
            tipPrefab.transform.Find("icon").gameObject.SetActive(false);
            for (int i = 0; i < maxCount; ++i)
                tip[i] = Instantiate<GameObject>(tipPrefab, stationTip.transform);
        }

        void StationInfoWindowUpdate()
        {
            if (!ShowStationInfo.Value)
                return;
            if (UIGame.viewMode == EViewMode.Normal || UIGame.viewMode == EViewMode.Globe)
            {
                stationTip.SetActive(true);
                if (GameMain.data == null)
                    return;
                PlanetData pd = GameMain.data.localPlanet;
                if (pd == null || pd.factory == null)
                    return;
                int index1 = 0;
                Vector3 localPosition = GameCamera.main.transform.localPosition;
                Vector3 forward = GameCamera.main.transform.forward;
                float realRadius = pd.realRadius;
                if (pd.factory.transport.stationCursor > 0)
                {
                    foreach (StationComponent stationComponent in pd.factory.transport.stationPool)
                    {
                        if (index1 == maxCount)
                        {
                            ++maxCount;
                            Instantiate(tipPrefab, stationTip.transform);
                            Array.Resize(ref tip, maxCount);
                            tip[maxCount - 1] = Instantiate(tipPrefab, stationTip.transform);
                        }
                        if (stationComponent != null && stationComponent.storage != null)
                        {
                            Vector3 position;
                            int num1;
                            if (stationComponent.isCollector)
                            {
                                position = pd.factory.entityPool[stationComponent.entityId].pos.normalized * (realRadius + 35f);
                                num1 = 2;
                            }
                            else if (stationComponent.isStellar)
                            {
                                position = pd.factory.entityPool[stationComponent.entityId].pos.normalized * (realRadius + 20f);
                                num1 = 5;
                            }
                            else if (stationComponent.isVeinCollector)
                            {
                                position = pd.factory.entityPool[stationComponent.entityId].pos.normalized * (realRadius + 5f);
                                num1 = 1;
                            }
                            else
                            {
                                position = pd.factory.entityPool[stationComponent.entityId].pos.normalized * (realRadius + 15f);
                                num1 = 4;
                            }
                            Vector3 rhs = position - localPosition;
                            float magnitude = rhs.magnitude;
                            float num2 = Vector3.Dot(forward, rhs);
                            if (magnitude < 1.0 || num2 < 1.0)
                            {
                                tip[index1].SetActive(false);
                            }
                            else
                            {
                                Vector2 rectPoint;
                                bool flag = UIRoot.ScreenPointIntoRect(GameCamera.main.WorldToScreenPoint(position), stationTip.GetComponent<RectTransform>(), out rectPoint);
                                if (Mathf.Abs(rectPoint.x) > 8000.0)
                                    flag = false;
                                if (Mathf.Abs(rectPoint.y) > 8000.0)
                                    flag = false;
                                if (Phys.RayCastSphere(localPosition, rhs / magnitude, magnitude, Vector3.zero, realRadius, out RCHCPU _))
                                    flag = false;
                                if (flag)
                                {
                                    rectPoint.x = Mathf.Round(rectPoint.x);
                                    rectPoint.y = Mathf.Round(rectPoint.y);
                                    tip[index1].GetComponent<RectTransform>().anchoredPosition = rectPoint;

                                    if (stationComponent.isCollector)
                                        tip[index1].GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 70f);
                                    else if (stationComponent.isStellar)
                                        tip[index1].GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 220f);
                                    else if (stationComponent.isVeinCollector)
                                        tip[index1].GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 40f);
                                    else
                                        tip[index1].GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 130f);
                                    for (int i = 0; i < num1; ++i)
                                    {
                                        if (stationComponent.storage[i].itemId > 0)
                                        {
                                            tip[index1].transform.Find("icon" + i).GetComponent<Image>().sprite = LDB.items.Select(stationComponent.storage[i].itemId)?.iconSprite;
                                            tip[index1].transform.Find("icon" + i).gameObject.SetActive(true);
                                            tip[index1].transform.Find("countText" + i).GetComponent<Text>().text = stationComponent.storage[i].count.ToString("#,##0");
                                            tip[index1].transform.Find("countText" + i).GetComponent<Text>().color = Color.white;
                                            tip[index1].transform.Find("countText" + i).gameObject.SetActive(true);
                                            tip[index1].SetActive(true);
                                        }
                                        else
                                        {
                                            tip[index1].transform.Find("icon" + i).gameObject.SetActive(false);
                                            tip[index1].transform.Find("countText" + i).GetComponent<Text>().text = "无";
                                            tip[index1].transform.Find("countText" + i).GetComponent<Text>().color = Color.white;
                                            tip[index1].transform.Find("countText" + i).gameObject.SetActive(true);
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(stationComponent.name))
                                    {
                                        tip[index1].transform.Find("icon" + num1).gameObject.SetActive(false);
                                        tip[index1].transform.Find("countText" + num1).GetComponent<Text>().text = stationComponent.name;
                                        tip[index1].transform.Find("countText" + num1).GetComponent<Text>().color = Color.white;
                                        tip[index1].transform.Find("countText" + num1).gameObject.SetActive(true);
                                    }
                                    else
                                    {
                                        tip[index1].transform.Find("icon" + num1).gameObject.SetActive(false);
                                        tip[index1].transform.Find("countText" + num1).gameObject.SetActive(false);
                                    }
                                    for (int i = 0; i < 3; ++i)
                                    {
                                        if (stationComponent.isCollector || stationComponent.isVeinCollector)
                                        {
                                            tip[index1].transform.Find("icontext" + i).gameObject.SetActive(false);
                                            continue;
                                        }
                                        if (i >= 1 && !stationComponent.isStellar)
                                        {
                                            tip[index1].transform.Find("icontext" + i).gameObject.SetActive(false);
                                        }
                                        else
                                        {
                                            tip[index1].transform.Find("icontext" + i).gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(i * 30, -25 - 30 * (string.IsNullOrEmpty(stationComponent.name) ? num1 : num1 + 1), 0);
                                            tip[index1].transform.Find("icontext" + i).GetComponent<Image>().sprite = LDB.items.Select(i != 2 ? 5001 + i : 1210).iconSprite;
                                            tip[index1].transform.Find("icontext" + i).Find("countText").GetComponent<Text>().color = Color.white;
                                            tip[index1].transform.Find("icontext" + i).Find("countText").GetComponent<Text>().text = i == 0 ? (stationComponent.idleDroneCount + stationComponent.workDroneCount).ToString() : (i == 1 ? (stationComponent.idleShipCount + stationComponent.workShipCount).ToString() : stationComponent.warperCount.ToString());
                                            if (i != 2)
                                            {
                                                tip[index1].transform.Find("icontext" + i).Find("countText2").GetComponent<Text>().color = Color.white;
                                                tip[index1].transform.Find("icontext" + i).Find("countText2").GetComponent<Text>().text = i == 0 ? stationComponent.idleDroneCount.ToString() : stationComponent.idleShipCount.ToString();
                                            }
                                            tip[index1].transform.Find("icontext" + i).gameObject.SetActive(true);
                                        }
                                    }
                                    if (magnitude < 50.0)
                                        tip[index1].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                                    else if (magnitude < 250.0)
                                    {
                                        float num3 = (float)(1.75 - magnitude * 0.005);
                                        tip[index1].transform.localScale = new Vector3(1, 1, 1) * num3;
                                    }
                                    else
                                        tip[index1].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                                    for (int i = string.IsNullOrEmpty(stationComponent.name) ? num1 : num1 + 1; i < 6; ++i)
                                    {
                                        tip[index1].transform.Find("icon" + i).gameObject.SetActive(false);
                                        tip[index1].transform.Find("countText" + i).gameObject.SetActive(false);
                                    }
                                    ++index1;
                                }
                            }
                        }
                    }
                }
                for (int index4 = index1; index4 < maxCount; ++index4)
                    tip[index4].SetActive(false);
            }
            else
                stationTip.SetActive(false);
        }

        public static void FindItemAndMove(int itemId, int itemCount)
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
        /// 添加飞机飞船翘曲器
        /// </summary>
        private void AddDroneShipToStation()
        {
            if (LocalPlanet == null || LocalPlanet.type == EPlanetType.Gas) return;
            int inc;
            foreach (var dc in LocalPlanet.factory.transport.dispenserPool)
            {
                if (dc == null) continue;
                if (dc.idleCourierCount + dc.workCourierCount < auto_supply_Courier.Value)
                {
                    dc.idleCourierCount += player.package.TakeItem(5003, auto_supply_Courier.Value - dc.idleCourierCount + dc.workCourierCount, out _);
                }
            }
            foreach (StationComponent sc in LocalPlanet.factory.transport.stationPool)
            {
                if (sc == null || sc.isVeinCollector) continue;
                if (sc.isStellar && sc.workShipCount + sc.idleShipCount < auto_supply_ship.Value)
                    sc.idleShipCount += player.package.TakeItem(5002, auto_supply_ship.Value - sc.workShipCount - sc.idleShipCount, out inc);
                if (sc.isStellar)
                {
                    if (sc.workDroneCount + sc.idleDroneCount < auto_supply_drone.Value)
                        sc.idleDroneCount += player.package.TakeItem(5001, auto_supply_drone.Value - sc.workDroneCount - sc.idleDroneCount, out inc);
                }
                else
                {
                    int tempdrone = auto_supply_drone.Value > 50 ? 50 : auto_supply_drone.Value;
                    if (sc.workDroneCount + sc.idleDroneCount < tempdrone)
                        sc.idleDroneCount += player.package.TakeItem(5001, tempdrone - sc.workDroneCount - sc.idleDroneCount, out inc);
                }
                if (sc.warperCount < auto_supply_warp.Value)
                    sc.warperCount += player.package.TakeItem(1210, auto_supply_warp.Value - sc.warperCount, out inc);
            }
            foreach (StationComponent sc in LocalPlanet.factory.transport.stationPool)
            {
                if (sc == null) continue;
                if (sc.isStellar && sc.workShipCount + sc.idleShipCount > 10)
                {
                    player.package.AddItem(5002, sc.workShipCount + sc.idleShipCount - 10, 0, out inc);
                    sc.idleShipCount -= sc.workShipCount + sc.idleShipCount - 10;
                }
                if (sc.isStellar)
                {
                    if (sc.workDroneCount + sc.idleDroneCount > 100)
                    {
                        sc.idleDroneCount -= sc.workDroneCount + sc.idleDroneCount - 100;
                    }
                }
                else
                {
                    if (sc.workDroneCount + sc.idleDroneCount > 50)
                    {
                        player.package.TakeItem(5001, sc.workDroneCount + sc.idleDroneCount - 50, out inc);
                        sc.idleDroneCount -= sc.workDroneCount + sc.idleDroneCount - 50;
                    }
                }
            }
        }

        /// <summary>
        /// 设置所有大型采矿机配置
        /// </summary>
        private void ChangeAllVeinCollectorSpeedConfig()
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
        private void ChangeAllStationConfig()
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
                    sc.warpEnableDist = stationwarpdist.Value * AU;
                    sc.deliveryShips = (int)(ShipStartCarry.Value * 10) * 10;
                    sc.tripRangeShips = stationshipdist.Value > 60 ? 24000000000 : stationshipdist.Value * 2400000;
                }
            }
        }

        /// <summary>
        /// 铺40个轨道采集器
        /// </summary>
        private void SetGasStation()
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
            int inc;
            if (sum >= 40) package.TakeItem(2105, 40, out inc);
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
                pos = pos + vector3_3;
                PrebuildData prebuild = new PrebuildData();
                prebuild.protoId = (short)2105;
                prebuild.modelIndex = (short)117;
                prebuild.pos = pos + quaternion3 * Vector3.zero;
                prebuild.pos2 = pos + quaternion3 * Vector3.zero;
                prebuild.rot = quaternion3 * Quaternion.identity;
                prebuild.rot2 = quaternion3 * Quaternion.identity;
                prebuild.pickOffset = 0;
                prebuild.insertOffset = 0;
                prebuild.recipeId = 0;
                prebuild.filterId = 0;
                prebuild.paramCount = 0;
                prebuild.InitParametersArray(0);
                player.controller.actionBuild.clickTool.factory.AddPrebuildDataWithComponents(prebuild);
            }
        }

        /// <summary>
        /// 自动添加燃料到人造恒星
        /// </summary>
        private void AddFuelToAllStar()
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

        private void SetSignalId(int signalId)
        {
            if (LDB.signals.IconSprite(signalId) == null) return;
            pointsignalid = signalId;
            beltWindow.transform.Find("item-sign").GetComponent<Image>().sprite = LDB.signals.IconSprite(signalId);
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
                    else if(labpools.Count>0 || ejectorpools.Count>0 || stationpools.Count>0 || powergenGammapools.Count > 0)
                    {
                        //无需操作
                    }
                    else if(beltpools.Count>0)
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

        private void StopAutoBuildThread()
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

        private void InitBluePrintData()
        {
            pointeRecipetype = ERecipeType.None;
            stationpools = new List<int>();
            assemblerpools = new List<int>();
            labpools = new List<int>();
            beltpools = new List<int>();
            monitorpools = new List<int>();
            powergenGammapools = new List<int>();
            ejectorpools=new List<int>();
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
            if(Time.time - trashfunctiontime < 30)
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

        private string GetStationlogic(int i)
        {
            switch (i)
            {
                case 0: return "仓储";
                case 1: return "供应";
                case 2: return "需求";
            }
            return "";
        }

        /// <summary>
        /// 自动添加翘曲器
        /// </summary>
        private void AutoAddwarp()
        {
            if (player != null && player.mecha != null && player.mecha.thrusterLevel >= 3 && !player.mecha.HasWarper())
            {
                int itemID = 1210;
                int count = 20;
                player.package.TakeTailItems(ref itemID, ref count, out int inc);
                if (itemID <= 0 || count <= 0)
                {
                    return;
                }
                player.mecha.warpStorage.AddItem(itemID, count, inc, out int remaininc);
            }
        }

        private void AutoAddFuel()
        {
            if (player != null && player.mecha != null && player.mecha.reactorStorage.isEmpty)
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
                        player.mecha.reactorStorage.AddItem(itemID, count, inc, out int remaininc);
                        if (player.mecha.reactorStorage.isFull || player.mecha.reactorStorage.GetItemCount(itemID) > 100000) return;
                    }
                }
            }
        }

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

        private void BuyoutTech(TechProto tp)
        {
            techProto = tp;
            if (!GameMain.data.history.HasPreTechUnlocked(tp.ID))
            {
                UIRealtimeTip.Popup("存在未解锁的前置科技".Translate(), true, 0);
                return;
            }
            if (!GameMain.data.history.CheckPropertyAdequateForBuyout(tp.ID))
            {
                UIRealtimeTip.Popup("元数据不足".Translate(), true, 0);
                return;
            }
            if (!GameMain.data.history.propertyData.hasUsedProperty)
            {
                UIMessageBox.Show("初次使用元数据标题".Translate(), "初次使用元数据描述".Translate(), "取消".Translate(), "确定".Translate(), 2, null, new UIMessageBox.Response(DoBuyoutTech));
                return;
            }
            else
            {
                DoBuyoutTech();
            }
        }

        private void DoBuyoutTech()
        {
            if (techProto.ID == 1)
            {
                return;
            }
            if (techProto == null)
            {
                return;
            }
            GameMain.history.BuyoutTech(techProto.ID);
        }

        //数量与单位之间的转化
        public string TGMKinttostring(double num, string unit = "")
        {
            double tempcoreEnergyCap = num;
            int t = 0;
            if (num < 1000)
                return num + unit;
            while (tempcoreEnergyCap >= 1000)
            {
                t += 1;
                tempcoreEnergyCap /= 1000;
            }
            string coreEnergyCap = string.Format("{0:N2}", tempcoreEnergyCap);
            if (t == 0) return coreEnergyCap + unit;
            if (t == 1) return coreEnergyCap + "K" + unit;
            if (t == 2) return coreEnergyCap + "M" + unit;
            if (t == 3) return coreEnergyCap + "G" + unit;
            if (t == 4) return coreEnergyCap + "T" + unit;

            return "";
        }

        public void moveWindow_xl_first(ref float x, ref float y, ref float x_move, ref float y_move, ref bool movewindow, ref float tempx, ref float tempy, float x_width)
        {
            Vector2 temp = Input.mousePosition;
            if (temp.x > x && temp.x < x + x_width && maxheight - temp.y > y && maxheight - temp.y < y + 20)
            {
                if (Input.GetMouseButton(0))
                {
                    if (!movewindow)
                    {
                        x_move = x;
                        y_move = y;
                        tempx = temp.x;
                        tempy = maxheight - temp.y;
                    }
                    movewindow = true;
                    x = x_move + temp.x - tempx;
                    y = y_move + (maxheight - temp.y) - tempy;
                }
                else
                {
                    movewindow = false;
                    tempx = x;
                    tempy = y;
                }
            }
            else if (movewindow)
            {
                movewindow = false;
                x = x_move + temp.x - tempx;
                y = y_move + (maxheight - temp.y) - tempy;
            }
        }

        public void scaling_window(float x, float y, ref float x_move, ref float y_move)
        {
            Vector2 temp = Input.mousePosition;
            if (Input.GetMouseButton(0))
            {
                if ((temp.x + 10 > x_move && temp.x - 10 < x_move) && (maxheight - temp.y >= y_move && maxheight - temp.y <= y_move + y) || leftscaling)
                {
                    x -= temp.x - x_move;
                    x_move = temp.x;
                    leftscaling = true;
                    rightscaling = false;
                }
                if ((temp.x + 10 > x_move + x && temp.x - 10 < x_move + x) && (maxheight - temp.y >= y_move && maxheight - temp.y <= y_move + y) || rightscaling)
                {
                    x += temp.x - x_move - x;
                    rightscaling = true;
                    leftscaling = false;
                }
                if ((maxheight - temp.y + 10 > y + y_move && maxheight - temp.y - 10 < y + y_move) && (temp.x >= x_move && temp.x <= x_move + x) || bottomscaling)
                {
                    y += maxheight - temp.y - (y_move + y);
                    bottomscaling = true;
                }
                if (rightscaling || leftscaling)
                {
                    if ((maxheight - temp.y + 10 > y_move && maxheight - temp.y - 10 < y_move) && (temp.x >= x_move && temp.x <= x_move + x) || topscaling)
                    {
                        y -= maxheight - temp.y - y_move;
                        y_move = maxheight - temp.y;
                        topscaling = true;
                    }
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                rightscaling = false;
                leftscaling = false;
                bottomscaling = false;
                topscaling = false;
            }
            window_width = x;
            window_height = y;
        }

        #region 应用戴森球蓝图
        private void LoadDysonBluePrintData()
        {
            DysonPanelBluePrintNum = 0;
            tempDysonBlueprintData = new List<TempDysonBlueprintData>();
            string path = new StringBuilder(GameConfig.overrideDocumentFolder).Append(GameConfig.gameName).Append("/DysonBluePrint/").ToString();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Stream sm = Assembly.GetExecutingAssembly().GetManifestResourceStream("Auxilaryfunction.450节点+框架+32壳面.txt");
                byte[] bs = new byte[sm.Length];
                sm.Read(bs, 0, (int)sm.Length);
                sm.Close();
                UTF8Encoding con = new UTF8Encoding();
                string strdata = con.GetString(bs);
                try
                {
                    File.WriteAllText(Path.Combine(path, "450节点+框架+32壳面.txt"), strdata);
                }
                catch { }
            }
            var filesPath = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
            for (int i = 0; i < filesPath.Length; i++)
            {
                string str64Data = File.ReadAllText(filesPath[i]);
                var data = new TempDysonBlueprintData()
                {
                    name = filesPath[i].Substring(path.Length),
                    path = filesPath[i],
                };
                data.ReadDysonSphereBluePrint(str64Data);
                if (data.type != EDysonBlueprintType.None)
                {
                    tempDysonBlueprintData.Add(data);
                }
            }
            DysonPanelBluePrintNum += tempDysonBlueprintData.Count;

        }
        private string ReaddataFromFile(string path)
        {
            if (selectDysonBlueprintData.path == "")
            {
                UIMessageBox.Show(ErrorTitle.getTranslate(), "路径为空，请重新选择".getTranslate(), "确定".Translate(), 3, null);
                return "";
            }
            if (File.Exists(path))
            {
                try
                {
                    string data = File.ReadAllText(path);
                    return data;
                }catch
                {
                    UIMessageBox.Show(ErrorTitle.getTranslate(), "文件读取失败".getTranslate(), "确定".Translate(), 3, null);
                }
            }
            UIMessageBox.Show(ErrorTitle.getTranslate(), "文件不存在".getTranslate(), "确定".Translate(), 3, null);
            return "";

        }
        private void RemoveLayerById(int Id)
        {
            var dysonSphere = UIRoot.instance.uiGame.dysonEditor.selection.viewDysonSphere;
            if (dysonSphere == null)
                return;
            var layer = dysonSphere.layersIdBased[Id];
            if (layer != null)
            {
                if (layer.nodeCount > 0 || layer.frameCount > 0 || layer.shellCount > 0)
                {
                    UIMessageBox.Show("删除层级非空标题".Translate(), "删除层级非空描述".Translate(), "取消".Translate(), "确定".Translate(), 1, null, new UIMessageBox.Response(()=>{
                        if (dysonSphere.layersSorted.Contains(layer))
                        {
                            layer?.RemoveAllStructure();
                            dysonSphere.RemoveLayer(layer);
                        }
                    }));
                    VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                    return;
                }
                dysonSphere.RemoveLayer(layer);
            }
        }
        private void ApplySingleLayerBlueprint(string data,DysonSphere dysonSphere, int id)
        {
            DysonBlueprintDataIOError dysonBlueprintDataIOError = new DysonBlueprintData().FromBase64String(data, EDysonBlueprintType.SingleLayer, dysonSphere, dysonSphere.layersIdBased[id]);
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.OK)
            {
                VFAudio.Create("ui-click-1", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.StressExceed)
            {
                UIMessageBox.Show("戴森球蓝图纬度过高标题".Translate(), "戴森球蓝图纬度过高描述".Translate(), "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            if (dysonBlueprintDataIOError >= DysonBlueprintDataIOError.TypeMismatchSphere)
            {
                string arg = "";
                switch (dysonBlueprintDataIOError)
                {
                    case DysonBlueprintDataIOError.TypeMismatchSphere:
                        arg = "戴森球".Translate();
                        break;
                    case DysonBlueprintDataIOError.TypeMismatchSwarm:
                        arg = "戴森云".Translate();
                        break;
                    case DysonBlueprintDataIOError.TypeMismatchLayers:
                        arg = "戴森壳".Translate();
                        break;
                }
                string message = string.Format("戴森球蓝图类型不匹配描述".Translate(), arg, "戴森层级".Translate());
                UIMessageBox.Show("戴森球蓝图类型不匹配标题".Translate(), message, "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            UIMessageBox.Show("戴森球蓝图错误标题".Translate(), "戴森球蓝图错误描述".Translate(), "确定".Translate(), 3, null);
            VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
        }
        private void ApplyDysonLayersBlueprint(string data, DysonSphere dysonSphere)
        {
            DysonBlueprintDataIOError dysonBlueprintDataIOError = new DysonBlueprintData().FromBase64String(data, EDysonBlueprintType.Layers, dysonSphere, null);
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.OK)
            {
                VFAudio.Create("ui-click-1", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.StressExceed)
            {
                UIMessageBox.Show("戴森球蓝图纬度过高标题".Translate(), "戴森球蓝图纬度过高描述".Translate(), "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            if (dysonBlueprintDataIOError >= DysonBlueprintDataIOError.TypeMismatchSphere)
            {
                string arg = "";
                if (dysonBlueprintDataIOError != DysonBlueprintDataIOError.TypeMismatchSwarm)
                {
                    if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.TypeMismatchSingleLayer)
                    {
                        arg = "戴森层级".Translate();
                    }
                }
                else
                {
                    arg = "戴森云".Translate();
                }
                string message = string.Format("戴森球蓝图类型不匹配描述".Translate(), arg, "戴森壳".Translate());
                UIMessageBox.Show("戴森球蓝图类型不匹配标题".Translate(), message, "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                return;
            }
            UIMessageBox.Show("戴森球蓝图错误标题".Translate(), "戴森球蓝图错误描述".Translate(), "确定".Translate(), 3, null);
            VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
        }
        private void ApplyDysonBlueprint(string data, DysonSphere dysonSphere)
        {
            DysonBlueprintDataIOError dysonBlueprintDataIOError = new DysonBlueprintData().FromBase64String(data, EDysonBlueprintType.DysonSphere, dysonSphere, null);
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.OK)
            {
                VFAudio.Create("ui-click-1", null, Vector3.zero, true, 1, -1, -1L);
            }
            else if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.StressExceed)
            {
                UIMessageBox.Show("戴森球蓝图纬度过高标题".Translate(), "戴森球蓝图纬度过高描述".Translate(), "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
            }
            else if (dysonBlueprintDataIOError >= DysonBlueprintDataIOError.TypeMismatchSphere)
            {
                string arg = "";
                switch (dysonBlueprintDataIOError)
                {
                    case DysonBlueprintDataIOError.TypeMismatchSwarm:
                        arg = "戴森云".Translate();
                        break;
                    case DysonBlueprintDataIOError.TypeMismatchLayers:
                        arg = "戴森壳".Translate();
                        break;
                    case DysonBlueprintDataIOError.TypeMismatchSingleLayer:
                        arg = "戴森层级".Translate();
                        break;
                }
                string message = string.Format("戴森球蓝图类型不匹配描述".Translate(), arg, "戴森球无空格".Translate());
                UIMessageBox.Show("戴森球蓝图类型不匹配标题".Translate(), message, "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
            }
            else
            {
                UIMessageBox.Show("戴森球蓝图错误标题".Translate(), "戴森球蓝图错误描述".Translate(), "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
            }
        }
        private void ApplySwarmBlueprint(string data, DysonSphere dysonSphere)
        {
            DysonBlueprintDataIOError dysonBlueprintDataIOError = new DysonBlueprintData().FromBase64String(data, EDysonBlueprintType.SwarmOrbits, dysonSphere, null);
            if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.OK)
            {
                VFAudio.Create("ui-click-1", null, Vector3.zero, true, 1, -1, -1L);
            }
            else if (dysonBlueprintDataIOError >= DysonBlueprintDataIOError.TypeMismatchSphere)
            {
                string arg = "";
                if (dysonBlueprintDataIOError != DysonBlueprintDataIOError.TypeMismatchLayers)
                {
                    if (dysonBlueprintDataIOError == DysonBlueprintDataIOError.TypeMismatchSingleLayer)
                    {
                        arg = "戴森层级".Translate();
                    }
                }
                else
                {
                    arg = "戴森壳".Translate();
                }
                string message = string.Format("戴森球蓝图类型不匹配描述".Translate(), arg, "戴森云".Translate());
                UIMessageBox.Show("戴森球蓝图类型不匹配标题".Translate(), message, "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
            }
            else
            {
                UIMessageBox.Show("戴森球蓝图错误标题".Translate(), "戴森球蓝图错误描述".Translate(), "确定".Translate(), 3, null);
                VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
            }
        }
        private void ApplyDysonBlueprintManage(string data, DysonSphere dysonSphere, EDysonBlueprintType type, int id = -1)
        {
            if (data == "" || dysonSphere ==null) return;
            
            switch (type)
            {
                case EDysonBlueprintType.SingleLayer:
                    if (id == -1)
                    {
                        UIMessageBox.Show(ErrorTitle.getTranslate(), "单层壳蓝图请选择层级".Translate(), "确定".Translate(), 3, null);
                        VFAudio.Create("ui-error", null, Vector3.zero, true, 1, -1, -1L);
                        return;
                    }
                    ApplySingleLayerBlueprint(data, dysonSphere, id);
                    break;
                case EDysonBlueprintType.Layers:
                    UIMessageBox.Show("戴森球多层壳蓝图应用".Translate(), "确定应用多层壳蓝图吗？使用时鼠标最好不要放在戴森球上".Translate(), "取消".Translate(), "确定".Translate(), 1, null, new UIMessageBox.Response(() => {
                        ApplyDysonLayersBlueprint(data, dysonSphere);
                    }));
                    break;
                case EDysonBlueprintType.SwarmOrbits:
                    ApplySwarmBlueprint(data, dysonSphere);
                    break;
                case EDysonBlueprintType.DysonSphere:
                    ApplyDysonBlueprint(data, dysonSphere);
                    break;
            }
        }
        #endregion
    }

    public class TempDysonBlueprintData
    {
        public string path="";
        public string name="";
        public EDysonBlueprintType type;
        public string TypeName="";

        private bool VerifyDysonBluePrintData(string Content)
        {
            if (Content.Length < 23 || Content.Substring(0, 5) != "DYBP:")
            {
                return false;
            }

            int firstindex = Content.IndexOf('"');
            int lastIndex = Content.LastIndexOf('"');
            if (firstindex == -1 || lastIndex == -1 || lastIndex < firstindex + 32)
            {
                return false;
            }
            string[] array = Content.Substring(5, firstindex - 5).Split(',');
            //Signature
            if (array.Length < 3 || Math.Max(Content.Length - 36, 0) > lastIndex && !MD5F.Compute(Content.Substring(0, lastIndex)).Equals(Content.Substring(lastIndex + 1, 32), StringComparison.Ordinal))
            {
                return false;
            }
            return true;
        }

        public void ReadDysonSphereBluePrint(string Content)
        {
            if (VerifyDysonBluePrintData(Content))
            {
                int firstindex = Content.IndexOf('"');
                string[] array = Content.Substring(5, firstindex - 5).Split(',');
                type = (EDysonBlueprintType)int.Parse(array[3]);
                switch (type)
                {
                    case EDysonBlueprintType.SingleLayer:
                        TypeName = "单层壳";
                        break;
                    case EDysonBlueprintType.Layers:
                        TypeName = "多层壳";
                        break;
                    case EDysonBlueprintType.SwarmOrbits:
                        TypeName = "戴森云";
                        break;
                    case EDysonBlueprintType.DysonSphere:
                        TypeName = "戴森球(包括壳、云)";
                        break;
                }
            }
        }
    }

}
