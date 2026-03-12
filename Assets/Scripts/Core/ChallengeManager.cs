using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using RobotTD.Analytics;

namespace RobotTD.Core
{
    /// <summary>
    /// Manages challenge mode gameplay, modifier application, and rotation.
    /// </summary>
    public class ChallengeManager : MonoBehaviour
    {
        public static ChallengeManager Instance { get; private set; }
        
        // ── Events ───────────────────────────────────────────────────────────
        
        public static event Action<ChallengeData> OnChallengeStarted;
        public static event Action<ChallengeData, bool> OnChallengeCompleted; // challenge, victory
        public static event Action OnDailyChallengeRotated;
        public static event Action OnWeeklyChallengeRotated;
        
        // ── Inspector ────────────────────────────────────────────────────────
        
        [Header("Challenge Library")]
        [SerializeField] private ChallengeData[] allChallenges;
        
        [Header("Rotation Settings")]
        [SerializeField] private bool enableDailyRotation = true;
        [SerializeField] private bool enableWeeklyRotation = true;
        [SerializeField] private int dailyChallengesCount = 3;
        [SerializeField] private int weeklyChallengesCount = 2;
        
        [Header("Modifier Values")]
        [SerializeField] private float speedRushMultiplier = 1.5f;
        [SerializeField] private float armoredAssaultHPMultiplier = 2.0f;
        [SerializeField] private float swarmModeCountMultiplier = 1.5f;
        [SerializeField] private float budgetCrisisCostMultiplier = 1.5f;
        [SerializeField] private float weakenedTowersDamageMultiplier = 0.7f;
        [SerializeField] private float economicHardshipRewardMultiplier = 0.5f;
        [SerializeField] private int towerLimitMax = 10;
        [SerializeField] private float placementDelay = 3f;
        [SerializeField] private float fastForwardWaveDelay = 3f;
        
        // Active modifier multipliers (for tower systems to query)
        private float activeTowerCostMultiplier = 1f;
        private float activeTowerDamageMultiplier = 1f;
        private float activeEconomyMultiplier = 1f;
        
        // ── State ────────────────────────────────────────────────────────────
        
        public bool IsChallengeActive { get; private set; }
        public ChallengeData CurrentChallenge { get; private set; }
        
        private Dictionary<string, ChallengeProgress> progressData = new Dictionary<string, ChallengeProgress>();
        private List<ChallengeData> activeDailyChallenges = new List<ChallengeData>();
        private List<ChallengeData> activeWeeklyChallenges = new List<ChallengeData>();
        private DateTime lastDailyRotation;
        private DateTime lastWeeklyRotation;
        private int placedTowerCount = 0;
        
        // ── Unity ────────────────────────────────────────────────────────────
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadProgress();
            InitializeRotation();
        }
        
        private void OnEnable()
        {
            GameManager.OnGameStarted += HandleGameStarted;
            GameManager.OnVictory += HandleVictory;
            GameManager.OnGameOver += HandleDefeat;
            
            if (Towers.TowerPlacementManager.Instance != null)
            {
                Towers.TowerPlacementManager.Instance.OnTowerPlaced += HandleTowerPlaced;
                Towers.TowerPlacementManager.Instance.OnTowerSold += HandleTowerSold;
            }
        }
        
        private void OnDisable()
        {
            GameManager.OnGameStarted -= HandleGameStarted;
            GameManager.OnVictory -= HandleVictory;
            GameManager.OnGameOver -= HandleDefeat;
            
            if (Towers.TowerPlacementManager.Instance != null)
            {
                Towers.TowerPlacementManager.Instance.OnTowerPlaced -= HandleTowerPlaced;
                Towers.TowerPlacementManager.Instance.OnTowerSold -= HandleTowerSold;
            }
        }
        
        private void Update()
        {
            CheckRotations();
        }
        
        // ── Challenge Control ────────────────────────────────────────────────
        
        /// <summary>
        /// Start a challenge by ID.
        /// </summary>
        public bool StartChallenge(string challengeId)
        {
            ChallengeData challenge = GetChallengeById(challengeId);
            if (challenge == null)
            {
                Debug.LogWarning($"[ChallengeManager] Challenge not found: {challengeId}");
                return false;
            }
            
            return StartChallenge(challenge);
        }
        
