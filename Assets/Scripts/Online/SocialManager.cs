using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RobotTD.Online
{
    /// <summary>
    /// Manages social features: friends, sharing, and social interactions.
    /// Integrates with authentication and leaderboard systems.
    /// </summary>
    public class SocialManager : MonoBehaviour
    {
        public static SocialManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableSocialFeatures = true;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private int maxFriends = 100;
        [SerializeField] private int maxPendingRequests = 50;
        [SerializeField] private float friendRequestTimeoutDays = 30f;

        [Header("Backend Selection")]
        [SerializeField] private SocialBackend backend = SocialBackend.Unity;
        [SerializeField] private string customBackendUrl = "https://api.yourgame.com";

        // Friend lists
        private List<FriendInfo> friends = new List<FriendInfo>();
        private List<FriendRequest> pendingRequests = new List<FriendRequest>();
        private List<FriendRequest> sentRequests = new List<FriendRequest>();
        private Dictionary<string, DateTime> recentActivity = new Dictionary<string, DateTime>();

        // Local player info
        private string localPlayerId;
        private string localPlayerName;

        // Events
        public event Action OnFriendsListUpdated;
        public event Action<FriendRequest> OnFriendRequestReceived;
        public event Action<FriendInfo> OnFriendAdded;
        public event Action<string> OnFriendRemoved;
        public event Action<ShareResult> OnScoreShared;
        public event Action<ShareResult> OnAchievementShared;
        public event Action<string> OnError;

        // Properties
        public List<FriendInfo> Friends => new List<FriendInfo>(friends);
        public List<FriendRequest> PendingRequests => new List<FriendRequest>(pendingRequests);
        public int FriendCount => friends.Count;
        public bool CanAddMoreFriends => friends.Count < maxFriends;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        // ── Initialization ────────────────────────────────────────────────────

        private void Initialize()
        {
            if (!enableSocialFeatures)
            {
                LogDebug("Social features disabled");
                return;
            }

            // Get player info from AuthenticationManager
            if (AuthenticationManager.Instance != null && AuthenticationManager.Instance.IsAuthenticated)
            {
                localPlayerId = AuthenticationManager.Instance.PlayerId;
                localPlayerName = AuthenticationManager.Instance.PlayerName;
                LogDebug($"Social system initialized for {localPlayerName} ({localPlayerId})");
            }
            else
            {
                // Fall back to local player ID
                localPlayerId = PlayerPrefs.GetString("PlayerId", Guid.NewGuid().ToString());
                localPlayerName = PlayerPrefs.GetString("PlayerName", $"Player_{UnityEngine.Random.Range(1000, 9999)}");
                LogDebug("Social system initialized with local ID (not authenticated)");
            }

            LoadLocalFriendData();
            InitializeBackend();

            // Subscribe to auth events
            if (AuthenticationManager.Instance != null)
            {
                AuthenticationManager.Instance.OnSignInSuccess += HandleSignInSuccess;
                AuthenticationManager.Instance.OnSignOut += HandleSignOut;
            }

            LogDebug($"Social features initialized - {friends.Count} friends, {pendingRequests.Count} pending requests");
        }

        private void OnDestroy()
        {
            if (AuthenticationManager.Instance != null)
            {
                AuthenticationManager.Instance.OnSignInSuccess -= HandleSignInSuccess;
                AuthenticationManager.Instance.OnSignOut -= HandleSignOut;
            }
        }

        private void InitializeBackend()
        {
#if UNITY_EDITOR
            LogDebug($"Backend initialized: {backend} (Editor mode - using local storage)");
#else
            switch (backend)
            {
                case SocialBackend.Unity:
                    InitializeUnityBackend();
                    break;
                case SocialBackend.PlayFab:
                    InitializePlayFabBackend();
                    break;
                case SocialBackend.Custom:
                    InitializeCustomBackend();
                    break;
            }
#endif
        }

        private void InitializeUnityBackend()
        {
            // TODO: Initialize Unity Gaming Services Friends API
            LogDebug("Unity Gaming Services social backend initialized");
        }

        private void InitializePlayFabBackend()
        {
            // TODO: Initialize PlayFab Friends API
            LogDebug("PlayFab social backend initialized");
        }

        private void InitializeCustomBackend()
        {
            // TODO: Initialize custom backend connection
            LogDebug($"Custom social backend initialized: {customBackendUrl}");
        }

        // ── Friend Management ─────────────────────────────────────────────────

        /// <summary>
        /// Send a friend request to another player by their ID or username.
        /// </summary>
        public void SendFriendRequest(string targetIdentifier)
        {
            if (!enableSocialFeatures)
            {
                OnError?.Invoke("Social features are disabled");
                return;
            }

            if (string.IsNullOrEmpty(targetIdentifier))
            {
                OnError?.Invoke("Invalid player identifier");
                return;
            }

            if (targetIdentifier == localPlayerId)
            {
                OnError?.Invoke("Cannot add yourself as a friend");
                return;
            }

            // Check if already friends
            if (friends.Any(f => f.playerId == targetIdentifier || f.playerName.Equals(targetIdentifier, StringComparison.OrdinalIgnoreCase)))
            {
                OnError?.Invoke("Already friends with this player");
                return;
            }

            // Check if request already sent
            if (sentRequests.Any(r => r.targetPlayerId == targetIdentifier && r.status == FriendRequestStatus.Pending))
            {
                OnError?.Invoke("Friend request already sent");
                return;
            }

            if (!CanAddMoreFriends)
            {
                OnError?.Invoke($"Friend list is full (max {maxFriends})");
                return;
            }

            // Create friend request
            var request = new FriendRequest
            {
                requestId = Guid.NewGuid().ToString(),
                senderPlayerId = localPlayerId,
                senderPlayerName = localPlayerName,
                targetPlayerId = targetIdentifier,
                timestamp = DateTime.UtcNow,
                status = FriendRequestStatus.Pending
            };

            sentRequests.Add(request);
            SaveLocalFriendData();

            // Send to backend
            StartCoroutine(SendFriendRequestToBackend(request));

            LogDebug($"Friend request sent to {targetIdentifier}");

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("friend_request_sent", new Dictionary<string, object>
                {
                    { "target_id", targetIdentifier }
                });
            }
        }

        /// <summary>
        /// Accept a pending friend request.
        /// </summary>
        public void AcceptFriendRequest(string requestId)
        {
            var request = pendingRequests.FirstOrDefault(r => r.requestId == requestId);
            if (request == null)
            {
                OnError?.Invoke("Friend request not found");
                return;
            }

            if (!CanAddMoreFriends)
            {
                OnError?.Invoke($"Friend list is full (max {maxFriends})");
                return;
            }

            // Add friend
            var friend = new FriendInfo
            {
                playerId = request.senderPlayerId,
                playerName = request.senderPlayerName,
                friendSince = DateTime.UtcNow,
                lastSeenOnline = DateTime.UtcNow,
                isOnline = false
            };

            friends.Add(friend);
            pendingRequests.Remove(request);
            SaveLocalFriendData();

            OnFriendAdded?.Invoke(friend);
            OnFriendsListUpdated?.Invoke();

            // Update backend
            StartCoroutine(AcceptFriendRequestOnBackend(requestId));

            LogDebug($"Friend request accepted: {friend.playerName}");

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("friend_request_accepted", new Dictionary<string, object>
                {
                    { "friend_id", friend.playerId },
                    { "total_friends", friends.Count }
                });
            }
        }

        /// <summary>
        /// Decline a pending friend request.
        /// </summary>
        public void DeclineFriendRequest(string requestId)
        {
            var request = pendingRequests.FirstOrDefault(r => r.requestId == requestId);
            if (request == null)
            {
                OnError?.Invoke("Friend request not found");
                return;
            }

            pendingRequests.Remove(request);
            SaveLocalFriendData();

            // Update backend
            StartCoroutine(DeclineFriendRequestOnBackend(requestId));

            LogDebug($"Friend request declined from {request.senderPlayerName}");

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("friend_request_declined", new Dictionary<string, object>
                {
                    { "sender_id", request.senderPlayerId }
                });
            }
        }

        /// <summary>
        /// Remove a friend from the friends list.
        /// </summary>
        public void RemoveFriend(string playerId)
        {
            var friend = friends.FirstOrDefault(f => f.playerId == playerId);
            if (friend == null)
            {
                OnError?.Invoke("Friend not found");
                return;
            }

            friends.Remove(friend);
            SaveLocalFriendData();

            OnFriendRemoved?.Invoke(playerId);
            OnFriendsListUpdated?.Invoke();

            // Update backend
            StartCoroutine(RemoveFriendOnBackend(playerId));

            LogDebug($"Friend removed: {friend.playerName}");

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("friend_removed", new Dictionary<string, object>
                {
                    { "friend_id", playerId },
                    { "total_friends", friends.Count }
                });
            }
        }

        /// <summary>
        /// Refresh the friends list from the backend.
        /// </summary>
        public void RefreshFriendsList()
        {
            StartCoroutine(FetchFriendsFromBackend());
        }

        /// <summary>
        /// Search for players by username or ID.
        /// </summary>
        public void SearchPlayers(string searchQuery, Action<List<PlayerSearchResult>> callback)
        {
            if (string.IsNullOrEmpty(searchQuery) || searchQuery.Length < 3)
            {
                callback?.Invoke(new List<PlayerSearchResult>());
                return;
            }

            StartCoroutine(SearchPlayersOnBackend(searchQuery, callback));
        }

        // ── Score Sharing ─────────────────────────────────────────────────────

        /// <summary>
        /// Share a score with friends.
        /// </summary>
        public void ShareScore(int score, string gameMode, string mapName = "")
        {
            if (!enableSocialFeatures)
            {
                OnError?.Invoke("Social features are disabled");
                return;
            }

            var shareData = new ScoreShareData
            {
                playerId = localPlayerId,
                playerName = localPlayerName,
                score = score,
                gameMode = gameMode,
                mapName = mapName,
                timestamp = DateTime.UtcNow
            };

            StartCoroutine(ShareScoreToBackend(shareData));

            LogDebug($"Score shared: {score} in {gameMode}");

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent(Analytics.AnalyticsEvents.SCORE_SHARED, new Dictionary<string, object>
                {
                    { "score", score },
                    { "game_mode", gameMode },
                    { "map_name", mapName },
                    { "friend_count", friends.Count }
                });
            }
        }

        /// <summary>
        /// Share an achievement unlock with friends.
        /// </summary>
        public void ShareAchievement(string achievementId, string achievementName)
        {
            if (!enableSocialFeatures)
            {
                OnError?.Invoke("Social features are disabled");
                return;
            }

            var shareData = new AchievementShareData
            {
                playerId = localPlayerId,
                playerName = localPlayerName,
                achievementId = achievementId,
                achievementName = achievementName,
                timestamp = DateTime.UtcNow
            };

            StartCoroutine(ShareAchievementToBackend(shareData));

            LogDebug($"Achievement shared: {achievementName}");

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent(Analytics.AnalyticsEvents.ACHIEVEMENT_SHARED, new Dictionary<string, object>
                {
                    { "achievement_id", achievementId },
                    { "achievement_name", achievementName },
                    { "friend_count", friends.Count }
                });
            }
        }

        // ── Friend Activity ───────────────────────────────────────────────────

        /// <summary>
        /// Get recent activity from friends (scores, achievements, etc.).
        /// </summary>
        public void GetFriendActivity(int maxResults, Action<List<FriendActivity>> callback)
        {
            StartCoroutine(FetchFriendActivityFromBackend(maxResults, callback));
        }

        /// <summary>
        /// Update the player's online status.
        /// </summary>
        public void UpdateOnlineStatus(bool isOnline)
        {
            StartCoroutine(UpdateOnlineStatusOnBackend(isOnline));
        }

        // ── Query Methods ─────────────────────────────────────────────────────

        /// <summary>
        /// Check if a player is a friend.
        /// </summary>
        public bool IsFriend(string playerId)
        {
            return friends.Any(f => f.playerId == playerId);
        }

        /// <summary>
        /// Get friend info by player ID.
        /// </summary>
        public FriendInfo GetFriend(string playerId)
        {
            return friends.FirstOrDefault(f => f.playerId == playerId);
        }

        /// <summary>
        /// Get online friends count.
        /// </summary>
        public int GetOnlineFriendsCount()
        {
            return friends.Count(f => f.isOnline);
        }

        /// <summary>
        /// Get friend IDs for leaderboard queries.
        /// </summary>
        public List<string> GetFriendIds()
        {
            return friends.Select(f => f.playerId).ToList();
        }

        // ── Backend Communication ─────────────────────────────────────────────

        private IEnumerator SendFriendRequestToBackend(FriendRequest request)
        {
#if UNITY_EDITOR
            // Editor simulation
            yield return new WaitForSeconds(0.5f);
            LogDebug("Friend request sent (editor simulation)");
#else
            // TODO: Implement actual backend call based on selected backend
            yield return null;
#endif
        }

        private IEnumerator AcceptFriendRequestOnBackend(string requestId)
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(0.5f);
            LogDebug("Friend request accepted on backend (editor simulation)");
