using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(DropItem), "Start")]
    public static class Patch_DropItem_Mark
    {
        static void Postfix() { DropContext.lastDropFrame = Time.frameCount; }
    }

    [HarmonyPatch(typeof(DropItemPro), "Start")]
    public static class Patch_DropItemPro_Mark
    {
        static void Postfix() { DropContext.lastDropFrame = Time.frameCount; }
    }

    [HarmonyPatch(typeof(DropOneItem), "Start")]
    public static class Patch_DropOneItem_Mark
    {
        static void Postfix() { DropContext.lastDropFrame = Time.frameCount; }
    }
}