using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 记录子弹生成时间，用于"出膛保护期"判定。
    /// 用 Dictionary + 定期清理，避免对象泄漏。
    /// </summary>
    public static class BulletSpawnTimeTracker
    {
        private static readonly Dictionary<BulletStatus, float> _spawnTimes
            = new Dictionary<BulletStatus, float>();
        private static float _nextCleanup;

        public static void Record(BulletStatus bs)
        {
            if (bs == null) return;
            _spawnTimes[bs] = Time.time;
            Cleanup();
        }

        public static float Get(BulletStatus bs)
        {
            if (bs == null) return 0f;
            float t;
            if (_spawnTimes.TryGetValue(bs, out t)) return t;
            return 0f;
        }

        private static void Cleanup()
        {
            if (Time.time < _nextCleanup) return;
            _nextCleanup = Time.time + 30f;

            var dead = new List<BulletStatus>();
            foreach (var kv in _spawnTimes)
            {
                if (kv.Key == null) dead.Add(kv.Key);
            }
            for (int i = 0; i < dead.Count; i++)
                _spawnTimes.Remove(dead[i]);
        }
    }

    [HarmonyPatch(typeof(BulletStatus), "Awake")]
    public static class Patch_BulletStatus_Awake_RecordSpawn
    {
        static void Postfix(BulletStatus __instance)
        {
            BulletSpawnTimeTracker.Record(__instance);
        }
    }
}