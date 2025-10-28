using System;
using System.Collections.Generic;
using System.Linq;
using Duckov;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace OneKeyLoot
{
    [HarmonyPatch(typeof(Duckov.UI.LootView))]
    internal static class LootViewPatches
    {
        private static readonly AccessTools.FieldRef<Duckov.UI.LootView, Button> PickAllRef =
            AccessTools.FieldRefAccess<Duckov.UI.LootView, Button>("pickAllButton");

        private static readonly AccessTools.FieldRef<
            Duckov.UI.LootView,
            InteractableLootbox
        > TargetLootBoxRef = AccessTools.FieldRefAccess<Duckov.UI.LootView, InteractableLootbox>(
            "targetLootBox"
        );

        private static readonly Color[] DefaultButtonColorPalette =
        [
            UIConstants.Color2,
            UIConstants.Color3,
            UIConstants.Color4,
            UIConstants.Color5,
        ];

        // ✅ 解析颜色 CSV：若任一 token 非法或最终为空 => 回退到默认调色板（最多取 4 个）
        private static List<Color> ParseColorCsv(string csv, IReadOnlyList<Color> fallback)
        {
            const int MaxButtons = 4;
            try
            {
                if (string.IsNullOrWhiteSpace(csv))
                {
                    return [.. fallback.Take(MaxButtons)];
                }

                var tokens = csv.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries);
                var list = new List<Color>(tokens.Length);
                bool invalid = false;
                foreach (var raw in tokens)
                {
                    if (ColorUtility.TryParseHtmlString(raw.Trim(), out var c))
                    {
                        list.Add(c);
                    }
                    else
                    {
                        invalid = true;
                        break;
                    }
                }
                if (invalid || list.Count == 0)
                {
                    return [.. fallback.Take(MaxButtons)];
                }
                return [.. list.Take(MaxButtons)];
            }
            catch
            {
                return [.. fallback.Take(MaxButtons)];
            }
        }

        private static Color GetButtonColor(IReadOnlyList<Color> palette, int ordinal) =>
            palette[Mathf.Abs(ordinal) % Mathf.Max(1, palette.Count)];

        private static readonly Func<Item, int> QualityChecker = static item => item.Quality;
        private static readonly Func<Item, int> ValueChecker = static item =>
            item.GetTotalRawValue() / 2;
        private static readonly Func<Item, int> ValueWeightChecker = static item =>
            (int)(ValueChecker(item) / item.SelfWeight);

        // CSV 解析：失败时回退到默认配置
        private static List<int> ParseRangeCsv(string csv, string fallbackCsv)
        {
            // 最多显示4个按钮
            const int MaxButtons = 4;
            try
            {
                if (string.IsNullOrWhiteSpace(csv))
                {
                    throw new ArgumentException("csv is empty");
                }

                var tokens = csv.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries);
                var list = new List<int>(tokens.Length);
                foreach (var raw in tokens)
                {
                    if (!int.TryParse(raw.Trim(), out var v))
                    {
                        throw new FormatException($"invalid token: {raw}");
                    }
                    list.Add(v);
                }
                return [.. list.Distinct().OrderBy((x) => x).Take(MaxButtons)];
            }
            catch
            {
                var tokens = (fallbackCsv ?? string.Empty).Split(
                    [',', ';', ' '],
                    StringSplitOptions.RemoveEmptyEntries
                );
                var list = new List<int>();
                foreach (var raw in tokens)
                {
                    if (int.TryParse(raw.Trim(), out var v))
                    {
                        list.Add(v);
                    }
                }
                return [.. list.Distinct().OrderBy((x) => x).Take(MaxButtons)];
            }
        }

        private static void PruneRowButtons(RectTransform row, string prefix, HashSet<int> keepSet)
        {
            if (!row)
            {
                return;
            }

            for (int i = row.childCount - 1; i >= 0; --i)
            {
                var t = row.GetChild(i) as RectTransform;
                if (!t)
                {
                    continue;
                }

                var name = t.name ?? "";
                if (!name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                // 从名字解析数值后缀；解析失败也视为应清理的旧节点
                var suffix = name.Substring(prefix.Length);
                if (!int.TryParse(suffix, out var v) || !keepSet.Contains(v))
                {
                    UnityEngine.Object.Destroy(t.gameObject);
                }
            }
        }

        private static void TryPickAllBy(
            Duckov.UI.LootView lv,
            int min,
            Func<Item, int> itemChecker
        )
        {
            AudioManager.Post("UI/confirm");
            var inv = lv?.TargetInventory;
            if (inv == null)
            {
                return;
            }

            Item invOwnerItem = inv.AttachedToItem;
            bool needInspect = inv.NeedInspection;

            var snap = new List<Item>(inv);
            var work = new List<Item>(snap.Count);

            foreach (var it in snap)
            {
                if (
                    it == null
                    || (invOwnerItem != null && ReferenceEquals(it, invOwnerItem))
                    || (needInspect && !it.Inspected)
                    || (
                        !ItemWishlist.Instance.IsManuallyWishlisted(it.TypeID)
                        && itemChecker(it) < min
                    )
                )
                {
                    continue;
                }

                work.Add(it);
            }

            if (work.Count == 0)
            {
                return;
            }
            PickAll(lv, work);
        }

        private static void PickAll(Duckov.UI.LootView lv, List<Item> work)
        {
            var characterItem = LevelManager.Instance?.MainCharacter?.CharacterItem;
            foreach (var item in work)
            {
                bool plugged = characterItem?.TryPlug(item, true, null, 0) ?? false;
                if (!plugged)
                {
                    ItemUtilities.SendToPlayerCharacterInventory(item, false);
                }
            }
        }

#pragma warning disable IDE0051
        [HarmonyPostfix]
        [HarmonyPatch("RefreshPickAllButton")]
        private static void Postfix_RefreshPickAllButton(object __instance)
        {
            if (!ModConfig.Runtime.showCollectAll)
            {
                return;
            }
            var lv = (Duckov.UI.LootView)__instance;

            var targetLootBox = TargetLootBoxRef(lv);
            if (targetLootBox != null && targetLootBox.name == "PlayerStorage")
            {
                return;
            }

            var pickAll = PickAllRef(lv);
            if (!pickAll)
            {
                return;
            }

            pickAll.gameObject.SetActive(true);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnOpen")]
        private static void Postfix_OnOpen(object __instance)
        {
            var lv = (Duckov.UI.LootView)__instance;

            var targetLootBox = TargetLootBoxRef(lv);
            if (targetLootBox != null && targetLootBox.name == "PlayerStorage")
            {
                return;
            }

            var pickAll = PickAllRef(lv);
            if (!pickAll)
            {
                return;
            }

            var parent = pickAll.transform.parent as RectTransform;
            if (!parent)
            {
                return;
            }

            // 基于 pickAll 推导控件基准宽高（布局未刷新时使用 preferred 值）
            var lePick =
                pickAll.GetComponent<LayoutElement>()
                ?? pickAll.gameObject.AddComponent<LayoutElement>();
            float baseW =
                lePick.preferredWidth > 0
                    ? lePick.preferredWidth
                    : pickAll.GetComponent<RectTransform>().rect.width;
            float baseH =
                lePick.preferredHeight > 0
                    ? lePick.preferredHeight
                    : pickAll.GetComponent<RectTransform>().rect.height;
            if (baseW <= 0)
            {
                baseW = 200f;
            }

            if (baseH <= 0)
            {
                baseH = 48f;
            }

            // Quality：Panel + Title + Row
            var qRow = UIHelpers.FindOrCreateRect(
                parent,
                UIConstants.QualityRowName,
                siblingAfter: pickAll.transform
            );
            UIHelpers.SetupRow(qRow, baseH);
            // 行宽固定为与 pickAll 一致，避免被父容器拉满
            var qRowLe =
                qRow.GetComponent<LayoutElement>() ?? qRow.gameObject.AddComponent<LayoutElement>();
            qRowLe.flexibleWidth = 0f;
            qRowLe.preferredWidth = baseW;

            var qPanel = UIHelpers.FindOrCreateRect(
                parent,
                UIConstants.QualityPanelName,
                siblingIndex: qRow.GetSiblingIndex()
            );
            qRow.SetSiblingIndex(qPanel.GetSiblingIndex() + 1);
            UIHelpers.SetupPanel(qPanel, pickAll.image, baseH); // 复用 pickAll 的 9-slice 样式与宽度
            var qTitle = UIHelpers.CreateOrUpdateTitleTMP(
                qPanel.gameObject,
                UIConstants.QualityTitleName,
                i18n.Quality.Title
            );
            UIHelpers.ApplyRefFont(qTitle, pickAll);

            // Value：Panel + Title + Row
            var vRow = UIHelpers.FindOrCreateRect(
                parent,
                UIConstants.ValueRowName,
                siblingAfter: qRow.transform
            );
            UIHelpers.SetupRow(vRow, baseH);
            var vRowLe =
                vRow.GetComponent<LayoutElement>() ?? vRow.gameObject.AddComponent<LayoutElement>();
            vRowLe.flexibleWidth = 0f;
            vRowLe.preferredWidth = baseW;

            var vPanel = UIHelpers.FindOrCreateRect(
                parent,
                UIConstants.ValuePanelName,
                siblingIndex: vRow.GetSiblingIndex()
            );
            vRow.SetSiblingIndex(vPanel.GetSiblingIndex() + 1);
            UIHelpers.SetupPanel(vPanel, pickAll.image, baseH);
            var vTitle = UIHelpers.CreateOrUpdateTitleTMP(
                vPanel.gameObject,
                UIConstants.ValueTitleName,
                i18n.Value.Title
            );
            UIHelpers.ApplyRefFont(vTitle, pickAll);

            // ValueWeight：Panel + Title + Row
            var vwRow = UIHelpers.FindOrCreateRect(
                parent,
                UIConstants.ValueWeightRowName,
                siblingAfter: vRow.transform
            );
            UIHelpers.SetupRow(vwRow, baseH);
            var vwRowLe =
                vwRow.GetComponent<LayoutElement>()
                ?? vwRow.gameObject.AddComponent<LayoutElement>();
            vwRowLe.flexibleWidth = 0f;
            vwRowLe.preferredWidth = baseW;

            var vwPanel = UIHelpers.FindOrCreateRect(
                parent,
                UIConstants.ValueWeightPanelName,
                siblingIndex: vwRow.GetSiblingIndex()
            );
            vwRow.SetSiblingIndex(vwPanel.GetSiblingIndex() + 1);
            UIHelpers.SetupPanel(vwPanel, pickAll.image, baseH);
            var vwTitle = UIHelpers.CreateOrUpdateTitleTMP(
                vwPanel.gameObject,
                UIConstants.ValueWeightTitleName,
                i18n.ValueWeight.Title
            );
            UIHelpers.ApplyRefFont(vwTitle, pickAll);

            var defaultPalette = DefaultButtonColorPalette;

            // 解析配置范围
            var cfg = ModConfig.Runtime;
            var qualityList = ParseRangeCsv(cfg.qualityRange, ModConfigData.Defaults.qualityRange);
            var valueList = ParseRangeCsv(cfg.valueRange, ModConfigData.Defaults.valueRange);
            var weightList = ParseRangeCsv(
                cfg.valueWeightRange,
                ModConfigData.Defaults.valueWeightRange
            );

            var qColors = ParseColorCsv(cfg.qualityColor, defaultPalette);
            var vColors = ParseColorCsv(cfg.valueColor, defaultPalette);
            var vwColors = ParseColorCsv(cfg.valueWeightColor, defaultPalette);

            // 清理旧按钮
            PruneRowButtons(qRow, "OKL_Button_Quality_", [.. qualityList]);
            PruneRowButtons(vRow, "OKL_Button_Value_", [.. valueList]);
            PruneRowButtons(vwRow, "OKL_Button_ValueWeight_", [.. weightList]);

            // 目标宽度
            float spacing = UIConstants.ButtonRowSpacing;
            float qTargetW = UIHelpers.CalcTargetWidth(baseW, spacing, qualityList.Count);
            float vTargetW = UIHelpers.CalcTargetWidth(baseW, spacing, valueList.Count);
            float vwTargetW = UIHelpers.CalcTargetWidth(baseW, spacing, weightList.Count);
            float targetH = baseH;

            var refShadow = pickAll.GetComponent<Shadow>();
            var refOutline = pickAll.GetComponent<Outline>();

            // 质量按钮
            for (int i = 0; i < qualityList.Count; i++)
            {
                int minQ = qualityList[i];
                string name = $"OKL_Button_Quality_{minQ}";
                var color = GetButtonColor(qColors, i);
                CreateFilterButton(
                    qRow,
                    pickAll,
                    qTargetW,
                    targetH,
                    name,
                    color,
                    minQ,
                    refShadow,
                    refOutline,
                    lv,
                    QualityChecker,
                    i18n.Quality.Button,
                    i18n.Quality.ButtonFontSize,
                    i
                );
            }

            // 价值按钮
            for (int i = 0; i < valueList.Count; i++)
            {
                int minV = valueList[i];
                string name = $"OKL_Button_Value_{minV}";
                var color = GetButtonColor(vColors, i);
                CreateFilterButton(
                    vRow,
                    pickAll,
                    vTargetW,
                    targetH,
                    name,
                    color,
                    minV,
                    refShadow,
                    refOutline,
                    lv,
                    ValueChecker,
                    i18n.Value.Button,
                    i18n.Value.ButtonFontSize,
                    i
                );
            }

            // 价重比按钮
            for (int i = 0; i < weightList.Count; i++)
            {
                int minW = weightList[i];
                string name = $"OKL_Button_ValueWeight_{minW}";
                var color = GetButtonColor(vwColors, i);
                CreateFilterButton(
                    vwRow,
                    pickAll,
                    vwTargetW,
                    targetH,
                    name,
                    color,
                    minW,
                    refShadow,
                    refOutline,
                    lv,
                    ValueWeightChecker,
                    i18n.ValueWeight.Button,
                    i18n.ValueWeight.ButtonFontSize,
                    i
                );
            }

            // 显隐控制
            qPanel.gameObject.SetActive(cfg.showQuality);
            qRow.gameObject.SetActive(cfg.showQuality);
            vPanel.gameObject.SetActive(cfg.showValue);
            vRow.gameObject.SetActive(cfg.showValue);
            vwPanel.gameObject.SetActive(cfg.showValueWeight);
            vwRow.gameObject.SetActive(cfg.showValueWeight);

            if (parent.Find("OKL_BottomSpacer") == null)
            {
                var sp = new GameObject("OKL_BottomSpacer", typeof(RectTransform));
                var spRt = sp.GetComponent<RectTransform>();
                spRt.SetParent(parent, false);
                spRt.anchorMin = new Vector2(0f, 0.5f);
                spRt.anchorMax = new Vector2(1f, 0.5f);
                spRt.pivot = new Vector2(0.5f, 0.5f);
                spRt.sizeDelta = Vector2.zero;
                var le = sp.AddComponent<LayoutElement>();
                le.flexibleWidth = 1f;
                le.preferredHeight = UIConstants.BottomSpacerHeight;
                sp.SetActive(true);
            }

            i18n.RelabelActiveLootViews();
        }
#pragma warning restore IDE0051
        // 新增通用按钮创建：质量/价值/价重比 共用
        private static void CreateFilterButton(
            RectTransform row,
            Button pickAll,
            float w,
            float h,
            string name,
            Color bgcolor,
            int minThreshold,
            Shadow refShadow,
            Outline refOutline,
            Duckov.UI.LootView lv,
            Func<Item, int> checker,
            Func<int, string> labelBuilder,
            int fontSize,
            int siblingIndex
        )
        {
            var child = row.Find(name) as RectTransform;
            GameObject go;
            RectTransform r;
            if (child)
            {
                r = child;
                go = child.gameObject;
            }
            else
            {
                go = UnityEngine.Object.Instantiate(pickAll.gameObject, row);
                go.name = name;
                r = go.GetComponent<RectTransform>();
            }
            int maxIdx = Mathf.Max(0, row.childCount - 1);
            // 保证按钮的兄弟顺序与排序后的列表索引一致
            r.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, maxIdx));

            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            le.minWidth = w;
            le.preferredWidth = w;
            le.flexibleWidth = 0f;
            le.minHeight = h;
            le.preferredHeight = h;
            le.flexibleHeight = 0f;

            // 同步到 RectTransform，确保与 Layout 一致
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

            var btn = go.GetComponent<Button>();
            var bg = btn ? btn.image : go.GetComponent<Image>();
            if (bg)
            {
                bg.material = null;
                var c = bg.color;
                c.a = 1f;
                bg.color = c;
                bg.CrossFadeAlpha(1f, 0f, true);
                bg.raycastTarget = true;
            }
            if (btn)
            {
                var colors = btn.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.pressedColor = Color.white;
                colors.selectedColor = Color.white;
                colors.disabledColor = Color.white;
                btn.colors = colors;

                btn.interactable = false;
                btn.interactable = true;
                btn.transition = Selectable.Transition.None;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => TryPickAllBy(lv, minThreshold, checker));
            }
            if (bg)
            {
                bg.color = bgcolor;
            }

            var refHasStyle = refShadow != null || refOutline != null;
            if (refHasStyle)
            {
                UIHelpers.CopyShadowAndOutline(refShadow, refOutline, go);
            }
            else
            {
                UIHelpers.EnsureDefaultShadow(go);
            }

            UIHelpers.EnsureSingleCenteredLabel(go, labelBuilder(minThreshold), pickAll, fontSize);

            foreach (var g in go.GetComponentsInChildren<Graphic>(true))
            {
                if (g == bg)
                {
                    continue;
                }
                g.raycastTarget = false;
            }
            go.SetActive(true);
        }
    }
}
