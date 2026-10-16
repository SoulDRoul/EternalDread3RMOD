using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 走地图出入口 → 记录待处理的时间跳跃。
    /// 
    /// 实际跳跃发生在【新场景 GlobalClock.Awake】时（见 Patch_GlobalClock_Awake_TimeCost）。
    /// 这样玩家看不到 clock 跳变和天色变化（此时屏幕已切换）。
    /// </summary>
    [HarmonyPatch(typeof(Teleporter), "ChangeMap")]
    public static class Patch_Teleporter_ChangeMap_TimeCost
    {
        static void Prefix()
        {
            float hours = Plugin.MapExitCostHours.Value;
            if (hours > 0f) GameTimeJump.SetPending(hours);
        }
    }
}