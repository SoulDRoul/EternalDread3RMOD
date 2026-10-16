using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 倒地伤害加成。
    /// 目标处于 down 状态时，受到的伤害按百分比放大。
    /// 对所有单位生效（玩家、敌人、友军、NPC）。
    /// 
    /// 实现：在 OnDamage 的 Prefix 修改 ref amount，
    ///      后续的原版逻辑（元素抗性、防御等）按比例放大。
    /// </summary>
    [HarmonyPatch(typeof(Status), "OnDamage")]
    [HarmonyPriority(Priority.Normal)]
    public static class Patch_Status_OnDamage_Downed
    {
        static void Prefix(Status __instance, ref int amount)
        {
            if (!Plugin.DownedDamageBoostEnabled.Value) return;
            if (__instance == null) return;
            if (!__instance.down) return;

            int pct = Plugin.DownedDamageBoostPercent.Value;
            if (pct <= 0) return;

            float mul = 1f + pct / 100f;
            long result = (long)amount * (int)(mul * 100) / 100;
            if (result > int.MaxValue) result = int.MaxValue;
            if (result < 0) result = 0;
            amount = (int)result;
        }
    }
}