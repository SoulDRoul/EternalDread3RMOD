using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 保存前裁掉玩家物品栏 / 装备栏的尾部全空页。
    ///
    /// 必须在 Prefix 里（早于 SaveGame 方法体）：
    ///   1. 裁 → player.items / equipments 变短
    ///   2. 原版 SaveGame → SavingPlayer → SavingStat.itemId = new int[items.Length]
    ///      → 自然写入缩短后的数据
    ///
    /// 硬编码：始终启用，不受配置控制。
    /// </summary>
    [HarmonyPatch(typeof(SaveLoadXML), "SaveGame")]
    public static class Patch_SaveLoadXML_SaveGame_Shrink
    {
        static void Prefix()
        {
            if (GlobalVar.mainPlayer == null) return;

            try
            {
                var inv = GlobalVar.mainPlayer.GetComponent<Inventory>();
                if (inv == null) return;

                InventoryShrinkHelper.TrimItems(inv,
                    InventoryShrinkHelper.MinPlayerItems,
                    InventoryExpandHelper.ItemSlotsPerPage);   // 14

                InventoryShrinkHelper.TrimEquipments(inv,
                    InventoryShrinkHelper.MinPlayerEquipments,
                    InventoryExpandHelper.EquipSlotsPerPage);  // 8
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] SaveGame shrink error: " + e);
            }
        }
    }
}