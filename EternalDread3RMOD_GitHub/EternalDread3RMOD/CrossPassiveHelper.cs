using System;
using UnityEngine;
using UnityEngine.UI;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 跨职业被动切换的核心逻辑。
    /// 缓存每个 tab 的原始 buttonId，切换时把当前 tab 的被动按钮
    /// 的 buttonId 替换成目标 tab 对应的 buttonId。
    ///
    /// 同时维护"职业相似轮"映射，用于成本缩放。
    /// </summary>
    public static class CrossPassiveHelper
    {
        // [tabIndex][slotIndex] = 原始 buttonId
        private static int[][] _originalIds = null;
        private static bool _initialized = false;
        private static System.Collections.Generic.HashSet<int> _passiveButtonIds = null;

        /// <summary>当前显示的被动职业（0~7）。切换时沿相似轮循环。</summary>
        public static int CurrentPassiveJob = -1;

        // ================================================================
        // 职业相似轮
        // 顺时针：Warrior → Guardian → BattleMage → Mage → Sage → Engineer → Ranger → Rogue
        // ================================================================

        /// <summary>轮位置 0~7 → 职业索引（PlayerJob）</summary>
        public static readonly int[] WheelOrder = new int[8]
        {
            0,  // 0: Warrior
            5,  // 1: Guardian
            3,  // 2: Battle Mage (SwordMage)
            2,  // 3: Mage
            6,  // 4: Sage
            4,  // 5: Engineer
            1,  // 6: Ranger
            7,  // 7: Rogue
        };

        /// <summary>职业索引（PlayerJob） → 轮位置 0~7</summary>
        public static readonly int[] JobToWheel = new int[8]
        {
            0,  // Warrior    → 0
            6,  // Ranger     → 6
            3,  // Mage       → 3
            2,  // BattleMage → 2
            5,  // Engineer   → 5
            1,  // Guardian   → 1
            4,  // Sage       → 4
            7,  // Rogue      → 7
        };

        /// <summary>
        /// 计算两个职业在相似轮上的最短距离（0~4）。
        /// 0 = 同一职业；4 = 对面职业。
        /// </summary>
        public static int GetWheelDistance(int jobA, int jobB)
        {
            if (jobA < 0 || jobA >= 8) return 0;
            if (jobB < 0 || jobB >= 8) return 0;

            int d1 = Mathf.Abs(JobToWheel[jobA] - JobToWheel[jobB]);
            int d2 = 8 - d1;
            return Mathf.Min(d1, d2);
        }

        /// <summary>
        /// 根据距离计算成本倍率。
        ///   距离 0 → ×1.0
        ///   距离 1 → ×NearMul  (默认 1.1)
        ///   距离 4 → ×FarMul   (默认 3.0)
        ///   中间线性插值
        /// </summary>
        public static float GetCostMul(int distance)
        {
            if (distance <= 0) return 1.0f;

            float near = Plugin.CrossPassiveCostNearMul.Value / 100f;
            float far = Plugin.CrossPassiveCostFarMul.Value / 100f;
            if (near < 1.0f) near = 1.0f;
            if (far < near) far = near;

            if (distance >= 4) return far;

            // 距离 1 → near, 距离 4 → far，中间线性
            float t = (distance - 1) / 3f;
            return near + (far - near) * t;
        }

        /// <summary>
        /// 获取"当前视图职业"（主职业或副职业，视当前激活的 jobTab 而定）。
        /// 该职业被视作"本职业"，距离 0。
        /// </summary>
        public static int GetCurrentViewJob(SkillTreeCanvas tree)
        {
            if (tree == null || tree.jobTab == null) return -1;
            for (int i = 0; i < tree.jobTab.Length; i++)
                if (tree.jobTab[i] != null && tree.jobTab[i].activeSelf) return i;
            return -1;
        }

        /// <summary>
        /// 计算当前被动职业相对"当前视图职业"的成本倍率。
        /// 若 CurrentPassiveJob < 0，返回 1.0。
        /// </summary>
        public static float GetCurrentCostMul(SkillTreeCanvas tree)
        {
            if (CurrentPassiveJob < 0) return 1.0f;
            int viewJob = GetCurrentViewJob(tree);
            if (viewJob < 0) return 1.0f;

            int dist = GetWheelDistance(CurrentPassiveJob, viewJob);
            return GetCostMul(dist);
        }

        /// <summary>
        /// 判断一个 buttonId 是否为被动技能按钮。
        /// 依赖 InitializeCache 收集到的"所有 tab 的 14~29 号按钮原始 buttonId"集合。
        /// 若尚未初始化，返回 false（视为主动，走原版）。
        /// </summary>
        public static bool IsPassiveButtonId(int buttonId)
        {
            if (_passiveButtonIds == null) return false;
            return _passiveButtonIds.Contains(buttonId);
        }

        // ================================================================
        // 按钮缓存
        // ================================================================

        public static void InitializeCache(SkillTreeCanvas tree)
        {
            if (tree == null || tree.allButtons == null) return;

            var ab = tree.allButtons;
            _originalIds = new int[8][];
            _originalIds[0] = CacheTab(ab.warriorSkill);
            _originalIds[1] = CacheTab(ab.rangerSkill);
            _originalIds[2] = CacheTab(ab.mageSkill);
            _originalIds[3] = CacheTab(ab.battleMageSkill);
            _originalIds[4] = CacheTab(ab.engineerSkill);
            _originalIds[5] = CacheTab(ab.guardianSkill);
            _originalIds[6] = CacheTab(ab.sageSkill);
            _originalIds[7] = CacheTab(ab.rogueSkill);
            _initialized = true;

            _passiveButtonIds = new System.Collections.Generic.HashSet<int>();
            for (int tab = 0; tab < _originalIds.Length; tab++)
            {
                var ids = _originalIds[tab];
                if (ids == null) continue;
                for (int i = 14; i <= 29; i++)
                {
                    if (i >= ids.Length) break;
                    if (ids[i] >= 0) _passiveButtonIds.Add(ids[i]);
                }
            }
        }

        private static int[] CacheTab(Button[] btns)
        {
            if (btns == null) return null;
            var ids = new int[btns.Length];
            for (int i = 0; i < btns.Length; i++)
            {
                var sbc = (btns[i] != null) ? btns[i].GetComponent<SkillButtonCanvas>() : null;
                ids[i] = (sbc != null) ? sbc.buttonId : -1;
            }
            return ids;
        }

        public static int GetCurrentTab(SkillTreeCanvas tree)
        {
            return GetCurrentViewJob(tree);
        }

        public static Button[] GetTabButtons(SkillTreeCanvas tree, int tab)
        {
            if (tree == null || tree.allButtons == null) return null;
            var ab = tree.allButtons;
            switch (tab)
            {
                case 0: return ab.warriorSkill;
                case 1: return ab.rangerSkill;
                case 2: return ab.mageSkill;
                case 3: return ab.battleMageSkill;
                case 4: return ab.engineerSkill;
                case 5: return ab.guardianSkill;
                case 6: return ab.sageSkill;
                case 7: return ab.rogueSkill;
            }
            return null;
        }

        /// <summary>把当前 tab 的被动按钮（索引 14~29）改成 passiveJob 对应的 buttonId。</summary>
        public static void ApplyPassiveJob(SkillTreeCanvas tree, int passiveJob)
        {
            if (!_initialized) InitializeCache(tree);
            if (_originalIds == null) return;

            int currentTab = GetCurrentTab(tree);
            if (currentTab < 0) return;

            Button[] currentBtns = GetTabButtons(tree, currentTab);
            if (currentBtns == null) return;

            for (int i = 14; i <= 29; i++)
            {
                if (i >= currentBtns.Length) break;
                var sbc = (currentBtns[i] != null)
                    ? currentBtns[i].GetComponent<SkillButtonCanvas>() : null;
                if (sbc == null) continue;

                int targetId = GetOriginalId(passiveJob, i);
                if (targetId < 0) continue;

                sbc.buttonId = targetId;
                sbc.SettingUp();
                sbc.UpdateIcon();
            }
        }

        /// <summary>把当前 tab 的被动按钮恢复成原始 buttonId。</summary>
        public static void ResetToOriginal(SkillTreeCanvas tree)
        {
            if (!_initialized) InitializeCache(tree);
            if (_originalIds == null) return;

            int currentTab = GetCurrentTab(tree);
            if (currentTab < 0) return;

            Button[] currentBtns = GetTabButtons(tree, currentTab);
            if (currentBtns == null) return;

            for (int i = 14; i <= 29; i++)
            {
                if (i >= currentBtns.Length) break;
                var sbc = (currentBtns[i] != null) ? currentBtns[i].GetComponent<SkillButtonCanvas>() : null;
                if (sbc == null) continue;

                int origId = GetOriginalId(currentTab, i);
                if (origId < 0) continue;

                sbc.buttonId = origId;
                sbc.SettingUp();
                sbc.UpdateIcon();
            }
        }

        private static int GetOriginalId(int tab, int slotIndex)
        {
            if (_originalIds == null) return -1;
            if (tab < 0 || tab >= _originalIds.Length) return -1;
            var arr = _originalIds[tab];
            if (arr == null || slotIndex >= arr.Length) return -1;
            return arr[slotIndex];
        }

        public static string GetJobName(int job)
        {
            switch (job)
            {
                case 0: return "Warrior";
                case 1: return "Ranger";
                case 2: return "Mage";
                case 3: return "Battle Mage";
                case 4: return "Engineer";
                case 5: return "Guardian";
                case 6: return "Sage";
                case 7: return "Rogue";
            }
            return "?";
        }
    }
}