using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 格挡反击的全局状态。
    /// LastBlockSuccessTime = 最近一次格挡成功的时间戳。
    /// TriggeredTargets = 本次格挡窗口内已经触发过反击的敌人集合，
    ///                   避免同一敌人被重复触发，同时允许不同敌人各自触发。
    /// </summary>
    public static class GuardCounterState
    {
        public static float LastBlockSuccessTime = -999f;

        public static readonly HashSet<GameObject> TriggeredTargets
            = new HashSet<GameObject>();

        /// <summary>格挡成功时调用：刷新时间戳，清空已触发集合。</summary>
        public static void OnBlockSuccess()
        {
            LastBlockSuccessTime = Time.time;
            TriggeredTargets.Clear();
        }
    }
}