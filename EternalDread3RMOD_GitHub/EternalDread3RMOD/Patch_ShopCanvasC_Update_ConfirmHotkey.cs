using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 商店数量弹窗快捷确认。
    /// 弹窗打开时，按下游戏内互动键（默认 E）等价于按回车：
    /// 走原版 ActivateSelection()，与 Return 路径 100% 一致。
    /// 覆盖：商店购买 / 商店贩卖 两处数量弹窗。
    /// </summary>
    [HarmonyPatch(typeof(ShopCanvasC), "Update")]
    public static class Patch_ShopCanvasC_Update_ConfirmHotkey
    {
        private static MethodInfo _activateSelection;

        static Patch_ShopCanvasC_Update_ConfirmHotkey()
        {
            _activateSelection = AccessTools.DeclaredMethod(typeof(ShopCanvasC), "ActivateSelection");
        }

        [HarmonyPostfix]
        private static void Postfix(ShopCanvasC __instance)
        {
            if (Plugin.ConfirmHotkeyEnabled == null || !Plugin.ConfirmHotkeyEnabled.Value) return;
            try
            {
                // 只在数量弹窗打开时响应，避免误触
                bool buyOpen = __instance.buyConfirmation != null
                    && __instance.buyConfirmation.basePanel != null
                    && __instance.buyConfirmation.basePanel.activeSelf;
                bool sellOpen = __instance.sellConfirmation != null
                    && __instance.sellConfirmation.basePanel != null
                    && __instance.sellConfirmation.basePanel.activeSelf;
                if (!buyOpen && !sellOpen) return;

                if (Input.GetKeyDown(ButtonInputOption.interact))
                {
                    if (_activateSelection != null)
                    {
                        _activateSelection.Invoke(__instance, null);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Plugin.Log != null)
                {
                    Plugin.Log.LogError("[ED3R] ShopConfirmHotkey error: " + ex);
                }
            }
        }
    }
}