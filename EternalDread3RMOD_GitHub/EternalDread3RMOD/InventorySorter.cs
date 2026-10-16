using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    public static class InventorySorter
    {
        private static System.Reflection.FieldInfo _dbField;

        // ============ 对外入口 ============

        public static void SortAll(Inventory inv)
        {
            if (inv == null) return;
            SortItems(inv);
            SortEquip(inv);
            inv.SetAllShortcut();
        }

        public static void SortItems(Inventory inv)
        {
            var db = GetItemDb(inv);
            if (db == null) return;

            var arr = inv.items;
            int rule = Plugin.SortRule.Value;
            bool rev = Plugin.SortReverse.Value;

            Array.Sort(arr, (a, b) =>
            {
                bool ae = (a.itemId <= 0);
                bool be = (b.itemId <= 0);
                if (ae && be) return 0;
                if (ae) return 1;
                if (be) return -1;

                int c = CompareItems(a, b, db, rule);
                return rev ? -c : c;
            });
        }

        public static void SortEquip(Inventory inv)
        {
            var db = GetItemDb(inv);
            if (db == null) return;

            var arr = inv.equipments;
            int rule = Plugin.SortRule.Value;
            bool rev = Plugin.SortReverse.Value;

            Array.Sort(arr, (a, b) =>
            {
                bool ae = (a.equipmentId <= 0);
                bool be = (b.equipmentId <= 0);
                if (ae && be) return 0;
                if (ae) return 1;
                if (be) return -1;

                int c = CompareEquip(a, b, db, rule);
                return rev ? -c : c;
            });
        }

        // ============ 带 rule 参数的重载（供共享箱子左右独立使用）============

        public static void SortItems(Inventory inv, int rule)
        {
            var db = GetItemDb(inv);
            if (db == null) return;

            var arr = inv.items;
            bool rev = Plugin.SortReverse.Value;

            System.Array.Sort(arr, (a, b) =>
            {
                bool ae = (a.itemId <= 0);
                bool be = (b.itemId <= 0);
                if (ae && be) return 0;
                if (ae) return 1;
                if (be) return -1;

                int c = CompareItems(a, b, db, rule);
                return rev ? -c : c;
            });
        }

        public static void SortEquip(Inventory inv, int rule)
        {
            var db = GetItemDb(inv);
            if (db == null) return;

            var arr = inv.equipments;
            bool rev = Plugin.SortReverse.Value;

            System.Array.Sort(arr, (a, b) =>
            {
                bool ae = (a.equipmentId <= 0);
                bool be = (b.equipmentId <= 0);
                if (ae && be) return 0;
                if (ae) return 1;
                if (be) return -1;

                int c = CompareEquip(a, b, db, rule);
                return rev ? -c : c;
            });
        }

        // ============ 物品栏比较 ============

        private static int CompareItems(ItemSet a, ItemSet b, ItemData db, int rule)
        {
            var ia = db.usableItem[a.itemId];
            var ib = db.usableItem[b.itemId];
            if (ia == null || ib == null) return a.itemId.CompareTo(b.itemId);

            switch (rule)
            {
                case 0: // ID
                    return a.itemId.CompareTo(b.itemId);

                case 1: // 类别 + ID
                    int ca = GetItemCategoryRank(ia, a.itemId);
                    int cb = GetItemCategoryRank(ib, b.itemId);
                    if (ca != cb) return ca.CompareTo(cb);
                    return a.itemId.CompareTo(b.itemId);

                case 2: // 名称
                    int nc = string.Compare(ia.itemName ?? "", ib.itemName ?? "",
                        StringComparison.OrdinalIgnoreCase);
                    if (nc != 0) return nc;
                    return a.itemId.CompareTo(b.itemId);

                case 3: // 数量降序
                    int qc = b.quantity.CompareTo(a.quantity);
                    if (qc != 0) return qc;
                    return a.itemId.CompareTo(b.itemId);
            }
            return 0;
        }

        // ============ 装备栏比较 ============

        private static int CompareEquip(EquipmentSet a, EquipmentSet b, ItemData db, int rule)
        {
            var ea = db.equipment[a.equipmentId];
            var eb = db.equipment[b.equipmentId];
            if (ea == null || eb == null) return a.equipmentId.CompareTo(b.equipmentId);

            switch (rule)
            {
                case 0:
                    return a.equipmentId.CompareTo(b.equipmentId);

                case 1: // 装备类别 + ID
                    int ta = GetEquipCategoryRank(ea, a.equipmentId);
                    int tb = GetEquipCategoryRank(eb, b.equipmentId);
                    if (ta != tb) return ta.CompareTo(tb);
                    return a.equipmentId.CompareTo(b.equipmentId);

                case 2: // 名称
                    int nc = string.Compare(ea.itemName ?? "", eb.itemName ?? "",
                        StringComparison.OrdinalIgnoreCase);
                    if (nc != 0) return nc;
                    return a.equipmentId.CompareTo(b.equipmentId);

                case 3: // 强化等级降序
                    int uc = b.upgrade.CompareTo(a.upgrade);
                    if (uc != 0) return uc;
                    return a.equipmentId.CompareTo(b.equipmentId);
            }
            return 0;
        }

        // ============ 物品类别判定（6 类）============
        // 0 = 恢复
        // 1 = 添加 Buff
        // 2 = 解 Buff
        // 3 = 攻击道具
        // 4 = 未识别（可用但无匹配）
        // 5 = 材料（unusable）

        private static int GetItemCategory(UsableItemSetting item, int itemId)
        {
            // 玩家覆盖优先
            int ov = GetOverride(itemId, Plugin.ItemCategoryOverride.Value);
            if (ov >= 0) return ov;

            // 1. 材料优先（不可用就是材料）
            if (item.unusable) return 5;

            // 2. 恢复
            if (item.hpRecovery > 0 || item.mpRecovery > 0) return 0;

            // 3. 添加 Buff
            if (item.buffs != null && item.buffs.Length > 0) return 1;

            // 4. 解 Buff
            if (item.curePoison || item.cureStun || item.cureSilence
                || item.cureFrost || item.cureFear || item.cureConfuse
                || item.cureElectrocute) return 2;

            // 5. 攻击道具
            if (item.attackItem != null && item.attackItem.enable) return 3;

            // 6. 未识别
            return 4;
        }

        // ============ 装备类别判定（6 类）============
        // 0=武器, 1=护甲, 2=头饰, 3=手套, 4=靴子, 5=饰品

        private static int GetEquipCategory(EquipmentSetting eq, int equipId)
        {
            // 玩家覆盖优先
            int ov = GetOverride(equipId, Plugin.EquipCategoryOverride.Value);
            if (ov >= 0) return ov;

            // 用枚举值直接做类别编号
            return (int)eq.type;
        }

        // ============ 类别优先级（把类别映射到排序权重）============

        private static int[] _itemRanks;
        private static string _itemPriorityCache;
        private static int[] _equipRanks;
        private static string _equipPriorityCache;

        private static int GetItemCategoryRank(UsableItemSetting item, int itemId)
        {
            int cat = GetItemCategory(item, itemId);
            if (_itemRanks == null || _itemPriorityCache != Plugin.CategoryPriority.Value)
            {
                _itemRanks = ParsePriority(Plugin.CategoryPriority.Value, 6);
                _itemPriorityCache = Plugin.CategoryPriority.Value;
            }
            return (cat >= 0 && cat < _itemRanks.Length) ? _itemRanks[cat] : cat;
        }

        private static int GetEquipCategoryRank(EquipmentSetting eq, int equipId)
        {
            int cat = GetEquipCategory(eq, equipId);
            if (_equipRanks == null || _equipPriorityCache != Plugin.EquipCategoryPriority.Value)
            {
                _equipRanks = ParsePriority(Plugin.EquipCategoryPriority.Value, 6);
                _equipPriorityCache = Plugin.EquipCategoryPriority.Value;
            }
            return (cat >= 0 && cat < _equipRanks.Length) ? _equipRanks[cat] : cat;
        }

        // ============ 覆盖配置解析 ============

        private static Dictionary<int, int> _overrideCache;
        private static string _overrideCacheStr;

        private static int GetOverride(int id, string cfg)
        {
            if (_overrideCache == null || _overrideCacheStr != cfg)
            {
                _overrideCache = ParseOverride(cfg);
                _overrideCacheStr = cfg;
            }
            int v;
            if (_overrideCache.TryGetValue(id, out v)) return v;
            return -1;
        }

        private static Dictionary<int, int> ParseOverride(string s)
        {
            var dict = new Dictionary<int, int>();
            if (string.IsNullOrEmpty(s)) return dict;
            try
            {
                var parts = s.Split(',');
                foreach (var p in parts)
                {
                    var kv = p.Split(':');
                    if (kv.Length != 2) continue;
                    int id = int.Parse(kv[0].Trim());
                    int cat = int.Parse(kv[1].Trim());
                    dict[id] = cat;
                }
            }
            catch { }
            return dict;
        }

        // ============ 优先级字符串解析 ============

        private static int[] ParsePriority(string s, int count)
        {
            var ranks = new int[count];
            for (int i = 0; i < count; i++) ranks[i] = i;

            if (string.IsNullOrEmpty(s)) return ranks;

            try
            {
                var parts = s.Split(',');
                for (int rank = 0; rank < parts.Length && rank < count; rank++)
                {
                    int cat = int.Parse(parts[rank].Trim());
                    if (cat >= 0 && cat < count) ranks[cat] = rank;
                }
            }
            catch { }

            return ranks;
        }

        // ============ DB 反射 ============

        private static ItemData GetItemDb(Inventory inv)
        {
            if (_dbField == null)
                _dbField = AccessTools.Field(typeof(Inventory), "db");
            return _dbField?.GetValue(inv) as ItemData;
        }
    }
}