using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobotTowerDefense.UI
{
    /// <summary>
    /// UI component for displaying and activating a single power-up.
    /// Shows icon, count, active status, and remaining time.
    /// </summary>
    public class PowerUpButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button activateButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Configuration")]
        [SerializeField] private Core.PowerUpType powerUpType;
        [SerializeField] private Sprite powerUpIcon;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.gray;

        private Core.PowerUpManager powerUpManager;
        private bool isActive = false;

        #region Unity Lifecycle
        private void Start()
        {
            powerUpManager = Core.PowerUpManager.Instance;

            if (activateButton != null)
            {
                activateButton.onClick.AddListener(OnActivateClicked);
            }

            if (powerUpManager != null)
            {
                powerUpManager.OnInventoryChanged += OnInventoryChanged;
                powerUpManager.OnPowerUpActivated += OnPowerUpActivated;
                powerUpManager.OnPowerUpExpired += OnPowerUpExpired;
                powerUpManager.OnPowerUpTimeUpdated += OnPowerUpTimeUpdated;
            }

            if (iconImage != null && powerUpIcon != null)
            {
                iconImage.sprite = powerUpIcon;
            }

            if (nameText != null)
            {
                nameText.text = powerUpManager != null ? powerUpManager.GetPowerUpName(powerUpType) : powerUpType.ToString();
            }

            UpdateUI();
        }

        private void OnDestroy()
        {
            if (activateButton != null)
            {
                activateButton.onClick.RemoveListener(OnActivateClicked);
            }

            if (powerUpManager != null)
            {
                powerUpManager.OnInventoryChanged -= OnInventoryChanged;
                powerUpManager.OnPowerUpActivated -= OnPowerUpActivated;
                powerUpManager.OnPowerUpExpired -= OnPowerUpExpired;
                powerUpManager.OnPowerUpTimeUpdated -= OnPowerUpTimeUpdated;
            }
        }
        #endregion

        #region UI Updates
        private void UpdateUI()
        {
            if (powerUpManager == null)
                return;

            // Update count
            int count = powerUpManager.GetPowerUpCount(powerUpType);
            if (countText != null)
            {
                countText.text = count.ToString();
            }

            // Update button interactability
            bool canActivate = count > 0 && !isActive;
            if (activateButton != null)
            {
                activateButton.interactable = canActivate;
            }

            // Update icon color
            if (iconImage != null)
            {
                iconImage.color = canActivate || isActive ? Color.white : inactiveColor;
            }

            // Update active indicator
            if (activeIndicator != null)
            {
                activeIndicator.SetActive(isActive);
            }

            // Update cooldown fill
            if (cooldownFillImage != null)
            {
                if (isActive)
                {
                    float duration = powerUpManager.GetDuration(powerUpType);
                    float remaining = powerUpManager.GetRemainingTime(powerUpType);
                    cooldownFillImage.fillAmount = remaining / duration;
                }
                else
                {
                    cooldownFillImage.fillAmount = 0f;
                }
            }

            // Update timer text
            if (timerText != null)
            {
                if (isActive)
                {
                    float remaining = powerUpManager.GetRemainingTime(powerUpType);
                    timerText.text = $"{remaining:F0}s";
                    timerText.gameObject.SetActive(true);
                }
                else
                {
                    timerText.gameObject.SetActive(false);
                }
            }
        }
        #endregion

        #region Button Callback
        private void OnActivateClicked()
        {
            if (powerUpManager == null)
                return;

            if (!powerUpManager.HasPowerUp(powerUpType))
            {
                ShowMessage("No power-ups available", false);
                return;
            }

            if (powerUpManager.IsActive(powerUpType))
            {
                ShowMessage("Already active", false);
                return;
            }

            bool success = powerUpManager.ActivatePowerUp(powerUpType);
            if (success)
            {
                ShowMessage($"{powerUpManager.GetPowerUpName(powerUpType)} activated!", true);
            }
        }
        #endregion

        #region Event Handlers
        private void OnInventoryChanged(Core.PowerUpType type, int newCount)
        {
            if (type == powerUpType)
            {
                UpdateUI();
            }
        }

        private void OnPowerUpActivated(Core.PowerUpType type, float duration)
        {
            if (type == powerUpType)
            {
                isActive = true;
                UpdateUI();
            }
        }

        private void OnPowerUpExpired(Core.PowerUpType type)
        {
            if (type == powerUpType)
            {
                isActive = false;
                UpdateUI();
            }
        }

        private void OnPowerUpTimeUpdated(Core.PowerUpType type, float remainingTime)
        {
            if (type == powerUpType && isActive)
            {
                UpdateUI();
            }
        }
        #endregion

        #region Helpers
        private void ShowMessage(string message, bool success)
        {
            var toast = FindObjectOfType<ToastNotification>();
            if (toast != null)
            {
                toast.Show(message, success ? ToastType.Success : ToastType.Warning);
            }
        }

        /// <summary>
        /// Set power-up type dynamically.
        /// </summary>
        public void SetPowerUpType(Core.PowerUpType type)
        {
            powerUpType = type;
            UpdateUI();
        }
        #endregion
    }
}
