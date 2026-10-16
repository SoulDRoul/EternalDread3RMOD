using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(ChestAddItem), "Start")]
    public static class Patch_ChestAddItem_Start_Minimap
    {
        static void Postfix(ChestAddItem __instance)
        {
            if (!Plugin.MinimapChestEnabled.Value) return;
            if (IsOpened(__instance)) return;
            if (__instance.GetComponent<MinimapIconTag>() != null) return;

            var color = MinimapIconManager.ParseColor(
                Plugin.MinimapChestColor.Value,
                new Color(1f, 0.63f, 0.16f));
            float h = MinimapIconManager.MM_CHEST_HEIGHT / 100f;
            float size = MinimapIconManager.MM_SIZE_BASE
                * MinimapIconManager.MM_CHEST_PCT / 10000f;
            int order = MinimapIconManager.MM_CHEST_ORDER;

            var icon = MinimapIconManager.AttachIcon(
                __instance.gameObject,
                color,
                h,
                size,
                IconShape.Square,
                MinimapIconCategory.Chest,
                null,
                order);

            __instance.gameObject.AddComponent<MinimapIconTag>();

            if (icon != null)
            {
                var w = __instance.gameObject.GetComponent<ChestMinimapWatcher>();
                if (w == null)
                    w = __instance.gameObject.AddComponent<ChestMinimapWatcher>();
                w.icon = icon;
                w.chest = __instance;
            }
        }

        public static bool IsOpened(ChestAddItem chest)
        {
            if (chest.useSapling)
                return GlobalVar.sapling[chest.localId] > 0;
            return GlobalVar.chests[chest.localId];
        }
    }

    public class ChestMinimapWatcher : MonoBehaviour
    {
        public MinimapIcon icon;
        public ChestAddItem chest;

        private float _nextCheck;

        private void Update()
        {
            if (icon == null) { Destroy(this); return; }

            if (Time.time < _nextCheck) return;
            _nextCheck = Time.time + 0.5f;

            if (chest == null) { Destroy(icon.gameObject); Destroy(this); return; }
            if (Patch_ChestAddItem_Start_Minimap.IsOpened(chest))
            {
                Destroy(icon.gameObject);
                Destroy(this);
            }
        }
    }
}