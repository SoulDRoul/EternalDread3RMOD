using HarmonyLib;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(RespawnMonster), "Start")]
    public static class Patch_RespawnMonster_Start_EcologySpawn
    {
        static bool Prefix(RespawnMonster __instance)
        {
            if (EcologySpawnGuard.ShouldBypass()) return true;
            if (!Plugin.EcologyEnabled.Value) return true;

            // ★ 新增：本场景是否启用生态
            if (!EcologySceneState.IsEcologyEnabledForCurrentScene()) return true;

            var comp = __instance.gameObject.AddComponent<RespawnMonsterEcologyDelay>();
            comp.Init(__instance);
            return true;
        }
    }
}