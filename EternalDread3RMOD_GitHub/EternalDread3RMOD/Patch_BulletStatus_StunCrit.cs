using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 眩晕中的目标受到攻击时，攻击者本次的暴击率临时 +N%。
    /// 用 __state 保存原值，Postfix 恢复，穿透子弹也不会污染后续目标。
    /// Priority.High 保证在 Patch_BulletStatus_MultiCrit(Low) 之前执行，
    /// 使得加成的暴击率参与多重暴击的 overflow 计算。
    /// </summary>
    [HarmonyPatch(typeof(BulletStatus), "DealingDamage")]
    [HarmonyPriority(Priority.High)]
    public static class Patch_BulletStatus_StunCrit
    {
        static void Prefix(BulletStatus __instance, GameObject other, out int __state)
        {
            __state = -1;   // -1 = 未修改（criticalPlus 永远非负，安全哨兵）
            if (!Plugin.StunCritBonusEnabled.Value) return;
            if (other == null) return;

            var stat = other.GetComponent<Status>();
            if (stat == null) return;
            if (stat.onStun <= 0f) return;

            __state = __instance.criticalPlus;
            __instance.criticalPlus += Plugin.StunCritBonusPercent.Value;
        }

        static void Postfix(BulletStatus __instance, int __state)
        {
            if (__state < 0) return;
            __instance.criticalPlus = __state;
        }
    }
}