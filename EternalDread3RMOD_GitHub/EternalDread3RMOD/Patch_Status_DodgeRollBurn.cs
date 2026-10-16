using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 玩家翻滚一次减 N 层燃烧。原版减 1 层，本补丁补减 (N-1) 层。
    /// 用 dodgeRoll 上升沿检测，避免 Patch 协程。
    /// </summary>
    [HarmonyPatch(typeof(Status), "Update")]
    public static class Patch_Status_DodgeRollBurn
    {
        private static Dictionary<Status, bool> _prevDodge
            = new Dictionary<Status, bool>();

        static void Postfix(Status __instance)
        {
            if (!Plugin.DodgeRollBurnReduceEnabled.Value) return;
            if (__instance.gameObject != GlobalVar.mainPlayer) return;

            bool prev;
            _prevDodge.TryGetValue(__instance, out prev);
            bool cur = __instance.dodgeRoll;
            _prevDodge[__instance] = cur;

            // 只在上升沿处理（翻滚开始那一帧）
            if (!cur || prev) return;
            if (__instance.onBurn <= 0f) return;

            int extra = Plugin.DodgeRollBurnReduceTicks.Value - 1;
            if (extra <= 0) return;

            __instance.onBurn -= extra;
            if (__instance.onBurn <= 0f)
            {
                __instance.onBurn = 0f;
                __instance.CancelBurn();
            }
        }

        public static void CleanInstance(Status stat)
        {
            _prevDodge.Remove(stat);
        }
    }
}