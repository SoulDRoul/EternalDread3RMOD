using System;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 物品栏 / 装备栏容量扩展的共用工具。
    ///
    /// 两种模式：
    ///   1. 按静态目标长度扩（EnsureItems / EnsureEquipments）—— 读档时按 SavingStat 对齐
    ///   2. 动态增长（GrowItems / GrowEquipments）—— 玩家拾取/购买时自动 +1 页
    ///
    /// 所有方法幂等、只扩不缩，异常安全。
    /// </summary>
    public static class InventoryExpandHelper
    {
        /// <summary>物品栏每页格数（与原版 UI 的 pageMultiply 一致）。</summary>
        public const int ItemSlotsPerPage = 14;

        /// <summary>装备栏每页格数（与原版 UI 的 eqMultiply 一致）。</summary>
        public const int EquipSlotsPerPage = 8;

        // ============ 静态对齐（读档 / SettingNewPlayer 用）============

        /// <summary>确保 items 数组长度 ≥ minLength（只扩不缩）。</summary>
        public static void EnsureItems(Inventory inv, int minLength)
        {
            if (inv == null || minLength <= 0) return;
            inv.items = ExpandArray(inv.items, minLength);
        }

        /// <summary>确保 equipments 数组长度 ≥ minLength（只扩不缩）。</summary>
        public static void EnsureEquipments(Inventory inv, int minLength)
        {
            if (inv == null || minLength <= 0) return;
            inv.equipments = ExpandArray(inv.equipments, minLength);
        }

        // ============ 动态增长（AddItem / AddEquipment 用）============

        /// <summary>物品栏 items 长度 + grow 格。</summary>
        public static void GrowItems(Inventory inv, int grow)
        {
            if (inv == null || grow <= 0) return;
            inv.items = ExpandArray(inv.items, inv.items.Length + grow);
        }

        /// <summary>装备栏 equipments 长度 + grow 格。</summary>
        public static void GrowEquipments(Inventory inv, int grow)
        {
            if (inv == null || grow <= 0) return;
            inv.equipments = ExpandArray(inv.equipments, inv.equipments.Length + grow);
        }

        // ============ 空位扫描 ============

        /// <summary>统计 items 中空槽数量（itemId == 0 或元素为 null）。</summary>
        public static int CountEmptyItemSlots(Inventory inv)
        {
            if (inv == null || inv.items == null) return 0;
            int n = 0;
            for (int i = 0; i < inv.items.Length; i++)
            {
                var s = inv.items[i];
                if (s == null || s.itemId == 0) n++;
            }
            return n;
        }

        /// <summary>统计 equipments 中空槽数量（equipmentId == 0 或元素为 null）。</summary>
        public static int CountEmptyEquipSlots(Inventory inv)
        {
            if (inv == null || inv.equipments == null) return 0;
            int n = 0;
            for (int i = 0; i < inv.equipments.Length; i++)
            {
                var s = inv.equipments[i];
                if (s == null || s.equipmentId == 0) n++;
            }
            return n;
        }

        /// <summary>检查 items 中是否已有同 id 的堆叠位（用于判断是否需要扩容量）。</summary>
        public static bool HasStackableItemSlot(Inventory inv, int id)
        {
            if (id <= 0) return true;   // 无效 id，不做扩容决策
            if (inv == null || inv.items == null) return false;
            for (int i = 0; i < inv.items.Length; i++)
            {
                var s = inv.items[i];
                if (s != null && s.itemId == id) return true;
            }
            return false;
        }

        // ============ 底层 ============

        private static T[] ExpandArray<T>(T[] old, int target) where T : new()
        {
            if (target <= 0) return old;
            if (old != null && old.Length >= target) return old;

            var arr = new T[target];
            int oldLen = 0;
            if (old != null)
            {
                oldLen = old.Length;
                Array.Copy(old, arr, oldLen);
            }
            // 关键：新增元素 new 出来，避免 class 类型出现 null
            for (int i = oldLen; i < target; i++)
                arr[i] = new T();
            return arr;
        }
    }
}