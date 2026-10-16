using System.Collections.Generic;
using UnityEngine;
using System;

namespace EternalDread3RMOD
{
    public class ComboUI : MonoBehaviour
    {
        // ============ 硬编码常量：Combo.UI.FadeDurationSec ============
        // 原 Combo.UI.FadeDurationSec —— 连杀 UI 超时前的淡出时长（秒）
        private const float FADE_DURATION_SEC = 0.6f;

        public static ComboUI Instance;

        private static float rewardShowTime = -1f;
        private static string rewardText = "";
        private static System.Reflection.FieldInfo _uiMasterHpField = null;
        private static bool _uiMasterFieldSearched = false;

        // ============ 玩家组件缓存 ============
        // 避免每帧 GetComponent<UiMaster>() / GetComponent<Status>()。
        // 玩家对象变化（换存档 / 重开局）时自动失效。
        private static object _cachedPlayerRef;
        private static UiMaster _cachedUiMaster;
        private static Status _cachedStatus;

        // ============ 字体缓存 ============
        // 避免每帧 FontLoader.GetFont(Plugin.ComboFontName.Value)。
        private static Font _cachedFont;
        private static string _cachedFontName;

        // ============ GUIStyle 缓存 ============
        // 每个 (fontSize, bold, italic) 组合缓存一对 GUIStyle：Main + Border。
        // 绘制时只改 normal.textColor，不重建样式。
        private class StylePair
        {
            public GUIStyle Main;
            public GUIStyle Border;
            public Font Font;
        }
        private static readonly Dictionary<int, StylePair> _stylePairs = new Dictionary<int, StylePair>();

        // ============ 颜色字符串解析缓存 ============
        // 避免每帧 Split + int.Parse。
        private static readonly Dictionary<string, Color> _colorCache = new Dictionary<string, Color>();

        // 边框偏移表（Spread 模式）
        private static readonly int[] BorderDx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] BorderDy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        // ============ 连杀字母/时间条颜色脉动：硬编码参数 ============
        // 目标色映射全部来自 Plugin 配置（见 GetPulseTarget），此处只放"强度和节奏"。
        // 修改这些常量后重新编译即可调整视觉效果。
        //
        // 【作用范围】
        //   1) 字母主色：朝"下一档"的目标色脉动
        //   2) 时间条颜色：以 ComboTimerBarColor（默认绿）为起点，
        //      朝"当前档位字母主色"脉动，形成 绿→黄→红 的健康度色阶
        //   两者共用下列所有节奏参数，自动同相。

        // 各档基础振幅（D ~ SSS）。数值 = 朝目标色插值的最大比例。
        // 说明：
        //   · 数组按档位顺序：[0]=D [1]=C [2]=B [3]=A [4]=S [5]=SS [6]=SSS
        private static readonly float[] PULSE_AMP_BY_RANK = {
            0.11f,  // D   近白 → C 蓝，极轻
            0.22f,  // C   蓝 → B 黄，轻（幅度限制，视觉约到绿）
            0.33f,  // B   黄 → A 橙，中等
            0.44f,  // A   橙 → SSS 红
            0.55f,  // S   红 → 金，明显
            0.66f,  // SS  深红 → 金，更强
            0.77f,  // SSS 暗红 → 金，最抢眼
        };

        // 一次完整呼吸的周期（秒）。
        private const float PULSE_PERIOD_SEC = 1.3f;

        // 剩余连杀时间为 0 时的最大紧迫放大倍率。
        private const float PULSE_URGENCY_MAX = 4.2f;

        // 最终插值量的硬上限（0~1）。防止最高档 + 最高紧迫时颜色越界。
        private const float PULSE_AMP_HARD_CAP = 0.8f;

        private void Awake()
        {
            Instance = this;
            Plugin.DebugLog("[ComboUI] Awake called");
        }

