using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 储物箱数量弹窗快捷确认。
    /// 弹窗打开时，按下游戏内互动键（默认 E）等价于按回车：
    /// 走原版 ActivateSelection()，与 Return 路径 100% 一致。
    /// 覆盖：存入物品 / 取出物品 两处数量弹窗。
    /// </summary>
    [HarmonyPatch(typeof(SharedStashCanvas), "Update")]
    public static class Patch_SharedStashCanvas_Update_ConfirmHotkey
    {
        private static MethodInfo _activateSelection;

        static Patch_SharedStashCanvas_Update_ConfirmHotkey()
        {
            _activateSelection = AccessTools.DeclaredMethod(typeof(SharedStashCanvas), "ActivateSelection");
        }

        [HarmonyPostfix]
        private static void Postfix(SharedStashCanvas __instance)
        {
            if (Plugin.ConfirmHotkeyEnabled == null || !Plugin.ConfirmHotkeyEnabled.Value) return;
            try
            {
                // 只在数量弹窗打开时响应，避免误触
                bool stashOpen = __instance.setNumber != null
                    && __instance.setNumber.basePanel != null
                    && __instance.setNumber.basePanel.activeSelf;
                bool playerOpen = __instance.setNumberPlayer != null
                    && __instance.setNumberPlayer.basePanel != null
                    && __instance.setNumberPlayer.basePanel.activeSelf;
                if (!stashOpen && !playerOpen) return;

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
                    Plugin.Log.LogError("[ED3R] StashConfirmHotkey error: " + ex);
                }
            }
        }
    }
}