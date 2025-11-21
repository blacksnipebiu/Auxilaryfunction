using HarmonyLib;

namespace Auxilaryfunction.Patch;

public class PlayerAudioMutePatch
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
                _patch = Harmony.CreateAndPatchAll(typeof(PlayerAudioMutePatch));
            }
            else
            {
                _patch.UnpatchSelf();
            }
        }
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerAudio), "Update")]
    public static bool PlayerAudioUpdatePatch()
    {
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerFootsteps), "PlayFootstepSound")]
    public static bool PlayerFootstepsPatch()
    {
        return false;
    }
}
