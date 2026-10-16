using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    // ============ OnTriggerEnter ============
    [HarmonyPatch(typeof(BulletStatus), "OnTriggerEnter")]
    public static class Patch_BulletStatus_OnTriggerEnter_SelfHit
    {
        static void Postfix(BulletStatus __instance, Collider other)
        {
            if (other == null) return;
            if (__instance.notActive) return;
            if (__instance.source == null) return;
            if (other.gameObject != __instance.source) return;
            if (!Plugin.AoeSelfDamageEnabled.Value) return;
            if (!Plugin.SelfHitBulletsEnabled.Value) return;

            float spawnTime = BulletSpawnTimeTracker.Get(__instance);
            if (Time.time - spawnTime < Plugin.SelfHitProtectionSec.Value) return;

            // 冷却
            if (!SelfHitCooldown.Allow(__instance, 0.5f)) return;

            SelfDamageHelper.DealSelfDamage(__instance, other.gameObject);
        }
    }

    // ============ ExplosionDamage ============
    [HarmonyPatch(typeof(BulletStatus), "ExplosionDamage")]
    public static class Patch_BulletStatus_ExplosionDamage_SelfHit
    {
        static void Postfix(BulletStatus __instance)
        {
            if (!Plugin.AoeSelfDamageEnabled.Value) return;
            if (__instance.source == null) return;
            if (__instance.bombHitSetting == null) return;

            float dist = Vector3.Distance(
                __instance.transform.position,
                __instance.source.transform.position);
            if (dist > __instance.bombHitSetting.bombRadius) return;

            // 冷却（1 秒），与毒雾 tick 频率对齐
            if (!SelfHitCooldown.Allow(__instance, 1.0f)) return;

            SelfDamageHelper.DealSelfDamage(__instance, __instance.source);
        }
    }

    // ============ AreaDamageSkill ============
    [HarmonyPatch(typeof(AreaDamageSkill), "ApplyDamage")]
    public static class Patch_AreaDamageSkill_SelfHit
    {
        private static System.Reflection.FieldInfo _blField;

        static void Postfix(AreaDamageSkill __instance)
        {
            if (!Plugin.AoeSelfDamageEnabled.Value) return;

            if (_blField == null)
                _blField = AccessTools.Field(typeof(AreaDamageSkill), "bl");
            if (_blField == null) return;

            var bl = _blField.GetValue(__instance) as BulletStatus;
            if (bl == null || bl.source == null) return;

            float dist = Vector3.Distance(
                __instance.transform.position,
                bl.source.transform.position);
            if (dist > __instance.radius) return;

            // 冷却，用 AreaDamageSkill 实例作为 key
            if (!SelfHitCooldown.Allow(__instance, 1.0f)) return;

            SelfDamageHelper.DealSelfDamage(bl, bl.source);
        }
    }

    // ============ Aura ============
    [HarmonyPatch(typeof(Aura), "ApplyDamageAura")]
    public static class Patch_Aura_SelfHit
    {
        static void Postfix(Aura __instance)
        {
            if (!Plugin.AoeSelfDamageEnabled.Value) return;
            if (__instance.source == null) return;

            var sourceGo = __instance.source.gameObject;
            float dist = Vector3.Distance(
                __instance.transform.position,
                sourceGo.transform.position);
            if (dist > __instance.radius) return;

            // 冷却
            if (!SelfHitCooldown.Allow(__instance, 1.0f)) return;
            // 新增：自伤开关判定
            if (!SelfDamageHelper.IsSelfDamageAllowed(sourceGo)) return;

            int oMin = __instance.valueMin;
            int oMax = __instance.valueMax;
            int oAdd = __instance.additionValue;

            float mul = SelfDamageHelper.GetMultiplier(sourceGo);
            __instance.valueMin = Mathf.Max(1, Mathf.RoundToInt(oMin * mul));
            __instance.valueMax = Mathf.Max(1, Mathf.RoundToInt(oMax * mul));
            __instance.additionValue = Mathf.RoundToInt(oAdd * mul);

            try
            {
                __instance.SendMessage("Damage", sourceGo,
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