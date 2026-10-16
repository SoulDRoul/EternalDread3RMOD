using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 场景级生态生效判定。
    ///
    /// 核心：本场景是否有 MinimapCamera。
    ///   有 → 生态过滤生效（玩家能看到 UI）
    ///   无 → 生态过滤失效（玩家看不到 UI，不该起作用）
    ///
    /// 用 2 秒定时刷新缓存，覆盖以下动态场景：
    ///   - 切图窗口：MinimapCamera 尚未就绪 → 暂时不过滤
    ///   - mod 动态添加小地图：2 秒后自动生效
    ///   - 游戏更新：同上
    ///   - 玩家手动隐藏小地图：GetComponentInChildren(true) 依然拿到 → 不受影响
    /// </summary>
    public static class EcologySceneState
    {
        private static MinimapCamera _cachedMm = null;
        private static float _lastRefresh = -999f;
        private const float RefreshIntervalSec = 2f;

        public static bool IsEcologyEnabledForCurrentScene()
        {
            if (Time.unscaledTime - _lastRefresh > RefreshIntervalSec)
            {
                _lastRefresh = Time.unscaledTime;
                _cachedMm = EcologyQuestBypassHelper.GetMinimapCamera();
            }
            return _cachedMm != null;
        }

        /// <summary>手动失效缓存（可选）。目前没调用点，保留以备将来使用。</summary>
        public static void Invalidate()
        {
            _lastRefresh = -999f;
            _cachedMm = null;
        }
    }
}