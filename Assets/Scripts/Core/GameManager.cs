using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using RobotTD.Analytics;

namespace RobotTD.Core
{
    /// <summary>
    /// Central game manager that controls game state, economy, and core systems.
    /// Singleton pattern for easy access throughout the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private int startingCredits = 500;
        [SerializeField] private int startingLives = 20;
        [SerializeField] private float gameSpeed = 1f;

        [Header("Economy Settings")]
        [SerializeField] private int waveCompletionBonus = 100;
        [SerializeField] private float interestRate = 0.05f; // 5% interest on saved credits

        // Game State
        public enum GameState { MainMenu, Playing, Paused, Tutorial, Victory, GameOver }
        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        // Economy
        public int Credits { get; private set; }
        public int Lives { get; private set; }
        public int Score { get; private set; }
        public int StartingLives => startingLives;
        public int CurrentWave => WaveManager.Instance?.CurrentWave ?? 0;
        public bool IsPaused => CurrentState == GameState.Paused;

        // Unity inspector events
        public UnityEvent<int> OnCreditsChanged;
        public UnityEvent<int> OnLivesChanged;
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<GameState> OnGameStateChanged;
        public UnityEvent OnGameOver;
        public UnityEvent OnVictory;

        // C# action events (for code-only subscriptions)
        public System.Action OnGamePaused;
        public System.Action OnGameResumed;

