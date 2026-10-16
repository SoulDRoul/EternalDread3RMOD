using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Status), "Awake")]
    public static class Patch_Status_Awake_Cleanup
    {
        static void Postfix(Status __instance)
        {
            if (__instance.GetComponent<StatusCleanupTracker>() != null) return;
            __instance.gameObject.AddComponent<StatusCleanupTracker>();
        }
    }
}