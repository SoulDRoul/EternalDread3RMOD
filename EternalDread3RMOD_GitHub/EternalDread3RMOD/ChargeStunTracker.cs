using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 冲锋追踪器。
    /// 核心判定（自晕）：
    ///   1. 起步窗口：累积位移不足 → "起步被阻止"直接触发。
    ///   2. Running：当前速度 / 峰值速度 低于阈值持续 N 帧触发。
    /// 2A：玩家自晕时，本次冲锋命中过的敌人也被随机施加 眩晕/击倒。
    /// 2B：命中伤害按"冲锋起点 → 命中点"距离缩放。
    /// </summary>
    public class ChargeStunTracker : MonoBehaviour
    {
        // ============ 硬编码常量：Combat.ChargeStun 内部阈值 ============
        // 原 Combat.ChargeStun.IncludeSlidePhase —— EnemyCharging 的滑行阶段是否纳入检测
        private const bool INCLUDE_SLIDE_PHASE = true;
        // 原 Combat.ChargeStun.ExemptDuringHurtInvincible —— 受击无敌期豁免自晕
        private const bool EXEMPT_DURING_HURT_INVINCIBLE = true;
        // 原 Combat.ChargeStun.SpeedDropRatio —— 触发阈值：当前速度 / 冲锋速度 低于此值视为被阻止
        private const float SPEED_DROP_RATIO = 0.55f;
        // 原 Combat.ChargeStun.MinChargeSpeed —— 冲锋期间最高速度低于此值不检测
        private const float MIN_CHARGE_SPEED = 3.0f;
        // 原 Combat.ChargeStun.StartupWindowSec —— 起步窗口时长（秒）
        private const float STARTUP_WINDOW_SEC = 0.15f;
        // 原 Combat.ChargeStun.StartupMinDistance —— 起步窗口内最小位移（米）
        private const float STARTUP_MIN_DISTANCE = 0.5f;
        // 原 Combat.ChargeStun.RequiredFrames —— 连续 N 帧满足阈值才触发
        private const int REQUIRED_FRAMES = 2;
        // 原 Combat.ChargeStun.DurationScale —— 时长缩放幅度（越大减速越狠晕越久）
        private const float DURATION_SCALE = 0.8f;

        // ============ 反射缓存 ============
        private static FieldInfo _f_atk_onDashing;
        private static FieldInfo _f_ec_onTackle;
        private static FieldInfo _f_ec_onSlide;
        private static FieldInfo _f_es_onDashing;
        private static FieldInfo _f_fe_onTackle;
        private static MethodInfo _m_inflictStun;
        private static MethodInfo _m_knockDown;
        private static bool _reflectInitialized;

        // ============ 组件缓存 ============
        private Status _stat;
        private AttackTrigger _atk;
        private EnemyCharging _ec;
        private EnemySkill _es;
        private FlyingEnemy _fe;
        private bool _componentsCached;
        private bool _hasAnyChargeComp;

        // ============ 状态 ============
        private bool _prevCharging;
        private Vector3 _lastPos;
        private float _lastTime;

        private Vector3 _chargeStartPos;
        private readonly List<Status> _hitTargetsThisCharge = new List<Status>();

        private float _startupElapsed;
        private float _startupMoved;
        private bool _startupPassed;

        private float _peakSpeed;
        private int _lowSpeedFrames;
        private float _lastTriggerTime = -999f;

        private void Awake()
        {
            _stat = GetComponent<Status>();
            _lastPos = transform.position;
            _lastTime = Time.time;
        }

        private void LateUpdate()
        {
            if (!Plugin.ChargeStunEnabled.Value) { ResetState(); return; }
            if (_stat == null || _stat.dead) { ResetState(); return; }

            EnsureComponentsCached();
            if (!_hasAnyChargeComp) { enabled = false; return; }

            bool charging = IsChargingNow();

            // ===== 非冲锋 =====
            if (!charging)
            {
                _lastPos = transform.position;
                _lastTime = Time.time;
                _prevCharging = false;
                ResetChargeState();
                return;
            }

            // ===== 进入冲锋 =====
            if (!_prevCharging)
            {
                _prevCharging = true;
                _lastPos = transform.position;
                _lastTime = Time.time;
                _chargeStartPos = transform.position;
                _hitTargetsThisCharge.Clear();
                ResetChargeState();
                Dbg(gameObject.name + " ENTER pos=" + transform.position);
                return;
            }

            // ===== 采样 =====
            float now = Time.time;
            float dt = now - _lastTime;
            if (dt < 0.005f) dt = 0.005f;
            if (dt > 0.5f) dt = 0.5f;

            Vector3 curPos = transform.position;
            Vector3 diff = curPos - _lastPos;
            diff.y = 0f;
            float currentSpeed = diff.magnitude / dt;

            _lastPos = curPos;
            _lastTime = now;

            // ===== 起步窗口 =====
            if (!_startupPassed)
            {
                _startupElapsed += dt;
                _startupMoved += diff.magnitude;
                if (currentSpeed > _peakSpeed) _peakSpeed = currentSpeed;

                if (_startupElapsed < STARTUP_WINDOW_SEC)
                    return;

                _startupPassed = true;
                Dbg(gameObject.name + " STARTUP done moved=" + _startupMoved.ToString("F2")
                    + " peak=" + _peakSpeed.ToString("F1"));

                if (_startupMoved < STARTUP_MIN_DISTANCE)
                {
                    Dbg(gameObject.name + " STARTUP-BLOCKED trigger");
                    TriggerStun(0f);
                    return;
                }
            }

            // ===== Running =====
            if (currentSpeed > _peakSpeed) _peakSpeed = currentSpeed;

            float minSpeed = MIN_CHARGE_SPEED;
            if (_peakSpeed < minSpeed) return;

            float ratio = currentSpeed / _peakSpeed;
            if (ratio < SPEED_DROP_RATIO)
            {
                _lowSpeedFrames++;
                Dbg(gameObject.name + " LOW ratio=" + ratio.ToString("F2")
                    + " cur=" + currentSpeed.ToString("F1")
                    + " peak=" + _peakSpeed.ToString("F1")
                    + " frames=" + _lowSpeedFrames);
                if (_lowSpeedFrames >= REQUIRED_FRAMES)
                {
                    TriggerStun(ratio);
                    _lowSpeedFrames = 0;
                }
            }
            else
            {
                _lowSpeedFrames = 0;
            }
        }

        private void TriggerStun(float ratio)
        {
            Dbg(gameObject.name + " TRIGGER ratio=" + ratio.ToString("F2")
                + " invincible=" + InvincibleData.IsInvincible(_stat)
                + " recentlyHurt=" + IsRecentlyHurt(_stat)
                + " onStun=" + _stat.onStun.ToString("F2")
                + " freezeStack=" + _stat.freezeStack
                + " stunResist=" + _stat.totalStatus.stun
                + " tag=" + gameObject.tag
                + " hits=" + _hitTargetsThisCharge.Count);

            if (!IsAffectedByConfig()) { Dbg("  skip: config"); return; }
            if (Time.time - _lastTriggerTime < Plugin.ChargeStunCooldownSec.Value)
            { Dbg("  skip: cooldown"); return; }

            if (EXEMPT_DURING_HURT_INVINCIBLE
                && IsRecentlyHurt(_stat))
            { Dbg("  skip: recentlyHurt"); return; }

            if (ratio < 0f) ratio = 0f;
            if (ratio > 1f) ratio = 1f;
            float severity = 1f - ratio;

            float baseSec = Plugin.ChargeStunDurationBase.Value;
            float scale = DURATION_SCALE;
            float maxSec = Plugin.ChargeStunDurationMax.Value;

            float dur = baseSec + severity * scale;
            if (dur > maxSec) dur = maxSec;
            if (dur <= 0f) { Dbg("  skip: dur<=0"); return; }

            _lastTriggerTime = Time.time;

            // ===== 1. 玩家自晕 =====
            EnsureEffectMethods();
            if (_m_inflictStun != null)
                _m_inflictStun.Invoke(_stat, new object[] { dur });
            else
                _stat.ApplyAbnormalStat(1, dur);

            Dbg("  after player: dur=" + dur.ToString("F2")
                + " onStun=" + _stat.onStun.ToString("F2")
                + " freezeStack=" + _stat.freezeStack);

            // ===== 2A：本次冲锋命中过的敌人都被随机施加效果 =====
            if (Plugin.ChargeEnemyRandomEffectEnabled.Value)
            {
                ApplyEffectsToHitTargets(dur);
            }

            ResetChargeState();
        }

        private void ApplyEffectsToHitTargets(float dur)
        {
            if (_hitTargetsThisCharge.Count == 0) return;

            for (int i = 0; i < _hitTargetsThisCharge.Count; i++)
            {
                var enemy = _hitTargetsThisCharge[i];
                if (enemy == null || enemy.dead) continue;
                ApplyRandomEffect(enemy, dur);
            }
        }

        /// <summary>等概率随机首选（眩晕/击倒），失败走 fallback 试另一个。</summary>
        private void ApplyRandomEffect(Status enemy, float dur)
        {
            if (enemy == null || enemy.dead) return;
            EnsureEffectMethods();

            // 0=眩晕 1=击倒
            int pick = Random.Range(0, 2);

            if (TryApplyEffect(enemy, pick, dur)) return;
            if (TryApplyEffect(enemy, 1 - pick, dur)) return;

            Dbg("[2A] " + enemy.gameObject.name + " all effects failed");
        }

        private bool TryApplyEffect(Status enemy, int type, float dur)
        {
            switch (type)
            {
                case 0: // 眩晕
                    if (_m_inflictStun == null) return false;
                    try { _m_inflictStun.Invoke(enemy, new object[] { dur }); }
                    catch { return false; }
                    if (enemy.onStun > 0f)
                    {
                        Dbg("[2A] " + enemy.gameObject.name + " → Stun " + dur.ToString("F2"));
                        return true;
                    }
                    return false;

                case 1: // 击倒
                    if (_m_knockDown == null) return false;
                    try { _m_knockDown.Invoke(enemy, new object[] { dur }); }
                    catch { return false; }
                    if (enemy.down)
                    {
                        Dbg("[2A] " + enemy.gameObject.name + " → KnockDown " + dur.ToString("F2"));
                        return true;
                    }
                    return false;
            }
            return false;
        }

        private static void EnsureEffectMethods()
        {
            if (_m_inflictStun == null)
                _m_inflictStun = AccessTools.Method(typeof(Status), "InflictStun", new[] { typeof(float) });
            if (_m_knockDown == null)
                _m_knockDown = AccessTools.Method(typeof(Status), "KnockDown", new[] { typeof(float) });
        }

        // ============ 供 Patch 调用 ============

        public bool IsChargingNow()
        {
            if (!_reflectInitialized)
            {
                _reflectInitialized = true;
                _f_atk_onDashing = AccessTools.Field(typeof(AttackTrigger), "onDashing");
                _f_ec_onTackle = AccessTools.Field(typeof(EnemyCharging), "onTackle");
                _f_ec_onSlide = AccessTools.Field(typeof(EnemyCharging), "onSlide");
                _f_es_onDashing = AccessTools.Field(typeof(EnemySkill), "onDashing");
                _f_fe_onTackle = AccessTools.Field(typeof(FlyingEnemy), "onTackle");
            }

            if (_atk != null && _f_atk_onDashing != null
                && (bool)_f_atk_onDashing.GetValue(_atk))
                return true;

            if (_ec != null)
            {
                if (_f_ec_onTackle != null && (bool)_f_ec_onTackle.GetValue(_ec))
                    return true;
                if (INCLUDE_SLIDE_PHASE
                    && _f_ec_onSlide != null && (bool)_f_ec_onSlide.GetValue(_ec))
                    return true;
            }

            if (_es != null && _f_es_onDashing != null
                && (bool)_f_es_onDashing.GetValue(_es))
                return true;

            if (_fe != null && _f_fe_onTackle != null
                && (bool)_f_fe_onTackle.GetValue(_fe))
                return true;

            return false;
        }

        /// <summary>命中敌人时调用（由 Patch_BulletStatus_ChargeEffects 调用）。</summary>
        public void RegisterHit(Status target)
        {
            if (target == null) return;
            if (_hitTargetsThisCharge.Contains(target)) return;
            _hitTargetsThisCharge.Add(target);
        }

        /// <summary>按"冲锋起点 → 命中点"的距离映射伤害倍率。</summary>
        public float GetChargeMultiplier(Vector3 hitPos)
        {
            if (!Plugin.ChargeDamageBoostEnabled.Value) return 1f;

            Vector3 a = _chargeStartPos; a.y = 0f;
            Vector3 b = hitPos; b.y = 0f;
            float dist = Vector3.Distance(a, b);

            float minD = Plugin.ChargeDamageBoostMinDistance.Value;
            float maxD = Plugin.ChargeDamageBoostMaxDistance.Value;
            float maxMul = Plugin.ChargeDamageBoostMaxMul.Value;

            if (maxD <= minD) return maxMul;
            if (dist <= minD) return 1f;
            if (dist >= maxD) return maxMul;

            float t = (dist - minD) / (maxD - minD);
            return 1f + t * (maxMul - 1f);
        }

        // ============ 内部辅助 ============

        private void ResetChargeState()
        {
            _startupElapsed = 0f;
            _startupMoved = 0f;
            _startupPassed = false;
            _peakSpeed = 0f;
            _lowSpeedFrames = 0;
        }

        private void ResetState()
        {
            if (_prevCharging) Dbg(gameObject.name + " EXIT");
            _prevCharging = false;
            ResetChargeState();
        }

        private void EnsureComponentsCached()
        {
            if (_componentsCached) return;
            _componentsCached = true;

            _atk = GetComponent<AttackTrigger>();
            _ec = GetComponent<EnemyCharging>();
            _es = GetComponent<EnemySkill>();
            _fe = GetComponent<FlyingEnemy>();

            _hasAnyChargeComp = _atk != null || _ec != null || _es != null || _fe != null;
        }

        private static bool IsRecentlyHurt(Status stat)
        {
            float t;
            if (!InvincibleData.lastHurtTime.TryGetValue(stat, out t)) return false;
            float window = Plugin.HurtInvincibleMs.Value / 1000f + 0.1f;
            return Time.time - t < window;
        }

        private bool IsAffectedByConfig()
        {
            string tag = gameObject.tag;
            if (tag == "Player") return Plugin.ChargeStunAffectPlayer.Value;
            if (tag == "Ally") return Plugin.ChargeStunAffectAlly.Value;
            return Plugin.ChargeStunAffectEnemy.Value;
        }

        private static void Dbg(string msg)
        {
            if (Plugin.ChargeStunDebugLog != null && Plugin.ChargeStunDebugLog.Value)
                Plugin.DebugLog("[ChargeStun] " + msg);
        }
    }
}