namespace Auxilaryfunction;

/// <summary>
/// 游戏 UI 层级路径与子物体名。
/// 这些字符串与游戏版本强耦合，集中在此，游戏更新时只需改这一处。
/// </summary>
public static class GameUi
{
    // GameObject.Find 使用的绝对路径
    public const string SceneUIs = "UI Root/Overlay Canvas/In Game/Scene UIs";
    public const string VeinMarks = "UI Root/Overlay Canvas/In Game/Scene UIs/Vein Marks";
    public const string VeinTipPrefab = "UI Root/Overlay Canvas/In Game/Scene UIs/Vein Marks/vein-tips/vein-tip-prefab";
    public const string KeyTipPrefab = "UI Root/Overlay Canvas/In Game/Windows/Key Tips/tip-prefab";
    public const string EntityBriefIcon = "UI Root/Overlay Canvas/In Game/Top Tips/Entity Briefs/brief-info-top/brief-info/content/icons/icon";
    public const string GameMenuButton1Bg = "UI Root/Overlay Canvas/In Game/Game Menu/button-1-bg";
    public const string PlayerInventory = "UI Root/Overlay Canvas/In Game/Windows/Player Inventory";
    public const string StorageWindow = "UI Root/Overlay Canvas/In Game/Windows/Storage Window";

    // 子物体 / 子节点名（含按索引拼接的公共前缀）
    public const string InfoText = "info-text";
    public const string Icon = "icon";
    public const string CountText = "countText";
    public const string CountText2 = "countText2";
    public const string IconLocal = "iconLocal";
    public const string IconRemote = "iconremote";
    public const string IconText = "icontext";
    public const string ShadowCountText = "count-text";
    public const string Bg = "bg";
    public const string Inc = "inc";
    public const string Button1Icon = "button-1/icon";
    public const string DragTrigger = "panel-bg/drag-trigger";
    public const string FilterGroup = "filter-group";
    public const string BansBar = "bans-bar";
    public const string SortBtn = "panel-bg/btn-box/sort-btn";
    public const string CloseWndBtn = "panel-bg/btn-box/close-wnd-btn";
}

/// <summary>
/// 游戏物品 ID。集中在此，避免散落的魔法数字。
/// </summary>
public static class GameIds
{
    // 物流单位
    public const int Drone = 5001;   // 运输机（飞机）
    public const int Vessel = 5002;  // 运输船（飞船）
    public const int Courier = 5003; // 配送机
    public const int Warper = 1210;  // 翘曲器

    // 人造恒星燃料
    public const int StarFuelA = 1803;
    public const int StarFuelB = 1804;

    // 建筑 / 图标
    public const int LogisticsStation = 2103;            // 行星物流站
    public const int InterstellarLogisticsStation = 2104;// 星际物流站
    public const int OrbitalCollector = 2105;            // 轨道采集器
    public const int LogisticsDispenser = 2107;          // 物流配送器
    public const int RayReceiver = 2208;                 // 射线接收站
    public const int EnergyExchanger = 2209;             // 能量枢纽
    public const int ArtificialStar = 2210;              // 人造恒星
}
