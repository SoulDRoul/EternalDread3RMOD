using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 睡眠加速档位控制。
    /// 3 档：1x（暂停加速）/ 20x（原版）/ 60x。
    /// 默认档位 20x，每次入睡重置。
    /// A 变慢，D 变快。
    /// 
    /// 【重要】绑定"活跃 SleepTime 实例"，避免场景里多个 SleepTime 互相干扰。
    /// </summary>
    public static class SleepSpeedController
    {
        private static readonly float[] Durations = { 3.0f, 0.15f, 0.05f };
        private static readonly string[] Labels = { "1x", "20x", "60x" };
        private const int DefaultIndex = 1;

        private static int _currentIndex = DefaultIndex;
        private static SleepTime _activeInstance = null;

        // ============ GlobalClock 缓存 ============
        // 原 ApplyDuration 每次都 GameObject.Find("GlobalClock")（全场景扫描）。
        // 睡眠时玩家可能连按 A/D 调档，每次都会触发。
        private static GlobalClock _cachedClock;

        public static bool IsSleeping { get { return _activeInstance != null; } }
        public static bool IsActive(SleepTime instance) { return _activeInstance == instance; }
        public static string CurrentLabel { get { return Labels[_currentIndex]; } }

        public static void OnSleepStart(SleepTime instance)
        {
            if (!Plugin.SleepSpeedControlEnabled.Value) return;
            if (instance == null) return;
            if (_activeInstance == instance) return;

            _activeInstance = instance;
            _currentIndex = DefaultIndex;
            DodgeTapState.lockedSprint = false;
            ApplyDuration();
            if (Plugin.Log != null)
                Plugin.DebugLog("[ED3R] Sleep start on " + instance.gameObject.name
                    + ", speed=" + Labels[_currentIndex]);
        }

        public static void OnSleepEnd(SleepTime instance)
        {
            if (_activeInstance != instance) return;
            _activeInstance = null;
            DodgeTapState.lockedSprint = false;
            if (Plugin.Log != null)
                Plugin.DebugLog("[ED3R] Sleep end");
        }

        public static void SetSleeping(bool sleeping, SleepTime instance)
        {
            if (sleeping)
            {
                if (_activeInstance == null) OnSleepStart(instance);
            }
            else
            {
                if (_activeInstance == instance) OnSleepEnd(instance);
            }
        }

        public static bool ShiftDown()
        {
            if (_currentIndex <= 0) return false;
            _currentIndex--;
            ApplyDuration();
            return true;
        }

        public static bool ShiftUp()
        {
            if (_currentIndex >= Durations.Length - 1) return false;
            _currentIndex++;
            ApplyDuration();
            return true;
        }

        private static void ApplyDuration()
        {
            if (_cachedClock == null)
            {
                var go = GameObject.Find("GlobalClock");
                if (go == null) return;
                _cachedClock = go.GetComponent<GlobalClock>();
                if (_cachedClock == null) return;
            }
            _cachedClock.duration = Durations[_currentIndex];
        }
    }
}