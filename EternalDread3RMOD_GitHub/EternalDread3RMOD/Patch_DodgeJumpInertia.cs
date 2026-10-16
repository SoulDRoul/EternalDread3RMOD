using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(PlayerInputController), "Update")]
    public static class Patch_DodgeJumpInertia
    {
        // ============ 硬编码常量：Stamina.DodgeJumpInertiaDurationSec ============
        // 原 Stamina.DodgeJumpInertiaDurationSec —— 惯性衰减时长（秒），从完整速度线性衰减到 0
        private const float DODGE_JUMP_INERTIA_DURATION_SEC = 0.4f;

        private static System.Reflection.FieldInfo _onRisingField = null;
        private static System.Reflection.FieldInfo _controllerField = null;
        private static System.Reflection.FieldInfo _sprintField = null;

        static void Postfix(PlayerInputController __instance)
        {
            if (!Plugin.DodgeJumpInertiaEnabled.Value) return;
            if (__instance.gameObject != GlobalVar.mainPlayer) return;

            var atk = __instance.GetComponent<AttackTrigger>();
            if (atk == null) return;

            if (_onRisingField == null)
            {
                _onRisingField = AccessTools.Field(typeof(AttackTrigger), "onRising");
            }
            if (_onRisingField == null) return;

            bool onRising = (bool)_onRisingField.GetValue(atk);

            // 上升沿：捕获速度
            if (onRising && !DodgeJumpInertia.prevOnRising)
            {
                CaptureVelocity(__instance, atk);
            }

            // 下降沿：清空
            if (!onRising && DodgeJumpInertia.prevOnRising)
            {
                DodgeJumpInertia.active = false;
                DodgeJumpInertia.velocity = Vector3.zero;
            }

            DodgeJumpInertia.prevOnRising = onRising;

            // 应用惯性
            if (DodgeJumpInertia.active)
            {
                ApplyInertia(__instance);
            }
        }

        private static void CaptureVelocity(PlayerInputController p, AttackTrigger atk)
        {
            // 速度大小
            if (_sprintField == null)
            {
                _sprintField = AccessTools.Field(typeof(PlayerInputController), "sprint");
            }
            bool originalSprint = _sprintField != null && (bool)_sprintField.GetValue(p);
            bool sprinting = originalSprint || DodgeTapState.lockedSprint;
            float speed = sprinting ? p.sprintSpeed : p.walkSpeed;
            speed *= Plugin.DodgeJumpInertiaMul.Value / 100f;

            // 方向
            Vector3 dir = Vector3.zero;
            bool foundTarget = false;

            if (atk.autoLockTarget != null
                && atk.autoLockTarget.enable
                && atk.autoLockTarget.lockTarget != null)
            {
                Vector3 toEnemy = atk.autoLockTarget.lockTarget.position - p.transform.position;
                toEnemy.y = 0f;
                float dist = toEnemy.magnitude;
                if (dist > 0.1f && dist <= atk.autoLockTarget.lockOnRange)
                {
                    dir = toEnemy.normalized;
                    foundTarget = true;
                }
            }

            if (!foundTarget)
            {
                // 用输入方向
                Vector3 inputDir = new Vector3(
                    ButtonInputOption.Horizontal, 0f, ButtonInputOption.Vertical);
                if (inputDir.sqrMagnitude > 0.01f)
                {
                    inputDir.Normalize();
                    dir = p.transform.rotation * inputDir;
                }
                else
                {
                    // 无输入时朝向正前方
                    dir = p.transform.forward;
                }
            }

            DodgeJumpInertia.velocity = dir * speed;
            DodgeJumpInertia.startTime = Time.time;
            DodgeJumpInertia.active = true;

            Plugin.DebugLog("[JumpInertia] captured velocity=" + DodgeJumpInertia.velocity
                + " (foundTarget=" + foundTarget + ")");
        }

        private static void ApplyInertia(PlayerInputController p)
        {
            float duration = DODGE_JUMP_INERTIA_DURATION_SEC;
            if (duration <= 0f)
            {
                DodgeJumpInertia.active = false;
                DodgeJumpInertia.velocity = Vector3.zero;
                return;
            }

            float elapsed = Time.time - DodgeJumpInertia.startTime;
            float factor = 1f - Mathf.Clamp01(elapsed / duration);
            if (factor <= 0f)
            {
                DodgeJumpInertia.active = false;
                DodgeJumpInertia.velocity = Vector3.zero;
                return;
            }

            if (_controllerField == null)
            {
                _controllerField = AccessTools.Field(typeof(PlayerInputController), "controller");
            }
            if (_controllerField == null) return;

            var ctrl = (CharacterController)_controllerField.GetValue(p);
            if (ctrl == null) return;

            Vector3 step = DodgeJumpInertia.velocity * factor * Time.deltaTime;
            ctrl.Move(step);
        }
    }
}