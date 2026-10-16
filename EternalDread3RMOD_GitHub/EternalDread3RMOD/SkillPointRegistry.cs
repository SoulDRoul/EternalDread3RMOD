using System.Collections.Generic;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 技能点刷新的内存状态。读写只在场景加载/拾取/存档时发生。
    /// </summary>
    public static class SkillPointRegistry
    {
        private static readonly Dictionary<int, SkillPointRecord> _records
            = new Dictionary<int, SkillPointRecord>();

        private static int _todayRefreshCount = 0;
        private static int _todayDayNumber = -1;

        public static float GetLastPickup(int boolId)
        {
            SkillPointRecord r;
            if (_records.TryGetValue(boolId, out r)) return r.LastPickupGameHours;
            return -1f;
        }

        public static void RecordPickup(int boolId, float gameHours)
        {
            SkillPointRecord r;
            if (!_records.TryGetValue(boolId, out r))
            {
                r = new SkillPointRecord();
                r.BoolId = boolId;
                _records[boolId] = r;
            }
            r.LastPickupGameHours = gameHours;
        }

        public static bool GetIsSpawned(int boolId)
        {
            SkillPointRecord r;
            if (_records.TryGetValue(boolId, out r)) return r.IsSpawned;
            return false;
        }

        public static void SetIsSpawned(int boolId, bool value)
        {
            SkillPointRecord r;
            if (!_records.TryGetValue(boolId, out r))
            {
                r = new SkillPointRecord();
                r.BoolId = boolId;
                r.LastPickupGameHours = -1f;
                _records[boolId] = r;
            }
            r.IsSpawned = value;
        }

        /// <summary>检查今天是否还能刷新。currentDay = floor(gameHours / 24)。</summary>
        public static bool CanRefreshToday(int currentDay, int dailyLimit)
        {
            if (dailyLimit <= 0) return true;
            if (_todayDayNumber != currentDay)
            {
                _todayDayNumber = currentDay;
                _todayRefreshCount = 0;
            }
            return _todayRefreshCount < dailyLimit;
        }

        /// <summary>消耗一次每日刷新配额。</summary>
        public static void ConsumeRefreshSlot(int currentDay, int dailyLimit)
        {
            if (dailyLimit <= 0) return;
            if (_todayDayNumber != currentDay)
            {
                _todayDayNumber = currentDay;
                _todayRefreshCount = 0;
            }
            _todayRefreshCount++;
        }

        public static SkillPointSaveData Snapshot()
        {
            var d = new SkillPointSaveData();
            d.TodayRefreshCount = _todayRefreshCount;
            d.TodayDayNumber = _todayDayNumber;
            foreach (var r in _records.Values)
            {
                d.Records.Add(new SkillPointRecord
                {
                    BoolId = r.BoolId,
                    LastPickupGameHours = r.LastPickupGameHours,
                    IsSpawned = r.IsSpawned
                });
            }
            return d;
        }

        public static void ReplaceAll(SkillPointSaveData data)
        {
            _records.Clear();
            _todayRefreshCount = 0;
            _todayDayNumber = -1;
            if (data == null) return;

            _todayRefreshCount = data.TodayRefreshCount;
            _todayDayNumber = data.TodayDayNumber;

            if (data.Records == null) return;
            foreach (var r in data.Records)
            {
                if (r == null) continue;
                _records[r.BoolId] = r;
            }
        }

        public static void Clear()
        {
            _records.Clear();
            _todayRefreshCount = 0;
            _todayDayNumber = -1;
        }

        public static int GetTodayRefreshCount()
        {
            return _todayRefreshCount;
        }
    }
}