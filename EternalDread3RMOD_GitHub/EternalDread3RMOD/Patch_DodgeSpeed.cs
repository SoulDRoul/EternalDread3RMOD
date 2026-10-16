using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(PlayerInputController), "Update")]
    public static class Patch_DodgeSpeed
    {
        private const float VanillaDodgeSpeed = 8f;

        // 反射字段缓存：只查一次，之后复用
        private static System.Reflection.FieldInfo _sprintField = null;
        private static System.Reflection.FieldInfo _dirField = null;

        static void Prefix(PlayerInputController __instance)
        {
            if (!__instance.dodging) return;

            // ===== 反射字段缓存（懒加载）=====
            if (_sprintField == null)
                _sprintField = AccessTools.Field(typeof(PlayerInputController), "sprint");
            if (_dirField == null)
                _dirField = AccessTools.Field(typeof(PlayerInputController), "dir");

            // 判断是否处于冲刺状态
            bool sprinting = (bool)_sprintField.GetValue(__instance);

            // 双击触发的锁定冲刺
            if (DodgeTapState.lockedSprint && __instance.stamina > 0f) sprinting = true;

            // 按住 sprint 键
            bool sprintKey = Input.GetKey(ButtonInputOption.sprint)
                          || Input.GetKey(KeyCode.JoystickButton8);
            if (sprintKey && __instance.stamina > 0f) sprinting = true;

            float currentSpeed = sprinting
                ? __instance.sprintSpeed
                : __instance.walkSpeed;

            // 计算缩放系数
            float mul = Plugin.DodgeSpeedPercent.Value / 100f;
            float k = currentSpeed * mul / VanillaDodgeSpeed;
            if (k <= 0f) return;

            // 读取 dir 字段
            Vector3 dir = (Vector3)_dirField.GetValue(__instance);

            // 归一化 x/z 分量，防止累积
            Vector3 flat = new Vector3(dir.x, 0f, dir.z);
            if (flat.sqrMagnitude < 0.0001f) return;

            flat.Normalize();
            flat *= k;

            dir.x = flat.x;
            dir.z = flat.z;
            _dirField.SetValue(__instance, dir);
        }
    }
}