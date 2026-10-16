using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 任务上下文判据（公共）。
    ///
    /// 命中条件：
    ///   1. 能拿到 MinimapCamera
    ///   2. MinimapCamera.objective 有效
    ///   3. 给定位置与 objective 的【XZ 水平距离】≤ 配置半径
    ///
    /// 说明：
    ///   - objective 常挂在高空（y 可达 60+ 米），必须用 XZ 水平距离
    ///   - MinimapCamera 不在 minimapCam 上，是其子对象
    /// </summary>
    public static class EcologyQuestBypassHelper
    {
        // ============ 硬编码常量：Ecology.QuestBypassRadius ============
        // 原 Ecology.QuestBypassRadius —— 任务豁免的检测半径（米，XZ 水平距离）
        private const int QUEST_BYPASS_RADIUS = 60;

        public static MinimapCamera GetMinimapCamera()
        {
            // 方式 1：通过 MinimapOnOff.minimapCam 的子孙组件拿（推荐）
            var onOff = Object.FindObjectOfType<MinimapOnOff>();
            if (onOff != null && onOff.minimapCam != null)
            {
                var mm = onOff.minimapCam.GetComponentInChildren<MinimapCamera>(true);
                if (mm != null) return mm;
            }

            // 方式 2：全局查找（含 inactive），覆盖 MinimapOnOff 不存在
            //         但 MinimapCamera 存在的情况（比如未来 mod 注入）
            var all = Resources.FindObjectsOfTypeAll<MinimapCamera>();
            if (all != null)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    var m = all[i];
                    if (m == null) continue;
                    // 过滤 prefab 资源（无 scene 名）
                    if (string.IsNullOrEmpty(m.gameObject.scene.name)) continue;
                    return m;
                }
            }

            return null;
        }

        /// <summary>
        /// 判定给定位置是否在任务目标半径内。
        /// 返回：
        ///   true  = 明确在任务半径内 → 应放行
        ///   false = 明确不在任务半径内 → 应过滤
        ///   null  = MinimapCamera 未就绪，无法判定
        /// </summary>
        public static bool? TryIsTaskContext(Vector3 worldPos)
        {
            var mm = GetMinimapCamera();
            if (mm == null) return null;
            if (mm.objective == null) return null;
            if (!mm.objective.gameObject.activeInHierarchy) return null;

            int radius = QUEST_BYPASS_RADIUS;
            if (radius < 1) radius = 1;

            Vector3 objPos = mm.objective.position;
            float dx = objPos.x - worldPos.x;
            float dz = objPos.z - worldPos.z;
            float distXZ = Mathf.Sqrt(dx * dx + dz * dz);

            return distXZ <= radius;
        }
    }
}