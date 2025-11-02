using System;
using System.Collections.Generic;
using Duckov.UI;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

namespace OneKeyLoot
{
    internal static class i18n
    {
        // ===== 组标识 =====
        // ===== 그룹 식별자 =====
        public enum LootGroupId
        {
            Config,
            Quality,
            Value,
            ValueWeight,
        }

        // ===== 文案 Key =====
        // ===== 텍스트 Key 정의 =====
        public static class Keys
        {
            public static class Config
            {
                public const string ShowCollectAll = "OKL.Config_ShowCollectAll";
                public const string ShowQuality = "OKL.Config_ShowQuality";
                public const string ShowValue = "OKL.Config_ShowValue";
                public const string ShowValueWeight = "OKL.Config_ShowValueWeight";
                public const string QualityRange = "OKL.Config_QualityRange";
                public const string ValueRange = "OKL.Config_ValueRange";
                public const string ValueWeightRange = "OKL.Config_ValueWeightRange";
                public const string UseAutoValueColor = "OKL.Config_UseAutoValueColor";
                public const string QualityColor = "OKL.Config_QualityColor";
                public const string ValueColor = "OKL.Config_ValueColor";
                public const string ValueWeightColor = "OKL.Config_ValueWeightColor";
            }

            public static class Quality
            {
                public const string Title = "OKL.Quality_Title";
                public const string ButtonText = "OKL.Quality_ButtonText"; // 需要占位符 ≥{0}
                public const string ButtonFontSize = "OKL.Quality_ButtonFontSize"; // 元数据：字号（数字）
            }

            public static class Value
            {
                public const string Title = "OKL.Value_Title";
                public const string ButtonText = "OKL.Value_ButtonText"; // 需要占位符 ≥{0}
                public const string ButtonFontSize = "OKL.Value_ButtonFontSize"; // 元数据：字号（数字）
            }

            public static class ValueWeight
            {
                public const string Title = "OKL.ValueWeight_Title";
                public const string ButtonText = "OKL.ValueWeight_ButtonText"; // 需要占位符 ≥{0}
                public const string ButtonFontSize = "OKL.ValueWeight_ButtonFontSize"; // 元数据：字号（数字）
            }
        }

        // ===== 字体与排版（简单对齐）=====
        // ===== 글꼴 레이아웃 (간단 정렬) =====
        public static class Typography
        {
            public static readonly TextAlignmentOptions TitleAlign = TextAlignmentOptions.Midline;
            public static readonly TextAlignmentOptions ButtonAlign = TextAlignmentOptions.Midline;
        }

        // ===== 语言包：文本 + 元数据 =====
        // ===== 언어팩 : 텍스트 + 메타데이터 =====
        private sealed class LanguagePack
        {
            public readonly Dictionary<string, string> Texts = new(StringComparer.Ordinal);
            public readonly Dictionary<string, object> Meta = new(StringComparer.Ordinal);

            public LanguagePack AddText(string key, string value)
            {
                Texts[key] = value;
                return this;
            }

            public LanguagePack AddMeta(string key, object value)
            {
                Meta[key] = value;
                return this;
            }
        }

