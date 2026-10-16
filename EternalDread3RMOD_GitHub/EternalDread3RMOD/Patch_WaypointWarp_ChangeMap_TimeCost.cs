using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 城镇快速传送阵（WaypointWarp）→ 记录待处理的时间跳跃。
    /// 实际跳跃发生在新场景 GlobalClock.Awake 时。
    /// </summary>
    [HarmonyPatch(typeof(WaypointWarp), "ChangeMap")]
    public static class Patch_WaypointWarp_ChangeMap_TimeCost
    {
        static void Prefix()
        {
            float minutes = Plugin.TeleporterCostMinutes.Value;
            if (minutes > 0f) GameTimeJump.SetPending(minutes / 60f);
        }
    }
}