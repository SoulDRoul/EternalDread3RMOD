using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 读档 / 重生时缩减当前 maxExp 与 exp，保持进度百分比不变。
    /// 具体逻辑在 ExpRequirementHelper.TryApplyToStatus 里。
    /// </summary>
    [HarmonyPatch(typeof(GlobalVar), "SettingNewPlayer")]
    public static class Patch_GlobalVar_SettingNewPlayer_ExpRequirement
    {
        static void Postfix()
        {
            if (!Plugin.ExpRequirementEnabled.Value) return;
            if (GlobalVar.mainPlayer == null) return;

            Status status = GlobalVar.mainPlayer.GetComponent<Status>();
            if (status == null) return;

            try
            {
                ExpRequirementHelper.TryApplyToStatus(status);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] ExpRequirement SettingNewPlayer patch error: " + e);
            }
        }
    }
}