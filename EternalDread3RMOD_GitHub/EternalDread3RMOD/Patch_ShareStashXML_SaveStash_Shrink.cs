using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 共享箱子保存前裁掉尾部全空页。
    ///
    /// ShareStashXML.SaveStash 只在 SharedStashCanvas.CloseMenu 里被调（关闭箱子时）。
    /// 参数 stash 是传入的 Inventory，就是要裁的那个。
    ///
    /// 硬编码：始终启用，不受配置控制。
    /// </summary>
    [HarmonyPatch(typeof(ShareStashXML), "SaveStash")]
    public static class Patch_ShareStashXML_SaveStash_Shrink
    {
        static void Prefix(Inventory stash)
        {
            if (stash == null) return;
            if (stash.gameObject == null) return;
            if (stash.gameObject.name != "ShareStash") return;

            try
            {
                InventoryShrinkHelper.TrimItems(stash,
                    InventoryShrinkHelper.MinSharedStash,
                    InventoryExpandHelper.EquipSlotsPerPage);   // 8

                InventoryShrinkHelper.TrimEquipments(stash,
                    InventoryShrinkHelper.MinSharedStash,
                    InventoryExpandHelper.EquipSlotsPerPage);   // 8
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] ShareStash shrink error: " + e);
            }
        }
    }
}