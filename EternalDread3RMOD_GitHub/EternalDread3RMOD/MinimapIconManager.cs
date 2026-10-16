using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    public enum IconShape
    {
        Circle,
        Triangle,       // 正三角（尖朝上）
        TriangleDown,   // 倒三角（尖朝下）
        Diamond,
        Square,         // 正方形
        Arrow,          // ★ 新增：圆+尖，尖朝 +Y（本地坐标）
        CircleSmall     // ★ 新增：小圆点，半径 0.55
    }

    public enum MinimapIconCategory
    {
        Enemy,
        Ally,
        Resource,
        Chest,
        SkillPoint
    }

    public struct IconLayer
    {
        public float Scale;
        public Color Color;

        public IconLayer(float scale, Color color)
        {
            Scale = scale;
            Color = color;
        }
    }

    public static class MinimapIconManager
    {
        public const int MinimapLayer = 11;

        // ============ 硬编码常量：Minimap.Sizes（public，供各 Patch 调用 AttachIcon 时使用） ============
        // 原 Minimap.Sizes.BaseSize —— 基准尺寸（所有图标按百分比乘以此值）
        public const int MM_SIZE_BASE = 1500;
        // 原 Minimap.Sizes.EnemyHighPct —— 高级敌人 225%
        public const int MM_ENEMY_HIGH_PCT = 225;
        // 原 Minimap.Sizes.EnemyNearPct —— 同级敌人 210%
        public const int MM_ENEMY_NEAR_PCT = 210;
        // 原 Minimap.Sizes.EnemyLowPct —— 低级敌人 200%
        public const int MM_ENEMY_LOW_PCT = 200;
        // 原 Minimap.Sizes.ResourceLogPct —— 木头 120%
        public const int MM_RES_LOG_PCT = 120;
        // 原 Minimap.Sizes.ResourceOrePct —— 矿石 120%
        public const int MM_RES_ORE_PCT = 120;
        // 原 Minimap.Sizes.ResourcePlantPct —— 植物 100%
        public const int MM_RES_PLANT_PCT = 100;
        // 原 Minimap.Sizes.ChestPct —— 宝箱 100%
        public const int MM_CHEST_PCT = 100;
        // 原 Minimap.Sizes.SkillPointPct —— 技能点 110%
        public const int MM_SKILLPOINT_PCT = 110;
        // 原 Minimap.Sizes.AllyPct —— 友军 80%
        public const int MM_ALLY_PCT = 80;

        // ============ 硬编码常量：Minimap.Orders（public，供各 Patch 调用 AttachIcon 时使用） ============
        // 数字越大越靠上层
        public const int MM_ENEMY_HIGH_ORDER = 300;
        public const int MM_ENEMY_NEAR_ORDER = 200;
        public const int MM_ENEMY_LOW_ORDER = 150;
        public const int MM_RES_LOG_ORDER = 160;
        public const int MM_RES_ORE_ORDER = 160;
        public const int MM_RES_PLANT_ORDER = 130;
        public const int MM_SKILLPOINT_ORDER = 400;
        public const int MM_CHEST_ORDER = 180;
        public const int MM_ALLY_ORDER = 100;

        // ============ 硬编码常量：Minimap 高度（public，供各 Patch 与扫描器调用 AttachIcon 时使用） ============
        // 原 Minimap.Height —— 敌人/友军图标相对脚底的高度（百分比；100 = 1 单位）
        public const int MM_ICON_HEIGHT = 200;
        // 原 Minimap.ResourceHeight —— 资源图标高度（木头/矿石/植物共用）
        public const int MM_RES_HEIGHT = 150;
        // 原 Minimap.ChestHeight —— 宝箱图标高度
        public const int MM_CHEST_HEIGHT = 150;
        // 原 Minimap.SkillPointHeight —— 技能点图标高度
        public const int MM_SKILLPOINT_HEIGHT = 150;

        // ============ 硬编码常量：Minimap.Outline（private，仅本类 BuildLayers 使用） ============
        // 原 Minimap.Outline.BlackPct —— 黑边厚度（占图标半径百分比）
        private const int MM_OUTLINE_BLACK_PCT = 16;
        // 原 Minimap.Outline.WhitePct —— 白边厚度（资源图标最外层）
        private const int MM_OUTLINE_WHITE_PCT = 5;
        // 原 Minimap.Outline.AllyBlackPct —— 友军细黑边厚度
        private const int MM_ALLY_BLACK_PCT = 20;
        // 原 Minimap.Outline.SkillPointGoldPct —— 技能点金边厚度
        private const int MM_SKILLPOINT_GOLD_PCT = 50;
        // 原 Minimap.Outline.RedPct —— 敌人图标最外层白边厚度
        private const int MM_OUTLINE_RED_PCT = 10;
        // 原 Minimap.Outline.RedColor —— 敌人图标白边颜色，原默认 "255,255,255"
        private static readonly Color MM_OUTLINE_RED_COLOR = new Color(1f, 1f, 1f);

        private const int TexSize = 64;
        private const int SubSamples = 2;   // 每轴 2 采样 = 4 sub-sample/px

        private static Dictionary<string, Sprite> _cache
            = new Dictionary<string, Sprite>();

        // ============ 颜色解析缓存 ============
        // 避免 RefreshAllEnemies 每 3 秒 × N 敌人重复 Split + int.Parse。
        private static readonly Dictionary<string, Color> _colorCache
            = new Dictionary<string, Color>();

        // ============ 对外 API ============

        public static MinimapIcon AttachIcon(
            GameObject target,
            Color color,
            float worldHeight,
            float worldSize,
            IconShape shape,
            MinimapIconCategory category,
            Status targetStatus = null,
            int sortingOrder = 100)
        {
            if (target == null) return null;

            var layers = BuildLayers(category, color);
            var sprite = GetSprite(shape, layers);

            var go = new GameObject("MinimapIcon");
            go.layer = MinimapLayer;
            go.transform.position = target.transform.position + Vector3.up * worldHeight;
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = Vector3.one * worldSize;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = Color.white;   // 颜色已在 sprite 里
            sr.sortingOrder = sortingOrder;

            var icon = go.AddComponent<MinimapIcon>();
            icon.target = target.transform;
            icon.targetStatus = targetStatus;
            icon.category = category;
            icon.baseColor = color;
            icon.worldHeight = worldHeight;
            icon.worldSize = worldSize;
            icon.shape = shape;
            icon.sortingOrder = sortingOrder;
            icon.spriteRenderer = sr;

            

            if (category == MinimapIconCategory.Enemy)
            {
                MinimapIconRegistry.Register(icon);
                icon.UpdateEnemyColor();
            }

            return icon;
        }

        /// <summary>敌人变色时需要重新生成 sprite，用这个方法替换。</summary>
        public static void RefreshIconSprite(
    MinimapIcon icon, Color newColor,
    IconShape shape, MinimapIconCategory category)
        {
            if (icon == null) return;
            bool isAngry = icon.IsAngry;
            bool isLocked = icon.IsLocked;
            var layers = BuildLayers(category, newColor, isAngry, isLocked);
            var sprite = GetSprite(shape, layers);
            if (icon.spriteRenderer != null)
            {
                icon.spriteRenderer.sprite = sprite;
                icon.spriteRenderer.color = Color.white;
            }
        }

        // ============ 层构建 ============

        private static IconLayer[] BuildLayers(MinimapIconCategory cat, Color main, bool isAngry = false, bool isLocked = false)
        {
            // 描边厚度（相对图标半径的百分比，0~100）
            float blackPct = MM_OUTLINE_BLACK_PCT / 100f;
            float whitePct = MM_OUTLINE_WHITE_PCT / 100f;
            float goldPct = MM_SKILLPOINT_GOLD_PCT / 100f;
            float allyBlackPct = MM_ALLY_BLACK_PCT / 100f;

            switch (cat)
            {
                case MinimapIconCategory.Enemy:
                    {
                        float redPct = MM_OUTLINE_RED_PCT / 100f;
                        if (redPct < 0f) redPct = 0f;
                        if (redPct > 0.5f) redPct = 0.5f;

                        if (redPct <= 0.001f)
                        {
                            // 不画红边 → 退回原逻辑
                            return new[]
                            {
            new IconLayer(1f - blackPct, main),
            new IconLayer(1f, Color.black)
        };
                        }

                        Color borderColor;
                        if (isAngry && Plugin.AngryBorderEnabled.Value)
                            borderColor = ParseColor(Plugin.AngryBorderColor.Value,
                                new Color(1f, 0.24f, 0.24f));
                        else
                            borderColor = MM_OUTLINE_RED_COLOR;

                        // 色 → 黑 → 红（从内到外）
                        return new[]
                        {
        new IconLayer(1f - blackPct - redPct, main),
        new IconLayer(1f - redPct, Color.black),
        new IconLayer(1f, borderColor)
    };
                    }

                case MinimapIconCategory.Chest:
                    return new[]
                    {
                        new IconLayer(1f - blackPct, main),
                        new IconLayer(1f, Color.black)
                    };

                case MinimapIconCategory.Ally:
                    return new[]
                    {
                        new IconLayer(1f - allyBlackPct, main),
                        new IconLayer(1f, Color.black)
                    };

                case MinimapIconCategory.Resource:
                    // 色 → 黑 → 白
                    return new[]
                    {
                        new IconLayer(1f - blackPct - whitePct, main),
                        new IconLayer(1f - whitePct, Color.black),
                        new IconLayer(1f, Color.white)
                    };

                case MinimapIconCategory.SkillPoint:
                    // 白 → 金（无黑边）；被锁定时改为 白 → 红
                    Color gold = ParseColor(Plugin.MinimapSkillPointGoldColor.Value,
                        new Color(1f, 0.82f, 0.24f));
                    Color spBorder = gold;
                    if (isLocked)
                    {
                        spBorder = ParseColor(Plugin.MinimapSkillPointLockedColor.Value,
                            new Color(1f, 0.24f, 0.24f));
                    }
                    return new[]
                    {
                        new IconLayer(1f - goldPct, main),
                        new IconLayer(1f, spBorder)
                    };
            }

            return new[] { new IconLayer(1f, main) };
        }

        // ============ Sprite 缓存 ============

        private static Sprite GetSprite(IconShape shape, IconLayer[] layers)
        {
            string key = MakeKey(shape, layers);
            Sprite s;
            if (_cache.TryGetValue(key, out s) && s != null) return s;
            s = RenderSprite(shape, layers);
            _cache[key] = s;
            return s;
        }

        private static string MakeKey(IconShape shape, IconLayer[] layers)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append((int)shape);
            for (int i = 0; i < layers.Length; i++)
            {
                sb.Append('|');
                sb.Append(Mathf.RoundToInt(layers[i].Scale * 1000));
                sb.Append(':');
                sb.Append(Mathf.RoundToInt(layers[i].Color.r * 255));
                sb.Append(',');
                sb.Append(Mathf.RoundToInt(layers[i].Color.g * 255));
                sb.Append(',');
                sb.Append(Mathf.RoundToInt(layers[i].Color.b * 255));
            }
            return sb.ToString();
        }

        // ============ 渲染核心 ============

        private static Sprite RenderSprite(IconShape shape, IconLayer[] layers)
        {
            var tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color32[TexSize * TexSize];
            float half = TexSize * 0.5f;
            float invSub = 1f / SubSamples;
            int totalSub = SubSamples * SubSamples;

            for (int py = 0; py < TexSize; py++)
            {
                for (int px = 0; px < TexSize; px++)
                {
                    Color accum = Color.clear;
                    for (int sy = 0; sy < SubSamples; sy++)
                    {
                        for (int sx = 0; sx < SubSamples; sx++)
                        {
                            // sub-pixel 坐标 → [-1, 1]
                            float fx = (px + (sx + 0.5f) * invSub - half) / half;
                            float fy = (py + (sy + 0.5f) * invSub - half) / half;
                            accum += SampleColor(shape, layers, fx, fy);
                        }
                    }
                    accum /= totalSub;
                    pixels[py * TexSize + px] = (Color32)accum;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            return Sprite.Create(tex,
                new Rect(0, 0, TexSize, TexSize),
                new Vector2(0.5f, 0.5f),
                TexSize);
        }

        private static Color SampleColor(IconShape shape, IconLayer[] layers, float x, float y)
        {
            // 从最内到最外，找到第一个包含 (x,y) 的层
            for (int i = 0; i < layers.Length; i++)
            {
                if (IsInside(shape, x, y, layers[i].Scale))
                    return layers[i].Color;
            }
            return Color.clear;
        }

        // ============ 形状判定 ============

        /// <summary>scale = 1 时是标准形状（半径 1），scale &lt; 1 是收缩后的。</summary>
        private static bool IsInside(IconShape shape, float x, float y, float scale)
        {
            if (scale <= 0f) return false;
            float sx = x / scale;
            float sy = y / scale;

            switch (shape)
            {
                case IconShape.Circle:
                    return sx * sx + sy * sy <= 1f;

                case IconShape.Diamond:
                    return Mathf.Abs(sx) + Mathf.Abs(sy) <= 1f;

                case IconShape.Triangle:
                    // 尖朝上：顶点 (0, 0.9)，底边 y = -0.6，底边半宽 0.75
                    if (sy < -0.6f || sy > 0.9f) return false;
                    {
                        float t = (sy + 0.6f) / 1.5f;       // 0=底边, 1=顶点
                        float halfW = (1f - t) * 0.75f;
                        return Mathf.Abs(sx) <= halfW;
                    }

                case IconShape.TriangleDown:
                    // 尖朝下：顶点 (0, -0.9)，顶边 y = 0.6
                    if (sy > 0.6f || sy < -0.9f) return false;
                    {
                        float t = (0.6f - sy) / 1.5f;
                        float halfW = (1f - t) * 0.75f;
                        return Mathf.Abs(sx) <= halfW;
                    }

                case IconShape.Square:
                    return Mathf.Abs(sx) <= 0.85f && Mathf.Abs(sy) <= 0.85f;

                case IconShape.Arrow:
                    {
                        // ========== 圆：以圆心 (0, 0.35) 同心缩放 ==========
                        {
                            float cr = 0.4f * scale;
                            float cdx = sx;
                            float cdy = sy - 0.45f;
                            if (cdx * cdx + cdy * cdy <= cr * cr) return true;
                        }

                        // ========== 三角形：以三角形中心 (0, -0.396) 为锚点缩放 ==========
                        const float triCy = -0.396f;
                        float usx = sx / scale;
                        float usy = (sy - triCy) / scale + triCy;

                        if (usy < -0.9f || usy > 0.108f) return false;
                        float triT = (usy + 0.9f) / 1.008f;
                        float triW = triT * 0.45f;
                        return Mathf.Abs(usx) <= triW;
                    }

            }
            return false;
        }

        // ============ 工具 ============

        public static Color ParseColor(string str, Color fallback)
        {
            if (string.IsNullOrEmpty(str)) return fallback;
            if (_colorCache.TryGetValue(str, out Color cached)) return cached;

            Color result;
            try
            {
                var parts = str.Split(',');
                if (parts.Length < 3)
                {
                    result = fallback;
                }
                else
                {
                    int r = int.Parse(parts[0].Trim());
                    int g = int.Parse(parts[1].Trim());
                    int b = int.Parse(parts[2].Trim());
                    result = new Color(r / 255f, g / 255f, b / 255f);
                }
            }
            catch
            {
                result = fallback;
            }
            _colorCache[str] = result;
            return result;
        }
    }
}