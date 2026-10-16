namespace EternalDread3RMOD
{
    /// <summary>
    /// 升级经验需求的共享工具。
    /// 
    /// 提供两项：
    ///   1. 独立复刻原版 fallback 路径的 maxExp 公式
    ///   2. 按等级查 5 段百分比配置
    /// </summary>
    public static class ExpRequirementHelper
    {
        /// <summary>
        /// 独立复刻原版 fallback 路径的 maxExp。
        /// 返回处于指定等级时的原版 maxExp（未缩）。
        /// 
        /// 复刻自 Status.LevelUp 里 fallback 分支的迭代逻辑。
        /// 已验证：level=20 → 13440，level=48 → 309010。
        /// </summary>
        public static int ComputeOriginalMaxExp(int level)
        {
            if (level <= 1) return 100;

            int maxExp = 100;
            for (int lv = 2; lv <= level; lv++)
            {
                int num = 140;
                if (lv >= 5) num = 135;
                if (lv >= 10) num = 125;
                if (lv >= 20) num = 118;
                if (lv >= 30) num = 110;
                if (lv >= 40) num = 108;
                if (lv >= 55) num = 105;
                if (lv >= 70) num = 101;

                long tmp = (long)num * maxExp / 100;
                if (tmp >= 2000000000) tmp = 2000000000;
                maxExp = (int)tmp;
            }
            return maxExp;
        }

        /// <summary>
        /// 按等级查当前段百分比。
        /// 段位：1~20 → 索引 0；21~40 → 1；41~60 → 2；61~80 → 3；81~100 → 4。
        /// 解析失败时返回 100（原版速度）。
        /// </summary>
        public static int GetPercentForLevel(int level)
        {
            int idx = (level - 1) / 20;
            if (idx < 0) idx = 0;
            if (idx > 4) idx = 4;

            if (Plugin.ExpRequirementPercents == null) return 100;
            string str = Plugin.ExpRequirementPercents.Value;
            if (string.IsNullOrEmpty(str)) return 100;

            string[] parts = str.Split(',');
            if (idx >= parts.Length) return 100;

            int p;
            if (!int.TryParse(parts[idx].Trim(), out p)) return 100;
            if (p < 1) p = 1;
            if (p > 1000) p = 1000;
            return p;
        }

        /// <summary>
        /// 把当前 status 的 maxExp / exp 按当前等级百分比缩减。
        /// 只在 maxExp 仍为原版值时生效（防重复缩）。
        /// 调用方负责判断 ExpRequirementEnabled。
        /// </summary>
        public static void TryApplyToStatus(Status status)
        {
            if (status == null) return;
            if (status.level < 1 || status.level >= 100) return;

            int original = ComputeOriginalMaxExp(status.level);
            if (status.maxExp != original) return;   // 不是原版值 → 已缩过 → 跳过

            int percent = GetPercentForLevel(status.level);
            if (percent >= 100) return;              // 100% = 原版，不用缩

            long newMax = (long)original * percent / 100;
            if (newMax < 1) newMax = 1;
            status.maxExp = (int)newMax;

            long newExp = (long)status.exp * percent / 100;
            status.exp = (int)newExp;
        }
    }
}