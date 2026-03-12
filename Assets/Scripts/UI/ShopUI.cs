using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace RobotTD.UI
{
    /// <summary>
    /// Shop UI for displaying and purchasing IAP products.
    /// Shows product catalog with prices, descriptions, and purchase buttons.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Transform productListContainer;
        [SerializeField] private GameObject productCardPrefab;
        [SerializeField] private Button closeButton;

        [Header("Tabs")]
        [SerializeField] private Button consumablesTabButton;
        [SerializeField] private Button permanentTabButton;
        [SerializeField] private Button subscriptionTabButton;

        [Header("Status")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header(" Restore")]
        [SerializeField] private Button restoreButton;

        private Monetization.IAPManager iapManager;
        private List<ShopProductCard> productCards = new List<ShopProductCard>();
        private ProductType currentTab = ProductType.Consumable;

        private void Awake()
        {
            closeButton?.onClick.AddListener(Hide);
            consumablesTabButton?.onClick.AddListener(() => ShowTab(ProductType.Consumable));
            permanentTabButton?.onClick.AddListener(() => ShowTab(ProductType.NonConsumable));
            subscriptionTabButton?.onClick.AddListener(() => ShowTab(ProductType.Subscription));
            restoreButton?.onClick.AddListener(OnRestoreButtonClicked);
        }

        private void Start()
        {
            iapManager = Monetization.IAPManager.Instance;

            if (iapManager == null)
            {
                Debug.LogError("[ShopUI] IAPManager not found!");
                ShowStatus("Shop unavailable");
                return;
            }

            // Subscribe to IAP events
            iapManager.OnInitialized += OnIAPInitialized;
            iapManager.OnInitializeFailed += OnIAPInitializeFailed;
            iapManager.OnPurchaseComplete += OnPurchaseComplete;
            iapManager.OnPurchaseFailed += OnPurchaseFailed;

            // Show loading if not initialized
            if (!iapManager.IsInitialized)
            {
                ShowStatus("Loading shop...");
                ShowLoadingState(true);
            }
            else
            {
                PopulateShop();
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (iapManager != null)
            {
                iapManager.OnInitialized -= OnIAPInitialized;
                iapManager.OnInitializeFailed -= OnIAPInitializeFailed;
                iapManager.OnPurchaseComplete -= OnPurchaseComplete;
                iapManager.OnPurchaseFailed -= OnPurchaseFailed;
            }
        }

        // ── Shop Display ──────────────────────────────────────────────────────

        private void PopulateShop()
        {
            ClearProductList();

            var products = iapManager.GetProductsByType(currentTab);

            foreach (var product in products)
            {
                CreateProductCard(product);
            }

            UpdateTabButtons();
            ShowLoadingState(false);
        }

        private void CreateProductCard(Monetization.IAPProduct product)
        {
            if (productCardPrefab == null || productListContainer == null)
            {
                Debug.LogError("[ShopUI] Product card prefab or container not assigned!");
                return;
            }

            GameObject cardObj = Instantiate(productCardPrefab, productListContainer);
            ShopProductCard card = cardObj.GetComponent<ShopProductCard>();

            if (card != null)
            {
                card.Initialize(product, iapManager);
                productCards.Add(card);
            }
        }

        private void ClearProductList()
        {
            foreach (var card in productCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            productCards.Clear();
        }

        private void ShowTab(ProductType type)
        {
            currentTab = type;
            PopulateShop();
        }

        private void UpdateTabButtons()
        {
            // Highlight active tab (to be styled with separate colors/graphics)
            if (consumablesTabButton != null)
                consumablesTabButton.interactable = (currentTab != ProductType.Consumable);
            
            if (permanentTabButton != null)
                permanentTabButton.interactable = (currentTab != ProductType.NonConsumable);
            
            if (subscriptionTabButton != null)
                subscriptionTabButton.interactable = (currentTab != ProductType.Subscription);
        }

        // ── Event Handlers ────────────────────────────────────────────────────

        private void OnIAPInitialized()
        {
            PopulateShop();
            ShowStatus("");
        }

        private void OnIAPInitializeFailed(string error)
        {
            ShowStatus($"Shop unavailable: {error}");
            ShowLoadingState(false);
        }

        private void OnPurchaseComplete(string productId, string transactionId)
        {
            ShowStatus("Purchase successful!");
            
            // Refresh UI (update "purchased" status)
            RefreshProductCards();

            // Hide status after delay
            Invoke(nameof(ClearStatus), 3f);
        }

        private void OnPurchaseFailed(string productId, string reason)
        {
            ShowStatus($"Purchase failed: {reason}");
            
            // Hide status after delay
            Invoke(nameof(ClearStatus), 5f);
        }

        private void OnRestoreButtonClicked()
        {
            if (iapManager != null)
            {
                ShowStatus("Restoring purchases...");
                iapManager.RestorePurchases();
            }
        }

        private void RefreshProductCards()
        {
            foreach (var card in productCards)
            {
                if (card != null)
                {
                    card.Refresh();
                }
            }
        }

        // ── UI State Management ───────────────────────────────────────────────

        public void Show()
        {
            shopPanel?.SetActive(true);

            // Track shop open
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("shop_opened", null);
            }
        }

        public void Hide()
        {
            shopPanel?.SetActive(false);
        }

        private void ShowLoadingState(bool loading)
        {
            loadingPanel?.SetActive(loading);
        }

        private void ShowStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
            }
        }

        private void ClearStatus()
        {
            ShowStatus("");
        }
    }

    /// <summary>
    /// Individual product card UI component.
    /// Displays product info and handles purchase button click.
    /// </summary>
    public class ShopProductCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private GameObject purchasedBadge;
        [SerializeField] private Image productIcon;

        [Header("Product Type Colors")]
        [SerializeField] private Color consumableColor = new Color(0.3f, 0.8f, 0.3f);
        [SerializeField] private Color nonConsumableColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color subscriptionColor = new Color(1f, 0.7f, 0.2f);

        private Monetization.IAPProduct product;
        private Monetization.IAPManager iapManager;

        public void Initialize(Monetization.IAPProduct product, Monetization.IAPManager manager)
        {
            this.product = product;
            this.iapManager = manager;

            // Setup UI
            if (titleText != null)
                titleText.text = product.title;

            if (descriptionText != null)
                descriptionText.text = product.description;

            if (priceText != null)
                priceText.text = manager.GetLocalizedPrice(product.productId);

            // Color by product type
            if (productIcon != null)
            {
                productIcon.color = GetProductTypeColor(product.type);
            }

            // Setup button
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
            }

            Refresh();
        }

        public void Refresh()
        {
            // Check if already purchased (for non-consumables)
            bool isPurchased = product.type != ProductType.Consumable && iapManager.HasPurchased(product.productId);

            if (purchasedBadge != null)
            {
                purchasedBadge.SetActive(isPurchased);
            }

            if (purchaseButton != null)
            {
                purchaseButton.interactable = !isPurchased;
            }

            if (buttonText != null)
            {
                buttonText.text = isPurchased ? "Purchased" : "Buy";
            }
        }

        private void OnPurchaseButtonClicked()
        {
            if (iapManager != null && product != null)
            {
                // Disable button during purchase
                if (purchaseButton != null)
                {
                    purchaseButton.interactable = false;
                }

                if (buttonText != null)
                {
                    buttonText.text = "Processing...";
                }

                iapManager.BuyProduct(product.productId);

                // Re-enable after delay (will be updated by refresh on complete)
                Invoke(nameof(EnableButton), 2f);
            }
        }

        private void EnableButton()
        {
            if (purchaseButton != null && product != null)
            {
                bool isPurchased = product.type != ProductType.Consumable && iapManager.HasPurchased(product.productId);
                purchaseButton.interactable = !isPurchased;
                
                if (buttonText != null)
                {
                    buttonText.text = isPurchased ? "Purchased" : "Buy";
                }
            }
        }

        private Color GetProductTypeColor(ProductType type)
        {
            switch (type)
            {
                case ProductType.Consumable:
                    return consumableColor;
                case ProductType.NonConsumable:
                    return nonConsumableColor;
                case ProductType.Subscription:
                    return subscriptionColor;
                default:
                    return Color.white;
            }
        }
    }
}
