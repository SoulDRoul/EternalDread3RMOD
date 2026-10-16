using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// SetTag 延迟判定组件。
    ///
    /// AiStatus.Start 可能早于 MinimapCamera.Start，
    /// 此时 SetTag 拿不到 objective。挂此组件在后续帧持续尝试判定：
    ///   - MinimapCamera 就绪 → 立即判定
    ///   - 超过 deadline → 视为无任务，走正常过滤
    /// </summary>
    public class EcologySpawnSetTagDelay : MonoBehaviour
    {
        private float _deadline;
        private bool _decided = false;

        public void Init(float waitSec)
        {
            _deadline = Time.time + waitSec;
        }

        private void Update()
        {
            if (_decided) return;

            // 场景中途变化（如本场景被移出生态范围）→ 直接放行
            if (!EcologySceneState.IsEcologyEnabledForCurrentScene())
            {
                _decided = true;
                Destroy(this);
                return;
            }

            bool? taskCtx = EcologyQuestBypassHelper.TryIsTaskContext(transform.position);

            if (taskCtx == null)
            {
                if (Time.time < _deadline) return;   // 继续等
                taskCtx = false;                      // 超时：视为无任务
            }

            _decided = true;

            if (taskCtx.Value)
            {
                Destroy(this);
                return;
            }

            Destroy(gameObject);
        }
    }
}