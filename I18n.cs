using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Duckov.UI;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

namespace OneKeyLoot
{
    /// <summary>
    /// i18n（语言与文案）
    /// - 不包含 fallback / try-catch：假定所有键值必定存在
    /// - 使用 LocalizationManager.OnSetLanguage 驱动语言切换
    /// - 保留缓存：标题、按钮格式、字号
    /// - 适配 Quality/Value 各自独立 Panel/Row/Title 名称，按钮文本扫描自节点名后缀
    /// </summary>
    internal static class i18n
    {
        // ===== 组标识 =====
        public enum LootGroupId
        {
            Config,
            Quality,
            Value,
        }

        // ===== 文案 Key =====
        public static class Keys
        {
            public static class Config
            {
                public const string ShowCollectAll = "OKL.Config_ShowCollectAll";
                public const string ShowQuality = "OKL.Config_ShowQuality";
                public const string QaulityRange = "OKL.Config_QaulityRange";
                public const string ShowValue = "OKL.Config_ShowValue";
                public const string ValueRange = "OKL.Config_ValueRange";
            }

            public static class Quality
            {
                public const string Title = "OKL.Quality_Title";
                public const string ButtonTextDefault = "OKL.Quality_ButtonTextDefault"; // 需要占位符 ≥{0}
                public const string ButtonFontSize = "OKL.Quality_ButtonFontSize"; // 元数据：字号（数字）
            }

            public static class Value
            {
                public const string Title = "OKL.Value_Title";
                public const string ButtonTextDefault = "OKL.Value_ButtonTextDefault"; // 需要占位符 ≥{0}
                public const string ButtonFontSize = "OKL.Value_ButtonFontSize"; // 元数据：字号（数字）
            }
        }

        // ===== 字体与排版（简单对齐）=====
        public static class Typography
        {
            public static readonly TextAlignmentOptions TitleAlign = TextAlignmentOptions.Midline;
            public static readonly TextAlignmentOptions ButtonAlign = TextAlignmentOptions.Midline;
        }

        // ===== 语言包：文本 + 元数据 =====
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

        // ===== 内置语言包（根据需要可继续扩展）=====
        private static readonly LanguagePack PackEN = new LanguagePack()
            // Config
            .AddText(Keys.Config.ShowCollectAll, "Display [Collect All]")
            .AddText(Keys.Config.ShowQuality, "Display [One-Key Loot (by Quality)]")
            .AddText(Keys.Config.QaulityRange, "Quality Range (1~9)")
            .AddText(Keys.Config.ShowValue, "Display [One-Key Loot (by Value)]")
            .AddText(Keys.Config.ValueRange, "Value Range (1+)")
            // Quality
            .AddText(Keys.Quality.Title, "One-Key Loot (by Quality)")
            .AddText(Keys.Quality.ButtonTextDefault, "Quality ≥ {0}")
            .AddMeta(Keys.Quality.ButtonFontSize, 20)
            // Value
            .AddText(Keys.Value.Title, "One-Key Loot (by Value)")
            .AddText(Keys.Value.ButtonTextDefault, "Value ≥ {0}")
            .AddMeta(Keys.Value.ButtonFontSize, 20);

        private static readonly LanguagePack PackZH = new LanguagePack()
            // Config
            .AddText(Keys.Config.ShowCollectAll, "显示【全部拾取】")
            .AddText(Keys.Config.ShowQuality, "显示【一键收集战利品（按品质）】")
            .AddText(Keys.Config.QaulityRange, "品质范围（1~9）")
            .AddText(Keys.Config.ShowValue, "显示【一键收集战利品（按价值）】")
            .AddText(Keys.Config.ValueRange, "价值范围（1+）")
            // Quality
            .AddText(Keys.Quality.Title, "一键收集战利品（按品质）")
            .AddText(Keys.Quality.ButtonTextDefault, "品质≥{0}")
            .AddMeta(Keys.Quality.ButtonFontSize, 24)
            // Value
            .AddText(Keys.Value.Title, "一键收集战利品（按价值）")
            .AddText(Keys.Value.ButtonTextDefault, "价值≥{0}")
            .AddMeta(Keys.Value.ButtonFontSize, 24);

        private static LanguagePack s_CurrentPack = PackEN;

        // ===== 组配置（UI 寻址、命名约定）=====
        private sealed class ButtonGroupConfig
        {
            public LootGroupId Id;
            public string TitleKey;
            public string ButtonFmtKey;
            public string FontSizeKey;

            public string PanelName; // e.g., UIConstants.QualityPanelName / ValuePanelName
            public string TitleNodeName; // e.g., UIConstants.QualityTitleName / ValueTitleName
            public string RowName; // e.g., UIConstants.QualityRowName / ValueRowName
            public string ButtonNamePattern; // "OKL_Button_Quality_{0}" / "OKL_Button_Value_{0}"

            public ButtonGroupConfig(
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
                Id = id;
                TitleKey = titleKey;
                ButtonFmtKey = btnFmtKey;
                FontSizeKey = fontKey;
                PanelName = panelName;
                TitleNodeName = titleName;
                RowName = rowName;
                ButtonNamePattern = buttonPattern;
            }
        }

