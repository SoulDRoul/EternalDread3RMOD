using System;
using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    public static class PauseInfoRenderer
    {
        private const int FontSize = 19;
        private const int LineSpacing = 4;
        private const int PadX = 12;
        private const int PadY = 10;
        private const int BorderThickness = 2;
        private const int PanelGap = 6;

        private const int MarginTop = 40;
        private const int MarginBottom = 30;
        private const int MarginRight = 30;

        private const int Detail3MarginLeft = 30;
        private const int Detail3MarginTop = 40;

        private static readonly Color DefaultTextColor = new Color(230f / 255f, 230f / 255f, 230f / 255f, 1f);
        private static readonly Color TextBorderColor = Color.black;
        private static readonly Color DefaultBgColor = new Color(0f, 0f, 0f, 180f / 255f);
        private static readonly Color DefaultBorderColor = new Color(100f / 255f, 100f / 255f, 100f / 255f, 1f);
        private static readonly Color SectionTitleColor = new Color(150f / 255f, 150f / 255f, 150f / 255f, 1f);
        private static readonly Color DisabledColor = new Color(140f / 255f, 140f / 255f, 140f / 255f, 1f);

        private static readonly Color PdTextColor = new Color(255f / 255f, 50f / 255f, 50f / 255f, 1f);
        private static readonly Color PdBgColor = new Color(50f / 255f, 10f / 255f, 10f / 255f, 180f / 255f);
        private static readonly Color PdBorderColor = new Color(150f / 255f, 25f / 255f, 25f / 255f, 1f);

        // ============ GUIStyle 缓存 ============
        // Draw() 由 ComboUI.OnGUI 每帧调用，原实现每绘制一行都会 new GUIStyle。
        // 这里按 (fontSize, color) 缓存，避免每帧堆分配。
        private static readonly Dictionary<long, GUIStyle> _styleCache = new Dictionary<long, GUIStyle>();

        // ============ Box 结构缓存（脏标记） ============
        // Box / Line 内容只在 detail / 难度 / 永久死亡 / 分辨率变化时重建。
        private static Cache _cache;
        private static int _cachedDetail = -1;
        private static int _cachedDiff = -1;
        private static bool _cachedPd;
        private static int _cachedScreenW = -1;
        private static int _cachedScreenH = -1;

        private class Cache
        {
            public List<Box> RightBoxes;
            public Box LeftBox;
            public float RightWidth;
            public float LeftWidth;
        }

        private class Line
        {
            public string Text;
            public GUIContent Content;
            public bool HasOverride;
            public Color OverrideColor;

            public Line(string text)
            {
                Text = text;
                Content = new GUIContent(text);
            }
            public Line(string text, Color color)
            {
                Text = text;
                Content = new GUIContent(text);
                HasOverride = true;
                OverrideColor = color;
            }
        }

        private class Box
        {
            public List<Line> Lines = new List<Line>();
            public Color TextColor = DefaultTextColor;
            public Color BgColor = DefaultBgColor;
            public Color BorderColor = DefaultBorderColor;
        }

        public static void Draw()
        {
            int detail = Plugin.PauseInfoDetail.Value;
            if (detail <= 0) return;

            int diff = GlobalVar.difficultLv;
            bool pd = PermadeathState.CurrentSavePermadeath;
            int sw = Screen.width;
            int sh = Screen.height;

            if (_cache == null
                || _cachedDetail != detail
                || _cachedDiff != diff
                || _cachedPd != pd
                || _cachedScreenW != sw
                || _cachedScreenH != sh)
            {
                RebuildCache(detail);
                _cachedDetail = detail;
                _cachedDiff = diff;
                _cachedPd = pd;
                _cachedScreenW = sw;
                _cachedScreenH = sh;
            }

            DrawRightStack(_cache.RightBoxes, _cache.RightWidth);
            if (detail >= 3 && _cache.LeftBox != null)
                DrawLeftBox(_cache.LeftBox, _cache.LeftWidth);
        }

        private static void RebuildCache(int detail)
        {
            var cache = new Cache();

            cache.RightBoxes = new List<Box>();
            cache.RightBoxes.Add(BuildStatusBox());
            if (detail >= 1) cache.RightBoxes.Add(BuildCoreSummaryBox());
            if (detail >= 2)
            {
                cache.RightBoxes.Add(BuildComboDetailBox());
                cache.RightBoxes.Add(BuildSurvivalCombatBox());
                cache.RightBoxes.Add(BuildLevelBonusDetailBox());
                cache.RightBoxes.Add(BuildStatusEffectBox());
            }
            cache.RightWidth = ComputeBoxWidth(cache.RightBoxes);

            if (detail >= 3)
            {
                cache.LeftBox = BuildAllFeaturesBox();
                cache.LeftWidth = ComputeSingleBoxWidth(cache.LeftBox);
            }

            _cache = cache;
        }

        private static void DrawRightStack(List<Box> boxes, float width)
        {
            if (boxes.Count == 0) return;

            float totalHeight = ComputeTotalHeight(boxes);
            float availHeight = Screen.height - MarginTop - MarginBottom;
            float rightX = Screen.width - MarginRight;
            float leftX = rightX - width;

            if (totalHeight <= availHeight)
            {
                float y = Screen.height - MarginBottom;
                for (int i = boxes.Count - 1; i >= 0; i--)
                {
                    float h = ComputeBoxHeight(boxes[i]);
                    y -= h;
                    DrawBox(boxes[i], leftX, y, width);
                    y -= PanelGap;
                }
            }
            else
            {
                float y = MarginTop;
                for (int i = 0; i < boxes.Count; i++)
                {
                    float h = ComputeBoxHeight(boxes[i]);
                    DrawBox(boxes[i], leftX, y, width);
                    y += h + PanelGap;
                }
            }
        }

        private static void DrawLeftBox(Box box, float width)
        {
            if (box.Lines.Count == 0) return;
            DrawBox(box, Detail3MarginLeft, Detail3MarginTop, width);
        }

        private static float ComputeBoxWidth(List<Box> boxes)
        {
            float maxW = 0f;
            for (int i = 0; i < boxes.Count; i++)
            {
                for (int j = 0; j < boxes[i].Lines.Count; j++)
                {
                    Vector2 size = GetStyle(FontSize, DefaultTextColor)
                        .CalcSize(boxes[i].Lines[j].Content);
                    if (size.x > maxW) maxW = size.x;
                }
            }
            return maxW + PadX * 2 + BorderThickness * 2;
        }

        private static float ComputeSingleBoxWidth(Box box)
        {
            float maxW = 0f;
            for (int j = 0; j < box.Lines.Count; j++)
            {
                Vector2 size = GetStyle(FontSize, DefaultTextColor)
                    .CalcSize(box.Lines[j].Content);
                if (size.x > maxW) maxW = size.x;
            }
            return maxW + PadX * 2 + BorderThickness * 2;
        }

        private static float ComputeBoxHeight(Box box)
        {
            float lineHeight = FontSize + LineSpacing;
            return lineHeight * box.Lines.Count + PadY * 2 + BorderThickness * 2;
        }

        private static float ComputeTotalHeight(List<Box> boxes)
        {
            float total = 0f;
            for (int i = 0; i < boxes.Count; i++)
            {
                total += ComputeBoxHeight(boxes[i]);
                if (i < boxes.Count - 1) total += PanelGap;
            }
            return total;
        }

        private static void DrawBox(Box box, float leftX, float topY, float width)
        {
            if (box.Lines.Count == 0) return;

            float lineHeight = FontSize + LineSpacing;
            float height = lineHeight * box.Lines.Count + PadY * 2 + BorderThickness * 2;

            if (box.BorderColor.a > 0f)
            {
                GUI.color = box.BorderColor;
                GUI.DrawTexture(new Rect(leftX, topY, width, height), Texture2D.whiteTexture);
            }

            if (box.BgColor.a > 0f)
            {
                GUI.color = box.BgColor;
                GUI.DrawTexture(new Rect(
                    leftX + BorderThickness,
                    topY + BorderThickness,
                    width - BorderThickness * 2,
                    height - BorderThickness * 2), Texture2D.whiteTexture);
            }

            GUI.color = Color.white;

            float textX = leftX + BorderThickness + PadX;
            float textY = topY + BorderThickness + PadY;
            float textW = width - BorderThickness * 2 - PadX * 2;

            for (int i = 0; i < box.Lines.Count; i++)
            {
                var line = box.Lines[i];
                Color c = line.HasOverride ? line.OverrideColor : box.TextColor;
                Rect rect = new Rect(textX, textY + i * lineHeight, textW, lineHeight);
                DrawLine(rect, line.Content, c, TextBorderColor);
            }
        }

        private static readonly int[] BorderDx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] BorderDy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        private static void DrawLine(Rect rect, GUIContent content, Color textColor, Color borderColor)
        {
            var mainStyle = GetStyle(FontSize, textColor);
            if (borderColor.a <= 0f)
            {
                GUI.Label(rect, content, mainStyle);
                return;
            }

            var borderStyle = GetStyle(FontSize, borderColor);
            for (int i = 0; i < 8; i++)
            {
                GUI.Label(new Rect(rect.x + BorderDx[i], rect.y + BorderDy[i], rect.width, rect.height),
                    content, borderStyle);
            }
            GUI.Label(rect, content, mainStyle);
        }

        private static GUIStyle MakeStyle(int fontSize, Color color)
        {
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleLeft;
            style.wordWrap = false;
            style.fontSize = fontSize;
            style.normal.textColor = color;
            return style;
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
            long key = StyleKey(fontSize, color);
            if (_styleCache.TryGetValue(key, out var style)) return style;
            style = MakeStyle(fontSize, color);
            _styleCache[key] = style;
            return style;
        }

        private static void AddLine(Box box, string text, bool enabled)
        {
            box.Lines.Add(new Line(text, enabled ? DefaultTextColor : DisabledColor));
        }

        private static void AddSectionTitle(Box box, string title)
        {
            box.Lines.Add(new Line("── " + title + " ──", SectionTitleColor));
        }

        private static Box BuildStatusBox()
        {
            var box = new Box();
            bool pd = PermadeathState.CurrentSavePermadeath;
            string diffName = GetDifficultyName();

            if (pd)
            {
                box.TextColor = PdTextColor;
                box.BgColor = PdBgColor;
                box.BorderColor = PdBorderColor;
                box.Lines.Add(new Line(Loc.T("ui.pause.permadeath", "[ ! ] 永久死亡模式") + "  |  " + Loc.T("ui.pause.difficulty", "难度") + ": " + diffName));
            }
            else
            {
                box.Lines.Add(new Line(Loc.T("ui.pause.difficulty", "难度") + ": " + diffName));
            }

            return box;
        }

        private static string GetDifficultyName()
        {
            switch (GlobalVar.difficultLv)
            {
                case 0: return Loc.T("ui.pause.diff.simple", "简单");
                case 1: return Loc.T("ui.pause.diff.normal", "普通");
                case 2: return Loc.T("ui.pause.diff.challenge", "挑战");
                case 3: return Loc.T("ui.pause.diff.hell", "地狱");
                default: return Loc.T("ui.pause.diff.normal", "普通");
            }
        }

        private static Box BuildCoreSummaryBox()
        {
            var box = new Box();

            box.Lines.Add(new Line(Loc.T("ui.pause.combo_window", "连杀窗口") + ": "
                + (Plugin.ComboWindowMs.Value / 1000f).ToString("0.0") + "s"));

            box.Lines.Add(new Line(Loc.T("ui.pause.challenge_mode", "挑战模式") + ": " + FormatChallengeMode()));
            box.Lines.Add(new Line(Loc.T("ui.pause.defense_recover", "防御恢复") + ": " + FormatDefenseRecover()));

            AddLine(box, Loc.T("ui.pause.level_bonus", "等级补正") + ": " + (Plugin.LevelBonusEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.LevelBonusEnabled.Value);

            int scale = GetCurrentEnemyScale();
            box.Lines.Add(new Line(Loc.T("ui.pause.enemy_level_cap", "敌人等级上限") + ": " + scale + "x"));

            int nightChance = GetCurrentNightChance();
            int nightLevel = GetCurrentNightLevel();
            if (nightChance > 0 || nightLevel > 0)
            {
                box.Lines.Add(new Line(Loc.T("ui.pause.night_bonus", "夜晚敌人加成") + ": " + nightChance + "% " + Loc.T("ui.pause.night_affix_rate", "词条率") + " / +" + nightLevel + " " + Loc.T("ui.pause.levels", "级")));
            }
            else
            {
                box.Lines.Add(new Line(Loc.T("ui.pause.night_bonus", "夜晚敌人加成") + ": " + Loc.T("ui.pause.off", "关"), DisabledColor));
            }

            box.Lines.Add(new Line(Loc.T("ui.pause.sprint_cost", "冲刺消耗") + ": " + Plugin.SprintCostPercent.Value + "%"));

            return box;
        }

        private static Box BuildComboDetailBox()
        {
            var box = new Box();

            AddLine(box, Loc.T("ui.pause.hit_window", "HIT 窗口") + ": "
                + (Plugin.HitWindowMs.Value / 1000f).ToString("0.0") + "s",
                Plugin.HitComboEnabled.Value);

            if (Plugin.HitComboEnabled.Value)
            {
                box.Lines.Add(new Line(Loc.T("ui.pause.hit_recover", "HIT 恢复") + ": "
                    + ComboState.HIT_RECOVER_MAX_PER_WINDOW.ToString("0.0") + "s / "
                    + ComboState.HIT_RECOVER_WINDOW_SEC.ToString("0.0") + "s"));
            }
            else
            {
                box.Lines.Add(new Line(Loc.T("ui.pause.hit_recover", "HIT 恢复") + ": " + Loc.T("ui.pause.off", "关"), DisabledColor));
            }

            string reward;
            if (Plugin.HitRewardEvery.Value > 0)
            {
                reward = Loc.T("ui.pause.every", "每") + " " + Plugin.HitRewardEvery.Value + " HIT / "
                    + Plugin.StaminaRewardPercent.Value + "%";
            }
            else
            {
                reward = Loc.T("ui.pause.every", "每") + " " + Plugin.StaminaRewardEvery.Value + " " + Loc.T("ui.pause.kills", "杀") + " / "
                    + Plugin.StaminaRewardPercent.Value + "%";
            }
            box.Lines.Add(new Line(Loc.T("ui.pause.stamina_reward", "体力奖励") + ": " + reward));

            box.Lines.Add(new Line(Loc.T("ui.pause.valid_kill", "有效击杀") + ": " + Loc.T("ui.pause.after_damage", "伤害后") + " "
                + (Plugin.ComboPlayerDamageWindowMs.Value / 1000f).ToString("0.0") + "s " + Loc.T("ui.pause.within", "内")));

            int minDiff = Plugin.ComboMinEnemyLevelDiff.Value;
            string levelReq = minDiff <= 0
                ? Loc.T("ui.pause.enemy_level_ge_player", "敌人等级 ≥ 玩家") + " " + minDiff
                : Loc.T("ui.pause.enemy_level_ge_player", "敌人等级 ≥ 玩家") + " +" + minDiff;
            box.Lines.Add(new Line(Loc.T("ui.pause.combo_judge", "连杀判定") + ": " + levelReq));

            return box;
        }

        private static Box BuildSurvivalCombatBox()
        {
            var box = new Box();

            box.Lines.Add(new Line(Loc.T("ui.pause.dodge_invincible", "闪避无敌") + ": "
                + (Plugin.DodgeInvincibleMs.Value / 1000f).ToString("0.0") + "s"));

            box.Lines.Add(new Line(Loc.T("ui.pause.hurt_invincible", "受击无敌") + ": "
                + (Plugin.HurtInvincibleMs.Value / 1000f).ToString("0.0")
                + "s (" + Loc.T("ui.pause.cooldown", "冷却") + " " + (Plugin.HurtInvincibleCooldownMs.Value / 1000f).ToString("0.0") + "s)"));

            AddLine(box, Loc.T("ui.pause.sprint_direction", "冲刺方向") + ": "
                + (Plugin.SprintAnyDirection.Value ? Loc.T("ui.pause.all_directions", "全方向") : Loc.T("ui.pause.front_only", "仅正面")),
                Plugin.SprintAnyDirection.Value);

            AddLine(box, Loc.T("ui.pause.pickup_range", "拾取范围") + ": " + Plugin.PickupRangePercent.Value + "%",
                Plugin.PickupRangeEnabled.Value);

            AddLine(box, Loc.T("ui.pause.self_damage", "自伤") + ": "
                + (Plugin.AoeSelfDamageEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关"))
                + " (" + Loc.T("ui.pause.player", "玩家") + " " + Plugin.SelfDamagePlayerPercent.Value + "%)",
                Plugin.AoeSelfDamageEnabled.Value);

            AddLine(box, Loc.T("ui.pause.ally_friendly_fire", "友军误伤玩家") + ": "
                + (Plugin.AoeAllyFriendlyFireEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.AoeAllyFriendlyFireEnabled.Value);

            AddLine(box, Loc.T("ui.pause.backstab", "背刺") + ": "
                + (Plugin.BackStabEnabled.Value
                    ? "+" + Plugin.BackStabBonusCrit.Value + "% " + Loc.T("ui.pause.crit", "暴击")
                    : Loc.T("ui.pause.off", "关")),
                Plugin.BackStabEnabled.Value);

            return box;
        }

        private static Box BuildLevelBonusDetailBox()
        {
            var box = new Box();
            bool on = Plugin.LevelBonusEnabled.Value;

            if (!on)
            {
                box.Lines.Add(new Line(Loc.T("ui.pause.crit_rate", "暴击率") + ": " + Loc.T("ui.pause.off", "关"), DisabledColor));
                box.Lines.Add(new Line(Loc.T("ui.pause.exp", "经验") + ": " + Loc.T("ui.pause.off", "关"), DisabledColor));
                box.Lines.Add(new Line(Loc.T("ui.pause.drop_rate", "掉率") + ": " + Loc.T("ui.pause.off", "关"), DisabledColor));
                box.Lines.Add(new Line(Loc.T("ui.pause.multi_crit", "多重暴击") + ": " + Loc.T("ui.pause.off", "关"), DisabledColor));
                return box;
            }

            box.Lines.Add(new Line(Loc.T("ui.pause.crit_rate", "暴击率") + ": +" + Plugin.LevelCritRatePercentPerLevel.Value
                + "%/" + Loc.T("ui.pause.level", "级") + " (" + Loc.T("ui.pause.cap", "封顶") + " " + Plugin.LevelCritRateMaxDiff.Value + " " + Loc.T("ui.pause.levels", "级") + ")"));

            box.Lines.Add(new Line(Loc.T("ui.pause.exp", "经验") + ": +" + Plugin.LevelExpPercentPerLevel.Value
                + "%/" + Loc.T("ui.pause.level", "级") + " (" + Loc.T("ui.pause.over_level", "越级") + ") / -" + Plugin.LevelExpNegativePercentPerLevel.Value
                + "%/" + Loc.T("ui.pause.level", "级") + " (" + Loc.T("ui.pause.under_level", "越低") + ")"));

            box.Lines.Add(new Line(Loc.T("ui.pause.drop_rate", "掉率") + ": +" + Plugin.LevelDropPercentPerLevel.Value
                + "%/" + Loc.T("ui.pause.level", "级") + " (" + Loc.T("ui.pause.over_level", "越级") + ")"));

            if (Plugin.MultiCritEnabled.Value)
            {
                box.Lines.Add(new Line(Loc.T("ui.pause.multi_crit", "多重暴击") + ": +" + Plugin.LevelMultiCritPercentPerLevel.Value
                    + "%/" + Loc.T("ui.pause.level", "级") + " (" + Loc.T("ui.pause.cap", "封顶") + " " + Plugin.LevelMultiCritMaxDiff.Value
                    + " " + Loc.T("ui.pause.levels", "级") + ", " + Loc.T("ui.pause.max", "最多") + " " + Plugin.MultiCritMaxExtra.Value + " " + Loc.T("ui.pause.segments", "段") + ")"));
            }
            else
            {
                box.Lines.Add(new Line(Loc.T("ui.pause.multi_crit", "多重暴击") + ": " + Loc.T("ui.pause.off", "关"), DisabledColor));
            }

            return box;
        }

        private static Box BuildStatusEffectBox()
        {
            var box = new Box();

            AddLine(box, Loc.T("ui.pause.stun_crit", "眩晕暴击") + ": "
                + (Plugin.StunCritBonusEnabled.Value
                    ? "+" + Plugin.StunCritBonusPercent.Value + "%"
                    : Loc.T("ui.pause.off", "关")),
                Plugin.StunCritBonusEnabled.Value);

            AddLine(box, Loc.T("ui.pause.frost_pierce", "冰冻穿透") + ": "
                + (Plugin.FrostPierceEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.FrostPierceEnabled.Value);

            AddLine(box, Loc.T("ui.pause.dot_kill", "毒烧致死") + ": "
                + (Plugin.DoTKillEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.DoTKillEnabled.Value);

            AddLine(box, Loc.T("ui.pause.burn_duration", "燃烧时长") + ": x" + Plugin.BurnDurationMul.Value,
                Plugin.BurnDurationMulEnabled.Value);

            AddLine(box, Loc.T("ui.pause.burn_spread", "燃烧蔓延") + ": "
                + (Plugin.BurnSpreadEnabled.Value
                    ? Plugin.BurnSpreadRadius.Value.ToString("0.0") + "m"
                    : Loc.T("ui.pause.off", "关")),
                Plugin.BurnSpreadEnabled.Value);

            return box;
        }

        private static Box BuildAllFeaturesBox()
        {
            var box = new Box();

            AddSectionTitle(box, Loc.T("ui.pause.section.combat", "战斗"));

            AddLine(box, Loc.T("ui.pause.self_damage", "自伤") + ": " + (Plugin.AoeSelfDamageEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关"))
                + " (" + Loc.T("ui.pause.player", "玩家") + " " + Plugin.SelfDamagePlayerPercent.Value + "%)",
                Plugin.AoeSelfDamageEnabled.Value);

            AddLine(box, Loc.T("ui.pause.ally_friendly_fire", "友军误伤") + ": " + (Plugin.AoeAllyFriendlyFireEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.AoeAllyFriendlyFireEnabled.Value);

            AddLine(box, Loc.T("ui.pause.backstab", "背刺") + ": " + (Plugin.BackStabEnabled.Value
                ? "+" + Plugin.BackStabBonusCrit.Value + "% " + Loc.T("ui.pause.crit", "暴击") : Loc.T("ui.pause.off", "关")),
                Plugin.BackStabEnabled.Value);

            AddLine(box, Loc.T("ui.pause.flank_crit", "绕后暴击") + ": " + (Plugin.FlankCritEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.FlankCritEnabled.Value);

            AddLine(box, Loc.T("ui.pause.charge_stun", "冲锋自晕") + ": " + (Plugin.ChargeStunEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.ChargeStunEnabled.Value);

            AddLine(box, Loc.T("ui.pause.guard_counter", "格挡反击") + ": " + (Plugin.GuardCounterEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.GuardCounterEnabled.Value);

            AddLine(box, Loc.T("ui.pause.guard_interrupt", "格挡取消") + ": " + (Plugin.GuardInterruptEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.GuardInterruptEnabled.Value);

            AddLine(box, Loc.T("ui.pause.downed_boost", "倒地加成") + ": " + (Plugin.DownedDamageBoostEnabled.Value
                ? "+" + Plugin.DownedDamageBoostPercent.Value + "% " + Loc.T("ui.pause.damage", "伤害") : Loc.T("ui.pause.off", "关")),
                Plugin.DownedDamageBoostEnabled.Value);

            AddLine(box, Loc.T("ui.pause.elemental_immunity", "属性免疫修正") + ": " + (Plugin.ElementalImmunityFixEnabled.Value
                ? Loc.T("ui.pause.min", "最低") + " " + Plugin.ElementalImmunityPercent.Value + "% " + Loc.T("ui.pause.damage", "伤害") : Loc.T("ui.pause.off", "关")),
                Plugin.ElementalImmunityFixEnabled.Value);

            AddLine(box, Loc.T("ui.pause.phys_magic_pierce", "物魔免属性穿透") + ": " + (Plugin.ImmunityFixEnabled.Value
                ? Plugin.ElementalPiercePercent.Value + "%" : Loc.T("ui.pause.off", "关")),
                Plugin.ImmunityFixEnabled.Value);

            AddSectionTitle(box, Loc.T("ui.pause.section.ai", "AI / 敌人"));

            AddLine(box, Loc.T("ui.pause.cone_vision", "锥形视野") + ": " + (Plugin.ConeVisionEnabled.Value
                ? Plugin.ConeVisionAngle.Value + "°" : Loc.T("ui.pause.off", "关")),
                Plugin.ConeVisionEnabled.Value);

            AddLine(box, Loc.T("ui.pause.detect_range", "探测范围") + ": " + GetCurrentDetectScale().ToString("0.0") + "x",
                GetCurrentDetectScale() != 1.0f);

            AddLine(box, Loc.T("ui.pause.angry_duration", "愤怒时长") + ": " + GetCurrentAngryScale().ToString("0.0") + "x",
                GetCurrentAngryScale() != 1.0f);

            AddLine(box, Loc.T("ui.pause.call_radius", "呼叫范围") + ": " + GetCurrentCallScale().ToString("0.0") + "x",
                GetCurrentCallScale() != 1.0f);

            AddLine(box, Loc.T("ui.pause.noise_alert", "噪音警觉") + ": " + (Plugin.NoiseEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.NoiseEnabled.Value);

            AddLine(box, Loc.T("ui.pause.alert_mark", "惊叹号提示") + ": " + (Plugin.AlertEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.AlertEnabled.Value);

            AddSectionTitle(box, Loc.T("ui.pause.section.skill", "技能"));

            AddLine(box, Loc.T("ui.pause.exp_requirement", "升级经验需求") + ": " + (Plugin.ExpRequirementEnabled.Value
                ? FormatPercents(Plugin.ExpRequirementPercents.Value) : Loc.T("ui.pause.vanilla", "原版")),
                Plugin.ExpRequirementEnabled.Value);

            AddLine(box, Loc.T("ui.pause.skill_prereq", "技能组前置") + ": " + (Plugin.SkillPrereqRelaxEnabled.Value
                ? Loc.T("ui.pause.on", "开") + " (" + Plugin.SkillPrereqRequiredCount.Value + " " + Loc.T("ui.pause.count", "个") + ")" : Loc.T("ui.pause.off", "关")),
                Plugin.SkillPrereqRelaxEnabled.Value);

            AddLine(box, Loc.T("ui.pause.skill_point_refresh", "技能点刷新") + ": " + (Plugin.SkillPointRefreshEnabled.Value
                ? Plugin.SkillPointRefreshBaseChance.Value + "% " + Loc.T("ui.pause.base", "基础") : Loc.T("ui.pause.off", "关")),
                Plugin.SkillPointRefreshEnabled.Value);

            AddLine(box, Loc.T("ui.pause.cross_passive", "跨职业被动") + ": " + (Plugin.CrossPassiveEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.CrossPassiveEnabled.Value);

            AddLine(box, Loc.T("ui.pause.passive_slot", "被动槽位") + ": " + (Plugin.PassiveSlotExpandEnabled.Value
                ? "+1/" + Plugin.PassiveSlotPerLevels.Value + " " + Loc.T("ui.pause.level", "级") : Loc.T("ui.pause.off", "关")),
                Plugin.PassiveSlotExpandEnabled.Value);

            AddSectionTitle(box, Loc.T("ui.pause.section.convenience", "便利"));

            AddLine(box, Loc.T("ui.pause.pickup_range", "拾取范围") + ": " + Plugin.PickupRangePercent.Value + "%",
                Plugin.PickupRangeEnabled.Value);

            AddLine(box, Loc.T("ui.pause.quick_gather", "快速采集") + ": " + (Plugin.QuickGatherEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.QuickGatherEnabled.Value);

            AddLine(box, Loc.T("ui.pause.quick_use", "快速使用") + ": " + (Plugin.QuickUseItemEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.QuickUseItemEnabled.Value);

            AddLine(box, Loc.T("ui.pause.dialogue_skip", "对话跳过") + ": " + (Plugin.DialogueCtrlSkipEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.DialogueCtrlSkipEnabled.Value);

            AddLine(box, Loc.T("ui.pause.inventory_expand", "物品栏扩容") + ": " + (Plugin.InventoryExpandEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.InventoryExpandEnabled.Value);

            AddLine(box, Loc.T("ui.pause.confirm_hotkey", "物品弹窗 互动键 确认") + ": " + (Plugin.ConfirmHotkeyEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.ConfirmHotkeyEnabled.Value);

            AddLine(box, Loc.T("ui.pause.quantity_hotkey", "物品弹窗 移动键 控制数量") + ": " + (Plugin.QuantityHotkeyEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.QuantityHotkeyEnabled.Value);

            AddLine(box, Loc.T("ui.pause.auto_sort", "自动排序") + ": " + (Plugin.AutoSortOnOpen.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.AutoSortOnOpen.Value);

            AddLine(box, Loc.T("ui.pause.minimap_icon", "小地图指示") + ": " + (Plugin.MinimapIconEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.MinimapIconEnabled.Value);

            AddLine(box, Loc.T("ui.pause.threat_ring", "威胁指示") + ": " + (Plugin.ThreatRingEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.ThreatRingEnabled.Value);

            AddLine(box, Loc.T("ui.pause.player_light", "玩家光源") + ": " + (Plugin.PlayerLightEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.PlayerLightEnabled.Value);

            AddLine(box, Loc.T("ui.pause.sleep_speed", "睡眠加速") + ": " + (Plugin.SleepSpeedControlEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.SleepSpeedControlEnabled.Value);

            AddSectionTitle(box, Loc.T("ui.pause.section.ecology", "生态 / 时间"));

            AddLine(box, Loc.T("ui.pause.ecology_memory", "生态记忆") + ": " + (Plugin.EcologyEnabled.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.EcologyEnabled.Value);

            AddLine(box, Loc.T("ui.pause.cross_save_share", "跨存档共享") + ": " + (Plugin.CrossSaveShare.Value ? Loc.T("ui.pause.on", "开") : Loc.T("ui.pause.off", "关")),
                Plugin.CrossSaveShare.Value);

            AddLine(box, Loc.T("ui.pause.map_exit_cost", "地图切换") + ": " + Plugin.MapExitCostHours.Value.ToString("0.#") + "h",
                Plugin.MapExitCostHours.Value > 0f);

            AddLine(box, Loc.T("ui.pause.teleporter_cost", "传送") + ": " + Plugin.TeleporterCostMinutes.Value.ToString("0.#") + "min",
                Plugin.TeleporterCostMinutes.Value > 0f);

            AddLine(box, Loc.T("ui.pause.death_skip", "死亡跳过") + ": " + Plugin.DeathTimeSkipHours.Value.ToString("0.#") + "h",
                Plugin.DeathTimeSkipHours.Value > 0f);

            return box;
        }

        private static string FormatPercents(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            return raw.Replace(",", "%,") + "%";
        }

        private static string FormatChallengeMode()
        {
            int hitMode = Plugin.ChallengeHitMode.Value;
            if (hitMode == 0) return Loc.T("ui.pause.challenge.none", "无 (简单)");
            if (hitMode == 2) return Loc.T("ui.pause.challenge.reset", "重置 (挑战)");
            int reduceType = Plugin.ChallengeHitReduceType.Value;
            if (reduceType == 0) return Loc.T("ui.pause.challenge.reduce", "减") + " " + Plugin.ChallengeHitReduceSeconds.Value.ToString("0.0") + "s";
            if (reduceType == 2) return Loc.T("ui.pause.challenge.reduce", "减") + " " + Plugin.ChallengeHitReducePercent.Value + "% (" + Loc.T("ui.pause.current", "当前") + ")";
            return Loc.T("ui.pause.challenge.reduce", "减") + " " + Plugin.ChallengeHitReducePercent.Value + "% (" + Loc.T("ui.pause.max", "最大") + ")";
        }

        private static string FormatDefenseRecover()
        {
            if (!Plugin.ChallengeDefenseRecoverEnabled.Value) return Loc.T("ui.pause.off", "关闭");
            int type = Plugin.ChallengeDefenseRecoverType.Value;
            if (type == 0) return Plugin.ChallengeDefenseRecoverSeconds.Value.ToString("0.0") + "s";
            return Plugin.ChallengeDefenseRecoverPercent.Value + "% (" + Loc.T("ui.pause.max", "最大") + ")";
        }

        private static int GetCurrentEnemyScale()
        {
            switch (GlobalVar.difficultLv)
            {
                case 0: return Plugin.EnemyLevelScale_S0.Value;
                case 1: return Plugin.EnemyLevelScale_S1.Value;
                case 2: return Plugin.EnemyLevelScale_S2.Value;
                case 3: return Plugin.EnemyLevelScale_S3.Value;
                default: return 1;
            }
        }

        private static int GetCurrentNightChance()
        {
            switch (GlobalVar.difficultLv)
            {
                case 0: return Plugin.NightChanceS0.Value;
                case 1: return Plugin.NightChanceS1.Value;
                case 2: return Plugin.NightChanceS2.Value;
                case 3: return Plugin.NightChanceS3.Value;
                default: return 0;
            }
        }

        private static int GetCurrentNightLevel()
        {
            switch (GlobalVar.difficultLv)
            {
                case 0: return Plugin.NightLevelS0.Value;
                case 1: return Plugin.NightLevelS1.Value;
                case 2: return Plugin.NightLevelS2.Value;
                case 3: return Plugin.NightLevelS3.Value;
                default: return 0;
            }
        }

        private static float GetCurrentDetectScale()
        {
            switch (GlobalVar.difficultLv)
            {
                case 0: return Plugin.DetectRangeScaleS0.Value;
                case 1: return Plugin.DetectRangeScaleS1.Value;
                case 2: return Plugin.DetectRangeScaleS2.Value;
                case 3: return Plugin.DetectRangeScaleS3.Value;
                default: return 1f;
            }
        }

        private static float GetCurrentAngryScale()
        {
            switch (GlobalVar.difficultLv)
            {
                case 0: return Plugin.AngryDurationScaleS0.Value;
                case 1: return Plugin.AngryDurationScaleS1.Value;
                case 2: return Plugin.AngryDurationScaleS2.Value;
                case 3: return Plugin.AngryDurationScaleS3.Value;
                default: return 1f;
            }
        }

        private static float GetCurrentCallScale()
        {
            switch (GlobalVar.difficultLv)
            {
                case 0: return Plugin.CallRadiusScaleS0.Value;
                case 1: return Plugin.CallRadiusScaleS1.Value;
                case 2: return Plugin.CallRadiusScaleS2.Value;
                case 3: return Plugin.CallRadiusScaleS3.Value;
                default: return 1f;
            }
        }
    }
}