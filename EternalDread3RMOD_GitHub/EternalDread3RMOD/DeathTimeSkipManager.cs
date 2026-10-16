using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 死亡复活时的统一处理器：
    ///   1. 应用 EcologyPlayerDeathRecovery（原在死亡瞬间，现移到复活时）
    ///   2. 应用 DeathTimeSkipHours（跳时间 + 生态 tick + UI 提示）
    /// 触发链：
    ///   Status.Death (Postfix)  → RecordDeathScene
    ///   GameOver.Start (Postfix) → MarkGameOver
    ///   SettingNewPlayer (Postfix) → TryApply
    /// </summary>
    public static class DeathTimeSkipManager
    {
        public static bool PendingFromGameOver = false;
        public static string DeathScene = null;

        private static float _markRealTime = 0f;
        private const float TimeoutSec = 30f;

        public static void MarkGameOver()
        {
            if (ModSceneHelper.IsTutorial()) return;

            PendingFromGameOver = true;
            _markRealTime = Time.realtimeSinceStartup;
        }

        public static void Clear()
        {
            PendingFromGameOver = false;
            DeathScene = null;
        }

        /// <summary>死亡时记录场景名，等复活时应用。</summary>
        public static void RecordDeathScene()
        {
            if (!Plugin.EcologyEnabled.Value) return;
            string scene = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(scene)) return;
            DeathScene = scene;
        }

        /// <summary>复活时统一处理：死亡恢复 + 跳时间。</summary>
        public static void TryApply()
        {
            if (!PendingFromGameOver) return;

            if (Time.realtimeSinceStartup - _markRealTime > TimeoutSec)
            {
                Clear();
                return;
            }

            PendingFromGameOver = false;

            // ===== 1. 死亡恢复百分比 =====
            if (Plugin.EcologyEnabled.Value
                && !string.IsNullOrEmpty(DeathScene)
                && Plugin.EcologyPlayerDeathRecovery.Value > 0f)
            {
                EcologyRegistry.OnPlayerDeath(DeathScene);

                if (Plugin.Log != null)
                {
                    var r = EcologyRegistry.GetOrCreate(DeathScene);
                    if (r != null)
                        Plugin.DebugLog("[ED3R] Player death recovery in " + DeathScene
                            + " -> pop=" + r.PopulationPercent.ToString("F1") + "%");
                }
            }

            // ===== 2. 时间跳跃 + 生态 tick + UI =====
            float hours = Plugin.DeathTimeSkipHours.Value;
            if (hours > 0f)
            {
                GameTimeJump.JumpTime(hours);
                EcologyRegistry.TickAll(GameTimeTracker.GetTotalGameHours());
                DeathTimeSkipUI.Show(hours);

                if (Plugin.Log != null)
                    Plugin.DebugLog("[ED3R] Death time skip +" + hours.ToString("F2") + "h");
            }

            DeathScene = null;
        }
    }
}