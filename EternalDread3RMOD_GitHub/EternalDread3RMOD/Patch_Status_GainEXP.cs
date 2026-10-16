using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Status), "GainEXP")]
    public static class Patch_Status_GainEXP
    {
        static void Prefix(Status __instance, ref int gain)
        {
            if (__instance.gameObject != GlobalVar.mainPlayer) return;
            if (!Plugin.LevelBonusEnabled.Value) return;

            int diff = ExpContext.EnemyLevel - __instance.level;
            if (diff > 0)
            {
                // 敌人等级高于玩家，加经验
                int percent = 100 + diff * Plugin.LevelExpPercentPerLevel.Value;
                long tmp = (long)gain * percent / 100;
                if (tmp > int.MaxValue) tmp = int.MaxValue;
                gain = (int)tmp;
            }
            else if (diff < 0)
            {
                // 玩家等级高于敌人，减经验
                int percent = 100 - (-diff) * Plugin.LevelExpNegativePercentPerLevel.Value;
                int minPercent = Plugin.LevelExpMinPercent.Value;
                if (percent < minPercent) percent = minPercent;
                long tmp = (long)gain * percent / 100;
                if (tmp > int.MaxValue) tmp = int.MaxValue;
                gain = (int)tmp;
                if (gain < 1) gain = 1;
            }
        }
    }
}