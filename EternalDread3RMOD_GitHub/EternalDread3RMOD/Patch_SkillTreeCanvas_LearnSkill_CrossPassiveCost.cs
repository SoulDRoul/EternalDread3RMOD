using System;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 学习被动技能时，按"职业相似轮"动态计算技能点消耗。
    ///
    /// 规则：
    ///   buttonId 0~13  → 主动技能，完全走原版
    ///   buttonId 14~29 → 被动技能，动态扣点
    ///
    /// 倍率：
    ///   距离 0（当前视图职业）→ ×1.0
    ///   距离 1 → ×NearMul  (默认 1.1)
    ///   距离 4 → ×FarMul   (默认 3.0)
    ///   中间线性插值
    ///
    /// 实际消耗 = ceil(原版 skPointUse × 倍率)，保底 1。
    ///
    /// 实现方式：Prefix 返回 false，完全接管 LearnSkill 逻辑，
    /// 避免临时修改 skPointUse 被异常打断。
    /// </summary>
    [HarmonyPatch(typeof(SkillTreeCanvas), "LearnSkill")]
    public static class Patch_SkillTreeCanvas_LearnSkill_CrossPassiveCost
    {
        static bool Prefix(SkillTreeCanvas __instance, int buttonId)
        {
            if (!Plugin.CrossPassiveCostEnabled.Value) return true;
            if (__instance == null) return true;
            if (__instance.player == null) return true;
            if (__instance.skillSlots == null) return true;
            if (buttonId < 0 || buttonId >= __instance.skillSlots.Length) return true;

            // 主动技能：走原版
            if (!CrossPassiveHelper.IsPassiveButtonId(buttonId)) return true;

            try
            {
                var slot = __instance.skillSlots[buttonId];
                if (slot == null) return true;

                var status = __instance.player.GetComponent<Status>();
                if (status == null) return true;

                int originalCost = slot.skPointUse;
                float mul = CrossPassiveHelper.GetCurrentCostMul(__instance);
                int actualCost = (int)Math.Ceiling(originalCost * mul);
                if (actualCost < 1) actualCost = 1;

                if (status.skillPoint < actualCost)
                {
                    MonoBehaviour.print("Not enough Skill Point");
                    // 阻止原版继续，因为原版会用原版成本判断
                    return false;
                }

                status.skillPoint -= actualCost;

                var skill = __instance.player.GetComponent<Skill>();
                if (skill != null)
                {
                    skill.AddSkill(slot.skillId);
                }

                __instance.CheckUnlockSkill();
                __instance.UpdateSkillButton();

                if (__instance.learnSkillSound)
                    __instance.audi.PlayOneShot(__instance.learnSkillSound);

                Plugin.DebugLog("[ED3R] CrossPassiveCost: buttonId=" + buttonId
                    + " orig=" + originalCost
                    + " mul=" + mul.ToString("F2")
                    + " actual=" + actualCost);

                // 已接管，跳过原版
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("[ED3R] CrossPassiveCost error: " + e);
                // 出错时走原版，保证功能不崩
                return true;
            }
        }
    }
}