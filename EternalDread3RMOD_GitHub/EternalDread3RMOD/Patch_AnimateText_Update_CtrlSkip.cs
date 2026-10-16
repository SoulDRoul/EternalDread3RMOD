using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 按住 Ctrl 快速跳过事件系统对话（AnimateText）。
    /// 每 SkipIntervalSec 秒调一次 DoneText：正在打字 → 显示全部；已打完 → 下一句。
    /// </summary>
    [HarmonyPatch(typeof(AnimateText), "Update")]
    public static class Patch_AnimateText_Update_CtrlSkip
    {
        private static MethodInfo _doneTextMethod;
        private static bool _searched = false;
        private static float _lastSkipTime = 0f;

        static void Postfix(AnimateText __instance)
        {
            if (__instance == null) return;
            if (!Plugin.DialogueCtrlSkipEnabled.Value) return;

            if (!IsCtrlHeld()) return;

            float interval = Plugin.DialogueCtrlSkipIntervalSec.Value;
            if (interval < 0.01f) interval = 0.01f;
            if (Time.unscaledTime - _lastSkipTime < interval) return;
            _lastSkipTime = Time.unscaledTime;

            if (!_searched)
            {
                _searched = true;
                _doneTextMethod = AccessTools.Method(typeof(AnimateText), "DoneText");
                if (_doneTextMethod == null && Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] AnimateText.DoneText not found");
            }
            if (_doneTextMethod == null) return;

            try
            {
                _doneTextMethod.Invoke(__instance, null);
            }
            catch (System.Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] CtrlSkip DoneText failed: " + e.Message);
            }
        }

        private static bool IsCtrlHeld()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }
    }
}