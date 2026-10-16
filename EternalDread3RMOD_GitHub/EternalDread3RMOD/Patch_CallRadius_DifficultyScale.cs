using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 按难度缩放敌人的呼叫援军范围（callRadius）。
    /// 影响 4 个有 callNearbyAlly 字段的类：
    ///   EnemyBasic / EnemySkill / FlyingEnemy / AllyBasic。
    /// 与原版 Patch_DetectRangeScale 风格一致。
    /// </summary>
    internal static class CallRadiusScaleHelper
    {
        public static void Apply(CallingAlly caller)
        {
            if (caller == null) return;
            float scale = GetScale(GlobalVar.difficultLv);
            if (Mathf.Abs(scale - 1f) < 0.001f) return;
            caller.callRadius *= scale;
        }

        private static float GetScale(int lv)
        {
            switch (lv)
            {
                case 0: return Plugin.CallRadiusScaleS0.Value;
                case 1: return Plugin.CallRadiusScaleS1.Value;
                case 2: return Plugin.CallRadiusScaleS2.Value;
                case 3: return Plugin.CallRadiusScaleS3.Value;
                default: return 1f;
            }
        }
    }

    [HarmonyPatch(typeof(EnemyBasic), "Start")]
    public static class Patch_EnemyBasic_CallRadius
    {
        static void Postfix(EnemyBasic __instance)
        {
            CallRadiusScaleHelper.Apply(__instance.callNearbyAlly);
        }
    }

    [HarmonyPatch(typeof(EnemySkill), "Start")]
    public static class Patch_EnemySkill_CallRadius
    {
        static void Postfix(EnemySkill __instance)
        {
            CallRadiusScaleHelper.Apply(__instance.callNearbyAlly);
        }
    }

    [HarmonyPatch(typeof(FlyingEnemy), "Start")]
    public static class Patch_FlyingEnemy_CallRadius
    {
        static void Postfix(FlyingEnemy __instance)
        {
            CallRadiusScaleHelper.Apply(__instance.callNearbyAlly);
        }
    }

    [HarmonyPatch(typeof(AllyBasic), "Start")]
    public static class Patch_AllyBasic_CallRadius
    {
        static void Postfix(AllyBasic __instance)
        {
            CallRadiusScaleHelper.Apply(__instance.callNearbyAlly);
        }
    }
}