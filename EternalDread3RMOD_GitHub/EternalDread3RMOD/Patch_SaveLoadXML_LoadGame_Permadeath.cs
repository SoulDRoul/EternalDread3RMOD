using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 读档成功后同步 permadeath 状态到内存。
    /// 只在真正读档时（非创角选槽位）触发。
    /// </summary>
    [HarmonyPatch(typeof(SaveLoadXML), "LoadGame")]
    public static class Patch_SaveLoadXML_LoadGame_Permadeath
    {
        static void Postfix(SaveLoadXML __instance)
        {
            // createNewChar = true 时只是选槽位，不是真正读档
            if (__instance.createNewChar) return;

            int slot = __instance.saveSlot;
            PermadeathState.CurrentSavePermadeath = PermadeathFileHelper.ReadPermadeath(slot);

            if (Plugin.Log != null && PermadeathState.CurrentSavePermadeath)
                Plugin.DebugLog("[ED3R] LoadGame: permadeath=true, slot=" + slot);
        }
    }
}