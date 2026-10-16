using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 瞬间跳过 N 游戏小时。
    /// 
    /// 职责：
    ///   1. 精确推进 GlobalVar.clock（含分钟进位）
    ///   2. 检测跨天 → 反射调 GlobalClock.ResetResources
    ///   3. 同步 GlobalVar.time（昼夜状态，含 DayNight.fading 触发）
    ///   4. 累加 GameTimeTracker
    ///   5. 输出日志
    /// 
    /// 不做：
    ///   - 不 patch Teleporter（第三步）
    ///   - 不改天气（决策 W-1）
    ///   - 不做睡觉加速（第四步）
    /// </summary>
    public static class GameTimeJump
    {
        private static MethodInfo _resetResourcesMethod;
        private static bool _resetResourcesMethodSearched = false;
        private static GlobalClock _currentClockInstance = null;

        /// <summary>
        /// 瞬间跳过 N 游戏小时。hours 必须 > 0。
        /// </summary>
        public static void JumpTime(float hours)
        {
            if (hours <= 0f) return;

            float before = GlobalVar.clock.x * 60f + GlobalVar.clock.y;
            float after = before + hours * 60f;

            int beforeDays = Mathf.FloorToInt(before / 1440f);
            int afterDays = Mathf.FloorToInt(after / 1440f);
            int daysCrossed = afterDays - beforeDays;

            float dayMinutes = after - afterDays * 1440f;
            int newHour = Mathf.FloorToInt(dayMinutes / 60f);
            int newMinute = Mathf.FloorToInt(dayMinutes - newHour * 60f);

            GlobalVar.clock.x = newHour;
            GlobalVar.clock.y = newMinute;

            if (daysCrossed >= 1)
            {
                TryResetResources();
            }

            RefreshDayTime();

            GameTimeTracker.AdvanceHours(hours);

            if (Plugin.Log != null)
            {
                Plugin.DebugLog("[ED3R] JumpTime +" + hours.ToString("F2")
                    + "h -> " + newHour + ":" + newMinute.ToString("D2")
                    + ", daysCrossed=" + daysCrossed);
            }
        }

        // ===== 待处理跳跃（由 Teleporter / WaypointWarp 设置，由新场景 GlobalClock.Awake 消费）=====

        private static float _pendingJumpHours = 0f;
        private static float _pendingSetRealTime = 0f;
        private const float PendingTimeoutSec = 10f;

        /// <summary>记录待处理的时间跳跃。由 Teleporter / WaypointWarp 的 ChangeMap Prefix 调用。</summary>
        public static void SetPending(float hours)
        {
            if (hours <= 0f) return;
            _pendingJumpHours = hours;
            _pendingSetRealTime = Time.realtimeSinceStartup;
        }

        /// <summary>消费待处理的跳跃。由新场景 GlobalClock.Awake Prefix 调用。
        /// 超过 PendingTimeoutSec 秒的 pending 视为过期（防止残留误触发读档等）。</summary>
        public static void TryConsumePending(GlobalClock clockInstance)
        {
            _currentClockInstance = clockInstance;
            TryConsumePendingInternal();
            _currentClockInstance = null;
        }

        private static void TryConsumePendingInternal()
        {
            if (_pendingJumpHours <= 0f) return;

            if (Time.realtimeSinceStartup - _pendingSetRealTime > PendingTimeoutSec)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] Pending jump expired: " + _pendingJumpHours.ToString("F2") + "h");
                _pendingJumpHours = 0f;
                return;
            }

            float h = _pendingJumpHours;
            _pendingJumpHours = 0f;
            JumpTime(h);
        }

        /// <summary>
        /// 反射调用 GlobalClock.ResetResources（private）。
        /// 失败时输出警告日志，不中断流程。
        /// </summary>
        private static void TryResetResources()
        {
            if (!_resetResourcesMethodSearched)
            {
                _resetResourcesMethodSearched = true;
                _resetResourcesMethod = AccessTools.Method(typeof(GlobalClock), "ResetResources");
                if (_resetResourcesMethod == null && Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] ResetResources method not found, will skip");
            }

            if (_resetResourcesMethod == null) return;

            // 优先用 Awake 传入的实例（Prefix 时对象名还没设好，Find 会失败）
            GlobalClock clock = _currentClockInstance;
            if (clock == null)
            {
                var go = GameObject.Find("GlobalClock");
                if (go != null) clock = go.GetComponent<GlobalClock>();
            }

            if (clock == null)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] GlobalClock not found, skip ResetResources");
                return;
            }

            try
            {
                _resetResourcesMethod.Invoke(clock, null);
            }
            catch (Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] ResetResources invoke failed: " + e.Message);
            }
        }

        /// <summary>
        /// 同步 GlobalVar.time，逻辑对齐原版 GlobalClock.TimeFwd / CrossHour。
        /// 时段变化时触发 DayNight.fading = true。
        /// </summary>
        private static void RefreshDayTime()
        {
            float h = GlobalVar.clock.x;
            float m = GlobalVar.clock.y;

            if (h >= 6f && h < 9f)
            {
                if (GlobalVar.time == TheTimes.Night) DayNight.fading = true;
                GlobalVar.time = TheTimes.Morning;
            }
            if (h >= 9f && h < 16f)
            {
                if (GlobalVar.time == TheTimes.Morning) DayNight.fading = true;
                GlobalVar.time = TheTimes.Day;
            }
            if ((h >= 16f && h < 18f) || (h == 18f && m < 30f))
            {
                if (GlobalVar.time == TheTimes.Day) DayNight.fading = true;
                GlobalVar.time = TheTimes.Dusk;
            }
            if ((h == 18f && m >= 30f) || h >= 19f || h < 6f)
            {
                if (GlobalVar.time == TheTimes.Dusk) DayNight.fading = true;
                GlobalVar.time = TheTimes.Night;
            }
        }

        /// <summary>外部调用：同步 GlobalVar.time 到当前 clock。</summary>
        public static void RefreshDayTimePublic()
        {
            RefreshDayTime();
        }
    }
}