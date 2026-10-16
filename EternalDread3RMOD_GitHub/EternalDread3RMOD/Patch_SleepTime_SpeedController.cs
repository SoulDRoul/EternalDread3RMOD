using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(SleepTime), "GoSleep")]
    public static class Patch_SleepTime_GoSleep_Speed
    {
        static void Postfix(SleepTime __instance)
        {
            SleepSpeedController.OnSleepStart(__instance);
        }
    }

    [HarmonyPatch(typeof(SleepTime), "Update")]
    public static class Patch_SleepTime_Update_Speed
    {
        private static FieldInfo _sleepingField = null;

        static void Prefix(SleepTime __instance)
        {
            if (!Plugin.SleepSpeedControlEnabled.Value) return;
            if (!SleepSpeedController.IsSleeping) return;
            if (!SleepSpeedController.IsActive(__instance)) return;

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (SleepSpeedController.ShiftDown() && Plugin.Log != null)
                    Plugin.DebugLog("[ED3R] Sleep speed -> " + SleepSpeedController.CurrentLabel);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (SleepSpeedController.ShiftUp() && Plugin.Log != null)
                    Plugin.DebugLog("[ED3R] Sleep speed -> " + SleepSpeedController.CurrentLabel);
            }
        }

        static void Postfix(SleepTime __instance)
        {
            if (_sleepingField == null)
                _sleepingField = AccessTools.Field(typeof(SleepTime), "sleeping");
            if (_sleepingField == null) return;

            bool sleeping = (bool)_sleepingField.GetValue(__instance);
            SleepSpeedController.SetSleeping(sleeping, __instance);
        }
    }
}