using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 储物箱数量弹窗 - 移动键快捷调数量。
    /// 键位映射与商店一致，走原版 Selection* 方法。
    /// </summary>
    [HarmonyPatch(typeof(SharedStashCanvas), "Update")]
    public static class Patch_SharedStashCanvas_Update_QuantityHotkey
    {
        private static MethodInfo _selUp, _selDown, _selLeft, _selRight;

        static Patch_SharedStashCanvas_Update_QuantityHotkey()
        {
            var t = typeof(SharedStashCanvas);
            _selUp = AccessTools.DeclaredMethod(t, "SelectionUp");
            _selDown = AccessTools.DeclaredMethod(t, "SelectionDown");
            _selLeft = AccessTools.DeclaredMethod(t, "SelectionLeft");
            _selRight = AccessTools.DeclaredMethod(t, "SelectionRight");
        }

        [HarmonyPostfix]
        private static void Postfix(SharedStashCanvas __instance)
        {
            if (Plugin.QuantityHotkeyEnabled == null || !Plugin.QuantityHotkeyEnabled.Value) return;
            try
            {
                bool stashOpen = __instance.setNumber != null
                    && __instance.setNumber.basePanel != null
                    && __instance.setNumber.basePanel.activeSelf;
                bool playerOpen = __instance.setNumberPlayer != null
                    && __instance.setNumberPlayer.basePanel != null
                    && __instance.setNumberPlayer.basePanel.activeSelf;
                if (!stashOpen && !playerOpen) return;

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
                    Plugin.Log.LogError("[ED3R] StashQuantityHotkey error: " + ex);
            }
        }
    }
}