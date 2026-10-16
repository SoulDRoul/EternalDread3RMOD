using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>GameOver 出现时标记：本次复活需要跳时间。</summary>
    [HarmonyPatch(typeof(GameOver), "Start")]
    public static class Patch_GameOver_Start_Flag
    {
        static void Postfix()
        {
            DeathTimeSkipManager.MarkGameOver();
        }
    }
}