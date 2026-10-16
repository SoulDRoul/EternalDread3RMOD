using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>标题界面加载时重置所有 permadeath 状态。</summary>
    [HarmonyPatch(typeof(Title), "Start")]
    public static class Patch_Title_Start_Permadeath
    {
        static void Postfix()
        {
            PermadeathState.Reset();
        }
    }
}