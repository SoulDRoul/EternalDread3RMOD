using System.Reflection;
using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 对话提示（一次性）：
    ///   StartAnimate → 显示
    ///   DoneText 且 begin=false → 隐藏（翻页或结束都隐藏）
    /// 作为 Ctrl 快进的"功能存在感"提示，第一次对话出现时足够。
    /// </summary>
    [HarmonyPatch(typeof(AnimateText), "StartAnimate")]
    public static class Patch_AnimateText_StartAnimate_CtrlHint
    {
        static void Postfix(AnimateText __instance)
        {
            DialogueCtrlHintUI.NotifyShow(__instance);
        }
    }

    [HarmonyPatch(typeof(AnimateText), "DoneText")]
    public static class Patch_AnimateText_DoneText_CtrlHint
    {
        private static FieldInfo _beginField;

        static void Prefix(AnimateText __instance, out bool __state)
        {
            if (_beginField == null)
                _beginField = AccessTools.Field(typeof(AnimateText), "begin");
            __state = _beginField != null && (bool)_beginField.GetValue(__instance);
        }

        static void Postfix(AnimateText __instance, bool __state)
        {
            // __state=true  → 打字中，本次只是"显示全部"，对话继续，不隐藏
            // __state=false → 已打完，走了 source.FinishDialogue → 隐藏
            if (!__state)
                DialogueCtrlHintUI.NotifyHide(__instance);
        }
    }
}