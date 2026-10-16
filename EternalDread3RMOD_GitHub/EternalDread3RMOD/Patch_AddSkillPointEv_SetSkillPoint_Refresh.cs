using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 拾取技能点。Prefix 完全重写原版逻辑。
    /// 
    /// 拾取时：
    ///   - 清除 IsSpawned 标记（不再保留物体）
    ///   - 记录 LastPickup 时间（进入冷却）
    ///   - 首次拾取时才触发 AchievementCheck（保留成就）
    /// </summary>
    [HarmonyPatch(typeof(AddSkillPointEv), "SetSkillPoint")]
    public static class Patch_AddSkillPointEv_SetSkillPoint_Refresh
    {
        private static MethodInfo _achievementCheck;

        static bool Prefix(AddSkillPointEv __instance)
        {
            if (!Plugin.SkillPointRefreshEnabled.Value) return true;
            if (__instance == null) return true;

            int boolId = __instance.boolId;
            if (boolId <= 0 || boolId > 42) return true;

            try
            {
                if (!GlobalVar.mainPlayer)
                    GlobalVar.mainPlayer = GameObject.FindWithTag("Player");
                if (!GlobalVar.mainPlayer) return false;

                bool isFirstPickup = !EventSetting.globalBoolean[boolId];

                // 加技能点
                GlobalVar.mainPlayer.GetComponent<Status>().skillPoint += __instance.addSkillPoint;
                EventSetting.globalBoolean[boolId] = true;

                var ui = GlobalVar.mainPlayer.GetComponent<UiMaster>();
                if (ui != null) ui.ShowStatUp();

                GlobalVar.eventsId[72]++;

                // 只在首次拾取时触发成就
                if (isFirstPickup)
                {
                    if (_achievementCheck == null)
                        _achievementCheck = AccessTools.Method(typeof(AddSkillPointEv), "AchievementCheck");
                    if (_achievementCheck != null)
                        _achievementCheck.Invoke(__instance, null);
                }

                // 清除刷出标记，记录拾取时间（进入冷却）
                float now = GameTimeTracker.GetTotalGameHours();
                SkillPointRegistry.SetIsSpawned(boolId, false);
                SkillPointRegistry.RecordPickup(boolId, now);

                if (__instance.targetObject != null)
                    Object.Destroy(__instance.targetObject);

                Plugin.DebugLog("[ED3R] SkillPoint picked: boolId=" + boolId
                    + " firstPickup=" + isFirstPickup);

                return false;  // 跳过原版
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] SkillPointRefresh SetSkillPoint error: " + e);
                return true;  // 异常时走原版
            }
        }
    }
}