#else
            // TODO: Implement actual backend call
            yield return null;
#endif
        }

        private IEnumerator DeclineFriendRequestOnBackend(string requestId)
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(0.5f);
            LogDebug("Friend request declined on backend (editor simulation)");
#else
            // TODO: Implement actual backend call
            yield return null;
#endif
        }

        private IEnumerator RemoveFriendOnBackend(string playerId)
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(0.5f);
            LogDebug("Friend removed on backend (editor simulation)");
#else
            // TODO: Implement actual backend call
            yield return null;
#endif
        }

        private IEnumerator FetchFriendsFromBackend()
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(1f);
            LogDebug("Friends list refreshed (editor simulation)");
            OnFriendsListUpdated?.Invoke();
#else
            // TODO: Implement actual backend call
            yield return null;
#endif
        }

        private IEnumerator SearchPlayersOnBackend(string searchQuery, Action<List<PlayerSearchResult>> callback)
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(0.5f);
            // Editor simulation - return mock results
            var results = new List<PlayerSearchResult>
            {
                new PlayerSearchResult 
                { 
                    playerId = "player_001", 
                    playerName = $"TestPlayer{searchQuery}", 
                    isOnline = true,
                    isFriend = false
                }
            };
            callback?.Invoke(results);
#else
            // TODO: Implement actual backend search
            yield return null;
            callback?.Invoke(new List<PlayerSearchResult>());
