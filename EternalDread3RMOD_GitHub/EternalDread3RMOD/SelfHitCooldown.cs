using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    internal static class SelfHitCooldown
    {
        private static readonly Dictionary<object, float> _last
            = new Dictionary<object, float>();
        private static float _nextCleanup;

        /// <summary>返回 true 表示允许本次 self-hit</summary>
        public static bool Allow(object key, float cooldownSec)
        {
            if (key == null) return false;

            float last;
            if (_last.TryGetValue(key, out last)
                && Time.time - last < cooldownSec)
                return false;

            _last[key] = Time.time;
            Cleanup();
            return true;
        }

        public static void Remove(object key)
        {
            if (key != null) _last.Remove(key);
        }

        private static void Cleanup()
        {
            if (Time.time < _nextCleanup) return;
            _nextCleanup = Time.time + 30f;

            var dead = new List<object>();
            foreach (var kv in _last)
            {
                var uo = kv.Key as UnityEngine.Object;
                if (uo == null) dead.Add(kv.Key);
            }
            for (int i = 0; i < dead.Count; i++) _last.Remove(dead[i]);
        }
    }
}