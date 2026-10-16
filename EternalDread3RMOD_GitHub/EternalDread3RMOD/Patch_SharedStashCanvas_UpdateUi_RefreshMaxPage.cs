using System;
using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 共享箱子 UI 每次刷新时，按实际数组长度重算 maxPage / maxPagePl。
    ///
    /// SetMaxPage 只在 OpenStash / SwitchMode 时被调用；玩家在箱子里存物品触发动态扩容后
    /// maxPage 不会更新，导致新页翻不到。
    /// 这里 Patch UpdateUi 的 Prefix，实时重算但**不重置当前页**。
    ///
    /// mode: 0 = items, 1 = equipments。
    /// </summary>
    [HarmonyPatch(typeof(SharedStashCanvas), "UpdateUi")]
    public static class Patch_SharedStashCanvas_UpdateUi_RefreshMaxPage
    {
        private static System.Reflection.FieldInfo _maxPageField;
        private static System.Reflection.FieldInfo _maxPagePlField;
        private static System.Reflection.FieldInfo _modeField;

        // ============ 缓存 ============
        // 缓存 Inventory 组件 + 上次写入的 maxPage/maxPagePl，
        // 避免每次 UpdateUi 都 GetComponent + 反射 GetValue。
        private static SharedStashCanvas _cachedCanvas;
        private static Inventory _cachedInventory;
        private static int _lastMode = -1;
        private static int _lastWrittenMaxPage = -1;
        private static int _lastWrittenMaxPagePl = -1;

        static void Prefix(SharedStashCanvas __instance)
        {
            if (__instance == null) return;
            if (Plugin.InventoryExpandEnabled == null
                || !Plugin.InventoryExpandEnabled.Value) return;

            if (_maxPageField == null)
                _maxPageField = AccessTools.Field(typeof(SharedStashCanvas), "maxPage");
            if (_maxPagePlField == null)
                _maxPagePlField = AccessTools.Field(typeof(SharedStashCanvas), "maxPagePl");
            if (_modeField == null)
                _modeField = AccessTools.Field(typeof(SharedStashCanvas), "mode");

            if (_maxPageField == null || _maxPagePlField == null || _modeField == null) return;

            try
            {
                var stash = __instance.stash;
                var player = __instance.player;
                if (stash == null || player == null) return;

                // 缓存 Inventory 组件（canvas 变化时失效）
                if (!ReferenceEquals(_cachedCanvas, __instance))
                {
                    _cachedCanvas = __instance;
                    _cachedInventory = null;
                    _lastMode = -1;
                    _lastWrittenMaxPage = -1;
                    _lastWrittenMaxPagePl = -1;
                }
                if (_cachedInventory == null)
                {
                    _cachedInventory = player.GetComponent<Inventory>();
                    if (_cachedInventory == null) return;
                }
                var inv = _cachedInventory;

                // mode 只读一次，变化时失效缓存
                int mode = (int)_modeField.GetValue(__instance);
                if (mode != _lastMode)
                {
                    _lastMode = mode;
                    _lastWrittenMaxPage = -1;
                    _lastWrittenMaxPagePl = -1;
                }

                int stashPageSize = (__instance.stashUi != null && __instance.stashUi.Length > 0)
                    ? __instance.stashUi.Length : 8;
                int playerPageSize = (__instance.playerUi != null && __instance.playerUi.Length > 0)
                    ? __instance.playerUi.Length : 8;

                int stashLen = mode == 0 ? (stash.items != null ? stash.items.Length : 0)
                                          : (stash.equipments != null ? stash.equipments.Length : 0);
                int playerLen = mode == 0 ? (inv.items != null ? inv.items.Length : 0)
                                          : (inv.equipments != null ? inv.equipments.Length : 0);

                int newMaxPage = (stashLen + stashPageSize - 1) / stashPageSize;
                int newMaxPagePl = (playerLen + playerPageSize - 1) / playerPageSize;
                if (newMaxPage < 1) newMaxPage = 1;
                if (newMaxPagePl < 1) newMaxPagePl = 1;

                // 与上次写入值比较，跳过反射 GetValue
                if (_lastWrittenMaxPage != newMaxPage)
                {
                    _maxPageField.SetValue(__instance, newMaxPage);
                    _lastWrittenMaxPage = newMaxPage;
                }
                if (_lastWrittenMaxPagePl != newMaxPagePl)
                {
                    _maxPagePlField.SetValue(__instance, newMaxPagePl);
                    _lastWrittenMaxPagePl = newMaxPagePl;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("[ED3R] SharedStash UpdateUi refresh error: " + e);
            }
        }
    }
}