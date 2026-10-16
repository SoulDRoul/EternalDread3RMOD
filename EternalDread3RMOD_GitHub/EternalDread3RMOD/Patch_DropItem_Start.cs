using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(DropItem), "Start")]
    public static class Patch_DropItem_Start
    {
        static bool Prefix(DropItem __instance)
        {
            if (!Plugin.LevelBonusEnabled.Value) return true;
            if (GlobalVar.mainPlayer == null) return true;
            var playerStat = GlobalVar.mainPlayer.GetComponent<Status>();
            if (playerStat == null) return true;

            var gainExp = __instance.GetComponent<GainEXP>();
            int enemyLevel = gainExp != null ? gainExp.realLv : 1;
            int diff = enemyLevel - playerStat.level;

            for (int i = 0; i < __instance.itemDropSetting.Length; i++)
            {
                int dropChance = __instance.itemDropSetting[i].dropChance;
                if (diff > 0)
                {
                    int percent = 100 + diff * Plugin.LevelDropPercentPerLevel.Value;
                    dropChance = dropChance * percent / 100;
                }
                if (dropChance > 100) dropChance = 100;

                int num = Random.Range(0, 100);
                if (num <= dropChance)
                {
                    Vector3 position = __instance.transform.position;
                    position.x += Random.Range(0f, __instance.randomPosition);
                    position.z += Random.Range(0f, __instance.randomPosition);
                    position.y += __instance.dropUpward;

                    GameObject go = Object.Instantiate(__instance.itemDropSetting[i].itemPrefab, position, __instance.transform.rotation);
                    go.name = __instance.itemDropSetting[i].itemPrefab.name;
                }
            }
            return false;
        }
    }
}