using System;
using System.IO;
using Duckov.Modding;
using UnityEngine;

namespace OneKeyLoot
{
    /// <summary>
    /// 配置数据
    /// </summary>
    /// <koreanSummary>
    /// 구성 데이터
    /// </koreanSummary>
    [Serializable]
    public class ModConfigData
    {
        public bool showCollectAll = true;
        public bool showQuality = true;
        public bool showValue = true;
        public bool showValueWeight = true;

        public string qualityRange = "2,3,4,5";
        public string valueRange = "100,500,1000";
        public string valueWeightRange = "500,2500,5000";

        public string qualityColor = "#4CAF4F,#42A5F5,#BA68C6,#BF7F33";
        public string valueColor = "#4CAF4F,#42A5F5,#BA68C6,#BF7F33";
        public string valueWeightColor = "#4CAF4F,#42A5F5,#BA68C6,#BF7F33";

        public string configToken = "OneKeyLoot_config_v2";

        public static readonly ModConfigData Defaults = new();
    }

    /// <summary>
    /// 配置中心：集中管理 Mod 的设置项注册、加载/保存、运行时快照与事件绑定
    /// </summary>
    /// <koreanSummary>
    /// 구성 센터: Mod의 설정 항목 등록, 로드/저장, 런타임 스냅샷 및 이벤트 바인딩을 중앙 집중식으로 관리
    /// </koreanSummary>
    public static class ModConfig
    {
        public const string DisplayName = "一键拾取战利品 One-Key Loot";

        // 配置变更事件
        // 구성 변경 이벤트
        public static event Action RuntimeChanged;

        // 内部持久化实体（与 ModConfigAPI 交互）
        // 내부 영구 엔터티 (ModConfigAPI와 상호 작용)
        private static readonly ModConfigData _config = new();

        // 运行时快照（供运行期读取，不直接与 UI 绑定）
        // 런타임 스냅샷 (런타임 읽기 전용, UI에 직접 바인딩되지 않음)
        private static ModConfigData s_RuntimeConfig = new();
        internal static ModConfigData Runtime => s_RuntimeConfig ??= new ModConfigData();

        private static string PersistentConfigPath =>
            Path.Combine(Application.streamingAssetsPath, "OneKeyLootConfig.txt");

        public static void InitializeOnEnable()
        {
            if (ModConfigAPI.IsAvailable())
            {
                Debug.Log("[OneKeyLoot]: ModConfig already available!");
                SetupModConfig();
                LoadConfigFromModConfig();
                UpdateRuntimeConfigSnapshot(_config);
            }

            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnModConfigOptionsChanged);
        }

        public static void OnDisableCleanup()
        {
            ModConfigAPI.SafeRemoveOnOptionsChangedDelegate(OnModConfigOptionsChanged);
        }

        public static void OnModActivated(ModInfo info, Duckov.Modding.ModBehaviour behaviour)
        {
            if (info.name == ModConfigAPI.ModConfigName)
            {
                Debug.Log("[OneKeyLoot]: ModConfig activated!");
                SetupModConfig();
                LoadConfigFromModConfig();
                UpdateRuntimeConfigSnapshot(_config);
            }
        }

