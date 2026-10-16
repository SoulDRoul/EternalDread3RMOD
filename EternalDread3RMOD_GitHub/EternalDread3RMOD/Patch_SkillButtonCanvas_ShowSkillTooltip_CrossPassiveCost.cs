using System;
using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 被动技能 tooltip 底部的"Require Skill Point"文字，
    /// 原版直接显示 skPointUse，未经过跨职业倍率。
    ///
    /// 这里在 Postfix 里重写这段文字，显示动态计算后的消耗。
    /// 只作用于被动技能（buttonId 14~29），主动技能不受影响。
    /// 已学会的技能不覆盖（原版走 negativeText 分支）。
    /// </summary>
    [HarmonyPatch(typeof(SkillButtonCanvas), "ShowSkillTooltip")]
    public static class Patch_SkillButtonCanvas_ShowSkillTooltip_CrossPassiveCost
    {
        static void Postfix(SkillButtonCanvas __instance)
        {
            if (!Plugin.CrossPassiveCostEnabled.Value) return;
            if (__instance == null) return;
            if (__instance.skillTree == null) return;
            if (__instance.skillTree.skillSlots == null) return;
            if (__instance.tooltipNegative == null) return;

            int buttonId2 = __instance.buttonId;

            // 主动技能：不处理
            if (!CrossPassiveHelper.IsPassiveButtonId(buttonId2)) return;
            if (buttonId2 >= __instance.skillTree.skillSlots.Length) return;

            var slot = __instance.skillTree.skillSlots[buttonId2];
            if (slot == null) return;

            // 已学会：原版走 negativeText 分支，不覆盖
            if (slot.learned) return;

            try
            {
                float mul = CrossPassiveHelper.GetCurrentCostMul(__instance.skillTree);
                int actualCost = (int)Math.Ceiling(slot.skPointUse * mul);
                if (actualCost < 1) actualCost = 1;

                if (slot.unlockLevel > 1)
                {
                    // 保持原版文本格式
                    __instance.tooltipNegative.text = "Require LV: "
                        + slot.unlockLevel.ToString()
                        + " , Skill Point "
                        + actualCost.ToString();
                }
                else
                {
                    __instance.tooltipNegative.text = "Require Skill Point : "
                        + actualCost.ToString();
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("[ED3R] ShowSkillTooltip cost patch error: " + e);
            }
        }
    }
}