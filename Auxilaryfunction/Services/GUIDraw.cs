using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Auxilaryfunction.Auxilaryfunction;
using static Auxilaryfunction.Constant;
using static Auxilaryfunction.Services.DysonBluePrintDataService;
using static Auxilaryfunction.Services.TechService;
using static UnityEngine.Object;
using Application = UnityEngine.Application;
using Image = UnityEngine.UI.Image;
using Text = UnityEngine.UI.Text;

namespace Auxilaryfunction.Services
{
    public class GUIDraw
    {
        private int PanelIndex;
        private int baseSize;
        private int heightdis;
        private float _windowwidth;
        private float _windowheight;
        private bool firstDraw;
        private bool RefreshBaseSize;
        private bool DeleteDysonLayer;
        private GameObject ui_AuxilaryPanelPanel;
        public static int recipewindowx;
        public static int recipewindowy;
        private int[] locallogics = new int[5];
        private int[] remotelogics = new int[5];
        public static List<int> fuelItems = new List<int>();
        public static Dictionary<int, bool> FuelFilter = new Dictionary<int, bool>();
        public static List<string> ConfigNames = new List<string>();
        public static Vector2 scrollPosition;
        public static Vector2 dysonBluePrintscrollPosition;
        public static ConfigEntry<int> scale;
        public static KeyboardShortcut tempShowWindow;
        public static bool blueprintopen;
        public static bool showwindow;
        public static bool ChangeQuickKey;
        public static bool autosetstationconfig;
        public static bool limitmaterial;
        private Color OrangeColor;
        private Color BlueColor;
        public Texture2D mytexture;
        private bool moving;
        private bool leftscaling;
        private bool rightscaling;
        private bool bottomscaling;
        public bool selectautoaddtechid;
        public float MainWindow_x_move = 200;
        public float MainWindow_y_move = 200;
        public float temp_MainWindow_x = 10;
        public float temp_MainWindow_y = 200;
        public float MainWindow_x = 300;
        public float MainWindow_y = 200;
        public float MainWindowWidth
        {
            get => _windowwidth;
            set
            {
                if (_windowwidth != value)
                {
                    _windowwidth = value;
                    window_width.Value = value;
                }
            }
        }
        public float MainWindowHeight
        {
            get => _windowheight;
            set
            {
                if (_windowheight != value)
                {
                    _windowheight = value;
                    window_height.Value = value;
                }
            }
        }
        public static List<float[]> boundaries = new List<float[]>();
        GUIStyle normalstyle = new GUIStyle();
        GUIStyle styleblue = new GUIStyle();
        GUIStyle styleyellow = new GUIStyle();
        GUIStyle styleitemname = null;
        GUIStyle buttonstyleyellow = null;
        GUIStyle buttonstyleblue = null;
        GUIStyle labelstyle = null;
        GUIStyle whitestyle = null;
        GUIStyle nomarginButtonStyle = null;
        GUILayoutOption[] HorizontalSlideroptions;
        GUILayoutOption[] buttonoptions;
        GUILayoutOption[] iconbuttonoptions;
        public int BaseSize
        {
            get => baseSize;
            set
            {
                baseSize = value;
                scale.Value = value;
                RefreshBaseSize = true;
                heightdis = value * 2;
            }
        }

        public GUIDraw(int baseSize, GameObject panel)
        {
            BaseSize = baseSize;
            ui_AuxilaryPanelPanel = panel;
            Init();
        }
        public Sprite leftsprite;
        public Sprite rightsprite;
        public Sprite flatsprite;
        private void Init()
        {
            RefreshBaseSize = true;
            autosetstationconfig = true;
            MainWindowWidth = window_width.Value;
            MainWindowHeight = window_height.Value;
            leftsprite = LoadImage("Auxilaryfunction.left.png");
            rightsprite = LoadImage("Auxilaryfunction.right.png");
            flatsprite = LoadImage("Auxilaryfunction.flat.png");
            OrangeColor = new Color(224f / 255, 139f / 255, 93f / 255);
            BlueColor = new Color(75f / 255, 172f / 255, 205f / 255);
            mytexture = new Texture2D(10, 10);
            for (int i = 0; i < mytexture.width; i++)
                for (int j = 0; j < mytexture.height; j++)
                    mytexture.SetPixel(i, j, new Color(0, 0, 0, 1));
            mytexture.Apply();

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

            styleblue.fontStyle = FontStyle.Bold;
            styleblue.fontSize = 20;
            styleblue.normal.textColor = new Color32(167, 255, 255, 255);
            styleyellow.fontStyle = FontStyle.Bold;
            styleyellow.fontSize = 20;
            styleyellow.normal.textColor = new Color32(240, 191, 103, 255);
            BeltMonitorWindowOpen();
        }

        public Sprite LoadImage(string filename)
        {
            Stream sm = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename);
            byte[] bs = new byte[sm.Length];
            sm.Read(bs, 0, (int)sm.Length);
            sm.Close();
            Texture2D texture = new Texture2D(16, 16);
            texture.LoadImage(bs);
            return Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
        }

        public void GUIUpdate()
        {
            if (QuickKey.Value.IsDown() && !ChangingQuickKey && ready)
            {
                showwindow = !showwindow;
                if (showwindow)
                {
                    firstDraw = true;
                }
                ui_AuxilaryPanelPanel.SetActive(showwindow);
            }
            StationInfoWindowUpdate();
        }

