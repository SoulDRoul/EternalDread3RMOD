using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Status), "OnDamage")]
    public static class Patch_Status_OnDamage
    {
        static bool Prefix(Status __instance, ref string __result)
        {
            if (InvincibleData.IsInvincible(__instance))
            {
                __result = __instance.dodgeRoll ? "Evaded" : "Miss";
                return false;
            }
            return true;
        }

        static void Postfix(Status __instance, string __result)
        {
            if (__instance.gameObject != GlobalVar.mainPlayer) return;

            // ===== 挑战模式：闪避/格挡恢复计时 =====
            // 注意：Evaded 可能来自 Prefix 拦截（真无敌+闪避），也可能来自原方法 dodgeRoll 判断
            //       Blocked 只会来自原方法的 block 判断
            //       两者都算"防御成功"，触发恢复
            if (__result == "Evaded" || __result == "Blocked")
            {
                ApplyDefenseRecover(__result);
            }

            // 只有真正扣血才继续处理（返回数字字符串）
            int dmg;
            if (!int.TryParse(__result, out dmg)) return;

            // ===== 挑战模式：受击减时间 =====
            ApplyChallengeHitPenalty();

            // ===== 受击无敌 =====
            float last;
            bool hasLast = InvincibleData.lastHurtTime.TryGetValue(__instance, out last);

            float cooldownSec = Plugin.HurtInvincibleCooldownMs.Value / 1000f;
            if (!hasLast || Time.time - last >= cooldownSec)
            {
                float sec = Plugin.HurtInvincibleMs.Value / 1000f;
                InvincibleData.SetInvincible(__instance, sec);
                InvincibleData.lastHurtTime[__instance] = Time.time;
                Plugin.DebugLog("[ED3R] Hurt invincible " + sec + "s");
            }
        }

        // 受击时对连杀计时做扣减或重置
        private static void ApplyChallengeHitPenalty()
        {

            // 没有连杀时无影响
            if (ComboState.comboCount <= 0) return;

            int mode = Plugin.ChallengeHitMode.Value;

            // 模式 0：无（简单模式，不处理）
            if (mode == 0) return;

            // 模式 2：重置连杀（挑战模式）
            if (mode == 2)
            {
                ComboState.ResetCombo();
                Plugin.DebugLog("[Challenge] 受击触发，连杀重置");
                return;
            }

            // 模式 1（默认）：减时间
            // 其他非法值也走这条默认路径
            int reduceType = Plugin.ChallengeHitReduceType.Value;
            float reduceSec;

            if (reduceType == 0)
            {
                // 固定秒数
                float sec = Plugin.ChallengeHitReduceSeconds.Value;
                if (sec < 0f) sec = 0f;
                if (sec > 60f) sec = 60f;
                reduceSec = sec;
            }
            else if (reduceType == 2)
            {
                // 当前剩余时间的百分比
                int pct = Plugin.ChallengeHitReducePercent.Value;
                if (pct < 0) pct = 0;
                if (pct > 100) pct = 100;
                reduceSec = ComboState.GetRemainingTime() * pct / 100f;
            }
            else
            {
                // 默认：最大时间的百分比（reduceType=1 或非法值）
                int pct = Plugin.ChallengeHitReducePercent.Value;
                if (pct < 0) pct = 0;
                if (pct > 100) pct = 100;
                reduceSec = ComboState.GetMaxWindowTime() * pct / 100f;
            }

            ComboState.comboEndTime = ComboState.CalcReduceEndTime(reduceSec);
            Plugin.DebugLog("[Challenge] 受击触发，扣减 " + reduceSec
                + " 秒，剩余 " + ComboState.GetRemainingTime() + " 秒");
        }

        // 闪避/格挡成功时恢复连杀计时
        private static void ApplyDefenseRecover(string result)
        {
            // 未启用恢复
            if (!Plugin.ChallengeDefenseRecoverEnabled.Value) return;

            // 没有连杀时无影响
            if (ComboState.comboCount <= 0) return;

            int recoverType = Plugin.ChallengeDefenseRecoverType.Value;
            float recoverSec;

            if (recoverType == 0)
            {
                // 固定秒数
                float sec = Plugin.ChallengeDefenseRecoverSeconds.Value;
                if (sec < 0f) sec = 0f;
                if (sec > 60f) sec = 60f;
                recoverSec = sec;
            }
            else
            {
                // 默认：最大时间的百分比（recoverType=1 或非法值）
                int pct = Plugin.ChallengeDefenseRecoverPercent.Value;
                if (pct < 0) pct = 0;
                if (pct > 100) pct = 100;
                recoverSec = ComboState.GetMaxWindowTime() * pct / 100f;
            }

            if (recoverSec <= 0f) return;

            ComboState.comboEndTime = ComboState.CalcRecoverEndTime(recoverSec);
            Plugin.DebugLog("[Challenge] " + result + " 触发，恢复 "
                + recoverSec + " 秒，剩余 " + ComboState.GetRemainingTime() + " 秒");
        }
    }
}