using HarmonyLib;
using UnityEngine.UI;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 切换 tab（主职业/副职业）时：
    ///   1. 被动职业重置为新 tab 的职业
    ///   2. 刷新职业名称标签
    ///   3. 刷新成本倍率标签
    ///
    /// 关键：只有 tab 真正变化时才处理。
    /// 玩家按"未解锁的副职业按钮"时，原版 SwitchJobTab 内部什么都不做，
    /// 此时不能重置 CurrentPassiveJob，否则会出现"名称是主职业、
    /// 图标还是之前切到的职业"的矛盾状态。
    /// </summary>
    [HarmonyPatch(typeof(SkillTreeCanvas), "SwitchJobTab")]
    public static class Patch_SkillTreeCanvas_SwitchJobTab_CrossPassive
    {
        // Prefix 记录切换前的 active tab 索引，通过 __state 传给 Postfix
        static void Prefix(SkillTreeCanvas __instance, out int __state)
        {
            __state = CrossPassiveHelper.GetCurrentTab(__instance);
        }

        static void Postfix(SkillTreeCanvas __instance, int __state)
        {
            if (!Plugin.CrossPassiveEnabled.Value) return;
            if (__instance == null) return;

            int afterTab = CrossPassiveHelper.GetCurrentTab(__instance);

            // tab 没有变化：例如玩家按了未解锁的副职业按钮，
            // 原版 SwitchJobTab 内部什么都没做。此时不应重置被动职业、
            // 不应刷新标签，保持 UI 状态一致。
            if (afterTab == __state) return;
            if (afterTab < 0) return;

            // tab 真正切换了：重置被动职业为新 tab 的职业
            CrossPassiveHelper.CurrentPassiveJob = afterTab;

            if (__instance.jobSwitchTabSprite == null
                || __instance.jobSwitchTabSprite.Length < 2
                || __instance.jobSwitchTabSprite[1] == null
                || __instance.jobSwitchTabSprite[1].btn == null) return;

            var parent = __instance.jobSwitchTabSprite[1].btn.transform.parent;
            if (parent == null) return;

            var labelGo = parent.Find("CrossPassiveJobLabel");
            if (labelGo != null)
            {
                var text = labelGo.GetComponent<Text>();
                if (text != null)
                    text.text = CrossPassiveHelper.GetJobName(afterTab);
            }

            var mulGo = parent.Find("CrossPassiveMulLabel");
            if (mulGo != null)
            {
                var text = mulGo.GetComponent<Text>();
                if (text != null)
                    text.text = CrossPassiveButtonUI.FormatMulLabel(__instance);
            }
        }
    }
}