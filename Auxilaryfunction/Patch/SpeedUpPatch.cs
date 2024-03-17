using HarmonyLib;
using UnityEngine;

namespace Auxilaryfunction.Patch
{
    internal class SpeedUpPatch
    {
        public static float SpeedMultiple;
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
                    _patch = Harmony.CreateAndPatchAll(typeof(SpeedUpPatch));
                }
                else
                {
                    _patch.UnpatchSelf();
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPSController), "Update")]
        public static void SpeedUp()
        {
            Time.fixedDeltaTime = 1f / (SpeedMultiple * 60);
        }
    }
}
