using Auxilaryfunction.Patch;
using BepInEx.Configuration;
using HarmonyLib;
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
        private int whichpannel;
        private string[] menus;
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
        public static List<string> ConfigNames;
        public static Vector2 scrollPosition;
        public static Vector2 dysonBluePrintscrollPosition;
        public static ConfigEntry<int> scale;
        public static KeyboardShortcut tempShowWindow;
        public static bool blueprintopen;
        public static bool showwindow;
        public static bool ChangeQuickKey;
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
        public static List<float[]> boundaries;
        GUIStyle normalstyle;
        GUIStyle styleblue;
        GUIStyle styleyellow;
        GUIStyle styleitemname = null;
        GUIStyle buttonstyleyellow = null;
        GUIStyle buttonstyleblue = null;
        GUIStyle labelstyle = null;
        GUIStyle whitestyle = null;
        GUIStyle nomarginButtonStyle = null;
        private GUIStyle selectedButtonStyle;
        private GUILayoutOption[] menusbuttonoptions;
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
        public Sprite trashcan;
        public Sprite trashcanOpen;
        private void Init()
        {
            whichpannel = 1;
            RefreshBaseSize = true;
            boundaries = new List<float[]>();
            ConfigNames = new List<string>();
            normalstyle = new GUIStyle();
            styleblue = new GUIStyle();
            styleyellow = new GUIStyle();
            MainWindowWidth = window_width.Value;
            MainWindowHeight = window_height.Value;
            leftsprite = LoadImage("Auxilaryfunction.Resources.left.png");
            rightsprite = LoadImage("Auxilaryfunction.Resources.right.png");
            flatsprite = LoadImage("Auxilaryfunction.Resources.flat.png");
            trashcan = LoadImage("Auxilaryfunction.Resources.trashcan.png");
            trashcanOpen = LoadImage("Auxilaryfunction.Resources.trashcanOpen.png");
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
            boundaries.Add(new float[] { 0.01f, 1 });
            ConfigNames.Add("翘曲填充数量");
            boundaries.Add(new float[] { 0, 50 });
            ConfigNames.Add("大型采矿机采矿速率");
            boundaries.Add(new float[] { 10, 30 });
            menus = new string[7] { "", "自动菜单", "渲染屏蔽", "便捷小功能", "文字科技树", "戴森球面板", "战斗" };

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
            if (!ChangingQuickKey && GameDataImported)
            {
                bool isClick = false;
                if (QuickKey.Value.Modifiers.Count() == 0)
                {
                    if (Input.GetKeyDown(QuickKey.Value.MainKey))
                    {
                        isClick = true;
                    }
                }
                else
                {
                    if (Input.GetKey(QuickKey.Value.MainKey))
                    {
                        isClick = true;
                        foreach (var modify in QuickKey.Value.Modifiers)
                        {
                            if (!Input.GetKeyDown(modify))
                            {
                                isClick = false;
                            }
                        }
                    }
                }
                if (isClick)
                {
                    showwindow = !showwindow;
                    if (showwindow)
                    {
                        firstDraw = true;
                    }
                    ui_AuxilaryPanelPanel.SetActive(showwindow);
                }
            }
            //if (QuickKey.Value.IsPressed() && !ChangingQuickKey && GameDataImported)
            //{
            //    showwindow = !showwindow;
            //    if (showwindow)
            //    {
            //        firstDraw = true;
            //    }
            //    ui_AuxilaryPanelPanel.SetActive(showwindow);
            //}
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
                menusbuttonoptions = new[] { GUILayout.Height(heightdis), GUILayout.Width(heightdis * 4) };
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
                selectedButtonStyle = new GUIStyle(GUI.skin.button);
                selectedButtonStyle.normal.textColor = new Color32(215, 186, 245, 255);
            }
            if (showwindow)
            {
                UIPanelSet();
                MoveWindow();
                Scaling_window();
                Rect window = new Rect(MainWindow_x, MainWindow_y, MainWindowWidth, MainWindowHeight);
                GUI.DrawTexture(window, mytexture);
                window = GUI.Window(20210827, window, MainWindow, "辅助面板".getTranslate() + "(" + VERSION + ")" + "ps:ctrl+↑↓");
            }
            if (autonavigation_bool.Value && GameDataImported && GameMain.mainPlayer != null && PlayerOperation.NeedNavigation)
            {
                GUILayout.BeginArea(new Rect(10, 250, 500, 300));
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();

                if (GUILayout.Button(PlayerOperation.FlyStatus ? "暂停导航".getTranslate() : "继续导航".getTranslate(), GUILayout.MinHeight(heightdis)))
                {
                    PlayerOperation.FlyStatus = !PlayerOperation.FlyStatus;
                }
                if (GUILayout.Button("取消方向指示".getTranslate(), GUILayout.MinHeight(heightdis)))
                {
                    PlayerOperation.FlyStatus = false;
                    UIRoot.instance?.uiGame?.dfMonitor?.trackingHives?.Clear();
                    PlayerOperation.ClearFollow();
                }


                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            //LocalPlanet.factory.prebuildPool.ToList().Exists(x => x.protoId != 0)
            if (automovetounbuilt.Value && player != null && LocalPlanet?.factory != null && player.movementState == EMovementState.Fly)
            {
                for (int i = 1; i < GameMain.localPlanet.factory.prebuildCursor; i++)
                {
                    int preid = GameMain.localPlanet.factory.prebuildPool[i].id;
                    if (preid == i && GameMain.localPlanet.factory.prebuildPool[i].protoId == 0)
                    {
                        GameMain.localPlanet.factory.RemovePrebuildWithComponents(preid);
                    }
                }
                if (LocalPlanet.factory.prebuildCount > 0)
                {
                    if (GUI.Button(new Rect(10, 360, 150, 60), StartAutoMovetounbuilt ? "停止寻找未完成建筑".getTranslate() : "开始寻找未完成建筑".getTranslate()))
                    {
                        StartAutoMovetounbuilt = !StartAutoMovetounbuilt;
                        player.gameObject.GetComponent<SphereCollider>().enabled = !StartAutoMovetounbuilt;
                        player.gameObject.GetComponent<CapsuleCollider>().enabled = !StartAutoMovetounbuilt;
                    }
                    if (StartAutoMovetounbuilt && LocalPlanet.gasItems == null && GUI.Button(new Rect(10, 420, 150, 60), autobuildgetitem ? "停止自动补充材料".getTranslate() : "开始自动补充材料".getTranslate()))
                    {
                        autobuildgetitem = !autobuildgetitem;
                    }
                }
                else
                {
                    StartAutoMovetounbuilt = false;
                    player.gameObject.GetComponent<SphereCollider>().enabled = !StartAutoMovetounbuilt;
                    player.gameObject.GetComponent<CapsuleCollider>().enabled = !StartAutoMovetounbuilt;
                }
            }
            else if (StartAutoMovetounbuilt)
            {
                StartAutoMovetounbuilt = false;
                player.gameObject.GetComponent<SphereCollider>().enabled = true;
                player.gameObject.GetComponent<CapsuleCollider>().enabled = true;
            }

            if (automovetodarkfog.Value && player != null && LocalPlanet?.factory != null && player.movementState == EMovementState.Fly)
            {
                if (LocalPlanet.factory.enemyCount > 0)
                {
                    if (GUI.Button(new Rect(10, 490, 150, 60), StartAutoMovetoDarkfog ? "停止飞向黑雾基地".getTranslate() : "开始飞向黑雾基地".getTranslate()))
                    {
                        StartAutoMovetoDarkfog = !StartAutoMovetoDarkfog;
                        player.gameObject.GetComponent<SphereCollider>().enabled = !StartAutoMovetoDarkfog;
                        player.gameObject.GetComponent<CapsuleCollider>().enabled = !StartAutoMovetoDarkfog;
                    }
                    if (StartAutoMovetoDarkfog && GUI.Button(new Rect(10, 560, 150, 60), autoRemoveRuin ? "停止自动填埋".getTranslate() : "开始自动填埋".getTranslate()))
                    {
                        autoRemoveRuin = !autoRemoveRuin;
                    }
                }
                else
                {
                    StartAutoMovetoDarkfog = false;
                    player.gameObject.GetComponent<SphereCollider>().enabled = true;
                    player.gameObject.GetComponent<CapsuleCollider>().enabled = true;
                }
            }
            else if (StartAutoMovetoDarkfog)
            {
                StartAutoMovetoDarkfog = false;
                player.gameObject.GetComponent<SphereCollider>().enabled = true;
                player.gameObject.GetComponent<CapsuleCollider>().enabled = true;
            }
            BluePrintRecipeSet();
        }

        public void BluePrintRecipeSet()
        {
            if (!blueprintopen || LocalPlanet?.factory == null)
                return;
            int tempwidth = 0;
            int tempheight = 0;
            PlanetFactory factory = LocalPlanet.factory;
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
                            else
                            {
                                LocalPlanet.factory.WriteExtraInfoOnEntity(sc.entityId, StationNames[i]);
                            }
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
            if (!ShowStationInfo.Value || GameMain.localPlanet?.factory?.transport == null)
            {
                if (stationTip.activeSelf)
                {
                    stationTip.SetActive(false);
                }
                return;
            }
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
                            string stationComponentName = pd.factory.ReadExtraInfoOnEntity(stationComponent.entityId);
                            if (!string.IsNullOrEmpty(stationComponentName))
                            {
                                var lastcountTextPosition = lastcountText.GetComponent<RectTransform>().anchoredPosition3D;
                                lastcountText.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(90, lastcountTextPosition.y, 0);
                                lastcountText.GetComponent<Text>().fontSize = 18;
                                lastcountText.GetComponent<Text>().text = stationComponentName;
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
            #region TurretWindow
            TurretWindow = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Turret Window"), GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Blueprint Copy Mode").transform);
            TurretWindow.GetComponent<RectTransform>().position = new Vector3(7.5f, 50, 20);
            TurretWindow.name = "auxilaryfunction_TurretWindow";
            TurretWindow.gameObject.SetActive(false);
            Destroy(TurretWindow.transform.Find("details-desc").gameObject);
            Destroy(TurretWindow.transform.Find("ammo-desc").gameObject);
            Destroy(TurretWindow.transform.Find("supernova-desc/switch-button-1").gameObject);
            Destroy(TurretWindow.transform.Find("supernova-desc/switch-button-2").gameObject);
            Destroy(TurretWindow.transform.Find("supernova-desc/switch-button-3").gameObject);
            Destroy(TurretWindow.transform.Find("supernova-desc/tap").gameObject);
            Destroy(TurretWindow.transform.Find("supernova-desc/effect-desc").gameObject);
            uiturretWindow = TurretWindow.GetComponent<UITurretWindow>();
            uiturretWindow.phaseShiftGroup.SetActive(false);
            uiturretWindow.supernovaGroup.SetActive(value: true);
            uiturretWindow.supernovaBtnText.text = "超新星".Translate();
            uiturretWindow.vsGroundRangeLabelRt.gameObject.SetActive(true);
            uiturretWindow.vsGroundRangeTextRt.gameObject.SetActive(true);
            uiturretWindow.vsSpaceRangeLabelRt.gameObject.SetActive(true);
            uiturretWindow.vsSpaceRangeTextRt.gameObject.SetActive(true);
            uiturretWindow.mode0Btn.button.interactable = true;
            uiturretWindow.mode1Btn.button.interactable = true;
            uiturretWindow.mode2Btn.button.interactable = true;
            uiturretWindow.mode3Btn.button.interactable = true;

            uiturretWindow.mode0Btn.tips.tipTitle = "地面按钮标题".Translate();
            uiturretWindow.mode1Btn.tips.tipTitle = "低空按钮标题".Translate();
            uiturretWindow.mode2Btn.tips.tipTitle = "高空按钮标题".Translate();
            uiturretWindow.mode3Btn.tips.tipTitle = "太空按钮标题".Translate();
            uiturretWindow.superNovaBtn.onClick += _ =>
            {
                foreach (var turretId in turrentpools)
                {
                    ref TurretComponent ptr = ref GameMain.localPlanet.factory.defenseSystem.turrets.buffer[turretId];
                    if (ptr.id != turretId)
                    {
                        return;
                    }
                    if (ptr.inSupernova)
                    {
                        ptr.CancelSupernova();
                    }
                    else
                    {
                        ptr.SetSupernova();
                    }
                    if (ptr.inSupernova)
                    {
                        GameMain.gameScenario.NotifyOnSupernovaUITriggered();
                    }
                }
            };
            Action<int> tempOnVSModeButtonClicked = new Action<int>(value =>
            {
                Traverse.Create(uiturretWindow).Field("vsModeSelectIndex").SetValue(value);
                uiturretWindow.selectBoxRt.gameObject.SetActive(true);
                if (value == 0)
                {
                    uiturretWindow.selectBoxRt.SetParent(uiturretWindow.mode0Rt, false);
                    return;
                }
                if (value == 1)
                {
                    uiturretWindow.selectBoxRt.SetParent(uiturretWindow.mode1Rt, false);
                    return;
                }
                if (value == 2)
                {
                    uiturretWindow.selectBoxRt.SetParent(uiturretWindow.mode2Rt, false);
                    return;
                }
                if (value == 3)
                {
                    uiturretWindow.selectBoxRt.SetParent(uiturretWindow.mode3Rt, false);
                }
            });
            Action<int> tempGroupSelectionBtn_onClick = new Action<int>(value =>
            {
                foreach (var turretId in turrentpools)
                {
                    ref TurretComponent ptr = ref GameMain.localPlanet.factory.defenseSystem.turrets.buffer[turretId];
                    if (ptr.id != turretId)
                    {
                        continue;
                    }
                    if (value == ptr.group)
                    {
                        ptr.SetGroup(0);
                    }
                    else
                    {
                        ptr.SetGroup(value);
                    }
                    for (int i = 0; i < uiturretWindow.groupSelectionBtns.Length; i++)
                    {
                        UIButton uibutton = uiturretWindow.groupSelectionBtns[i];
                        uibutton.highlighted = (uibutton.data == ptr.group);
                    }
                }
            });

            var OnPrioritySelectButtonClicked = new Action<int>(value =>
            {
                var vsModeSelectIndex = (int)Traverse.Create(uiturretWindow).Field("vsModeSelectIndex").GetValue();
                foreach (var turretId in turrentpools)
                {
                    ref TurretComponent ptr = ref GameMain.localPlanet.factory.defenseSystem.turrets.buffer[turretId];
                    if (ptr.id != turretId)
                    {
                        continue;
                    }
                    VSLayerMask vsSettings = ptr.vsSettings;
                    if (vsModeSelectIndex == 0)
                    {
                        ptr.vsSettings &= VSLayerMask.AirAngOrbitAndSpace;
                    }
                    else if (vsModeSelectIndex == 1)
                    {
                        ptr.vsSettings &= VSLayerMask.GroundAndOrbitAndSpace;
                    }
                    else if (vsModeSelectIndex == 2)
                    {
                        ptr.vsSettings &= VSLayerMask.GroundAndAirAndSpace;
                    }
                    else if (vsModeSelectIndex == 3)
                    {
                        ptr.vsSettings &= VSLayerMask.GroundAndAirAndOrbit;
                    }
                    ptr.vsSettings |= (VSLayerMask)(value << vsModeSelectIndex * 2);
                    if (ptr.vsSettings != vsSettings)
                    {
                        ptr.Search(GameMain.localPlanet.factory, PlanetFactory.PrefabDescByModelIndex[GameMain.localPlanet.factory.entityPool[ptr.entityId].modelIndex], GameMain.gameTick);
                    }
                }
                var modeIcons = new Image[4] { uiturretWindow.mode0Icon, uiturretWindow.mode1Icon, uiturretWindow.mode2Icon, uiturretWindow.mode3Icon };
                var modeFloors = new Image[4] { uiturretWindow.mode0Floor, uiturretWindow.mode1Floor, uiturretWindow.mode2Floor, uiturretWindow.mode3Floor };
                var mode0Btn = new UIButton[4] { uiturretWindow.mode0Btn, uiturretWindow.mode1Btn, uiturretWindow.mode2Btn, uiturretWindow.mode3Btn };
                var tipText = new string[4] { "已关闭".Translate(), "低优先".Translate(), "均衡".Translate(), "高优先".Translate() };

                if (value == 0)
                {
                    modeIcons[vsModeSelectIndex].sprite = uiturretWindow.fullTransparentSprite;
                    modeFloors[vsModeSelectIndex].color = uiturretWindow.disableModeColor;
                    mode0Btn[vsModeSelectIndex].tips.tipText = tipText[value];
                }
                else
                {
                    modeFloors[vsModeSelectIndex].color = Color.clear;
                    switch (value)
                    {
                        case 1:
                            modeIcons[vsModeSelectIndex].sprite = uiturretWindow.lowPrioritySprite;
                            break;
                        case 2:
                            modeIcons[vsModeSelectIndex].sprite = uiturretWindow.normalPrioritySprite;
                            break;
                        case 3:
                            modeIcons[vsModeSelectIndex].sprite = uiturretWindow.highPrioritySprite;
                            break;
                    }
                    mode0Btn[vsModeSelectIndex].tips.tipText = tipText[value];
                }
                uiturretWindow.selectBoxRt.gameObject.SetActive(false);
            });
            uiturretWindow.mode0Btn.onClick += tempOnVSModeButtonClicked;
            uiturretWindow.mode1Btn.onClick += tempOnVSModeButtonClicked;
            uiturretWindow.mode2Btn.onClick += tempOnVSModeButtonClicked;
            uiturretWindow.mode3Btn.onClick += tempOnVSModeButtonClicked;
            uiturretWindow.select0Btn.onClick += OnPrioritySelectButtonClicked;
            uiturretWindow.select1Btn.onClick += OnPrioritySelectButtonClicked;
            uiturretWindow.select2Btn.onClick += OnPrioritySelectButtonClicked;
            uiturretWindow.select3Btn.onClick += OnPrioritySelectButtonClicked;
            uiturretWindow.clearMagBtn.onClick += (_) =>
            {
                foreach (var turretId in turrentpools)
                {
                    ref TurretComponent ptr = ref GameMain.localPlanet.factory.defenseSystem.turrets.buffer[turretId];
                    if (ptr.id != turretId)
                    {
                        continue;
                    }
                    if (ptr.itemId == 0)
                    {
                        continue;
                    }
                    int upCount = player.TryAddItemToPackage((int)ptr.itemId, (int)ptr.itemCount, (int)ptr.itemInc, false, 0, false);
                    UIItemup.Up(ptr.itemId, upCount);
                    ptr.ClearItem();
                }
            };
            for (int i = 0; i < uiturretWindow.groupSelectionBtns.Length; i++)
            {
                uiturretWindow.groupSelectionBtns[i].onClick += tempGroupSelectionBtn_onClick;
            }


            #endregion
            #region PowerExchangerWindow
            PowerExchangerWindow = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Power Exchanger Window"), GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Blueprint Copy Mode").transform);
            PowerExchangerWindow.GetComponent<RectTransform>().position = new Vector3(7.5f, 50, 20);
            PowerExchangerWindow.name = "PowerExchangerWindow";
            PowerExchangerWindow.gameObject.SetActive(false);
            Destroy(PowerExchangerWindow.transform.Find("power-network-desc").gameObject);
            Destroy(PowerExchangerWindow.transform.Find("power-network-desc").gameObject);
            Destroy(PowerExchangerWindow.transform.Find("exchanger-desc/power-state/power-text").gameObject);
            uipowerExchangerWindow = PowerExchangerWindow.GetComponent<UIPowerExchangerWindow>();
            Vector2 transition0Pos = uipowerExchangerWindow.transition0.anchoredPosition;
            Vector2 transition1Pos = uipowerExchangerWindow.transition1.anchoredPosition;
            Action<int> OnModeButtonClick = new Action<int>(targetState =>
            {
                foreach (var powerexchangerid in powerexchangertools)
                {
                    GameMain.localPlanet.factory.powerSystem.excPool[powerexchangerid].targetState = (float)targetState;
                }

                switch (targetState)
                {
                    case -1:
                        uipowerExchangerWindow.exchangerMode1UIButton.highlighted = true;
                        uipowerExchangerWindow.exchangerMode2UIButton.highlighted = false;
                        uipowerExchangerWindow.exchangerMode3UIButton.highlighted = false;
                        uipowerExchangerWindow.transition0.anchoredPosition = transition0Pos;
                        uipowerExchangerWindow.transition1.anchoredPosition = transition1Pos;
                        if (!uipowerExchangerWindow.transition0.gameObject.activeSelf && !uipowerExchangerWindow.transition1.gameObject.activeSelf)
                        {
                            uipowerExchangerWindow.transition0.gameObject.SetActive(true);
                            uipowerExchangerWindow.transition1.gameObject.SetActive(true);
                        }
                        break;
                    case 0:
                        uipowerExchangerWindow.exchangerMode1UIButton.highlighted = false;
                        uipowerExchangerWindow.exchangerMode2UIButton.highlighted = true;
                        uipowerExchangerWindow.exchangerMode3UIButton.highlighted = false;
                        if (uipowerExchangerWindow.transition0.gameObject.activeSelf && uipowerExchangerWindow.transition1.gameObject.activeSelf)
                        {
                            uipowerExchangerWindow.transition0.gameObject.SetActive(false);
                            uipowerExchangerWindow.transition1.gameObject.SetActive(false);
                        }
                        break;
                    case 1:
                        uipowerExchangerWindow.exchangerMode1UIButton.highlighted = false;
                        uipowerExchangerWindow.exchangerMode2UIButton.highlighted = false;
                        uipowerExchangerWindow.exchangerMode3UIButton.highlighted = true;
                        uipowerExchangerWindow.transition0.anchoredPosition = transition1Pos;
                        uipowerExchangerWindow.transition1.anchoredPosition = transition0Pos;
                        if (!uipowerExchangerWindow.transition0.gameObject.activeSelf && !uipowerExchangerWindow.transition1.gameObject.activeSelf)
                        {
                            uipowerExchangerWindow.transition0.gameObject.SetActive(true);
                            uipowerExchangerWindow.transition1.gameObject.SetActive(true);
                        }
                        break;
                }
            });
            uipowerExchangerWindow.exchangerMode1UIButton.onClick += OnModeButtonClick;
            uipowerExchangerWindow.exchangerMode2UIButton.onClick += OnModeButtonClick;
            uipowerExchangerWindow.exchangerMode3UIButton.onClick += OnModeButtonClick;
            uipowerExchangerWindow.emptyIncs[0].enabled = false;
            uipowerExchangerWindow.emptyIncs[1].enabled = false;
            uipowerExchangerWindow.emptyIncs[2].enabled = false;
            uipowerExchangerWindow.fullIncs[0].enabled = false;
            uipowerExchangerWindow.fullIncs[1].enabled = false;
            uipowerExchangerWindow.fullIncs[2].enabled = false;
            uipowerExchangerWindow.emptyCountWarning.gameObject.SetActive(false);
            uipowerExchangerWindow.fullCountWarning.gameObject.SetActive(false);


            #endregion
            #region StationWindow
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
            #endregion
            #region TrashStorageWindow
            TrashCanButton = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Game Menu/button-1-bg"), GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Player Inventory").transform);
            TrashCanButton.transform.Find("button-1/icon").GetComponent<Image>().sprite = trashcan;
            TrashCanButton.transform.Find("button-1/icon").GetComponent<RectTransform>().sizeDelta = new Vector2(22, 22);
            TrashCanButton.transform.localPosition = new Vector3(630, -60, 0);
            TrashCanButton.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
            TrashCanButton.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
            TrashCanButton.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            TrashCanButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-58, -65);
            TrashCanButton.GetComponent<UIButton>().tips.tipTitle = "垃圾桶".getTranslate();
            TrashCanButton.GetComponent<UIButton>().tips.tipText = "开关显示垃圾桶，垃圾桶关闭后自动清理内部物品".getTranslate();
            TrashCanButton.GetComponent<UIButton>().onClick += (_) =>
            {
                bool isOpen = !TrashStorageWindow.activeSelf;
                if (isOpen)
                {
                    UIRoot.instance.uiGame.ShutAllFunctionWindow();
                    TrashStorageWindow.SetActive(isOpen);
                    TrashCanButton.transform.Find("button-1/icon").GetComponent<Image>().sprite = trashcanOpen;
                    uiTrashStorageWindow.storageUI._Free();
                    uiTrashStorageWindow.storageUI._Init(TrashStorage);
                    uiTrashStorageWindow.storageUI._Open();
                    uiTrashStorageWindow.storageUI.OnStorageDataChanged();
                }
                else
                {
                    CloseTrashStorageWindow();
                }
            };

            TrashStorage = new StorageComponent(30);
            TrashStorageWindow = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Storage Window"), GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Player Inventory").transform);
            TrashStorageWindow.name = "TrashStorageWindow";

            Destroy(TrashStorageWindow.transform.Find("filter-group").gameObject);
            Destroy(TrashStorageWindow.transform.Find("panel-bg/drag-trigger").gameObject);
            Destroy(TrashStorageWindow.transform.Find("bans-bar").gameObject);

            uiTrashStorageWindow = TrashStorageWindow.GetComponent<UIStorageWindow>();
            uiTrashStorageWindow.windowTrans.anchorMin = new Vector2(1, 1);
            uiTrashStorageWindow.windowTrans.anchorMax = new Vector2(1, 1);
            uiTrashStorageWindow.windowTrans.anchoredPosition = new Vector2(-14, -25);
            uiTrashStorageWindow.windowTrans.pivot = new Vector2(0, 1);
            uiTrashStorageWindow.windowTrans.sizeDelta = new Vector2(uiTrashStorageWindow.windowTrans.sizeDelta.x, 240);
            uiTrashStorageWindow.titleText.text = "垃圾桶".getTranslate();
            uiTrashStorageWindow.storageUI._Create();
            var sortbtn = uiTrashStorageWindow.transform.Find("panel-bg/btn-box/sort-btn").GetComponent<UIButton>();
            sortbtn.onClick += (_) =>
            {
                TrashStorage.Sort(true);
            };
            UIButton closebutton = TrashStorageWindow.transform.Find("panel-bg/btn-box/close-btn").GetComponent<UIButton>();
            closebutton.onClick += (_) =>
            {
                CloseTrashStorageWindow();
            };
            TrashStorageWindow.SetActive(false);
            TrashCanButton.SetActive(TrashStorageWindow_bool.Value);
            #endregion

        }

        public void CloseTrashStorageWindow()
        {
            TrashCanButton.transform.Find("button-1/icon").GetComponent<Image>().sprite = trashcan;
            TrashStorageWindow.SetActive(false);
            uiTrashStorageWindow.storageUI._Free();
            TrashStorage.Clear();
        }

        private void SetSignalId(int signalId)
        {
            if (LDB.signals.IconSprite(signalId) == null) return;
            pointsignalid = signalId;
            beltWindow.transform.Find("item-sign").GetComponent<Image>().sprite = LDB.signals.IconSprite(signalId);
        }

        #region 窗口UI
        private void MainWindow(int winId)
        {
            GUILayout.BeginArea(new Rect(10, 20, MainWindowWidth, MainWindowHeight));

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            for (int i = 1; i <= 6; i++)
            {
                if (whichpannel == i)
                {
                    GUILayout.Button(menus[i].getTranslate(), selectedButtonStyle, menusbuttonoptions);
                }
                else
                {
                    if (GUILayout.Button(menus[i].getTranslate(), menusbuttonoptions)) { whichpannel = i; }
                }
            }
            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, new[] { GUILayout.Width(MainWindowWidth - 20), GUILayout.Height(MainWindowHeight - heightdis * 2) });

            switch (whichpannel)
            {
                case 1:
                    DefaultPanel();
                    break;
                case 2:
                    DisablePaintingPanel();
                    break;
                case 3:
                    EasyUsePanel();
                    break;
                case 4:
                    TextTechPanel();
                    break;
                case 5:
                    DysonPanel();
                    break;
                case 6:
                    BattlePanel();
                    break;
            }


            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DefaultPanel()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                auto_supply_station.Value = GUILayout.Toggle(auto_supply_station.Value, "自动配置新运输站".getTranslate(), buttonoptions);
                autosetstationconfig.Value = GUILayout.Toggle(autosetstationconfig.Value, "配置参数".getTranslate(), buttonoptions);
                GUILayout.EndHorizontal();
                if (autosetstationconfig.Value)
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
                                showinfo = ((int)(ShipStartCarry.Value * 10) * 10 == 0 ? "1" : "" + (int)(ShipStartCarry.Value * 10) * 10) + "% ";
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
            }
            GUILayout.EndVertical();
            GUILayout.Space(20);

            GUILayout.BeginVertical();
            {
                autosetSomevalue_bool.Value = GUILayout.Toggle(autosetSomevalue_bool.Value, "自动填充人造恒星".getTranslate());
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 2; j++)
                {
                    int itemID = 1803 + j;
                    GUIStyle style = normalstyle;
                    if (auto_supply_starfuelID.Value == itemID)
                        style = whitestyle;
                    if (GUILayout.Button(LDB.items.Select(itemID).iconSprite.texture, style, GUILayout.Height(heightdis), GUILayout.Width(heightdis)))
                    {
                        auto_supply_starfuelID.Value = itemID;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("人造恒星燃料数量".getTranslate() + "：" + auto_supply_starfuel.Value);
                auto_supply_starfuel.Value = (int)GUILayout.HorizontalSlider(auto_supply_starfuel.Value, 1, 100, HorizontalSlideroptions);
                if (GUILayout.Button("填充当前星球人造恒星".getTranslate(), buttonoptions)) AddFuelToAllStar();

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
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("科技等级上限".getTranslate() + ":");
                    string t = GUILayout.TextField(auto_add_techmaxLevel.Value.ToString(), new[] { GUILayout.Height(heightdis), GUILayout.Width(heightdis * 3) });
                    GUILayout.EndHorizontal();
                    bool reset = !int.TryParse(Regex.Replace(t, @"^[^0-9]", ""), out int maxlevel);
                    if (maxlevel != 0)
                    {
                        auto_add_techmaxLevel.Value = Math.Min(maxlevel, 10000);
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
                auto_setejector_bool.Value = GUILayout.Toggle(auto_setejector_bool.Value, "自动配置太阳帆弹射器".getTranslate());
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
            }
            GUILayout.EndVertical();

            GUILayout.Space(30);
            GUILayout.BeginVertical();
            GUILayout.Label("机甲自动化");
            autoAddwarp.Value = GUILayout.Toggle(autoAddwarp.Value, "自动添加翘曲器".getTranslate());

            automovetounbuilt.Value = GUILayout.Toggle(automovetounbuilt.Value, "自动飞向未完成建筑".getTranslate());
            automovetodarkfog.Value = GUILayout.Toggle(automovetodarkfog.Value, "自动飞向地面黑雾".getTranslate());

            autonavigation_bool.Value = GUILayout.Toggle(autonavigation_bool.Value, "自动导航".getTranslate());
            if (autonavigation_bool.Value)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Space(20);
                GUILayout.BeginVertical();
                autoAddPlayerVel.Value = GUILayout.Toggle(autoAddPlayerVel.Value, "自动加速".getTranslate());
                autowarpcommand.Value = GUILayout.Toggle(autowarpcommand.Value, "自动导航使用曲速".getTranslate());
                GUILayout.Label("自动曲速最低距离".getTranslate() + ":" + string.Format("{0:N2}", autowarpdistance.Value) + "光年".getTranslate());
                autowarpdistance.Value = GUILayout.HorizontalSlider(autowarpdistance.Value, 0, 15, HorizontalSlideroptions);
                GUILayout.Label("自动曲速最低能量".getTranslate() + ":" + string.Format("{0:N2}", autowarpdistanceEnergyPercent.Value) + "%");
                autowarpdistanceEnergyPercent.Value = GUILayout.HorizontalSlider(autowarpdistanceEnergyPercent.Value, 0, 95, HorizontalSlideroptions);
                autowarpdistanceEnergyPercent.Value = (int)(autowarpdistanceEnergyPercent.Value / 5 * 5);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DisablePaintingPanel()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            {
                GUILayout.Label("模型屏蔽");
                norender_dysonshell_bool.Value = GUILayout.Toggle(norender_dysonshell_bool.Value, "不渲染戴森壳".getTranslate());
                norender_dysonswarm_bool.Value = GUILayout.Toggle(norender_dysonswarm_bool.Value, "不渲染戴森云".getTranslate());
                norender_lab_bool.Value = GUILayout.Toggle(norender_lab_bool.Value, "不渲染研究站".getTranslate());
                norender_beltitem.Value = GUILayout.Toggle(norender_beltitem.Value, "不渲染传送带货物".getTranslate());
                norender_DarkFog.Value = GUILayout.Toggle(norender_DarkFog.Value, "不渲染黑雾".getTranslate());
                norender_shipdrone_bool.Value = GUILayout.Toggle(norender_shipdrone_bool.Value, "不渲染运输船和飞机".getTranslate());
                norender_entity_bool.Value = GUILayout.Toggle(norender_entity_bool.Value, "不渲染实体".getTranslate());
                if (simulatorrender != GUILayout.Toggle(simulatorrender, "不渲染全部".getTranslate()))
                {
                    simulatorrender = !simulatorrender;
                    simulatorchanging = true;
                }
            }
            GUILayout.EndVertical();

            GUILayout.Space(30);

            GUILayout.BeginVertical();
            {
                GUILayout.Label("UI屏蔽");
                norender_powerdisk_bool.Value = GUILayout.Toggle(norender_powerdisk_bool.Value, "不渲染电网覆盖".getTranslate());
                CloseUIRandomTip.Value = GUILayout.Toggle(CloseUIRandomTip.Value, "关闭建筑栏提示".getTranslate());
                CloseUITutorialTip.Value = GUILayout.Toggle(CloseUITutorialTip.Value, "关闭教学提示".getTranslate());
                CloseUIAdvisor.Value = GUILayout.Toggle(CloseUIAdvisor.Value, "关闭顾问".getTranslate());
                CloseMilestone.Value = GUILayout.Toggle(CloseMilestone.Value, "关闭里程碑提示".getTranslate());
                if (HideDarkFogMonitor != GUILayout.Toggle(HideDarkFogMonitor, "隐藏黑雾威胁度检测".getTranslate()))
                {
                    HideDarkFogMonitor = !HideDarkFogMonitor;
                    UIRoot.instance.uiGame.dfMonitor.gameObject.SetActive(!HideDarkFogMonitor);
                }
                if (HideDarkFogAssaultTip != GUILayout.Toggle(HideDarkFogAssaultTip, "隐藏黑雾入侵提示".getTranslate()))
                {
                    HideDarkFogAssaultTip = !HideDarkFogAssaultTip;
                    UIRoot.instance.uiGame.dfAssaultTip.gameObject.SetActive(!HideDarkFogAssaultTip);
                }
            }
            GUILayout.EndVertical();

            GUILayout.Space(30);

            GUILayout.BeginVertical();
            {
                GUILayout.Label("声音屏蔽");
                closeplayerflyaudio.Value = GUILayout.Toggle(closeplayerflyaudio.Value, "关闭玩家走路飞行声音".getTranslate());
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();



            GUILayout.EndHorizontal();
        }

        private void EasyUsePanel()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            {
                GUILayout.Label("蓝图功能优化".getTranslate());
                BluePrintSelectAll.Value = GUILayout.Toggle(BluePrintSelectAll.Value, "蓝图全选".getTranslate() + "(ctrl+A）");
                BluePrintDelete.Value = GUILayout.Toggle(BluePrintDelete.Value, "蓝图删除".getTranslate() + "(ctrl+X）");
                BluePrintRevoke.Value = GUILayout.Toggle(BluePrintRevoke.Value, "蓝图撤销".getTranslate() + "(ctrl+Z)");
                BluePrintSetRecipe.Value = GUILayout.Toggle(BluePrintSetRecipe.Value, "蓝图设置配方".getTranslate() + "(ctrl+F)");
                SaveLastOpenBluePrintBrowserPathConfig.Value = GUILayout.Toggle(SaveLastOpenBluePrintBrowserPathConfig.Value, "记录上次蓝图路径".getTranslate(), buttonoptions);

            }
            GUILayout.EndVertical();

            GUILayout.Space(30);

            GUILayout.BeginVertical();
            {
                GUILayout.Label("物流站功能优化".getTranslate());
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
                ChangeQuickKey = GUILayout.Toggle(ChangeQuickKey, !ChangeQuickKey ? "改变窗口快捷键".getTranslate() : "点击确认".getTranslate(), buttonoptions);
                if (ChangeQuickKey)
                {
                    GUILayout.Label(tempShowWindow.ToString(), labelstyle);
                }
            }
            GUILayout.EndVertical();

            GUILayout.Space(30);


            GUILayout.BeginVertical();
            {
                SpeedUpEnable = GUILayout.Toggle(SpeedUpEnable, "启动时间流速修改".getTranslate());
                GUILayout.BeginHorizontal();
                GUILayout.Label("流速倍率".getTranslate() + ":");
                string tempupsfixstr = GUILayout.TextField(string.Format("{0:N2}", SpeedMultiple), GUILayout.MinWidth(heightdis * 3));
                if (tempupsfixstr != string.Format("{0:N2}", SpeedMultiple) && float.TryParse(tempupsfixstr, out float tempupsfix))
                {
                    if (tempupsfix < 0.01) tempupsfix = 0.01f;
                    if (tempupsfix > 10) tempupsfix = 10;
                    SpeedMultiple = tempupsfix;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                SpeedMultiple = GUILayout.HorizontalSlider(SpeedMultiple, 0.01f, 10, HorizontalSlideroptions);
                upsquickset.Value = GUILayout.Toggle(upsquickset.Value, "加速减速".getTranslate() + "(shift,'+''-')", buttonoptions);

                autosavetimechange.Value = GUILayout.Toggle(autosavetimechange.Value, "修改自动保存时长".getTranslate(), autosavetimechange.Value ? new GUILayoutOption[0] : buttonoptions);
                if (autosavetimechange.Value)
                {
                    GUILayout.Label("自动保存时间".getTranslate() + "/min：", buttonoptions);
                    int tempint = autosavetime.Value / 60;
                    if (int.TryParse(Regex.Replace(GUILayout.TextField(tempint + "", GUILayout.Height(heightdis), GUILayout.Width(5 * heightdis)), @"[^0-9]", ""), out tempint))
                    {
                        if (tempint < 1) tempint = 1;
                        if (tempint > 10000) tempint = 10000;
                        autosavetime.Value = tempint * 60;
                    }
                }
                KeepBeltHeight.Value = GUILayout.Toggle(KeepBeltHeight.Value, "保持传送带高度(shift)".getTranslate(), buttonoptions);
                Quickstop_bool.Value = GUILayout.Toggle(Quickstop_bool.Value, "ctrl+空格快速开关".getTranslate());
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                GameTickPatch.Enable = GUILayout.Toggle(GameTickPatch.Enable, "停止工厂".getTranslate());
                GUILayout.EndHorizontal();

                noscaleuitech_bool.Value = GUILayout.Toggle(noscaleuitech_bool.Value, "科技面板选中不缩放".getTranslate(), buttonoptions);
                if (TrashStorageWindow_bool.Value != GUILayout.Toggle(TrashStorageWindow_bool.Value, "背包垃圾桶".getTranslate(), buttonoptions))
                {
                    TrashStorageWindow_bool.Value = !TrashStorageWindow_bool.Value;
                    TrashCanButton.SetActive(TrashStorageWindow_bool.Value);
                }
                SunLightOpen.Value = GUILayout.Toggle(SunLightOpen.Value, "夜灯".getTranslate());

            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void TextTechPanel()
        {
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
                if (tp.ID > 2000 || tp.IsHiddenTech || tp.IsObsolete) break;
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
                if (tp.ID < 2000 || readyresearch.Contains(tp.ID) || tp.IsObsolete || tp.IsHiddenTech || !GameMain.history.CanEnqueueTech(tp.ID) || tp.MaxLevel > 20 || tp.MaxLevel > 100 || GameMain.history.TechUnlocked(tp.ID)) continue;
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
        }

        private void BattlePanel()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            AutoNavigateToDarkFogHive.Value = GUILayout.Toggle(AutoNavigateToDarkFogHive.Value, "黑雾巢穴自动导航");
            AutoNavigateToDarkFogHiveKeepDistance.Value = (int)GUILayout.HorizontalSlider(AutoNavigateToDarkFogHiveKeepDistance.Value, 10, 240);
            GUILayout.Label("跟随距离：" + NumberTranslate.DistanceTranslate(AutoNavigateToDarkFogHiveKeepDistance.Value * 100));
            GUILayout.EndHorizontal();
            if (GameMain.data?.spaceSector?.dfHives != null)
            {
                int num = 0;
                List<string> originstars = new List<string>();
                List<string> targetstars = new List<string>();
                List<string> showdistances = new List<string>();
                List<string> showremaindistances = new List<string>();
                List<int> enermyIds = new List<int>();
                var spacesector = GameMain.data.spaceSector;
                foreach (var dfHive in spacesector.dfHives)
                {
                    if (dfHive == null)
                        continue;
                    for (EnemyDFHiveSystem enemyDFHiveSystem = dfHive; enemyDFHiveSystem != null; enemyDFHiveSystem = enemyDFHiveSystem.nextSibling)
                    {
                        foreach (var t in enemyDFHiveSystem.tinders.buffer)
                        {
                            if (t.id <= 0 || t.enemyId <= 0)
                            {
                                continue;
                            }
                            var pos = GameMain.data.spaceSector.enemyPool[t.enemyId].pos;
                            if (pos.magnitude < 1000)
                            {
                                continue;
                            }
                            var targetStar = spacesector.GetHiveByAstroId(t.targetHiveAstroId)?.starData;
                            if (targetStar != null)
                            {
                                string targetStarName = spacesector.GetHiveByAstroId(t.targetHiveAstroId)?.starData?.displayName ?? "无".getTranslate();
                                targetstars.Add(targetStarName);
                                double remaindis = (pos - targetStar.uPosition).magnitude;
                                showremaindistances.Add(NumberTranslate.DistanceTranslate(remaindis));
                            }
                            num++;
                            originstars.Add(spacesector.GetHiveByAstroId(t.originHiveAstroId)?.starData?.displayName ?? "无".getTranslate());
                            double distance = (player.uPosition - pos).magnitude;
                            showdistances.Add(NumberTranslate.DistanceTranslate(distance));
                            enermyIds.Add(t.enemyId);
                        }
                    }
                }
                GUILayout.Space(30);
                GUILayout.Label("火种列表");
                if (num == 0)
                {
                    GUILayout.Label("无火种");
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    GUILayout.Label("出发星系");
                    for (int i = 0; i < num; i++)
                    {
                        GUILayout.Label(originstars[i]);
                    }
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    GUILayout.Label("目标星系");
                    for (int i = 0; i < num; i++)
                    {
                        GUILayout.Label(targetstars[i]);
                    }
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    GUILayout.Label("与玩家距离");
                    for (int i = 0; i < num; i++)
                    {
                        GUILayout.Label(showdistances[i]);
                    }
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    GUILayout.Label("离目标星系距离");
                    for (int i = 0; i < num; i++)
                    {
                        GUILayout.Label(showremaindistances[i]);
                    }
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    GUILayout.Label("");
                    for (int i = 0; i < num; i++)
                    {
                        if (GUILayout.Button("导航"))
                        {
                            if (autonavigation_bool.Value)
                            {
                                GameMain.mainPlayer.navigation.indicatorEnemyId = enermyIds[i];
                            }
                            else
                            {
                                UIMessageBox.Show("自动导航失败", "未启用自动导航", "确定".Translate(), 3);
                            }
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }

            }
            if (GameMain.gameScenario?.cosmicMessageManager?.messageSimulators != null)
            {
                int num = 0;
                GUILayout.Space(30);
                List<string> showdistances = new List<string>();
                List<CosmicMessageData> CosmicMessageDatas = new List<CosmicMessageData>();
                foreach (var t in GameMain.gameScenario.cosmicMessageManager.messageSimulators)
                {
                    if (t?.messageData == null)
                        continue;
                    num++;
                    double distance = (player.uPosition - t.messageData.uPosition).magnitude;
                    showdistances.Add(NumberTranslate.DistanceTranslate(distance));
                    CosmicMessageDatas.Add(t.messageData);
                }

                GUILayout.Label("通讯器列表");
                if (num == 0)
                {
                    GUILayout.Label("无通讯器");
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    GUILayout.Label("与玩家距离");
                    for (int i = 0; i < num; i++)
                    {
                        GUILayout.Label(showdistances[i]);
                    }
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    GUILayout.Label("");
                    for (int i = 0; i < num; i++)
                    {
                        if (GUILayout.Button("导航"))
                        {
                            if (autonavigation_bool.Value)
                            {
                                player.navigation.indicatorMsgId = CosmicMessageDatas[i].protoId;
                                PlayerOperation.FlyStatus = true;
                            }
                            else
                            {
                                UIMessageBox.Show("自动导航失败", "未启用自动导航", "确定".Translate(), 3);
                            }
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
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
