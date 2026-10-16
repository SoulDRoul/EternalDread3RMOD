using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    public static class ComboState
    {
        // ============ 硬编码常量：Combo.HitCombo.HitRecover ============
        // 原 Combo.HitCombo.RecoverWindowSec —— HIT 恢复连杀时间的窗口长度（秒）
        public const float HIT_RECOVER_WINDOW_SEC = 2f;
        // 原 Combo.HitCombo.RecoverMaxPerWindow —— 每个窗口内最多恢复多少秒连杀时间
        public const float HIT_RECOVER_MAX_PER_WINDOW = 1f;

        public static int comboCount = 0;
        public static float comboEndTime = 0f;
        public static int killsSinceLastReward = 0;

        // HIT 连击
        public static int hitCombo = 0;
        public static float lastHitGameTime = -999f;
        public static float recoverWindowStart = -999f;
        public static float recoveredInWindow = 0f;

        // 玩家伤害过的敌人 -> 最后一次伤害时间
        public static Dictionary<Status, float> playerDamagedEnemies
            = new Dictionary<Status, float>();

        public static void RecordPlayerDamage(Status enemy)
        {
            if (enemy == null) return;
            playerDamagedEnemies[enemy] = Time.time;
        }

        public static bool WasDamagedByPlayer(Status enemy, float windowSec)
        {
            if (enemy == null) return false;
            float t;
            if (!playerDamagedEnemies.TryGetValue(enemy, out t)) return false;
            return Time.time - t <= windowSec;
        }

        public static void ClearEnemyRecord(Status enemy)
        {
            if (enemy == null) return;
            playerDamagedEnemies.Remove(enemy);
        }

        // ============ 过期条目清理 ============
        // 解决泄漏：玩家打伤但未击杀的敌人（含逃跑 / 被 Destroy / BreakableEn /
        // 场景切换 / 召唤物）会在字典里永久留下引用。
        // 这里定期清理超过伤害窗口的条目。
        private static float _lastCleanupTime = -999f;
        private const float CLEANUP_INTERVAL_SEC = 30f;

        /// <summary>
        /// 清理超过伤害窗口的记录（含 Unity 已销毁的 null 引用）。
        /// 建议在 ComboUI.Update 里调用，内部自身 30 秒节流。
        /// </summary>
        public static void CleanupStaleDamageRecords(float windowSec)
        {
            if (Time.time - _lastCleanupTime < CLEANUP_INTERVAL_SEC) return;
            _lastCleanupTime = Time.time;

            if (playerDamagedEnemies.Count == 0) return;

            // 阈值加 50% 余量，避免清理掉边界附近的合法记录
            float threshold = windowSec * 1.5f;
            if (threshold < 15f) threshold = 15f;

            // 不能在遍历中直接 Remove，先收集
            List<Status> toRemove = null;
            foreach (var kv in playerDamagedEnemies)
            {
                // Unity 对象被销毁后 == null 返回 true
                if (kv.Key == null || Time.time - kv.Value > threshold)
                {
                    if (toRemove == null) toRemove = new List<Status>();
                    toRemove.Add(kv.Key);
                }
            }

            if (toRemove != null)
            {
                for (int i = 0; i < toRemove.Count; i++)
                    playerDamagedEnemies.Remove(toRemove[i]);

                Plugin.DebugLog("[ComboState] Cleaned " + toRemove.Count
                    + " stale damage records, remaining=" + playerDamagedEnemies.Count);
            }
        }

        // 累积的游戏内时间（秒）
        public static float gameTime = 0f;

        public static void UpdateGameTime()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            float normal = Patch_GlobalClock_Awake.NormalDuration;
            float cur = Patch_GlobalClock_Update.CurrentDuration;

            if (normal <= 0f) normal = cur;   // 还没捕获到，就用当前值
            if (cur <= 0f) cur = normal;
            if (normal <= 0f) return;         // 都没值，跳过

            // 正常时 cur == normal → 系数 = 1（1:1）
            // 睡觉时 cur == 0.15 → 系数 = normal/0.15
            float factor = normal / cur;
            gameTime += dt * factor;
        }

        public static void ResetCombo()
        {
            comboCount = 0;
            comboEndTime = 0f;
            killsSinceLastReward = 0;

            // HIT 也随之清空
            hitCombo = 0;
            lastHitGameTime = -999f;
            recoverWindowStart = -999f;
            recoveredInWindow = 0f;
        }

        // 命中敌人时调用。返回 true 表示触发了 HIT 体力奖励
        public static bool OnHit()
        {
            if (!Plugin.HitComboEnabled.Value) return false;

            hitCombo++;
            lastHitGameTime = gameTime;

            // 尝试恢复连杀时间
            TryRecoverFromHit();

            // 检查 HIT 奖励
            if (Plugin.HitRewardEvery.Value > 0
                && hitCombo % Plugin.HitRewardEvery.Value == 0)
            {
                return true;
            }
            return false;
        }

        // HIT 恢复连杀时间（限流：每 N 秒最多恢复 M 秒）
        private static void TryRecoverFromHit()
        {
            if (comboCount <= 0) return;

            float windowSec = HIT_RECOVER_WINDOW_SEC;
            float maxPerWindow = HIT_RECOVER_MAX_PER_WINDOW;
            if (windowSec <= 0f || maxPerWindow <= 0f) return;

            // 计算当前窗口起点
            float currentWindowStart = gameTime - (gameTime % windowSec);

            if (currentWindowStart != recoverWindowStart)
            {
                recoverWindowStart = currentWindowStart;
                recoveredInWindow = 0f;
            }

            if (recoveredInWindow >= maxPerWindow) return;

            float canRecover = maxPerWindow - recoveredInWindow;
            if (canRecover <= 0f) return;

            comboEndTime = CalcRecoverEndTime(canRecover);
            recoveredInWindow += canRecover;

            Plugin.DebugLog("[HitCombo] 命中 x" + hitCombo
                + "，恢复 " + canRecover + " 秒");
        }

        // 每帧调用，检查 HIT 是否超时
        public static void TickHit()
        {
            if (hitCombo <= 0) return;

            float windowSec = Plugin.HitWindowMs.Value / 1000f;
            if (gameTime - lastHitGameTime > windowSec)
            {
                hitCombo = 0;
            }
        }

        // 返回 HIT 窗口剩余比例（1.0 = 刚开始，0.0 = 即将超时），用于透明度
        public static float GetHitWindowRatio()
        {
            if (hitCombo <= 0) return 0f;

            float windowSec = Plugin.HitWindowMs.Value / 1000f;
            if (windowSec <= 0f) return 0f;

            float elapsed = gameTime - lastHitGameTime;
            float remain = windowSec - elapsed;
            if (remain <= 0f) return 0f;
            if (remain >= windowSec) return 1f;
            return remain / windowSec;
        }

        // 返回当前剩余计时（秒）
        public static float GetRemainingTime()
        {
            float remain = comboEndTime - gameTime;
            return remain > 0f ? remain : 0f;
        }

        // 返回窗口最大时长（秒）
        public static float GetMaxWindowTime()
        {
            return Plugin.ComboWindowMs.Value / 1000f;
        }

        // 返回扣减后的新 comboEndTime（不小于 gameTime）
        public static float CalcReduceEndTime(float reduceSec)
        {
            float newEnd = comboEndTime - reduceSec;
            if (newEnd < gameTime) newEnd = gameTime;
            return newEnd;
        }

        // 返回恢复后的新 comboEndTime（不超过最大窗口）
        public static float CalcRecoverEndTime(float recoverSec)
        {
            float maxEnd = gameTime + GetMaxWindowTime();
            float newEnd = comboEndTime + recoverSec;
            if (newEnd > maxEnd) newEnd = maxEnd;
            return newEnd;
        }
    }
}