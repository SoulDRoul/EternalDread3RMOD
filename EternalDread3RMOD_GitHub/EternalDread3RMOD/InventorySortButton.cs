using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EternalDread3RMOD
{
    public class InventorySortButton : MonoBehaviour
    {
        private Text _label;
        private Button _btn;
        private InventoryUiCanvas _ui;

        public static void Attach(InventoryUiCanvas ui)
        {
            if (ui == null) return;

            var panel = ui.transform.Find("Panel");
            if (panel == null) return;

            if (panel.Find("SortButton") != null) return;

            // 找 UsableTab 下的 NextPage 作为模板
            var usable = panel.Find("UsableTab");
            if (usable == null) return;
            var template = usable.Find("NextPage");
            if (template == null) return;

            var go = Object.Instantiate(template.gameObject, panel);
            go.name = "SortButton";

            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(295f, -236f);
            }

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick = new Button.ButtonClickedEvent();
            }

            var txt = go.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = LabelFor((int)Plugin.SortRule.Value);
                txt.fontSize = 16;

                var trt = txt.GetComponent<RectTransform>();
                if (trt != null)
                {
                    trt.anchoredPosition += new Vector2(0f, -3f);
                }
            }

            var comp = go.AddComponent<InventorySortButton>();
            comp._ui = ui;
            comp._label = txt;
            comp._btn = btn;

            if (btn != null)
                btn.onClick.AddListener(comp.OnClick);

            Plugin.DebugLog("[InvSort] Sort button attached");
        }

        public void OnClick()
        {
            int rule = Plugin.SortRule.Value;
            rule = (rule + 1) % 4;
            Plugin.SortRule.Value = rule;

            if (_label != null) _label.text = LabelFor(rule);

            if (_ui != null && _ui.player != null)
            {
                var inv = _ui.player.GetComponent<Inventory>();
                if (inv != null)
                {
                    // 1. 排序
                    InventorySorter.SortAll(inv);

                    // 2. 缩容（只裁尾部全空页）
                    try
                    {
                        InventoryShrinkHelper.TrimItems(inv,
                            InventoryShrinkHelper.MinPlayerItems,
                            InventoryExpandHelper.ItemSlotsPerPage);

                        InventoryShrinkHelper.TrimEquipments(inv,
                            InventoryShrinkHelper.MinPlayerEquipments,
                            InventoryExpandHelper.EquipSlotsPerPage);
                    }
                    catch (System.Exception e)
                    {
                        Plugin.Log.LogError("[ED3R] InvSort shrink error: " + e);
                    }

                    // 3. 同步 UI 页数（maxPage 跟随实际数组长度）
                    try { InventoryUiPageHelper.Refresh(_ui); }
                    catch { }

                    // 4. 回到第 1 页 + 第 1 个栏位
                    ResetToFirstPage();
                }
            }
        }

        /// <summary>让 UI 回到第一页第一个栏位。</summary>
        private void ResetToFirstPage()
        {
            if (_ui == null) return;
            try
            {
                _ui.ResetPage();

                bool usable = _ui.usableTab != null && _ui.usableTab.activeSelf;
                bool equip = _ui.equipmentTab != null && _ui.equipmentTab.activeSelf;

                if (usable)
                    _ui.ShowItemTooltip(0);
                else if (equip)
                    _ui.ShowEquipmentTooltip(0);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] InvSort ResetToFirstPage error: " + e);
            }
        }

        private static string LabelFor(int rule)
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

    [HarmonyPatch(typeof(InventoryUiCanvas), "Start")]
    public static class Patch_InventorySortButton_Attach
    {
        static void Postfix(InventoryUiCanvas __instance)
        {
            InventorySortButton.Attach(__instance);
        }
    }
}