        private static void UpdateRuntimeConfigSnapshot(ModConfigData src)
        {
            s_RuntimeConfig ??= new ModConfigData();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(src), s_RuntimeConfig);
            RuntimeChanged?.Invoke();
        }

        private static void SaveConfig(ModConfigData cfg)
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

        private static void SetupModConfig()
        {
            if (!ModConfigAPI.IsAvailable())
            {
                Debug.LogWarning("OneKeyLoot: ModConfig not available");
                return;
            }

            Debug.Log("准备添加ModConfig配置项");
            ModConfigAPI.SafeAddOnOptionsChangedDelegate(OnModConfigOptionsChanged);

            // 倒序添加以保证显示顺序
            // 뒤에서부터 추가하여 표시 순서 보장
            ModConfigAPI.SafeAddInputWithSlider(
                DisplayName,
                "valueWeightColor",
                i18n.Config.ValueWeightColorLabel,
                typeof(string),
                _config.valueWeightColor,
                null
            );
            ModConfigAPI.SafeAddInputWithSlider(
                DisplayName,
                "valueWeightRange",
                i18n.Config.ValueWeightRangeLabel,
                typeof(string),
                _config.valueWeightRange,
                null
            );
            ModConfigAPI.SafeAddBoolDropdownList(
                DisplayName,
                "showValueWeight",
                i18n.Config.ShowValueWeightLabel,
                _config.showValueWeight
            );
            ModConfigAPI.SafeAddInputWithSlider(
                DisplayName,
                "valueColor",
                i18n.Config.ValueColorLabel,
                typeof(string),
                _config.valueColor,
                null
            );
            ModConfigAPI.SafeAddInputWithSlider(
                DisplayName,
                "valueRange",
                i18n.Config.ValueRangeLabel,
                typeof(string),
                _config.valueRange,
                null
            );
            ModConfigAPI.SafeAddBoolDropdownList(
                DisplayName,
                "showValue",
                i18n.Config.ShowValueLabel,
                _config.showValue
            );
            ModConfigAPI.SafeAddInputWithSlider(
                DisplayName,
                "qualityColor",
                i18n.Config.QualityColorLabel,
                typeof(string),
                _config.qualityColor,
                null
            );
            ModConfigAPI.SafeAddInputWithSlider(
                DisplayName,
                "qualityRange",
                i18n.Config.QualityRangeLabel,
                typeof(string),
                _config.qualityRange,
                null
            );
            ModConfigAPI.SafeAddBoolDropdownList(
                DisplayName,
                "showQuality",
                i18n.Config.ShowQualityLabel,
                _config.showQuality
            );
            ModConfigAPI.SafeAddBoolDropdownList(
                DisplayName,
                "showCollectAll",
                i18n.Config.ShowCollectAllLabel,
                _config.showCollectAll
            );

            Debug.Log("[OneKeyLoot]: ModConfig setup completed");
        }

        private static void OnModConfigOptionsChanged(string key)
        {
            if (!key.StartsWith(DisplayName + "_"))
            {
                return;
            }

            LoadConfigFromModConfig();
            SaveConfig(_config);
            UpdateRuntimeConfigSnapshot(_config);
            i18n.RelabelActiveLootViews();

            Debug.Log($"[OneKeyLoot]: ModConfig updated - {key}");
        }

        private static void LoadConfigFromModConfig()
        {
            _config.showCollectAll = ModConfigAPI.SafeLoad(
                DisplayName,
                "showCollectAll",
                _config.showCollectAll
            );
            _config.showQuality = ModConfigAPI.SafeLoad(
                DisplayName,
                "showQuality",
                _config.showQuality
            );
            _config.qualityRange = ModConfigAPI.SafeLoad(
                DisplayName,
                "qualityRange",
                _config.qualityRange
            );
            _config.showValue = ModConfigAPI.SafeLoad(DisplayName, "showValue", _config.showValue);
            _config.valueRange = ModConfigAPI.SafeLoad(
                DisplayName,
                "valueRange",
                _config.valueRange
            );
            _config.showValueWeight = ModConfigAPI.SafeLoad(
                DisplayName,
                "showValueWeight",
                _config.showValueWeight
            );
            _config.valueWeightRange = ModConfigAPI.SafeLoad(
                DisplayName,
                "valueWeightRange",
                _config.valueWeightRange
            );
            _config.qualityColor = ModConfigAPI.SafeLoad(
                DisplayName,
                "qualityColor",
                _config.qualityColor
            );
            _config.valueColor = ModConfigAPI.SafeLoad(
                DisplayName,
                "valueColor",
                _config.valueColor
            );
            _config.valueWeightColor = ModConfigAPI.SafeLoad(
                DisplayName,
                "valueWeightColor",
                _config.valueWeightColor
            );
        }
    }
}
