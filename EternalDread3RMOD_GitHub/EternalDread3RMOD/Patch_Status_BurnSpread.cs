using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 燃烧蔓延：燃烧中的单位每秒尝试对半径内其他单位施加燃烧。
    /// 
    /// 走原版 ApplyAbnormalStat(9)，自然遵循：
    ///   - 目标必须有 burnEffect
    ///   - 目标 totalStatus.fire > 50
    ///   - statusResist.burn 抗性判定
    ///   - waterRipple（水中）判定
    ///   - dead / evImmortal / GlobalVar.allFreeze 判定
    /// 
    /// 首次进入燃烧立即传播一次，之后按 BurnSpreadIntervalMs 轮询。
    /// </summary>
    [HarmonyPatch(typeof(Status), "Update")]
    public static class Patch_Status_BurnSpread
    {
        // ============ 硬编码常量：StatusEffects.BurnSpreadIntervalMs ============
        // 原 StatusEffects.BurnSpreadIntervalMs —— 蔓延轮询间隔（毫秒）
        private const int SPREAD_INTERVAL_MS = 1000;

        private static readonly Dictionary<Status, float> nextSpreadTime
            = new Dictionary<Status, float>();

        // ============ 复用的碰撞体缓冲区 ============
        // 避免 TrySpread 每次分配新的 Collider[]。
        // 燃烧蔓延半径通常 1~3 米，128 个碰撞体远超实际需要。
        private static readonly Collider[] _hitBuffer = new Collider[128];

        static void Postfix(Status __instance)
        {
            if (!Plugin.BurnSpreadEnabled.Value) return;

            if (__instance.onBurn <= 0f)
            {
                // 只在字典非空时才尝试移除，避免空字典的无意义哈希计算
                if (nextSpreadTime.Count > 0)
                    nextSpreadTime.Remove(__instance);
                return;
            }

            if (!IsTagAllowed(__instance.gameObject)) return;

            float interval = SPREAD_INTERVAL_MS / 1000f;
            if (interval < 0.5f) interval = 0.5f;

            float next;
            if (!nextSpreadTime.TryGetValue(__instance, out next))
            {
                // 首次进入燃烧：立即传播一次
                TrySpread(__instance);
                nextSpreadTime[__instance] = Time.time + interval;
                return;
            }

            if (Time.time < next) return;

            TrySpread(__instance);
            nextSpreadTime[__instance] = Time.time + interval;
        }

        private static void TrySpread(Status source)
        {
            float radius = Plugin.BurnSpreadRadius.Value;
            if (radius <= 0f) return;

            float dur = Plugin.BurnSpreadDur.Value;
            if (dur <= 0f) return;

            int hitCount;
            try
            {
                hitCount = Physics.OverlapSphereNonAlloc(
                    source.transform.position, radius, _hitBuffer);
            }
            catch
            {
                return;
            }

            if (hitCount <= 0) return;

            for (int i = 0; i < hitCount; i++)
            {
                var h = _hitBuffer[i];
                if (h == null) continue;

                // collider 通常在子物体上，Status 在 root 上
                var target = h.GetComponentInParent<Status>();
                if (target == null) continue;
                if (target == source) continue;
                if (target.dead) continue;
                if (target.onBurn > 0f) continue;   // 已烧着，避免重复
                if (!IsTagAllowed(target.gameObject)) continue;

                target.ApplyAbnormalStat(9, dur);   // 9 = Burn
            }
        }

        // 模式：0=仅敌人，1=敌人+玩家，2=全部
        private static bool IsTagAllowed(GameObject go)
        {
            int mode = Plugin.BurnSpreadMode.Value;
            string t = go.tag;

            if (mode <= 0) return t == "Enemy";
            if (mode == 1) return t == "Enemy" || t == "Player";
            return true;
        }

        public static void CleanInstance(Status stat)
        {
            nextSpreadTime.Remove(stat);
        }
    }
}