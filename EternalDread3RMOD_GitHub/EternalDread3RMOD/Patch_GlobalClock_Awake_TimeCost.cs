using HarmonyLib;
using UnityEngine.SceneManagement;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 新场景加载后：
    ///   1. Prefix：消费待处理的时间跳跃
    ///   2. Postfix：
    ///      a. 新游戏时，等玩家离开 IntroVillage 才应用生态
    ///      b. 懒更新所有区域的生态状态
    /// </summary>
    [HarmonyPatch(typeof(GlobalClock), "Awake")]
    public static class Patch_GlobalClock_Awake_TimeCost
    {
        static void Prefix(GlobalClock __instance)
        {
            GameTimeJump.TryConsumePending(__instance);
        }

        static void Postfix()
        {
            if (!Plugin.EcologyEnabled.Value) return;

            // ===== 新游戏：离开 IntroVillage 后才加载生态 =====
            if (!EcologyLoadHelper.HasApplied)
            {
                string scene = SceneManager.GetActiveScene().name;
                if (scene != "IntroVillage" && scene != "TitleED3")
                {
                    int slot = GlobalVar.eventsId[95];
                    EcologyLoadHelper.LoadAndApply(slot);
                }
            }

            // ===== 懒更新所有区域 =====
            EcologyRegistry.TickAll(GameTimeTracker.GetTotalGameHours());
        }
    }
}