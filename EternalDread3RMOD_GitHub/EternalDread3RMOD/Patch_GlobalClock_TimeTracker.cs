using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(GlobalClock), "TimeFwd")]
    public static class Patch_GlobalClock_TimeFwd_TimeTracker
    {
        static void Postfix()
        {
            GameTimeTracker.AdvanceMinutes(1f);
        }
    }

    [HarmonyPatch(typeof(GlobalClock), "CrossHour")]
    public static class Patch_GlobalClock_CrossHour_TimeTracker
    {
        static void Postfix(int hh)
        {
            GameTimeTracker.AdvanceHours(hh);
        }
    }
}