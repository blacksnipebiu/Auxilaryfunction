using HarmonyLib;
using UnityEngine;

namespace Auxilaryfunction.Patch;

/// <summary>
/// 逻辑帧暂停 Patch。
/// 通过启用 Harmony 前缀，短路 <c>GameMain.FixedUpdate</c> 的默认执行，实现暂停/恢复游戏主逻辑；
/// 同时保留必要的 Universe/Physics/Audio 相关调用以避免界面/场景冻结。
/// </summary>
public class GameTickPatch
{
    private static Harmony _patch;
    private static bool enable;

    public static bool Enable
    {
        get => enable;
        set
        {
            if (enable == value) return;
            enable = value;
            if (enable)
            {
                _patch = Harmony.CreateAndPatchAll(typeof(GameTickPatch));
            }
            else
            {
                _patch.UnpatchSelf();
            }
        }
    }

    /// <summary>
    /// 前缀拦截游戏固定更新：在未暂停时手动调用必要的逻辑阶段，
    /// 返回 false 以跳过原方法体，达到暂停/自定义执行的目的。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameMain), "FixedUpdate")]
    public static bool GameTick1Path(GameMain __instance)
    {
        DeepProfiler.BeginSample(DPEntry.PlanetLoading, -1, -1L);
        UniverseGen.Update();
        DeepProfiler.EndSample(-1, -2L);
        if (!__instance.running)
        {
            return false;
        }
        bool t = Traverse.Create(__instance).Field<bool>("_fullscreenPausedUnlockOneFrame").Value;
        GameMain.data.mainPlayer.ApplyGamePauseState(GameMain.isPaused || (GameMain.isFullscreenPaused && !t));
        if (!GameMain.isPaused)
        {
            Traverse.Create(GameMain.logic).Method("UniverseGameTick").GetValue();
            Traverse.Create(GameMain.logic).Method("LocalPlanetPhysics").GetValue();
            Traverse.Create(GameMain.logic).Method("LocalPlanetAudio").GetValue();
            Traverse.Create(GameMain.logic).Method("SpaceSectorAudio").GetValue();
            Traverse.Create(GameMain.logic).Method("PlayerGameTick").GetValue();
            Traverse.Create(GameMain.logic).Method("FactoryBeforeGameTick").GetValue();
            Traverse.Create(GameMain.logic).Method("TrashSystemGameTick").GetValue();
            Traverse.Create(GameMain.logic).Method("FactoryConstructionSystemGameTick").GetValue();
            Traverse.Create(GameMain.logic).Method("WarningSystemGameTick").GetValue();
            Traverse.Create(GameMain.logic).Method("StatisticsPrepare").GetValue();
            Traverse.Create(GameMain.logic).Method("StatisticsGameTick").GetValue();
            Traverse.Create(GameMain.logic).Method("StatisticsPostGameTick").GetValue();
            Traverse.Create(GameMain.logic).Method("ScenarioGameTick").GetValue();
            Traverse.Create(GameMain.logic).Method("CollectPreferences").GetValue();
            Traverse.Create(GameMain.logic).Method("SpaceSectorPhysics").GetValue();
            Traverse.Create(GameMain.logic).Method("SpaceSectorAudioPost").GetValue();
            //GameMain.logic.threadController.threadManager.ProcessFrame(GameMain.gameTick);
        }
        //待实现
        return false;
    }

    /// <summary>
    /// 后缀修正：需要飞行计数器保持更新，避免暂停态下飞行无响应。
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerController), "SetDesireUPosition")]
    public static void SetDesireUPosition(VectorLF3 uPos)
    {
        GameMain.mainPlayer.controller.actionSail.sailCounter = 5;
    }
}
