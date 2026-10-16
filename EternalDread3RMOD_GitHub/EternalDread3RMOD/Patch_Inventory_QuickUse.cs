using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EternalDread3RMOD
{
    [HarmonyPatch(typeof(Inventory), "UseAnimation")]
    public static class Patch_Inventory_QuickUse
    {
        // ============ 硬编码常量：QuickUseItem.MaxWaitSec ============
        // 原 QuickUseItem.MaxWaitSec —— 动画最多等待时长（秒），限制异常长的 clip
        private const float MAX_WAIT_SEC = 1.0f;

        private static float _lastUseTime = -999f;
        private static FieldInfo _dbField;

        static bool Prefix(Inventory __instance, int slot, ref IEnumerator __result)
        {
            if (!Plugin.QuickUseItemEnabled.Value) return true;

            var stat = __instance.GetComponent<Status>();
            var atk = __instance.GetComponent<AttackTrigger>();
            if (stat == null || atk == null) return true;
            if (atk.onAttacking || stat.freezeStack > 0) return true;

            // 冷却：防止连点刷药
            if (Time.time - _lastUseTime < Plugin.QuickUseItemMinIntervalSec.Value)
                return false;
            _lastUseTime = Time.time;

            __result = CustomUse(__instance, slot, stat);
            return false;
        }

        static IEnumerator CustomUse(Inventory inv, int slot, Status stat)
        {
            stat.freezeStack++;

            // ===== 播放动画 =====
            Animation legacy = stat.mainModel.GetComponent<Animation>();
            Animator animator = stat.mainModel.GetComponent<Animator>();
            float clipLen = 0.45f;

            if (legacy != null && inv.useItemAnimation != null)
            {
                legacy[inv.useItemAnimation.name].layer = 30;
                legacy.Play(inv.useItemAnimation.name);
                clipLen = legacy[inv.useItemAnimation.name].length;
            }
            else if (animator != null && inv.useItemAnimation != null)
            {
                animator.Play(inv.useItemAnimation.name);
            }

            // ===== 立即治疗 + 效果 =====
            ApplyItemEffects(inv, slot, stat);
            ConsumeItem(inv, slot);

            // ===== 可打断的等待 =====
            float elapsed = 0f;
            float waitTime = clipLen - 0.25f;
            if (waitTime < 0.05f) waitTime = 0.05f;
            float maxWait = MAX_WAIT_SEC;
            if (maxWait > 0f && waitTime > maxWait) waitTime = maxWait;

            while (elapsed < waitTime)
            {
                if (ShouldInterrupt()) break;
                // 用 unscaledDeltaTime：暂停（timeScale=0）期间也能推进
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // ===== 停动画 =====
            if (legacy != null && inv.useItemAnimation != null)
                legacy.Stop(inv.useItemAnimation.name);

            // ===== 解冻 =====
            stat.freezeStack--;
            if (stat.freezeStack < 0) stat.freezeStack = 0;
        }

        static void ApplyItemEffects(Inventory inv, int slot, Status stat)
        {
            var db = GetDb(inv);
            if (db == null) return;

            var item = db.usableItem[inv.items[slot].itemId];

            stat.Heal(item.hpRecovery, item.mpRecovery);

            if (item.effectPrefab)
            {
                var fx = Object.Instantiate(item.effectPrefab,
                    inv.transform.position, inv.transform.rotation);
                var sources = fx.GetComponentsInChildren<AudioSource>();
                for (int i = 0; i < sources.Length; i++)
                    sources[i].volume = OptionData.effectVolume;
            }

            if (!string.IsNullOrEmpty(item.sendMessage))
                inv.SendMessage(item.sendMessage);

            if (item.curePoison) stat.CancelPoison();
            if (item.cureStun) stat.CancelStun();
            if (item.cureSilence) stat.CancelSilence();
            if (item.cureFrost) stat.CancelFrost();
            if (item.cureFear) stat.CancelFear();
            if (item.cureConfuse) stat.CancelConfuse();
            if (item.cureElectrocute) stat.CancelElectrocute();

            if (item.buffs != null && item.buffs.Length > 0)
            {
                for (int i = 0; i < item.buffs.Length; i++)
                    stat.ApplyBuff((int)item.buffs[i].buffs,
                        item.buffs[i].statusDuration);
            }
        }

        static void ConsumeItem(Inventory inv, int slot)
        {
            var db = GetDb(inv);
            if (db == null) return;

            var item = db.usableItem[inv.items[slot].itemId];
            if (!item.unlimited) inv.items[slot].quantity--;

            if (inv.items[slot].quantity <= 0)
            {
                inv.items[slot].itemId = 0;
                inv.items[slot].quantity = 0;
                if (inv.items[slot].shortcut > 0)
                {
                    inv.RemoveShortCut(inv.items[slot].shortcut);
                    inv.items[slot].shortcut = 0;
                }
            }
            inv.AutoSortItem();
        }

        static ItemData GetDb(Inventory inv)
        {
            if (_dbField == null)
                _dbField = AccessTools.Field(typeof(Inventory), "db");
            return _dbField?.GetValue(inv) as ItemData;
        }

        static bool ShouldInterrupt()
        {
            if (Plugin.QuickUseItemAnyKey.Value && Input.anyKeyDown) return true;

            if (Mathf.Abs(ButtonInputOption.Horizontal) > 0.1f) return true;
            if (Mathf.Abs(ButtonInputOption.Vertical) > 0.1f) return true;

            if (Input.GetKeyDown(ButtonInputOption.jump)) return true;
            if (Input.GetKeyDown(ButtonInputOption.joyJump)) return true;
            if (Input.GetKeyDown(ButtonInputOption.attack)) return true;
            if (Input.GetKeyDown(ButtonInputOption.guard)) return true;
            if (Input.GetKeyDown(ButtonInputOption.joyGuard)) return true;

            return false;
        }
    }
}