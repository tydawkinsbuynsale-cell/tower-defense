using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Core;
using RobotTD.Online;
using RobotTD.Analytics;

namespace RobotTD.Competitive
{
    /// <summary>
    /// Tournament and competitive ladder system for ranked matches.
    /// Features weekly tournaments, skill-based matchmaking, ranking tiers, and prize pools.
    /// Integrates with leaderboards and authentication systems.
    /// </summary>
    public class TournamentManager : MonoBehaviour
    {
        public static TournamentManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableTournaments = true;
        [SerializeField] private bool enableRankedMode = true;
        [SerializeField] private int matchmakingTimeout = 30; // seconds
        [SerializeField] private int rankDecayDays = 14; // Days of inactivity before rank decay
        [SerializeField] private bool verboseLogging = true;

        [Header("Tournament Settings")]
        [SerializeField] private int weeklyTournamentDuration = 7; // days
        [SerializeField] private int tournamentParticipantLimit = 100;
        [SerializeField] private TournamentReward[] prizePool;

        [Header("Ranking Tiers")]
        [SerializeField] private RankTier[] rankTiers;

        // State
        private bool isInitialized = false;
        private PlayerRankData currentPlayerRank;
        private TournamentData activeTournament;
        private List<TournamentData> tournamentHistory = new List<TournamentData>();
        private MatchmakingQueue matchmakingQueue = new MatchmakingQueue();
        private Dictionary<string, TournamentEntry> currentTournamentEntries = new Dictionary<string, TournamentEntry>();
        private bool isSearchingForMatch = false;

        // Events
        public event Action<PlayerRankData> OnRankChanged;
        public event Action<TournamentData> OnTournamentStarted;
        public event Action<TournamentData> OnTournamentEnded;
        public event Action<MatchData> OnMatchFound;
        public event Action OnMatchmakingStarted;
        public event Action OnMatchmakingCancelled;
        public event Action<RankedMatchResult> OnRankedMatchCompleted;
        public event Action<string> OnTournamentJoined; // tournamentId

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
            if (!enableTournaments && !enableRankedMode)
            {
                LogDebug("Tournament system disabled");
                return;
            }

            StartCoroutine(InitializeTournamentSystem());
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeTournamentSystem()
        {
            LogDebug("Initializing tournament system...");

            yield return new WaitForSeconds(0.5f);

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

                if (!authManager.IsAuthenticated)
                {
                    LogDebug("Not authenticated - tournaments require login");
                }
            }

            // Load player rank data
            LoadPlayerRankFromLocal();

            // Load tournament history
            LoadTournamentHistoryFromLocal();

            // Check for active tournaments
            CheckForActiveTournaments();

            // Check for rank decay
            CheckRankDecay();

            isInitialized = true;
            LogDebug("Tournament system initialized");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("tournament_system_initialized", new Dictionary<string, object>
                {
                    { "player_rank", currentPlayerRank?.rankTier ?? "unranked" },
                    { "rating", currentPlayerRank?.rating ?? 0 },
                    { "has_active_tournament", activeTournament != null }
                });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Ranking System ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Gets the player's current rank data.
        /// </summary>
        public PlayerRankData GetPlayerRank()
        {
            return currentPlayerRank;
        }

        /// <summary>
        /// Updates player rating after a ranked match.
        /// </summary>
        public void UpdateRating(RankedMatchResult result)
        {
            if (currentPlayerRank == null)
            {
                InitializePlayerRank();
            }

            int ratingChange = CalculateRatingChange(result);
            int oldRating = currentPlayerRank.rating;
            string oldTier = currentPlayerRank.rankTier;

            currentPlayerRank.rating += ratingChange;
            currentPlayerRank.rating = Mathf.Max(0, currentPlayerRank.rating); // Prevent negative rating

            // Update win/loss record
            if (result.isVictory)
            {
                currentPlayerRank.wins++;
                currentPlayerRank.currentWinStreak++;
                if (currentPlayerRank.currentWinStreak > currentPlayerRank.bestWinStreak)
                {
                    currentPlayerRank.bestWinStreak = currentPlayerRank.currentWinStreak;
                }
            }
            else
            {
                currentPlayerRank.losses++;
                currentPlayerRank.currentWinStreak = 0;
            }

            currentPlayerRank.totalMatches++;
            currentPlayerRank.lastMatchDate = DateTime.UtcNow;

            // Update rank tier
            UpdateRankTier();

            SavePlayerRankToLocal();

            OnRankChanged?.Invoke(currentPlayerRank);

            LogDebug($"Rating updated: {oldRating} -> {currentPlayerRank.rating} ({(ratingChange >= 0 ? "+" : "")}{ratingChange})");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("ranked_match_completed", new Dictionary<string, object>
                {
                    { "is_victory", result.isVictory },
                    { "rating_change", ratingChange },
                    { "new_rating", currentPlayerRank.rating },
                    { "old_tier", oldTier },
                    { "new_tier", currentPlayerRank.rankTier },
                    { "win_streak", currentPlayerRank.currentWinStreak }
                });
            }

