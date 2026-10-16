using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 用于延迟销毁的场景初始怪过滤。
    /// SetTag() 时对象还在初始化，不能立即 Destroy，
    /// 挂此组件，等 Start 阶段再销毁。
    /// </summary>
    public class EcologySpawnFilter : MonoBehaviour
    {
        public bool ShouldDestroy;

        private void Start()
        {
            if (!ShouldDestroy)
            {
                Destroy(this);
                return;
            }
            Destroy(gameObject);
        }
    }
}