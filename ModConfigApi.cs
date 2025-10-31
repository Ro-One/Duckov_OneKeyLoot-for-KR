using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

///替换为你的mod命名空间, 防止多个同名ModConfigAPI冲突
///Replace with your mod namespace to prevent conflicts with multiple ModConfigAPI of the same name
/// 동일한 이름의 여러 ModConfigAPI와의 충돌을 방지하기 위해 모드 네임스페이스로 교체
namespace OneKeyLoot
{
    /// <summary>
    /// ModConfig 安全接口封装类 - 提供不抛异常的静态接口
    /// ModConfig Safe API Wrapper Class - Provides non-throwing static interfaces
    /// </summary>
    /// <koreansummary>
    /// ModConfig 안전한 API 래퍼 클래스 - 예외를 발생시키지 않는 정적 인터페이스 제공
    /// </koreansummary>
    public static class ModConfigAPI
    {
        public static string ModConfigName = "ModConfig";

        //Ensure this match the number of ModConfig.ModBehaviour.VERSION
        //这里确保版本号与ModConfig.ModBehaviour.VERSION匹配
        //여기서 ModConfig.ModBehaviour.VERSION과 버전 번호가 일치하는지 확인하십시오
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
        /// </summary>
        /// <koreansummary>
        /// 버전 호환성 확인
        /// </koreansummary>
        private static bool CheckVersionCompatibility()
        {
            if (versionChecked)
                return isVersionCompatible;

            try
            {
                // 尝试获取 ModConfig 的版本号
                // Try to get ModConfig version number
                FieldInfo versionField = modBehaviourType.GetField(
                    "VERSION",
                    BindingFlags.Public | BindingFlags.Static
                );
                if (versionField != null && versionField.FieldType == typeof(int))
                {
                    int modConfigVersion = (int)versionField.GetValue(null);
                    isVersionCompatible = (modConfigVersion == ModConfigVersion);

                    if (!isVersionCompatible)
                    {
                        // Debug.LogError(
                        //     $"[{TAG}] 版本不匹配！API版本: {ModConfigVersion}, ModConfig版本: {modConfigVersion}"
                        // );
                        Debug.LogError(
                            $"[{TAG}] ModConfig 버전이 일치하지 않습니다! API 버전: {ModConfigVersion}, ModConfig 버전: {modConfigVersion}"
                        );
                        return false;
                    }

                    // Debug.Log($"[{TAG}] 版本检查通过: {ModConfigVersion}");
                    Debug.Log($"[{TAG}] 버전 검사 통과: {ModConfigVersion}");
                    versionChecked = true;
                    return true;
                }
                else
                {
                    // 如果找不到版本字段，发出警告但继续运行（向后兼容）
                    // If version field not found, warn but continue (backward compatibility)
                    // Debug.LogWarning($"[{TAG}] 未找到版本信息字段，跳过版本检查");
                    Debug.LogWarning($"[{TAG}] 버전 정보 필드를 찾을 수 없어 버전 검사를 건너뜁니다");
                    isVersionCompatible = true;
                    versionChecked = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[{TAG}] 版本检查失败: {ex.Message}");
                Debug.LogError($"[{TAG}] 버전 검사 실패: {ex.Message}");
                isVersionCompatible = false;
                versionChecked = true;
                return false;
            }
        }

        /// <summary>
        /// 初始化 ModConfigAPI，检查必要的函数是否存在
        /// Initialize ModConfigAPI, check if necessary functions exist
        /// </summary>
        /// <koreansummary>
        /// ModConfigAPI 초기화, 필요한 함수가 존재하는지 확인
        /// </koreansummary>
        public static bool Initialize()
        {
            try
            {
                if (isInitialized)
                    return true;

                // 获取 ModBehaviour 类型
                // Get ModBehaviour type
                // ModBehaviour 유형 가져오기
                modBehaviourType = FindTypeInAssemblies("ModConfig.ModBehaviour");
                if (modBehaviourType == null)
                {
                    // Debug.LogWarning(
                    //     $"[{TAG}] ModConfig.ModBehaviour 类型未找到，ModConfig 可能未加载"
                    // );
                    Debug.LogWarning(
                        $"[{TAG}] ModConfig.ModBehaviour 유형을 찾을 수 없습니다. ModConfig가 로드되지 않았을 수 있습니다."
                    );
                    return false;
                }

                // 获取 OptionsManager_Mod 类型
                // Get OptionsManager_Mod type
                // OptionsManager_Mod 유형 가져오기
                optionsManagerType = FindTypeInAssemblies("ModConfig.OptionsManager_Mod");
                if (optionsManagerType == null)
                {
                    // Debug.LogWarning($"[{TAG}] ModConfig.OptionsManager_Mod 类型未找到");
                    Debug.LogWarning($"[{TAG}] ModConfig.OptionsManager_Mod 유형을 찾을 수 없습니다.");
                    return false;
                }

                // 检查版本兼容性
                // Check version compatibility
                // 버전 호환성 확인
                if (!CheckVersionCompatibility())
                {
                    // Debug.LogWarning($"[{TAG}] ModConfig version mismatch!!!");
                    Debug.LogWarning($"[{TAG}] ModConfig 버전 불일치!!!");
                    return false;
                }

                // 检查必要的静态方法是否存在
                // Check if necessary static methods exist
                // 필요한 정적 메서드가 존재하는지 확인
                string[] requiredMethods =
                {
                    "AddDropdownList",
                    "AddInputWithSlider",
                    "AddBoolDropdownList",
                    "AddOnOptionsChangedDelegate",
                    "RemoveOnOptionsChangedDelegate",
                };

                foreach (string methodName in requiredMethods)
                {
                    MethodInfo method = modBehaviourType.GetMethod(
                        methodName,
                        BindingFlags.Public | BindingFlags.Static
                    );
                    if (method == null)
                    {
                        // Debug.LogError($"[{TAG}] 必要方法 {methodName} 未找到");
                        Debug.LogError($"[{TAG}] 필요한 메서드 {methodName}를 찾을 수 없습니다.");
                        return false;
                    }
                }

                isInitialized = true;
                // Debug.Log($"[{TAG}] ModConfigAPI 初始化成功");
                Debug.Log($"[{TAG}] ModConfigAPI 초기화 성공");
                return true;
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[{TAG}] 初始化失败: {ex.Message}");
                Debug.LogError($"[{TAG}] 초기화 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 在所有已加载的程序集中查找类型
        /// </summary>
        /// <koreansummary>
        /// 모든 로드된 어셈블리에서 유형 찾기
        /// </koreansummary>
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
                        // Check if assembly name contains ModConfig
                        // ModConfig 어셈블리 이름이 포함되어 있는지 확인
                        if (assembly.FullName.Contains("ModConfig"))
                        {
                            // Debug.Log($"[{TAG}] 找到 ModConfig 相关程序集: {assembly.FullName}");
                            Debug.Log($"[{TAG}] ModConfig 관련 어셈블리 찾음: {assembly.FullName}");
                        }

                        // 尝试在该程序集中查找类型
                        // Try to find the type in that assembly
                        // 해당 어셈블리에서 유형 찾기 시도
                        Type type = assembly.GetType(typeName);
                        if (type != null)
                        {
                            // Debug.Log(
                            //     $"[{TAG}] 在程序集 {assembly.FullName} 中找到类型 {typeName}"
                            // );
                            Debug.Log(
                                $"[{TAG}] 어셈블리 {assembly.FullName}에서 유형 {typeName} 찾음"
                            );
                            return type;
                        }
                    }
                    catch (Exception)
                    {
                        // 忽略单个程序集的查找错误
                        // Ignore find errors for individual assemblies
                        // 개별 어셈블리의 찾기 오류 무시
                        continue;
                    }
                }

                // 记录所有已加载的程序集用于调试
                // Log all loaded assemblies for debugging
                // 디버깅을 위해 로드된 모든 어셈블리 기록
                // Debug.LogWarning(
                //     $"[{TAG}] 在所有程序集中未找到类型 {typeName}，已加载程序集数量: {assemblies.Length}"
                // );
                Debug.LogWarning(
                    $"[{TAG}] 모든 어셈블리에서 유형 {typeName}을(를) 찾을 수 없습니다. 로드된 어셈블리 수: {assemblies.Length}"
                );
                foreach (var assembly in assemblies.Where(a => a.FullName.Contains("ModConfig")))
                {
                    // Debug.Log($"[{TAG}] ModConfig 相关程序集: {assembly.FullName}");
                    Debug.Log($"[{TAG}] ModConfig 관련 어셈블리: {assembly.FullName}");
                }

                return null;
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[{TAG}] 程序集扫描失败: {ex.Message}");
                Debug.LogError($"[{TAG}] 어셈블리 스캔 실패: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 安全地添加选项变更事件委托
        /// Safely add options changed event delegate
        /// </summary>
        /// <param name="action">事件处理委托，参数为变更的选项键名</param>
        /// <returns>是否成功添加</returns>
        /// <koreansummary>
        /// 옵션 변경 이벤트 대리자 안전하게 추가
        /// </koreansummary>
        public static bool SafeAddOnOptionsChangedDelegate(Action<string> action)
        {
            if (!Initialize())
                return false;

            if (action == null)
            {
                // Debug.LogWarning($"[{TAG}] 不能添加空的事件委托");
                Debug.LogWarning($"[{TAG}] 빈 이벤트 대리자를 추가할 수 없습니다.");
                return false;
            }

            try
            {
                MethodInfo method = modBehaviourType.GetMethod(
                    "AddOnOptionsChangedDelegate",
                    BindingFlags.Public | BindingFlags.Static
                );
                method.Invoke(null, new object[] { action });

                // Debug.Log($"[{TAG}] 成功添加选项变更事件委托");
                Debug.Log($"[{TAG}] 옵션 변경 이벤트 대리자 성공적으로 추가됨");
                return true;
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[{TAG}] 添加选项变更事件委托失败: {ex.Message}");
                Debug.LogError($"[{TAG}] 옵션 변경 이벤트 대리자 추가 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地移除选项变更事件委托
        /// Safely remove options changed event delegate
        /// </summary>
        /// <param name="action">要移除的事件处理委托</param>
        /// <returns>是否成功移除</returns>
        /// <koreansummary>
        /// 옵션 변경 이벤트 대리자 안전하게 제거
        /// </koreansummary>
        public static bool SafeRemoveOnOptionsChangedDelegate(Action<string> action)
        {
            if (!Initialize())
                return false;

            if (action == null)
            {
                // Debug.LogWarning($"[{TAG}] 不能移除空的事件委托");
                Debug.LogWarning($"[{TAG}] 빈 이벤트 대리자를 제거할 수 없습니다.");
                return false;
            }

            try
            {
                MethodInfo method = modBehaviourType.GetMethod(
                    "RemoveOnOptionsChangedDelegate",
                    BindingFlags.Public | BindingFlags.Static
                );
                method.Invoke(null, new object[] { action });

                // Debug.Log($"[{TAG}] 成功移除选项变更事件委托");
                Debug.Log($"[{TAG}] 옵션 변경 이벤트 대리자 성공적으로 제거됨");
                return true;
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[{TAG}] 移除选项变更事件委托失败: {ex.Message}");
                Debug.LogError($"[{TAG}] 옵션 변경 이벤트 대리자 제거 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地添加下拉列表配置项
        /// Safely add dropdown list configuration item
        /// </summary>
        /// <koreansummary>
        /// 드롭다운 목록 구성 항목을 안전하게 추가
        /// </koreansummary>
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
                return false;

            try
            {
                MethodInfo method = modBehaviourType.GetMethod(
                    "AddDropdownList",
                    BindingFlags.Public | BindingFlags.Static
                );
                method.Invoke(
                    null,
                    new object[] { modName, key, description, options, valueType, defaultValue }
                );

                // Debug.Log($"[{TAG}] 成功添加下拉列表: {modName}.{key}");
                Debug.Log($"[{TAG}] 드롭다운 목록 성공적으로 추가됨: {modName}.{key}");
                return true;
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[{TAG}] 添加下拉列表失败 {modName}.{key}: {ex.Message}");
                Debug.LogError($"[{TAG}] 드롭다운 목록 추가 실패 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地添加带滑条的输入框配置项
        /// Safely add input box with slider configuration item
        /// </summary>
        /// <koreansummary>
        /// 슬라이더가 있는 입력 상자 구성 항목을 안전하게 추가
        /// </koreansummary>
        public static bool SafeAddInputWithSlider(
            string modName,
            string key,
            string description,
            Type valueType,
            object defaultValue,
            UnityEngine.Vector2? sliderRange = null
        )
        {
            key = $"{modName}_{key}";

            if (!Initialize())
                return false;

            try
            {
                MethodInfo method = modBehaviourType.GetMethod(
                    "AddInputWithSlider",
                    BindingFlags.Public | BindingFlags.Static
                );

                // 处理可空参数
                // Handle nullable parameters
                // null 파라미터 처리
                object[] parameters = sliderRange.HasValue
                    ? new object[]
                    {
                        modName,
                        key,
                        description,
                        valueType,
                        defaultValue,
                        sliderRange.Value,
                    }
                    : new object[] { modName, key, description, valueType, defaultValue, null };

                method.Invoke(null, parameters);

                // Debug.Log($"[{TAG}] 成功添加滑条输入框: {modName}.{key}");
                Debug.Log($"[{TAG}] 슬라이더 입력 상자 성공적으로 추가됨: {modName}.{key}");
                return true;
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[{TAG}] 添加滑条输入框失败 {modName}.{key}: {ex.Message}");
                Debug.LogError($"[{TAG}] 슬라이더 입력 상자 추가 실패 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地添加布尔下拉列表配置项
        /// Safely add boolean dropdown list configuration item
        /// </summary>
        /// <koreansummary>
        /// 부울 드롭다운 목록 구성 항목을 안전하게 추가
        /// </koreansummary>
        public static bool SafeAddBoolDropdownList(
            string modName,
            string key,
            string description,
            bool defaultValue
        )
        {
            key = $"{modName}_{key}";

            if (!Initialize())
                return false;

            try
            {
                MethodInfo method = modBehaviourType.GetMethod(
                    "AddBoolDropdownList",
                    BindingFlags.Public | BindingFlags.Static
                );
                method.Invoke(null, new object[] { modName, key, description, defaultValue });

                // Debug.Log($"[{TAG}] 成功添加布尔下拉列表: {modName}.{key}");
                Debug.Log($"[{TAG}] 부울 드롭다운 목록 성공적으로 추가됨: {modName}.{key}");
                return true;
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[{TAG}] 添加布尔下拉列表失败 {modName}.{key}: {ex.Message}");
                Debug.LogError($"[{TAG}] 부울 드롭다운 목록 추가 실패 {modName}.{key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 安全地加载配置值
        /// Safely load configuration value
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>加载的值或默认值</returns>
        /// <koreansummary>
        /// 구성 값 안전하게 로드
        /// </koreansummary>
        public static T SafeLoad<T>(string mod_name, string key, T defaultValue = default(T))
        {
            key = $"{mod_name}_{key}";

            if (!Initialize())
                return defaultValue;

            if (string.IsNullOrEmpty(key))
            {
                // Debug.LogWarning($"[{TAG}] 配置键不能为空");
                Debug.LogWarning($"[{TAG}] 구성 키는 비어 있을 수 없습니다.");
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
                    // Debug.LogError($"[{TAG}] 未找到 OptionsManager_Mod.Load 方法");
                    Debug.LogError($"[{TAG}] OptionsManager_Mod.Load 메서드를 찾을 수 없습니다.");
                    return defaultValue;
                }

                // 获取泛型方法
                // Get generic method
                // 제네릭 메서드 가져오기
                MethodInfo genericLoadMethod = loadMethod.MakeGenericMethod(typeof(T));
                object result = genericLoadMethod.Invoke(null, new object[] { key, defaultValue });

                // Debug.Log($"[{TAG}] 成功加载配置: {key} = {result}");
                Debug.Log($"[{TAG}] 구성 성공적으로 로드됨: {key} = {result}");
                return (T)result;
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[{TAG}] 加载配置失败 {key}: {ex.Message}");
                Debug.LogError($"[{TAG}] 구성 로드 실패 {key}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 安全地保存配置值
        /// Safely save configuration value
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">配置键</param>
        /// <param name="value">要保存的值</param>
        /// <returns>是否保存成功</returns>
        /// <koreansummary>
        /// 구성 값 안전하게 저장
        /// </koreansummary>
        public static bool SafeSave<T>(string mod_name, string key, T value)
        {
            key = $"{mod_name}_{key}";

            if (!Initialize())
                return false;

            if (string.IsNullOrEmpty(key))
            {
                // Debug.LogWarning($"[{TAG}] 配置键不能为空");
                Debug.LogWarning($"[{TAG}] 구성 키는 비어 있을 수 없습니다.");
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
                    // Debug.LogError($"[{TAG}] 未找到 OptionsManager_Mod.Save 方法");
                    Debug.LogError($"[{TAG}] OptionsManager_Mod.Save 메서드를 찾을 수 없습니다.");
                    return false;
                }

                // 获取泛型方法
                // Get generic method

                MethodInfo genericSaveMethod = saveMethod.MakeGenericMethod(typeof(T));
                genericSaveMethod.Invoke(null, new object[] { key, value });

                // Debug.Log($"[{TAG}] 成功保存配置: {key} = {value}");
                Debug.Log($"[{TAG}] 구성 성공적으로 저장됨: {key} = {value}");
                return true;
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[{TAG}] 保存配置失败 {key}: {ex.Message}");
                Debug.LogError($"[{TAG}] 구성 저장 실패 {key}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查 ModConfig 是否可用
        /// Check if ModConfig is available
        /// </summary>
        /// <koreansummary>
        /// ModConfig 사용 가능 여부 확인
        /// </koreansummary>
        public static bool IsAvailable()
        {
            return Initialize();
        }

        /// <summary>
        /// 获取 ModConfig 版本信息（如果存在）
        /// Get ModConfig version information (if exists)
        /// </summary>
        /// <koreansummary>
        /// ModConfig 버전 정보 가져오기(있는 경우)
        /// </koreansummary>
        public static string GetVersionInfo()
        {
            if (!Initialize())
                return "ModConfig 未加载 | ModConfig not loaded";

            try
            {
                // 尝试获取版本信息（如果 ModBehaviour 有相关字段或属性）
                // Try to get version information (if ModBehaviour has related fields or properties)
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
        /// </summary>
        /// <koreansummary>
        /// 버전 호환성 확인
        /// </koreansummary>
        public static bool IsVersionCompatible()
        {
            if (!Initialize())
                return false;
            return isVersionCompatible;
        }
    }
}
