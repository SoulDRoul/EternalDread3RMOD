using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 永久死亡模式死亡界面：
    ///   全屏黑 + YOU DIED 渐隐（0-1.5s 淡入，1.5s-倒计时结束 线性淡出）
    ///   倒计时结束 → 删档 + 返回主菜单
    ///   期间保持全黑直到新场景加载完成
    /// </summary>
    public class PermadeathDeathUI : MonoBehaviour
    {
        public static PermadeathDeathUI Instance;

        private static bool _active = false;
        private static bool _returning = false;
        private static float _startTime = -1f;
        private static string _targetScene = "";

        private const string PERMADEATH_TITLE_SCENE_NAME = "TitleED3";

        private const float FadeInSec = 1.5f;

        // ============ 颜色解析缓存 ============
        private static readonly Dictionary<string, Color> _colorCache = new Dictionary<string, Color>();

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

        // 边框偏移表（对应 offset = 3）
        private static readonly int[] BorderDx = { -3, 0, 3, -3, 3, -3, 0, 3 };
        private static readonly int[] BorderDy = { -3, -3, -3, 0, 0, 3, 3, 3 };

        private void Awake()
        {
            Instance = this;
            if (Plugin.Log != null)
                Plugin.Log.LogInfo("[ED3R] PermadeathDeathUI Awake");
        }

        public static void Show()
        {
            if (Instance == null)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogWarning("[ED3R] PermadeathDeathUI.Show: Instance is null!");
                return;
            }
            _active = true;
            _returning = false;
            _startTime = Time.unscaledTime;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            UiSuppress.All = true;
            EcologyUI.SuppressRender = true;

            if (Plugin.Log != null)
                Plugin.Log.LogInfo("[ED3R] PermadeathDeathUI.Show activated");
        }

        private void Update()
        {
            if (!_active) return;

            if (Cursor.lockState != CursorLockMode.None)
                Cursor.lockState = CursorLockMode.None;
            if (!Cursor.visible)
                Cursor.visible = true;

            if (_returning)
            {
                string cur = SceneManager.GetActiveScene().name;
                if (!string.IsNullOrEmpty(cur) && cur == _targetScene)
                {
                    EndSequence();
                }
                else if (Time.unscaledTime - _startTime > 30f)
                {
                    EndSequence();
                }
                return;
            }

            float cd = GetCountdownSec();
            float elapsed = Time.unscaledTime - _startTime;
            if (elapsed >= cd)
            {
                ReturnToTitle();
            }
        }

        private void EndSequence()
        {
            _active = false;
            _returning = false;
            _targetScene = "";

            if (Plugin.Log != null)
                Plugin.Log.LogInfo("[ED3R] PermadeathDeathUI: sequence ended");
        }

        private static float GetCountdownSec()
        {
            if (Plugin.PermadeathCountdownSec != null)
                return Plugin.PermadeathCountdownSec.Value;
            return 9f;
        }

        private void OnGUI()
        {
            if (!_active) return;
            try { DrawUI(); }
            catch (System.Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] PermadeathDeathUI OnGUI error: " + e.ToString());
                EndSequence();
            }
        }

        private void DrawUI()
        {
            // 全屏黑（不透明，无渐入）
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float elapsed = Time.unscaledTime - _startTime;
            float cd = GetCountdownSec();

            // 淡化曲线：0-1.5s 淡入，1.5s-cd 线性淡出
            float alpha;
            if (elapsed < FadeInSec)
                alpha = FadeInSec > 0f ? elapsed / FadeInSec : 1f;
            else if (cd > FadeInSec)
                alpha = 1f - (elapsed - FadeInSec) / (cd - FadeInSec);
            else
                alpha = 0f;
            alpha = Mathf.Clamp01(alpha);

            string deathText = "YOU DIED";
            if (Plugin.PermadeathDeathText != null
                && !string.IsNullOrEmpty(Plugin.PermadeathDeathText.Value))
                deathText = Plugin.PermadeathDeathText.Value;

            int fontSize = 120;
            if (Plugin.PermadeathDeathTextFontSize != null)
                fontSize = Plugin.PermadeathDeathTextFontSize.Value;

            string textColorStr = "200,0,0";
            if (Plugin.PermadeathDeathTextColor != null)
                textColorStr = Plugin.PermadeathDeathTextColor.Value;

            string borderColorStr = "80,0,0";
            if (Plugin.PermadeathDeathTextBorderColor != null)
                borderColorStr = Plugin.PermadeathDeathTextBorderColor.Value;

            Color textColor = ParseColor(textColorStr, new Color(0.78f, 0f, 0f, 1f));
            textColor.a *= alpha;
            Color borderColor = ParseColor(borderColorStr, new Color(0.2f, 0f, 0f, 1f));
            borderColor.a *= alpha;

            var pair = GetStylePair(fontSize);
            pair.Main.normal.textColor = textColor;
            pair.Border.normal.textColor = borderColor;

            float textCy = Screen.height * 0.55f;
            DrawCentered(Screen.width / 2f, textCy, deathText, pair.Main, pair.Border, 3);
        }

        private void ReturnToTitle()
        {
            if (_returning) return;
            _returning = true;

            int slot = GlobalVar.eventsId[95];
            DeleteSaveFiles(slot);

            if (Plugin.Log != null)
                Plugin.Log.LogInfo("[ED3R] Permadeath: deleted save slot=" + slot);

            string scene = GetTitleSceneName();
            _targetScene = scene;

            if (Plugin.Log != null)
                Plugin.Log.LogInfo("[ED3R] Loading title scene: '" + scene + "'");

            GlobalVar.ResetAllVariable();
            UiMaster.pauseGame = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Camera.main != null)
                Object.Destroy(Camera.main.gameObject);

