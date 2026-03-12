using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobotTD.UI
{
    /// <summary>
    /// Displays challenge completion results.
    /// </summary>
    public class ChallengeResultUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI challengeNameText;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI multiplierText;
        [SerializeField] private TextMeshProUGUI rewardsText;
        [SerializeField] private GameObject firstCompletionBanner;
        
        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Button nextChallengeButton;
        
        [Header("Colors")]
        [SerializeField] private Color victoryColor = Color.green;
        [SerializeField] private Color defeatColor = Color.red;
        
        private Core.ChallengeData currentChallenge;
        private bool victory;
        
        // ── Unity ────────────────────────────────────────────────────────────
        
        private void Awake()
        {
            if (panel != null)
                panel.SetActive(false);
        }
        
        private void OnEnable()
        {
            Core.ChallengeManager.OnChallengeCompleted += HandleChallengeCompleted;
        }
        
        private void OnDisable()
        {
            Core.ChallengeManager.OnChallengeCompleted -= HandleChallengeCompleted;
        }
        
        private void Start()
        {
            // Setup button listeners
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetryClicked);
            
            if (menuButton != null)
                menuButton.onClick.AddListener(OnMenuClicked);
            
            if (nextChallengeButton != null)
                nextChallengeButton.onClick.AddListener(OnNextChallengeClicked);
        }
        
        // ── Display ──────────────────────────────────────────────────────────
        
        private void HandleChallengeCompleted(Core.ChallengeData challenge, bool won)
        {
            currentChallenge = challenge;
            victory = won;
            
            Show();
        }
        
        public void Show()
        {
            if (panel != null)
                panel.SetActive(true);
            
            // Title and result
            if (titleText != null)
            {
                titleText.text = victory ? "CHALLENGE COMPLETED!" : "CHALLENGE FAILED";
                titleText.color = victory ? victoryColor : defeatColor;
            }
            
            if (challengeNameText != null)
                challengeNameText.text = currentChallenge.ChallengeName;
            
            if (resultText != null)
            {
                resultText.text = victory ? "Victory!" : "Defeat";
                resultText.color = victory ? victoryColor : defeatColor;
            }
            
            // Stats
            long score = Core.GameManager.Instance?.Score ?? 0;
            int wave = Core.WaveManager.Instance?.CurrentWave ?? 0;
            float multiplier = currentChallenge.GetDifficultyMultiplier();
            long finalScore = Mathf.RoundToInt(score * multiplier);
            
            if (scoreText != null)
                scoreText.text = $"Score: {finalScore:N0}";
            
            if (waveText != null)
                waveText.text = $"Wave Reached: {wave}";
            
            if (multiplierText != null)
                multiplierText.text = $"Difficulty Multiplier: {multiplier:F1}x";
            
            // Rewards (only on first completion)
            var progress = Core.ChallengeManager.Instance?.GetProgress(currentChallenge.ChallengeId);
            bool firstCompletion = victory && progress != null && !progress.completed;
            
            if (firstCompletionBanner != null)
                firstCompletionBanner.SetActive(firstCompletion);
            
            if (rewardsText != null)
            {
                if (firstCompletion)
                {
                    rewardsText.text = $"Rewards:\n" +
                                     $"<sprite=0> {currentChallenge.CreditReward} Credits\n" +
                                     $"<sprite=1> {currentChallenge.TechPointReward} Tech Points";
                }
                else if (victory)
                {
                    rewardsText.text = "Challenge Already Completed";
                }
                else
                {
                    rewardsText.text = "No Rewards";
                }
            }
            
            // Play audio
            if (victory)
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.Victory);
            else
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.GameOver);
            
            // Pause game
            Time.timeScale = 0f;
        }
        
        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);
            
            Time.timeScale = 1f;
        }
        
        // ── Button Handlers ──────────────────────────────────────────────────
        
        private void OnRetryClicked()
        {
            Hide();
            
            // Restart the same challenge
            Core.ChallengeManager.Instance?.StartChallenge(currentChallenge);
            
            // Reload scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
            
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.ButtonClick);
        }
        
        private void OnMenuClicked()
        {
            Hide();
            
            // Return to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.ButtonClick);
        }
        
        private void OnNextChallengeClicked()
        {
            Hide();
            
            // Go to challenge selector
            UnityEngine.SceneManagement.SceneManager.LoadScene("ChallengeMenu");
            
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.ButtonClick);
        }
    }
}
