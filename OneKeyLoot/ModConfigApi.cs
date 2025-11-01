using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

//替换为你的mod命名空间, 防止多个同名ModConfigAPI冲突
//당신의 모드 네임스페이스로 교체하여 동일한 이름의 ModConfigAPI 충돌 방지
namespace OneKeyLoot
{
    /// <summary>
    /// ModConfig 安全接口封装类 - 提供不抛异常的静态接口
    /// ModConfig Safe API Wrapper Class - Provides non-throwing static interfaces
    /// ModConfig 안전 인터페이스 래퍼 클래스 - 예외를 발생시키지 않는 정적 인터페이스 제공
    /// </summary>
    public static class ModConfigAPI
    {
        public static string ModConfigName = "ModConfig";

        //Ensure this match the number of ModConfig.ModBehaviour.VERSION
        //这里确保版本号与ModConfig.ModBehaviour.VERSION匹配
        //여기서는 ModConfig.ModBehaviour.VERSION과 버전 번호가 일치하는지 확인합니다.
        private const int ModConfigVersion = 1;

        private static string TAG = $"ModConfig_v{ModConfigVersion}";

        private static Type modBehaviourType;
        private static Type optionsManagerType;
        public static bool isInitialized = false;
        private static bool versionChecked = false;
        private static bool isVersionCompatible = false;

