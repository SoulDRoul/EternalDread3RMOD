using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Status), "Awake")]
    public static class Patch_Status_Awake_ChargeStun
    {
        static void Postfix(Status __instance)
        {
            if (!Plugin.ChargeStunEnabled.Value) return;
            if (__instance.GetComponent<ChargeStunTracker>() != null) return;
            __instance.gameObject.AddComponent<ChargeStunTracker>();
        }
    }
}