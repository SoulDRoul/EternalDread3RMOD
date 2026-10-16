using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>标题场景 Start 时解除所有 UI 抑制。</summary>
    [HarmonyPatch(typeof(Title), "Start")]
    public static class Patch_Title_Start_UiResume
    {
        static void Postfix()
        {
            UiSuppress.All = false;
            EcologyUI.SuppressRender = false;

            if (Plugin.Log != null)
                Plugin.DebugLog("[ED3R] UiSuppress resumed at title");
        }
    }
}