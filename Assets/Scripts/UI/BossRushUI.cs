using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Core;

namespace RobotTD.UI
{
    /// <summary>
    /// Boss Rush Mode information panel in the main menu.
    /// Shows boss rush description, personal best, and leaderboard access.
    /// </summary>
    public class BossRushUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI bestRunText;
        [SerializeField] private Button viewLeaderboardButton;
        [SerializeField] private Button startBossRushButton;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject leaderboardPanel;

        [Header("Info Display")]
        [SerializeField] private TextMeshProUGUI bossInfoText;
        [SerializeField] private TextMeshProUGUI rewardsInfoText;

        private void Start()
        {
            // Setup button listeners
            if (viewLeaderboardButton != null)
            {
                viewLeaderboardButton.onClick.AddListener(ShowBossRushLeaderboard);
            }

            if (startBossRushButton != null)
            {
                startBossRushButton.onClick.AddListener(StartBossRush);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(ClosePanel);
            }

            // Initialize display
            RefreshDisplay();
        }

        private void OnEnable()
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            // Set title
            if (titleText != null)
                titleText.text = "Boss Rush Mode";

            // Set description
            if (descriptionText != null)
            {
                descriptionText.text = "Face an endless gauntlet of powerful bosses!\n\n" +
                    "<color=#FF4444>How to Play:</color>\n" +
                    "• Fight one boss at a time in sequential battles\n" +
                    "• Each boss is stronger than the last\n" +
                    "• 30-second prep time between bosses to upgrade\n" +
                    "• Earn bonus credits for each boss defeated\n" +
                    "• Compete on the global leaderboard!\n\n" +
                    "<color=#FFD700>Think you can survive? Test your limits!</color>";
            }

            // Load and display player's best stats
            var saveData = SaveManager.Instance?.Data;
            if (saveData != null)
            {
                if (highScoreText != null)
                {
                    long bestScore = saveData.bossRushHighScore;
                    highScoreText.text = bestScore > 0 
                        ? $"Best Score: {bestScore:N0}" 
                        : "Best Score: Not Yet Played";
                }

                if (bestRunText != null)
                {
                    int bestRun = saveData.bossRushBestRun;
                    bestRunText.text = bestRun > 0 
                        ? $"Best Run: {bestRun} Bosses" 
                        : "Best Run: 0 Bosses";
                }
            }

            // Set boss info
            if (bossInfoText != null)
            {
                bossInfoText.text = "<b>Boss Types:</b>\n" +
                    "• <color=#FF6B6B>Swarm Mother</color> - Spawns drone minions\n" +
                    "• <color=#6B9EFF>Shield Commander</color> - Protects nearby enemies\n" +
                    "• <color=#FFB86B>Tank Destroyer</color> - High damage output\n" +
                    "• <color=#B8FF6B>Repair Master</color> - Regenerates health\n" +
                    "• <color=#FF6BFF>Artillery Juggernaut</color> - Long-range attacks\n\n" +
                    "<b>Scaling:</b> +50% HP, +10% speed per boss";
            }

            // Set rewards info
            if (rewardsInfoText != null)
            {
                rewardsInfoText.text = "<b>Rewards Per Boss:</b>\n" +
                    "Base: <color=#FFD700>500 credits</color>\n" +
                    "Scaling: <color=#FFD700>+100 credits</color> per boss\n\n" +
                    "<b>Example:</b>\n" +
                    "Boss 1: 500 credits\n" +
                    "Boss 5: 900 credits\n" +
                    "Boss 10: 1,400 credits";
            }
        }

        private void ShowBossRushLeaderboard()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UITap);

            // If leaderboard panel exists in the hierarchy, show it
            if (leaderboardPanel != null)
            {
                leaderboardPanel.SetActive(true);
            }
            else
            {
                // Otherwise, try to find and activate LeaderboardUI
                var leaderboardUI = FindObjectOfType<LeaderboardUI>();
                if (leaderboardUI != null)
                {
                    leaderboardUI.gameObject.SetActive(true);
                    // Call ShowBossRushLeaderboard if the method exists
                    leaderboardUI.SendMessage("ShowBossRushLeaderboard", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    Debug.LogWarning("[BossRushUI] LeaderboardUI not found in scene.");
                }
            }
        }

        private void StartBossRush()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UIConfirm);

            // Load the game scene and start boss rush mode
            // This would typically load a specific boss rush map/scene
            if (FindObjectOfType<MainMenuUI>() != null)
            {
                // For now, just show a message
                // In production, this would load the boss rush scene
                ToastNotification.Instance?.Show(
                    "Boss Rush scene loading would start here!",
                    ToastNotification.ToastType.Info);

                Debug.Log("[BossRushUI] Starting Boss Rush Mode...");
                
                // Example: Load boss rush scene
                // FindObjectOfType<MainMenuUI>().LoadScene("BossRushScene");
            }
        }

        private void ClosePanel()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
            gameObject.SetActive(false);
        }
    }
}