        // ===== 内置语言包 =====
        // ===== 내장언어팩 (필요시 확장 가능) =====
        private static readonly LanguagePack PackEN = new LanguagePack()
            // Config
            .AddText(Keys.Config.ShowCollectAll, "Display [Collect All]")
            .AddText(Keys.Config.ShowQuality, "Display [One-Key Loot (by Quality)]")
            .AddText(Keys.Config.ShowValue, "Display [One-Key Loot (by Value)]")
            .AddText(Keys.Config.ShowValueWeight, "Display [One-Key Loot (by Value per unit Weight)]")
            .AddText(Keys.Config.QualityRange, "Quality Range (1~9)")
            .AddText(Keys.Config.ValueRange, "Value Range (1+)")
            .AddText(Keys.Config.ValueWeightRange, "Value/Weight Range (1+)")
            .AddText(Keys.Config.QualityColor, "Quality ButtonGroup Color")
            .AddText(Keys.Config.UseAutoValueColor, "Use Automatically Filling Value Button Color")
            .AddText(Keys.Config.ValueColor, "Value ButtonGroup Color")
            .AddText(Keys.Config.ValueWeightColor, "Value/Weight ButtonGroup Color")
            // Quality
            .AddText(Keys.Quality.Title, "One-Key Loot (by Quality)")
            .AddText(Keys.Quality.ButtonText, "Quality ≥ {0}")
            .AddMeta(Keys.Quality.ButtonFontSize, 20)
            // Value
            .AddText(Keys.Value.Title, "One-Key Loot (by Value)")
            .AddText(Keys.Value.ButtonText, "Value ≥ {0}")
            .AddMeta(Keys.Value.ButtonFontSize, 20)
            // Value/Weight
            .AddText(Keys.ValueWeight.Title, "One-Key Loot (by Value per unit Weight)")
            .AddText(Keys.ValueWeight.ButtonText, "$/kg ≥ {0}")
            .AddMeta(Keys.ValueWeight.ButtonFontSize, 22);

        private static readonly LanguagePack PackZH = new LanguagePack()
            // Config
            .AddText(Keys.Config.ShowCollectAll, "显示【全部拾取】")
            .AddText(Keys.Config.ShowQuality, "显示【一键收集战利品（按品质）】")
            .AddText(Keys.Config.QualityRange, "品质范围（1~9）")
            .AddText(Keys.Config.ShowValue, "显示【一键收集战利品（按价值）】")
            .AddText(Keys.Config.ValueRange, "价值范围（1+）")
            .AddText(Keys.Config.ShowValueWeight, "显示【一键收集战利品（按价重比）】")
            .AddText(Keys.Config.ValueWeightRange, "价重比范围 (1+)")
            .AddText(Keys.Config.UseAutoValueColor, "自动填充价值按钮颜色")
            .AddText(Keys.Config.QualityColor, "品质按钮组颜色")
            .AddText(Keys.Config.ValueColor, "价值按钮组颜色")
            .AddText(Keys.Config.ValueWeightColor, "价重比按钮组颜色")
            // Quality
            .AddText(Keys.Quality.Title, "一键收集战利品（按品质）")
            .AddText(Keys.Quality.ButtonText, "品质≥{0}")
            .AddMeta(Keys.Quality.ButtonFontSize, 24)
            // Value
            .AddText(Keys.Value.Title, "一键收集战利品（按价值）")
            .AddText(Keys.Value.ButtonText, "价值≥{0}")
            .AddMeta(Keys.Value.ButtonFontSize, 24)
            // Value/Weight
            .AddText(Keys.ValueWeight.Title, "一键收集战利品（按价重比）")
            .AddText(Keys.ValueWeight.ButtonText, "价重比≥{0}")
            .AddMeta(Keys.ValueWeight.ButtonFontSize, 20);
            
        private static readonly LanguagePack PackKR = new LanguagePack()
            // Config
            .AddText(Keys.Config.ShowCollectAll, "【일괄 수집】 표시")
            .AddText(Keys.Config.ShowQuality, "【희귀도 기준 일괄 수집】 표시")
            .AddText(Keys.Config.QualityRange, "희귀도 범위 (1~9)")
            .AddText(Keys.Config.ShowValue, "【가치 기준 일괄 수집】 표시")
            .AddText(Keys.Config.ValueRange, "가치 범위 (1 이상)")
            .AddText(Keys.Config.ShowValueWeight, "【중량 대비 가치 기준 일괄 수집】 표시")
            .AddText(Keys.Config.ValueWeightRange, "중량 대비 가치 범위 (1 이상)")
            .AddText(Keys.Config.UseAutoValueColor, "가치 버튼 색상 자동 채우기 사용")
            .AddText(Keys.Config.QualityColor, "희귀도 버튼 색상")
            .AddText(Keys.Config.ValueColor, "가치 버튼 색상")
            .AddText(Keys.Config.ValueWeightColor, "중량 대비 가치 버튼 색상")
            // Quality
            .AddText(Keys.Quality.Title, "희귀도 기준 일괄 수집")
            .AddText(Keys.Quality.ButtonText, "희귀도≥{0}")
            .AddMeta(Keys.Quality.ButtonFontSize, 24)
            // Value
            .AddText(Keys.Value.Title, "가치 기준 일괄 수집")
            .AddText(Keys.Value.ButtonText, "가치≥{0}")
            .AddMeta(Keys.Value.ButtonFontSize, 24)
            // Value/Weight
            .AddText(Keys.ValueWeight.Title, "중량 대비 가치 기준 일괄 수집")
            .AddText(Keys.ValueWeight.ButtonText, "중량 대비 가치≥{0}")
            .AddMeta(Keys.ValueWeight.ButtonFontSize, 20);

