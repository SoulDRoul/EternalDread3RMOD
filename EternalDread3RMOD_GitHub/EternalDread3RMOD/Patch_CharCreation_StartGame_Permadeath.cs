using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(CharCreation), "StartGame")]
    public static class Patch_CharCreation_StartGame_Permadeath
    {
        static void Prefix()
        {
            PermadeathState.CurrentSavePermadeath = PermadeathState.PendingPermadeath;

            PermadeathButtonUI.Hide();

            if (Plugin.Log != null && PermadeathState.PendingPermadeath)
                Plugin.DebugLog("[ED3R] New game started with permadeath");
        }
    }
}