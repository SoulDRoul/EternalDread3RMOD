using System;
using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    public class AlertUI : MonoBehaviour
    {
        // ============ 硬编码常量：Alert 段 ============
        // 原 Alert.DurationSec —— 感叹号显示时长（秒）
        private const float ALERT_DURATION_SEC = 0.8f;
        // 原 Alert.FadeStartPercent —— 淡出起点，占总时长的百分比
        private const int ALERT_FADE_START_PERCENT = 75;
        // 原 Alert.BorderOffset —— 感叹号描边粗细（像素）
        private const int ALERT_BORDER_OFFSET = 2;
        // 原 Alert.ExtraHeightOffset —— 在单位头部之上额外增加的高度（像素）
        private const int ALERT_EXTRA_HEIGHT = 30;

        // ============ 硬编码常量：AI.Noise 段（视觉相关） ============
        // 原 AI.Noise.VisualDurationSec —— 噪音 !! 提示的持续时长（秒）
        private const float NOISE_VISUAL_DURATION_SEC = 1.5f;
        // 原 AI.Noise.BorderOffset —— 噪音 !! 的红色边框粗细（像素）
        private const int NOISE_BORDER_OFFSET = 3;

        public static AlertUI Instance;

        private class AlertInstance
        {
            public float posX;
            public float posZ;
            public float baseHeight;     // 相对敌人 root 的高度
            public Transform followY;    // Y 跟随对象
            public float startTime;
            public bool isNoise;
            public float duration;
        }

        private static List<AlertInstance> _alerts = new List<AlertInstance>();

        // ============ 颜色解析缓存 ============
        private static readonly Dictionary<string, Color> _colorCache = new Dictionary<string, Color>();

        // ============ 字体缓存 ============
        // 字体名 = AlertFontName；留空时回退 ComboFontName。
        private static Font _cachedFont;
        private static string _cachedFontName;

        // ============ GUIStyle 缓存 ============
        // 按 (fontSize, bold, italic) 缓存一对 Main + Border。绘制时只改 normal.textColor。
        private class StylePair
        {
            public GUIStyle Main;
            public GUIStyle Border;
            public Font Font;
        }
        private static readonly Dictionary<int, StylePair> _stylePairs = new Dictionary<int, StylePair>();

        // 边框偏移表
        private static readonly int[] BorderDx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] BorderDy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        private void Awake()
        {
            Instance = this;
        }

        public static void Spawn(Transform target)
        {
            Spawn(target, false);
        }

        public static void SpawnNoise(Transform target)
        {
            Spawn(target, true);
        }

        private static void Spawn(Transform target, bool isNoise)
        {
            if (Instance == null || target == null) return;
            if (!Plugin.AlertEnabled.Value) return;

            int maxCount = Plugin.AlertMaxCount.Value;
            if (maxCount <= 0) return;
            if (_alerts.Count >= maxCount) return;

            float topY = target.position.y + 2f;
            var colliders = target.GetComponentsInChildren<Collider>();
            if (colliders != null && colliders.Length > 0)
            {
                float maxY = float.MinValue;
                bool found = false;
                foreach (var c in colliders)
                {
                    if (c == null) continue;
                    if (c.bounds.max.y > maxY)
                    {
                        maxY = c.bounds.max.y;
                        found = true;
                    }
                }
                if (found) topY = maxY;
            }

            AlertInstance a = new AlertInstance();
            a.posX = target.position.x;
            a.posZ = target.position.z;
            a.baseHeight = topY - target.position.y;
            a.followY = target;
            a.startTime = Time.time;
            a.isNoise = isNoise;
            a.duration = isNoise
                ? NOISE_VISUAL_DURATION_SEC
                : ALERT_DURATION_SEC;

            _alerts.Add(a);
        }

        private void OnGUI()
        {
            if (UiSuppress.All) return;
            if (!Plugin.AlertEnabled.Value) return;
            if (_alerts.Count == 0) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            int fadeStart = ALERT_FADE_START_PERCENT;
            if (fadeStart < 0) fadeStart = 0;
            if (fadeStart > 100) fadeStart = 100;

            Color normalTextColor = ParseColor(Plugin.AlertTextColor.Value,
                new Color(0.86f, 0.12f, 0.12f, 1f));
            Color normalBorderColor = ParseColor(Plugin.AlertBorderColor.Value, Color.white);

            // 循环外读一次配置
            int normalFontSize = Plugin.AlertFontSize.Value;
            int normalBorderOffset = ALERT_BORDER_OFFSET;
            float extraY = ALERT_EXTRA_HEIGHT;

            int noiseFontSize = Plugin.AlertFontSize.Value;
            int noiseBorderOffset = NOISE_BORDER_OFFSET;
            Color noiseMainColor = Color.black;
            Color noiseBorderColor = Color.red;

            string noiseSymbol = Plugin.NoiseSymbol.Value;
            if (string.IsNullOrEmpty(noiseSymbol)) noiseSymbol = "!!";

            string alertSymbol = Plugin.AlertSymbol.Value;

            bool bold = Plugin.AlertBold.Value;
            bool italic = Plugin.AlertItalic.Value;

            for (int i = _alerts.Count - 1; i >= 0; i--)
            {
                var a = _alerts[i];
                float elapsed = Time.time - a.startTime;
                float duration = a.duration;

                if (elapsed > duration)
                {
                    _alerts.RemoveAt(i);
                    continue;
                }

                float worldY;
                if (a.followY != null)
                    worldY = a.followY.position.y + a.baseHeight;
                else
                    worldY = a.baseHeight;

                Vector3 world = new Vector3(a.posX, worldY, a.posZ);
                Vector3 screen = cam.WorldToScreenPoint(world);
                if (screen.z <= 0f) continue;

                float guiY = Screen.height - screen.y;

                float fadeStartTime = duration * fadeStart / 100f;
                float alpha = 1f;
                if (elapsed > fadeStartTime && duration > fadeStartTime)
                    alpha = 1f - (elapsed - fadeStartTime) / (duration - fadeStartTime);
                alpha = Mathf.Clamp01(alpha);

                if (a.isNoise)
                {
                    // 抖动（前 0.3 秒）
                    float jitter = 0f;
                    if (elapsed < 0.3f)
                        jitter = Mathf.Sin(elapsed * 40f) * 4f;

                    var nc = noiseMainColor; nc.a = alpha;
                    var nb = noiseBorderColor; nb.a = alpha;

                    int w = 200;
                    int h = noiseFontSize + 20;
                    Rect rect = new Rect(
                        screen.x - w / 2f + jitter,
                        guiY - h / 2f - extraY,
                        w, h);

                    var pair = GetStylePair(noiseFontSize, bold, italic);
                    pair.Main.normal.textColor = nc;
                    pair.Border.normal.textColor = nb;

                    DrawWithBorder(rect, noiseSymbol, pair.Main, pair.Border, noiseBorderOffset);
                }
                else
                {
                    var tc = normalTextColor; tc.a = alpha;
                    var bc = normalBorderColor; bc.a = alpha;

                    int w = 120;
                    int h = normalFontSize + 20;
                    Rect rect = new Rect(
                        screen.x - w / 2f,
                        guiY - h / 2f - extraY,
                        w, h);

                    var pair = GetStylePair(normalFontSize, bold, italic);
                    pair.Main.normal.textColor = tc;
                    pair.Border.normal.textColor = bc;

                    DrawWithBorder(rect, alertSymbol, pair.Main, pair.Border, normalBorderOffset);
                }
            }
        }

        // ============ 字体缓存 ============
        // 字体名 = AlertFontName；留空时回退 ComboFontName。
        private static Font GetCachedFont()
        {
            string fontName = Plugin.AlertFontName.Value;
            if (string.IsNullOrEmpty(fontName))
                fontName = Plugin.ComboFontName.Value;

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

        // ============ GUIStyle 缓存 ============
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
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;

            // 字体：先设 font，再设 fontStyle（Unity 2017 font 赋值会重置 fontStyle）
            if (font != null) style.font = font;

            if (bold && italic) style.fontStyle = FontStyle.BoldAndItalic;
            else if (bold) style.fontStyle = FontStyle.Bold;
            else if (italic) style.fontStyle = FontStyle.Italic;
            else style.fontStyle = FontStyle.Normal;

            style.fontSize = fontSize;
            style.normal.textColor = color;
            return style;
        }

        private void DrawWithBorder(Rect rect, string text,
            GUIStyle mainStyle, GUIStyle borderStyle, int offset)
        {
            if (offset <= 0)
            {
                GUI.Label(rect, text, mainStyle);
                return;
            }
            for (int i = 0; i < 8; i++)
            {
                GUI.Label(new Rect(rect.x + BorderDx[i] * offset, rect.y + BorderDy[i] * offset,
                    rect.width, rect.height), text, borderStyle);
            }
            GUI.Label(rect, text, mainStyle);
        }

        // ============ 颜色解析（带缓存） ============
        private static Color ParseColor(string str, Color fallback)
        {
            if (string.IsNullOrEmpty(str)) return fallback;
            if (_colorCache.TryGetValue(str, out Color cached)) return cached;

            Color result;
            try
            {
                var parts = str.Split(',');
                if (parts.Length < 3)
                {
                    result = fallback;
                }
                else
                {
                    int r = int.Parse(parts[0].Trim());
                    int g = int.Parse(parts[1].Trim());
                    int b = int.Parse(parts[2].Trim());
                    result = new Color(r / 255f, g / 255f, b / 255f);
                }
            }
            catch
            {
                result = fallback;
            }
            _colorCache[str] = result;
            return result;
        }
    }
}