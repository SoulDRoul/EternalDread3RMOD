using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>存档时写生态文件。</summary>
    [HarmonyPatch(typeof(SaveLoadXML), "SaveGame")]
    public static class Patch_SaveLoadXML_SaveGame_Ecology
    {
        static void Postfix()
        {

            

            try
            {
                var data = new EcologySaveData();
                data.TotalGameHours = GameTimeTracker.GetTotalGameHours();
                data.CharName = SavingStat.charName ?? string.Empty;
                foreach (var r in EcologyRegistry.AllRegions)
                    data.Regions.Add(r);

                EcologyPersistence.Save(data);
            }
            catch (System.Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] SaveGame ecology postfix failed: " + e.Message);
            }
        }
    }
}