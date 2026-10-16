using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 读档 / 重生时，把 player 的 items / equipments 数组按 SavingStat 长度对齐（只扩不缩）。
    ///
    /// 【为什么必须在 Prefix】
    /// 原版 SettingNewPlayer 方法体内部：
    ///   for (int i = 0; i < component4.items.Length; i++)
    ///       component4.items[i].itemId = SavingStat.itemId[i];
    /// 循环上限是 player.items.Length（prefab 默认）。
    /// 如果 SavingStat.itemId 更长（XML 里保存的动态长度）→ 越界 NPE。
    /// 如果 SavingStat.itemId 更短 → 后段保留 prefab 默认值（0），安全。
    ///
    /// 注意：equipments 是直接引用替换，原版 `component4.equipments = SavingStat.equipments;`
    /// 所以扩 SavingStat 就等于扩 player 的。
    /// </summary>
    [HarmonyPatch(typeof(GlobalVar), "SettingNewPlayer")]
    public static class Patch_GlobalVar_SettingNewPlayer_ExpandInventory
    {
        static void Prefix(GameObject player)
        {
            if (Plugin.InventoryExpandEnabled == null
                || !Plugin.InventoryExpandEnabled.Value) return;

            var p = player;
            if (p == null) p = GlobalVar.mainPlayer;
            if (p == null) return;

            var inv = p.GetComponent<Inventory>();
            if (inv == null) return;

            try
            {
                int itemTarget = (SavingStat.itemId != null)
                    ? SavingStat.itemId.Length : 0;
                int eqTarget = (SavingStat.equipments != null)
                    ? SavingStat.equipments.Length : 0;

                if (itemTarget > 0)
                    InventoryExpandHelper.EnsureItems(inv, itemTarget);
                if (eqTarget > 0)
                    InventoryExpandHelper.EnsureEquipments(inv, eqTarget);

                Plugin.DebugLog("[InvExpand] SettingNewPlayer align: items="
                    + inv.items.Length + " equipments=" + inv.equipments.Length);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] SettingNewPlayer ExpandInventory error: " + e);
            }
        }
    }
}