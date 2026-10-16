using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 商店数量弹窗 - 移动键快捷调数量。
    /// 弹窗打开时，用游戏内上下左右移动键（默认 WASD）替代方向键：
    ///   up    → SelectionUp    (+10)
    ///   down  → SelectionDown  (-10)
    ///   left  → SelectionLeft  (-1)
    ///   right → SelectionRight (+1)
    /// 直接调用原版 Selection* 方法，行为 100% 一致。
    /// </summary>
    [HarmonyPatch(typeof(ShopCanvasC), "Update")]
    public static class Patch_ShopCanvasC_Update_QuantityHotkey
    {
        private static MethodInfo _selUp, _selDown, _selLeft, _selRight;

        static Patch_ShopCanvasC_Update_QuantityHotkey()
        {
            var t = typeof(ShopCanvasC);
            _selUp = AccessTools.DeclaredMethod(t, "SelectionUp");
            _selDown = AccessTools.DeclaredMethod(t, "SelectionDown");
            _selLeft = AccessTools.DeclaredMethod(t, "SelectionLeft");
            _selRight = AccessTools.DeclaredMethod(t, "SelectionRight");
        }

        [HarmonyPostfix]
        private static void Postfix(ShopCanvasC __instance)
        {
            if (Plugin.QuantityHotkeyEnabled == null || !Plugin.QuantityHotkeyEnabled.Value) return;
            try
            {
                bool buyOpen = __instance.buyConfirmation != null
                    && __instance.buyConfirmation.basePanel != null
                    && __instance.buyConfirmation.basePanel.activeSelf;
                bool sellOpen = __instance.sellConfirmation != null
                    && __instance.sellConfirmation.basePanel != null
                    && __instance.sellConfirmation.basePanel.activeSelf;
                if (!buyOpen && !sellOpen) return;

                if (Input.GetKeyDown(ButtonInputOption.up) && _selUp != null)
                    _selUp.Invoke(__instance, null);
                else if (Input.GetKeyDown(ButtonInputOption.down) && _selDown != null)
                    _selDown.Invoke(__instance, null);
                else if (Input.GetKeyDown(ButtonInputOption.left) && _selLeft != null)
                    _selLeft.Invoke(__instance, null);
                else if (Input.GetKeyDown(ButtonInputOption.right) && _selRight != null)
                    _selRight.Invoke(__instance, null);
            }
            catch (Exception ex)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] ShopQuantityHotkey error: " + ex);
            }
        }
    }
}