#endif
        }

        private IEnumerator ShareScoreToBackend(ScoreShareData shareData)
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(0.5f);
            var result = new ShareResult { success = true, message = "Score shared successfully (editor)" };
            OnScoreShared?.Invoke(result);
#else
            // TODO: Implement actual backend call
            yield return null;
#endif
        }

        private IEnumerator ShareAchievementToBackend(AchievementShareData shareData)
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(0.5f);
            var result = new ShareResult { success = true, message = "Achievement shared successfully (editor)" };
            OnAchievementShared?.Invoke(result);
#else
            // TODO: Implement actual backend call
            yield return null;
#endif
        }

        private IEnumerator FetchFriendActivityFromBackend(int maxResults, Action<List<FriendActivity>> callback)
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(0.5f);
            // Editor simulation
            callback?.Invoke(new List<FriendActivity>());
#else
            // TODO: Implement actual backend call
            yield return null;
            callback?.Invoke(new List<FriendActivity>());
#endif
        }

        private IEnumerator UpdateOnlineStatusOnBackend(bool isOnline)
        {
#if UNITY_EDITOR
            yield return new WaitForSeconds(0.2f);
            LogDebug($"Online status updated: {isOnline} (editor simulation)");
#else
            // TODO: Implement actual backend call
            yield return null;
#endif
        }

        // ── Event Handlers ────────────────────────────────────────────────────

        private void HandleSignInSuccess(string playerId, string playerName)
        {
            localPlayerId = playerId;
            localPlayerName = playerName;
            RefreshFriendsList();
            UpdateOnlineStatus(true);
        }

        private void HandleSignOut()
        {
            UpdateOnlineStatus(false);
            friends.Clear();
            pendingRequests.Clear();
            sentRequests.Clear();
            SaveLocalFriendData();
        }

        // ── Persistence ───────────────────────────────────────────────────────

        private void LoadLocalFriendData()
        {
            string json = PlayerPrefs.GetString("SocialData", "{}");
            
            try
            {
                var data = JsonUtility.FromJson<SocialSaveData>(json);
                if (data != null)
                {
                    friends = data.friends != null ? new List<FriendInfo>(data.friends) : new List<FriendInfo>();
                    pendingRequests = data.pendingRequests != null ? new List<FriendRequest>(data.pendingRequests) : new List<FriendRequest>();
                    sentRequests = data.sentRequests != null ? new List<FriendRequest>(data.sentRequests) : new List<FriendRequest>();
                    
                    // Clean up expired requests
                    CleanupExpiredRequests();
                    
                    LogDebug($"Loaded {friends.Count} friends, {pendingRequests.Count} pending requests");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocialManager] Failed to load friend data: {e.Message}");
            }
        }

        private void SaveLocalFriendData()
        {
            var data = new SocialSaveData
            {
                friends = friends.ToArray(),
                pendingRequests = pendingRequests.ToArray(),
                sentRequests = sentRequests.ToArray()
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("SocialData", json);
            PlayerPrefs.Save();
        }

        private void CleanupExpiredRequests()
        {
            DateTime cutoff = DateTime.UtcNow.AddDays(-friendRequestTimeoutDays);
            
            pendingRequests.RemoveAll(r => r.timestamp < cutoff);
            sentRequests.RemoveAll(r => r.timestamp < cutoff && r.status == FriendRequestStatus.Pending);
        }

        // ── Utility ───────────────────────────────────────────────────────────

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SocialManager] {message}");
            }
        }

        // ── Context Menu Commands ─────────────────────────────────────────────

