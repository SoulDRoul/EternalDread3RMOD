namespace EternalDread3RMOD
{
    /// <summary>永久死亡模式的内存状态。</summary>
    public static class PermadeathState
    {
        /// <summary>创角界面勾选的状态，等待第一次存档写入。</summary>
        public static bool PendingPermadeath = false;

        /// <summary>当前存档是否 permadeath。LoadGame 或 StartGame 时更新。</summary>
        public static bool CurrentSavePermadeath = false;

        public static void Reset()
        {
            PendingPermadeath = false;
            CurrentSavePermadeath = false;
        }
    }
}