using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(AddItem), "Start")]
    public static class Patch_AddItem_Start
    {
        static void Postfix(AddItem __instance)
        {
            if (!Plugin.PickupRangeEnabled.Value) return;

            int diff = Time.frameCount - DropContext.lastDropFrame;
            if (diff > 1) return;

            float mul = Plugin.PickupRangePercent.Value / 100f;
            if (mul <= 1.0f) return;

            Transform root = __instance.transform.root;
            foreach (var col in root.GetComponentsInChildren<Collider>(true))
            {
                if (col == null) continue;
                if (!col.isTrigger) continue;

                var sphere = col as SphereCollider;
                if (sphere != null) { sphere.radius *= mul; continue; }

                var capsule = col as CapsuleCollider;
                if (capsule != null) { capsule.radius *= mul; continue; }

                var box = col as BoxCollider;
                if (box != null) { box.size *= mul; continue; }
            }
        }
    }
}