using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneKeyLoot
{
    /// <summary>
    /// 通用 UI 辅助方法
    /// </summary>
    internal static class UIHelpers
    {
        // 行容器：水平布局
        internal static void SetupRow(RectTransform row, float baseH)
        {
            var layout =
                row.GetComponent<HorizontalLayoutGroup>()
                ?? row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true; // 让行宽由子按钮的 preferredWidth 控制
            layout.childControlHeight = false;
            layout.spacing = UIConstants.ButtonRowSpacing;
            layout.padding = new RectOffset(0, 0, 0, 0);

            var le =
                row.GetComponent<LayoutElement>() ?? row.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 0f; // 行不再被父容器拉满
            le.preferredHeight = baseH;
        }

        // 标题面板：对齐与背景（复用 pickAll 的样式）
        internal static void SetupPanel(RectTransform panel, Image refImage, float baseH)
        {
            if (!panel)
            {
                return;
            }

            // 没有 LayoutElement 则补充，避免 NRE
            var le =
                panel.GetComponent<LayoutElement>()
                ?? panel.gameObject.AddComponent<LayoutElement>();

            // 与“全部拾取”同宽
            float baseW = 0f;
            if (refImage != null && refImage.rectTransform != null)
            {
                baseW = refImage.rectTransform.rect.width;
            }
            if (baseW <= 0f)
            {
                baseW = 200f;
            }

            le.flexibleWidth = 0f;
            le.preferredWidth = baseW;
            le.preferredHeight = Mathf.Round(baseH * 0.90f);

            // 没有 Image 则补充，并复用 pickAll 的 9-slice
            var img = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
            if (refImage != null)
            {
                img.sprite = refImage.sprite;
                img.type = refImage.type;
                img.pixelsPerUnitMultiplier = refImage.pixelsPerUnitMultiplier;
                img.material = refImage.material;
            }
            img.color = new Color(0f, 0f, 0f, 0.35f);
            img.raycastTarget = false;
        }

        // 创建/更新标题 TMP
        internal static RectTransform CreateOrUpdateTitleTMP(
            GameObject host,
            string nodeName,
            string text
        )
        {
            RectTransform rt;
            var child = host.transform.Find(nodeName) as RectTransform;
            if (child)
            {
                rt = child;
            }
            else
            {
                var go = new GameObject(nodeName, typeof(RectTransform));
                rt = go.GetComponent<RectTransform>();
                rt.SetParent(host.transform, false);
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(12f, 0f);
                rt.sizeDelta = Vector2.zero;
            }

            var tmp =
                rt.GetComponent<TextMeshProUGUI>() ?? rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null && tmp.font == null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.fontSize = UIConstants.TitleFontSize;
            tmp.color = UIConstants.TitleColor;
            tmp.enableAutoSizing = false;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;

            // 深色阴影提升可读性
            var sh = tmp.gameObject.GetComponent<Shadow>() ?? tmp.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.5f);
            sh.effectDistance = new Vector2(2f, -2f);
            return rt;
        }

        internal static RectTransform FindOrCreateRect(
            Transform parent,
            string name,
            Transform siblingAfter = null,
            int? siblingIndex = null
        )
        {
            var t = parent.Find(name) as RectTransform;
            if (t)
            {
                return t;
            }

            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            if (siblingIndex.HasValue)
            {
                rt.SetSiblingIndex(Mathf.Clamp(siblingIndex.Value, 0, parent.childCount - 1));
            }
            else if (siblingAfter != null)
            {
                rt.SetSiblingIndex(siblingAfter.GetSiblingIndex() + 1);
            }
            return rt;
        }

        internal static void CopyShadowAndOutline(
            Shadow srcShadow,
            Outline srcOutline,
            GameObject dst
        )
        {
            if (srcShadow != null)
            {
                var s = dst.GetComponent<Shadow>() ?? dst.AddComponent<Shadow>();
                s.effectColor = srcShadow.effectColor;
                s.effectDistance = srcShadow.effectDistance;
            }
            if (srcOutline != null)
            {
                var o = dst.GetComponent<Outline>() ?? dst.AddComponent<Outline>();
                o.effectColor = srcOutline.effectColor;
                o.effectDistance = srcOutline.effectDistance;
            }
        }

        internal static void EnsureDefaultShadow(GameObject go)
        {
            var s = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>();
            s.effectColor = new Color(0f, 0f, 0f, 0.35f);
            s.effectDistance = new Vector2(1.2f, -1.2f);
        }

        internal static void ApplyRefFont(RectTransform rt, Button refPickAll)
        {
            if (!rt || !refPickAll)
            {
                return;
            }

            var dst = rt.GetComponent<TextMeshProUGUI>();
            var src = refPickAll.GetComponentInChildren<TextMeshProUGUI>(true);
            if (dst && src)
            {
                dst.font = src.font;
                dst.fontMaterial = src.fontMaterial;
            }
        }

        internal static RectTransform EnsureSingleCenteredLabel(
            GameObject host,
            string text,
            Button refPickAllForTextEffect
        )
        {
            var tmps = host.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in tmps)
            {
                if (t && t.gameObject.name != UIConstants.LabelName)
                {
                    Object.Destroy(t.gameObject);
                }
            }
            var uguiTexts = host.GetComponentsInChildren<Text>(true);
            foreach (var tt in uguiTexts)
            {
                if (tt && tt.gameObject.name != UIConstants.LabelName)
                {
                    Object.Destroy(tt.gameObject);
                }
            }

            var label = host.transform.Find(UIConstants.LabelName) as RectTransform;
            if (!label)
            {
                var go = new GameObject(UIConstants.LabelName, typeof(RectTransform));
                label = go.GetComponent<RectTransform>();
                label.SetParent(host.transform, false);
                label.anchorMin = Vector2.zero;
                label.anchorMax = Vector2.one;
                label.pivot = new Vector2(0.5f, 0.5f);
                label.offsetMin = Vector2.zero;
                label.offsetMax = Vector2.zero;
            }

            var tmp =
                label.GetComponent<TextMeshProUGUI>()
                ?? label.gameObject.AddComponent<TextMeshProUGUI>();
            var refTmp = refPickAllForTextEffect
                ? refPickAllForTextEffect.GetComponentInChildren<TextMeshProUGUI>(true)
                : null;
            if (refTmp)
            {
                tmp.font = refTmp.font;
                tmp.fontMaterial = refTmp.fontMaterial;
            }
            else if (TMP_Settings.defaultFontAsset != null && tmp.font == null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }

            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Midline;
            tmp.fontSize = 28;
            tmp.raycastTarget = false;
            return label;
        }

        internal static RectTransform EnsureSingleCenteredLabel(
            GameObject host,
            string text,
            Button refPickAllForTextEffect,
            int fontSize
        )
        {
            var rt = EnsureSingleCenteredLabel(host, text, refPickAllForTextEffect);
            var tmp = rt.GetComponent<TextMeshProUGUI>();
            if (tmp)
            {
                tmp.fontSize = fontSize;
            }
            return rt;
        }

        internal static float CalcTargetWidth(float totalWidth, float spacing, int count)
        {
            count = Mathf.Max(1, count);
            return Mathf.Max(1f, (totalWidth - spacing * (count - 1)) / count);
        }
    }
}
