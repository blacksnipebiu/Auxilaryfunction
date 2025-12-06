using HarmonyLib;
using UnityEngine;
using static Auxilaryfunction.Auxilaryfunction;
using static Auxilaryfunction.AuxConfig;

namespace Auxilaryfunction;

/// <summary>
/// 渲染屏蔽 Patch。
/// </summary>
public class SkipRenderPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FactoryModel), "Update")]
    public static bool FactoryModelUpdatePatch() => !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FactoryModel), "LateUpdate")]
    public static bool FactoryModelLateUpdatePatch() => !norender_entity_bool.Value && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LogisticDroneRenderer), "Draw")]
    public static bool DroneDrawPatch() => !norender_shipdrone_bool.Value && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LogisticShipUIRenderer), "Draw")]
    public static bool LogisticShipUIRendererDrawPatch() => !norender_shipdrone_bool.Value && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LogisticCourierRenderer), "Draw")]
    public static bool CourierDrawPatch() => !norender_shipdrone_bool.Value && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LogisticShipRenderer), "Draw")]
    public static bool ShipDrawPatch() => !norender_shipdrone_bool.Value && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LabRenderer), "Render")]
    public static bool LabRendererPatch(LabRenderer __instance)
    {
        if (__instance.modelId == 70 || __instance.modelId == 455)
            return !norender_lab_bool.Value && !simulatorrender;
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SectorModel), "LateUpdate")]
    public static bool EnemyDFGroundRendererRenderPatch() => !norender_DarkFog.Value && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DysonSphere), "DrawModel")]
    public static bool DysonDrawModelPatch() => !norender_dysonshell_bool.Value && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DysonSphere), "DrawPost")]
    public static bool DysonDrawPostPatch() => !norender_dysonswarm_bool.Value && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIPowerGizmo), "DrawArea")]
    public static bool UIPowerGizmoDrawAreaPatch() => !norender_powerdisk_bool.Value && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIDarkFogMonitor), "Determine")]
    public static bool UIDarkFogMonitorDetermine() => !HideDarkFogMonitor && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIDarkFogAssaultTip), "Determine")]
    public static bool UIDarkFogAssaultTipDetermine() => !HideDarkFogAssaultTip && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CargoContainer), "Draw")]
    public static bool PathRenderingBatchDrawPatch() => !norender_beltitem.Value && !simulatorrender;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIPowerGizmo), "DrawCover")]
    public static bool UIPowerGizmoDrawCoverPatch() => !norender_powerdisk_bool.Value && !simulatorrender;

    /// <summary>
    /// 模拟器对象启停：当 <c>simulatorchanging</c> 为真时，批量切换背景星、行星与恒星模拟器的激活状态，
    /// 切换完成后复位该标志位。
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UniverseSimulator), "GameTick")]
    public static void UniverseSimulatorGameTickPatch(UniverseSimulator __instance)
    {
        if (simulatorchanging)
        {
            int num = 0;
            __instance.backgroundStars.gameObject.SetActive(!simulatorrender);
            while (__instance.planetSimulators != null && num < __instance.planetSimulators.Length)
            {
                if (__instance.planetSimulators[num] != null)
                {
                    __instance.planetSimulators[num].gameObject.SetActive(!simulatorrender);
                }
                num++;
            }
            num = 0;
            while (__instance.starSimulators != null && num < __instance.starSimulators.Length)
            {
                if (__instance.starSimulators[num] != null)
                {
                    if (__instance.starSimulators[num].starData.type == EStarType.NeutronStar && Configs.builtin.neutronStarPrefab.streamRenderer != null)
                    {
                        num++;
                        continue;
                    }
                    __instance.starSimulators[num].gameObject.SetActive(!simulatorrender);
                }
                num++;
            }
            simulatorchanging = false;
        }
    }
}
