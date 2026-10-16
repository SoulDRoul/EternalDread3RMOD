using UnityEngine;

namespace EternalDread3RMOD
{
    public class EnemyAlertTracker : MonoBehaviour
    {
        private AiStatus _ai;
        private Transform _lastTarget;
        private float _lastTriggerTime = -999f;
        private float _lastSoundTime = -999f;

        private const float SoundCooldownSec = 1.0f;

        private void Awake()
        {
            _ai = GetComponent<AiStatus>();
            if (_ai != null) _lastTarget = _ai.followTarget;
        }

        private void Update()
        {
            if (_ai == null) return;

            string tag = gameObject.tag;
            bool isEnemy = (tag == "Enemy");
            bool isAlly = (tag == "Ally");

            bool shouldShowAlert = Plugin.AlertEnabled.Value
                && ((isEnemy && Plugin.AlertShowEnemy.Value)
                    || (isAlly && Plugin.AlertShowAlly.Value));
            bool shouldPlaySound = Plugin.AlertSoundEnabled.Value;

            if (!shouldShowAlert && !shouldPlaySound) return;

            Transform current = _ai.followTarget;
            bool justFound = (current != null && _lastTarget == null);

            if (justFound)
            {
                // 感叹号（走原有冷却）
                if (shouldShowAlert)
                {
                    float cooldown = Plugin.AlertPerEnemyCooldownSec.Value;
                    if (Time.time - _lastTriggerTime >= cooldown)
                    {
                        _lastTriggerTime = Time.time;
                        AlertUI.Spawn(transform);
                    }
                }

                // 音效（独立 1 秒冷却）
                if (shouldPlaySound && Time.time - _lastSoundTime >= SoundCooldownSec)
                {
                    _lastSoundTime = Time.time;
                    PlayAlertSound();
                }
            }

            _lastTarget = current;
        }

        private void PlayAlertSound()
        {
            // 合并 attackVoice + hurtVoice，随机选
            var clips = new System.Collections.Generic.List<AudioClip>();

            var ai = GetComponent<AiStatus>();
            if (ai != null && ai.attackVoice != null)
            {
                for (int i = 0; i < ai.attackVoice.Length; i++)
                    if (ai.attackVoice[i] != null) clips.Add(ai.attackVoice[i]);
            }

            var stat = GetComponent<Status>();
            if (stat != null && stat.hurtVoice != null)
            {
                for (int i = 0; i < stat.hurtVoice.Length; i++)
                    if (stat.hurtVoice[i] != null) clips.Add(stat.hurtVoice[i]);
            }

            if (clips.Count == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Count)];

            // 手动创建 AudioSource，调整 3D 衰减参数
            var go = new GameObject("AlertSound");
            go.transform.position = transform.position + Vector3.up * 1.5f;
            var audio = go.AddComponent<AudioSource>();
            audio.clip = clip;
            audio.volume = OptionData.effectVolume;
            audio.spatialBlend = 1f;                    // 3D
            audio.minDistance = 3f;
            audio.maxDistance = 100f;
            audio.rolloffMode = AudioRolloffMode.Linear;
            audio.Play();

            Destroy(go, clip.length + 0.5f);
        }
    }
}