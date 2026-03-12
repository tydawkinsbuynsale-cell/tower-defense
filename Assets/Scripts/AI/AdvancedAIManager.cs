using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Core;
using RobotTD.Enemies;
using RobotTD.Towers;
using RobotTD.Analytics;

namespace RobotTD.AI
{
    /// <summary>
    /// Advanced AI system for dynamic difficulty and adaptive gameplay.
    /// Features dynamic difficulty adjustment, adaptive enemy strategies, smart routing, and ML-powered challenges.
    /// Analyzes player behavior and adjusts game difficulty in real-time.
    /// </summary>
    public class AdvancedAIManager : MonoBehaviour
    {
        public static AdvancedAIManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableAdvancedAI = true;
        [SerializeField] private bool enableDynamicDifficulty = true;
        [SerializeField] private bool enableAdaptiveStrategies = true;
        [SerializeField] private bool enableSmartRouting = true;
        [SerializeField] private float analysisInterval = 5f; // seconds
        [SerializeField] private bool verboseLogging = true;

        [Header("Difficulty Settings")]
        [SerializeField] private float difficultyAdjustmentSpeed = 0.1f;
        [SerializeField] private float minDifficultyMultiplier = 0.5f;
        [SerializeField] private float maxDifficultyMultiplier = 2.0f;
        [SerializeField] private int performanceSampleSize = 10;

        [Header("Routing Settings")]
        [SerializeField] private float pathDangerThreshold = 0.7f;
        [SerializeField] private int routingUpdateFrequency = 3; // waves

        // State
        private bool isInitialized = false;
        private float currentDifficultyMultiplier = 1.0f;
        private PlayerPerformanceProfile performanceProfile;
        private List<float> recentPerformanceScores = new List<float>();
        private Dictionary<string, float> pathDangerLevels = new Dictionary<string, float>();
        private Dictionary<string, TowerPlacementPattern> towerPatterns = new Dictionary<string, TowerPlacementPattern>();
        private AdaptiveStrategyData currentStrategy;
        private List<AdaptiveStrategyData> adaptiveStrategies = new List<AdaptiveStrategyData>();
        private int wavesSinceLastAnalysis = 0;

        // Events
        public event Action<float> OnDifficultyAdjusted; // new multiplier
        public event Action<AdaptiveStrategyData> OnStrategyChanged;
        public event Action<string, float> OnPathDangerUpdated; // pathId, dangerLevel
        public event Action OnAIAnalysisCompleted;

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
            if (!enableAdvancedAI)
            {
                LogDebug("Advanced AI disabled");
                return;
            }

            StartCoroutine(InitializeAISystem());
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Initialization ────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator InitializeAISystem()
        {
            LogDebug("Initializing advanced AI system...");

            yield return new WaitForSeconds(0.3f);

            // Initialize performance profile
            LoadPerformanceProfile();

            if (performanceProfile == null)
            {
                performanceProfile = new PlayerPerformanceProfile
                {
                    averageAccuracy = 0.5f,
                    averageEfficiency = 0.5f,
                    averageWinRate = 0.5f,
                    preferredTowerTypes = new List<string>(),
                    weakTowerTypes = new List<string>(),
                    totalGamesPlayed = 0,
                    averageGameDuration = 0f
                };
            }

            // Initialize adaptive strategies
            InitializeAdaptiveStrategies();

            // Start analysis coroutine
            StartCoroutine(PeriodicAnalysisCoroutine());

            isInitialized = true;
            LogDebug($"Advanced AI initialized (Difficulty: {currentDifficultyMultiplier:F2}x)");

            // Track analytics
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackEvent("advanced_ai_initialized", new Dictionary<string, object>
                {
                    { "difficulty_multiplier", currentDifficultyMultiplier },
                    { "dynamic_difficulty", enableDynamicDifficulty },
                    { "adaptive_strategies", enableAdaptiveStrategies },
                    { "smart_routing", enableSmartRouting }
                });
            }
        }