        // Speed control
        public float GameSpeed
        {
            get => gameSpeed;
            set
            {
                gameSpeed = Mathf.Clamp(value, 0f, 3f);
                Time.timeScale = CurrentState == GameState.Playing ? gameSpeed : 0f;
            }
        }

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize events
            OnCreditsChanged ??= new UnityEvent<int>();
            OnLivesChanged ??= new UnityEvent<int>();
            OnScoreChanged ??= new UnityEvent<int>();
            OnGameStateChanged ??= new UnityEvent<GameState>();
            OnGameOver ??= new UnityEvent();
            OnVictory ??= new UnityEvent();
        }

        private void Start()
        {
            InitializeGame();
        }

        /// <summary>
        /// Initialize or reset the game to starting state
        /// </summary>
        public void InitializeGame()
        {
            // Apply tech tree bonuses to starting values
            int bonusLives = 0;
            if (Progression.TechTree.Instance != null)
            {
                bonusLives = Progression.TechTree.Instance.BonusStartingLives;
            }

            Credits = startingCredits;
            Lives = startingLives + bonusLives;
            Score = 0;

            // Track game start
            string mapName = Map.MapManager.Instance != null ? Map.MapManager.Instance.CurrentMapId : "unknown";
            bool isTutorial = TutorialManager.Instance != null && !TutorialManager.Instance.IsTutorialComplete;
            AnalyticsManager.Instance?.TrackGameStart(mapName, 1, isTutorial);

            OnCreditsChanged?.Invoke(Credits);
            OnLivesChanged?.Invoke(Lives);
            OnScoreChanged?.Invoke(Score);

            SetGameState(GameState.Playing);

            // Reset achievement session tracking
            Progression.AchievementManager.Instance?.ResetSessionTracking();
        }

        /// <summary>
        /// Change the current game state
        /// </summary>
        public void SetGameState(GameState newState)
        {
            CurrentState = newState;
            
            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = gameSpeed;
                    break;
                case GameState.Tutorial:
                    // Keep time running during tutorial for animations
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                case GameState.MainMenu:
                case GameState.GameOver:
                case GameState.Victory:
                    Time.timeScale = 0f;
                    break;
            }

            OnGameStateChanged?.Invoke(newState);
        }

        #region Economy Methods

        /// <summary>
        /// Add credits (from kills, wave bonuses, etc.)
        /// </summary>
        public void AddCredits(int amount)
        {
            // Apply tech tree bonus
            if (Progression.TechTree.Instance != null)
            {
                float multiplier = Progression.TechTree.Instance.CreditRewardMultiplier;
                amount = Mathf.RoundToInt(amount * multiplier);
            }

            Credits += amount;
            OnCreditsChanged?.Invoke(Credits);

            // Track in save data
            SaveManager.Instance?.AddCreditsEarned(amount);
        }

        /// <summary>
        /// Spend credits (for tower purchases, upgrades)
        /// Returns true if successful, false if insufficient funds
        /// </summary>
        public bool SpendCredits(int amount)
        {
            // Apply tech tree cost reduction
            if (Progression.TechTree.Instance != null)
            {
                float multiplier = Progression.TechTree.Instance.CostMultiplier;
                amount = Mathf.RoundToInt(amount * multiplier);
            }

            if (Credits >= amount)
            {
                Credits -= amount;
                OnCreditsChanged?.Invoke(Credits);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if player can afford an amount
        /// </summary>
        public bool CanAfford(int amount) => Credits >= amount;

        /// <summary>
        /// Apply interest to saved credits (call at end of wave)
        /// </summary>
        public void ApplyInterest()
        {
            int interest = Mathf.FloorToInt(Credits * interestRate);
            AddCredits(interest);
        }

        /// <summary>
        /// Award wave completion bonus
        /// </summary>
        public void AwardWaveBonus()
        {
            AddCredits(waveCompletionBonus);
            ApplyInterest();
        }

        #endregion

        #region Lives & Score

        /// <summary>
        /// Grant additional lives (TechTree bonus, milestone reward, etc.)
        /// </summary>
        public void AddLives(int amount)
        {
            Lives += amount;
            OnLivesChanged?.Invoke(Lives);
        }

        /// <summary>
        /// Called when an enemy reaches the end
        /// </summary>
        public void LoseLife(int amount = 1)
        {
            Lives = Mathf.Max(0, Lives - amount);
            OnLivesChanged?.Invoke(Lives);

            // Notify achievement manager
            Progression.AchievementManager.Instance?.OnLifeLost();

            if (Lives <= 0)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// Add to score
        /// </summary>
        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score);
        }

        #endregion

        #region Game Flow

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
                OnGamePaused?.Invoke();
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
                OnGameResumed?.Invoke();
            }
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
                PauseGame();
            else if (CurrentState == GameState.Paused)
                ResumeGame();
        }

        public void ToggleSpeed()
        {
            // Cycle through speeds: 1x -> 2x -> 3x -> 1x
            if (GameSpeed < 1.5f)
                GameSpeed = 2f;
            else if (GameSpeed < 2.5f)
                GameSpeed = 3f;
            else
                GameSpeed = 1f;
        }

        private void TriggerGameOver()
        {
            SetGameState(GameState.GameOver);
            OnGameOver?.Invoke();
            
            // Post endless score if active
            EndlessMode.Instance?.PostEndlessScore();
            
            // Record game result in save system
            if (SaveManager.Instance != null && Map.MapManager.Instance != null)
            {
                string mapId = Map.MapManager.Instance.CurrentMapId;
                int wave = WaveManager.Instance?.CurrentWave ?? 0;
                float gameTime = Time.time - (SaveManager.Instance.Data.sessionStartTime);
                
                // Calculate stars (0 for defeat)
                int stars = 0;
                
                SaveManager.Instance.RecordMapResult(mapId, Score, wave, stars, gameTime, victory: false);
                
                // Track game end (defeat)
                AnalyticsManager.Instance?.TrackGameEnd("defeat", wave, Score, 0, gameTime);
            }
        }

        public void TriggerVictory()
        {
            SetGameState(GameState.Victory);
            OnVictory?.Invoke();
            
            // Post endless score if active
            EndlessMode.Instance?.PostEndlessScore();
            
            // Record game result in save system
            if (SaveManager.Instance != null && Map.MapManager.Instance != null)
            {
                string mapId = Map.MapManager.Instance.CurrentMapId;
                int wave = WaveManager.Instance?.CurrentWave ?? 0;
                float gameTime = Time.time - (SaveManager.Instance.Data.sessionStartTime);
                
                // Calculate stars based on lives remaining
                int stars = CalculateStars();
                
                SaveManager.Instance.RecordMapResult(mapId, Score, wave, stars, gameTime, victory: true);

                // Unlock next map if available
                string nextMapId = Map.MapManager.Instance.NextMapId;
                if (!string.IsNullOrEmpty(nextMapId))
                {
                    SaveManager.Instance.UnlockMap(nextMapId);
                }

                // Track game end (victory)
                int creditsEarned = SaveManager.Instance.Data.sessionCreditsEarned;
                AnalyticsManager.Instance?.TrackGameEnd("victory", wave, Score, creditsEarned, gameTime);

                // Trigger achievement checks
                Progression.AchievementManager.Instance?.CheckVictory(gameTime, stars, Lives, StartingLives);
            }
        }

        private int CalculateStars()
        {
            // Star rating based on lives remaining
            float livesPercent = (float)Lives / StartingLives;
            if (livesPercent >= 0.8f) return 3; // 80%+ lives = 3 stars
            if (livesPercent >= 0.5f) return 2; // 50%+ lives = 2 stars
            if (livesPercent > 0f) return 1;   // Any lives remaining = 1 star
            return 0;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        #endregion
    }
}
