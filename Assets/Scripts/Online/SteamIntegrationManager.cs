using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RobotTD.Progression;
using RobotTD.Analytics;

namespace RobotTD.Online
{
    /// <summary>
    /// Steam platform integration manager for achievements, leaderboards, and stats.
    /// Requires Steamworks.NET SDK for production builds.
    /// Provides simulation mode for editor testing without Steam SDK.
    /// 
    /// Setup Instructions:
    /// 1. Install Steamworks.NET from Unity Asset Store or GitHub
    /// 2. Configure steam_appid.txt with your Steam App ID
    /// 3. Define STEAM_ENABLED in Player Settings > Scripting Define Symbols
    /// 4. Map achievement IDs in Steam Partner dashboard to match AchievementId enum
    /// </summary>
    public class SteamIntegrationManager : MonoBehaviour
    {
        public static SteamIntegrationManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableSteamIntegration = true;
        [SerializeField] private bool verboseLogging = true;
        [SerializeField] private bool autoSyncAchievements = true;
        [SerializeField] private bool autoSyncStats = true;
        [SerializeField] private float statsUpdateInterval = 30f; // seconds

        [Header("Leaderboard Names")]
        [SerializeField] private string highScoreLeaderboard = "HighScores";
        [SerializeField] private string wavesLeaderboard = "HighestWave";
        [SerializeField] private string killsLeaderboard = "MostKills";

        // State
        private bool isInitialized = false;
        private bool isSteamRunning = false;
        private string steamUserId = "";
        private string steamUsername = "";
        private float statsUpdateTimer = 0f;

        // Achievement sync tracking
        private HashSet<AchievementId> syncedAchievements = new HashSet<AchievementId>();
        private Dictionary<string, int> cachedStats = new Dictionary<string, int>();

        // Events
        public event Action<bool> OnSteamInitialized; // success
        public event Action<AchievementId> OnAchievementUnlocked;
        public event Action<string, int> OnStatUpdated; // statName, value
        public event Action<string, bool, int, int> OnLeaderboardScoreUploaded; // leaderboard, success, score, rank
        public event Action<string> OnSteamError;

        // ══════════════════════════════════════════════════════════════════════
        // ── Unity Lifecycle ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!enableSteamIntegration)
            {
                LogDebug("Steam integration disabled");
                return;
            }

            StartCoroutine(InitializeSteam());
        }

        private void Update()
        {
            if (!isInitialized || !isSteamRunning) return;

            #if STEAM_ENABLED
            // Run Steam callbacks (required for Steamworks.NET)
            try
            {
                Steamworks.SteamAPI.RunCallbacks();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SteamIntegration] Error running callbacks: {ex.Message}");
            }
            #endif

