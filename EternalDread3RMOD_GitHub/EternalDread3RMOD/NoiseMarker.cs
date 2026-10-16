using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 噪音位置标记。
    /// 存活 N 秒后自毁，被吸引的 AI 的 followTarget 随之变 null，自动回到待机。
    /// </summary>
    public class NoiseMarker : MonoBehaviour
    {
        public float lifetime = 6f;
        private float _elapsed;

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= lifetime)
                Destroy(gameObject);
        }
    }
}