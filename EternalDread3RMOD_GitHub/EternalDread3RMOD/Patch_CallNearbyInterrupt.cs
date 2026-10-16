using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 敌人被攻击命中时，打断正在进行的 CallNearbyUnit 喊叫。
    /// 
    /// 原理：
    /// - CallNearbyUnit 协程开头会设 EnemyBasic.freeze = true + AiStatus.freeze = true
    /// - 通过这两个字段判断"是否正在喊叫"
    /// - StopCoroutine + 恢复 freeze，敌人立即恢复行动
    /// 
    /// 注：呼叫援军（SendMessage "Aggressive"）在动画播放的第一帧就已经发出，
    ///     所以打断喊叫不会打断援军。只是让敌人不再被动画卡住。
    /// </summary>
    [HarmonyPatch(typeof(Status), "OnDamage")]
    [HarmonyPriority(Priority.Low)]
    public static class Patch_CallNearbyInterrupt
    {
        // 缓存的 FieldInfo
        private static readonly System.Type[] _callingTypes = new System.Type[]
        {
            typeof(EnemyBasic),
            typeof(EnemySkill),
            typeof(FlyingEnemy),
            typeof(AllyBasic),
            typeof(EnemyCharging),
            typeof(Tuahia),
        };

        private static readonly System.Collections.Generic.Dictionary<System.Type, System.Reflection.FieldInfo>
            _freezeFieldCache = new System.Collections.Generic.Dictionary<System.Type, System.Reflection.FieldInfo>();

        static void Postfix(Status __instance, string __result)
        {
            if (!Plugin.InterruptCallNearbyOnDamage.Value) return;
            if (__instance.dead) return;

            // 只有真实命中才处理（返回纯数字）
            int dmg;
            if (!int.TryParse(__result, out dmg)) return;
            if (dmg <= 0) return;

            var go = __instance.gameObject;

            var ai = go.GetComponent<AiStatus>();
            if (ai == null) return;

            for (int i = 0; i < _callingTypes.Length; i++)
            {
                var t = _callingTypes[i];
                var comp = go.GetComponent(t) as MonoBehaviour;
                if (comp == null) continue;

                System.Reflection.FieldInfo f;
                if (!_freezeFieldCache.TryGetValue(t, out f))
                {
                    f = AccessTools.Field(t, "freeze");
                    _freezeFieldCache[t] = f;
                }
                if (f == null) continue;

                bool freeze = (bool)f.GetValue(comp);
                if (!freeze) continue;

                // 只有 AiStatus.freeze 也是 true 才是"喊叫状态"
                // （其他机制比如击晕不会设 AiStatus.freeze）
                if (!ai.freeze) continue;

                // 打断
                comp.StopCoroutine("CallNearbyUnit");
                f.SetValue(comp, false);
                ai.freeze = false;

                Plugin.DebugLog("[ConeVision] 打断 " + go.name + " 的呼喊");
                return;
            }
        }
    }
}