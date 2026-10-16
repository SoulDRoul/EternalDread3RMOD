using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(InventoryUiCanvas), "OpenUsableTab")]
    public static class Patch_InvSort_OnOpenUsable
    {
        static void Postfix(InventoryUiCanvas __instance)
        {
            if (!Plugin.AutoSortOnOpen.Value) return;
            if (__instance.player == null) return;
            var inv = __instance.player.GetComponent<Inventory>();
            InventorySorter.SortItems(inv);
            inv.SetAllShortcut();
        }
    }

    [HarmonyPatch(typeof(InventoryUiCanvas), "OpenEquipmentTab")]
    public static class Patch_InvSort_OnOpenEquip
    {
        static void Postfix(InventoryUiCanvas __instance)
        {
            if (!Plugin.AutoSortOnOpen.Value) return;
            if (__instance.player == null) return;
            var inv = __instance.player.GetComponent<Inventory>();
            InventorySorter.SortEquip(inv);
            inv.SetAllShortcut();
        }
    }
}