using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 物品栏动态扩容（玩家 + 共享箱子）。
    ///
    /// Prefix：无空位且无堆叠位 → 扩一页。
    /// Postfix：空位少于一页 → 主动扩一页预留。
    ///
    /// 页大小：
    ///   Player     = 14（玩家物品栏 UI 每页 14）
    ///   ShareStash = 8 （共享箱子 UI 每页 8）
    /// </summary>
    [HarmonyPatch(typeof(Inventory), "AddItem")]
    public static class Patch_Inventory_AddItem_Expand
    {
        static void Prefix(Inventory __instance, int id, out bool __state)
        {
            __state = false;
            if (!IsExpandable(__instance)) return;

            if (InventoryExpandHelper.CountEmptyItemSlots(__instance) > 0) return;
            if (InventoryExpandHelper.HasStackableItemSlot(__instance, id)) return;

            int grow = GetGrowSize(__instance);
            if (grow <= 0) return;

            try
            {
                int before = __instance.items.Length;
                InventoryExpandHelper.GrowItems(__instance, grow);
                Plugin.DebugLog("[InvExpand] AddItem Prefix items "
                    + before + " -> " + __instance.items.Length
                    + " (" + __instance.gameObject.name + ")");
                __state = true;
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] AddItem Prefix expand error: " + e);
            }
        }

        static void Postfix(Inventory __instance, bool __state)
        {
            if (!IsExpandable(__instance)) return;
            if (__state) return;

            int grow = GetGrowSize(__instance);
            if (grow <= 0) return;

            try
            {
                int empty = InventoryExpandHelper.CountEmptyItemSlots(__instance);
                if (empty >= grow) return;

                int before = __instance.items.Length;
                InventoryExpandHelper.GrowItems(__instance, grow);
                Plugin.DebugLog("[InvExpand] AddItem Postfix items "
                    + before + " -> " + __instance.items.Length
                    + " (empty=" + empty + ")");
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] AddItem Postfix expand error: " + e);
            }
        }

        internal static bool IsExpandable(Inventory inv)
        {
            if (inv == null || inv.gameObject == null) return false;
            if (Plugin.InventoryExpandEnabled == null
                || !Plugin.InventoryExpandEnabled.Value) return false;

            var go = inv.gameObject;
            if (go.CompareTag("Player")) return true;
            if (go.name == "ShareStash") return true;
            return false;
        }

        internal static int GetGrowSize(Inventory inv)
        {
            if (inv == null || inv.gameObject == null) return 0;
            if (inv.gameObject.CompareTag("Player"))
                return InventoryExpandHelper.ItemSlotsPerPage;   // 14
            if (inv.gameObject.name == "ShareStash")
                return InventoryExpandHelper.EquipSlotsPerPage;  // 8
            return 0;
        }
    }
}