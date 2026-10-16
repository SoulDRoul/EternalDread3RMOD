using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 按难度缩放敌人/友军的探测范围，弥补锥形视野削减的感知距离。
    /// 同时作用于 detectRange 和 angryDetectRange。
    /// </summary>
    [HarmonyPatch(typeof(AiStatus), "Start")]
    public static class Patch_DetectRangeScale
    {
        static void Postfix(AiStatus __instance)
        {
            float scale = GetScale(GlobalVar.difficultLv);
            if (Mathf.Abs(scale - 1f) < 0.001f) return;

            __instance.detectRange *= scale;
            __instance.angryDetectRange *= scale;
        }

        private static float GetScale(int lv)
        {
            switch (lv)
            {
                case 0: return Plugin.DetectRangeScaleS0.Value;
                case 1: return Plugin.DetectRangeScaleS1.Value;
                case 2: return Plugin.DetectRangeScaleS2.Value;
                case 3: return Plugin.DetectRangeScaleS3.Value;
                default: return 1f;
            }
        }
    }
}