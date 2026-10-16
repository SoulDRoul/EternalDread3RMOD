using UnityEngine;

namespace EternalDread3RMOD
{
    public class StatusCleanupTracker : MonoBehaviour
    {
        private Status _stat;


        private void Awake()
        {
            _stat = GetComponent<Status>();

            // 不判断 tag：Awake 阶段 tag 未必已被设置（敌人 tag 在 EnemyBasic.Start 里赋值）。
            // 任何 Status 都需要抢先初始化 defaultLevel，否则会被 Status.Start 里
            // 的 "if (defaultLevel == 0) defaultLevel = level" 污染。

            // 去掉 tag 判断：Awake 阶段 tag 未必已设为 "Enemy"
            if (_stat != null
                && _stat.defaultLevel == 0
                && _stat.level > 0)
            {
                _stat.defaultLevel = _stat.level;
            }
        }

        private void OnDestroy()
        {
            // 用 ReferenceEquals 绕过 Unity 的 == null 重载，
            // 保证 Status 已被 Destroy 时仍能拿到 C# 引用做字典 Remove
            if (ReferenceEquals(_stat, null)) return;

            ComboState.playerDamagedEnemies.Remove(_stat);
            InvincibleData.endTime.Remove(_stat);
            InvincibleData.lastHurtTime.Remove(_stat);
            InvincibleData.prevDodge.Remove(_stat);
            InvincibleVisual.blinking.Remove(_stat);
            Patch_Status_Update.CleanInstance(_stat);
            Patch_Status_BurnSpread.CleanInstance(_stat);
            Patch_Status_DoTKill.CleanInstance(_stat);
            Patch_Status_DodgeRollBurn.CleanInstance(_stat);

            Plugin.DebugLog("[Cleanup] Status 已清理: " + _stat.gameObject.name);
        }
    }
}