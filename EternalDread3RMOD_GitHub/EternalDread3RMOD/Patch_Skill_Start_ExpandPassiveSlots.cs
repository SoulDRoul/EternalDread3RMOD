using System;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Skill), "Start")]
    public static class Patch_Skill_Start_ExpandPassiveSlots
    {
        static void Postfix(Skill __instance)
        {
            if (!Plugin.PassiveSlotExpandEnabled.Value) return;
            if (__instance == null) return;

            try
            {
                int target = PassiveSlotHelper.GetTargetArrayLength();

                // 扩 shortCutSkillID
                if (__instance.shortCutSkillID == null)
                {
                    __instance.shortCutSkillID = new int[target];
                }
                else if (__instance.shortCutSkillID.Length < target)
                {
                    int[] newArr = new int[target];
                    Array.Copy(__instance.shortCutSkillID, newArr,
                        __instance.shortCutSkillID.Length);
                    __instance.shortCutSkillID = newArr;
                }

                // 扩 AttackTrigger 数组
                var atk = __instance.GetComponent<AttackTrigger>();
                if (atk == null) return;

                if (atk.skills == null || atk.skills.Length < target)
                {
                    var newSkills = new SkillSetting[target];
                    if (atk.skills != null)
                        Array.Copy(atk.skills, newSkills, atk.skills.Length);
                    atk.skills = newSkills;
                }

                if (atk.skillCoolDown == null || atk.skillCoolDown.Length < target)
                {
                    var newCD = new int[target];
                    if (atk.skillCoolDown != null)
                        Array.Copy(atk.skillCoolDown, newCD, atk.skillCoolDown.Length);
                    atk.skillCoolDown = newCD;
                }

                var waitField = AccessTools.Field(typeof(AttackTrigger), "wait");
                if (waitField != null)
                {
                    var oldWait = (float[])waitField.GetValue(atk);
                    if (oldWait == null || oldWait.Length < target)
                    {
                        var newWait = new float[target];
                        if (oldWait != null)
                            Array.Copy(oldWait, newWait, oldWait.Length);
                        waitField.SetValue(atk, newWait);
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("[ED3R] PassiveSlot Skill.Start error: " + e);
            }
        }
    }
}