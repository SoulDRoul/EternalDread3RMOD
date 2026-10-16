using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Status), "GainEXP")]
    [HarmonyPriority(Priority.First)]
    public static class Patch_Status_GainEXP_ExpMultiplier
    {
        static void Prefix(Status __instance, ref int gain)
        {
            if (!Plugin.TestExpMulEnabled.Value) return;
            if (__instance.gameObject != GlobalVar.mainPlayer) return;
            if (gain <= 0) return;

            int mul = Plugin.TestExpMul.Value;
            if (mul < 1) mul = 1;
            if (mul > 1000) mul = 1000;

            long result = (long)gain * mul;
            if (result > int.MaxValue) result = int.MaxValue;
            gain = (int)result;
        }
    }
}