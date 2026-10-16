using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 原版 ShowSignetSetting 刷新被动槽时用 i + 8 的公式，
    /// 导致扩展槽 icon 8..11 读到原版脏区 [16..19]，显示空白。
    ///
    /// 这里在 Postfix 里重新设置扩展槽的 icon：
    ///   原版 8 个槽（i=0..7）  → 原版逻辑已处理，不管
    ///   扩展槽（i=8..N-1）     → 对应 shortCutSkillID[20 + (i-8)]
    /// 空槽一律用 blank 图标，避免出现白色空块。
    /// </summary>
    [HarmonyPatch(typeof(SkillTreeCanvas), "ShowSignetSetting")]
    public static class Patch_SkillTreeCanvas_ShowSignetSetting_ExpandIcons
    {
        static void Postfix(SkillTreeCanvas __instance)
        {
            if (!Plugin.PassiveSlotExpandEnabled.Value) return;
            if (__instance == null) return;
            if (__instance.signetIcons == null) return;
            if (GlobalVar.mainPlayer == null) return;

            var atk = GlobalVar.mainPlayer.GetComponent<AttackTrigger>();
            if (atk == null || atk.skills == null) return;

            var icons = __instance.signetIcons;
            Sprite blank = PassiveSlotHelper.GetBlankIcon(__instance);

            for (int i = PassiveSlotHelper.BaseSlotCount; i < icons.Length; i++)
            {
                if (icons[i] == null) continue;

                int rel = i - PassiveSlotHelper.BaseSlotCount;
                int skillIdx = PassiveSlotHelper.VanillaArrayLength + rel;

                Sprite sp = blank;
                if (skillIdx < atk.skills.Length
                    && atk.skills[skillIdx] != null
                    && atk.skills[skillIdx].icon != null)
                {
                    sp = atk.skills[skillIdx].icon;
                }

                icons[i].sprite = sp;
                icons[i].color = Color.white;
            }
        }
    }
}