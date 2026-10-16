using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 让中毒/燃烧可以致死。原版在 tick 后把 health 钳制到 1。
    /// 我们检测"是否刚 tick"且 health <= 1，就调用 Death()。
    /// </summary>
    [HarmonyPatch(typeof(Status), "Update")]
    public static class Patch_Status_DoTKill
    {
        private static Dictionary<Status, float> _prevPoisonTime
            = new Dictionary<Status, float>();
        private static Dictionary<Status, float> _prevBurnTime
            = new Dictionary<Status, float>();

        // ============ 字段访问器（编译成委托，比反射 GetValue 快 20~50 倍）============
        private static AccessTools.FieldRef<Status, float> _poisonTimeRef;
        private static AccessTools.FieldRef<Status, float> _burnTimeRef;
        private static bool _refsInitialized;

        private static void EnsureRefs()
        {
            if (_refsInitialized) return;
            _refsInitialized = true;
            try
            {
                _poisonTimeRef = AccessTools.FieldRefAccess<Status, float>("poisonTime");
            }
            catch { _poisonTimeRef = null; }
            try
            {
                _burnTimeRef = AccessTools.FieldRefAccess<Status, float>("burnTime");
            }
            catch { _burnTimeRef = null; }
        }

        static void Prefix(Status __instance)
        {
            if (!Plugin.DoTKillEnabled.Value) return;

            EnsureRefs();

            float curP = _poisonTimeRef != null ? _poisonTimeRef(__instance) : 0f;
            float curB = _burnTimeRef != null ? _burnTimeRef(__instance) : 0f;

            // 只在有 DoT 计时时才写字典（非中毒非燃烧单位直接跳过）
            if (curP > 0f) _prevPoisonTime[__instance] = curP;
            if (curB > 0f) _prevBurnTime[__instance] = curB;
        }

        static void Postfix(Status __instance)
        {
            if (!Plugin.DoTKillEnabled.Value) return;

            EnsureRefs();

            float curP = _poisonTimeRef != null ? _poisonTimeRef(__instance) : 0f;
            float curB = _burnTimeRef != null ? _burnTimeRef(__instance) : 0f;

            // 计时归零 → 取出 prev 并从字典删除
            float prevP = 0f, prevB = 0f;
            if (curP <= 0f)
            {
                if (_prevPoisonTime.TryGetValue(__instance, out prevP))
                    _prevPoisonTime.Remove(__instance);
            }
            if (curB <= 0f)
            {
                if (_prevBurnTime.TryGetValue(__instance, out prevB))
                    _prevBurnTime.Remove(__instance);
            }

            if (__instance.dead) return;
            if (__instance.health > 1) return;

            bool poisonTicked = prevP >= 2f && curP == 0f;
            bool burnTicked = prevB >= 1.5f && curB == 0f;

            if (!poisonTicked && !burnTicked) return;

            __instance.health = 0;
            __instance.Death();
        }

        public static void CleanInstance(Status stat)
        {
            _prevPoisonTime.Remove(stat);
            _prevBurnTime.Remove(stat);
        }
    }
}