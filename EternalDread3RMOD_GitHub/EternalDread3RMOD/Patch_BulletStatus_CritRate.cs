using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(BulletStatus), "DealingDamage")]
    public static class Patch_BulletStatus_CritRate
    {
        // ============ 硬编码常量：Combat.FlankAngle ============
        // 原 Combat.FlankRearAngle —— 背面起始角度；攻击方向与目标朝向夹角 > 此值视为背面
        private const int FLANK_REAR_ANGLE = 120;
        // 原 Combat.FlankSideAngle —— 侧面起始角度；夹角 > 此值视为侧面（≤ 此值视为正面）
        private const int FLANK_SIDE_ANGLE = 60;

        // ============ 玩家 Status 缓存 ============
        private static GameObject _cachedPlayerObj;
        private static Status _cachedPlayerStat;

        [HarmonyPriority(Priority.High)]
        static void Prefix(BulletStatus __instance, GameObject other, out int __state)
        {
            __state = -1;   // -1 = 未修改
            if (other == null) return;

            int bonus = 0;
            var targetAi = other.GetComponent<AiStatus>();

            // ============ E17 背刺 ============
            // 目标未进入战斗状态（followTarget=null 且不 angry）→ 大加成
            bool isBackStab = false;
            if (Plugin.BackStabEnabled.Value
                && targetAi != null
                && targetAi.followTarget == null
                && !targetAi.angry)
            {
                bonus += Plugin.BackStabBonusCrit.Value;
                isBackStab = true;
            }

            // ============ Flank 绕后（E17 未触发时才走）============
            if (!isBackStab && Plugin.FlankCritEnabled.Value)
            {
                bonus += GetFlankBonus(__instance.source, other, targetAi);
            }

            // ============ 等级补正（仅玩家侧）============
            if (Plugin.LevelBonusEnabled.Value && IsPlayerSide(__instance.source))
            {
                var enemyStat = other.GetComponent<Status>();
                if (enemyStat != null)
                {
                    var mainPlayer = GlobalVar.mainPlayer;
                    Status playerStat = null;
                    if (mainPlayer != null)
                    {
                        if (!ReferenceEquals(_cachedPlayerObj, mainPlayer))
                        {
                            _cachedPlayerObj = mainPlayer;
                            _cachedPlayerStat = mainPlayer.GetComponent<Status>();
                        }
                        playerStat = _cachedPlayerStat;
                    }
                    if (playerStat != null)
                    {
                        int diff = playerStat.level - enemyStat.level;
                        if (diff > 0)
                        {
                            int effectiveDiff = diff;
                            if (effectiveDiff > Plugin.LevelCritRateMaxDiff.Value)
                                effectiveDiff = Plugin.LevelCritRateMaxDiff.Value;

                            bonus += effectiveDiff * Plugin.LevelCritRatePercentPerLevel.Value;
                        }
                    }
                }
            }

            if (bonus <= 0) return;

            __state = __instance.criticalPlus;
            __instance.criticalPlus += bonus;
        }

        [HarmonyPriority(Priority.High)]
        static void Postfix(BulletStatus __instance, int __state)
        {
            if (__state < 0) return;
            __instance.criticalPlus = __state;
        }

        /// <summary>
        /// 绕后加成：
        ///   角度 = Angle(目标朝向, 攻击者方向)
        ///   > RearAngle  → 背面 +RearBonus
        ///   > SideAngle  → 侧面 +SideBonus
        ///   否则         → 正面 无加成
        /// 只有目标有明确朝向（有 AiStatus 或 tag == Player）才生效。
        /// </summary>
        private static int GetFlankBonus(GameObject attacker, GameObject target, AiStatus targetAi)
        {
            if (attacker == null || target == null) return 0;

            // 判据：目标必须"有明确朝向"
            bool hasFacing = targetAi != null || target.CompareTag("Player");
            if (!hasFacing) return 0;

            Vector3 targetFwd = target.transform.forward;
            Vector3 toAttacker = attacker.transform.position - target.transform.position;

            // 水平化（忽略 Y 轴高度差）
            targetFwd.y = 0f;
            toAttacker.y = 0f;

            if (targetFwd.sqrMagnitude < 0.0001f) return 0;
            if (toAttacker.sqrMagnitude < 0.0001f) return 0;

            targetFwd.Normalize();
            toAttacker.Normalize();

            float angle = Vector3.Angle(targetFwd, toAttacker);

            if (angle > FLANK_REAR_ANGLE)
                return Plugin.FlankRearBonusCrit.Value;
            if (angle > FLANK_SIDE_ANGLE)
                return Plugin.FlankSideBonusCrit.Value;
            return 0;
        }

        private static bool IsPlayerSide(GameObject attacker)
        {
            if (attacker == null) return false;
            if (attacker == GlobalVar.mainPlayer) return true;

            var ai = attacker.GetComponent<AiStatus>();
            if (ai != null && ai.master != null
                && ai.master.gameObject == GlobalVar.mainPlayer)
            {
                return true;
            }
            return false;
        }
    }
}