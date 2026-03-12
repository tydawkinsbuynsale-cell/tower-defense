using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RobotTD.Towers;
using RobotTD.Core;

namespace RobotTD.UI
{
    /// <summary>
    /// Panel shown when a placed tower is selected.
    /// Shows stats, upgrade button, sell button, and targeting options.
    /// </summary>
    public class TowerInfoUI : MonoBehaviour
    {
        public static TowerInfoUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Tower Info")]
        [SerializeField] private Image towerIcon;
        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI statsText;

        [Header("Stat Bars")]
        [SerializeField] private Slider damageBar;
        [SerializeField] private Slider rangeBar;
        [SerializeField] private Slider fireRateBar;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI rangeText;
        [SerializeField] private TextMeshProUGUI fireRateText;

        [Header("Actions")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeCostText;
        [SerializeField] private Button sellButton;
        [SerializeField] private TextMeshProUGUI sellValueText;
        [SerializeField] private GameObject maxLevelIndicator;

        [Header("Targeting")]
        [SerializeField] private TMP_Dropdown targetingDropdown;

        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;

        private Tower currentTower;
        private bool isShowing = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Setup targeting dropdown
            if (targetingDropdown != null)
            {
                targetingDropdown.ClearOptions();
                targetingDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "First", "Last", "Strongest", "Weakest", "Closest"
                });
                targetingDropdown.onValueChanged.AddListener(OnTargetingChanged);
            }

            Hide();
        }

        private void Update()
        {
            // Smooth fade
            if (canvasGroup != null)
            {
                float targetAlpha = isShowing ? 1f : 0f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

                if (!isShowing && canvasGroup.alpha <= 0.01f)
                {
                    panel?.SetActive(false);
                }
            }

            // Update stats if tower exists
            if (isShowing && currentTower != null)
            {
                UpdateStats();
            }
        }

        public void Show(Tower tower)
        {
            if (tower == null) return;

            currentTower = tower;
            isShowing = true;
            panel?.SetActive(true);

            UpdateUI();
        }

        public void Hide()
        {
            isShowing = false;
            currentTower = null;
        }

        private void UpdateUI()
        {
            if (currentTower == null || currentTower.Data == null) return;

            var data = currentTower.Data;

            // Basic info
            if (towerIcon != null && data.icon != null)
            {
                towerIcon.sprite = data.icon;
                towerIcon.color = data.towerColor;
            }

            if (towerNameText != null)
            {
                towerNameText.text = data.towerName;
            }

            if (levelText != null)
            {
                levelText.text = $"Level {currentTower.Level}/{data.maxLevel}";
            }

            UpdateStats();
            UpdateButtons();
        }

        private void UpdateStats()
        {
            if (currentTower == null) return;

            // Stat bars (normalized 0-1 for visual)
            float maxDamage = 200f;  // For normalization
            float maxRange = 15f;
            float maxFireRate = 10f;

            if (damageBar != null)
            {
                damageBar.value = currentTower.CurrentDamage / maxDamage;
            }
            if (damageText != null)
            {
                damageText.text = $"{currentTower.CurrentDamage:F0}";
            }

            if (rangeBar != null)
            {
                rangeBar.value = currentTower.CurrentRange / maxRange;
            }
            if (rangeText != null)
            {
                rangeText.text = $"{currentTower.CurrentRange:F1}";
            }

            if (fireRateBar != null)
            {
                fireRateBar.value = currentTower.CurrentFireRate / maxFireRate;
            }
            if (fireRateText != null)
            {
                fireRateText.text = $"{currentTower.CurrentFireRate:F1}/s";
            }

            // Stats text
            if (statsText != null)
            {
                var data = currentTower.Data;
                string special = "";
                
                if (data.slowPercent > 0)
                    special += $"Slow: {data.slowPercent * 100}%\n";
                if (data.splashRadius > 0)
                    special += $"Splash: {data.splashRadius}m\n";
                if (data.chainCount > 0)
                    special += $"Chain: {data.chainCount} targets\n";
                if (data.dotDamage > 0)
                    special += $"Burn: {data.dotDamage}/s\n";

                statsText.text = special;
            }
        }

        private void UpdateButtons()
        {
            if (currentTower == null) return;

            // Upgrade button
            bool canUpgrade = currentTower.CanUpgrade();
            bool isMaxLevel = currentTower.IsMaxLevel;

            if (upgradeButton != null)
            {
                upgradeButton.interactable = canUpgrade;
                upgradeButton.gameObject.SetActive(!isMaxLevel);
            }

            if (upgradeCostText != null && !isMaxLevel)
            {
                int cost = currentTower.UpgradeCost;
                upgradeCostText.text = $"${cost}";
                upgradeCostText.color = GameManager.Instance.CanAfford(cost) 
                    ? Color.green 
                    : Color.red;
            }

            if (maxLevelIndicator != null)
            {
                maxLevelIndicator.SetActive(isMaxLevel);
            }

            // Sell button
            if (sellValueText != null)
            {
                sellValueText.text = $"${currentTower.SellValue}";
            }
        }

        #region Button Callbacks

        public void OnUpgradeClicked()
        {
            if (currentTower != null && currentTower.CanUpgrade())
            {
                currentTower.Upgrade();
                UpdateUI();
            }
        }

        public void OnSellClicked()
        {
            if (currentTower != null)
            {
                TowerPlacementManager.Instance?.RemoveTower(currentTower);
                currentTower.Sell();
                Hide();
            }
        }

        public void OnCloseClicked()
        {
            TowerPlacementManager.Instance?.DeselectTower();
            Hide();
        }

        private void OnTargetingChanged(int index)
        {
            if (currentTower != null)
            {
                currentTower.SetTargetPriority((TargetPriority)index);
            }
        }

        #endregion
    }

    /// <summary>
    /// World-space health bar for enemies
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Gradient healthGradient;
        [SerializeField] private bool lookAtCamera = true;
        [SerializeField] private Vector3 offset = Vector3.up * 2f;

        private Transform target;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            target = transform.parent;
        }

        private void LateUpdate()
        {
            // Follow target
            if (target != null)
            {
                transform.position = target.position + offset;
            }

            // Face camera
            if (lookAtCamera && mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.forward);
            }
        }

        public void SetHealth(float current, float max)
        {
            float percent = current / max;
            
            if (slider != null)
            {
                slider.value = percent;
            }

            if (fillImage != null && healthGradient != null)
            {
                fillImage.color = healthGradient.Evaluate(percent);
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }

    /// <summary>
    /// Floating damage numbers
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private AnimationCurve scaleCurve;

        private float timer;
        private Vector3 startScale;
        private Color startColor;

        private void Awake()
        {
            startScale = transform.localScale;
            if (text != null)
            {
                startColor = text.color;
            }
        }

        public void Show(float damage, Vector3 position, Color color)
        {
            transform.position = position;
            timer = 0f;

            if (text != null)
            {
                text.text = damage >= 1 ? $"{damage:F0}" : $"{damage:F1}";
                text.color = color;
                startColor = color;
            }

            transform.localScale = startScale;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            timer += Time.deltaTime;

            // Float up
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // Scale animation
            if (scaleCurve != null)
            {
                float t = timer / lifetime;
                transform.localScale = startScale * scaleCurve.Evaluate(t);
            }

            // Fade out
            if (text != null)
            {
                float alpha = Mathf.Lerp(startColor.a, 0f, timer / lifetime);
                text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            // Destroy when done
            if (timer >= lifetime)
            {
                var pooled = GetComponent<Core.PooledObject>();
                if (pooled != null)
                {
                    pooled.ReturnToPool();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
