using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 按难度缩放敌人愤怒持续时长（angryDuration）。
    /// 高难度下敌人愤怒更持久，不轻易放弃追击。
    /// 
    /// 注：angryDetectRange 已在 Patch_DetectRangeScale 里处理，
    ///    这里只处理 angryDuration。
    /// </summary>
    [HarmonyPatch(typeof(AiStatus), "Start")]
    public static class Patch_AiStatus_Start_AngryDuration
    {
        static void Postfix(AiStatus __instance)
        {
            float scale = GetScale(GlobalVar.difficultLv);
            if (Mathf.Abs(scale - 1f) < 0.001f) return;
            __instance.angryDuration *= scale;
        }

        private static float GetScale(int lv)
        {
            switch (lv)
            {
                case 0: return Plugin.AngryDurationScaleS0.Value;
                case 1: return Plugin.AngryDurationScaleS1.Value;
                case 2: return Plugin.AngryDurationScaleS2.Value;
                case 3: return Plugin.AngryDurationScaleS3.Value;
                default: return 1f;
            }
        }
    }
}