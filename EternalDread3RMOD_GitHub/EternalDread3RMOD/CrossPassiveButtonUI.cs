using UnityEngine;
using UnityEngine.UI;

namespace EternalDread3RMOD
{
    /// <summary>
    /// 跨职业被动切换按钮 + 职业名称标签 + 成本倍率标签。
    /// 切换顺序按"职业相似轮"走。
    /// </summary>
    public class CrossPassiveButtonUI : MonoBehaviour
    {
        private SkillTreeCanvas _tree;
        private Text _jobLabel;
        private Text _mulLabel;

        // ============ 硬编码常量：SkillTree.CrossPassive.UI ============
        // 原 SkillTree.CrossPassive.UI.Gap —— 两个切换按钮与原版副职业按钮之间的间距（像素）
        private const int BTN_GAP = 8;
        // 原 SkillTree.CrossPassive.UI.Scale —— 切换按钮尺寸缩放百分比（100 = 与原版副职业按钮一样大）
        private const int BTN_SCALE_PERCENT = 80;
        // 原 SkillTree.CrossPassive.UI.AspectRatioPercent —— 宽高比（50 = 宽 = 高 × 0.5）
        private const int BTN_ASPECT_PERCENT = 50;
        // 原 SkillTree.CrossPassive.UI.LabelOffsetX —— 职业名称标签相对副职业按钮中心的 X 偏移
        private const int JOB_LABEL_OFFSET_X = 55;
        // 原 SkillTree.CrossPassive.UI.LabelOffsetY —— 职业名称标签相对副职业按钮中心的 Y 偏移
        private const int JOB_LABEL_OFFSET_Y = 402;
        // 原 SkillTree.CrossPassive.UI.LabelFontSize —— 职业名称标签字号
        private const int JOB_LABEL_FONT_SIZE = 18;
        // 原 SkillTree.CrossPassive.UI.LabelColor —— 职业名称/倍率标签颜色（原字符串 "30,20,0"）
        private static readonly Color LABEL_COLOR = new Color(30f / 255f, 20f / 255f, 0f);
        // 原 SkillTree.CrossPassive.UI.MulLabelOffsetX —— 成本倍率标签相对副职业按钮中心的 X 偏移
        private const int MUL_LABEL_OFFSET_X = 62;
        // 原 SkillTree.CrossPassive.UI.MulLabelOffsetY —— 成本倍率标签相对副职业按钮中心的 Y 偏移
        private const int MUL_LABEL_OFFSET_Y = 384;
        // 原 SkillTree.CrossPassive.UI.MulLabelFontSize —— 成本倍率标签字号
        private const int MUL_LABEL_FONT_SIZE = 15;

