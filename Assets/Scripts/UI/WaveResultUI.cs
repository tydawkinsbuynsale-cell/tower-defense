using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

namespace RobotTD.UI
{
    /// <summary>
    /// Handles end-of-wave summary, victory screen, and defeat screen.
    /// Attach to the HUD canvas root alongside GameHUD.
    /// </summary>
    public class WaveResultUI : MonoBehaviour
    {
        // ── Wave Summary (shown after every wave) ────────────────────────────

        [Header("Wave Summary Panel")]
        [SerializeField] private GameObject waveSummaryPanel;
        [SerializeField] private TextMeshProUGUI waveNumberText;
        [SerializeField] private TextMeshProUGUI creditsEarnedText;
        [SerializeField] private TextMeshProUGUI bonusText;
        [SerializeField] private TextMeshProUGUI livesRemainingText;
        [SerializeField] private Button continueButton;
        [SerializeField] private float autoHideDelay = 4f;

        // ── Victory Screen ───────────────────────────────────────────────────

        [Header("Victory Panel")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TextMeshProUGUI victoryScoreText;
        [SerializeField] private TextMeshProUGUI victoryWavesText;
        [SerializeField] private TextMeshProUGUI victoryClearTimeText;
        [SerializeField] private TextMeshProUGUI victoryXPText;
        [SerializeField] private GameObject[] victoryStars;   // 3 star objects
        [SerializeField] private Button victoryMainMenuBtn;
        [SerializeField] private Button victoryRestartBtn;
        [SerializeField] private Button victoryReturnToEditorBtn;
        [SerializeField] private ParticleSystem victoryConfetti;

        // ── Defeat Screen ─────────────────────────────────────────────────────

        [Header("Defeat Panel")]
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private TextMeshProUGUI defeatScoreText;
        [SerializeField] private TextMeshProUGUI defeatWaveText;
        [SerializeField] private TextMeshProUGUI defeatTipText;
        [SerializeField] private Button defeatMainMenuBtn;
        [SerializeField] private Button defeatRestartBtn;
        [SerializeField] private Button defeatReturnToEditorBtn;

        [Header("Transition")]
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField] private float fadeTime = 0.35f;

        // ── Tips pool ─────────────────────────────────────────────────────────
        private readonly string[] tips = new string[]
        {
            "Tip: Freeze Turrets slow enemies before heavy hitters finish them.",
            "Tip: Upgrade towers in chokepoints for maximum efficiency.",
            "Tip: The Buff Station boosts damage of nearby towers by 25%.",
            "Tip: Sniper Bots deal critical hits — great against Bosses.",
            "Tip: Tesla Coils chain lightning between grouped enemies.",
            "Tip: Sell unused towers and reinvest in upgraded ones.",
            "Tip: Bosses spawn on every 5th wave — prepare ahead.",
            "Tip: Rocket Launchers shred tightly grouped enemies.",
            "Tip: Healer Bots restore nearby enemies — target them first!",
        };

        private Coroutine autoHideCoroutine;
        private int savedCreditsOnWaveStart;
        private int waveCompletedNumber;

        private void Start()
        {
            waveSummaryPanel?.SetActive(false);
            victoryPanel?.SetActive(false);
            defeatPanel?.SetActive(false);

            // Subscribe
            Core.WaveManager.Instance?.OnWaveCompleted.AddListener(OnWaveCompleted);
            Core.GameManager.Instance?.OnVictory.AddListener(OnVictory);
            Core.GameManager.Instance?.OnGameOver.AddListener(OnDefeat);
            Core.GameManager.Instance?.OnCreditsChanged.AddListener(c => savedCreditsOnWaveStart = c);

            continueButton?.onClick.AddListener(HideWaveSummary);
            victoryMainMenuBtn?.onClick.AddListener(GoToMainMenu);
            victoryRestartBtn?.onClick.AddListener(RestartLevel);
            victoryReturnToEditorBtn?.onClick.AddListener(ReturnToEditor);
            defeatMainMenuBtn?.onClick.AddListener(GoToMainMenu);
            defeatRestartBtn?.onClick.AddListener(RestartLevel);
            defeatReturnToEditorBtn?.onClick.AddListener(ReturnToEditor);

            // Setup test play mode UI
            SetupTestPlayUI();
        }

        private void SetupTestPlayUI()
        {
            bool isTestPlayMode = Core.MapEditorManager.IsTestPlayMode;
            
            // Show/hide return to editor buttons
            if (victoryReturnToEditorBtn != null)
            {
                victoryReturnToEditorBtn.gameObject.SetActive(isTestPlayMode);
            }
            
            if (defeatReturnToEditorBtn != null)
            {
                defeatReturnToEditorBtn.gameObject.SetActive(isTestPlayMode);
            }

            // In test play mode, hide main menu buttons and show editor returns
            if (isTestPlayMode)
            {
                if (victoryMainMenuBtn != null) victoryMainMenuBtn.gameObject.SetActive(false);
                if (defeatMainMenuBtn != null) defeatMainMenuBtn.gameObject.SetActive(false);
            }
        }

        // ── Wave complete ────────────────────────────────────────────────────

