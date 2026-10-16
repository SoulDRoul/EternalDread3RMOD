using System;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 读档 / 重生时扩数组。
    ///
    /// 关键：扩 AttackTrigger 数组必须放在 Prefix 里！
    /// 因为原版 SettingNewPlayer 方法体内部就会执行 AssignAllSkill()，
    /// 那个时刻 shortCutSkillID 已经是新长度（44），如果 AttackTrigger.skills
    /// 还是 20，就会越界崩溃 → 黑屏 → 回退标题 BGM。
    /// </summary>
    [HarmonyPatch(typeof(GlobalVar), "SettingNewPlayer")]
    public static class Patch_GlobalVar_SettingNewPlayer_ExpandArrays
    {
        static void Prefix(GameObject player)
        {
            if (!Plugin.PassiveSlotExpandEnabled.Value) return;

            int target = PassiveSlotHelper.GetTargetArrayLength();

            try
            {
                // 1. 扩 SavingStat.shortCutSkillID
                //    原版 SettingNewPlayer 内部会 component3.shortCutSkillID = SavingStat.shortCutSkillID;
                //    所以这里必须先扩好，原版方法体里拿到的才是新长度。
                if (SavingStat.shortCutSkillID == null
                    || SavingStat.shortCutSkillID.Length < target)
                {
                    var newArr = new int[target];
                    if (SavingStat.shortCutSkillID != null)
                        Array.Copy(SavingStat.shortCutSkillID, newArr,
                            SavingStat.shortCutSkillID.Length);
                    SavingStat.shortCutSkillID = newArr;
                }

                // 2. 扩 player 上的 AttackTrigger 数组
                //    必须在 Prefix 里做！原版方法体内部就会 AssignAllSkill()。
                GameObject p = player;
                if (p == null) p = GlobalVar.mainPlayer;
                if (p == null) return;

                ExpandAttackTriggerArrays(p, target);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("[ED3R] SettingNewPlayer Prefix error: " + e);
            }
        }

        static void Postfix(GameObject player)
        {
            if (!Plugin.PassiveSlotExpandEnabled.Value) return;
            if (player == null) return;

            int target = PassiveSlotHelper.GetTargetArrayLength();

            try
            {
                // 兜底：再次确保 Skill.shortCutSkillID 长度达标
                var skill = player.GetComponent<Skill>();
                if (skill != null)
                {
                    if (skill.shortCutSkillID == null
                        || skill.shortCutSkillID.Length < target)
                    {
                        var newArr = new int[target];
                        if (skill.shortCutSkillID != null)
                            Array.Copy(skill.shortCutSkillID, newArr,
                                skill.shortCutSkillID.Length);
                        skill.shortCutSkillID = newArr;
                    }
                    // 同步回 SavingStat，保证保存时也是新长度
                    SavingStat.shortCutSkillID = skill.shortCutSkillID;
                }

                // 兜底：再确保 AttackTrigger 数组长度
                ExpandAttackTriggerArrays(player, target);

                // 重跑 AssignAllSkill，让扩展槽 [20..] 的被动真正生效
                if (skill != null)
                    skill.AssignAllSkill();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("[ED3R] SettingNewPlayer Postfix error: " + e);
            }
        }

        private static void ExpandAttackTriggerArrays(GameObject player, int target)
        {
            var atk = player.GetComponent<AttackTrigger>();
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
    }
}