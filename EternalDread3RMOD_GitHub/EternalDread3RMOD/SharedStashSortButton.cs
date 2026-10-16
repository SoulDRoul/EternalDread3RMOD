using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 仓库/共用仓库界面的整理按钮。
    ///
    /// 规则独立：
    ///   - 左侧按钮（共享箱子）：静态 LeftRule，独立于 Plugin.SortRule.Value
    ///   - 右侧按钮（玩家背包）：复用 Plugin.SortRule.Value，跟玩家主界面一致
    ///
    /// 点击行为：只切换自己侧的规则 + 只更新自己的 label + 用自己侧的规则重排。
    /// </summary>
    public class SharedStashSortButton : MonoBehaviour
    {
        // 共享箱子左侧（箱子自身）独立规则，跨界面持久
        public static int LeftRule = 1;

        private static FieldInfo _modeField;
        private static FieldInfo _pageField;
        private static FieldInfo _cPageField;
        private static FieldInfo _pagePlField;
        private static FieldInfo _cPagePlField;

        public SharedStashCanvas canvas;
        public bool isLeft;
        public Text label;

        private void OnEnable()
        {
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (label == null) return;
            label.text = LabelFor(GetCurrentRule());
        }

        private int GetCurrentRule()
        {
            return isLeft ? LeftRule : (int)Plugin.SortRule.Value;
        }

        private void SetCurrentRule(int rule)
        {
            if (isLeft)
                LeftRule = rule;
            else
                Plugin.SortRule.Value = rule;
        }

        public void OnClick()
        {
            if (canvas == null) return;

            // 1. 只切换当前侧的规则
            int rule = (GetCurrentRule() + 1) % 4;
            SetCurrentRule(rule);

            // 2. 只更新自己的 label
            RefreshLabel();

            // 3. 用当前侧的规则排序
            int mode = GetMode();
            Inventory inv = null;
            if (isLeft)
                inv = canvas.stash;
            else if (canvas.player != null)
                inv = canvas.player.GetComponent<Inventory>();
            if (inv == null) return;

            if (mode == 0)
                InventorySorter.SortItems(inv, rule);
            else
                InventorySorter.SortEquip(inv, rule);

            // 4. 整理后顺带缩容
            TrimAfterSort(inv, mode);

            // 5. 回到第 1 页
            ResetToFirstPage();

            // 6. 刷新 UI
            canvas.UpdateUi();
        }

        private void TrimAfterSort(Inventory inv, int mode)
        {
            if (inv == null) return;
            try
            {
                if (isLeft)
                {
                    if (mode == 0)
                        InventoryShrinkHelper.TrimItems(inv,
                            InventoryShrinkHelper.MinSharedStash,
                            InventoryExpandHelper.EquipSlotsPerPage);
                    else
                        InventoryShrinkHelper.TrimEquipments(inv,
                            InventoryShrinkHelper.MinSharedStash,
                            InventoryExpandHelper.EquipSlotsPerPage);
                }
                else
                {
                    if (mode == 0)
                        InventoryShrinkHelper.TrimItems(inv,
                            InventoryShrinkHelper.MinPlayerItems,
                            InventoryExpandHelper.ItemSlotsPerPage);
                    else
                        InventoryShrinkHelper.TrimEquipments(inv,
                            InventoryShrinkHelper.MinPlayerEquipments,
                            InventoryExpandHelper.EquipSlotsPerPage);
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] Stash sort shrink error: " + e);
            }
        }

        private void ResetToFirstPage()
        {
            if (canvas == null) return;
            try
            {
                if (_pageField == null) _pageField = AccessTools.Field(typeof(SharedStashCanvas), "page");
                if (_cPageField == null) _cPageField = AccessTools.Field(typeof(SharedStashCanvas), "cPage");
                if (_pagePlField == null) _pagePlField = AccessTools.Field(typeof(SharedStashCanvas), "pagePl");
                if (_cPagePlField == null) _cPagePlField = AccessTools.Field(typeof(SharedStashCanvas), "cPagePl");

                if (isLeft)
                {
                    if (_pageField != null) _pageField.SetValue(canvas, 0);
                    if (_cPageField != null) _cPageField.SetValue(canvas, 0);
                    if (canvas.pageText != null) canvas.pageText.text = "1";
                }
                else
                {
                    if (_pagePlField != null) _pagePlField.SetValue(canvas, 0);
                    if (_cPagePlField != null) _cPagePlField.SetValue(canvas, 0);
                    if (canvas.pageTextPl != null) canvas.pageTextPl.text = "1";
                }

                canvas.selection = 0;
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] Stash ResetToFirstPage error: " + e);
            }
        }

        private int GetMode()
        {
            if (_modeField == null)
                _modeField = AccessTools.Field(typeof(SharedStashCanvas), "mode");
            if (_modeField == null) return 0;
            return (int)_modeField.GetValue(canvas);
        }

        public static string LabelFor(int rule)
        {
            switch (rule)
            {
                case 0: return "I";
                case 1: return "C";
                case 2: return "N";
                case 3: return "Q";
            }
            return "?";
        }
    }

    [HarmonyPatch(typeof(SharedStashCanvas), "Start")]
    public static class Patch_SharedStashCanvas_Start_SortButton
    {
        static void Postfix(SharedStashCanvas __instance)
        {
            if (!Plugin.AutoSortOnOpen.Value) return;
            if (__instance.mainPanel == null) return;

            var mainPanel = __instance.mainPanel.transform;

            var stashPanel = mainPanel.Find("StashPanel");
            if (stashPanel != null)
                AttachSortButton(stashPanel, __instance, true);

            var invPanel = mainPanel.Find("InventoryPanel");
            if (invPanel != null)
                AttachSortButton(invPanel, __instance, false);
        }

        static void AttachSortButton(Transform panel, SharedStashCanvas canvas, bool isLeft)
        {
            if (panel.Find("SortButton") != null) return;

            var template = panel.Find("NextPageButton");
            if (template == null) return;

            var go = Object.Instantiate(template.gameObject, panel);
            go.name = "SortButton";

            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = new Vector2(75f, -201f);

            var btn = go.GetComponent<Button>();
            if (btn != null)
                btn.onClick = new Button.ButtonClickedEvent();

            var txt = go.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.fontSize = 18;

                // 按当前侧的实际规则初始化 label
                int initRule = isLeft
                    ? SharedStashSortButton.LeftRule
                    : (int)Plugin.SortRule.Value;
                txt.text = SharedStashSortButton.LabelFor(initRule);
            }

            var comp = go.AddComponent<SharedStashSortButton>();
            comp.canvas = canvas;
            comp.isLeft = isLeft;
            comp.label = txt;

            if (btn != null)
                btn.onClick.AddListener(comp.OnClick);
        }
    }
}