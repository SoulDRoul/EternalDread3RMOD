using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    public static class InvincibleData
    {
        // 每个 Status 实例的真无敌结束时间
        public static Dictionary<Status, float> endTime = new Dictionary<Status, float>();
        // 受击无敌上次触发时间，用于 1 秒冷却
        public static Dictionary<Status, float> lastHurtTime = new Dictionary<Status, float>();
        // 上一帧 dodgeRoll 状态，用于检测闪避开始
        public static Dictionary<Status, bool> prevDodge = new Dictionary<Status, bool>();

        public static bool IsInvincible(Status stat)
        {
            float t;
            return endTime.TryGetValue(stat, out t) && Time.time < t;
        }

        public static void SetInvincible(Status stat, float duration)
        {
            endTime[stat] = Time.time + duration;
        }

        public static void ClearInvincible(Status stat)
        {
            endTime[stat] = 0f;
        }
    }
}