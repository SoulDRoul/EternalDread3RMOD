using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(GlobalClock), "Awake")]
    public static class Patch_GlobalClock_Awake
    {
        public static float NormalDuration = -1f;

        static void Postfix(GlobalClock __instance)
        {
            if (__instance.duration > NormalDuration)
            {
                NormalDuration = __instance.duration;
                Plugin.DebugLog("[ED3R] GlobalClock normal duration = " + NormalDuration);
            }
        }
    }

    [HarmonyPatch(typeof(GlobalClock), "Update")]
    public static class Patch_GlobalClock_Update
    {
        public static float CurrentDuration = 60f;

        static void Postfix(GlobalClock __instance)
        {
            CurrentDuration = __instance.duration;
        }
    }
}