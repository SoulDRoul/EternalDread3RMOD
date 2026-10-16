using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// Status.OnDamage 统一修改层。合并三块逻辑，避免多个 Prefix 使用 __state 冲突：
    ///   1. 冰冻破防
    ///   2. 元素抗性修正（A3）
    ///   3. 附魔攻击穿透物理/魔法免疫（A1/A2）
    /// 
    /// 顺序：A3 → A1/A2 → 冰冻破防
    /// 冰冻破防优先级最高，覆盖前两层。
    /// </summary>
    [HarmonyPatch(typeof(Status), "OnDamage")]
    [HarmonyPriority(Priority.Low)]
    public static class Patch_Status_OnDamage_Modifiers
    {
        // ============ 硬编码常量：StatusEffects.ElementalImmunityThreshold ============
        // 原 StatusEffects.ElementalImmunityThreshold —— 元素抗性 <= 此值时视为"弱势元素"，触发修正
        private const int ELEMENTAL_IMMUNITY_THRESHOLD = 20;

        private class Saved
        {
            public int normal, fire, ice, electric, earth, holy, darkness, water, plant, wind;
            public int defense, magicDefense;
            public int dmgAbsorb, magicAbsorb;
        }

        static void Prefix(Status __instance,
            int elem, int dmgType,
            ref bool cantBlock, ref bool cantDodge,
            out Saved __state)
        {
            __state = null;

            bool frostPierce = Plugin.FrostPierceEnabled.Value
                               && __instance.onFrost > 0f;

            bool immunFix = Plugin.ElementalImmunityFixEnabled.Value
                            && IsTagAllowedForImmunity(__instance.gameObject);

            bool elemPierce = Plugin.ImmunityFixEnabled.Value
                              && elem != 0;

            if (!frostPierce && !immunFix && !elemPierce) return;

            var ts = __instance.totalStatus;

            // 快照全部可能被改动的字段
            var s = new Saved
            {
                normal = ts.normal,
                fire = ts.fire,
                ice = ts.ice,
                electric = ts.electric,
                earth = ts.earth,
                holy = ts.holy,
                darkness = ts.darkness,
                water = ts.water,
                plant = ts.plant,
                wind = ts.wind,
                defense = ts.defense,
                magicDefense = ts.magicDefense,
                dmgAbsorb = ts.dmgAbsorb,
                magicAbsorb = ts.magicAbsorb,
            };

            bool changed = false;

            // ============ 第一层：A3 元素抗性修正 ============
            // 判定：抗性 <= Threshold 视为"弱势元素"，触发修正。
            // 修正：把当前元素抗性提升到至少 Percent，并清空本次防御。
            if (immunFix)
            {
                int threshold = ELEMENTAL_IMMUNITY_THRESHOLD;
                int pct = Plugin.ElementalImmunityPercent.Value;
                bool currentElemLow = false;

                switch (elem)
                {
                    case 0: if (ts.normal <= threshold) { ts.normal = Mathf.Max(ts.normal, pct); currentElemLow = true; } break;
                    case 1: if (ts.fire <= threshold) { ts.fire = Mathf.Max(ts.fire, pct); currentElemLow = true; } break;
                    case 2: if (ts.ice <= threshold) { ts.ice = Mathf.Max(ts.ice, pct); currentElemLow = true; } break;
                    case 3: if (ts.electric <= threshold) { ts.electric = Mathf.Max(ts.electric, pct); currentElemLow = true; } break;
                    case 4: if (ts.earth <= threshold) { ts.earth = Mathf.Max(ts.earth, pct); currentElemLow = true; } break;
                    case 5: if (ts.holy <= threshold) { ts.holy = Mathf.Max(ts.holy, pct); currentElemLow = true; } break;
                    case 6: if (ts.darkness <= threshold) { ts.darkness = Mathf.Max(ts.darkness, pct); currentElemLow = true; } break;
                    case 7: if (ts.water <= threshold) { ts.water = Mathf.Max(ts.water, pct); currentElemLow = true; } break;
                    case 8: if (ts.plant <= threshold) { ts.plant = Mathf.Max(ts.plant, pct); currentElemLow = true; } break;
                    case 9: if (ts.wind <= threshold) { ts.wind = Mathf.Max(ts.wind, pct); currentElemLow = true; } break;
                }

                if (currentElemLow)
                {
                    ts.defense = 0;
                    ts.magicDefense = 0;
                    changed = true;
                }
            }

            // ============ 第二层：A1/A2 附魔穿透 ============
            // 附魔攻击走"错类型"路径时，把 90 的免疫降到 ElementalPiercePercent
            if (elemPierce)
            {
                int pct = Plugin.ElementalPiercePercent.Value;

                // 附魔物理武器打物理免疫怪：dmgType=0, dmgAbsorb>=90
                if (dmgType == 0 && ts.dmgAbsorb >= 90)
                {
                    ts.dmgAbsorb = pct;
                    changed = true;
                }
                // 附魔魔法武器打魔法免疫怪：dmgType=1, magicAbsorb>=90
                if (dmgType == 1 && ts.magicAbsorb >= 90)
                {
                    ts.magicAbsorb = pct;
                    changed = true;
                }
            }

            // ============ 第三层：冰冻破防（最高优先级，覆盖前两层）============
            if (frostPierce)
            {
                ts.defense = 0;
                ts.magicDefense = 0;
                ts.dmgAbsorb = 0;
                ts.magicAbsorb = 0;

                ts.normal = 100; ts.fire = 100; ts.ice = 100;
                ts.electric = 100; ts.earth = 100; ts.holy = 100;
                ts.darkness = 100; ts.water = 100; ts.plant = 100;
                ts.wind = 100;

                cantBlock = true;
                cantDodge = true;

                changed = true;
            }

            if (!changed) return;

            __state = s;
        }

        static void Postfix(Status __instance, Saved __state)
        {
            if (__state == null) return;

            var ts = __instance.totalStatus;
            ts.normal = __state.normal;
            ts.fire = __state.fire;
            ts.ice = __state.ice;
            ts.electric = __state.electric;
            ts.earth = __state.earth;
            ts.holy = __state.holy;
            ts.darkness = __state.darkness;
            ts.water = __state.water;
            ts.plant = __state.plant;
            ts.wind = __state.wind;
            ts.defense = __state.defense;
            ts.magicDefense = __state.magicDefense;
            ts.dmgAbsorb = __state.dmgAbsorb;
            ts.magicAbsorb = __state.magicAbsorb;
        }

        // A3 的目标过滤
        private static bool IsTagAllowedForImmunity(GameObject go)
        {
            int mode = Plugin.ElementalImmunityTargetMode.Value;
            if (mode <= 0) return true;

            string t = go.tag;
            if (mode == 1) return t == "Enemy";
            if (mode == 2) return t == "Enemy" || t == "Player";
            return true;
        }
    }
}