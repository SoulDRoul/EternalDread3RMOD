using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 让敌人的 AOE（毒雾、闪电柱等）也能伤害敌人。
    /// 原版 AreaDamageSkill 的判定只对 Player/Ally 生效，对 Enemy 完全没写。
    /// </summary>
    [HarmonyPatch(typeof(AreaDamageSkill), "ApplyDamage")]
    public static class Patch_AreaDamageSkill_EnemyHit
    {
        private static System.Reflection.FieldInfo _blField;

        // ============ 复用的碰撞体缓冲区 ============
        // 原代码每次 ApplyDamage 都调用 Physics.OverlapSphere（非 NonAlloc），
        // 内部会分配一个新 Collider[]。AOE 区域（毒雾等）每帧执行，是真实的 GC 热点。
        // 128 足够容纳任何合理 AOE 半径内的碰撞体。
        private static readonly Collider[] _hitBuffer = new Collider[128];

        static void Postfix(AreaDamageSkill __instance)
        {
            if (!Plugin.EnemyAoeHitsEnemy.Value) return;

            if (_blField == null)
                _blField = AccessTools.Field(typeof(AreaDamageSkill), "bl");
            if (_blField == null) return;

            var bl = _blField.GetValue(__instance) as BulletStatus;
            if (bl == null) return;
            if (bl.shooterTag != "Enemy" && bl.shooterTag != "Enemy2") return;

            int count = Physics.OverlapSphereNonAlloc(
                __instance.transform.position, __instance.radius, _hitBuffer);
            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                var h = _hitBuffer[i];
                if (h == null) continue;
                if (h.tag != "Enemy") continue;
                if (h.gameObject == bl.source) continue;
                if (h.gameObject == __instance.gameObject) continue;

                bl.DealingDamage(h.gameObject);
            }
        }
    }
}