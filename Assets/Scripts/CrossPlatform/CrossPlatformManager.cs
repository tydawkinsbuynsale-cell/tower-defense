using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Core;
using RobotTD.Online;
using RobotTD.Analytics;

namespace RobotTD.CrossPlatform
{
    /// <summary>
    /// Cross-platform progression system for unified play experience.
    /// Handles unified accounts, cloud sync, cross-platform friends, and platform rewards.
    /// Supports Android, iOS, Steam, and Web platforms with seamless progression.
    /// </summary>
    public class CrossPlatformManager : MonoBehaviour
    {
        public static CrossPlatformManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableCrossPlatform = true;
        [SerializeField] private bool autoSyncOnChange = true;
        [SerializeField] private float syncInterval = 300f; // 5 minutes
        [SerializeField] private bool verboseLogging = true;

        [Header("Platform Rewards")]
        [SerializeField] private PlatformReward[] platformRewards;

        // State
        private bool isInitialized = false;
        private PlatformType currentPlatform;
        private UnifiedAccount unifiedAccount;
        private List<LinkedPlatform> linkedPlatforms = new List<LinkedPlatform>();
        private CrossPlatformFriendsList friendsList = new CrossPlatformFriendsList();
        private DateTime lastSyncTime = DateTime.MinValue;
        private bool isSyncing = false;
        private Coroutine autoSyncCoroutine;

        // Events
        public event Action<UnifiedAccount> OnAccountLinked;
        public event Action<PlatformType> OnPlatformLinked;
        public event Action<PlatformType> OnPlatformUnlinked;
        public event Action OnCloudSyncStarted;
        public event Action<bool> OnCloudSyncCompleted; // success
        public event Action OnCrossPlatformInitialized;
        public event Action<CrossPlatformFriend> OnFriendAdded;
        public event Action<string> OnFriendRemoved; // friendId
        public event Action<PlatformReward> OnPlatformRewardClaimed;

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
            if (!enableCrossPlatform)
            {
                LogDebug("Cross-platform system disabled");
                return;
            }

            StartCoroutine(InitializeCrossPlatformSystem());
        }

        private void OnDestroy()
        {
            if (autoSyncCoroutine != null)
            {
                StopCoroutine(autoSyncCoroutine);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeCrossPlatformSystem()
        {
            LogDebug("Initializing cross-platform system...");

            yield return new WaitForSeconds(0.3f);

            // Detect current platform
            DetectPlatform();

            // Wait for authentication
            var authManager = AuthenticationManager.Instance;
            if (authManager != null)
            {
                float timeout = 10f;
                float elapsed = 0f;
                while (!authManager.IsInitialized && elapsed < timeout)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }

                if (authManager.IsAuthenticated)
                {
                    // Load unified account
                    yield return StartCoroutine(LoadUnifiedAccount());
                }
            }

            // Load linked platforms
            LoadLinkedPlatforms();

            // Load friends list
            LoadFriendsList();

            // Check for platform rewards
            CheckPlatformRewards();

            // Start auto-sync if enabled
            if (autoSyncOnChange)
            {
                autoSyncCoroutine = StartCoroutine(AutoSyncCoroutine());
            }

            isInitialized = true;
            LogDebug($"Cross-platform system initialized on {currentPlatform}");

            OnCrossPlatformInitialized?.Invoke();

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("crossplatform_initialized", new Dictionary<string, object>
                {
                    { "platform", currentPlatform.ToString() },
                    { "linked_platforms", linkedPlatforms.Count },
                    { "friends_count", friendsList.friends.Count },
                    { "has_unified_account", unifiedAccount != null }
                });
            }
        }

