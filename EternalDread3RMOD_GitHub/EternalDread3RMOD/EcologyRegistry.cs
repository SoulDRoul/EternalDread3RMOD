using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 内存中的生态注册表。
    /// 所有 RegionState 常驻内存，读写只在存档/读档时发生。
    /// </summary>
    public static class EcologyRegistry
    {
        private static readonly Dictionary<string, RegionState> _regions
            = new Dictionary<string, RegionState>();

        public static RegionState GetOrCreate(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return null;
            RegionState rs;
            if (_regions.TryGetValue(regionId, out rs)) return rs;
            rs = new RegionState(regionId);
            _regions[regionId] = rs;
            return rs;
        }

        public static IEnumerable<RegionState> AllRegions { get { return _regions.Values; } }
        public static int Count { get { return _regions.Count; } }

        public static void Clear() { _regions.Clear(); }

        public static void ReplaceAll(IEnumerable<RegionState> regions)
        {
            _regions.Clear();
            if (regions == null) return;
            foreach (var r in regions)
            {
                if (r == null || string.IsNullOrEmpty(r.RegionId)) continue;
                _regions[r.RegionId] = r;
            }
        }

        /// <summary>懒更新单个区域到 currentTotalHours。</summary>
        public static void TickRegion(RegionState r, float currentTotalHours)
        {
            if (r == null) return;

            float elapsed = currentTotalHours - r.LastUpdateGameTime;
            if (elapsed <= 0f)
            {
                r.LastUpdateGameTime = currentTotalHours;
                return;
            }

            // 1. 先消耗"暂停恢复"时间
            float delayConsumed = Mathf.Min(elapsed, r.PendingDelayHours);
            r.PendingDelayHours -= delayConsumed;
            if (r.PendingDelayHours < 0f) r.PendingDelayHours = 0f;

            // 2. 剩余时间用于恢复
            float effectiveTime = elapsed - delayConsumed;
            if (effectiveTime > 0f)
            {
                float rate = Plugin.EcologyRecoveryPerHour.Value;
                r.PopulationPercent += rate * effectiveTime;
            }

            // 3. 边界
            float minPop = Plugin.EcologyMinPopulation.Value;
            if (r.PopulationPercent > 100f) r.PopulationPercent = 100f;
            if (r.PopulationPercent < minPop) r.PopulationPercent = minPop;

            // 4. 更新时间戳
            r.LastUpdateGameTime = currentTotalHours;
        }

        /// <summary>懒更新所有区域。</summary>
        public static void TickAll(float currentTotalHours)
        {
            foreach (var r in _regions.Values)
                TickRegion(r, currentTotalHours);
        }

        /// <summary>击杀当前区域的 1 只敌人。</summary>
        public static void OnKill(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return;
            var r = GetOrCreate(regionId);
            if (r == null) return;

            float now = GameTimeTracker.GetTotalGameHours();

            // 先补上流逝（数值更准）
            TickRegion(r, now);

            // 应用击杀效果
            r.PopulationPercent -= Plugin.EcologyKillDropPercent.Value;

            float minPop = Plugin.EcologyMinPopulation.Value;
            if (r.PopulationPercent < minPop) r.PopulationPercent = minPop;
            if (r.PopulationPercent > 100f) r.PopulationPercent = 100f;

            r.PendingDelayHours += Plugin.EcologyDelayPerKillMinutes.Value / 60f;
        }

        /// <summary>玩家死亡：当前区域恢复一定比例（与 PendingDelay 无关）。</summary>
        public static void OnPlayerDeath(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return;
            var r = GetOrCreate(regionId);
            if (r == null) return;

            float now = GameTimeTracker.GetTotalGameHours();

            // 先 Tick，让数值准确
            TickRegion(r, now);

            // 直接恢复（不影响 PendingDelayHours）
            r.PopulationPercent += Plugin.EcologyPlayerDeathRecovery.Value;

            float minPop = Plugin.EcologyMinPopulation.Value;
            if (r.PopulationPercent > 100f) r.PopulationPercent = 100f;
            if (r.PopulationPercent < minPop) r.PopulationPercent = minPop;
        }
    }
}