        /// <summary>
        /// 检查版本兼容性
        /// Check version compatibility
        /// 버전 호환성 검사
        /// </summary>
        private static bool CheckVersionCompatibility()
        {
            if (versionChecked)
            {
                return isVersionCompatible;
            }

            try
            {
                // 尝试获取 ModConfig 的版本号
                // Try to get ModConfig version number
                // ModConfig의 버전 번호 가져오기 시도
                FieldInfo versionField = modBehaviourType.GetField(
                    "VERSION",
                    BindingFlags.Public | BindingFlags.Static
                );
                if (versionField != null && versionField.FieldType == typeof(int))
                {
                    int modConfigVersion = (int)versionField.GetValue(null);
                    isVersionCompatible = modConfigVersion == ModConfigVersion;

                    if (!isVersionCompatible)
                    {
                        Debug.LogError(
                            $"[{TAG}] 版本不匹配！API版本: {ModConfigVersion}, ModConfig版本: {modConfigVersion}"
                        );
                        return false;
                    }

                    Debug.Log($"[{TAG}] 版本检查通过: {ModConfigVersion}");
                    versionChecked = true;
                    return true;
                }
                else
                {
                    // 如果找不到版本字段，发出警告但继续运行（向后兼容）
                    // If version field not found, warn but continue (backward compatibility)
                    // 버전 필드를 찾을 수 없는 경우 경고를 표시하지만 계속 진행(하위 호환성)
                    Debug.LogWarning($"[{TAG}] 未找到版本信息字段，跳过版本检查");
                    isVersionCompatible = true;
                    versionChecked = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 版本检查失败: {ex.Message}");
                isVersionCompatible = false;
                versionChecked = true;
                return false;
            }
        }

        /// <summary>
        /// 初始化 ModConfigAPI，检查必要的函数是否存在
        /// Initialize ModConfigAPI, check if necessary functions exist
        /// ModConfigAPI 초기화, 필요한 함수가 존재하는지 확인
        /// </summary>
        public static bool Initialize()
        {
            try
            {
                if (isInitialized)
                {
                    return true;
                }

                // 获取 ModBehaviour 类型
                // Get ModBehaviour type
                // ModBehaviour 유형 가져오기
                modBehaviourType = FindTypeInAssemblies("ModConfig.ModBehaviour");
                if (modBehaviourType == null)
                {
                    Debug.LogWarning(
                        $"[{TAG}] ModConfig.ModBehaviour 类型未找到，ModConfig 可能未加载"
                    );
                    return false;
                }

                // 获取 OptionsManager_Mod 类型
                // Get OptionsManager_Mod type
                // OptionsManager_Mod 유형 가져오기
                optionsManagerType = FindTypeInAssemblies("ModConfig.OptionsManager_Mod");
                if (optionsManagerType == null)
                {
                    Debug.LogWarning($"[{TAG}] ModConfig.OptionsManager_Mod 类型未找到");
                    return false;
                }

                // 检查版本兼容性
                // Check version compatibility
                // 버전 호환성 검사
                if (!CheckVersionCompatibility())
                {
                    Debug.LogWarning($"[{TAG}] ModConfig version mismatch!!!");
                    return false;
                }

                // 检查必要的静态方法是否存在
                // Check if necessary static methods exist
                // 필요한 정적 메서드가 존재하는지 확인
                string[] requiredMethods =
                [
                    "AddDropdownList",
                    "AddInputWithSlider",
                    "AddBoolDropdownList",
                    "AddOnOptionsChangedDelegate",
                    "RemoveOnOptionsChangedDelegate",
                ];

                foreach (string methodName in requiredMethods)
                {
                    MethodInfo method = modBehaviourType.GetMethod(
                        methodName,
                        BindingFlags.Public | BindingFlags.Static
                    );
                    if (method == null)
                    {
                        Debug.LogError($"[{TAG}] 必要方法 {methodName} 未找到");
                        return false;
                    }
                }

                isInitialized = true;
                Debug.Log($"[{TAG}] ModConfigAPI 初始化成功");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 初始化失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 在所有已加载的程序集中查找类型
        /// Find type in all loaded assemblies
        /// 모든 로드된 어셈블리에서 유형 찾기
        /// </summary>
        private static Type FindTypeInAssemblies(string typeName)
        {
            try
            {
                // 获取当前域中的所有程序集
                // Get all assemblies in the current domain
                // 현재 도메인의 모든 어셈블리 가져오기
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (Assembly assembly in assemblies)
                {
                    try
                    {
                        // 检查程序集名称是否包含 ModConfig
                        // ModConfig가 어셈블리 이름에 포함되어 있는지 확인
                        if (assembly.FullName.Contains("ModConfig"))
                        {
                            Debug.Log($"[{TAG}] 找到 ModConfig 相关程序集: {assembly.FullName}");
                        }

                        // 尝试在该程序集中查找类型
                        // 해당 어셈블리에서 유형 찾기 시도
                        Type type = assembly.GetType(typeName);
                        if (type != null)
                        {
                            Debug.Log(
                                $"[{TAG}] 在程序集 {assembly.FullName} 中找到类型 {typeName}"
                            );
                            return type;
                        }
                    }
                    catch (Exception)
                    {
                        // 忽略单个程序集的查找错误
                        // 어셈블리별 검색 오류 무시
                        continue;
                    }
                }

                // 记录所有已加载的程序集用于调试
                // 디버깅을 위해 로드된 모든 어셈블리 기록
                Debug.LogWarning(
                    $"[{TAG}] 在所有程序集中未找到类型 {typeName}，已加载程序集数量: {assemblies.Length}"
                );
                foreach (var assembly in assemblies.Where(a => a.FullName.Contains("ModConfig")))
                {
                    Debug.Log($"[{TAG}] ModConfig 相关程序集: {assembly.FullName}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 程序集扫描失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 安全地添加选项变更事件委托
        /// Safely add options changed event delegate
        /// 안전하게 옵션 변경 이벤트 대리자 추가
        /// </summary>
        /// <param name="action">事件处理委托，参数为变更的选项键名</param>
        /// <returns>是否成功添加</returns>
        public static bool SafeAddOnOptionsChangedDelegate(Action<string> action)
        {
            if (!Initialize())
            {
                return false;
            }

            if (action == null)
            {
                Debug.LogWarning($"[{TAG}] 不能添加空的事件委托");
                return false;
            }

            try
            {
                MethodInfo method = modBehaviourType.GetMethod(
                    "AddOnOptionsChangedDelegate",
                    BindingFlags.Public | BindingFlags.Static
                );
                method.Invoke(null, [action]);

                Debug.Log($"[{TAG}] 成功添加选项变更事件委托");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 添加选项变更事件委托失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地移除选项变更事件委托
        /// Safely remove options changed event delegate
        /// 옵션 변경 이벤트 대리자 안전하게 제거
        /// </summary>
        /// <param name="action">要移除的事件处理委托</param>
        /// <returns>是否成功移除</returns>
        public static bool SafeRemoveOnOptionsChangedDelegate(Action<string> action)
        {
            if (!Initialize())
            {
                return false;
            }

            if (action == null)
            {
                Debug.LogWarning($"[{TAG}] 不能移除空的事件委托");
                return false;
            }

            try
            {
                MethodInfo method = modBehaviourType.GetMethod(
                    "RemoveOnOptionsChangedDelegate",
                    BindingFlags.Public | BindingFlags.Static
                );
                method.Invoke(null, [action]);

                Debug.Log($"[{TAG}] 成功移除选项变更事件委托");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 移除选项变更事件委托失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地添加下拉列表配置项
        /// Safely add dropdown list configuration item
        /// 드롭다운 목록 구성 항목을 안전하게 추가
        /// </summary>
        public static bool SafeAddDropdownList(
            string modName,
            string key,
            string description,
            System.Collections.Generic.SortedDictionary<string, object> options,
            Type valueType,
            object defaultValue
        )
        {
            key = $"{modName}_{key}";

            if (!Initialize())
            {
                return false;
            }

            try
            {
                MethodInfo method = modBehaviourType.GetMethod(
                    "AddDropdownList",
                    BindingFlags.Public | BindingFlags.Static
                );
                method.Invoke(null, [modName, key, description, options, valueType, defaultValue]);

                Debug.Log($"[{TAG}] 成功添加下拉列表: {modName}.{key}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 添加下拉列表失败 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地添加带滑条的输入框配置项
        /// Safely add input box with slider configuration item
        /// 슬라이더가 있는 입력 상자 구성 항목을 안전하게 추가
        /// </summary>
        public static bool SafeAddInputWithSlider(
            string modName,
            string key,
            string description,
            Type valueType,
            object defaultValue,
            Vector2? sliderRange = null
        )
        {
            key = $"{modName}_{key}";

            if (!Initialize())
            {
                return false;
            }

            try
            {
                MethodInfo method = modBehaviourType.GetMethod(
                    "AddInputWithSlider",
                    BindingFlags.Public | BindingFlags.Static
                );

                // 处理可空参数
                // Handle nullable parameters
                // null 허용 매개변수 처리
                object[] parameters = sliderRange.HasValue
                    ? [modName, key, description, valueType, defaultValue, sliderRange.Value]
                    : [modName, key, description, valueType, defaultValue, null];

                method.Invoke(null, parameters);

                Debug.Log($"[{TAG}] 成功添加滑条输入框: {modName}.{key}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 添加滑条输入框失败 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地添加布尔下拉列表配置项
        /// Safely add boolean dropdown list configuration item
        /// 부울 드롭다운 목록 구성 항목을 안전하게 추가
        /// </summary>
        public static bool SafeAddBoolDropdownList(
            string modName,
            string key,
            string description,
            bool defaultValue
        )
        {
            key = $"{modName}_{key}";

            if (!Initialize())
            {
                return false;
            }

            try
            {
                MethodInfo method = modBehaviourType.GetMethod(
                    "AddBoolDropdownList",
                    BindingFlags.Public | BindingFlags.Static
                );
                method.Invoke(null, [modName, key, description, defaultValue]);

                Debug.Log($"[{TAG}] 成功添加布尔下拉列表: {modName}.{key}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 添加布尔下拉列表失败 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地加载配置值
        /// Safely load configuration value
        /// 안전하게 구성 값 로드
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>加载的值或默认值</returns>
        public static T SafeLoad<T>(string mod_name, string key, T defaultValue = default)
        {
            key = $"{mod_name}_{key}";

            if (!Initialize())
            {
                return defaultValue;
            }

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[{TAG}] 配置键不能为空");
                return defaultValue;
            }

            try
            {
                MethodInfo loadMethod = optionsManagerType.GetMethod(
                    "Load",
                    BindingFlags.Public | BindingFlags.Static
                );
                if (loadMethod == null)
                {
                    Debug.LogError($"[{TAG}] 未找到 OptionsManager_Mod.Load 方法");
                    return defaultValue;
                }

                // 获取泛型方法
                MethodInfo genericLoadMethod = loadMethod.MakeGenericMethod(typeof(T));
                object result = genericLoadMethod.Invoke(null, [key, defaultValue]);

                Debug.Log($"[{TAG}] 成功加载配置: {key} = {result}");
                return (T)result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 加载配置失败 {key}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 安全地保存配置值
        /// Safely save configuration value
        /// 안전하게 구성 값 저장
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="value">要保存的值</param>
        /// <returns>是否保存成功</returns>
        public static bool SafeSave<T>(string mod_name, string key, T value)
        {
            key = $"{mod_name}_{key}";

            if (!Initialize())
            {
                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[{TAG}] 配置键不能为空");
                return false;
            }

            try
            {
                MethodInfo saveMethod = optionsManagerType.GetMethod(
                    "Save",
                    BindingFlags.Public | BindingFlags.Static
                );
                if (saveMethod == null)
                {
                    Debug.LogError($"[{TAG}] 未找到 OptionsManager_Mod.Save 方法");
                    return false;
                }

                // 获取泛型方法
                MethodInfo genericSaveMethod = saveMethod.MakeGenericMethod(typeof(T));
                genericSaveMethod.Invoke(null, [key, value]);

                Debug.Log($"[{TAG}] 成功保存配置: {key} = {value}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{TAG}] 保存配置失败 {key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查 ModConfig 是否可用
        /// Check if ModConfig is available
        /// ModConfig 사용 가능 여부 확인
        /// </summary>
        public static bool IsAvailable()
        {
            return Initialize();
        }

        /// <summary>
        /// 获取 ModConfig 版本信息（如果存在）
        /// Get ModConfig version information (if exists)
        /// ModConfig 버전 정보 가져오기(있는 경우)
        /// </summary>
        public static string GetVersionInfo()
        {
            if (!Initialize())
            {
                return "ModConfig 未加载 | ModConfig not loaded";
            }

            try
            {
                // 尝试获取版本信息（如果 ModBehaviour 有相关字段或属性）
                // Try to get version information (if ModBehaviour has related fields or properties)
                // ModBehaviour에 관련 필드나 속성이 있는 경우 버전 정보 가져오기 시도
                FieldInfo versionField = modBehaviourType.GetField(
                    "VERSION",
                    BindingFlags.Public | BindingFlags.Static
                );
                if (versionField != null && versionField.FieldType == typeof(int))
                {
                    int modConfigVersion = (int)versionField.GetValue(null);
                    string compatibility =
                        (modConfigVersion == ModConfigVersion) ? "兼容" : "不兼容";
                    return $"ModConfig v{modConfigVersion} (API v{ModConfigVersion}, {compatibility})";
                }

                PropertyInfo versionProperty = modBehaviourType.GetProperty(
                    "VERSION",
                    BindingFlags.Public | BindingFlags.Static
                );
                if (versionProperty != null)
                {
                    object versionValue = versionProperty.GetValue(null);
                    return versionValue?.ToString() ?? "未知版本 | Unknown version";
                }

                return "ModConfig 已加载（版本信息不可用） | ModConfig loaded (version info unavailable)";
            }
            catch
            {
                return "ModConfig 已加载（版本检查失败） | ModConfig loaded (version check failed)";
            }
        }

        /// <summary>
        /// 检查版本兼容性
        /// Check version compatibility
        /// 버전 호환성 검사
        /// </summary>
        public static bool IsVersionCompatible()
        {
            if (!Initialize())
            {
                return false;
            }

            return isVersionCompatible;
        }
    }
}
