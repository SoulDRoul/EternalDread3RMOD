using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>玩家重生时，如果是 GameOver 触发的，执行死亡跳时间。</summary>
    [HarmonyPatch(typeof(GlobalVar), "SettingNewPlayer")]
    public static class Patch_GlobalVar_SettingNewPlayer_TimeCost
    {
        static void Postfix()
        {
            DeathTimeSkipManager.TryApply();
        }
    }
}