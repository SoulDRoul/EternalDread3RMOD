using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(GlobalVar), "ResetAllVariable")]
    public static class Patch_GlobalVar_ResetAllVariable_TimeTracker
    {
        static void Postfix()
        {
            GameTimeTracker.Reset();
            EcologyRegistry.Clear();
            EcologyLoadHelper.HasApplied = false;
            EcologyUI.SuppressRender = false;   // ★ 新增
        }
    }
}