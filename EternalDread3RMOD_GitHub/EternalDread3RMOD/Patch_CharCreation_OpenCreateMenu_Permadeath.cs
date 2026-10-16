using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>创角界面打开时重置勾选状态。</summary>
    [HarmonyPatch(typeof(CharCreation), "OpenCreateMenu")]
    public static class Patch_CharCreation_OpenCreateMenu_Permadeath
    {
        static void Postfix()
        {
            PermadeathState.PendingPermadeath = false;
        }
    }
}