using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(ResourceNode), "Extract")]
    public static class Patch_ResourceNode_Mark
    {
        // Prefix 比 Postfix 更稳：Extract 里可能生成多个物品，
        // 但都在这一帧内完成，Prefix 设置一次就够
        static void Prefix()
        {
            DropContext.lastDropFrame = Time.frameCount;
        }
    }
}