using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 噪音警觉的全局注册表。
    /// 追踪所有挂有 AiStatus 的单位，用于统计周围 AI 数量与吸引范围遍历。
    /// 同时维护"共享冷却"（位置 + 时间）。
    /// </summary>
    public static class NoiseRegistry
    {
        public static readonly List<AiStatus> ActiveAIs = new List<AiStatus>();

        public static float LastNoiseTime = -999f;
        public static Vector3 LastNoisePosition = Vector3.zero;

        public static void Register(AiStatus ai)
        {
            if (ai == null) return;
            if (ActiveAIs.Contains(ai)) return;
            ActiveAIs.Add(ai);
        }

        public static void Unregister(AiStatus ai)
        {
            if (ai == null) return;
            ActiveAIs.Remove(ai);
        }

        /// <summary>
        /// 判断某个位置是否处于共享冷却内。
        /// 冷却范围内的痛叫共享同一个时间戳。
        /// </summary>
        public static bool IsInSharedCooldown(Vector3 pos, float cooldownSec, float radius)
        {
            if (Time.time - LastNoiseTime >= cooldownSec) return false;
            float dx = pos.x - LastNoisePosition.x;
            float dz = pos.z - LastNoisePosition.z;
            if (dx * dx + dz * dz > radius * radius) return false;
            return true;
        }

        public static void MarkNoise(Vector3 pos)
        {
            LastNoiseTime = Time.time;
            LastNoisePosition = pos;
        }
    }
}