            OnRankedMatchCompleted?.Invoke(result);
        }

        /// <summary>
        /// Calculates rating change based on match result using Elo-like system.
        /// </summary>
        private int CalculateRatingChange(RankedMatchResult result)
        {
            // K-factor (sensitivity to rating changes)
            int kFactor = 32;

            // Calculate expected score
            float ratingDiff = result.opponentRating - currentPlayerRank.rating;
            float expectedScore = 1.0f / (1.0f + Mathf.Pow(10, ratingDiff / 400f));

            // Calculate actual score
            float actualScore = result.isVictory ? 1.0f : 0.0f;

            // Calculate rating change
            int ratingChange = Mathf.RoundToInt(kFactor * (actualScore - expectedScore));

            // Bonus for performance
            if (result.performanceBonus > 0)
            {
                ratingChange += Mathf.RoundToInt(result.performanceBonus * 5);
            }

            return ratingChange;
        }

        /// <summary>
        /// Updates the player's rank tier based on rating.
        /// </summary>
        private void UpdateRankTier()
        {
            if (rankTiers == null || rankTiers.Length == 0)
            {
                currentPlayerRank.rankTier = "Unranked";
                return;
            }

            string previousTier = currentPlayerRank.rankTier;

            // Find appropriate tier
            for (int i = rankTiers.Length - 1; i >= 0; i--)
            {
                if (currentPlayerRank.rating >= rankTiers[i].minRating)
                {
                    currentPlayerRank.rankTier = rankTiers[i].tierName;
                    break;
                }
            }

            // Check for tier change
            if (previousTier != currentPlayerRank.rankTier)
            {
                LogDebug($"Rank tier changed: {previousTier} -> {currentPlayerRank.rankTier}");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("rank_tier_changed", new Dictionary<string, object>
                    {
                        { "old_tier", previousTier },
                        { "new_tier", currentPlayerRank.rankTier },
                        { "rating", currentPlayerRank.rating }
                    });
                }
            }
        }

        /// <summary>
        /// Checks for rank decay due to inactivity.
        /// </summary>
        private void CheckRankDecay()
        {
            if (currentPlayerRank == null || currentPlayerRank.rating == 0)
                return;

            TimeSpan inactiveTime = DateTime.UtcNow - currentPlayerRank.lastMatchDate;
            if (inactiveTime.TotalDays >= rankDecayDays)
            {
                int decayAmount = Mathf.RoundToInt((float)inactiveTime.TotalDays / rankDecayDays * 10);
                currentPlayerRank.rating = Mathf.Max(0, currentPlayerRank.rating - decayAmount);
                currentPlayerRank.lastMatchDate = DateTime.UtcNow; // Reset decay timer
                UpdateRankTier();
                SavePlayerRankToLocal();

                LogDebug($"Rank decay applied: -{decayAmount} rating");
            }
        }

        private void InitializePlayerRank()
        {
            currentPlayerRank = new PlayerRankData
            {
                rating = 1000, // Starting rating
                rankTier = "Bronze",
                wins = 0,
                losses = 0,
                totalMatches = 0,
                currentWinStreak = 0,
                bestWinStreak = 0,
                lastMatchDate = DateTime.UtcNow
            };

            UpdateRankTier();
            SavePlayerRankToLocal();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Matchmaking System ────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Starts searching for a ranked match.
        /// </summary>
        public void StartMatchmaking()
        {
            if (!isInitialized || !enableRankedMode)
            {
                LogDebug("Cannot start matchmaking: system not ready");
                return;
            }

            if (isSearchingForMatch)
            {
                LogDebug("Already searching for match");
                return;
            }

            var authManager = AuthenticationManager.Instance;
            if (authManager == null || !authManager.IsAuthenticated)
            {
                LogDebug("Cannot start matchmaking: not authenticated");
                return;
            }

            if (currentPlayerRank == null)
            {
                InitializePlayerRank();
            }

            isSearchingForMatch = true;
            OnMatchmakingStarted?.Invoke();

            StartCoroutine(MatchmakingCoroutine());

            LogDebug($"Started matchmaking (Rating: {currentPlayerRank.rating}, Tier: {currentPlayerRank.rankTier})");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("matchmaking_started", new Dictionary<string, object>
                {
                    { "player_rating", currentPlayerRank.rating },
                    { "player_tier", currentPlayerRank.rankTier }
                });
            }
        }

        /// <summary>
        /// Cancels matchmaking search.
        /// </summary>
        public void CancelMatchmaking()
        {
            if (!isSearchingForMatch)
                return;

            isSearchingForMatch = false;
            StopCoroutine(nameof(MatchmakingCoroutine));

            OnMatchmakingCancelled?.Invoke();

            LogDebug("Matchmaking cancelled");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("matchmaking_cancelled", new Dictionary<string, object>
                {
                    { "player_rating", currentPlayerRank.rating }
                });
            }
        }

        private IEnumerator MatchmakingCoroutine()
        {
            float elapsed = 0f;
            int ratingRange = 100; // Initial rating range

            while (isSearchingForMatch && elapsed < matchmakingTimeout)
            {
                // Simulate matchmaking (in production, this would query a server)
                #if UNITY_EDITOR
                // Simulation: find match after 3-5 seconds
                if (elapsed >= 3f)
                {
                    var matchData = SimulateMatchFound();
                    OnMatchFound?.Invoke(matchData);
                    isSearchingForMatch = false;

                    LogDebug($"Match found! Opponent rating: {matchData.opponentRating}");

                    // Track analytics
                    if (AnalyticsManager.Instance != null)
                    {
                        AnalyticsManager.Instance.TrackEvent("match_found", new Dictionary<string, object>
                        {
                            { "player_rating", currentPlayerRank.rating },
                            { "opponent_rating", matchData.opponentRating },
                            { "search_time", elapsed }
                        });
                    }

                    yield break;
                }
                #else
                // Production matchmaking logic would go here
                // Query server for available opponents within rating range
                #endif

                // Expand rating range over time
                if (elapsed > 10f)
                {
                    ratingRange = 200;
                }
                if (elapsed > 20f)
                {
                    ratingRange = 300;
                }

                yield return new WaitForSeconds(1f);
                elapsed += 1f;
            }

            // Timeout
            if (isSearchingForMatch)
            {
                isSearchingForMatch = false;
                OnMatchmakingCancelled?.Invoke();
                LogDebug("Matchmaking timeout");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("matchmaking_timeout", new Dictionary<string, object>
                    {
                        { "player_rating", currentPlayerRank.rating },
                        { "search_time", elapsed }
                    });
                }
            }
        }

        #if UNITY_EDITOR
        private MatchData SimulateMatchFound()
        {
            // Simulate opponent within rating range
            int ratingVariance = UnityEngine.Random.Range(-100, 101);
            int opponentRating = Mathf.Max(0, currentPlayerRank.rating + ratingVariance);

            return new MatchData
            {
                matchId = Guid.NewGuid().ToString(),
                opponentName = $"Player_{UnityEngine.Random.Range(1000, 9999)}",
                opponentRating = opponentRating,
                opponentTier = GetRankTierByRating(opponentRating),
                mapId = "ranked_map_1"
            };
        }
        #endif

        private string GetRankTierByRating(int rating)
        {
            if (rankTiers == null || rankTiers.Length == 0)
                return "Unranked";

            for (int i = rankTiers.Length - 1; i >= 0; i--)
            {
                if (rating >= rankTiers[i].minRating)
                {
                    return rankTiers[i].tierName;
                }
            }

            return rankTiers[0].tierName;
        }

        /// <summary>
        /// Checks if player is currently searching for a match.
        /// </summary>
        public bool IsMatchmaking()
        {
            return isSearchingForMatch;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Tournament System ─────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Checks for active tournaments and creates new weekly tournaments.
        /// </summary>
        private void CheckForActiveTournaments()
        {
            if (!enableTournaments)
                return;

            DateTime now = DateTime.UtcNow;

            // Check if current tournament has ended
            if (activeTournament != null && now > activeTournament.endDate)
            {
                EndTournament(activeTournament);
            }

            // Create new weekly tournament if none active
            if (activeTournament == null)
            {
                CreateWeeklyTournament();
            }
        }

        /// <summary>
        /// Creates a new weekly tournament.
        /// </summary>
        private void CreateWeeklyTournament()
        {
            DateTime now = DateTime.UtcNow;
            DateTime startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            DateTime endOfWeek = startOfWeek.AddDays(weeklyTournamentDuration);

            activeTournament = new TournamentData
            {
                tournamentId = $"weekly_{startOfWeek:yyyyMMdd}",
                tournamentName = $"Weekly Tournament - {startOfWeek:MMM dd}",
                startDate = startOfWeek,
                endDate = endOfWeek,
                participantLimit = tournamentParticipantLimit,
                prizePool = prizePool
            };

            currentTournamentEntries.Clear();

            OnTournamentStarted?.Invoke(activeTournament);

            LogDebug($"Created weekly tournament: {activeTournament.tournamentName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("tournament_created", new Dictionary<string, object>
                {
                    { "tournament_id", activeTournament.tournamentId },
                    { "tournament_name", activeTournament.tournamentName }
                });
            }
        }

        /// <summary>
        /// Joins the active tournament.
        /// </summary>
        public bool JoinTournament()
        {
            if (activeTournament == null)
            {
                LogDebug("No active tournament to join");
                return false;
            }

            var authManager = AuthenticationManager.Instance;
            if (authManager == null || !authManager.IsAuthenticated)
            {
                LogDebug("Cannot join tournament: not authenticated");
                return false;
            }

            string playerId = authManager.PlayerId;

            if (currentTournamentEntries.ContainsKey(playerId))
            {
                LogDebug("Already joined tournament");
                return false;
            }

            if (currentTournamentEntries.Count >= activeTournament.participantLimit)
            {
                LogDebug("Tournament is full");
                return false;
            }

            var entry = new TournamentEntry
            {
                playerId = playerId,
                playerName = authManager.PlayerName,
                score = 0,
                rank = 0,
                joinDate = DateTime.UtcNow
            };

            currentTournamentEntries[playerId] = entry;

            OnTournamentJoined?.Invoke(activeTournament.tournamentId);

            LogDebug($"Joined tournament: {activeTournament.tournamentName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("tournament_joined", new Dictionary<string, object>
                {
                    { "tournament_id", activeTournament.tournamentId },
                    { "participants", currentTournamentEntries.Count }
                });
            }

            return true;
        }

        /// <summary>
        /// Updates tournament score for the player.
        /// </summary>
        public void UpdateTournamentScore(int score)
        {
            if (activeTournament == null)
                return;

            var authManager = AuthenticationManager.Instance;
            if (authManager == null || !authManager.IsAuthenticated)
                return;

            string playerId = authManager.PlayerId;

            if (!currentTournamentEntries.ContainsKey(playerId))
            {
                LogDebug("Not participating in tournament");
                return;
            }

            var entry = currentTournamentEntries[playerId];
            entry.score = Mathf.Max(entry.score, score); // Keep best score
            entry.lastUpdateDate = DateTime.UtcNow;

            UpdateTournamentRankings();

            LogDebug($"Tournament score updated: {score}");
        }

        /// <summary>
        /// Updates tournament rankings.
        /// </summary>
        private void UpdateTournamentRankings()
        {
            var sortedEntries = currentTournamentEntries.Values
                .OrderByDescending(e => e.score)
                .ToList();

            for (int i = 0; i < sortedEntries.Count; i++)
            {
                sortedEntries[i].rank = i + 1;
            }
        }

        /// <summary>
        /// Gets the active tournament.
        /// </summary>
        public TournamentData GetActiveTournament()
        {
            return activeTournament;
        }

        /// <summary>
        /// Gets tournament leaderboard.
        /// </summary>
        public List<TournamentEntry> GetTournamentLeaderboard(int count = 100)
        {
            return currentTournamentEntries.Values
                .OrderByDescending(e => e.score)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Ends a tournament and distributes prizes.
        /// </summary>
        private void EndTournament(TournamentData tournament)
        {
            UpdateTournamentRankings();

            // Add to history
            tournament.finalParticipants = currentTournamentEntries.Count;
            tournamentHistory.Add(tournament);
            SaveTournamentHistoryToLocal();

            OnTournamentEnded?.Invoke(tournament);

            activeTournament = null;
            currentTournamentEntries.Clear();

            LogDebug($"Tournament ended: {tournament.tournamentName}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("tournament_ended", new Dictionary<string, object>
                {
                    { "tournament_id", tournament.tournamentId },
                    { "participants", tournament.finalParticipants }
                });
            }
        }

        /// <summary>
        /// Gets tournament history.
        /// </summary>
        public List<TournamentData> GetTournamentHistory()
        {
            return new List<TournamentData>(tournamentHistory);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Local Storage ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LoadPlayerRankFromLocal()
        {
            string json = PlayerPrefs.GetString("PlayerRankData", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    currentPlayerRank = JsonUtility.FromJson<PlayerRankData>(json);
                    LogDebug($"Loaded player rank: {currentPlayerRank.rankTier} ({currentPlayerRank.rating})");
                }
                catch
                {
                    LogDebug("Failed to load player rank");
                }
            }
        }

        private void SavePlayerRankToLocal()
        {
            if (currentPlayerRank == null)
                return;

            string json = JsonUtility.ToJson(currentPlayerRank);
            PlayerPrefs.SetString("PlayerRankData", json);
            PlayerPrefs.Save();
        }

        private void LoadTournamentHistoryFromLocal()
        {
            string json = PlayerPrefs.GetString("TournamentHistory", "{}");
            try
            {
                var data = JsonUtility.FromJson<TournamentHistoryData>(json);
                if (data != null && data.tournaments != null)
                {
                    tournamentHistory = new List<TournamentData>(data.tournaments);
                    LogDebug($"Loaded {tournamentHistory.Count} tournament records");
                }
            }
            catch
            {
                LogDebug("No tournament history found");
            }
        }

        private void SaveTournamentHistoryToLocal()
        {
            var data = new TournamentHistoryData
            {
                tournaments = tournamentHistory.ToArray()
            };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("TournamentHistory", json);
            PlayerPrefs.Save();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[TournamentManager] {message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ── Data Structures ───────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Player's rank and rating data.
    /// </summary>
    [Serializable]
    public class PlayerRankData
    {
        public int rating = 1000;
        public string rankTier = "Bronze";
        public int wins = 0;
        public int losses = 0;
        public int totalMatches = 0;
        public int currentWinStreak = 0;
        public int bestWinStreak = 0;
        public DateTime lastMatchDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Rank tier definition.
    /// </summary>
    [Serializable]
    public class RankTier
    {
        public string tierName;
        public int minRating;
        public Color tierColor = Color.white;
        public Sprite tierIcon;
    }

    /// <summary>
    /// Match data for matchmaking.
    /// </summary>
    [Serializable]
    public class MatchData
    {
        public string matchId;
        public string opponentName;
        public int opponentRating;
        public string opponentTier;
        public string mapId;
    }

    /// <summary>
    /// Result of a ranked match.
    /// </summary>
    [Serializable]
    public class RankedMatchResult
    {
        public bool isVictory;
        public int opponentRating;
        public float performanceBonus; // 0-1, based on score/time/efficiency
    }

    /// <summary>
    /// Tournament data.
    /// </summary>
    [Serializable]
    public class TournamentData
    {
        public string tournamentId;
        public string tournamentName;
        public DateTime startDate;
        public DateTime endDate;
        public int participantLimit;
        public int finalParticipants;
        public TournamentReward[] prizePool;
    }

    /// <summary>
    /// Tournament reward for top finishers.
    /// </summary>
    [Serializable]
    public class TournamentReward
    {
        public int topRank; // e.g., 1 for 1st place, 3 for top 3
        public string rewardType; // gems, credits, cosmetic, etc.
        public int rewardAmount;
    }

    /// <summary>
    /// Player's tournament entry.
    /// </summary>
    [Serializable]
    public class TournamentEntry
    {
        public string playerId;
        public string playerName;
        public int score;
        public int rank;
        public DateTime joinDate;
        public DateTime lastUpdateDate;
    }

    /// <summary>
    /// Matchmaking queue helper.
    /// </summary>
    public class MatchmakingQueue
    {
        // In production, this would manage server-side queue
    }

    /// <summary>
    /// Tournament history serialization helper.
    /// </summary>
    [Serializable]
    public class TournamentHistoryData
    {
        public TournamentData[] tournaments;
    }
}
