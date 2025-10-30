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
        private static Color Hex(string str)
        {
            if (ColorUtility.TryParseHtmlString(str, out var color))
            {
                return color;
            }
            return new Color(0.30f, 0.69f, 0.31f, 1f);
        }

        public static readonly Color Color0 = Hex("#FFFFFF"); // ≥0 하양
        public static readonly Color Color1 = Hex("#7cff7c"); // ≥1 연두
        public static readonly Color Color2 = Hex("#7cd5ff"); // ≥2 하늘
        public static readonly Color Color3 = Hex("#d0acff"); // ≥3 연보라
        public static readonly Color Color4 = Hex("#ffdc24"); // ≥4 노랑
        public static readonly Color Color5 = Hex("#ff5858"); // ≥5 주황
        public static readonly Color Color6 = Hex("#bb0000"); // ≥6 빨강
        public const string LabelName = "OKL_Label";
        public const float ButtonRowSpacing = 8f;
        public const float BottomSpacerHeight = 16f;

        public const string TitleName = "OKL_Title";

        public const string QualityRowName = "OKL_Row_Quality";
        public const string QualityPanelName = "OKL_LabelPanel_Quality";
        public const string QualityTitleName = "OKL_Title_Quality";
        public const string ValueRowName = "OKL_Row_Value";
        public const string ValuePanelName = "OKL_LabelPanel_Value";
        public const string ValueTitleName = "OKL_Title_Value";
        public const string ValueWeightRowName = "OKL_Row_ValueWeight";
        public const string ValueWeightPanelName = "OKL_LabelPanel_ValueWeight";
        public const string ValueWeightTitleName = "OKL_Title_ValueWeight";

        public const int TitleFontSize = 24;
        public static readonly Color TitleColor = new(0.90f, 0.90f, 0.90f, 1f);
    }

    [Serializable]
    public class DefaultConfig
    {
        public bool showCollectAll = true;
        public bool showQuality = true;
        public bool showValue = true;
        public bool showValueWeight = true;
        public bool autoChangeQualityColor = false;
        public string qualityRange = "2,3,4,5";
        public string valueRange = "100,500,1000";
        public string valueWeightRange = "500,2500,5000";
        public string qualityColor = "#4CAF4F,#42A5F5,#BA68C6,#BF7F33";
        public string valueColor = "#4CAF4F,#42A5F5,#BA68C6,#BF7F33";
        public string valueWeightColor = "#4CAF4F,#42A5F5,#BA68C6,#BF7F33";
        public string configToken = "OneKeyLoot_config_v2";

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
            s_RuntimeConfig ??= new DefaultConfig();
            s_RuntimeConfig.showCollectAll = src.showCollectAll;
            s_RuntimeConfig.showQuality = src.showQuality;
            s_RuntimeConfig.showValue = src.showValue;
            s_RuntimeConfig.showValueWeight = src.showValueWeight;
            s_RuntimeConfig.autoChangeQualityColor = src.autoChangeQualityColor;
            s_RuntimeConfig.qualityRange = src.qualityRange;
            s_RuntimeConfig.valueRange = src.valueRange;
            s_RuntimeConfig.valueWeightRange = src.valueWeightRange;
            s_RuntimeConfig.qualityColor = src.qualityColor;
            s_RuntimeConfig.valueColor = src.valueColor;
            s_RuntimeConfig.valueWeightColor = src.valueWeightColor;

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

            // 倒序添加以保证显示顺序
            // 역순으로 추가하여 표시 순서 보장
            ModConfigAPI.SafeAddInputWithSlider(
                Mod_DisplayName,
                "valueWeightColor",
                i18n.Config.ValueWeightColorLabel,
                typeof(string),
                config.valueWeightColor,
                null
            );
            ModConfigAPI.SafeAddInputWithSlider(
                Mod_DisplayName,
                "valueWeightRange",
                i18n.Config.ValueWeightRangeLabel,
                typeof(string),
                config.valueWeightRange,
                null
            );
            ModConfigAPI.SafeAddBoolDropdownList(
                Mod_DisplayName,
                "showValueWeight",
                i18n.Config.ShowValueWeightLabel,
                config.showValueWeight
            );
            ModConfigAPI.SafeAddInputWithSlider(
                Mod_DisplayName,
                "valueColor",
                i18n.Config.ValueColorLabel,
                typeof(string),
                config.valueColor,
                null
            );
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
                "qualityColor",
                i18n.Config.QualityColorLabel,
                typeof(string),
                config.qualityColor,
                null
            );
            ModConfigAPI.SafeAddBoolDropdownList(
                Mod_DisplayName,
                "AutoChangeQualityColorLabel",
                i18n.Config.AutoChangeQualityColorLabel,
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
            config.showValueWeight = ModConfigAPI.SafeLoad<bool>(
                Mod_DisplayName,
                "showValueWeight",
                config.showValueWeight
            );
            config.valueWeightRange = ModConfigAPI.SafeLoad<string>(
                Mod_DisplayName,
                "valueWeightRange",
                config.valueWeightRange
            );
            config.autoChangeQualityColor = ModConfigAPI.SafeLoad<bool>(
                Mod_DisplayName,
                "autoChangeQualityColor",
                config.autoChangeQualityColor
            );
            config.qualityColor = ModConfigAPI.SafeLoad<string>(
                Mod_DisplayName,
                "qualityColor",
                config.qualityColor
            );
            config.valueColor = ModConfigAPI.SafeLoad<string>(
                Mod_DisplayName,
                "valueColor",
                config.valueColor
            );
            config.valueWeightColor = ModConfigAPI.SafeLoad<string>(
                Mod_DisplayName,
                "valueWeightColor",
                config.valueWeightColor
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

            private static readonly Color[] DefaultButtonColorPalette =
            [
                UIConstants.Color1,
                UIConstants.Color2,
                UIConstants.Color3,
                UIConstants.Color4,
            ];
            private static readonly Color[] AutoQualityColorPalette =
            [
                UIConstants.Color0,
                UIConstants.Color1,
                UIConstants.Color2,
                UIConstants.Color3,
                UIConstants.Color4,
                UIConstants.Color5,
                UIConstants.Color6,
            ];

            // ✅ 解析颜色 CSV：若任一 token 非法或最终为空 => 回退到默认调色板（最多取 4 个）
            // ✅ 색상 CSV 구문 분석: 토큰이 잘못되었거나 최종적으로 비어 있는 경우 => 기본 팔레트로 돌아가기(최대 4개 가져오기)
            private static List<Color> ParseColorCsv(string scv, IReadOnlyList<Color> fallback)
            {
                const int MaxButtons = 4;
                try
                {
                    if (string.IsNullOrWhiteSpace(scv))
                    {
                        return [.. fallback.Take(MaxButtons)];
                    }

                    var tokens = scv.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries);
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

            private static List<Color> ParseQualityColorCsvByRange(string range, IReadOnlyList<Color> fallback)
            {
                const int MaxButtons = 4;
                try
                {
                    if (string.IsNullOrWhiteSpace(range))
                    {
                        return [.. fallback.Take(MaxButtons)];
                        Debug.Log("[OneKeyLoot]: quality color range is empty, using fallback");
                    }

                    var tokens = range.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries);
                    var list = new List<Color>(tokens.Length);
                    bool invalid = false;
                    foreach (var raw in tokens)
                    {
                        if (int.TryParse(raw.Trim(), out var v))
                        {
                            list.Add(AutoQualityColorPalette[v]);
                            Debug.Log($"[OneKeyLoot]: quality color for range {v} parsed");
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
                        Debug.Log("[OneKeyLoot]: quality color range invalid, using fallback");
                    }
                    return [.. list.Take(MaxButtons)];
                    Debug.Log("[OneKeyLoot]: quality color range parsed successfully");
                }
                catch
                {
                    return [.. fallback.Take(MaxButtons)];
                    Debug.Log("[OneKeyLoot]: quality color range parse exception, using fallback");
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
            // CSV 구문 분석: 실패 시 기본 구성으로 돌아가기
            private static List<int> ParseRangeCsv(string csv, string fallbackCsv)
            {
                // 最多显示4个按钮
                // 최대 4개의 버튼 표시
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
                    // 이름에서 숫자 접미사 구문 분석; 구문 분석 실패도 정리해야 하는 이전 노드로 간주
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

                    if (
                        !ItemWishlist.Instance.IsManuallyWishlisted(it.TypeID)
                        && itemChecker(it) < min
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
                // pickAll 기준 너비와 높이를 기반으로 컨트롤 유추(레이아웃이 새로 고쳐지지 않은 경우 선호 값 사용)
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
                // 희귀도: Panel + Title + Row
                var qRow = FindOrCreateRect(
                    parent,
                    UIConstants.QualityRowName,
                    siblingAfter: pickAll.transform
                );
                SetupRow(qRow, baseH);
                // 行宽固定为与 pickAll 一致，避免被父容器拉满
                // 행 너비는 pickAll과 동일하게 고정되어 부모 컨테이너에 의해 채워지는 것을 방지
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
                SetupPanel(qPanel, pickAll.image, baseH); // 复用 pickAll 的 9-slice 样式与宽度 // pickAll의 9-slice 스타일과 너비 재사용
                var qTitle = CreateOrUpdateTitleTMP(
                    qPanel.gameObject,
                    UIConstants.QualityTitleName,
                    i18n.Quality.Title
                );
                ApplyRefFont(qTitle, pickAll);

                // Value：Panel + Title + Row
                // 가치: Panel + Title + Row
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

                // ValueWeight：Panel + Title + Row
                // 무게 대비 가치: Panel + Title + Row
                var vwRow = FindOrCreateRect(
                    parent,
                    UIConstants.ValueWeightRowName,
                    siblingAfter: vRow.transform
                );
                SetupRow(vwRow, baseH);
                var vwRowLe =
                    vwRow.GetComponent<LayoutElement>()
                    ?? vwRow.gameObject.AddComponent<LayoutElement>();
                vwRowLe.flexibleWidth = 0f;
                vwRowLe.preferredWidth = baseW;

                var vwPanel = FindOrCreateRect(
                    parent,
                    UIConstants.ValueWeightPanelName,
                    siblingIndex: vwRow.GetSiblingIndex()
                );
                vwRow.SetSiblingIndex(vwPanel.GetSiblingIndex() + 1);
                SetupPanel(vwPanel, pickAll.image, baseH);
                var vwTitle = CreateOrUpdateTitleTMP(
                    vwPanel.gameObject,
                    UIConstants.ValueWeightTitleName,
                    i18n.ValueWeight.Title
                );
                ApplyRefFont(vwTitle, pickAll);

                var defaultPalette = DefaultButtonColorPalette;
                var cfg = RuntimeConfig; // 解析配置范围 // 구성 범위 구문 분석
                var qualityList = ParseRangeCsv(
                    cfg.qualityRange,
                    DefaultConfig.Defaults.qualityRange
                );
                var valueList = ParseRangeCsv(
                    cfg.valueRange,
                    DefaultConfig.Defaults.valueRange
                );
                var weightList = ParseRangeCsv(
                    cfg.valueWeightRange,
                    DefaultConfig.Defaults.valueWeightRange
                );
                var qColors = new List<Color>();
                if (cfg.autoChangeQualityColor)
                {
                    qColors = ParseQualityColorCsvByRange(
                        cfg.qualityRange,
                        defaultPalette
                    );
                }
                else
                {
                    qColors = ParseColorCsv(cfg.qualityColor, defaultPalette);
                }
                List<Color> vColors = ParseColorCsv(cfg.valueColor, defaultPalette);
                List<Color> vwColors = ParseColorCsv(cfg.valueWeightColor, defaultPalette);

                // 清理旧按钮
                // 이전 버튼 정리
                PruneRowButtons(qRow, "OKL_Button_Quality_", [.. qualityList]);
                PruneRowButtons(vRow, "OKL_Button_Value_", [.. valueList]);
                PruneRowButtons(vwRow, "OKL_Button_ValueWeight_", [.. weightList]);

                // 目标宽度
                // 목표 너비
                float spacing = UIConstants.ButtonRowSpacing;
                float qTargetW = CalcTargetWidth(baseW, spacing, qualityList.Count);
                float vTargetW = CalcTargetWidth(baseW, spacing, valueList.Count);
                float vwTargetW = CalcTargetWidth(baseW, spacing, weightList.Count);
                float targetH = baseH;

                var refShadow = pickAll.GetComponent<Shadow>();
                var refOutline = pickAll.GetComponent<Outline>();

                // 质量按钮
                // 희귀도 버튼
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
                // 가치 버튼
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

                // 价值权重按钮（价值/重量）
                //무게 대비 가치 버튼(가치/무게)
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
                // 가시성 제어
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

            // 行容器：水平布局
            // 행 컨테이너: 수평 레이아웃
            private static void SetupRow(RectTransform row, float baseH)
            {
                var layout =
                    row.GetComponent<HorizontalLayoutGroup>()
                    ?? row.gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.childControlWidth = true; // 让行宽由子按钮的 preferredWidth 控制 // 자식 버튼의 preferredWidth에 의해 행 너비 제어
                layout.childControlHeight = false;
                layout.spacing = UIConstants.ButtonRowSpacing;
                layout.padding = new RectOffset(0, 0, 0, 0);

                var le =
                    row.GetComponent<LayoutElement>()
                    ?? row.gameObject.AddComponent<LayoutElement>();
                le.flexibleWidth = 0f; // 行不再被父容器拉满 // 행이 더 이상 부모 컨테이너에 의해 채워지지 않음
                le.preferredHeight = baseH;
            }

            // 标题面板：对齐与背景（复用 pickAll 的样式）
            // 제목 패널: 정렬 및 배경(pickAll 스타일 재사용)
            private static void SetupPanel(RectTransform panel, Image refImage, float baseH)
            {
                if (!panel)
                {
                    return;
                }

                // 没有 LayoutElement 则补充，避免 NRE
                // LayoutElement가 없으면 NRE를 방지하기 위해 보충
                var le =
                    panel.GetComponent<LayoutElement>()
                    ?? panel.gameObject.AddComponent<LayoutElement>();

                // 与“全部拾取”同宽
                // "일괄 수집"과 동일한 너비
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
                // Image가 없으면 보충하고 pickAll의 9-slice 재사용
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
            // 제목 TMP 생성/업데이트
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
                // 가독성 향상을 위한 다크 섀도우
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

            // 新增通用按钮创建：质量/价值/价值权重 共用
            // 새로운 일반 버튼 생성: 희귀도/가치/무게 대비 가치 공용
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
                // 버튼의 형제 순서가 정렬된 목록 인덱스와 일치하는지 확인
                r.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, maxIdx));

                var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
                le.minWidth = w;
                le.preferredWidth = w;
                le.flexibleWidth = 0f;
                le.minHeight = h;
                le.preferredHeight = h;
                le.flexibleHeight = 0f;

                // 同步到 RectTransform，确保与 Layout 一致
                // RectTransform과 동기화하여 레이아웃과 일치
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

                if (refShadow != null || refOutline != null)
                {
                    CopyShadowAndOutline(refShadow, refOutline, go);
                }
                else
                {
                    EnsureDefaultShadow(go);
                }

                EnsureSingleCenteredLabel(go, labelBuilder(minThreshold), pickAll, fontSize);

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
