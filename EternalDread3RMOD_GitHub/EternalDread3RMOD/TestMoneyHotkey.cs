using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 测试用：按热键给玩家加钱。
    ///
    /// 键名用 string 配置，避免 BepInEx 在 KeyCode 类型配置里列出所有枚举值。
    /// 解析失败时该帧静默跳过（不刷屏）。
    /// </summary>
    public class TestMoneyHotkey : MonoBehaviour
    {
        private void Update()
        {
            if (Plugin.TestMoneyEnabled == null || !Plugin.TestMoneyEnabled.Value) return;
            if (GlobalVar.mainPlayer == null) return;
            if (Plugin.TestMoneyKey == null) return;

            KeyCode key;
            try
            {
                key = (KeyCode)System.Enum.Parse(
                    typeof(KeyCode), Plugin.TestMoneyKey.Value, true);
            }
            catch
            {
                return;   // 配置无效，静默跳过
            }

            if (!Input.GetKeyDown(key)) return;

            var inv = GlobalVar.mainPlayer.GetComponent<Inventory>();
            if (inv == null) return;

            try
            {
                long newCash = (long)inv.cash + Plugin.TestMoneyAmount.Value;
                if (newCash > int.MaxValue / 2)
                    newCash = int.MaxValue / 2;
                inv.cash = (int)newCash;

                Plugin.Log.LogInfo("[ED3R] Test money: +"
                    + Plugin.TestMoneyAmount.Value
                    + " → cash=" + inv.cash);
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("[ED3R] TestMoneyHotkey error: " + e);
            }
        }
    }
}