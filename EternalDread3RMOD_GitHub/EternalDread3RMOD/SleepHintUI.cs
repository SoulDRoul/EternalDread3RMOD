using System;
using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>睡眠中显示提示：按 A/D 切倍率。</summary>
    public class SleepHintUI : MonoBehaviour
    {
        private const int HintFontSize = 22;

        // ============ 字体缓存 ============
        private static Font _cachedFont;
        private static string _cachedFontName;

        // ============ GUIStyle 缓存 ============
        // 按 (fontSize, color) 缓存。主字 + 描边字共 2 个。
        private static readonly Dictionary<long, GUIStyle> _styleCache = new Dictionary<long, GUIStyle>();

        // 边框偏移表
        private static readonly int[] BorderDx = { -2, 0, 2, -2, 2, -2, 0, 2 };
        private static readonly int[] BorderDy = { -2, -2, -2, 0, 0, 2, 2, 2 };

        private static readonly Color MainColor = new Color(1f, 1f, 1f, 0.8f);
        private static readonly Color BorderColor = new Color(0f, 0f, 0f, 0.8f);

        private static Font GetCachedFont()
        {
            string fontName = Plugin.ComboFontName.Value;
            if (fontName == _cachedFontName) return _cachedFont;

            _cachedFontName = fontName;
            if (string.IsNullOrEmpty(fontName))
            {
                _cachedFont = null;
                return null;
            }
            try
            {
                _cachedFont = FontLoader.GetFont(fontName);
            }
            catch
            {
                _cachedFont = null;
            }
            return _cachedFont;
        }

        private static long StyleKey(int fontSize, Color color)
        {
            long key = fontSize;
            key = key * 397 + (int)(color.r * 255f + 0.5f);
            key = key * 397 + (int)(color.g * 255f + 0.5f);
            key = key * 397 + (int)(color.b * 255f + 0.5f);
            key = key * 397 + (int)(color.a * 255f + 0.5f);
            return key;
        }

        private static GUIStyle GetStyle(int fontSize, Color color)
        {
            Font font = GetCachedFont();
            long key = StyleKey(fontSize, color);

            if (_styleCache.TryGetValue(key, out var cached) && cached.font == font)
                return cached;

            var s = new GUIStyle(GUI.skin.label);
            s.alignment = TextAnchor.MiddleCenter;
            if (font != null) s.font = font;
            s.fontStyle = FontStyle.Bold;
            s.fontSize = fontSize;
            s.normal.textColor = color;
            _styleCache[key] = s;
            return s;
        }

        private void OnGUI()
        {
            if (UiSuppress.All) return;
            if (!Plugin.SleepSpeedControlEnabled.Value) return;
            if (!SleepSpeedController.IsSleeping) return;

            var main = GetStyle(HintFontSize, MainColor);
            var border = GetStyle(HintFontSize, BorderColor);

            string line1 = "A  −     D  +";
            string line2 = "Speed: " + SleepSpeedController.CurrentLabel;

            float cx = Screen.width / 2f;
            float cy = Screen.height * 0.85f;

            DrawCentered(cx, cy - 18, line1, main, border);
            DrawCentered(cx, cy + 18, line2, main, border);
        }

        private void DrawCentered(float cx, float cy, string text,
            GUIStyle main, GUIStyle border)
        {
            const int w = 500;
            const int h = 30;
            Rect r = new Rect(cx - w / 2f, cy - h / 2f, w, h);

            for (int i = 0; i < 8; i++)
                GUI.Label(new Rect(r.x + BorderDx[i], r.y + BorderDy[i], r.width, r.height), text, border);

            GUI.Label(r, text, main);
        }
    }
}