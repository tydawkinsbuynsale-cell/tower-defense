using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RobotTD.Core;
using RobotTD.Towers;

namespace RobotTD.UI
{
    /// <summary>
    /// Tower selection button for the bottom bar.
    /// Shows tower icon, cost, and handles tap to select.
    /// </summary>
    public class TowerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Colors")]
        [SerializeField] private Color affordableColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color expensiveColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f);

        [Header("Long Press")]
        [SerializeField] private float longPressThreshold = 0.5f;

        private TowerData towerData;
        private bool isSelected = false;
        private bool isPressed = false;
        private float pressStartTime;
        private bool isUnlocked = true;

        public void Setup(TowerData data)
        {
            towerData = data;

            // Update visuals
            if (iconImage != null && data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.color = data.towerColor;
            }

            if (nameText != null)
            {
                nameText.text = data.towerName;
            }

            UpdateCostDisplay();
            RefreshAffordability();
        }

        private void Update()
        {
            // Check for long press (show info)
            if (isPressed && Time.time - pressStartTime >= longPressThreshold)
            {
                ShowTowerInfo();
                isPressed = false; // Prevent multiple triggers
            }
        }

        public void RefreshAffordability()
        {
            if (towerData == null) return;

            bool canAfford = GameManager.Instance?.CanAfford(towerData.cost) ?? false;

            // Update cost text color
            if (costText != null)
            {
                costText.color = canAfford ? affordableColor : expensiveColor;
            }

            // Update button alpha
            if (canvasGroup != null)
            {
                canvasGroup.alpha = canAfford ? 1f : 0.5f;
            }

            // Update locked overlay
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!isUnlocked);
            }
        }

        private void UpdateCostDisplay()
        {
            if (costText != null && towerData != null)
            {
                costText.text = $"${towerData.cost}";
            }
        }

        #region Selection

        public void Select()
        {
            if (!isUnlocked) return;
            if (!GameManager.Instance.CanAfford(towerData.cost))
            {
                // Play error sound / shake animation
                return;
            }

            isSelected = true;
            
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(true);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = selectedColor;
            }

            // Start tower placement
            TowerPlacementManager.Instance?.StartPlacement(towerData);

            // Deselect other buttons
            foreach (var button in transform.parent.GetComponentsInChildren<TowerButton>())
            {
                if (button != this)
                {
                    button.Deselect();
                }
            }
        }

        public void Deselect()
        {
            isSelected = false;
            
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(false);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }
        }

        #endregion

        #region Pointer Events

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            pressStartTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isPressed && Time.time - pressStartTime < longPressThreshold)
            {
                // Quick tap - select tower
                if (isSelected)
                {
                    Deselect();
                    TowerPlacementManager.Instance?.CancelPlacement();
                }
                else
                {
                    Select();
                }
            }

            isPressed = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Hover effect (for desktop)
            if (backgroundImage != null && !isSelected)
            {
                backgroundImage.color = Color.Lerp(normalColor, selectedColor, 0.3f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (backgroundImage != null && !isSelected)
            {
                backgroundImage.color = normalColor;
            }
            isPressed = false;
        }

        #endregion

        private void ShowTowerInfo()
        {
            // Show detailed tower info panel
            TowerInfoPopup.Instance?.Show(towerData, transform.position);
        }

        public void SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
            RefreshAffordability();
        }
    }

    /// <summary>
    /// Tower info popup for long press
    /// </summary>
    public class TowerInfoPopup : MonoBehaviour
    {
        public static TowerInfoPopup Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Image iconImage;

        private void Awake()
        {
            Instance = this;
            panel?.SetActive(false);
        }

        public void Show(TowerData data, Vector3 position)
        {
            if (data == null) return;

            panel?.SetActive(true);

            // Position near the button
            transform.position = position + Vector3.up * 100f;

            // Update content
            if (titleText != null) titleText.text = data.towerName;
            if (descriptionText != null) descriptionText.text = data.description;
            if (iconImage != null && data.icon != null) iconImage.sprite = data.icon;

            if (statsText != null)
            {
                statsText.text = $"Damage: {data.baseDamage}\n" +
                                 $"Range: {data.baseRange}\n" +
                                 $"Fire Rate: {data.baseFireRate}/s\n" +
                                 $"Cost: ${data.cost}";
            }
        }

        public void Hide()
        {
            panel?.SetActive(false);
        }

        private void Update()
        {
            // Hide on tap anywhere
            if (panel.activeSelf && Input.GetMouseButtonDown(0))
            {
                Hide();
            }
        }
    }
}