#if UNITY_EDITOR
        [ContextMenu("Add Test Friend")]
        private void DEBUG_AddTestFriend()
        {
            var friend = new FriendInfo
            {
                playerId = $"test_friend_{friends.Count + 1}",
                playerName = $"TestFriend{friends.Count + 1}",
                friendSince = DateTime.UtcNow,
                lastSeenOnline = DateTime.UtcNow,
                isOnline = UnityEngine.Random.value > 0.5f
            };
            friends.Add(friend);
            SaveLocalFriendData();
            OnFriendsListUpdated?.Invoke();
            Debug.Log($"[SocialManager] Added test friend: {friend.playerName}");
        }

        [ContextMenu("Add Test Friend Request")]
        private void DEBUG_AddTestRequest()
        {
            var request = new FriendRequest
            {
                requestId = Guid.NewGuid().ToString(),
                senderPlayerId = $"test_player_{pendingRequests.Count + 1}",
                senderPlayerName = $"TestPlayer{pendingRequests.Count + 1}",
                targetPlayerId = localPlayerId,
                timestamp = DateTime.UtcNow,
                status = FriendRequestStatus.Pending
            };
            pendingRequests.Add(request);
            SaveLocalFriendData();
            OnFriendRequestReceived?.Invoke(request);
            Debug.Log($"[SocialManager] Added test friend request from {request.senderPlayerName}");
        }

        [ContextMenu("Clear All Friends")]
        private void DEBUG_ClearFriends()
        {
            friends.Clear();
            pendingRequests.Clear();
            sentRequests.Clear();
            SaveLocalFriendData();
            OnFriendsListUpdated?.Invoke();
            Debug.Log("[SocialManager] Cleared all friends and requests");
        }

        [ContextMenu("Print Friend List")]
        private void DEBUG_PrintFriends()
        {
            Debug.Log($"[SocialManager] Friends ({friends.Count}):");
            foreach (var friend in friends)
            {
                Debug.Log($"  - {friend.playerName} ({friend.playerId}) - Online: {friend.isOnline}");
            }
            Debug.Log($"[SocialManager] Pending Requests ({pendingRequests.Count}):");
            foreach (var request in pendingRequests)
            {
                Debug.Log($"  - From {request.senderPlayerName} ({request.senderPlayerId})");
            }
        }