        private static readonly List<ButtonGroupConfig> s_Groups =
        [
            new ButtonGroupConfig(
                LootGroupId.Quality,
                Keys.Quality.Title,
                Keys.Quality.ButtonTextDefault,
                Keys.Quality.ButtonFontSize,
                UIConstants.QualityPanelName,
                UIConstants.QualityTitleName,
                UIConstants.QualityRowName,
                "OKL_Button_Quality_{0}"
            ),
            new ButtonGroupConfig(
                LootGroupId.Value,
                Keys.Value.Title,
                Keys.Value.ButtonTextDefault,
                Keys.Value.ButtonFontSize,
                UIConstants.ValuePanelName,
                UIConstants.ValueTitleName,
                UIConstants.ValueRowName,
                "OKL_Button_Value_{0}"
            ),
        ];

        // ===== 每组缓存（避免频繁查字典/装箱）=====
        private sealed class GroupCache
        {
            public string Title;
            public string ButtonFmt;
            public int FontSize;

            public string Format(int x) => string.Format(ButtonFmt, x);
        }

        private static readonly Dictionary<LootGroupId, GroupCache> s_GroupCaches = new();

        // ===== Config 文案缓存 =====
        private sealed class ConfigCache
        {
            public string ShowCollectAllLabel;
            public string ShowQualityLabel;
            public string QualityRangeLabel;
            public string ShowValueLabel;
            public string ValueRangeLabel;
        }

        private static ConfigCache s_Config;

        // ===== 外部访问：Config 与两个分组 =====
        public static class Config
        {
            public static string ShowCollectAllLabel => s_Config.ShowCollectAllLabel;
            public static string ShowQualityLabel => s_Config.ShowQualityLabel;
            public static string QualityRangeLabel => s_Config.QualityRangeLabel;
            public static string ShowValueLabel => s_Config.ShowValueLabel;
            public static string ValueRangeLabel => s_Config.ValueRangeLabel;
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

        // ===== 生命周期 =====
        public static void Init()
        {
            // 先确保不会重复订阅
            LocalizationManager.OnSetLanguage -= OnSetLanguage;
            LocalizationManager.OnSetLanguage += OnSetLanguage;

            // 立刻构建 s_Config / s_GroupCaches，避免同帧读取空引用
            OnSetLanguage(LocalizationManager.CurrentLanguage);
        }

        public static void Dispose()
        {
            LocalizationManager.OnSetLanguage -= OnSetLanguage;
        }

        private static void OnSetLanguage(SystemLanguage lang)
        {
            // 简体/繁体 → 中文包；其余 → 英文包
            s_CurrentPack =
                (
                    lang == SystemLanguage.ChineseSimplified
                    || lang == SystemLanguage.ChineseTraditional
                )
                    ? PackZH
                    : PackEN;

            // 推送覆盖文本给全局（仅 Texts；Meta 仅内部使用）
            foreach (var kv in s_CurrentPack.Texts)
                LocalizationManager.SetOverrideText(kv.Key, kv.Value);

            // 重建组缓存（键必定存在）
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
            s_Config = new ConfigCache
            {
                ShowCollectAllLabel = s_CurrentPack.Texts[Keys.Config.ShowCollectAll],
                ShowQualityLabel = s_CurrentPack.Texts[Keys.Config.ShowQuality],
                QualityRangeLabel = s_CurrentPack.Texts[Keys.Config.QaulityRange],
                ShowValueLabel = s_CurrentPack.Texts[Keys.Config.ShowValue],
                ValueRangeLabel = s_CurrentPack.Texts[Keys.Config.ValueRange],
            };

            // 热刷新所有已打开的 LootView
            RelabelActiveLootViews();
        }

        // ===== UI 刷新（扫描按钮名后缀）=====
        public static void RelabelActiveLootViews()
        {
            var all = Resources.FindObjectsOfTypeAll<Duckov.UI.LootView>();
            foreach (var lv in all)
            {
                var rt = lv ? lv.GetComponent<RectTransform>() : null;
                if (!rt)
                    continue;

                foreach (var g in s_Groups)
                    UpdateGroupUI(rt, g);
            }
        }

        private static void UpdateGroupUI(RectTransform root, ButtonGroupConfig g)
        {
            var cache = s_GroupCaches[g.Id]; // 无 fallback：直接索引

            // 标题
            var titleRt = root.Find(g.TitleNodeName) as RectTransform;
            if (titleRt)
            {
                var tmp =
                    titleRt.GetComponent<TextMeshProUGUI>()
                    ?? titleRt.gameObject.AddComponent<TextMeshProUGUI>();
                if (TMP_Settings.defaultFontAsset != null && tmp.font == null)
                    tmp.font = TMP_Settings.defaultFontAsset;
                tmp.text = cache.Title;
                tmp.alignment = Typography.TitleAlign;
                tmp.fontSize = cache.FontSize;
            }

            // 行内按钮
            var row = root.Find(g.RowName) as RectTransform;
            if (!row)
                return;

            string prefix = g.ButtonNamePattern.Replace("{0}", string.Empty);

            for (int i = 0; i < row.childCount; i++)
            {
                var child = row.GetChild(i);
                if (!child)
                    continue;

                var name = child.name ?? string.Empty;
                if (!name.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                // 解析阈值
                if (!int.TryParse(name.Substring(prefix.Length), out var v))
                    continue;

                var label = child.Find(UIConstants.LabelName) as RectTransform;
                if (!label)
                    continue;

                var tmp =
                    label.GetComponent<TextMeshProUGUI>()
                    ?? label.gameObject.AddComponent<TextMeshProUGUI>();
                if (TMP_Settings.defaultFontAsset != null && tmp.font == null)
                    tmp.font = TMP_Settings.defaultFontAsset;

                tmp.text = cache.Format(v);
                tmp.alignment = Typography.ButtonAlign;
                tmp.fontSize = cache.FontSize;
            }
        }
    }
}
