using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(AiStatus), "Start")]
    public static class Patch_AiStatus_Start_Alert
    {
        static void Postfix(AiStatus __instance)
        {
            if (!Plugin.AlertEnabled.Value) return;
            if (__instance.GetComponent<EnemyAlertTracker>() != null) return;

            __instance.gameObject.AddComponent<EnemyAlertTracker>();
        }
    }
}