            // Auto-sync stats
            if (autoSyncStats)
            {
                statsUpdateTimer += Time.unscaledDeltaTime;
                if (statsUpdateTimer >= statsUpdateInterval)
                {
                    statsUpdateTimer = 0f;
                    SyncStatsToSteam();
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (isInitialized && isSteamRunning)
            {
                ShutdownSteam();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeSteam()
        {
            LogDebug("Initializing Steam integration...");

            yield return new WaitForSeconds(0.5f); // Wait for other managers

            #if STEAM_ENABLED
            try
            {
                // Initialize Steamworks API
                if (Steamworks.SteamAPI.RestartAppIfNecessary(new Steamworks.AppId_t(480))) // Replace 480 with your App ID
                {
                    Debug.LogWarning("[SteamIntegration] Restarting app through Steam...");
                    Application.Quit();
                    yield break;
                }

                bool steamInitialized = Steamworks.SteamAPI.Init();
                
                if (steamInitialized)
                {
                    isSteamRunning = true;
                    steamUserId = Steamworks.SteamUser.GetSteamID().ToString();
                    steamUsername = Steamworks.SteamFriends.GetPersonaName();

                    LogDebug($"Steam initialized successfully! User: {steamUsername} ({steamUserId})");

                    // Subscribe to Achievement Manager events
                    SubscribeToAchievementEvents();

                    // Initial sync
                    if (autoSyncAchievements)
                    {
                        yield return new WaitForSeconds(1f);
                        SyncAchievementsToSteam();
                    }

                    isInitialized = true;
                    OnSteamInitialized?.Invoke(true);

                    // Track analytics
                    if (AnalyticsManager.Instance != null)
                    {
                        AnalyticsManager.Instance.TrackEvent("steam_initialized", new Dictionary<string, object>
                        {
                            { "user_id", steamUserId },
                            { "username", steamUsername }
                        });
                    }
                }
                else
                {
                    Debug.LogError("[SteamIntegration] Failed to initialize Steam API!");
                    OnSteamInitialized?.Invoke(false);
                    OnSteamError?.Invoke("Failed to initialize Steam API");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SteamIntegration] Exception during initialization: {ex.Message}");
                OnSteamInitialized?.Invoke(false);
                OnSteamError?.Invoke($"Steam initialization error: {ex.Message}");
            }
            #else
            // Editor simulation mode
            LogDebug("[SIMULATION MODE] Steam integration running without Steamworks.NET SDK");
            isSteamRunning = false; // Simulation only
            isInitialized = true;
            steamUserId = "SIMULATION_USER_123";
            steamUsername = "SimulatedPlayer";

            SubscribeToAchievementEvents();

            OnSteamInitialized?.Invoke(true);
            LogDebug("Steam simulation mode initialized");
            #endif
        }

        private void ShutdownSteam()
        {
            #if STEAM_ENABLED
            if (isSteamRunning)
            {
                // Final stats sync
                SyncStatsToSteam();

                Steamworks.SteamAPI.Shutdown();
                LogDebug("Steam API shutdown");
            }
            #endif
        }

        private void SubscribeToAchievementEvents()
        {
            var achievementManager = AchievementManager.Instance;
            if (achievementManager != null)
            {
                achievementManager.OnAchievementUnlocked += HandleAchievementUnlocked;
                LogDebug("Subscribed to AchievementManager events");
            }
            else
            {
                Debug.LogWarning("[SteamIntegration] AchievementManager not found!");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Achievement Management ────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unlocks a Steam achievement.
        /// Called automatically when AchievementManager unlocks an achievement.
        /// </summary>
        private void HandleAchievementUnlocked(AchievementId achievementId)
        {
            if (!isInitialized || syncedAchievements.Contains(achievementId))
                return;

            UnlockSteamAchievement(achievementId);
        }

        /// <summary>
        /// Manually unlock a Steam achievement by ID.
        /// </summary>
        public void UnlockSteamAchievement(AchievementId achievementId)
        {
            if (!isInitialized)
            {
                LogDebug($"Cannot unlock achievement {achievementId}: Steam not initialized");
                return;
            }

            #if STEAM_ENABLED
            if (!isSteamRunning)
            {
                LogDebug($"Cannot unlock achievement {achievementId}: Steam not running");
                return;
            }

            try
            {
                string steamAchievementName = ConvertToSteamAchievementName(achievementId);
                
                bool success = Steamworks.SteamUserStats.SetAchievement(steamAchievementName);
                
                if (success)
                {
                    // Store achievements
                    Steamworks.SteamUserStats.StoreStats();
                    
                    syncedAchievements.Add(achievementId);
                    OnAchievementUnlocked?.Invoke(achievementId);
                    
                    LogDebug($"Unlocked Steam achievement: {steamAchievementName}");

                    // Track analytics
                    if (AnalyticsManager.Instance != null)
                    {
                        AnalyticsManager.Instance.TrackEvent("steam_achievement_unlocked", new Dictionary<string, object>
                        {
                            { "achievement_id", achievementId.ToString() },
                            { "steam_name", steamAchievementName }
                        });
                    }
                }
                else
                {
                    Debug.LogWarning($"[SteamIntegration] Failed to unlock achievement: {steamAchievementName}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SteamIntegration] Error unlocking achievement: {ex.Message}");
            }
            #else
            // Simulation mode
            syncedAchievements.Add(achievementId);
            OnAchievementUnlocked?.Invoke(achievementId);
            LogDebug($"[SIMULATED] Unlocked achievement: {achievementId}");
            #endif
        }

        /// <summary>
        /// Syncs all unlocked achievements from AchievementManager to Steam.
        /// Useful for first-time sync or after reinstalling.
        /// </summary>
        public void SyncAchievementsToSteam()
        {
            if (!isInitialized)
            {
                LogDebug("Cannot sync achievements: Steam not initialized");
                return;
            }

            var achievementManager = AchievementManager.Instance;
            if (achievementManager == null)
            {
                Debug.LogWarning("[SteamIntegration] AchievementManager not found for sync!");
                return;
            }

            LogDebug("Syncing achievements to Steam...");

            int syncedCount = 0;
            var unlockedAchievements = achievementManager.GetUnlockedAchievements();

            foreach (var achievementId in unlockedAchievements)
            {
                if (!syncedAchievements.Contains(achievementId))
                {
                    UnlockSteamAchievement(achievementId);
                    syncedCount++;
                }
            }

            LogDebug($"Achievement sync complete: {syncedCount} new achievements synced");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("steam_achievements_synced", new Dictionary<string, object>
                {
                    { "synced_count", syncedCount },
                    { "total_unlocked", unlockedAchievements.Count }
                });
            }
        }

        /// <summary>
        /// Clears a Steam achievement (for testing only).
        /// </summary>
        public void ClearSteamAchievement(AchievementId achievementId)
        {
            #if STEAM_ENABLED && UNITY_EDITOR
            if (!isSteamRunning) return;

            try
            {
                string steamAchievementName = ConvertToSteamAchievementName(achievementId);
                Steamworks.SteamUserStats.ClearAchievement(steamAchievementName);
                Steamworks.SteamUserStats.StoreStats();
                
                syncedAchievements.Remove(achievementId);
                LogDebug($"Cleared Steam achievement: {steamAchievementName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SteamIntegration] Error clearing achievement: {ex.Message}");
            }
            #endif
        }

        /// <summary>
        /// Checks if an achievement is unlocked on Steam.
        /// </summary>
        public bool IsSteamAchievementUnlocked(AchievementId achievementId)
        {
            #if STEAM_ENABLED
            if (!isSteamRunning) return false;

            try
            {
                string steamAchievementName = ConvertToSteamAchievementName(achievementId);
                bool achieved;
                Steamworks.SteamUserStats.GetAchievement(steamAchievementName, out achieved);
                return achieved;
            }
            catch
            {
                return false;
            }
            #else
            return syncedAchievements.Contains(achievementId);
            #endif
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Stats Management ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Sets an integer stat value on Steam.
        /// </summary>
        public void SetStat(string statName, int value)
        {
            if (!isInitialized)
                return;

            #if STEAM_ENABLED
            if (!isSteamRunning) return;

            try
            {
                bool success = Steamworks.SteamUserStats.SetStat(statName, value);
                
                if (success)
                {
                    cachedStats[statName] = value;
                    OnStatUpdated?.Invoke(statName, value);
                    LogDebug($"Set Steam stat: {statName} = {value}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SteamIntegration] Error setting stat {statName}: {ex.Message}");
            }
            #else
            cachedStats[statName] = value;
            OnStatUpdated?.Invoke(statName, value);
            LogDebug($"[SIMULATED] Set stat: {statName} = {value}");
            #endif
        }

        /// <summary>
        /// Gets an integer stat value from Steam.
        /// </summary>
        public int GetStat(string statName)
        {
            #if STEAM_ENABLED
            if (!isSteamRunning) return 0;

            try
            {
                int value;
                Steamworks.SteamUserStats.GetStat(statName, out value);
                return value;
            }
            catch
            {
                return 0;
            }
            #else
            return cachedStats.ContainsKey(statName) ? cachedStats[statName] : 0;
            #endif
        }

        /// <summary>
        /// Increments a stat value.
        /// </summary>
        public void IncrementStat(string statName, int amount = 1)
        {
            int currentValue = GetStat(statName);
            SetStat(statName, currentValue + amount);
        }

        /// <summary>
        /// Syncs all tracked stats to Steam.
        /// Called automatically at intervals.
        /// </summary>
        private void SyncStatsToSteam()
        {
            if (!isInitialized)
                return;

            #if STEAM_ENABLED
            if (!isSteamRunning) return;

            try
            {
                // Store stats to Steam
                bool success = Steamworks.SteamUserStats.StoreStats();
                
                if (success)
                {
                    LogDebug("Stats synced to Steam successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SteamIntegration] Error syncing stats: {ex.Message}");
            }
            #else
            LogDebug("[SIMULATED] Stats sync (no-op in simulation mode)");
            #endif
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Leaderboard Management ────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Uploads a score to a Steam leaderboard.
        /// </summary>
        public void UploadLeaderboardScore(string leaderboardName, int score)
        {
            if (!isInitialized)
                return;

            StartCoroutine(UploadLeaderboardScoreCoroutine(leaderboardName, score));
        }

        private IEnumerator UploadLeaderboardScoreCoroutine(string leaderboardName, int score)
        {
            LogDebug($"Uploading score to leaderboard {leaderboardName}: {score}");

            #if STEAM_ENABLED
            if (!isSteamRunning)
            {
                OnLeaderboardScoreUploaded?.Invoke(leaderboardName, false, score, 0);
                yield break;
            }

            // TODO: Implement actual Steamworks leaderboard upload
            // Requires callback handling with Steamworks.NET
            yield return new WaitForSeconds(1f);
            
            LogDebug($"[PLACEHOLDER] Leaderboard score upload: {leaderboardName} = {score}");
            OnLeaderboardScoreUploaded?.Invoke(leaderboardName, true, score, 0);
            #else
            // Simulation mode
            yield return new WaitForSeconds(0.5f);
            LogDebug($"[SIMULATED] Uploaded score to {leaderboardName}: {score}");
            OnLeaderboardScoreUploaded?.Invoke(leaderboardName, true, score, UnityEngine.Random.Range(1, 1000));
            #endif

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("steam_leaderboard_upload", new Dictionary<string, object>
                {
                    { "leaderboard", leaderboardName },
                    { "score", score }
                });
            }
        }

        /// <summary>
        /// Uploads high score to the main leaderboard.
        /// </summary>
        public void UploadHighScore(int score)
        {
            UploadLeaderboardScore(highScoreLeaderboard, score);
        }

        /// <summary>
        /// Uploads highest wave reached.
        /// </summary>
        public void UploadHighestWave(int wave)
        {
            UploadLeaderboardScore(wavesLeaderboard, wave);
        }

        /// <summary>
        /// Uploads total kills.
        /// </summary>
        public void UploadTotalKills(int kills)
        {
            UploadLeaderboardScore(killsLeaderboard, kills);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Helper Methods ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════────════════════

        /// <summary>
        /// Converts AchievementId enum to Steam achievement string identifier.
        /// Must match achievement IDs configured in Steam Partner dashboard.
        /// </summary>
        private string ConvertToSteamAchievementName(AchievementId achievementId)
        {
            // Format: ACH_{ENUM_NAME_UPPERCASE}
            // Example: AchievementId.FirstVictory -> "ACH_FIRST_VICTORY"
            return $"ACH_{achievementId.ToString().ToUpper()}";
        }

        /// <summary>
        /// Gets the Steam user ID.
        /// </summary>
        public string GetSteamUserId()
        {
            return steamUserId;
        }

        /// <summary>
        /// Gets the Steam username.
        /// </summary>
        public string GetSteamUsername()
        {
            return steamUsername;
        }

        /// <summary>
        /// Checks if Steam is running and initialized.
        /// </summary>
        public bool IsSteamRunning()
        {
            return isSteamRunning;
        }

        /// <summary>
        /// Checks if the manager is initialized (including simulation mode).
        /// </summary>
        public bool IsInitialized()
        {
            return isInitialized;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[SteamIntegration] {message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Public API Summary ────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /*
         * ACHIEVEMENTS:
         * - UnlockSteamAchievement(AchievementId) - Manually unlock achievement
         * - SyncAchievementsToSteam() - Sync all unlocked achievements
         * - IsSteamAchievementUnlocked(AchievementId) - Check unlock status
         * - ClearSteamAchievement(AchievementId) - Clear for testing (Editor only)
         *
         * STATS:
         * - SetStat(string, int) - Set integer stat
         * - GetStat(string) - Get integer stat
         * - IncrementStat(string, int) - Increment stat by amount
         *
         * LEADERBOARDS:
         * - UploadLeaderboardScore(string, int) - Upload to named leaderboard
         * - UploadHighScore(int) - Shortcut for high scores
         * - UploadHighestWave(int) - Shortcut for wave leaderboard
         * - UploadTotalKills(int) - Shortcut for kills leaderboard
         *
         * UTILITIES:
         * - GetSteamUserId() - Get Steam user ID
         * - GetSteamUsername() - Get Steam persona name
         * - IsSteamRunning() - Check if Steam API is active
         * - IsInitialized() - Check if manager is ready
         */
    }
}
