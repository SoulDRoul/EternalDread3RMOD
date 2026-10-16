using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(AiStatus), "SetTag")]
    public static class Patch_AiStatus_SetTag_EcologySpawn
    {
        static void Postfix(AiStatus __instance)
        {
            if (__instance == null) return;

            // 通用 bypass（教程 / 过场 / 死亡窗口）
            if (EcologySpawnGuard.ShouldBypass()) return;

            if (!Plugin.EcologyEnabled.Value) return;
            if (__instance.aiType != AiTag.Enemy) return;

            // ★ 新增：本场景是否启用生态（无小地图场景直接不过滤）
            if (!EcologySceneState.IsEcologyEnabledForCurrentScene()) return;

            string region = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(region)) return;

            var r = EcologyRegistry.GetOrCreate(region);
            if (r == null) return;

            float roll = Random.Range(0f, 100f);
            if (roll < r.PopulationPercent) return;   // 保留

            // 命中过滤候选：先尝试立即判定任务上下文
            Vector3 pos = __instance.transform.position;
            bool? taskCtx = EcologyQuestBypassHelper.TryIsTaskContext(pos);

            if (taskCtx == true)
            {
                return;   // 任务上下文，放行
            }

            if (taskCtx == false)
            {
                var marker = __instance.gameObject.AddComponent<EcologySpawnFilter>();
                marker.ShouldDestroy = true;
                return;
            }

            // taskCtx == null：MinimapCamera 未就绪，挂延迟组件（最多等 10 秒）
            var delay = __instance.gameObject.AddComponent<EcologySpawnSetTagDelay>();
            delay.Init(10f);
        }
    }
}