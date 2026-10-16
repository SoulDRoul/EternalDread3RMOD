using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 每次打开技能面板时，按玩家等级重建被动槽位。
    /// 底排 8 个原版槽位不动，新槽位往上堆，每行 8 个，左对齐。
    ///
    /// 索引映射：
    ///   UI 槽 0..7   → shortCutSkillID[8..15]（原版被动）
    ///   UI 槽 8..N-1 → shortCutSkillID[20..]（扩展槽，跳过 16..19）
    /// </summary>
    [HarmonyPatch(typeof(SkillTreeCanvas), "Start")]
    public static class Patch_SkillTreeCanvas_Start_ExpandPassiveSlots
    {
        static void Postfix(SkillTreeCanvas __instance)
        {
            if (!Plugin.PassiveSlotExpandEnabled.Value) return;
            if (__instance == null || __instance.allButtons == null) return;
            if (__instance.allButtons.signetBtn == null) return;
            if (__instance.signetIcons == null) return;
            if (GlobalVar.mainPlayer == null) return;

            var status = GlobalVar.mainPlayer.GetComponent<Status>();
            if (status == null) return;

            // 先扩 AttackTrigger 数组（防止界面刷新时越界）
            ExpandAttackTriggerArrays(GlobalVar.mainPlayer);

            int unlocked = PassiveSlotHelper.GetUnlockedSlotCount(status.level);
            int total = Math.Max(PassiveSlotHelper.BaseSlotCount, unlocked);

            try
            {
                ApplySlotCount(__instance, total);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("[ED3R] PassiveSlot SkillTreeCanvas.Start error: " + e);
            }
        }

        private static void ExpandAttackTriggerArrays(GameObject player)
        {
            if (player == null) return;

            try
            {
                int target = PassiveSlotHelper.GetTargetArrayLength();

                var atk = player.GetComponent<AttackTrigger>();
                if (atk == null) return;

                if (atk.skills == null || atk.skills.Length < target)
                {
                    var newSkills = new SkillSetting[target];
                    if (atk.skills != null)
                        Array.Copy(atk.skills, newSkills, atk.skills.Length);
                    atk.skills = newSkills;
                }

                if (atk.skillCoolDown == null || atk.skillCoolDown.Length < target)
                {
                    var newCD = new int[target];
                    if (atk.skillCoolDown != null)
                        Array.Copy(atk.skillCoolDown, newCD, atk.skillCoolDown.Length);
                    atk.skillCoolDown = newCD;
                }

                var waitField = AccessTools.Field(typeof(AttackTrigger), "wait");
                if (waitField != null)
                {
                    var oldWait = (float[])waitField.GetValue(atk);
                    if (oldWait == null || oldWait.Length < target)
                    {
                        var newWait = new float[target];
                        if (oldWait != null)
                            Array.Copy(oldWait, newWait, oldWait.Length);
                        waitField.SetValue(atk, newWait);
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("[ED3R] PassiveSlot expand atk arrays error: " + e);
            }
        }

        private static void ApplySlotCount(SkillTreeCanvas tree, int total)
        {
            var oldIcons = tree.signetIcons;
            var oldBtns = tree.allButtons.signetBtn;

            int baseCount = PassiveSlotHelper.BaseSlotCount;
            if (oldBtns.Length < baseCount) return;

            // 计算底排均匀间距
            Button first = oldBtns[0];
            Button last = oldBtns[baseCount - 1];
            if (first == null || last == null) return;

            RectTransform rt0 = first.GetComponent<RectTransform>();
            RectTransform rtLast = last.GetComponent<RectTransform>();
            if (rt0 == null || rtLast == null) return;

            Vector2 pos0 = rt0.anchoredPosition;
            Vector2 posLast = rtLast.anchoredPosition;

            float leftX = pos0.x;
            float baseY = pos0.y;
            float stepX = (posLast.x - pos0.x) / (baseCount - 1);
            if (Mathf.Abs(stepX) < 0.01f) stepX = 53f;

            // 重排原版 8 个槽位
            for (int i = 0; i < baseCount; i++)
            {
                if (oldBtns[i] == null) continue;
                RectTransform rt = oldBtns[i].GetComponent<RectTransform>();
                if (rt == null) continue;

                float x = leftX + i * stepX;
                rt.anchoredPosition = new Vector2(x, baseY);
            }

            // 数组长度已匹配 → 只刷新图标
            if (oldIcons.Length == total && oldBtns.Length == total)
            {
                RefreshSignetIcons(tree);
                return;
            }

            // 清理已有的额外槽位（index >= 8）
            for (int i = oldBtns.Length - 1; i >= baseCount; i--)
            {
                if (oldBtns[i] != null)
                    UnityEngine.Object.Destroy(oldBtns[i].gameObject);
            }

            Image[] newIcons = new Image[total];
            Button[] newBtns = new Button[total];

            for (int i = 0; i < baseCount && i < oldIcons.Length && i < oldBtns.Length; i++)
            {
                newIcons[i] = oldIcons[i];
                newBtns[i] = oldBtns[i];
            }

            // 创建新槽位
            if (total > baseCount)
            {
                Button template = oldBtns[0];
                if (template != null)
                {
                    Transform parent = template.transform.parent;
                    if (parent != null)
                    {
                        for (int i = baseCount; i < total; i++)
                        {
                            int rel = i - baseCount;
                            int row = rel / 8 + 1;
                            int col = rel % 8;

                            float x = leftX + col * stepX;
                            float y = baseY + row * stepX;

                            Button nb = CloneSlot(template, parent, i, new Vector2(x, y), tree);
                            newBtns[i] = nb;

                            Image iconImg = FindChildImage(nb);
                            newIcons[i] = iconImg;
                        }
                    }
                }
            }

            AccessTools.Field(typeof(SkillTreeCanvas), "signetIcons").SetValue(tree, newIcons);
            AccessTools.Field(typeof(SkillTreeCanvas.TheSelectorButton), "signetBtn")
                .SetValue(tree.allButtons, newBtns);

            RefreshSignetIcons(tree);

            Plugin.DebugLog("[ED3R] PassiveSlot: expanded to " + total
                + " (stepX=" + stepX.ToString("F2") + ")");
        }

        /// <summary>
        /// 刷新所有 signetIcons 图标为玩家已装被动。
        /// 空槽一律用原版 blank 图标，避免出现白色空块。
        /// </summary>
        private static void RefreshSignetIcons(SkillTreeCanvas tree)
        {
            if (GlobalVar.mainPlayer == null) return;
            var atk = GlobalVar.mainPlayer.GetComponent<AttackTrigger>();
            if (atk == null || atk.skills == null) return;

            var icons = tree.signetIcons;
            if (icons == null) return;

            Sprite blank = PassiveSlotHelper.GetBlankIcon(tree);

            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] == null) continue;

                int skillIdx;
                if (i < PassiveSlotHelper.BaseSlotCount)
                {
                    // 原版 8 个 UI 槽：对应 shortCutSkillID[8..15]
                    skillIdx = i + PassiveSlotHelper.BaseSlotCount;
                }
                else
                {
                    // 扩展槽：对应 shortCutSkillID[20..]
                    int rel = i - PassiveSlotHelper.BaseSlotCount;
                    skillIdx = PassiveSlotHelper.VanillaArrayLength + rel;
                }

                Sprite sp = blank;
                if (skillIdx < atk.skills.Length
                    && atk.skills[skillIdx] != null
                    && atk.skills[skillIdx].icon != null)
                {
                    sp = atk.skills[skillIdx].icon;
                }

                icons[i].sprite = sp;
                icons[i].color = Color.white;
            }
        }

        private static Button CloneSlot(Button template, Transform parent, int newIndex,
            Vector2 pos, SkillTreeCanvas tree)
        {
            GameObject go = UnityEngine.Object.Instantiate(template.gameObject, parent);
            go.name = "SetSlot (" + newIndex + ")";

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = pos;

            // 清空克隆出的内部图标：用 blank 占位，避免白块
            Sprite blank = PassiveSlotHelper.GetBlankIcon(tree);
            foreach (Transform child in go.transform)
            {
                if (child.name.StartsWith("SignetShortcut"))
                {
                    var img = child.GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = blank;
                        img.color = new Color(1f, 1f, 1f, 1f);
                    }
                }
            }

            Button btn = go.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick = new Button.ButtonClickedEvent();
                int capturedIndex = newIndex;
                btn.onClick.AddListener(() =>
                {
                    if (tree != null)
                    {
                        // 扩展槽从 shortCutSkillID[20] 开始，跳过原版预留区 [16..19]
                        int rel = capturedIndex - PassiveSlotHelper.BaseSlotCount;
                        int slot = PassiveSlotHelper.VanillaArrayLength + rel;
                        tree.SetSlot(slot);
                    }
                });
            }

            return btn;
        }

        private static Image FindChildImage(Button btn)
        {
            if (btn == null) return null;

            foreach (Transform child in btn.transform)
            {
                if (child.name.StartsWith("SignetShortcut"))
                {
                    var img = child.GetComponent<Image>();
                    if (img != null) return img;
                }
            }

            Image[] images = btn.GetComponentsInChildren<Image>();
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].gameObject != btn.gameObject)
                    return images[i];
            }

            return null;
        }
    }
}