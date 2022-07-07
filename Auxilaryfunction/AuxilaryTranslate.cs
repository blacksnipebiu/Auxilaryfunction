using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auxilaryfunction
{
    public static class AuxilaryTranslate
    {
        private static Dictionary<string, string> TranslateDict = new Dictionary<string, string>();
        public static string getTranslate(this string s) => Localization.language != Language.zhCN && TranslateDict.ContainsKey(s) ? TranslateDict[s] : s;
        public static void regallTranslate()
        {
            TranslateDict.Clear();
            TranslateDict.Add("自动配置新运输站", "AutoStation");
            TranslateDict.Add("配置参数", "Configurations");
            TranslateDict.Add("填充飞机数量", "Fill the number of drones");
            TranslateDict.Add("填充飞船数量", "Fill the number of vessel");
            TranslateDict.Add("最大充电功率", "Max. Charging Power");
            TranslateDict.Add("运输机最远路程","Transport range of Drones");
            TranslateDict.Add("运输船最远路程","Transport range of Vessels");
            TranslateDict.Add("曲速启用路程","Distance to enable Warp");
            TranslateDict.Add("运输机起送量","Min. Load of Drones");
            TranslateDict.Add("运输船起送量","Min. Load of Vessels");
            TranslateDict.Add("翘曲填充数量","Fill the number of warper");
            TranslateDict.Add("大型采矿机采矿速率", "Advanced mining machine gathering Speed");
            TranslateDict.Add("批量配置当前星球大型采矿机采矿速率", "set all advanced mining machine gathering Speed");
            TranslateDict.Add("铺满轨道采集器","Full orbital Collector this planet(gas)");
            TranslateDict.Add("填充当前星球飞机飞船、翘曲器","Fill all stations with vessels,drones,warpers");
            TranslateDict.Add("批量配置当前星球物流站","set all stations");
            TranslateDict.Add("不渲染戴森壳","Don't render Dyson shell");
            TranslateDict.Add("不渲染戴森云", "Don't render Dyson swarm");
            TranslateDict.Add("不渲染研究站", "Don't render lab");
            TranslateDict.Add("不渲染运输船和飞机", "Don't render vessels,drones");
            TranslateDict.Add("不渲染实体", "Don't render entity"); 
            TranslateDict.Add("不渲染全部", "Don't render at all"); 
            TranslateDict.Add("不渲染电网覆盖", "Don't render power grid");
            TranslateDict.Add("关闭玩家走路飞行声音", "Turn off the player's walking and flying sound");
            TranslateDict.Add("自动向右飞","auto fly to right");
            TranslateDict.Add("自动向上飞","auto fly to up");
            TranslateDict.Add("停止工厂","stop factory");
            TranslateDict.Add("停止戴森球","stop DysonSphere");
            TranslateDict.Add("ctrl+空格快速开关","quick start and stop(ctrl+space)");

            TranslateDict.Add("自动飞向未完成建筑", "auto fly to unfinished buildings");
            TranslateDict.Add("自动添加科技队列","auto add tech queue");
            TranslateDict.Add("未选择","no selected");
            TranslateDict.Add("自动配置太阳帆弹射器","auto set Ejector");
            TranslateDict.Add("自动导航","auto navigation");
            TranslateDict.Add("自动导航使用曲速","auto use warper");
            TranslateDict.Add("自动使用翘曲器距离","auto use warper distance");
            TranslateDict.Add("光年","light-years");
            TranslateDict.Add("精准拿取","Accurately take");
            TranslateDict.Add("建筑计数","Building count");
            TranslateDict.Add("一键闭嘴","One key to shut up");
            TranslateDict.Add("科技面板选中不缩放","Technologies panel no scale");
            TranslateDict.Add("蓝图删除","Blueprint delete");
            TranslateDict.Add("蓝图撤销", "Blueprint Revocation");
            TranslateDict.Add("蓝图设置配方", "Blueprint setting recipe");
            TranslateDict.Add("蓝图复制直接粘贴", "Blueprint copy and paste directly"); 
            TranslateDict.Add("物流站信息显示", "Show station info");
            TranslateDict.Add("物流站物品设置复制粘贴","Station grids copy(<) and paste(>)");
            TranslateDict.Add("30s间隔自动吸收垃圾","Auto. recycle Litter in 30s");
            TranslateDict.Add("只回收建筑","Recycle only buildings");
            TranslateDict.Add("30s间隔自动清除垃圾","Auto. clear Litter in 30s");
            TranslateDict.Add("改变窗口快捷键","Change Window shortcut");
            TranslateDict.Add("点击确认","Click to confirm");
            TranslateDict.Add("生产加速", "Extra Products");
            TranslateDict.Add("额外产出", "Production Speedup");

            TranslateDict.Add("自动画最密戴森球","Auto. draw DysonSphere");
            TranslateDict.Add("缓慢最密画法","slow draw method");
            TranslateDict.Add("根","root");
            TranslateDict.Add("带壳","With Shell");
            TranslateDict.Add("带","yes");
            TranslateDict.Add("不带","no");
            TranslateDict.Add("只画单层", "only single layer");
            TranslateDict.Add("启动","start draw");
            TranslateDict.Add("正在画","Drawing");
            TranslateDict.Add("停止","Stop"); 
            TranslateDict.Add("启动时间流速修改", "Start time flow rate modification");
            TranslateDict.Add("流速倍率", "Flow rate multiplier");
            TranslateDict.Add("加速减速", "Speed up and slow down");
            TranslateDict.Add("自动配置建筑", "Automatic configuration of buildings");
            TranslateDict.Add("人造恒星燃料数量", "Artificial Star fuel quantity");
            TranslateDict.Add("填充当前星球人造恒星", "Fill the current planet with artificial stars");
            TranslateDict.Add("自动保存","Autosave");
            TranslateDict.Add("自动保存时间","Autosavetime");

            TranslateDict.Add("文字科技树", "Text technology tree");
            TranslateDict.Add("限制材料", "Restricted material");
            TranslateDict.Add("自动乱点", "auto random select");
            TranslateDict.Add("停止导航", "stop navigation");
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
        }
    }
}
