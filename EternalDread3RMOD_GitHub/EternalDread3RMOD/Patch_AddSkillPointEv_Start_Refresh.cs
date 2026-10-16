using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 技能点刷新。原版 Start 会销毁 globalBoolean 为 true 的技能点；
    /// 本 patch 在冷却结束、每日配额未满、场景限流未达、概率命中时阻止销毁，让物体保留。
    /// 
    /// 持久化：命中后 IsSpawned = true。该状态不因切图消失，
    /// 玩家再次进入场景时只要 IsSpawned 为 true 就直接保留物体，不再走概率判定。
    /// 只有真正拾取时才清除 IsSpawned 并记录冷却。
    /// </summary>
    [HarmonyPatch(typeof(AddSkillPointEv), "Start")]
    public static class Patch_AddSkillPointEv_Start_Refresh
    {
        private static string _lastScene = null;
        private static float _lastSceneLoadTime = -1f;
        private static int _sceneRefreshCount = 0;

        static bool Prefix(AddSkillPointEv __instance)
        {
            if (!Plugin.SkillPointRefreshEnabled.Value) return true;
            if (__instance == null) return true;

            int boolId = __instance.boolId;
            if (boolId <= 0 || boolId > 42) return true;

            if (!__instance.targetObject) __instance.targetObject = __instance.gameObject;

            // 未拾取过 → 原版保留物体
            if (!EventSetting.globalBoolean[boolId]) return true;

            try
            {
                // 已刷出未拾取 → 直接保留，不走判定
                if (SkillPointRegistry.GetIsSpawned(boolId))
                {
                    Plugin.DebugLog("[ED3R] SkillPoint keep (spawned): boolId=" + boolId);
                    return false;
                }

                float now = GameTimeTracker.GetTotalGameHours();
                float lastPickup = SkillPointRegistry.GetLastPickup(boolId);

                // 旧存档：mod 没记录，视为很久以前拾取
                if (lastPickup < 0f) lastPickup = -999999f;

                // 冷却未过 → 走原版销毁
                float cooldown = Plugin.SkillPointRefreshCooldownHours.Value;
                if (cooldown > 0f && (now - lastPickup) < cooldown) return true;

                // 每日配额已满 → 走原版销毁
                int currentDay = Mathf.FloorToInt(now / 24f);
                int dailyLimit = Plugin.SkillPointRefreshDailyLimit.Value;
                if (!SkillPointRegistry.CanRefreshToday(currentDay, dailyLimit)) return true;

                // 本次场景加载限流：每次切图最多刷 1 个
                string scene = SceneManager.GetActiveScene().name;
                if (scene != _lastScene || now - _lastSceneLoadTime > 5f)
                {
                    _lastScene = scene;
                    _lastSceneLoadTime = now;
                    _sceneRefreshCount = 0;
                }
                if (_sceneRefreshCount >= 1) return true;

                // 概率判定
                var region = EcologyRegistry.GetOrCreate(scene);
                float pop = (region != null) ? region.PopulationPercent : 100f;
                if (pop < 0f) pop = 0f;
                if (pop > 100f) pop = 100f;

                int baseChance = Plugin.SkillPointRefreshBaseChance.Value;
                int bonusMax = Plugin.SP_ECOLOGY_BONUS_MAX;
                int cap = Plugin.SP_CHANCE_CAP;

                float chance = baseChance + (1f - pop / 100f) * bonusMax;
                if (chance > cap) chance = cap;
                if (chance < 0f) chance = 0f;

                int roll = UnityEngine.Random.Range(0, 100);
                if (roll >= chance) return true;

                // 命中：消耗配额、场景计数、回复生态、标记 IsSpawned
                SkillPointRegistry.ConsumeRefreshSlot(currentDay, dailyLimit);
                _sceneRefreshCount++;

                float gain = Plugin.SkillPointRefreshEcologyGain.Value;
                if (gain > 0f && region != null)
                {
                    region.PopulationPercent += gain;
                    if (region.PopulationPercent > 100f) region.PopulationPercent = 100f;
                }

                SkillPointRegistry.SetIsSpawned(boolId, true);

                Plugin.DebugLog("[ED3R] SkillPoint refresh hit: boolId=" + boolId
                    + " scene=" + scene + " pop=" + pop.ToString("F1")
                    + " chance=" + chance.ToString("F1"));

                return false;  // 跳过原版 → 保留物体
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] SkillPointRefresh Start error: " + e);
                return true;
            }
        }
    }
}