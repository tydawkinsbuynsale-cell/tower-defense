using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobotTD.UI
{
    /// <summary>
    /// Individual challenge card display component.
    /// </summary>
    public class ChallengeCardUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI rewardsText;
        [SerializeField] private Button playButton;
        [SerializeField] private GameObject completedBadge;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        
        [Header("Modifiers Display")]
        [SerializeField] private Transform modifiersContainer;
        [SerializeField] private GameObject modifierIconPrefab;
        
        [Header("Difficulty Colors")]
        [SerializeField] private Color easyColor = Color.green;
        [SerializeField] private Color mediumColor = Color.yellow;
        [SerializeField] private Color hardColor = new Color(1f, 0.5f, 0f); // Orange
        [SerializeField] private Color extremeColor = Color.red;
        
        private Core.ChallengeData challengeData;
        
        // ── Setup ────────────────────────────────────────────────────────────
        
        public void Setup(Core.ChallengeData challenge)
        {
            challengeData = challenge;
            
            // Basic info
            if (nameText != null)
                nameText.text = challenge.ChallengeName;
            
            if (descriptionText != null)
                descriptionText.text = challenge.Description;
            
            if (iconImage != null && challenge.Icon != null)
                iconImage.sprite = challenge.Icon;
            
            // Difficulty
            if (difficultyText != null)
            {
                difficultyText.text = GetDifficultyString(challenge.Difficulty);
                difficultyText.color = GetDifficultyColor(challenge.Difficulty);
            }
            
            // Rewards
            if (rewardsText != null)
            {
                rewardsText.text = $"<sprite=0> {challenge.CreditReward}  <sprite=1> {challenge.TechPointReward}";
            }
            
            // Progress
            UpdateProgress();
            
            // Modifiers
            DisplayModifiers();
            
            // Play button
            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(OnPlayClicked);
            }
        }
        
        private void UpdateProgress()
        {
            if (Core.ChallengeManager.Instance == null) return;
            
            var progress = Core.ChallengeManager.Instance.GetProgress(challengeData.ChallengeId);
            
            if (progress != null && progress.completed)
            {
                if (completedBadge != null)
                    completedBadge.SetActive(true);
                
                if (bestScoreText != null)
                {
                    bestScoreText.gameObject.SetActive(true);
                    bestScoreText.text = $"Best: {progress.bestScore:N0}\nWave {progress.bestWave}";
                }
            }
            else
            {
                if (completedBadge != null)
                    completedBadge.SetActive(false);
                
                if (bestScoreText != null)
                    bestScoreText.gameObject.SetActive(false);
            }
        }
        
        private void DisplayModifiers()
        {
            if (modifiersContainer == null || modifierIconPrefab == null) return;
            
            // Clear existing
            foreach (Transform child in modifiersContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create icons for each modifier
            foreach (var modifier in challengeData.Modifiers)
            {
                GameObject iconObj = Instantiate(modifierIconPrefab, modifiersContainer);
                TextMeshProUGUI iconText = iconObj.GetComponentInChildren<TextMeshProUGUI>();
                
                if (iconText != null)
                {
                    iconText.text = GetModifierIcon(modifier);
                }
                
                // Optional: Add tooltip component here
            }
        }
        
        // ── Button Handlers──────────────────────────────────────────────────
        
        private void OnPlayClicked()
        {
            if (challengeData == null) return;
            
            // Start the challenge
            bool success = Core.ChallengeManager.Instance?.StartChallenge(challengeData) ?? false;
            
            if (success)
            {
                // Load the appropriate map scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
                
                Audio.AudioManager.Instance?.PlaySFX(Audio.SFX.WaveStart);
            }
            else
            {
                Debug.LogWarning($"[ChallengeCardUI] Failed to start challenge: {challengeData.ChallengeName}");
            }
        }
        
        // ── Helpers ──────────────────────────────────────────────────────────
        
        private string GetDifficultyString(Core.DifficultyTier difficulty)
        {
            return difficulty switch
            {
                Core.DifficultyTier.Easy => "★☆☆☆",
                Core.DifficultyTier.Medium => "★★☆☆",
                Core.DifficultyTier.Hard => "★★★☆",
                Core.DifficultyTier.Extreme => "★★★★",
                _ => "?"
            };
        }
        
        private Color GetDifficultyColor(Core.DifficultyTier difficulty)
        {
            return difficulty switch
            {
                Core.DifficultyTier.Easy => easyColor,
                Core.DifficultyTier.Medium => mediumColor,
                Core.DifficultyTier.Hard => hardColor,
                Core.DifficultyTier.Extreme => extremeColor,
                _ => Color.white
            };
        }
        
        private string GetModifierIcon(Core.ChallengeModifier modifier)
        {
            // Return emoji or text representation
            return modifier switch
            {
                Core.ChallengeModifier.SpeedRush => "⚡",
                Core.ChallengeModifier.ArmoredAssault => "🛡",
                Core.ChallengeModifier.SwarmMode => "🐝",
                Core.ChallengeModifier.BossRush => "👹",
                Core.ChallengeModifier.LimitedArsenal => "🔒",
                Core.ChallengeModifier.BudgetCrisis => "💰",
                Core.ChallengeModifier.TowerLimit => "🚫",
                Core.ChallengeModifier.NoUpgrades => "⛔",
                Core.ChallengeModifier.WeakenedTowers => "⬇",
                Core.ChallengeModifier.EconomicHardship => "📉",
                Core.ChallengeModifier.StartingDebt => "💸",
                Core.ChallengeModifier.FastForward => "⏩",
                Core.ChallengeModifier.NoBreaks => "⏱",
                Core.ChallengeModifier.PerfectDefense => "❤",
                Core.ChallengeModifier.TimeAttack => "⏰",
                _ => "?"
            };
        }
    }
}
