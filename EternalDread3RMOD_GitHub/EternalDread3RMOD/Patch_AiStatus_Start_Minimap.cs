using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(AiStatus), "Start")]
    public static class Patch_AiStatus_Start_Minimap
    {
        static void Postfix(AiStatus __instance)
        {
            if (!Plugin.MinimapIconEnabled.Value) return;
            if (__instance.GetComponent<MinimapIconTag>() != null) return;

            string tag = __instance.gameObject.tag;
            Color color;
            MinimapIconCategory cat;
            int order;
            float pct;
            IconShape shape;   // ★ 新增

            if (tag == "Enemy")
            {
                // 初始用同级颜色，挂载后 UpdateEnemyColor 会立即覆盖
                color = MinimapIconManager.ParseColor(
                    Plugin.MinimapEnemyLevelNearColor.Value,
                    new Color(1f, 0.55f, 0f));
                cat = MinimapIconCategory.Enemy;
                order = MinimapIconManager.MM_ENEMY_NEAR_ORDER;
                pct = MinimapIconManager.MM_ENEMY_NEAR_PCT;
                shape = IconShape.Arrow;      // ★ 敌人用箭头
            }
            else if (tag == "Ally")
            {
                color = MinimapIconManager.ParseColor(
                    Plugin.MinimapIconAllyColor.Value,
                    new Color(0.24f, 0.71f, 1f));
                cat = MinimapIconCategory.Ally;
                order = MinimapIconManager.MM_ALLY_ORDER;
                pct = MinimapIconManager.MM_ALLY_PCT;
                shape = IconShape.Circle;     // ★ 友军保持圆点
            }
            else
            {
                return;
            }

            float h = MinimapIconManager.MM_ICON_HEIGHT / 100f;
            float size = MinimapIconManager.MM_SIZE_BASE * pct / 10000f;

            var status = __instance.GetComponent<Status>();

            MinimapIconManager.AttachIcon(
                __instance.gameObject,
                color,
                h,
                size,
                shape,           // ★ 用变量
                cat,
                status,
                order);

            __instance.gameObject.AddComponent<MinimapIconTag>();
        }
    }
}