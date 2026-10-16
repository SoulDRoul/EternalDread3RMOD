using HarmonyLib;

namespace EternalDread3RMOD
{
    // 异常状态
    [HarmonyPatch(typeof(Status), "ApplyAbnormalStat")]
    public static class Patch_Status_ApplyAbnormalStat
    {
        static bool Prefix(Status __instance)
        {
            if (InvincibleData.IsInvincible(__instance))
                return false;
            return true;
        }
    }

    // 即死
    [HarmonyPatch(typeof(Status), "InstantKilled")]
    public static class Patch_Status_InstantKilled
    {
        static bool Prefix(Status __instance)
        {
            if (InvincibleData.IsInvincible(__instance))
                return false;
            return true;
        }
    }

    // 陷阱/尖刺
    [HarmonyPatch(typeof(Status), "SpikeDamage")]
    public static class Patch_Status_SpikeDamage
    {
        static bool Prefix(Status __instance)
        {
            if (InvincibleData.IsInvincible(__instance))
                return false;
            return true;
        }
    }

    // 手绑
    [HarmonyPatch(typeof(Status), "InflictHandTied")]
    public static class Patch_Status_InflictHandTied
    {
        static bool Prefix(Status __instance)
        {
            if (InvincibleData.IsInvincible(__instance))
                return false;
            return true;
        }
    }

    // 网/捆绑
    [HarmonyPatch(typeof(Status), "InflictTiedUp")]
    public static class Patch_Status_InflictTiedUp
    {
        static bool Prefix(Status __instance)
        {
            if (InvincibleData.IsInvincible(__instance))
                return false;
            return true;
        }
    }

    // 击飞
    [HarmonyPatch(typeof(Status), "BounceUp")]
    public static class Patch_Status_BounceUp
    {
        static bool Prefix(Status __instance)
        {
            if (InvincibleData.IsInvincible(__instance))
                return false;
            return true;
        }
    }

    // 击倒
    [HarmonyPatch(typeof(Status), "KnockDown")]
    public static class Patch_Status_KnockDown
    {
        static bool Prefix(Status __instance)
        {
            if (InvincibleData.IsInvincible(__instance))
                return false;
            return true;
        }
    }

    // 掉落伤害
    [HarmonyPatch(typeof(Status), "FallingDamage")]
    public static class Patch_Status_FallingDamage
    {
        static bool Prefix(Status __instance)
        {
            if (InvincibleData.IsInvincible(__instance))
                return false;
            return true;
        }
    }
}