        private void InitializeAdaptiveStrategies()
        {
            adaptiveStrategies = new List<AdaptiveStrategyData>
            {
                new AdaptiveStrategyData
                {
                    strategyId = "aggressive",
                    strategyName = "Aggressive Rush",
                    description = "Fast enemies with high damage",
                    healthMultiplier = 0.8f,
                    speedMultiplier = 1.3f,
                    damageMultiplier = 1.2f,
                    spawnRateMultiplier = 1.2f,
                    preferredCondition = "Player has strong defenses"
                },
                new AdaptiveStrategyData
                {
                    strategyId = "defensive",
                    strategyName = "Defensive Swarm",
                    description = "Many weak enemies",
                    healthMultiplier = 0.6f,
                    speedMultiplier = 1.0f,
                    damageMultiplier = 0.8f,
                    spawnRateMultiplier = 1.5f,
                    preferredCondition = "Player has area-of-effect towers"
                },
                new AdaptiveStrategyData
                {
                    strategyId = "tank",
                    strategyName = "Heavy Tank",
                    description = "Few but very strong enemies",
                    healthMultiplier = 1.5f,
                    speedMultiplier = 0.7f,
                    damageMultiplier = 1.3f,
                    spawnRateMultiplier = 0.6f,
                    preferredCondition = "Player has high damage towers"
                },
                new AdaptiveStrategyData
                {
                    strategyId = "balanced",
                    strategyName = "Balanced Assault",
                    description = "Standard enemy composition",
                    healthMultiplier = 1.0f,
                    speedMultiplier = 1.0f,
                    damageMultiplier = 1.0f,
                    spawnRateMultiplier = 1.0f,
                    preferredCondition = "Default strategy"
                }
            };

            currentStrategy = adaptiveStrategies[3]; // Start with balanced
            LogDebug($"Initialized {adaptiveStrategies.Count} adaptive strategies");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Dynamic Difficulty Adjustment ─────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Analyzes player performance and adjusts difficulty.
        /// </summary>
        public void AnalyzePerformance(GamePerformanceData performanceData)
        {
            if (!enableDynamicDifficulty || !isInitialized)
                return;

            // Calculate performance score (0-1 scale)
            float performanceScore = CalculatePerformanceScore(performanceData);
            recentPerformanceScores.Add(performanceScore);

            // Keep only recent samples
            if (recentPerformanceScores.Count > performanceSampleSize)
            {
                recentPerformanceScores.RemoveAt(0);
            }

            // Calculate average performance
            float averagePerformance = recentPerformanceScores.Average();

            // Update performance profile
            UpdatePerformanceProfile(performanceData, performanceScore);

            // Adjust difficulty based on performance
            AdjustDifficulty(averagePerformance);

            LogDebug($"Performance analyzed: Score={performanceScore:F2}, Avg={averagePerformance:F2}, Difficulty={currentDifficultyMultiplier:F2}x");
        }

        private float CalculatePerformanceScore(GamePerformanceData data)
        {
            // Higher score = player doing well (increase difficulty)
            // Lower score = player struggling (decrease difficulty)

            float accuracyScore = data.accuracy; // 0-1
            float efficiencyScore = data.efficiency; // 0-1
            float survivabilityScore = Mathf.Clamp01(data.livesRemaining / 20f); // Normalize lives
            float economyScore = Mathf.Clamp01(data.creditsEarned / 10000f); // Normalize credits

            // Weighted average
            float totalScore = (accuracyScore * 0.3f) +
                               (efficiencyScore * 0.25f) +
                               (survivabilityScore * 0.3f) +
                               (economyScore * 0.15f);

            return Mathf.Clamp01(totalScore);
        }

        private void AdjustDifficulty(float averagePerformance)
        {
            float targetMultiplier = currentDifficultyMultiplier;

            // If player is performing well (>0.7), increase difficulty
            if (averagePerformance > 0.7f)
            {
                targetMultiplier += difficultyAdjustmentSpeed * (averagePerformance - 0.7f);
            }
            // If player is struggling (<0.4), decrease difficulty
            else if (averagePerformance < 0.4f)
            {
                targetMultiplier -= difficultyAdjustmentSpeed * (0.4f - averagePerformance);
            }

            // Clamp to min/max
            targetMultiplier = Mathf.Clamp(targetMultiplier, minDifficultyMultiplier, maxDifficultyMultiplier);

            if (Mathf.Abs(targetMultiplier - currentDifficultyMultiplier) > 0.01f)
            {
                float oldMultiplier = currentDifficultyMultiplier;
                currentDifficultyMultiplier = targetMultiplier;
                SavePerformanceProfile();

                OnDifficultyAdjusted?.Invoke(currentDifficultyMultiplier);

                LogDebug($"Difficulty adjusted: {oldMultiplier:F2}x -> {currentDifficultyMultiplier:F2}x");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("difficulty_adjusted", new Dictionary<string, object>
                    {
                        { "old_multiplier", oldMultiplier },
                        { "new_multiplier", currentDifficultyMultiplier },
                        { "performance_score", averagePerformance }
                    });
                }
            }
        }

