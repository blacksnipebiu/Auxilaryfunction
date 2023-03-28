using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Auxilaryfunction.Services.GUIService;

namespace Auxilaryfunction.Services
{
    internal class TechService
    {
        public static void BuyoutTech(TechProto tp)
        {
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
                UIMessageBox.Show("初次使用元数据标题".Translate(), "初次使用元数据描述".Translate(), "取消".Translate(), "确定".Translate(), 2, null, new UIMessageBox.Response(()=>DoBuyoutTech(tp)));
                return;
            }
            else
            {
                DoBuyoutTech(tp);
            }
        }

        public static void DoBuyoutTech(TechProto tp)
        {
            if (tp == null || tp.ID == 1)
            {
                return;
            }
            GameMain.history.BuyoutTech(tp.ID);
        }

        #region
        private void LoadTechBluePrintData()
        {
            //TechPanelBluePrintNum = 0;

        }
        #endregion
    }
}
