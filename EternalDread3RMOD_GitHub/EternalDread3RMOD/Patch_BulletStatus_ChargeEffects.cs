using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 冲锋命中效果：
    ///   Prefix  —— 2B：按冲锋距离放大伤害（临时改 totalDamage）
    ///   Postfix —— 2A：把命中的敌人注册到 Tracker，并还原 totalDamage
    /// </summary>
    [HarmonyPatch(typeof(BulletStatus), "DealingDamage")]
    public static class Patch_BulletStatus_ChargeEffects
    {
        static void Prefix(BulletStatus __instance, GameObject other, out int __state)
        {
            __state = -1;
            if (!Plugin.ChargeDamageBoostEnabled.Value) return;
            if (__instance.source == null || other == null) return;
            if (__instance.source.tag != "Player") return;

            var tracker = __instance.source.GetComponent<ChargeStunTracker>();
            if (tracker == null || !tracker.IsChargingNow()) return;

            float mul = tracker.GetChargeMultiplier(other.transform.position);
            if (mul <= 1.0f) return;

            __state = __instance.totalDamage;
            __instance.totalDamage = Mathf.RoundToInt(__state * mul);
        }

        static void Postfix(BulletStatus __instance, GameObject other, int __state)
        {
            // 还原伤害
            if (__state >= 0) __instance.totalDamage = __state;

            // 2A：注册命中
            if (!Plugin.ChargeEnemyRandomEffectEnabled.Value) return;
            if (__instance.source == null || other == null) return;
            if (__instance.source.tag != "Player") return;
            if (other.tag != "Enemy" && other.tag != "BreakableEn") return;

            var targetStat = other.GetComponent<Status>();
            if (targetStat == null || targetStat.dead) return;

            var tracker = __instance.source.GetComponent<ChargeStunTracker>();
            if (tracker == null || !tracker.IsChargingNow()) return;

            tracker.RegisterHit(targetStat);
        }
    }
}