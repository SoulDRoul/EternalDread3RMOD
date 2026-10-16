using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 生态率拦截的通用 bypass 判断。
    /// 任何一条命中就放行（不拦截）。
    /// </summary>
    public static class EcologySpawnGuard
    {
        private const float PlayerDeathGraceSec = 10f;

        public static bool ShouldBypass()
        {
            // 1. 教程隔离
            if (ModSceneHelper.IsTutorial()) return true;

            // 2. 剧情/过场多信号
            if (GlobalVar.freezeCam) return true;
            if (GlobalVar.allFreeze) return true;
            if (GlobalVar.interact) return true;

            // 3. 镜头 target 不是玩家
            if (IsCameraOffPlayer()) return true;

            // 4. 玩家死亡窗口
            float elapsed = Time.realtimeSinceStartup - Patch_Status_Death_Ecology.PlayerDeathTime;
            if (elapsed >= 0f && elapsed < PlayerDeathGraceSec) return true;

            return false;
        }

        private static bool IsCameraOffPlayer()
        {
            var cam = Camera.main;
            if (cam == null) return false;

            var arpg = cam.GetComponent<ARPGcamera>();
            if (arpg == null) return false;
            if (arpg.target == null) return false;
            if (GlobalVar.mainPlayer == null) return false;

            return arpg.target.gameObject != GlobalVar.mainPlayer;
        }

    }
}