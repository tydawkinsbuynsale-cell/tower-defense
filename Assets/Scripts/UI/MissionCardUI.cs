using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Core;
using System;

namespace RobotTD.UI
{
    /// <summary>
    /// Individual mission card UI component.
    /// Displays mission info, progress, and claim button.
    /// </summary>
    public class MissionCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Button claimButton;
        [SerializeField] private GameObject completedBadge;
        [SerializeField] private GameObject lockedOverlay;
        
        [Header("Difficulty Colors")]
        [SerializeField] private Color easyColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color mediumColor = new Color(0.8f, 0.6f, 0.2f);
        [SerializeField] private Color hardColor = new Color(0.8f, 0.2f, 0.2f);
        
        [Header("Stars (Difficulty Indicator)")]
        [SerializeField] private GameObject[] difficultyStars;
        
        private MissionData missionData;
        private MissionProgress missionProgress;
        
        public string MissionId => missionData?.MissionId;
        public event Action<string> OnRewardClaimed;
        
        private void Start()
        {
            if (claimButton != null)
            {
                claimButton.onClick.AddListener(ClaimReward);
            }
        }
        
        // ── Setup ─────────────────────────────────────────────────────────────
        
        public void Setup(MissionData data, MissionProgress progress)
        {
            missionData = data;
            missionProgress = progress ?? new MissionProgress(data.MissionId);
            
            RefreshDisplay();
        }
        
        private void RefreshDisplay()
        {
            if (missionData == null) return;
            
            // Title and description
            if (titleText != null)
                titleText.text = missionData.MissionName;
            
            if (descriptionText != null)
                descriptionText.text = missionData.GetFormattedDescription();
            
            // Icon
            if (iconImage != null && missionData.Icon != null)
                iconImage.sprite = missionData.Icon;
            
            // Difficulty color
            Color difficultyColor = GetDifficultyColor(missionData.Difficulty);
            if (backgroundImage != null)
                backgroundImage.color = difficultyColor * 0.3f; // Dimmed for background
            
            // Difficulty stars
            UpdateDifficultyStars(missionData.Difficulty);
            
            // Rewards
            if (rewardText != null)
            {
                rewardText.text = $"<sprite name=\"coin\"> {missionData.CreditReward}  " +
                                 $"<sprite name=\"tech\"> {missionData.TechPointReward}";
                
                // Fallback if no sprites
                if (string.IsNullOrEmpty(rewardText.text))
                {
                    rewardText.text = $"${missionData.CreditReward} + {missionData.TechPointReward} TP";
                }
            }
            
            UpdateProgress(missionProgress);
        }
        
        public void UpdateProgress(MissionProgress progress)
        {
            if (progress == null || missionData == null) return;
            
            missionProgress = progress;
            
            // Progress text
            if (progressText != null)
                progressText.text = missionData.GetProgressText(progress.currentProgress);
            
            // Progress bar
            if (progressBar != null)
            {
                float percentage = missionData.GetCompletionPercentage(progress.currentProgress);
                progressBar.value = percentage;
                
                // Color based on progress
                Image fillImage = progressBar.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (progress.completed)
                        fillImage.color = new Color(0.2f, 0.8f, 0.2f); // Green
                    else if (percentage > 0.5f)
                        fillImage.color = new Color(0.8f, 0.6f, 0.2f); // Orange
                    else
                        fillImage.color = new Color(0.3f, 0.6f, 1f); // Blue
                }
            }
            
            // Claim button state
            if (claimButton != null)
            {
                bool canClaim = progress.completed && !progress.rewardClaimed;
                claimButton.interactable = canClaim;
                
                TextMeshProUGUI buttonText = claimButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    if (progress.rewardClaimed)
                        buttonText.text = "Claimed";
                    else if (progress.completed)
                        buttonText.text = "Claim Reward";
                    else
                        buttonText.text = "In Progress";
                }
            }
            
            // Completed badge
            if (completedBadge != null)
                completedBadge.SetActive(progress.completed);
            
            // Locked overlay (for future use)
            if (lockedOverlay != null)
                lockedOverlay.SetActive(false);
        }
        
        // ── Visual Helpers ────────────────────────────────────────────────────
        
        private Color GetDifficultyColor(MissionDifficulty difficulty)
        {
            return difficulty switch
            {
                MissionDifficulty.Easy => easyColor,
                MissionDifficulty.Medium => mediumColor,
                MissionDifficulty.Hard => hardColor,
                _ => Color.white
            };
        }
        
        private void UpdateDifficultyStars(MissionDifficulty difficulty)
        {
            if (difficultyStars == null || difficultyStars.Length == 0)
                return;
            
            int starCount = (int)difficulty;
            
            for (int i = 0; i < difficultyStars.Length; i++)
            {
                if (difficultyStars[i] != null)
                {
                    difficultyStars[i].SetActive(i < starCount);
                }
            }
        }
        
        // ── Animations ────────────────────────────────────────────────────────
        
        public void PlayCompletionAnimation()
        {
            // Scale pulse effect
            LeanTween.cancel(gameObject);
            
            Vector3 originalScale = transform.localScale;
            LeanTween.scale(gameObject, originalScale * 1.1f, 0.2f)
                .setEaseOutQuad()
                .setOnComplete(() =>
                {
                    LeanTween.scale(gameObject, originalScale, 0.2f).setEaseInQuad();
                });
            
            // Flash background
            if (backgroundImage != null)
            {
                Color originalColor = backgroundImage.color;
                Color flashColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
                
                LeanTween.value(gameObject, 0f, 1f, 0.3f)
                    .setOnUpdate((float t) =>
                    {
                        backgroundImage.color = Color.Lerp(flashColor, originalColor, t);
                    });
            }
        }
        
        public void PlayClaimAnimation()
        {
            // Spin and fade out effect
            LeanTween.rotateZ(gameObject, 360f, 0.5f).setEaseInOutQuad();
            
            CanvasGroup group = GetComponent<CanvasGroup>();
            if (group == null)
                group = gameObject.AddComponent<CanvasGroup>();
            
            LeanTween.alphaCanvas(group, 0.5f, 0.3f)
                .setDelay(0.2f)
                .setOnComplete(() =>
                {
                    LeanTween.alphaCanvas(group, 1f, 0.2f);
                });
        }
        
        // ── User Actions ──────────────────────────────────────────────────────
        
        private void ClaimReward()
        {
            if (missionData == null || missionProgress == null)
                return;
            
            if (!missionProgress.completed || missionProgress.rewardClaimed)
                return;
            
            // Claim through manager
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.ClaimReward(missionData.MissionId);
                
                // Update display
                missionProgress.rewardClaimed = true;
                UpdateProgress(missionProgress);
                
                // Play animation
                PlayClaimAnimation();
                
                // Notify parent
                OnRewardClaimed?.Invoke(missionData.MissionId);
            }
        }
        
        // ── Interaction Feedback ──────────────────────────────────────────────
        
        public void OnPointerEnter()
        {
            // Subtle hover effect
            LeanTween.scale(gameObject, Vector3.one * 1.02f, 0.1f).setEaseOutQuad();
        }
        
        public void OnPointerExit()
        {
            LeanTween.scale(gameObject, Vector3.one, 0.1f).setEaseInQuad();
        }
    }
}