        public static void ShowRewardText(int amount)
        {
            rewardText = "+ " + amount + " S T A M I N A";
            rewardShowTime = ComboState.gameTime;
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

        private static void GetRank(int count,
            out string rank, out Color mainColor, out Color borderColor)
        {
            if (count >= Plugin.ComboThreshold_SSS.Value)
            {
                rank = "SSS";
                mainColor = ParseColor(Plugin.ComboColor_SSS.Value, Color.red);
                borderColor = ParseColor(Plugin.ComboBorder_SSS.Value, Color.yellow);
            }
            else if (count >= Plugin.ComboThreshold_SS.Value)
            {
                rank = "SS";
                mainColor = ParseColor(Plugin.ComboColor_SS.Value, Color.red);
                borderColor = ParseColor(Plugin.ComboBorder_SS.Value, Color.white);
            }
            else if (count >= Plugin.ComboThreshold_S.Value)
            {
                rank = "S";
                mainColor = ParseColor(Plugin.ComboColor_S.Value, Color.red);
                borderColor = ParseColor(Plugin.ComboBorder_S.Value, Color.gray);
            }
            else if (count >= Plugin.ComboThreshold_A.Value)
            {
                rank = "A";
                mainColor = ParseColor(Plugin.ComboColor_A.Value, Color.yellow);
                borderColor = ParseColor(Plugin.ComboBorder_A.Value, Color.red);
            }
            else if (count >= Plugin.ComboThreshold_B.Value)
            {
                rank = "B";
                mainColor = ParseColor(Plugin.ComboColor_B.Value, Color.yellow);
                borderColor = ParseColor(Plugin.ComboBorder_B.Value, Color.red);
            }
            else if (count >= Plugin.ComboThreshold_C.Value)
            {
                rank = "C";
                mainColor = ParseColor(Plugin.ComboColor_C.Value, Color.blue);
                borderColor = ParseColor(Plugin.ComboBorder_C.Value, Color.red);
            }
            else
            {
                rank = "D";
                mainColor = ParseColor(Plugin.ComboColor_D.Value, Color.white);
                borderColor = ParseColor(Plugin.ComboBorder_D.Value, Color.red);
            }
        }

        // ============ 连杀字母 / 时间条颜色脉动 ============

        // 档位字符串 → 索引（对应 PULSE_AMP_BY_RANK 数组）
        private static int RankToIndex(string rank)
        {
            switch (rank)
            {
                case "SSS": return 6;
                case "SS": return 5;
                case "S": return 4;
                case "A": return 3;
                case "B": return 2;
                case "C": return 1;
                default: return 0;  // D
            }
        }

        // 档位 → 波动目标色（字母用）。全部来自 Plugin 配置，不硬编码 RGB。
        // 映射规则：
        //   D       → C 色（近白向蓝推）
        //   C       → B 色（蓝向黄推，振幅限制在视觉到绿）
        //   B       → A 色（黄向橙推）
        //   A       → SSS 色（橙向深红推）
        //   S/SS/SSS→ 金色（复用 Border_SSS 配置）
        private static Color GetPulseTarget(string rank)
        {
            switch (rank)
            {
                case "SSS":
                case "SS":
                case "S":
                    return ParseColor(Plugin.ComboBorder_SSS.Value,
                        new Color(1f, 0.686f, 0f));          // 金 fallback
                case "A":
                    return ParseColor(Plugin.ComboColor_SSS.Value, Color.red);
                case "B":
                    return ParseColor(Plugin.ComboColor_A.Value, Color.yellow);
                case "C":
                    return ParseColor(Plugin.ComboColor_B.Value, Color.yellow);
                case "D":
                    return ParseColor(Plugin.ComboColor_C.Value, Color.blue);
                default:
                    return Color.white;
            }
        }

        // 核心脉冲函数：把 current 颜色朝 target 颜色轻轻推。
        //   baseAmplitude  = 该档基础振幅（查表）
        //   remainSec      = 连杀剩余秒数
        //   windowSec      = 连杀总窗口秒数
        // 相位源 = Time.time（真实时间，不受游戏内加速/睡觉影响）
        // alpha  = 保留 current 的 alpha，不被脉冲覆盖
        private static Color PulseColor(Color current, Color target,
            float baseAmplitude, float remainSec, float windowSec)
        {
            // 相位（正弦 0~1）
            float phase = Time.time / PULSE_PERIOD_SEC;
            float sine01 = 0.5f + 0.5f * Mathf.Sin(phase * Mathf.PI * 2f);

            // 紧迫倍率：剩余时间越少越强。remainRatio² 让紧迫感集中在最后 20~30%
            float remainRatio = Mathf.Clamp01(remainSec / Mathf.Max(0.01f, windowSec));
            float urgency = Mathf.Lerp(PULSE_URGENCY_MAX, 1f, remainRatio * remainRatio);

            // 最终插值量（clamp 硬上限）
            float t = Mathf.Clamp(baseAmplitude * urgency * sine01,
                0f, PULSE_AMP_HARD_CAP);

            Color result = Color.Lerp(current, target, t);
            result.a = current.a;
            return result;
        }

        // ============ 字体缓存 ============
        // 只在 ComboFontName.Value 变化时才调用 FontLoader.GetFont。
        private static Font GetCachedFont()
        {
            string name = Plugin.ComboFontName.Value;
            if (name == _cachedFontName) return _cachedFont;

            _cachedFontName = name;
            try
            {
                _cachedFont = FontLoader.GetFont(name);
            }
            catch (Exception e)
            {
                Plugin.Log?.LogError("[ED3R] FontLoader 异常: " + e.Message);
                _cachedFont = null;
            }
            return _cachedFont;
        }

        // ============ GUIStyle 缓存 ============
        // 按 (fontSize, bold, italic) 缓存一对 GUIStyle。绘制时只改颜色。
        private static int StyleKey(int fontSize, bool bold, bool italic)
        {
            return fontSize * 4 + (bold ? 2 : 0) + (italic ? 1 : 0);
        }

        private static StylePair GetStylePair(int fontSize, bool bold, bool italic)
        {
            Font currentFont = GetCachedFont();
            int key = StyleKey(fontSize, bold, italic);

            if (_stylePairs.TryGetValue(key, out var pair) && pair.Font == currentFont)
                return pair;

            pair = new StylePair
            {
                Main = CreateStyle(fontSize, Color.white, bold, italic, currentFont),
                Border = CreateStyle(fontSize, Color.black, bold, italic, currentFont),
                Font = currentFont
            };
            _stylePairs[key] = pair;
            return pair;
        }

        private static GUIStyle CreateStyle(int fontSize, Color color,
            bool bold, bool italic, Font font)
        {
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;

            // 先设字体（Unity 2017 里赋值 font 会重置 fontStyle，所以后面要再设一次）
            if (font != null) style.font = font;

            // 再设置 fontStyle（放在 font 之后，确保不被覆盖）
            if (bold && italic) style.fontStyle = FontStyle.BoldAndItalic;
            else if (bold) style.fontStyle = FontStyle.Bold;
            else if (italic) style.fontStyle = FontStyle.Italic;
            else style.fontStyle = FontStyle.Normal;

            style.fontSize = fontSize;
            style.normal.textColor = color;
            return style;
        }

        private void DrawTextWithBorder(Rect rect, string text, int fontSize,
            Color mainColor, Color borderColor, bool bold, bool italic)
        {
            string mode = Plugin.ComboBorderStyle.Value;
            float scale = Plugin.ComboUIScale.Value;

            var pair = GetStylePair(fontSize, bold, italic);
            pair.Main.normal.textColor = mainColor;
            pair.Border.normal.textColor = borderColor;

            if (mode == "None")
            {
                GUI.Label(rect, text, pair.Main);
                return;
            }

            if (mode == "Scale")
            {
                int scaleOffset = Mathf.Max(1, Plugin.ComboScaleBorderOffset.Value);
                var bigPair = GetStylePair(fontSize + scaleOffset, bold, italic);
                bigPair.Main.normal.textColor = borderColor;
                GUI.Label(rect, text, bigPair.Main);
                GUI.Label(rect, text, pair.Main);
                return;
            }

            // Spread
            int offset = Mathf.Max(1, (int)(Plugin.ComboBorderOffset.Value * scale));
            for (int i = 0; i < 8; i++)
            {
                GUI.Label(new Rect(rect.x + BorderDx[i] * offset, rect.y + BorderDy[i] * offset,
                    rect.width, rect.height), text, pair.Border);
            }
            GUI.Label(rect, text, pair.Main);
        }

        private void OnGUI()
        {
            if (UiSuppress.All) return;
            if (!Plugin.ComboEnabled.Value) return;

            // 暂停信息面板（独立于连杀系统）
            if (Plugin.PauseInfoDetail.Value > 0 && UiMaster.pauseGame)
            {
                try
                {
                    PauseInfoRenderer.Draw();
                }
                catch (System.Exception e)
                {
                    Plugin.Log.LogError("[ComboUI] PauseInfo Exception: " + e.ToString());
                }
            }

            if (ShouldHideComboUI()) return;

            // 判断连杀是否处于激活状态
            bool comboActive = ComboState.comboCount > 0
                            && ComboState.gameTime <= ComboState.comboEndTime;

            try
            {
                float scale = Plugin.ComboUIScale.Value;
                float anchorX = Screen.width * Plugin.ComboUIPosXPercent.Value / 100f;
                float anchorY = Screen.height * Plugin.ComboUIPosYPercent.Value / 100f;

                // ============================================================
                // 连杀部分：只在 combo 激活时绘制
                // ============================================================
                if (comboActive)
                {
                    // ===== 计算淡出 alpha =====
                    float remainSec = ComboState.comboEndTime - ComboState.gameTime;
                    float fadeAlpha = 1f;
                    float fadeDuration = FADE_DURATION_SEC;
                    if (fadeDuration <= 0f)
                    {
                        fadeAlpha = 1f;
                    }
                    else if (remainSec < fadeDuration)
                    {
                        fadeAlpha = Mathf.Clamp01(remainSec / fadeDuration);
                    }

                    // ===== 提前获取档位信息（时间条 + 字母共用）=====
                    string rank;
                    Color mainColor, borderColor;
                    GetRank(ComboState.comboCount, out rank, out mainColor, out borderColor);
                    int rankIdx = RankToIndex(rank);
                    float rankAmp = PULSE_AMP_BY_RANK[rankIdx];
                    float windowSec = Plugin.ComboWindowMs.Value / 1000f;

                    // ============ 时间条 ============
                    float barW = Plugin.TimerBarWidth.Value * scale;
                    float barH = Plugin.TimerBarHeight.Value * scale;
                    float barX = anchorX - barW / 2f;
                    float barY = anchorY - barH / 2f;

                    float remain = ComboState.comboEndTime - ComboState.gameTime;
                    float ratio = Mathf.Clamp01(remain / windowSec);

                    // 时间条颜色：绿底 → 当前档位字母主色，复用字母脉冲的全部节奏参数
                    Color barColor = ParseColor(Plugin.ComboTimerBarColor.Value, Color.green);
                    if (Plugin.LetterPulse.Value)
                    {
                        try
                        {
                            barColor = PulseColor(barColor, mainColor, rankAmp, remainSec, windowSec);
                        }
                        catch (Exception e)
                        {
                            Plugin.Log.LogError("[ComboUI] BarPulse Exception: " + e.Message);
                        }
                    }
                    barColor.a *= fadeAlpha;

                    // 白底（最外层）
                    GUI.color = new Color(1f, 1f, 1f, fadeAlpha);
                    GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);
                    // 黑底（内衬）
                    GUI.color = new Color(0f, 0f, 0f, fadeAlpha);
                    GUI.DrawTexture(new Rect(barX + 2, barY + 2, barW - 4, barH - 4), Texture2D.whiteTexture);
                    // 彩色进度条
                    GUI.color = barColor;
                    GUI.DrawTexture(new Rect(barX + 2, barY + 2, (barW - 4) * ratio, barH - 4), Texture2D.whiteTexture);

                    // 两侧三角形（保持白，不受脉冲影响）
                    GUI.color = new Color(1f, 1f, 1f, fadeAlpha);
                    float triW = 6f * scale;
                    float triH = barH;
                    for (int dx = 0; dx < (int)triW; dx++)
                    {
                        float h = triH * (1f - (float)dx / triW);
                        float y = barY + (triH - h) / 2f;
                        GUI.DrawTexture(new Rect(barX - dx - 1, y, 1, h), Texture2D.whiteTexture);
                    }
                    for (int dx = 0; dx < (int)triW; dx++)
                    {
                        float h = triH * (1f - (float)dx / triW);
                        float y = barY + (triH - h) / 2f;
                        GUI.DrawTexture(new Rect(barX + barW + dx, y, 1, h), Texture2D.whiteTexture);
                    }
                    GUI.color = Color.white;

                    // ============ 字母 ============
                    mainColor.a *= fadeAlpha;
                    borderColor.a *= fadeAlpha;

                    // 字母主色脉动（朝下一档目标色）
                    Color letterMain = mainColor;
                    if (Plugin.LetterPulse.Value)
                    {
                        try
                        {
                            Color target = GetPulseTarget(rank);
                            letterMain = PulseColor(mainColor, target, rankAmp, remainSec, windowSec);
                        }
                        catch (Exception e)
                        {
                            Plugin.Log.LogError("[ComboUI] LetterPulse Exception: " + e.Message);
                        }
                    }

                    int letterSize = (int)(Plugin.LetterFontSize.Value * scale);
                    float letterCx = anchorX + Plugin.LetterOffsetX.Value * scale;
                    float letterCy = anchorY + Plugin.LetterOffsetY.Value * scale;
                    Rect letterRect = new Rect(letterCx - 200 * scale, letterCy - 60 * scale, 400 * scale, 120 * scale);
                    DrawTextWithBorder(letterRect, rank, letterSize, letterMain, borderColor,
                        Plugin.LetterBold.Value, Plugin.LetterItalic.Value);

                    // ============ 数字 ============
                    int numberSize = (int)(Plugin.NumberFontSize.Value * scale);
                    float numCx = anchorX + Plugin.NumberOffsetX.Value * scale;
                    float numCy = anchorY + Plugin.NumberOffsetY.Value * scale;
                    Rect numberRect = new Rect(numCx - 200 * scale, numCy - 40 * scale, 400 * scale, 80 * scale);
                    string numText = Plugin.ComboNumberPrefix.Value + ComboState.comboCount;
                    Color numMain = new Color(1f, 1f, 1f, fadeAlpha);
                    Color numBorder = new Color(0f, 0f, 0f, fadeAlpha);
                    DrawTextWithBorder(numberRect, numText, numberSize,
                        numMain, numBorder,
                        Plugin.NumberBold.Value, Plugin.NumberItalic.Value);
                }

                // ============================================================
                // HIT 部分：无论连杀是否激活，都尝试绘制
                // ============================================================
                if (Plugin.HitComboEnabled.Value && ComboState.hitCombo > 0)
                {
                    float hitRatio = ComboState.GetHitWindowRatio();
                    if (hitRatio > 0f)
                    {
                        string hitText = Plugin.ComboHitPrefix.Value + ComboState.hitCombo;

                        Color hitMain = ParseColor(Plugin.HitTextColor.Value,
                            new Color(0.78f, 0.78f, 0.78f));
                        Color hitBorder = ParseColor(Plugin.HitBorderColor.Value, Color.black);
                        hitMain.a = hitRatio;
                        hitBorder.a = hitRatio;

                        int hitSize = (int)(Plugin.HitFontSize.Value * scale);
                        float hitCx = anchorX + Plugin.HitOffsetX.Value * scale;
                        float hitCy = anchorY + Plugin.HitOffsetY.Value * scale;
                        Rect hitRect = new Rect(hitCx - 150 * scale, hitCy - 20 * scale,
                            300 * scale, 40 * scale);

                        DrawTextWithBorder(hitRect, hitText, hitSize, hitMain, hitBorder,
                            Plugin.HitBold.Value, Plugin.HitItalic.Value);
                    }
                }

                // ============================================================
                // 奖励文字：无论连杀是否激活，都尝试绘制
                // （纯 Boss 战时 HIT 奖励也可能触发，所以放外面）
                // ============================================================
                if (rewardShowTime > 0)
                {
                    float elapsed = ComboState.gameTime - rewardShowTime;
                    if (elapsed > 1.5f)
                    {
                        rewardShowTime = -1f;
                    }
                    else
                    {
                        float alpha = elapsed < 0.8f
                            ? 1f
                            : Mathf.Clamp01(1f - (elapsed - 0.8f) / 0.7f);

                        Color main = ParseColor(Plugin.ComboRewardTextColor.Value, Color.green);
                        Color border = ParseColor(Plugin.ComboRewardTextBorderColor.Value, Color.red);
                        main.a = alpha;
                        border.a = alpha;

                        int rewardSize = (int)(Plugin.RewardFontSize.Value * scale);
                        float rwCx = anchorX + Plugin.RewardOffsetX.Value * scale;
                        float rwCy = anchorY + Plugin.RewardOffsetY.Value * scale;
                        Rect rewardRect = new Rect(rwCx - 150 * scale, rwCy - 25 * scale, 300 * scale, 50 * scale);
                        DrawTextWithBorder(rewardRect, rewardText, rewardSize, main, border,
                            Plugin.RewardBold.Value, Plugin.RewardItalic.Value);
                    }
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ComboUI] Exception: " + e.ToString());
            }
        }

