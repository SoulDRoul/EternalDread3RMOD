using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 物品栏 / 装备栏的页数文字 UI 宽度修复。
    ///
    /// 原版 pageText：rect 30x30，fontSize 22，hOverflow=Wrap。
    /// 3 位以上数字（页数 ≥ 100）会被裁切。
    /// 扩宽度到 80，足够显示 6 位数（即 100 万格 / 每页数）。
    ///
    /// 同时把 raycastTarget 关掉，避免扩宽后的 rect 挡住翻页按钮。
    /// </summary>
    [HarmonyPatch(typeof(InventoryUiCanvas), "Start")]
    public static class Patch_InventoryUi_PageTextWidth
    {
        private const float TargetWidth = 80f;

        static void Postfix(InventoryUiCanvas __instance)
        {
            try
            {
                FixPageText(__instance.pageText);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] PageText width patch error: " + e);
            }
        }

        private static void FixPageText(Text t)
        {
            if (t == null) return;

            // 关键：关掉 raycastTarget，让点击穿透到下层翻页按钮
            // 即使 rect 不变宽也建议关掉（原版可能也是 true，只是 rect 小没遮挡）
            t.raycastTarget = false;

            var rt = t.GetComponent<RectTransform>();
            if (rt == null) return;
            if (rt.sizeDelta.x >= TargetWidth) return;

            float oldWidth = rt.sizeDelta.x;
            rt.sizeDelta = new Vector2(TargetWidth, rt.sizeDelta.y);
            Plugin.DebugLog("[InvExpand] PageText width "
                + oldWidth + " -> " + TargetWidth
                + ", raycastTarget=false");
        }
    }
}