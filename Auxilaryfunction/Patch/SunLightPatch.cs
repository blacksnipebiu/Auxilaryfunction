using HarmonyLib;
using UnityEngine;
using static Auxilaryfunction.AuxConfig;
using static Auxilaryfunction.Auxilaryfunction;

namespace Auxilaryfunction.Patch;

public static class SunLightPatch
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
                _patch = Harmony.CreateAndPatchAll(typeof(SunLightPatch));
            }
            else
            {
                _patch.UnpatchSelf();
            }
        }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(PostEffectController), "Update")]
    public static void PostEffectControllerUpdatePatch(PostEffectController __instance)
    {
        if (!SunLightOpen.Value || __instance.sunShaft == null || GameMain.localStar == null || GameMain.localPlanet == null || SunLight == null || FactoryModel.whiteMode0)
        {
            return;
        }
        float magnitude = GameCamera.main.transform.localPosition.magnitude;
        if (GameMain.universeSimulator != null && GameMain.localStar.type != EStarType.BlackHole)
        {
            StarSimulator starSimulator = GameMain.universeSimulator.LocalStarSimulator();
            if (starSimulator != null)
            {
                __instance.sunShaft.sunTransform = SunLight.transform;
                __instance.sunShaft.sunColor = GameMain.universeSimulator.sunshaftColor.Evaluate(starSimulator.sunColorParam);
                SunLight.shadowBias = Mathf.Clamp01((magnitude - 300f) / 700f) * 0.5f + 0.045f;
            }
        }
        __instance.sunShaft.enabled = (__instance.sunShaft.sunTransform != null);
        __instance.sunShaft.sunShaftIntensity = 0.25f + Mathf.Clamp01((300f - GameMain.mainPlayer.position.magnitude + GameMain.localPlanet.realRadius) / 200f) * 1.25f;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StarSimulator), "LateUpdate")]
    public static bool StarSimulatorLateUpdate(ref StarSimulator __instance, Material ___bodyMaterial, Material ___haloMaterial)
    {
        if (!SunLightOpen.Value || FactoryModel.whiteMode0 || GameMain.localStar != __instance.starData)
        {
            return true;
        }
        __instance.sunLight.enabled = false;
        Shader.SetGlobalVector("_Global_SunDir", GameMain.mainPlayer.transform.up);
        Shader.SetGlobalColor("_Global_SunsetColor0", Color.Lerp(Color.white, __instance.sunsetColor0, __instance.useSunsetColor));
        Shader.SetGlobalColor("_Global_SunsetColor1", Color.Lerp(Color.white, __instance.sunsetColor1, __instance.useSunsetColor));
        Shader.SetGlobalColor("_Global_SunsetColor2", Color.Lerp(Color.white, __instance.sunsetColor2, __instance.useSunsetColor));
        ___bodyMaterial.renderQueue = 2981;
        ___haloMaterial.renderQueue = 2981;
        __instance.blackRenderer.enabled = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlanetSimulator), "LateUpdate")]
    public static bool PlanetSimulatorLateUpdatePatch(PlanetSimulator __instance)
    {
        PlanetData localPlanet = GameMain.localPlanet;
        if (!GameDataImported || !SunLightOpen.Value || FactoryModel.whiteMode0 || localPlanet == null || localPlanet != __instance.planetData || SunLight == null)
        {
            return true;
        }
        Vector3 vector3 = GameMain.mainPlayer.transform.up;
        if (__instance.surfaceRenderer?.Length != 0)
            __instance.surfaceRenderer[0]?.sharedMaterial?.SetVector("_SunDir", vector3);
        __instance.reformMat0?.SetVector("_SunDir", vector3);
        __instance.reformMat1?.SetVector("_SunDir", vector3);
        __instance.atmoMat?.SetVector("_SunDir", vector3);
        __instance.atmoMatLate?.SetVector("_SunDir", vector3);
        __instance.cloudSimulator?.OnLateUpdate();
        if (__instance.oceanMat != null)
        {
            if (__instance.oceanRenderQueue > 2987)
            {
                __instance.oceanMat.renderQueue = ((localPlanet == __instance.planetData) ? __instance.oceanRenderQueue : 2988);
            }
            __instance.oceanMat.SetColor("_Planet_WaterAmbientColor0", __instance.planetData.ambientDesc.waterAmbientColor0);
            __instance.oceanMat.SetColor("_Planet_WaterAmbientColor1", __instance.planetData.ambientDesc.waterAmbientColor1);
            __instance.oceanMat.SetColor("_Planet_WaterAmbientColor2", __instance.planetData.ambientDesc.waterAmbientColor2);
        }
        if (__instance.atmoTrans0 != null && __instance.planetData != null && !__instance.planetData.loading && !__instance.planetData.factoryLoading)
        {
            StarSimulator star = GameMain.universeSimulator.FindStarSimulator(__instance.planetData.star); ;
            __instance.atmoTrans0.rotation = Camera.main.transform.localRotation;
            Vector4 value = (GameCamera.generalTarget == null) ? Vector3.zero : GameCamera.generalTarget.position;
            Vector3 position = GameCamera.main.transform.position;
            if (value.sqrMagnitude == 0f)
            {
                if (GameCamera.instance.isPlanetMode)
                {
                    value = (position + GameCamera.main.transform.forward * 30f).normalized * __instance.planetData.realRadius;
                }
                else
                {
                    value = GameMain.mainPlayer.position;
                }
            }
            Vector3 lhs = Camera.main.transform.localPosition - __instance.transform.localPosition;
            float magnitude = lhs.magnitude;
            VectorLF3 vectorLF = __instance.planetData.uPosition;
            if (localPlanet != null)
            {
                vectorLF = VectorLF3.zero;
            }
            else if (GameMain.mainPlayer != null)
            {
                vectorLF -= GameMain.mainPlayer.uPosition;
            }
            float d = 1f;
            Vector3 vector;
            UniverseSimulator.VirtualMapping(vectorLF.x, vectorLF.y, vectorLF.z, position, out vector, out d);
            float num = Vector3.Dot(lhs, Camera.main.transform.forward);
            __instance.atmoTrans1.localPosition = new Vector3(0f, 0f, Mathf.Clamp(num + 10f, 0f, 320f));
            float value2 = Mathf.Clamp01(8000f / magnitude);
            float num2 = Mathf.Clamp01(4000f / magnitude);
            float value3 = Mathf.Max(0f, magnitude / 6000f - 1f);
            Vector4 vector2 = __instance.atmoMatRadiusParam;
            vector2.z = vector2.x + (__instance.atmoMatRadiusParam.z - __instance.atmoMatRadiusParam.x) * (2.7f - num2 * 1.7f);
            vector2 *= d;
            __instance.atmoMat.SetVector("_PlanetPos", __instance.transform.localPosition);
            __instance.atmoMat.SetVector("_SunDir", vector3);
            __instance.atmoMat.SetVector("_PlanetRadius", vector2);
            __instance.atmoMat.SetColor("_Color4", star.sunAtmosColor);
            __instance.atmoMat.SetColor("_Sky4", star.sunriseAtmosColor);
            __instance.atmoMat.SetVector("_LocalPos", value);
            __instance.atmoMat.SetFloat("_SunRiseScatterPower", Mathf.Max(60f, (magnitude - __instance.planetData.realRadius * 2f) * 0.18f));
            __instance.atmoMat.SetFloat("_IntensityControl", value2);
            __instance.atmoMat.SetFloat("_DistanceControl", value3);
            __instance.atmoMat.renderQueue = ((__instance.planetData == localPlanet) ? 2991 : 2989);
            __instance.atmoMatLate.SetVector("_PlanetPos", __instance.transform.localPosition);
            __instance.atmoMatLate.SetVector("_SunDir", vector3);
            __instance.atmoMatLate.SetVector("_PlanetRadius", vector2);
            __instance.atmoMatLate.SetColor("_Color4", star.sunAtmosColor);
            __instance.atmoMatLate.SetColor("_Sky4", star.sunriseAtmosColor);
            __instance.atmoMatLate.SetVector("_LocalPos", value);
            __instance.atmoMatLate.SetFloat("_SunRiseScatterPower", Mathf.Max(60f, (magnitude - __instance.planetData.realRadius * 2f) * 0.18f));
            __instance.atmoMatLate.SetFloat("_IntensityControl", value2);
            __instance.atmoMatLate.SetFloat("_DistanceControl", value3);
            if (__instance.planetData == localPlanet)
            {
                __instance.atmoMatLate.renderQueue = 3200;
                __instance.atmoMatLate.SetInt("_StencilRef", 2);
                __instance.atmoMatLate.SetInt("_StencilComp", 3);
            }
            else
            {
                __instance.atmoMatLate.renderQueue = 2989;
                __instance.atmoMatLate.SetInt("_StencilRef", 0);
                __instance.atmoMatLate.SetInt("_StencilComp", 1);
            }
        }
        if (PerformanceMonitor.GpuProfilerOn)
        {
            if (__instance.surfaceRenderer != null)
            {
                foreach (Renderer renderer in __instance.surfaceRenderer)
                {
                    if (renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                    {
                        PerformanceMonitor.RecordGpuWork(EGpuWorkEntry.Universe, 1, renderer.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                    }
                }
            }
            if (__instance.reformRenderer != null && __instance.reformRenderer.enabled && __instance.reformRenderer.gameObject.activeInHierarchy)
            {
                PerformanceMonitor.RecordGpuWork(EGpuWorkEntry.Universe, 1, __instance.reformRenderer.GetComponent<MeshFilter>().sharedMesh.vertexCount);
            }
            if (__instance.oceanCollider != null && __instance.oceanCollider.gameObject.activeInHierarchy)
            {
                MeshFilter component = __instance.oceanCollider.GetComponent<MeshFilter>();
                if (component != null)
                {
                    PerformanceMonitor.RecordGpuWork(EGpuWorkEntry.Universe, 1, component.sharedMesh.vertexCount);
                }
            }
        }
        return false;
    }

}