        private static LanguagePack s_CurrentPack = PackEN;

        // ===== 组配置 =====
        // ===== 그룹 구성 (UI 주소지정, 명명 규칙) =====
        private sealed class ButtonGroupConfig(
            LootGroupId id,
            string titleKey,
            string btnFmtKey,
            string fontKey,
            string panelName,
            string titleName,
            string rowName,
            string buttonPattern
        )
        {
            public LootGroupId Id = id;
            public string TitleKey = titleKey;
            public string ButtonFmtKey = btnFmtKey;
            public string FontSizeKey = fontKey;

            public string PanelName = panelName; // e.g., UIConstants.QualityPanelName / ValuePanelName
            public string TitleNodeName = titleName; // e.g., UIConstants.QualityTitleName / ValueTitleName
            public string RowName = rowName; // e.g., UIConstants.QualityRowName / ValueRowName
            public string ButtonNamePattern = buttonPattern; // "OKL_Button_Quality_{0}" / "OKL_Button_Value_{0}"
        }

        private static readonly List<ButtonGroupConfig> s_Groups =
        [
            new ButtonGroupConfig(
                LootGroupId.Quality,
                Keys.Quality.Title,
                Keys.Quality.ButtonText,
                Keys.Quality.ButtonFontSize,
                UIConstants.QualityPanelName,
                UIConstants.QualityTitleName,
                UIConstants.QualityRowName,
                "OKL_Button_Quality_{0}"
            ),
            new ButtonGroupConfig(
                LootGroupId.Value,
                Keys.Value.Title,
                Keys.Value.ButtonText,
                Keys.Value.ButtonFontSize,
                UIConstants.ValuePanelName,
                UIConstants.ValueTitleName,
                UIConstants.ValueRowName,
                "OKL_Button_Value_{0}"
            ),
            new ButtonGroupConfig(
                LootGroupId.ValueWeight,
                Keys.ValueWeight.Title,
                Keys.ValueWeight.ButtonText,
                Keys.ValueWeight.ButtonFontSize,
                UIConstants.ValueWeightPanelName,
                UIConstants.ValueWeightTitleName,
                UIConstants.ValueWeightRowName,
                "OKL_Button_ValueWeight_{0}"
            ),
        ];

        // ===== 每组缓存（避免频繁查字典/装箱）=====
        // ===== 각 그룹 캐시 (잦은 딕셔너리 조회/박싱 방지)
        private sealed class GroupCache
        {
            public string Title;
            public string ButtonFmt;
            public int FontSize;

            public string Format(int x) => string.Format(ButtonFmt, x);
        }

        private static readonly Dictionary<LootGroupId, GroupCache> s_GroupCaches = [];

        // ===== Config 文案缓存 =====
        // ===== Config 복사 캐싱 =====
        private sealed class ConfigCache
        {
            public string ShowCollectAllLabel;
            public string ShowQualityLabel;
            public string ShowValueLabel;
            public string ShowValueWeightLabel;
            public string QualityRangeLabel;
            public string ValueRangeLabel;
            public string ValueWeightRangeLabel;
            public string UseAutoValueColorLabel;
            public string QualityColorLabel;
            public string ValueColorLabel;
            public string ValueWeightColorLabel;
        }

