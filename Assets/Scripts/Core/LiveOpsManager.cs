using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RobotTD.LiveOps
{
    /// <summary>
    /// Manages live operations including remote configuration, A/B testing, and feature flags.
    /// Allows dynamic content updates and experimentation without app updates.
    /// </summary>
    public class LiveOpsManager : MonoBehaviour
    {
        private static LiveOpsManager _instance;
        public static LiveOpsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("LiveOpsManager");
                    _instance = go.AddComponent<LiveOpsManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private bool enableLiveOps = true;
        [SerializeField] private float configRefreshInterval = 300f; // 5 minutes
        [SerializeField] private bool verboseLogging = true;

        [Header("Remote Configuration")]
        [SerializeField] private bool enableRemoteConfig = true;
        [SerializeField] private string remoteConfigUrl = "https://api.yourgame.com/config";

        [Header("A/B Testing")]
        [SerializeField] private bool enableABTesting = true;
        [SerializeField] private int maxActiveTests = 5;

        [Header("Feature Flags")]
        [SerializeField] private bool enableFeatureFlags = true;
        [SerializeField] private float featureFlagRefreshInterval = 60f; // 1 minute

        // Remote Configuration
        private Dictionary<string, RemoteConfigValue> _remoteConfig = new Dictionary<string, RemoteConfigValue>();
        private Dictionary<string, object> _defaultConfig = new Dictionary<string, object>();
        
        // A/B Testing
        private Dictionary<string, ABTest> _activeTests = new Dictionary<string, ABTest>();
        private Dictionary<string, string> _userTestAssignments = new Dictionary<string, string>();
        
        // Feature Flags
        private Dictionary<string, FeatureFlag> _featureFlags = new Dictionary<string, FeatureFlag>();
        
        // Dynamic Content
        private Dictionary<string, DynamicContent> _dynamicContent = new Dictionary<string, DynamicContent>();
        
        // State
        private bool _isInitialized = false;
        private DateTime _lastConfigRefresh;
        private DateTime _lastFeatureFlagRefresh;
        private string _userId;
        
        // Events
        public event Action<string, RemoteConfigValue> OnConfigValueChanged;
        public event Action<string, string> OnABTestAssigned;
        public event Action<string, bool> OnFeatureFlagChanged;
        public event Action<string> OnDynamicContentUpdated;
        public event Action OnLiveOpsRefreshed;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (enableLiveOps)
            {
                StartCoroutine(InitializeAsync());
            }
        }

        private IEnumerator InitializeAsync()
        {
            if (verboseLogging)
                Debug.Log("[LiveOps] Initializing Live Operations Manager...");

            yield return new WaitForSeconds(0.5f);

            // Generate or load user ID
            _userId = PlayerPrefs.GetString("UserId", Guid.NewGuid().ToString());
            PlayerPrefs.SetString("UserId", _userId);
            PlayerPrefs.Save();

            // Initialize default configuration
            InitializeDefaultConfig();

            // Load cached data
            LoadCachedData();

            // Fetch remote data
            if (enableRemoteConfig)
                yield return StartCoroutine(FetchRemoteConfigAsync());
            
            if (enableABTesting)
                yield return StartCoroutine(FetchABTestsAsync());
            
            if (enableFeatureFlags)
                yield return StartCoroutine(FetchFeatureFlagsAsync());

            _isInitialized = true;
            _lastConfigRefresh = DateTime.Now;
            _lastFeatureFlagRefresh = DateTime.Now;

            if (verboseLogging)
                Debug.Log($"[LiveOps] Initialized with {_remoteConfig.Count} configs, {_activeTests.Count} A/B tests, {_featureFlags.Count} feature flags");

            // Track initialization
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent(
                    RobotTD.Analytics.AnalyticsEvents.LIVEOPS_INITIALIZED,
                    new Dictionary<string, object>
                    {
                        { "config_count", _remoteConfig.Count },
                        { "ab_test_count", _activeTests.Count },
                        { "feature_flag_count", _featureFlags.Count }
                    }
                );
            }

            // Start auto-refresh coroutine
            StartCoroutine(AutoRefreshCoroutine());
        }

        private void InitializeDefaultConfig()
        {
            // Define default configuration values
            _defaultConfig["daily_reward_amount"] = 100;
            _defaultConfig["ad_cooldown_seconds"] = 30;
            _defaultConfig["max_lives"] = 5;
            _defaultConfig["energy_recharge_rate"] = 1;
            _defaultConfig["shop_discount_percentage"] = 0f;
            _defaultConfig["event_notification_enabled"] = true;
            _defaultConfig["maintenance_mode"] = false;
            _defaultConfig["min_supported_version"] = "1.0.0";
            _defaultConfig["max_tower_level"] = 10;
            _defaultConfig["pvp_enabled"] = true;
        }

        private IEnumerator AutoRefreshCoroutine()
        {
            while (enableLiveOps)
            {
                yield return new WaitForSeconds(configRefreshInterval);

                if (enableRemoteConfig)
                    yield return StartCoroutine(FetchRemoteConfigAsync());

                if (enableFeatureFlags && (DateTime.Now - _lastFeatureFlagRefresh).TotalSeconds >= featureFlagRefreshInterval)
                {
                    yield return StartCoroutine(FetchFeatureFlagsAsync());
                    _lastFeatureFlagRefresh = DateTime.Now;
                }

                OnLiveOpsRefreshed?.Invoke();
            }
        }

        #region Remote Configuration

        private IEnumerator FetchRemoteConfigAsync()
        {
            if (verboseLogging)
                Debug.Log("[LiveOps] Fetching remote configuration...");

            // Simulate network delay
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.3f));

            // In production, this would fetch from a real server
            // For now, simulate with some example configs
            SimulateRemoteConfig();

            SaveRemoteConfig();
            _lastConfigRefresh = DateTime.Now;

            if (verboseLogging)
                Debug.Log($"[LiveOps] Remote config fetched: {_remoteConfig.Count} values");
        }

        private void SimulateRemoteConfig()
        {
            // Simulate receiving config from server
            SetRemoteConfigValue("daily_reward_amount", 150, ConfigValueType.Int);
            SetRemoteConfigValue("ad_cooldown_seconds", 25, ConfigValueType.Int);
            SetRemoteConfigValue("shop_discount_percentage", 10.0f, ConfigValueType.Float);
            SetRemoteConfigValue("event_notification_enabled", true, ConfigValueType.Bool);
            SetRemoteConfigValue("maintenance_mode", false, ConfigValueType.Bool);
            SetRemoteConfigValue("welcome_message", "Welcome to Robot Tower Defense!", ConfigValueType.String);
            SetRemoteConfigValue("pvp_enabled", true, ConfigValueType.Bool);
        }

        private void SetRemoteConfigValue(string key, object value, ConfigValueType type)
        {
            var configValue = new RemoteConfigValue
            {
                key = key,
                value = value,
                valueType = type,
                lastUpdated = DateTime.Now
            };

            bool valueChanged = !_remoteConfig.ContainsKey(key) || 
                               !_remoteConfig[key].value.Equals(value);

            _remoteConfig[key] = configValue;

            if (valueChanged)
            {
                OnConfigValueChanged?.Invoke(key, configValue);
                
                if (verboseLogging)
                    Debug.Log($"[LiveOps] Config updated: {key} = {value}");
            }
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (_remoteConfig.ContainsKey(key) && _remoteConfig[key].valueType == ConfigValueType.Int)
                return (int)_remoteConfig[key].value;
            
            if (_defaultConfig.ContainsKey(key))
                return Convert.ToInt32(_defaultConfig[key]);
            
            return defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (_remoteConfig.ContainsKey(key) && _remoteConfig[key].valueType == ConfigValueType.Float)
                return (float)_remoteConfig[key].value;
            
            if (_defaultConfig.ContainsKey(key))
                return Convert.ToSingle(_defaultConfig[key]);
            
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_remoteConfig.ContainsKey(key) && _remoteConfig[key].valueType == ConfigValueType.Bool)
                return (bool)_remoteConfig[key].value;
            
            if (_defaultConfig.ContainsKey(key))
                return Convert.ToBoolean(_defaultConfig[key]);
            
            return defaultValue;
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (_remoteConfig.ContainsKey(key) && _remoteConfig[key].valueType == ConfigValueType.String)
                return (string)_remoteConfig[key].value;
            
            if (_defaultConfig.ContainsKey(key))
                return _defaultConfig[key].ToString();
            
            return defaultValue;
        }

        #endregion

        #region A/B Testing

        private IEnumerator FetchABTestsAsync()
        {
            if (verboseLogging)
                Debug.Log("[LiveOps] Fetching A/B tests...");

            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.3f));

            // Simulate A/B tests
            SimulateABTests();

            SaveABTests();

            if (verboseLogging)
                Debug.Log($"[LiveOps] A/B tests fetched: {_activeTests.Count} active tests");
        }

        private void SimulateABTests()
        {
            // Example test 1: Shop discount test
            CreateABTest("shop_discount_test", "Shop Discount Experiment", 
                new string[] { "control", "discount_10", "discount_20" },
                new float[] { 0.34f, 0.33f, 0.33f });

            // Example test 2: Tutorial flow test
            CreateABTest("tutorial_flow_test", "Tutorial Flow Optimization",
                new string[] { "control", "short_tutorial", "interactive_tutorial" },
                new float[] { 0.33f, 0.33f, 0.34f });

            // Example test 3: Reward amount test
            CreateABTest("reward_amount_test", "Daily Reward Amount Test",
                new string[] { "control", "increased_reward" },
                new float[] { 0.5f, 0.5f });
        }

        private void CreateABTest(string testId, string testName, string[] variants, float[] weights)
        {
            if (_activeTests.ContainsKey(testId))
                return;

            var test = new ABTest
            {
                testId = testId,
                testName = testName,
                variants = variants.ToList(),
                variantWeights = weights.ToList(),
                isActive = true,
                startDate = DateTime.Now,
                participantCount = 0
            };

            _activeTests[testId] = test;

            // Assign user to variant if not already assigned
            if (!_userTestAssignments.ContainsKey(testId))
            {
                string assignedVariant = AssignUserToVariant(test);
                _userTestAssignments[testId] = assignedVariant;
                
                if (RobotTD.Analytics.AnalyticsManager.Instance != null)
                {
                    RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent(
                        RobotTD.Analytics.AnalyticsEvents.LIVEOPS_AB_TEST_ASSIGNED,
                        new Dictionary<string, object>
                        {
                            { "test_id", testId },
                            { "test_name", testName },
                            { "variant", assignedVariant }
                        }
                    );
                }

                OnABTestAssigned?.Invoke(testId, assignedVariant);
            }
        }

        private string AssignUserToVariant(ABTest test)
        {
            // Use consistent hashing based on user ID for stable assignments
            int hash = (_userId + test.testId).GetHashCode();
            float random = (Mathf.Abs(hash) % 10000) / 10000f;

            float cumulativeWeight = 0f;
            for (int i = 0; i < test.variants.Count; i++)
            {
                cumulativeWeight += test.variantWeights[i];
                if (random <= cumulativeWeight)
                {
                    test.participantCount++;
                    return test.variants[i];
                }
            }

            return test.variants[0]; // Fallback
        }

        public string GetABTestVariant(string testId, string defaultVariant = "control")
        {
            if (_userTestAssignments.ContainsKey(testId))
                return _userTestAssignments[testId];
            
            if (_activeTests.ContainsKey(testId))
            {
                string variant = AssignUserToVariant(_activeTests[testId]);
                _userTestAssignments[testId] = variant;
                SaveABTests();
                return variant;
            }

            return defaultVariant;
        }

        public bool IsInABTestVariant(string testId, string variant)
        {
            return GetABTestVariant(testId) == variant;
        }

        public void TrackABTestEvent(string testId, string eventName, Dictionary<string, object> parameters = null)
        {
            if (!_activeTests.ContainsKey(testId))
                return;

            string variant = GetABTestVariant(testId);
            
            var eventParams = parameters ?? new Dictionary<string, object>();
            eventParams["ab_test_id"] = testId;
            eventParams["ab_test_variant"] = variant;
            eventParams["ab_test_event"] = eventName;

            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent(
                    RobotTD.Analytics.AnalyticsEvents.LIVEOPS_AB_TEST_EVENT,
                    eventParams
                );
            }
        }

        #endregion

        #region Feature Flags

        private IEnumerator FetchFeatureFlagsAsync()
        {
            if (verboseLogging)
                Debug.Log("[LiveOps] Fetching feature flags...");

            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.3f));

            // Simulate feature flags
            SimulateFeatureFlags();

            SaveFeatureFlags();

            if (verboseLogging)
                Debug.Log($"[LiveOps] Feature flags fetched: {_featureFlags.Count} flags");
        }

        private void SimulateFeatureFlags()
        {
            // Example feature flags
            CreateFeatureFlag("new_tower_unlocked", "New Artillery Tower", true, 1.0f);
            CreateFeatureFlag("clan_wars_enabled", "Clan Wars Feature", true, 1.0f);
            CreateFeatureFlag("seasonal_events", "Seasonal Events", true, 1.0f);
            CreateFeatureFlag("pvp_ranked_mode", "PvP Ranked Mode", true, 0.5f); // 50% rollout
            CreateFeatureFlag("new_map_editor", "Advanced Map Editor", false, 0.0f);
            CreateFeatureFlag("achievement_system_v2", "Achievement System V2", true, 0.25f); // 25% rollout
        }

        private void CreateFeatureFlag(string flagId, string flagName, bool enabled, float rolloutPercentage)
        {
            bool valueChanged = !_featureFlags.ContainsKey(flagId) || 
                               _featureFlags[flagId].enabled != enabled ||
                               _featureFlags[flagId].rolloutPercentage != rolloutPercentage;

            var flag = new FeatureFlag
            {
                flagId = flagId,
                flagName = flagName,
                enabled = enabled,
                rolloutPercentage = rolloutPercentage,
                lastUpdated = DateTime.Now
            };

            _featureFlags[flagId] = flag;

            if (valueChanged)
            {
                bool userHasAccess = IsFeatureEnabled(flagId);
                OnFeatureFlagChanged?.Invoke(flagId, userHasAccess);

                if (verboseLogging)
                    Debug.Log($"[LiveOps] Feature flag updated: {flagId} = {enabled} ({rolloutPercentage * 100}% rollout)");

                // Track flag change
                if (RobotTD.Analytics.AnalyticsManager.Instance != null)
                {
                    RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent(
                        RobotTD.Analytics.AnalyticsEvents.LIVEOPS_FEATURE_FLAG_TOGGLED,
                        new Dictionary<string, object>
                        {
                            { "flag_id", flagId },
                            { "flag_name", flagName },
                            { "enabled", enabled },
                            { "rollout_percentage", rolloutPercentage },
                            { "user_has_access", userHasAccess }
                        }
                    );
                }
            }
        }

        public bool IsFeatureEnabled(string flagId)
        {
            if (!_featureFlags.ContainsKey(flagId))
                return false;

            var flag = _featureFlags[flagId];
            if (!flag.enabled)
                return false;

            // Check rollout percentage
            if (flag.rolloutPercentage >= 1.0f)
                return true;

            // Use consistent hashing for stable rollout
            int hash = (_userId + flagId).GetHashCode();
            float userRollout = (Mathf.Abs(hash) % 10000) / 10000f;
            return userRollout <= flag.rolloutPercentage;
        }

        public void SetFeatureFlagOverride(string flagId, bool enabled)
        {
            string key = $"FeatureFlagOverride_{flagId}";
            PlayerPrefs.SetInt(key, enabled ? 1 : 0);
            PlayerPrefs.Save();

            if (verboseLogging)
                Debug.Log($"[LiveOps] Feature flag override set: {flagId} = {enabled}");
        }

        public void ClearFeatureFlagOverride(string flagId)
        {
            string key = $"FeatureFlagOverride_{flagId}";
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();

            if (verboseLogging)
                Debug.Log($"[LiveOps] Feature flag override cleared: {flagId}");
        }

        private bool HasFeatureFlagOverride(string flagId, out bool overrideValue)
        {
            string key = $"FeatureFlagOverride_{flagId}";
            if (PlayerPrefs.HasKey(key))
            {
                overrideValue = PlayerPrefs.GetInt(key) == 1;
                return true;
            }
            overrideValue = false;
            return false;
        }

        #endregion

        #region Dynamic Content

        public void UpdateDynamicContent(string contentId, string contentName, string contentData)
        {
            var content = new DynamicContent
            {
                contentId = contentId,
                contentName = contentName,
                contentData = contentData,
                lastUpdated = DateTime.Now
            };

            _dynamicContent[contentId] = content;
            SaveDynamicContent();

            OnDynamicContentUpdated?.Invoke(contentId);

            if (verboseLogging)
                Debug.Log($"[LiveOps] Dynamic content updated: {contentId}");

            // Track content update
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent(
                    RobotTD.Analytics.AnalyticsEvents.LIVEOPS_CONTENT_UPDATED,
                    new Dictionary<string, object>
                    {
                        { "content_id", contentId },
                        { "content_name", contentName }
                    }
                );
            }
        }

        public DynamicContent GetDynamicContent(string contentId)
        {
            return _dynamicContent.ContainsKey(contentId) ? _dynamicContent[contentId] : null;
        }

        public List<DynamicContent> GetAllDynamicContent()
        {
            return _dynamicContent.Values.ToList();
        }

        #endregion

        #region Persistence

        private void LoadCachedData()
        {
            // Load remote config
            if (PlayerPrefs.HasKey("LiveOps_RemoteConfig"))
            {
                string json = PlayerPrefs.GetString("LiveOps_RemoteConfig");
                var data = JsonUtility.FromJson<RemoteConfigData>(json);
                if (data != null && data.configs != null)
                {
                    foreach (var config in data.configs)
                    {
                        _remoteConfig[config.key] = config;
                    }
                }
            }

            // Load A/B tests
            if (PlayerPrefs.HasKey("LiveOps_ABTests"))
            {
                string json = PlayerPrefs.GetString("LiveOps_ABTests");
                var data = JsonUtility.FromJson<ABTestData>(json);
                if (data != null && data.tests != null)
                {
                    foreach (var test in data.tests)
                    {
                        _activeTests[test.testId] = test;
                    }
                }
            }

            // Load test assignments
            if (PlayerPrefs.HasKey("LiveOps_TestAssignments"))
            {
                string json = PlayerPrefs.GetString("LiveOps_TestAssignments");
                var data = JsonUtility.FromJson<TestAssignmentData>(json);
                if (data != null && data.assignments != null)
                {
                    foreach (var assignment in data.assignments)
                    {
                        _userTestAssignments[assignment.testId] = assignment.variant;
                    }
                }
            }

            // Load feature flags
            if (PlayerPrefs.HasKey("LiveOps_FeatureFlags"))
            {
                string json = PlayerPrefs.GetString("LiveOps_FeatureFlags");
                var data = JsonUtility.FromJson<FeatureFlagData>(json);
                if (data != null && data.flags != null)
                {
                    foreach (var flag in data.flags)
                    {
                        _featureFlags[flag.flagId] = flag;
                    }
                }
            }

            // Load dynamic content
            if (PlayerPrefs.HasKey("LiveOps_DynamicContent"))
            {
                string json = PlayerPrefs.GetString("LiveOps_DynamicContent");
                var data = JsonUtility.FromJson<DynamicContentData>(json);
                if (data != null && data.contents != null)
                {
                    foreach (var content in data.contents)
                    {
                        _dynamicContent[content.contentId] = content;
                    }
                }
            }
        }

        private void SaveRemoteConfig()
        {
            var data = new RemoteConfigData
            {
                configs = _remoteConfig.Values.ToList()
            };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("LiveOps_RemoteConfig", json);
            PlayerPrefs.Save();
        }

        private void SaveABTests()
        {
            var testData = new ABTestData
            {
                tests = _activeTests.Values.ToList()
            };
            string testJson = JsonUtility.ToJson(testData);
            PlayerPrefs.SetString("LiveOps_ABTests", testJson);

            var assignmentData = new TestAssignmentData
            {
                assignments = _userTestAssignments.Select(kvp => new TestAssignment
                {
                    testId = kvp.Key,
                    variant = kvp.Value
                }).ToList()
            };
            string assignmentJson = JsonUtility.ToJson(assignmentData);
            PlayerPrefs.SetString("LiveOps_TestAssignments", assignmentJson);
            PlayerPrefs.Save();
        }

        private void SaveFeatureFlags()
        {
            var data = new FeatureFlagData
            {
                flags = _featureFlags.Values.ToList()
            };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("LiveOps_FeatureFlags", json);
            PlayerPrefs.Save();
        }

        private void SaveDynamicContent()
        {
            var data = new DynamicContentData
            {
                contents = _dynamicContent.Values.ToList()
            };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("LiveOps_DynamicContent", json);
            PlayerPrefs.Save();
        }

        #endregion

        #region Utility Methods

        public bool IsInitialized => _isInitialized;

        public void ForceRefresh()
        {
            if (!_isInitialized)
                return;

            StartCoroutine(ForceRefreshAsync());
        }

        private IEnumerator ForceRefreshAsync()
        {
            if (enableRemoteConfig)
                yield return StartCoroutine(FetchRemoteConfigAsync());
            
            if (enableABTesting)
                yield return StartCoroutine(FetchABTestsAsync());
            
            if (enableFeatureFlags)
                yield return StartCoroutine(FetchFeatureFlagsAsync());

            OnLiveOpsRefreshed?.Invoke();

            if (verboseLogging)
                Debug.Log("[LiveOps] Force refresh completed");

            // Track refresh
            if (RobotTD.Analytics.AnalyticsManager.Instance != null)
            {
                RobotTD.Analytics.AnalyticsManager.Instance.TrackEvent(
                    RobotTD.Analytics.AnalyticsEvents.LIVEOPS_REFRESHED,
                    new Dictionary<string, object>
                    {
                        { "config_count", _remoteConfig.Count },
                        { "ab_test_count", _activeTests.Count },
                        { "feature_flag_count", _featureFlags.Count }
                    }
                );
            }
        }

        public Dictionary<string, RemoteConfigValue> GetAllConfigs()
        {
            return new Dictionary<string, RemoteConfigValue>(_remoteConfig);
        }

        public Dictionary<string, ABTest> GetAllABTests()
        {
            return new Dictionary<string, ABTest>(_activeTests);
        }

        public Dictionary<string, FeatureFlag> GetAllFeatureFlags()
        {
            return new Dictionary<string, FeatureFlag>(_featureFlags);
        }

        public string GetUserId()
        {
            return _userId;
        }

        #endregion
    }

    #region Data Structures

    [Serializable]
    public enum ConfigValueType
    {
        Int,
        Float,
        Bool,
        String
    }

    [Serializable]
    public class RemoteConfigValue
    {
        public string key;
        public object value;
        public ConfigValueType valueType;
        public DateTime lastUpdated;
    }

    [Serializable]
    public class RemoteConfigData
    {
        public List<RemoteConfigValue> configs;
    }

    [Serializable]
    public class ABTest
    {
        public string testId;
        public string testName;
        public List<string> variants;
        public List<float> variantWeights;
        public bool isActive;
        public DateTime startDate;
        public int participantCount;
    }

    [Serializable]
    public class ABTestData
    {
        public List<ABTest> tests;
    }

    [Serializable]
    public class TestAssignment
    {
        public string testId;
        public string variant;
    }

    [Serializable]
    public class TestAssignmentData
    {
        public List<TestAssignment> assignments;
    }

    [Serializable]
    public class FeatureFlag
    {
        public string flagId;
        public string flagName;
        public bool enabled;
        public float rolloutPercentage;
        public DateTime lastUpdated;
    }

    [Serializable]
    public class FeatureFlagData
    {
        public List<FeatureFlag> flags;
    }

    [Serializable]
    public class DynamicContent
    {
        public string contentId;
        public string contentName;
        public string contentData;
        public DateTime lastUpdated;
    }

    [Serializable]
    public class DynamicContentData
    {
        public List<DynamicContent> contents;
    }

    #endregion
}
