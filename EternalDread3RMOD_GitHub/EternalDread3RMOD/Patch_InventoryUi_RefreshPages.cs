using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 打开物品栏 / 装备栏 Tab 时，按 player.Inventory 的实际数组长度重算页数。
    /// 动态扩容后页数会随之增长，缩容后也会跟着下降。
    /// </summary>
    public static class InventoryUiPageHelper
    {
        public static void Refresh(InventoryUiCanvas ui)
        {
            if (ui == null) return;
            if (Plugin.InventoryExpandEnabled == null
                || !Plugin.InventoryExpandEnabled.Value) return;

            var player = ui.player;
            if (player == null) player = GlobalVar.mainPlayer;
            if (player == null) return;

            var inv = player.GetComponent<Inventory>();
            if (inv == null) return;

            int itemLen = inv.items != null ? inv.items.Length : 0;
            int eqLen = inv.equipments != null ? inv.equipments.Length : 0;

            int pm = ui.pageMultiply > 0 ? ui.pageMultiply : InventoryExpandHelper.ItemSlotsPerPage;
            int em = ui.eqMultiply > 0 ? ui.eqMultiply : InventoryExpandHelper.EquipSlotsPerPage;

            int neededItemPage = itemLen > 0 ? (itemLen - 1) / pm : 0;
            int neededEqPage = eqLen > 0 ? (eqLen - 1) / em : 0;

            if (ui.maxPage != neededItemPage) ui.maxPage = neededItemPage;
            if (ui.maxPageEq != neededEqPage) ui.maxPageEq = neededEqPage;

            Plugin.DebugLog("[InvExpand] RefreshPages maxPage=" + ui.maxPage
                + " maxPageEq=" + ui.maxPageEq
                + " itemLen=" + itemLen + " eqLen=" + eqLen);
        }
    }

    [HarmonyPatch(typeof(InventoryUiCanvas), "OpenUsableTab")]
    public static class Patch_InventoryUi_RefreshPages_Usable
    {
        static void Postfix(InventoryUiCanvas __instance)
        {
            try { InventoryUiPageHelper.Refresh(__instance); }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] RefreshPages (Usable) error: " + e);
            }
        }
    }

    [HarmonyPatch(typeof(InventoryUiCanvas), "OpenEquipmentTab")]
    public static class Patch_InventoryUi_RefreshPages_Equip
    {
        static void Postfix(InventoryUiCanvas __instance)
        {
            try { InventoryUiPageHelper.Refresh(__instance); }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] RefreshPages (Equip) error: " + e);
            }
        }
    }
}