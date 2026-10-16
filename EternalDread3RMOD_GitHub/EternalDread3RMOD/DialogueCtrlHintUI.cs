using UnityEngine;
using UnityEngine.UI;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 对话 Ctrl 快进提示：在对话框面板左下角绘制 "Ctrl →"。
    /// 由 AnimateText 的 OnEnable/OnDisable 维护活跃引用，仅对话期间显示。
    /// </summary>
    public class DialogueCtrlHintUI : MonoBehaviour
    {
        // ============ 硬编码常量：Dialogue 段 ============
        // 原 Dialogue.CtrlHintOffsetX —— 提示相对对话框面板左下角的 X 偏移（像素，正值向右）
        private const int CTRL_HINT_OFFSET_X = 40;
        // 原 Dialogue.CtrlHintOffsetY —— 提示相对对话框面板左下角的 Y 偏移（像素，正值向上）
        private const int CTRL_HINT_OFFSET_Y = 178;
        // 原 Dialogue.CtrlHintFontSize —— 提示字号
        private const int CTRL_HINT_FONT_SIZE = 20;

        private static AnimateText _activeText;

        public static void NotifyShow(AnimateText t) { _activeText = t; }
        public static void NotifyHide(AnimateText t)
        {
            if (_activeText == t) _activeText = null;
        }

        private void OnGUI()
        {
            if (UiSuppress.All) return;
            if (Plugin.DialogueCtrlHintEnabled == null
                || !Plugin.DialogueCtrlHintEnabled.Value) return;

            var t = _activeText;
            if (t == null) return;

            // 兜底：销毁或未激活
            if (t.gameObject == null) { _activeText = null; return; }
            if (!t.gameObject.activeInHierarchy) return;

            RectTransform panel = FindPanel(t.transform);
            if (panel == null) return;

            // 面板世界左下角 → 屏幕坐标
            Vector3[] corners = new Vector3[4];
            panel.GetWorldCorners(corners);   // 0=BL, 1=TL, 2=TR, 3=BR

            Canvas canvas = panel.GetComponentInParent<Canvas>();
            Camera cam = (canvas != null
                          && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                         ? canvas.worldCamera : null;

            Vector2 bl = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            float guiX = bl.x;
            float guiYTop = Screen.height - bl.y;   // GUI y 从上往下

            float offX = CTRL_HINT_OFFSET_X;
            float offY = CTRL_HINT_OFFSET_Y;
            int fontSize = CTRL_HINT_FONT_SIZE;
            string text = Plugin.DialogueCtrlHintText.Value;
            if (string.IsNullOrEmpty(text)) text = "Ctrl →";

            Color mainCol = ParseColor(Plugin.DialogueCtrlHintColor.Value,
                new Color(0.78f, 0.78f, 0.78f, 0.7f));
            Color borderCol = ParseColor(Plugin.DialogueCtrlHintBorderColor.Value,
                new Color(0f, 0f, 0f, 0.5f));

            // 字体：优先用对话框本身的字体
            Font font = null;
            var txt = t.GetComponent<Text>();
            if (txt != null) font = txt.font;
            if (font == null)
            {
                string fontName = Plugin.ComboFontName.Value;
                if (!string.IsNullOrEmpty(fontName))
                {
                    try { font = FontLoader.GetFont(fontName); } catch { }
                }
            }

            const int w = 260;
            const int h = 40;
            float x = guiX + offX;
            float y = guiYTop - offY - h;   // 底部向上

            Rect rect = new Rect(x, y, w, h);

            var mainStyle = MakeStyle(fontSize, mainCol, font);
            var borderStyle = MakeStyle(fontSize, borderCol, font);

            DrawWithBorder(rect, text, mainStyle, borderStyle, 1);
        }

        private static RectTransform FindPanel(Transform t)
        {
            // 1) 名字精确匹配
            var cur = t;
            while (cur != null)
            {
                if (cur.name == "Dialogue" || cur.name == "DialogueBox")
                {
                    var rt = cur as RectTransform;
                    if (rt != null) return rt;
                }
                cur = cur.parent;
            }
            // 2) 向上找第一个带 Image 的祖先
            cur = t.parent;
            while (cur != null)
            {
                if (cur.GetComponent<Image>() != null && cur is RectTransform rt2)
                    return rt2;
                cur = cur.parent;
            }
            // 3) 直接父节点
            if (t.parent is RectTransform prt) return prt;
            // 4) 自身
            return t as RectTransform;
        }

        private static GUIStyle MakeStyle(int size, Color color, Font font)
        {
            var s = new GUIStyle(GUI.skin.label);
            s.alignment = TextAnchor.LowerLeft;
            s.fontSize = size;
            s.normal.textColor = color;
            if (font != null) s.font = font;
            return s;
        }

        private static void DrawWithBorder(Rect rect, string text,
            GUIStyle main, GUIStyle border, int offset)
        {
            if (offset > 0)
            {
                int[] dx = { -offset, 0, offset, -offset, offset, -offset, 0, offset };
                int[] dy = { -offset, -offset, -offset, 0, 0, offset, offset, offset };
                for (int i = 0; i < 8; i++)
                    GUI.Label(new Rect(rect.x + dx[i], rect.y + dy[i], rect.width, rect.height),
                        text, border);
            }
            GUI.Label(rect, text, main);
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
                int a = 255;
                if (parts.Length >= 4) a = int.Parse(parts[3].Trim());
                return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
            }
            catch { return fallback; }
        }
    }
}