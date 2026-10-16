using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 检测敌人是否处于"正在攻击"状态。
    /// 覆盖所有已知攻击类型：普攻 / 冲锋 / 技能 / 飞行冲锋 / Tuahia / AllyBasic。
    /// 
    /// 语义：攻击动作从开始到结束（含前摇 + 命中 + 收招）全程为 true。
    /// 不做精确到帧的命中判定预警。
    /// </summary>
    public static class ThreatAttackingDetector
    {
        private static FieldInfo _f_ec_attacking;
        private static FieldInfo _f_es_attacking;
        private static FieldInfo _f_fe_attacking;
        private static FieldInfo _f_tu_attacking;
        private static FieldInfo _f_ab_attacking;
        private static bool _init;

        public static bool IsAttacking(AiStatus ai)
        {
            if (ai == null) return false;
            EnsureInit();

            // 1. 基础 AiStatus.attacking（public，用于 EnemyBasic 等）
            if (ai.attacking) return true;

            // 2. 各子类的 private attacking
            if (Check<EnemyCharging>(ai, _f_ec_attacking)) return true;
            if (Check<EnemySkill>(ai, _f_es_attacking)) return true;
            if (Check<FlyingEnemy>(ai, _f_fe_attacking)) return true;
            if (Check<Tuahia>(ai, _f_tu_attacking)) return true;
            if (Check<AllyBasic>(ai, _f_ab_attacking)) return true;

            return false;
        }

        private static bool Check<T>(AiStatus ai, FieldInfo field) where T : Component
        {
            if (field == null) return false;
            var comp = ai.GetComponent<T>();
            if (comp == null) return false;
            try { return (bool)field.GetValue(comp); }
            catch { return false; }
        }

        private static void EnsureInit()
        {
            if (_init) return;
            _init = true;
            _f_ec_attacking = AccessTools.Field(typeof(EnemyCharging), "attacking");
            _f_es_attacking = AccessTools.Field(typeof(EnemySkill), "attacking");
            _f_fe_attacking = AccessTools.Field(typeof(FlyingEnemy), "attacking");
            _f_tu_attacking = AccessTools.Field(typeof(Tuahia), "attacking");
            _f_ab_attacking = AccessTools.Field(typeof(AllyBasic), "attacking");
        }
    }
}