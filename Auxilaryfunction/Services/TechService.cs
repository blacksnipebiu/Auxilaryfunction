using System.Collections.Generic;

namespace Auxilaryfunction.Services;

internal class TechService
{
    /// <summary>
    /// 区分“有限升级科技”和“多级/无限升级科技”的等级上限分界（MaxLevel ≥ 该值算后者）。
    /// 自动顺序添加用它排序，让高等级科技垫底；不再用它过滤显示。
    /// </summary>
    public const int FiniteUpgradeMaxLevel = 20;

    /// <summary>
    /// 文字科技树的范围上限：MaxLevel 超过该值的科技（例如上万级的无穷科技）完全不呈现、也不自动入列。
    /// </summary>
    public const int TechTreeMaxLevelLimit = 50;

    /// <summary>
    /// 该科技是否超出文字科技树的范围（MaxLevel &gt; TechTreeMaxLevelLimit）。
    /// 用于从“准备研究”里剔除旧版本留下的这类科技。
    /// </summary>
    public static bool IsBeyondTechTreeLimit(int techId)
    {
        TechProto tp = LDB.techs.Select(techId);
        return tp != null && tp.MaxLevel > TechTreeMaxLevelLimit;
    }

    /// <summary>
    /// 收集“当前就能入队研究”的科技ID，顺序为 LDB.techs.dataArray（科技表）顺序。
    /// upgrade=false 取普通科技（ID≤2000，“科技”页签），upgrade=true 取升级科技（ID≥2000，“升级”页签）。
    /// 面板列表和“自动顺序添加”都用这里，保证两边完全一致：都是当前能入队、且 MaxLevel ≤ TechTreeMaxLevelLimit。
    /// </summary>
    /// <param name="limitMaterial">勾选“限制材料”时，只保留所需材料都已解锁的科技</param>
    public static List<int> CollectResearchableTechIds(bool upgrade, bool limitMaterial)
    {
        var list = new List<int>();
        foreach (TechProto tp in LDB.techs.dataArray)
        {
            // 隐藏/废弃/另一类（普通<->升级）的科技直接跳过
            if (tp.IsHiddenTech || tp.IsObsolete) continue;
            if (upgrade ? tp.ID < 2000 : tp.ID > 2000) continue;
            // 前置科技没全解锁的、已经研究完的都不算“可以研究”
            if (!GameMain.history.CanEnqueueTech(tp.ID) || GameMain.history.TechUnlocked(tp.ID)) continue;
            // 等级上限过高的科技不进文字科技树
            if (tp.MaxLevel > TechTreeMaxLevelLimit) continue;
            if (limitMaterial && !MaterialsUnlocked(tp)) continue;
            list.Add(tp.ID);
        }
        return list;
    }

    /// <summary>
    /// “限制材料”用：科技所需材料是否都已解锁。
    /// </summary>
    public static bool MaterialsUnlocked(TechProto tp)
    {
        foreach (int itemId in tp.Items)
        {
            if (!GameMain.history.ItemUnlocked(itemId)) return false;
        }
        return true;
    }

    /// <summary>矩阵物品ID范围（6001 电磁 ~ 6006 宇宙）</summary>
    public const int MatrixItemMin = 6001;
    public const int MatrixItemMax = 6006;

    /// <summary>
    /// 科技要求的矩阵档次：取所需矩阵里最高的一档（只要 6001 就是 6001，要 6001~6004 就是 6004）。
    /// 没有矩阵要求的返回 0。档次低的科技先入列，因为它们吃的是低阶矩阵、产线压力小。
    /// </summary>
    public static int MatrixTier(TechProto tp)
    {
        if (tp == null || tp.Items == null) return 0;
        int tier = 0;
        foreach (int itemId in tp.Items)
        {
            if (itemId < MatrixItemMin || itemId > MatrixItemMax) continue;
            if (itemId > tier) tier = itemId;
        }
        return tier;
    }

    /// <summary>
    /// 科技需要几种矩阵（去重计数，只算 6001~6006）。
    /// 与 MatrixTier 一起衡量“矩阵要求的轻重”：档次相同、种类少的更轻
    /// —— 只要 6004 的科技比要 6001~6004 的科技轻，应该先研究。
    /// </summary>
    public static int MatrixTypeCount(TechProto tp)
    {
        if (tp == null || tp.Items == null) return 0;
        int count = 0;
        for (int i = 0; i < tp.Items.Length; i++)
        {
            int itemId = tp.Items[i];
            if (itemId < MatrixItemMin || itemId > MatrixItemMax) continue;
            bool seen = false;
            for (int j = 0; j < i; j++)
            {
                if (tp.Items[j] == itemId) { seen = true; break; }
            }
            if (!seen) count++;
        }
        return count;
    }

    /// <summary>
    /// 科技当前这一级需要的矩阵总个数（同档次下用它比较“消耗量”）。
    /// 公式取自游戏自身 TechProto.RescurCalcUnlockNeedItems / 原版 Multifunction 的统计口径：
    /// 每种基质数量 = ItemPoints[i] * GetHashNeeded(level) / 3600。
    /// </summary>
    public static long MatrixCost(TechProto tp, int level)
    {
        if (tp == null || tp.Items == null || tp.ItemPoints == null) return 0;
        long hash = tp.GetHashNeeded(level);
        if (hash <= 0) return 0;
        long total = 0;
        int count = System.Math.Min(tp.Items.Length, tp.ItemPoints.Length);
        for (int i = 0; i < count; i++)
        {
            // 只统计矩阵，别的材料不计入“矩阵消耗量”
            if (tp.Items[i] < MatrixItemMin || tp.Items[i] > MatrixItemMax) continue;
            total += (long)tp.ItemPoints[i] * hash / 3600;
        }
        return total;
    }

    public static void BuyoutTech(TechProto tp)
    {
        if (!GameMain.data.history.HasPreTechUnlocked(tp.ID))
        {
            UIRealtimeTip.Popup("存在未解锁的前置科技".Translate(), true, 0);
            return;
        }
        if (GameMain.sandboxToolsEnabled)
        {
            if (!GameMain.data.history.HasPreTechUnlocked(tp.ID))
            {
                UIRealtimeTip.Popup("存在未解锁的前置科技".Translate(), true, 0);
                return;
            }
            GameMain.history.UnlockTechUnlimited(tp.ID, true);
            return;
        }
        if (!GameMain.data.history.CheckPropertyAdequateForBuyout(tp.ID))
        {
            UIRealtimeTip.Popup("元数据不足".Translate(), true, 0);
            return;
        }
        if (!GameMain.data.history.propertyData.hasUsedProperty)
        {
            UIMessageBox.Show("初次使用元数据标题".Translate(), "初次使用元数据描述".Translate(), "取消".Translate(), "确定".Translate(), 2, null, new UIMessageBox.Response(() => DoBuyoutTech(tp)));
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

}
