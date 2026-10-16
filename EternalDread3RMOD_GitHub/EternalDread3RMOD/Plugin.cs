using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [BepInPlugin("com.SoulDRoul.EternalDread3Rmod", "ED3R", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource Log;

        // 跑步
        public static ConfigEntry<int> SprintCostPercent;
        public static ConfigEntry<bool> SprintAnyDirection;
        public static ConfigEntry<int> WalkSpeedMul;
        public static ConfigEntry<int> SprintSpeedMul;
        //翻滚速度倍率
        public static ConfigEntry<int> DodgeSpeedPercent;
        //双击方向键的行为
        public static ConfigEntry<int> DodgeRollTapMode;

        //跳斩类技能相关
        public static ConfigEntry<bool> DodgeJumpInertiaEnabled;
        public static ConfigEntry<int> DodgeJumpInertiaMul;


        // 无敌参数
        public static ConfigEntry<int> DodgeInvincibleMs;
        public static ConfigEntry<int> HurtInvincibleMs;
        public static ConfigEntry<int> HurtInvincibleCooldownMs;
        public static ConfigEntry<bool> BlinkEnabled;


        // 敌人误伤
        public static ConfigEntry<bool> EnemyFriendlyFire;
        public static ConfigEntry<bool> EnemyAoeHitsEnemy;

        //等级补正
        public static ConfigEntry<bool> LevelBonusEnabled;
        public static ConfigEntry<int> LevelCritRatePercentPerLevel;
        public static ConfigEntry<int> LevelExpPercentPerLevel;
        public static ConfigEntry<int> LevelDropPercentPerLevel;
        public static ConfigEntry<int> LevelExpNegativePercentPerLevel;
        public static ConfigEntry<int> LevelExpMinPercent;
        public static ConfigEntry<int> LevelMultiCritPercentPerLevel;
        public static ConfigEntry<int> LevelMultiCritMaxDiff;
        public static ConfigEntry<int> LevelCritRateMaxDiff;

        // ---- 升级经验需求 ----
        public static ConfigEntry<bool> ExpRequirementEnabled;
        public static ConfigEntry<string> ExpRequirementPercents;

        // ---- 技能组前置放宽 ----
        public static ConfigEntry<bool> SkillPrereqRelaxEnabled;
        public static ConfigEntry<int> SkillPrereqRequiredCount;

        // ---- 技能点刷新 ----
        public static ConfigEntry<bool> SkillPointRefreshEnabled;
        public static ConfigEntry<int> SkillPointRefreshBaseChance;
        public static ConfigEntry<float> SkillPointRefreshEcologyGain;
        public static ConfigEntry<float> SkillPointRefreshCooldownHours;
        public static ConfigEntry<int> SkillPointRefreshDailyLimit;

        // ============ 硬编码常量：SkillPointRefresh 共享项 ============
        // 原 SkillPointRefresh.EcologyBonusMax —— 生态加成最大值（%）
        public const int SP_ECOLOGY_BONUS_MAX = 100;
        // 原 SkillPointRefresh.ChanceCap —— 刷新概率封顶（%）
        public const int SP_CHANCE_CAP = 100;

        // ---- 跨职业被动 POC ----
        public static ConfigEntry<bool> CrossPassiveEnabled;
        // ---- 跨职业被动：成本缩放 ----
        public static ConfigEntry<bool> CrossPassiveCostEnabled;
        public static ConfigEntry<int> CrossPassiveCostNearMul;
        public static ConfigEntry<int> CrossPassiveCostFarMul;

        // ---- 被动槽位扩展 ----
        public static ConfigEntry<bool> PassiveSlotExpandEnabled;
        public static ConfigEntry<int> PassiveSlotPerLevels;
        public static ConfigEntry<int> PassiveSlotMaxCount;

        //多重暴击
        public static ConfigEntry<bool> MultiCritEnabled;
        public static ConfigEntry<int> MultiCritMaxExtra;

        //拾取
        public static ConfigEntry<bool> PickupRangeEnabled;
        public static ConfigEntry<int> PickupRangePercent;

        // ============ 连杀系统 ============
        public static ConfigEntry<bool> ComboEnabled;
        public static ConfigEntry<int> ComboWindowMs;
        public static ConfigEntry<int> ComboMinEnemyLevelDiff;
        public static ConfigEntry<int> ComboPlayerDamageWindowMs;
        public static ConfigEntry<int> StaminaRewardEvery;
        public static ConfigEntry<int> StaminaRewardPercent;
        public static ConfigEntry<int> ComboUIPosXPercent;
        public static ConfigEntry<int> ComboUIPosYPercent;

        public static ConfigEntry<string> ComboTimerBarColor;
        public static ConfigEntry<string> ComboRewardTextColor;
        public static ConfigEntry<string> ComboRewardTextBorderColor;

        public static ConfigEntry<string> ComboColor_D, ComboBorder_D;
        public static ConfigEntry<string> ComboColor_C, ComboBorder_C;
        public static ConfigEntry<string> ComboColor_B, ComboBorder_B;
        public static ConfigEntry<string> ComboColor_A, ComboBorder_A;
        public static ConfigEntry<string> ComboColor_S, ComboBorder_S;
        public static ConfigEntry<string> ComboColor_SS, ComboBorder_SS;
        public static ConfigEntry<string> ComboColor_SSS, ComboBorder_SSS;

        public static ConfigEntry<int> ComboThreshold_C;
        public static ConfigEntry<int> ComboThreshold_B;
        public static ConfigEntry<int> ComboThreshold_A;
        public static ConfigEntry<int> ComboThreshold_S;
        public static ConfigEntry<int> ComboThreshold_SS;
        public static ConfigEntry<int> ComboThreshold_SSS;

        public static ConfigEntry<string> ComboBorderStyle;
        public static ConfigEntry<int> ComboBorderOffset;
        public static ConfigEntry<float> ComboUIScale;
        public static ConfigEntry<int> ComboScaleBorderOffset;

        // ============ 字体样式开关 ============
        public static ConfigEntry<bool> LetterBold;
        public static ConfigEntry<bool> LetterItalic;
        public static ConfigEntry<bool> LetterPulse;
        public static ConfigEntry<bool> NumberBold;
        public static ConfigEntry<bool> NumberItalic;
        public static ConfigEntry<bool> RewardBold;
        public static ConfigEntry<bool> RewardItalic;

        public static ConfigEntry<string> ComboFontName;

        // ============ 元素尺寸与偏移 ============

        // 时间条
        public static ConfigEntry<int> TimerBarWidth;
        public static ConfigEntry<int> TimerBarHeight;

        // 字母
        public static ConfigEntry<int> LetterFontSize;
        public static ConfigEntry<int> LetterOffsetX;
        public static ConfigEntry<int> LetterOffsetY;

        // 数字
        public static ConfigEntry<int> NumberFontSize;
        public static ConfigEntry<int> NumberOffsetX;
        public static ConfigEntry<int> NumberOffsetY;

        // 奖励文字
        public static ConfigEntry<int> RewardFontSize;
        public static ConfigEntry<int> RewardOffsetX;
        public static ConfigEntry<int> RewardOffsetY;

        public static ConfigEntry<string> ComboNumberPrefix;
        public static ConfigEntry<string> ComboHitPrefix;

        // ============ HIT 连击系统 ============
        public static ConfigEntry<bool> HitComboEnabled;
        public static ConfigEntry<int> HitWindowMs;
        public static ConfigEntry<int> HitRewardEvery;
        public static ConfigEntry<int> HitFontSize;
        public static ConfigEntry<int> HitOffsetX;
        public static ConfigEntry<int> HitOffsetY;
        public static ConfigEntry<string> HitTextColor;
        public static ConfigEntry<string> HitBorderColor;
        public static ConfigEntry<bool> HitBold;
        public static ConfigEntry<bool> HitItalic;

        // ============ 挑战模式 ============
        public static ConfigEntry<int> ChallengeHitMode;
        public static ConfigEntry<int> ChallengeHitReduceType;
        public static ConfigEntry<int> ChallengeHitReduceSeconds;
        public static ConfigEntry<int> ChallengeHitReducePercent;

        public static ConfigEntry<bool> ChallengeDefenseRecoverEnabled;
        public static ConfigEntry<int> ChallengeDefenseRecoverType;
        public static ConfigEntry<float> ChallengeDefenseRecoverSeconds;
        public static ConfigEntry<int> ChallengeDefenseRecoverPercent;

        // ============ 暂停信息面板 ============
        public static ConfigEntry<int> PauseInfoDetail;

        // ============ 敌人/友军感叹号 ============
        public static ConfigEntry<bool> AlertEnabled;
        public static ConfigEntry<string> AlertSymbol;
        public static ConfigEntry<int> AlertFontSize;
        public static ConfigEntry<string> AlertTextColor;
        public static ConfigEntry<string> AlertBorderColor;
        public static ConfigEntry<string> AlertFontName;
        public static ConfigEntry<bool> AlertBold;
        public static ConfigEntry<bool> AlertItalic;
        public static ConfigEntry<int> AlertMaxCount;
        public static ConfigEntry<float> AlertPerEnemyCooldownSec;
        public static ConfigEntry<bool> AlertShowEnemy;
        public static ConfigEntry<bool> AlertShowAlly;

        // ============ 夜晚强化敌人 ============
        public static ConfigEntry<bool> NightEnemyEnabled;
        public static ConfigEntry<int> NightChanceS0;
        public static ConfigEntry<int> NightChanceS1;
        public static ConfigEntry<int> NightChanceS2;
        public static ConfigEntry<int> NightChanceS3;
        public static ConfigEntry<int> NightLevelS0;
        public static ConfigEntry<int> NightLevelS1;
        public static ConfigEntry<int> NightLevelS2;
        public static ConfigEntry<int> NightLevelS3;
        public static ConfigEntry<int> NightLevelSkipThreshold;

        // ============ 敌人等级缩放 ============
        public static ConfigEntry<int> EnemyLevelScale_S0;
        public static ConfigEntry<int> EnemyLevelScale_S1;
        public static ConfigEntry<int> EnemyLevelScale_S2;
        public static ConfigEntry<int> EnemyLevelScale_S3;

        // ============ 倒地伤害加成 ============
        public static ConfigEntry<bool> DownedDamageBoostEnabled;
        public static ConfigEntry<int> DownedDamageBoostPercent;

        // ============ 状态效果强化 ============
        public static ConfigEntry<bool> StunCritBonusEnabled;
        public static ConfigEntry<int> StunCritBonusPercent;

        public static ConfigEntry<bool> FrostPierceEnabled;

        public static ConfigEntry<bool> DoTKillEnabled;

        // ---- 燃烧强化 ----
        public static ConfigEntry<bool> BurnDurationMulEnabled;
        public static ConfigEntry<int> BurnDurationMul;

        public static ConfigEntry<bool> BurnSpreadEnabled;
        public static ConfigEntry<float> BurnSpreadRadius;
        public static ConfigEntry<float> BurnSpreadDur;
        public static ConfigEntry<int> BurnSpreadMode;
        public static ConfigEntry<bool> DodgeRollBurnReduceEnabled;
        public static ConfigEntry<int> DodgeRollBurnReduceTicks;

        // ---- 免疫修正 ----
        public static ConfigEntry<bool> ImmunityFixEnabled;
        public static ConfigEntry<int> ElementalPiercePercent;

        public static ConfigEntry<bool> ElementalImmunityFixEnabled;
        public static ConfigEntry<int> ElementalImmunityPercent;
        public static ConfigEntry<int> ElementalImmunityTargetMode;

        // ---- 睡眠加速 ----
        public static ConfigEntry<bool> SleepSpeedControlEnabled;

        // ---- 杂项 ----
        public static ConfigEntry<bool> HideMinimapOnQuit;
        public static ConfigEntry<bool> HideMinimapOnCutscene;

        // ---- 生态 ----
        public static ConfigEntry<bool> CrossSaveShare;
        public static ConfigEntry<bool> EcologyEnabled;
        public static ConfigEntry<bool> EcologyQuestBypass;
        public static ConfigEntry<float> EcologyRecoveryPerHour;
        public static ConfigEntry<float> EcologyDelayPerKillMinutes;
        public static ConfigEntry<float> EcologyKillDropPercent;
        public static ConfigEntry<float> EcologyMinPopulation;
        public static ConfigEntry<float> EcologyPlayerDeathRecovery;

        // ---- 死亡跳过时间 ----
        public static ConfigEntry<float> DeathTimeSkipHours;
        public static ConfigEntry<bool> DeathTimeSkipUIEnabled;
        public static ConfigEntry<string> DeathTimeSkipUITextTemplate;
        public static ConfigEntry<int> DeathTimeSkipUIFontSize;
        public static ConfigEntry<int> DeathTimeSkipUICenterYPercent;

        // ---- 生态 UI ----
        public static ConfigEntry<bool> EcologyUIEnabled;

        // ---- 地图切换时间消耗 ----
        public static ConfigEntry<float> MapExitCostHours;
        public static ConfigEntry<float> TeleporterCostMinutes;


        // ---- 小地图图标 ----
        public static ConfigEntry<bool> MinimapIconEnabled;
        public static ConfigEntry<string> MinimapIconAllyColor;

        // ---- 小地图：资源 ----
        public static ConfigEntry<bool> MinimapResourceEnabled;
        public static ConfigEntry<string> MinimapLogColor;
        public static ConfigEntry<string> MinimapOreColor;
        public static ConfigEntry<string> MinimapPlantColor;

        // ---- 小地图：宝箱 ----
        public static ConfigEntry<bool> MinimapChestEnabled;
        public static ConfigEntry<string> MinimapChestColor;

        // ---- 小地图：技能点 ----
        public static ConfigEntry<bool> MinimapSkillPointEnabled;
        public static ConfigEntry<string> MinimapSkillPointColor;
        public static ConfigEntry<string> MinimapSkillPointLockedColor;

        // ---- 小地图：敌人等级色 ----
        public static ConfigEntry<bool> MinimapEnemyLevelColorEnabled;
        public static ConfigEntry<string> MinimapEnemyLevelLowColor;
        public static ConfigEntry<string> MinimapEnemyLevelNearColor;
        public static ConfigEntry<string> MinimapEnemyLevelHighColor;
        public static ConfigEntry<int> MinimapEnemyLevelLowThreshold;
        public static ConfigEntry<int> MinimapEnemyLevelHighThreshold;

        // ---- 小地图：描边 ----
        public static ConfigEntry<bool> AngryBorderEnabled;
        public static ConfigEntry<string> AngryBorderColor;

        // ============ 环绕方位指示器 ============
        public static ConfigEntry<bool> ThreatRingEnabled;
        public static ConfigEntry<float> ThreatRingRadius;
        public static ConfigEntry<float> ThreatRingMaxDistance;
        public static ConfigEntry<float> ThreatRingTriangleSize;
        public static ConfigEntry<string> ThreatRingLockedColor;
        public static ConfigEntry<string> ThreatRingAttackingColor;
        public static ConfigEntry<bool> ThreatRingShowOnAttack;

        // ---- 小地图：技能点金色 ----
        public static ConfigEntry<string> MinimapSkillPointGoldColor;

        // ---- 小地图：距离渐隐 ----
        public static ConfigEntry<bool> MinimapFadeEnabled;
        public static ConfigEntry<float> MinimapResourceFadeStart;
        public static ConfigEntry<float> MinimapResourceFadeEnd;
        public static ConfigEntry<float> MinimapEnemyFadeStart;
        public static ConfigEntry<float> MinimapEnemyFadeEnd;


        // ---- 快速采集 ----
        public static ConfigEntry<bool> QuickGatherEnabled;
        public static ConfigEntry<float> QuickGatherMinIntervalSec;
        public static ConfigEntry<bool> QuickGatherInterruptAnyKey;
        public static ConfigEntry<bool> QuickGatherInvincibleDuringHarvest;
        public static ConfigEntry<float> QuickGatherExtractDelaySec;

        //自身误伤+友军误伤
        public static ConfigEntry<bool> AoeSelfDamageEnabled;
        public static ConfigEntry<bool> AoeAllyFriendlyFireEnabled;
        public static ConfigEntry<bool> SelfHitBulletsEnabled;
        public static ConfigEntry<float> SelfHitProtectionSec;
        public static ConfigEntry<bool> AllyFriendlyFireUseProtectionSec;
        public static ConfigEntry<int> SelfDamagePlayerPercent;
        public static ConfigEntry<int> SelfDamageEnemyPercent;
        public static ConfigEntry<int> SelfDamageAllyPercent;

        // 自伤分开关
        public static ConfigEntry<bool> SelfDamagePlayerEnabled;
        public static ConfigEntry<bool> SelfDamageEnemyEnabled;
        public static ConfigEntry<bool> SelfDamageAllyEnabled;

        // 友军误伤目标开关
        public static ConfigEntry<bool> AllyFriendlyFireTargetPlayerEnabled;
        public static ConfigEntry<bool> AllyFriendlyFireTargetAllyEnabled;

        // E17 背刺暴击
        public static ConfigEntry<bool> BackStabEnabled;
        public static ConfigEntry<int> BackStabBonusCrit;
        // Flank 绕后暴击
        public static ConfigEntry<bool> FlankCritEnabled;
        public static ConfigEntry<int> FlankRearBonusCrit;
        public static ConfigEntry<int> FlankSideBonusCrit;

        // E16 锥形视野
        public static ConfigEntry<bool> ConeVisionEnabled;
        public static ConfigEntry<int> ConeVisionAngle;
        public static ConfigEntry<bool> ConeVisionBypassWhenAngry;
        public static ConfigEntry<bool> InterruptCallNearbyOnDamage;

        // ============ 愤怒持续时长缩放 ============
        public static ConfigEntry<float> AngryDurationScaleS0;
        public static ConfigEntry<float> AngryDurationScaleS1;
        public static ConfigEntry<float> AngryDurationScaleS2;
        public static ConfigEntry<float> AngryDurationScaleS3;

        // 按难度缩放探测范围
        public static ConfigEntry<float> DetectRangeScaleS0;
        public static ConfigEntry<float> DetectRangeScaleS1;
        public static ConfigEntry<float> DetectRangeScaleS2;
        public static ConfigEntry<float> DetectRangeScaleS3;

        // 近距离完全感知
        public static ConfigEntry<float> ConeVisionCloseRange;

        // 视线遮挡
        public static ConfigEntry<bool> ConeVisionWallBlockEnabled;

        // 发现音效
        public static ConfigEntry<bool> AlertSoundEnabled;

        // 呼叫援军范围按难度缩放
        public static ConfigEntry<float> CallRadiusScaleS0;
        public static ConfigEntry<float> CallRadiusScaleS1;
        public static ConfigEntry<float> CallRadiusScaleS2;
        public static ConfigEntry<float> CallRadiusScaleS3;

        // ============ 噪音警觉 ============
        public static ConfigEntry<bool> NoiseEnabled;
        public static ConfigEntry<int> NoiseHpThresholdPercent;
        public static ConfigEntry<float> NoiseTriggerDelaySec;
        public static ConfigEntry<float> NoiseCallRadius;
        public static ConfigEntry<float> NoiseCallRadiusScaleS0;
        public static ConfigEntry<float> NoiseCallRadiusScaleS1;
        public static ConfigEntry<float> NoiseCallRadiusScaleS2;
        public static ConfigEntry<float> NoiseCallRadiusScaleS3;
        public static ConfigEntry<float> NoiseSharedCooldownSec;
        public static ConfigEntry<int> NoiseMaxNearbyUnits;
        public static ConfigEntry<bool> NoiseShowVisualIndicator;
        public static ConfigEntry<string> NoiseSymbol;

        //使用物品后摇
        public static ConfigEntry<bool> QuickUseItemEnabled;
        public static ConfigEntry<float> QuickUseItemMinIntervalSec;
        public static ConfigEntry<bool> QuickUseItemAnyKey;

        //物品装备栏排序
        public static ConfigEntry<bool> AutoSortOnOpen;
        public static ConfigEntry<int> SortRule;
        public static ConfigEntry<bool> SortReverse;
        public static ConfigEntry<string> CategoryPriority;
        public static ConfigEntry<string> EquipCategoryPriority;
        public static ConfigEntry<string> ItemCategoryOverride;
        public static ConfigEntry<string> EquipCategoryOverride;

        // ============ 冲锋自晕（速度比判定）============
        public static ConfigEntry<bool> ChargeStunEnabled;
        public static ConfigEntry<bool> ChargeStunAffectPlayer;
        public static ConfigEntry<bool> ChargeStunAffectEnemy;
        public static ConfigEntry<bool> ChargeStunAffectAlly;

        public static ConfigEntry<float> ChargeStunCooldownSec;

        public static ConfigEntry<float> ChargeStunDurationBase;
        public static ConfigEntry<float> ChargeStunDurationMax;

        // ============ 冲锋自晕：2A + 2B ============
        public static ConfigEntry<bool> ChargeEnemyRandomEffectEnabled;
        public static ConfigEntry<bool> ChargeDamageBoostEnabled;
        public static ConfigEntry<float> ChargeDamageBoostMinDistance;
        public static ConfigEntry<float> ChargeDamageBoostMaxDistance;
        public static ConfigEntry<float> ChargeDamageBoostMaxMul;

        // ============ 格挡反击链 ============
        public static ConfigEntry<bool> GuardCounterEnabled;
        public static ConfigEntry<float> GuardCounterWindowSec;
        public static ConfigEntry<float> GuardCounterMaxDistance;
        public static ConfigEntry<float> GuardCounterCooldownSec;
        public static ConfigEntry<float> GuardCounterDuration;

        // ============ 格挡可打断 ============
        public static ConfigEntry<bool> GuardInterruptEnabled;
        public static ConfigEntry<bool> GuardInterruptDodgeEnabled;
        // ============ 格挡音效 ============
        public static ConfigEntry<bool> GuardSoundEnabled;

        // ---- 调试 ----
        public static ConfigEntry<bool> DebugLogEnabled;

        //测试暴击
        public static ConfigEntry<bool> TestCritBonusEnabled;
        public static ConfigEntry<int> TestCritBonus;

        //测试经验倍率
        public static ConfigEntry<bool> TestExpMulEnabled;
        public static ConfigEntry<int> TestExpMul;
        public static ConfigEntry<bool> ExpShowMultiplierBracket;

        // 测试金钱热键
        public static ConfigEntry<bool> TestMoneyEnabled;
        public static ConfigEntry<string> TestMoneyKey;
        public static ConfigEntry<int> TestMoneyAmount;

        // ---- 永久死亡 ----
        public static ConfigEntry<bool> PermadeathShowInCharacterCreation;
        public static ConfigEntry<string> PermadeathDeathText;
        public static ConfigEntry<int> PermadeathDeathTextFontSize;
        public static ConfigEntry<string> PermadeathDeathTextColor;
        public static ConfigEntry<string> PermadeathDeathTextBorderColor;
        public static ConfigEntry<float> PermadeathCountdownSec;
        public static ConfigEntry<string> PermadeathButtonSymbol;
        public static ConfigEntry<string> PermadeathButtonOffBgColor;
        public static ConfigEntry<string> PermadeathButtonOffTextColor;
        public static ConfigEntry<string> PermadeathButtonOnBgColor;
        public static ConfigEntry<string> PermadeathButtonOnTextColor;

        public static ConfigEntry<bool> ChargeStunDebugLog;

        // ---- 对话 Ctrl 快速跳过 ----
        public static ConfigEntry<bool> DialogueCtrlSkipEnabled;
        public static ConfigEntry<float> DialogueCtrlSkipIntervalSec;
        // ---- 对话 Ctrl 快进提示 ----
        public static ConfigEntry<bool> DialogueCtrlHintEnabled;
        public static ConfigEntry<string> DialogueCtrlHintText;
        public static ConfigEntry<string> DialogueCtrlHintColor;
        public static ConfigEntry<string> DialogueCtrlHintBorderColor;

        // ---- 物品栏 / 装备栏容量扩展 ----
        public static ConfigEntry<bool> InventoryExpandEnabled;

        // ---- 数量弹窗快捷确认 ----
        public static ConfigEntry<bool> ConfirmHotkeyEnabled;
        // ---- 数量弹窗移动键调数量 ----
        public static ConfigEntry<bool> QuantityHotkeyEnabled;

        // ---- 玩家光源 ----
        public static ConfigEntry<bool> PlayerLightEnabled;
        public static ConfigEntry<string> PlayerLightColor;
        public static ConfigEntry<float> PlayerLightRange;

        /// <summary>
        /// 输出调试日志。受 DebugLogEnabled 开关控制。
        /// 用法：Plugin.DebugLog("[ED3R] xxx");
        /// </summary>
        public static void DebugLog(string msg)
        {
            if (DebugLogEnabled != null && DebugLogEnabled.Value && Log != null)
                Log.LogInfo(msg);
        }

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            // ========== 本地化初始化（必须在所有 Config.Bind 之前） ==========
            Loc.Init();

            // ============================================================
            // 01.Player
            // ============================================================

            // ---- 01.Player.01_Movement ----
            SprintCostPercent = Config.Bind("01.Player.01_Movement", "SprintCostPercent", 25,
                Loc.CfgDesc("01.Player.01_Movement", "SprintCostPercent", "跑步体力消耗百分比。100=原版，50=消耗减半，0=不消耗。建议 25"));

            SprintAnyDirection = Config.Bind("01.Player.01_Movement", "SprintAnyDirection", true,
                Loc.CfgDesc("01.Player.01_Movement", "SprintAnyDirection", "是否允许任意方向冲刺。true=任意方向按住 sprint 键都能冲刺，false=原版(仅正面可冲刺)"));

            WalkSpeedMul = Config.Bind("01.Player.01_Movement", "WalkSpeedMul", 150,
                Loc.CfgDesc("01.Player.01_Movement", "WalkSpeedMul", "走路速度倍率。100 = 原版(6)，150 = x1.5(9)"));

            SprintSpeedMul = Config.Bind("01.Player.01_Movement", "SprintSpeedMul", 110,
                Loc.CfgDesc("01.Player.01_Movement", "SprintSpeedMul",
                    "冲刺速度倍率。100 = 原版(12)，110 = x1.1(13.2)，150 = x1.5(18)。"
                    + "建议 ≥ WalkSpeedMul，否则冲刺会显得比走路还慢，与走路速度是独立的速度"));

            DodgeSpeedPercent = Config.Bind("01.Player.01_Movement", "DodgeSpeedPercent", 100,
                Loc.CfgDesc("01.Player.01_Movement", "DodgeSpeedPercent",
                    "翻滚速度倍率。翻滚速度 = 当前移动速度 × 此倍率。\n"
                    + "【注意】翻滚速度 = 走路速度或冲刺速度 × 此值，"
                    + "取决于翻滚瞬间是否按住 冲刺 键。\n"
                    + "\n"
                    + "  0    = 不修改（使用原版固定速度 8）\n"
                    + "  50   = 翻滚速度 = 当前速度 × 0.5\n"
                    + "  100  = 翻滚速度 = 当前速度（默认，推荐）\n"
                    + "  150  = 翻滚速度 = 当前速度 × 1.5\n"));

            DodgeRollTapMode = Config.Bind("01.Player.01_Movement", "DodgeRollTapMode", 1,
                Loc.CfgDesc("01.Player.01_Movement", "DodgeRollTapMode",
                    "双击方向键的行为。\n"
                    + "  0 = 原版（如果开了双击翻滚=触发翻滚闪避，没开则双击无反应）\n"
                    + "  1 = 触发锁定冲刺（双击任意方向键进入持续冲刺，停下/体力不足-退出）\n"
                    + "按住 冲刺 键的冲刺不受影响，始终走原版逻辑"));

            DodgeJumpInertiaEnabled = Config.Bind("01.Player.01_Movement", "DodgeJumpInertiaEnabled", true,
                Loc.CfgDesc("01.Player.01_Movement", "DodgeJumpInertiaEnabled", "跳斩类技能是否继承移动惯性。true=开启，false=原版（原地跳斩）"));

            DodgeJumpInertiaMul = Config.Bind("01.Player.01_Movement", "DodgeJumpInertiaMul", 100,
                Loc.CfgDesc("01.Player.01_Movement", "DodgeJumpInertiaMul",
                    "惯性倍率。100 = 完整继承当前移动速度（走路或冲刺速度），"
                    + "50 = 继承一半，0 = 无惯性（等同于关闭）"));

            // ---- 01.Player.02_Invincible ----
            DodgeInvincibleMs = Config.Bind("01.Player.02_Invincible", "DodgeInvincibleMs", 300,
                Loc.CfgDesc("01.Player.02_Invincible", "DodgeInvincibleMs", "闪避无敌时长（毫秒）。300 = 0.3 秒。建议 100~800"));

            HurtInvincibleMs = Config.Bind("01.Player.02_Invincible", "HurtInvincibleMs", 500,
                Loc.CfgDesc("01.Player.02_Invincible", "HurtInvincibleMs", "受击无敌时长（毫秒）。500 = 0.5 秒。建议 200~1500"));

            HurtInvincibleCooldownMs = Config.Bind("01.Player.02_Invincible", "HurtInvincibleCooldownMs", 1000,
                Loc.CfgDesc("01.Player.02_Invincible", "HurtInvincibleCooldownMs", "受击无敌冷却（毫秒）。1000 = 1 秒。表示两次受击无敌之间至少要间隔这么久。建议 0~3000"));

            BlinkEnabled = Config.Bind("01.Player.02_Invincible", "BlinkEnabled", true,
                Loc.CfgDesc("01.Player.02_Invincible", "BlinkEnabled", "无敌时是否让角色模型闪烁。true=闪烁，false=不闪烁"));

            // ---- 01.Player.03_Interaction ----
            QuickGatherEnabled = Config.Bind("01.Player.03_Interaction", "QuickGatherEnabled", true,
                Loc.CfgDesc("01.Player.03_Interaction", "QuickGatherEnabled",
                    "采集（蘑菇、野果、木头、矿石）时，任何操作键可以立刻打断动画并获得物品。\n"
                    + "连按互动键可以快速连续采集。"));

            QuickGatherMinIntervalSec = Config.Bind("01.Player.03_Interaction", "QuickGatherMinIntervalSec", 0.5f,
                Loc.CfgDesc("01.Player.03_Interaction", "QuickGatherMinIntervalSec",
                    "两次采集之间最短间隔（秒）。防止连点刷物品。\n"
                    + "0.5 = 一秒最多约 2 次采集。0 = 无冷却（可能刷物品）。"));

            QuickGatherInterruptAnyKey = Config.Bind("01.Player.03_Interaction", "QuickGatherInterruptAnyKey", false,
                Loc.CfgDesc("01.Player.03_Interaction", "QuickGatherInterruptAnyKey",
                    "是否允许任意键打断采集。\n"
                    + "false = 只对移动/攻击/跳跃/格挡/互动等主要键响应；\n"
                    + "true = 任意键（包括鼠标）都打断。"));

            QuickGatherInvincibleDuringHarvest = Config.Bind("01.Player.03_Interaction", "QuickGatherInvincibleDuringHarvest", false,
                Loc.CfgDesc("01.Player.03_Interaction", "QuickGatherInvincibleDuringHarvest",
                    "采集期间是否无敌。\n"
                    + "false = 采集时可被打断（默认，更真实）；\n"
                    + "true = 保持原版行为，采集期间免疫伤害。"));

            QuickGatherExtractDelaySec = Config.Bind("01.Player.03_Interaction", "QuickGatherExtractDelaySec", 0.6f,
                Loc.CfgDesc("01.Player.03_Interaction", "QuickGatherExtractDelaySec",
                    "按 E 后多久出现物品（秒），推荐比 MinIntervalSec 稍大。\n"
                    + "1 = 按 E 后 0.6 秒物品出现，与动画节奏更自然；\n"
                    + "0   = 按 E 立刻出物品（较突兀）。\n"
                    + "此配置独立于 MinIntervalSec。"));

            QuickUseItemEnabled = Config.Bind("01.Player.03_Interaction", "QuickUseItemEnabled", true,
                Loc.CfgDesc("01.Player.03_Interaction", "QuickUseItemEnabled",
                    "使用恢复类物品（血瓶、蓝瓶等）时，治疗效果立即生效，"
                    + "动画期间可以被操作打断。"));

            QuickUseItemMinIntervalSec = Config.Bind("01.Player.03_Interaction", "QuickUseItemMinIntervalSec", 0.5f,
                Loc.CfgDesc("01.Player.03_Interaction", "QuickUseItemMinIntervalSec", "两次使用物品之间最短间隔（秒）。防止连点刷药。"));

            QuickUseItemAnyKey = Config.Bind("01.Player.03_Interaction", "QuickUseItemAnyKey", true,
                Loc.CfgDesc("01.Player.03_Interaction", "QuickUseItemAnyKey",
                    "是否允许任意键打断。\n"
                    + "false = 只对移动/攻击/跳跃/格挡响应；true = 任意键。"));

            PickupRangeEnabled = Config.Bind("01.Player.03_Interaction", "PickupRangeEnabled", true,
                Loc.CfgDesc("01.Player.03_Interaction", "PickupRangeEnabled", "是否扩大地面掉落物的拾取范围"));

            PickupRangePercent = Config.Bind("01.Player.03_Interaction", "PickupRangePercent", 300,
                Loc.CfgDesc("01.Player.03_Interaction", "PickupRangePercent", "拾取范围百分比。100=原版，300=范围扩大 300%"));

            // ---- 01.Player.04_Inventory ----
            InventoryExpandEnabled = Config.Bind("01.Player.04_Inventory", "InventoryExpandEnabled", true,
                Loc.CfgDesc("01.Player.04_Inventory", "InventoryExpandEnabled",
                    "物品栏 / 装备栏动态扩容。\n"
                    + "捡到装不下的物品/装备时，自动扩展一页（物品 +14，装备 +8）。\n"
                    + "关闭后保持原版容量。"));

            AutoSortOnOpen = Config.Bind("01.Player.04_Inventory", "AutoSortOnOpen", true,
                Loc.CfgDesc("01.Player.04_Inventory", "AutoSortOnOpen", "打开物品栏/装备栏时自动排序。"));

            SortRule = Config.Bind("01.Player.04_Inventory", "SortRule", 1,
                Loc.CfgDesc("01.Player.04_Inventory", "SortRule",
                    "排序规则。\n"
                    + "0 = 按物品 ID 升序\n"
                    + "1 = 按类别（用 CategoryPriority 控制顺序）\n"
                    + "2 = 按名称（英文）\n"
                    + "3 = 按数量降序（物品栏）/ 强化等级降序（装备栏）"));

            SortReverse = Config.Bind("01.Player.04_Inventory", "SortReverse", false,
                Loc.CfgDesc("01.Player.04_Inventory", "SortReverse", "是否反转排序结果。"));

            CategoryPriority = Config.Bind("01.Player.04_Inventory", "CategoryPriority", "0,1,2,3,4,5",
                Loc.CfgDesc("01.Player.04_Inventory", "CategoryPriority",
                    "物品类别优先级。从左到右优先级递减。\n"
                    + "0 = 恢复（hp/mp 恢复）\n"
                    + "1 = 添加 Buff\n"
                    + "2 = 解 Buff（解毒/解晕等）\n"
                    + "3 = 攻击道具\n"
                    + "4 = 未识别（可用但无法判定用途）\n"
                    + "5 = 材料（unusable 不可用）"));

            EquipCategoryPriority = Config.Bind("01.Player.04_Inventory", "EquipCategoryPriority", "0,1,2,3,4,5",
                Loc.CfgDesc("01.Player.04_Inventory", "EquipCategoryPriority",
                    "装备类别优先级。\n"
                    + "0 = 武器, 1 = 护甲, 2 = 头饰, 3 = 手套, 4 = 靴子, 5 = 饰品"));

            ItemCategoryOverride = Config.Bind("01.Player.04_Inventory", "ItemCategoryOverride", "",
                Loc.CfgDesc("01.Player.04_Inventory", "ItemCategoryOverride",
                    "手动指定物品类别（兜底）。格式：itemId:categoryId，多个用逗号分隔。\n"
                    + "示例：44:2,60:3  →  物品 44 归到类别 2（解 Buff），物品 60 归到类别 3（攻击）\n"
                    + "留空则全部自动判定。"));

            EquipCategoryOverride = Config.Bind("01.Player.04_Inventory", "EquipCategoryOverride", "",
                Loc.CfgDesc("01.Player.04_Inventory", "EquipCategoryOverride", "手动指定装备类别（兜底）。格式同 ItemCategoryOverride。"));

            // ---- 01.Player.05_Light ----
            PlayerLightEnabled = Config.Bind("01.Player.05_Light", "PlayerLightEnabled", true,
                Loc.CfgDesc("01.Player.05_Light", "PlayerLightEnabled",
                    "玩家自带光源开关。\n"
                    + "常开，不检测场景亮度。默认强度较低，避免在有其他光源时显得突兀。"));

            PlayerLightColor = Config.Bind("01.Player.05_Light", "PlayerLightColor", "200,200,200,150",
                Loc.CfgDesc("01.Player.05_Light", "PlayerLightColor",
                    "光源颜色 R,G,B,A。\n"
                    + "  R,G,B = 颜色（0~255）\n"
                    + "  A     = 强度（0~255），映射到 intensity 0.0~1.0"));

            PlayerLightRange = Config.Bind("01.Player.05_Light", "PlayerLightRange", 15f,
                Loc.CfgDesc("01.Player.05_Light", "PlayerLightRange", "照明半径（米）。"));

            // ---- 01.Player.06_Sleep ----
            SleepSpeedControlEnabled = Config.Bind("01.Player.06_Sleep", "SleepSpeedControlEnabled", true,
                Loc.CfgDesc("01.Player.06_Sleep", "SleepSpeedControlEnabled",
                    "睡眠时按键切换时间加速倍率。\n"
                    + "  A 键：变慢\n"
                    + "  D 键：变快\n"
                    + "档位：1x（暂停加速）/ 20x（原版）/ 60x\n"
                    + "每次入睡重置为 20x。"));

            // ============================================================
            // 02.Combat
            // ============================================================

            // ---- 02.Combat.01_Damage ----
            DownedDamageBoostEnabled = Config.Bind("02.Combat.01_Damage", "DownedDamageBoostEnabled", true,
                Loc.CfgDesc("02.Combat.01_Damage", "DownedDamageBoostEnabled",
                    "倒地状态下的目标受到的伤害按百分比放大。\n"
                    + "对所有单位生效（玩家倒地也受此加成）。\n"
                    + "让倒地从'鸡肋控制'变成'战术窗口'。"));

            DownedDamageBoostPercent = Config.Bind("02.Combat.01_Damage", "DownedDamageBoostPercent", 100,
                Loc.CfgDesc("02.Combat.01_Damage", "DownedDamageBoostPercent",
                    "倒地时受到的伤害加成百分比。\n"
                    + "  100 = 伤害翻倍（默认）\n"
                    + "  50  = 伤害 ×1.5\n"
                    + "  0   = 不加成"));

            BackStabEnabled = Config.Bind("02.Combat.01_Damage", "BackStabEnabled", true,
                Loc.CfgDesc("02.Combat.01_Damage", "BackStabEnabled",
                    "背刺暴击开关。敌人未进入战斗状态（followTarget=null 且不 angry）时，"
                    + "攻击者的暴击率 +BackStabBonusCrit。对所有攻击者生效。"));

            BackStabBonusCrit = Config.Bind("02.Combat.01_Damage", "BackStabBonusCrit", 100,
                Loc.CfgDesc("02.Combat.01_Damage", "BackStabBonusCrit", "背刺暴击率加成。100 = +100% 暴击率。"));

            FlankCritEnabled = Config.Bind("02.Combat.01_Damage", "FlankCritEnabled", true,
                Loc.CfgDesc("02.Combat.01_Damage", "FlankCritEnabled",
                    "绕后暴击开关。目标已进入战斗状态时，根据攻击方向给予暴击率加成。\n"
                    + "与背刺互斥（背刺触发时不叠加）。"));

            FlankRearBonusCrit = Config.Bind("02.Combat.01_Damage", "FlankRearBonusCrit", 25,
                Loc.CfgDesc("02.Combat.01_Damage", "FlankRearBonusCrit", "背面暴击率加成（目标背后攻击时）。默认 25 = +25%。"));

            FlankSideBonusCrit = Config.Bind("02.Combat.01_Damage", "FlankSideBonusCrit", 10,
                Loc.CfgDesc("02.Combat.01_Damage", "FlankSideBonusCrit", "侧面暴击率加成（目标左右两侧攻击时）。默认 10 = +10%。"));

            // ---- 02.Combat.02_Status ----
            StunCritBonusEnabled = Config.Bind("02.Combat.02_Status", "StunCritBonusEnabled", true,
                Loc.CfgDesc("02.Combat.02_Status", "StunCritBonusEnabled", "眩晕中的目标受到攻击时，攻击者本次暴击率 +N%。对所有攻击者、所有目标生效。"));

            StunCritBonusPercent = Config.Bind("02.Combat.02_Status", "StunCritBonusPercent", 100,
                Loc.CfgDesc("02.Combat.02_Status", "StunCritBonusPercent", "眩晕暴击率加成数值。默认 100（+100% 暴击率）。"));

            FrostPierceEnabled = Config.Bind("02.Combat.02_Status", "FrostPierceEnabled", true,
                Loc.CfgDesc("02.Combat.02_Status", "FrostPierceEnabled",
                    "冰冻中的目标受到攻击时，无视防御、抗性，且无法格挡/闪避。"
                    + "对所有目标生效（含玩家）。"));

            DoTKillEnabled = Config.Bind("02.Combat.02_Status", "DoTKillEnabled", true,
                Loc.CfgDesc("02.Combat.02_Status", "DoTKillEnabled",
                    "中毒/燃烧可以致死。\n"
                    + "true = 毒烧可以导致死亡；\n"
                    + "false = 保持原版（钳制到 1 血）。"));

            BurnDurationMulEnabled = Config.Bind("02.Combat.02_Status", "BurnDurationMulEnabled", true,
                Loc.CfgDesc("02.Combat.02_Status", "BurnDurationMulEnabled", "燃烧时长倍率开关。"));

            BurnDurationMul = Config.Bind("02.Combat.02_Status", "BurnDurationMul", 3,
                Loc.CfgDesc("02.Combat.02_Status", "BurnDurationMul", "燃烧时长倍率。3 = 原版时长的 3 倍（含蔓延传染的燃烧）。"));

            BurnSpreadEnabled = Config.Bind("02.Combat.02_Status", "BurnSpreadEnabled", true,
                Loc.CfgDesc("02.Combat.02_Status", "BurnSpreadEnabled", "燃烧是否蔓延到附近单位。首次立即传播一次，之后每秒轮询。"));

            BurnSpreadRadius = Config.Bind("02.Combat.02_Status", "BurnSpreadRadius", 2.0f,
                Loc.CfgDesc("02.Combat.02_Status", "BurnSpreadRadius", "蔓延半径（米）。1.0 = 原版贴身，3.0 = 有明显传播距离。"));

            BurnSpreadDur = Config.Bind("02.Combat.02_Status", "BurnSpreadDur", 1.0f,
                Loc.CfgDesc("02.Combat.02_Status", "BurnSpreadDur",
                    "蔓延传染时施加的燃烧强度。会再被 BurnDurationMul 放大。"
                    + "1.0 = 较弱（约 1.5 秒），2.0 = 与常见点燃强度一致。"));

            BurnSpreadMode = Config.Bind("02.Combat.02_Status", "BurnSpreadMode", 1,
                Loc.CfgDesc("02.Combat.02_Status", "BurnSpreadMode",
                    "蔓延允许的单位类型。0 = 仅敌人；1 = 敌人 + 玩家（默认）；2 = 全部单位（包括召唤物/NPC）。"
                    + "模式 1/2 下玩家会被敌人连锁点燃，但可以靠翻滚灭火。"));

            DodgeRollBurnReduceEnabled = Config.Bind("02.Combat.02_Status", "DodgeRollBurnReduceEnabled", true,
                Loc.CfgDesc("02.Combat.02_Status", "DodgeRollBurnReduceEnabled", "翻滚时减少更多燃烧层数。"));

            DodgeRollBurnReduceTicks = Config.Bind("02.Combat.02_Status", "DodgeRollBurnReduceTicks", 3,
                Loc.CfgDesc("02.Combat.02_Status", "DodgeRollBurnReduceTicks",
                    "翻滚一次减少多少层燃烧。\n"
                    + "原版每层 = 1.5 秒，3 层 = 4.5 秒。\n"
                    + "3 = 一次翻滚减少约 3 秒的时长（推荐）。"));

            ImmunityFixEnabled = Config.Bind("02.Combat.02_Status", "ImmunityFixEnabled", true,
                Loc.CfgDesc("02.Combat.02_Status", "ImmunityFixEnabled",
                    "属性攻击（带元素属性的物理/魔法武器）打物理/魔法免疫怪时，"
                    + "元素伤害不再被完全吃掉（原版只吃 10%）。"));

            ElementalPiercePercent = Config.Bind("02.Combat.02_Status", "ElementalPiercePercent", 50,
                Loc.CfgDesc("02.Combat.02_Status", "ElementalPiercePercent",
                    "属性攻击穿透免疫时保留的伤害百分比。50 = 保留 50% 伤害。"
                    + "作用示例：火焰技能打物理免疫怪 → 从 10% 伤害提升到 50%。"));

            ElementalImmunityFixEnabled = Config.Bind("02.Combat.02_Status", "ElementalImmunityFixEnabled", true,
                Loc.CfgDesc("02.Combat.02_Status", "ElementalImmunityFixEnabled",
                    "元素免疫修正。某些种族对特定元素免疫，"
                    + "原版效果是该元素攻击只能造成 1 点伤害。开启后改为造成指定百分比伤害。"));

            ElementalImmunityPercent = Config.Bind("02.Combat.02_Status", "ElementalImmunityPercent", 10,
                Loc.CfgDesc("02.Combat.02_Status", "ElementalImmunityPercent",
                    "元素抗性修正后的保底百分比。\n"
                    + "触发后，该元素抗性会被提升到至少这个值，同时清空本次攻击的防御。\n"
                    + "例：imp 火抗 10，火球打上去原本被魔防吃成 1 点，现在变成 10% 伤害。\n"
                    + "10 = 保底 10% 伤害；20 = 保底 20% 伤害。"));

            ElementalImmunityTargetMode = Config.Bind("02.Combat.02_Status", "ElementalImmunityTargetMode", 0,
                Loc.CfgDesc("02.Combat.02_Status", "ElementalImmunityTargetMode",
                    "元素免疫修正的生效目标。0 = 所有目标（默认）；"
                    + "1 = 仅敌人（排除玩家/同伴/NPC）；2 = 敌人 + 玩家（排除同伴/NPC）。"));

            // ---- 02.Combat.03_ChargeStun ----
            ChargeStunEnabled = Config.Bind("02.Combat.03_ChargeStun", "ChargeStunEnabled", true,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeStunEnabled",
                    "冲锋期间被强制停下（撞墙 / 被对方阻挡）会眩晕。\n"
                    + "判定方式：当前速度 / 冲锋最高速度 低于阈值，持续 N 帧触发。\n"
                    + "玩家的眩晕抗性越高，自己冲锋被撞晕的概率越低（如果AffectPlayer打开的话）。\n"
                    + "冲锋正常结束不会触发（状态字段翻转为 false 后豁免）。"));

            ChargeStunAffectPlayer = Config.Bind("02.Combat.03_ChargeStun", "ChargeStunAffectPlayer", true,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeStunAffectPlayer", "玩家冲锋被阻止时是否自晕。"));

            ChargeStunAffectEnemy = Config.Bind("02.Combat.03_ChargeStun", "ChargeStunAffectEnemy", true,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeStunAffectEnemy",
                    "敌人冲锋被阻止时是否自晕。\n"
                    + "tag 不是 Player / Ally 的单位都按此项处理。"));

            ChargeStunAffectAlly = Config.Bind("02.Combat.03_ChargeStun", "ChargeStunAffectAlly", true,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeStunAffectAlly", "友军 / 召唤物冲锋被阻止时是否自晕。"));

            ChargeStunCooldownSec = Config.Bind("02.Combat.03_ChargeStun", "ChargeStunCooldownSec", 2.0f,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeStunCooldownSec", "两次触发之间的最小间隔（秒）。"));

            ChargeStunDurationBase = Config.Bind("02.Combat.03_ChargeStun", "ChargeStunDurationBase", 0.2f,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeStunDurationBase", "基础眩晕时长（秒）。"));

            ChargeStunDurationMax = Config.Bind("02.Combat.03_ChargeStun", "ChargeStunDurationMax", 1.0f,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeStunDurationMax", "眩晕时长上限（秒）。最大惩罚。"));

            ChargeEnemyRandomEffectEnabled = Config.Bind("02.Combat.03_ChargeStun", "ChargeEnemyRandomEffectEnabled", true,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeEnemyRandomEffectEnabled",
                    "玩家冲锋自晕时，本次冲锋命中过的敌人也被随机施加一种效果。\n"
                    + "效果池（等概率）：眩晕 / 击倒。\n"
                    + "失败 fallback：[击倒 → 眩晕]。\n"
                    + "效果时长与玩家自晕时长相同。\n"
                    + "精英/Boss 因 stability 或 stunResist 可能全部免疫 → 允许轮空。\n"
                    + "依附玩家自晕事件：ChargeStunAffectPlayer = false 时此项也不触发。"));

            ChargeDamageBoostEnabled = Config.Bind("02.Combat.03_ChargeStun", "ChargeDamageBoostEnabled", true,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeDamageBoostEnabled",
                    "玩家冲锋伤害按冲锋距离缩放。\n"
                    + "距离 = 冲锋起点 → 命中点的水平距离（忽略 Y）。"));

            ChargeDamageBoostMinDistance = Config.Bind("02.Combat.03_ChargeStun", "ChargeDamageBoostMinDistance", 2f,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeDamageBoostMinDistance",
                    "冲锋距离小于此值时不加成（倍率 1.0）。\n"
                    + "避免贴脸冲锋也吃满倍率。"));

            ChargeDamageBoostMaxDistance = Config.Bind("02.Combat.03_ChargeStun", "ChargeDamageBoostMaxDistance", 12f,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeDamageBoostMaxDistance", "冲锋距离大于此值时吃满倍率。中间线性插值。"));

            ChargeDamageBoostMaxMul = Config.Bind("02.Combat.03_ChargeStun", "ChargeDamageBoostMaxMul", 1.5f,
                Loc.CfgDesc("02.Combat.03_ChargeStun", "ChargeDamageBoostMaxMul",
                    "冲锋距离达到上限时的伤害倍率。\n"
                    + "1.5 = 1.5 倍伤害。\n"
                    + "【平衡】与 mod 现有系统（多重暴击等）叠加时注意总倍率。"));

            // ---- 02.Combat.04_Guard ----
            GuardCounterEnabled = Config.Bind("02.Combat.04_Guard", "GuardCounterEnabled", true,
                Loc.CfgDesc("02.Combat.04_Guard", "GuardCounterEnabled",
                    "玩家格挡成功后短时间内攻击近处敌人，触发反击效果。\n"
                    + "效果：随机施加 眩晕 / 击倒（失败走 fallback）。\n"
                    + "消耗时间戳：一次格挡最多触发一次反击，不限敌人数量。"));

            GuardCounterWindowSec = Config.Bind("02.Combat.04_Guard", "GuardCounterWindowSec", 0.7f,
                Loc.CfgDesc("02.Combat.04_Guard", "GuardCounterWindowSec",
                    "格挡成功后多久内的攻击算反击（秒）。\n"
                    + "0.5 = 需要快速反应；1.0 = 宽松。"));

            GuardCounterMaxDistance = Config.Bind("02.Combat.04_Guard", "GuardCounterMaxDistance", 3f,
                Loc.CfgDesc("02.Combat.04_Guard", "GuardCounterMaxDistance",
                    "反击生效的距离上限（米）。\n"
                    + "玩家与命中敌人的水平距离。忽略 Y 轴，不限攻击类型。"));

            GuardCounterCooldownSec = Config.Bind("02.Combat.04_Guard", "GuardCounterCooldownSec", 1.0f,
                Loc.CfgDesc("02.Combat.04_Guard", "GuardCounterCooldownSec", "两次反击之间的最小间隔（秒）。防止连续触发。"));

            GuardCounterDuration = Config.Bind("02.Combat.04_Guard", "GuardCounterDuration", 1.0f,
                Loc.CfgDesc("02.Combat.04_Guard", "GuardCounterDuration", "反击效果（眩晕/击倒）的持续时间（秒）。"));

            GuardInterruptEnabled = Config.Bind("02.Combat.04_Guard", "GuardInterruptEnabled", true,
                Loc.CfgDesc("02.Combat.04_Guard", "GuardInterruptEnabled",
                    "格挡期间按攻击/技能键可以立即打断格挡动画并执行动作。\n"
                    + "默认打开：配合格挡反击链使用。\n"
                    + "关闭则保持原版（格挡后摇 0.75 秒内不能行动）。\n"
                    + "注意：格挡中跳跃键、方向键都不会打断格挡（原地不动）。"));

            GuardInterruptDodgeEnabled = Config.Bind("02.Combat.04_Guard", "GuardInterruptDodgeEnabled", true,
                Loc.CfgDesc("02.Combat.04_Guard", "GuardInterruptDodgeEnabled",
                    "格挡中 [按住]方向+[再次按下]右键 可以打断格挡并直接闪避。\n"
                    + "  严格触发：需要松开右键再按下（与原版闪避操作一致）。\n"
                    + "  体力不足时不会打断格挡（保持格挡状态，避免\"格挡被打断且闪避失败\"）。\n"
                    + "关闭则只能用攻击/技能键打断格挡。"));

            GuardSoundEnabled = Config.Bind("02.Combat.04_Guard", "GuardSoundEnabled", true,
                Loc.CfgDesc("02.Combat.04_Guard", "GuardSoundEnabled", "进行格挡时播放[武器攻击]的音效。"));

            // ---- 02.Combat.05_FriendlyFire ----
            EnemyFriendlyFire = Config.Bind("02.Combat.05_FriendlyFire", "EnemyFriendlyFire", true,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "EnemyFriendlyFire", "敌人是否会被敌人的攻击误伤。true=开启，false=原版"));

            EnemyAoeHitsEnemy = Config.Bind("02.Combat.05_FriendlyFire", "EnemyAoeHitsEnemy", true,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "EnemyAoeHitsEnemy",
                    "敌人的持续 AOE（毒雾、闪电柱等）能伤害其他敌人。\n"
                    + "true = 敌人 AOE 无差别攻击（敌人也伤敌人）；\n"
                    + "false = 保持原版（敌人 AOE 只伤玩家方）。"));

            AoeSelfDamageEnabled = Config.Bind("02.Combat.05_FriendlyFire", "AoeSelfDamageEnabled", true,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "AoeSelfDamageEnabled",
                    "AOE（爆炸/持续伤害区/光环）能伤害发射者自己。\n"
                    + "玩家 AOE 炸自己；敌人 AOE 炸自己。"));

            AoeAllyFriendlyFireEnabled = Config.Bind("02.Combat.05_FriendlyFire", "AoeAllyFriendlyFireEnabled", false,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "AoeAllyFriendlyFireEnabled", "友军/召唤物的 AOE 能伤害自己阵营（包括对玩家可造成伤害）。默认关闭。"));

            SelfHitBulletsEnabled = Config.Bind("02.Combat.05_FriendlyFire", "SelfHitBulletsEnabled", true,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "SelfHitBulletsEnabled",
                    "普通攻击（近战/远程/子弹）也能伤害发射者自己。\n"
                    + "true = 所有攻击都能自伤；\n"
                    + "false = 只有 AOE（爆炸/区域/光环）能自伤。"));

            SelfHitProtectionSec = Config.Bind("02.Combat.05_FriendlyFire", "SelfHitProtectionSec", 1f,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "SelfHitProtectionSec",
                    "出膛保护期（秒）。子弹生成后的这段时间内不会伤害发射者，\n"
                    + "避免子弹生成瞬间命中自己（范围爆炸不算）。\n"
                    + "1 = 生成后 1 秒内不打自己（推荐）。"));

            AllyFriendlyFireUseProtectionSec = Config.Bind("02.Combat.05_FriendlyFire", "AllyFriendlyFireUseProtectionSec", false,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "AllyFriendlyFireUseProtectionSec",
                    "友军误伤是否使用出膛保护期。\n"
                    + "true = 复用 SelfHitProtectionSec（子弹/爆炸生成后一段时间内不伤害友军/玩家）；\n"
                    + "false = 无出膛保护期，友军会立刻误伤队友（但不会误伤自己）。"));

            SelfDamagePlayerPercent = Config.Bind("02.Combat.05_FriendlyFire", "SelfDamagePlayerPercent", 10,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "SelfDamagePlayerPercent",
                    "玩家自伤百分比。10 = 玩家自伤只受 10% 伤害。\n"
                    + "100 = 保持满伤。0 = 视为 1 点保底伤害。"));

            SelfDamageEnemyPercent = Config.Bind("02.Combat.05_FriendlyFire", "SelfDamageEnemyPercent", 100,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "SelfDamageEnemyPercent",
                    "敌人自伤百分比。100 = 保持原版（敌人自伤满伤）。\n"
                    + "若想敌人自伤也打折，改小此值。"));

            SelfDamageAllyPercent = Config.Bind("02.Combat.05_FriendlyFire", "SelfDamageAllyPercent", 10,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "SelfDamageAllyPercent", "友军/召唤物自伤百分比。默认 10。"));

            SelfDamagePlayerEnabled = Config.Bind("02.Combat.05_FriendlyFire", "SelfDamagePlayerEnabled", true,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "SelfDamagePlayerEnabled", "玩家 AOE 炸自己是否开启。false = 完全忽略（不伤、不受击反应）。"));

            SelfDamageEnemyEnabled = Config.Bind("02.Combat.05_FriendlyFire", "SelfDamageEnemyEnabled", true,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "SelfDamageEnemyEnabled", "敌人 AOE 炸自己是否开启。"));

            SelfDamageAllyEnabled = Config.Bind("02.Combat.05_FriendlyFire", "SelfDamageAllyEnabled", false,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "SelfDamageAllyEnabled", "友军/召唤物 AOE 炸自己是否开启。默认关闭（避免误伤己方触发受击反应）。"));

            AllyFriendlyFireTargetPlayerEnabled = Config.Bind("02.Combat.05_FriendlyFire", "AllyFriendlyFireTargetPlayerEnabled", false,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "AllyFriendlyFireTargetPlayerEnabled", "友军/召唤物的攻击能伤害玩家。需 AoeAllyFriendlyFireEnabled = true。"));

            AllyFriendlyFireTargetAllyEnabled = Config.Bind("02.Combat.05_FriendlyFire", "AllyFriendlyFireTargetAllyEnabled", false,
                Loc.CfgDesc("02.Combat.05_FriendlyFire", "AllyFriendlyFireTargetAllyEnabled", "友军/召唤物的攻击能伤害其他友军。需 AoeAllyFriendlyFireEnabled = true。"));

            // ============================================================
            // 03.Skill
            // ============================================================

            // ---- 03.Skill.01_Tree ----
            SkillPrereqRelaxEnabled = Config.Bind("03.Skill.01_Tree", "SkillPrereqRelaxEnabled", true,
                Loc.CfgDesc("03.Skill.01_Tree", "SkillPrereqRelaxEnabled",
                    "技能组前置放宽。\n"
                    + "true = 不再需要点满当前组全部技能才能解锁下一组，只需点 N 个即可（见下方）。\n"
                    + "false = 保持原版（必须点满所有前置）。"));

            SkillPrereqRequiredCount = Config.Bind("03.Skill.01_Tree", "SkillPrereqRequiredCount", 2,
                Loc.CfgDesc("03.Skill.01_Tree", "SkillPrereqRequiredCount",
                    "解锁下一组技能所需的前置数量。\n"
                    + "原版 = 前置技能的全部数量（点满才能解锁）。\n"
                    + "此项 = 至少点 N 个前置即可解锁，N 可小于全部数量。\n"
                    + "  1 = 只需点 1 个前置（最宽松）\n"
                    + "  2 = 需要点 2 个前置（推荐默认）\n"
                    + "  3 = 需要点 3 个前置\n"
                    + "当 N ≥ 前置总数时，行为等同原版。\n"
                    + "注：只影响'解锁条件'，不影响等级门槛。"));

            // ---- 03.Skill.02_PointRefresh ----
            SkillPointRefreshEnabled = Config.Bind("03.Skill.02_PointRefresh", "SkillPointRefreshEnabled", true,
                Loc.CfgDesc("03.Skill.02_PointRefresh", "SkillPointRefreshEnabled",
                    "技能点可重复刷新。\n"
                    + "已拾取的技能点，满足条件时会在场景加载时重新出现。\n"
                    + "与生态率联动：生态越低，刷新概率越高。"));

            SkillPointRefreshBaseChance = Config.Bind("03.Skill.02_PointRefresh", "SkillPointRefreshBaseChance", 3,
                Loc.CfgDesc("03.Skill.02_PointRefresh", "SkillPointRefreshBaseChance", "基础刷新概率（%）。与生态加成叠加前的起始概率。"));

            SkillPointRefreshEcologyGain = Config.Bind("03.Skill.02_PointRefresh", "SkillPointRefreshEcologyGain", 25f,
                Loc.CfgDesc("03.Skill.02_PointRefresh", "SkillPointRefreshEcologyGain",
                    "刷新命中时，当前地图生态率立即增加的百分比。\n"
                    + "用于防止玩家把生态刷低后反复切图刷技能点。\n"
                    + "支持小数。0 = 不恢复。"));

            SkillPointRefreshCooldownHours = Config.Bind("03.Skill.02_PointRefresh", "SkillPointRefreshCooldownHours", 24f,
                Loc.CfgDesc("03.Skill.02_PointRefresh", "SkillPointRefreshCooldownHours",
                    "拾取后冷却（游戏内小时）。\n"
                    + "冷却期间该技能点不会刷新。支持小数。"));

            SkillPointRefreshDailyLimit = Config.Bind("03.Skill.02_PointRefresh", "SkillPointRefreshDailyLimit", 3,
                Loc.CfgDesc("03.Skill.02_PointRefresh", "SkillPointRefreshDailyLimit",
                    "每日刷新上限（个）。\n"
                    + "按游戏内天数计（总游戏小时 / 24）。\n"
                    + "0 = 无上限。"));

            // ---- 03.Skill.03_CrossPassive ----
            CrossPassiveEnabled = Config.Bind("03.Skill.03_CrossPassive", "CrossPassiveEnabled", true,
                Loc.CfgDesc("03.Skill.03_CrossPassive", "CrossPassiveEnabled",
                    "跨职业被动切换（POC）。\n"
                    + "开启后，在技能面板按 Tab 键循环切换右侧被动词显示的职业。\n"
                    + "只切换图标显示，不影响原版副职业按钮。"));

            CrossPassiveCostEnabled = Config.Bind("03.Skill.03_CrossPassive", "CrossPassiveCostEnabled", true,
                Loc.CfgDesc("03.Skill.03_CrossPassive", "CrossPassiveCostEnabled",
                    "跨职业被动学习的成本缩放。\n"
                    + "按'职业相似轮'距离计算倍率：\n"
                    + "  距离 0（当前视图职业）→ ×1.0\n"
                    + "  距离 1（最相似）      → ×NearMul\n"
                    + "  距离 4（对面）        → ×FarMul\n"
                    + "  中间距离线性插值\n"
                    + "实际消耗 = ceil(原版 skPointUse × 倍率)，保底 1。\n"
                    + "只作用于被动技能（buttonId 14~29），主动技能不受影响。"));

            CrossPassiveCostNearMul = Config.Bind("03.Skill.03_CrossPassive", "CrossPassiveCostNearMul", 110,
                Loc.CfgDesc("03.Skill.03_CrossPassive", "CrossPassiveCostNearMul",
                    "最相似职业（相似轮距离 1）的成本倍率。\n"
                    + "  100 = ×1.0（不加价）\n"
                    + "  110 = ×1.1（默认）\n"
                    + "  150 = ×1.5\n"
                    + "低于 100 会被钳制到 100。"));

            CrossPassiveCostFarMul = Config.Bind("03.Skill.03_CrossPassive", "CrossPassiveCostFarMul", 300,
                Loc.CfgDesc("03.Skill.03_CrossPassive", "CrossPassiveCostFarMul",
                    "对面职业（相似轮距离 4）的成本倍率。\n"
                    + "  300 = ×3.0（默认）\n"
                    + "  500 = ×5.0\n"
                    + "低于 NearMul 会被钳制到 NearMul。"));

            // ---- 03.Skill.04_PassiveSlot ----
            PassiveSlotExpandEnabled = Config.Bind("03.Skill.04_PassiveSlot", "PassiveSlotExpandEnabled", true,
                Loc.CfgDesc("03.Skill.04_PassiveSlot", "PassiveSlotExpandEnabled",
                    "被动技能栏扩展。\n"
                    + "开启后，玩家升级会解锁更多的被动技能槽。\n"
                    + "未解锁的槽位不显示（完全不存在）。"));

            PassiveSlotPerLevels = Config.Bind("03.Skill.04_PassiveSlot", "PassiveSlotPerLevels", 4,
                Loc.CfgDesc("03.Skill.04_PassiveSlot", "PassiveSlotPerLevels",
                    "每 N 级解锁 1 个额外被动槽。\n"
                    + "  4  = 每 4 级 +1 槽（默认），100 级共 32 个\n"
                    + "  6  = 每 6 级 +1 槽，100 级共 24 个\n"
                    + "  12 = 每 12 级 +1 槽，100 级共 16 个\n"
                    + "公式：槽位 = 8 + (等级-1)/N，封顶于 MaxCount"));

            PassiveSlotMaxCount = Config.Bind("03.Skill.04_PassiveSlot", "PassiveSlotMaxCount", 32,
                Loc.CfgDesc("03.Skill.04_PassiveSlot", "PassiveSlotMaxCount",
                    "被动槽位上限（含原版 8 个）。\n"
                    + "硬上限 48。\n"
                    + "默认 32（4 排 × 8 个，100 级正好解锁满）。\n"
                    + "建议保持为 8 的倍数，方便布局成矩形。"));

            // ---- 03.Skill.05_Exp ----
            ExpRequirementEnabled = Config.Bind("03.Skill.05_Exp", "ExpRequirementEnabled", true,
                Loc.CfgDesc("03.Skill.05_Exp", "ExpRequirementEnabled",
                    "是否启用升级经验需求调整。\n"
                    + "true = 按下方 5 段百分比缩减升级所需经验。\n"
                    + "false = 保持原版经验曲线。"));

            ExpRequirementPercents = Config.Bind("03.Skill.05_Exp", "ExpRequirementPercents", "60,50,40,30,20",
                Loc.CfgDesc("03.Skill.05_Exp", "ExpRequirementPercents",
                    "5 段升级经验需求百分比（相对于原版）。\n"
                    + "格式：逗号分隔的 5 个整数，分别对应：\n"
                    + "  第 1 段（1~20 级）\n"
                    + "  第 2 段（21~40 级）\n"
                    + "  第 3 段（41~60 级）\n"
                    + "  第 4 段（61~80 级）\n"
                    + "  第 5 段（81~100 级）\n"
                    + "数值含义：\n"
                    + "  100 = 原版需求\n"
                    + "  50  = 只需原版一半经验即可升级\n"
                    + "  20  = 只需原版 1/5 经验\n"
                    + "数字越小 = 升级越快。\n"
                    + "默认 \"60,50,40,30,20\" 表示升级速度逐段加快。\n"
                    + "想保持完全原版就填 \"100,100,100,100,100\"。"));

            LevelBonusEnabled = Config.Bind("03.Skill.05_Exp", "LevelBonusEnabled", true,
                Loc.CfgDesc("03.Skill.05_Exp", "LevelBonusEnabled", "是否启用等级差补正"));

            LevelCritRatePercentPerLevel = Config.Bind("03.Skill.05_Exp", "LevelCritRatePercentPerLevel", 5,
                Loc.CfgDesc("03.Skill.05_Exp", "LevelCritRatePercentPerLevel", "玩家等级>敌人等级时，每级暴击率 +N%。默认 5"));

            LevelCritRateMaxDiff = Config.Bind("03.Skill.05_Exp", "LevelCritRateMaxDiff", 20,
                Loc.CfgDesc("03.Skill.05_Exp", "LevelCritRateMaxDiff", "等级差超过此值后，暴击率补正不再增加。默认 20，表示差 20 级封顶（配合 CritRatePercentPerLevel=5 时，封顶 +100%）"));

            LevelExpPercentPerLevel = Config.Bind("03.Skill.05_Exp", "LevelExpPercentPerLevel", 5,
                Loc.CfgDesc("03.Skill.05_Exp", "LevelExpPercentPerLevel", "敌人等级>玩家等级时，每级经验 +N%。默认 5，无上限"));

            LevelDropPercentPerLevel = Config.Bind("03.Skill.05_Exp", "LevelDropPercentPerLevel", 5,
                Loc.CfgDesc("03.Skill.05_Exp", "LevelDropPercentPerLevel", "敌人等级>玩家等级时，每级掉率 +N%。默认 5，最终掉率上限为 100%"));

            LevelExpNegativePercentPerLevel = Config.Bind("03.Skill.05_Exp", "LevelExpNegativePercentPerLevel", 5,
                Loc.CfgDesc("03.Skill.05_Exp", "LevelExpNegativePercentPerLevel", "玩家等级>敌人等级时，每级经验 -N%。默认 5，最低不低于 ExpMinPercent"));

            LevelExpMinPercent = Config.Bind("03.Skill.05_Exp", "LevelExpMinPercent", 1,
                Loc.CfgDesc("03.Skill.05_Exp", "LevelExpMinPercent", "经验惩罚的下限。1 表示高等级玩家打低级怪，至少还有原版 1% 经验"));

            MultiCritEnabled = Config.Bind("03.Skill.05_Exp", "MultiCritEnabled", true,
                Loc.CfgDesc("03.Skill.05_Exp", "MultiCritEnabled", "是否启用多重暴击。暴击率溢出 100% 的部分变成额外暴击次数"));

            MultiCritMaxExtra = Config.Bind("03.Skill.05_Exp", "MultiCritMaxExtra", 10,
                Loc.CfgDesc("03.Skill.05_Exp", "MultiCritMaxExtra", "多重暴击的最大额外段数。"));

            LevelMultiCritPercentPerLevel = Config.Bind("03.Skill.05_Exp", "LevelMultiCritPercentPerLevel", 10,
                Loc.CfgDesc("03.Skill.05_Exp", "LevelMultiCritPercentPerLevel", "玩家等级<怪物等级时，每级差提供 +N% 虚拟溢出暴击率，用于多重暴击判定。默认 10"));

            LevelMultiCritMaxDiff = Config.Bind("03.Skill.05_Exp", "LevelMultiCritMaxDiff", 20,
                Loc.CfgDesc("03.Skill.05_Exp", "LevelMultiCritMaxDiff", "等级差超过此值后，不再增加多重暴击概率。默认 20，表示差 20 级封顶"));

            // ============================================================
            // 04.AI
            // ============================================================

            // ---- 04.AI.01_Vision ----
            ConeVisionEnabled = Config.Bind("04.AI.01_Vision", "ConeVisionEnabled", true,
                Loc.CfgDesc("04.AI.01_Vision", "ConeVisionEnabled",
                    "锥形视野开关。敌人/友军的初始发现限制在前方扇形内。"
                    + "已追踪的目标不会因为玩家绕到背后而丢失。"));

            ConeVisionAngle = Config.Bind("04.AI.01_Vision", "ConeVisionAngle", 100,
                Loc.CfgDesc("04.AI.01_Vision", "ConeVisionAngle",
                    "视野角度（总角度）。180 = 前方 ±90 度（半球），90 = 前方 ±45 度，"
                    + "360 = 原版全方向。推荐 100 度左右。"));

            ConeVisionBypassWhenAngry = Config.Bind("04.AI.01_Vision", "ConeVisionBypassWhenAngry", true,
                Loc.CfgDesc("04.AI.01_Vision", "ConeVisionBypassWhenAngry",
                    "敌人处于 angry 状态时，跳过锥形视野判断（恢复 360 度）。"
                    + "被攻击后的敌人会立即转向并追击-被打到就警觉-关闭后会导致允许一直偷袭。"));

            ConeVisionCloseRange = Config.Bind("04.AI.01_Vision", "ConeVisionCloseRange", 1.5f,
                Loc.CfgDesc("04.AI.01_Vision", "ConeVisionCloseRange",
                    "近距离完全感知范围（米）。\n"
                    + "玩家进入这个距离内，无视锥形视野必定被发现（模拟触碰感知）。\n"
                    + "用于防止贴脸背刺。0 = 关闭此功能。"));

            ConeVisionWallBlockEnabled = Config.Bind("04.AI.01_Vision", "ConeVisionWallBlockEnabled", true,
                Loc.CfgDesc("04.AI.01_Vision", "ConeVisionWallBlockEnabled",
                    "视线遮挡开关。锥形视野判断时，如果玩家和敌人之间有墙（tag = Wall），\n"
                    + "即使玩家在视野角度内也不会被发现。\n"
                    + "用于模拟\"躲在墙后\"的潜行玩法。"));

            InterruptCallNearbyOnDamage = Config.Bind("04.AI.01_Vision", "InterruptCallNearbyOnDamage", true,
                Loc.CfgDesc("04.AI.01_Vision", "InterruptCallNearbyOnDamage",
                    "敌人被攻击命中时，打断正在进行的呼叫援军喊叫动作。\n"
                    + "true = 命中打断（敌人立即恢复行动）；false = 保持原版（喊完再恢复）。\n"
                    + "注：呼叫援军的通知本身已经发出，打断只影响敌人的等待动画。"));

            AlertSoundEnabled = Config.Bind("04.AI.01_Vision", "AlertSoundEnabled", true,
                Loc.CfgDesc("04.AI.01_Vision", "AlertSoundEnabled",
                    "敌人/友军发现目标时播放音效。\n"
                    + "使用该单位自己的受伤音效（hurtVoice）作为发现提示音。\n"
                    + "无 hurtVoice 的单位不播放。"));

            // ---- 04.AI.02_Difficulty ----
            AngryDurationScaleS0 = Config.Bind("04.AI.02_Difficulty", "AngryDurationScaleS0", 0.5f,
                Loc.CfgDesc("04.AI.02_Difficulty", "AngryDurationScaleS0",
                    "简单难度下，敌人愤怒状态持续时长的缩放倍率。\n"
                    + "0.5 = 10 秒。\n"
                    + "愤怒期间敌人会无视 lostSight 持续追击。"));

            AngryDurationScaleS1 = Config.Bind("04.AI.02_Difficulty", "AngryDurationScaleS1", 1.0f,
                Loc.CfgDesc("04.AI.02_Difficulty", "AngryDurationScaleS1", "普通难度下的愤怒时长缩放倍率。原版默认 20 秒"));

            AngryDurationScaleS2 = Config.Bind("04.AI.02_Difficulty", "AngryDurationScaleS2", 2.0f,
                Loc.CfgDesc("04.AI.02_Difficulty", "AngryDurationScaleS2", "挑战难度下的愤怒时长缩放倍率。= 40 秒"));

            AngryDurationScaleS3 = Config.Bind("04.AI.02_Difficulty", "AngryDurationScaleS3", 3.0f,
                Loc.CfgDesc("04.AI.02_Difficulty", "AngryDurationScaleS3", "地狱难度下的愤怒时长缩放倍率。= 60 秒"));

            DetectRangeScaleS0 = Config.Bind("04.AI.02_Difficulty", "DetectRangeScaleS0", 0.6f,
                Loc.CfgDesc("04.AI.02_Difficulty", "DetectRangeScaleS0",
                    "简单难度下，敌人/友军探测范围缩放倍率。\n"
                    + "1.0 = 原版，1.5 = 范围扩大 50%，2.0 = 翻倍。\n"
                    + "同时作用于 detectRange 和 angryDetectRange。"));

            DetectRangeScaleS1 = Config.Bind("04.AI.02_Difficulty", "DetectRangeScaleS1", 1.0f,
                Loc.CfgDesc("04.AI.02_Difficulty", "DetectRangeScaleS1", "普通难度下的探测范围缩放倍率。"));

            DetectRangeScaleS2 = Config.Bind("04.AI.02_Difficulty", "DetectRangeScaleS2", 2.0f,
                Loc.CfgDesc("04.AI.02_Difficulty", "DetectRangeScaleS2", "挑战难度下的探测范围缩放倍率。"));

            DetectRangeScaleS3 = Config.Bind("04.AI.02_Difficulty", "DetectRangeScaleS3", 3.0f,
                Loc.CfgDesc("04.AI.02_Difficulty", "DetectRangeScaleS3", "地狱难度下的探测范围缩放倍率。"));

            CallRadiusScaleS0 = Config.Bind("04.AI.02_Difficulty", "CallRadiusScaleS0", 0.6f,
                Loc.CfgDesc("04.AI.02_Difficulty", "CallRadiusScaleS0",
                    "简单难度下，敌人呼叫援军的范围缩放倍率。\n"
                    + "1.0 = 原版（callRadius 默认 15 米），1.5 = 范围扩大 50% 。"));

            CallRadiusScaleS1 = Config.Bind("04.AI.02_Difficulty", "CallRadiusScaleS1", 1.0f,
                Loc.CfgDesc("04.AI.02_Difficulty", "CallRadiusScaleS1", "普通难度下的呼叫范围缩放倍率。"));

            CallRadiusScaleS2 = Config.Bind("04.AI.02_Difficulty", "CallRadiusScaleS2", 2.0f,
                Loc.CfgDesc("04.AI.02_Difficulty", "CallRadiusScaleS2", "挑战难度下的呼叫范围缩放倍率。"));

            CallRadiusScaleS3 = Config.Bind("04.AI.02_Difficulty", "CallRadiusScaleS3", 3.0f,
                Loc.CfgDesc("04.AI.02_Difficulty", "CallRadiusScaleS3",
                    "地狱难度下的呼叫范围缩放倍率。\n"
                    + "3.0 = 范围翻倍（15 米 → 45 米），一次呼叫拉更多援军。"));

            // ---- 04.AI.03_Noise ----
            NoiseEnabled = Config.Bind("04.AI.03_Noise", "NoiseEnabled", true,
                Loc.CfgDesc("04.AI.03_Noise", "NoiseEnabled",
                    "噪音警觉。单位 HP 低于阈值时发出噪音，吸引附近 其他单位。\n"
                    + "重伤会叫喊，喊声吸引周围目标。"));

            NoiseHpThresholdPercent = Config.Bind("04.AI.03_Noise", "NoiseHpThresholdPercent", 30,
                Loc.CfgDesc("04.AI.03_Noise", "NoiseHpThresholdPercent",
                    "触发噪音的 HP 阈值（百分比）。\n"
                    + "血量首次降到该值以下时 将 延迟启动。"));

            NoiseTriggerDelaySec = Config.Bind("04.AI.03_Noise", "NoiseTriggerDelaySec", 1.5f,
                Loc.CfgDesc("04.AI.03_Noise", "NoiseTriggerDelaySec",
                    "HP 跨过阈值后延迟多久才真正触发噪音（秒）。\n"
                    + "反应窗口——在此期间击杀或控制可打断。"));

            NoiseCallRadius = Config.Bind("04.AI.03_Noise", "NoiseCallRadius", 15f,
                Loc.CfgDesc("04.AI.03_Noise", "NoiseCallRadius", "吸引半径（米）。被吸引的 其他单位 会朝噪音位置移动。"));

            NoiseCallRadiusScaleS0 = Config.Bind("04.AI.03_Noise", "NoiseCallRadiusScaleS0", 0.6f, Loc.CfgDesc("04.AI.03_Noise", "NoiseCallRadiusScaleS0", "简单难度吸引半径倍率。"));
            NoiseCallRadiusScaleS1 = Config.Bind("04.AI.03_Noise", "NoiseCallRadiusScaleS1", 1.0f, Loc.CfgDesc("04.AI.03_Noise", "NoiseCallRadiusScaleS1", "普通难度吸引半径倍率。"));
            NoiseCallRadiusScaleS2 = Config.Bind("04.AI.03_Noise", "NoiseCallRadiusScaleS2", 2.0f, Loc.CfgDesc("04.AI.03_Noise", "NoiseCallRadiusScaleS2", "挑战难度吸引半径倍率。"));
            NoiseCallRadiusScaleS3 = Config.Bind("04.AI.03_Noise", "NoiseCallRadiusScaleS3", 3.0f, Loc.CfgDesc("04.AI.03_Noise", "NoiseCallRadiusScaleS3", "地狱难度吸引半径倍率。"));

            NoiseSharedCooldownSec = Config.Bind("04.AI.03_Noise", "NoiseSharedCooldownSec", 9f,
                Loc.CfgDesc("04.AI.03_Noise", "NoiseSharedCooldownSec", "共享冷却（秒）。噪音半径内共享同一冷却时间。"));

            NoiseMaxNearbyUnits = Config.Bind("04.AI.03_Noise", "NoiseMaxNearbyUnits", 3,
                Loc.CfgDesc("04.AI.03_Noise", "NoiseMaxNearbyUnits",
                    "周围 AI 单位数量阈值（含自己）。\n"
                    + "超过此值时不触发噪音—。\n"
                    + "  3 = 单位自己 + 2 个 AI 单位以内触发。"));

            NoiseShowVisualIndicator = Config.Bind("04.AI.03_Noise", "NoiseShowVisualIndicator", true,
                Loc.CfgDesc("04.AI.03_Noise", "NoiseShowVisualIndicator", "是否显示头顶的 !! 提示。"));

            NoiseSymbol = Config.Bind("04.AI.03_Noise", "NoiseSymbol", "!!",
                Loc.CfgDesc("04.AI.03_Noise", "NoiseSymbol",
                    "噪音提示显示的字符。\n"
                    + "默认 '!!'（双感叹号）。\n"
                    + "可改成其他符号，如 '??' / '♨' / '♪' 等。\n"
                    + "填错不会报错，会原样显示。"));

            // ---- 04.AI.04_Alert ----
            AlertEnabled = Config.Bind("04.AI.04_Alert", "AlertEnabled", true,
                Loc.CfgDesc("04.AI.04_Alert", "AlertEnabled", "是否启用发现目标的感叹号提示"));

            AlertSymbol = Config.Bind("04.AI.04_Alert", "AlertSymbol", "!",
                Loc.CfgDesc("04.AI.04_Alert", "AlertSymbol", "感叹号字符。可改成其他符号"));

            AlertFontSize = Config.Bind("04.AI.04_Alert", "AlertFontSize", 70,
                Loc.CfgDesc("04.AI.04_Alert", "AlertFontSize", "字号"));

            AlertTextColor = Config.Bind("04.AI.04_Alert", "AlertTextColor", "255,0,0",
                Loc.CfgDesc("04.AI.04_Alert", "AlertTextColor", "主色 R,G,B"));

            AlertBorderColor = Config.Bind("04.AI.04_Alert", "AlertBorderColor", "255,255,255",
                Loc.CfgDesc("04.AI.04_Alert", "AlertBorderColor", "描边色 R,G,B"));

            AlertFontName = Config.Bind("04.AI.04_Alert", "AlertFontName", "",
                Loc.CfgDesc("04.AI.04_Alert", "AlertFontName",
                    "字体名。留空 = 默认跟随 自定义字体.连杀计数字体.FontName；"
                    + "填其他名字则使用独立字体"));

            AlertBold = Config.Bind("04.AI.04_Alert", "AlertBold", true,
                Loc.CfgDesc("04.AI.04_Alert", "AlertBold", "是否粗体"));

            AlertItalic = Config.Bind("04.AI.04_Alert", "AlertItalic", false,
                Loc.CfgDesc("04.AI.04_Alert", "AlertItalic", "是否斜体"));

            AlertMaxCount = Config.Bind("04.AI.04_Alert", "AlertMaxCount", 10,
                Loc.CfgDesc("04.AI.04_Alert", "AlertMaxCount", "同屏最大显示数量"));

            AlertPerEnemyCooldownSec = Config.Bind("04.AI.04_Alert", "AlertPerEnemyCooldownSec", 3f,
                Loc.CfgDesc("04.AI.04_Alert", "AlertPerEnemyCooldownSec", "同一目标的触发冷却（秒）"));

            AlertShowEnemy = Config.Bind("04.AI.04_Alert", "AlertShowEnemy", true,
                Loc.CfgDesc("04.AI.04_Alert", "AlertShowEnemy", "是否显示敌人的感叹号"));

            AlertShowAlly = Config.Bind("04.AI.04_Alert", "AlertShowAlly", true,
                Loc.CfgDesc("04.AI.04_Alert", "AlertShowAlly", "是否显示友军/召唤物的感叹号"));

            // ---- 04.AI.05_Night ----
            NightEnemyEnabled = Config.Bind("04.AI.05_Night", "NightEnemyEnabled", true,
                Loc.CfgDesc("04.AI.05_Night", "NightEnemyEnabled", "是否启用夜晚强化敌人。夜晚生成的敌人会按难度概率获得随机词条，并获得等级加成"));

            NightChanceS0 = Config.Bind("04.AI.05_Night", "NightChanceS0", 0,
                Loc.CfgDesc("04.AI.05_Night", "NightChanceS0", "简单难度下，夜晚敌人获得词条的概率（0~100）"));
            NightChanceS1 = Config.Bind("04.AI.05_Night", "NightChanceS1", 25,
                Loc.CfgDesc("04.AI.05_Night", "NightChanceS1", "普通难度下，夜晚敌人获得词条的概率（0~100）"));
            NightChanceS2 = Config.Bind("04.AI.05_Night", "NightChanceS2", 50,
                Loc.CfgDesc("04.AI.05_Night", "NightChanceS2", "挑战难度下，夜晚敌人获得词条的概率（0~100）"));
            NightChanceS3 = Config.Bind("04.AI.05_Night", "NightChanceS3", 100,
                Loc.CfgDesc("04.AI.05_Night", "NightChanceS3", "地狱难度下，夜晚敌人获得词条的概率（0~100）"));

            NightLevelS0 = Config.Bind("04.AI.05_Night", "NightLevelS0", 0,
                Loc.CfgDesc("04.AI.05_Night", "NightLevelS0", "简单难度下，夜晚敌人的等级加成"));
            NightLevelS1 = Config.Bind("04.AI.05_Night", "NightLevelS1", 3,
                Loc.CfgDesc("04.AI.05_Night", "NightLevelS1", "普通难度下，夜晚敌人的等级加成"));
            NightLevelS2 = Config.Bind("04.AI.05_Night", "NightLevelS2", 6,
                Loc.CfgDesc("04.AI.05_Night", "NightLevelS2", "挑战难度下，夜晚敌人的等级加成"));
            NightLevelS3 = Config.Bind("04.AI.05_Night", "NightLevelS3", 9,
                Loc.CfgDesc("04.AI.05_Night", "NightLevelS3", "地狱难度下，夜晚敌人的等级加成"));
            NightLevelSkipThreshold = Config.Bind("04.AI.05_Night", "NightLevelSkipThreshold", 9,
                Loc.CfgDesc("04.AI.05_Night", "NightLevelSkipThreshold",
                    "如果敌人等级 ≥ 玩家等级 + 此值，跳过夜晚等级加成，防止召唤物二次叠加。"
                    + "0 = 不检查，每次都 +N；9 = 玩家 18 级时敌人 ≥ 27 级就跳过等级叠加（推荐）"));

            // ---- 04.AI.06_LevelScale ----
            EnemyLevelScale_S0 = Config.Bind("04.AI.06_LevelScale", "EnemyLevelScale_S0", 1,
                Loc.CfgDesc("04.AI.06_LevelScale", "EnemyLevelScale_S0",
                    "简单难度下，敌人等级跟随玩家的上限倍数（基于原生等级）。\n"
                    + "1 = 不跟随（敌人保持原生等级），3 = 最高跟随到原生等级 × 3"));

            EnemyLevelScale_S1 = Config.Bind("04.AI.06_LevelScale", "EnemyLevelScale_S1", 1,
                Loc.CfgDesc("04.AI.06_LevelScale", "EnemyLevelScale_S1", "普通难度下的上限倍数。默认 1（不跟随）"));

            EnemyLevelScale_S2 = Config.Bind("04.AI.06_LevelScale", "EnemyLevelScale_S2", 3,
                Loc.CfgDesc("04.AI.06_LevelScale", "EnemyLevelScale_S2", "挑战难度下的上限倍数。默认 3"));

            EnemyLevelScale_S3 = Config.Bind("04.AI.06_LevelScale", "EnemyLevelScale_S3", 9,
                Loc.CfgDesc("04.AI.06_LevelScale", "EnemyLevelScale_S3", "地狱难度下的上限倍数。默认 9"));

            // ============================================================
            // 05.Combo
            // ============================================================

            // ---- 05.Combo.01_Core ----
            ComboEnabled = Config.Bind("05.Combo.01_Core", "ComboEnabled", true, Loc.CfgDesc("05.Combo.01_Core", "ComboEnabled", "是否启用连杀系统"));
            ComboWindowMs = Config.Bind("05.Combo.01_Core", "ComboWindowMs", 9000,
                Loc.CfgDesc("05.Combo.01_Core", "ComboWindowMs", "连杀超时窗口（毫秒）。9000 = 9 秒 = 现实 9 秒。正常游玩时按现实时间计，睡觉/加速时间时会随游戏内时间一起加速"));
            ComboMinEnemyLevelDiff = Config.Bind("05.Combo.01_Core", "ComboMinEnemyLevelDiff", -6, Loc.CfgDesc("05.Combo.01_Core", "ComboMinEnemyLevelDiff", "敌人等级 ≥ 玩家等级 + 此值才算有效连杀"));
            ComboPlayerDamageWindowMs = Config.Bind("05.Combo.01_Core", "ComboPlayerDamageWindowMs", 10000, Loc.CfgDesc("05.Combo.01_Core", "ComboPlayerDamageWindowMs", "玩家伤害敌人后多久内敌人死亡算有效（毫秒）"));
            StaminaRewardEvery = Config.Bind("05.Combo.01_Core", "StaminaRewardEvery", 3, Loc.CfgDesc("05.Combo.01_Core", "StaminaRewardEvery", "每 N 杀触发一次体力奖励"));
            StaminaRewardPercent = Config.Bind("05.Combo.01_Core", "StaminaRewardPercent", 15, Loc.CfgDesc("05.Combo.01_Core", "StaminaRewardPercent", "体力恢复百分比（相对 maxStamina）"));
            ComboUIPosXPercent = Config.Bind("05.Combo.01_Core", "ComboUIPosXPercent", 83, Loc.CfgDesc("05.Combo.01_Core", "ComboUIPosXPercent", "UI 位置：数字中心相对于屏幕宽度的百分比"));
            ComboUIPosYPercent = Config.Bind("05.Combo.01_Core", "ComboUIPosYPercent", 33, Loc.CfgDesc("05.Combo.01_Core", "ComboUIPosYPercent", "UI 位置：数字中心相对于屏幕高度的百分比"));

            ComboTimerBarColor = Config.Bind("05.Combo.01_Core", "ComboTimerBarColor", "0,230,0", Loc.CfgDesc("05.Combo.01_Core", "ComboTimerBarColor", "计时条颜色 R,G,B"));
            ComboRewardTextColor = Config.Bind("05.Combo.01_Core", "ComboRewardTextColor", "10,200,10", Loc.CfgDesc("05.Combo.01_Core", "ComboRewardTextColor", "奖励文字主色 R,G,B"));
            ComboRewardTextBorderColor = Config.Bind("05.Combo.01_Core", "ComboRewardTextBorderColor", "255,0,120", Loc.CfgDesc("05.Combo.01_Core", "ComboRewardTextBorderColor", "奖励文字镶边色 R,G,B"));

            // ---- 05.Combo.02_Grade ----
            ComboColor_D = Config.Bind("05.Combo.02_Grade", "ComboColor_D", "235,245,245", Loc.CfgDesc("05.Combo.02_Grade", "ComboColor_D", "D 档主色"));
            ComboBorder_D = Config.Bind("05.Combo.02_Grade", "ComboBorder_D", "50,0,0", Loc.CfgDesc("05.Combo.02_Grade", "ComboBorder_D", "D 档镶边色"));
            ComboColor_C = Config.Bind("05.Combo.02_Grade", "ComboColor_C", "50,150,250", Loc.CfgDesc("05.Combo.02_Grade", "ComboColor_C", "C 档主色"));
            ComboBorder_C = Config.Bind("05.Combo.02_Grade", "ComboBorder_C", "75,0,0", Loc.CfgDesc("05.Combo.02_Grade", "ComboBorder_C", "C 档镶边色"));
            ComboColor_B = Config.Bind("05.Combo.02_Grade", "ComboColor_B", "255,210,40", Loc.CfgDesc("05.Combo.02_Grade", "ComboColor_B", "B 档主色"));
            ComboBorder_B = Config.Bind("05.Combo.02_Grade", "ComboBorder_B", "100,0,0", Loc.CfgDesc("05.Combo.02_Grade", "ComboBorder_B", "B 档镶边色"));
            ComboColor_A = Config.Bind("05.Combo.02_Grade", "ComboColor_A", "255,100,20", Loc.CfgDesc("05.Combo.02_Grade", "ComboColor_A", "A 档主色"));
            ComboBorder_A = Config.Bind("05.Combo.02_Grade", "ComboBorder_A", "125,0,0", Loc.CfgDesc("05.Combo.02_Grade", "ComboBorder_A", "A 档镶边色"));
            ComboColor_S = Config.Bind("05.Combo.02_Grade", "ComboColor_S", "250,55,35", Loc.CfgDesc("05.Combo.02_Grade", "ComboColor_S", "S 档主色"));
            ComboBorder_S = Config.Bind("05.Combo.02_Grade", "ComboBorder_S", "100,100,100", Loc.CfgDesc("05.Combo.02_Grade", "ComboBorder_S", "S 档镶边色"));
            ComboColor_SS = Config.Bind("05.Combo.02_Grade", "ComboColor_SS", "225,25,25", Loc.CfgDesc("05.Combo.02_Grade", "ComboColor_SS", "SS 档主色"));
            ComboBorder_SS = Config.Bind("05.Combo.02_Grade", "ComboBorder_SS", "200,200,200", Loc.CfgDesc("05.Combo.02_Grade", "ComboBorder_SS", "SS 档镶边色"));
            ComboColor_SSS = Config.Bind("05.Combo.02_Grade", "ComboColor_SSS", "200,0,0", Loc.CfgDesc("05.Combo.02_Grade", "ComboColor_SSS", "SSS 档主色"));
            ComboBorder_SSS = Config.Bind("05.Combo.02_Grade", "ComboBorder_SSS", "255,175,0", Loc.CfgDesc("05.Combo.02_Grade", "ComboBorder_SSS", "SSS 档镶边色"));

            ComboThreshold_C = Config.Bind("05.Combo.02_Grade", "ComboThreshold_C", 3, Loc.CfgDesc("05.Combo.02_Grade", "ComboThreshold_C", "C 档阈值"));
            ComboThreshold_B = Config.Bind("05.Combo.02_Grade", "ComboThreshold_B", 6, Loc.CfgDesc("05.Combo.02_Grade", "ComboThreshold_B", "B 档阈值"));
            ComboThreshold_A = Config.Bind("05.Combo.02_Grade", "ComboThreshold_A", 10, Loc.CfgDesc("05.Combo.02_Grade", "ComboThreshold_A", "A 档阈值"));
            ComboThreshold_S = Config.Bind("05.Combo.02_Grade", "ComboThreshold_S", 15, Loc.CfgDesc("05.Combo.02_Grade", "ComboThreshold_S", "S 档阈值"));
            ComboThreshold_SS = Config.Bind("05.Combo.02_Grade", "ComboThreshold_SS", 20, Loc.CfgDesc("05.Combo.02_Grade", "ComboThreshold_SS", "SS 档阈值"));
            ComboThreshold_SSS = Config.Bind("05.Combo.02_Grade", "ComboThreshold_SSS", 30, Loc.CfgDesc("05.Combo.02_Grade", "ComboThreshold_SSS", "SSS 档阈值"));

            // ---- 05.Combo.03_UI ----
            TimerBarWidth = Config.Bind("05.Combo.03_UI", "TimerBarWidth", 90,
                Loc.CfgDesc("05.Combo.03_UI", "TimerBarWidth", "时间条宽度（基础像素，会乘以 UIScale）"));
            TimerBarHeight = Config.Bind("05.Combo.03_UI", "TimerBarHeight", 8,
                Loc.CfgDesc("05.Combo.03_UI", "TimerBarHeight", "时间条高度（基础像素，会乘以 UIScale）"));

            LetterFontSize = Config.Bind("05.Combo.03_UI", "LetterFontSize", 83,
                Loc.CfgDesc("05.Combo.03_UI", "LetterFontSize", "字母字号（基础值，会乘以 UIScale）"));
            LetterOffsetX = Config.Bind("05.Combo.03_UI", "LetterOffsetX", 0,
                Loc.CfgDesc("05.Combo.03_UI", "LetterOffsetX", "字母相对时间条中心的 X 偏移（基础像素，会乘以 UIScale）"));
            LetterOffsetY = Config.Bind("05.Combo.03_UI", "LetterOffsetY", -40,
                Loc.CfgDesc("05.Combo.03_UI", "LetterOffsetY", "字母相对时间条中心的 Y 偏移。负值向上，正值向下"));

            NumberFontSize = Config.Bind("05.Combo.03_UI", "NumberFontSize", 40,
                Loc.CfgDesc("05.Combo.03_UI", "NumberFontSize", "数字字号（基础值，会乘以 UIScale）"));
            NumberOffsetX = Config.Bind("05.Combo.03_UI", "NumberOffsetX", 0,
                Loc.CfgDesc("05.Combo.03_UI", "NumberOffsetX", "数字相对时间条中心的 X 偏移"));
            NumberOffsetY = Config.Bind("05.Combo.03_UI", "NumberOffsetY", 28,
                Loc.CfgDesc("05.Combo.03_UI", "NumberOffsetY", "数字相对时间条中心的 Y 偏移。负值向上，正值向下"));

            RewardFontSize = Config.Bind("05.Combo.03_UI", "RewardFontSize", 21,
                Loc.CfgDesc("05.Combo.03_UI", "RewardFontSize", "奖励文字字号（基础值，会乘以 UIScale）"));
            RewardOffsetX = Config.Bind("05.Combo.03_UI", "RewardOffsetX", 0,
                Loc.CfgDesc("05.Combo.03_UI", "RewardOffsetX", "奖励文字相对时间条中心的 X 偏移"));
            RewardOffsetY = Config.Bind("05.Combo.03_UI", "RewardOffsetY", 88,
                Loc.CfgDesc("05.Combo.03_UI", "RewardOffsetY", "奖励文字相对时间条中心的 Y 偏移。负值向上，正值向下"));

            ComboNumberPrefix = Config.Bind("05.Combo.03_UI", "ComboNumberPrefix", "KILL x",
                Loc.CfgDesc("05.Combo.03_UI", "ComboNumberPrefix",
                    "连杀数字前缀。默认 'KILL x'，最终显示为 'KILL x13'。"
                    + "想只显示数字就留空，想显示 'x 13' 就填 'x '"));

            ComboHitPrefix = Config.Bind("05.Combo.03_UI", "ComboHitPrefix", "HIT x",
                Loc.CfgDesc("05.Combo.03_UI", "ComboHitPrefix",
                    "HIT 数字前缀。默认 'HIT x'，最终显示为 'HIT x13'。"
                    + "想只显示数字就留空，想显示 'x 13' 就填 'x '"));

            ComboBorderStyle = Config.Bind("05.Combo.03_UI", "ComboBorderStyle", "Spread",
                Loc.CfgDesc("05.Combo.03_UI", "ComboBorderStyle", "边框绘制方式。可选：Spread（扩散式，推荐）/ Scale（放大式）/ None（无边框）"));
            ComboBorderOffset = Config.Bind("05.Combo.03_UI", "ComboBorderOffset", 2,
                Loc.CfgDesc("05.Combo.03_UI", "ComboBorderOffset", "扩散式边框的偏移像素。建议 2~4"));
            ComboScaleBorderOffset = Config.Bind("05.Combo.03_UI", "ComboScaleBorderOffset", 14,
                Loc.CfgDesc("05.Combo.03_UI", "ComboScaleBorderOffset", "Scale 模式边框相对主字号的放大像素。建议 6~21。仅 BorderStyle = Scale 时有效"));
            ComboUIScale = Config.Bind("05.Combo.03_UI", "ComboUIScale", 1.6f,
                Loc.CfgDesc("05.Combo.03_UI", "ComboUIScale", "UI 整体缩放。1.0=标准，1.2=大一些，0.8=小一些"));

            LetterBold = Config.Bind("05.Combo.03_UI", "LetterBold", true,
                Loc.CfgDesc("05.Combo.03_UI", "LetterBold", "连杀字母是否使用粗体"));
            LetterItalic = Config.Bind("05.Combo.03_UI", "LetterItalic", true,
                Loc.CfgDesc("05.Combo.03_UI", "LetterItalic", "连杀是否使用斜体"));
            LetterPulse = Config.Bind("05.Combo.03_UI", "LetterPulse", true,
                Loc.CfgDesc("05.Combo.03_UI", "LetterPulse",
                    "连杀字母颜色脉动。\n"
                    + "字母主色会随时间朝向'下一档'的目标色轻轻波动。"));
            NumberBold = Config.Bind("05.Combo.03_UI", "NumberBold", true,
                Loc.CfgDesc("05.Combo.03_UI", "NumberBold", "连杀数字是否使用粗体"));
            NumberItalic = Config.Bind("05.Combo.03_UI", "NumberItalic", true,
                Loc.CfgDesc("05.Combo.03_UI", "NumberItalic", "连杀数字是否使用斜体"));
            RewardBold = Config.Bind("05.Combo.03_UI", "RewardBold", true,
                Loc.CfgDesc("05.Combo.03_UI", "RewardBold", "连杀奖励文字是否使用粗体"));
            RewardItalic = Config.Bind("05.Combo.03_UI", "RewardItalic", true,
                Loc.CfgDesc("05.Combo.03_UI", "RewardItalic", "连杀奖励文字是否使用斜体"));

            ComboFontName = Config.Bind("05.Combo.03_UI", "ComboFontName", "",
                Loc.CfgDesc("05.Combo.03_UI", "ComboFontName",
                    "自定义字体。留空 = 使用游戏默认字体。\n"
                    + "\n"
                    + "【安装字体（关键步骤）】\n"
                    + "下载字体后，右键 → 选择【为所有用户安装】。\n"
                    + "  · 不要选普通\"安装\"，mod无法读取\n"
                    + "  · 如果之前已经普通安装过，重新\"为所有用户安装\"\n"
                    + "  · 需要管理员权限\n"
                    + "\n"
                    + "【填写字体名】\n"
                    + "方式 A（推荐）：填文件名\n"
                    + "  1. 打开 C:\\Windows\\Fonts\\ 文件夹，找到刚装的字体文件\n"
                    + "  2. 右键后选择属性，复制文件名（如 SHOWG.TTF，含 .ttf 后缀）\n"
                    + "  3. 粘贴到此处，重新进入游戏\n"
                    + "方式 B：填字体内部名\n"
                    + "  · 严格区分大小写，一个字都不能错\n"
                    + "  · 可从字体文件属性 → 详细信息中查看\n"
                    + "\n"
                    + "【重要限制】\n"
                    + "  · 优先使用英文字体，大部分中文字体无法读取\n"
                    + "  · 部分艺术字体即使安装正确也可能无法加载（Unity 2017 限制）\n"
                    + "  · 更换字体后可能需要微调 Combo.UI 段的位置和大小\n"
                    + "  · 填错名字不会闪退，会自动回退到默认字体\n"
                    + "\n"
                    + "【提示】想恢复默认字体，把此处清空即可。"));

            // ---- 05.Combo.04_HitCombo ----
            HitComboEnabled = Config.Bind("05.Combo.04_HitCombo", "HitComboEnabled", true,
                Loc.CfgDesc("05.Combo.04_HitCombo", "HitComboEnabled", "是否启用 HIT 连击系统。命中敌人时累计一个瞬态计数，不参与奖励和档位"));

            HitWindowMs = Config.Bind("05.Combo.04_HitCombo", "HitWindowMs", 3000,
                Loc.CfgDesc("05.Combo.04_HitCombo", "HitWindowMs", "HIT 窗口（毫秒）。超过此时间无命中则 HIT 清零"));

            HitRewardEvery = Config.Bind("05.Combo.04_HitCombo", "HitRewardEvery", 0,
                Loc.CfgDesc("05.Combo.04_HitCombo", "HitRewardEvery",
                    "每 N 次命中给一次体力奖励。0 = 关闭，走击杀奖励；"
                    + "非 0 时，击杀奖励被禁用，只通过 HIT 触发奖励"));

            HitFontSize = Config.Bind("05.Combo.04_HitCombo", "HitFontSize", 21,
                Loc.CfgDesc("05.Combo.04_HitCombo", "HitFontSize", "HIT 数字字号（基础值，会乘以 UIScale）"));
            HitOffsetX = Config.Bind("05.Combo.04_HitCombo", "HitOffsetX", 0,
                Loc.CfgDesc("05.Combo.04_HitCombo", "HitOffsetX", "HIT 数字相对时间条中心的 X 偏移"));
            HitOffsetY = Config.Bind("05.Combo.04_HitCombo", "HitOffsetY", 60,
                Loc.CfgDesc("05.Combo.04_HitCombo", "HitOffsetY", "HIT 数字相对时间条中心的 Y 偏移"));
            HitTextColor = Config.Bind("05.Combo.04_HitCombo", "HitTextColor", "255,0,0",
                Loc.CfgDesc("05.Combo.04_HitCombo", "HitTextColor", "HIT 数字主色 R,G,B"));
            HitBorderColor = Config.Bind("05.Combo.04_HitCombo", "HitBorderColor", "255,255,255",
                Loc.CfgDesc("05.Combo.04_HitCombo", "HitBorderColor", "HIT 数字镶边色 R,G,B"));
            HitBold = Config.Bind("05.Combo.04_HitCombo", "HitBold", false,
                Loc.CfgDesc("05.Combo.04_HitCombo", "HitBold", "HIT 数字是否粗体"));
            HitItalic = Config.Bind("05.Combo.04_HitCombo", "HitItalic", false,
                Loc.CfgDesc("05.Combo.04_HitCombo", "HitItalic", "HIT 数字是否斜体"));

            // ---- 05.Combo.05_Challenge ----
            ChallengeHitMode = Config.Bind("05.Combo.05_Challenge", "ChallengeHitMode", 1,
                Loc.CfgDesc("05.Combo.05_Challenge", "ChallengeHitMode",
                    "受击模式。0=无（简单模式，受击不影响连杀），"
                    + "1=减时间（默认），2=重置计时（挑战模式）"));

            ChallengeHitReduceType = Config.Bind("05.Combo.05_Challenge", "ChallengeHitReduceType", 1,
                Loc.CfgDesc("05.Combo.05_Challenge", "ChallengeHitReduceType",
                    "减时间方式。0=固定秒数，1=最大时间的百分比（默认），2=当前剩余时间的百分比。"
                    + "仅在 HitMode=1 时有效"));

            ChallengeHitReduceSeconds = Config.Bind("05.Combo.05_Challenge", "ChallengeHitReduceSeconds", 3,
                Loc.CfgDesc("05.Combo.05_Challenge", "ChallengeHitReduceSeconds", "固定扣减秒数。仅在 HitReduceType=0 时有效"));

            ChallengeHitReducePercent = Config.Bind("05.Combo.05_Challenge", "ChallengeHitReducePercent", 30,
                Loc.CfgDesc("05.Combo.05_Challenge", "ChallengeHitReducePercent", "扣减百分比（0~100）。仅在 HitReduceType=1 或 2 时有效"));

            ChallengeDefenseRecoverEnabled = Config.Bind("05.Combo.05_Challenge", "ChallengeDefenseRecoverEnabled", true,
                Loc.CfgDesc("05.Combo.05_Challenge", "ChallengeDefenseRecoverEnabled", "闪避/格挡成功时是否恢复连杀计时"));

            ChallengeDefenseRecoverType = Config.Bind("05.Combo.05_Challenge", "ChallengeDefenseRecoverType", 1,
                Loc.CfgDesc("05.Combo.05_Challenge", "ChallengeDefenseRecoverType", "恢复方式。0=固定秒数，1=最大时间的百分比（默认）"));

            ChallengeDefenseRecoverSeconds = Config.Bind("05.Combo.05_Challenge", "ChallengeDefenseRecoverSeconds", 1.5f,
                Loc.CfgDesc("05.Combo.05_Challenge", "ChallengeDefenseRecoverSeconds", "固定恢复秒数（小数）。仅在 DefenseRecoverType=0 时有效"));

            ChallengeDefenseRecoverPercent = Config.Bind("05.Combo.05_Challenge", "ChallengeDefenseRecoverPercent", 15,
                Loc.CfgDesc("05.Combo.05_Challenge", "ChallengeDefenseRecoverPercent", "恢复百分比（0~100）。100 表示回满。仅在 DefenseRecoverType=1 时有效"));

            // ---- 05.Combo.06_PauseInfo ----
            PauseInfoDetail = Config.Bind("05.Combo.06_PauseInfo", "PauseInfoDetail", 1,
                Loc.CfgDesc("05.Combo.06_PauseInfo", "PauseInfoDetail",
                    "暂停时显示配置摘要的详细程度。\n"
                    + "  0 = 关闭\n"
                    + "  1 = 核心摘要\n"
                    + "  2 = 额外细节\n"
                    + "  3 = 全功能一览"));

            // ============================================================
            // 06.UI
            // ============================================================

            // ---- 06.UI.01_ThreatRing ----
            ThreatRingEnabled = Config.Bind("06.UI.01_ThreatRing", "ThreatRingEnabled", true,
                Loc.CfgDesc("06.UI.01_ThreatRing", "ThreatRingEnabled",
                    "环绕方位指示器。玩家周围 8 个方位显示三角形，指示锁定玩家阵营的敌人。\n"
                    + "  灰色半透明 = 敌人锁定目标\n"
                    + "  红色闪烁   = 敌人正在攻击\n"
                    + "三角尖头指向玩家。"));

            ThreatRingRadius = Config.Bind("06.UI.01_ThreatRing", "ThreatRingRadius", 245f,
                Loc.CfgDesc("06.UI.01_ThreatRing", "ThreatRingRadius",
                    "三角形距玩家屏幕中心的半径（像素）。\n"
                    + "  200~300 = 推荐（不遮挡主角）。"));

            ThreatRingMaxDistance = Config.Bind("06.UI.01_ThreatRing", "ThreatRingMaxDistance", 30f,
                Loc.CfgDesc("06.UI.01_ThreatRing", "ThreatRingMaxDistance", "超过此距离的敌人不显示（米）。"));

            ThreatRingTriangleSize = Config.Bind("06.UI.01_ThreatRing", "ThreatRingTriangleSize", 45f,
                Loc.CfgDesc("06.UI.01_ThreatRing", "ThreatRingTriangleSize", "三角形边长（像素）。"));

            ThreatRingLockedColor = Config.Bind("06.UI.01_ThreatRing", "ThreatRingLockedColor", "180,180,180,100",
                Loc.CfgDesc("06.UI.01_ThreatRing", "ThreatRingLockedColor",
                    "锁定状态颜色。格式 R,G,B,A。A=透明度 0~255，0 全透明，255 不透明。\n"
                    + "默认 180,180,180,100 = 半透明灰色。有呼吸脉冲，透明度在 A 基础上波动。"));

            ThreatRingAttackingColor = Config.Bind("06.UI.01_ThreatRing", "ThreatRingAttackingColor", "255,60,60,200",
                Loc.CfgDesc("06.UI.01_ThreatRing", "ThreatRingAttackingColor",
                    "攻击状态颜色。格式 R,G,B,A。A=透明度 0~255，0 全透明，255 不透明。\n"
                    + "默认 255,60,60,200 = 微透明红色。有闪烁，透明度在 A 基础上波动。"));

            ThreatRingShowOnAttack = Config.Bind("06.UI.01_ThreatRing", "ThreatRingShowOnAttack", true,
                Loc.CfgDesc("06.UI.01_ThreatRing", "ThreatRingShowOnAttack",
                    "是否显示攻击起手提示（红色闪烁）。\n"
                    + "关闭则所有三角形都显示为灰色锁定状态。"));

            // ---- 06.UI.02_Dialogue ----
            DialogueCtrlSkipEnabled = Config.Bind("06.UI.02_Dialogue", "DialogueCtrlSkipEnabled", true,
                Loc.CfgDesc("06.UI.02_Dialogue", "DialogueCtrlSkipEnabled",
                    "按住 Ctrl 键快速跳过对话。\n"
                    + "按住时每 X 秒自动推进一次（显示全部文字 / 下一句）。\n"
                    + "支持事件系统对话和 NPC 对话。"));

            DialogueCtrlSkipIntervalSec = Config.Bind("06.UI.02_Dialogue", "DialogueCtrlSkipIntervalSec", 0.05f,
                Loc.CfgDesc("06.UI.02_Dialogue", "DialogueCtrlSkipIntervalSec",
                    "每次跳过的间隔（秒）。\n"
                    + "0.05 = 每秒跳 20 次（推荐）。\n"
                    + "0.02 = 极快，可能错过内容。\n"
                    + "0.1  = 较慢，能看到每句开头。"));

            DialogueCtrlHintEnabled = Config.Bind("06.UI.02_Dialogue", "DialogueCtrlHintEnabled", true,
                Loc.CfgDesc("06.UI.02_Dialogue", "DialogueCtrlHintEnabled", "在对话框左下角显示 \"Ctrl →\" 快进提示。"));

            DialogueCtrlHintText = Config.Bind("06.UI.02_Dialogue", "DialogueCtrlHintText", "Ctrl →",
                Loc.CfgDesc("06.UI.02_Dialogue", "DialogueCtrlHintText",
                    "提示文字内容。\n"
                    + "可改为 '按 Ctrl 快进' / 'Ctrl' 等。"));

            DialogueCtrlHintColor = Config.Bind("06.UI.02_Dialogue", "DialogueCtrlHintColor", "200,200,200,200",
                Loc.CfgDesc("06.UI.02_Dialogue", "DialogueCtrlHintColor", "提示文字颜色 R,G,B,A。默认灰白 半透明。"));

            DialogueCtrlHintBorderColor = Config.Bind("06.UI.02_Dialogue", "DialogueCtrlHintBorderColor", "20,20,20,200",
                Loc.CfgDesc("06.UI.02_Dialogue", "DialogueCtrlHintBorderColor", "提示文字边框颜色 R,G,B,A。A=0 则不画边框。"));

            // ---- 06.UI.03_Hotkeys ----
            ConfirmHotkeyEnabled = Config.Bind("06.UI.03_Hotkeys", "ConfirmHotkeyEnabled", true,
                Loc.CfgDesc("06.UI.03_Hotkeys", "ConfirmHotkeyEnabled",
                    "快捷确认。\n"
                    + "在弹窗中按下【游戏内互动键】（默认 E，可在游戏按键设置中修改）即可直接确认（与回车按键效果一致）。"));

            QuantityHotkeyEnabled = Config.Bind("06.UI.03_Hotkeys", "QuantityHotkeyEnabled", true,
                Loc.CfgDesc("06.UI.03_Hotkeys", "QuantityHotkeyEnabled",
                    "数量弹窗移动键调数量。\n"
                    + "在数量选择窗口中，可用游戏内【移动键】快捷调整数量。"));

            // ============================================================
            // 07.Minimap
            // ============================================================

            // ---- 07.Minimap.01_Core ----
            MinimapIconEnabled = Config.Bind("07.Minimap.01_Core", "MinimapIconEnabled", true,
                Loc.CfgDesc("07.Minimap.01_Core", "MinimapIconEnabled",
                    "小地图上显示敌人和友军图标。\n"
                    + "true = 开启；false = 关闭。"));

            MinimapIconAllyColor = Config.Bind("07.Minimap.01_Core", "MinimapIconAllyColor", "50,170,50",
                Loc.CfgDesc("07.Minimap.01_Core", "MinimapIconAllyColor", "友军颜色（暗绿）。"));

            // ---- 07.Minimap.02_Resource ----
            MinimapResourceEnabled = Config.Bind("07.Minimap.02_Resource", "MinimapResourceEnabled", true,
                Loc.CfgDesc("07.Minimap.02_Resource", "MinimapResourceEnabled", "小地图显示采集资源（木头/矿石/植物）。"));

            MinimapLogColor = Config.Bind("07.Minimap.02_Resource", "MinimapLogColor", "60,220,80",
                Loc.CfgDesc("07.Minimap.02_Resource", "MinimapLogColor", "木头颜色（亮绿）。"));

            MinimapOreColor = Config.Bind("07.Minimap.02_Resource", "MinimapOreColor", "60,160,240",
                Loc.CfgDesc("07.Minimap.02_Resource", "MinimapOreColor", "矿石颜色（天蓝）。"));

            MinimapPlantColor = Config.Bind("07.Minimap.02_Resource", "MinimapPlantColor", "120,240,200",
                Loc.CfgDesc("07.Minimap.02_Resource", "MinimapPlantColor", "植物颜色（薄荷）。"));

            // ---- 07.Minimap.03_Chest ----
            MinimapChestEnabled = Config.Bind("07.Minimap.03_Chest", "MinimapChestEnabled", true,
                Loc.CfgDesc("07.Minimap.03_Chest", "MinimapChestEnabled", "小地图显示未开启的宝箱。"));

            MinimapChestColor = Config.Bind("07.Minimap.03_Chest", "MinimapChestColor", "230,170,50",
                Loc.CfgDesc("07.Minimap.03_Chest", "MinimapChestColor", "宝箱颜色 R,G,B。木棕。"));

            // ---- 07.Minimap.04_SkillPoint ----
            MinimapSkillPointEnabled = Config.Bind("07.Minimap.04_SkillPoint", "MinimapSkillPointEnabled", true,
                Loc.CfgDesc("07.Minimap.04_SkillPoint", "MinimapSkillPointEnabled", "小地图显示未领取的技能点闪光。"));

            MinimapSkillPointColor = Config.Bind("07.Minimap.04_SkillPoint", "MinimapSkillPointColor", "255,255,255",
                Loc.CfgDesc("07.Minimap.04_SkillPoint", "MinimapSkillPointColor", "技能点颜色 R,G,B。纯白。"));

            MinimapSkillPointLockedColor = Config.Bind("07.Minimap.04_SkillPoint", "MinimapSkillPointLockedColor", "255,60,60",
                Loc.CfgDesc("07.Minimap.04_SkillPoint", "MinimapSkillPointLockedColor",
                    "技能点被怪锁定时（红球状态）的边框颜色 R,G,B。\n"
                    + "白色主体 + 此边框色。\n"
                    + "默认亮红 (255,60,60)。"));

            MinimapSkillPointGoldColor = Config.Bind("07.Minimap.04_SkillPoint", "MinimapSkillPointGoldColor", "255,210,60",
                Loc.CfgDesc("07.Minimap.04_SkillPoint", "MinimapSkillPointGoldColor", "技能点金边颜色 R,G,B。"));

            // ---- 07.Minimap.05_EnemyLevel ----
            MinimapEnemyLevelColorEnabled = Config.Bind("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelColorEnabled", true,
                Loc.CfgDesc("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelColorEnabled", "敌人图标按等级差变色（低=灰白，接近=暗红，高=品红）。"));

            MinimapEnemyLevelLowColor = Config.Bind("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelLowColor", "255,250,120",
                Loc.CfgDesc("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelLowColor", "低级敌人颜色（亮黄）。"));

            MinimapEnemyLevelNearColor = Config.Bind("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelNearColor", "255,150,0",
                Loc.CfgDesc("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelNearColor", "同级敌人颜色（亮橙）。"));

            MinimapEnemyLevelHighColor = Config.Bind("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelHighColor", "200,0,255",
                Loc.CfgDesc("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelHighColor", "高级敌人颜色（亮紫）。"));

            MinimapEnemyLevelLowThreshold = Config.Bind("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelLowThreshold", 6,
                Loc.CfgDesc("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelLowThreshold", "玩家等级 - 敌人等级 ≥ 此值时视为低等级敌人。"));

            MinimapEnemyLevelHighThreshold = Config.Bind("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelHighThreshold", 6,
                Loc.CfgDesc("07.Minimap.05_EnemyLevel", "MinimapEnemyLevelHighThreshold", "敌人等级 - 玩家等级 ≥ 此值时视为高等级敌人。"));

            // ---- 07.Minimap.06_Border ----
            AngryBorderEnabled = Config.Bind("07.Minimap.06_Border", "AngryBorderEnabled", true,
                Loc.CfgDesc("07.Minimap.06_Border", "AngryBorderEnabled",
                    "敌人进入愤怒状态（angry）时，小地图图标外边框变为红色。\n"
                    + "  默认颜色 = 正常\n"
                    + "  红色     = 愤怒中（战斗状态）\n"
                    + "实时更新：进入/退出愤怒时立即刷新。"));

            AngryBorderColor = Config.Bind("07.Minimap.06_Border", "AngryBorderColor", "255,60,60",
                Loc.CfgDesc("07.Minimap.06_Border", "AngryBorderColor",
                    "愤怒状态下的边框颜色 R,G,B。\n"
                    + "默认亮红，比普通边框更醒目。"));

            // ---- 07.Minimap.07_Fade ----
            MinimapFadeEnabled = Config.Bind("07.Minimap.07_Fade", "MinimapFadeEnabled", true,
                Loc.CfgDesc("07.Minimap.07_Fade", "MinimapFadeEnabled", "图标按距离渐隐。远处图标变淡，近处全亮。"));

            MinimapResourceFadeStart = Config.Bind("07.Minimap.07_Fade", "MinimapResourceFadeStart", 50f,
                Loc.CfgDesc("07.Minimap.07_Fade", "MinimapResourceFadeStart", "资源/宝箱/技能点的渐隐起始距离（米）。小于此距离完全不透明。"));

            MinimapResourceFadeEnd = Config.Bind("07.Minimap.07_Fade", "MinimapResourceFadeEnd", 100f,
                Loc.CfgDesc("07.Minimap.07_Fade", "MinimapResourceFadeEnd", "资源/宝箱/技能点的渐隐结束距离（米）。大于此距离完全透明（看不见）。"));

            MinimapEnemyFadeStart = Config.Bind("07.Minimap.07_Fade", "MinimapEnemyFadeStart", 30f,
                Loc.CfgDesc("07.Minimap.07_Fade", "MinimapEnemyFadeStart", "敌人的渐隐起始距离（米）。"));

            MinimapEnemyFadeEnd = Config.Bind("07.Minimap.07_Fade", "MinimapEnemyFadeEnd", 60f,
                Loc.CfgDesc("07.Minimap.07_Fade", "MinimapEnemyFadeEnd", "敌人的渐隐结束距离（米）。"));

            // ---- 07.Minimap.08_Visibility ----
            HideMinimapOnQuit = Config.Bind("07.Minimap.08_Visibility", "HideMinimapOnQuit", true,
                Loc.CfgDesc("07.Minimap.08_Visibility", "HideMinimapOnQuit",
                    "退出到标题时隐藏小地图相机，修复原版地图闪烁问题。\n"
                    + "true = 退出时不闪；false = 保持原版行为。"));

            HideMinimapOnCutscene = Config.Bind("07.Minimap.08_Visibility", "HideMinimapOnCutscene", true,
                Loc.CfgDesc("07.Minimap.08_Visibility", "HideMinimapOnCutscene",
                    "过场动画期间隐藏小地图。\n"
                    + "检测信号：玩家血条被隐藏（原版过场动画期间会隐藏血条和玩家 HUD）。\n"
                    + "true = 过场期间强制隐藏小地图和任务指针；过场结束后恢复玩家之前的状态。\n"
                    + "false = 保持原版行为（过场动画期间小地图仍然显示）。"));

            // ============================================================
            // 08.Ecology
            // ============================================================

            // ---- 08.Ecology.01_Core ----
            CrossSaveShare = Config.Bind("08.Ecology.01_Core", "CrossSaveShare", true,
                Loc.CfgDesc("08.Ecology.01_Core", "CrossSaveShare",
                    "生态存档共享模式。\n"
                    + "  true  = 共通模式：所有角色共享同一份生态数据和时间轴\n"
                    + "  false = 独立模式：每个存档槽位单独记录\n"
                    + "\n"
                    + "切换模式不会迁移数据。\n"
                    + "从 A 模式切到 B 模式，各自保留自己上一次的记录。\n"
                    + "切回 A 模式时，看到的是 A 模式最后保存的状态。"));

            EcologyEnabled = Config.Bind("08.Ecology.01_Core", "EcologyEnabled", true,
                Loc.CfgDesc("08.Ecology.01_Core", "EcologyEnabled",
                    "生态记忆系统总开关。\n"
                    + "  true  = 击杀会减少区域怪物密度，随时间恢复\n"
                    + "  false = 保持原版（击杀后固定重生）"));

            EcologyQuestBypass = Config.Bind("08.Ecology.01_Core", "EcologyQuestBypass", true,
                Loc.CfgDesc("08.Ecology.01_Core", "EcologyQuestBypass",
                    "任务区域豁免生态过滤。\n"
                    + "刷怪点半径内存在 MinimapCamera.objective（当前主线任务目标）时豁免过滤。\n"
                    + "保证剧情任务所需敌人在低生态下也能正常刷新。"));

            EcologyRecoveryPerHour = Config.Bind("08.Ecology.01_Core", "EcologyRecoveryPerHour", 1f,
                Loc.CfgDesc("08.Ecology.01_Core", "EcologyRecoveryPerHour",
                    "生态每小时恢复的怪物密度百分比。\n"
                    + "  1 = 每小时恢复 1%（默认）\n"
                    + "  0 = 不恢复\n"
                    + "支持小数。需要切换地图怪物密度才能恢复。"));

            EcologyDelayPerKillMinutes = Config.Bind("08.Ecology.01_Core", "EcologyDelayPerKillMinutes", 5f,
                Loc.CfgDesc("08.Ecology.01_Core", "EcologyDelayPerKillMinutes",
                    "每次击杀后暂停恢复的分钟数。\n"
                    + "  5 = 击杀后 5 分钟内不恢复（默认）\n"
                    + "  0 = 击杀不影响恢复\n"
                    + "暂停时间会累加。例：杀 10 只怪 → 暂停恢复 50 分钟。\n"
                    + "支持小数。"));

            EcologyKillDropPercent = Config.Bind("08.Ecology.01_Core", "EcologyKillDropPercent", 0.5f,
                Loc.CfgDesc("08.Ecology.01_Core", "EcologyKillDropPercent",
                    "每次击杀扣减的怪物密度百分比。\n"
                    + "  0.5 = 每杀一只扣 0.5%（默认，200 只刷到 0%）\n"
                    + "  1   = 每杀一只扣 1%（100 只刷到 0%）\n"
                    + "支持小数。"));

            EcologyMinPopulation = Config.Bind("08.Ecology.01_Core", "EcologyMinPopulation", 3f,
                Loc.CfgDesc("08.Ecology.01_Core", "EcologyMinPopulation",
                    "怪物密度下限。\n"
                    + "  0 = 可以刷到 0%（该区域不再生成怪）\n"
                    + "  20 = 最低保持 20% 生成率\n"
                    + "支持小数。"));

            EcologyPlayerDeathRecovery = Config.Bind("08.Ecology.01_Core", "EcologyPlayerDeathRecovery", 10f,
                Loc.CfgDesc("08.Ecology.01_Core", "EcologyPlayerDeathRecovery",
                    "玩家死亡时，当前区域恢复的怪物密度百分比。\n"
                    + "  0 = 关闭（死亡不影响生态）\n"
                    + "  25 = 死亡后当前区域立即恢复 25%（默认）\n"
                    + "与恢复倒计时无关，直接生效。\n"
                    + "支持小数。"));

            // ---- 08.Ecology.02_UI ----
            EcologyUIEnabled = Config.Bind("08.Ecology.02_UI", "EcologyUIEnabled", true,
                Loc.CfgDesc("08.Ecology.02_UI", "EcologyUIEnabled",
                    "生态 UI 显示开关。\n"
                    + "显示内容：\n"
                    + "  左上角时间旁：累计探索时长\n"
                    + "  小地图上方：当前区域怪物密度进度条\n"
                    + "  小地图上方：恢复暂停倒计时（仅暂停期间显示）\n"
                    + "教程场景自动隐藏。\n"
                    + "小地图关闭时进度条也一起隐藏。"));

            // ---- 08.Ecology.03_DeathTime ----
            DeathTimeSkipHours = Config.Bind("08.Ecology.03_DeathTime", "DeathTimeSkipHours", 12f,
                Loc.CfgDesc("08.Ecology.03_DeathTime", "DeathTimeSkipHours",
                    "玩家死亡复活后跳过多少游戏小时。\n"
                    + "生态恢复和暂停倒计时会一起消耗。\n"
                    + "12 = 死后直接推进 12 小时（推荐）。支持小数，0 = 关闭。"));

            DeathTimeSkipUIEnabled = Config.Bind("08.Ecology.03_DeathTime", "DeathTimeSkipUIEnabled", true,
                Loc.CfgDesc("08.Ecology.03_DeathTime", "DeathTimeSkipUIEnabled", "是否显示 'X HOURS LATER......' 迷因风格提示。"));

            DeathTimeSkipUITextTemplate = Config.Bind("08.Ecology.03_DeathTime", "DeathTimeSkipUITextTemplate", "{0} LATER......",
                Loc.CfgDesc("08.Ecology.03_DeathTime", "DeathTimeSkipUITextTemplate",
                    "{0} 是自动格式化的时长（如 '12 HOURS' / '1 DAY' / '1 DAY 12 HOURS' / '1 HOUR 30 MINUTES'）。\n"
                    + "默认英文迷因风格。想改中文可填 '{0} 后……'。"));

            DeathTimeSkipUIFontSize = Config.Bind("08.Ecology.03_DeathTime", "DeathTimeSkipUIFontSize", 26,
                Loc.CfgDesc("08.Ecology.03_DeathTime", "DeathTimeSkipUIFontSize", "字号。"));

            DeathTimeSkipUICenterYPercent = Config.Bind("08.Ecology.03_DeathTime", "DeathTimeSkipUICenterYPercent", 85,
                Loc.CfgDesc("08.Ecology.03_DeathTime", "DeathTimeSkipUICenterYPercent", "屏幕垂直位置百分比。50 = 正中，85 = 偏下。"));

            // ============================================================
            // 09.World
            // ============================================================

            // ---- 09.World.01_TimeCost ----
            MapExitCostHours = Config.Bind("09.World.01_TimeCost", "MapExitCostHours", 3f,
                Loc.CfgDesc("09.World.01_TimeCost", "MapExitCostHours",
                    "走地图出入口（场景切换）消耗的游戏小时数。\n"
                    + "  0 = 不消耗\n"
                    + "  3 = 3 游戏小时（默认）\n"
                    + "支持小数。"));

            TeleporterCostMinutes = Config.Bind("09.World.01_TimeCost", "TeleporterCostMinutes", 30f,
                Loc.CfgDesc("09.World.01_TimeCost", "TeleporterCostMinutes",
                    "使用传送阵消耗的游戏分钟数。\n"
                    + "  0   = 不消耗\n"
                    + "  30  = 30 游戏分钟（默认）"));

            // ============================================================
            // 10.Permadeath
            // ============================================================

            // ---- 10.Permadeath.01_UI ----
            PermadeathShowInCharacterCreation = Config.Bind("10.Permadeath.01_UI", "PermadeathShowInCharacterCreation", true,
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathShowInCharacterCreation",
                    "是否在创角界面显示永久死亡模式的勾选框。\n"
                    + "false = 完全隐藏入口，功能关闭。"));

            PermadeathDeathText = Config.Bind("10.Permadeath.01_UI", "PermadeathDeathText", "YOU DIED",
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathDeathText", "死亡时显示的文字。"));

            PermadeathDeathTextFontSize = Config.Bind("10.Permadeath.01_UI", "PermadeathDeathTextFontSize", 120,
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathDeathTextFontSize", "死亡文字字号。"));

            PermadeathDeathTextColor = Config.Bind("10.Permadeath.01_UI", "PermadeathDeathTextColor", "200,0,0",
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathDeathTextColor", "死亡文字颜色 R,G,B。默认深红。"));

            PermadeathDeathTextBorderColor = Config.Bind("10.Permadeath.01_UI", "PermadeathDeathTextBorderColor", "80,0,0",
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathDeathTextBorderColor", "死亡文字描边颜色 R,G,B。默认暗红。"));

            PermadeathCountdownSec = Config.Bind("10.Permadeath.01_UI", "PermadeathCountdownSec", 9f,
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathCountdownSec",
                    "死亡界面停留秒数。\n"
                    + "期间 YOU DIED 文字淡入淡出。\n"
                    + "期间玩家强制退出游戏能保留存档。\n"
                    + "默认 9 秒。"));

            PermadeathButtonSymbol = Config.Bind("10.Permadeath.01_UI", "PermadeathButtonSymbol", "☠",
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathButtonSymbol",
                    "按钮显示的符号。默认骷髅 ☠。\n"
                    + "如果显示成方块，可以换成 'X' / 'RIP' / '†' 等。"));

            PermadeathButtonOffBgColor = Config.Bind("10.Permadeath.01_UI", "PermadeathButtonOffBgColor", "47,47,47",
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathButtonOffBgColor", "未选中时底色 R,G,B。默认深灰（和游戏 Tab 黑方块一致）。"));

            PermadeathButtonOffTextColor = Config.Bind("10.Permadeath.01_UI", "PermadeathButtonOffTextColor", "240,240,240",
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathButtonOffTextColor", "未选中时符号颜色 R,G,B。默认白。"));

            PermadeathButtonOnBgColor = Config.Bind("10.Permadeath.01_UI", "PermadeathButtonOnBgColor", "200,60,60",
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathButtonOnBgColor", "已选中时底色 R,G,B。默认红。"));

            PermadeathButtonOnTextColor = Config.Bind("10.Permadeath.01_UI", "PermadeathButtonOnTextColor", "20,20,20",
                Loc.CfgDesc("10.Permadeath.01_UI", "PermadeathButtonOnTextColor", "已选中时符号颜色 R,G,B。默认黑（反色）。"));

            // ============================================================
            // 99.Debug
            // ============================================================

            // ---- 99.Debug.01_Log ----
            DebugLogEnabled = Config.Bind("99.Debug.01_Log", "DebugLogEnabled", false,
                Loc.CfgDesc("99.Debug.01_Log", "DebugLogEnabled",
                    "输出详细调试日志到 BepInEx/LogOutput.log。\n"
                    + "  false = 安静（默认）\n"
                    + "  true  = 输出击杀 / 切图 / 生态读写等详细日志\n"
                    + "排查问题时开启，正常游玩请保持 false。"));

            // ---- 99.Debug.02_Test ----
            TestCritBonusEnabled = Config.Bind("99.Debug.02_Test", "TestCritBonusEnabled", false,
                Loc.CfgDesc("99.Debug.02_Test", "TestCritBonusEnabled", "测试用：是否给玩家的攻击附加额外暴击率。正式游玩时请保持 false"));

            TestCritBonus = Config.Bind("99.Debug.02_Test", "TestCritBonus", 100,
                Loc.CfgDesc("99.Debug.02_Test", "TestCritBonus", "测试用：附加的暴击率数值。100 表示 +100% 暴击率"));

            TestExpMulEnabled = Config.Bind("99.Debug.02_Test", "TestExpMulEnabled", false,
                Loc.CfgDesc("99.Debug.02_Test", "TestExpMulEnabled", "测试用：是否启用经验倍率。正式游玩时请保持 false"));

            TestExpMul = Config.Bind("99.Debug.02_Test", "TestExpMul", 10,
                Loc.CfgDesc("99.Debug.02_Test", "TestExpMul", "测试用：经验倍率（1~1000）。原始经验 × 此值"));

            ExpShowMultiplierBracket = Config.Bind("99.Debug.02_Test", "ExpShowMultiplierBracket", true,
                Loc.CfgDesc("99.Debug.02_Test", "ExpShowMultiplierBracket", "经验弹字是否显示倍率括号。true = 显示 EXP +5000 (x1000)；false = 只显示 EXP +5000"));

            TestMoneyEnabled = Config.Bind("99.Debug.02_Test", "TestMoneyEnabled", false,
                Loc.CfgDesc("99.Debug.02_Test", "TestMoneyEnabled",
                    "测试用：是否启用金钱热键。\n"
                    + "按热键直接给玩家加钱，方便测试商店 / 购买 / 容量相关功能。\n"
                    + "正式游玩请保持 false。"));

            TestMoneyKey = Config.Bind("99.Debug.02_Test", "TestMoneyKey", "P",
                Loc.CfgDesc("99.Debug.02_Test", "TestMoneyKey",
                    "测试用：金钱热键。填写 KeyCode 名称，区分大小写不敏感。\n"
                    + "常用值：P /PageUp / PageDown 默认 P。"));

            TestMoneyAmount = Config.Bind("99.Debug.02_Test", "TestMoneyAmount", 100000000,
                Loc.CfgDesc("99.Debug.02_Test", "TestMoneyAmount",
                    "测试用：每次按热键增加的金钱数量。\n"
                    + "默认 1 亿。"));

            // ---- 99.Debug.03_ChargeStun ----
            ChargeStunDebugLog = Config.Bind("99.Debug.03_ChargeStun", "ChargeStunDebugLog", false,
                Loc.CfgDesc("99.Debug.03_ChargeStun", "ChargeStunDebugLog",
                    "输出冲锋自晕调试日志到 BepInEx/LogOutput.log。\n"
                    + "开启后每次冲锋会记录进入/退出/起步窗口/触发/状态。\n"
                    + "仅在诊断距离敏感问题期间开启，正常游玩请关闭。"));

            // ============================================================
            // UI 挂载（保持不变）
            // ============================================================
            var uiObj = new GameObject("ED3R_ComboUI");
            GameObject.DontDestroyOnLoad(uiObj);
            uiObj.AddComponent<ComboUI>();

            var alertObj = new GameObject("ED3R_AlertUI");
            GameObject.DontDestroyOnLoad(alertObj);
            alertObj.AddComponent<AlertUI>();

            var threatRingObj = new GameObject("ED3R_ThreatRingUI");
            GameObject.DontDestroyOnLoad(threatRingObj);
            threatRingObj.AddComponent<ThreatRingUI>();

            var sleepHintObj = new GameObject("ED3R_SleepHintUI");
            GameObject.DontDestroyOnLoad(sleepHintObj);
            sleepHintObj.AddComponent<SleepHintUI>();

            var ecologyUIObj = new GameObject("ED3R_EcologyUI");
            GameObject.DontDestroyOnLoad(ecologyUIObj);
            ecologyUIObj.AddComponent<EcologyUI>();

            var deathTimeSkipUIObj = new GameObject("ED3R_DeathTimeSkipUI");
            GameObject.DontDestroyOnLoad(deathTimeSkipUIObj);
            deathTimeSkipUIObj.AddComponent<DeathTimeSkipUI>();

            var permadeathUIObj = new GameObject("ED3R_PermadeathDeathUI");
            GameObject.DontDestroyOnLoad(permadeathUIObj);
            permadeathUIObj.AddComponent<PermadeathDeathUI>();

            var permadeathBtnObj = new GameObject("ED3R_PermadeathButtonUI");
            GameObject.DontDestroyOnLoad(permadeathBtnObj);
            permadeathBtnObj.AddComponent<PermadeathButtonUI>();

            var cutsceneMinimapHiderObj = new GameObject("ED3R_CutsceneMinimapHider");
            GameObject.DontDestroyOnLoad(cutsceneMinimapHiderObj);
            cutsceneMinimapHiderObj.AddComponent<CutsceneMinimapHider>();

            var refreshObj = new GameObject("ED3R_MinimapRefresher");
            GameObject.DontDestroyOnLoad(refreshObj);
            refreshObj.AddComponent<MinimapIconRefresher>();

            var skillPointScannerObj = new GameObject("ED3R_SkillPointIconScanner");
            GameObject.DontDestroyOnLoad(skillPointScannerObj);
            skillPointScannerObj.AddComponent<SkillPointIconScanner>();

            var dialogueHintObj = new GameObject("ED3R_DialogueCtrlHintUI");
            GameObject.DontDestroyOnLoad(dialogueHintObj);
            dialogueHintObj.AddComponent<DialogueCtrlHintUI>();

            var playerLightObj = new GameObject("ED3R_PlayerLight");
            GameObject.DontDestroyOnLoad(playerLightObj);
            playerLightObj.AddComponent<PlayerLightController>();

            var testMoneyObj = new GameObject("ED3R_TestMoneyHotkey");
            GameObject.DontDestroyOnLoad(testMoneyObj);
            testMoneyObj.AddComponent<TestMoneyHotkey>();

            var harmony = new Harmony("com.SoulDRoul.EternalDread3Rmod");
            harmony.PatchAll();
            Log.LogInfo("Mod loaded!");

        }
    }
}
