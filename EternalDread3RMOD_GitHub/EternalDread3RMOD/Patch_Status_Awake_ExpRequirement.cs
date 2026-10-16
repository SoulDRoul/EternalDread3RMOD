using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 新建角色时，Status.Awake 是 1 级 maxExp 唯一的写入时机。
    /// 新建角色不走 GlobalVar.SettingNewPlayer，所以必须在这里补一次
    /// ExpRequirementHelper.TryApplyToStatus。
    ///
    /// 过滤条件用 tag == "Player"：Awake 阶段 GlobalVar.mainPlayer 还没赋值。
    /// 其他 Status（敌人 / 盟友 / 箱子）tag 都不是 Player，不受影响。
    /// </summary>
    [HarmonyPatch(typeof(Status), "Awake")]
    public static class Patch_Status_Awake_ExpRequirement
    {
        static void Postfix(Status __instance)
        {
            if (!Plugin.ExpRequirementEnabled.Value) return;
            if (__instance == null) return;
            if (__instance.gameObject.tag != "Player") return;

            try
            {
                ExpRequirementHelper.TryApplyToStatus(__instance);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] ExpRequirement Status.Awake patch error: " + e);
            }
        }
    }
}