        // ============ 玩家组件缓存更新 ============
        // 只在玩家引用变化时重新 GetComponent。
        private static void UpdatePlayerCache()
        {
            var player = GlobalVar.mainPlayer;
            if (player == null)
            {
                _cachedPlayerRef = null;
                _cachedUiMaster = null;
                _cachedStatus = null;
                return;
            }
            if (ReferenceEquals(_cachedPlayerRef, player)) return;

            _cachedPlayerRef = player;
            _cachedUiMaster = player.GetComponent<UiMaster>();
            _cachedStatus = player.GetComponent<Status>();
        }

        private void Update()
        {
            ComboState.UpdateGameTime();
            ComboState.TickHit();

            // 清理过期的"玩家伤害过"记录（内部 30 秒节流）
            ComboState.CleanupStaleDamageRecords(
                Plugin.ComboPlayerDamageWindowMs.Value / 1000f);

            UpdatePlayerCache();

            if (ComboState.comboCount <= 0) return;

            if (GlobalVar.mainPlayer == null)
            {
                ComboState.ResetCombo();
                return;
            }

            var stat = _cachedStatus;
            if (stat == null || stat.dead)
            {
                ComboState.ResetCombo();
                return;
            }

            if (ComboState.gameTime > ComboState.comboEndTime)
            {
                ComboState.ResetCombo();
                return;
            }
        }

