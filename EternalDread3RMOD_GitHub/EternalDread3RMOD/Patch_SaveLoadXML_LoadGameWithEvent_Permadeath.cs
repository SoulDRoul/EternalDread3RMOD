using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 拦截读档：已死亡的 permadeath 存档不能读。
    /// 之后会加 UI 提示；当前版本只写日志。
    /// </summary>
    [HarmonyPatch(typeof(SaveLoadXML), "LoadGameWithEvent")]
    public static class Patch_SaveLoadXML_LoadGameWithEvent_Permadeath
    {
        static bool Prefix(SaveLoadXML __instance, int sll)
        {
            int pageMultiply = GetPageMultiply(__instance);
            int realSlot = sll + pageMultiply;

            if (PermadeathFileHelper.ReadDead(realSlot))
            {
                if (Plugin.Log != null)
                    Plugin.Log.LogInfo("[ED3R] Load blocked: dead permadeath slot " + realSlot);
                return false;
            }
            return true;
        }

        private static int GetPageMultiply(SaveLoadXML instance)
        {
            try
            {
                var f = AccessTools.Field(typeof(SaveLoadXML), "pageMultiply");
                if (f != null)
                    return (int)f.GetValue(instance);
            }
            catch { }
            return 0;
        }
    }
}