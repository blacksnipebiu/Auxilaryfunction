using System.Collections.Generic;

namespace Auxilaryfunction
{
    public static class AuxilaryTranslate
    {
        public static HashSet<string> notranslateStr = new HashSet<string>();
        private static Dictionary<string, string> TranslateDict = new Dictionary<string, string>();
        public static string getTranslate(this string s)
        {
            if (Localization.CurrentLanguage.glyph != 0)
            {
                return s;
            }
            if (TranslateDict.ContainsKey(s))
            {
                return TranslateDict[s];
            }
            notranslateStr.Add(s);

            return s;
        }
        public static void regallTranslate()
        {
            TranslateDict.Clear();
            TranslateDict.Add("辅助面板错误提示", "Auxilaryfunction Error");
            #region 戴森球面板
            TranslateDict.Add("默认面板", "Default Panel");
            TranslateDict.Add("戴森球面板", "Dyson Sphere Panel");
            TranslateDict.Add("选择一个蓝图后，点击右侧的层级可以自动导入", "Select BluePrintFile,you can click enabled layers to paste");
            TranslateDict.Add("单层壳", "Single Layer");
            TranslateDict.Add("多层壳", "Layers");
            TranslateDict.Add("戴森云", "Dyson Swarm");
            TranslateDict.Add("戴森球(包括壳、云)", "Dyson Sphere(include layers and swarm)");
            TranslateDict.Add("打开戴森球蓝图文件夹", "Open DysonBlueprint Folder");
            TranslateDict.Add("刷新文件", "Refresh files list");
            TranslateDict.Add("复制选中文件代码", "Copy selected file cotent");
            TranslateDict.Add("清除剪贴板", "clear clipboard");
            TranslateDict.Add("应用蓝图", "Apply Selected BluePrint File");
            TranslateDict.Add("自动生成最大半径戴森壳", "Auto Add bigest layers");
            TranslateDict.Add("删除全部空壳", "Delete all empty layers");
            TranslateDict.Add("可用戴森壳层级:", "Enabled layers:");
            TranslateDict.Add("不可用戴森壳层级:", "Disabled layers:");
            TranslateDict.Add("勾选即可点击删除", "Tick to click delete layer");
            TranslateDict.Add("路径为空，请重新选择", "The path is empty, please select again");
            TranslateDict.Add("文件读取失败", "file read failed");
            TranslateDict.Add("文件不存在", "file does not exist");
            TranslateDict.Add("自动清除空戴森球", "Auto Clear Empty Dyson");
            TranslateDict.Add("每次打开戴森球面板(Y)都会自动进行清理", "Auto Clear Empty Dyson when you open DysonPanel(Y)");
            TranslateDict.Add("记录上次蓝图路径", "Record the last blueprint path");

            TranslateDict.Add("单层壳蓝图请选择层级", " Single layer blueprint please select the layer");

            #endregion
            TranslateDict.Add("自动配置新运输站", "AutoStation");
            TranslateDict.Add("配置参数", "Configurations");
            TranslateDict.Add("填充飞机数量", "Fill the number of drones");
            TranslateDict.Add("填充飞船数量", "Fill the number of vessel");
            TranslateDict.Add("最大充电功率", "Max. Charging Power");
            TranslateDict.Add("运输机最远路程", "Transport range of Drones");
            TranslateDict.Add("运输船最远路程", "Transport range of Vessels");
            TranslateDict.Add("曲速启用路程", "Distance to enable Warp");
            TranslateDict.Add("运输机起送量", "Min. Load of Drones");
            TranslateDict.Add("运输船起送量", "Min. Load of Vessels");
            TranslateDict.Add("翘曲填充数量", "Fill the number of warper");
            TranslateDict.Add("大型采矿机采矿速率", "Advanced mining machine gathering Speed");
            TranslateDict.Add("批量配置当前星球大型采矿机采矿速率", "set all advanced mining machine gathering Speed");
            TranslateDict.Add("铺满轨道采集器", "Full orbital Collector this planet(gas)");
            TranslateDict.Add("填充当前星球配送机飞机飞船、翘曲器", "Fill all stations with courier,vessels,drones,warpers");
            TranslateDict.Add("批量配置当前星球物流站", "set all stations");
            TranslateDict.Add("不渲染戴森壳", "Don't render Dyson shell");
            TranslateDict.Add("不渲染戴森云", "Don't render Dyson swarm");
            TranslateDict.Add("不渲染研究站", "Don't render lab");
            TranslateDict.Add("不渲染传送带货物", "Don't render belt item");
            TranslateDict.Add("不渲染运输船和飞机", "Don't render vessels,drones");
            TranslateDict.Add("不渲染实体", "Don't render entity");
            TranslateDict.Add("不渲染全部", "Don't render at all");
            TranslateDict.Add("不渲染电网覆盖", "Don't render power grid");
            TranslateDict.Add("关闭玩家走路飞行声音", "Turn off the player's walking and flying sound");
            TranslateDict.Add("自动添加翘曲器", "Auto add Warp");
            TranslateDict.Add("自动添加机甲燃料", "Auto add fuel");
            TranslateDict.Add("停止工厂", "stop factory");
            TranslateDict.Add("ctrl+空格快速开关", "quick start and stop(ctrl+space)");

            TranslateDict.Add("自动飞向未完成建筑", "Auto fly to unfinished buildings");
            TranslateDict.Add("自动飞向地面黑雾基地", "Autp fly to dark fog on ground");
            TranslateDict.Add("自动添加科技队列", "Auto add tech queue");
            TranslateDict.Add("未选择", "no selected");
            TranslateDict.Add("自动配置太阳帆弹射器", "Auto set Ejector");
            TranslateDict.Add("自动导航", "Auto navigation");
            TranslateDict.Add("自动导航使用曲速", "Auto use warper");
            TranslateDict.Add("自动使用翘曲器距离", "Auto use warper distance");
            TranslateDict.Add("自动添加科技等级上限", "Auto add technology level cap");
            TranslateDict.Add("光年", "light-years");
            TranslateDict.Add("精准拿取", "Accurately take");
            TranslateDict.Add("建筑计数", "Building count");
            TranslateDict.Add("一键闭嘴", "One key to shut up");
            TranslateDict.Add("科技面板选中不缩放", "Technologies panel no scale");
            TranslateDict.Add("蓝图全选", "Blueprint SelectAll");
            TranslateDict.Add("蓝图删除", "Blueprint Delete");
            TranslateDict.Add("蓝图撤销", "Blueprint Revocation");
            TranslateDict.Add("蓝图设置配方", "Blueprint setting recipe");
            TranslateDict.Add("蓝图复制直接粘贴", "Blueprint copy and paste directly");
            TranslateDict.Add("物流站信息显示", "Show station info");
            TranslateDict.Add("详细模式", "Detailed mode");
            TranslateDict.Add("简易模式", "Simple Mode");
            TranslateDict.Add("物流站物品设置复制粘贴", "Station grids copy(<) and paste(>)");
            TranslateDict.Add("30s间隔自动吸收垃圾", "Auto. recycle Litter in 30s");
            TranslateDict.Add("只回收建筑", "Recycle only buildings");
            TranslateDict.Add("30s间隔自动清除垃圾", "Auto. clear Litter in 30s");
            TranslateDict.Add("改变窗口快捷键", "Change Window shortcut");
            TranslateDict.Add("点击确认", "Click to confirm");
            TranslateDict.Add("生产加速", "Production Speedup");
            TranslateDict.Add("额外产出", "Extra Products");

            TranslateDict.Add("自动画最密戴森球", "Auto. draw DysonSphere");
            TranslateDict.Add("缓慢最密画法", "slow draw method");
            TranslateDict.Add("根", "root");
            TranslateDict.Add("带壳", "With Shell");
            TranslateDict.Add("带", "yes");
            TranslateDict.Add("不带", "no");
            TranslateDict.Add("只画单层", "only single layer");
            TranslateDict.Add("启动", "start draw");
            TranslateDict.Add("正在画", "Drawing");
            TranslateDict.Add("停止", "Stop");
            TranslateDict.Add("启动时间流速修改", "Start time flow rate modification");
            TranslateDict.Add("流速倍率", "Flow rate multiplier");
            TranslateDict.Add("加速减速", "Speed up and slow down");
            TranslateDict.Add("自动填充人造恒星", "Auto add fuel to Artificial Star");
            TranslateDict.Add("人造恒星燃料数量", "Artificial Star fuel quantity");
            TranslateDict.Add("填充当前星球人造恒星", "Fill the current planet with artificial stars");
            TranslateDict.Add("保持传送带高度(shift)", "Keep belt height(shift)");
            TranslateDict.Add("修改自动保存时长", "Modify AutosaveTime");
            TranslateDict.Add("自动保存时间", "Autosavetime");

            TranslateDict.Add("文字科技树", "Text technology tree");
            TranslateDict.Add("限制材料", "Restricted material");
            TranslateDict.Add("自动乱点", "Auto random select");
            TranslateDict.Add("暂停导航", "stop navigation");
            TranslateDict.Add("继续导航", "continue navigation");
            TranslateDict.Add("取消方向指示", "cacel Indicatior");
            TranslateDict.Add("辅助面板", "Auxilaryfunction");
            TranslateDict.Add("无", "none");
            TranslateDict.Add("粘贴物流站配方", "paste station grids set");
            TranslateDict.Add("上限", "Upper limit");
            TranslateDict.Add("本地", "Local");
            TranslateDict.Add("星际", "Remote");
            TranslateDict.Add("第", "No.");
            TranslateDict.Add("格", "Grid");
            TranslateDict.Add("准备研究", "Research Queue");
            TranslateDict.Add("科技", "Technologies");
            TranslateDict.Add("关闭白色面板", "ClosePanel");
            TranslateDict.Add("升级", "Upgrades");
            TranslateDict.Add("光子生成", "Photon Generation");
            TranslateDict.Add("直接发电", "Power Generation");

            TranslateDict.Add("垃圾桶", "trash can");
            TranslateDict.Add("暂无选中设备", "No equipment selected");
            TranslateDict.Add("开关显示垃圾桶，垃圾桶关闭后自动清理内部物品", "Switch to display the trash can and automatically clean the contents when the trash can is closed");
            TranslateDict.Add("火种列表", "Tinder list");
            TranslateDict.Add("无火种", "No tinder");
            TranslateDict.Add("通讯器列表", "List of communicators");
            TranslateDict.Add("与玩家距离", "Distance from the player");
            TranslateDict.Add("导航", "navigation");
            TranslateDict.Add("黑雾巢穴自动导航", "Auto move to DarkFog");
            TranslateDict.Add("跟随距离", "Follow Distance");
            TranslateDict.Add("模型屏蔽", "Model masking");
            TranslateDict.Add("UI屏蔽", "UI masking");
            TranslateDict.Add("声音屏蔽", "Sound blocking");
            TranslateDict.Add("机甲自动化", "Mecha automation");
            TranslateDict.Add("自动菜单", "Auto Menu");
            TranslateDict.Add("渲染屏蔽", "Render Mask");
            TranslateDict.Add("便捷小功能", "Convenience Features");
            TranslateDict.Add("战斗", "Battles");
            TranslateDict.Add("蓝图设置", "Blueprint settings");
            TranslateDict.Add("填充当前星球飞机飞船、翘曲器", "Fill current planet aircraft ships, warpers");
            TranslateDict.Add("自动填充配送运输机", "Auto-fill delivery transports");
            TranslateDict.Add("填充配送运输机数量", "Fill the number of delivery transports");
            TranslateDict.Add("填充当前星球配送机", "Fill current planet's dispensers");
            TranslateDict.Add("自动加速", "Auto Acceleration");
            TranslateDict.Add("自动曲速最低距离", "Auto Warp Minimum Distance");
            TranslateDict.Add("自动曲速最低能量", "Auto-warp minimum energy");
            TranslateDict.Add("不渲染黑雾", "Do not render black fog");
            TranslateDict.Add("关闭建筑栏提示", "Turn off building bar prompts");
            TranslateDict.Add("关闭教学提示", "Turn off teaching tips");
            TranslateDict.Add("关闭顾问", "Turn off advisors");
            TranslateDict.Add("关闭里程碑提示", "Turn off milestone alerts");
            TranslateDict.Add("隐藏黑雾威胁度检测", "Hide black fog threat level detection");
            TranslateDict.Add("隐藏黑雾入侵提示", "Hide Black Fog Intrusion Alerts");
            TranslateDict.Add("蓝图功能优化", "Blueprint optimization");
            TranslateDict.Add("物流站功能优化", "Logistics Station Optimization");
            TranslateDict.Add("背包垃圾桶", "Backpack trash can");
            TranslateDict.Add("夜灯", "Night Light");
            TranslateDict.Add("当前选中", "Currently selected");
            TranslateDict.Add("进入蓝图模式可使用", "Available in Blueprint Mode");
            TranslateDict.Add("全选当前星球建筑", "Select all buildings on current planet");
            TranslateDict.Add("删除选中建筑", "Delete selected buildings");
            TranslateDict.Add("可修改建筑", "Modify buildings");
            TranslateDict.Add("科研模式", "Research mode");
            TranslateDict.Add("辅助面板提示", "Auxiliary Panel Tips");

        }
    }
}
