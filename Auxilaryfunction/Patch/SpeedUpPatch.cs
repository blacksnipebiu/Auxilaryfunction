using HarmonyLib;
using UnityEngine;

namespace Auxilaryfunction.Patch
{
    internal class SpeedUpPatch
    {
        private static float speedMultiple;
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
                    Time.timeScale = SpeedMultiple;
                    //_patch = Harmony.CreateAndPatchAll(typeof(SpeedUpPatch));
                }
                else
                {
                    Time.timeScale = 1;
                    //_patch.UnpatchSelf();
                }
            }
        }

        public static float SpeedMultiple
        {
            get
            {
                return speedMultiple;
            }

            set
            {
                speedMultiple = value;
                if (Enable)
                {
                    Time.timeScale = SpeedMultiple;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSController), "Update")]
        public static void SpeedUp(FPSController __instance)
        {
            //Time.fixedDeltaTime = 1f / (SpeedMultiple * 60);
            Time.timeScale = SpeedMultiple;
        }
    }
}
