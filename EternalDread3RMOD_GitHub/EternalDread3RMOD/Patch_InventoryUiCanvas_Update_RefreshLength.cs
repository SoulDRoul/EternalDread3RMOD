using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 修复动态扩容后 UI 不刷新的问题。
    ///
    /// InventoryUiCanvas 的 itemLength / eqLength 在 Start 里缓存一次，
    /// 之后不再更新。动态扩容后，翻页到超出旧长度的页时，
    /// 循环条件 `i + cPage < itemLength` 不成立 → 图标不刷新 → 显示残留。
    ///
    /// Prefix Update 里每帧同步这两个字段（只在值变化时写），开销可忽略。
    /// </summary>
    [HarmonyPatch(typeof(InventoryUiCanvas), "Update")]
    public static class Patch_InventoryUiCanvas_Update_RefreshLength
    {
        private static System.Reflection.FieldInfo _itemLenField;
        private static System.Reflection.FieldInfo _eqLenField;

        // ============ Inventory 组件缓存 ============
        // InventoryUiCanvas 是单例，其绑定的 Inventory 也相对稳定。
        // 缓存 (canvas -> inventory)，避免每帧 GetComponent<Inventory>()。
        private static InventoryUiCanvas _cachedCanvas;
        private static Inventory _cachedInventory;

        // ============ 上次写入的长度缓存 ============
        // 用本地值代替反射 GetValue：只有"想要的值 != 上次写过的值"才走反射。
        // 反射 SetValue 仍会装箱，但调用频率从"每帧 2 次"降到"变化时 1 次"。
        private static int _lastWrittenItemLen = -1;
        private static int _lastWrittenEqLen = -1;

        static void Prefix(InventoryUiCanvas __instance)
        {
            if (__instance == null) return;

            // 缓存 Inventory 组件
            if (!ReferenceEquals(_cachedCanvas, __instance))
            {
                _cachedCanvas = __instance;
                _cachedInventory = null;
                _lastWrittenItemLen = -1;
                _lastWrittenEqLen = -1;
            }
            if (_cachedInventory == null)
            {
                var player = __instance.player;
                if (player == null) player = GlobalVar.mainPlayer;
                if (player == null) return;
                _cachedInventory = player.GetComponent<Inventory>();
                if (_cachedInventory == null) return;
            }

            if (_itemLenField == null)
                _itemLenField = AccessTools.Field(typeof(InventoryUiCanvas), "itemLength");
            if (_eqLenField == null)
                _eqLenField = AccessTools.Field(typeof(InventoryUiCanvas), "eqLength");
            if (_itemLenField == null || _eqLenField == null) return;

            try
            {
                int wantItem = _cachedInventory.items != null ? _cachedInventory.items.Length : 0;
                if (wantItem != _lastWrittenItemLen)
                {
                    _itemLenField.SetValue(__instance, wantItem);
                    _lastWrittenItemLen = wantItem;
                }

                int wantEq = _cachedInventory.equipments != null ? _cachedInventory.equipments.Length : 0;
                if (wantEq != _lastWrittenEqLen)
                {
                    _eqLenField.SetValue(__instance, wantEq);
                    _lastWrittenEqLen = wantEq;
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] RefreshLength error: " + e);
            }
        }
    }
}