        private void OnWaveCompleted(int wave)
        {
            waveCompletedNumber = wave;
            int credits = Core.GameManager.Instance?.Credits ?? 0;
            int lives   = Core.GameManager.Instance?.Lives   ?? 0;
            int bonus   = Core.GameManager.Instance != null
                          ? Mathf.FloorToInt(credits * 0.05f) // 5% interest preview
                          : 0;

            if (waveNumberText     != null) waveNumberText.text     = $"Wave {wave} Complete!";
            if (creditsEarnedText  != null) creditsEarnedText.text  = $"Credits: {credits:N0}";
            if (bonusText          != null) bonusText.text          = $"Wave Bonus: +{100 + wave * 10}";
            if (livesRemainingText != null) livesRemainingText.text = $"Lives: {lives}";

            waveSummaryPanel?.SetActive(true);
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);

            if (autoHideCoroutine != null) StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = StartCoroutine(AutoHideSummary());
        }

        private IEnumerator AutoHideSummary()
        {
            yield return new WaitForSecondsRealtime(autoHideDelay);
            HideWaveSummary();
        }

        private void HideWaveSummary()
        {
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
            waveSummaryPanel?.SetActive(false);
        }

        // ── Victory ──────────────────────────────────────────────────────────

        private void OnVictory()
        {
            HideWaveSummary();
            StartCoroutine(ShowVictoryDelayed());
        }

        private IEnumerator ShowVictoryDelayed()
        {
            yield return new WaitForSecondsRealtime(1.5f);

            var gm = Core.GameManager.Instance;
            int score = gm?.Score ?? 0;
            int lives = gm?.Lives ?? 0;
            int startingLives = gm?.StartingLives ?? 20;

            int stars = lives == startingLives ? 3 : lives >= startingLives / 2 ? 2 : 1;

            if (victoryScoreText  != null) victoryScoreText.text  = $"Score: {score:N0}";
            if (victoryWavesText  != null) victoryWavesText.text  = $"Waves: {waveCompletedNumber}";
            if (victoryXPText     != null) victoryXPText.text     = $"+{100 + stars * 50} XP";

            float elapsed = Time.realtimeSinceStartup; // crude clear time from scene load
            if (victoryClearTimeText != null)
                victoryClearTimeText.text = FormatTime(elapsed);

            if (victoryStars != null)
                for (int i = 0; i < victoryStars.Length; i++)
                    victoryStars[i].SetActive(i < stars);

            victoryPanel?.SetActive(true);
            victoryConfetti?.Play();

            // Animate stars in sequence
            if (victoryStars != null)
                StartCoroutine(AnimateStars(stars));
        }

        private IEnumerator AnimateStars(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForSecondsRealtime(0.4f);
                if (victoryStars != null && i < victoryStars.Length)
                {
                    victoryStars[i].SetActive(true);
                    Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);
                }
            }
        }

        // ── Defeat ────────────────────────────────────────────────────────────

        private void OnDefeat()
        {
            HideWaveSummary();
            StartCoroutine(ShowDefeatDelayed());
        }

        private IEnumerator ShowDefeatDelayed()
        {
            yield return new WaitForSecondsRealtime(1.5f);

            int score = Core.GameManager.Instance?.Score ?? 0;
            int wave  = Core.WaveManager.Instance?.CurrentWave ?? 0;

            if (defeatScoreText != null) defeatScoreText.text = $"Score: {score:N0}";
            if (defeatWaveText  != null) defeatWaveText.text  = $"Reached Wave {wave}";
            if (defeatTipText   != null) defeatTipText.text   = tips[Random.Range(0, tips.Length)];

            yield return StartCoroutine(FadePanel(defeatPanel, true));
        }

        // ── Navigation ───────────────────────────────────────────────────────

        private void GoToMainMenu()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
            Time.timeScale = 1f;
            StartCoroutine(FadeAndLoad("MainMenu"));
        }

        private void RestartLevel()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);
            Time.timeScale = 1f;
            StartCoroutine(FadeAndLoad(SceneManager.GetActiveScene().name));
        }

        private void ReturnToEditor()
        {
            if (!Core.MapEditorManager.IsTestPlayMode)
            {
                Debug.LogWarning("Not in test play mode!");
                return;
            }

            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                var gm = Core.GameManager.Instance;
                Analytics.AnalyticsManager.Instance.TrackEvent("map_editor_return_from_test", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "final_wave", Core.WaveManager.Instance?.CurrentWave ?? 0 },
                    { "final_score", gm?.Score ?? 0 },
                    { "result", gm?.CurrentState == Core.GameManager.GameState.Victory ? "victory" : "defeat" }
                });
            }

            Time.timeScale = 1f;
            Core.MapEditorManager.ReturnToEditor();
        }

        private IEnumerator FadeAndLoad(string scene)
        {
            yield return StartCoroutine(FadePanel(null, true)); // fade screen to black
            SceneManager.LoadScene(scene);
        }

        private IEnumerator FadePanel(GameObject panel, bool fadeIn)
        {
            if (panel != null) panel.SetActive(true);
            if (fadeGroup == null) yield break;

            float from = fadeIn ? 0f : 1f;
            float to   = fadeIn ? 1f : 0f;
            float elapsed = 0f;
            fadeGroup.alpha = from;

            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeTime);
                yield return null;
            }

            fadeGroup.alpha = to;
        }

        private string FormatTime(float seconds)
        {
            int m = Mathf.FloorToInt(seconds / 60);
            int s = Mathf.FloorToInt(seconds % 60);
            return $"{m:00}:{s:00}";
        }
    }
}
