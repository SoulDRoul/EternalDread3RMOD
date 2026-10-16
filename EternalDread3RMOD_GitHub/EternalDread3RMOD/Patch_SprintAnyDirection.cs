using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(PlayerInputController), "Update")]
    public static class Patch_SprintAnyDirection
    {
        // 反射字段缓存：只查一次，之后复用
        private static System.Reflection.FieldInfo _motorField = null;
        private static System.Reflection.FieldInfo _sprintSpeedField = null;
        private static System.Reflection.FieldInfo _walkSpeedField = null;
        private static System.Reflection.FieldInfo _controllerField = null;
        private static System.Reflection.FieldInfo _staminaField = null;
        private static System.Reflection.FieldInfo _sprintField = null;
        private static System.Reflection.FieldInfo _useStaminaField = null;

        static void Postfix(PlayerInputController __instance)
        {
            if (!Plugin.SprintAnyDirection.Value) return;
            if (__instance.gameObject != GlobalVar.mainPlayer) return;
            if (SleepSpeedController.IsSleeping) return;
            if (__instance.dodging) return;   // 翻滚期间不干预 motor

            // ===== 反射字段缓存（懒加载）=====
            if (_motorField == null)
                _motorField = AccessTools.Field(typeof(PlayerInputController), "motor");
            if (_sprintSpeedField == null)
                _sprintSpeedField = AccessTools.Field(typeof(PlayerInputController), "sprintSpeed");
            if (_walkSpeedField == null)
                _walkSpeedField = AccessTools.Field(typeof(PlayerInputController), "walkSpeed");
            if (_controllerField == null)
                _controllerField = AccessTools.Field(typeof(PlayerInputController), "controller");
            if (_staminaField == null)
                _staminaField = AccessTools.Field(typeof(PlayerInputController), "stamina");
            if (_sprintField == null)
                _sprintField = AccessTools.Field(typeof(PlayerInputController), "sprint");
            if (_useStaminaField == null)
                _useStaminaField = AccessTools.Field(typeof(PlayerInputController), "useStamina");

            var motor = (CharacterMotorC)_motorField.GetValue(__instance);
            if (motor == null) return;

            float sprintSpeed = (float)_sprintSpeedField.GetValue(__instance);
            float walkSpeed = (float)_walkSpeedField.GetValue(__instance);

            // 玩家是否按住 sprint 键
            bool sprintKey = Input.GetKey(ButtonInputOption.sprint)
              || Input.GetKey(KeyCode.JoystickButton8)
              || DodgeTapState.lockedSprint;

            // 是否有方向输入
            bool hasDirection = Mathf.Abs(ButtonInputOption.Horizontal) >= 0.1f
                             || Mathf.Abs(ButtonInputOption.Vertical) >= 0.1f;

            // 是否在地面上
            var controller = (CharacterController)_controllerField.GetValue(__instance);
            bool onGround = controller != null
                && (controller.collisionFlags & CollisionFlags.Below) != CollisionFlags.None;

            // 体力
            float stamina = (float)_staminaField.GetValue(__instance);

            // 原版 sprint 是否已经激活
            bool alreadySprinting = (bool)_sprintField.GetValue(__instance);

            bool shouldSprint = sprintKey && hasDirection && onGround && stamina > 0f
                    && PlayerActionStateHelper.IsPlayerActionable(__instance);

            if (shouldSprint)
            {
                // 加速三个方向
                motor.movement.maxForwardSpeed = sprintSpeed;
                motor.movement.maxSidewaysSpeed = sprintSpeed;
                motor.movement.maxBackwardsSpeed = sprintSpeed;

                // 原版 sprint 没触发时，手动消耗体力
                if (!alreadySprinting)
                {
                    float useStamina = (float)_useStaminaField.GetValue(__instance);
                    float drainPerSecond = 1f / useStamina;

                    stamina -= drainPerSecond * Time.deltaTime;
                    if (stamina < 0f) stamina = 0f;
                    _staminaField.SetValue(__instance, stamina);
                }
            }
            else
            {
                // 不该冲刺：无条件把 motor 恢复到走路速度
                // 原版 Update 不会主动重设走路速度，所以每帧都要覆盖
                motor.movement.maxForwardSpeed = walkSpeed;
                motor.movement.maxSidewaysSpeed = walkSpeed;
                motor.movement.maxBackwardsSpeed = walkSpeed;
            }
            // 锁定冲刺期间强制关闭体力恢复
            if (DodgeTapState.lockedSprint && hasDirection && onGround && stamina > 0f)
            {
                __instance.recover = false;
            }
        }
    }
}