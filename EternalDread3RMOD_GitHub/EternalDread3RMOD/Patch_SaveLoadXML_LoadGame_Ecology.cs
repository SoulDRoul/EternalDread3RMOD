using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 读档时：
    ///   1. 同步 GlobalVar.eventsId[95] 到当前槽位（原版不更新，会污染后续保存）
    ///   2. 加载对应槽位的生态数据
    ///   3. 同步 clock
    ///   4. 懒更新
    /// </summary>
    [HarmonyPatch(typeof(SaveLoadXML), "LoadGame")]
    public static class Patch_SaveLoadXML_LoadGame_Ecology
    {
        static void Postfix(SaveLoadXML __instance)
        {
            if (__instance == null) return;
            if (!Plugin.EcologyEnabled.Value) return;

            int slot = __instance.saveSlot;

            // 修正原版：读档后 eventsId[95] 未更新，会影响后续保存 / 生态文件定位
            GlobalVar.eventsId[95] = slot;

            // 走共用加载流程
            EcologyLoadHelper.LoadAndApply(slot);
        }
    }
}