        public void Draw()
        {
            if (firstDraw)
            {
                firstDraw = false;
                BaseSize = GUI.skin.label.fontSize;
            }
            if (showwindow && Input.GetKey(KeyCode.LeftControl))
            {
                int t = (int)(Input.GetAxis("Mouse Wheel") * 10);
                int temp = BaseSize + t;
                if (Input.GetKeyDown(KeyCode.UpArrow)) { temp++; }
                if (Input.GetKeyDown(KeyCode.DownArrow)) { temp--; }
                temp = Math.Max(5, Math.Min(temp, 35));
                BaseSize = temp;
            }
            if (RefreshBaseSize)
            {
                RefreshBaseSize = false;
                GUI.skin.label.fontSize = BaseSize;
                GUI.skin.button.fontSize = BaseSize;
                GUI.skin.toggle.fontSize = BaseSize;
                GUI.skin.textField.fontSize = BaseSize;
                GUI.skin.textArea.fontSize = BaseSize;
                labelstyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = BaseSize - 3
                };
                nomarginButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    margin = new RectOffset()
                };
                labelstyle.normal.textColor = GUI.skin.toggle.normal.textColor;
                HorizontalSlideroptions = new[] { GUILayout.Height(BaseSize), GUILayout.Width(BaseSize * 10) };
                buttonoptions = new[] { GUILayout.Height(BaseSize * 2) };
                iconbuttonoptions = new[] { GUILayout.Width(heightdis), GUILayout.Height(heightdis) };
            }
            if (styleitemname == null)
            {
                whitestyle = new GUIStyle();
                whitestyle.normal.background = Texture2D.whiteTexture;
                styleitemname = new GUIStyle(GUI.skin.label);
                styleitemname.normal.textColor = Color.white;
                buttonstyleblue = new GUIStyle(GUI.skin.button);
                buttonstyleblue.normal.textColor = styleblue.normal.textColor;
                buttonstyleyellow = new GUIStyle(GUI.skin.button);
                buttonstyleyellow.normal.textColor = styleyellow.normal.textColor;
            }
            if (showwindow)
            {
                UIPanelSet();
                MoveWindow();
                Scaling_window();
                Rect window = new Rect(MainWindow_x, MainWindow_y, MainWindowWidth, MainWindowHeight);
                GUI.DrawTexture(window, mytexture);
                window = GUI.Window(20210827, window, MainWindow, "辅助面板".getTranslate() + "(" + VERSION + ")" + "ps:ctrl+↑↓");
                int window2width = Localization.language != Language.zhCN ? 15 * BaseSize : 15 * BaseSize / 2;
                Rect switchwindow = new Rect(MainWindow_x - window2width, MainWindow_y, window2width, 25 * BaseSize);
                switchwindow = GUI.Window(202108228, switchwindow, SwitchWindow, "");
                GUI.DrawTexture(switchwindow, mytexture);
            }
            if (autonavigation_bool.Value && player?.navigation != null && player.navigation._indicatorAstroId != 0)
            {
                GUILayout.BeginArea(new Rect(10, 250, 500, 300));
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();

                if (GUILayout.Button(PlayerOperation.fly ? "停止导航".getTranslate() : "继续导航".getTranslate(), GUILayout.MinHeight(heightdis)))
                {
                    PlayerOperation.fly = !PlayerOperation.fly;
                }
                if (GUILayout.Button("取消方向指示".getTranslate(), GUILayout.MinHeight(heightdis)))
                {
                    player.navigation._indicatorAstroId = 0;
                }


                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            //LocalPlanet.factory.prebuildPool.ToList().Exists(x => x.protoId != 0)
            if (automovetounbuilt.Value && player != null && LocalPlanet?.factory != null && player.movementState == EMovementState.Fly && LocalPlanet.factory.prebuildCount > 0)
            {
                for (int i = 1; i < GameMain.localPlanet.factory.prebuildCursor; i++)
                {
                    int preid = GameMain.localPlanet.factory.prebuildPool[i].id;
                    if (preid == i && GameMain.localPlanet.factory.prebuildPool[i].protoId == 0)
                    {
                        GameMain.localPlanet.factory.RemovePrebuildWithComponents(preid);
                    }
                }
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
            BluePrintRecipeSet();
        }

        public void BluePrintRecipeSet()
        {
            if (!blueprintopen)
                return;
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
                    if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 50, Screen.height - recipewindowy + tempheight * 50, 50, 50), rp.iconSprite.texture))
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
                    if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 50, Screen.height - recipewindowy + tempheight++ * 50, 50, 50), "无".getTranslate()))
                    {
                        for (int j = 0; j < assemblerpools.Count; j++)
                        {
                            LocalPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].SetRecipe(0, LocalPlanet.factory.entitySignPool);
                        }
                    }
                    if (GUI.Button(new Rect(recipewindowx, Screen.height - recipewindowy + tempheight * 50, 200, 50), "额外产出".getTranslate()))
                    {
                        for (int j = 0; j < assemblerpools.Count; j++)
                        {
                            if (LocalPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].productive)
                                LocalPlanet.factory.factorySystem.assemblerPool[assemblerpools[j]].forceAccMode = false;
                        }
                    }
                    if (GUI.Button(new Rect(recipewindowx + 200, Screen.height - recipewindowy + tempheight * 50, 200, 50), "生产加速".getTranslate()))
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
                    if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 50, Screen.height - recipewindowy + tempheight * 50, 50, 50), LDB.items.Select(LabComponent.matrixIds[i]).iconSprite.texture))
                    {
                        for (int j = 0; j < labpools.Count; j++)
                        {
                            LocalPlanet.factory.factorySystem.labPool[labpools[j]].SetFunction(false, LDB.items.Select(LabComponent.matrixIds[i]).maincraft.ID, 0, LocalPlanet.factory.entitySignPool);
                        }
                    }
                if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 50, Screen.height - recipewindowy + tempheight++ * 50, 50, 50), "无".getTranslate()))
                {
                    for (int j = 0; j < labpools.Count; j++)
                    {
                        LocalPlanet.factory.factorySystem.labPool[labpools[j]].SetFunction(false, 0, 0, LocalPlanet.factory.entitySignPool);
                    }
                }
                if (GUI.Button(new Rect(recipewindowx, Screen.height - recipewindowy + tempheight++ * 50, 200, 50), "科研模式".getTranslate()))
                {
                    for (int j = 0; j < labpools.Count; j++)
                    {
                        LocalPlanet.factory.factorySystem.labPool[labpools[j]].SetFunction(true, 0, GameMain.history.currentTech, LocalPlanet.factory.entitySignPool);
                    }
                }
                if (GUI.Button(new Rect(recipewindowx, Screen.height - recipewindowy + tempheight * 50, 200, 50), "额外产出".getTranslate()))
                {
                    for (int j = 0; j < labpools.Count; j++)
                    {
                        if (LocalPlanet.factory.factorySystem.labPool[labpools[j]].productive)
                            LocalPlanet.factory.factorySystem.labPool[labpools[j]].forceAccMode = false;
                    }
                }
                if (GUI.Button(new Rect(recipewindowx + 200, Screen.height - recipewindowy + tempheight * 50, 200, 50), "生产加速".getTranslate()))
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
                        if (ds.OrbitExist(orbitid) && GUI.Button(new Rect(recipewindowx + j * 50, Screen.height - recipewindowy + tempheight * 50, 50, 50), orbitid.ToString()))
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
                for (int i = 0; i < 8; i++)
                {
                    if (GUI.Button(new Rect(recipewindowx + tempwidth++ * 130, Screen.height - recipewindowy + tempheight * 50, 130, 50), StationNames[i]))
                    {

                        for (int j = 0; j < stationpools.Count; j++)
                        {
                            StationComponent sc = LocalPlanet.factory.transport.stationPool[stationpools[j]];
                            if (i == 7)
                            {
                                if (sc.storage[4].count > 0 && sc.storage[4].itemId != 1210)
                                    player.TryAddItemToPackage(sc.storage[4].itemId, sc.storage[4].count, 0, false);
                                LocalPlanet.factory.transport.SetStationStorage(stationpools[j], stationindex, 1210, (int)batchnum * 100, (ELogisticStorage)locallogic, (ELogisticStorage)remotelogic, player);
                            }
                            else sc.name = StationNames[i];
                        }
                        stationpools.Clear();
                        break;
                    }
                    if (i == 4 || i == 6)
                    {
                        tempheight++;
                        tempwidth = 0;
                    }
                }
                int tempx = recipewindowx + tempwidth * 130;
                int tempy = Screen.height - recipewindowy + tempheight++ * 50;
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
                if (GUI.Button(new Rect(recipewindowx, Screen.height - recipewindowy + tempheight++ * 50, 130, 50), "粘贴物流站配方".getTranslate()))
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
                int heightdis = BaseSize * 2;
                GUILayout.BeginArea(new Rect(recipewindowx, Screen.height - recipewindowy + tempheight++ * 50, heightdis * 15, heightdis * 10));
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
                GUILayout.BeginArea(new Rect(recipewindowx, Screen.height - recipewindowy, BaseSize * 10, BaseSize * 10));
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

        private void StationInfoWindowUpdate()
        {
            if (!ShowStationInfo.Value || GameMain.localPlanet?.factory == null)
                return;
            if (UIGame.viewMode == EViewMode.Normal || UIGame.viewMode == EViewMode.Globe)
            {
                stationTip.SetActive(true);
                var pd = GameMain.localPlanet;
                int tipIndex = 0;
                Vector3 localPosition = GameCamera.main.transform.localPosition;
                Vector3 forward = GameCamera.main.transform.forward;
                float realRadius = pd.realRadius;
                if (pd.factory.transport.stationCursor > 0)
                {
                    foreach (StationComponent stationComponent in pd.factory.transport.stationPool)
                    {
                        if (stationComponent?.storage == null) continue;
                        if (tipIndex == maxCount)
                        {
                            ++maxCount;
                            Instantiate(tipPrefab, stationTip.transform);
                            Array.Resize(ref tips, maxCount);
                            tips[maxCount - 1] = Instantiate(tipPrefab, stationTip.transform);
                        }
                        Vector3 position = pd.factory.entityPool[stationComponent.entityId].pos.normalized;
                        int storageNum = Math.Min(stationComponent.storage.Length, 5);
                        if (stationComponent.isCollector)
                        {
                            storageNum = 2;
                            position *= realRadius + 35;
                        }
                        else if (stationComponent.isStellar)
                        {
                            position *= realRadius + 20;
                        }
                        else
                        {
                            position *= realRadius + 15;
                        }
                        var tip = tips[tipIndex];
                        Vector3 rhs = position - localPosition;
                        float magnitude = rhs.magnitude;
                        float num2 = Vector3.Dot(forward, rhs);
                        if (magnitude < 1.0 || num2 < 1.0)
                        {
                            tip.SetActive(false);
                            continue;
                        }
                        bool flag = UIRoot.ScreenPointIntoRect(GameCamera.main.WorldToScreenPoint(position), stationTip.GetComponent<RectTransform>(), out Vector2 rectPoint);
                        if (Mathf.Abs(rectPoint.x) > 8000.0 || Mathf.Abs(rectPoint.y) > 8000.0)
                            flag = false;
                        if (Phys.RayCastSphere(localPosition, rhs / magnitude, magnitude, Vector3.zero, realRadius, out RCHCPU _))
                            flag = false;
                        if (flag)
                        {
                            tipIndex++;
                            rectPoint.x = Mathf.Round(rectPoint.x);
                            rectPoint.y = Mathf.Round(rectPoint.y);
                            tip.GetComponent<RectTransform>().anchoredPosition = rectPoint;
                            float tipWindowHeight = 40 * storageNum + 20;
                            if (stationComponent.isCollector)
                                tipWindowHeight -= 20;
                            else if (stationComponent.isVeinCollector)
                                tipWindowHeight -= 20;
                            for (int i = 0; i < storageNum; ++i)
                            {
                                var storage = stationComponent.storage[i];
                                var icon = tip.transform.Find("icon" + i);
                                var iconposition = icon.GetComponent<RectTransform>().anchoredPosition3D;
                                var iconLocal = tip.transform.Find("iconLocal" + i);
                                var iconremote = tip.transform.Find("iconremote" + i);
                                var iconlocalimage = iconLocal.GetComponent<Image>();
                                var iconremoteimage = iconremote.GetComponent<Image>();
                                var countText = tip.transform.Find("countText" + i);
                                var countTextUitext = countText.GetComponent<Text>();
                                if (storage.itemId > 0)
                                {
                                    switch (storage.localLogic)
                                    {
                                        case ELogisticStorage.Supply:
                                            iconlocalimage.sprite = rightsprite;
                                            iconlocalimage.color = BlueColor;
                                            countTextUitext.color = BlueColor;
                                            break;
                                        case ELogisticStorage.Demand:
                                            iconlocalimage.sprite = leftsprite;
                                            iconlocalimage.color = OrangeColor;
                                            countTextUitext.color = OrangeColor;
                                            break;
                                        case ELogisticStorage.None:
                                            iconlocalimage.sprite = flatsprite;
                                            iconlocalimage.color = Color.gray;
                                            countTextUitext.color = Color.gray;
                                            break;
                                    }
                                    if (stationComponent.isStellar || stationComponent.isCollector)
                                    {
                                        switch (storage.remoteLogic)
                                        {
                                            case ELogisticStorage.Supply:
                                                iconremoteimage.sprite = rightsprite;
                                                iconremoteimage.color = BlueColor;
                                                break;
                                            case ELogisticStorage.Demand:
                                                iconremoteimage.sprite = leftsprite;
                                                iconremoteimage.color = OrangeColor;
                                                break;
                                            case ELogisticStorage.None:
                                                iconremoteimage.sprite = flatsprite;
                                                iconremoteimage.color = Color.gray;
                                                break;
                                        }
                                        iconremote.gameObject.SetActive(true);
                                    }
                                    if (ShowStationInfoMode.Value)
                                    {
                                        iconLocal.gameObject.SetActive(false);
                                        if (stationComponent.isStellar || stationComponent.isCollector)
                                        {
                                            iconremote.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
                                            iconremote.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(100, iconposition.y, 0);
                                            countText.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(70, iconposition.y, 0);
                                        }
                                        else
                                        {
                                            iconremote.gameObject.SetActive(false);
                                            countText.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(90, iconposition.y, 0);
                                        }
                                    }
                                    else
                                    {
                                        iconLocal.gameObject.SetActive(true);
                                        countText.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(70, iconposition.y, 0);
                                        if (stationComponent.isStellar || stationComponent.isCollector)
                                        {
                                            iconLocal.GetComponent<RectTransform>().sizeDelta = new Vector2(21, 21);
                                            iconremote.GetComponent<RectTransform>().sizeDelta = new Vector2(21, 21);
                                            iconLocal.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(105, iconposition.y, 0);
                                            iconremote.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(105, iconposition.y - 15, 0);
                                        }
                                        else
                                        {
                                            iconremote.gameObject.SetActive(false);
                                            iconLocal.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(100, iconposition.y, 0);
                                            iconLocal.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
                                        }
                                    }
                                    icon.GetComponent<Image>().sprite = LDB.items.Select(storage.itemId)?.iconSprite;
                                    icon.gameObject.SetActive(true);
                                    countTextUitext.text = storage.count.ToString("#,##0");
                                    tip.SetActive(true);
                                }
                                else
                                {
                                    iconLocal.gameObject.SetActive(false);
                                    iconremote.gameObject.SetActive(false);
                                    icon.gameObject.SetActive(false);
                                    countTextUitext.color = Color.white;
                                    countTextUitext.text = "无";
                                    if (ShowStationInfoMode.Value)
                                    {
                                        if (stationComponent.isStellar || stationComponent.isCollector)
                                        {
                                            countTextUitext.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(70, iconposition.y, 0);
                                        }
                                        else
                                        {
                                            countTextUitext.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(90, iconposition.y, 0);
                                        }
                                    }
                                    else
                                    {
                                        countTextUitext.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(70, iconposition.y, 0);
                                    }
                                }
                                countText.gameObject.SetActive(true);
                            }
                            var lasticon = tip.transform.Find("icon" + storageNum);
                            var lastcountText = tip.transform.Find("countText" + storageNum);
                            lasticon.gameObject.SetActive(false);
                            int lastLine = storageNum;
                            if (!string.IsNullOrEmpty(stationComponent.name))
                            {
                                var lastcountTextPosition = lastcountText.GetComponent<RectTransform>().anchoredPosition3D;
                                lastcountText.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(90, lastcountTextPosition.y, 0);
                                lastcountText.GetComponent<Text>().fontSize = 18;
                                lastcountText.GetComponent<Text>().text = stationComponent.name;
                                lastcountText.GetComponent<Text>().color = Color.white;
                                lastcountText.gameObject.SetActive(true);
                                tipWindowHeight += 27;
                                lastLine++;
                            }
                            else
                            {
                                lasticon.gameObject.SetActive(false);
                                lastcountText.gameObject.SetActive(false);
                            }
                            for (int i = 0; i < 3; ++i)
                            {
                                var icontext = tip.transform.Find("icontext" + i);
                                if (stationComponent.isCollector || stationComponent.isVeinCollector || (i >= 1 && !stationComponent.isStellar))
                                {
                                    icontext.gameObject.SetActive(false);
                                    continue;
                                }
                                int itemId;
                                string totalCount;
                                if (i == 0)
                                {
                                    itemId = 5001;
                                    totalCount = (stationComponent.idleDroneCount + stationComponent.workDroneCount).ToString();
                                    icontext.Find("countText2").GetComponent<Text>().color = Color.white;
                                    icontext.Find("countText2").GetComponent<Text>().text = stationComponent.idleDroneCount.ToString();
                                }
                                else if (i == 1)
                                {
                                    itemId = 5002;
                                    totalCount = (stationComponent.idleShipCount + stationComponent.workShipCount).ToString();
                                    icontext.Find("countText2").GetComponent<Text>().color = Color.white;
                                    icontext.Find("countText2").GetComponent<Text>().text = stationComponent.idleShipCount.ToString();
                                }
                                else
                                {
                                    itemId = 1210;
                                    totalCount = stationComponent.warperCount.ToString();
                                }
                                icontext.gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(i * 30, -30 - 30 * lastLine, 0);
                                icontext.GetComponent<Image>().sprite = LDB.items.Select(itemId).iconSprite;
                                icontext.Find("countText").GetComponent<Text>().color = Color.white;
                                icontext.Find("countText").GetComponent<Text>().text = totalCount;
                                icontext.gameObject.SetActive(true);
                            }
                            float localScaleMultiple;
                            if (magnitude < 50.0)
                                localScaleMultiple = 1.5f;
                            else if (magnitude < 250.0)
                                localScaleMultiple = (float)(1.75 - magnitude * 0.005);
                            else
                                localScaleMultiple = 0.5f;
                            tip.transform.localScale = Vector3.one * localScaleMultiple;
                            for (int i = lastLine; i < 13; ++i)
                            {
                                tip.transform.Find("iconLocal" + i).gameObject.SetActive(false);
                                tip.transform.Find("iconremote" + i).gameObject.SetActive(false);
                                tip.transform.Find("icon" + i).gameObject.SetActive(false);
                                tip.transform.Find("countText" + i).gameObject.SetActive(false);
                            }
                            tip.GetComponent<RectTransform>().sizeDelta = new Vector2(125f, tipWindowHeight);
                        }
                    }
                }
                for (int index4 = tipIndex; index4 < maxCount; ++index4)
                    tips[index4].SetActive(false);
            }
            else
                stationTip.SetActive(false);
        }

        private void BeltMonitorWindowOpen()
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
                if (!float.TryParse(str, out float result))
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
            tipPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 160f);
            tipPrefab.GetComponent<Image>().enabled = true;
            tipPrefab.transform.localPosition = new Vector3(200f, 800f, 0);
            tipPrefab.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            Destroy(tipPrefab.GetComponent<UIVeinDetailNode>());
            tipPrefab.SetActive(false);
            for (int index = 0; index < 13; ++index)
            {
                GameObject gameObject1 = Instantiate(tipPrefab.transform.Find("info-text").gameObject, new Vector3(0, 0, 0), Quaternion.identity, tipPrefab.transform);
                gameObject1.name = "countText" + index;
                float y = (-5 - 35 * index);
                gameObject1.GetComponent<Text>().fontSize = index == 5 ? 15 : 19;
                gameObject1.GetComponent<Text>().text = "99999";
                gameObject1.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                gameObject1.GetComponent<RectTransform>().sizeDelta = new Vector2(95, 30);
                gameObject1.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                gameObject1.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                gameObject1.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                gameObject1.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(70, y, 0);
                Destroy(gameObject1.GetComponent<Shadow>());
                GameObject gameObject2 = Instantiate(tipPrefab.transform.Find("icon").gameObject, new Vector3(0, 0, 0), Quaternion.identity, tipPrefab.transform);
                gameObject2.name = "icon" + index;
                gameObject1.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
                gameObject2.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                gameObject2.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                gameObject2.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                gameObject2.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, y, 0);
                GameObject iconLocal = Instantiate(tipPrefab.transform.Find("icon").gameObject, new Vector3(0, 0, 0), Quaternion.identity, tipPrefab.transform);
                iconLocal.name = "iconLocal" + index;
                iconLocal.GetComponent<Image>().material = null;
                iconLocal.GetComponent<RectTransform>().sizeDelta = new Vector2(21, 21);
                iconLocal.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                iconLocal.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                iconLocal.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                iconLocal.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(105, y, 0);
                GameObject iconremote = Instantiate(tipPrefab.transform.Find("icon").gameObject, new Vector3(0, 0, 0), Quaternion.identity, tipPrefab.transform);
                iconremote.name = "iconremote" + index;
                iconremote.GetComponent<Image>().material = null;
                iconremote.GetComponent<RectTransform>().sizeDelta = new Vector2(21, 21);
                iconremote.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                iconremote.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                iconremote.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                iconremote.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(105, y - 15, 0);
                iconLocal.SetActive(false);
                iconremote.SetActive(false);
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
                tips[i] = Instantiate(tipPrefab, stationTip.transform);
        }

        private void SetSignalId(int signalId)
        {
            if (LDB.signals.IconSprite(signalId) == null) return;
            pointsignalid = signalId;
            beltWindow.transform.Find("item-sign").GetComponent<Image>().sprite = LDB.signals.IconSprite(signalId);
        }

        #region 窗口UI
        private void SwitchWindow(int winId)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            string[] menus = new string[3] { "默认面板", "文字科技树", "戴森球面板" };
            for (int i = 0; i < 3; i++)
            {
                bool temp = GUILayout.Toggle(PanelIndex == i, menus[i].getTranslate());
                if (temp)
                {
                    PanelIndex = i;
                }
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

        }
        private void MainWindow(int winId)
        {
            if (PanelIndex == 1)
            {
                TextTechPanel();
            }
            else if (PanelIndex == 2)
            {
                DysonPanel();
            }
            else
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
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
                    norender_powerdisk_bool.Value = GUILayout.Toggle(norender_powerdisk_bool.Value, "不渲染电网覆盖".getTranslate());
                    norender_shipdrone_bool.Value = GUILayout.Toggle(norender_shipdrone_bool.Value, "不渲染运输船和飞机".getTranslate());
                    norender_entity_bool.Value = GUILayout.Toggle(norender_entity_bool.Value, "不渲染实体".getTranslate());
                    if (simulatorrender != GUILayout.Toggle(simulatorrender, "不渲染全部".getTranslate()))
                    {
                        simulatorrender = !simulatorrender;
                        simulatorchanging = true;
                    }
                    SunLightOpen.Value = GUILayout.Toggle(SunLightOpen.Value, "夜灯".getTranslate());
                    closeplayerflyaudio.Value = GUILayout.Toggle(closeplayerflyaudio.Value, "关闭玩家走路飞行声音".getTranslate());
                }
                GUILayout.EndVertical();
                GUILayout.Space(20);

                GUILayout.BeginVertical();
                {
                    if (autoaddtech_bool.Value != GUILayout.Toggle(autoaddtech_bool.Value, "自动添加科技队列".getTranslate()))
                    {
                        autoaddtech_bool.Value = !autoaddtech_bool.Value;
                        if (!autoaddtech_bool.Value)
                        {
                            autoaddtechid = 0;
                            auto_add_techid.Value = 0;
                        }
                    }
                    if (autoaddtech_bool.Value)
                    {
                        GUILayout.Label("自动添加科技等级上限".getTranslate() + ":");
                        string t = GUILayout.TextField(auto_add_techmaxLevel.Value.ToString(), new[] { GUILayout.Height(heightdis), GUILayout.Width(heightdis * 3) });
                        bool reset = !int.TryParse(Regex.Replace(t, @"^[^0-9]", ""), out int maxlevel);
                        if (maxlevel != 0)
                        {
                            auto_add_techmaxLevel.Value = maxlevel;
                        }
                        var pointtech = LDB.techs.Select(auto_add_techid.Value);
                        var name = "未选择".getTranslate();
                        if (pointtech != null)
                        {
                            TechState techstate = GameMain.history.techStates[pointtech.ID];
                            if (techstate.curLevel != techstate.maxLevel)
                            {
                                name = pointtech.name + "level" + techstate.curLevel;
                            }
                            if (reset)
                            {
                                auto_add_techmaxLevel.Value = techstate.maxLevel;
                            }
                        }
                        if (GUILayout.Button(name, buttonoptions))
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
                                        auto_add_techid.Value = LDB.techs.dataArray[i].ID;
                                    }
                                }
                            }
                        }
                    }
                    autoAddwarp.Value = GUILayout.Toggle(autoAddwarp.Value, "自动添加翘曲器".getTranslate());
                    autoAddFuel.Value = GUILayout.Toggle(autoAddFuel.Value, "自动添加燃料".getTranslate());
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
                                GUIStyle style = normalstyle;
                                if (FuelFilter[itemID])
                                    style = whitestyle;
                                if (GUILayout.Button(LDB.items.Select(itemID).iconSprite.texture, style, GUILayout.Height(heightdis), GUILayout.Width(heightdis)))
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
                    auto_setejector_bool.Value = GUILayout.Toggle(auto_setejector_bool.Value, "自动配置太阳帆弹射器".getTranslate());
                    automovetounbuilt.Value = GUILayout.Toggle(automovetounbuilt.Value, "自动飞向未完成建筑".getTranslate());
                    close_alltip_bool.Value = GUILayout.Toggle(close_alltip_bool.Value, "一键闭嘴".getTranslate());
                    noscaleuitech_bool.Value = GUILayout.Toggle(noscaleuitech_bool.Value, "科技面板选中不缩放".getTranslate(), buttonoptions);
                    BluePrintSelectAll.Value = GUILayout.Toggle(BluePrintSelectAll.Value, "蓝图全选".getTranslate() + "(ctrl+A）");
                    BluePrintDelete.Value = GUILayout.Toggle(BluePrintDelete.Value, "蓝图删除".getTranslate() + "(ctrl+X）");
                    BluePrintRevoke.Value = GUILayout.Toggle(BluePrintRevoke.Value, "蓝图撤销".getTranslate() + "(ctrl+Z)");
                    BluePrintSetRecipe.Value = GUILayout.Toggle(BluePrintSetRecipe.Value, "蓝图设置配方".getTranslate() + "(ctrl+F)", buttonoptions);
                    bool temp = GUILayout.Toggle(ShowStationInfo.Value, "物流站信息显示".getTranslate());
                    if (temp != ShowStationInfo.Value)
                    {
                        ShowStationInfo.Value = temp;
                        if (!temp)
                            for (int index = 0; index < maxCount; ++index)
                                tips[index].SetActive(false);
                    }
                    if (temp)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30);
                        GUILayout.BeginVertical();
                        bool temp1 = GUILayout.Toggle(!ShowStationInfoMode.Value, "详细模式".getTranslate());
                        bool temp2 = GUILayout.Toggle(ShowStationInfoMode.Value, "简易模式".getTranslate());
                        if (temp1 && temp2)
                        {
                            ShowStationInfoMode.Value = !ShowStationInfoMode.Value;
                        }
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                    }

                    stationcopyItem_bool.Value = GUILayout.Toggle(stationcopyItem_bool.Value, "物流站物品设置复制粘贴".getTranslate(), buttonoptions);
                    if (autoabsorttrash_bool.Value != GUILayout.Toggle(autoabsorttrash_bool.Value, "30s间隔自动吸收垃圾".getTranslate()))
                    {
                        autoabsorttrash_bool.Value = !autoabsorttrash_bool.Value;
                        if (autoabsorttrash_bool.Value)
                        {
                            autocleartrash_bool.Value = false;
                        }
                    }
                    if (autoabsorttrash_bool.Value)
                    {
                        onlygetbuildings.Value = GUILayout.Toggle(onlygetbuildings.Value, "只回收建筑".getTranslate());
                    }
                    if (autocleartrash_bool.Value != GUILayout.Toggle(autocleartrash_bool.Value, "30s间隔自动清除垃圾".getTranslate()))
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
                        GUILayout.Label(tempShowWindow.ToString(), labelstyle);
                    }
                }
                GUILayout.EndVertical();
                GUILayout.Space(20);

                GUILayout.BeginVertical();
                {
                    changeups = GUILayout.Toggle(changeups, "启动时间流速修改".getTranslate());
                    GUILayout.Label("流速倍率".getTranslate() + ":" + string.Format("{0:N2}", upsfix));
                    string tempupsfixstr = GUILayout.TextField(string.Format("{0:N2}", upsfix), new[] { GUILayout.Width(5 * heightdis) });

                    if (tempupsfixstr != string.Format("{0:N2}", upsfix) && float.TryParse(tempupsfixstr, out float tempupsfix))
                    {
                        if (tempupsfix < 0.01) tempupsfix = 0.01f;
                        if (tempupsfix > 10) tempupsfix = 10;
                        upsfix = tempupsfix;
                    }
                    upsfix = GUILayout.HorizontalSlider(upsfix, 0.01f, 10, HorizontalSlideroptions);
                    upsquickset.Value = GUILayout.Toggle(upsquickset.Value, "加速减速".getTranslate() + "(shift,'+''-')", buttonoptions);

                    autosetSomevalue_bool.Value = GUILayout.Toggle(autosetSomevalue_bool.Value, "自动配置建筑".getTranslate());
                    GUILayout.Label("人造恒星燃料数量".getTranslate() + "：" + auto_supply_starfuel.Value);
                    auto_supply_starfuel.Value = (int)GUILayout.HorizontalSlider(auto_supply_starfuel.Value, 4, 100, HorizontalSlideroptions);
                    if (GUILayout.Button("填充当前星球人造恒星".getTranslate(), buttonoptions)) AddFuelToAllStar();
                    _ = new GUILayoutOption[0];
                    autosavetimechange.Value = GUILayout.Toggle(autosavetimechange.Value, "自动保存".getTranslate(), autosavetimechange.Value ? new GUILayoutOption[0] : buttonoptions);
                    if (autosavetimechange.Value)
                    {
                        GUILayout.Label("自动保存时间".getTranslate() + "/min：", buttonoptions);
                        int tempint = autosavetime.Value / 60;
                        if (int.TryParse(Regex.Replace(GUILayout.TextField(tempint + "", GUILayout.Height(heightdis), GUILayout.Width(5 * heightdis)), @"[^0-9]", ""), out tempint))
                        {
                            if (tempint < 5) tempint = 5;
                            if (tempint > 10000) tempint = 10000;
                            autosavetime.Value = tempint * 60;
                        }
                    }
                    KeepBeltHeight.Value = GUILayout.Toggle(KeepBeltHeight.Value, "保持传送带高度(shift)".getTranslate(), buttonoptions);
                    SaveLastOpenBluePrintBrowserPathConfig.Value = GUILayout.Toggle(SaveLastOpenBluePrintBrowserPathConfig.Value, "记录上次蓝图路径".getTranslate(), buttonoptions);
                    Quickstop_bool.Value = GUILayout.Toggle(Quickstop_bool.Value, "ctrl+空格快速开关".getTranslate());
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    stopfactory = GUILayout.Toggle(stopfactory, "停止工厂".getTranslate());
                    GUILayout.EndHorizontal();
                    autonavigation_bool.Value = GUILayout.Toggle(autonavigation_bool.Value, "自动导航".getTranslate());
                    if (autonavigation_bool.Value)
                    {
                        autowarpcommand.Value = GUILayout.Toggle(autowarpcommand.Value, "自动导航使用曲速".getTranslate());
                        GUILayout.Label("自动使用翘曲器距离".getTranslate() + ":" + string.Format("{0:N2}", autowarpdistance.Value) + "光年".getTranslate());
                        autowarpdistance.Value = GUILayout.HorizontalSlider(autowarpdistance.Value, 0, 30, HorizontalSlideroptions);
                    }
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
            }
        }

        private void TextTechPanel()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            int buttonwidth = heightdis * 5;
            int colnum = (int)MainWindowWidth / buttonwidth;
            var propertyicon = UIRoot.instance.uiGame.techTree.nodePrefab.buyoutButton.transform.Find("icon").GetComponent<Image>().mainTexture;
            GUILayout.BeginVertical();
            limitmaterial = GUILayout.Toggle(limitmaterial, "限制材料".getTranslate());
            GUILayout.Label("准备研究".getTranslate());
            var buttonSize = new GUILayoutOption[] { GUILayout.Width(buttonwidth), GUILayout.Height(heightdis * 2) };
            int totalNum = readyresearch.Count;
            int lines = totalNum / colnum + ((totalNum % colnum) > 0 ? 1 : 0);
            var showList = new List<int>(readyresearch);
            for (int i = 0; i < lines; i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < colnum; j++)
                {
                    int index = i * colnum + j;
                    if (index == totalNum) break;
                    GUILayout.BeginVertical(GUILayout.Width(buttonwidth));
                    TechProto tp = LDB.techs.Select(showList[index]);
                    var buttoncontent = tp.ID < 2000 ? tp.name : (tp.name + tp.Level);
                    if (GUILayout.Button(buttoncontent, nomarginButtonStyle, buttonSize))
                    {
                        if (GameMain.history.TechInQueue(showList[i]))
                        {
                            for (int k = 0; k < GameMain.history.techQueue.Length; k++)
                            {
                                if (GameMain.history.techQueue[k] != showList[index]) continue;
                                GameMain.history.RemoveTechInQueue(k);
                                break;
                            }
                        }
                        readyresearch.RemoveAt(i);
                    }

                    GUILayout.BeginHorizontal();
                    foreach (ItemProto ip in tp.itemArray)
                    {
                        GUILayout.Button(ip.iconSprite.texture, nomarginButtonStyle, iconbuttonoptions);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    foreach (RecipeProto rp in tp.unlockRecipeArray)
                    {
                        GUILayout.Button(rp.iconSprite.texture, nomarginButtonStyle, iconbuttonoptions);
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(propertyicon, nomarginButtonStyle, iconbuttonoptions))
                    {
                        BuyoutTech(tp);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
            }


            GUILayout.Label("科技".getTranslate());
            var techList = new List<int>();
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
                techList.Add(tp.ID);
            }
            totalNum = techList.Count;
            lines = totalNum / colnum + ((totalNum % colnum) > 0 ? 1 : 0);
            showList = techList;
            for (int i = 0; i < lines; i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < colnum; j++)
                {
                    int index = i * colnum + j;
                    if (index == totalNum) break;
                    GUILayout.BeginVertical(GUILayout.Width(buttonwidth));
                    TechProto tp = LDB.techs.Select(showList[index]);
                    if (GUILayout.Button(tp.name, nomarginButtonStyle, buttonSize))
                    {
                        readyresearch.Add(tp.ID);
                    }

                    GUILayout.BeginHorizontal();
                    foreach (ItemProto ip in tp.itemArray)
                    {
                        GUILayout.Button(ip.iconSprite.texture, nomarginButtonStyle, iconbuttonoptions);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    foreach (RecipeProto rp in tp.unlockRecipeArray)
                    {
                        GUILayout.Button(rp.iconSprite.texture, nomarginButtonStyle, iconbuttonoptions);
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(propertyicon, nomarginButtonStyle, iconbuttonoptions))
                    {
                        BuyoutTech(tp);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Label("升级".getTranslate());

            techList = new List<int>();
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
                techList.Add(tp.ID);
            }
            totalNum = techList.Count;
            lines = totalNum / colnum + ((totalNum % colnum) > 0 ? 1 : 0);
            showList = techList;

            for (int i = 0; i < lines; i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < colnum; j++)
                {
                    int index = i * colnum + j;
                    if (index == totalNum) break;
                    GUILayout.BeginVertical(GUILayout.Width(buttonwidth));
                    TechProto tp = LDB.techs.Select(showList[index]);
                    if (GUILayout.Button(tp.name, nomarginButtonStyle, buttonSize))
                    {
                        readyresearch.Add(tp.ID);
                    }

                    GUILayout.BeginHorizontal();
                    foreach (ItemProto ip in tp.itemArray)
                    {
                        GUILayout.Button(ip.iconSprite.texture, nomarginButtonStyle, iconbuttonoptions);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    foreach (RecipeProto rp in tp.unlockRecipeArray)
                    {
                        GUILayout.Button(rp.iconSprite.texture, nomarginButtonStyle, iconbuttonoptions);
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(propertyicon, nomarginButtonStyle, iconbuttonoptions))
                    {
                        BuyoutTech(tp);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
        private void DysonPanel()
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

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginHorizontal();
            #region 左侧面板
            GUILayout.BeginVertical();
            GUILayout.Label("选择一个蓝图后，点击右侧的层级可以自动导入".getTranslate());
            if (GUILayout.Button("打开戴森球蓝图文件夹".getTranslate(), GUILayout.Height(heightdis)))
            {
                string path = new StringBuilder(GameConfig.overrideDocumentFolder).Append(GameConfig.gameName).Append("/DysonBluePrint/").ToString();
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                Application.OpenURL(path);
            }
            if (GUILayout.Button("刷新文件".getTranslate(), GUILayout.Height(heightdis)))
            {
                selectDysonBlueprintData.path = "";
                LoadDysonBluePrintData();
            }
            dysonBluePrintscrollPosition = GUILayout.BeginScrollView(dysonBluePrintscrollPosition);

            GUILayout.BeginVertical();
            if (tempDysonBlueprintData.Exists(o => o.type == EDysonBlueprintType.SingleLayer))
            {
                DysonPanelSingleLayer.Value = GUILayout.Toggle(DysonPanelSingleLayer.Value, "单层壳".getTranslate());
                if (DysonPanelSingleLayer.Value)
                {
                    var templist = tempDysonBlueprintData.Where(x => x.type == EDysonBlueprintType.SingleLayer).ToList();
                    DysonBluePrintDataDraw(templist);
                }
            }
            if (tempDysonBlueprintData.Exists(o => o.type == EDysonBlueprintType.Layers))
            {
                DysonPanelLayers.Value = GUILayout.Toggle(DysonPanelLayers.Value, "多层壳".getTranslate());
                if (DysonPanelLayers.Value)
                {
                    var templist = tempDysonBlueprintData.Where(x => x.type == EDysonBlueprintType.Layers).ToList();
                    DysonBluePrintDataDraw(templist);
                }
            }
            if (tempDysonBlueprintData.Exists(o => o.type == EDysonBlueprintType.SwarmOrbits))
            {
                DysonPanelSwarm.Value = GUILayout.Toggle(DysonPanelSwarm.Value, "戴森云".getTranslate());
                if (DysonPanelSwarm.Value)
                {
                    var templist = tempDysonBlueprintData.Where(x => x.type == EDysonBlueprintType.SwarmOrbits).ToList();
                    DysonBluePrintDataDraw(templist);
                }
            }
            if (tempDysonBlueprintData.Exists(o => o.type == EDysonBlueprintType.DysonSphere))
            {
                DysonPanelDysonSphere.Value = GUILayout.Toggle(DysonPanelDysonSphere.Value, "戴森球(包括壳、云)".getTranslate());
                if (DysonPanelDysonSphere.Value)
                {
                    var templist = tempDysonBlueprintData.Where(x => x.type == EDysonBlueprintType.DysonSphere).ToList();
                    DysonBluePrintDataDraw(templist);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            #endregion
            GUILayout.Space(20);
            #region 右侧面板
            GUILayout.BeginVertical();
            if (GUILayout.Button("复制选中文件代码".getTranslate(), GUILayout.Height(heightdis)))
            {
                string data = ReaddataFromFile(selectDysonBlueprintData.path);
                GUIUtility.systemCopyBuffer = data;
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(10000);
                    GUIUtility.systemCopyBuffer = "";
                });
            }
            if (GUILayout.Button("清除剪贴板".getTranslate(), GUILayout.Height(heightdis)))
            {
                GUIUtility.systemCopyBuffer = "";
            }
            if (GUILayout.Button("应用蓝图".getTranslate(), GUILayout.Height(heightdis)) && dyson != null)
            {
                string data = ReaddataFromFile(selectDysonBlueprintData.path);
                ApplyDysonBlueprintManage(data, dyson, selectDysonBlueprintData.type);
            }
            if (GUILayout.Button("自动生成最大半径戴森壳".getTranslate(), GUILayout.Height(heightdis)) && dyson != null)
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
            if (GUILayout.Button("删除全部空壳".getTranslate(), GUILayout.Height(heightdis)) && dyson != null)
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (dyson.layersIdBased[i] != null && dyson.layersIdBased[i].nodeCount == 0)
                    {
                        dyson.RemoveLayer(dyson.layersIdBased[i]);
                    }
                }
            }
            if (autoClearEmptyDyson.Value != GUILayout.Toggle(autoClearEmptyDyson.Value, "自动清除空戴森球".getTranslate()))
            {
                autoClearEmptyDyson.Value = !autoClearEmptyDyson.Value;
                if (autoClearEmptyDyson.Value)
                {
                    UIMessageBox.Show(ErrorTitle.getTranslate(), "每次打开戴森球面板都会自动进行清理".getTranslate(), "确定".Translate(), 3, null);
                }
            }


            GUILayout.Label("当前选中".getTranslate() + ":" +
                UIRoot.instance?.uiGame?.dysonEditor?.selection?.viewDysonSphere?.starData.name ?? "");
            GUILayout.Label("可用戴森壳层级:".getTranslate());
            for (int i = 0; i < 2; i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 5; j++)
                {
                    int index = i * 5 + j + 1;
                    if (GUILayout.Button(dysonlayers[index] ? (index).ToString() : "", nomarginButtonStyle, iconbuttonoptions) && dysonlayers[index])
                    {
                        string data = ReaddataFromFile(selectDysonBlueprintData.path);
                        ApplyDysonBlueprintManage(data, dyson, EDysonBlueprintType.SingleLayer, index);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("不可用戴森壳层级:".getTranslate());
            DeleteDysonLayer = GUILayout.Toggle(DeleteDysonLayer, "勾选即可点击删除".getTranslate());
            GUILayout.EndHorizontal();
            for (int i = 0; i < 2; i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 5; j++)
                {
                    int index = i * 5 + j + 1;
                    if (GUILayout.Button(!dysonlayers[index] ? index.ToString() : "", nomarginButtonStyle, iconbuttonoptions) && DeleteDysonLayer)
                    {
                        RemoveLayerById(index);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            #endregion
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }
        private void DysonBluePrintDataDraw(List<TempDysonBlueprintData> datalist)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(heightdis);
            GUILayout.BeginVertical();
            for (int i = 0; i < datalist.Count; i++)
            {
                bool temp = GUILayout.Toggle(datalist[i].path == selectDysonBlueprintData.path, datalist[i].name, GUILayout.Height(heightdis));
                if (temp != (datalist[i].path == selectDysonBlueprintData.path))
                {
                    selectDysonBlueprintData = datalist[i];
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        #endregion

        #region 窗口操作

        private void UIPanelSet()
        {
            var Canvasrt = UIRoot.instance.overlayCanvas.GetComponent<RectTransform>();
            var rt = ui_AuxilaryPanelPanel.GetComponent<RectTransform>();
            float CanvaswidthMultiple = Canvasrt.sizeDelta.x * 1.0f / Screen.width;
            float CanvasheightMultiple = Canvasrt.sizeDelta.y * 1.0f / Screen.height;
            rt.sizeDelta = new Vector2(CanvaswidthMultiple * MainWindowWidth, CanvasheightMultiple * MainWindowHeight);
            rt.localPosition = new Vector2(-Canvasrt.sizeDelta.x / 2 + MainWindow_x * CanvaswidthMultiple, Canvasrt.sizeDelta.y / 2 - MainWindow_y * CanvasheightMultiple - rt.sizeDelta.y);
        }

        /// <summary>
        /// 移动窗口
        /// </summary>
        private void MoveWindow()
        {
            if (leftscaling || rightscaling || bottomscaling) return;
            Vector2 temp = Input.mousePosition;
            if (temp.x > MainWindow_x && temp.x < MainWindow_x + MainWindowWidth && Screen.height - temp.y > MainWindow_y && Screen.height - temp.y < MainWindow_y + 20)
            {
                if (Input.GetMouseButton(0))
                {
                    if (!moving)
                    {
                        MainWindow_x_move = MainWindow_x;
                        MainWindow_y_move = MainWindow_y;
                        temp_MainWindow_x = temp.x;
                        temp_MainWindow_y = Screen.height - temp.y;
                    }
                    moving = true;
                    MainWindow_x = MainWindow_x_move + temp.x - temp_MainWindow_x;
                    MainWindow_y = MainWindow_y_move + (Screen.height - temp.y) - temp_MainWindow_y;
                }
                else
                {
                    moving = false;
                    temp_MainWindow_x = MainWindow_x;
                    temp_MainWindow_y = MainWindow_y;
                }
            }
            else if (moving)
            {
                moving = false;
                MainWindow_x = MainWindow_x_move + temp.x - temp_MainWindow_x;
                MainWindow_y = MainWindow_y_move + (Screen.height - temp.y) - temp_MainWindow_y;
            }
            MainWindow_y = Math.Max(10, Math.Min(Screen.height - 10, MainWindow_y));
            MainWindow_x = Math.Max(10, Math.Min(Screen.width - 10, MainWindow_x));
        }

        /// <summary>
        /// 改变窗口大小
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="window_x"></param>
        /// <param name="window_y"></param>
        private void Scaling_window()
        {
            Vector2 temp = Input.mousePosition;
            float x = MainWindowWidth;
            float y = MainWindowHeight;
            if (Input.GetMouseButton(0))
            {
                if ((temp.x + 10 > MainWindow_x && temp.x - 10 < MainWindow_x) && (Screen.height - temp.y >= MainWindow_y && Screen.height - temp.y <= MainWindow_y + y) || leftscaling)
                {
                    x -= temp.x - MainWindow_x;
                    MainWindow_x = temp.x;
                    leftscaling = true;
                    rightscaling = false;
                }
                if ((temp.x + 10 > MainWindow_x + x && temp.x - 10 < MainWindow_x + x) && (Screen.height - temp.y >= MainWindow_y && Screen.height - temp.y <= MainWindow_y + y) || rightscaling)
                {
                    x += temp.x - MainWindow_x - x;
                    rightscaling = true;
                    leftscaling = false;
                }
                if ((Screen.height - temp.y + 10 > y + MainWindow_y && Screen.height - temp.y - 10 < y + MainWindow_y) && (temp.x >= MainWindow_x && temp.x <= MainWindow_x + x) || bottomscaling)
                {
                    y += Screen.height - temp.y - (MainWindow_y + y);
                    bottomscaling = true;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                rightscaling = false;
                leftscaling = false;
                bottomscaling = false;
            }
            MainWindowWidth = x;
            MainWindowHeight = y;
        }
        #endregion
    }
}
