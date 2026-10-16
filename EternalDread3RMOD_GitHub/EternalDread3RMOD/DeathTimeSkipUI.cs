using System;
using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>死亡跳过时间的屏幕提示："X HOURS LATER" 迷因风格。</summary>
    public class DeathTimeSkipUI : MonoBehaviour
    {
        public static DeathTimeSkipUI Instance;

        // ===== 固定视觉参数（不需要配置） =====
        private static readonly Color FixedTextColor = new Color(245f / 255f, 245f / 255f, 245f / 255f);
        private static readonly Color FixedBorderColor = new Color(20f / 255f, 20f / 255f, 20f / 255f);
        private const int FixedBorderOffset = 3;
        private static readonly bool FixedBold = true;
        private static readonly bool FixedItalic = false;
        private const float FixedFadeInSec = 2f;
        private const float FixedHoldSec = 1f;
        private const float FixedFadeOutSec = 3f;

        // ============ 字体缓存 ============
        private static Font _cachedFont;
        private static string _cachedFontName;

        // ============ GUIStyle 缓存 ============
        private class StylePair
        {
            public GUIStyle Main;
            public GUIStyle Border;
            public Font Font;
        }
        private static readonly Dictionary<int, StylePair> _stylePairs = new Dictionary<int, StylePair>();

        // 边框偏移表（对应 FixedBorderOffset = 3）
        private static readonly int[] BorderDx = { -3, 0, 3, -3, 3, -3, 0, 3 };
        private static readonly int[] BorderDy = { -3, -3, -3, 0, 0, 3, 3, 3 };

        private static float _showStartTime = -999f;
        private static string _showText = "";
        private static float _totalDuration = -1f;

        private void Awake() { Instance = this; }

        public static void Show(float hours)
        {
            if (Instance == null) return;
            if (!Plugin.DeathTimeSkipUIEnabled.Value) return;

            _showText = FormatDuration(hours);
            if (string.IsNullOrEmpty(_showText)) return;

            _showStartTime = Time.unscaledTime;
            _totalDuration = FixedFadeInSec + FixedHoldSec + FixedFadeOutSec;
        }

        private void OnGUI()
        {
            if (!Plugin.DeathTimeSkipUIEnabled.Value) return;
            if (_showStartTime < 0f) return;

            float elapsed = Time.unscaledTime - _showStartTime;
            if (elapsed > _totalDuration)
            {
                _showStartTime = -999f;
                return;
            }

            float alpha;
            if (elapsed < FixedFadeInSec)
                alpha = elapsed / FixedFadeInSec;
            else if (elapsed < FixedFadeInSec + FixedHoldSec)
                alpha = 1f;
            else
                alpha = 1f - (elapsed - FixedFadeInSec - FixedHoldSec) / FixedFadeOutSec;
            alpha = Mathf.Clamp01(alpha);

            int fontSize = Plugin.DeathTimeSkipUIFontSize.Value;
            float cy = Screen.height * Plugin.DeathTimeSkipUICenterYPercent.Value / 100f;

            Color textColor = FixedTextColor; textColor.a *= alpha;
            Color borderColor = FixedBorderColor; borderColor.a *= alpha;

            var pair = GetStylePair(fontSize, FixedBold, FixedItalic);
            pair.Main.normal.textColor = textColor;
            pair.Border.normal.textColor = borderColor;

            DrawCentered(Screen.width / 2f, cy, _showText, pair.Main, pair.Border, FixedBorderOffset);
        }

        // ============ 字体缓存 ============
        private static Font GetCachedFont()
        {
            string name = Plugin.ComboFontName.Value;
            if (name == _cachedFontName) return _cachedFont;

            _cachedFontName = name;
            if (string.IsNullOrEmpty(name))
            {
                _cachedFont = null;
                return null;
            }
            try { _cachedFont = FontLoader.GetFont(name); }
            catch { _cachedFont = null; }
            return _cachedFont;
        }

        private static int StyleKey(int fontSize, bool bold, bool italic)
        {
            return fontSize * 4 + (bold ? 2 : 0) + (italic ? 1 : 0);
        }

        private static StylePair GetStylePair(int fontSize, bool bold, bool italic)
        {
            Font font = GetCachedFont();
            int key = StyleKey(fontSize, bold, italic);

            if (_stylePairs.TryGetValue(key, out var pair) && pair.Font == font)
                return pair;

            pair = new StylePair
            {
                Main = CreateStyle(fontSize, Color.white, bold, italic, font),
                Border = CreateStyle(fontSize, Color.black, bold, italic, font),
                Font = font
            };
            _stylePairs[key] = pair;
            return pair;
        }

        private static GUIStyle CreateStyle(int fontSize, Color color,
            bool bold, bool italic, Font font)
        {
            var s = new GUIStyle(GUI.skin.label);
            s.alignment = TextAnchor.MiddleCenter;

            if (font != null) s.font = font;

            if (bold && italic) s.fontStyle = FontStyle.BoldAndItalic;
            else if (bold) s.fontStyle = FontStyle.Bold;
            else if (italic) s.fontStyle = FontStyle.Italic;
            else s.fontStyle = FontStyle.Normal;

            s.fontSize = fontSize;
            s.normal.textColor = color;
            return s;
        }

        private void DrawCentered(float cx, float cy, string text,
            GUIStyle main, GUIStyle border, int offset)
        {
            const int w = 1500;
            const int h = 200;
            Rect r = new Rect(cx - w / 2f, cy - h / 2f, w, h);

            if (offset > 0)
            {
                for (int i = 0; i < 8; i++)
                    GUI.Label(new Rect(r.x + BorderDx[i], r.y + BorderDy[i], r.width, r.height),
                        text, border);
            }
            GUI.Label(r, text, main);
        }

        /// <summary>方案 2：自动升单位。天 > 小时 > 分钟，各段只在非 0 时出现。</summary>
        public static string FormatDuration(float hours)
        {
            if (hours <= 0f) return "";

            int totalMinutes = Mathf.Max(1, Mathf.RoundToInt(hours * 60f));
            int days = totalMinutes / (24 * 60);
            int rem = totalMinutes % (24 * 60);
            int h = rem / 60;
            int m = rem % 60;

            string body = "";
            if (days > 0)
                body += (days == 1 ? "1 DAY" : days + " DAYS");
            if (h > 0)
            {
                if (body.Length > 0) body += " ";
                body += (h == 1 ? "1 HOUR" : h + " HOURS");
            }
            if (m > 0)
            {
                if (body.Length > 0) body += " ";
                body += (m == 1 ? "1 MINUTE" : m + " MINUTES");
            }
            if (body.Length == 0) return "";

            string template = Plugin.DeathTimeSkipUITextTemplate.Value;
            if (string.IsNullOrEmpty(template)) template = "{0} LATER......";

            try { return string.Format(template, body); }
            catch { return body + " LATER......"; }
        }
    }
}