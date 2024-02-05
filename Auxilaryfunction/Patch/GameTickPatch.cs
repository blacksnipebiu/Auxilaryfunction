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
        public static bool GameTick1Patch(ref long time, GameData __instance)
        {
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Statistics);
            if (!DSPGame.IsMenuDemo)
            {
                __instance.statistics.PrepareTick();
                __instance.history.PrepareTick();
            }
            __instance.mainPlayer.packageUtility.Count();
            PerformanceMonitor.EndSample(ECpuWorkEntry.Statistics);

            PerformanceMonitor.BeginSample(ECpuWorkEntry.Skill);
            __instance.spaceSector.skillSystem.PrepareTick();
            PerformanceMonitor.EndSample(ECpuWorkEntry.Skill);
            __instance.spaceSector.skillSystem.CollectTempStates();

            if (__instance.localPlanet != null && __instance.localPlanet.factoryLoaded)
            {
                PerformanceMonitor.BeginSample(ECpuWorkEntry.LocalPhysics);
                __instance.localPlanet.factory.cargoTraffic.ClearStates();
                __instance.localPlanet.physics.GameTick();
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
                __instance.mainPlayer.GameTick(time);
            __instance.DetermineRelative();
            __instance.spaceSector.skillSystem.CollectPlayerStates();
            PerformanceMonitor.EndSample(ECpuWorkEntry.Player);

            if (__instance.localPlanet != null && __instance.localPlanet.factoryLoaded)
            {
                PerformanceMonitor.BeginSample(ECpuWorkEntry.LocalAudio);
                __instance.localPlanet.audio.GameTick();
                PerformanceMonitor.EndSample(ECpuWorkEntry.LocalAudio);
            }
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Factory);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.PowerSystem);
            for (int j = 0; j < __instance.factoryCount; j++)
            {
                Assert.NotNull(__instance.factories[j]);
                if (__instance.factories[j] != null)
                {
                    __instance.factories[j].BeforeGameTick(time);
                }
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.PowerSystem);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Construction);
            for (int k = 0; k < __instance.factoryCount; k++)
            {
                bool isActive = GameMain.localPlanet == __instance.factories[k].planet;
                if (__instance.factories[k].constructionSystem != null)
                {
                    __instance.factories[k].constructionSystem.GameTick(time, isActive, false);
                }
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.Construction);
            if (GameMain.multithreadSystem.multithreadSystemEnable)
            {
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


            PerformanceMonitor.BeginSample(ECpuWorkEntry.Combat);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Defense);
            if (GameMain.multithreadSystem.multithreadSystemEnable)
            {
                for (int num14 = 0; num14 < __instance.factoryCount; num14++)
                {
                    bool isActive7 = GameMain.localPlanet == __instance.factories[num14].planet;
                    __instance.factories[num14].defenseSystem.GameTick(time, isActive7);
                    __instance.factories[num14].planetATField.GameTick(time, isActive7);
                }
            }
            else
            {
                for (int num15 = 0; num15 < __instance.factoryCount; num15++)
                {
                    bool isActive8 = GameMain.localPlanet == __instance.factories[num15].planet;
                    __instance.factories[num15].defenseSystem.GameTick(time, isActive8);
                    __instance.factories[num15].planetATField.GameTick(time, isActive8);
                }
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.Defense);
            PerformanceMonitor.EndSample(ECpuWorkEntry.Combat);
            __instance.preferences.Collect();
            return false;
        }

    }
}
