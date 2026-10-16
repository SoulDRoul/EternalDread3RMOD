using UnityEngine;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 技能点图标补挂扫描器。
    ///
    /// 背景：红球状态下 SkillPoint 的 GameObject 是 SetActive(false)，
    ///       Unity 不会为 inactive 对象调用 Start，
    ///       导致 Patch_AddSkillPointEv_Start_Minimap 的 Postfix 从未执行，
    ///       图标也就没挂上。
    ///
    /// 方案：每 2 秒扫描一次所有 AddSkillPointEv（含 inactive），
    ///       给没有 MinimapIconTag 的补挂图标。
    ///       挂上后由 MinimapIcon.UpdateLockedState 负责颜色自动切换。
    /// </summary>
    public class SkillPointIconScanner : MonoBehaviour
    {
        private float _next;

        private void Update()
        {
            if (!Plugin.MinimapSkillPointEnabled.Value) return;
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 6f;

            var sps = Resources.FindObjectsOfTypeAll<AddSkillPointEv>();
            if (sps == null) return;

            for (int i = 0; i < sps.Length; i++)
            {
                var sp = sps[i];
                if (sp == null) continue;

                var go = sp.gameObject;
                if (go == null) continue;
                if (!go.scene.IsValid() || !go.scene.isLoaded) continue;
                if (go.GetComponent<MinimapIconTag>() != null) continue;

                AttachIcon(go);
            }
        }

        private void AttachIcon(GameObject go)
        {
            var color = MinimapIconManager.ParseColor(
                Plugin.MinimapSkillPointColor.Value, Color.white);
            float h = MinimapIconManager.MM_SKILLPOINT_HEIGHT / 100f;
            float size = MinimapIconManager.MM_SIZE_BASE
                * MinimapIconManager.MM_SKILLPOINT_PCT / 10000f;
            int order = MinimapIconManager.MM_SKILLPOINT_ORDER;

            MinimapIconManager.AttachIcon(
                go,
                color,
                h,
                size,
                IconShape.Diamond,
                MinimapIconCategory.SkillPoint,
                null,
                order);

            go.AddComponent<MinimapIconTag>();
        }
    }
}