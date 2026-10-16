using System.Collections.Generic;

namespace EternalDread3RMOD
{
    public static class DodgeTapState
    {
        // 是否处于"双击触发的锁定冲刺"状态
        public static bool lockedSprint = false;

        // 每个方向独立记录上一次单击的时间
        public static Dictionary<string, float> lastTapTime
            = new Dictionary<string, float>();

        // 双击窗口（秒）
        public const float TapWindow = 0.2f;

        // 方向键 id
        public const string DirUp = "up";
        public const string DirDown = "down";
        public const string DirLeft = "left";
        public const string DirRight = "right";
    }
}