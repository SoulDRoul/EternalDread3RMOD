using HarmonyLib;
using UnityEngine.SceneManagement;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Status), "Death")]
    public static class Patch_Status_Death_Ecology
    {
        /// <summary>玩家死亡时间戳，用于放行尸体上的 RespawnMonster。</summary>
        public static float PlayerDeathTime = -999f;

        static void Postfix(Status __instance)
        {
            if (__instance == null) return;

            // ===== 玩家死亡 =====
            if (__instance.gameObject == GlobalVar.mainPlayer)
            {
                PlayerDeathTime = UnityEngine.Time.realtimeSinceStartup;

                // 生态恢复和跳时间都改到 SettingNewPlayer 时执行。
                // 此处只记录场景名，并清理上一次可能残留的状态。
                DeathTimeSkipManager.Clear();
                DeathTimeSkipManager.RecordDeathScene();
                return;
            }

            // ===== 敌人死亡 =====
            if (!Plugin.EcologyEnabled.Value) return;

            var ai = __instance.GetComponent<AiStatus>();
            if (ai == null) return;
            if (ai.aiType != AiTag.Enemy) return;

            string scene2 = SceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(scene2)) return;

            EcologyRegistry.OnKill(scene2);

            if (Plugin.Log != null)
            {
                var r = EcologyRegistry.GetOrCreate(scene2);
                if (r != null)
                    Plugin.DebugLog("[ED3R] Kill in " + scene2
                        + " -> pop=" + r.PopulationPercent.ToString("F1")
                        + "%, delay=" + (r.PendingDelayHours * 60f).ToString("F1") + "min");
            }
        }
    }
}