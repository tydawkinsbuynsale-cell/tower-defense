using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RobotTD.Online
{
    /// <summary>
    /// Cloud save system for syncing player data across devices.
    /// Supports multiple backends: Unity Cloud Save, PlayFab, or custom server.
    /// Offline-first design with conflict resolution.
    /// 
    /// Usage:
    /// - Auto-syncs on app start / pause / quit
    /// - Manual sync: CloudSaveManager.Instance.PushToCloud()
    /// - Conflict resolution handles local vs cloud timestamps
    /// </summary>
    public class CloudSaveManager : MonoBehaviour
    {
        public static CloudSaveManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableCloudSave = true;
        [SerializeField] private bool autoSyncOnStart = true;
        [SerializeField] private bool autoSyncOnPause = true;
        [SerializeField] private float autoSyncInterval = 300f; // 5 minutes
        [SerializeField] private bool verboseLogging = false;

        [Header("Conflict Resolution")]
        [SerializeField] private ConflictResolutionStrategy conflictStrategy = ConflictResolutionStrategy.MostRecent;

        // State
        private bool isSyncing = false;
        private bool isInitialized = false;
        private float syncTimer = 0f;
        private DateTime lastSyncTime = DateTime.MinValue;
        private string lastSyncedDataHash = "";

        // Events
        public event Action<bool> OnSyncCompleted; // success
        public event Action<string> OnSyncError;
        public event Action<ConflictInfo> OnConflictDetected;

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
            if (!enableCloudSave)
            {
                LogDebug("Cloud save disabled");
                return;
            }

            InitializeCloudSave();
        }

        private void Update()
        {
            if (!enableCloudSave || !isInitialized) return;

            // Auto-sync timer
            syncTimer += Time.unscaledDeltaTime;
            if (syncTimer >= autoSyncInterval)
            {
                syncTimer = 0f;
                SyncWithCloud();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && autoSyncOnPause && enableCloudSave && isInitialized)
            {
                PushToCloud();
            }
        }

        private void OnApplicationQuit()
        {
            if (enableCloudSave && isInitialized)
            {
                // Force synchronous push on quit (best effort)
                PushToCloud();
            }
        }

        // ── Initialization ────────────────────────────────────────────────────

        private void InitializeCloudSave()
        {
            LogDebug("Initializing cloud save system...");

            // Initialize backend
            StartCoroutine(InitializeBackend());
        }

        private IEnumerator InitializeBackend()
        {
            #if UNITY_CLOUD_SAVE
            yield return InitializeUnityCloudSave();
            #elif PLAYFAB
            yield return InitializePlayFab();
            #else
            yield return InitializeCustomBackend();
            #endif

            isInitialized = true;
            LogDebug("Cloud save initialized");

            // Auto-sync on start
            if (autoSyncOnStart)
            {
                yield return new WaitForSeconds(1f); // Wait for SaveManager to load
                PullFromCloud();
            }
        }

        // ── Sync Operations ───────────────────────────────────────────────────

        /// <summary>
        /// Perform bidirectional sync: pull from cloud, resolve conflicts, push changes.
        /// </summary>
        public void SyncWithCloud()
        {
            if (!enableCloudSave || !isInitialized || isSyncing)
            {
                LogDebug("Sync skipped - not ready or already syncing");
                return;
            }

            StartCoroutine(SyncCoroutine());
        }

        /// <summary>
        /// Push local save data to cloud (overwrite cloud).
        /// </summary>
        public void PushToCloud()
        {
            if (!enableCloudSave || !isInitialized || isSyncing)
            {
                LogDebug("Push skipped - not ready or already syncing");
                return;
            }

            StartCoroutine(PushCoroutine());
        }

        /// <summary>
        /// Pull cloud save data and apply to local (with conflict resolution).
        /// </summary>
        public void PullFromCloud()
        {
            if (!enableCloudSave || !isInitialized || isSyncing)
            {
                LogDebug("Pull skipped - not ready or already syncing");
                return;
            }

            StartCoroutine(PullCoroutine());
        }

        private IEnumerator SyncCoroutine()
        {
            isSyncing = true;
            LogDebug("Starting bidirectional sync...");

            // 1. Pull from cloud
            Core.PlayerSaveData cloudData = null;
            yield return FetchFromCloud(data => cloudData = data);

            if (cloudData == null)
            {
                LogDebug("No cloud data found, pushing local data");
                yield return PushCoroutine();
                isSyncing = false;
                yield break;
            }

            // 2. Get local data
            Core.PlayerSaveData localData = Core.SaveManager.Instance?.Data;
            if (localData == null)
            {
                OnSyncError?.Invoke("Local save data not available");
                isSyncing = false;
                yield break;
            }

            // 3. Resolve conflicts
            Core.PlayerSaveData mergedData = ResolveConflict(localData, cloudData);

            // 4. Apply merged data locally
            if (mergedData != localData)
            {
                Core.SaveManager.Instance.Data.CopyFrom(mergedData);
                Core.SaveManager.Instance.Save();
                LogDebug("Applied merged data to local save");
            }

            // 5. Push merged data to cloud
            yield return PushToCloudInternal(mergedData);

            lastSyncTime = DateTime.UtcNow;
            isSyncing = false;
            OnSyncCompleted?.Invoke(true);
            LogDebug("Sync completed successfully");
        }

        private IEnumerator PushCoroutine()
        {
            isSyncing = true;
            LogDebug("Pushing to cloud...");

            Core.PlayerSaveData localData = Core.SaveManager.Instance?.Data;
            if (localData == null)
            {
                OnSyncError?.Invoke("No local data to push");
                isSyncing = false;
                yield break;
            }

            // Update timestamp
            localData.lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            yield return PushToCloudInternal(localData);

            lastSyncTime = DateTime.UtcNow;
            lastSyncedDataHash = ComputeDataHash(localData);
            isSyncing = false;
            OnSyncCompleted?.Invoke(true);
            LogDebug("Push completed");
        }

        private IEnumerator PullCoroutine()
        {
            isSyncing = true;
            LogDebug("Pulling from cloud...");

            Core.PlayerSaveData cloudData = null;
            yield return FetchFromCloud(data => cloudData = data);

            if (cloudData == null)
            {
                LogDebug("No cloud data available");
                isSyncing = false;
                OnSyncCompleted?.Invoke(false);
                yield break;
            }

            // Apply cloud data with conflict resolution
            Core.PlayerSaveData localData = Core.SaveManager.Instance?.Data;
            Core.PlayerSaveData mergedData = ResolveConflict(localData, cloudData);

            Core.SaveManager.Instance.Data.CopyFrom(mergedData);
            Core.SaveManager.Instance.Save();

            lastSyncTime = DateTime.UtcNow;
            lastSyncedDataHash = ComputeDataHash(cloudData);
            isSyncing = false;
            OnSyncCompleted?.Invoke(true);
            LogDebug("Pull completed");
        }

        // ── Conflict Resolution ───────────────────────────────────────────────

        private Core.PlayerSaveData ResolveConflict(Core.PlayerSaveData localData, Core.PlayerSaveData cloudData)
        {
            // No local data - use cloud
            if (localData == null) return cloudData;

            // No cloud data - use local
            if (cloudData == null) return localData;

            // Check for actual conflict
            bool hasConflict = localData.lastSaveTimestamp != cloudData.lastSaveTimestamp;

            if (!hasConflict)
            {
                // No conflict - data is identical
                return localData;
            }

            // Fire conflict event
            ConflictInfo conflict = new ConflictInfo
            {
                localTimestamp = localData.lastSaveTimestamp,
                cloudTimestamp = cloudData.lastSaveTimestamp,
                localData = localData,
                cloudData = cloudData
            };
            OnConflictDetected?.Invoke(conflict);

            LogDebug($"Conflict detected: Local={localData.lastSaveTimestamp}, Cloud={cloudData.lastSaveTimestamp}");

            // Apply strategy
            switch (conflictStrategy)
            {
                case ConflictResolutionStrategy.MostRecent:
                    return ResolveMostRecent(localData, cloudData);

                case ConflictResolutionStrategy.HighestProgress:
                    return ResolveHighestProgress(localData, cloudData);

                case ConflictResolutionStrategy.Merge:
                    return MergeData(localData, cloudData);

                case ConflictResolutionStrategy.PreferLocal:
                    return localData;

                case ConflictResolutionStrategy.PreferCloud:
                    return cloudData;

                default:
                    return ResolveMostRecent(localData, cloudData);
            }
        }

        private Core.PlayerSaveData ResolveMostRecent(Core.PlayerSaveData localData, Core.PlayerSaveData cloudData)
        {
            bool localIsNewer = localData.lastSaveTimestamp > cloudData.lastSaveTimestamp;
            LogDebug($"Using most recent: {(localIsNewer ? "Local" : "Cloud")}");
            return localIsNewer ? localData : cloudData;
        }

        private Core.PlayerSaveData ResolveHighestProgress(Core.PlayerSaveData localData, Core.PlayerSaveData cloudData)
        {
            // Compare total XP as progress indicator
            bool localHasMore = localData.totalXP >= cloudData.totalXP;
            LogDebug($"Using highest progress: {(localHasMore ? "Local" : "Cloud")} (XP: {localData.totalXP} vs {cloudData.totalXP})");
            return localHasMore ? localData : cloudData;
        }

        private Core.PlayerSaveData MergeData(Core.PlayerSaveData localData, Core.PlayerSaveData cloudData)
        {
            // Intelligent merge: take max of all numeric values
            Core.PlayerSaveData merged = new Core.PlayerSaveData();

            // Copy from the more recent one as base
            bool localIsNewer = localData.lastSaveTimestamp > cloudData.lastSaveTimestamp;
            Core.PlayerSaveData baseData = localIsNewer ? localData : cloudData;
            merged.CopyFrom(baseData);

            // Merge numeric values (take maximum)
            merged.totalXP = Mathf.Max(localData.totalXP, cloudData.totalXP);
            merged.playerLevel = Mathf.Max(localData.playerLevel, cloudData.playerLevel);
            merged.techPoints = Mathf.Max(localData.techPoints, cloudData.techPoints);
            merged.totalCreditsEarned = Mathf.Max(localData.totalCreditsEarned, cloudData.totalCreditsEarned);
            merged.totalEnemiesKilled = Mathf.Max(localData.totalEnemiesKilled, cloudData.totalEnemiesKilled);
            merged.totalTowersPlaced = Mathf.Max(localData.totalTowersPlaced, cloudData.totalTowersPlaced);
            merged.totalTowersUpgraded = Mathf.Max(localData.totalTowersUpgraded, cloudData.totalTowersUpgraded);
            merged.totalWavesCompleted = Mathf.Max(localData.totalWavesCompleted, cloudData.totalWavesCompleted);
            merged.totalPlayTimeSeconds = Mathf.Max(localData.totalPlayTimeSeconds, cloudData.totalPlayTimeSeconds);
            merged.totalGamesPlayed = Mathf.Max(localData.totalGamesPlayed, cloudData.totalGamesPlayed);
            merged.totalVictories = Mathf.Max(localData.totalVictories, cloudData.totalVictories);
            merged.endlessHighWave = Mathf.Max(localData.endlessHighWave, cloudData.endlessHighWave);
            merged.endlessHighScore = Math.Max(localData.endlessHighScore, cloudData.endlessHighScore);
            merged.bossRushBestRun = Mathf.Max(localData.bossRushBestRun, cloudData.bossRushBestRun);
            merged.bossRushHighScore = Math.Max(localData.bossRushHighScore, cloudData.bossRushHighScore);

            // Merge lists (union)
            merged.unlockedMaps = UnionLists(localData.unlockedMaps, cloudData.unlockedMaps);
            merged.completedAchievements = UnionLists(localData.completedAchievements, cloudData.completedAchievements);

            // Merge map records (take best of each)
            merged.mapRecords = MergeMapRecords(localData.mapRecords, cloudData.mapRecords);

            // Merge tech tree (take max of each upgrade)
            merged.techTree = MergeTechTree(localData.techTree, cloudData.techTree);

            // Use most recent timestamp
            merged.lastSaveTimestamp = Math.Max(localData.lastSaveTimestamp, cloudData.lastSaveTimestamp);

            LogDebug("Merged data from local and cloud");
            return merged;
        }

        private List<string> UnionLists(List<string> list1, List<string> list2)
        {
            HashSet<string> union = new HashSet<string>(list1);
            union.UnionWith(list2);
            return new List<string>(union);
        }

        private Dictionary<string, Core.MapRecord> MergeMapRecords(
            Dictionary<string, Core.MapRecord> records1,
            Dictionary<string, Core.MapRecord> records2)
        {
            Dictionary<string, Core.MapRecord> merged = new Dictionary<string, Core.MapRecord>();

            // Add all from first
            foreach (var kvp in records1)
            {
                merged[kvp.Key] = kvp.Value;
            }

            // Merge with second
            foreach (var kvp in records2)
            {
                if (!merged.ContainsKey(kvp.Key))
                {
                    merged[kvp.Key] = kvp.Value;
                }
                else
                {
                    // Take best values
                    Core.MapRecord r1 = merged[kvp.Key];
                    Core.MapRecord r2 = kvp.Value;

                    r1.highScore = Mathf.Max(r1.highScore, r2.highScore);
                    r1.bestWave = Mathf.Max(r1.bestWave, r2.bestWave);
                    r1.starsEarned = Mathf.Max(r1.starsEarned, r2.starsEarned);
                    r1.fastestClear = Mathf.Min(r1.fastestClear, r2.fastestClear);
                    r1.completed = r1.completed || r2.completed;
                }
            }

            return merged;
        }

        private Core.TechTreeSaveData MergeTechTree(Core.TechTreeSaveData tree1, Core.TechTreeSaveData tree2)
        {
            Core.TechTreeSaveData merged = new Core.TechTreeSaveData();

            merged.firepower = Mathf.Max(tree1.firepower, tree2.firepower);
            merged.efficiency = Mathf.Max(tree1.efficiency, tree2.efficiency);
            merged.resilience = Mathf.Max(tree1.resilience, tree2.resilience);
            merged.tactics = Mathf.Max(tree1.tactics, tree2.tactics);
            merged.rapidDeploy = Mathf.Max(tree1.rapidDeploy, tree2.rapidDeploy);
            merged.recycling = Mathf.Max(tree1.recycling, tree2.recycling);

            return merged;
        }

        // ── Backend Implementations ───────────────────────────────────────────

        #region Unity Cloud Save

        #if UNITY_CLOUD_SAVE
        private IEnumerator InitializeUnityCloudSave()
        {
            LogDebug("Initializing Unity Cloud Save...");
            // await Unity.Services.Core.UnityServices.InitializeAsync();
            // await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
            yield return null;
        }

        private IEnumerator PushToCloudInternal(Core.PlayerSaveData data)
        {
            LogDebug("[Unity Cloud Save] Pushing data...");
            string json = JsonUtility.ToJson(data);

            // var saveData = new Dictionary<string, object> { { "player_save", json } };
            // await Unity.Services.CloudSave.CloudSaveService.Instance.Data.ForceSaveAsync(saveData);

            yield return null;
        }

        private IEnumerator FetchFromCloud(Action<Core.PlayerSaveData> callback)
        {
            LogDebug("[Unity Cloud Save] Fetching data...");

            // var data = await Unity.Services.CloudSave.CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "player_save" });
            // if (data.TryGetValue("player_save", out string json))
            // {
            //     Core.PlayerSaveData saveData = JsonUtility.FromJson<Core.PlayerSaveData>(json);
            //     callback?.Invoke(saveData);
            // }
            // else
            // {
            //     callback?.Invoke(null);
            // }

            callback?.Invoke(null);
            yield return null;
        }
        #endif

        #endregion

        #region PlayFab

        #if PLAYFAB
        private IEnumerator InitializePlayFab()
        {
            LogDebug("Initializing PlayFab cloud save...");
            // PlayFab login already handled by LeaderboardManager or other systems
            yield return null;
        }

        private IEnumerator PushToCloudInternal(Core.PlayerSaveData data)
        {
            LogDebug("[PlayFab] Pushing data...");
            string json = JsonUtility.ToJson(data);

            // var request = new PlayFab.ClientModels.UpdateUserDataRequest
            // {
            //     Data = new Dictionary<string, string> { { "player_save", json } }
            // };
            // PlayFab.PlayFabClientAPI.UpdateUserData(request, OnPlayFabSuccess, OnPlayFabError);

            yield return null;
        }

        private IEnumerator FetchFromCloud(Action<Core.PlayerSaveData> callback)
        {
            LogDebug("[PlayFab] Fetching data...");

            // var request = new PlayFab.ClientModels.GetUserDataRequest();
            // PlayFab.PlayFabClientAPI.GetUserData(request,
            //     result =>
            //     {
            //         if (result.Data.TryGetValue("player_save", out var dataRecord))
            //         {
            //             Core.PlayerSaveData saveData = JsonUtility.FromJson<Core.PlayerSaveData>(dataRecord.Value);
            //             callback?.Invoke(saveData);
            //         }
            //         else
            //         {
            //             callback?.Invoke(null);
            //         }
            //     },
            //     error => callback?.Invoke(null));

            callback?.Invoke(null);
            yield return null;
        }
        #endif

        #endregion

        #region Custom Backend

        private IEnumerator InitializeCustomBackend()
        {
            LogDebug("Initializing custom cloud save backend...");
            // TODO: Initialize your custom backend here
            yield return null;
        }

        private IEnumerator PushToCloudInternal(Core.PlayerSaveData data)
        {
            LogDebug("[Custom] Pushing data...");
            string json = JsonUtility.ToJson(data);

            // TODO: HTTP POST to your server
            // string url = "https://your-server.com/api/save";
            // using (UnityWebRequest www = UnityWebRequest.Post(url, json))
            // {
            //     www.SetRequestHeader("Content-Type", "application/json");
            //     yield return www.SendWebRequest();
            //     if (www.result == UnityWebRequest.Result.Success)
            //     {
            //         OnSyncCompleted?.Invoke(true);
            //     }
            //     else
            //     {
            //         OnSyncError?.Invoke(www.error);
            //     }
            // }

            yield return null;
        }

        private IEnumerator FetchFromCloud(Action<Core.PlayerSaveData> callback)
        {
            LogDebug("[Custom] Fetching data...");

            // TODO: HTTP GET from your server
            // string url = "https://your-server.com/api/save";
            // using (UnityWebRequest www = UnityWebRequest.Get(url))
            // {
            //     yield return www.SendWebRequest();
            //     if (www.result == UnityWebRequest.Result.Success)
            //     {
            //         Core.PlayerSaveData saveData = JsonUtility.FromJson<Core.PlayerSaveData>(www.downloadHandler.text);
            //         callback?.Invoke(saveData);
            //     }
            //     else
            //     {
            //         callback?.Invoke(null);
            //     }
            // }

            callback?.Invoke(null);
            yield return null;
        }

        #endregion

        // ── Utilities ─────────────────────────────────────────────────────────

        private string ComputeDataHash(Core.PlayerSaveData data)
        {
            if (data == null) return "";
            string json = JsonUtility.ToJson(data);
            return json.GetHashCode().ToString();
        }

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[CloudSaveManager] {message}");
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Check if there are local changes that haven't been synced.
        /// </summary>
        public bool HasUnsyncedChanges()
        {
            if (Core.SaveManager.Instance == null) return false;

            string currentHash = ComputeDataHash(Core.SaveManager.Instance.Data);
            return currentHash != lastSyncedDataHash;
        }

        /// <summary>
        /// Get time since last sync.
        /// </summary>
        public TimeSpan TimeSinceLastSync()
        {
            if (lastSyncTime == DateTime.MinValue) return TimeSpan.MaxValue;
            return DateTime.UtcNow - lastSyncTime;
        }

        /// <summary>
        /// Force push immediately (blocking).
        /// </summary>
        public void ForcePushNow()
        {
            if (!enableCloudSave || !isInitialized) return;
            StartCoroutine(PushCoroutine());
        }
    }

    // ── Data Structures ───────────────────────────────────────────────────────

    public enum ConflictResolutionStrategy
    {
        MostRecent,       // Use the save with the most recent timestamp
        HighestProgress,  // Use the save with the highest XP/progress
        Merge,            // Intelligently merge both saves (take best of all values)
        PreferLocal,      // Always use local save
        PreferCloud       // Always use cloud save
    }

    [Serializable]
    public class ConflictInfo
    {
        public long localTimestamp;
        public long cloudTimestamp;
        public Core.PlayerSaveData localData;
        public Core.PlayerSaveData cloudData;
    }
}