        private static bool ShouldHideComboUI()
        {
            UpdatePlayerCache();

            if (GlobalVar.mainPlayer == null) return true;

            var uiMaster = _cachedUiMaster;
            if (uiMaster == null) return false;   // 没有 UiMaster，不做判断

            if (!_uiMasterFieldSearched)
            {
                _uiMasterFieldSearched = true;
                _uiMasterHpField = HarmonyLib.AccessTools.Field(typeof(UiMaster), "hp");
            }
            if (_uiMasterHpField == null) return false;

            var hpObj = _uiMasterHpField.GetValue(uiMaster) as GameObject;
            if (hpObj == null) return false;

            return !hpObj.activeInHierarchy;
        }

        // ============ 暂停信息面板 ============
        // 注：此方法已被 PauseInfoRenderer.Draw() 取代，保留是为了兼容旧调用 / 备忘。
        private List<string> BuildPauseInfoLines(int detail)
        {
            var lines = new List<string>();

            // ===== 档位 1：核心 =====
            lines.Add("连杀窗口: " + (Plugin.ComboWindowMs.Value / 1000f).ToString("0.0") + "s");

            string challengeMode;
            int hitMode = Plugin.ChallengeHitMode.Value;
            if (hitMode == 0)
            {
                challengeMode = "无 (简单)";
            }
            else if (hitMode == 2)
            {
                challengeMode = "重置 (挑战)";
            }
            else
            {
                int reduceType = Plugin.ChallengeHitReduceType.Value;
                if (reduceType == 0)
                    challengeMode = "减 " + Plugin.ChallengeHitReduceSeconds.Value + "s";
                else if (reduceType == 2)
                    challengeMode = "减 " + Plugin.ChallengeHitReducePercent.Value + "% (当前)";
                else
                    challengeMode = "减 " + Plugin.ChallengeHitReducePercent.Value + "% (最大)";
            }
            lines.Add("挑战模式: " + challengeMode);

            string defenseRecover;
            if (!Plugin.ChallengeDefenseRecoverEnabled.Value)
            {
                defenseRecover = "关闭";
            }
            else
            {
                int recoverType = Plugin.ChallengeDefenseRecoverType.Value;
                if (recoverType == 0)
                    defenseRecover = Plugin.ChallengeDefenseRecoverSeconds.Value.ToString("0.0") + "s";
                else
                    defenseRecover = Plugin.ChallengeDefenseRecoverPercent.Value + "% (最大)";
            }
            lines.Add("防御恢复: " + defenseRecover);

            if (Plugin.HitComboEnabled.Value)
                lines.Add("HIT 窗口: " + (Plugin.HitWindowMs.Value / 1000f).ToString("0.0") + "s");
            else
                lines.Add("HIT 连击: 关闭");

            if (detail < 2) return lines;

            // ===== 档位 2：详细 =====
            int minDiff = Plugin.ComboMinEnemyLevelDiff.Value;
            string levelReq;
            if (minDiff <= 0)
                levelReq = "敌人等级 ≥ 玩家 " + minDiff;
            else
                levelReq = "敌人等级 ≥ 玩家 +" + minDiff;
            lines.Add("连杀判定: " + levelReq);

            lines.Add("有效击杀: 玩家伤害后 "
                + (Plugin.ComboPlayerDamageWindowMs.Value / 1000f).ToString("0.0") + "s 内");

            if (Plugin.HitComboEnabled.Value)
            {
                lines.Add("HIT 恢复: " + ComboState.HIT_RECOVER_MAX_PER_WINDOW.ToString("0.0")
                    + "s / " + ComboState.HIT_RECOVER_WINDOW_SEC.ToString("0.0") + "s");
            }

            string reward;
            if (Plugin.HitRewardEvery.Value > 0)
            {
                reward = "每 " + Plugin.HitRewardEvery.Value + " HIT / "
                    + Plugin.StaminaRewardPercent.Value + "%";
            }
            else
            {
                reward = "每 " + Plugin.StaminaRewardEvery.Value + " 杀 / "
                    + Plugin.StaminaRewardPercent.Value + "%";
            }
            lines.Add("体力奖励: " + reward);

            if (detail < 3) return lines;

            // ===== 档位 3：全部 =====

            // --- 字体 ---
            string comboFont = Plugin.ComboFontName.Value;
            lines.Add("连杀字体: " + (string.IsNullOrEmpty(comboFont) ? "默认" : comboFont));

            // --- 生存 ---
            lines.Add("──────── 生存 ────────");
            lines.Add("闪避无敌: " + (Plugin.DodgeInvincibleMs.Value / 1000f).ToString("0.0") + "s");
            lines.Add("受击无敌: " + (Plugin.HurtInvincibleMs.Value / 1000f).ToString("0.0")
                + "s (冷却 " + (Plugin.HurtInvincibleCooldownMs.Value / 1000f).ToString("0.0") + "s)");

            // --- 等级补正 ---
            lines.Add("──────── 等级补正 ────────");
            if (!Plugin.LevelBonusEnabled.Value)
            {
                lines.Add("已关闭");
            }
            else
            {
                lines.Add("暴击率: +" + Plugin.LevelCritRatePercentPerLevel.Value
                    + "%/级 (封顶 " + Plugin.LevelCritRateMaxDiff.Value + " 级)");

                lines.Add("经验: +" + Plugin.LevelExpPercentPerLevel.Value
                    + "%/级 (越级) / -" + Plugin.LevelExpNegativePercentPerLevel.Value
                    + "%/级 (越低级, 下限 " + Plugin.LevelExpMinPercent.Value + "%)");

                lines.Add("掉率: +" + Plugin.LevelDropPercentPerLevel.Value + "%/级 (越级)");

                if (Plugin.MultiCritEnabled.Value)
                {
                    lines.Add("多重暴击: +" + Plugin.LevelMultiCritPercentPerLevel.Value
                        + "%/级 (封顶 " + Plugin.LevelMultiCritMaxDiff.Value
                        + " 级, 最多 " + Plugin.MultiCritMaxExtra.Value + " 段)");
                }
                else
                {
                    lines.Add("多重暴击: 关闭");
                }
            }

            // --- 其他 ---
            lines.Add("──────── 其他 ────────");
            string sprint = Plugin.SprintCostPercent.Value + "% 消耗 / "
                + (Plugin.SprintAnyDirection.Value ? "全方向" : "仅正面");
            lines.Add("冲刺: " + sprint);

            lines.Add("拾取范围: " + Plugin.PickupRangePercent.Value + "%");
            lines.Add("敌人误伤: " + (Plugin.EnemyFriendlyFire.Value ? "开" : "关"));

            return lines;
        }
    }
}