using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(SaveLoadXML), "SaveGame")]
    public static class Patch_SaveLoadXML_SaveGame_Permadeath
    {
        static void Postfix(SaveLoadXML __instance)
        {
            if (PermadeathState.PendingPermadeath)
            {
                PermadeathState.CurrentSavePermadeath = true;
                PermadeathState.PendingPermadeath = false;
            }

            int slot = GlobalVar.eventsId[95];
            bool permadeath = PermadeathState.CurrentSavePermadeath;

            PermadeathFileHelper.Save(slot, permadeath, false);
            PermadeathState.CurrentSavePermadeath = PermadeathFileHelper.ReadPermadeath(slot);

            if (Plugin.Log != null && PermadeathState.CurrentSavePermadeath)
                Plugin.DebugLog("[ED3R] SaveGame: permadeath=true, slot=" + slot);
        }
    }
}