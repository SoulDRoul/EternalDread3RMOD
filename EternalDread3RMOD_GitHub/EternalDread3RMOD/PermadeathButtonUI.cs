using UnityEngine;
using UnityEngine.UI;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 创角界面的永久死亡模式开关。
    /// 复用游戏原生 Tab 黑方块模板，Instantiate 后改符号、清事件。
    /// </summary>
    public class PermadeathButtonUI : MonoBehaviour
    {
        // ============ 硬编码常量：Permadeath 按钮几何 ============
        // 原 Permadeath.ButtonAnchorX —— 相对屏幕中心的 X 偏移（像素，基于 1600x900 参考分辨率）
        private const int BUTTON_ANCHOR_X = -438;
        // 原 Permadeath.ButtonAnchorY —— 相对屏幕中心的 Y 偏移（像素）
        private const int BUTTON_ANCHOR_Y = -330;
        // 原 Permadeath.ButtonWidth —— 开关宽度（像素）
        private const int BUTTON_WIDTH = 125;
        // 原 Permadeath.ButtonHeight —— 开关高度（像素）
        private const int BUTTON_HEIGHT = 55;
        // 原 Permadeath.ButtonFontSize —— 符号字号
        private const int BUTTON_FONT_SIZE = 40;

        public static PermadeathButtonUI Instance;

        private GameObject _btnGo;
        private Image _btnImage;
        private Text _btnText;
        private bool _built = false;

        private void Awake()
        {
            Instance = this;
        }

        public static void Show()
        {
            if (Instance == null) return;
            if (Plugin.PermadeathShowInCharacterCreation == null
                || !Plugin.PermadeathShowInCharacterCreation.Value) return;

            Instance.EnsureBuilt();
            if (Instance._btnGo == null) return;

            Instance._btnGo.SetActive(true);
            Instance.ApplyPosition();
            Instance.RefreshVisual();
        }

        public static void Hide()
        {
            if (Instance == null) return;
            if (Instance._btnGo != null)
                Instance._btnGo.SetActive(false);
        }

        // ============================================================
        // 构建
        // ============================================================

        private void EnsureBuilt()
        {
            // 场景切换后旧按钮被销毁，需要重建
            if (_built && _btnGo == null)
            {
                _built = false;
                _btnText = null;
                _btnImage = null;
            }

            if (_built) return;

            var cc = Object.FindObjectOfType<CharCreation>();
            if (cc == null)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] PermadeathButtonUI: CharCreation not found");
                return;
            }

            // 模板路径来自 dump：
            //   CharCreation/CharCreationPanel/KeyboardButton/Image (1)
            Transform template = null;
            try
            {
                // 从 Canvas 开始找（CharCreation 本身无子节点）
                Canvas canvas = null;
                var allCanvas = Object.FindObjectsOfType<Canvas>();
                for (int i = 0; i < allCanvas.Length; i++)
                {
                    if (allCanvas[i] == null) continue;
                    if (!allCanvas[i].gameObject.activeInHierarchy) continue;
                    // 找含有 CharCreationPanel 子节点的 Canvas
                    if (allCanvas[i].transform.Find("CharCreationPanel") != null)
                    {
                        canvas = allCanvas[i];
                        break;
                    }
                }

                if (canvas != null)
                {
                    template = canvas.transform.Find(
                        "CharCreationPanel/KeyboardButton/Image (1)");
                }
            }
            catch { }

            if (template == null)
            {
                if (Plugin.Log != null)
                    Plugin.DebugLog("[ED3R] template found = " + (template != null));
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] PermadeathButtonUI: template not found");
                return;
            }

            // 挂到 CharCreationPanel 下（和 KeyboardButton 平级）
            Transform parent = template.parent != null ? template.parent.parent : null;
            if (parent == null) parent = template.parent;
            if (parent == null)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] PermadeathButtonUI: parent not found");
                return;
            }

            // Instantiate 模板
            _btnGo = Object.Instantiate(template.gameObject);
            _btnGo.name = "ED3R_PermadeathToggle";
            _btnGo.transform.SetParent(parent, false);
            _btnGo.SetActive(true);

            // 清理子物体：只保留一个 Text，其余删除
            var allTexts = _btnGo.GetComponentsInChildren<Text>(true);
            if (allTexts.Length > 0)
            {
                _btnText = allTexts[0];
                // 删除其余 Text
                for (int i = 1; i < allTexts.Length; i++)
                {
                    if (allTexts[i] != null)
                        Object.Destroy(allTexts[i].gameObject);
                }
                // 让 Text 填满整个按钮
                var rt = _btnText.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                _btnText.alignment = TextAnchor.MiddleCenter;
                _btnText.fontSize = BUTTON_FONT_SIZE;
                _btnText.text = Plugin.PermadeathButtonSymbol.Value;
                _btnText.color = new Color(0.94f, 0.94f, 0.94f);
                // 不移除字体，继承模板
            }
            else
            {
                // 没有 Text 就自己建一个
                var txtGo = new GameObject("Symbol");
                txtGo.transform.SetParent(_btnGo.transform, false);
                var txtRt = txtGo.AddComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.sizeDelta = Vector2.zero;
                txtRt.anchoredPosition = Vector2.zero;
                _btnText = txtGo.AddComponent<Text>();
                _btnText.alignment = TextAnchor.MiddleCenter;
                _btnText.fontSize = BUTTON_FONT_SIZE;
                _btnText.text = Plugin.PermadeathButtonSymbol.Value;
                _btnText.color = new Color(0.94f, 0.94f, 0.94f);
                _btnText.raycastTarget = false;
                try
                {
                    _btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch { }
            }

            // Image
            _btnImage = _btnGo.GetComponent<Image>();
            if (_btnImage == null)
            {
                var imgChild = _btnGo.GetComponentInChildren<Image>();
                if (imgChild != null) _btnImage = imgChild;
            }

            // Button onClick 清理
            var btn = _btnGo.GetComponent<Button>();
            if (btn == null) btn = _btnGo.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnClick);
            }

            _built = true;

            if (Plugin.Log != null)
                Plugin.DebugLog("[ED3R] PermadeathButtonUI built");
        }

        private void ApplyPosition()
        {
            if (_btnGo == null) return;
            var rt = _btnGo.GetComponent<RectTransform>();
            if (rt == null) return;

            // 相对 Canvas 中心
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(
                BUTTON_ANCHOR_X,
                BUTTON_ANCHOR_Y);

            rt.sizeDelta = new Vector2(
                BUTTON_WIDTH,
                BUTTON_HEIGHT);
        }

        // ============================================================
        // 交互
        // ============================================================

        private void OnClick()
        {
            PermadeathState.PendingPermadeath = !PermadeathState.PendingPermadeath;
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            if (_btnImage == null || _btnText == null) return;

            bool on = PermadeathState.PendingPermadeath;

            // 方案 B：完全反色
            Color bg = on
                ? ParseColor(Plugin.PermadeathButtonOnBgColor.Value, new Color(0.78f, 0.24f, 0.24f))
                : ParseColor(Plugin.PermadeathButtonOffBgColor.Value, new Color(0.184f, 0.184f, 0.184f));
            Color fg = on
                ? ParseColor(Plugin.PermadeathButtonOnTextColor.Value, new Color(0.08f, 0.08f, 0.08f))
                : ParseColor(Plugin.PermadeathButtonOffTextColor.Value, new Color(0.94f, 0.94f, 0.94f));

            _btnImage.color = bg;
            _btnText.color = fg;
            _btnText.text = Plugin.PermadeathButtonSymbol.Value;
            _btnText.fontSize = BUTTON_FONT_SIZE;
        }

        private static Color ParseColor(string str, Color fallback)
        {
            try
            {
                var parts = str.Split(',');
                if (parts.Length < 3) return fallback;
                int r = int.Parse(parts[0].Trim());
                int g = int.Parse(parts[1].Trim());
                int b = int.Parse(parts[2].Trim());
                return new Color(r / 255f, g / 255f, b / 255f);
            }
            catch { return fallback; }
        }
    }
}