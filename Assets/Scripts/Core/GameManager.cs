using UnityEngine;
using UnityEngine.Events;
using System.Collections;

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
        public enum GameState { MainMenu, Playing, Paused, Victory, GameOver }
        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        // Economy
        public int Credits { get; private set; }
        public int Lives { get; private set; }
        public int Score { get; private set; }
        public int CurrentWave => WaveManager.Instance?.CurrentWave ?? 0;

        // Events for UI updates and other systems
        public UnityEvent<int> OnCreditsChanged;
        public UnityEvent<int> OnLivesChanged;
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<GameState> OnGameStateChanged;
        public UnityEvent OnGameOver;
        public UnityEvent OnVictory;

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
            Credits = startingCredits;
            Lives = startingLives;
            Score = 0;

            OnCreditsChanged?.Invoke(Credits);
            OnLivesChanged?.Invoke(Lives);
            OnScoreChanged?.Invoke(Score);

            SetGameState(GameState.Playing);
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
            Credits += amount;
            OnCreditsChanged?.Invoke(Credits);
        }

        /// <summary>
        /// Spend credits (for tower purchases, upgrades)
        /// Returns true if successful, false if insufficient funds
        /// </summary>
        public bool SpendCredits(int amount)
        {
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
        /// Called when an enemy reaches the end
        /// </summary>
        public void LoseLife(int amount = 1)
        {
            Lives = Mathf.Max(0, Lives - amount);
            OnLivesChanged?.Invoke(Lives);

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
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
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
            
            // Save high score
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (Score > highScore)
            {
                PlayerPrefs.SetInt("HighScore", Score);
                PlayerPrefs.Save();
            }
        }

        public void TriggerVictory()
        {
            SetGameState(GameState.Victory);
            OnVictory?.Invoke();
            
            // Save high score
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (Score > highScore)
            {
                PlayerPrefs.SetInt("HighScore", Score);
                PlayerPrefs.Save();
            }
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
