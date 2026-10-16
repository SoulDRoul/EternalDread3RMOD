using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 锥形视野：
    /// - 已追踪目标 → 不干预
    /// - 从无到有：
    ///   - Prefix 提前判断玩家是否在背后
    ///   - 如果在背后 → 临时禁用 callNearbyAlly.enable
    ///     （防止 CallNearbyUnit 里的 LookAt 污染 transform.forward）
    ///   - Postfix 用"进入方法前"的 forward 做角度判断
    ///   - 判定为背后 → 清空 followTarget + 还原朝向
    /// - 玩家攻击命中后，由 Patch_CallNearbyInterrupt 打断喊叫
    /// </summary>
    [HarmonyPatch]
    public static class Patch_ConeVision
    {
        private class State
        {
            public Transform followTarget;
            public Vector3 forward;
            public object callingAlly;
            public bool origEnable;
            public bool touchedEnable;
        }

        private static Dictionary<System.Type, FieldInfo> _callNearbyAllyCache
            = new Dictionary<System.Type, FieldInfo>();
        private static FieldInfo _callingAllyEnableField;

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(EnemyBasic), "FindClosestEnemy");
            yield return AccessTools.Method(typeof(EnemySkill), "FindClosestEnemy");
            yield return AccessTools.Method(typeof(AllyBasic), "FindClosestEnemy");
            yield return AccessTools.Method(typeof(FlyingEnemy), "FindClosestEnemy");
            yield return AccessTools.Method(typeof(EnemyCharging), "FindClosestEnemy");
            yield return AccessTools.Method(typeof(BeenionActivator), "FindClosestEnemy");
            yield return AccessTools.Method(typeof(Tuahia), "FindClosestEnemy");
        }

        static void Prefix(object __instance, out State __state)
        {
            __state = null;
            if (!Plugin.ConeVisionEnabled.Value) return;

            var comp = __instance as Component;
            if (comp == null) return;

            var ai = comp.GetComponent<AiStatus>();
            if (ai == null) return;

            var s = new State();
            s.followTarget = ai.followTarget;

            Vector3 fwd = comp.transform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
            fwd.Normalize();
            s.forward = fwd;

            // ===== 提前判断玩家是否在背后 =====
            // 如果在背后 → 临时禁用 CallNearbyUnit，防止原方法里同步执行的 LookAt
            // 污染 transform.forward，导致 Postfix 无法正确判断角度。
            var player = GlobalVar.mainPlayer;
            if (player != null)
            {
                Vector3 toPlayer = player.transform.position - comp.transform.position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    toPlayer.Normalize();
                    float angleToPlayer = Vector3.Angle(fwd, toPlayer);
                    float halfAngle = Plugin.ConeVisionAngle.Value * 0.5f;

                    if (angleToPlayer > halfAngle)
                    {
                        var ca = GetCallingAlly(comp);
                        if (ca != null && GetEnable(ca))
                        {
                            s.callingAlly = ca;
                            s.origEnable = true;
                            SetEnable(ca, false);
                            s.touchedEnable = true;
                        }
                    }
                }
            }

            __state = s;
        }

        static void Postfix(object __instance, State __state)
        {
            if (__state == null) return;

            if (__state.touchedEnable && __state.callingAlly != null)
            {
                SetEnable(__state.callingAlly, __state.origEnable);
            }

            if (!Plugin.ConeVisionEnabled.Value) return;
            if (__state.followTarget != null) return;

            var comp = __instance as Component;
            if (comp == null) return;

            var ai = comp.GetComponent<AiStatus>();
            if (ai == null) return;
            if (ai.followTarget == null) return;

            // ===== 近距感知 =====
            float closeRange = Plugin.ConeVisionCloseRange.Value;
            if (closeRange > 0f)
            {
                float d = Vector3.Distance(comp.transform.position, ai.followTarget.position);
                if (d < closeRange) return;
            }

            Vector3 toTarget = ai.followTarget.position - comp.transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f) return;
            toTarget.Normalize();

            float halfAngle = Plugin.ConeVisionAngle.Value * 0.5f;
            float angle = Vector3.Angle(__state.forward, toTarget);

            bool blocked = false;

            // ===== 角度判断：angry 时跳过 =====
            bool bypassAngle = Plugin.ConeVisionBypassWhenAngry.Value && ai.angry;
            if (!bypassAngle && angle > halfAngle)
            {
                blocked = true;
            }

            // ===== 墙检测：无论 angry 与否都生效 =====
            if (!blocked && Plugin.ConeVisionWallBlockEnabled.Value)
            {
                Vector3 eyePos = comp.transform.position;
                var cc = comp.GetComponent<CharacterController>();
                if (cc != null && cc.height > 0.1f)
                    eyePos += Vector3.up * (cc.height * 0.8f);
                else
                    eyePos += Vector3.up * 1.0f;

                Vector3 targetPos = ai.followTarget.position;
                var tcc = ai.followTarget.GetComponent<CharacterController>();
                if (tcc != null && tcc.height > 0.1f)
                    targetPos += Vector3.up * (tcc.height * 0.6f);
                else
                    targetPos += Vector3.up * 1.0f;

                Vector3 dir = targetPos - eyePos;
                float dist = dir.magnitude;

                if (dist > 0.01f)
                {
                    dir /= dist;
                    RaycastHit[] hits = Physics.RaycastAll(eyePos, dir, dist);
                    System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                    for (int i = 0; i < hits.Length; i++)
                    {
                        var hitT = hits[i].transform;

                        bool isSelf = (hitT == comp.transform) || hitT.IsChildOf(comp.transform);
                        if (isSelf) continue;

                        bool isTarget = (hitT == ai.followTarget) || hitT.IsChildOf(ai.followTarget);
                        if (isTarget) break;

                        blocked = true;
                        break;
                    }
                }
            }

            if (blocked)
            {
                ai.followTarget = null;

                // angry 时不要还原朝向（让它乱转去）
                if (!ai.angry)
                {
                    Vector3 curEuler = comp.transform.eulerAngles;
                    Vector3 savedEuler = Quaternion.LookRotation(__state.forward, Vector3.up).eulerAngles;
                    comp.transform.rotation = Quaternion.Euler(curEuler.x, savedEuler.y, curEuler.z);
                }
            }
        }

        // ===== 用 System.Reflection 静默查找 =====

        private static object GetCallingAlly(Component comp)
        {
            var t = comp.GetType();
            FieldInfo f;
            if (!_callNearbyAllyCache.TryGetValue(t, out f))
            {
                f = t.GetField("callNearbyAlly",
                    BindingFlags.Public | BindingFlags.Instance);
                _callNearbyAllyCache[t] = f;
            }
            if (f == null) return null;
            return f.GetValue(comp);
        }

        private static bool GetEnable(object callingAlly)
        {
            if (callingAlly == null) return false;
            EnsureEnableField(callingAlly);
            if (_callingAllyEnableField == null) return false;
            return (bool)_callingAllyEnableField.GetValue(callingAlly);
        }

        private static void SetEnable(object callingAlly, bool val)
        {
            if (callingAlly == null) return;
            EnsureEnableField(callingAlly);
            if (_callingAllyEnableField != null)
                _callingAllyEnableField.SetValue(callingAlly, val);
        }

        private static void EnsureEnableField(object callingAlly)
        {
            if (_callingAllyEnableField == null)
            {
                _callingAllyEnableField = callingAlly.GetType().GetField("enable",
                    BindingFlags.Public | BindingFlags.Instance);
            }
        }
    }
}