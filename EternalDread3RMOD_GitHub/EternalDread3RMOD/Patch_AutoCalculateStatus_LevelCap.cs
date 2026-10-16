using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(AutoCalculateStatus), "CalculateStatLv")]
    public static class Patch_AutoCalculateStatus_LevelCap
    {
        private static bool _inRecalc = false;

        static void Postfix(AutoCalculateStatus __instance)
        {
            if (_inRecalc) return;

            var stat = __instance.GetComponent<Status>();
            if (stat == null) return;
            if (stat.gameObject.tag == "Player") return;
            if (stat.gameObject.tag == "Ally") return;

            int native = stat.defaultLevel;
            if (native <= 0) native = stat.level;
            if (native <= 0) return;

            int scale = GetScale(GlobalVar.difficultLv);
            if (scale < 1) scale = 1;
            long cap = (long)native * scale;
            if (cap > __instance.maxLevel) cap = __instance.maxLevel;

            int playerLevel = native;
            if (GlobalVar.mainPlayer != null)
            {
                var ps = GlobalVar.mainPlayer.GetComponent<Status>();
                if (ps != null) playerLevel = ps.level;
            }

            int target = native;
            int clampedPlayer = playerLevel < cap ? playerLevel : (int)cap;
            if (clampedPlayer > target) target = clampedPlayer;

            if (GlobalVar.time == TheTimes.Night)
            {
                int bonus = GetNightBonus(GlobalVar.difficultLv);
                if (bonus > 0)
                {
                    int threshold = Plugin.NightLevelSkipThreshold.Value;
                    if (threshold <= 0 || native < playerLevel + threshold)
                    {
                        target += bonus;
                    }
                }
            }

            if (target > __instance.maxLevel) target = __instance.maxLevel;
            if (target < 1) target = 1;

            if (stat.level == target) return;

            stat.level = target;

            // 重算属性：临时禁用所有会改等级的逻辑
            _inRecalc = true;
            bool savedArea = __instance.getLevelArea;
            bool savedEqual = __instance.levelEqualPlayer;
            int savedDiff = GlobalVar.difficultLv;

            __instance.getLevelArea = false;
            __instance.levelEqualPlayer = false;
            GlobalVar.difficultLv = 0;

            try
            {
                __instance.CalculateStatLv();
            }
            finally
            {
                GlobalVar.difficultLv = savedDiff;
                __instance.levelEqualPlayer = savedEqual;
                __instance.getLevelArea = savedArea;
                _inRecalc = false;
            }
            stat.SetName();
        }

        private static int GetScale(int difficultLv)
        {
            switch (difficultLv)
            {
                case 0: return Plugin.EnemyLevelScale_S0.Value;
                case 1: return Plugin.EnemyLevelScale_S1.Value;
                case 2: return Plugin.EnemyLevelScale_S2.Value;
                case 3: return Plugin.EnemyLevelScale_S3.Value;
                default: return 1;
            }
        }

        private static int GetNightBonus(int difficultLv)
        {
            switch (difficultLv)
            {
                case 0: return Plugin.NightLevelS0.Value;
                case 1: return Plugin.NightLevelS1.Value;
                case 2: return Plugin.NightLevelS2.Value;
                case 3: return Plugin.NightLevelS3.Value;
                default: return 0;
            }
        }
    }
}