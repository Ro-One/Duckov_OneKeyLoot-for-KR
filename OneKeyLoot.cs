using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Duckov;
using Duckov.Modding;
using Duckov.Options;
using Duckov.UI;
using Duckov.Utilities;
using HarmonyLib;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OneKeyLoot
{
    public static class UIConstants
    {
        public static readonly Color Color2 = new Color(0.30f, 0.69f, 0.31f, 1f); // ≥2  #4CAF50 草绿
        public static readonly Color Color3 = new Color(0.26f, 0.65f, 0.96f, 1f); // ≥3  #42A5F5 天蓝
        public static readonly Color Color4 = new Color(0.73f, 0.41f, 0.78f, 1f); // ≥4  #BA68C8 柔紫
        public static readonly Color Color5 = new Color(0.75f, 0.50f, 0.20f, 1f); // ≥5  #C08032 暖橙
        public const string LabelName = "OKL_Label";
        public const float ButtonRowSpacing = 8f;
        public const float BottomSpacerHeight = 16f;

        public const string TitleName = "OKL_Title";

        public const string QualityRowName = "OKL_Row_Quality";
        public const string ValueRowName = "OKL_Row_Value";
        public const string QualityPanelName = "OKL_LabelPanel_Quality";
        public const string ValuePanelName = "OKL_LabelPanel_Value";
        public const string QualityTitleName = "OKL_Title_Quality";
        public const string ValueTitleName = "OKL_Title_Value";

        public const int TitleFontSize = 24;
        public static readonly Color TitleColor = new(0.90f, 0.90f, 0.90f, 1f);
    }

    [System.Serializable]
    public class DefaultConfig
    {
        public bool showCollectAll = true;
        public bool showQuality = true;
        public string qualityRange = "2,3,4,5";
        public bool showValue = true;
        public string valueRange = "100,500,1000";
        public string configToken = "OneKeyLoot_config_v1";

        public static readonly DefaultConfig Defaults = new();
    }

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public readonly string Mod_DisplayName = "一键拾取战利品 One-Key Loot";
        public readonly string Mod_Name = "DreamNya.OneKeyLoot";

        readonly DefaultConfig config = new();

        private static DefaultConfig s_RuntimeConfig = new();
        internal static DefaultConfig RuntimeConfig => s_RuntimeConfig ?? new DefaultConfig();

        private static string PersistentConfigPath =>
            Path.Combine(Application.streamingAssetsPath, "OneKeyLootConfig.txt");

        private Harmony harmony;

        private void Awake()
        {
            Debug.Log($"[OneKeyLoot]: {Mod_DisplayName} Awake");
        }

        private void OnEnable()
        {
            Debug.Log($"[OneKeyLoot]: {Mod_DisplayName} OnEnable");
            ModManager.OnModActivated += OnModActivated;
            i18n.Init();
            if (ModConfigAPI.IsAvailable())
            {
                Debug.Log("[OneKeyLoot]: ModConfig already available!");
                SetupModConfig();
                LoadConfigFromModConfig();
                UpdateRuntimeConfigSnapshot(config);
            }
            harmony = new Harmony(Mod_Name);
            harmony.PatchAll();
        }

        private void OnModActivated(ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            if (info.name == ModConfigAPI.ModConfigName)
            {
                Debug.Log("[OneKeyLoot]: ModConfig activated!");
                SetupModConfig();
                LoadConfigFromModConfig();
                UpdateRuntimeConfigSnapshot(config);
            }
        }

        private void OnDisable()
        {
            Debug.Log($"[OneKeyLoot]: {Mod_DisplayName} OnDisable");
            ModManager.OnModActivated -= OnModActivated;
            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(OnModConfigOptionsChanged);
            harmony?.UnpatchAll(Mod_Name);
            i18n.Dispose();
        }

        private static void UpdateRuntimeConfigSnapshot(DefaultConfig src)
        {
            if (s_RuntimeConfig == null)
            {
                s_RuntimeConfig = new DefaultConfig();
            }
            s_RuntimeConfig.showCollectAll = src.showCollectAll;
            s_RuntimeConfig.showQuality = src.showQuality;
            s_RuntimeConfig.qualityRange = src.qualityRange;
            s_RuntimeConfig.showValue = src.showValue;
            s_RuntimeConfig.valueRange = src.valueRange;
            s_RuntimeConfig.configToken = src.configToken;
        }

        private void SaveConfig(DefaultConfig cfg)
        {
            try
            {
                string json = JsonUtility.ToJson(cfg, true);
                File.WriteAllText(PersistentConfigPath, json);
                Debug.Log("OneKeyLoot: Config saved");
            }
            catch (Exception e)
            {
                Debug.LogError($"OneKeyLoot: Failed to save config: {e}");
            }
        }

        private void SetupModConfig()
        {
            if (!ModConfigAPI.IsAvailable())
            {
                Debug.LogWarning("OneKeyLoot: ModConfig not available");
                return;
            }

            Debug.Log("准备添加ModConfig配置项");
            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnModConfigOptionsChanged);

            ModConfigAPI.SafeAddInputWithSlider(
                Mod_DisplayName,
                "valueRange",
                i18n.Config.ValueRangeLabel,
                typeof(string),
                config.valueRange,
                null
            );
            ModConfigAPI.SafeAddBoolDropdownList(
                Mod_DisplayName,
                "showValue",
                i18n.Config.ShowValueLabel,
                config.showValue
            );
            ModConfigAPI.SafeAddInputWithSlider(
                Mod_DisplayName,
                "qualityRange",
                i18n.Config.QualityRangeLabel,
                typeof(string),
                config.qualityRange,
                null
            );
            ModConfigAPI.SafeAddBoolDropdownList(
                Mod_DisplayName,
                "showQuality",
                i18n.Config.ShowQualityLabel,
                config.showQuality
            );
            ModConfigAPI.SafeAddBoolDropdownList(
                Mod_DisplayName,
                "showCollectAll",
                i18n.Config.ShowCollectAllLabel,
                config.showCollectAll
            );

            Debug.Log("[OneKeyLoot]: ModConfig setup completed");
        }

        private void OnModConfigOptionsChanged(string key)
        {
            if (!key.StartsWith(Mod_DisplayName + "_"))
            {
                return;
            }

            LoadConfigFromModConfig();
            SaveConfig(config);
            UpdateRuntimeConfigSnapshot(config);
            i18n.RelabelActiveLootViews();

            Debug.Log($"[OneKeyLoot]: ModConfig updated - {key}");
        }

        private void LoadConfigFromModConfig()
        {
            config.showCollectAll = ModConfigAPI.SafeLoad<bool>(
                Mod_DisplayName,
                "showCollectAll",
                config.showCollectAll
            );
            config.showQuality = ModConfigAPI.SafeLoad<bool>(
                Mod_DisplayName,
                "showQuality",
                config.showQuality
            );
            config.qualityRange = ModConfigAPI.SafeLoad<string>(
                Mod_DisplayName,
                "qualityRange",
                config.qualityRange
            );
            config.showValue = ModConfigAPI.SafeLoad<bool>(
                Mod_DisplayName,
                "showValue",
                config.showValue
            );
            config.valueRange = ModConfigAPI.SafeLoad<string>(
                Mod_DisplayName,
                "valueRange",
                config.valueRange
            );
        }

        [HarmonyPatch(typeof(Duckov.UI.LootView))]
        internal static class LootViewPatches
        {
            private static readonly AccessTools.FieldRef<Duckov.UI.LootView, Button> PickAllRef =
                AccessTools.FieldRefAccess<Duckov.UI.LootView, Button>("pickAllButton");

            private static readonly AccessTools.FieldRef<
                Duckov.UI.LootView,
                InteractableLootbox
            > TargetLootBoxRef = AccessTools.FieldRefAccess<
                Duckov.UI.LootView,
                InteractableLootbox
            >("targetLootBox");

            private static readonly Color[] ButtonColorPalette =
            [
                UIConstants.Color2,
                UIConstants.Color3,
                UIConstants.Color4,
                UIConstants.Color5,
            ];

            private static Color GetButtonColor(int ordinal) =>
                ButtonColorPalette[ordinal % ButtonColorPalette.Length];

            private static readonly Func<Item, int> s_MetricValue = static item =>
                item.GetTotalRawValue() / 2;
            private static readonly Func<Item, int> s_MetricQuality = static item => item.Quality;

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

            private static void PruneRowButtons(
                RectTransform row,
                string prefix,
                HashSet<int> keepSet
            )
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

            private static float CalcTargetWidth(float totalWidth, float spacing, int count)
            {
                count = Mathf.Max(1, count);
                return Mathf.Max(1f, (totalWidth - spacing * (count - 1)) / count);
            }

            private static void TryPickAllBy(
                Duckov.UI.LootView lv,
                int min,
                Func<Item, int> metricSelector
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
                    if (it == null)
                    {
                        continue;
                    }
                    if (invOwnerItem != null && ReferenceEquals(it, invOwnerItem))
                    {
                        continue;
                    }
                    if (needInspect && !it.Inspected)
                    {
                        continue;
                    }

                    int metric = metricSelector(it);
                    if (!ItemWishlist.Instance.IsManuallyWishlisted(it.TypeID) && metric < min)
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

            [HarmonyPostfix]
            [HarmonyPatch("RefreshPickAllButton")]
            private static void Postfix_RefreshPickAllButton(object __instance)
            {
                if (!RuntimeConfig.showCollectAll)
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
                float baseW = (
                    lePick.preferredWidth > 0
                        ? lePick.preferredWidth
                        : pickAll.GetComponent<RectTransform>().rect.width
                );
                float baseH = (
                    lePick.preferredHeight > 0
                        ? lePick.preferredHeight
                        : pickAll.GetComponent<RectTransform>().rect.height
                );
                if (baseW <= 0)
                {
                    baseW = 200f;
                }
                if (baseH <= 0)
                {
                    baseH = 48f;
                }

                // Quality：Panel + Title + Row
                var qRow = FindOrCreateRect(
                    parent,
                    UIConstants.QualityRowName,
                    siblingAfter: pickAll.transform
                );
                SetupRow(qRow, baseH);
                // 行宽固定为与 pickAll 一致，避免被父容器拉满
                var qRowLe =
                    qRow.GetComponent<LayoutElement>()
                    ?? qRow.gameObject.AddComponent<LayoutElement>();
                qRowLe.flexibleWidth = 0f;
                qRowLe.preferredWidth = baseW;

                var qPanel = FindOrCreateRect(
                    parent,
                    UIConstants.QualityPanelName,
                    siblingIndex: qRow.GetSiblingIndex()
                );
                qRow.SetSiblingIndex(qPanel.GetSiblingIndex() + 1);
                SetupPanel(qPanel, pickAll.image, baseH); // 复用 pickAll 的 9-slice 样式与宽度
                var qTitle = CreateOrUpdateTitleTMP(
                    qPanel.gameObject,
                    UIConstants.QualityTitleName,
                    i18n.Quality.Title
                );
                ApplyRefFont(qTitle, pickAll);

                // Value：Panel + Title + Row
                var vRow = FindOrCreateRect(
                    parent,
                    UIConstants.ValueRowName,
                    siblingAfter: qRow.transform
                );
                SetupRow(vRow, baseH);
                var vRowLe =
                    vRow.GetComponent<LayoutElement>()
                    ?? vRow.gameObject.AddComponent<LayoutElement>();
                vRowLe.flexibleWidth = 0f;
                vRowLe.preferredWidth = baseW;

                var vPanel = FindOrCreateRect(
                    parent,
                    UIConstants.ValuePanelName,
                    siblingIndex: vRow.GetSiblingIndex()
                );
                vRow.SetSiblingIndex(vPanel.GetSiblingIndex() + 1);
                SetupPanel(vPanel, pickAll.image, baseH);
                var vTitle = CreateOrUpdateTitleTMP(
                    vPanel.gameObject,
                    UIConstants.ValueTitleName,
                    i18n.Value.Title
                );
                ApplyRefFont(vTitle, pickAll);

                // 配置解析
                var cfg = RuntimeConfig;
                var qualityList = ParseRangeCsv(
                    cfg.qualityRange,
                    DefaultConfig.Defaults.qualityRange
                );
                var valueList = ParseRangeCsv(cfg.valueRange, DefaultConfig.Defaults.valueRange);

                PruneRowButtons(qRow, "OKL_Button_Quality_", [.. qualityList]);
                PruneRowButtons(vRow, "OKL_Button_Value_", [.. valueList]);

                float spacing = UIConstants.ButtonRowSpacing;
                float qTargetW = CalcTargetWidth(baseW, spacing, qualityList.Count);
                float vTargetW = CalcTargetWidth(baseW, spacing, valueList.Count);
                float targetH = baseH;

                var refShadow = pickAll.GetComponent<Shadow>();
                var refOutline = pickAll.GetComponent<Outline>();

                for (int i = 0; i < qualityList.Count; i++)
                {
                    int minQ = qualityList[i];
                    string name = $"OKL_Button_Quality_{minQ}";
                    var color = GetButtonColor(i);
                    CreateQualityButton(
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
                        qRow
                    );
                }

                for (int i = 0; i < valueList.Count; i++)
                {
                    int minV = valueList[i];
                    string name = $"OKL_Button_Value_{minV}";
                    var color = GetButtonColor(i);
                    CreateValueButton(
                        vRow,
                        pickAll,
                        vTargetW,
                        targetH,
                        name,
                        color,
                        minV,
                        refShadow,
                        refOutline,
                        lv
                    );
                }

                qPanel.gameObject.SetActive(cfg.showQuality);
                qRow.gameObject.SetActive(cfg.showQuality);
                vPanel.gameObject.SetActive(cfg.showValue);
                vRow.gameObject.SetActive(cfg.showValue);

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

            // 行容器：水平布局
            private static void SetupRow(RectTransform row, float baseH)
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
                    row.GetComponent<LayoutElement>()
                    ?? row.gameObject.AddComponent<LayoutElement>();
                le.flexibleWidth = 0f; // 行不再被父容器拉满
                le.preferredHeight = baseH;
            }

            // 标题面板：对齐与背景（复用 pickAll 的样式）
            private static void SetupPanel(RectTransform panel, Image refImage, float baseH)
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
            private static RectTransform CreateOrUpdateTitleTMP(
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
                    rt.GetComponent<TextMeshProUGUI>()
                    ?? rt.gameObject.AddComponent<TextMeshProUGUI>();
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
                var sh =
                    tmp.gameObject.GetComponent<Shadow>() ?? tmp.gameObject.AddComponent<Shadow>();
                sh.effectColor = new Color(0f, 0f, 0f, 0.5f);
                sh.effectDistance = new Vector2(2f, -2f);
                return rt;
            }

            private static RectTransform FindOrCreateRect(
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

            private static void CopyShadowAndOutline(
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

            private static void EnsureDefaultShadow(GameObject go)
            {
                var s = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>();
                s.effectColor = new Color(0f, 0f, 0f, 0.35f);
                s.effectDistance = new Vector2(1.2f, -1.2f);
            }

            private static void ApplyRefFont(RectTransform rt, Button refPickAll)
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

            private static RectTransform EnsureSingleCenteredLabel(
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
                        UnityEngine.Object.Destroy(t.gameObject);
                    }
                }
                var uguiTexts = host.GetComponentsInChildren<Text>(true);
                foreach (var tt in uguiTexts)
                {
                    if (tt && tt.gameObject.name != UIConstants.LabelName)
                    {
                        UnityEngine.Object.Destroy(tt.gameObject);
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

            private static RectTransform EnsureSingleCenteredLabel(
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

            private static void CreateQualityButton(
                RectTransform row,
                Button pickAll,
                float w,
                float h,
                string name,
                Color bgcolor,
                int minQuality,
                Shadow refShadow,
                Outline refOutline,
                Duckov.UI.LootView lv,
                RectTransform rowForContext
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
                    btn.onClick.AddListener(() => TryPickAllBy(lv, minQuality, s_MetricQuality));
                }
                if (bg)
                {
                    bg.color = bgcolor;
                }

                if (refShadow != null || refOutline != null)
                {
                    CopyShadowAndOutline(refShadow, refOutline, go);
                }
                else
                {
                    EnsureDefaultShadow(go);
                }

                EnsureSingleCenteredLabel(
                    go,
                    i18n.Quality.Button(minQuality),
                    pickAll,
                    i18n.Quality.ButtonFontSize
                );

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

            private static void CreateValueButton(
                RectTransform row,
                Button pickAll,
                float w,
                float h,
                string name,
                Color bgcolor,
                int minValue,
                Shadow refShadow,
                Outline refOutline,
                Duckov.UI.LootView lv
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
                    btn.onClick.AddListener(() => TryPickAllBy(lv, minValue, s_MetricValue));
                }
                if (bg)
                {
                    bg.color = bgcolor;
                }

                if (refShadow != null || refOutline != null)
                {
                    CopyShadowAndOutline(refShadow, refOutline, go);
                }
                else
                {
                    EnsureDefaultShadow(go);
                }

                EnsureSingleCenteredLabel(
                    go,
                    i18n.Value.Button(minValue),
                    pickAll,
                    i18n.Value.ButtonFontSize
                );

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
}
