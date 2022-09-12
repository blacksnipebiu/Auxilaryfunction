using BepInEx;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.Events;
using static Auxilaryfunction.Constant;
using System.Reflection;
using System.Threading;
using System.IO;

namespace Auxilaryfunction
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInProcess(GAME_PROCESS)]
    public class Auxilaryfunction : BaseUnityPlugin
    {
        public const long AU = 40000;
        public const string GUID = "cn.blacksnipe.dsp.Auxilaryfunction";
        public const string NAME = "Auxilaryfunction";
        public const string VERSION = "1.7.4";
        private const string GAME_PROCESS = "DSPGAME.exe";
        public int stationindex = 4;
        public int locallogic = 0;
        public int remotelogic = 2;
        public int autoaddtechid = 0;
        public int maxheight;
        public int windowmaxheight;
        public int recipewindowx;
        public int recipewindowy;
        public int pointlayerid = 0;
        public int pointsignalid = 0;
        public List<int> assemblerpools;
        public List<int> labpools;
        public List<int> fuelItems = new List<int>();
        public List<int> beltpools = new List<int>();
        public List<int> monitorpools = new List<int>();
        public List<int> ejectorpools = new List<int>();
        public List<int> pointlayeridlist = new List<int>();
        public List<int> stationpools = new List<int>();
        public List<int> readyresearch = new List<int>();
        private static Dictionary<int, bool> FuelFilter = new Dictionary<int, bool>();
        public string[] stationname = new string[6] { "星球矿机", "垃圾站", "星球无限供货机", "星球量子传输站", "星系量子传输站", "设置翘曲需求" };
        public float slowconstructspeed = 1;
        public float drawdysonlasttime;
        public float trashlasttime;
        public float autobuildtime;
        public float window_x_move = 200;
        public float window_y_move = 200;
        public float temp_window_x = 10;
        public float temp_window_y = 200;
        public float window_x = 300;
        public float window_y = 200;
        public float batchnum = 1;
        public float window_width = 800;
        public float window_height = 710;
        public static float upsfix = 1;
        public static bool temp;
        public static bool simulatorrender;
        public static bool simulatorchanging;
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
        public bool ready;
        public bool firstStart = true;
        public bool moving;
        public bool leftscaling;
        public bool startdrawdyson;
        public bool rightscaling;
        public bool topscaling;
        public bool bottomscaling;
        public bool showwindow;
        public bool ChangeQuickKey;
        public bool ChangingQuickKey;
        public bool autoaddtech;
        public bool changename;
        public bool auto_setejector_start;
        public bool autoAddwarp_start = false;
        public bool autoAddFuel_start;
        public static Dictionary<int, List<int>> EjectorDictionary;
        public static List<ItemProto> ItemList = new List<ItemProto>();
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
        public List<TechProto> techlist = null;
        public ERecipeType pointeRecipetype = ERecipeType.None;
        public static bool closecollider = false;
        public static bool slowdownsail = false;
        public static bool stopDysonSphere = false;
        public static bool stopfactory = false;
        public static bool fly = false;
        public static bool changeups = false;
        public static bool autobuildgetitem = false;
        public static PlanetData flyfocusPlanet;
        public static readonly FieldInfo _uiGridSplit_sliderImg = AccessTools.Field(typeof(UIGridSplit), "sliderImg");
        public static readonly FieldInfo _uiGridSplit_valueText = AccessTools.Field(typeof(UIGridSplit), "valueText");
        public static ConfigEntry<Boolean> closeplayerflyaudio;
        public static ConfigEntry<Boolean> autosetSomevalue_bool;
        public static ConfigEntry<Boolean> noscaleuitech_bool;
        public static ConfigEntry<Boolean> close_alltip_bool;
        public static ConfigEntry<Boolean> autowarpcommand;
        public static ConfigEntry<Boolean> autonavigation_bool;
        public static ConfigEntry<Boolean> autocleartrash_bool;
        public static ConfigEntry<Boolean> autoabsorttrash_bool;
        public static ConfigEntry<Boolean> stationcopyItem_bool;
        public static ConfigEntry<Boolean> blueprintcopytopaste_bool;
        public static ConfigEntry<Boolean> blueprintdelete_bool;
        public static ConfigEntry<Boolean> blueprintrevoke_bool;
        public static ConfigEntry<Boolean> blueprintsetrecipe_bool;
        public static ConfigEntry<Boolean> norender_shipdrone_bool;
        public static ConfigEntry<Boolean> norender_lab_bool;
        public static ConfigEntry<Boolean> norender_beltitem;
        public static ConfigEntry<Boolean> norender_dysonshell_bool;
        public static ConfigEntry<Boolean> norender_dysonswarm_bool;
        public static ConfigEntry<Boolean> norender_entity_bool;
        public static ConfigEntry<Boolean> norender_powerdisk_bool;
        public static ConfigEntry<Boolean> autoaddtech_bool;
        public static ConfigEntry<Boolean> Quickstop_bool;
        public static ConfigEntry<Boolean> automovetounbuilt;
        public static ConfigEntry<Boolean> upsquickset;
        public static ConfigEntry<Boolean> onlygetbuildings;
        public static ConfigEntry<Boolean> autosavetimechange;
        public static ConfigEntry<Boolean> auto_supply_station;
        public static ConfigEntry<Boolean> auto_setejector_bool;
        public static ConfigEntry<Boolean> ShowStationInfo;
        public static ConfigEntry<Boolean> CloseUIpanel;
        public static ConfigEntry<Boolean> KeepBeltHeight;
        public static ConfigEntry<Boolean> autoAddFuel;
        public static ConfigEntry<Boolean> autoAddwarp;
        public static ConfigEntry<double> stationwarpdist;
        public static ConfigEntry<KeyboardShortcut> QuickKey;
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
        public static Thread autobuildThread;

        public static int maxCount = 0;
        public static GameObject[] tip = new GameObject[maxCount];
        public static GameObject stationTip;
        public static GameObject tipPrefab;
        public static GameObject testitem;
        public static GameObject testitem1;
        public static int count;
        private GameObject AuxilaryPanel;
        private GameObject ui_AuxilaryPanelPanel;

        void Start()
        {
            new Harmony(GUID).PatchAll();
            AssetBundle assetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Auxilaryfunction.auxilarypanel"));
            //AssetBundle assetBundle = AssetBundle.LoadFromFile("E:/game/game1/New Unity Project (4)/AssetBundles/StandaloneWindows64/panel");
            AuxilaryPanel = assetBundle.LoadAsset<GameObject>("AuxilaryPanel");
            trashlasttime = Time.time;
            drawdysonlasttime = Time.time;
            QuickKey = Config.Bind("打开窗口快捷键", "Key", new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftAlt));
            tempShowWindow = QuickKey.Value;
            AuxilaryTranslate.regallTranslate();
            auto_setejector_bool = Config.Bind("自动配置太阳帆弹射器", "auto_setejector_bool", false);

            auto_supply_station = Config.Bind("自动配置新运输站", "auto_supply_station", false);
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
            blueprintdelete_bool = Config.Bind("蓝图删除", "blueprintdelete_bool", false);
            blueprintrevoke_bool = Config.Bind("蓝图撤销", "blueprintrevoke_bool", false);
            blueprintsetrecipe_bool = Config.Bind("蓝图配方", "blueprintsetrecipe_bool", false);
            blueprintcopytopaste_bool = Config.Bind("蓝图直接粘贴", "blueprintcopytopaste_bool", false);
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
            norender_lab_bool = Config.Bind("不渲染研究室", "norender_shipdrone_bool", false);
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
            maxheight = Screen.height;
            scrollPosition[0] = 0;
            pdselectscrollPosition[0] = 0;
            mytexture = new Texture2D(10, 10);
            for (int i = 0; i < mytexture.width; i++)
                for (int j = 0; j < mytexture.height; j++)
                    mytexture.SetPixel(i, j, new Color(0, 0, 0, 1));
            mytexture.Apply();

            styleblue.fontStyle = FontStyle.Bold;
            styleblue.fontSize = 20;
            styleblue.normal.textColor = new Color32(167, 255, 255, 255);
            styleyellow.fontStyle = FontStyle.Bold;
            styleyellow.fontSize = 20;
            styleyellow.normal.textColor = new Color32(240, 191, 103, 255);
            StationinfoStart();
        }
        void Update()
        {
            ChangeQuickKeyMethod();
            StartAndStopGame();
            StationinfoUpdate();
            if (Input.GetKeyDown(KeyCode.F9))
            {
                //temp = !temp;
            }
            if (GameMain.instance != null)
            {
                if (autosavetimechange.Value && UIAutoSave.autoSaveTime != autosavetime.Value)
                {
                    DSPGame.globalOption.autoSaveTime = autosavetime.Value;
                    DSPGame.globalOption.Apply();
                    UIAutoSave.autoSaveTime = autosavetime.Value;
                }
                if (!GameMain.instance.running)
                {
                    firstStart = false;
                    closecollider = false;
                    if (autobuildThread != null)
                    {
                        if (autobuildThread.ThreadState == ThreadState.WaitSleepJoin)
                            autobuildThread.Interrupt();
                        else
                            autobuildThread.Abort();
                        autobuildThread = null;
                    }
                    ItemList = new List<ItemProto>(LDB.items.dataArray);
                    readyresearch = new List<int>();
                    upsfix = 1;
                    autoaddtech = false;
                }
                if (GameMain.instance.running && !firstStart)
                {
                    ready = true;
                    EjectorDictionary = new Dictionary<int, List<int>>();
                    techlist = new List<TechProto>(LDB.techs.dataArray);
                    if (fuelItems.Count == 0)
                    {
                        foreach (var item in LDB.items.dataArray)
                        {
                            if (item.HeatValue > 0)
                            {
                                fuelItems.Add(item.ID);
                                FuelFilter.Add(item.ID, false);
                            }
                        }
                    }
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
                    firstStart = true;
                }
                if (GameMain.history != null && GameMain.mainPlayer != null && firstStart)
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
                    if (GameMain.history.techQueueLength == 0 && autoaddtech_bool.Value && autoaddtechid > 0)
                    {
                        GameMain.history.EnqueueTech(autoaddtechid);
                    }
                    TrashFunction();
                    BluePrintoptimize();
                }
            }

            if (QuickKey.Value.IsDown() && !ChangingQuickKey && ready)
            {
                showwindow = !showwindow;
                if (ui_AuxilaryPanelPanel == null)
                {
                    ui_AuxilaryPanelPanel = UnityEngine.Object.Instantiate<GameObject>(AuxilaryPanel, UIRoot.instance.overlayCanvas.transform);
                }
                ui_AuxilaryPanelPanel.SetActive(showwindow && !CloseUIpanel.Value);
            }
            if (showwindow && Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) { scale.Value++; changescale = true; }
                if (Input.GetKeyDown(KeyCode.DownArrow)) { scale.Value--; changescale = true; }
                if (scale.Value < 5) scale.Value = 5;
                if (scale.Value > 35) scale.Value = 35;
            }
            if (autoaddtech && LDB.techs != null && LDB.techs.dataArray != null && GameMain.history != null)
            {
                foreach (TechProto tp in LDB.techs.dataArray)
                {
                    if (readyresearch.Contains(tp.ID) || !GameMain.history.CanEnqueueTech(tp.ID) || tp.MaxLevel > 20 || GameMain.history.TechUnlocked(tp.ID)) continue;
                    bool condition = true;
                    foreach (int ip in tp.Items)
                    {
                        if (GameMain.history.ItemUnlocked(ip)) continue;
                        condition = false;
                        break;
                    }
                    if (!condition) continue;
                    readyresearch.Add(tp.ID);
                }
            }
            for (int i = 0; i < readyresearch.Count && GameMain.history != null && GameMain.history.techQueueLength < 7; i++)
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
            if (GameMain.mainPlayer != null && GameMain.mainPlayer.navigation != null && GameMain.mainPlayer.navigation._indicatorAstroId != 0)
            {
                if (GUI.Button(new Rect(10, 250, 150, 60), fly ? "停止导航".getTranslate() : "继续导航".getTranslate()))
                {
                    fly = !fly;
                    if (fly) slowdownsail = false;
                }
                if (GUI.Button(new Rect(10, 300, 150, 60), "取消方向指示".getTranslate()))
                {
                    GameMain.mainPlayer.navigation._indicatorAstroId = 0;
                }
            }
            if (automovetounbuilt.Value && GameMain.mainPlayer != null && GameMain.localPlanet != null && GameMain.localPlanet.factory != null && GameMain.localPlanet.factory.prebuildCount > 0 && GameMain.mainPlayer.movementState == EMovementState.Fly)
            {
                if (GUI.Button(new Rect(10, 360, 150, 60), closecollider ? "停止寻找未完成建筑".getTranslate() : "开始寻找未完成建筑".getTranslate()))
                {
                    closecollider = !closecollider;
                    GameMain.mainPlayer.gameObject.GetComponent<SphereCollider>().enabled = !closecollider;
                    GameMain.mainPlayer.gameObject.GetComponent<CapsuleCollider>().enabled = !closecollider;
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
            }
            else if (closecollider)
            {
                closecollider = !closecollider;
                GameMain.mainPlayer.gameObject.GetComponent<SphereCollider>().enabled = !closecollider;
                GameMain.mainPlayer.gameObject.GetComponent<CapsuleCollider>().enabled = !closecollider;
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
            if (closecollider && GameMain.localPlanet.gasItems == null && GUI.Button(new Rect(10, 420, 150, 60), autobuildgetitem ? "停止自动补充材料".getTranslate() : "开始自动补充材料".getTranslate()))
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
                                GameMain.localPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].SetRecipe(rp.ID, GameMain.localPlanet.factory.entitySignPool);
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
                                GameMain.localPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].SetRecipe(0, GameMain.localPlanet.factory.entitySignPool);
                            }
                        }
                        if (GUI.Button(new Rect(recipewindowx, maxheight - recipewindowy + tempheight * 50, 200, 50), "额外产出".getTranslate()))
                        {
                            for (int j = 0; j < assemblerpools.Count; j++)
                            {
                                if (GameMain.localPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].productive)
                                    GameMain.localPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].forceAccMode = false;
                            }
                        }
                        if (GUI.Button(new Rect(recipewindowx + 200, maxheight - recipewindowy + tempheight * 50, 200, 50), "生产加速".getTranslate()))
                        {
                            for (int j = 0; j < assemblerpools.Count; j++)
                            {
                                if (GameMain.localPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].productive)
                                    GameMain.localPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].forceAccMode = true;
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
                                GameMain.localPlanet.factory.factorySystem.labPool[labpools[j]].SetFunction(false, LDB.items.Select(LabComponent.matrixIds[i]).maincraft.ID, 0, GameMain.localPlanet.factory.entitySignPool);
                            }
                        }
                    if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 50, maxheight - recipewindowy + tempheight++ * 50, 50, 50), "无".getTranslate()))
                    {
                        for (int j = 0; j < labpools.Count; j++)
                        {
                            GameMain.localPlanet.factory.factorySystem.labPool[labpools[j]].SetFunction(false, 0, 0, GameMain.localPlanet.factory.entitySignPool);
                        }
                    }
                    if (GUI.Button(new Rect(recipewindowx, maxheight - recipewindowy + tempheight++ * 50, 200, 50), "科研模式".getTranslate()))
                    {
                        for (int j = 0; j < labpools.Count; j++)
                        {
                            GameMain.localPlanet.factory.factorySystem.labPool[labpools[j]].SetFunction(true, 0, GameMain.history.currentTech, GameMain.localPlanet.factory.entitySignPool);
                        }
                    }
                    if (GUI.Button(new Rect(recipewindowx, maxheight - recipewindowy + tempheight * 50, 200, 50), "额外产出".getTranslate()))
                    {
                        for (int j = 0; j < labpools.Count; j++)
                        {
                            if (GameMain.localPlanet.factory.factorySystem.labPool[labpools[j]].productive)
                                GameMain.localPlanet.factory.factorySystem.labPool[labpools[j]].forceAccMode = false;
                        }
                    }
                    if (GUI.Button(new Rect(recipewindowx + 200, maxheight - recipewindowy + tempheight * 50, 200, 50), "生产加速".getTranslate()))
                    {
                        for (int j = 0; j < labpools.Count; j++)
                        {
                            if (GameMain.localPlanet.factory.factorySystem.labPool[labpools[j]].productive)
                                GameMain.localPlanet.factory.factorySystem.labPool[labpools[j]].forceAccMode = true;
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
                                    GameMain.localPlanet.factory.factorySystem.ejectorPool[ejectorpools[k]].SetOrbit(orbitid);
                                }
                            }
                        }
                        tempheight++;
                    }
                }
                else if (changename && stationpools.Count > 0)
                {
                    if (tempheight + tempwidth > 0) tempheight++;
                    tempwidth = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 130, maxheight - recipewindowy + tempheight * 50, 130, 50), stationname[i]))
                        {

                            for (int j = 0; j < stationpools.Count; j++)
                            {
                                StationComponent sc = GameMain.localPlanet.factory.transport.stationPool[stationpools[j]];
                                if (i == 5)
                                {
                                    if (sc.storage[4].count > 0 && sc.storage[4].itemId != 1210)
                                        GameMain.mainPlayer.TryAddItemToPackage(sc.storage[4].itemId, sc.storage[4].count, 0, false);
                                    GameMain.localPlanet.factory.transport.SetStationStorage(stationpools[j], stationindex, 1210, (int)batchnum * 100, (ELogisticStorage)locallogic, (ELogisticStorage)remotelogic, GameMain.mainPlayer);
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
                    int tempy = maxheight - recipewindowy + tempheight * 50;
                    batchnum = (int)GUI.HorizontalSlider(new Rect(tempx, tempy, 150, 30), batchnum, 0, 100);
                    GUI.Label(new Rect(tempx, tempy + 30, 100, 30), "上限".getTranslate() + ":" + batchnum * 100);
                    if (GUI.Button(new Rect(tempx + 150, tempy, 100, 30), "第".getTranslate() + (stationindex + 1) + "格".getTranslate()))
                    {
                        stationindex++;
                        stationindex %= 5;
                    }
                    if (GUI.Button(new Rect(tempx + 250, tempy, 100, 30), "本地".getTranslate() + getStationlogic(locallogic)))
                    {
                        locallogic++;
                        locallogic %= 3;
                    }
                    if (GUI.Button(new Rect(tempx + 350, tempy, 100, 30), "星际".getTranslate() + getStationlogic(remotelogic)))
                    {
                        remotelogic++;
                        remotelogic %= 3;
                    }
                    tempheight++;
                    if (GUI.Button(new Rect(recipewindowx, maxheight - recipewindowy + tempheight * 50, 130, 50), "粘贴物流站配方".getTranslate()))
                    {
                        PlanetFactory factory = GameMain.localPlanet.factory;
                        for (int j = 0; j < stationpools.Count; j++)
                        {
                            StationComponent sc = factory.transport.stationPool[stationpools[j]];
                            for (int i = 0; i < sc.storage.Length && i < 5; i++)
                            {
                                if (stationcopyItem[i, 0] > 0)
                                {
                                    if (sc.storage[i].count > 0 && sc.storage[i].itemId != stationcopyItem[i, 0])
                                        GameMain.mainPlayer.TryAddItemToPackage(sc.storage[i].itemId, sc.storage[i].count, 0, false);
                                    factory.transport.SetStationStorage(stationpools[j], i, stationcopyItem[i, 0], stationcopyItem[i, 1], (ELogisticStorage)stationcopyItem[i, 2]
                                        , (ELogisticStorage)stationcopyItem[i, 3], GameMain.mainPlayer);
                                }
                                else
                                    factory.transport.SetStationStorage(stationpools[j], i, 0, 0, ELogisticStorage.None, ELogisticStorage.None, GameMain.mainPlayer);
                            }
                        }
                        stationpools.Clear();
                    }
                }

            }
            else
            {
                labpools = new List<int>();
                stationpools = new List<int>();
                ejectorpools = new List<int>();
                assemblerpools = new List<int>();
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
            }
            if (limitmaterial != GUI.Toggle(new Rect(heightdis / 2, 10 + heightdis, widthlen2, heightdis), limitmaterial, "限制材料".getTranslate()))
            {
                limitmaterial = !limitmaterial;
                if (limitmaterial) TextTech = true;
            }
            if (autoaddtech != GUI.Toggle(new Rect(heightdis / 2, 10 + heightdis * 2, widthlen2, heightdis), autoaddtech, "自动乱点".getTranslate()))
            {
                autoaddtech = !autoaddtech;
            }

            GUILayout.EndArea();

        }
        public void DoMyWindow1(int winId)
        {
            int tempheight = 0;
            int heightdis = scale.Value * 2;
            int widthlen1 = 75 * scale.Value / 8;
            int widthlen2 = Localization.language != Language.zhCN ? 15 * scale.Value : 25 * scale.Value / 2;
            int oneareamaxwidth = Localization.language != Language.zhCN ? widthlen1 + widthlen2 : widthlen2;
            GUILayout.BeginArea(new Rect(10, 20, window_width + 100, window_height));
            if (TextTech)
            {
                scrollPosition = GUI.BeginScrollView(new Rect(0, 0, window_width - 10, window_height - heightdis), scrollPosition, new Rect(0, 0, 37 * heightdis, windowmaxheight), true, true);

                GUI.Label(new Rect(0, 0, heightdis * 3, heightdis), "准备研究".getTranslate());
                int i = 0;
                tempheight += heightdis;
                for (; i < readyresearch.Count; i++)
                {
                    TechProto tp = LDB.techs.Select(readyresearch[i]);
                    if (i != 0 && i % 7 == 0) tempheight += heightdis * 4;
                    if (GUI.Button(new Rect(i % 7 * heightdis * 5, tempheight, heightdis * 5, heightdis * 2), tp.ID < 2000 ? tp.name : (tp.name + tp.Level)))
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
                        GUI.Button(new Rect(i % 7 * heightdis * 5 + k++ * heightdis, heightdis * 2 + tempheight, heightdis, heightdis), ip.iconSprite.texture);
                    }
                    k = 0;
                    foreach (RecipeProto rp in tp.unlockRecipeArray)
                    {
                        GUI.Button(new Rect(i % 7 * heightdis * 5 + k++ * heightdis, heightdis * 3 + tempheight, heightdis, heightdis), rp.iconSprite.texture);
                    }
                }
                tempheight += heightdis * 4;
                GUI.Label(new Rect(0, tempheight, heightdis * 3, heightdis), "科技".getTranslate());
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
                    if (i != 0 && i % 5 == 0) tempheight += heightdis * 4;
                    if (GUI.Button(new Rect(i % 5 * heightdis * 5, tempheight, heightdis * 5, heightdis * 2), tp.name))
                    {
                        readyresearch.Add(tp.ID);
                    }
                    int k = 0;
                    foreach (ItemProto ip in tp.itemArray)
                    {
                        GUI.Button(new Rect(i % 5 * heightdis * 5 + k++ * heightdis, heightdis * 2 + tempheight, heightdis, heightdis), ip.iconSprite.texture);
                    }
                    k = 0;
                    foreach (RecipeProto rp in tp.unlockRecipeArray)
                    {
                        GUI.Button(new Rect(i % 5 * heightdis * 5 + k++ * heightdis, heightdis * 3 + tempheight, heightdis, heightdis), rp.iconSprite.texture);
                    }
                    i++;
                }
                tempheight += heightdis * 4;
                GUI.Label(new Rect(0, tempheight, heightdis * 4, heightdis), "升级".getTranslate());
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
                    if (i != 0 && i % 5 == 0) tempheight += heightdis * 3;
                    if (GUI.Button(new Rect(i % 5 * heightdis * 5, tempheight, heightdis * 5, heightdis * 2), tp.name + tp.Level))
                    {
                        readyresearch.Add(tp.ID);
                    }
                    int k = 0;
                    foreach (ItemProto ip in tp.itemArray)
                    {
                        GUI.Button(new Rect(i % 5 * heightdis * 5 + k++ * heightdis, heightdis * 2 + tempheight, heightdis, heightdis), ip.iconSprite.texture);
                    }
                    i++;
                }
                windowmaxheight = tempheight + heightdis * 4;
                GUI.EndScrollView();
            }
            else
            {
                int finalheight = 0;
                oneareamaxwidth = widthlen1 + widthlen2;
                {
                    GUILayout.BeginArea(new Rect(10, 0, 20 + widthlen1 + widthlen2, window_height));
                    auto_supply_station.Value = GUI.Toggle(new Rect(10, heightdis * tempheight, widthlen1, heightdis), auto_supply_station.Value, "自动配置新运输站".getTranslate());
                    autosetstationconfig = GUI.Toggle(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, heightdis), autosetstationconfig, "配置参数".getTranslate());
                    if (autosetstationconfig)
                    {
                        auto_supply_drone.Value = (int)GUI.HorizontalSlider(new Rect(10, 5 + heightdis * tempheight, widthlen1, heightdis), auto_supply_drone.Value, 0, 100);
                        GUI.Label(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, heightdis), auto_supply_drone.Value + " " + "填充飞机数量".getTranslate());
                        auto_supply_ship.Value = (int)GUI.HorizontalSlider(new Rect(10, 5 + heightdis * tempheight, widthlen1, heightdis), auto_supply_ship.Value, 0, 10);
                        GUI.Label(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, heightdis), auto_supply_ship.Value + " " + "填充飞船数量".getTranslate());
                        stationmaxpowerpertick.Value = (int)GUI.HorizontalSlider(new Rect(10, 5 + heightdis * tempheight, widthlen1, heightdis), stationmaxpowerpertick.Value, 30, 300);
                        GUI.Label(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, 30), (int)stationmaxpowerpertick.Value + "MW " + "最大充电功率".getTranslate());

                        stationdronedist.Value = (int)GUI.HorizontalSlider(new Rect(10, 5 + heightdis * tempheight, widthlen1, heightdis), (float)stationdronedist.Value, 20, 180);
                        GUI.Label(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, heightdis), stationdronedist.Value + "° " + "运输机最远路程".getTranslate());
                        stationshipdist.Value = (int)GUI.HorizontalSlider(new Rect(10, 5 + heightdis * tempheight, widthlen1, heightdis), (float)stationshipdist.Value, 1, 61);
                        GUI.Label(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, heightdis), (stationshipdist.Value == 61 ? "∞ " : stationshipdist.Value + "ly ") + "运输船最远路程".getTranslate());
                        stationwarpdist.Value = (int)GUI.HorizontalSlider(new Rect(10, 5 + heightdis * tempheight, widthlen1, heightdis), (float)stationwarpdist.Value, 0.5f, 60);
                        if (stationwarpdist.Value == 0) stationwarpdist.Value = 0.5;
                        GUI.Label(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, heightdis), stationwarpdist.Value + "AU " + "曲速启用路程".getTranslate());
                        DroneStartCarry.Value = GUI.HorizontalSlider(new Rect(10, 5 + heightdis * tempheight, widthlen1, heightdis), DroneStartCarry.Value, 0.01f, 1);
                        DroneStartCarry.Value = DroneStartCarry.Value == 0 ? 0.01f : DroneStartCarry.Value;

                        GUI.Label(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, heightdis), ((int)(DroneStartCarry.Value * 10) * 10 == 0 ? "1" : "" + (int)(DroneStartCarry.Value * 10) * 10) + "% " + "运输机起送量".getTranslate());
                        ShipStartCarry.Value = GUI.HorizontalSlider(new Rect(10, 5 + heightdis * tempheight, widthlen1, heightdis), ShipStartCarry.Value, 0.1f, 1);
                        GUI.Label(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, heightdis), (int)(ShipStartCarry.Value * 10) * 10 + "% " + "运输船起送量".getTranslate());
                        auto_supply_warp.Value = (int)GUI.HorizontalSlider(new Rect(10, 5 + heightdis * tempheight, widthlen1, heightdis), auto_supply_warp.Value, 0, 50);
                        GUI.Label(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, heightdis), auto_supply_warp.Value + " " + "翘曲填充数量".getTranslate());
                        veincollectorspeed.Value = (int)GUI.HorizontalSlider(new Rect(10, 5 + heightdis * tempheight, widthlen1, heightdis), veincollectorspeed.Value, 10, 30);
                        GUI.Label(new Rect(20 + widthlen1, heightdis * tempheight++, widthlen2, heightdis * 2), veincollectorspeed.Value / 10.0f + " " + "大型采矿机采矿速率".getTranslate());
                    }
                    tempheight += Localization.language != Language.zhCN ? 1 : 0;
                    if (GUI.Button(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), "铺满轨道采集器".getTranslate())) setgasstation();
                    if (GUI.Button(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), "填充当前星球飞机飞船、翘曲器".getTranslate())) addDroneShiptooldstation();
                    if (GUI.Button(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), "批量配置当前星球物流站".getTranslate())) changeallstationconfig();
                    if (GUI.Button(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), "批量配置当前星球大型采矿机采矿速率".getTranslate())) changeallveincollectorspeedconfig();

                    norender_dysonshell_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), norender_dysonshell_bool.Value, "不渲染戴森壳".getTranslate());
                    norender_dysonswarm_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), norender_dysonswarm_bool.Value, "不渲染戴森云".getTranslate());
                    norender_lab_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), norender_lab_bool.Value, "不渲染研究站".getTranslate());
                    norender_beltitem.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), norender_beltitem.Value, "不渲染传送带货物".getTranslate());
                    norender_shipdrone_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), norender_shipdrone_bool.Value, "不渲染运输船和飞机".getTranslate());
                    norender_entity_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), norender_entity_bool.Value, "不渲染实体".getTranslate());
                    if (simulatorrender != GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), simulatorrender, "不渲染全部".getTranslate()))
                    {
                        simulatorrender = !simulatorrender;
                        simulatorchanging = true;
                    }
                    norender_powerdisk_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), norender_powerdisk_bool.Value, "不渲染电网覆盖".getTranslate());
                    closeplayerflyaudio.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), closeplayerflyaudio.Value, "关闭玩家走路飞行声音".getTranslate());

                    finalheight = heightdis * (tempheight + 1);
                    GUILayout.EndArea();

                }

                oneareamaxwidth = Localization.language != Language.zhCN ? widthlen1 + widthlen2 : widthlen2;
                {
                    tempheight = 0;
                    GUILayout.BeginArea(new Rect(30 + widthlen1 + widthlen2, 0, oneareamaxwidth + 10, window_height));

                    if (autoaddtech_bool.Value != GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), autoaddtech_bool.Value, "自动添加科技队列".getTranslate()))
                    {
                        autoaddtech_bool.Value = !autoaddtech_bool.Value;
                        if (!autoaddtech_bool.Value) autoaddtechid = 0;
                    }
                    if (autoaddtech_bool.Value)
                    {
                        if (GUI.Button(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), LDB.techs.Select(autoaddtechid) == null ? "未选择".getTranslate() : LDB.techs.Select(autoaddtechid).name))
                        {
                            selectautoaddtechid = !selectautoaddtechid;
                        }
                        if (techlist != null && selectautoaddtechid)
                        {
                            for (int i = 0; i < techlist.Count; i++)
                            {
                                TechState techstate = GameMain.history.techStates[techlist[i].ID];
                                if (techstate.curLevel < techstate.maxLevel && techstate.maxLevel > 10)
                                {
                                    if (GUI.Button(new Rect(10, heightdis * tempheight++, widthlen2, heightdis), techlist[i].name + " " + techstate.curLevel + " " + techstate.maxLevel))
                                    {
                                        autoaddtechid = techlist[i].ID;
                                    }
                                }
                            }
                        }

                    }
                    autoAddwarp.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), autoAddwarp.Value, "自动添加翘曲器");
                    autoAddFuel.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), autoAddFuel.Value, "自动添加燃料");
                    int iconIndex=0;
                    foreach(var itemID in fuelItems)
                    {
                        GUIStyle style = new GUIStyle();
                        if (FuelFilter[itemID])
                            style.normal.background = Texture2D.whiteTexture;
                        if (GUI.Button(new Rect(10+ iconIndex++*heightdis, heightdis * tempheight, heightdis, heightdis), LDB.items.Select(itemID).iconSprite.texture, style))
                        {
                            FuelFilter[itemID] = !FuelFilter[itemID];
                        }
                        if (iconIndex % 6 == 0)
                        {
                            iconIndex = 0;
                            tempheight++;
                        }
                    }
                    tempheight++;
                    auto_setejector_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), auto_setejector_bool.Value, "自动配置太阳帆弹射器".getTranslate());
                    autonavigation_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), autonavigation_bool.Value, "自动导航".getTranslate());
                    autowarpcommand.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), autowarpcommand.Value, "自动导航使用曲速".getTranslate());
                    GUI.Label(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), "自动使用翘曲器距离".getTranslate() + ":");
                    autowarpdistance.Value = GUI.HorizontalSlider(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), autowarpdistance.Value, 0, 30);
                    GUI.Label(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), String.Format("{0:N2}", autowarpdistance.Value) + "光年".getTranslate() + "\n");
                    automovetounbuilt.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), automovetounbuilt.Value, "自动飞向未完成建筑".getTranslate());
                    close_alltip_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), close_alltip_bool.Value, "一键闭嘴".getTranslate());
                    noscaleuitech_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), noscaleuitech_bool.Value, "科技面板选中不缩放".getTranslate());
                    blueprintdelete_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), blueprintdelete_bool.Value, "蓝图删除".getTranslate() + "(ctrl+X）");
                    blueprintrevoke_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), blueprintrevoke_bool.Value, "蓝图撤销".getTranslate() + "(ctrl+Z)");
                    blueprintsetrecipe_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), blueprintsetrecipe_bool.Value, "蓝图设置配方".getTranslate() + "(ctrl+F)");
                    blueprintcopytopaste_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), blueprintcopytopaste_bool.Value, "蓝图复制直接粘贴".getTranslate());
                    bool temp = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), ShowStationInfo.Value, "物流站信息显示".getTranslate());
                    stationcopyItem_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), stationcopyItem_bool.Value, "物流站物品设置复制粘贴".getTranslate());
                    if (temp != ShowStationInfo.Value)
                    {
                        ShowStationInfo.Value = temp;
                        if (!temp)
                            for (int index = 0; index < maxCount; ++index)
                                tip[index].SetActive(false);
                    }
                    if (autoabsorttrash_bool.Value != GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), autoabsorttrash_bool.Value, "30s间隔自动吸收垃圾".getTranslate()))
                    {
                        autoabsorttrash_bool.Value = !autoabsorttrash_bool.Value;
                        if (autoabsorttrash_bool.Value)
                        {
                            autocleartrash_bool.Value = false;
                        }
                    }
                    if (autoabsorttrash_bool.Value)
                    {
                        onlygetbuildings.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), onlygetbuildings.Value, "只回收建筑".getTranslate());
                    }
                    if (autocleartrash_bool.Value != GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), autocleartrash_bool.Value, "30s间隔自动清除垃圾".getTranslate()))
                    {
                        autocleartrash_bool.Value = !autocleartrash_bool.Value;
                        if (autocleartrash_bool.Value)
                        {
                            autoabsorttrash_bool.Value = false;
                            onlygetbuildings.Value = false;
                        }
                    }



                    ChangeQuickKey = GUI.Toggle(new Rect(10, heightdis * tempheight++, widthlen2, heightdis), ChangeQuickKey, !ChangeQuickKey ? "改变窗口快捷键".getTranslate() : "点击确认".getTranslate());
                    GUI.TextArea(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), tempShowWindow.ToString());
                    finalheight = Math.Max(finalheight, heightdis * (tempheight + 1));
                    GUILayout.EndArea();

                }

                {
                    tempheight = 0;
                    GUILayout.BeginArea(new Rect(40 + widthlen1 + widthlen2 + oneareamaxwidth, 0, oneareamaxwidth + 10, window_height));
                    changeups = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), changeups, "启动时间流速修改".getTranslate());
                    GUI.Label(new Rect(10, heightdis * tempheight++, oneareamaxwidth, 40), "流速倍率".getTranslate() + ":" + string.Format("{0:N2}", upsfix));
                    upsfix = GUI.HorizontalSlider(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), upsfix, 0.01f, 10);
                    if (upsfix < 0.01) upsfix = 0.01f;
                    upsquickset.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), upsquickset.Value, "加速减速".getTranslate() + "(shift,'+''-')");

                    autosetSomevalue_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), autosetSomevalue_bool.Value, "自动配置建筑".getTranslate());
                    GUI.Label(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), "人造恒星燃料数量".getTranslate() + "：" + auto_supply_starfuel.Value);
                    auto_supply_starfuel.Value = (int)GUI.HorizontalSlider(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), auto_supply_starfuel.Value, 4, 100);
                    if (GUI.Button(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), "填充当前星球人造恒星".getTranslate())) addfueltoallStar();

                    autosavetimechange.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), autosavetimechange.Value, "自动保存".getTranslate());
                    GUI.Label(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), "自动保存时间".getTranslate() + "/min：");
                    int tempint = autosavetime.Value / 60;
                    if (int.TryParse(Regex.Replace(GUI.TextField(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), tempint + ""), @"[^0-9]", ""), out tempint))
                    {
                        if (tempint < 1) tempint = 1;
                        if (tempint > 10000) tempint = 10000;
                        autosavetime.Value = tempint * 60;
                    }
                    finalheight = Math.Max(finalheight, heightdis * (tempheight + 1));
                    if (CloseUIpanel.Value != GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), CloseUIpanel.Value, "关闭白色面板".getTranslate()))
                    {
                        CloseUIpanel.Value = !CloseUIpanel.Value;
                        ui_AuxilaryPanelPanel.SetActive(!CloseUIpanel.Value);
                    }
                    KeepBeltHeight.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), KeepBeltHeight.Value, "保持传送带高度".getTranslate());
                    Quickstop_bool.Value = GUI.Toggle(new Rect(10, heightdis * tempheight++, oneareamaxwidth, heightdis), Quickstop_bool.Value, "ctrl+空格快速开关".getTranslate());
                    stopfactory = GUI.Toggle(new Rect(20, heightdis * tempheight++, oneareamaxwidth, heightdis), stopfactory, "停止工厂".getTranslate());
                    stopDysonSphere = GUI.Toggle(new Rect(20, heightdis * tempheight++, oneareamaxwidth, heightdis), stopDysonSphere, "停止戴森球".getTranslate());
                    GUILayout.EndArea();
                }
                window_height = finalheight;
            }
            window_width = 70 + widthlen1 + widthlen2 + oneareamaxwidth * 2 < window_width ? window_width : 70 + widthlen1 + widthlen2 + oneareamaxwidth * 2;
            GUILayout.EndArea();
        }
        void StationinfoStart()
        {
            # region BeltWindow
            testitem = Instantiate<GameObject>(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Belt Window"), GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Blueprint Copy Mode").transform);
            testitem.GetComponent<RectTransform>().position = new Vector3(7.5f, 50, 20);
            testitem.name = "test";
            Vector3 item_sign_localposition = testitem.transform.Find("item-sign").GetComponent<RectTransform>().localPosition;
            testitem.transform.Find("item-sign").GetComponent<RectTransform>().localPosition = item_sign_localposition - new Vector3(item_sign_localposition.x, -30, 0);
            Vector3 number_input_localposition = testitem.transform.Find("number-input").GetComponent<RectTransform>().localPosition;
            testitem.transform.Find("number-input").GetComponent<RectTransform>().localPosition = number_input_localposition - new Vector3(number_input_localposition.x, -30, 0);
            Destroy(testitem.transform.Find("state").gameObject);
            Destroy(testitem.transform.Find("item-display").gameObject);
            Destroy(testitem.transform.Find("panel-bg").Find("title-text").gameObject);
            testitem.transform.Find("item-sign").GetComponent<Button>().onClick.AddListener(() =>
            {
                if (UISignalPicker.isOpened)
                    UISignalPicker.Close();
                else
                    UISignalPicker.Popup(new Vector2(-300, Screen.height / 3), new Action<int>(testint));
            });
            testitem.transform.Find("number-input").GetComponent<InputField>().onEndEdit.AddListener((string str) =>
            {
                float result = 0.0f;
                if (!float.TryParse(str, out result))
                    return;
                if (beltpools.Count > 0)
                {
                    foreach (int i in beltpools)
                    {
                        GameMain.localPlanet.factory.cargoTraffic.SetBeltSignalIcon(i, pointsignalid);
                        GameMain.localPlanet.factory.cargoTraffic.SetBeltSignalNumber(i, result);
                    }
                }
            });
            testitem.gameObject.SetActive(false);
            #endregion
            #region MonitorWindow
            testitem1 = Instantiate<GameObject>(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Monitor Window"), GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Blueprint Copy Mode").transform);
            testitem1.GetComponent<RectTransform>().position = new Vector3(7.5f, 50, 20);
            testitem1.name = "test1";
            testitem1.gameObject.SetActive(false);
            testitem1.transform.Find("speaker-panel").gameObject.SetActive(false);
            testitem1.transform.Find("speaker-panel").GetComponent<RectTransform>().position = new Vector3(8, 47, 20);
            Destroy(testitem1.transform.Find("flow-statistics").gameObject);
            Destroy(testitem1.transform.Find("alarm-settings").gameObject);
            Destroy(testitem1.transform.Find("monitor-settings").gameObject);
            Destroy(testitem1.transform.Find("sep-line").gameObject);
            Destroy(testitem1.transform.Find("sep-line").gameObject);
            GameObject speaker_panel = testitem1.transform.Find("speaker-panel").gameObject;
            GameObject pitch = speaker_panel.transform.Find("pitch").gameObject;
            GameObject volume = speaker_panel.transform.Find("volume").gameObject;
            speaker_panel.GetComponent<UISpeakerPanel>().toneCombo.onItemIndexChange.AddListener(new UnityAction(() =>
            {
                if (monitorpools != null && monitorpools.Count > 0)
                {
                    UIComboBox toneCombo = speaker_panel.GetComponent<UISpeakerPanel>().toneCombo;
                    foreach (int i in monitorpools)
                    {
                        int speakerId = GameMain.localPlanet.factory.cargoTraffic.monitorPool[i].speakerId;
                        GameMain.localPlanet.factory.digitalSystem.speakerPool[speakerId].SetTone(toneCombo.ItemsData[toneCombo.itemIndex]);
                        GameMain.localPlanet.factory.digitalSystem.speakerPool[speakerId].Play(ESpeakerPlaybackOrigin.Current);
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
                        int speakerId = GameMain.localPlanet.factory.cargoTraffic.monitorPool[i].speakerId;
                        GameMain.localPlanet.factory.digitalSystem.speakerPool[speakerId].SetPitch((int)f);
                        GameMain.localPlanet.factory.digitalSystem.speakerPool[speakerId].Play(ESpeakerPlaybackOrigin.Current);
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
                        int speakerId = GameMain.localPlanet.factory.cargoTraffic.monitorPool[i].speakerId;
                        GameMain.localPlanet.factory.digitalSystem.speakerPool[speakerId].SetVolume((int)speaker_panel.GetComponent<UISpeakerPanel>().volumeSlider.value);
                        GameMain.localPlanet.factory.digitalSystem.speakerPool[speakerId].Play(ESpeakerPlaybackOrigin.Current);
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
                          int speakerId = GameMain.localPlanet.factory.cargoTraffic.monitorPool[i].speakerId;
                          GameMain.localPlanet.factory.digitalSystem.speakerPool[speakerId].SetPitch((int)speaker_panel.GetComponent<UISpeakerPanel>().pitchSlider.value);
                          GameMain.localPlanet.factory.digitalSystem.speakerPool[speakerId].SetVolume((int)speaker_panel.GetComponent<UISpeakerPanel>().volumeSlider.value);
                          GameMain.localPlanet.factory.digitalSystem.speakerPool[speakerId].SetTone(toneCombo.ItemsData[toneCombo.itemIndex]);
                          GameMain.localPlanet.factory.digitalSystem.speakerPool[speakerId].Play(ESpeakerPlaybackOrigin.Current);
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
        void StationinfoUpdate()
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
                            Instantiate<GameObject>(tipPrefab, stationTip.transform);
                            Array.Resize<GameObject>(ref tip, maxCount);
                            tip[maxCount - 1] = Instantiate<GameObject>(tipPrefab, stationTip.transform);
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
                                num1 = 3;
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
                                            tip[index1].transform.Find("icontext" + i).gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(i * 30, -5 - 30 * (string.IsNullOrEmpty(stationComponent.name) ? num1 : num1 + 1), 0);
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
            if (GameMain.localPlanet == null || GameMain.localPlanet.factory == null || GameMain.localPlanet.gasItems != null) return;
            if (LDB.items == null || LDB.items.Select(itemId) == null) return;
            GameMain.mainPlayer.package.Sort();
            int packageGridLen = GameMain.mainPlayer.package.grids.Length;
            int stackMax;
            int stackSize = 0;
            for (int i = packageGridLen - 1; i >= 0; i--, stackSize++)
            {
                if (GameMain.mainPlayer.package.grids[i].count != 0) break;
            }
            stackMax = LDB.items.Select(itemId).StackSize * stackSize;
            itemCount = stackMax > itemCount ? itemCount : stackMax;
            PlanetFactory pf = GameMain.localPlanet.factory;

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
                    int inc = 0;
                    sc.TakeItem(ref tempItemId, ref tempItemCount, out inc);
                    result += tempItemCount;
                    itemCount -= tempItemCount;
                    resultinc += inc;
                }
                GameMain.mainPlayer.TryAddItemToPackage(itemId, result, resultinc, false);
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
                            int inc;
                            int temp = sc.TakeItem(itemId, itemCount, out inc);
                            itemCount -= temp;
                            if (temp > 0)
                            {
                                GameMain.mainPlayer.TryAddItemToPackage(itemId, temp, inc, false);
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
                                GameMain.mainPlayer.TryAddItemToPackage(itemId, tempItemCount, inc, false);
                            }
                        }
                    }
                }
            }
        }
        private void addDroneShiptooldstation()
        {
            if (GameMain.localPlanet == null || GameMain.localPlanet.type == EPlanetType.Gas) return;
            int inc;
            foreach (StationComponent sc in GameMain.localPlanet.factory.transport.stationPool)
            {
                if (sc == null || sc.isVeinCollector) continue;
                if (sc.isStellar && sc.workShipCount + sc.idleShipCount < auto_supply_ship.Value)
                    sc.idleShipCount += GameMain.mainPlayer.package.TakeItem(5002, auto_supply_ship.Value - sc.workShipCount - sc.idleShipCount, out inc);
                if (sc.isStellar)
                {
                    if (sc.workDroneCount + sc.idleDroneCount < auto_supply_drone.Value)
                        sc.idleDroneCount += GameMain.mainPlayer.package.TakeItem(5001, auto_supply_drone.Value - sc.workDroneCount - sc.idleDroneCount, out inc);
                }
                else
                {
                    int tempdrone = auto_supply_drone.Value > 50 ? 50 : auto_supply_drone.Value;
                    if (sc.workDroneCount + sc.idleDroneCount < tempdrone)
                        sc.idleDroneCount += GameMain.mainPlayer.package.TakeItem(5001, tempdrone - sc.workDroneCount - sc.idleDroneCount, out inc);
                }
                if (sc.warperCount < auto_supply_warp.Value)
                    sc.warperCount += GameMain.mainPlayer.package.TakeItem(1210, auto_supply_warp.Value - sc.warperCount, out inc);
            }
            foreach (StationComponent sc in GameMain.localPlanet.factory.transport.stationPool)
            {
                if (sc == null) continue;
                if (sc.isStellar && sc.workShipCount + sc.idleShipCount > 10)
                {
                    GameMain.mainPlayer.package.AddItem(5002, sc.workShipCount + sc.idleShipCount - 10, 0, out inc);
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
                        GameMain.mainPlayer.package.TakeItem(5001, sc.workDroneCount + sc.idleDroneCount - 50, out inc);
                        sc.idleDroneCount -= sc.workDroneCount + sc.idleDroneCount - 50;
                    }
                }
            }
        }
        private void changeallveincollectorspeedconfig()
        {
            if (GameMain.localPlanet == null || GameMain.localPlanet.type == EPlanetType.Gas) return;

            foreach (StationComponent sc in GameMain.localPlanet.factory.transport.stationPool)
            {
                if (sc == null || !sc.isVeinCollector) continue;
                GameMain.localPlanet.factory.factorySystem.minerPool[sc.minerId].speed = veincollectorspeed.Value * 1000;
                //GameMain.localPlanet.factory.powerSystem.consumerPool[sc.pcId].workEnergyPerTick = (long)((double)LDB.items.Select((int)GameMain.localPlanet.factory.entityPool[sc.entityId].protoId).prefabDesc.workEnergyPerTick * (veincollectorspeed.Value / 10.0) * (veincollectorspeed.Value / 10.0));
            }
        }
        private void setgasstation()
        {
            if (GameMain.localPlanet == null || GameMain.localPlanet.type != EPlanetType.Gas)
                return;
            PlanetFactory pf = GameMain.localPlanet.factory;
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
            StorageComponent package = GameMain.mainPlayer.package;
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
                pos = new Vector3((float)(posx * GameMain.mainPlayer.planetData.realRadius), 0, (float)(posz * GameMain.mainPlayer.planetData.realRadius));
                Vector3 vector3_3 = 0.025f * pos.normalized * GameMain.mainPlayer.planetData.realRadius;
                Quaternion quaternion3 = Maths.SphericalRotation(pos, GameMain.mainPlayer.controller.actionBuild.clickTool.yaw);
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
                GameMain.mainPlayer.controller.actionBuild.clickTool.factory.AddPrebuildDataWithComponents(prebuild);
            }
        }
        private void addfueltoallStar()
        {
            if (GameMain.localPlanet == null) return;
            PlanetFactory fs = GameMain.localPlanet.factory;
            foreach (PowerGeneratorComponent pgc in fs.powerSystem.genPool)
            {
                int inc;
                if (pgc.fuelMask == 4 && pgc.fuelCount < auto_supply_starfuel.Value)
                    fs.powerSystem.genPool[pgc.id].SetNewFuel(1803, (short)(GameMain.mainPlayer.package.TakeItem(1803, auto_supply_starfuel.Value - pgc.fuelCount, out inc) + pgc.fuelCount), (short)inc);
            }
        }
        private void testint(int signalId)
        {
            if (LDB.signals.IconSprite(signalId) == null) return;
            pointsignalid = signalId;
            testitem.transform.Find("item-sign").GetComponent<Image>().sprite = LDB.signals.IconSprite(signalId);
        }
        private void BluePrintoptimize()
        {
            Player player = GameMain.mainPlayer;
            blueprintopen = false;
            //蓝图复制操作优化
            if (player == null || player.controller.actionBuild == null)
            {
                return;
            }
            PlayerAction_Build build = player.controller.actionBuild;
            if (build.blueprintCopyTool != null && build.blueprintCopyTool.active && build.blueprintCopyTool.bpPool != null)
            {
                blueprintopen = true;
                BuildTool_BlueprintCopy blue_copy = build.blueprintCopyTool;
                if (blueprintrevoke_bool.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
                {
                    blue_copy.ClearSelection();
                    blue_copy.ClearPreSelection();
                    blue_copy.RefreshBlueprintData();
                    for (int i = 0; i < GameMain.localPlanet.factory.prebuildPool.Length; i++)
                    {
                        int itemId = (int)GameMain.localPlanet.factory.prebuildPool[i].protoId;
                        if (GameMain.localPlanet.factory.prebuildPool[i].itemRequired == 0)
                            player.TryAddItemToPackage(itemId, 1, 0, true);
                        GameMain.localPlanet.factory.RemovePrebuildWithComponents(i);
                    }
                    pointeRecipetype = ERecipeType.None;
                }
                if (blueprintsetrecipe_bool.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
                {
                    recipewindowx = (int)Input.mousePosition.x;
                    recipewindowy = (int)Input.mousePosition.y;
                    if (!changename)
                    {
                        stationpools = new List<int>();
                        foreach (BuildPreview bp in blue_copy.bpPool)
                        {
                            if (bp != null && bp.item != null && bp.objId > 0)
                            {
                                if (build.factory.entityPool[bp.objId].stationId > 0)
                                {
                                    if (build.factory.entityPool[bp.objId].minerId > 0) continue;
                                    stationpools.Add(build.factory.entityPool[bp.objId].stationId);
                                    changename = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        changename = false;
                        stationpools = new List<int>();
                    }

                    if (pointeRecipetype == ERecipeType.None && labpools.Count == 0 && assemblerpools.Count == 0)
                    {
                        assemblerpools = new List<int>();
                        labpools = new List<int>();
                        foreach (BuildPreview bp in blue_copy.bpPool)
                        {
                            if (bp != null && bp.item != null && bp.objId > 0 && bp.item.prefabDesc.isLab)
                            {
                                labpools.Add(build.factory.entityPool[bp.objId].labId);
                            }
                        }
                        foreach (BuildPreview bp in blue_copy.bpPool)
                        {
                            if (bp != null && bp.item != null && bp.objId > 0 && bp.item.prefabDesc.assemblerRecipeType != ERecipeType.None)
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
                        beltpools = new List<int>();
                        monitorpools = new List<int>();
                        if (!testitem1.gameObject.activeSelf)
                        {
                            bool exist = false;
                            foreach (BuildPreview bp in blue_copy.bpPool)
                            {
                                if (bp != null && bp.item != null && bp.objId > 0 && bp.item.ID == 2030 && build.factory.entityPool[bp.objId].monitorId > 0)
                                {
                                    monitorpools.Add(build.factory.entityPool[bp.objId].monitorId);
                                    int speakerId = build.factory.cargoTraffic.monitorPool[build.factory.entityPool[bp.objId].monitorId].speakerId;

                                    if (!exist && speakerId > 0)
                                    {
                                        exist = true;
                                        SpeakerComponent speakerComponent = build.factory.digitalSystem.speakerPool[speakerId];
                                        testitem1.transform.Find("speaker-panel").GetComponent<UISpeakerPanel>().pitchSlider.value = speakerComponent.pitch;
                                        testitem1.transform.Find("speaker-panel").GetComponent<UISpeakerPanel>().volumeSlider.value = speakerComponent.volume;
                                        testitem1.transform.Find("speaker-panel").GetComponent<UISpeakerPanel>().toneCombo.itemIndex = speakerComponent.tone > 0 ? testitem1.transform.Find("speaker-panel").GetComponent<UISpeakerPanel>().toneCombo.ItemsData.IndexOf(speakerComponent.tone) : 0;
                                    }
                                }
                            }
                            if (monitorpools.Count > 0)
                            {
                                testitem1.gameObject.SetActive(true);
                                testitem1.transform.Find("speaker-panel").gameObject.SetActive(true);
                            }
                        }
                        else
                        {
                            testitem1.gameObject.SetActive(false);
                            testitem1.transform.Find("speaker-panel").gameObject.SetActive(false);
                        }
                        if (!testitem.gameObject.activeSelf)
                        {
                            int tempsignaliconid = -1;
                            int tempnumberinput = 0;
                            foreach (BuildPreview bp in blue_copy.bpPool)
                            {
                                if (bp != null && bp.item != null && bp.objId > 0 && bp.item.prefabDesc.beltSpeed > 0 && GameMain.localPlanet.factory.entitySignPool[bp.objId].iconId0 > 0)
                                {
                                    if (tempsignaliconid == -1)
                                    {
                                        tempsignaliconid = (int)GameMain.localPlanet.factory.entitySignPool[bp.objId].iconId0;
                                        tempnumberinput = (int)GameMain.localPlanet.factory.entitySignPool[bp.objId].count0;
                                    }
                                    beltpools.Add(build.factory.entityPool[bp.objId].id);
                                }
                            }
                            if (beltpools.Count > 0)
                            {
                                testitem.gameObject.SetActive(true);
                                if (tempsignaliconid > 0)
                                {
                                    pointsignalid = tempsignaliconid;
                                    testitem.transform.Find("item-sign").GetComponent<Image>().sprite = LDB.signals.IconSprite(tempsignaliconid);
                                    testitem.transform.Find("number-input").GetComponent<InputField>().text = tempnumberinput + "";
                                }
                            }
                        }
                        else
                        {
                            testitem.gameObject.SetActive(false);
                            if (UISignalPicker.isOpened)
                                UISignalPicker.Close();
                        }
                    }
                    else
                    {
                        blue_copy.ClearSelection();
                        blue_copy.ClearPreSelection();
                        blue_copy.RefreshBlueprintData();
                        pointeRecipetype = ERecipeType.None;
                        changename = false;
                        stationpools = new List<int>();
                        labpools = new List<int>();
                        assemblerpools = new List<int>();
                        testitem.gameObject.SetActive(false);
                        testitem1.gameObject.SetActive(false);
                        if (UISignalPicker.isOpened)
                            UISignalPicker.Close();
                        testitem1.transform.Find("speaker-panel").gameObject.SetActive(false);
                    }
                    if (ejectorpools.Count == 0)
                    {
                        ejectorpools = new List<int>();
                        foreach (BuildPreview bp in blue_copy.bpPool)
                        {
                            if (bp != null && bp.item != null && bp.objId > 0 && bp.item.prefabDesc.isEjector)
                            {
                                ejectorpools.Add(build.factory.entityPool[bp.objId].ejectorId);
                            }
                        }
                    }
                    else
                    {
                        ejectorpools = new List<int>();
                    }
                }
                if (blueprintdelete_bool.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
                {
                    foreach (BuildPreview bp in blue_copy.bpPool)
                    {
                        if (bp != null && bp.item != null && bp.objId > 0)
                        {
                            int stationId = GameMain.localPlanet.factory.entityPool[bp.objId].stationId;
                            if (stationId > 0)
                            {
                                StationComponent sc = GameMain.localPlanet.factory.transport.stationPool[stationId];
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
                if (blueprintcopytopaste_bool.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
                    blue_copy.UseToPasteNow();
            }
            else
            {
                pointeRecipetype = ERecipeType.None;
                changename = false;
                stationpools = new List<int>();
                assemblerpools = new List<int>();
                labpools = new List<int>();
                beltpools = new List<int>();
                monitorpools = new List<int>();
                testitem.gameObject.SetActive(false);
                testitem1.gameObject.SetActive(false);
                testitem1.transform.Find("speaker-panel").gameObject.SetActive(false);
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
        private void changeallstationconfig()
        {
            if (GameMain.localPlanet == null || GameMain.localPlanet.factory == null || GameMain.localPlanet.factory.transport == null || GameMain.localPlanet.factory.transport.stationPool == null) return;
            foreach (StationComponent sc in GameMain.localPlanet.factory.transport.stationPool)
            {
                if (sc == null || sc.isVeinCollector) continue;

                sc.tripRangeDrones = Math.Cos(stationdronedist.Value * Math.PI / 180);
                GameMain.localPlanet.factory.powerSystem.consumerPool[sc.pcId].workEnergyPerTick = (long)stationmaxpowerpertick.Value * 16667;
                if (stationmaxpowerpertick.Value > 60 && !sc.isStellar)
                {
                    GameMain.localPlanet.factory.powerSystem.consumerPool[sc.pcId].workEnergyPerTick = (long)60 * 16667;
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
        private void StartAndStopGame()
        {
            if (Quickstop_bool.Value && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Space))
            {
                if (stopDysonSphere != stopfactory) stopDysonSphere = stopfactory;
                stopDysonSphere = !stopDysonSphere;
                stopfactory = !stopfactory;
            }
        }
        private void TrashFunction()
        {
            if (GameMain.data == null) return;
            if (autoabsorttrash_bool.Value && Time.time - trashlasttime > 30)
            {
                trashlasttime = Time.time;
                int index = 0;
                foreach (TrashObject to in GameMain.data.trashSystem.container.trashObjPool)
                    if (to.expire != 0)
                    {
                        if (onlygetbuildings.Value)
                        {
                            if (LDB.items.Select(to.item) != null && LDB.items.Select(to.item).CanBuild)
                                GameMain.data.trashSystem.container.trashObjPool[index++].expire = 60;
                            else
                                GameMain.data.trashSystem.container.RemoveTrash(index++);
                        }
                        else
                            GameMain.data.trashSystem.container.trashObjPool[index++].expire = 60;
                    }
            }
            if (autocleartrash_bool.Value && Time.time - trashlasttime > 30 && GameMain.data.trashSystem != null)
            {
                trashlasttime = Time.time;
                GameMain.data.trashSystem.ClearAllTrash();
            }
        }
        private string getStationlogic(int i)
        {
            switch (i)
            {
                case 0: return "仓储";
                case 1: return "供应";
                case 2: return "需求";
            }
            return "";
        }
        private void AutoAddwarp()
        {
            if (GameMain.mainPlayer != null && GameMain.mainPlayer.mecha != null && GameMain.mainPlayer.mecha.thrusterLevel >= 3 && !GameMain.mainPlayer.mecha.HasWarper())
            {
                int itemID = 1210;
                int count = 20;
                GameMain.mainPlayer.package.TakeTailItems(ref itemID, ref count, out int inc);
                if (itemID <= 0 || count <= 0)
                {
                    return;
                }
                GameMain.mainPlayer.mecha.warpStorage.AddItem(itemID, count, inc, out int remaininc);
            }
        }
        private void AutoAddFuel()
        {
            if (GameMain.mainPlayer != null && GameMain.mainPlayer.mecha != null && GameMain.mainPlayer.mecha.reactorStorage.isEmpty)
            {
                foreach(var itemID in fuelItems)
                {
                    if (!FuelFilter[itemID] || GameMain.mainPlayer.package.GetItemCount(itemID) == 0)
                    {
                        continue;
                    }
                    var item = LDB.items.Select(itemID);
                    int tempitemID=itemID;
                    int count = item.StackSize;
                    while (!GameMain.mainPlayer.mecha.reactorStorage.isFull)
                    {
                        GameMain.mainPlayer.package.TakeTailItems(ref tempitemID, ref count, out int inc);
                        if (tempitemID <= 0 || count <= 0)
                        {
                            break ;
                        }
                        GameMain.mainPlayer.mecha.reactorStorage.AddItem(itemID, count, inc, out int remaininc);
                        if (GameMain.mainPlayer.mecha.reactorStorage.isFull || GameMain.mainPlayer.mecha.reactorStorage.GetItemCount(itemID)>100000) return;
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
            int num = 0;
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
            if (left && right) num = 0;
            else if (!left && !right) num = 2;
            else num = 1;
            if (num == 2)
            {
                tempShowWindow = new KeyboardShortcut((KeyCode)result[1], (KeyCode)result[0]);
            }
            else if (num == 1)
            {
                int keynum = Math.Max(result[0], result[1]);
                tempShowWindow = new KeyboardShortcut((KeyCode)keynum);
            }
        }
        private string Translate(string s)
        {
            if (s == null)
                return "";
            StringProtoSet strings = Localization.strings;
            if (strings == null)
                return s;
            StringProto stringProto = strings[s];
            if (stringProto == null)
                return s;
            switch (Localization.language)
            {
                case Language.zhCN:
                    return stringProto.ZHCN;
                case Language.enUS:
                    return stringProto.ENUS;
                case Language.frFR:
                    return stringProto.FRFR;
                default:
                    return s;
            }
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


    }


}
