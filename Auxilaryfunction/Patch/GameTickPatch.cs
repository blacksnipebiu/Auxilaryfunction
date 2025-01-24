using HarmonyLib;

namespace Auxilaryfunction.Patch
{
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData), "GameTick")]
        public static bool GameTick1Path(ref long time, GameData __instance)
        {
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Statistics);
            if (!DSPGame.IsMenuDemo)
            {
                __instance.statistics.PrepareTick(time);
                __instance.history.PrepareTick();
            }
            __instance.mainPlayer.packageUtility.Count();
            PerformanceMonitor.EndSample(ECpuWorkEntry.Statistics);
            __instance.spaceSector.skillSystem.CollectTempStates();
            if (__instance.localPlanet != null && __instance.localPlanet.factoryLoaded)
            {
                PerformanceMonitor.BeginSample(ECpuWorkEntry.LocalPhysics);
                __instance.localPlanet.factory.cargoTraffic.ClearStates();
                __instance.localPlanet.physics.GameTick();
                PerformanceMonitor.EndSample(ECpuWorkEntry.LocalPhysics);
            }
            if (__instance.spaceSector != null)
            {
                PerformanceMonitor.BeginSample(ECpuWorkEntry.LocalPhysics);
                if (!DSPGame.IsMenuDemo)
                {
                    __instance.spaceSector.physics.GameTick();
                }
                PerformanceMonitor.EndSample(ECpuWorkEntry.LocalPhysics);
            }
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Scenario);
            if (__instance.guideMission != null)
            {
                __instance.guideMission.GameTick();
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.Scenario);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Player);
            if (__instance.mainPlayer != null)
            {
                __instance.mainPlayer.GameTick(time);
            }
            __instance.DetermineRelative();
            __instance.mainPlayer.packageUtility.Count();
            __instance.spaceSector.skillSystem.CollectPlayerStates();
            PerformanceMonitor.EndSample(ECpuWorkEntry.Player);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Factory);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.PowerSystem);
            for (int j = 0; j < __instance.factoryCount; j++)
            {
                if (__instance.factories[j] != null)
                {
                    __instance.factories[j].BeforeGameTick(time);
                }
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.PowerSystem);
            if (GameMain.multithreadSystem.multithreadSystemEnable)
            {
                PerformanceMonitor.BeginSample(ECpuWorkEntry.Construction);
                for (int k = 0; k < __instance.factoryCount; k++)
                {
                    bool isActive = GameMain.localPlanet == __instance.factories[k].planet;
                    if (__instance.factories[k].constructionSystem != null)
                    {
                        __instance.factories[k].constructionSystem.GameTick(time, isActive, false);
                        __instance.factories[k].constructionSystem.ExcuteDeferredTargetChange();
                    }
                }
                PerformanceMonitor.EndSample(ECpuWorkEntry.Construction);

                PerformanceMonitor.BeginSample(ECpuWorkEntry.Digital);
                for (int num5 = 0; num5 < __instance.factoryCount; num5++)
                {
                    bool isActive4 = GameMain.localPlanet == __instance.factories[num5].planet;
                    if (__instance.factories[num5].digitalSystem != null)
                    {
                        __instance.factories[num5].digitalSystem.GameTick(isActive4);
                    }
                }
                PerformanceMonitor.EndSample(ECpuWorkEntry.Digital);
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.Factory);

            PerformanceMonitor.BeginSample(ECpuWorkEntry.Trash);
            __instance.trashSystem.GameTick(time);
            PerformanceMonitor.EndSample(ECpuWorkEntry.Trash);

            if (__instance.localPlanet != null && __instance.localPlanet.factoryLoaded)
            {
                PerformanceMonitor.BeginSample(ECpuWorkEntry.LocalAudio);
                __instance.localPlanet.audio.GameTick();
                PerformanceMonitor.EndSample(ECpuWorkEntry.LocalAudio);
            }
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Statistics);
            if (!DSPGame.IsMenuDemo)
            {
                __instance.statistics.GameTick(time);
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.Statistics);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Digital);
            if (!DSPGame.IsMenuDemo)
            {
                __instance.warningSystem.GameTick(time);
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.Digital);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Scenario);
            __instance.milestoneSystem.GameTick(time);
            PerformanceMonitor.EndSample(ECpuWorkEntry.Scenario);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Statistics);
            __instance.history.AfterTick();
            __instance.statistics.AfterTick();
            PerformanceMonitor.EndSample(ECpuWorkEntry.Statistics);
            __instance.preferences.Collect();
            return false;
        }
    }
}