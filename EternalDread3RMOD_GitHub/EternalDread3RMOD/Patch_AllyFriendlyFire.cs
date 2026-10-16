using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    internal static class AllyFriendlyFireHelper
    {
        /// <summary>友军误伤：目标是否应受伤</summary>
        public static bool ShouldDamage(GameObject target)
        {
            if (target == null) return false;

            var stat = target.GetComponent<Status>();
            if (stat == null) return false;
            if (stat.dead) return false;

            string t = target.tag;
            if (t == "Player") return Plugin.AllyFriendlyFireTargetPlayerEnabled.Value;
            if (t == "Ally") return Plugin.AllyFriendlyFireTargetAllyEnabled.Value;
            return false;
        }

        /// <summary>是否还在出膛保护期内</summary>
        public static bool IsInProtection(BulletStatus bl)
        {
            if (!Plugin.AllyFriendlyFireUseProtectionSec.Value) return false;
            if (bl == null) return false;

            float spawnTime = BulletSpawnTimeTracker.Get(bl);
            return Time.time - spawnTime < Plugin.SelfHitProtectionSec.Value;
        }
    }

    // ============ 入口 1：子弹直接命中 ============
    [HarmonyPatch(typeof(BulletStatus), "OnTriggerEnter")]
    public static class Patch_AllyFriendlyFire_BulletTrigger
    {
        static void Postfix(BulletStatus __instance, Collider other)
        {
            if (!Plugin.AoeAllyFriendlyFireEnabled.Value) return;
            if (other == null) return;
            if (__instance.notActive) return;
            if (__instance.source == null) return;
            if (__instance.source.tag != "Ally") return;
            if (other.gameObject == __instance.source) return;   // 自伤走自伤路径

            if (AllyFriendlyFireHelper.IsInProtection(__instance)) return;

            if (!AllyFriendlyFireHelper.ShouldDamage(other.gameObject)) return;
            if (!SelfHitCooldown.Allow(other.gameObject, 0.5f)) return;

            SelfDamageHelper.DealSelfDamage(__instance, other.gameObject);
        }
    }

    // ============ 入口 2：子弹爆炸 ============
    [HarmonyPatch(typeof(BulletStatus), "ExplosionDamage")]
    public static class Patch_AllyFriendlyFire_Explosion
    {
        static void Postfix(BulletStatus __instance)
        {
            if (!Plugin.AoeAllyFriendlyFireEnabled.Value) return;
            if (__instance.source == null) return;
            if (__instance.source.tag != "Ally") return;
            if (__instance.bombHitSetting == null) return;

            if (AllyFriendlyFireHelper.IsInProtection(__instance)) return;

            Collider[] hits = Physics.OverlapSphere(
                __instance.transform.position,
                __instance.bombHitSetting.bombRadius);
            if (hits == null) return;

            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h == null) continue;
                if (h.gameObject == __instance.source) continue;
                if (!AllyFriendlyFireHelper.ShouldDamage(h.gameObject)) continue;
                if (!SelfHitCooldown.Allow(h.gameObject, 0.5f)) continue;

                SelfDamageHelper.DealSelfDamage(__instance, h.gameObject);
            }
        }
    }

    // ============ 入口 3：持续区域 ============
    [HarmonyPatch(typeof(AreaDamageSkill), "ApplyDamage")]
    public static class Patch_AllyFriendlyFire_AreaDamage
    {
        private static FieldInfo _blField;

        static void Postfix(AreaDamageSkill __instance)
        {
            if (!Plugin.AoeAllyFriendlyFireEnabled.Value) return;

            if (_blField == null)
                _blField = AccessTools.Field(typeof(AreaDamageSkill), "bl");
            if (_blField == null) return;

            var bl = _blField.GetValue(__instance) as BulletStatus;
            if (bl == null || bl.source == null) return;
            if (bl.source.tag != "Ally") return;

            if (AllyFriendlyFireHelper.IsInProtection(bl)) return;

            if (!SelfHitCooldown.Allow(__instance, 1.0f)) return;

            Collider[] hits = Physics.OverlapSphere(
                __instance.transform.position,
                __instance.radius);
            if (hits == null) return;

            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h == null) continue;
                if (h.gameObject == bl.source) continue;
                if (!AllyFriendlyFireHelper.ShouldDamage(h.gameObject)) continue;

                SelfDamageHelper.DealSelfDamage(bl, h.gameObject);
            }
        }
    }

    // ============ 入口 4：光环 ============
    [HarmonyPatch(typeof(Aura), "ApplyDamageAura")]
    public static class Patch_AllyFriendlyFire_Aura
    {
        static void Postfix(Aura __instance)
        {
            if (!Plugin.AoeAllyFriendlyFireEnabled.Value) return;
            if (__instance.source == null) return;
            if (__instance.source.tag != "Ally") return;

            var sourceGo = __instance.source.gameObject;

            // Aura 本身有 delayPerHit 延迟。这里不再叠加保护期——
            // 但如果用户想统一行为，也可以在这里加判断。当前设计是
            // Aura 天然已经过了保护期（首次生效在 delayPerHit 秒后）。
            if (!SelfHitCooldown.Allow(__instance, 1.0f)) return;

            Collider[] hits = Physics.OverlapSphere(
                __instance.transform.position,
                __instance.radius);
            if (hits == null) return;

            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h == null) continue;
                if (h.gameObject == sourceGo) continue;
                if (!AllyFriendlyFireHelper.ShouldDamage(h.gameObject)) continue;

                float mul = SelfDamageHelper.GetMultiplier(h.gameObject);

                int oMin = __instance.valueMin;
                int oMax = __instance.valueMax;
                int oAdd = __instance.additionValue;

                __instance.valueMin = Mathf.Max(1, Mathf.RoundToInt(oMin * mul));
                __instance.valueMax = Mathf.Max(1, Mathf.RoundToInt(oMax * mul));
                __instance.additionValue = Mathf.RoundToInt(oAdd * mul);

                try
                {
                    __instance.SendMessage("Damage", h.gameObject,
                        SendMessageOptions.DontRequireReceiver);
                }
                finally
                {
                    __instance.valueMin = oMin;
                    __instance.valueMax = oMax;
                    __instance.additionValue = oAdd;
                }
            }
        }
    }
}