using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 挂在 RespawnMonster 上，延迟到原版生成前判定生态过滤。
    /// 任务场景豁免：刷怪点在 MinimapCamera.objective 水平半径内 → 放行。
    ///
    /// 与 SetTag 路径共用 EcologyQuestBypassHelper 判据。
    /// </summary>
    public class RespawnMonsterEcologyDelay : MonoBehaviour
    {
        private RespawnMonster _monster;
        private float _triggerTime;
        private bool _done = false;

        public void Init(RespawnMonster m)
        {
            _monster = m;
            float delay = (m != null) ? m.delay : 3f;
            float offset = Mathf.Max(1.0f, delay * 0.5f);
            _triggerTime = Time.time + offset;
        }

        private void Update()
        {
            if (_done) return;
            if (Time.time < _triggerTime) return;
            _done = true;

            if (_monster == null) { Destroy(this); return; }

            try
            {
                // ★ 二次校验：延迟期间场景可能变化（虽然 patch 时已判过）
                if (!EcologySceneState.IsEcologyEnabledForCurrentScene())
                {
                    Destroy(this);
                    return;
                }

                bool? taskCtx = EcologyQuestBypassHelper.TryIsTaskContext(_monster.transform.position);

                if (taskCtx == true)
                {
                    Destroy(this);
                    return;
                }

                string region = SceneManager.GetActiveScene().name;
                if (!string.IsNullOrEmpty(region))
                {
                    var r = EcologyRegistry.GetOrCreate(region);
                    if (r != null)
                    {
                        float roll = Random.Range(0f, 100f);
                        if (roll >= r.PopulationPercent)
                        {
                            Object.Destroy(_monster.gameObject);
                            return;
                        }
                    }
                }

                Destroy(this);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] RespawnMonsterEcologyDelay error: " + e);
                Destroy(this);
            }
        }
    }
}