        public static void Attach(SkillTreeCanvas tree)
        {
            if (tree == null) return;
            if (tree.jobSwitchTabSprite == null) return;
            if (tree.jobSwitchTabSprite.Length < 2) return;
            if (tree.jobSwitchTabSprite[0] == null) return;
            if (tree.jobSwitchTabSprite[1] == null) return;
            if (tree.jobSwitchTabSprite[0].btn == null) return;
            if (tree.jobSwitchTabSprite[1].btn == null) return;

            CrossPassiveHelper.InitializeCache(tree);

            // 每次进入面板都重置被动职业为当前视图职业
            int currentTab = CrossPassiveHelper.GetCurrentTab(tree);
            if (currentTab >= 0)
                CrossPassiveHelper.CurrentPassiveJob = currentTab;

            var btn0 = tree.jobSwitchTabSprite[0].btn;
            var btn1 = tree.jobSwitchTabSprite[1].btn;

            var rt0 = btn0.GetComponent<RectTransform>();
            var rt1 = btn1.GetComponent<RectTransform>();
            if (rt0 == null || rt1 == null) return;

            var parent = btn1.transform.parent;
            if (parent == null) return;

            if (parent.Find("CrossPassivePrevBtn") != null)
            {
                // 已挂载：重置被动按钮回主职业，刷新标签
                CrossPassiveHelper.ResetToOriginal(tree);

                var existingLabel = parent.Find("CrossPassiveJobLabel");
                if (existingLabel != null)
                {
                    var txt = existingLabel.GetComponent<Text>();
                    if (txt != null)
                        txt.text = CrossPassiveHelper.GetJobName(CrossPassiveHelper.CurrentPassiveJob);
                }

                var existingMul = parent.Find("CrossPassiveMulLabel");
                if (existingMul != null)
                {
                    var txt = existingMul.GetComponent<Text>();
                    if (txt != null)
                        txt.text = FormatMulLabel(tree);
                }
                return;
            }

            // 从副职业按钮提取视觉参数
            var refImg = btn1.GetComponent<Image>();
            Sprite sprite = (refImg != null) ? refImg.sprite : null;
            Color bgColor = (refImg != null) ? refImg.color : new Color(0.2f, 0.2f, 0.2f, 1f);
            Image.Type imgType = (refImg != null) ? refImg.type : Image.Type.Simple;

            var refText = btn1.GetComponentInChildren<Text>();
            Font font = (refText != null) ? refText.font : null;
            int fontSize = (refText != null) ? refText.fontSize : 20;
            Color textColor = (refText != null) ? refText.color : Color.white;

            float refH = rt1.sizeDelta.y;
            float refW = rt1.sizeDelta.x;
            float scale = BTN_SCALE_PERCENT / 100f;
            float aspect = BTN_ASPECT_PERCENT / 100f;
            float gap = BTN_GAP;

            float h = refH * scale;
            float w = h * aspect;

            Vector2 pos0 = rt0.anchoredPosition;
            Vector2 pos1 = rt1.anchoredPosition;
            float halfRefW = refW / 2f;
            float y = pos1.y;

            float prevX = pos0.x - halfRefW - gap - w / 2f;
            float nextX = pos1.x + halfRefW + gap + w / 2f;

            var prevGo = CreateButton(parent, "CrossPassivePrevBtn", "<",
                sprite, bgColor, imgType, font, fontSize, textColor,
                new Vector2(prevX, y), w, h);

            var nextGo = CreateButton(parent, "CrossPassiveNextBtn", ">",
                sprite, bgColor, imgType, font, fontSize, textColor,
                new Vector2(nextX, y), w, h);

            var comp = parent.gameObject.GetComponent<CrossPassiveButtonUI>();
            if (comp == null) comp = parent.gameObject.AddComponent<CrossPassiveButtonUI>();
            comp._tree = tree;

            var prevBtn = prevGo.GetComponent<Button>();
            var nextBtn = nextGo.GetComponent<Button>();
            if (prevBtn != null) prevBtn.onClick.AddListener(comp.OnPrev);
            if (nextBtn != null) nextBtn.onClick.AddListener(comp.OnNext);

            // ===== 职业名称标签 =====
            if (parent.Find("CrossPassiveJobLabel") == null)
            {
                var labelGo = new GameObject("CrossPassiveJobLabel");
                var labelRt = labelGo.AddComponent<RectTransform>();
                labelRt.SetParent(parent, false);
                labelRt.anchorMin = new Vector2(0.5f, 0.5f);
                labelRt.anchorMax = new Vector2(0.5f, 0.5f);
                labelRt.pivot = new Vector2(0.5f, 0.5f);
                labelRt.sizeDelta = new Vector2(240f, 40f);

                labelRt.anchoredPosition = pos1 + new Vector2(JOB_LABEL_OFFSET_X, JOB_LABEL_OFFSET_Y);

                var labelText = labelGo.AddComponent<Text>();
                labelText.text = CrossPassiveHelper.GetJobName(CrossPassiveHelper.CurrentPassiveJob);
                labelText.alignment = TextAnchor.MiddleCenter;
                labelText.font = font;
                labelText.fontSize = JOB_LABEL_FONT_SIZE;
                labelText.color = LABEL_COLOR;
                labelText.raycastTarget = false;

                comp._jobLabel = labelText;
            }

            // ===== 成本倍率标签（放在职业名称下方）=====
            if (parent.Find("CrossPassiveMulLabel") == null)
            {
                var mulGo = new GameObject("CrossPassiveMulLabel");
                var mulRt = mulGo.AddComponent<RectTransform>();
                mulRt.SetParent(parent, false);
                mulRt.anchorMin = new Vector2(0.5f, 0.5f);
                mulRt.anchorMax = new Vector2(0.5f, 0.5f);
                mulRt.pivot = new Vector2(0.5f, 0.5f);
                mulRt.sizeDelta = new Vector2(240f, 30f);

                mulRt.anchoredPosition = pos1 + new Vector2(MUL_LABEL_OFFSET_X, MUL_LABEL_OFFSET_Y);

                var mulText = mulGo.AddComponent<Text>();
                mulText.text = FormatMulLabel(tree);
                mulText.alignment = TextAnchor.MiddleCenter;
                mulText.font = font;
                mulText.fontSize = MUL_LABEL_FONT_SIZE;
                mulText.color = LABEL_COLOR;
                mulText.raycastTarget = false;

                comp._mulLabel = mulText;
            }
        }

        /// <summary>格式化倍率标签，例如 "x1.1" / "x1.0"。</summary>
        public static string FormatMulLabel(SkillTreeCanvas tree)
        {
            float mul = CrossPassiveHelper.GetCurrentCostMul(tree);
            return "x" + mul.ToString("F1");
        }

        private static GameObject CreateButton(Transform parent, string name, string label,
            Sprite sprite, Color bgColor, Image.Type imgType,
            Font font, int fontSize, Color textColor,
            Vector2 pos, float w, float h)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = bgColor;
            img.type = imgType;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var textGo = new GameObject("Text");
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.SetParent(go.transform, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = font;
            text.fontSize = fontSize;
            text.color = textColor;
            text.raycastTarget = false;

            return go;
        }

        /// <summary>按相似轮前进。</summary>
        public void OnNext()
        {
            if (_tree == null) return;
            int cur = CrossPassiveHelper.CurrentPassiveJob;
            if (cur < 0) return;

            int wheel = CrossPassiveHelper.JobToWheel[cur];
            int nextWheel = (wheel + 1) % 8;
            int nextJob = CrossPassiveHelper.WheelOrder[nextWheel];

            CrossPassiveHelper.CurrentPassiveJob = nextJob;
            CrossPassiveHelper.ApplyPassiveJob(_tree, nextJob);
            UpdateLabels();
        }

        /// <summary>按相似轮后退。</summary>
        public void OnPrev()
        {
            if (_tree == null) return;
            int cur = CrossPassiveHelper.CurrentPassiveJob;
            if (cur < 0) return;

            int wheel = CrossPassiveHelper.JobToWheel[cur];
            int prevWheel = (wheel + 7) % 8;
            int prevJob = CrossPassiveHelper.WheelOrder[prevWheel];

            CrossPassiveHelper.CurrentPassiveJob = prevJob;
            CrossPassiveHelper.ApplyPassiveJob(_tree, prevJob);
            UpdateLabels();
        }

        public void UpdateLabels()
        {
            if (_jobLabel != null)
                _jobLabel.text = CrossPassiveHelper.GetJobName(CrossPassiveHelper.CurrentPassiveJob);
            if (_mulLabel != null && _tree != null)
                _mulLabel.text = FormatMulLabel(_tree);
        }

        private void UpdateLabel()
        {
            UpdateLabels();
        }
    }
}