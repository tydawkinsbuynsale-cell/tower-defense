using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Core;

namespace RobotTD.UI
{
    /// <summary>
    /// Main game HUD controller.
    /// Manages the top info bar and bottom action bar.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        [Header("Top Bar - Resources")]
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Wave Progress")]
        [SerializeField] private Slider waveProgressSlider;
        [SerializeField] private TextMeshProUGUI enemyCountText;

        [Header("Speed Control")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button speedButton;
        [SerializeField] private Image speedButtonIcon;
        [SerializeField] private Sprite playIcon;
        [SerializeField] private Sprite pauseIcon;
        [SerializeField] private Sprite speed1xIcon;
        [SerializeField] private Sprite speed2xIcon;
        [SerializeField] private Sprite speed3xIcon;

        [Header("Bottom Bar - Tower Buttons")]
        [SerializeField] private Transform towerButtonContainer;
        [SerializeField] private TowerButton towerButtonPrefab;
        [SerializeField] private Button startWaveButton;
        [SerializeField] private TextMeshProUGUI startWaveText;

        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TowerInfoUI towerInfoPanel;

        [Header("Tower Data")]
        [SerializeField] private Towers.TowerData[] availableTowers;

        // Animation
        private Animator animator;
        private bool isPaused = false;

        private void Awake()
        {
            Instance = this;
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            // Subscribe to game events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnCreditsChanged.AddListener(UpdateCredits);
                GameManager.Instance.OnLivesChanged.AddListener(UpdateLives);
                GameManager.Instance.OnScoreChanged.AddListener(UpdateScore);
                GameManager.Instance.OnGameStateChanged.AddListener(OnGameStateChanged);
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted.AddListener(OnWaveStarted);
                WaveManager.Instance.OnWaveCompleted.AddListener(OnWaveCompleted);
                WaveManager.Instance.OnEnemyCountChanged.AddListener(UpdateEnemyCount);
            }

            // Setup tower buttons
            CreateTowerButtons();

            // Initial UI state
            RefreshUI();
            HideAllPanels();
        }

        private void Update()
        {
            // Update wave progress
            if (WaveManager.Instance != null && WaveManager.Instance.WaveInProgress)
            {
                waveProgressSlider.value = WaveManager.Instance.GetWaveProgress();
            }
        }

        #region Resource Updates

        private void UpdateCredits(int amount)
        {
            creditsText.text = $"${amount:N0}";
            
            // Refresh tower buttons to show affordability
            RefreshTowerButtons();
        }

        private void UpdateLives(int amount)
        {
            livesText.text = $"❤ {amount}";
            
            // Flash red when losing lives
            if (amount < 10)
            {
                livesText.color = Color.red;
            }
        }

        private void UpdateScore(int amount)
        {
            scoreText.text = $"Score: {amount:N0}";
        }

        private void UpdateEnemyCount(int count)
        {
            enemyCountText.text = $"Enemies: {count}";
        }

        private void RefreshUI()
        {
            if (GameManager.Instance != null)
            {
                UpdateCredits(GameManager.Instance.Credits);
                UpdateLives(GameManager.Instance.Lives);
                UpdateScore(GameManager.Instance.Score);
                waveText.text = $"Wave {GameManager.Instance.CurrentWave}";
            }
        }

        #endregion

        #region Tower Buttons

        private void CreateTowerButtons()
        {
            // Clear existing buttons
            foreach (Transform child in towerButtonContainer)
            {
                Destroy(child.gameObject);
            }

            // Create button for each tower type
            foreach (var towerData in availableTowers)
            {
                TowerButton button = Instantiate(towerButtonPrefab, towerButtonContainer);
                button.Setup(towerData);
            }
        }

        public void RefreshTowerButtons()
        {
            foreach (Transform child in towerButtonContainer)
            {
                TowerButton button = child.GetComponent<TowerButton>();
                if (button != null)
                {
                    button.RefreshAffordability();
                }
            }
        }

        #endregion

        #region Wave Control

        private void OnWaveStarted(int wave)
        {
            waveText.text = $"Wave {wave}";
            startWaveButton.interactable = false;
            startWaveText.text = "In Progress...";
            waveProgressSlider.value = 0;
        }

        private void OnWaveCompleted(int wave)
        {
            startWaveButton.interactable = true;
            startWaveText.text = "Start Wave";
            waveProgressSlider.value = 1;
            
            // Show wave complete notification
            ShowNotification($"Wave {wave} Complete! +$100");
        }

        public void OnStartWaveClicked()
        {
            WaveManager.Instance?.StartNextWave();
        }

        #endregion

        #region Speed Control

        public void OnPauseClicked()
        {
            if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                GameManager.Instance.PauseGame();
                ShowPausePanel();
            }
            else if (GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                GameManager.Instance.ResumeGame();
                HidePausePanel();
            }
        }

        public void OnSpeedClicked()
        {
            GameManager.Instance?.ToggleSpeed();
            UpdateSpeedIcon();
        }

        private void UpdateSpeedIcon()
        {
            if (GameManager.Instance == null) return;

            float speed = GameManager.Instance.GameSpeed;
            if (speed <= 1.1f)
                speedButtonIcon.sprite = speed1xIcon;
            else if (speed <= 2.1f)
                speedButtonIcon.sprite = speed2xIcon;
            else
                speedButtonIcon.sprite = speed3xIcon;
        }

        #endregion

        #region Panels

        private void OnGameStateChanged(GameManager.GameState state)
        {
            HideAllPanels();

            switch (state)
            {
                case GameManager.GameState.Paused:
                    ShowPausePanel();
                    break;
                case GameManager.GameState.GameOver:
                    ShowGameOverPanel();
                    break;
                case GameManager.GameState.Victory:
                    ShowVictoryPanel();
                    break;
            }
        }

        private void HideAllPanels()
        {
            pausePanel?.SetActive(false);
            gameOverPanel?.SetActive(false);
            victoryPanel?.SetActive(false);
        }

        private void ShowPausePanel()
        {
            pausePanel?.SetActive(true);
        }

        private void HidePausePanel()
        {
            pausePanel?.SetActive(false);
        }

        private void ShowGameOverPanel()
        {
            gameOverPanel?.SetActive(true);
            // Update final score display
            var scoreDisplay = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (scoreDisplay != null)
            {
                scoreDisplay.text = $"Final Score: {GameManager.Instance.Score:N0}\nWave Reached: {GameManager.Instance.CurrentWave}";
            }
        }

        private void ShowVictoryPanel()
        {
            victoryPanel?.SetActive(true);
        }

        public void ShowTowerInfo(Towers.Tower tower)
        {
            towerInfoPanel?.Show(tower);
        }

        public void HideTowerInfo()
        {
            towerInfoPanel?.Hide();
        }

        #endregion

        #region Notifications

        public void ShowNotification(string message)
        {
            ToastNotification.Instance?.ShowInfo(message);
        }

        public void ShowSuccessNotification(string message)
        {
            ToastNotification.Instance?.ShowSuccess(message);
        }

        public void ShowWarningNotification(string message)
        {
            ToastNotification.Instance?.ShowWarning(message);
        }

        public void ShowErrorNotification(string message)
        {
            ToastNotification.Instance?.ShowError(message);
        }

        #endregion

        #region Button Callbacks

        public void OnResumeClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        public void OnRestartClicked()
        {
            GameManager.Instance?.RestartGame();
        }

        public void OnMainMenuClicked()
        {
            GameManager.Instance?.LoadMainMenu();
        }

        public void OnSettingsClicked()
        {
            // TODO: Show settings panel
        }

        #endregion
    }
}
