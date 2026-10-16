using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 燃烧时长倍率。InflictBurn 是 private，用 Prefix 改 ref float dur。
    /// 影响所有点燃来源（技能、武器、蔓延），因为蔓延也走 ApplyAbnormalStat(9)。
    /// </summary>
    [HarmonyPatch(typeof(Status), "InflictBurn")]
    public static class Patch_Status_InflictBurn_Duration
    {
        static void Prefix(ref float dur)
        {
            if (!Plugin.BurnDurationMulEnabled.Value) return;

            int mul = Plugin.BurnDurationMul.Value;
            if (mul <= 0) return;

            dur *= mul;
        }
    }
}