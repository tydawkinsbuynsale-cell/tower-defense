using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RobotTD.Enemies;
using RobotTD.Online;

namespace RobotTD.Core
{
    /// <summary>
    /// Boss Rush Mode — Sequential boss battles with escalating difficulty.
    /// Fight one boss at a time with breaks between encounters.
    /// Tracks defeated bosses and awards credits for each victory.
    /// </summary>
    public class BossRushMode : MonoBehaviour
    {
        public static BossRushMode Instance { get; private set; }

        // ── Events ───────────────────────────────────────────────────────────

        public static event Action<int, string> OnBossEncounterStarted;   // boss number, boss name
        public static event Action<int, int> OnBossDefeated;              // boss number, credits earned
        public static event Action OnPrepPhaseStarted;                    // break time between bosses

        // ── Inspector ────────────────────────────────────────────────────────

        [Header("Boss Configuration")]
        [SerializeField] private EnemyData[] bossPrefabs;  // Array of boss enemy data
        [SerializeField] private string[] bossNames = { 
            "Swarm Mother", "Shield Commander", "Tank Destroyer", 
            "Repair Drone Master", "Artillery Juggernaut" 
        };

        [Header("Scaling")]
        [SerializeField] private float healthScalePerBoss = 0.5f;   // +50% HP each boss
        [SerializeField] private float speedScalePerBoss = 0.1f;    // +10% speed each boss
        [SerializeField] private int baseCreditsReward = 500;       // Credits per boss defeated

        [Header("Timings")]
        [SerializeField] private float prepPhaseDuration = 30f;     // Break time between bosses
        [SerializeField] private float bossIntroDelay = 3f;         // Delay before boss spawns

        [Header("Integration")]
        [SerializeField] private Transform bossSpawnPoint;          // Where boss spawns (different from enemy path)

        // ── State ────────────────────────────────────────────────────────────

        public bool IsActive { get; private set; }
        public bool InPrepPhase { get; private set; }
        public int CurrentBossNumber { get; private set; }
        public long TotalScore { get; private set; }
        public float PrepTimeRemaining { get; private set; }

        private Coroutine bossRushRoutine;
        private Enemy currentBoss;
        private List<EnemyData> bossQueue;

        // ── Unity ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Activation ───────────────────────────────────────────────────────

        /// <summary>
        /// Start Boss Rush Mode from main menu or after campaign completion.
        /// </summary>
        public void StartBossRush()
        {
            if (IsActive) return;

            IsActive = true;
            InPrepPhase = false;
            CurrentBossNumber = 0;
            TotalScore = 0;
            currentBoss = null;

            // Build boss queue
            BuildBossQueue();

            // Prepare game state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetCredits(1000);  // Start with more credits
                GameManager.Instance.ResetLives(20);       // More lives for boss fights
            }

            bossRushRoutine = StartCoroutine(BossRushLoop());
            Debug.Log("[BossRushMode] Boss Rush Mode started!");
        }

        public void StopBossRush()
        {
            IsActive = false;
            InPrepPhase = false;
            if (bossRushRoutine != null) StopCoroutine(bossRushRoutine);
            if (currentBoss != null && !currentBoss.IsDead)
            {
                currentBoss.Die();
            }
        }

        // ── Boss Queue Management ────────────────────────────────────────────

        private void BuildBossQueue()
        {
            bossQueue = new List<EnemyData>();

            // If bossPrefabs is empty, we'll need to generate bosses dynamically
            if (bossPrefabs == null || bossPrefabs.Length == 0)
            {
                Debug.LogWarning("[BossRushMode] No boss prefabs assigned! Boss Rush will not work properly.");
                return;
            }

            // Create endless boss queue by repeating and scaling
            int bossIndex = 0;
            for (int i = 0; i < 100; i++)  // Support up to 100 bosses (virtually infinite)
            {
                bossQueue.Add(bossPrefabs[bossIndex % bossPrefabs.Length]);
                bossIndex++;
            }
        }

        // ── Main Loop ────────────────────────────────────────────────────────

        private IEnumerator BossRushLoop()
        {
            while (IsActive)
            {
                if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
                {
                    yield break;
                }

                CurrentBossNumber++;

                // Check if there are bosses left
                if (bossQueue == null || CurrentBossNumber > bossQueue.Count)
                {
                    Debug.Log("[BossRushMode] All bosses defeated! Victory!");
                    GameManager.Instance?.TriggerVictory();
                    yield break;
                }

                // Prep phase (except before first boss)
                if (CurrentBossNumber > 1)
                {
                    yield return StartCoroutine(PrepPhase());
                }

                // Boss intro delay
                yield return new WaitForSeconds(bossIntroDelay);

                // Spawn boss
                SpawnBoss(CurrentBossNumber);

                // Wait for boss to be defeated or game over
                yield return new WaitUntil(() => 
                    currentBoss == null || 
                    currentBoss.IsDead || 
                    !IsActive || 
                    GameManager.Instance.IsGameOver);

                // Check if boss was defeated
                if (IsActive && currentBoss != null && currentBoss.IsDead)
                {
                    RewardBossDefeat(CurrentBossNumber);
                }
                else if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                {
                    // Player lost
                    yield break;
                }
            }
        }

        // ── Prep Phase ───────────────────────────────────────────────────────