        private void DetectPlatform()
        {
            #if UNITY_ANDROID
            currentPlatform = PlatformType.Android;
            #elif UNITY_IOS
            currentPlatform = PlatformType.iOS;
            #elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            currentPlatform = PlatformType.Steam;
            #elif UNITY_WEBGL
            currentPlatform = PlatformType.Web;
            #else
            currentPlatform = PlatformType.Unknown;
            #endif

            LogDebug($"Platform detected: {currentPlatform}");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Unified Account System ────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates or retrieves unified account for the player.
        /// </summary>
        private IEnumerator LoadUnifiedAccount()
        {
            var authManager = AuthenticationManager.Instance;
            if (authManager == null || !authManager.IsAuthenticated)
            {
                LogDebug("Cannot load unified account: not authenticated");
                yield break;
            }

            // In production, this would query a backend server
            // For now, load from local storage
            LoadUnifiedAccountFromLocal();

            if (unifiedAccount == null)
            {
                // Create new unified account
                unifiedAccount = new UnifiedAccount
                {
                    accountId = Guid.NewGuid().ToString(),
                    playerId = authManager.PlayerId,
                    playerName = authManager.PlayerName,
                    createdDate = DateTime.UtcNow,
                    lastSyncDate = DateTime.UtcNow,
                    primaryPlatform = currentPlatform
                };

                SaveUnifiedAccountToLocal();
                LogDebug($"Created unified account: {unifiedAccount.accountId}");
            }
            else
            {
                LogDebug($"Loaded unified account: {unifiedAccount.accountId}");
            }

            OnAccountLinked?.Invoke(unifiedAccount);
            yield return null;
        }

        /// <summary>
        /// Gets the unified account.
        /// </summary>
        public UnifiedAccount GetUnifiedAccount()
        {
            return unifiedAccount;
        }

        /// <summary>
        /// Links a platform to the unified account.
        /// </summary>
        public bool LinkPlatform(PlatformType platform, string platformUserId)
        {
            if (unifiedAccount == null)
            {
                LogDebug("Cannot link platform: no unified account");
                return false;
            }

            if (linkedPlatforms.Any(p => p.platform == platform))
            {
                LogDebug($"Platform already linked: {platform}");
                return false;
            }

            var linkedPlatform = new LinkedPlatform
            {
                platform = platform,
                platformUserId = platformUserId,
                linkedDate = DateTime.UtcNow,
                isActive = true
            };

            linkedPlatforms.Add(linkedPlatform);
            SaveLinkedPlatforms();

            OnPlatformLinked?.Invoke(platform);
            LogDebug($"Linked platform: {platform}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("platform_linked", new Dictionary<string, object>
                {
                    { "platform", platform.ToString() },
                    { "total_linked", linkedPlatforms.Count }
                });
            }

            return true;
        }

        /// <summary>
        /// Unlinks a platform from the unified account.
        /// </summary>
        public bool UnlinkPlatform(PlatformType platform)
        {
            if (platform == unifiedAccount?.primaryPlatform)
            {
                LogDebug("Cannot unlink primary platform");
                return false;
            }

            var linkedPlatform = linkedPlatforms.FirstOrDefault(p => p.platform == platform);
            if (linkedPlatform == null)
            {
                LogDebug($"Platform not linked: {platform}");
                return false;
            }

            linkedPlatforms.Remove(linkedPlatform);
            SaveLinkedPlatforms();

            OnPlatformUnlinked?.Invoke(platform);
            LogDebug($"Unlinked platform: {platform}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("platform_unlinked", new Dictionary<string, object>
                {
                    { "platform", platform.ToString() },
                    { "remaining_linked", linkedPlatforms.Count }
                });
            }

