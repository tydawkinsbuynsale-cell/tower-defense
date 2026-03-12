using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace RobotTD.UI
{
    /// <summary>
    /// In-game pause menu overlay.
    /// Triggered by the pause button in GameHUD or the Android back button.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsSubPanel;  // embedded settings

        [Header("Pause Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Settings Sub-Panel")]
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Button lowQualBtn;
        [SerializeField] private Button medQualBtn;
        [SerializeField] private Button highQualBtn;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private TextMeshProUGUI creditsText;

        private bool isPaused;

        private void Start()
        {
            pausePanel?.SetActive(false);
            settingsSubPanel?.SetActive(false);

            resumeButton?.onClick.AddListener(Resume);
            restartButton?.onClick.AddListener(Restart);
            settingsButton?.onClick.AddListener(OpenSettings);
            mainMenuButton?.onClick.AddListener(GoToMainMenu);
            settingsBackButton?.onClick.AddListener(CloseSettings);

            // Wire inline quality buttons
            lowQualBtn?.onClick.AddListener(() => SetQualityPreset(Core.PerformanceManager.QualityPreset.Low));
            medQualBtn?.onClick.AddListener(() => SetQualityPreset(Core.PerformanceManager.QualityPreset.Medium));
            highQualBtn?.onClick.AddListener(() => SetQualityPreset(Core.PerformanceManager.QualityPreset.High));

            // Wire volume sliders immediately (values loaded from save)
            SyncSlidersFromSave();
            masterSlider?.onValueChanged.AddListener(v =>
            {
                Audio.AudioManager.Instance.MasterVolume = v;
                if (Core.SaveManager.Instance != null) Core.SaveManager.Instance.Data.masterVolume = v;
            });
            sfxSlider?.onValueChanged.AddListener(v =>
            {
                Audio.AudioManager.Instance.SFXVolume = v;
                if (Core.SaveManager.Instance != null) Core.SaveManager.Instance.Data.sfxVolume = v;
            });
            musicSlider?.onValueChanged.AddListener(v =>
            {
                Audio.AudioManager.Instance.MusicVolume = v;
                if (Core.SaveManager.Instance != null) Core.SaveManager.Instance.Data.musicVolume = v;
            });
        }

        private void Update()
        {
            // Android back button
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingsSubPanel != null && settingsSubPanel.activeSelf)
                    CloseSettings();
                else
                    TogglePause();
            }
        }

        // ── Pause / Resume ───────────────────────────────────────────────────

        public void TogglePause()
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }

        private void Pause()
        {
            if (isPaused) return;
            isPaused = true;

            Core.GameManager.Instance?.PauseGame();
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);

            // Refresh stats display
            RefreshStats();
            pausePanel?.SetActive(true);
        }

        private void Resume()
        {
            isPaused = false;
            Core.GameManager.Instance?.ResumeGame();
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
            pausePanel?.SetActive(false);
            settingsSubPanel?.SetActive(false);
        }

        // ── Settings sub-panel ───────────────────────────────────────────────

        private void OpenSettings()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
            SyncSlidersFromSave();
            settingsSubPanel?.SetActive(true);
        }

        private void CloseSettings()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
            Core.SaveManager.Instance?.Save();
            settingsSubPanel?.SetActive(false);
        }

        private void SyncSlidersFromSave()
        {
            var data = Core.SaveManager.Instance?.Data;
            if (data == null) return;
            if (masterSlider != null) masterSlider.value = data.masterVolume;
            if (sfxSlider    != null) sfxSlider.value    = data.sfxVolume;
            if (musicSlider  != null) musicSlider.value  = data.musicVolume;
        }

        private void SetQualityPreset(Core.PerformanceManager.QualityPreset preset)
        {
            if (Core.PerformanceManager.Instance != null)
            {
                Core.PerformanceManager.Instance.ApplyQualityPreset(preset);
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);
            }
        }

        // ── Navigation ───────────────────────────────────────────────────────

        private void Restart()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void GoToMainMenu()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
            Core.SaveManager.Instance?.Save();
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        // ── Stats refresh ────────────────────────────────────────────────────

        private void RefreshStats()
        {
            var gm = Core.GameManager.Instance;
            var wm = Core.WaveManager.Instance;
            if (gm == null) return;

            if (scoreText   != null) scoreText.text   = $"Score\n{gm.Score:N0}";
            if (waveText    != null) waveText.text     = $"Wave\n{gm.CurrentWave}";
            if (livesText   != null) livesText.text    = $"Lives\n{gm.Lives}";
            if (creditsText != null) creditsText.text  = $"Credits\n{gm.Credits:N0}";
        }
    }
}
