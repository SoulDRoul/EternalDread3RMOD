using System.Collections.Generic;
using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 环绕方位指示器。
    /// 在玩家周围 8 个方位绘制三角形：
    ///   - 灰色半透明脉冲 = 有敌人锁定玩家阵营
    ///   - 红色高亮闪烁   = 有敌人正在攻击（AiStatus.attacking）
    /// 方位计算基于相机朝向，屏幕外敌人也会正确显示。
    /// </summary>
    public class ThreatRingUI : MonoBehaviour
    {
        private const int Sectors = 8;
        private const float DegreesPerSector = 360f / Sectors;

        private readonly bool[] _locked = new bool[Sectors];
        private readonly bool[] _attacking = new bool[Sectors];

        private static Texture2D _triangleTex;

        // ============ 颜色解析缓存 ============
        // 避免每帧 Split + int.Parse。
        private static readonly Dictionary<string, Color> _colorCache = new Dictionary<string, Color>();

        // ============ 相机缓存 ============
        // Unity 2017 里 Camera.main 每次访问都会做一次 FindGameObjectsWithTag("MainCamera")，
        // 开销比想象中大。缓存引用，只在失效时重新查。
        private static Camera _cachedCam;

        private static Camera GetMainCamera()
        {
            if (_cachedCam != null) return _cachedCam;
            _cachedCam = Camera.main;
            return _cachedCam;
        }

        private void OnGUI()
        {
            if (UiSuppress.All) return;
            if (!Plugin.ThreatRingEnabled.Value) return;
            if (GlobalVar.mainPlayer == null) return;
            Camera cam = GetMainCamera();
            if (cam == null) return;

            for (int i = 0; i < Sectors; i++)
            {
                _locked[i] = false;
                _attacking[i] = false;
            }

            CollectThreats(cam);
            DrawRing(cam);
        }

        private void CollectThreats(Camera cam)
        {
            Vector3 playerPos = GlobalVar.mainPlayer.transform.position;
            float maxDist = Plugin.ThreatRingMaxDistance.Value;
            float sqrMaxDist = maxDist * maxDist;

            // 相机朝向（水平化）
            Vector3 camFwd = cam.transform.forward; camFwd.y = 0f;
            Vector3 camRight = cam.transform.right; camRight.y = 0f;
            if (camFwd.sqrMagnitude < 0.0001f) camFwd = Vector3.forward;
            camFwd.Normalize();
            if (camRight.sqrMagnitude < 0.0001f) camRight = Vector3.right;
            camRight.Normalize();

            var list = NoiseRegistry.ActiveAIs;
            for (int i = 0; i < list.Count; i++)
            {
                var ai = list[i];
                if (ai == null) continue;

                if (!IsLockingPlayerSide(ai)) continue;

                Vector3 toEnemy = ai.transform.position - playerPos;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude > sqrMaxDist) continue;

                float fwd = Vector3.Dot(toEnemy, camFwd);
                float right = Vector3.Dot(toEnemy, camRight);

                float angleDeg = Mathf.Atan2(right, fwd) * Mathf.Rad2Deg;
                if (angleDeg < 0f) angleDeg += 360f;

                int sector = Mathf.RoundToInt(angleDeg / DegreesPerSector) % Sectors;

                _locked[sector] = true;

                if (Plugin.ThreatRingShowOnAttack.Value && ThreatAttackingDetector.IsAttacking(ai))
                    _attacking[sector] = true;
            }
        }

        private static bool IsLockingPlayerSide(AiStatus ai)
        {
            var ft = ai.followTarget;
            if (ft == null) return false;
            return ft.CompareTag("Player") || ft.CompareTag("Ally");
        }

        private void DrawRing(Camera cam)
        {
            Vector3 playerPos = GlobalVar.mainPlayer.transform.position;
            Vector3 playerScreen = cam.WorldToScreenPoint(playerPos);
            if (playerScreen.z <= 0f) return;

            float cx = playerScreen.x;
            float cy = Screen.height - playerScreen.y;

            float radius = Plugin.ThreatRingRadius.Value;
            float triSize = Plugin.ThreatRingTriangleSize.Value;

            // 颜色 A 作为基础透明度，动态脉冲/闪烁在此基础上波动。
            // 默认 锁定 A=128（约 50%），攻击 A=255（不透明）。
            Color lockedCol = ParseColor(Plugin.ThreatRingLockedColor.Value,
                new Color(0.7f, 0.7f, 0.7f, 128f / 255f));
            Color attackCol = ParseColor(Plugin.ThreatRingAttackingColor.Value,
                new Color(1f, 0.24f, 0.24f, 1f));

            Texture2D tex = GetTriangleTexture();

            // 波动系数（不含颜色 A）
            float pulse = Mathf.PingPong(Time.time * 0.7f, 1f);
            float lockedPulse = Mathf.Lerp(0.5f, 1.0f, pulse);
            float attackPulse = Mathf.PingPong(Time.time * 3f, 1f) * 0.5f + 0.5f;

            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;

            for (int s = 0; s < Sectors; s++)
            {
                bool isAttack = _attacking[s];
                bool isLock = _locked[s];
                if (!isAttack && !isLock) continue;

                float angleDeg = s * DegreesPerSector;
                float rad = angleDeg * Mathf.Deg2Rad;

                float ex = cx + Mathf.Sin(rad) * radius;
                float ey = cy - Mathf.Cos(rad) * radius;

                Color c = isAttack ? attackCol : lockedCol;
                float baseAlpha = c.a;  // 来自颜色配置的 A
                c.a = (isAttack ? attackPulse : lockedPulse) * baseAlpha;

                GUI.color = c;
                GUI.matrix = Matrix4x4.TRS(
                    new Vector3(ex, ey, 0f),
                    Quaternion.Euler(0f, 0f, angleDeg + 180f),
                    Vector3.one);
                GUI.DrawTexture(
                    new Rect(-triSize * 0.5f, -triSize * 0.5f, triSize, triSize),
                    tex);
            }

            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
        }

        private static Texture2D GetTriangleTexture()
        {
            if (_triangleTex != null) return _triangleTex;

            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float fx = (x + 0.5f) / size * 2f - 1f;
                    float fy = (y + 0.5f) / size * 2f - 1f;

                    // 三角形尖端在纹理顶部 (fy=+1)，底边在底部 (fy=-1)
                    float halfWidth = (1f - fy) * 0.5f;
                    float edge = halfWidth - Mathf.Abs(fx);

                    byte alpha = 0;
                    if (edge > 0.05f) alpha = 255;
                    else if (edge > -0.05f) alpha = (byte)((edge + 0.05f) / 0.1f * 255f);

                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            _triangleTex = tex;
            return tex;
        }

        /// <summary>
        /// 解析颜色字符串（带缓存）。
        /// 支持 "R,G,B" 或 "R,G,B,A"，范围 0~255。
        /// 省略 A 时默认 255（不透明）。
        /// </summary>
        private static Color ParseColor(string str, Color fallback)
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
                    int r = Mathf.Clamp(int.Parse(parts[0].Trim()), 0, 255);
                    int g = Mathf.Clamp(int.Parse(parts[1].Trim()), 0, 255);
                    int b = Mathf.Clamp(int.Parse(parts[2].Trim()), 0, 255);
                    int a = 255;
                    if (parts.Length >= 4)
                        a = Mathf.Clamp(int.Parse(parts[3].Trim()), 0, 255);
                    result = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
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