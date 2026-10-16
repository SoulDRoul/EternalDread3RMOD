using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 升级经验需求调整（Prefix 完全重写 Status.LevelUp）。
    /// 
    /// 目标：升级时把新 maxExp 按段位百分比缩减。
    /// 
    /// 生效条件：ExpRequirementEnabled = true
    ///           __instance 是 GlobalVar.mainPlayer
    ///           未挂 ExpTable（保守判断）
    /// 
    /// 溢出处理：ExpClearOverflow = true 时，remainingEXP 丢弃，每次最多升 1 级。
    ///           ExpClearOverflow = false 时，保留溢出，可能连升。
    /// </summary>
    [HarmonyPatch(typeof(Status), "LevelUp")]
    public static class Patch_Status_LevelUp_ExpRequirement
    {
        // ============ 硬编码常量：ExpRequirement.ClearOverflow ============
        // 原 ExpRequirement.ClearOverflow —— 升级后是否清零溢出经验（true = 每次最多升 1 级）
        private static readonly bool CLEAR_OVERFLOW = true;

        static bool Prefix(Status __instance, int remainingEXP)
        {
            if (!Plugin.ExpRequirementEnabled.Value) return true;
            if (__instance.gameObject != GlobalVar.mainPlayer) return true;
            if (__instance.GetComponent<ExpTable>() != null) return true;
            if (__instance.level >= 100) return true;

            try
            {
                // ===== 1. 等级 +1，exp 处理 =====
                __instance.level++;
                __instance.exp = 0;
                if (!CLEAR_OVERFLOW)
                {
                    __instance.exp += remainingEXP;
                }

                if (__instance.level >= 100)
                {
                    __instance.level = 100;
                    __instance.exp = 0;
                    __instance.maxExp = 0;
                }
                else
                {
                    // ===== 2. 升级奖励 =====
                    __instance.statusPoint += 5;
                    __instance.skillPoint++;

                    // ===== 3. 算原版 maxExp，再按百分比缩 =====
                    int original = ExpRequirementHelper.ComputeOriginalMaxExp(__instance.level);
                    int percent = ExpRequirementHelper.GetPercentForLevel(__instance.level);
                    long newMax = (long)original * percent / 100;
                    if (newMax < 1) newMax = 1;
                    if (newMax > int.MaxValue) newMax = int.MaxValue;
                    __instance.maxExp = (int)newMax;
                }

                // ===== 4. 复刻原版后续副作用 =====
                var cps = __instance.GetComponent<CalculatePlayerStatus>();
                if (cps != null) cps.CalculateStatLv();

                __instance.CalculateTotalStatus();
                __instance.GainEXP(0);

                if (__instance.lvUpEffect != null)
                {
                    GameObject go = Object.Instantiate(
                        __instance.lvUpEffect,
                        __instance.transform.position,
                        __instance.transform.rotation);
                    go.transform.parent = __instance.transform;
                    AudioSource[] audios = go.GetComponentsInChildren<AudioSource>();
                    for (int i = 0; i < audios.Length; i++)
                    {
                        audios[i].volume = OptionData.effectVolume;
                    }
                }

                var skill = __instance.GetComponent<Skill>();
                if (skill != null) skill.LearnSkillByLevel(__instance.level);

                var ui = __instance.GetComponent<UiMaster>();
                if (ui != null) ui.ShowStatUp();

                var acs = __instance.GetComponent<AutoCalculateStatus>();
                if (acs != null) acs.CalculateStatLv();

                __instance.health = __instance.totalStatus.maxHealth;
                __instance.mana = __instance.totalStatus.maxMana;
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] ExpRequirement LevelUp patch error: " + e);
            }

            return false;
        }
    }
}