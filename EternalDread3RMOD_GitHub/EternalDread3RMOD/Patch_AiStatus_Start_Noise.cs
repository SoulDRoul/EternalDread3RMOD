using HarmonyLib;

namespace EternalDread3RMOD
{
    /// <summary>
    /// AiStatus.Start 时：
    ///   1. 注册到 NoiseRegistry
    ///   2. 挂载 NoiseTracker
    /// </summary>
    [HarmonyPatch(typeof(AiStatus), "Start")]
    public static class Patch_AiStatus_Start_Noise
    {
        static void Postfix(AiStatus __instance)
        {
            if (!Plugin.NoiseEnabled.Value) return;

            NoiseRegistry.Register(__instance);

            if (__instance.GetComponent<NoiseTracker>() != null) return;
            __instance.gameObject.AddComponent<NoiseTracker>();
        }
    }
}