        private IEnumerator PrepPhase()
        {
            InPrepPhase = true;
            PrepTimeRemaining = prepPhaseDuration;
            OnPrepPhaseStarted?.Invoke();

            Debug.Log($"[BossRushMode] Prep phase started - {prepPhaseDuration} seconds");

            while (PrepTimeRemaining > 0 && IsActive)
            {
                PrepTimeRemaining -= Time.deltaTime;
                yield return null;
            }

            InPrepPhase = false;
            Debug.Log("[BossRushMode] Prep phase complete");
        }

        // ── Boss Spawning ────────────────────────────────────────────────────

        private void SpawnBoss(int bossNumber)
        {
            if (bossQueue == null || bossNumber < 1 || bossNumber > bossQueue.Count)
            {
                Debug.LogError($"[BossRushMode] Invalid boss number: {bossNumber}");
                return;
            }

            EnemyData bossData = bossQueue[bossNumber - 1];
            if (bossData == null || bossData.enemyPrefab == null)
            {
                Debug.LogError($"[BossRushMode] Boss #{bossNumber} has no prefab assigned!");
                return;
            }

            // Calculate scaling
            float healthMult = 1f + (bossNumber - 1) * healthScalePerBoss;
            float speedMult = 1f + (bossNumber - 1) * speedScalePerBoss;

            // Spawn boss
            Vector3 spawnPos = bossSpawnPoint != null 
                ? bossSpawnPoint.position 
                : Vector3.zero;

            GameObject bossObj = ObjectPooler.Instance?.GetPooledObject(bossData.enemyPrefab.name);
            if (bossObj == null)
            {
                bossObj = Instantiate(bossData.enemyPrefab);
            }

            bossObj.transform.position = spawnPos;
            currentBoss = bossObj.GetComponent<Enemy>();

            if (currentBoss != null)
            {
                // Get path from WaveManager
                Transform[] path = WaveManager.Instance?.GetEnemyPath();
                if (path == null || path.Length == 0)
                {
                    Debug.LogError("[BossRushMode] No enemy path available for boss!");
                    Destroy(bossObj);
                    return;
                }

                // Initialize with scaling
                currentBoss.Initialize(path, healthMult, speedMult);

                // Subscribe to death event
                currentBoss.OnDied.AddListener(() => OnCurrentBossDied());
            }

            // Get boss name
            string bossName = bossNumber <= bossNames.Length 
                ? bossNames[bossNumber - 1] 
                : $"Unknown Boss #{bossNumber}";

            OnBossEncounterStarted?.Invoke(bossNumber, bossName);
            Debug.Log($"[BossRushMode] Boss #{bossNumber} ({bossName}) spawned! HP: {healthMult * 100}%, Speed: {speedMult * 100}%");
        }

        private void OnCurrentBossDied()
        {
            Debug.Log($"[BossRushMode] Boss #{CurrentBossNumber} defeated!");
        }

        // ── Rewards ──────────────────────────────────────────────────────────

        private void RewardBossDefeat(int bossNumber)
        {
            // Calculate reward with scaling
            int reward = baseCreditsReward + (bossNumber * 100);
            GameManager.Instance?.AddCredits(reward);
            TotalScore += reward;

            OnBossDefeated?.Invoke(bossNumber, reward);

            // Achievement hook
            Progression.AchievementManager.Instance?.CheckBossRushProgress(bossNumber);

            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.Victory);

            Debug.Log($"[BossRushMode] Boss {bossNumber} defeated — rewarded {reward} credits. Total score: {TotalScore}");
        }

        // ── Leaderboard Hook ─────────────────────────────────────────────────

        /// <summary>
        /// Call this to post the boss rush score (e.g. on game over).
        /// Submits to LeaderboardManager with bosses defeated and score.
        /// </summary>
        public void PostBossRushScore()
        {
            if (!IsActive || TotalScore <= 0) return;

            // Combined score: GameManager base score + boss rush bonus
            long totalScore = (GameManager.Instance?.Score ?? 0) + TotalScore;

            // Submit to leaderboard
            LeaderboardManager.Instance?.SubmitBossRushScore(CurrentBossNumber, totalScore);

            Debug.Log($"[BossRushMode] Final boss rush score: {totalScore} (bosses defeated: {CurrentBossNumber})");

            // Save personal best for boss rush mode
            var sm = SaveManager.Instance;
            if (sm != null)
            {
                bool newRecord = false;

                // Update boss rush high score
                if (totalScore > sm.Data.bossRushHighScore)
                {
                    sm.Data.bossRushHighScore = totalScore;
                    newRecord = true;
                }

                // Update boss rush best run (most bosses defeated)
                if (CurrentBossNumber > sm.Data.bossRushBestRun)
                {
                    sm.Data.bossRushBestRun = CurrentBossNumber;
                    newRecord = true;
                }

                sm.Data.bossRushGamesPlayed++;
                sm.Save();

                if (newRecord)
                {
                    Debug.Log($"[BossRushMode] New boss rush record! Bosses: {CurrentBossNumber}, Score: {totalScore}");
                }
            }
        }

        // ── Debug ────────────────────────────────────────────────────────────

        private void OnDrawGizmos()
        {
            if (bossSpawnPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(bossSpawnPoint.position, 2f);
            }
        }
    }
}
