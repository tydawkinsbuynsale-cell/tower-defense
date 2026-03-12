using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RobotTD.Analytics
{
    /// <summary>
    /// In-game analytics dashboard for viewing player statistics and performance metrics.
    /// Provides detailed insights into gameplay patterns, tower effectiveness, and progression.
    /// Tracks historical data for trend analysis and personal records.
    /// </summary>
    public class AnalyticsDashboardManager : MonoBehaviour
    {
        public static AnalyticsDashboardManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableDashboard = true;
        [SerializeField] private int maxSessionHistory = 100;
        [SerializeField] private bool trackDetailedStats = true;
        [SerializeField] private bool verboseLogging = true;

        [Header("Data Retention")]
        [SerializeField] private int dataRetentionDays = 90;
        [SerializeField] private bool autoCleanOldData = true;

        // State
        private bool isInitialized = false;
        private PlayerStatistics currentStats;
        private List<GameSessionData> sessionHistory = new List<GameSessionData>();
        private Dictionary<string, TowerEffectivenessData> towerStats = new Dictionary<string, TowerEffectivenessData>();
        private Dictionary<string, MapCompletionData> mapStats = new Dictionary<string, MapCompletionData>();
        private PersonalRecords personalRecords;
        private List<MilestoneData> achievedMilestones = new List<MilestoneData>();

        // Events
        public event Action OnDashboardUpdated;
        public event Action<MilestoneData> OnMilestoneAchieved;
        public event Action<string> OnPersonalRecordBroken; // record name

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
            if (!enableDashboard)
            {
                LogDebug("Analytics Dashboard disabled");
                return;
            }

            StartCoroutine(InitializeDashboard());
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeDashboard()
        {
            LogDebug("Initializing Analytics Dashboard...");

            yield return new WaitForSeconds(0.3f);

            // Load saved data
            LoadPlayerStatistics();
            LoadSessionHistory();
            LoadTowerStats();
            LoadMapStats();
            LoadPersonalRecords();
            LoadMilestones();

            // Initialize if no data exists
            if (currentStats == null)
            {
                currentStats = new PlayerStatistics
                {
                    totalGamesPlayed = 0,
                    totalGamesWon = 0,
                    totalPlayTime = 0f,
                    totalEnemiesKilled = 0,
                    totalTowersPlaced = 0,
                    totalCreditsEarned = 0,
                    totalDamageDealt = 0,
                    firstPlayDate = DateTime.Now,
                    lastPlayDate = DateTime.Now
                };
            }

            if (personalRecords == null)
            {
                personalRecords = new PersonalRecords
                {
                    highestWaveReached = 0,
                    mostKillsInGame = 0,
                    fastestGameCompletion = float.MaxValue,
                    highestAccuracy = 0f,
                    mostEfficientGame = 0f,
                    longestWinStreak = 0,
                    highestDamageInGame = 0
                };
            }

            // Clean old data if enabled
            if (autoCleanOldData)
            {
                CleanOldSessionData();
            }

            isInitialized = true;
            LogDebug($"Analytics Dashboard initialized (Sessions: {sessionHistory.Count}, Records: {achievedMilestones.Count})");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("dashboard_initialized", new Dictionary<string, object>
                {
                    { "total_games", currentStats.totalGamesPlayed },
                    { "total_playtime", currentStats.totalPlayTime },
                    { "session_count", sessionHistory.Count }
                });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Game Session Tracking ─────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Records a completed game session.
        /// </summary>
        public void RecordGameSession(GameSessionData sessionData)
        {
            if (!isInitialized || sessionData == null)
                return;

            sessionData.timestamp = DateTime.Now;
            sessionHistory.Add(sessionData);

            // Limit history size
            if (sessionHistory.Count > maxSessionHistory)
            {
                sessionHistory.RemoveAt(0);
            }

            // Update overall statistics
            UpdatePlayerStatistics(sessionData);

            // Update tower effectiveness
            UpdateTowerEffectiveness(sessionData);

            // Update map completion
            UpdateMapCompletion(sessionData);

            // Check for personal records
            CheckPersonalRecords(sessionData);

            // Check for milestones
            CheckMilestones();

            // Save data
            SaveAllData();

            OnDashboardUpdated?.Invoke();

            LogDebug($"Recorded game session: {sessionData.mapName}, Victory: {sessionData.isVictory}");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("dashboard_session_recorded", new Dictionary<string, object>
                {
                    { "map", sessionData.mapName },
                    { "victory", sessionData.isVictory },
                    { "wave", sessionData.wavesCompleted },
                    { "kills", sessionData.enemiesKilled }
                });
            }
        }

        private void UpdatePlayerStatistics(GameSessionData session)
        {
            currentStats.totalGamesPlayed++;
            if (session.isVictory)
                currentStats.totalGamesWon++;

            currentStats.totalPlayTime += session.duration;
            currentStats.totalEnemiesKilled += session.enemiesKilled;
            currentStats.totalTowersPlaced += session.towersPlaced;
            currentStats.totalCreditsEarned += session.creditsEarned;
            currentStats.totalDamageDealt += session.damageDealt;
            currentStats.lastPlayDate = DateTime.Now;

            // Update win rate
            currentStats.winRate = (float)currentStats.totalGamesWon / currentStats.totalGamesPlayed;

            // Update average stats
            currentStats.averageKillsPerGame = (float)currentStats.totalEnemiesKilled / currentStats.totalGamesPlayed;
            currentStats.averagePlayTime = currentStats.totalPlayTime / currentStats.totalGamesPlayed;
        }

        private void UpdateTowerEffectiveness(GameSessionData session)
        {
            if (session.towerStats == null)
                return;

            foreach (var towerStat in session.towerStats)
            {
                if (!towerStats.ContainsKey(towerStat.towerType))
                {
                    towerStats[towerStat.towerType] = new TowerEffectivenessData
                    {
                        towerType = towerStat.towerType,
                        timesPlaced = 0,
                        totalKills = 0,
                        totalDamage = 0,
                        totalCost = 0
                    };
                }

                var data = towerStats[towerStat.towerType];
                data.timesPlaced += towerStat.placementCount;
                data.totalKills += towerStat.kills;
                data.totalDamage += towerStat.damageDealt;
                data.totalCost += towerStat.totalCost;

                // Calculate averages
                data.averageKillsPerTower = (float)data.totalKills / data.timesPlaced;
                data.averageDamagePerTower = (float)data.totalDamage / data.timesPlaced;
                data.killsPerCreditSpent = data.totalCost > 0 ? (float)data.totalKills / data.totalCost : 0f;
            }
        }

        private void UpdateMapCompletion(GameSessionData session)
        {
            if (!mapStats.ContainsKey(session.mapName))
            {
                mapStats[session.mapName] = new MapCompletionData
                {
                    mapName = session.mapName,
                    timesPlayed = 0,
                    timesCompleted = 0,
                    bestTime = float.MaxValue,
                    highestWave = 0,
                    totalStars = 0
                };
            }

            var data = mapStats[session.mapName];
            data.timesPlayed++;

            if (session.isVictory)
            {
                data.timesCompleted++;

                if (session.duration < data.bestTime)
                {
                    data.bestTime = session.duration;
                }
            }

            if (session.wavesCompleted > data.highestWave)
            {
                data.highestWave = session.wavesCompleted;
            }

            data.completionRate = (float)data.timesCompleted / data.timesPlayed;
        }

        private void CheckPersonalRecords(GameSessionData session)
        {
            bool recordBroken = false;

            // Highest wave
            if (session.wavesCompleted > personalRecords.highestWaveReached)
            {
                personalRecords.highestWaveReached = session.wavesCompleted;
                OnPersonalRecordBroken?.Invoke("Highest Wave Reached");
                recordBroken = true;
            }

            // Most kills
            if (session.enemiesKilled > personalRecords.mostKillsInGame)
            {
                personalRecords.mostKillsInGame = session.enemiesKilled;
                OnPersonalRecordBroken?.Invoke("Most Kills in Game");
                recordBroken = true;
            }

            // Fastest completion (only for victories)
            if (session.isVictory && session.duration < personalRecords.fastestGameCompletion)
            {
                personalRecords.fastestGameCompletion = session.duration;
                OnPersonalRecordBroken?.Invoke("Fastest Game Completion");
                recordBroken = true;
            }

            // Highest accuracy
            if (session.accuracy > personalRecords.highestAccuracy)
            {
                personalRecords.highestAccuracy = session.accuracy;
                OnPersonalRecordBroken?.Invoke("Highest Accuracy");
                recordBroken = true;
            }

            // Most efficient
            if (session.efficiency > personalRecords.mostEfficientGame)
            {
                personalRecords.mostEfficientGame = session.efficiency;
                OnPersonalRecordBroken?.Invoke("Most Efficient Game");
                recordBroken = true;
            }

            // Highest damage
            if (session.damageDealt > personalRecords.highestDamageInGame)
            {
                personalRecords.highestDamageInGame = session.damageDealt;
                OnPersonalRecordBroken?.Invoke("Highest Damage in Game");
                recordBroken = true;
            }

            // Win streak
            if (session.isVictory)
            {
                personalRecords.currentWinStreak++;
                if (personalRecords.currentWinStreak > personalRecords.longestWinStreak)
                {
                    personalRecords.longestWinStreak = personalRecords.currentWinStreak;
                    OnPersonalRecordBroken?.Invoke("Longest Win Streak");
                    recordBroken = true;
                }
            }
            else
            {
                personalRecords.currentWinStreak = 0;
            }

            if (recordBroken)
            {
                SavePersonalRecords();
                LogDebug("Personal record(s) broken!");
            }
        }

        private void CheckMilestones()
        {
            List<MilestoneData> newMilestones = new List<MilestoneData>();

            // Games played milestones
            CheckMilestone(newMilestones, "FirstGame", "First Game", "Play your first game", currentStats.totalGamesPlayed >= 1);
            CheckMilestone(newMilestones, "Games10", "10 Games", "Play 10 games", currentStats.totalGamesPlayed >= 10);
            CheckMilestone(newMilestones, "Games50", "50 Games", "Play 50 games", currentStats.totalGamesPlayed >= 50);
            CheckMilestone(newMilestones, "Games100", "100 Games", "Play 100 games", currentStats.totalGamesPlayed >= 100);
            CheckMilestone(newMilestones, "Games500", "500 Games", "Play 500 games", currentStats.totalGamesPlayed >= 500);

            // Kills milestones
            CheckMilestone(newMilestones, "Kills100", "100 Kills", "Kill 100 enemies", currentStats.totalEnemiesKilled >= 100);
            CheckMilestone(newMilestones, "Kills1000", "1,000 Kills", "Kill 1,000 enemies", currentStats.totalEnemiesKilled >= 1000);
            CheckMilestone(newMilestones, "Kills10000", "10,000 Kills", "Kill 10,000 enemies", currentStats.totalEnemiesKilled >= 10000);

            // Win rate milestones
            if (currentStats.totalGamesPlayed >= 10)
            {
                CheckMilestone(newMilestones, "WinRate50", "50% Win Rate", "Achieve 50% win rate (min 10 games)", currentStats.winRate >= 0.5f);
                CheckMilestone(newMilestones, "WinRate75", "75% Win Rate", "Achieve 75% win rate (min 10 games)", currentStats.winRate >= 0.75f);
            }

            // Playtime milestones
            float hoursPlayed = currentStats.totalPlayTime / 3600f;
            CheckMilestone(newMilestones, "Playtime1h", "1 Hour", "Play for 1 hour", hoursPlayed >= 1f);
            CheckMilestone(newMilestones, "Playtime10h", "10 Hours", "Play for 10 hours", hoursPlayed >= 10f);
            CheckMilestone(newMilestones, "Playtime100h", "100 Hours", "Play for 100 hours", hoursPlayed >= 100f);

            // Notify new milestones
            foreach (var milestone in newMilestones)
            {
                achievedMilestones.Add(milestone);
                OnMilestoneAchieved?.Invoke(milestone);
                LogDebug($"Milestone achieved: {milestone.milestoneName}");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("dashboard_milestone_achieved", new Dictionary<string, object>
                    {
                        { "milestone_id", milestone.milestoneId },
                        { "milestone_name", milestone.milestoneName }
                    });
                }
            }

            if (newMilestones.Count > 0)
            {
                SaveMilestones();
            }
        }

        private void CheckMilestone(List<MilestoneData> newMilestones, string id, string name, string description, bool condition)
        {
            // Check if already achieved
            if (achievedMilestones.Any(m => m.milestoneId == id))
                return;

            if (condition)
            {
                newMilestones.Add(new MilestoneData
                {
                    milestoneId = id,
                    milestoneName = name,
                    description = description,
                    achievedDate = DateTime.Now
                });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Data Retrieval ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Gets overall player statistics.
        /// </summary>
        public PlayerStatistics GetPlayerStatistics()
        {
            return currentStats;
        }

        /// <summary>
        /// Gets session history with optional filtering.
        /// </summary>
        public List<GameSessionData> GetSessionHistory(TimeFilter filter = TimeFilter.AllTime)
        {
            if (filter == TimeFilter.AllTime)
                return new List<GameSessionData>(sessionHistory);

            DateTime cutoffDate = GetCutoffDate(filter);
            return sessionHistory.Where(s => s.timestamp >= cutoffDate).ToList();
        }

        /// <summary>
        /// Gets performance trend over time.
        /// </summary>
        public List<PerformanceTrendData> GetPerformanceTrend(TimeFilter filter = TimeFilter.Last30Days)
        {
            var sessions = GetSessionHistory(filter);
            var trendData = new List<PerformanceTrendData>();

            // Group by day
            var groupedSessions = sessions
                .GroupBy(s => s.timestamp.Date)
                .OrderBy(g => g.Key);

            foreach (var group in groupedSessions)
            {
                var daySessions = group.ToList();
                trendData.Add(new PerformanceTrendData
                {
                    date = group.Key,
                    gamesPlayed = daySessions.Count,
                    averageWinRate = daySessions.Count(s => s.isVictory) / (float)daySessions.Count,
                    averageKills = daySessions.Average(s => s.enemiesKilled),
                    averageAccuracy = daySessions.Average(s => s.accuracy),
                    averageEfficiency = daySessions.Average(s => s.efficiency)
                });
            }

            return trendData;
        }

        /// <summary>
        /// Gets tower effectiveness statistics.
        /// </summary>
        public List<TowerEffectivenessData> GetTowerEffectiveness()
        {
            return towerStats.Values.OrderByDescending(t => t.totalKills).ToList();
        }

        /// <summary>
        /// Gets map completion statistics.
        /// </summary>
        public List<MapCompletionData> GetMapCompletion()
        {
            return mapStats.Values.OrderByDescending(m => m.completionRate).ToList();
        }

        /// <summary>
        /// Gets personal records.
        /// </summary>
        public PersonalRecords GetPersonalRecords()
        {
            return personalRecords;
        }

        /// <summary>
        /// Gets achieved milestones.
        /// </summary>
        public List<MilestoneData> GetAchievedMilestones()
        {
            return new List<MilestoneData>(achievedMilestones);
        }

        /// <summary>
        /// Gets recent sessions (last N).
        /// </summary>
        public List<GameSessionData> GetRecentSessions(int count = 10)
        {
            return sessionHistory.OrderByDescending(s => s.timestamp).Take(count).ToList();
        }

        /// <summary>
        /// Gets best performing tower.
        /// </summary>
        public TowerEffectivenessData GetBestTower()
        {
            if (towerStats.Count == 0)
                return null;

            return towerStats.Values.OrderByDescending(t => t.killsPerCreditSpent).First();
        }

        /// <summary>
        /// Gets most played map.
        /// </summary>
        public MapCompletionData GetMostPlayedMap()
        {
            if (mapStats.Count == 0)
                return null;

            return mapStats.Values.OrderByDescending(m => m.timesPlayed).First();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Data Export ───────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Exports dashboard data as JSON string.
        /// </summary>
        public string ExportDashboardData()
        {
            DashboardExportData exportData = new DashboardExportData
            {
                playerStats = currentStats,
                sessionHistory = sessionHistory,
                towerStats = towerStats.Values.ToList(),
                mapStats = mapStats.Values.ToList(),
                personalRecords = personalRecords,
                milestones = achievedMilestones,
                exportDate = DateTime.Now
            };

            string json = JsonUtility.ToJson(exportData, true);
            LogDebug("Dashboard data exported");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("dashboard_data_exported", new Dictionary<string, object>
                {
                    { "session_count", sessionHistory.Count },
                    { "milestone_count", achievedMilestones.Count }
                });
            }

            return json;
        }

        /// <summary>
        /// Gets summary statistics for quick overview.
        /// </summary>
        public DashboardSummary GetDashboardSummary()
        {
            return new DashboardSummary
            {
                totalGames = currentStats.totalGamesPlayed,
                winRate = currentStats.winRate,
                totalPlaytimeHours = currentStats.totalPlayTime / 3600f,
                totalKills = currentStats.totalEnemiesKilled,
                favoriteMap = GetMostPlayedMap()?.mapName ?? "None",
                bestTower = GetBestTower()?.towerType ?? "None",
                longestWinStreak = personalRecords.longestWinStreak,
                milestonesAchieved = achievedMilestones.Count
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Data Management ───────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void CleanOldSessionData()
        {
            DateTime cutoffDate = DateTime.Now.AddDays(-dataRetentionDays);
            int originalCount = sessionHistory.Count;

            sessionHistory.RemoveAll(s => s.timestamp < cutoffDate);

            if (sessionHistory.Count < originalCount)
            {
                LogDebug($"Cleaned {originalCount - sessionHistory.Count} old sessions");
                SaveSessionHistory();
            }
        }

        private DateTime GetCutoffDate(TimeFilter filter)
        {
            switch (filter)
            {
                case TimeFilter.Today:
                    return DateTime.Today;
                case TimeFilter.Last7Days:
                    return DateTime.Now.AddDays(-7);
                case TimeFilter.Last30Days:
                    return DateTime.Now.AddDays(-30);
                case TimeFilter.LastYear:
                    return DateTime.Now.AddYears(-1);
                default:
                    return DateTime.MinValue;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Local Storage ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void SaveAllData()
        {
            SavePlayerStatistics();
            SaveSessionHistory();
            SaveTowerStats();
            SaveMapStats();
            SavePersonalRecords();
        }

        private void LoadPlayerStatistics()
        {
            string json = PlayerPrefs.GetString("PlayerStatistics", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    currentStats = JsonUtility.FromJson<PlayerStatistics>(json);
                    LogDebug("Loaded player statistics");
                }
                catch { LogDebug("Failed to load player statistics"); }
            }
        }

        private void SavePlayerStatistics()
        {
            if (currentStats == null) return;
            string json = JsonUtility.ToJson(currentStats);
            PlayerPrefs.SetString("PlayerStatistics", json);
            PlayerPrefs.Save();
        }

        private void LoadSessionHistory()
        {
            string json = PlayerPrefs.GetString("SessionHistory", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    SessionHistoryData data = JsonUtility.FromJson<SessionHistoryData>(json);
                    sessionHistory = data.sessions ?? new List<GameSessionData>();
                    LogDebug($"Loaded {sessionHistory.Count} sessions");
                }
                catch { LogDebug("Failed to load session history"); }
            }
        }

        private void SaveSessionHistory()
        {
            SessionHistoryData data = new SessionHistoryData { sessions = sessionHistory };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("SessionHistory", json);
            PlayerPrefs.Save();
        }

        private void LoadTowerStats()
        {
            string json = PlayerPrefs.GetString("TowerStats", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    TowerStatsCollection data = JsonUtility.FromJson<TowerStatsCollection>(json);
                    towerStats = data.stats.ToDictionary(t => t.towerType);
                    LogDebug($"Loaded {towerStats.Count} tower stats");
                }
                catch { LogDebug("Failed to load tower stats"); }
            }
        }

        private void SaveTowerStats()
        {
            TowerStatsCollection data = new TowerStatsCollection { stats = towerStats.Values.ToList() };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("TowerStats", json);
            PlayerPrefs.Save();
        }

        private void LoadMapStats()
        {
            string json = PlayerPrefs.GetString("MapStats", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    MapStatsCollection data = JsonUtility.FromJson<MapStatsCollection>(json);
                    mapStats = data.stats.ToDictionary(m => m.mapName);
                    LogDebug($"Loaded {mapStats.Count} map stats");
                }
                catch { LogDebug("Failed to load map stats"); }
            }
        }

        private void SaveMapStats()
        {
            MapStatsCollection data = new MapStatsCollection { stats = mapStats.Values.ToList() };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("MapStats", json);
            PlayerPrefs.Save();
        }

        private void LoadPersonalRecords()
        {
            string json = PlayerPrefs.GetString("PersonalRecords", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    personalRecords = JsonUtility.FromJson<PersonalRecords>(json);
                    LogDebug("Loaded personal records");
                }
                catch { LogDebug("Failed to load personal records"); }
            }
        }

        private void SavePersonalRecords()
        {
            if (personalRecords == null) return;
            string json = JsonUtility.ToJson(personalRecords);
            PlayerPrefs.SetString("PersonalRecords", json);
            PlayerPrefs.Save();
        }

        private void LoadMilestones()
        {
            string json = PlayerPrefs.GetString("DashboardMilestones", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    MilestonesCollection data = JsonUtility.FromJson<MilestonesCollection>(json);
                    achievedMilestones = data.milestones ?? new List<MilestoneData>();
                    LogDebug($"Loaded {achievedMilestones.Count} milestones");
                }
                catch { LogDebug("Failed to load milestones"); }
            }
        }

        private void SaveMilestones()
        {
            MilestonesCollection data = new MilestonesCollection { milestones = achievedMilestones };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("DashboardMilestones", json);
            PlayerPrefs.Save();
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[AnalyticsDashboard] {message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ── Data Structures ───────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Overall player statistics.
    /// </summary>
    [Serializable]
    public class PlayerStatistics
    {
        public int totalGamesPlayed;
        public int totalGamesWon;
        public float winRate;
        public float totalPlayTime; // seconds
        public int totalEnemiesKilled;
        public int totalTowersPlaced;
        public int totalCreditsEarned;
        public long totalDamageDealt;
        public float averageKillsPerGame;
        public float averagePlayTime;
        public DateTime firstPlayDate;
        public DateTime lastPlayDate;
    }

    /// <summary>
    /// Individual game session data.
    /// </summary>
    [Serializable]
    public class GameSessionData
    {
        public DateTime timestamp;
        public string mapName;
        public bool isVictory;
        public int wavesCompleted;
        public int enemiesKilled;
        public int towersPlaced;
        public int creditsEarned;
        public long damageDealt;
        public float accuracy;
        public float efficiency;
        public float duration; // seconds
        public List<SessionTowerStat> towerStats;
    }

    /// <summary>
    /// Tower statistics within a session.
    /// </summary>
    [Serializable]
    public class SessionTowerStat
    {
        public string towerType;
        public int placementCount;
        public int kills;
        public long damageDealt;
        public int totalCost;
    }

    /// <summary>
    /// Tower effectiveness tracking.
    /// </summary>
    [Serializable]
    public class TowerEffectivenessData
    {
        public string towerType;
        public int timesPlaced;
        public int totalKills;
        public long totalDamage;
        public int totalCost;
        public float averageKillsPerTower;
        public float averageDamagePerTower;
        public float killsPerCreditSpent;
    }

    /// <summary>
    /// Map completion tracking.
    /// </summary>
    [Serializable]
    public class MapCompletionData
    {
        public string mapName;
        public int timesPlayed;
        public int timesCompleted;
        public float completionRate;
        public float bestTime;
        public int highestWave;
        public int totalStars;
    }

    /// <summary>
    /// Personal records.
    /// </summary>
    [Serializable]
    public class PersonalRecords
    {
        public int highestWaveReached;
        public int mostKillsInGame;
        public float fastestGameCompletion;
        public float highestAccuracy;
        public float mostEfficientGame;
        public int longestWinStreak;
        public int currentWinStreak;
        public long highestDamageInGame;
    }

    /// <summary>
    /// Milestone achievement.
    /// </summary>
    [Serializable]
    public class MilestoneData
    {
        public string milestoneId;
        public string milestoneName;
        public string description;
        public DateTime achievedDate;
    }

    /// <summary>
    /// Performance trend data point.
    /// </summary>
    [Serializable]
    public class PerformanceTrendData
    {
        public DateTime date;
        public int gamesPlayed;
        public float averageWinRate;
        public float averageKills;
        public float averageAccuracy;
        public float averageEfficiency;
    }

    /// <summary>
    /// Dashboard summary for quick overview.
    /// </summary>
    [Serializable]
    public class DashboardSummary
    {
        public int totalGames;
        public float winRate;
        public float totalPlaytimeHours;
        public int totalKills;
        public string favoriteMap;
        public string bestTower;
        public int longestWinStreak;
        public int milestonesAchieved;
    }

    /// <summary>
    /// Dashboard export data.
    /// </summary>
    [Serializable]
    public class DashboardExportData
    {
        public PlayerStatistics playerStats;
        public List<GameSessionData> sessionHistory;
        public List<TowerEffectivenessData> towerStats;
        public List<MapCompletionData> mapStats;
        public PersonalRecords personalRecords;
        public List<MilestoneData> milestones;
        public DateTime exportDate;
    }

    // Serialization helpers
    [Serializable]
    public class SessionHistoryData
    {
        public List<GameSessionData> sessions;
    }

    [Serializable]
    public class TowerStatsCollection
    {
        public List<TowerEffectivenessData> stats;
    }

    [Serializable]
    public class MapStatsCollection
    {
        public List<MapCompletionData> stats;
    }

    [Serializable]
    public class MilestonesCollection
    {
        public List<MilestoneData> milestones;
    }

    /// <summary>
    /// Time filter for data queries.
    /// </summary>
    public enum TimeFilter
    {
        Today,
        Last7Days,
        Last30Days,
        LastYear,
        AllTime
    }
}
