using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(SkillTreeCanvas), "Start")]
    public static class Patch_SkillTreeCanvas_Start_CrossPassiveUI
    {
        static void Postfix(SkillTreeCanvas __instance)
        {
            if (!Plugin.CrossPassiveEnabled.Value) return;
            CrossPassiveButtonUI.Attach(__instance);
        }
    }
}