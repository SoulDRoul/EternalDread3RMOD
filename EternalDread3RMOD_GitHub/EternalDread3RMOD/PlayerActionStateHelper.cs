using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 判断玩家当前是否"可行动"。
    /// 不可行动的典型场景：
    ///   - 与商店 / 储物箱交互中
    ///   - 打开菜单
    ///   - 对话中
    ///   - 过场动画
    ///   - 其他全局冻结
    /// 用于双击冲刺、冲刺加速等输入相关功能的门禁判断。
    /// </summary>
    public static class PlayerActionStateHelper
    {
        public static bool IsPlayerActionable(PlayerInputController pic)
        {
            if (GlobalVar.allFreeze) return false;
            if (GlobalVar.interact) return false;
            if (GlobalVar.freezeCam) return false;

            if (pic != null)
            {
                var status = pic.GetComponent<Status>();
                if (status != null)
                {
                    if (status.interacting) return false;
                    if (status.menuOn) return false;
                }
            }
            return true;
        }
    }
}