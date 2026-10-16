using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    public static class InvincibleVisual
    {
        // ============ 硬编码常量：Invincible.BlinkIntervalMs ============
        // 原 Invincible.BlinkIntervalMs —— 无敌闪烁间隔（毫秒）
        private const int BLINK_INTERVAL_MS = 20;

        public static Dictionary<Status, bool> blinking = new Dictionary<Status, bool>();

        public static void StartBlink(Status stat, MonoBehaviour runner)
        {
            if (!Plugin.BlinkEnabled.Value) return;
            if (blinking.ContainsKey(stat) && blinking[stat]) return;
            blinking[stat] = true;
            runner.StartCoroutine(BlinkCoroutine(stat));
        }

        private static IEnumerator BlinkCoroutine(Status stat)
        {
            float blinkInterval = BLINK_INTERVAL_MS / 1000f;
            List<Renderer> lastOnList = new List<Renderer>();

            while (InvincibleData.IsInvincible(stat))
            {
                lastOnList.Clear();
                var all = stat.GetComponentsInChildren<Renderer>(true);
                foreach (var r in all)
                {
                    if (r != null && r.enabled)
                    {
                        lastOnList.Add(r);
                        r.enabled = false;
                    }
                }
                yield return new WaitForSecondsRealtime(blinkInterval);

                foreach (var r in lastOnList)
                {
                    if (r != null) r.enabled = true;
                }
                yield return new WaitForSecondsRealtime(blinkInterval);
            }

            // 如果循环正好停在关闭帧，这里恢复
            foreach (var r in lastOnList)
            {
                if (r != null) r.enabled = true;
            }

            // ★ 关键：协程结束，重置状态，允许下一次触发
            blinking[stat] = false;
        }
    }
}