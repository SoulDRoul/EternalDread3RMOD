using UnityEngine;

namespace EternalDread3RMOD
{
    public class MinimapIcon : MonoBehaviour
    {
        public Transform target;
        public Status targetStatus;
        public MinimapIconCategory category;
        public Color baseColor;
        public float worldHeight;
        public float worldSize;
        public IconShape shape;
        public int sortingOrder;
        public SpriteRenderer spriteRenderer;
        private AiStatus _ai;
        public bool IsAngry { get; private set; }
        public bool IsLocked { get; private set; }
        private bool _lockedInitialized;

        // ============ 玩家 Status 缓存 ============
        // UpdateEnemyColor 每 3 秒被全体敌人图标调用一次，
        // 原实现每次都 GetComponent<Status>()。这里缓存，玩家对象变化时自动失效。
        private static object _cachedPlayerRef;
        private static Status _cachedPlayerStatus;

        public void SetColor(Color c)
        {
            baseColor = c;
            MinimapIconManager.RefreshIconSprite(this, c, shape, category);
        }

        public void UpdateEnemyColor()
        {
            if (category != MinimapIconCategory.Enemy) return;
            if (!Plugin.MinimapEnemyLevelColorEnabled.Value) return;
            if (targetStatus == null) return;

            var player = GlobalVar.mainPlayer;
            if (player == null) return;

            // 玩家对象变化时才重新查 Status
            if (!ReferenceEquals(_cachedPlayerRef, player))
            {
                _cachedPlayerRef = player;
                _cachedPlayerStatus = player.GetComponent<Status>();
            }
            var playerStat = _cachedPlayerStatus;
            if (playerStat == null) return;

            int diff = playerStat.level - targetStatus.level;

            Color c;
            float size;
            int order;

            if (diff >= Plugin.MinimapEnemyLevelLowThreshold.Value)
            {
                c = MinimapIconManager.ParseColor(
                    Plugin.MinimapEnemyLevelLowColor.Value, new Color(1f, 0.9f, 0.24f));
                size = MinimapIconManager.MM_SIZE_BASE
                    * MinimapIconManager.MM_ENEMY_LOW_PCT / 100f / 100f;
                order = MinimapIconManager.MM_ENEMY_LOW_ORDER;
            }
            else if (diff <= -Plugin.MinimapEnemyLevelHighThreshold.Value)
            {
                c = MinimapIconManager.ParseColor(
                    Plugin.MinimapEnemyLevelHighColor.Value, new Color(0.78f, 0.24f, 0.86f));
                size = MinimapIconManager.MM_SIZE_BASE
                    * MinimapIconManager.MM_ENEMY_HIGH_PCT / 100f / 100f;
                order = MinimapIconManager.MM_ENEMY_HIGH_ORDER;
            }
            else
            {
                c = MinimapIconManager.ParseColor(
                    Plugin.MinimapEnemyLevelNearColor.Value, new Color(1f, 0.55f, 0f));
                size = MinimapIconManager.MM_SIZE_BASE
                    * MinimapIconManager.MM_ENEMY_NEAR_PCT / 100f / 100f;
                order = MinimapIconManager.MM_ENEMY_NEAR_ORDER;
            }

            baseColor = c;
            worldSize = size;
            sortingOrder = order;

            MinimapIconManager.RefreshIconSprite(this, c, shape, category);
            if (spriteRenderer != null) spriteRenderer.sortingOrder = order;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                MinimapIconRegistry.Unregister(this);
                Destroy(gameObject);
                return;
            }

            transform.position = target.position + Vector3.up * worldHeight;
            transform.localScale = Vector3.one * worldSize;   // ★ 这一行不能少

            if (shape == IconShape.Arrow)
            {
                float yaw = target.eulerAngles.y;
                transform.rotation = Quaternion.Euler(90f, yaw + 180f, 0f);
            }
            else
            {
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }

            UpdateAngryState();
            UpdateLockedState();
            UpdateFade();
        }

        private void UpdateAngryState()
        {
            if (category != MinimapIconCategory.Enemy) return;
            if (!Plugin.AngryBorderEnabled.Value) return;

            if (_ai == null && target != null)
                _ai = target.GetComponent<AiStatus>();
            if (_ai == null) return;

            bool now = false;
            var ft = _ai.followTarget;
            if (ft != null)
            {
                if (ft.CompareTag("Player") || ft.CompareTag("Ally"))
                    now = true;
            }

            if (now == IsAngry) return;

            IsAngry = now;
            MinimapIconManager.RefreshIconSprite(this, baseColor, shape, category);
        }

        private void UpdateLockedState()
        {
            if (category != MinimapIconCategory.SkillPoint) return;
            if (target == null) return;

            bool locked = !target.gameObject.activeInHierarchy;

            // 首次：若已是红球（默认 sprite 是金边），需刷新；若白球，默认 sprite 已对
            if (!_lockedInitialized)
            {
                _lockedInitialized = true;
                if (!locked) return;
            }
            else
            {
                if (locked == IsLocked) return;
            }

            IsLocked = locked;
            MinimapIconManager.RefreshIconSprite(this, baseColor, shape, category);
        }

        private void UpdateFade()
        {
            if (spriteRenderer == null) return;

            // 关闭渐隐 → 保证完全不透明
            if (!Plugin.MinimapFadeEnabled.Value)
            {
                if (Mathf.Abs(spriteRenderer.color.a - 1f) > 0.001f)
                {
                    spriteRenderer.color = Color.white;
                }
                return;
            }

            var player = GlobalVar.mainPlayer;
            if (player == null || target == null) return;

            float dist = Vector3.Distance(
                transform.position,
                player.transform.position);

            float start, end;
            if (category == MinimapIconCategory.Enemy)
            {
                start = Plugin.MinimapEnemyFadeStart.Value;
                end = Plugin.MinimapEnemyFadeEnd.Value;
            }
            else
            {
                start = Plugin.MinimapResourceFadeStart.Value;
                end = Plugin.MinimapResourceFadeEnd.Value;
            }

            if (end <= start) end = start + 1f;

            float alpha;
            if (dist <= start) alpha = 1f;
            else if (dist >= end) alpha = 0f;
            else alpha = 1f - (dist - start) / (end - start);

            var c = spriteRenderer.color;
            if (Mathf.Abs(c.a - alpha) > 0.005f)
                spriteRenderer.color = new Color(1f, 1f, 1f, alpha);

        }

        private void OnDestroy()
        {
            MinimapIconRegistry.Unregister(this);
        }
    }
}