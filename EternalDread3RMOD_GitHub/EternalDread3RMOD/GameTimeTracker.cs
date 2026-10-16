using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 玩家累计度过的游戏时间追踪器。
    /// 
    /// 语义：
    ///   - _totalGameHours 是"玩家旅程时长"，与 GlobalVar.clock（昼夜显示）解耦。
    ///   - 由 GlobalClock.TimeFwd / CrossHour 驱动，逐分钟/逐小时累加。
    ///   - 新游戏从 0 开始。
    ///   - 抛弃"天数"概念，避免与 clock 产生认知冲突。
    ///   - OtherEventFunction.SetTime 只改 clock，不影响此值，无需 patch。
    /// </summary>
    public static class GameTimeTracker
    {
        private static float _totalGameHours = 0f;

        /// <summary>会话边界重置（新游戏 / 回标题）。</summary>
        public static void Reset()
        {
            _totalGameHours = 0f;
            if (Plugin.Log != null)
                Plugin.DebugLog("[ED3R] TimeTracker reset");
        }

        /// <summary>由 GlobalClock.TimeFwd 调用，推进 1 游戏分钟。</summary>
        public static void AdvanceMinutes(float m)
        {
            _totalGameHours += m / 60f;
        }

        /// <summary>由 GlobalClock.CrossHour 调用，推进 N 游戏小时。</summary>
        public static void AdvanceHours(float h)
        {
            _totalGameHours += h;
        }

        /// <summary>累计度过小时数（浮点，含分钟精度）。</summary>
        public static float GetTotalGameHours()
        {
            return _totalGameHours;
        }

        /// <summary>累计度过小时数（取整），UI 显示用。</summary>
        public static int GetElapsedHours()
        {
            return Mathf.FloorToInt(_totalGameHours);
        }

        // ===== 持久化 / 共通模式预留（第五步用）=====

        public static float ExportTotalHours()
        {
            return _totalGameHours;
        }

        public static void ImportTotalHours(float v)
        {
            _totalGameHours = v;
            if (Plugin.Log != null)
                Plugin.DebugLog("[ED3R] TimeTracker import: totalHours=" + v.ToString("F2"));
        }
    }
}