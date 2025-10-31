using System;
using System.Collections.Generic;
using System.Linq;
using Duckov;
using Duckov.UI;
using HarmonyLib;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace OneKeyLoot
{
    [HarmonyPatch(typeof(LootView))]
    internal static class LootViewPatches
    {
        private static readonly AccessTools.FieldRef<LootView, Button> PickAllRef =
            AccessTools.FieldRefAccess<LootView, Button>("pickAllButton");

        private static readonly AccessTools.FieldRef<
            LootView,
            InteractableLootbox
        > TargetLootBoxRef = AccessTools.FieldRefAccess<LootView, InteractableLootbox>(
            "targetLootBox"
        );

        private static readonly Color[] DefaultButtonColorPalette =
        [
            UIConstants.Color2,
            UIConstants.Color3,
            UIConstants.Color4,
            UIConstants.Color5,
        ];

        // 缓存结构 + 全局列表（弱引用，避免持有强引用导致泄漏）
        private sealed class CacheEntry
        {
            public WeakReference<LootView> LvRef;
            public RectTransform Parent;
            public RectTransform Root;
            public Button PickAll;
        }

        private static readonly Dictionary<RectTransform, CacheEntry> s_cache = new();

        static LootViewPatches()
        {
            ModConfig.RuntimeChanged += OnRuntimeConfigChanged;
        }

        private static void OnRuntimeConfigChanged()
        {
            var toRemove = new List<RectTransform>();
            foreach (KeyValuePair<RectTransform, CacheEntry> kvp in s_cache)
            {
                var entry = kvp.Value;
                if (entry == null || entry.Parent == null || !entry.Parent)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }
                if (!entry.LvRef.TryGetTarget(out var lv) || lv == null)
                {
                    toRemove.Add(kvp.Key);
                    continue;
                }
                ReapplyFromConfig(lv, entry.Parent, entry.PickAll);
            }
            foreach (var key in toRemove)
            {
                s_cache.Remove(key);
            }

            i18n.RelabelActiveLootViews();
        }

        // 创建根节点，所有UI元素全部挂载在根节点下，方便一键控制
        private static RectTransform CreateRootNode(RectTransform parent, Transform placeAfter)
        {
            var rt = parent.Find("OKL_Root") as RectTransform;
            if (rt == null)
            {
                var go = new GameObject("OKL_Root", typeof(RectTransform));
                rt = go.GetComponent<RectTransform>();
                rt.SetParent(parent, false);

                // 拉伸到父节点宽度（高度由子物体+Layout计算）
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(1f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = Vector2.zero;

                // 让 OKL_Root 作为一个“块”参与父容器的竖直布局与自适应高度
                var vlg = go.AddComponent<VerticalLayoutGroup>();
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.spacing = 0;
                vlg.padding = new RectOffset(0, 0, 0, 0);

                var csf = go.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // 可选：占位用的 LayoutElement（不设也行）
                go.AddComponent<LayoutElement>();
            }

            // 把 OKL_Root 放在 PickAll 后面一位（保证整体顺序）
            if (placeAfter != null)
            {
                rt.SetSiblingIndex(placeAfter.GetSiblingIndex() + 1);
            }

            // 只要在使用前确保它是激活的
            if (!rt.gameObject.activeSelf)
            {
                rt.gameObject.SetActive(true);
            }

            return rt;
        }

        // 根据 ModConfig 重新渲染计算按钮
        private static void ReapplyFromConfig(LootView lv, RectTransform parent, Button pickAll)
        {
            if (!parent || !pickAll)
            {
                return;
            }

            Debug.Log($"[OneKeyLoot]: ReapplyFromConfig Start");

            var root = LookupRoot(lv, parent, pickAll);

            // —— 计算基准宽高 ——
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
            /*
            if (baseW <= 0)
            {
                baseW = 200f;
            }

            if (baseH <= 0)
            {
                baseH = 48f;
            }
            */

            // —— 找到/创建三组容器：Panel + Title + Row ——
            var qPanel = UIHelpers.FindOrCreateRect(root, UIConstants.QualityPanelName);
            UIHelpers.SetupPanel(qPanel, pickAll.image, baseH);
            var qTitle = UIHelpers.CreateOrUpdateTitleTMP(
                qPanel.gameObject,
                UIConstants.QualityTitleName,
                i18n.Quality.Title
            );
            UIHelpers.ApplyRefFont(qTitle, pickAll);

            var qRow = UIHelpers.FindOrCreateRect(
                root,
                UIConstants.QualityRowName,
                siblingAfter: qPanel.transform
            );
            UIHelpers.SetupRow(qRow, baseH);
            (
                qRow.GetComponent<LayoutElement>() ?? qRow.gameObject.AddComponent<LayoutElement>()
            ).preferredWidth = baseW;

            var vPanel = UIHelpers.FindOrCreateRect(root, UIConstants.ValuePanelName);
            UIHelpers.SetupPanel(vPanel, pickAll.image, baseH);
            var vTitle = UIHelpers.CreateOrUpdateTitleTMP(
                vPanel.gameObject,
                UIConstants.ValueTitleName,
                i18n.Value.Title
            );
            UIHelpers.ApplyRefFont(vTitle, pickAll);

            var vRow = UIHelpers.FindOrCreateRect(
                root,
                UIConstants.ValueRowName,
                siblingAfter: vPanel.transform
            );
            UIHelpers.SetupRow(vRow, baseH);
            (
                vRow.GetComponent<LayoutElement>() ?? vRow.gameObject.AddComponent<LayoutElement>()
            ).preferredWidth = baseW;

            var vwPanel = UIHelpers.FindOrCreateRect(root, UIConstants.ValueWeightPanelName);
            UIHelpers.SetupPanel(vwPanel, pickAll.image, baseH);
            var vwTitle = UIHelpers.CreateOrUpdateTitleTMP(
                vwPanel.gameObject,
                UIConstants.ValueWeightTitleName,
                i18n.ValueWeight.Title
            );
            UIHelpers.ApplyRefFont(vwTitle, pickAll);

            var vwRow = UIHelpers.FindOrCreateRect(
                root,
                UIConstants.ValueWeightRowName,
                siblingAfter: vwPanel.transform
            );
            UIHelpers.SetupRow(vwRow, baseH);
            (
                vwRow.GetComponent<LayoutElement>()
                ?? vwRow.gameObject.AddComponent<LayoutElement>()
            ).preferredWidth = baseW;

            // —— 解析当前配置 ——
            var cfg = ModConfig.Runtime;
            var defaultPalette = DefaultButtonColorPalette;
            var qualityList = ParseRangeCsv(cfg.qualityRange, ModConfigData.Defaults.qualityRange);
            var valueList = ParseRangeCsv(cfg.valueRange, ModConfigData.Defaults.valueRange);
            var valueWeightList = ParseRangeCsv(
                cfg.valueWeightRange,
                ModConfigData.Defaults.valueWeightRange
            );

            var qColors = ParseColorCsv(cfg.qualityColor, defaultPalette);
            var vColors = ParseColorCsv(cfg.valueColor, defaultPalette);
            var vwColors = ParseColorCsv(cfg.valueWeightColor, defaultPalette);

            // —— 清掉不需要的旧按钮，并补齐新按钮 ——
            PruneRowButtons(qRow, "OKL_Button_Quality_", [.. qualityList]);
            PruneRowButtons(vRow, "OKL_Button_Value_", [.. valueList]);
            PruneRowButtons(vwRow, "OKL_Button_ValueWeight_", [.. valueWeightList]);

            // —— 目标宽高与参考样式 ——
            float spacing = UIConstants.ButtonRowSpacing;
            float qTargetW = UIHelpers.CalcTargetWidth(baseW, spacing, qualityList.Count);
            float vTargetW = UIHelpers.CalcTargetWidth(baseW, spacing, valueList.Count);
            float vwTargetW = UIHelpers.CalcTargetWidth(baseW, spacing, valueWeightList.Count);
            float targetH = baseH;

            var refShadow = pickAll.GetComponent<Shadow>();
            var refOutline = pickAll.GetComponent<Outline>();

            // —— 质量按钮 ——
            for (int i = 0; i < qualityList.Count; i++)
            {
                int minQ = qualityList[i];
                CreateFilterButton(
                    qRow,
                    pickAll,
                    qTargetW,
                    targetH,
                    $"OKL_Button_Quality_{minQ}",
                    GetButtonColor(qColors, i),
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
            // —— 价值按钮 ——
            for (int i = 0; i < valueList.Count; i++)
            {
                int minV = valueList[i];
                CreateFilterButton(
                    vRow,
                    pickAll,
                    vTargetW,
                    targetH,
                    $"OKL_Button_Value_{minV}",
                    GetButtonColor(vColors, i),
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
            // —— 价重比按钮 ——
            for (int i = 0; i < valueWeightList.Count; i++)
            {
                int minW = valueWeightList[i];
                CreateFilterButton(
                    vwRow,
                    pickAll,
                    vwTargetW,
                    targetH,
                    $"OKL_Button_ValueWeight_{minW}",
                    GetButtonColor(vwColors, i),
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

            // —— 显隐统一在“配置变更”时套用 ——
            qPanel.gameObject.SetActive(cfg.showQuality);
            qRow.gameObject.SetActive(cfg.showQuality);
            vPanel.gameObject.SetActive(cfg.showValue);
            vRow.gameObject.SetActive(cfg.showValue);
            vwPanel.gameObject.SetActive(cfg.showValueWeight);
            vwRow.gameObject.SetActive(cfg.showValueWeight);

            if (root.Find("OKL_BottomSpacer") == null)
            {
                var sp = new GameObject("OKL_BottomSpacer", typeof(RectTransform));
                var spRt = sp.GetComponent<RectTransform>();
                spRt.SetParent(root, false);
                spRt.anchorMin = new Vector2(0f, 0.5f);
                spRt.anchorMax = new Vector2(1f, 0.5f);
                spRt.pivot = new Vector2(0.5f, 0.5f);
                spRt.sizeDelta = Vector2.zero;
                var le = sp.AddComponent<LayoutElement>();
                le.flexibleWidth = 1f;
                le.preferredHeight = UIConstants.BottomSpacerHeight;
                sp.SetActive(true);
            }
            Debug.Log($"[OneKeyLoot]: ReapplyFromConfig End");
        }

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
            item.SelfWeight == 0f ? int.MaxValue : (int)(ValueChecker(item) / item.SelfWeight);

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

        private static void TryPickAllBy(LootView lv, int min, Func<Item, int> itemChecker)
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
            PickAll(work);
        }

        private static void PickAll(List<Item> work)
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

        // 控制根节点显隐
        private static void toggleRootNode(RectTransform parent, bool show)
        {
            var root = s_cache.TryGetValue(parent, out var ce) ? ce?.Root : null;
            if (root)
            {
                root.gameObject.SetActive(show);
            }
        }

        // 根据parent创建并缓存根节点
        // 所有UI元素全部挂载在根节点下，方便一键控制
        private static RectTransform LookupRoot(LootView lv, RectTransform parent, Button pickAll)
        {
            if (s_cache.TryGetValue(parent, out var entry))
            {
                return entry.Root;
            }
            Debug.Log($"[OneKeyLoot]: Creating new root node for LootView");
            var root = CreateRootNode(parent, pickAll.transform);
            s_cache.Clear();
            s_cache[parent] = new CacheEntry
            {
                LvRef = new WeakReference<LootView>(lv),
                Parent = parent,
                Root = root,
                PickAll = pickAll,
            };
            return root;
        }

#pragma warning disable IDE0051
        /// <summary>
        /// 控制战利品界面所有按钮显隐
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("RefreshPickAllButton")]
        private static void Postfix_RefreshPickAllButton(object __instance)
        {
            var lv = (LootView)__instance;
            if (lv.TargetInventory == null)
            {
                return;
            }

            var targetLootBox = TargetLootBoxRef(lv);
            if (!targetLootBox)
            {
                return;
            }
            // Debug.Log($"[OneKeyLoot]: LootViewPatches TargetLootBox: {targetLootBox?.name}");
            var pickAll = PickAllRef(lv);
            if (!pickAll)
            {
                return;
            }
            if (targetLootBox.name == "PlayerStorage")
            {
                toggleRootNode(pickAll.transform.parent as RectTransform, false);
                return;
            }

            pickAll.gameObject.SetActive(ModConfig.Runtime.showCollectAll);
            toggleRootNode(pickAll.transform.parent as RectTransform, true);
        }

        /// <summary>
        /// 打开战利品界面后创建UI文本、按钮/更新缓存
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("OnOpen")]
        private static void Postfix_OnOpen(object __instance)
        {
            var lv = (LootView)__instance;
            if (lv.TargetInventory == null)
            {
                return;
            }
            var targetLootBox = TargetLootBoxRef(lv);
            if (!targetLootBox)
            {
                return;
            }
            // Debug.Log($"[OneKeyLoot]: OnOpen TargetLootBox: {targetLootBox?.name}");
            if (targetLootBox.name == "PlayerStorage")
            {
                return;
            }

            var pickAll = PickAllRef(lv);
            if (!pickAll)
            {
                return;
            }

            var parent = pickAll.transform.parent as RectTransform;
            // 更换场景时parent也会发生变化
            if (!parent || s_cache.ContainsKey(parent))
            {
                return;
            }

            ReapplyFromConfig(lv, parent, pickAll);

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
            LootView lv,
            Func<Item, int> checker,
            Func<int, string> labelBuilder,
            int fontSize,
            int siblingIndex
        )
        {
            var child = row.Find(name) as RectTransform;
            GameObject go;
            RectTransform r;
            bool isNew = !child;
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

            // 只在新建时拷贝一次样式，避免重复叠加导致变暗
            if (isNew)
            {
                var refHasStyle = refShadow != null || refOutline != null;
                if (refHasStyle)
                {
                    foreach (var s in go.GetComponents<Shadow>())
                    {
                        UnityEngine.Object.Destroy(s);
                    }

                    foreach (var o in go.GetComponents<Outline>())
                    {
                        UnityEngine.Object.Destroy(o);
                    }
                    UIHelpers.CopyShadowAndOutline(refShadow, refOutline, go);
                }
                else
                {
                    UIHelpers.EnsureDefaultShadow(go);
                }
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
