using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(BulletStatus), "OnTriggerEnter")]
    public static class Patch_BulletStatus_OnTriggerEnter
    {
        private static System.Reflection.FieldInfo _exceptTargetField;

        static bool Prefix(BulletStatus __instance, Collider other)
        {
            if (!Plugin.EnemyFriendlyFire.Value) return true;
            if (other == null) return true;

            // 只处理敌方子弹打到敌人
            bool isEnemyBullet = __instance.shooterTag == "Enemy"
                              || __instance.shooterTag == "Enemy2";
            if (!isEnemyBullet) return true;
            if (other.tag != "Enemy") return true;
            if (__instance.notActive) return true;

            // 防止子弹打到发射者自己
            if (__instance.source != null && other.gameObject == __instance.source) return true;

            __instance.red = true;
            __instance.DealingDamage(other.gameObject);

            // 触发爆炸
            if (__instance.bombHitSetting != null && __instance.bombHitSetting.enable)
            {
                if (_exceptTargetField == null)
                    _exceptTargetField = AccessTools.Field(typeof(BulletStatus), "exceptTarget");
                if (_exceptTargetField != null)
                    _exceptTargetField.SetValue(__instance, other.gameObject);
                __instance.ExplosionDamage();
            }

            // 销毁子弹（除非穿透）
            if (!__instance.penetrate)
            {
                UnityEngine.Object.Destroy(__instance.gameObject);
            }

            return false; // 跳过原方法
        }
    }

    [HarmonyPatch(typeof(BulletStatus), "ExplosionDamage")]
    public static class Patch_BulletStatus_ExplosionDamage
    {
        private static System.Reflection.FieldInfo _exceptTargetField;

        // ============ 复用的碰撞体缓冲区 ============
        // 避免 Physics.OverlapSphere 每次分配新的 Collider[]。
        private static readonly Collider[] _hitBuffer = new Collider[128];

        static void Postfix(BulletStatus __instance)
        {
            if (!Plugin.EnemyFriendlyFire.Value) return;

            bool isEnemyBullet = __instance.shooterTag == "Enemy"
                              || __instance.shooterTag == "Enemy2";
            if (!isEnemyBullet) return;
            if (__instance.bombHitSetting == null) return;

            if (_exceptTargetField == null)
                _exceptTargetField = AccessTools.Field(typeof(BulletStatus), "exceptTarget");
            if (_exceptTargetField == null) return;

            int count = Physics.OverlapSphereNonAlloc(
                __instance.transform.position,
                __instance.bombHitSetting.bombRadius,
                _hitBuffer);
            if (count <= 0) return;

            GameObject except = (GameObject)_exceptTargetField.GetValue(__instance);

            for (int i = 0; i < count; i++)
            {
                var h = _hitBuffer[i];
                if (h == null || h.tag != "Enemy") continue;
                if (__instance.source != null && h.gameObject == __instance.source) continue;
                if (except != null && h.gameObject == except) continue;

                __instance.DealingDamage(h.gameObject);
            }
        }
    }
}