        private static ConfigCache s_Config;

        // ===== 外部访问：Config 与两个分组 =====
        // ===== 외부접근 : Config 및 3개 그룹
        public static class Config
        {
            public static string ShowCollectAllLabel => s_Config.ShowCollectAllLabel;
            public static string ShowQualityLabel => s_Config.ShowQualityLabel;
            public static string ShowValueLabel => s_Config.ShowValueLabel;
            public static string ShowValueWeightLabel => s_Config.ShowValueWeightLabel;
            public static string QualityRangeLabel => s_Config.QualityRangeLabel;
            public static string ValueRangeLabel => s_Config.ValueRangeLabel;
            public static string ValueWeightRangeLabel => s_Config.ValueWeightRangeLabel;
            public static string UseAutoValueColorLabel => s_Config.UseAutoValueColorLabel;
            public static string QualityColorLabel => s_Config.QualityColorLabel;
            public static string ValueColorLabel => s_Config.ValueColorLabel;
            public static string ValueWeightColorLabel => s_Config.ValueWeightColorLabel;
        }

        public static class Quality
        {
            private static GroupCache C => s_GroupCaches[LootGroupId.Quality];
            public static string Title => C.Title;
            public static int ButtonFontSize => C.FontSize;

            public static string Button(int v) => C.Format(v);
        }

        public static class Value
        {
            private static GroupCache C => s_GroupCaches[LootGroupId.Value];
            public static string Title => C.Title;
            public static int ButtonFontSize => C.FontSize;

            public static string Button(int v) => C.Format(v);
        }

        public static class ValueWeight
        {
            private static GroupCache C => s_GroupCaches[LootGroupId.ValueWeight];
            public static string Title => C.Title;
            public static int ButtonFontSize => C.FontSize;

            public static string Button(int v) => C.Format(v);
        }

        // ===== 生命周期 =====
        // ===== 생명주기 =====
        public static void Init()
        {
            // 确保不会重复订阅
            // 중복 구독되지 않도록 확인
            LocalizationManager.OnSetLanguage -= OnSetLanguage;
            LocalizationManager.OnSetLanguage += OnSetLanguage;

            OnSetLanguage(LocalizationManager.CurrentLanguage);
        }

        public static void Dispose()
        {
            LocalizationManager.OnSetLanguage -= OnSetLanguage;
        }

        private static void OnSetLanguage(SystemLanguage lang)
        {
            // 简体/繁体 → 中文；其余 → 英文
            // 간체/번체 -> 중국어팩; 한국어 -> 한국어 팩; 그외 -> 영어팩
            
            // Change s_CurrentPack based on lang
            switch(lang)
            {
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    s_CurrentPack = PackZH;
                    break;
                case SystemLanguage.Korean:
                    s_CurrentPack = PackKR;
                    break;
                default:
                    s_CurrentPack = PackEN;
                    break;
            }
            // s_CurrentPack =
            //     (
            //         lang == SystemLanguage.ChineseSimplified
            //         || lang == SystemLanguage.ChineseTraditional
            //     )
            //         ? PackZH
            //         : PackEN;

            // 推送覆盖文本给全局（仅 Texts；Meta 仅内部使用）
            // 텍스트 전역 덮어쓰기 (Texts만 해당; Meta는 내부 전용)
            foreach (var kv in s_CurrentPack.Texts)
            {
                LocalizationManager.SetOverrideText(kv.Key, kv.Value);
            }

            // 重建组缓存（键必定存在）
            // 그룹 캐시 재구성 (키는 반드시 존재)
            s_GroupCaches.Clear();
            foreach (var g in s_Groups)
            {
                s_GroupCaches[g.Id] = new GroupCache
                {
                    Title = s_CurrentPack.Texts[g.TitleKey],
                    ButtonFmt = s_CurrentPack.Texts[g.ButtonFmtKey],
                    FontSize = (int)s_CurrentPack.Meta[g.FontSizeKey],
                };
            }

            // Config 文案缓存
            // Config 텍스트 캐싱
            s_Config = new ConfigCache
            {
                ShowCollectAllLabel = s_CurrentPack.Texts[Keys.Config.ShowCollectAll],
                ShowQualityLabel = s_CurrentPack.Texts[Keys.Config.ShowQuality],
                ShowValueLabel = s_CurrentPack.Texts[Keys.Config.ShowValue],
                ShowValueWeightLabel = s_CurrentPack.Texts[Keys.Config.ShowValueWeight],
                QualityRangeLabel = s_CurrentPack.Texts[Keys.Config.QualityRange],
                ValueRangeLabel = s_CurrentPack.Texts[Keys.Config.ValueRange],
                ValueWeightRangeLabel = s_CurrentPack.Texts[Keys.Config.ValueWeightRange],
                UseAutoValueColorLabel = s_CurrentPack.Texts[Keys.Config.UseAutoValueColor],
                QualityColorLabel = s_CurrentPack.Texts[Keys.Config.QualityColor],
                ValueColorLabel = s_CurrentPack.Texts[Keys.Config.ValueColor],
                ValueWeightColorLabel = s_CurrentPack.Texts[Keys.Config.ValueWeightColor],
            };

            // 热刷新所有已打开的 LootView
            // 열린 모든 LootView 핫 리프레시
            RelabelActiveLootViews();
        }

