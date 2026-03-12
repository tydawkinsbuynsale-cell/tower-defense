using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobotTowerDefense.UI
{
    /// <summary>
    /// UI button for watching rewarded ads to earn bonuses.
    /// Can be placed in game over screen, shop, daily rewards, etc.
    /// </summary>
    public class RewardedAdButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button watchAdButton;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private GameObject cooldownPanel;
        [SerializeField] private Image cooldownFillImage;

        [Header("Reward Configuration")]
        [SerializeField] private RewardType rewardType = RewardType.Gems;
        [SerializeField] private int rewardAmount = 50;

        [Header("Display")]
        [SerializeField] private string rewardTextFormat = "Watch Ad\n+{0} {1}";
        [SerializeField] private bool hideIfAdsRemoved = true;

        private Monetization.AdManager adManager;
        private bool isWatchingAd = false;

        #region Unity Lifecycle
        private void Start()
        {
            adManager = Monetization.AdManager.Instance;

            if (watchAdButton != null)
            {
                watchAdButton.onClick.AddListener(OnWatchAdClicked);
            }

            UpdateUI();
            InvokeRepeating(nameof(UpdateCooldown), 0f, 1f);
        }

        private void OnDestroy()
        {
            if (watchAdButton != null)
            {
                watchAdButton.onClick.RemoveListener(OnWatchAdClicked);
            }

            CancelInvoke();
        }
        #endregion

        #region UI Updates
        private void UpdateUI()
        {
            // Check if ads have been removed via IAP
            if (hideIfAdsRemoved && Monetization.IAPManager.Instance != null && 
                Monetization.IAPManager.Instance.AreAdsRemoved())
            {
                gameObject.SetActive(false);
                return;
            }

            // Update reward text
            if (rewardText != null)
            {
                string rewardName = GetRewardName();
                rewardText.text = string.Format(rewardTextFormat, rewardAmount, rewardName);
            }

            UpdateCooldown();
        }

        private void UpdateCooldown()
        {
            if (adManager == null || isWatchingAd)
                return;

            bool canShow = adManager.CanShowRewarded();
            float cooldownRemaining = adManager.GetRewardedCooldownRemaining();

            // Update button interactability
            if (watchAdButton != null)
            {
                watchAdButton.interactable = canShow && adManager.IsInitialized;
            }

            // Update cooldown display
            if (cooldownPanel != null)
            {
                cooldownPanel.SetActive(!canShow && cooldownRemaining > 0);
            }

            if (cooldownText != null && cooldownRemaining > 0)
            {
                int minutes = Mathf.FloorToInt(cooldownRemaining / 60f);
                int seconds = Mathf.FloorToInt(cooldownRemaining % 60f);

                if (minutes > 0)
                {
                    cooldownText.text = $"{minutes}:{seconds:D2}";
                }
                else
                {
                    cooldownText.text = $"{seconds}s";
                }
            }

            // Update cooldown fill image
            if (cooldownFillImage != null)
            {
                float maxCooldown = 60f; // Default rewarded cooldown from AdManager
                cooldownFillImage.fillAmount = 1f - (cooldownRemaining / maxCooldown);
            }
        }
        #endregion

        #region Button Callback
        private void OnWatchAdClicked()
        {
            if (adManager == null)
            {
                Debug.LogWarning("[RewardedAdButton] AdManager not found");
                return;
            }

            if (!adManager.IsInitialized)
            {
                Debug.LogWarning("[RewardedAdButton] Ads not initialized");
                ShowMessage("Ads not available", false);
                return;
            }

            if (!adManager.CanShowRewarded())
            {
                Debug.Log("[RewardedAdButton] Rewarded ad on cooldown");
                return;
            }

            Debug.Log($"[RewardedAdButton] Showing rewarded ad - Reward: {rewardType} x{rewardAmount}");
            isWatchingAd = true;

            if (watchAdButton != null)
            {
                watchAdButton.interactable = false;
            }

            adManager.ShowRewardedAd(rewardType.ToString(), OnAdComplete);
        }

        private void OnAdComplete(bool success)
        {
            isWatchingAd = false;

            if (success)
            {
                Debug.Log($"[RewardedAdButton] Ad completed - granting reward: {rewardType} x{rewardAmount}");
                GrantReward();
                ShowMessage($"+{rewardAmount} {GetRewardName()}!", true);

                // Track analytics
                Analytics.AnalyticsManager.Instance?.TrackEvent("rewarded_ad_reward_claimed", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "reward_type", rewardType.ToString() },
                    { "reward_amount", rewardAmount }
                });
            }
            else
            {
                Debug.Log("[RewardedAdButton] Ad not completed - no reward");
                ShowMessage("Ad cancelled", false);
            }

            UpdateUI();
        }
        #endregion

        #region Reward Granting
        private void GrantReward()
        {
            switch (rewardType)
            {
                case RewardType.Gems:
                    GrantGems(rewardAmount);
                    break;

                case RewardType.Credits:
                    GrantCredits(rewardAmount);
                    break;

                case RewardType.ContinueGame:
                    // Handle in GameManager or caller
                    Debug.Log("[RewardedAdButton] Continue game reward granted");
                    break;

                case RewardType.DoubleReward:
                    // Handle in GameManager or caller
                    Debug.Log("[RewardedAdButton] Double reward granted");
                    break;

                case RewardType.ExtraLife:
                    // Handle in GameManager or caller
                    Debug.Log("[RewardedAdButton] Extra life granted");
                    break;
            }
        }

        private void GrantGems(int amount)
        {
            var saveManager = Core.SaveManager.Instance;
            if (saveManager != null)
            {
                // Add gems to save data (requires gems field in PlayerSaveData)
                // saveManager.Data.gems += amount;
                // saveManager.Save();

                Debug.Log($"[RewardedAdButton] Granted {amount} gems (SaveManager integration needed)");
            }
            else
            {
                Debug.LogWarning("[RewardedAdButton] SaveManager not found");
            }
        }

        private void GrantCredits(int amount)
        {
            var gameManager = Core.GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.AddCredits(amount);
                Debug.Log($"[RewardedAdButton] Granted {amount} credits");
            }
            else
            {
                Debug.LogWarning("[RewardedAdButton] GameManager not found");
            }
        }
        #endregion

        #region Helpers
        private string GetRewardName()
        {
            switch (rewardType)
            {
                case RewardType.Gems: return "Gems";
                case RewardType.Credits: return "Credits";
                case RewardType.ContinueGame: return "Continue";
                case RewardType.DoubleReward: return "2x Reward";
                case RewardType.ExtraLife: return "Extra Life";
                default: return "Reward";
            }
        }

        private void ShowMessage(string message, bool success)
        {
            // Use ToastNotification if available
            var toast = FindObjectOfType<ToastNotification>();
            if (toast != null)
            {
                toast.Show(message, success ? ToastType.Success : ToastType.Warning);
            }
            else
            {
                Debug.Log($"[RewardedAdButton] {message}");
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Set the reward type and amount dynamically.
        /// </summary>
        public void SetReward(RewardType type, int amount)
        {
            rewardType = type;
            rewardAmount = amount;
            UpdateUI();
        }

        /// <summary>
        /// Check if button can be shown (ads available and not removed).
        /// </summary>
        public bool CanShow()
        {
            if (hideIfAdsRemoved && Monetization.IAPManager.Instance != null &&
                Monetization.IAPManager.Instance.AreAdsRemoved())
            {
                return false;
            }

            return adManager != null && adManager.IsInitialized;
        }
        #endregion

        #region Context Menu
        [ContextMenu("Test Watch Ad")]
        private void TestWatchAd()
        {
            OnWatchAdClicked();
        }
        #endregion
    }

    #region Enums
    public enum RewardType
    {
        Gems,           // Premium currency
        Credits,        // Soft currency
        ContinueGame,   // Revive in game over screen
        DoubleReward,   // 2x rewards after victory
        ExtraLife       // Additional health/lives
    }
    #endregion
}
