using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 生态 UI：
    ///   - 时间文本：原版时间旁（红框 1）
    ///   - 生态条 + 暂停恢复文字 + 技能点刷新 UI：小地图上方（红框 4）
    /// 教程场景（IntroVillage / TitleED3）隐藏。
    /// </summary>
    public class EcologyUI : MonoBehaviour
    {
        public static bool SuppressRender = false;

        // ===== 时间文本位置（红框 1）=====
        private const float TimeX = 160f;
        private const float TimeY = 22f;
        private const float TimeW = 300f;
        private const float TimeH = 30f;
        private const int TimeFontSize = 16;

        // ===== 进度条位置（红框 4）=====
        private const float BarX = 33f;
        private const float BarWidth = 95f;
        private const float BarHeight = 18f;
        private const float BarYFromBottom = 365f;
        private const float RowGap = 3f;
        private const int BorderThick = 1;
        private const int BarFontSize = 13;
        private const float BarTextWidth = 40f;

        // ============ 硬编码常量：SkillPointRefresh.UI 段 + EcologyBonusMax/ChanceCap ============
        private const int SP_BAR_WIDTH = 90;
        private const int SP_BAR_HEIGHT = 18;
        private const int SP_ICON_WIDTH = 21;
        private const int SP_COUNT_WIDTH = 45;
        private const int SP_GAP = 20;
        private const int SP_ACTIVE_ALPHA = 175;
        private static readonly Color SP_FLASH_COLOR = new Color(1f, 1f, 1f);
        private const float SP_FLASH_DURATION_SEC = 0.6f;
        private const float SP_FLASH_MIN_WIDTH = 1f;

        // ===== 颜色 =====
        private static readonly Color BorderColor = new Color(0.88f, 0.88f, 0.88f, 0.35f);
        private static readonly Color TextColor = new Color(0.65f, 0.75f, 0.88f, 0.35f);
        private static readonly Color BarFill = new Color(0.42f, 0.55f, 0.72f, 0.35f);
        private static readonly Color BarEmpty = new Color(0.13f, 0.13f, 0.13f, 0.35f);
        private static readonly Color SkillPointInactive = new Color(0.65f, 0.71f, 0.77f, 0.35f);

        private GUIStyle _timeStyle, _timeBorder;
        private GUIStyle _barValueStyle, _barValueBorder;
        private GUIStyle _delayStyle, _delayBorder;
        private GUIStyle _spIconStyle, _spIconBorder;
        private GUIStyle _spCountStyle, _spCountBorder;

        private static MinimapOnOff _cachedMinimap = null;
        private static float _lastMinimapCheck = -999f;

        private static UiTab _cachedUiTab = null;
        private static float _lastUiTabCheck = -999f;

        private float _lastTickTime = -999f;
        private const float TickIntervalSec = 3.0f;

        // ===== 技能点 UI 状态缓存 =====
        private SkillPointUIState _spState;
        private float _lastSpScan = -999f;
        private const float SpScanIntervalSec = 1.0f;
        private string _lastSpScene = "";

        // ===== 场景名缓存 =====
        // SceneManager.GetActiveScene().name 每帧读取有字符串开销，
        // 场景名只在切场景时变化，用 0.5s 节流足够。
        private string _cachedSceneName = "";
        private float _lastSceneNameCheck = -999f;
        private const float SceneNameCheckIntervalSec = 0.5f;

        // ===== 时间文本缓存 =====
        // 小时数一小时才变一次，字符串只在变化时重拼。
        private int _lastDisplayHours = int.MinValue;
        private int _lastDisplayDay = int.MinValue;
        private string _cachedTimeText = "";

        // ===== 暂停恢复文本缓存 =====
        // PendingDelayHours 是每 3 秒更新一次，按分钟粒度比较。
        private int _lastDelayMinutes = int.MinValue;
        private string _cachedDelayText = "";

        private class SpFlashSegment
        {
            public float StartRatio;
            public float EndRatio;
            public float StartTime;
        }

        private readonly List<SpFlashSegment> _spFlashSegments = new List<SpFlashSegment>();
        private float _spPrevRatio = -1f;

        private struct SkillPointUIState
        {
            public bool HasReady;
            public bool HasLockedOnly;
            public bool InCooldown;
            public float Chance;
            public int Remaining;
            public int DailyLimit;
        }

        /// <summary>节流读取场景名（0.5s 缓存）。</summary>
        private string GetSceneName()
        {
            if (Time.unscaledTime - _lastSceneNameCheck > SceneNameCheckIntervalSec)
            {
                _lastSceneNameCheck = Time.unscaledTime;
                _cachedSceneName = SceneManager.GetActiveScene().name;
            }
            return _cachedSceneName;
        }

        private void Update()
        {
            if (!Plugin.EcologyUIEnabled.Value) return;
            if (SuppressRender || UiSuppress.All) return;

            string scene = GetSceneName();
            if (scene == "IntroVillage" || scene == "TitleED3") return;

            if (Time.unscaledTime - _lastTickTime < TickIntervalSec) return;
            _lastTickTime = Time.unscaledTime;

            var r = EcologyRegistry.GetOrCreate(scene);
            if (r == null) return;
            EcologyRegistry.TickRegion(r, GameTimeTracker.GetTotalGameHours());
        }

        private void OnGUI()
        {
            if (!Plugin.EcologyUIEnabled.Value) return;
            if (SuppressRender || UiSuppress.All) return;

            string scene = GetSceneName();
            if (scene == "IntroVillage" || scene == "TitleED3") return;

            // ★ 过场动画期间原版会隐藏玩家血条，此时 mod UI 也一起隐藏
            if (IsPlayerHudHidden()) return;

            EnsureStyles();

            Color oldColor = GUI.color;

            DrawTimeText();

            if (IsMinimapVisible() && !IsInventoryOpen())
                DrawEcologyBars(scene);

            GUI.color = oldColor;
        }

        // ============================================================
        // 小地图可见性
        // ============================================================

        private bool IsMinimapVisible()
        {
            if (Time.unscaledTime - _lastMinimapCheck > 0.5f)
            {
                _lastMinimapCheck = Time.unscaledTime;
                _cachedMinimap = Object.FindObjectOfType<MinimapOnOff>();
            }
            if (_cachedMinimap == null) return false;
            if (_cachedMinimap.minimapCam == null) return false;
            return _cachedMinimap.minimapCam.activeInHierarchy;
        }

        /// <summary>
        /// 玩家 HUD 是否被隐藏（过场动画 / 死亡界面期间原版会隐藏血条）。
        /// 血条隐藏 = 玩家看不到 HUD，mod UI 也应一起隐藏。
        /// </summary>
        private bool IsPlayerHudHidden()
        {
            if (GlobalVar.mainPlayer == null) return false;
            if (Time.unscaledTime - _lastUiTabCheck > 1.0f)
            {
                _lastUiTabCheck = Time.unscaledTime;
                _cachedUiTab = Object.FindObjectOfType<UiTab>();
            }
            if (_cachedUiTab == null) return false;
            if (_cachedUiTab.healthBar == null) return false;
            return !_cachedUiTab.healthBar.activeInHierarchy;
        }

        private bool IsInventoryOpen()
        {
            var inv = UiTab.invUi;
            if (inv == null) return false;
            return inv.gameObject.activeInHierarchy;
        }

        // ============================================================
        // 样式
        // ============================================================

        private void EnsureStyles()
        {
            if (_timeStyle != null) return;

            _timeStyle = MakeStyle(TimeFontSize, TextColor, TextAnchor.MiddleLeft);
            _timeBorder = MakeStyle(TimeFontSize, BorderColor, TextAnchor.MiddleLeft);
            _barValueStyle = MakeStyle(BarFontSize, TextColor, TextAnchor.MiddleRight);
            _barValueBorder = MakeStyle(BarFontSize, BorderColor, TextAnchor.MiddleRight);
            _delayStyle = MakeStyle(BarFontSize, TextColor, TextAnchor.MiddleLeft);
            _delayBorder = MakeStyle(BarFontSize, BorderColor, TextAnchor.MiddleLeft);

            // 技能点图标：主体白色，金边由 4 方向偏移画出
            Color spMain = ParseColorSafe(Plugin.MinimapSkillPointColor, Color.white);
            Color spGold = ParseColorSafe(Plugin.MinimapSkillPointGoldColor,
                new Color(1f, 0.82f, 0.24f));

            int iconFont = SP_ICON_WIDTH;
            if (iconFont < 10) iconFont = 10;

            _spIconStyle = MakeStyle(iconFont, spMain, TextAnchor.MiddleCenter);
            _spIconBorder = MakeStyle(iconFont, spGold, TextAnchor.MiddleCenter);

            _spCountStyle = MakeStyle(BarFontSize, TextColor, TextAnchor.MiddleLeft);
            _spCountBorder = MakeStyle(BarFontSize, BorderColor, TextAnchor.MiddleLeft);
        }

        private Color ParseColorSafe(ConfigEntry<string> entry, Color fallback)
        {
            try
            {
                if (entry == null) return fallback;
                return MinimapIconManager.ParseColor(entry.Value, fallback);
            }
            catch { return fallback; }
        }

        private GUIStyle MakeStyle(int size, Color color, TextAnchor align)
        {
            var s = new GUIStyle(GUI.skin.label);
            s.alignment = align;
            s.fontStyle = FontStyle.Normal;
            s.fontSize = size;
            s.normal.textColor = color;

            string fontName = Plugin.ComboFontName.Value;
            if (!string.IsNullOrEmpty(fontName))
            {
                try
                {
                    Font f = FontLoader.GetFont(fontName);
                    if (f != null) s.font = f;
                }
                catch { }
            }
            return s;
        }

        // ============================================================
        // 时间文本（红框 1）
        // ============================================================

        private void DrawTimeText()
        {
            float totalHours = GameTimeTracker.GetTotalGameHours();
            int hours = Mathf.FloorToInt(totalHours);
            int day = Mathf.FloorToInt(totalHours / 24f) + 1;

            // 只在小时数 / 天数变化时重拼字符串
            if (hours != _lastDisplayHours || day != _lastDisplayDay)
            {
                _lastDisplayHours = hours;
                _lastDisplayDay = day;
                _cachedTimeText = "E " + hours + "h (Day " + day + ")";
            }

            Rect rect = new Rect(TimeX, TimeY, TimeW, TimeH);
            DrawTextWithBorder4(rect, _cachedTimeText, _timeStyle, _timeBorder);
        }

        // ============================================================
        // 生态条 + 暂停文字（红框 4）
        // ============================================================

        private void DrawEcologyBars(string scene)
        {
            var r = EcologyRegistry.GetOrCreate(scene);
            if (r == null) return;

            float baseY = Screen.height - BarYFromBottom;

            // 延迟文本按分钟粒度缓存（PendingDelayHours 每 3 秒才变）
            int delayMinutes = r.PendingDelayHours > 0f
                ? Mathf.CeilToInt(r.PendingDelayHours * 60f)
                : 0;
            if (delayMinutes != _lastDelayMinutes)
            {
                _lastDelayMinutes = delayMinutes;
                _cachedDelayText = FormatDelay(r.PendingDelayHours);
            }

            if (!string.IsNullOrEmpty(_cachedDelayText))
            {
                float textH = BarFontSize + 4f;
                float textY = baseY - RowGap - textH;
                Rect textRect = new Rect(BarX, textY, BarWidth, textH);
                DrawTextWithBorder4(textRect, _cachedDelayText, _delayStyle, _delayBorder);
            }

            float ecoRatio = Mathf.Clamp01(r.PopulationPercent / 100f);
            string ecoValue = Mathf.RoundToInt(r.PopulationPercent) + "%";
            DrawBar(new Rect(BarX, baseY, BarWidth, BarHeight), ecoRatio, ecoValue);

            // ===== 技能点 UI（紧接在生态百分比右侧）=====
            if (Plugin.SkillPointRefreshEnabled.Value)
                DrawSkillPointUI(scene, baseY);
        }

        private string FormatDelay(float delayHours)
        {
            if (delayHours <= 0f) return "";

            int totalMinutes = Mathf.CeilToInt(delayHours * 60f);
            if (totalMinutes <= 0) return "";

            int days = totalMinutes / 1440;
            int rem = totalMinutes % 1440;
            int hours = rem / 60;
            int minutes = rem % 60;

            if (days > 0)
            {
                string s = days + "d";
                if (hours > 0) s += " " + hours + "h";
                if (minutes > 0) s += " " + minutes + "m";
                return s;
            }
            if (hours > 0)
            {
                string s = hours + "h";
                if (minutes > 0) s += " " + minutes + "m";
                return s;
            }
            return minutes + "m";
        }

        private void DrawBar(Rect rect, float ratio, string valueText)
        {
            if (ratio < 0f) ratio = 0f;
            if (ratio > 1f) ratio = 1f;

            GUI.color = BorderColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            Rect inner = new Rect(
                rect.x + BorderThick, rect.y + BorderThick,
                rect.width - BorderThick * 2, rect.height - BorderThick * 2);
            GUI.color = BarEmpty;
            GUI.DrawTexture(inner, Texture2D.whiteTexture);

            float fillW = inner.width * ratio;
            if (fillW > 0.5f)
            {
                GUI.color = BarFill;
                GUI.DrawTexture(new Rect(inner.x, inner.y, fillW, inner.height),
                    Texture2D.whiteTexture);
            }

            GUI.color = Color.white;

            Rect textRect = new Rect(rect.xMax + 1f, rect.y, BarTextWidth, rect.height);
            DrawTextWithBorder4(textRect, valueText, _barValueStyle, _barValueBorder);
        }

        // ============================================================
        // 技能点 UI
        // ============================================================

        private void DrawSkillPointUI(string scene, float baseY)
        {
            var s = GetSkillPointUIState(scene);

            float iconFont = SP_ICON_WIDTH;
            float iconSlot = iconFont * 1.2f;
            float barW = SP_BAR_WIDTH;
            float barH = SP_BAR_HEIGHT;
            float countW = SP_COUNT_WIDTH;
            float gap = SP_GAP;

            float iconX = BarX + BarWidth + 1f + BarTextWidth + gap;

            Color gold = ParseColorSafe(Plugin.MinimapSkillPointGoldColor,
                new Color(1f, 0.82f, 0.24f));
            Color white = ParseColorSafe(Plugin.MinimapSkillPointColor, Color.white);

            float activeAlpha = SP_ACTIVE_ALPHA / 255f;
            if (activeAlpha < 0f) activeAlpha = 0f;
            if (activeAlpha > 1f) activeAlpha = 1f;

            // ===== 图标 =====
            Rect iconRect = new Rect(iconX, baseY, iconSlot, barH);

            if (s.HasReady)
            {
                var oldMain = _spIconStyle.normal.textColor;
                var oldBorder = _spIconBorder.normal.textColor;

                Color mainC = oldMain;
                mainC.a = activeAlpha;
                Color borderC = oldBorder;
                borderC.a = activeAlpha;

                _spIconStyle.normal.textColor = mainC;
                _spIconBorder.normal.textColor = borderC;
                DrawTextWithBorder4Thick(iconRect, "◆", _spIconStyle, _spIconBorder, 2);
                _spIconStyle.normal.textColor = oldMain;
                _spIconBorder.normal.textColor = oldBorder;
            }
            else if (s.HasLockedOnly)
            {
                var oldMain = _spIconStyle.normal.textColor;
                var oldBorder = _spIconBorder.normal.textColor;

                Color mainC = oldMain;
                mainC.a = activeAlpha;
                Color lockedBorder = ParseColorSafe(Plugin.MinimapSkillPointLockedColor,
                    new Color(1f, 0.24f, 0.24f));
                lockedBorder.a = activeAlpha;

                _spIconStyle.normal.textColor = mainC;
                _spIconBorder.normal.textColor = lockedBorder;
                DrawTextWithBorder4Thick(iconRect, "◆", _spIconStyle, _spIconBorder, 2);
                _spIconStyle.normal.textColor = oldMain;
                _spIconBorder.normal.textColor = oldBorder;
            }
            else
            {
                var oldMain = _spIconStyle.normal.textColor;
                var oldBorder = _spIconBorder.normal.textColor;
                _spIconStyle.normal.textColor = SkillPointInactive;
                _spIconBorder.normal.textColor = SkillPointInactive;
                DrawTextWithBorder4Thick(iconRect, "◆", _spIconStyle, _spIconBorder, 1);
                _spIconStyle.normal.textColor = oldMain;
                _spIconBorder.normal.textColor = oldBorder;
            }

            // ===== 进度条 =====
            float barX = iconX + iconSlot + 1f;
            Rect barRect = new Rect(barX, baseY, barW, barH);

            float ratio;
            Color fillColor;
            Color borderColor;

            if (s.HasReady)
            {
                ratio = 1f;
                fillColor = white;
                fillColor.a = activeAlpha;
                borderColor = gold;
                borderColor.a = activeAlpha;
                _spFlashSegments.Clear();
                _spPrevRatio = ratio;
            }
            else if (s.HasLockedOnly)
            {
                ratio = 1f;
                fillColor = white;
                fillColor.a = activeAlpha;
                Color lockedBorder = ParseColorSafe(Plugin.MinimapSkillPointLockedColor,
                    new Color(1f, 0.24f, 0.24f));
                lockedBorder.a = activeAlpha;
                borderColor = lockedBorder;
                _spFlashSegments.Clear();
                _spPrevRatio = ratio;
            }
            else
            {
                ratio = s.Chance;
                fillColor = SkillPointInactive;
                borderColor = BorderColor;

                float now = Time.unscaledTime;
                float dur = SP_FLASH_DURATION_SEC;

                if (dur <= 0f)
                {
                    _spFlashSegments.Clear();
                }
                else
                {
                    for (int i = _spFlashSegments.Count - 1; i >= 0; i--)
                    {
                        if (now - _spFlashSegments[i].StartTime > dur)
                            _spFlashSegments.RemoveAt(i);
                    }

                    if (_spPrevRatio >= 0f && ratio > _spPrevRatio + 0.001f)
                    {
                        var seg = new SpFlashSegment();
                        seg.StartRatio = _spPrevRatio;
                        seg.EndRatio = ratio;
                        seg.StartTime = now;
                        _spFlashSegments.Add(seg);
                    }
                }

                _spPrevRatio = ratio;
            }

            DrawBarColoredWithFlash(barRect, ratio, fillColor, borderColor);

            // ===== [N/M] =====
            string countText;
            if (s.DailyLimit <= 0) countText = "[∞]";
            else countText = "[" + s.Remaining + "/" + s.DailyLimit + "]";

            Rect countRect = new Rect(barX + barW + 1f, baseY, countW, barH);
            DrawTextWithBorder4(countRect, countText, _spCountStyle, _spCountBorder);
        }

        private void DrawBarColored(Rect rect, float ratio, Color fillColor, Color borderColor)
        {
            if (ratio < 0f) ratio = 0f;
            if (ratio > 1f) ratio = 1f;

            GUI.color = borderColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            Rect inner = new Rect(
                rect.x + BorderThick, rect.y + BorderThick,
                rect.width - BorderThick * 2, rect.height - BorderThick * 2);
            GUI.color = BarEmpty;
            GUI.DrawTexture(inner, Texture2D.whiteTexture);

            float fillW = inner.width * ratio;
            if (fillW > 0.5f)
            {
                GUI.color = fillColor;
                GUI.DrawTexture(new Rect(inner.x, inner.y, fillW, inner.height),
                    Texture2D.whiteTexture);
            }

            GUI.color = Color.white;
        }

        private void DrawBarColoredWithFlash(Rect rect, float ratio, Color fillColor, Color borderColor)
        {
            if (ratio < 0f) ratio = 0f;
            if (ratio > 1f) ratio = 1f;

            GUI.color = borderColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            Rect inner = new Rect(
                rect.x + BorderThick, rect.y + BorderThick,
                rect.width - BorderThick * 2, rect.height - BorderThick * 2);

            GUI.color = BarEmpty;
            GUI.DrawTexture(inner, Texture2D.whiteTexture);

            float fillW = inner.width * ratio;
            if (fillW > 0.5f)
            {
                GUI.color = fillColor;
                GUI.DrawTexture(new Rect(inner.x, inner.y, fillW, inner.height),
                    Texture2D.whiteTexture);
            }

            if (_spFlashSegments.Count > 0)
            {
                float dur = SP_FLASH_DURATION_SEC;
                if (dur > 0f)
                {
                    float now = Time.unscaledTime;

                    Color flashBase = SP_FLASH_COLOR;
                    float flashAlpha = SP_ACTIVE_ALPHA / 255f;
                    if (flashAlpha < 0f) flashAlpha = 0f;
                    if (flashAlpha > 1f) flashAlpha = 1f;

                    float minSegPx = SP_FLASH_MIN_WIDTH;
                    float minSegRatio = (inner.width > 0.5f) ? (minSegPx / inner.width) : 0f;

                    for (int i = 0; i < _spFlashSegments.Count; i++)
                    {
                        var seg = _spFlashSegments[i];

                        float t = (now - seg.StartTime) / dur;
                        if (t < 0f) t = 0f;
                        if (t > 1f) t = 1f;

                        float startR = seg.StartRatio;
                        float endR = seg.EndRatio;
                        if (endR - startR < minSegRatio)
                            startR = endR - minSegRatio;
                        if (startR < 0f) startR = 0f;

                        float sx = inner.x + inner.width * startR;
                        float ex = inner.x + inner.width * endR;
                        float w = ex - sx;
                        if (w <= 0.5f) continue;

                        Color c = Color.Lerp(
                            new Color(flashBase.r, flashBase.g, flashBase.b, flashAlpha),
                            new Color(fillColor.r, fillColor.g, fillColor.b, flashAlpha),
                            t);
                        GUI.color = c;
                        GUI.DrawTexture(new Rect(sx, inner.y, w, inner.height),
                            Texture2D.whiteTexture);
                    }
                }
            }

            GUI.color = Color.white;
        }

        // ============================================================
        // 技能点 UI 状态
        // ============================================================

        private SkillPointUIState GetSkillPointUIState(string scene)
        {
            if (scene != _lastSpScene)
            {
                _spFlashSegments.Clear();
                _spPrevRatio = -1f;
            }

            if (scene != _lastSpScene || Time.unscaledTime - _lastSpScan > SpScanIntervalSec)
            {
                _lastSpScan = Time.unscaledTime;
                _lastSpScene = scene;
                _spState = ScanSkillPointState(scene);
            }
            return _spState;
        }

        private SkillPointUIState ScanSkillPointState(string scene)
        {
            var s = new SkillPointUIState();

            var region = EcologyRegistry.GetOrCreate(scene);
            float pop = (region != null) ? region.PopulationPercent : 100f;
            if (pop < 0f) pop = 0f;
            if (pop > 100f) pop = 100f;

            float chance = Plugin.SkillPointRefreshBaseChance.Value
                + (1f - pop / 100f) * Plugin.SP_ECOLOGY_BONUS_MAX;
            int cap = Plugin.SP_CHANCE_CAP;
            if (chance > cap) chance = cap;
            if (chance < 0f) chance = 0f;
            s.Chance = chance / 100f;

            s.DailyLimit = Plugin.SkillPointRefreshDailyLimit.Value;
            float now = GameTimeTracker.GetTotalGameHours();
            int currentDay = Mathf.FloorToInt(now / 24f);

            SkillPointRegistry.CanRefreshToday(currentDay, s.DailyLimit);

            int used = SkillPointRegistry.GetTodayRefreshCount();
            if (s.DailyLimit <= 0)
            {
                s.Remaining = -1;
            }
            else
            {
                s.Remaining = s.DailyLimit - used;
                if (s.Remaining < 0) s.Remaining = 0;
            }

            var all = Resources.FindObjectsOfTypeAll<AddSkillPointEv>();
            bool hasReady = false;
            bool hasLocked = false;
            if (all != null)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    var sp = all[i];
                    if (sp == null) continue;
                    var go = sp.gameObject;
                    if (go == null) continue;
                    if (!go.scene.IsValid() || !go.scene.isLoaded) continue;

                    int id = sp.boolId;
                    if (id <= 0 || id > 42) continue;

                    if (go.activeInHierarchy) hasReady = true;
                    else hasLocked = true;
                }
            }

            s.HasReady = hasReady;
            s.HasLockedOnly = hasLocked && !hasReady;
            s.InCooldown = !hasReady && !hasLocked;
            return s;
        }

        // ============================================================
        // 工具
        // ============================================================

        private void DrawTextWithBorder4(Rect rect, string text,
            GUIStyle main, GUIStyle border)
        {
            GUI.Label(new Rect(rect.x - 1, rect.y, rect.width, rect.height), text, border);
            GUI.Label(new Rect(rect.x + 1, rect.y, rect.width, rect.height), text, border);
            GUI.Label(new Rect(rect.x, rect.y - 1, rect.width, rect.height), text, border);
            GUI.Label(new Rect(rect.x, rect.y + 1, rect.width, rect.height), text, border);
            GUI.Label(rect, text, main);
        }

        private void DrawTextWithBorder4Thick(Rect rect, string text,
            GUIStyle main, GUIStyle border, int thick)
        {
            for (int dx = -thick; dx <= thick; dx++)
            {
                for (int dy = -thick; dy <= thick; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    GUI.Label(new Rect(rect.x + dx, rect.y + dy, rect.width, rect.height),
                        text, border);
                }
            }
            GUI.Label(rect, text, main);
        }
    }
}