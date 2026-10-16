using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 玩家自带光源。
    /// 常开（不检测场景亮度），Point Light 跟随玩家位置。
    /// 默认强度较低，避免在有其他光源时显得突兀。
    /// 
    /// 设计取舍：
    ///   - 不做场景暗检测：洞穴/神殿/夜晚都可能已有火把，检测成本高、误判多
    ///   - 常开 + 低强度：有火把时玩家感觉不到，无光源时玩家刚好看得见
    ///   - 无阴影：Point Light 阴影开销大，性能优先
    ///   - 独立 GameObject：与玩家对象解耦，玩家重建/换场景都不受影响
    /// </summary>
    public class PlayerLightController : MonoBehaviour
    {
        // ============================================================
        // 【固定项】—— 如需调整请修改此处常量
        // ============================================================

        /// <summary>光源相对玩家脚底的高度偏移（米）。1.0 = 腰部高度，避免贴地。</summary>
        private const float HeightOffset = 1.0f;

        /// <summary>光源类型。Point = 全向光（跟随玩家位置，不跟随朝向）。</summary>
        private const LightType FixedLightType = LightType.Point;

        /// <summary>阴影类型。None = 无阴影（性能友好，Point Light 阴影开销大）。</summary>
        private const LightShadows FixedShadows = LightShadows.None;

        /// <summary>渲染模式。Auto = 让 Unity 根据距离自动选择像素/顶点光照。</summary>
        private const LightRenderMode FixedRenderMode = LightRenderMode.Auto;

        // ============================================================
        // 运行时状态
        // ============================================================

        private Light _light;

        // 颜色字符串缓存（避免每帧 Split 产生 GC）
        private string _lastColorStr;
        private Color _cachedColor = new Color(1f, 0.92f, 0.78f, 1f);
        private float _cachedIntensity = 0.5f;

        private void Update()
        {
            bool on = Plugin.PlayerLightEnabled != null && Plugin.PlayerLightEnabled.Value;
            var player = GlobalVar.mainPlayer;

            // 关闭状态 / 无玩家 → 关闭 Light 组件（但不销毁对象，便于重新开启）
            if (!on || player == null)
            {
                if (_light != null && _light.enabled)
                    _light.enabled = false;
                return;
            }

            // 首次需要时创建 Light 组件
            if (_light == null)
                CreateLight();

            if (!_light.enabled)
                _light.enabled = true;

            // --- 同步位置 ---
            transform.position = player.transform.position
                + new Vector3(0f, HeightOffset, 0f);

            // --- 同步颜色 / 强度（只在变化时写，避免 Unity 内部脏检查开销）---
            RefreshColor(Plugin.PlayerLightColor.Value);
            if (_light.color != _cachedColor) _light.color = _cachedColor;
            if (_light.intensity != _cachedIntensity) _light.intensity = _cachedIntensity;

            // --- 同步范围 ---
            float range = Plugin.PlayerLightRange.Value;
            if (_light.range != range) _light.range = range;
        }

        private void CreateLight()
        {
            _light = gameObject.AddComponent<Light>();
            _light.type = FixedLightType;
            _light.shadows = FixedShadows;
            _light.renderMode = FixedRenderMode;

            // 初始值（Update 下一帧会覆盖）
            _light.color = _cachedColor;
            _light.intensity = _cachedIntensity;
            _light.range = 15f;
        }

        /// <summary>
        /// 解析颜色字符串并缓存结果。
        /// 字符串未变化时跳过解析，避免每帧 Split 产生 GC。
        /// </summary>
        private void RefreshColor(string str)
        {
            if (str == _lastColorStr) return;
            _lastColorStr = str;

            // 解析失败时保留上一次的有效值
            _cachedColor = ParseColor(str, out _cachedIntensity,
                _cachedColor, _cachedIntensity);
        }

        /// <summary>
        /// 解析 "R,G,B,A" 格式的颜色字符串。
        ///   R,G,B = 颜色（0~255）
        ///   A     = 强度（0~255），映射到 intensity 0.0~1.0
        /// 省略 A 时默认 128（强度 0.5）。
        /// 解析失败时返回 fallbackColor / fallbackIntensity。
        /// </summary>
        private static Color ParseColor(string str, out float intensity,
            Color fallbackColor, float fallbackIntensity)
        {
            intensity = fallbackIntensity;
            try
            {
                var parts = str.Split(',');
                if (parts.Length < 3) return fallbackColor;

                int r = Mathf.Clamp(int.Parse(parts[0].Trim()), 0, 255);
                int g = Mathf.Clamp(int.Parse(parts[1].Trim()), 0, 255);
                int b = Mathf.Clamp(int.Parse(parts[2].Trim()), 0, 255);

                int a = 128; // 默认强度 0.5
                if (parts.Length >= 4)
                    a = Mathf.Clamp(int.Parse(parts[3].Trim()), 0, 255);

                intensity = a / 255f;
                return new Color(r / 255f, g / 255f, b / 255f, 1f);
            }
            catch
            {
                return fallbackColor;
            }
        }
    }
}