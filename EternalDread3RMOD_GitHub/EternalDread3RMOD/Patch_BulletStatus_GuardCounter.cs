using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 格挡反击链。
    /// 玩家格挡成功后 WindowSec 秒内，攻击 1.5m 内的敌人时，触发反击效果。
    /// 效果：随机施加 眩晕 / 击倒（失败走 fallback）。
    /// 触发后清空时间戳，避免同一次格挡反复利用。
    /// </summary>
    [HarmonyPatch(typeof(BulletStatus), "DealingDamage")]
    public static class Patch_BulletStatus_GuardCounter
    {
        private static MethodInfo _m_inflictStun;
        private static MethodInfo _m_knockDown;

        static void Postfix(BulletStatus __instance, GameObject other)
        {
            if (!Plugin.GuardCounterEnabled.Value) return;
            if (__instance.source == null || other == null) return;
            if (__instance.source != GlobalVar.mainPlayer) return;

            // 时间窗口（不消耗，窗口过期自然失效）
            float elapsed = Time.time - GuardCounterState.LastBlockSuccessTime;
            if (elapsed > Plugin.GuardCounterWindowSec.Value) return;

            // 该敌人是否已触发过
            if (GuardCounterState.TriggeredTargets.Contains(other)) return;

            // 目标 tag
            if (other.tag != "Enemy" && other.tag != "BreakableEn") return;

            // 距离判定（水平距离，忽略 Y）
            Vector3 a = __instance.source.transform.position;
            Vector3 b = other.transform.position;
            a.y = 0f; b.y = 0f;
            float dist = Vector3.Distance(a, b);
            if (dist > Plugin.GuardCounterMaxDistance.Value) return;

            // 目标状态
            var targetStat = other.GetComponent<Status>();
            if (targetStat == null || targetStat.dead) return;

            // 冷却（用玩家 GameObject 做 key）
            if (!SelfHitCooldown.Allow(__instance.source, Plugin.GuardCounterCooldownSec.Value))
                return;

            // 应用反击效果
            float dur = Plugin.GuardCounterDuration.Value;
            if (dur <= 0f) return;

            ApplyRandomCounterEffect(targetStat, dur);

            // 标记该敌人已处理（无论效果是否成功，都避免重复尝试）
            GuardCounterState.TriggeredTargets.Add(other);
        }

        /// <summary>随机首选，失败 fallback 到另一个。返回是否至少成功一次。</summary>
        private static bool ApplyRandomCounterEffect(Status enemy, float dur)
        {
            EnsureMethods();

            // 0=眩晕, 1=击倒
            int pick = Random.Range(0, 2);

            if (TryApplyEffect(enemy, pick, dur)) return true;
            if (TryApplyEffect(enemy, 1 - pick, dur)) return true;
            return false;
        }

        private static bool TryApplyEffect(Status enemy, int type, float dur)
        {
            if (enemy == null || enemy.dead) return false;

            if (type == 0)  // 眩晕
            {
                if (_m_inflictStun == null) return false;
                try { _m_inflictStun.Invoke(enemy, new object[] { dur }); }
                catch { return false; }
                return enemy.onStun > 0f;
            }
            else  // 击倒
            {
                if (_m_knockDown == null) return false;
                try { _m_knockDown.Invoke(enemy, new object[] { dur }); }
                catch { return false; }
                return enemy.down;
            }
        }

        private static void EnsureMethods()
        {
            if (_m_inflictStun == null)
                _m_inflictStun = AccessTools.Method(typeof(Status), "InflictStun", new[] { typeof(float) });
            if (_m_knockDown == null)
                _m_knockDown = AccessTools.Method(typeof(Status), "KnockDown", new[] { typeof(float) });
        }
    }
}