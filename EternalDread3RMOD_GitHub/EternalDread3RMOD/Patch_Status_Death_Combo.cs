using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Status), "Death")]
    public static class Patch_Status_Death_Combo
    {
        // ============ 玩家 Status 缓存 ============
        // 避免每次敌人死亡都 GetComponent<Status>()。
        // 玩家对象变化（换存档 / 重开局）时引用变化，缓存自动失效。
        private static GameObject _cachedPlayerObj;
        private static Status _cachedPlayerStat;

        static void Prefix(Status __instance)
        {
            if (!Plugin.ComboEnabled.Value) return;
            if (__instance.dead) return;

            var mainPlayer = GlobalVar.mainPlayer;
            if (mainPlayer == null) return;
            if (__instance.gameObject == mainPlayer) return;

            // 只统计敌人
            if (__instance.gameObject.tag != "Enemy") return;

            // 玩家对象变化时才重新查 Status
            if (!ReferenceEquals(_cachedPlayerObj, mainPlayer))
            {
                _cachedPlayerObj = mainPlayer;
                _cachedPlayerStat = mainPlayer.GetComponent<Status>();
            }
            var playerStat = _cachedPlayerStat;
            if (playerStat == null) return;

            // 等级过滤
            int diff = __instance.level - playerStat.level;
            if (diff < Plugin.ComboMinEnemyLevelDiff.Value)
            {
                ComboState.ClearEnemyRecord(__instance);
                return;
            }

            // 玩家伤害过滤
            float window = Plugin.ComboPlayerDamageWindowMs.Value / 1000f;
            if (!ComboState.WasDamagedByPlayer(__instance, window))
            {
                ComboState.ClearEnemyRecord(__instance);
                return;
            }

            // 计数
            ComboState.comboCount++;
            Plugin.DebugLog("[Combo] kill counted, combo=" + ComboState.comboCount);
            ComboState.comboEndTime = ComboState.gameTime + Plugin.ComboWindowMs.Value / 1000f;
            ComboState.killsSinceLastReward++;
            ComboState.ClearEnemyRecord(__instance);

            // HIT 奖励开启时，击杀奖励失效
            if (Plugin.HitRewardEvery.Value <= 0)
            {
                if (ComboState.killsSinceLastReward >= Plugin.StaminaRewardEvery.Value)
                {
                    ComboState.killsSinceLastReward = 0;
                    GiveStaminaReward(playerStat);
                }
            }
        }

        private static void GiveStaminaReward(Status playerStat)
        {
            var input = playerStat.GetComponent<PlayerInputController>();
            if (input == null) return;

            float max = input.maxStamina;
            float amount = max * Plugin.StaminaRewardPercent.Value / 100f;
            input.stamina = Mathf.Min(input.stamina + amount, max);

            ComboUI.ShowRewardText((int)amount);

        }
    }
}