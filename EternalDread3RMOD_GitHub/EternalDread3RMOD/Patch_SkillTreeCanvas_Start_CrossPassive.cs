using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>技能面板打开时初始化缓存，并把被动职业重置为当前职业。</summary>
    [HarmonyPatch(typeof(SkillTreeCanvas), "Start")]
    public static class Patch_SkillTreeCanvas_Start_CrossPassive
    {
        static void Postfix(SkillTreeCanvas __instance)
        {
            if (!Plugin.CrossPassiveEnabled.Value) return;
            if (__instance == null) return;

            CrossPassiveHelper.InitializeCache(__instance);

            int currentTab = CrossPassiveHelper.GetCurrentTab(__instance);
            if (currentTab >= 0)
                CrossPassiveHelper.CurrentPassiveJob = currentTab;
        }
    }
}