        private void UpdatePerformanceProfile(GamePerformanceData data, float performanceScore)
        {
            performanceProfile.totalGamesPlayed++;

            // Running average for accuracy and efficiency
            float n = performanceProfile.totalGamesPlayed;
            performanceProfile.averageAccuracy = ((performanceProfile.averageAccuracy * (n - 1)) + data.accuracy) / n;
            performanceProfile.averageEfficiency = ((performanceProfile.averageEfficiency * (n - 1)) + data.efficiency) / n;
            performanceProfile.averageWinRate = ((performanceProfile.averageWinRate * (n - 1)) + (data.isVictory ? 1f : 0f)) / n;
            performanceProfile.averageGameDuration = ((performanceProfile.averageGameDuration * (n - 1)) + data.gameDuration) / n;

            SavePerformanceProfile();
        }

        /// <summary>
        /// Gets current difficulty multiplier.
        /// </summary>
        public float GetDifficultyMultiplier()
        {
            return currentDifficultyMultiplier;
        }

        /// <summary>
        /// Gets player performance profile.
        /// </summary>
        public PlayerPerformanceProfile GetPerformanceProfile()
        {
            return performanceProfile;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Adaptive Enemy Strategies ─────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Updates adaptive strategy based on player's tower composition.
        /// </summary>
        public void UpdateAdaptiveStrategy(List<string> activeTowerTypes)
        {
            if (!enableAdaptiveStrategies || !isInitialized)
                return;

            // Analyze tower composition
            AnalyzeTowerComposition(activeTowerTypes);

            // Select best counter-strategy
            AdaptiveStrategyData bestStrategy = SelectCounterStrategy(activeTowerTypes);

            if (bestStrategy != currentStrategy)
            {
                currentStrategy = bestStrategy;
                OnStrategyChanged?.Invoke(currentStrategy);

                LogDebug($"Strategy changed to: {currentStrategy.strategyName}");

                // Track analytics
                if (AnalyticsManager.Instance != null)
                {
                    AnalyticsManager.Instance.TrackEvent("ai_strategy_changed", new Dictionary<string, object>
                    {
                        { "strategy_id", currentStrategy.strategyId },
                        { "strategy_name", currentStrategy.strategyName }
                    });
                }
            }
        }

        private void AnalyzeTowerComposition(List<string> activeTowerTypes)
        {
            // Track tower type counts
            var towerTypeCounts = activeTowerTypes
                .GroupBy(t => t)
                .ToDictionary(g => g.Key, g => g.Count());

            // Update tower patterns
            foreach (var kvp in towerTypeCounts)
            {
                if (!towerPatterns.ContainsKey(kvp.Key))
                {
                    towerPatterns[kvp.Key] = new TowerPlacementPattern
                    {
                        towerType = kvp.Key,
                        placementCount = 0,
                        totalEffectiveness = 0f
                    };
                }

                towerPatterns[kvp.Key].placementCount++;
            }
        }

        private AdaptiveStrategyData SelectCounterStrategy(List<string> activeTowerTypes)
        {
            // Count tower types
            int aoeCount = activeTowerTypes.Count(t => t.Contains("AOE") || t.Contains("Splash"));
            int singleTargetCount = activeTowerTypes.Count(t => !t.Contains("AOE") && !t.Contains("Splash"));
            int totalTowers = activeTowerTypes.Count;

            if (totalTowers == 0)
                return adaptiveStrategies[3]; // Balanced

            float aoeRatio = (float)aoeCount / totalTowers;

            // Select strategy based on composition
            if (aoeRatio > 0.6f)
            {
                // Player has many AOE towers -> use tank strategy (few strong enemies)
                return adaptiveStrategies.First(s => s.strategyId == "tank");
            }
            else if (singleTargetCount > 8)
            {
                // Player has many single-target towers -> use swarm strategy
                return adaptiveStrategies.First(s => s.strategyId == "defensive");
            }
            else if (totalTowers > 15)
            {
                // Player has strong defenses -> use aggressive rush
                return adaptiveStrategies.First(s => s.strategyId == "aggressive");
            }

            return adaptiveStrategies.First(s => s.strategyId == "balanced");
        }

        /// <summary>
        /// Gets current adaptive strategy.
        /// </summary>
        public AdaptiveStrategyData GetCurrentStrategy()
        {
            return currentStrategy;
        }

        /// <summary>
        /// Applies strategy modifiers to enemy stats.
        /// </summary>
        public void ApplyStrategyModifiers(Enemy enemy)
        {
            if (currentStrategy == null)
                return;

            // Apply strategy multipliers
            enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * currentStrategy.healthMultiplier * currentDifficultyMultiplier);
            enemy.currentHealth = enemy.maxHealth;
            enemy.maxSpeed *= currentStrategy.speedMultiplier;
            enemy.damage = Mathf.RoundToInt(enemy.damage * currentStrategy.damageMultiplier * currentDifficultyMultiplier);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Smart Enemy Routing ───────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Analyzes path danger levels based on tower coverage.
        /// </summary>
        public void AnalyzePathDanger(Dictionary<string, List<Vector3>> paths, List<Tower> activeTowers)
        {
            if (!enableSmartRouting || !isInitialized)
                return;

            pathDangerLevels.Clear();

            foreach (var pathKvp in paths)
            {
                string pathId = pathKvp.Key;
                List<Vector3> waypoints = pathKvp.Value;

                float dangerLevel = CalculatePathDanger(waypoints, activeTowers);
                pathDangerLevels[pathId] = dangerLevel;

                OnPathDangerUpdated?.Invoke(pathId, dangerLevel);
            }

            LogDebug($"Path danger analysis complete: {pathDangerLevels.Count} paths analyzed");
        }

        private float CalculatePathDanger(List<Vector3> waypoints, List<Tower> towers)
        {
            float totalDanger = 0f;
            int sampleCount = 0;

            // Sample points along path
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Vector3 start = waypoints[i];
                Vector3 end = waypoints[i + 1];
                float distance = Vector3.Distance(start, end);
                int samples = Mathf.Max(1, Mathf.RoundToInt(distance / 2f)); // Sample every 2 units

                for (int j = 0; j <= samples; j++)
                {
                    float t = (float)j / samples;
                    Vector3 samplePoint = Vector3.Lerp(start, end, t);

                    // Count towers in range of this point
                    int towersInRange = towers.Count(tower =>
                        Vector3.Distance(tower.transform.position, samplePoint) <= tower.GetRange());

                    totalDanger += towersInRange;
                    sampleCount++;
                }
            }

            // Normalize danger (0-1 scale, capped at 10 towers)
            return sampleCount > 0 ? Mathf.Clamp01(totalDanger / (sampleCount * 10f)) : 0f;
        }

