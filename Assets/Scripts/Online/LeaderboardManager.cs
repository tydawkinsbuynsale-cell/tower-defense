using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RobotTD.Online
{
    /// <summary>
    /// Manages leaderboards for competitive scoring across game modes.
    /// Supports multiple backends: Unity Gaming Services, PlayFab, or custom server.
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableLeaderboards = true;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool offlineMode = false; // Use local scores only
        [SerializeField] private int maxLocalScores = 100;
        [SerializeField] private int maxFetchedScores = 50;

        [Header("Leaderboard IDs")]
        [SerializeField] private string endlessLeaderboardId = "endless_high_score";
        [SerializeField] private string bossRushLeaderboardId = "boss_rush_score";
        [SerializeField] private string dailyLeaderboardId = "daily_challenge";
        [SerializeField] private string weeklyLeaderboardId = "weekly_challenge";

        // Local cache
        private Dictionary<string, List<LeaderboardEntry>> localScores = new Dictionary<string, List<LeaderboardEntry>>();
        private Dictionary<string, List<LeaderboardEntry>> cachedOnlineScores = new Dictionary<string, List<LeaderboardEntry>>();
        private Dictionary<string, DateTime> cacheExpiration = new Dictionary<string, DateTime>();
        private TimeSpan cacheLifetime = TimeSpan.FromMinutes(5);

        // Player data
        private string playerId;
        private string playerName;

        // Events
        public event Action<string, bool> OnScoreSubmitted; // leaderboardId, success
        public event Action<string, List<LeaderboardEntry>> OnScoresLoaded;
        public event Action<string> OnError;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeLeaderboards();
        }

        // ── Initialization ────────────────────────────────────────────────────

        private void InitializeLeaderboards()
        {
            if (!enableLeaderboards)
            {
                LogDebug("Leaderboards disabled");
                return;
            }

            // Check if AuthenticationManager is available
            var authManager = AuthenticationManager.Instance;
            if (authManager != null && authManager.IsAuthenticated)
            {
                // Use authenticated player info
                playerId = authManager.PlayerId;
                playerName = authManager.PlayerName;
                LogDebug($"Using authenticated player: {playerName} ({playerId})");
            }
            else
            {
                // Fall back to local player ID
                playerId = GetOrCreatePlayerId();
                playerName = GetPlayerName();
                LogDebug($"Using local player ID (not authenticated): {playerName} ({playerId})");
            }

            // Load local scores from PlayerPrefs
            LoadLocalScores();

            // Initialize backend
            if (!offlineMode)
            {
                InitializeBackend();
            }

            LogDebug($"Leaderboards initialized - Player: {playerName} ({playerId})");
        }

        private string GetOrCreatePlayerId()
        {
            string id = PlayerPrefs.GetString("LeaderboardPlayerId", "");
            
            if (string.IsNullOrEmpty(id))
            {
                // Generate new ID
                id = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString("LeaderboardPlayerId", id);
                PlayerPrefs.Save();
            }

            return id;
        }

        private string GetPlayerName()
        {
            string name = PlayerPrefs.GetString("PlayerName", "");
            
            if (string.IsNullOrEmpty(name))
            {
                // Generate default name
                name = $"Player{UnityEngine.Random.Range(1000, 9999)}";
                PlayerPrefs.SetString("PlayerName", name);
                PlayerPrefs.Save();
            }

            return name;
        }

        public void SetPlayerName(string newName)
        {
            if (string.IsNullOrEmpty(newName) || newName.Length < 3)
            {
                LogDebug("Player name must be at least 3 characters");
                return;
            }

            // Sanitize name
            newName = SanitizeName(newName);
            
            playerName = newName;
            PlayerPrefs.SetString("PlayerName", newName);
            PlayerPrefs.Save();

            LogDebug($"Player name set to: {playerName}");
        }

        private string SanitizeName(string name)
        {
            // Remove special characters, limit length
            name = name.Trim();
            name = System.Text.RegularExpressions.Regex.Replace(name, @"[^a-zA-Z0-9_\- ]", "");
            if (name.Length > 20)
                name = name.Substring(0, 20);
            return name;
        }

        private void InitializeBackend()
        {
            // TODO: Initialize your chosen backend
            // Example integrations shown in methods below

            #if UNITY_GAMING_SERVICES
            InitializeUnityGamingServices();
            #elif PLAYFAB
            InitializePlayFab();
            #else
            LogDebug("No backend defined, using offline mode");
            offlineMode = true;
            #endif
        }

        // ── Score Submission ──────────────────────────────────────────────────

        /// <summary>
        /// Submit a score to the specified leaderboard.
        /// </summary>
        public void SubmitScore(string leaderboardId, long score, Dictionary<string, object> metadata = null)
        {
            if (!enableLeaderboards)
                return;

            // Save locally first
            SaveScoreLocally(leaderboardId, score, metadata);

            // Track with analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("leaderboard_score_submit", new Dictionary<string, object>
                {
                    { "leaderboard_id", leaderboardId },
                    { "score", score }
                });
            }

            // Submit online if not in offline mode
            if (!offlineMode)
            {
                StartCoroutine(SubmitScoreOnline(leaderboardId, score, metadata));
            }
            else
            {
                OnScoreSubmitted?.Invoke(leaderboardId, true);
            }

            LogDebug($"Score submitted to {leaderboardId}: {score}");
        }

        /// <summary>
        /// Submit endless mode high score.
        /// </summary>
        public void SubmitEndlessScore(int wave, long score)
        {
            var metadata = new Dictionary<string, object>
            {
                { "wave", wave },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SubmitScore(endlessLeaderboardId, score, metadata);
        }

        /// <summary>        /// Submit boss rush mode score.
        /// </summary>
        public void SubmitBossRushScore(int bossesDefeated, long score)
        {
            var metadata = new Dictionary<string, object>
            {
                { "bosses_defeated", bossesDefeated },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            SubmitScore(bossRushLeaderboardId, score, metadata);
        }

        /// <summary>        /// Submit daily challenge score.
        /// </summary>
        public void SubmitDailyScore(long score, string challengeId)
        {
            var metadata = new Dictionary<string, object>
            {
                { "challenge_id", challengeId },
                { "date", DateTime.UtcNow.ToString("yyyy-MM-dd") }
            };

            SubmitScore(dailyLeaderboardId, score, metadata);
        }

        private void SaveScoreLocally(string leaderboardId, long score, Dictionary<string, object> metadata)
        {
            if (!localScores.ContainsKey(leaderboardId))
                localScores[leaderboardId] = new List<LeaderboardEntry>();

            var entry = new LeaderboardEntry
            {
                playerId = playerId,
                playerName = playerName,
                score = score,
                rank = 0, // Will be calculated when sorted
                timestamp = DateTime.UtcNow,
                metadata = metadata ?? new Dictionary<string, object>()
            };

            // Check if player already has a score
            var existingIndex = localScores[leaderboardId].FindIndex(e => e.playerId == playerId);
            
            if (existingIndex >= 0)
            {
                // Update if new score is higher
                if (score > localScores[leaderboardId][existingIndex].score)
                {
                    localScores[leaderboardId][existingIndex] = entry;
                    LogDebug($"Updated local high score: {score}");
                }
            }
            else
            {
                // Add new entry
                localScores[leaderboardId].Add(entry);
            }

            // Sort and trim
            localScores[leaderboardId] = localScores[leaderboardId]
                .OrderByDescending(e => e.score)
                .Take(maxLocalScores)
                .ToList();

            // Update ranks
            for (int i = 0; i < localScores[leaderboardId].Count; i++)
            {
                var e = localScores[leaderboardId][i];
                e.rank = i + 1;
                localScores[leaderboardId][i] = e;
            }

            // Save to PlayerPrefs
            SaveLocalScoresToPrefs(leaderboardId);
        }

        private IEnumerator SubmitScoreOnline(string leaderboardId, long score, Dictionary<string, object> metadata)
        {
            // Backend-specific implementation
            #if UNITY_GAMING_SERVICES
            yield return SubmitScoreUGS(leaderboardId, score);
            #elif PLAYFAB
            yield return SubmitScorePlayFab(leaderboardId, score);
            #else
            yield return SubmitScoreCustom(leaderboardId, score, metadata);
            #endif
        }

        // ── Score Fetching ────────────────────────────────────────────────────

        /// <summary>
        /// Fetch top scores for a leaderboard.
        /// Returns cached scores if available and not expired.
        /// </summary>
        public void FetchLeaderboard(string leaderboardId, LeaderboardScope scope = LeaderboardScope.Global, int maxResults = 50)
        {
            if (!enableLeaderboards)
                return;

            // Check cache first
            if (IsCacheValid(leaderboardId))
            {
                LogDebug($"Returning cached scores for {leaderboardId}");
                OnScoresLoaded?.Invoke(leaderboardId, cachedOnlineScores[leaderboardId]);
                return;
            }

            // Fetch online or use local
            if (!offlineMode)
            {
                StartCoroutine(FetchLeaderboardOnline(leaderboardId, scope, maxResults));
            }
            else
            {
                // Return local scores
                if (localScores.ContainsKey(leaderboardId))
                {
                    OnScoresLoaded?.Invoke(leaderboardId, localScores[leaderboardId]);
                }
                else
                {
                    OnScoresLoaded?.Invoke(leaderboardId, new List<LeaderboardEntry>());
                }
            }
        }

        /// <summary>
        /// Fetch player's current rank on a leaderboard.
        /// </summary>
        public void FetchPlayerRank(string leaderboardId)
        {
            if (!enableLeaderboards)
                return;

            if (!offlineMode)
            {
                StartCoroutine(FetchPlayerRankOnline(leaderboardId));
            }
            else
            {
                // Return local rank
                if (localScores.ContainsKey(leaderboardId))
                {
                    var entry = localScores[leaderboardId].FirstOrDefault(e => e.playerId == playerId);
                    if (entry != null)
                    {
                        LogDebug($"Local rank: {entry.rank}");
                    }
                }
            }
        }

        /// <summary>
        /// Fetch scores near the player's rank.
        /// </summary>
        public void FetchScoresNearPlayer(string leaderboardId, int range = 5)
        {
            if (!enableLeaderboards)
                return;

            if (!offlineMode)
            {
                StartCoroutine(FetchScoresNearPlayerOnline(leaderboardId, range));
            }
            else
            {
                // Return local scores near player
                if (localScores.ContainsKey(leaderboardId))
                {
                    var entry = localScores[leaderboardId].FirstOrDefault(e => e.playerId == playerId);
                    if (entry != null)
                    {
                        int startIndex = Math.Max(0, entry.rank - range - 1);
                        int count = range * 2 + 1;
                        var nearbyScores = localScores[leaderboardId].Skip(startIndex).Take(count).ToList();
                        OnScoresLoaded?.Invoke(leaderboardId, nearbyScores);
                    }
                }
            }
        }

        /// <summary>
        /// Fetch friend leaderboard scores.
        /// Requires SocialManager to be initialized with friends list.
        /// </summary>
        public void FetchFriendLeaderboard(string leaderboardId, int maxResults = 50)
        {
            if (!enableLeaderboards)
                return;

            // Get friend IDs from SocialManager
            if (SocialManager.Instance == null)
            {
                LogDebug("SocialManager not available, cannot fetch friend leaderboard");
                OnScoresLoaded?.Invoke(leaderboardId, new List<LeaderboardEntry>());
                return;
            }

            List<string> friendIds = SocialManager.Instance.GetFriendIds();
            
            if (friendIds == null || friendIds.Count == 0)
            {
                LogDebug("No friends found, returning empty leaderboard");
                OnScoresLoaded?.Invoke(leaderboardId, new List<LeaderboardEntry>());
                return;
            }

            // Add player's own ID to include them in results
            if (!friendIds.Contains(playerId))
            {
                friendIds.Add(playerId);
            }

            if (!offlineMode)
            {
                StartCoroutine(FetchFriendLeaderboardOnline(leaderboardId, friendIds, maxResults));
            }
            else
            {
                // Filter local scores by friend IDs
                if (localScores.ContainsKey(leaderboardId))
                {
                    var friendScores = localScores[leaderboardId]
                        .Where(e => friendIds.Contains(e.playerId))
                        .OrderByDescending(e => e.score)
                        .Take(maxResults)
                        .ToList();

                    // Recalculate ranks for friend-only list
                    for (int i = 0; i < friendScores.Count; i++)
                    {
                        friendScores[i].rank = i + 1;
                    }

                    OnScoresLoaded?.Invoke(leaderboardId, friendScores);
                }
                else
                {
                    OnScoresLoaded?.Invoke(leaderboardId, new List<LeaderboardEntry>());
                }
            }

            LogDebug($"Fetching friend leaderboard for {friendIds.Count} friends");
        }

        private IEnumerator FetchLeaderboardOnline(string leaderboardId, LeaderboardScope scope, int maxResults)
        {
            #if UNITY_GAMING_SERVICES
            yield return FetchLeaderboardUGS(leaderboardId, scope, maxResults);
            #elif PLAYFAB
            yield return FetchLeaderboardPlayFab(leaderboardId, maxResults);
            #else
            yield return FetchLeaderboardCustom(leaderboardId, scope, maxResults);
            #endif
        }

        private IEnumerator FetchPlayerRankOnline(string leaderboardId)
        {
            // Implementation depends on backend
            yield return null;
            LogDebug($"Player rank fetch not implemented for current backend");
        }

        private IEnumerator FetchScoresNearPlayerOnline(string leaderboardId, int range)
        {
            // Implementation depends on backend
            yield return null;
            LogDebug($"Nearby scores fetch not implemented for current backend");
        }

        private IEnumerator FetchFriendLeaderboardOnline(string leaderboardId, List<string> friendIds, int maxResults)
        {
            #if UNITY_GAMING_SERVICES
            yield return FetchFriendLeaderboardUGS(leaderboardId, friendIds, maxResults);
            #elif PLAYFAB
            yield return FetchFriendLeaderboardPlayFab(leaderboardId, friendIds, maxResults);
            #else
            yield return FetchFriendLeaderboardCustom(leaderboardId, friendIds, maxResults);
            #endif
        }

        // ── Backend Implementations ───────────────────────────────────────────

        #region Unity Gaming Services

        #if UNITY_GAMING_SERVICES
        private void InitializeUnityGamingServices()
        {
            // Initialize Unity Gaming Services
            // Requires: com.unity.services.core, com.unity.services.leaderboards
            LogDebug("Initializing Unity Gaming Services...");
            // Unity.Services.Core.UnityServices.InitializeAsync();
        }

        private IEnumerator SubmitScoreUGS(string leaderboardId, long score)
        {
            LogDebug($"[UGS] Submitting score: {score}");
            // await Unity.Services.Leaderboards.LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            OnScoreSubmitted?.Invoke(leaderboardId, true);
            yield return null;
        }

        private IEnumerator FetchLeaderboardUGS(string leaderboardId, LeaderboardScope scope, int maxResults)
        {
            LogDebug($"[UGS] Fetching leaderboard: {leaderboardId}");
            // var result = await Unity.Services.Leaderboards.LeaderboardsService.Instance.GetScoresAsync(leaderboardId);
            // Convert and cache results
            yield return null;
        }

        private IEnumerator FetchFriendLeaderboardUGS(string leaderboardId, List<string> friendIds, int maxResults)
        {
            LogDebug($"[UGS] Fetching friend leaderboard: {leaderboardId}");
            // Unity Gaming Services friend leaderboard implementation
            // var result = await Unity.Services.Leaderboards.LeaderboardsService.Instance.GetPlayerRangeAsync(leaderboardId, friendIds);
            // List<LeaderboardEntry> entries = ConvertUGSResults(result);
            // CacheScores(leaderboardId + "_friends", entries);
            // OnScoresLoaded?.Invoke(leaderboardId, entries);
            yield return null;
        }
        #endif

        #endregion

        #region PlayFab

        #if PLAYFAB
        private void InitializePlayFab()
        {
            LogDebug("Initializing PlayFab...");
            // PlayFab.PlayFabSettings.TitleId = "YOUR_TITLE_ID";
            // Login player
        }

        private IEnumerator SubmitScorePlayFab(string leaderboardId, long score)
        {
            LogDebug($"[PlayFab] Submitting score: {score}");
            // var request = new PlayFab.ClientModels.UpdatePlayerStatisticsRequest
            // {
            //     Statistics = new List<PlayFab.ClientModels.StatisticUpdate>
            //     {
            //         new PlayFab.ClientModels.StatisticUpdate { StatisticName = leaderboardId, Value = (int)score }
            //     }
            // };
            // PlayFab.PlayFabClientAPI.UpdatePlayerStatistics(request, OnPlayFabSuccess, OnPlayFabError);
            yield return null;
        }

        private IEnumerator FetchLeaderboardPlayFab(string leaderboardId, int maxResults)
        {
            LogDebug($"[PlayFab] Fetching leaderboard: {leaderboardId}");
            // var request = new PlayFab.ClientModels.GetLeaderboardRequest
            // {
            //     StatisticName = leaderboardId,
            //     StartPosition = 0,
            //     MaxResultsCount = maxResults
            // };
            // PlayFab.PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardSuccess, OnPlayFabError);
            yield return null;
        }

        private IEnumerator FetchFriendLeaderboardPlayFab(string leaderboardId, List<string> friendIds, int maxResults)
        {
            LogDebug($"[PlayFab] Fetching friend leaderboard: {leaderboardId}");
            // PlayFab friend leaderboard implementation
            // var request = new PlayFab.ClientModels.GetFriendLeaderboardRequest
            // {
            //     StatisticName = leaderboardId,
            //     MaxResultsCount = maxResults,
            //     IncludeSteamFriends = false,
            //     IncludeFacebookFriends = false
            // };
            // PlayFab.PlayFabClientAPI.GetFriendLeaderboard(request, OnGetFriendLeaderboardSuccess, OnPlayFabError);
            yield return null;
        }
        #endif

        #endregion

        #region Custom Backend

        private IEnumerator SubmitScoreCustom(string leaderboardId, long score, Dictionary<string, object> metadata)
        {
            LogDebug($"[Custom] Submitting score: {score}");

            string url = $"https://your-server.com/api/leaderboards/{leaderboardId}/submit";
            
            var payload = new Dictionary<string, object>
            {
                { "player_id", playerId },
                { "player_name", playerName },
                { "score", score },
                { "metadata", metadata },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            string json = JsonUtility.ToJson(payload);

            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post(url, json))
            {
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    OnScoreSubmitted?.Invoke(leaderboardId, true);
                    LogDebug("Score submitted successfully");
                }
                else
                {
                    OnScoreSubmitted?.Invoke(leaderboardId, false);
                    OnError?.Invoke($"Failed to submit score: {www.error}");
                    LogDebug($"Error submitting score: {www.error}");
                }
            }
        }

        private IEnumerator FetchLeaderboardCustom(string leaderboardId, LeaderboardScope scope, int maxResults)
        {
            LogDebug($"[Custom] Fetching leaderboard: {leaderboardId}");

            string scopeParam = scope == LeaderboardScope.Global ? "global" : "friends";
            string url = $"https://your-server.com/api/leaderboards/{leaderboardId}?scope={scopeParam}&limit={maxResults}";

            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    // Parse JSON response
                    // List<LeaderboardEntry> entries = ParseCustomResponse(www.downloadHandler.text);
                    // CacheScores(leaderboardId, entries);
                    // OnScoresLoaded?.Invoke(leaderboardId, entries);
                    LogDebug("Leaderboard fetched successfully");
                }
                else
                {
                    OnError?.Invoke($"Failed to fetch leaderboard: {www.error}");
                    LogDebug($"Error fetching leaderboard: {www.error}");
                }
            }
        }

        private IEnumerator FetchFriendLeaderboardCustom(string leaderboardId, List<string> friendIds, int maxResults)
        {
            LogDebug($"[Custom] Fetching friend leaderboard: {leaderboardId}");

            string url = $"https://your-server.com/api/leaderboards/{leaderboardId}/friends";

            var payload = new Dictionary<string, object>
            {
                { "friend_ids", friendIds },
                { "limit", maxResults }
            };

            string json = JsonUtility.ToJson(payload);

            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post(url, json))
            {
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    // Parse JSON response
                    // List<LeaderboardEntry> entries = ParseCustomResponse(www.downloadHandler.text);
                    // CacheScores(leaderboardId + "_friends", entries);
                    // OnScoresLoaded?.Invoke(leaderboardId, entries);
                    LogDebug($"Friend leaderboard fetched successfully ({friendIds.Count} friends)");
                }
                else
                {
                    OnError?.Invoke($"Failed to fetch friend leaderboard: {www.error}");
                    LogDebug($"Error fetching friend leaderboard: {www.error}");
                }
            }
        }

        #endregion

        // ── Local Storage ─────────────────────────────────────────────────────

        private void LoadLocalScores()
        {
            string[] leaderboardIds = { endlessLeaderboardId, dailyLeaderboardId, weeklyLeaderboardId };

            foreach (string id in leaderboardIds)
            {
                string json = PlayerPrefs.GetString($"LocalScores_{id}", "");
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var wrapper = JsonUtility.FromJson<LeaderboardEntryListWrapper>(json);
                        localScores[id] = wrapper.entries;
                        LogDebug($"Loaded {localScores[id].Count} local scores for {id}");
                    }
                    catch (Exception e)
                    {
                        LogDebug($"Error loading local scores: {e.Message}");
                        localScores[id] = new List<LeaderboardEntry>();
                    }
                }
                else
                {
                    localScores[id] = new List<LeaderboardEntry>();
                }
            }
        }

        private void SaveLocalScoresToPrefs(string leaderboardId)
        {
            if (!localScores.ContainsKey(leaderboardId))
                return;

            try
            {
                var wrapper = new LeaderboardEntryListWrapper { entries = localScores[leaderboardId] };
                string json = JsonUtility.ToJson(wrapper);
                PlayerPrefs.SetString($"LocalScores_{leaderboardId}", json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                LogDebug($"Error saving local scores: {e.Message}");
            }
        }

        // ── Caching ───────────────────────────────────────────────────────────

        private void CacheScores(string leaderboardId, List<LeaderboardEntry> entries)
        {
            cachedOnlineScores[leaderboardId] = entries;
            cacheExpiration[leaderboardId] = DateTime.Now + cacheLifetime;
        }

        private bool IsCacheValid(string leaderboardId)
        {
            if (!cachedOnlineScores.ContainsKey(leaderboardId))
                return false;

            if (!cacheExpiration.ContainsKey(leaderboardId))
                return false;

            return DateTime.Now < cacheExpiration[leaderboardId];
        }

        public void ClearCache(string leaderboardId = null)
        {
            if (leaderboardId == null)
            {
                cachedOnlineScores.Clear();
                cacheExpiration.Clear();
                LogDebug("All cache cleared");
            }
            else
            {
                cachedOnlineScores.Remove(leaderboardId);
                cacheExpiration.Remove(leaderboardId);
                LogDebug($"Cache cleared for {leaderboardId}");
            }
        }

        // ── Utilities ─────────────────────────────────────────────────────────

        public List<LeaderboardEntry> GetLocalScores(string leaderboardId)
        {
            return localScores.ContainsKey(leaderboardId) ? localScores[leaderboardId] : new List<LeaderboardEntry>();
        }

        public LeaderboardEntry GetPlayerScore(string leaderboardId)
        {
            if (!localScores.ContainsKey(leaderboardId))
                return null;

            return localScores[leaderboardId].FirstOrDefault(e => e.playerId == playerId);
        }

        public string GetPlayerName() => playerName;
        public string GetPlayerId() => playerId;
        public bool IsOfflineMode() => offlineMode;
        public void SetOfflineMode(bool offline) => offlineMode = offline;

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[LeaderboardManager] {message}");
            }
        }

        // ── Context Menu Commands ─────────────────────────────────────────────

        [ContextMenu("Submit Test Score")]
        private void SubmitTestScore()
        {
            long score = UnityEngine.Random.Range(1000, 100000);
            SubmitEndlessScore(UnityEngine.Random.Range(5, 50), score);
            Debug.Log($"Submitted test score: {score}");
        }

        [ContextMenu("Fetch Endless Leaderboard")]
        private void FetchEndlessLeaderboard()
        {
            FetchLeaderboard(endlessLeaderboardId);
        }

        [ContextMenu("Print Local Scores")]
        private void PrintLocalScores()
        {
            foreach (var kvp in localScores)
            {
                Debug.Log($"--- {kvp.Key} ---");
                foreach (var entry in kvp.Value)
                {
                    Debug.Log($"{entry.rank}. {entry.playerName}: {entry.score}");
                }
            }
        }

        [ContextMenu("Clear All Data")]
        private void ClearAllData()
        {
            PlayerPrefs.DeleteKey("LeaderboardPlayerId");
            PlayerPrefs.DeleteKey("PlayerName");
            PlayerPrefs.DeleteKey($"LocalScores_{endlessLeaderboardId}");
            PlayerPrefs.DeleteKey($"LocalScores_{dailyLeaderboardId}");
            PlayerPrefs.DeleteKey($"LocalScores_{weeklyLeaderboardId}");
            PlayerPrefs.Save();
            
            localScores.Clear();
            ClearCache();
            
            Debug.Log("All leaderboard data cleared");
        }
    }

    // ── Data Structures ───────────────────────────────────────────────────────

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerId;
        public string playerName;
        public long score;
        public int rank;
        public DateTime timestamp;
        public Dictionary<string, object> metadata;

        public LeaderboardEntry()
        {
            metadata = new Dictionary<string, object>();
        }
    }

    [Serializable]
    public class LeaderboardEntryListWrapper
    {
        public List<LeaderboardEntry> entries;
    }

    public enum LeaderboardScope
    {
        Global,   // All players worldwide
        Friends,  // Player's friends only
        Regional  // Players in same region
    }
}
