using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(PlayerInputController), "Start")]
    public static class Patch_PlayerInputController_Stamina
    {
        static void Postfix(PlayerInputController __instance)
        {
            // 用组件去重，每个实例只处理一次
            if (__instance.GetComponent<SpeedAppliedTracker>() != null) return;
            __instance.gameObject.AddComponent<SpeedAppliedTracker>();

            // ===== 1. 体力消耗倍率 =====
            var field = AccessTools.Field(typeof(PlayerInputController), "useStamina");
            if (field == null)
            {
                Plugin.Log.LogError("[ED3R] Cannot find field useStamina!");
            }
            else
            {
                float oldVal = (float)field.GetValue(__instance);
                int pct = Plugin.SprintCostPercent.Value;

                if (pct <= 0)
                {
                    field.SetValue(__instance, 999999f);
                }
                else
                {
                    field.SetValue(__instance, oldVal * 100f / pct);
                }
            }

            // ===== 2. 走路/冲刺速度倍率 =====
            float oldWalk = __instance.walkSpeed;
            float newWalk = oldWalk * Plugin.WalkSpeedMul.Value / 100f;
            __instance.walkSpeed = newWalk;

            float oldSprint = __instance.sprintSpeed;
            float newSprint = oldSprint * Plugin.SprintSpeedMul.Value / 100f;
            __instance.sprintSpeed = newSprint;

            Plugin.DebugLog("[Speed] (" + __instance.gameObject.name + ")"
                + " walk " + oldWalk + " -> " + newWalk
                + ", sprint " + oldSprint + " -> " + newSprint);
        }
    }
}