        /// <summary>
        /// Start a challenge with the given data.
        /// </summary>
        public bool StartChallenge(ChallengeData challenge)
        {
            if (IsChallengeActive)
            {
                Debug.LogWarning("[ChallengeManager] Challenge already active!");
                return false;
            }
            
            CurrentChallenge = challenge;
            IsChallengeActive = true;
            placedTowerCount = 0;
            
            // Track attempt
            if (!progressData.ContainsKey(challenge.ChallengeId))
                progressData[challenge.ChallengeId] = new ChallengeProgress(challenge.ChallengeId);
            
            progressData[challenge.ChallengeId].attempts++;
            
            // Apply modifiers
            ApplyModifiers(challenge);
            
            OnChallengeStarted?.Invoke(challenge);
            
            // Analytics
            AnalyticsManager.Instance?.TrackChallengeStarted(
                challenge.ChallengeId,
                challenge.ChallengeName,
                challenge.Difficulty.ToString()
            );
            
            Debug.Log($"[ChallengeManager] Started challenge: {challenge.ChallengeName}");
            return true;
        }
        
        /// <summary>
        /// End the current challenge.
        /// </summary>
        public void EndChallenge(bool victory)
        {
            if (!IsChallengeActive) return;
            
            ChallengeData challenge = CurrentChallenge;
            
            if (victory)
            {
                CompleteChallenge(challenge);
            }
            
            OnChallengeCompleted?.Invoke(challenge, victory);
            
            // Analytics
            long finalScore = GameManager.Instance?.Score ?? 0;
            int finalWave = WaveManager.Instance?.CurrentWave ?? 0;
            
            AnalyticsManager.Instance?.TrackChallengeCompleted(
                challenge.ChallengeId,
                victory,
                finalScore,
                finalWave
            );
            
            // Cleanup
            RemoveModifiers(challenge);
            IsChallengeActive = false;
            CurrentChallenge = null;
            placedTowerCount = 0;
        }
        
        /// <summary>
        /// Mark challenge as completed and give rewards.
        /// </summary>
        private void CompleteChallenge(ChallengeData challenge)
        {
            if (!progressData.ContainsKey(challenge.ChallengeId))
                progressData[challenge.ChallengeId] = new ChallengeProgress(challenge.ChallengeId);
            
            var progress = progressData[challenge.ChallengeId];
            long currentScore = GameManager.Instance?.Score ?? 0;
            int currentWave = WaveManager.Instance?.CurrentWave ?? 0;
            
            // Update best score
            if (currentScore > progress.bestScore)
            {
                progress.bestScore = currentScore;
                progress.bestWave = currentWave;
            }
            
            // First time completion
            if (!progress.completed)
            {
                progress.completed = true;
                progress.completionDate = DateTime.Now;
                
                // Give rewards
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddCredits(challenge.CreditReward);
                }
                
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.Data.techPoints += challenge.TechPointReward;
                }
                
                // Unlock achievement if linked
                if (!string.IsNullOrEmpty(challenge.AchievementId))
                {
                    Progression.AchievementManager.Instance?.UnlockAchievement(challenge.AchievementId);
                }
                
                // Check challenge completion achievements
                Progression.AchievementManager.Instance?.CheckChallengeComplete(
                    challenge.ChallengeId, 
                    challenge.Difficulty.ToString()
                );
                
