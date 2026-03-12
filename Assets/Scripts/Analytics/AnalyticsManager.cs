using UnityEngine;
using System;
using System.Collections.Generic;

namespace RobotTD.Analytics
{
    /// <summary>
    /// Analytics and telemetry system for tracking player behavior, performance, and engagement.
    /// Supports multiple analytics backends (Unity Analytics, Firebase, custom backend).
    /// </summary>
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool sendInEditor = false;

        [Header("Session Tracking")]
        [SerializeField] private float sessionTimeoutMinutes = 5f;

        private string sessionId;
        private DateTime sessionStartTime;
        private DateTime lastActivityTime;
        private int sessionNumber;
        private bool isNewUser;

        // Performance metrics
        private List<float> fpsHistory = new List<float>();
        private List<float> frameTimeHistory = new List<float>();
        private int crashCount;
        private int errorCount;

        // Progression metrics
        private int totalPlayTime;
        private int totalSessions;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAnalytics();
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void OnApplicationQuit()
        {
            EndSession();
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                EndSession();
            }
            else
            {
                // Check if session timed out
                TimeSpan timeSinceActivity = DateTime.Now - lastActivityTime;
                if (timeSinceActivity.TotalMinutes > sessionTimeoutMinutes)
                {
                    StartNewSession();
                }
                else
                {
                    lastActivityTime = DateTime.Now;
                }
            }
        }

        // ── Initialization ────────────────────────────────────────────────────

        private void InitializeAnalytics()
        {
            if (!enableAnalytics)
            {
                LogDebug("Analytics disabled");
                return;
            }

            // Check if first time user
            isNewUser = !PlayerPrefs.HasKey("AnalyticsInitialized");
            if (isNewUser)
            {
                PlayerPrefs.SetInt("AnalyticsInitialized", 1);
                PlayerPrefs.SetString("FirstLaunchDate", DateTime.Now.ToString("o"));
                PlayerPrefs.Save();
            }

            // Load session data
            sessionNumber = PlayerPrefs.GetInt("SessionNumber", 0);
            totalSessions = PlayerPrefs.GetInt("TotalSessions", 0);
            totalPlayTime = PlayerPrefs.GetInt("TotalPlayTime", 0);
            crashCount = PlayerPrefs.GetInt("CrashCount", 0);

            StartNewSession();

            LogDebug($"Analytics initialized - New User: {isNewUser}, Session: {sessionNumber}");
        }

        private void StartNewSession()
        {
            sessionId = System.Guid.NewGuid().ToString();
            sessionStartTime = DateTime.Now;
            lastActivityTime = DateTime.Now;
            sessionNumber++;
            totalSessions++;

            PlayerPrefs.SetInt("SessionNumber", sessionNumber);
            PlayerPrefs.SetInt("TotalSessions", totalSessions);
            PlayerPrefs.Save();

            // Track session start
            TrackEvent(AnalyticsEvents.SESSION_START, new Dictionary<string, object>
            {
                { "session_id", sessionId },
                { "session_number", sessionNumber },
                { "is_new_user", isNewUser },
                { "platform", Application.platform.ToString() },
                { "device_model", SystemInfo.deviceModel },
                { "os_version", SystemInfo.operatingSystem }
            });

            LogDebug($"New session started: {sessionId}");
        }

        private void EndSession()
        {
            if (string.IsNullOrEmpty(sessionId))
                return;

            TimeSpan sessionDuration = DateTime.Now - sessionStartTime;
            int sessionSeconds = (int)sessionDuration.TotalSeconds;

            totalPlayTime += sessionSeconds;
            PlayerPrefs.SetInt("TotalPlayTime", totalPlayTime);
            PlayerPrefs.Save();

            // Track session end
            TrackEvent(AnalyticsEvents.SESSION_END, new Dictionary<string, object>
            {
                { "session_id", sessionId },
                { "duration_seconds", sessionSeconds },
                { "total_play_time", totalPlayTime }
            });

            LogDebug($"Session ended: {sessionSeconds}s");
        }

        // ── Core Tracking Methods ─────────────────────────────────────────────

        /// <summary>
        /// Track a custom event with parameters.
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!enableAnalytics)
                return;

            if (!Application.isPlaying || (Application.isEditor && !sendInEditor))
            {
                LogDebug($"[MOCK] Event: {eventName}");
                return;
            }

            lastActivityTime = DateTime.Now;

            // Add common parameters
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            parameters["session_id"] = sessionId;
            parameters["timestamp"] = DateTime.Now.ToString("o");

            // Send to analytics backend
            SendEventToBackend(eventName, parameters);

            // Send to editor dashboard
            #if UNITY_EDITOR
            try
            {
                var dashboardType = System.Type.GetType("RobotTD.Editor.AnalyticsDashboard, Assembly-CSharp-Editor");
                if (dashboardType != null)
                {
                    var method = dashboardType.GetMethod("OnEventTracked", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    method?.Invoke(null, new object[] { eventName, parameters });
                }
            }
            catch { /* Dashboard not found or not open */ }
            #endif

            LogDebug($"Event tracked: {eventName}");
        }

        /// <summary>
        /// Track a custom event with a single value.
        /// </summary>
        public void TrackEvent(string eventName, string paramKey, object paramValue)
        {
            TrackEvent(eventName, new Dictionary<string, object> { { paramKey, paramValue } });
        }

        /// <summary>
        /// Track a simple event with no parameters.
        /// </summary>
        public void TrackEvent(string eventName)
        {
            TrackEvent(eventName, null);
        }

        // ── Gameplay Tracking ─────────────────────────────────────────────────

        public void TrackGameStart(string mapName, int difficulty, bool isTutorial)
        {
            TrackEvent(AnalyticsEvents.GAME_START, new Dictionary<string, object>
            {
                { "map_name", mapName },
                { "difficulty", difficulty },
                { "is_tutorial", isTutorial }
            });
        }

        public void TrackGameEnd(string result, int finalWave, int finalScore, int creditsEarned, float playTimeSeconds)
        {
            TrackEvent(AnalyticsEvents.GAME_END, new Dictionary<string, object>
            {
                { "result", result },  // "victory", "defeat", "quit"
                { "final_wave", finalWave },
                { "final_score", finalScore },
                { "credits_earned", creditsEarned },
                { "play_time_seconds", playTimeSeconds }
            });
        }

        public void TrackWaveComplete(int waveNumber, int enemiesKilled, int livesRemaining, int creditsEarned)
        {
            TrackEvent(AnalyticsEvents.WAVE_COMPLETE, new Dictionary<string, object>
            {
                { "wave_number", waveNumber },
                { "enemies_killed", enemiesKilled },
                { "lives_remaining", livesRemaining },
                { "credits_earned", creditsEarned }
            });
        }

        public void TrackTowerPlaced(string towerType, int towerLevel, int cost, Vector2 position)
        {
            TrackEvent(AnalyticsEvents.TOWER_PLACED, new Dictionary<string, object>
            {
                { "tower_type", towerType },
                { "tower_level", towerLevel },
                { "cost", cost },
                { "position_x", position.x },
                { "position_y", position.y }
            });
        }

        public void TrackTowerUpgraded(string towerType, int oldLevel, int newLevel, int cost)
        {
            TrackEvent(AnalyticsEvents.TOWER_UPGRADED, new Dictionary<string, object>
            {
                { "tower_type", towerType },
                { "old_level", oldLevel },
                { "new_level", newLevel },
                { "cost", cost }
            });
        }

        public void TrackTowerSold(string towerType, int level, int refund)
        {
            TrackEvent(AnalyticsEvents.TOWER_SOLD, new Dictionary<string, object>
            {
                { "tower_type", towerType },
                { "level", level },
                { "refund", refund }
            });
        }

        // ── Progression Tracking ──────────────────────────────────────────────

        public void TrackAchievementUnlock(string achievementId, string achievementName)
        {
            TrackEvent(AnalyticsEvents.ACHIEVEMENT_UNLOCKED, new Dictionary<string, object>
            {
                { "achievement_id", achievementId },
                { "achievement_name", achievementName },
                { "session_number", sessionNumber }
            });
        }

        public void TrackTechUpgrade(string upgradeName, int newLevel, int cost)
        {
            TrackEvent(AnalyticsEvents.TECH_UPGRADED, new Dictionary<string, object>
            {
                { "upgrade_name", upgradeName },
                { "new_level", newLevel },
                { "cost", cost }
            });
        }

        public void TrackTutorialStep(int stepNumber, string stepName, bool completed)
        {
            TrackEvent(AnalyticsEvents.TUTORIAL_STEP, new Dictionary<string, object>
            {
                { "step_number", stepNumber },
                { "step_name", stepName },
                { "completed", completed }
            });
        }

        public void TrackTutorialComplete(float completionTime)
        {
            TrackEvent(AnalyticsEvents.TUTORIAL_COMPLETE, new Dictionary<string, object>
            {
                { "completion_time_seconds", completionTime }
            });
        }

        // ── Monetization Tracking (for future IAP) ────────────────────────────

        public void TrackPurchaseInitiated(string productId, decimal price, string currency)
        {
            TrackEvent(AnalyticsEvents.PURCHASE_INITIATED, new Dictionary<string, object>
            {
                { "product_id", productId },
                { "price", price },
                { "currency", currency }
            });
        }

        public void TrackPurchaseComplete(string productId, decimal price, string currency, string transactionId)
        {
            TrackEvent(AnalyticsEvents.PURCHASE_COMPLETE, new Dictionary<string, object>
            {
                { "product_id", productId },
                { "price", price },
                { "currency", currency },
                { "transaction_id", transactionId }
            });
        }

        public void TrackPurchaseFailed(string productId, string reason)
        {
            TrackEvent(AnalyticsEvents.PURCHASE_FAILED, new Dictionary<string, object>
            {
                { "product_id", productId },
                { "reason", reason }
            });
        }

        // ── Performance Tracking ──────────────────────────────────────────────

        public void TrackPerformanceMetrics(float avgFPS, float minFPS, float avgFrameTime, float memoryMB)
        {
            TrackEvent(AnalyticsEvents.PERFORMANCE_SAMPLE, new Dictionary<string, object>
            {
                { "avg_fps", avgFPS },
                { "min_fps", minFPS },
                { "avg_frame_time_ms", avgFrameTime },
                { "memory_mb", memoryMB },
                { "quality_level", QualitySettings.GetQualityLevel() }
            });
        }

        public void TrackQualityChange(string oldPreset, string newPreset, string reason)
        {
            TrackEvent(AnalyticsEvents.QUALITY_CHANGED, new Dictionary<string, object>
            {
                { "old_preset", oldPreset },
                { "new_preset", newPreset },
                { "reason", reason }  // "manual", "auto", "battery"
            });
        }

        public void TrackBatterySaveActivated(float batteryLevel)
        {
            TrackEvent(AnalyticsEvents.BATTERY_SAVE_ACTIVATED, new Dictionary<string, object>
            {
                { "battery_level", batteryLevel }
            });
        }

        // ── Error Tracking ────────────────────────────────────────────────────

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (!enableAnalytics)
                return;

            if (type == LogType.Error || type == LogType.Exception)
            {
                errorCount++;

                TrackEvent(AnalyticsEvents.ERROR_LOGGED, new Dictionary<string, object>
                {
                    { "error_type", type.ToString() },
                    { "error_message", logString },
                    { "stack_trace", stackTrace },
                    { "error_count", errorCount }
                });
            }
        }

        public void TrackCrash(string crashMessage, string stackTrace)
        {
            crashCount++;
            PlayerPrefs.SetInt("CrashCount", crashCount);
            PlayerPrefs.Save();

            TrackEvent(AnalyticsEvents.CRASH, new Dictionary<string, object>
            {
                { "crash_message", crashMessage },
                { "stack_trace", stackTrace },
                { "crash_count", crashCount }
            });
        }

        // ── User Properties ───────────────────────────────────────────────────

        public void SetUserProperty(string propertyName, string value)
        {
            if (!enableAnalytics)
                return;

            // Store user properties for analytics backend
            PlayerPrefs.SetString($"UserProperty_{propertyName}", value);
            PlayerPrefs.Save();

            LogDebug($"User property set: {propertyName} = {value}");
        }

        public void SetUserProperty(string propertyName, int value)
        {
            if (!enableAnalytics)
                return;

            PlayerPrefs.SetInt($"UserProperty_{propertyName}", value);
            PlayerPrefs.Save();

            LogDebug($"User property set: {propertyName} = {value}");
        }

        // ── Backend Integration ───────────────────────────────────────────────

        private void SendEventToBackend(string eventName, Dictionary<string, object> parameters)
        {
            // TODO: Implement actual analytics backend integration
            // Examples:
            // - Unity Analytics: Analytics.CustomEvent(eventName, parameters);
            // - Firebase: FirebaseAnalytics.LogEvent(eventName, parameters);
            // - Custom backend: Send HTTP request to your server

            #if UNITY_ANALYTICS
            // Unity Analytics integration
            // Analytics.CustomEvent(eventName, parameters);
            #endif

            #if FIREBASE_ANALYTICS
            // Firebase Analytics integration
            // FirebaseAnalytics.LogEvent(eventName, ConvertToFirebaseParameters(parameters));
            #endif

            // For now, just log to console in debug mode
            if (enableDebugLogs)
            {
                string paramsStr = parameters != null ? string.Join(", ", parameters) : "none";
                Debug.Log($"[Analytics] {eventName} | {paramsStr}");
            }
        }

        // ── Helper Methods ────────────────────────────────────────────────────

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[AnalyticsManager] {message}");
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        public string GetSessionId() => sessionId;
        public int GetSessionNumber() => sessionNumber;
        public int GetTotalSessions() => totalSessions;
        public int GetTotalPlayTimeSeconds() => totalPlayTime;
        public bool IsNewUser() => isNewUser;
        public int GetSessionDurationSeconds() => (int)(DateTime.Now - sessionStartTime).TotalSeconds;

        public Dictionary<string, object> GetSessionInfo()
        {
            return new Dictionary<string, object>
            {
                { "session_id", sessionId },
                { "session_number", sessionNumber },
                { "total_sessions", totalSessions },
                { "total_play_time", totalPlayTime },
                { "is_new_user", isNewUser },
                { "session_duration", GetSessionDurationSeconds() }
            };
        }

        // ── Debug Tools ───────────────────────────────────────────────────────

        [ContextMenu("Print Session Info")]
        private void PrintSessionInfo()
        {
            Debug.Log($"Session ID: {sessionId}");
            Debug.Log($"Session Number: {sessionNumber}");
            Debug.Log($"Total Sessions: {totalSessions}");
            Debug.Log($"Total Play Time: {totalPlayTime}s");
            Debug.Log($"Is New User: {isNewUser}");
            Debug.Log($"Session Duration: {GetSessionDurationSeconds()}s");
        }

        [ContextMenu("Reset Analytics Data")]
        private void ResetAnalyticsData()
        {
            PlayerPrefs.DeleteKey("AnalyticsInitialized");
            PlayerPrefs.DeleteKey("SessionNumber");
            PlayerPrefs.DeleteKey("TotalSessions");
            PlayerPrefs.DeleteKey("TotalPlayTime");
            PlayerPrefs.DeleteKey("CrashCount");
            PlayerPrefs.DeleteKey("FirstLaunchDate");
            PlayerPrefs.Save();
            Debug.Log("Analytics data reset");
        }
    }
}
