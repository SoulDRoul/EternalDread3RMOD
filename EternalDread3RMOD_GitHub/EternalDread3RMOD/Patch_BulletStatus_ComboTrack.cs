using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(BulletStatus), "DealingDamage")]
    public static class Patch_BulletStatus_ComboTrack
    {
        private static System.Reflection.FieldInfo _popDamageField = null;

        static void Prefix(BulletStatus __instance, GameObject other)
        {
            // 记录"玩家伤害过的敌人"，用于后续击杀判定
            if (!Plugin.ComboEnabled.Value) return;
            if (__instance.source == null || other == null) return;
            if (!IsPlayerSide(__instance.source)) return;
            if (other.tag != "Enemy" && other.tag != "BreakableEn") return;

            var enemyStat = other.GetComponent<Status>();
            if (enemyStat != null)
            {
                ComboState.RecordPlayerDamage(enemyStat);
            }
        }

        static void Postfix(BulletStatus __instance, GameObject other)
        {
            // HIT 连击计数
            if (!Plugin.ComboEnabled.Value) return;
            if (!Plugin.HitComboEnabled.Value) return;
            if (__instance.source == null || other == null) return;
            if (!IsPlayerSide(__instance.source)) return;
            if (other.tag != "Enemy") return;

            // 读 popDamage
            if (_popDamageField == null)
            {
                _popDamageField = AccessTools.Field(typeof(BulletStatus), "popDamage");
            }
            if (_popDamageField == null) return;

            string popDamage = _popDamageField.GetValue(__instance) as string;
            if (string.IsNullOrEmpty(popDamage)) return;

            // 只有数字才算命中（排除 Blocked / Evaded / Miss / Invulnerable / Guard / 空）
            int dmg;
            if (!int.TryParse(popDamage, out dmg)) return;
            if (dmg <= 0) return;

            // 尸体不计数
            var enemyStat = other.GetComponent<Status>();
            if (enemyStat == null) return;
            if (enemyStat.dead || enemyStat.health <= 0) return;

            bool reward = ComboState.OnHit();
            if (reward)
            {
                GiveStaminaReward();
            }
        }

        private static void GiveStaminaReward()
        {
            var player = GlobalVar.mainPlayer;
            if (player == null) return;

            var input = player.GetComponent<PlayerInputController>();
            if (input == null) return;

            float max = input.maxStamina;
            float amount = max * Plugin.StaminaRewardPercent.Value / 100f;
            input.stamina = Mathf.Min(input.stamina + amount, max);

            ComboUI.ShowRewardText((int)amount);

            Plugin.DebugLog("[HitCombo] 触发体力奖励 +" + amount);
        }

        private static bool IsPlayerSide(GameObject attacker)
        {
            if (attacker == null) return false;
            if (attacker == GlobalVar.mainPlayer) return true;

            var ai = attacker.GetComponent<AiStatus>();
            if (ai != null && ai.master != null
                && ai.master.gameObject == GlobalVar.mainPlayer)
            {
                return true;
            }
            return false;
        }
    }
}