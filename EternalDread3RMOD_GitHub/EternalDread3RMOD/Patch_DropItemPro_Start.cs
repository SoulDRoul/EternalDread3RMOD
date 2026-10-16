using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(DropItemPro), "Start")]
    public static class Patch_DropItemPro_Start
    {
        static bool Prefix(DropItemPro __instance)
        {
            if (!Plugin.LevelBonusEnabled.Value) return true;
            if (GlobalVar.mainPlayer == null) return true;
            var playerStat = GlobalVar.mainPlayer.GetComponent<Status>();
            if (playerStat == null) return true;

            var gainExp = __instance.GetComponent<GainEXP>();
            int enemyLevel = gainExp != null ? gainExp.realLv : __instance.level;
            int diff = enemyLevel - playerStat.level;

            for (int i = 0; i < __instance.itemDrop.Length; i++)
            {
                int dropChance = __instance.itemDrop[i].dropChance;
                if (diff > 0)
                {
                    int percent = 100 + diff * Plugin.LevelDropPercentPerLevel.Value;
                    dropChance = dropChance * percent / 100;
                }
                if (dropChance > 100) dropChance = 100;

                int num = Random.Range(0, 100);
                if (num <= dropChance
                    && (float)enemyLevel >= __instance.itemDrop[i].levelRange.x
                    && (float)enemyLevel <= __instance.itemDrop[i].levelRange.y)
                {
                    Vector3 position = __instance.transform.position;
                    position.x += Random.Range(0f, __instance.randomPosition);
                    position.z += Random.Range(0f, __instance.randomPosition);
                    position.y += __instance.dropUpward;

                    GameObject go = Object.Instantiate(__instance.itemDrop[i].itemPrefab, position, __instance.transform.rotation);
                    if (__instance.itemDrop[i].dropModify == DrType.AssignId)
                    {
                        go.GetComponentInChildren<AddItem>().itemID = __instance.itemDrop[i].id;
                        go.GetComponentInChildren<AddItem>().itemQuantity = __instance.itemDrop[i].quantity;
                    }
                    else if (__instance.itemDrop[i].dropModify == DrType.Cash)
                    {
                        go.GetComponentInChildren<AddCash>().cashMin = __instance.itemDrop[i].cashMin;
                        go.GetComponentInChildren<AddCash>().cashMax = __instance.itemDrop[i].cashMax;
                    }
                }
            }
            return false; // 跳过原方法
        }
    }
}