                Debug.Log($"[ChallengeManager] Challenge completed! Rewards: {challenge.CreditReward} credits, {challenge.TechPointReward} tech points");
            }
            
            SaveProgress();
        }
        
        // ── Modifier Application ─────────────────────────────────────────────
        
        private void ApplyModifiers(ChallengeData challenge)
        {
            // Accumulate all multipliers before applying
            float totalHealthMult = 1f;
            float totalSpeedMult = 1f;
            float totalCountMult = 1f;
            
            foreach (var modifier in challenge.Modifiers)
            {
                Debug.Log($"[ChallengeManager] Applying modifier: {modifier}");
                
                switch (modifier)
                {
                    case ChallengeModifier.SpeedRush:
                        totalSpeedMult *= speedRushMultiplier;
                        break;
                    
                    case ChallengeModifier.ArmoredAssault:
                        totalHealthMult *= armoredAssaultHPMultiplier;
                        break;
                    
                    case ChallengeModifier.SwarmMode:
                        totalCountMult *= swarmModeCountMultiplier;
                        break;
                    
                    case ChallengeModifier.BudgetCrisis:
                        ApplyTowerCostModifier(budgetCrisisCostMultiplier);
                        break;
                    
                    case ChallengeModifier.WeakenedTowers:
                        ApplyTowerDamageModifier(weakenedTowersDamageMultiplier);
                        break;
                    
                    case ChallengeModifier.EconomicHardship:
                        ApplyEconomyModifier(economicHardshipRewardMultiplier);
                        break;
                    
                    case ChallengeModifier.StartingDebt:
                        ApplyStartingCreditsModifier(0.5f);
                        break;
                    
                    case ChallengeModifier.FastForward:
                        ApplyWaveDelayModifier(fastForwardWaveDelay);
                        break;
                    
                    case ChallengeModifier.PerfectDefense:
                        ApplyOneLifeModifier();
                        break;
                }
            }
            
            // Apply accumulated enemy modifiers to WaveManager
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.SetChallengeMultipliers(totalHealthMult, totalSpeedMult, totalCountMult);
            }
        }
        
        private void RemoveModifiers(ChallengeData challenge)
        {individual modifier methods (for non-enemy modifiers)
        private void ApplyTowerCostModifier(float multiplier)
        {
            activeTowerCostMultiplier = 1f;
            activeTowerDamageMultiplier = 1f;
            activeEconomyMultiplier = 1f;
            activeTowerCostMultiplier *= multiplier;
            Debug.Log($"[ChallengeManager] Tower cost multiplier: {activeTowerCostMultiplier}x");
        }
        
        private void ApplyTowerDamageModifier(float multiplier)
        {
            activeTowerDamageMultiplier *= multiplier;
            Debug.Log($"[ChallengeManager] Tower damage multiplier: {activeTowerDamageMultiplier}x");
        }
        
        private void ApplyEconomyModifier(float multiplier)
        {
            activeEconomyMultiplier *= multiplier;
            Debug.Log($"[ChallengeManager] Economy multiplier: {activeEconomyM
            Debug.Log($"[ChallengeManager] Tower damage multiplier: {multiplier}x");
        }
        
        private void ApplyEconomyModifier(float multiplier)
        {
            Debug.Log($"[ChallengeManager] Economy multiplier: {multiplier}x");
        }
        
        private void ApplyStartingCreditsModifier(float multiplier)
        {
            if (GameManager.Instance != null)
            {
                int normalCredits = 500; // Should read from GameManager default
                int reducedCredits = Mathf.RoundToInt(normalCredits * multiplier);
                GameManager.Instance.SetStartingCredits(reducedCredits);
            }
        }
        
        private void ApplyWaveDelayModifier(float delay)
        {
            Debug.Log($"[ChallengeManager] Wave delay: {delay}s");
        }
        
        private void ApplyOneLifeModifier()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetLives(1);
            }
        }
        
        // ── Tower Limit Enforcement ──────────────────────────────────────────
        
        private void HandleTowerPlaced(Towers.Tower tower)
        {
            if (!IsChallengeActive) return;
            if (!CurrentChallenge.HasModifier(ChallengeModifier.TowerLimit)) return;
            
            placedTowerCount++;
        }
        
        private void HandleTowerSold(Towers.Tower tower)
        {
            if (!IsChallengeActive) return;
            if (!CurrentChallenge.HasModifier(ChallengeModifier.TowerLimit)) return;
            
            placedTowerCount--;
        }
        
        public bool CanPlaceTower()
        {
            if (!IsChallengeActive) return true;
            if (!CurrentChallenge.HasModifier(ChallengeModifier.TowerLimit)) return true;
            
            return placedTowerCount < towerLimitMax;
        }
        
        // ── Rotation System ──────────────────────────────────────────────────
        
        private void InitializeRotation()
        {
            // Load last rotation dates from PlayerPrefs
            string dailyDate = PlayerPrefs.GetString("LastDailyRotation", "");
            string weeklyDate = PlayerPrefs.GetString("LastWeeklyRotation", "");
            
            if (DateTime.TryParse(dailyDate, out DateTime daily))
                lastDailyRotation = daily;
            else
                lastDailyRotation = DateTime.Now;
            
            if (DateTime.TryParse(weeklyDate, out DateTime weekly))
                lastWeeklyRotation = weekly;
            else
                lastWeeklyRotation = DateTime.Now;
            
            // Force initial rotation
            RotateDaily();
            RotateWeekly();
        }
        
        private void CheckRotations()
        {
            if (enableDailyRotation && (DateTime.Now - lastDailyRotation).TotalHours >= 24)
            {
                RotateDaily();
            }
            
            if (enableWeeklyRotation && (DateTime.Now - lastWeeklyRotation).TotalDays >= 7)
            {
                RotateWeekly();
            }
        }
        
        private void RotateDaily()
        {
            activeDailyChallenges.Clear();
            
            var dailyChallenges = allChallenges
                .Where(c => c.RotationType == ChallengeRotationType.Daily)
                .OrderBy(c => UnityEngine.Random.value)
                .Take(dailyChallengesCount)
                .ToList();
            
            activeDailyChallenges.AddRange(dailyChallenges);
            lastDailyRotation = DateTime.Now;
            
            PlayerPrefs.SetString("LastDailyRotation", lastDailyRotation.ToString());
            PlayerPrefs.Save();
            
            OnDailyChallengeRotated?.Invoke();
            Debug.Log($"[ChallengeManager] Daily challenges rotated: {activeDailyChallenges.Count} active");
        }
        
        private void RotateWeekly()
        {
            activeWeeklyChallenges.Clear();
            
            var weeklyChallenges = allChallenges
                .Where(c => c.RotationType == ChallengeRotationType.Weekly)
                .OrderBy(c => UnityEngine.Random.value)
                .Take(weeklyChallengesCount)
                .ToList();
            
            activeWeeklyChallenges.AddRange(weeklyChallenges);
            lastWeeklyRotation = DateTime.Now;
            
            PlayerPrefs.SetString("LastWeeklyRotation", lastWeeklyRotation.ToString());
            PlayerPrefs.Save();
            
            OnWeeklyChallengeRotated?.Invoke();
            Debug.Log($"[ChallengeManager] Weekly challenges rotated: {activeWeeklyChallenges.Count} active");
        }
        
        // ── Query API ────────────────────────────────────────────────────────
        
        public ChallengeData GetChallengeById(string id)
        {
            return allChallenges.FirstOrDefault(c => c.ChallengeId == id);
        }
        
        public List<ChallengeData> GetActiveDailyChallenges() => new List<ChallengeData>(activeDailyChallenges);
        public List<ChallengeData> GetActiveWeeklyChallenges() => new List<ChallengeData>(activeWeeklyChallenges);
        
        public List<ChallengeData> GetPermanentChallenges()
        {
            return allChallenges.Where(c => c.RotationType == ChallengeRotationType.Permanent).ToList();
        }
        
        public ChallengeProgress GetProgress(string challengeId)
        {
            return progressData.ContainsKey(challengeId) ? progressData[challengeId] : null;
        }
        
        public bool IsChallengeCompleted(string challengeId)
        {
            return progressData.ContainsKey(challengeId) && progressData[challengeId].completed;
        }
        
        // ── Event Handlers ───────────────────────────────────────────────────
        
        private void HandleGameStarted()
        {
            // Challenge could modify game start behavior
        }
        
        private void HandleVictory()
        {
            if (IsChallengeActive)
                EndChallenge(true);
        }
        
        private void HandleDefeat()
        {
            if (IsChallengeActive)
                EndChallenge(false);
        }
        
        // ── Public Helper API ────────────────────────────────────────────────
        
        /// <summary>
        /// Get the tower cost multiplier for current challenge (1.0 = no modifier).
        /// Call this when calculating tower purchase or upgrade costs.
        /// </summary>
        public float GetTowerCostMultiplier()
        {
            return IsChallengeActive ? activeTowerCostMultiplier : 1f;
        }
        
        /// <summary>
        /// Get the tower damage multiplier for current challenge (1.0 = no modifier).
        /// Call this when calculating tower damage output.
        /// </summary>
        public float GetTowerDamageMultiplier()
        {
            return IsChallengeActive ? activeTowerDamageMultiplier : 1f;
        }
        
        /// <summary>
        /// Get the economy multiplier for current challenge (1.0 = no modifier).
        /// Call this when calculating enemy kill rewards.
        /// </summary>
        public float GetEconomyMultiplier()
        {
            return IsChallengeActive ? activeEconomyMultiplier : 1f;
        }
        
        /// <summary>
        /// Check if a specific modifier is active in the current challenge.
        /// </summary>
        public bool HasActiveModifier(ChallengeModifier modifier)
        {
            return IsChallengeActive && CurrentChallenge != null && CurrentChallenge.HasModifier(modifier);
        }
        
        // ── Persistence ──────────────────────────────────────────────────────
        
        private void LoadProgress()
        {
            string json = PlayerPrefs.GetString("ChallengeProgress", "{}");
            
            try
            {
                var wrapper = JsonUtility.FromJson<ChallengeProgressWrapper>(json);
                if (wrapper?.challenges != null)
                {
                    foreach (var progress in wrapper.challenges)
                    {
                        progressData[progress.challengeId] = progress;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ChallengeManager] Failed to load progress: {ex.Message}");
            }
        }
        
        private void SaveProgress()
        {
            var wrapper = new ChallengeProgressWrapper
            {
                challenges = progressData.Values.ToList()
            };
            
            string json = JsonUtility.ToJson(wrapper, true);
            PlayerPrefs.SetString("ChallengeProgress", json);
            PlayerPrefs.Save();
        }
        
        [Serializable]
        private class ChallengeProgressWrapper
        {
            public List<ChallengeProgress> challenges;
        }
    }
}
