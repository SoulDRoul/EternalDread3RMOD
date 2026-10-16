using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(CharCreation), "OpenCreateMenu")]
    public static class Patch_CharCreation_OpenCreateMenu_PermadeathButton
    {
        static void Postfix()
        {
            PermadeathButtonUI.Show();
        }
    }
}