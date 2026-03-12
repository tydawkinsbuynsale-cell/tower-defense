using UnityEngine;
using System;
using System.Collections;
using RobotTD.Online;

namespace RobotTD.Core
{
    /// <summary>
    /// Endless Mode — activates after the last designed wave completes.
    /// Generates infinitely escalating waves using the existing WaveManager
    /// infrastructure, tracks a separate Endless Score, and awards periodic
    /// milestones.
    /// </summary>
    public class EndlessMode : MonoBehaviour
    {
        public static EndlessMode Instance { get; private set; }

        // ── Events ───────────────────────────────────────────────────────────

        public static event Action<int> OnEndlessWaveStarted;   // endless wave number (1-based)
        public static event Action<int, long> OnMilestoneReached; // milestone wave, bonus credits

        // ── Inspector ────────────────────────────────────────────────────────

        [Header("Scaling")]
        [SerializeField] private float healthScalePerWave = 0.25f;  // +25 % HP each wave
        [SerializeField] private float speedScalePerWave = 0.05f;   // +5 % speed each wave
        [SerializeField] private float spawnRateScalePerWave = 0.08f;
        [SerializeField] private int baseEnemiesPerWave = 10;
        [SerializeField] private int enemiesIncreasePerWave = 3;

        [Header("Milestones")]
        [SerializeField] private int milestoneInterval = 5;         // bonus every N endless waves
        [SerializeField] private int milestoneBaseCreditBonus = 200;

        [Header("Timings")]
        [SerializeField] private float secondsBetweenWaves = 8f;

        // ── State ────────────────────────────────────────────────────────────

        public bool IsActive { get; private set; }
        public int EndlessWaveNumber { get; private set; }
        public long EndlessScore { get; private set; }

        private Coroutine endlessRoutine;

        // ── Unity ────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            WaveManager.Instance_OnAllWavesCompleted += HandleNormalModeDone;
        }

        private void OnDisable()
        {
            WaveManager.Instance_OnAllWavesCompleted -= HandleNormalModeDone;
        }

        // ── Activation ───────────────────────────────────────────────────────

        /// <summary>Called by WaveManager when designed waves are exhausted.</summary>
        private void HandleNormalModeDone()
        {
            // Only trigger if the player has unlocked endless mode
            // (always available — but could gate behind TechTree node later)
            StartEndless();
        }

        public void StartEndless()
        {
            if (IsActive) return;
            IsActive = true;
            EndlessWaveNumber = 0;
            EndlessScore = 0;

            WaveManager.Instance?.SetEndlessMode(true);

            endlessRoutine = StartCoroutine(EndlessLoop());
            Debug.Log("[EndlessMode] Endless mode started!");
        }

        public void StopEndless()
        {
            IsActive = false;
            if (endlessRoutine != null) StopCoroutine(endlessRoutine);
            WaveManager.Instance?.SetEndlessMode(false);
        }

        // ── Main Loop ────────────────────────────────────────────────────────

        private IEnumerator EndlessLoop()
        {
            while (IsActive)
            {
                yield return new WaitForSeconds(secondsBetweenWaves);

                if (!IsActive) yield break;
                if (GameManager.Instance == null || GameManager.Instance.IsGameOver) yield break;

                EndlessWaveNumber++;
                SpawnEndlessWave(EndlessWaveNumber);
                OnEndlessWaveStarted?.Invoke(EndlessWaveNumber);

                // Wait for wave to clear before giving reward & starting next
                yield return new WaitUntil(() =>
                    WaveManager.Instance == null || !WaveManager.Instance.IsWaveActive);

                if (IsActive)
                    RewardWaveClear(EndlessWaveNumber);

                // Check milestone
                if (EndlessWaveNumber % milestoneInterval == 0)
                    TriggerMilestone(EndlessWaveNumber);
            }
        }

        // ── Wave Spawning ────────────────────────────────────────────────────

        private void SpawnEndlessWave(int waveNum)
        {
            int totalWavesDesigned = WaveManager.Instance != null ? WaveManager.Instance.TotalWaves : 10;
            int effectiveWave = totalWavesDesigned + waveNum;

            // Scale enemy stats via WaveManager's existing multiplier API
            float hpMult = 1f + (waveNum * healthScalePerWave);
            float spdMult = 1f + (waveNum * speedScalePerWave);
            int count = baseEnemiesPerWave + (waveNum - 1) * enemiesIncreasePerWave;

            WaveManager.Instance?.SpawnEndlessWave(effectiveWave, count, hpMult, spdMult);

            Debug.Log($"[EndlessMode] Wave {waveNum} | Enemies: {count} | HP ×{hpMult:F2} | Spd ×{spdMult:F2}");
        }

        // ── Rewards ──────────────────────────────────────────────────────────

        private void RewardWaveClear(int waveNum)
        {
            int reward = 50 + (waveNum * 15);
            GameManager.Instance?.AddCredits(reward);
            EndlessScore += reward;
            Debug.Log($"[EndlessMode] Wave {waveNum} cleared — rewarded {reward} credits.");
        }

        private void TriggerMilestone(int waveNum)
        {
            int bonus = milestoneBaseCreditBonus * (waveNum / milestoneInterval);
            GameManager.Instance?.AddCredits(bonus);
            EndlessScore += bonus;

            OnMilestoneReached?.Invoke(waveNum, bonus);

            // Achievement hook
            Progression.AchievementManager.Instance?.CheckEndlessMilestone(waveNum);

            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.WaveStart);

            Debug.Log($"[EndlessMode] MILESTONE wave {waveNum} — bonus {bonus} credits!");
        }

        // ── Leaderboard Hook ─────────────────────────────────────────────────

        /// <summary>
        /// Call this to post the endless score (e.g. on game over).
        /// Submits to LeaderboardManager with wave and score data.
        /// </summary>
        public void PostEndlessScore()
        {
            if (!IsActive || EndlessScore <= 0) return;

            // Combined score: GameManager base score + endless bonus
            long totalScore = (GameManager.Instance?.Score ?? 0) + EndlessScore;

            // Submit to leaderboard
            LeaderboardManager.Instance?.SubmitEndlessScore(EndlessWaveNumber, totalScore);

            Debug.Log($"[EndlessMode] Final endless score: {totalScore} (endless portion: {EndlessScore}, wave: {EndlessWaveNumber})");

            // Save personal best for endless mode
            var sm = SaveManager.Instance;
            if (sm != null)
            {
                bool newRecord = false;

                // Update endless high score
                if (totalScore > sm.Data.endlessHighScore)
                {
                    sm.Data.endlessHighScore = totalScore;
                    newRecord = true;
                }

                // Update endless high wave
                if (EndlessWaveNumber > sm.Data.endlessHighWave)
                {
                    sm.Data.endlessHighWave = EndlessWaveNumber;
                    newRecord = true;
                }

                // Update overall high score if applicable
                if (totalScore > sm.Data.highScore)
                {
                    sm.Data.highScore = totalScore;
                }

                sm.Data.endlessGamesPlayed++;
                sm.Save();

                if (newRecord)
                {
                    Debug.Log($"[EndlessMode] New endless record! Wave: {EndlessWaveNumber}, Score: {totalScore}");
                }
            }
        }
    }
}
