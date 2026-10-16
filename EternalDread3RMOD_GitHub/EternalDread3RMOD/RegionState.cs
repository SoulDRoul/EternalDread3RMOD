using System;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 单个区域的生态状态。
    /// </summary>
    [Serializable]
    public class RegionState
    {
        public string RegionId;             // 场景名，如 "Grassland1"
        public float PopulationPercent;     // 0~100 种群率
        public float PendingDelayHours;     // 剩余"暂停恢复"时长（游戏小时）
        public float LastUpdateGameTime;    // 上次更新的累计度过小时

        public RegionState()
        {
            RegionId = string.Empty;
            PopulationPercent = 100f;
            PendingDelayHours = 0f;
            LastUpdateGameTime = 0f;
        }

        public RegionState(string id)
        {
            RegionId = id ?? string.Empty;
            PopulationPercent = 100f;
            PendingDelayHours = 0f;
            LastUpdateGameTime = 0f;
        }
    }
}