using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(SaveLoadXML), "SaveGame")]
    public static class Patch_SaveLoadXML_SaveGame_SkillPoint
    {
        static void Postfix()
        {
            if (!Plugin.SkillPointRefreshEnabled.Value) return;
            try
            {
                var snap = SkillPointRegistry.Snapshot();
                SkillPointPersistence.Save(snap);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] SkillPoint save patch error: " + e);
            }
        }
    }
}