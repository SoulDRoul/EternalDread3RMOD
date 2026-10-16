using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(ResourcePickup), "Start")]
    public static class Patch_ResourcePickup_Start_Minimap
    {
        static void Postfix(ResourcePickup __instance)
        {
            if (!Plugin.MinimapResourceEnabled.Value) return;
            if (__instance.GetComponent<MinimapIconTag>() != null) return;

            string name = __instance.gameObject.name;
            // 蘑菇、草药、野果 → 倒三角、薄荷绿
            Color color = MinimapIconManager.ParseColor(
                Plugin.MinimapPlantColor.Value,
                new Color(0.47f, 0.94f, 0.78f));

            float h = MinimapIconManager.MM_RES_HEIGHT / 100f;
            float size = MinimapIconManager.MM_SIZE_BASE
                * MinimapIconManager.MM_RES_PLANT_PCT / 10000f;
            int order = MinimapIconManager.MM_RES_PLANT_ORDER;

            MinimapIconManager.AttachIcon(
                __instance.gameObject,
                color,
                h,
                size,
                IconShape.TriangleDown,
                MinimapIconCategory.Resource,
                null,
                order);

            __instance.gameObject.AddComponent<MinimapIconTag>();

            Plugin.DebugLog("[ResMinimap] " + name + " → Plant");
        }
    }
}