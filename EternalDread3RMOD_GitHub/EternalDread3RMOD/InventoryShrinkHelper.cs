using System;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 保存时裁掉尾部连续全空页，防止存档膨胀。
    ///
    /// 核心原则：
    ///   1. 只裁全空页（页内有任何物品 → 停止）
    ///   2. 不低于下限（= prefab 默认值，否则下次读档反而恢复成默认值）
    ///   3. 不修改物品数据，只改数组长度
    ///
    /// 下限硬编码为原版 prefab 值，不开放配置（防玩家填错导致存档异常）。
    /// </summary>
    public static class InventoryShrinkHelper
    {
        // ============ 硬编码下限（= 原版 prefab 序列化值）============
        // 若改成其他值，必须确认 prefab 里的实际初始长度，否则下次读档会涨回来。

        /// <summary>玩家物品栏下限。原版 prefab = 112（8 页 × 14）。</summary>
        public const int MinPlayerItems = 112;

        /// <summary>玩家装备栏下限。原版 prefab = 100（12.5 页 × 8）。</summary>
        public const int MinPlayerEquipments = 100;

        /// <summary>共享箱子下限。原版 prefab = 64（8 页 × 8）。</summary>
        public const int MinSharedStash = 64;

        // ============ 对外接口 ============

        /// <summary>裁 items 尾部全空页。</summary>
        public static void TrimItems(Inventory inv, int minSlots, int pageSize)
        {
            if (inv == null || inv.items == null) return;
            if (pageSize <= 0) return;

            int oldLen = inv.items.Length;
            int newLen = ComputeTrimmedLength(oldLen, pageSize, minSlots,
                i => IsItemEmpty(inv.items[i]));
            if (newLen >= oldLen) return;

            var arr = new ItemSet[newLen];
            Array.Copy(inv.items, arr, newLen);
            inv.items = arr;
            Plugin.DebugLog("[InvShrink] items " + oldLen + " -> " + newLen
                + " (" + inv.gameObject.name + ")");
        }

        /// <summary>裁 equipments 尾部全空页。</summary>
        public static void TrimEquipments(Inventory inv, int minSlots, int pageSize)
        {
            if (inv == null || inv.equipments == null) return;
            if (pageSize <= 0) return;

            int oldLen = inv.equipments.Length;
            int newLen = ComputeTrimmedLength(oldLen, pageSize, minSlots,
                i => IsEquipEmpty(inv.equipments[i]));
            if (newLen >= oldLen) return;

            var arr = new EquipmentSet[newLen];
            Array.Copy(inv.equipments, arr, newLen);
            inv.equipments = arr;
            Plugin.DebugLog("[InvShrink] equipments " + oldLen + " -> " + newLen
                + " (" + inv.gameObject.name + ")");
        }

        // ============ 内部 ============

        private static bool IsItemEmpty(ItemSet s)
        {
            return s == null || s.itemId == 0;
        }

        private static bool IsEquipEmpty(EquipmentSet s)
        {
            return s == null || s.equipmentId == 0;
        }

        /// <summary>
        /// 从尾部逐页向前扫，找到第一个"页内有物品"的页则停止。
        /// 返回裁剪后的新长度。
        /// </summary>
        private static int ComputeTrimmedLength(int oldLen, int pageSize,
            int minSlots, Func<int, bool> isEmpty)
        {
            if (oldLen <= minSlots) return oldLen;

            int n = oldLen;
            while (n - pageSize >= minSlots)
            {
                bool allEmpty = true;
                for (int i = n - pageSize; i < n; i++)
                {
                    if (!isEmpty(i)) { allEmpty = false; break; }
                }
                if (!allEmpty) break;
                n -= pageSize;
            }
            return n;
        }
    }
}