            return true;
        }

        /// <summary>
        /// Gets all linked platforms.
        /// </summary>
        public List<LinkedPlatform> GetLinkedPlatforms()
        {
            return new List<LinkedPlatform>(linkedPlatforms);
        }

        /// <summary>
        /// Checks if a platform is linked.
        /// </summary>
        public bool IsPlatformLinked(PlatformType platform)
        {
            return linkedPlatforms.Any(p => p.platform == platform && p.isActive);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Cloud Sync System ─────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Manually triggers cloud sync.
        /// </summary>
        public void TriggerCloudSync()
        {
            if (!isInitialized || isSyncing)
            {
                LogDebug("Cannot sync: system not ready or already syncing");
                return;
            }

            StartCoroutine(PerformCloudSync());
        }

        private IEnumerator PerformCloudSync()
        {
            isSyncing = true;
            OnCloudSyncStarted?.Invoke();

            LogDebug("Starting cloud sync...");

            // Wait for CloudSaveManager
            var cloudSaveManager = CloudSaveManager.Instance;
            if (cloudSaveManager == null)
            {
                LogDebug("CloudSaveManager not available");
                isSyncing = false;
                OnCloudSyncCompleted?.Invoke(false);
                yield break;
            }

            // Upload current save
            bool uploadSuccess = false;
            yield return StartCoroutine(UploadSaveToCloud(result => uploadSuccess = result));

            if (!uploadSuccess)
            {
                LogDebug("Cloud sync failed: upload error");
                isSyncing = false;
                OnCloudSyncCompleted?.Invoke(false);
                yield break;
            }

            // Update last sync time
            lastSyncTime = DateTime.UtcNow;
            if (unifiedAccount != null)
            {
                unifiedAccount.lastSyncDate = lastSyncTime;
                SaveUnifiedAccountToLocal();
            }

            isSyncing = false;
            OnCloudSyncCompleted?.Invoke(true);

            LogDebug("Cloud sync completed successfully");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("cloud_sync_completed", new Dictionary<string, object>
                {
                    { "platform", currentPlatform.ToString() },
                    { "success", true }
                });
            }
        }

        private IEnumerator UploadSaveToCloud(Action<bool> callback)
        {
            // In production, this would upload to backend server
            // For now, simulate cloud upload
            yield return new WaitForSeconds(0.5f);

            var cloudSaveManager = CloudSaveManager.Instance;
            if (cloudSaveManager != null)
            {
                // Use existing cloud save system
                cloudSaveManager.SaveToCloud();
                yield return new WaitForSeconds(1f);
                callback?.Invoke(true);
            }
            else
            {
                callback?.Invoke(false);
            }
        }

        private IEnumerator AutoSyncCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(syncInterval);

                if (isInitialized && !isSyncing)
                {
                    // Check if enough time has passed since last sync
                    TimeSpan timeSinceSync = DateTime.UtcNow - lastSyncTime;
                    if (timeSinceSync.TotalSeconds >= syncInterval)
                    {
                        LogDebug("Auto-sync triggered");
                        yield return StartCoroutine(PerformCloudSync());
                    }
                }
            }
        }

        /// <summary>
        /// Gets time since last sync.
        /// </summary>
        public TimeSpan GetTimeSinceLastSync()
        {
            return DateTime.UtcNow - lastSyncTime;
        }

        /// <summary>
        /// Checks if currently syncing.
        /// </summary>
        public bool IsSyncing()
        {
            return isSyncing;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Cross-Platform Friends ────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Adds a friend across platforms.
        /// </summary>
        public bool AddFriend(string friendId, string friendName, PlatformType friendPlatform)
        {
            if (friendsList.friends.Any(f => f.friendId == friendId))
            {
                LogDebug($"Friend already added: {friendId}");
                return false;
            }

            var friend = new CrossPlatformFriend
            {
                friendId = friendId,
                friendName = friendName,
                platform = friendPlatform,
                addedDate = DateTime.UtcNow,
                lastSeenDate = DateTime.UtcNow,
                isOnline = false
            };

            friendsList.friends.Add(friend);
            SaveFriendsList();

            OnFriendAdded?.Invoke(friend);
            LogDebug($"Added friend: {friendName} ({friendPlatform})");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("crossplatform_friend_added", new Dictionary<string, object>
                {
                    { "friend_platform", friendPlatform.ToString() },
                    { "total_friends", friendsList.friends.Count }
                });
            }

            return true;
        }

        /// <summary>
        /// Removes a friend.
        /// </summary>
        public bool RemoveFriend(string friendId)
        {
            var friend = friendsList.friends.FirstOrDefault(f => f.friendId == friendId);
            if (friend == null)
            {
                LogDebug($"Friend not found: {friendId}");
                return false;
            }

            friendsList.friends.Remove(friend);
            SaveFriendsList();

            OnFriendRemoved?.Invoke(friendId);
            LogDebug($"Removed friend: {friend.friendName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("crossplatform_friend_removed", new Dictionary<string, object>
                {
                    { "remaining_friends", friendsList.friends.Count }
                });
            }

            return true;
        }

        /// <summary>
        /// Gets all cross-platform friends.
        /// </summary>
        public List<CrossPlatformFriend> GetFriends()
        {
            return new List<CrossPlatformFriend>(friendsList.friends);
        }

        /// <summary>
        /// Gets friends by platform.
        /// </summary>
        public List<CrossPlatformFriend> GetFriendsByPlatform(PlatformType platform)
        {
            return friendsList.friends.Where(f => f.platform == platform).ToList();
        }

        /// <summary>
        /// Gets online friends count.
        /// </summary>
        public int GetOnlineFriendsCount()
        {
            return friendsList.friends.Count(f => f.isOnline);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Platform Rewards ──────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void CheckPlatformRewards()
        {
            if (platformRewards == null || platformRewards.Length == 0)
                return;

            foreach (var reward in platformRewards)
            {
                if (reward.platform == currentPlatform && !reward.isClaimed)
                {
                    LogDebug($"Platform reward available: {reward.rewardName}");
                }
            }
        }

        /// <summary>
        /// Claims platform-exclusive reward.
        /// </summary>
        public bool ClaimPlatformReward(string rewardId)
        {
            var reward = platformRewards.FirstOrDefault(r => r.rewardId == rewardId);
            if (reward == null)
            {
                LogDebug($"Reward not found: {rewardId}");
                return false;
            }

            if (reward.platform != currentPlatform)
            {
                LogDebug($"Reward not available on {currentPlatform}");
                return false;
            }

            if (reward.isClaimed)
            {
                LogDebug($"Reward already claimed: {rewardId}");
                return false;
            }

            reward.isClaimed = true;
            reward.claimedDate = DateTime.UtcNow;
            SavePlatformRewards();

            OnPlatformRewardClaimed?.Invoke(reward);
            LogDebug($"Claimed platform reward: {reward.rewardName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("platform_reward_claimed", new Dictionary<string, object>
                {
                    { "reward_id", rewardId },
                    { "reward_type", reward.rewardType.ToString() },
                    { "platform", currentPlatform.ToString() }
                });
            }

            return true;
        }

        /// <summary>
        /// Gets available platform rewards.
        /// </summary>
        public List<PlatformReward> GetAvailablePlatformRewards()
        {
            return platformRewards
                .Where(r => r.platform == currentPlatform && !r.isClaimed)
                .ToList();
        }

        /// <summary>
        /// Gets claimed platform rewards.
        /// </summary>
        public List<PlatformReward> GetClaimedPlatformRewards()
        {
            return platformRewards
                .Where(r => r.platform == currentPlatform && r.isClaimed)
                .ToList();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Cross-Platform Leaderboards ───────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Submits score to cross-platform leaderboard.
        /// </summary>
        public void SubmitCrossPlatformScore(string leaderboardId, int score)
        {
            var leaderboardManager = LeaderboardManager.Instance;
            if (leaderboardManager != null)
            {
                // Submit to unified leaderboard
                leaderboardManager.SubmitScore(leaderboardId, score);
                LogDebug($"Submitted cross-platform score: {score} to {leaderboardId}");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("crossplatform_score_submitted", new Dictionary<string, object>
                    {
                        { "leaderboard_id", leaderboardId },
                        { "score", score },
                        { "platform", currentPlatform.ToString() }
                    });
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Helper Methods ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Gets current platform.
        /// </summary>
        public PlatformType GetCurrentPlatform()
        {
            return currentPlatform;
        }

        /// <summary>
        /// Checks if system is initialized.
        /// </summary>
        public bool IsInitialized()
        {
            return isInitialized;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Local Storage ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LoadUnifiedAccountFromLocal()
        {
            string json = PlayerPrefs.GetString("UnifiedAccount", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    unifiedAccount = JsonUtility.FromJson<UnifiedAccount>(json);
                    LogDebug($"Loaded unified account from local: {unifiedAccount.accountId}");
                }
                catch
                {
                    LogDebug("Failed to load unified account");
                }
            }
        }

        private void SaveUnifiedAccountToLocal()
        {
            if (unifiedAccount == null)
                return;

            string json = JsonUtility.ToJson(unifiedAccount);
            PlayerPrefs.SetString("UnifiedAccount", json);
            PlayerPrefs.Save();
        }

        private void LoadLinkedPlatforms()
        {
            string json = PlayerPrefs.GetString("LinkedPlatforms", "{}");
            try
            {
                var data = JsonUtility.FromJson<LinkedPlatformsData>(json);
                if (data != null && data.platforms != null)
                {
                    linkedPlatforms = new List<LinkedPlatform>(data.platforms);
                    LogDebug($"Loaded {linkedPlatforms.Count} linked platforms");
                }
            }
            catch
            {
                LogDebug("No linked platforms found");
            }
        }

        private void SaveLinkedPlatforms()
        {
            var data = new LinkedPlatformsData
            {
                platforms = linkedPlatforms.ToArray()
            };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("LinkedPlatforms", json);
            PlayerPrefs.Save();
        }

        private void LoadFriendsList()
        {
            string json = PlayerPrefs.GetString("CrossPlatformFriends", "{}");
            try
            {
                var data = JsonUtility.FromJson<CrossPlatformFriendsList>(json);
                if (data != null && data.friends != null)
                {
                    friendsList = data;
                    LogDebug($"Loaded {friendsList.friends.Count} cross-platform friends");
                }
            }
            catch
            {
                LogDebug("No friends list found");
            }
        }

        private void SaveFriendsList()
        {
            string json = JsonUtility.ToJson(friendsList);
            PlayerPrefs.SetString("CrossPlatformFriends", json);
            PlayerPrefs.Save();
        }

        private void SavePlatformRewards()
        {
            // In production, save to backend
            // For now, rewards are in SerializeField and persist with scene
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[CrossPlatformManager] {message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ── Data Structures ───────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Platform types supported.
    /// </summary>
    public enum PlatformType
    {
        Unknown,
        Android,
        iOS,
        Steam,
        Web
    }

    /// <summary>
    /// Unified account across all platforms.
    /// </summary>
    [Serializable]
    public class UnifiedAccount
    {
        public string accountId;
        public string playerId;
        public string playerName;
        public DateTime createdDate;
        public DateTime lastSyncDate;
        public PlatformType primaryPlatform;
    }

    /// <summary>
    /// Linked platform information.
    /// </summary>
    [Serializable]
    public class LinkedPlatform
    {
        public PlatformType platform;
        public string platformUserId;
        public DateTime linkedDate;
        public bool isActive;
    }

    /// <summary>
    /// Cross-platform friend data.
    /// </summary>
    [Serializable]
    public class CrossPlatformFriend
    {
        public string friendId;
        public string friendName;
        public PlatformType platform;
        public DateTime addedDate;
        public DateTime lastSeenDate;
        public bool isOnline;
    }

    /// <summary>
    /// Platform-exclusive reward.
    /// </summary>
    [Serializable]
    public class PlatformReward
    {
        public string rewardId;
        public string rewardName;
        public PlatformType platform;
        public RewardType rewardType;
        public int rewardAmount;
        public bool isClaimed;
        public DateTime claimedDate;
    }

    /// <summary>
    /// Reward types for platform rewards.
    /// </summary>
    public enum RewardType
    {
        Gems,
        Credits,
        TowerSkin,
        Cosmetic,
        Avatar,
        Banner,
        Title
    }

    /// <summary>
    /// Friends list container.
    /// </summary>
    [Serializable]
    public class CrossPlatformFriendsList
    {
        public List<CrossPlatformFriend> friends = new List<CrossPlatformFriend>();
    }

    /// <summary>
    /// Linked platforms serialization helper.
    /// </summary>
    [Serializable]
    public class LinkedPlatformsData
    {
        public LinkedPlatform[] platforms;
    }
}
