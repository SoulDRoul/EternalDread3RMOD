using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 格挡可打断（攻击/技能键）。
    /// 格挡期间按攻击或技能键，立即打断格挡动画并执行动作。
    /// 跳跃键、方向键不打断（避免误触，且跳跃动画与格挡姿势视觉冲突）。
    /// 
    /// 闪避打断由 Patch_PlayerInputController_GuardDodge 处理。
    /// </summary>
    [HarmonyPatch(typeof(AttackTrigger), "Update")]
    public static class Patch_AttackTrigger_GuardInterrupt
    {
        private static readonly KeyCode[] SkillKeys = new KeyCode[]
        {
            ButtonInputOption.skill1,
            ButtonInputOption.skill2,
            ButtonInputOption.skill3,
            ButtonInputOption.skill4,
            ButtonInputOption.skill5,
            ButtonInputOption.skill6,
            ButtonInputOption.skill7,
            ButtonInputOption.skill8,
        };

        static void Prefix(AttackTrigger __instance)
        {
            if (!Plugin.GuardInterruptEnabled.Value) return;
            if (__instance.gameObject != GlobalVar.mainPlayer) return;

            var stat = __instance.GetComponent<Status>();
            if (stat == null || !stat.block) return;

            bool interrupt = false;

            // 攻击
            if (Input.GetKeyDown(ButtonInputOption.attack)
                || Input.GetKeyDown(ButtonInputOption.joyAttack))
                interrupt = true;

            // 技能 1~8
            if (!interrupt)
            {
                for (int i = 0; i < SkillKeys.Length; i++)
                {
                    if (Input.GetKeyDown(SkillKeys[i]))
                    {
                        interrupt = true;
                        break;
                    }
                }
            }

            if (!interrupt) return;

            stat.StopCoroutine("GuardUp");
            stat.GuardBreak();
            ResetPlayerAnimation(stat);
        }

        private static void ResetPlayerAnimation(Status stat)
        {
            if (stat == null) return;
            var legacy = stat.GetComponent<LegacyAnimationSet>();
            if (legacy != null)
            {
                try { legacy.ForceIdle(); }
                catch { }
            }
        }
    }
}