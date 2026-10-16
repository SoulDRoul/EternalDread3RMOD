using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 格挡中按右键+方向 → 打断格挡并触发闪避。
    /// 
    /// 严格触发：GetKeyDown(guard)。玩家必须松开右键再按才能触发（与原版闪避操作一致）。
    /// 体力不足时不打断：保持格挡状态，避免"格挡消失但闪避失败"。
    /// 只打断格挡，让原版 PlayerInputController.Update 自己触发 DodgeRoll：
    ///   - 避免双触发
    ///   - 避免 dir 私有字段未设置
    ///   - 复用原版地面检测（collisionFlags & Below）
    /// </summary>
    [HarmonyPatch(typeof(PlayerInputController), "Update")]
    public static class Patch_PlayerInputController_GuardDodge
    {
        static void Prefix(PlayerInputController __instance)
        {
            if (!Plugin.GuardInterruptEnabled.Value) return;
            if (!Plugin.GuardInterruptDodgeEnabled.Value) return;
            if (__instance.gameObject != GlobalVar.mainPlayer) return;

            var stat = __instance.GetComponent<Status>();
            if (stat == null || !stat.block) return;

            // 右键按下（严格：需要松开再按）
            bool guardDown = Input.GetKeyDown(ButtonInputOption.guard)
                          || Input.GetKeyDown(ButtonInputOption.joyGuard);
            if (!guardDown) return;

            // 必须有方向输入
            float h = ButtonInputOption.Horizontal;
            float v = ButtonInputOption.Vertical;
            if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f) return;

            // 体力不足 → 不打断格挡
            if (__instance.stamina < __instance.dodgeRollSetting.staminaUse) return;

            // 已在闪避 / 技能中 → 不处理
            if (__instance.dodging) return;
            var atk = __instance.GetComponent<AttackTrigger>();
            if (atk != null && atk.onUsingSkill) return;

            // 打断格挡，让原版 Update 自己触发 DodgeRoll
            stat.StopCoroutine("GuardUp");
            stat.GuardBreak();
            ResetPlayerAnimation(stat);
        }

        private static void ResetPlayerAnimation(Status stat)
        {
            if (stat == null) return;
            var legacy = stat.GetComponent<LegacyAnimationSet>();
            if (legacy != null)
            {
                try { legacy.ForceIdle(); }
                catch { }
            }
        }
    }
}