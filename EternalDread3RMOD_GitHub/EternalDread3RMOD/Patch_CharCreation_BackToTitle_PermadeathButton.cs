using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(CharCreation), "BackToTitle")]
    public static class Patch_CharCreation_BackToTitle_PermadeathButton
    {
        static void Postfix()
        {
            PermadeathButtonUI.Hide();
        }
    }
}