using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Core;

namespace RobotTD.UI
{
    /// <summary>
    /// Endless Mode information panel in the main menu.
    /// Shows endless mode description, high score, and leaderboard access.
    /// </summary>
    public class EndlessModeUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI bestWaveText;
        [SerializeField] private Button viewLeaderboardButton;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject leaderboardPanel;

        [Header("Info Display")]
        [SerializeField] private TextMeshProUGUI scalingInfoText;
        [SerializeField] private TextMeshProUGUI milestonesInfoText;

        private void Start()
        {
            // Setup button listeners
            if (viewLeaderboardButton != null)
            {
                viewLeaderboardButton.onClick.AddListener(ShowEndlessLeaderboard);
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
                titleText.text = "Endless Mode";

            // Set description
            if (descriptionText != null)
            {
                descriptionText.text = "Face infinite waves of increasingly powerful enemies!\n\n" +
                    "<color=#FFD700>How to Play:</color>\n" +
                    "• Complete any map's campaign to unlock Endless Mode\n" +
                    "• Enemies grow stronger with each wave\n" +
                    "• Earn milestone bonuses every 5 waves\n" +
                    "• Compete on the global leaderboard!\n\n" +
                    "<color=#FF6B6B>The challenge never ends... how long can you survive?</color>";
            }

            // Load and display player's best endless stats
            var saveData = SaveManager.Instance?.Data;
            if (saveData != null)
            {
                if (highScoreText != null)
                {
                    long bestScore = saveData.endlessHighScore;
                    highScoreText.text = bestScore > 0 
                        ? $"Best Score: {bestScore:N0}" 
                        : "Best Score: Not Yet Played";
                }

                if (bestWaveText != null)
                {
                    int bestWave = saveData.endlessHighWave;
                    bestWaveText.text = bestWave > 0 
                        ? $"Best Wave: {bestWave}" 
                        : "Best Wave: 0";
                }
            }

            // Set scaling info
            if (scalingInfoText != null)
            {
                scalingInfoText.text = "<b>Enemy Scaling:</b>\n" +
                    "• <color=#FF6B6B>+25% HP</color> per wave\n" +
                    "• <color=#6B9EFF>+5% Speed</color> per wave\n" +
                    "• <color=#FFB86B>+3 Enemies</color> per wave";
            }

            // Set milestones info
            if (milestonesInfoText != null)
            {
                milestonesInfoText.text = "<b>Milestone Rewards:</b>\n" +
                    "Every <color=#FFD700>5 waves</color>, receive:\n" +
                    "• Bonus Credits\n" +
                    "• Achievement Progress\n" +
                    "• Leaderboard Update";
            }
        }

        private void ShowEndlessLeaderboard()
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
                    // Call ShowEndlessLeaderboard if the method exists
                    leaderboardUI.SendMessage("ShowEndlessLeaderboard", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    Debug.LogWarning("[EndlessModeUI] LeaderboardUI not found in scene.");
                }
            }
        }

        private void ClosePanel()
        {
            Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.UICancel);
            gameObject.SetActive(false);
        }
    }
}
