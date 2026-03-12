using UnityEngine;

namespace RobotTD.Core
{
    /// <summary>
    /// Central integration hub. Subscribes to events from all managers and
    /// routes them to Audio, VFX, Save, Achievement, and UI systems.
    /// Attach to the same persistent root GameObject as GameManager.
    /// </summary>
    public class GameIntegrator : MonoBehaviour
    {
        public static GameIntegrator Instance { get; private set; }

        private float sessionStartTime;
        private int wavesCompletedThisGame;
        private bool lifeLostThisWave;
        private float gameClearTime;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            sessionStartTime = Time.time;
            SubscribeEvents();
        }

        private void OnDestroy() => UnsubscribeEvents();

        // ── Event wiring ─────────────────────────────────────────────────────

        private void SubscribeEvents()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            gm.OnCreditsChanged.AddListener(HandleCreditsChanged);
            gm.OnLivesChanged.AddListener(HandleLivesChanged);
            gm.OnGameOver.AddListener(HandleGameOver);
            gm.OnVictory.AddListener(HandleVictory);
            gm.OnGamePaused  += HandlePaused;
            gm.OnGameResumed += HandleResumed;

            var wm = WaveManager.Instance;
            if (wm != null)
            {
                wm.OnWaveStarted.AddListener(HandleWaveStarted);
                wm.OnWaveCompleted.AddListener(HandleWaveCompleted);
                wm.OnEnemyKilled   += HandleEnemyKilled;
                wm.OnBossSpawned   += HandleBossSpawned;
            }
        }

        private void UnsubscribeEvents()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            gm.OnCreditsChanged.RemoveListener(HandleCreditsChanged);
            gm.OnLivesChanged.RemoveListener(HandleLivesChanged);
            gm.OnGameOver.RemoveListener(HandleGameOver);
            gm.OnVictory.RemoveListener(HandleVictory);
            gm.OnGamePaused  -= HandlePaused;
            gm.OnGameResumed -= HandleResumed;

            var wm = WaveManager.Instance;
            if (wm != null)
            {
                wm.OnWaveStarted.RemoveListener(HandleWaveStarted);
                wm.OnWaveCompleted.RemoveListener(HandleWaveCompleted);
                wm.OnEnemyKilled   -= HandleEnemyKilled;
                wm.OnBossSpawned   -= HandleBossSpawned;
            }
        }

        // ── Handlers ─────────────────────────────────────────────────────────

        private void HandleCreditsChanged(int newAmount)
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.CreditGain, 0.5f);
            VFX.VFXManager.Instance?.Play(VFX.VFXType.CreditPickup,
                Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }

        private void HandleLivesChanged(int newLives)
        {
            lifeLostThisWave = true;
            Progression.AchievementManager.Instance?.OnLifeLost();
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.EnemyReachEnd);
            Map.CameraController.Instance?.Shake(0.3f, 0.4f);
        }

        private void HandleWaveStarted(int waveNumber)
        {
            lifeLostThisWave = false;
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.WaveStart);

            // Scale music intensity relative to total waves
            int totalWaves = WaveManager.Instance?.TotalWaves ?? 30;
            float progress = (float)waveNumber / totalWaves;
            Audio.AudioManager.Instance?.SetMusicIntensity(progress);

            VFX.VFXManager.Instance?.Play(VFX.VFXType.WaveStartPulse, Vector3.zero);
        }

        private void HandleWaveCompleted(int waveNumber)
        {
            wavesCompletedThisGame++;

            bool noLifeLost = !lifeLostThisWave;
            Progression.AchievementManager.Instance?.CheckWaveComplete(waveNumber, noLifeLost);

            // Award XP for wave completion
            int xp = 25 + waveNumber * 5;
            SaveManager.Instance?.AddXP(xp);
        }

        private void HandleEnemyKilled(Enemies.Enemy enemy)
        {
            if (enemy == null) return;

            // VFX
            bool isBoss = enemy.GetComponent<Enemies.BossEnemy>() != null;
            enemy.GetComponent<VFX.EnemyVFX>()?.PlayDeathEffect(isBoss);

            // SFX
            Audio.AudioManager.Instance?.PlaySFXAt(Audio.SFX.EnemyDie, enemy.transform.position);

            if (isBoss)
            {
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.EnemyBossRoar);
                Map.CameraController.Instance?.Shake(0.5f, 0.6f);
                Progression.AchievementManager.Instance?.OnBossKilled();
            }

            // Save kill count
            SaveManager.Instance?.AddKills(1);
        }

        private void HandleBossSpawned()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.EnemyBossRoar);
            Audio.AudioManager.Instance?.PlayMusic(Audio.MusicTrack.Battle_High, crossfade: true);
        }

        private void HandleGameOver()
        {
            float playTime = Time.time - sessionStartTime;
            SaveManager.Instance.Data.totalPlayTimeSeconds += playTime;

            Audio.AudioManager.Instance?.StopMusic(fade: true);
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.DefeatSound);

            int score = GameManager.Instance?.Score ?? 0;
            string mapId = Map.MapManager.Instance?.CurrentMapId ?? "unknown";
            SaveManager.Instance?.RecordMapResult(mapId, score, wavesCompletedThisGame, 0, float.MaxValue);
            SaveManager.Instance?.Save();
        }

        private void HandleVictory()
        {
            gameClearTime = Time.time - sessionStartTime;
            SaveManager.Instance.Data.totalPlayTimeSeconds += gameClearTime;

            Audio.AudioManager.Instance?.StopMusic(fade: true);
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.VictoryFanfare);
            Audio.AudioManager.Instance?.PlayMusic(Audio.MusicTrack.Victory, crossfade: false);

            VFX.VFXManager.Instance?.Play(VFX.VFXType.VictoryBurst, Vector3.zero);

            // Calculate stars (3 = no lives lost, 2 = few lives lost, 1 = survived)
            int livesRemaining = GameManager.Instance?.Lives ?? 0;
            int startingLives  = GameManager.Instance?.StartingLives ?? 20;
            int stars = livesRemaining == startingLives ? 3
                       : livesRemaining >= startingLives / 2 ? 2 : 1;

            int score  = GameManager.Instance?.Score ?? 0;
            string mapId = Map.MapManager.Instance?.CurrentMapId ?? "unknown";

            SaveManager.Instance?.RecordMapResult(mapId, score, wavesCompletedThisGame, stars, gameClearTime);
            Progression.AchievementManager.Instance?.CheckVictory(gameClearTime);

            // Unlock next map
            string nextMapId = Map.MapManager.Instance?.NextMapId;
            if (!string.IsNullOrEmpty(nextMapId))
                SaveManager.Instance?.UnlockMap(nextMapId);

            // Award bonus XP
            int bonusXP = 100 + stars * 50;
            SaveManager.Instance?.AddXP(bonusXP);
            SaveManager.Instance?.Save();
        }

        private void HandlePaused()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
        }

        private void HandleResumed()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
        }
    }
}
