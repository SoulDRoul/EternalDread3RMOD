using UnityEngine;

namespace EternalDread3RMOD
{
    internal static class SelfDamageHelper
    {
        /// <summary>根据目标阵营，返回自伤/误伤是否允许</summary>
        public static bool IsSelfDamageAllowed(GameObject target)
        {
            if (target == null) return false;

            if (target.tag == "Player") return Plugin.SelfDamagePlayerEnabled.Value;
            if (target.tag == "Enemy") return Plugin.SelfDamageEnemyEnabled.Value;
            if (target.tag == "Ally") return Plugin.SelfDamageAllyEnabled.Value;

            return true;
        }

        /// <summary>根据目标阵营，返回伤害倍率</summary>
        public static float GetMultiplier(GameObject target)
        {
            if (target == null) return 1f;

            if (target.tag == "Player")
                return Plugin.SelfDamagePlayerPercent.Value / 100f;
            if (target.tag == "Enemy")
                return Plugin.SelfDamageEnemyPercent.Value / 100f;
            if (target.tag == "Ally")
                return Plugin.SelfDamageAllyPercent.Value / 100f;

            return 1f;
        }

        /// <summary>临时缩放 totalDamage 后调用 DealingDamage</summary>
        public static void DealSelfDamage(BulletStatus bl, GameObject target)
        {
            if (bl == null || target == null) return;

            int orig = bl.totalDamage;
            float mul = GetMultiplier(target);
            int scaled = Mathf.Max(1, Mathf.RoundToInt(orig * mul));

            bl.totalDamage = scaled;
            try
            {
                bl.DealingDamage(target);
            }
            finally
            {
                bl.totalDamage = orig;
            }
        }
    }
}