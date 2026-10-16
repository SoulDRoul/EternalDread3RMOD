using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(GainEXP), "Start")]
    public static class Patch_GainEXP_Start
    {
        static void Prefix(GainEXP __instance)
        {
            ExpContext.EnemyLevel = __instance.realLv > 0 ? __instance.realLv : 1;
        }

        static void Postfix(GainEXP __instance)
        {
            var dp = __instance.GetComponent<DamagePopup>();
            if (dp == null) return;

            string dmg = dp.damage;
            if (string.IsNullOrEmpty(dmg)) return;
            if (!dmg.StartsWith("EXP +")) return;

            int baseVal;
            if (!int.TryParse(dmg.Substring(5), out baseVal)) return;

            // ===== 独立重算实际到账值 =====
            long gain = baseVal;

            // 1. 测试倍率
            int testMul = 1;
            bool testOn = Plugin.TestExpMulEnabled.Value;
            if (testOn)
            {
                testMul = Plugin.TestExpMul.Value;
                if (testMul < 1) testMul = 1;
                if (testMul > 1000) testMul = 1000;
                gain = gain * testMul;
            }

            // 2. 等级补正
            float levelMul = 1f;
            bool levelOn = Plugin.LevelBonusEnabled.Value;
            Status playerStat = GlobalVar.mainPlayer != null
                ? GlobalVar.mainPlayer.GetComponent<Status>()
                : null;

            if (levelOn && playerStat != null)
            {
                int enemyLevel = __instance.realLv > 0 ? __instance.realLv : 1;
                int diff = enemyLevel - playerStat.level;
                int percent = 100;
                if (diff > 0)
                {
                    percent = 100 + diff * Plugin.LevelExpPercentPerLevel.Value;
                }
                else if (diff < 0)
                {
                    percent = 100 - (-diff) * Plugin.LevelExpNegativePercentPerLevel.Value;
                    int minPercent = Plugin.LevelExpMinPercent.Value;
                    if (percent < minPercent) percent = minPercent;
                }
                levelMul = percent / 100f;
                gain = gain * percent / 100;
            }

            // 3. wisdom 被动
            bool wisdom = playerStat != null && playerStat.ability.wisdom;
            if (wisdom) gain *= 2;

            if (gain > int.MaxValue) gain = int.MaxValue;
            if (gain < 0) gain = 0;

            // ===== 总倍率 + 显示判定 =====
            float totalMul = 1f;
            if (testOn) totalMul *= testMul;
            if (levelOn) totalMul *= levelMul;
            if (wisdom) totalMul *= 2f;

            bool showBracket =
                Plugin.ExpShowMultiplierBracket.Value
                && (testOn
                || (levelOn && Mathf.Abs(levelMul - 1f) > 0.001f)
                || wisdom);

            string newText;
            if (showBracket)
            {
                newText = "EXP +" + gain + " (x" + FormatMul(totalMul) + ")";
            }
            else
            {
                newText = "EXP +" + gain;
            }

            if (newText == dmg) return;

            dp.damage = newText;

            // Canvas 模式：TMP 需要手动刷新
            if (dp.canvasSetting != null
                && dp.canvasSetting.useCanvas
                && dp.canvasSetting.damageText != null)
            {
                dp.canvasSetting.damageText.text = newText;
            }
        }

        private static string FormatMul(float mul)
        {
            // 整数倍率显示整数（x1000），小数倍率最多 2 位去尾零（x0.5, x1.25）
            return mul.ToString("0.##",
                System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}