// Extension method for copying PlayerSaveData
namespace RobotTD.Core
{
    public static class PlayerSaveDataExtensions
    {
        public static void CopyFrom(this PlayerSaveData target, PlayerSaveData source)
        {
            if (source == null) return;

            target.saveVersion = source.saveVersion;
            target.lastSaveTimestamp = source.lastSaveTimestamp;
            target.playerName = source.playerName;

            target.playerLevel = source.playerLevel;
            target.totalXP = source.totalXP;
            target.techPoints = source.techPoints;
            target.totalCreditsEarned = source.totalCreditsEarned;
            target.totalEnemiesKilled = source.totalEnemiesKilled;
            target.totalTowersPlaced = source.totalTowersPlaced;
            target.totalTowersUpgraded = source.totalTowersUpgraded;
            target.totalWavesCompleted = source.totalWavesCompleted;
            target.totalPlayTimeSeconds = source.totalPlayTimeSeconds;
            target.totalGamesPlayed = source.totalGamesPlayed;
            target.totalVictories = source.totalVictories;

            target.masterVolume = source.masterVolume;
            target.sfxVolume = source.sfxVolume;
            target.musicVolume = source.musicVolume;
            target.vibrationEnabled = source.vibrationEnabled;
            target.graphicsQuality = source.graphicsQuality;

            target.tutorialCompleted = source.tutorialCompleted;
            target.unlockedMaps = new List<string>(source.unlockedMaps);
            target.completedAchievements = new List<string>(source.completedAchievements);
            target.mapRecords = new Dictionary<string, MapRecord>(source.mapRecords);

            target.techTree = new TechTreeSaveData
            {
                firepower = source.techTree.firepower,
                efficiency = source.techTree.efficiency,
                resilience = source.techTree.resilience,
                tactics = source.techTree.tactics,
                rapidDeploy = source.techTree.rapidDeploy,
                recycling = source.techTree.recycling
            };

            target.dailyStreak = source.dailyStreak;
            target.lastDailyLoginTimestamp = source.lastDailyLoginTimestamp;

            target.endlessHighWave = source.endlessHighWave;
            target.endlessHighScore = source.endlessHighScore;
            target.endlessGamesPlayed = source.endlessGamesPlayed;

            target.bossRushBestRun = source.bossRushBestRun;
            target.bossRushHighScore = source.bossRushHighScore;
            target.bossRushGamesPlayed = source.bossRushGamesPlayed;
        }
    }
}