#endif
    }

    // ── Data Structures ───────────────────────────────────────────────────────

    public enum SocialBackend
    {
        Unity,          // Unity Gaming Services
        PlayFab,        // PlayFab
        Custom          // Custom HTTP backend
    }

    [Serializable]
    public class FriendInfo
    {
        public string playerId;
        public string playerName;
        public DateTime friendSince;
        public DateTime lastSeenOnline;
        public bool isOnline;
        public int level;
        public string avatarUrl;
    }

    [Serializable]
    public class FriendRequest
    {
        public string requestId;
        public string senderPlayerId;
        public string senderPlayerName;
        public string targetPlayerId;
        public DateTime timestamp;
        public FriendRequestStatus status;
    }

    public enum FriendRequestStatus
    {
        Pending,
        Accepted,
        Declined,
        Expired
    }

    [Serializable]
    public class PlayerSearchResult
    {
        public string playerId;
        public string playerName;
        public bool isOnline;
        public bool isFriend;
        public int level;
        public string avatarUrl;
    }

    [Serializable]
    public class ScoreShareData
    {
        public string playerId;
        public string playerName;
        public int score;
        public string gameMode;
        public string mapName;
        public DateTime timestamp;
    }

    [Serializable]
    public class AchievementShareData
    {
        public string playerId;
        public string playerName;
        public string achievementId;
        public string achievementName;
        public DateTime timestamp;
    }

    [Serializable]
    public class FriendActivity
    {
        public string friendId;
        public string friendName;
        public FriendActivityType activityType;
        public string activityData;
        public DateTime timestamp;
    }

    public enum FriendActivityType
    {
        ScorePosted,
        AchievementUnlocked,
        LevelUp,
        GameCompleted,
        ChallengeCompleted
    }

    [Serializable]
    public class ShareResult
    {
        public bool success;
        public string message;
    }

    [Serializable]
    public class SocialSaveData
    {
        public FriendInfo[] friends;
        public FriendRequest[] pendingRequests;
        public FriendRequest[] sentRequests;
    }
}
