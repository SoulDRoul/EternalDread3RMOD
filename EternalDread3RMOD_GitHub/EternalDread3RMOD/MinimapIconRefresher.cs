using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 定时刷新所有敌人图标的等级色。
    /// 玩家升级时会立即触发一次（见 Patch_Status_LevelUp_Minimap）。
    /// </summary>
    public class MinimapIconRefresher : MonoBehaviour
    {
        private float _nextRefresh;

        // ============ 硬编码常量：Minimap.EnemyLevelRefreshSec ============
        // 原 Minimap.EnemyLevelRefreshSec —— 敌人等级色定时刷新间隔（秒）
        private const float ENEMY_LEVEL_REFRESH_SEC = 3.0f;

        private void Update()
        {
            if (UiSuppress.All) return;
            if (Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + ENEMY_LEVEL_REFRESH_SEC;
            MinimapIconRegistry.RefreshAllEnemies();
        }
    }
}