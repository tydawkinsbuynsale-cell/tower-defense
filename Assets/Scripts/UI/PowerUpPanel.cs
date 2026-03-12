using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RobotTowerDefense.UI
{
    /// <summary>
    /// UI panel displaying all power-ups with activation buttons.
    /// Shows current inventory and active power-ups.
    /// </summary>
    public class PowerUpPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform powerUpButtonContainer;
        [SerializeField] private GameObject powerUpButtonPrefab;
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI totalCountText;

        [Header("Settings")]
        [SerializeField] private bool startExpanded = false;
        [SerializeField] private bool showInGameplay = true;

        private Core.PowerUpManager powerUpManager;
        private List<PowerUpButton> powerUpButtons = new List<PowerUpButton>();
        private bool isExpanded = false;

        #region Unity Lifecycle
        private void Start()
        {
            powerUpManager = Core.PowerUpManager.Instance;

            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(TogglePanel);
            }

            if (powerUpManager != null)
            {
                powerUpManager.OnInventoryChanged += OnInventoryChanged;
            }

            CreatePowerUpButtons();

            isExpanded = startExpanded;
            UpdatePanelVisibility();
            UpdateTotalCount();
        }

        private void OnDestroy()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(TogglePanel);
            }

            if (powerUpManager != null)
            {
                powerUpManager.OnInventoryChanged -= OnInventoryChanged;
            }
        }
        #endregion

        #region Panel Management
        private void TogglePanel()
        {
            isExpanded = !isExpanded;
            UpdatePanelVisibility();

            Analytics.AnalyticsManager.Instance?.TrackEvent("powerup_panel_toggled", new Dictionary<string, object>
            {
                { "expanded", isExpanded }
            });
        }

        private void UpdatePanelVisibility()
        {
            if (panel != null)
            {
                panel.SetActive(isExpanded);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            isExpanded = true;
            UpdatePanelVisibility();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        #endregion

        #region Power-Up Buttons
        private void CreatePowerUpButtons()
        {
            if (powerUpButtonContainer == null || powerUpButtonPrefab == null)
            {
                Debug.LogWarning("[PowerUpPanel] Missing button container or prefab");
                return;
            }

            // Clear existing buttons
            foreach (var button in powerUpButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            powerUpButtons.Clear();

            // Create button for each power-up type
            foreach (Core.PowerUpType type in System.Enum.GetValues(typeof(Core.PowerUpType)))
            {
                GameObject buttonObj = Instantiate(powerUpButtonPrefab, powerUpButtonContainer);
                PowerUpButton button = buttonObj.GetComponent<PowerUpButton>();

                if (button != null)
                {
                    button.SetPowerUpType(type);
                    powerUpButtons.Add(button);
                }
            }

            Debug.Log($"[PowerUpPanel] Created {powerUpButtons.Count} power-up buttons");
        }
        #endregion

        #region Event Handlers
        private void OnInventoryChanged(Core.PowerUpType type, int newCount)
        {
            UpdateTotalCount();
        }

        private void UpdateTotalCount()
        {
            if (powerUpManager == null || totalCountText == null)
                return;

            int total = powerUpManager.GetTotalPowerUpCount();
            totalCountText.text = $"Power-Ups: {total}";
        }
        #endregion
    }
}
