using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 挂在带 AiStatus 的单位上。
    /// 检测 HP 首次跨过阈值，延迟后触发噪音：
    ///   - 播放随机音效（attackVoice / hurtVoice）
    ///   - 头顶显示 !!!
    ///   - 创建噪音标记
    ///   - 吸引范围内非战斗状态的 AI 单位（followTarget == null && !angry）
    /// </summary>
    public class NoiseTracker : MonoBehaviour
    {
        // ============ 硬编码常量：AI.Noise 段（音效/标记） ============
        // 原 AI.Noise.MarkerLifetimeSec —— 噪音位置存在时长（秒），超时后 AI 自动回待机
        private const float NOISE_MARKER_LIFETIME_SEC = 9f;
        // 原 AI.Noise.AudioVolume —— 噪音音效的音量倍率（相对于游戏效果音量）
        private const float NOISE_AUDIO_VOLUME = 2f;

        private Status _stat;
        private AiStatus _ai;

        private bool _wasBelow;
        private float _pendingTriggerTime = -1f;

        // ============ Update 节流 ============
        // HP 变化没有事件通知，只能轮询，但没必要每帧查。
        // 0.2 秒一次：延迟感知 < 阈值延迟（默认 1.5 秒），玩家无感。
        private const float HP_CHECK_INTERVAL_SEC = 0.2f;
        private float _nextHpCheckTime;

        private void Awake()
        {
            _stat = GetComponent<Status>();
            _ai = GetComponent<AiStatus>();
        }

        private void OnDestroy()
        {
            NoiseRegistry.Unregister(_ai);
        }

        private void Update()
        {
            if (!Plugin.NoiseEnabled.Value) return;

            // 0.2 秒节流：即使 AI 很多，每帧成本也恒定
            if (Time.time < _nextHpCheckTime) return;
            _nextHpCheckTime = Time.time + HP_CHECK_INTERVAL_SEC;

            if (_stat == null || _ai == null) return;
            if (_stat.dead) return;

            float maxHp = _stat.totalStatus.maxHealth;
            if (maxHp <= 0) return;

            float threshold = maxHp * Plugin.NoiseHpThresholdPercent.Value / 100f;
            bool below = _stat.health <= threshold;

            // HP 回到阈值以上 → 完全重置
            if (!below)
            {
                _wasBelow = false;
                _pendingTriggerTime = -1f;
                return;
            }

            // HP 首次降到阈值以下 → 启动延迟
            if (!_wasBelow)
            {
                _wasBelow = true;
                _pendingTriggerTime = Time.time + Plugin.NoiseTriggerDelaySec.Value;
                return;
            }

            // HP 持续低于阈值 → 按间隔反复尝试触发
            // TryTrigger 内部的 SharedCooldownSec 会做实际节流
            if (_pendingTriggerTime > 0f && Time.time >= _pendingTriggerTime)
            {
                TryTrigger();
                _pendingTriggerTime = Time.time + Plugin.NoiseTriggerDelaySec.Value;
            }
        }

        private void TryTrigger()
        {
            if (_stat == null || _stat.dead) return;

            // 被控制状态 → 不触发
            if (_stat.onStun > 0f || _stat.onFrost > 0f
                || _stat.onWebbedUp > 0 || _stat.onStone > 0
                || _stat.down || _stat.launched
                || _stat.freezeStack > 0)
                return;

            Vector3 pos = transform.position;
            float radius = GetCallRadius();

            // 共享冷却
            if (NoiseRegistry.IsInSharedCooldown(pos,
                Plugin.NoiseSharedCooldownSec.Value, radius))
                return;

            // 周围 AI 单位数量
            int count = CountNearbyAI(pos, radius);
            if (count > Plugin.NoiseMaxNearbyUnits.Value) return;

            NoiseRegistry.MarkNoise(pos);
            TriggerNoise(pos, radius);
        }

        private int CountNearbyAI(Vector3 center, float radius)
        {
            int count = 1;  // 包括自己
            float sqrRadius = radius * radius;
            var list = NoiseRegistry.ActiveAIs;

            for (int i = 0; i < list.Count; i++)
            {
                var ai = list[i];
                if (ai == null) continue;
                if (ai.gameObject == gameObject) continue;

                Vector3 d = ai.transform.position - center;
                d.y = 0f;
                if (d.sqrMagnitude <= sqrRadius) count++;
            }
            return count;
        }

        private void TriggerNoise(Vector3 pos, float radius)
        {
            // 音效
            PlayNoiseSound();

            // 视觉提示
            if (Plugin.NoiseShowVisualIndicator.Value)
                AlertUI.SpawnNoise(transform);

            // 噪音标记
            var markerGO = new GameObject("NoiseMarker");
            markerGO.transform.position = pos;
            var marker = markerGO.AddComponent<NoiseMarker>();
            marker.lifetime = NOISE_MARKER_LIFETIME_SEC;

            // 吸引周围 AI
            AttractNearbyAI(pos, radius, markerGO.transform);
        }

        private void PlayNoiseSound()
        {
            AudioClip clip = PickClip();
            if (clip == null) return;

            var audio = GetComponent<AudioSource>();
            if (audio == null) return;

            audio.volume = OptionData.effectVolume * NOISE_AUDIO_VOLUME;
            audio.PlayOneShot(clip);
        }

        private AudioClip PickClip()
        {
            var candidates = new List<AudioClip>();

            if (_ai != null && _ai.attackVoice != null)
            {
                for (int i = 0; i < _ai.attackVoice.Length; i++)
                    if (_ai.attackVoice[i] != null) candidates.Add(_ai.attackVoice[i]);
            }
            if (_stat != null && _stat.hurtVoice != null)
            {
                for (int i = 0; i < _stat.hurtVoice.Length; i++)
                    if (_stat.hurtVoice[i] != null) candidates.Add(_stat.hurtVoice[i]);
            }

            if (candidates.Count == 0) return null;
            return candidates[Random.Range(0, candidates.Count)];
        }

        private void AttractNearbyAI(Vector3 center, float radius, Transform marker)
        {
            float sqrRadius = radius * radius;
            var list = NoiseRegistry.ActiveAIs;

            for (int i = 0; i < list.Count; i++)
            {
                var ai = list[i];
                if (ai == null) continue;
                if (ai.gameObject == gameObject) continue;

                // 只吸引非战斗状态的 AI
                if (ai.followTarget != null) continue;
                if (ai.angry) continue;

                var s = ai.GetComponent<Status>();
                if (s == null || s.dead) continue;

                Vector3 d = ai.transform.position - center;
                d.y = 0f;
                if (d.sqrMagnitude > sqrRadius) continue;

                ai.followTarget = marker;
                ai.state = AIState.Moving;
            }
        }

        private float GetCallRadius()
        {
            float scale;
            switch (GlobalVar.difficultLv)
            {
                case 0: scale = Plugin.NoiseCallRadiusScaleS0.Value; break;
                case 1: scale = Plugin.NoiseCallRadiusScaleS1.Value; break;
                case 2: scale = Plugin.NoiseCallRadiusScaleS2.Value; break;
                case 3: scale = Plugin.NoiseCallRadiusScaleS3.Value; break;
                default: scale = 1f; break;
            }
            return Plugin.NoiseCallRadius.Value * scale;
        }
    }
}