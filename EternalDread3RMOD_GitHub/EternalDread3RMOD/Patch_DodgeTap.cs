using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(PlayerInputController), "Update")]
    public static class Patch_DodgeTap
    {
        private static bool _backupDodgeRollTap = false;

        static void Prefix(PlayerInputController __instance)
        {
            if (__instance.gameObject != GlobalVar.mainPlayer) return;
            if (Plugin.DodgeRollTapMode.Value != 1) return;
            if (SleepSpeedController.IsSleeping) return;

            // 1. 备份并关闭原版双击闪避
            _backupDodgeRollTap = OptionData.dodgeRollTap;
            OptionData.dodgeRollTap = false;

            // === 新增：不可行动时清理锁定冲刺，本帧不介入 ===
            if (!PlayerActionStateHelper.IsPlayerActionable(__instance))
            {
                if (DodgeTapState.lockedSprint)
                {
                    DodgeTapState.lockedSprint = false;
                    if (!__instance.recover)
                    {
                        __instance.recover = true;
                        __instance.recoverStamina = 0f;
                    }
                }
                return;
            }

            float stamina = __instance.stamina;

            // 2. 双击检测（体力 > 0 时才检测，体力为 0 不允许进入锁定冲刺）
            bool grounded = (__instance.GetComponent<CharacterController>().collisionFlags
                & CollisionFlags.Below) != CollisionFlags.None;

            bool justTapped = false;
            if (grounded && stamina > 0f)
            {
                if (DetectTap(ButtonInputOption.up, DodgeTapState.DirUp)) justTapped = true;
                if (DetectTap(ButtonInputOption.down, DodgeTapState.DirDown)) justTapped = true;
                if (DetectTap(ButtonInputOption.left, DodgeTapState.DirLeft)) justTapped = true;
                if (DetectTap(ButtonInputOption.right, DodgeTapState.DirRight)) justTapped = true;
            }

            if (justTapped)
            {
                // 双击进入锁定冲刺
                DodgeTapState.lockedSprint = true;
                // 停止体力恢复（和原版设 sprint=true 的效果一致）
                __instance.recover = false;
            }
            else if (DodgeTapState.lockedSprint)
            {
                // 3. 检查退出锁定冲刺的条件
                bool shouldExit = false;

                // 条件 A：体力耗尽
                if (stamina <= 0f) shouldExit = true;

                // 条件 B：松开所有方向键
                bool anyDir = Mathf.Abs(ButtonInputOption.Horizontal) > 0.1f
                           || Mathf.Abs(ButtonInputOption.Vertical) > 0.1f;
                if (!anyDir) shouldExit = true;

                if (shouldExit)
                {
                    DodgeTapState.lockedSprint = false;
                    // 触发体力恢复（和原版退出 sprint 一致）
                    if (!__instance.recover)
                    {
                        __instance.recover = true;
                        __instance.recoverStamina = 0f;
                    }
                }
            }
        }

        static void Postfix(PlayerInputController __instance)
        {
            if (__instance.gameObject != GlobalVar.mainPlayer) return;
            if (Plugin.DodgeRollTapMode.Value != 1) return;
            if (SleepSpeedController.IsSleeping) return;

            // 恢复原版设置
            OptionData.dodgeRollTap = _backupDodgeRollTap;

            // 锁定冲刺时，强制显示体力条（原版只在 this.sprint 时显示）
            if (DodgeTapState.lockedSprint
                && __instance.canvasElement.useCanvas
                && __instance.canvasElement.staminaBar)
            {
                if (!__instance.canvasElement.border.activeSelf)
                {
                    __instance.canvasElement.border.SetActive(true);
                }
                float num = __instance.stamina / __instance.maxStamina;
                __instance.canvasElement.staminaBar.fillAmount = num;
            }
        }

        private static bool DetectTap(KeyCode key, string dirId)
        {
            if (!Input.GetKeyDown(key)) return false;

            float last;
            if (DodgeTapState.lastTapTime.TryGetValue(dirId, out last)
                && Time.time - last < DodgeTapState.TapWindow)
            {
                DodgeTapState.lastTapTime[dirId] = -100f;
                return true;
            }
            else
            {
                DodgeTapState.lastTapTime[dirId] = Time.time;
                return false;
            }
        }
    }
}