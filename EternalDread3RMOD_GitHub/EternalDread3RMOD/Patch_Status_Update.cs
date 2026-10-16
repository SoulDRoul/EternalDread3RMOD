using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Status), "Update")]
    public static class Patch_Status_Update
    {
        private static Dictionary<Status, float> dodgeStartTime = new Dictionary<Status, float>();
        private static Dictionary<Status, bool> prevInvincible = new Dictionary<Status, bool>();


        public static void CleanInstance(Status stat)
        {
            dodgeStartTime.Remove(stat);
            prevInvincible.Remove(stat);
        }

        static void Postfix(Status __instance)
        {
            var mp = GlobalVar.mainPlayer;
            if (mp == null || __instance.gameObject != mp) return;

            // --- 闪避无敌 ---
            bool prev;
            InvincibleData.prevDodge.TryGetValue(__instance, out prev);
            if (__instance.dodgeRoll && !prev)
            {
                float sec = Plugin.DodgeInvincibleMs.Value / 1000f;
                InvincibleData.SetInvincible(__instance, sec);
                dodgeStartTime[__instance] = Time.time;
            }

            // 闪避动作 0.5 秒，但只无敌 0.3 秒。0.3 秒后强制关闭 dodgeRoll
            if (__instance.dodgeRoll)
            {
                float start;
                if (dodgeStartTime.TryGetValue(__instance, out start)
                    && Time.time - start >= Plugin.DodgeInvincibleMs.Value / 1000f)
                {
                    __instance.dodgeRoll = false;
                }
            }
            InvincibleData.prevDodge[__instance] = __instance.dodgeRoll;

            // --- 无敌闪烁 ---
            bool nowInvincible = InvincibleData.IsInvincible(__instance);
            bool wasInvincible;
            prevInvincible.TryGetValue(__instance, out wasInvincible);

            if (nowInvincible && !wasInvincible)
            {
                if (Plugin.BlinkEnabled.Value)
                {
                    InvincibleVisual.StartBlink(__instance, __instance);
                }
            }
            prevInvincible[__instance] = nowInvincible;
        }
    }
}