        /// <summary>
        /// Selects optimal path based on danger levels.
        /// </summary>
        public string SelectOptimalPath(Dictionary<string, List<Vector3>> paths)
        {
            if (!enableSmartRouting || pathDangerLevels.Count == 0)
                return paths.Keys.First(); // Default to first path

            // Find path with lowest danger
            string optimalPath = pathDangerLevels.OrderBy(kvp => kvp.Value).First().Key;

            LogDebug($"Selected optimal path: {optimalPath} (Danger: {pathDangerLevels[optimalPath]:F2})");

            return optimalPath;
        }

        /// <summary>
        /// Checks if enemy should use alternate path.
        /// </summary>
        public bool ShouldUseAlternatePath(string currentPath)
        {
            if (!pathDangerLevels.ContainsKey(currentPath))
                return false;

            float currentDanger = pathDangerLevels[currentPath];
            return currentDanger > pathDangerThreshold;
        }

        /// <summary>
        /// Gets path danger level.
        /// </summary>
        public float GetPathDanger(string pathId)
        {
            return pathDangerLevels.ContainsKey(pathId) ? pathDangerLevels[pathId] : 0f;
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Periodic Analysis ─────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private IEnumerator PeriodicAnalysisCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(analysisInterval);

                if (isInitialized)
                {
                    PerformPeriodicAnalysis();
                }
            }
        }

        private void PerformPeriodicAnalysis()
        {
            wavesSinceLastAnalysis++;

            // Update routing analysis periodically
            if (enableSmartRouting && wavesSinceLastAnalysis >= routingUpdateFrequency)
            {
                wavesSinceLastAnalysis = 0;
                // Path analysis would be triggered by game manager
            }

            OnAIAnalysisCompleted?.Invoke();

            LogDebug("Periodic AI analysis completed");
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Local Storage ─────────────────────────────────────────────────────
        // ══════════════════════════════════════════════════════════════════════

        private void LoadPerformanceProfile()
        {
            string json = PlayerPrefs.GetString("PlayerPerformanceProfile", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    performanceProfile = JsonUtility.FromJson<PlayerPerformanceProfile>(json);
                    currentDifficultyMultiplier = CalculateInitialDifficulty(performanceProfile);
                    LogDebug($"Loaded performance profile: Games={performanceProfile.totalGamesPlayed}, WinRate={performanceProfile.averageWinRate:F2}");
                }
                catch
                {
                    LogDebug("Failed to load performance profile");
                }
            }
        }

        private void SavePerformanceProfile()
        {
            if (performanceProfile == null)
                return;

            string json = JsonUtility.ToJson(performanceProfile);
            PlayerPrefs.SetString("PlayerPerformanceProfile", json);
            PlayerPrefs.Save();
        }

        private float CalculateInitialDifficulty(PlayerPerformanceProfile profile)
        {
            // Calculate initial difficulty based on past performance
            float baseMultiplier = 1.0f;

            if (profile.totalGamesPlayed > 5)
            {
                // Adjust based on win rate
                if (profile.averageWinRate > 0.7f)
                {
                    baseMultiplier = 1.2f;
                }
                else if (profile.averageWinRate < 0.3f)
                {
                    baseMultiplier = 0.8f;
                }
            }

            return Mathf.Clamp(baseMultiplier, minDifficultyMultiplier, maxDifficultyMultiplier);
        }

        // ══════════════════════════════════════════════════════════════════════
        // ── Logging ───────────────────────────────────────────────────────────
        // ══════════════════════════════════════────════════════════════════════

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[AdvancedAIManager] {message}");
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ── Data Structures ───────────────────────────────────────────────────────
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Player performance profile for AI learning.
    /// </summary>
    [Serializable]
    public class PlayerPerformanceProfile
    {
        public float averageAccuracy;
        public float averageEfficiency;
        public float averageWinRate;
        public int totalGamesPlayed;
        public float averageGameDuration;
        public List<string> preferredTowerTypes;
        public List<string> weakTowerTypes;
    }

    /// <summary>
    /// Game performance data for analysis.
    /// </summary>
    [Serializable]
    public class GamePerformanceData
    {
        public float accuracy; // 0-1
        public float efficiency; // 0-1
        public int livesRemaining;
        public int creditsEarned;
        public float gameDuration; // seconds
        public bool isVictory;
        public int enemiesKilled;
        public int wavesCompleted;
    }

    /// <summary>
    /// Adaptive strategy configuration.
    /// </summary>
    [Serializable]
    public class AdaptiveStrategyData
    {
        public string strategyId;
        public string strategyName;
        public string description;
        public float healthMultiplier;
        public float speedMultiplier;
        public float damageMultiplier;
        public float spawnRateMultiplier;
        public string preferredCondition;
    }

    /// <summary>
    /// Tower placement pattern tracking.
    /// </summary>
    [Serializable]
    public class TowerPlacementPattern
    {
        public string towerType;
        public int placementCount;
        public float totalEffectiveness;
        public List<Vector3> commonPositions;
    }
}
