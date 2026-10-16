using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 格挡动画播放时附加音效。
    /// 音效来源：玩家当前武器的攻击音效（AttackTrigger.attackSetting.voice）。
    /// 触发点：LegacyAnimationSet.GuardAnimation / MecanimAnimationSet.GuardAnimation。
    /// 
    /// 原版格挡只有动画无声音，玩家反馈弱。
    /// 格挡成功（Blocked）的音效由原版处理，这里只补姿态启动音。
    /// </summary>
    [HarmonyPatch(typeof(LegacyAnimationSet), "GuardAnimation")]
    public static class Patch_LegacyAnimationSet_GuardSound
    {
        static void Postfix(LegacyAnimationSet __instance)
        {
            GuardSoundHelper.Play(__instance);
        }
    }

    [HarmonyPatch(typeof(MecanimAnimationSet), "GuardAnimation")]
    public static class Patch_MecanimAnimationSet_GuardSound
    {
        static void Postfix(MecanimAnimationSet __instance)
        {
            GuardSoundHelper.Play(__instance);
        }
    }

    internal static class GuardSoundHelper
    {
        public static void Play(MonoBehaviour owner)
        {
            if (!Plugin.GuardSoundEnabled.Value) return;
            if (owner == null) return;

            var go = owner.gameObject;
            if (go != GlobalVar.mainPlayer) return;

            var atk = go.GetComponent<AttackTrigger>();
            if (atk == null) return;
            if (atk.attackSetting == null) return;

            var voice = atk.attackSetting.voice;
            if (voice == null) return;

            var audio = go.GetComponent<AudioSource>();
            if (audio == null) return;

            audio.volume = OptionData.effectVolume;
            audio.PlayOneShot(voice);
        }
    }
}