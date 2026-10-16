using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(BulletStatus), "DealingDamage")]
    public static class Patch_BulletStatus_MultiCrit
    {
        // ============ 硬编码常量：LevelBonus.MultiCritPopupDelayMs ============
        // 原 LevelBonus.MultiCritPopupDelayMs —— 多重暴击时，每段伤害数字弹出的间隔（毫秒）
        private const int MULTI_CRIT_POPUP_DELAY_MS = 10;

        // Prefix 里保存原始值，Postfix 里用
        private static Dictionary<BulletStatus, int> savedCrit
            = new Dictionary<BulletStatus, int>();
        private static Dictionary<BulletStatus, int> savedDamage
            = new Dictionary<BulletStatus, int>();
        private static Dictionary<BulletStatus, GameObject> savedOther
            = new Dictionary<BulletStatus, GameObject>();
        private static Dictionary<BulletStatus, bool> willCritMap
    = new Dictionary<BulletStatus, bool>();

        // ============ 玩家 Status 缓存 ============
        private static GameObject _cachedPlayerObj;
        private static Status _cachedPlayerStat;

        [HarmonyPriority(Priority.Low)]
        static void Prefix(BulletStatus __instance, GameObject other)
        {
            // 测试加成
            if (Plugin.TestCritBonusEnabled.Value
                && IsPlayerSide(__instance.source))
            {
                __instance.criticalPlus += Plugin.TestCritBonus.Value;
            }

            int realCrit = __instance.criticalPlus;
            savedCrit[__instance] = realCrit;
            savedDamage[__instance] = __instance.totalDamage;
            savedOther[__instance] = other;

            // 自己掷骰子：这次会不会暴击？
            int clamped = realCrit;
            if (clamped > 100) clamped = 100;
            if (clamped < 0) clamped = 0;
            bool willCrit = realCrit >= 1 && Random.Range(0, 100) <= clamped;
            willCritMap[__instance] = willCrit;

            // 强制原版暴击分支按我们的结论走
            __instance.criticalPlus = willCrit ? 100 : 0;
        }

        [HarmonyPriority(Priority.Low)]
        static void Postfix(BulletStatus __instance)
        {
            int realCrit;
            int baseDamage;
            GameObject other;
            if (!savedCrit.TryGetValue(__instance, out realCrit)) return;
            savedDamage.TryGetValue(__instance, out baseDamage);
            savedOther.TryGetValue(__instance, out other);
            savedCrit.Remove(__instance);
            savedDamage.Remove(__instance);
            savedOther.Remove(__instance);

            bool willCrit;
            willCritMap.TryGetValue(__instance, out willCrit);
            willCritMap.Remove(__instance);

            // ★ 恢复 criticalPlus（Prefix 里被强改成 100 或 0，这里还原）
            __instance.criticalPlus = realCrit;

            // 这次没暴击，什么都不做
            if (!willCrit) return;

            if (!Plugin.MultiCritEnabled.Value) return;
            if (other == null) return;
            if (!IsPlayerSide(__instance.source)) return;

            // 从真实暴击率算 overflow（注意 realCrit 是原值，不是强制后的 100）
            int overflow = realCrit - 100;

            if (Plugin.LevelBonusEnabled.Value && other != null)
            {
                var enemyStatForDiff = other.GetComponent<Status>();
                var mainPlayer = GlobalVar.mainPlayer;
                Status playerStat = null;
                if (mainPlayer != null)
                {
                    if (!ReferenceEquals(_cachedPlayerObj, mainPlayer))
                    {
                        _cachedPlayerObj = mainPlayer;
                        _cachedPlayerStat = mainPlayer.GetComponent<Status>();
                    }
                    playerStat = _cachedPlayerStat;
                }
                if (enemyStatForDiff != null && playerStat != null)
                {
                    int diff = enemyStatForDiff.level - playerStat.level;
                    if (diff > 0)
                    {
                        int effectiveDiff = diff;
                        if (effectiveDiff > Plugin.LevelMultiCritMaxDiff.Value)
                            effectiveDiff = Plugin.LevelMultiCritMaxDiff.Value;

                        int levelBonus = effectiveDiff * Plugin.LevelMultiCritPercentPerLevel.Value;
                        overflow += levelBonus;
                    }
                }
            }

            if (overflow <= 0) return;

            int guaranteed = overflow / 100;
            int extraChance = overflow % 100;
            int extraCount = guaranteed;
            if (Random.Range(0, 100) < extraChance) extraCount++;

            int maxExtra = Plugin.MultiCritMaxExtra.Value;
            if (extraCount > maxExtra) extraCount = maxExtra;
            if (extraCount <= 0) return;

            var enemyStat = other.GetComponent<Status>();
            if (enemyStat == null) return;

            int extraDamage = baseDamage * 2;

            // ① 立即结算所有伤害，同时记录每段的数字和弹字位置
            List<string> results = new List<string>();
            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < extraCount; i++)
            {
                if (enemyStat == null || enemyStat.dead) break;

                string result = enemyStat.OnDamage(
                    extraDamage,
                    __instance.accuracy,
                    (int)__instance.element,
                    __instance.cantBlock,
                    true,
                    (int)__instance.attackType);

                // 位置偏移：水平随机 + 每段往上叠
                Vector3 pos = other.transform.position;
                pos.x += Random.Range(-0.6f, 0.6f);
                pos.z += Random.Range(-0.3f, 0.3f);
                pos.y += 0.3f + i * 0.4f;

                results.Add(result);
                positions.Add(pos);
            }

            // ② 启动协程，按顺序、间隔弹数字
            if (__instance.popup != null && results.Count > 0)
            {
                Plugin.Instance.StartCoroutine(
                    SpawnPopupsDelayed(__instance.popup, positions, results));
            }
        }

        static void Finalizer(BulletStatus __instance)
        {
            savedCrit.Remove(__instance);
            savedDamage.Remove(__instance);
            savedOther.Remove(__instance);
            willCritMap.Remove(__instance);
        }


        private static System.Collections.IEnumerator SpawnPopupsDelayed(
            Transform popupPrefab, List<Vector3> positions, List<string> damages)
        {
            float delay = MULTI_CRIT_POPUP_DELAY_MS / 1000f;
            for (int i = 0; i < positions.Count; i++)
            {
                Transform t = Object.Instantiate(
                    popupPrefab, positions[i], Quaternion.identity);
                var dp = t.GetComponent<DamagePopup>();
                if (dp != null)
                {
                    dp.damage = damages[i];
                    dp.critical = true;
                }
                yield return new WaitForSeconds(delay);
            }
        }

        private static bool IsPlayerSide(GameObject attacker)
        {
            if (attacker == null) return false;
            if (attacker == GlobalVar.mainPlayer) return true;
            var ai = attacker.GetComponent<AiStatus>();
            if (ai != null && ai.master != null
                && ai.master.gameObject == GlobalVar.mainPlayer)
            {
                return true;
            }
            return false;
        }
    }
}