        // ===== UI 刷新（扫描按钮名后缀）=====
        // ===== UI 새로고침 (버튼 이름 접미사 스캔) =====
        public static void RelabelActiveLootViews()
        {
            var all = Resources.FindObjectsOfTypeAll<LootView>();
            foreach (var lv in all)
            {
                var rt = lv ? lv.GetComponent<RectTransform>() : null;
                if (!rt)
                {
                    continue;
                }

                foreach (var g in s_Groups)
                {
                    UpdateGroupUI(rt, g);
                }
            }
        }

        private static void UpdateGroupUI(RectTransform root, ButtonGroupConfig g)
        {
            // fallback 없음: 직접 인덱스
            var cache = s_GroupCaches[g.Id];

            // 标题
            // 제목
            var titleRt = root.Find(g.TitleNodeName) as RectTransform;
            if (titleRt)
            {
                var tmp =
                    titleRt.GetComponent<TextMeshProUGUI>()
                    ?? titleRt.gameObject.AddComponent<TextMeshProUGUI>();
                if (TMP_Settings.defaultFontAsset != null && tmp.font == null)
                {
                    tmp.font = TMP_Settings.defaultFontAsset;
                }

                tmp.text = cache.Title;
                tmp.alignment = Typography.TitleAlign;
                tmp.fontSize = cache.FontSize;
            }

            // 行内按钮
            // 행 내 버튼
            var row = root.Find(g.RowName) as RectTransform;
            if (!row)
            {
                return;
            }

            string prefix = g.ButtonNamePattern.Replace("{0}", string.Empty);

            for (int i = 0; i < row.childCount; i++)
            {
                var child = row.GetChild(i);
                if (!child)
                {
                    continue;
                }

                var name = child.name ?? string.Empty;
                if (!name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                // 解析阈值
                // 임계값 파싱
                if (!int.TryParse(name.Substring(prefix.Length), out var v))
                {
                    continue;
                }

                var label = child.Find(UIConstants.LabelName) as RectTransform;
                if (!label)
                {
                    continue;
                }

                var tmp =
                    label.GetComponent<TextMeshProUGUI>()
                    ?? label.gameObject.AddComponent<TextMeshProUGUI>();
                if (TMP_Settings.defaultFontAsset != null && tmp.font == null)
                {
                    tmp.font = TMP_Settings.defaultFontAsset;
                }

                tmp.text = cache.Format(v);
                tmp.alignment = Typography.ButtonAlign;
                tmp.fontSize = cache.FontSize;
            }
        }
    }
}
