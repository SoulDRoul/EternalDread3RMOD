using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 按住 Ctrl 快速跳过 NPC 对话（Dialogue）。
    /// NextPage 本身处理"打字中 → 显示全部；已打完 → 下一句"。
    /// </summary>
    [HarmonyPatch(typeof(Dialogue), "Update")]
    public static class Patch_Dialogue_Update_CtrlSkip
    {
        private static float _lastSkipTime = 0f;

        static void Postfix(Dialogue __instance)
        {
            if (__instance == null) return;
            if (!Plugin.DialogueCtrlSkipEnabled.Value) return;

            if (!IsCtrlHeld()) return;

            float interval = Plugin.DialogueCtrlSkipIntervalSec.Value;
            if (interval < 0.01f) interval = 0.01f;
            if (Time.unscaledTime - _lastSkipTime < interval) return;
            _lastSkipTime = Time.unscaledTime;

            try
            {
                __instance.NextPage();
            }
            catch (System.Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] CtrlSkip NextPage failed: " + e.Message);
            }
        }

        private static bool IsCtrlHeld()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }
    }
}