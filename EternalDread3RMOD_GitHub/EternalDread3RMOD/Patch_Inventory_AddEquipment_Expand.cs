using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 装备栏动态扩容（玩家 + 共享箱子）。
    /// 装备不堆叠：无空位就扩一页（都是 +8）。
    /// </summary>
    [HarmonyPatch(typeof(Inventory), "AddEquipment")]
    public static class Patch_Inventory_AddEquipment_Expand
    {
        static void Prefix(Inventory __instance, out bool __state)
        {
            __state = false;
            if (!Patch_Inventory_AddItem_Expand.IsExpandable(__instance)) return;
            if (InventoryExpandHelper.CountEmptyEquipSlots(__instance) > 0) return;

            int grow = InventoryExpandHelper.EquipSlotsPerPage;   // 玩家和 Stash 都是 8

            try
            {
                int before = __instance.equipments.Length;
                InventoryExpandHelper.GrowEquipments(__instance, grow);
                Plugin.DebugLog("[InvExpand] AddEquipment Prefix equipments "
                    + before + " -> " + __instance.equipments.Length
                    + " (" + __instance.gameObject.name + ")");
                __state = true;
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] AddEquipment Prefix expand error: " + e);
            }
        }

        static void Postfix(Inventory __instance, bool __state)
        {
            if (!Patch_Inventory_AddItem_Expand.IsExpandable(__instance)) return;
            if (__state) return;

            int grow = InventoryExpandHelper.EquipSlotsPerPage;

            try
            {
                int empty = InventoryExpandHelper.CountEmptyEquipSlots(__instance);
                if (empty >= grow) return;

                int before = __instance.equipments.Length;
                InventoryExpandHelper.GrowEquipments(__instance, grow);
                Plugin.DebugLog("[InvExpand] AddEquipment Postfix equipments "
                    + before + " -> " + __instance.equipments.Length
                    + " (empty=" + empty + ")");
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] AddEquipment Postfix expand error: " + e);
            }
        }
    }
}