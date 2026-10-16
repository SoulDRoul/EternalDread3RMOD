using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(AddSkillPointEv), "Start")]
    public static class Patch_AddSkillPointEv_Start_Minimap
    {
        static void Postfix(AddSkillPointEv __instance)
        {
            if (!Plugin.MinimapSkillPointEnabled.Value) return;
            if (__instance.GetComponent<MinimapIconTag>() != null) return;

            var color = MinimapIconManager.ParseColor(
                Plugin.MinimapSkillPointColor.Value,
                Color.white);
            float h = MinimapIconManager.MM_SKILLPOINT_HEIGHT / 100f;
            float size = MinimapIconManager.MM_SIZE_BASE
                * MinimapIconManager.MM_SKILLPOINT_PCT / 10000f;
            int order = MinimapIconManager.MM_SKILLPOINT_ORDER;

            MinimapIconManager.AttachIcon(
                __instance.gameObject,
                color,
                h,
                size,
                IconShape.Diamond,
                MinimapIconCategory.SkillPoint,
                null,
                order);

            __instance.gameObject.AddComponent<MinimapIconTag>();
        }
    }
}