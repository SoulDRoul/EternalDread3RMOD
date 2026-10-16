using UnityEngine;

namespace EternalDread3RMOD
{
    public static class PassiveSlotHelper
    {
        /// <summary>原版被动 UI 槽数（底排 8 个，不能动）。</summary>
        public const int BaseSlotCount = 8;

        /// <summary>
        /// 原版 Skill.shortCutSkillID 的长度。
        /// 索引 0..7 = 主动技能，8..15 = 原版被动技能，16..19 = 原版预留脏区。
        /// 扩展槽从 20 开始，跳过 16..19。
        /// </summary>
        public const int VanillaArrayLength = 20;

        /// <summary>UI 槽位的硬上限（含原版 8 个）。</summary>
        public const int HardMaxSlotCount = 48;

        /// <summary>返回当前等级应解锁的总 UI 槽数（含原版 8 个）。</summary>
        public static int GetUnlockedSlotCount(int level)
        {
            int perLevels = Plugin.PassiveSlotPerLevels.Value;
            int maxCount = GetMaxSlotCount();
            if (perLevels < 1) perLevels = 1;

            int unlocked = BaseSlotCount + (level - 1) / perLevels;
            if (unlocked > maxCount) unlocked = maxCount;
            if (unlocked < BaseSlotCount) unlocked = BaseSlotCount;
            return unlocked;
        }

        /// <summary>返回 UI 槽位上限（含原版 8 个）。</summary>
        public static int GetMaxSlotCount()
        {
            int maxCount = Plugin.PassiveSlotMaxCount.Value;
            if (maxCount > HardMaxSlotCount) maxCount = HardMaxSlotCount;
            if (maxCount < BaseSlotCount) maxCount = BaseSlotCount;
            return maxCount;
        }

        /// <summary>返回扩展槽的数量（不含原版 8 个 UI 槽）。</summary>
        public static int GetMaxExtraSlotCount()
        {
            return GetMaxSlotCount() - BaseSlotCount;
        }

        /// <summary>
        /// 返回 shortCutSkillID 应该扩展到的目标长度。
        /// = 原版 20 + 扩展槽数。
        /// 例如 UI 上限 32 个槽时，数组目标长度 = 20 + 24 = 44。
        /// </summary>
        public static int GetTargetArrayLength()
        {
            return VanillaArrayLength + GetMaxExtraSlotCount();
        }

        /// <summary>
        /// 获取原版被动空槽的黑色占位图标（sprite 名为 "blank"）。
        /// 优先从玩家第一个原版被动槽取，兜底从 SkillData.skills[0].icon 取。
        /// 取不到时返回 null（此时调用方不要覆盖原 sprite，避免白块）。
        /// </summary>
        public static Sprite GetBlankIcon(SkillTreeCanvas tree)
        {
            // 优先：原版第一个被动槽（skills[8]）的 icon
            if (GlobalVar.mainPlayer != null)
            {
                var atk = GlobalVar.mainPlayer.GetComponent<AttackTrigger>();
                if (atk != null && atk.skills != null
                    && atk.skills.Length > BaseSlotCount
                    && atk.skills[BaseSlotCount] != null
                    && atk.skills[BaseSlotCount].icon != null)
                {
                    return atk.skills[BaseSlotCount].icon;
                }
            }

            // 兜底：SkillData.skills[0].icon
            if (tree != null && tree.db != null
                && tree.db.skills != null
                && tree.db.skills.Length > 0
                && tree.db.skills[0] != null
                && tree.db.skills[0].icon != null)
            {
                return tree.db.skills[0].icon;
            }

            return null;
        }
    }
}