#pragma warning disable 0618
            try
            {
                if (Application.CanStreamedLevelBeLoaded(scene))
                {
                    Application.LoadLevel(scene);
                }
                else
                {
                    if (Plugin.Log != null)
                        Plugin.Log.LogWarning("[ED3R] Scene not loadable: '" + scene + "'");
                    if (Application.CanStreamedLevelBeLoaded("TitleED3"))
                        Application.LoadLevel("TitleED3");
                    else if (Application.CanStreamedLevelBeLoaded("TitleRiel"))
                        Application.LoadLevel("TitleRiel");
                }
            }
            catch (System.Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] LoadLevel failed: " + e.ToString());
            }
#pragma warning restore 0618
        }

        private static void DeleteSaveFiles(int slot)
        {
            try
            {
                string xmlPath = Application.dataPath + "/UqkasKjbHas" + slot + ".xml";
                string sidecarPath = Application.dataPath + "/UqkasKjbHas" + slot + ".permadeath";

                bool xmlExisted = File.Exists(xmlPath);
                bool sidecarExisted = File.Exists(sidecarPath);

                if (xmlExisted) File.Delete(xmlPath);
                if (sidecarExisted) File.Delete(sidecarPath);

                if (Plugin.Log != null)
                    Plugin.Log.LogInfo("[ED3R] DeleteSaveFiles: slot=" + slot
                        + " xmlExisted=" + xmlExisted
                        + " sidecarExisted=" + sidecarExisted);
            }
            catch (System.Exception e)
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogError("[ED3R] DeleteSaveFiles failed: " + e.ToString());
            }
        }

        private static string GetTitleSceneName()
        {
            if (!string.IsNullOrEmpty(PERMADEATH_TITLE_SCENE_NAME))
                return PERMADEATH_TITLE_SCENE_NAME;

            try
            {
                var pm = Object.FindObjectOfType<PauseMenuCanvas>();
                if (pm != null && !string.IsNullOrEmpty(pm.titleScene))
                    return pm.titleScene;
            }
            catch { }
            try
            {
                var ut = Object.FindObjectOfType<UiTab>();
                if (ut != null && !string.IsNullOrEmpty(ut.titleScene))
                    return ut.titleScene;
            }
            catch { }

            return "TitleED3";
        }

        // ============ 字体缓存 ============
        private static Font GetCachedFont()
        {
            string fontName = null;
            if (Plugin.ComboFontName != null)
                fontName = Plugin.ComboFontName.Value;

            if (fontName == _cachedFontName) return _cachedFont;

            _cachedFontName = fontName;
            if (string.IsNullOrEmpty(fontName))
            {
                _cachedFont = null;
                return null;
            }
            try { _cachedFont = FontLoader.GetFont(fontName); }
            catch { _cachedFont = null; }
            return _cachedFont;
        }

        // ============ GUIStyle 缓存 ============
        // PermadeathDeathUI 不加粗/斜体，style 只按 fontSize 区分。
        private static StylePair GetStylePair(int fontSize)
        {
            Font font = GetCachedFont();

            if (_stylePairs.TryGetValue(fontSize, out var pair) && pair.Font == font)
                return pair;

            pair = new StylePair
            {
                Main = CreateStyle(fontSize, Color.white, font),
                Border = CreateStyle(fontSize, Color.black, font),
                Font = font
            };
            _stylePairs[fontSize] = pair;
            return pair;
        }

        private static GUIStyle CreateStyle(int fontSize, Color color, Font font)
        {
            var s = new GUIStyle(GUI.skin.label);
            s.alignment = TextAnchor.MiddleCenter;
            if (font != null) s.font = font;
            s.fontSize = fontSize;
            s.normal.textColor = color;
            return s;
        }

        private void DrawCentered(float cx, float cy, string text,
            GUIStyle main, GUIStyle border, int offset)
        {
            const int w = 1500;
            const int h = 300;
            Rect r = new Rect(cx - w / 2f, cy - h / 2f, w, h);

            if (offset > 0)
            {
                for (int i = 0; i < 8; i++)
                    GUI.Label(new Rect(r.x + BorderDx[i], r.y + BorderDy[i], r.width, r.height),
                        text, border);
            }
            GUI.Label(r, text, main);
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