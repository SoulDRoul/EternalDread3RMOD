using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 记录玩家格挡成功的时间戳。
    /// 只做记录，不修改伤害流程。
    /// </summary>
    [HarmonyPatch(typeof(Status), "OnDamage")]
    public static class Patch_Status_OnDamage_GuardCounter
    {
        static void Postfix(Status __instance, string __result)
        {
            if (!Plugin.GuardCounterEnabled.Value) return;
            if (__result != "Blocked") return;
            if (__instance.gameObject != GlobalVar.mainPlayer) return;
            GuardCounterState.OnBlockSuccess();
        }
    }
}