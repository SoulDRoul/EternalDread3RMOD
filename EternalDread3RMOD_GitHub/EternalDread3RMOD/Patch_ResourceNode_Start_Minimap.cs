using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(ResourceNode), "Start")]
    public static class Patch_ResourceNode_Start_Minimap
    {
        static void Postfix(ResourceNode __instance)
        {
            if (!Plugin.MinimapResourceEnabled.Value) return;
            if (__instance.GetComponent<MinimapIconTag>() != null) return;

            string name = __instance.gameObject.name;
            Color color;
            int order;
            float pct;

            if (name.StartsWith("OreNode"))
            {
                color = MinimapIconManager.ParseColor(
                    Plugin.MinimapOreColor.Value,
                    new Color(0.24f, 0.63f, 0.94f));
                order = MinimapIconManager.MM_RES_ORE_ORDER;
                pct = MinimapIconManager.MM_RES_ORE_PCT;
            }
            else if (name.StartsWith("PlantNode")
                || name.StartsWith("Tree")
                || name.StartsWith("LogNode"))
            {
                color = MinimapIconManager.ParseColor(
                    Plugin.MinimapLogColor.Value,
                    new Color(0.24f, 0.86f, 0.31f));
                order = MinimapIconManager.MM_RES_LOG_ORDER;
                pct = MinimapIconManager.MM_RES_LOG_PCT;
            }
            else
            {
                return;
            }

            float h = MinimapIconManager.MM_RES_HEIGHT / 100f;
            float size = MinimapIconManager.MM_SIZE_BASE * pct / 10000f;

            MinimapIconManager.AttachIcon(
                __instance.gameObject,
                color,
                h,
                size,
                IconShape.Triangle,
                MinimapIconCategory.Resource,
                null,
                order);

            __instance.gameObject.AddComponent<MinimapIconTag>();
        }
    }
}