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
                    //Time.timeScale = SpeedMultiple;
                    _patch = Harmony.CreateAndPatchAll(typeof(SpeedUpPatch));
                }
                else
                {
                    //Time.timeScale = 1;
                    _patch.UnpatchSelf();
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
                TargetFixedDeltaTime = 1 / (speedMultiple * 60f);
                if (Enable)
                {
                    //Time.timeScale = SpeedMultiple;
                }
            }
        }

        public static float TargetFixedDeltaTime = 0.0166666f;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSController), "Update")]
        public static void SpeedUp()
        {
            Time.fixedDeltaTime = TargetFixedDeltaTime;
            //Time.timeScale = SpeedMultiple;
        }
    }
}
