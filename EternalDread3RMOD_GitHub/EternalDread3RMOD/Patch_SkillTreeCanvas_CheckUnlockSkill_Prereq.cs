using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 技能组前置放宽。
    /// 
    /// 原版：解锁下一组技能需要点满当前组全部前置（num >= Length）。
    /// 本 patch：只要点够 N 个前置即可（num >= min(N, Length)）。
    /// 
    /// 总开关关闭时，走原版逻辑。
    /// 非玩家、异常情况直接走原版。
    /// </summary>
    [HarmonyPatch(typeof(SkillTreeCanvas), "CheckUnlockSkill")]
    public static class Patch_SkillTreeCanvas_CheckUnlockSkill_Prereq
    {
        static bool Prefix(SkillTreeCanvas __instance)
        {
            if (!Plugin.SkillPrereqRelaxEnabled.Value) return true;
            if (__instance == null) return true;
            if (__instance.player == null) return true;

            try
            {
                __instance.CheckLearnedSkill();

                var skill = __instance.player.GetComponent<Skill>();
                var status = __instance.player.GetComponent<Status>();
                if (skill == null || status == null) return false;

                int playerLevel = status.level;
                int requiredCount = Plugin.SkillPrereqRequiredCount.Value;
                if (requiredCount < 1) requiredCount = 1;

                var slots = __instance.skillSlots;
                if (slots == null) return false;

                for (int i = 0; i < slots.Length; i++)
                {
                    var slot = slots[i];
                    if (slot == null) continue;

                    // 等级门槛仍按原版
                    if (playerLevel < slot.unlockLevel) continue;

                    // 前置条件判断
                    int[] cond = slot.unlockConditionId;
                    if (cond != null && cond.Length > 0)
                    {
                        int needCount = requiredCount < cond.Length ? requiredCount : cond.Length;
                        int haveCount = 0;
                        for (int j = 0; j < cond.Length; j++)
                        {
                            if (skill.HaveSkill(cond[j])) haveCount++;
                        }
                        if (haveCount < needCount) continue;
                    }

                    // 满足条件 → 解锁
                    slot.locked = false;
                }
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] SkillPrereq patch error: " + e);
            }

            return false;
        }
    }
}