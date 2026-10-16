using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Status), "LevelUp")]
    public static class Patch_Status_LevelUp_Minimap
    {
        static void Postfix(Status __instance)
        {
            if (__instance.gameObject != GlobalVar.mainPlayer) return;
            if (!Plugin.MinimapEnemyLevelColorEnabled.Value) return;
            MinimapIconRegistry.RefreshAllEnemies();
        }
    }
}