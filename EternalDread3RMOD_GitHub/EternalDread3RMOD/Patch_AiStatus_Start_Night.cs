using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(AiStatus), "Start")]
    public static class Patch_AiStatus_Start_Night
    {
        static void Postfix(AiStatus __instance)
        {
            if (!Plugin.NightEnemyEnabled.Value) return;
            if (ModSceneHelper.IsTutorial()) return;
            if (__instance.gameObject.tag != "Enemy") return;
            if (GlobalVar.time != TheTimes.Night) return;
            if (__instance.GetComponent<NightEnemyTracker>() != null) return;
            __instance.gameObject.AddComponent<NightEnemyTracker>();

            int lv = GlobalVar.difficultLv;
            int chance = GetChance(lv);

            // ===== 词条赋予 =====
            if (chance > 0)
            {
                int roll = Random.Range(0, 100);
                if (roll < chance)
                {
                    if (__instance.GetComponent<MonRandomAbility>() == null)
                    {
                        __instance.gameObject.AddComponent<MonRandomAbility>();
                        Plugin.DebugLog("[NightEnemy] " + __instance.gameObject.name
                            + " 词条已赋予 (roll=" + roll + "/" + chance + ")");
                    }
                }
            }
        }

        private static int GetChance(int lv)
        {
            switch (lv)
            {
                case 0: return Plugin.NightChanceS0.Value;
                case 1: return Plugin.NightChanceS1.Value;
                case 2: return Plugin.NightChanceS2.Value;
                case 3: return Plugin.NightChanceS3.Value;
                default: return 0;
            }
        }

    }
}