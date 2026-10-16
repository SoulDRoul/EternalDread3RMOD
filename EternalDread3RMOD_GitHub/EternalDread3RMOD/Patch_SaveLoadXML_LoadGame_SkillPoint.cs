using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(SaveLoadXML), "LoadGame")]
    public static class Patch_SaveLoadXML_LoadGame_SkillPoint
    {
        static void Postfix()
        {
            if (!Plugin.SkillPointRefreshEnabled.Value) return;
            try
            {
                var data = SkillPointPersistence.Load();
                SkillPointRegistry.ReplaceAll(data);
                Plugin.DebugLog("[ED3R] SkillPoint loaded: records="
                    + (data.Records != null ? data.Records.Count : 0)
                    + " todayCount=" + data.TodayRefreshCount
                    + " todayDay=" + data.TodayDayNumber);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] SkillPoint load patch error: " + e);
            }
        }
    }
}