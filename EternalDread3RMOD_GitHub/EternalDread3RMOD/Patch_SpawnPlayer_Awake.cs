using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(SpawnPlayer), "Awake")]
    public static class Patch_SpawnPlayer_Awake
    {
        static void Prefix(SpawnPlayer __instance)
        {
            // 场景判断：
            // 如果玩家已经存在（从主菜单进游戏，玩家被 DontDestroyOnLoad 保留）
            //   → SpawnPlayer 会把 walkSpeed 重设回 6，需要提前把 __instance.walkSpeed 改大
            // 如果玩家不存在（直接进游戏，SpawnPlayer 会创建玩家）
            //   → 让玩家的 Start 补丁自己处理，不在这里改
            if (GlobalVar.mainPlayer == null) return;

            float mul = Plugin.WalkSpeedMul.Value / 100f;
            float oldWalk = __instance.walkSpeed;
            __instance.walkSpeed = oldWalk * mul;

            Plugin.DebugLog("[SpawnPlayer] walkSpeed " + oldWalk
                + " -> " + __instance.walkSpeed);
        }
    }
}