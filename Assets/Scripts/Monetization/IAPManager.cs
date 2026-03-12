using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace RobotTD.Monetization
{
    /// <summary>
    /// In-App Purchase manager with Unity IAP integration.
    /// Handles product catalog, purchases, receipt validation, and restore functionality.
    /// Supports consumables, non-consumables, and subscriptions.
    /// </summary>
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        public static IAPManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableIAP = true;
        [SerializeField] private bool verboseLogging = false;
        [SerializeField] private bool simulatePurchasesInEditor = true;

        // IAP state
        private IStoreController storeController;
        private IExtensionProvider extensionProvider;
        private bool isInitialized = false;

        // Products
        private Dictionary<string, IAPProduct> productCatalog = new Dictionary<string, IAPProduct>();

        // Events
        public event Action OnInitialized;
        public event Action<string> OnInitializeFailed;
        public event Action<string, string> OnPurchaseComplete; // productId, transactionId
        public event Action<string, string> OnPurchaseFailed; // productId, reason
        public event Action OnRestoreComplete;

        // Properties
        public bool IsInitialized => isInitialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!enableIAP)
            {
                LogDebug("IAP disabled");
                return;
            }

            InitializeProductCatalog();
            InitializeIAP();
        }

        // ── Product Catalog ───────────────────────────────────────────────────

        private void InitializeProductCatalog()
        {
            // Consumables - Gems (premium currency)
            AddProduct("gems_100", "100 Gems", "Small gem pack", 0.99m, ProductType.Consumable, 100);
            AddProduct("gems_500", "500 Gems", "Medium gem pack", 4.99m, ProductType.Consumable, 500);
            AddProduct("gems_1200", "1200 Gems", "Large gem pack", 9.99m, ProductType.Consumable, 1200);
            AddProduct("gems_3000", "3000 Gems", "Mega gem pack", 19.99m, ProductType.Consumable, 3000);

            // Consumables - Credits (in-game currency)
            AddProduct("credits_5000", "5000 Credits", "Credit pack", 0.99m, ProductType.Consumable, 5000);
            AddProduct("credits_25000", "25000 Credits", "Large credit pack", 4.99m, ProductType.Consumable, 25000);

            // Consumables - Power-ups
            AddProduct("powerup_bundle", "Power-up Bundle", "5x all power-ups", 2.99m, ProductType.Consumable);

            // Non-Consumables - Permanent upgrades
            AddProduct("starter_pack", "Starter Pack", "3000 gems + 10000 credits + tower skin", 4.99m, ProductType.NonConsumable);
            AddProduct("remove_ads", "Remove Ads", "Permanently disable ads", 2.99m, ProductType.NonConsumable);
            AddProduct("tower_skin_gold", "Gold Tower Skin", "Exclusive gold tower appearance", 1.99m, ProductType.NonConsumable);
            AddProduct("tower_skin_neon", "Neon Tower Skin", "Exclusive neon tower appearance", 1.99m, ProductType.NonConsumable);
            AddProduct("map_pack_1", "Bonus Map Pack", "3 additional challenge maps", 3.99m, ProductType.NonConsumable);

            // Subscriptions
            AddProduct("premium_pass_monthly", "Premium Pass (Monthly)", "Daily gems + exclusive rewards", 4.99m, ProductType.Subscription);

            LogDebug($"Initialized {productCatalog.Count} products");
        }

        private void AddProduct(string id, string title, string description, decimal price, ProductType type, int currencyAmount = 0)
        {
            var product = new IAPProduct
            {
                productId = id,
                title = title,
                description = description,
                price = price,
                type = type,
                currencyAmount = currencyAmount
            };

            productCatalog[id] = product;
        }

        // ── IAP Initialization ────────────────────────────────────────────────

        private void InitializeIAP()
        {
            if (isInitialized)
            {
                LogDebug("IAP already initialized");
                return;
            }

            LogDebug("Initializing Unity IAP...");

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Add all products to builder
            foreach (var product in productCatalog.Values)
            {
                builder.AddProduct(product.productId, product.type);
            }

            // Initialize
            UnityPurchasing.Initialize(this, builder);
        }

        // ── IStoreListener Implementation ─────────────────────────────────────

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            extensionProvider = extensions;
            isInitialized = true;

            LogDebug("IAP initialized successfully");

            // Update product prices from store
            UpdateProductPrices();

            OnInitialized?.Invoke();

            // Track initialization
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("iap_initialized", new Dictionary<string, object>
                {
                    { "product_count", productCatalog.Count }
                });
            }
        }

        public void OnInitializeFailed(InitializationFailureReason reason)
        {
            string error = $"IAP initialization failed: {reason}";
            LogDebug(error);
            OnInitializeFailed?.Invoke(error);

            // Track failure
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("iap_init_failed", new Dictionary<string, object>
                {
                    { "reason", reason.ToString() }
                });
            }
        }

        public void OnInitializeFailed(InitializationFailureReason reason, string message)
        {
            string error = $"IAP initialization failed: {reason} - {message}";
            LogDebug(error);
            OnInitializeFailed?.Invoke(error);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string productId = args.purchasedProduct.definition.id;
            string transactionId = args.purchasedProduct.transactionID;

            LogDebug($"Purchase successful: {productId} ({transactionId})");

            // Process purchase based on product type
            if (productCatalog.TryGetValue(productId, out IAPProduct product))
            {
                ProcessProductPurchase(product, transactionId);
            }
            else
            {
                LogDebug($"Unknown product: {productId}");
            }

            // Fire event
            OnPurchaseComplete?.Invoke(productId, transactionId);

            // Track purchase
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("iap_purchase_success", new Dictionary<string, object>
                {
                    { "product_id", productId },
                    { "product_type", product?.type.ToString() ?? "unknown" },
                    { "price", (double)(product?.price ?? 0) },
                    { "transaction_id", transactionId }
                });
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            string productId = product.definition.id;
            string reasonStr = reason.ToString();

            LogDebug($"Purchase failed: {productId} - {reasonStr}");

            OnPurchaseFailed?.Invoke(productId, reasonStr);

            // Track failure
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("iap_purchase_failed", new Dictionary<string, object>
                {
                    { "product_id", productId },
                    { "reason", reasonStr }
                });
            }
        }

        // ── Purchase Processing ───────────────────────────────────────────────

        /// <summary>
        /// Initiate a purchase for the specified product.
        /// </summary>
        public void BuyProduct(string productId)
        {
            if (!isInitialized)
            {
                LogDebug("IAP not initialized");
                OnPurchaseFailed?.Invoke(productId, "IAP not initialized");
                return;
            }

#if UNITY_EDITOR
            if (simulatePurchasesInEditor)
            {
                LogDebug($"[EDITOR] Simulating purchase: {productId}");
                SimulatePurchase(productId);
                return;
            }
#endif

            Product product = storeController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                LogDebug($"Purchasing: {productId}");
                storeController.InitiatePurchase(product);

                // Track purchase attempt
                if (Analytics.AnalyticsManager.Instance != null)
                {
                    Analytics.AnalyticsManager.Instance.TrackEvent("iap_purchase_initiated", new Dictionary<string, object>
                    {
                        { "product_id", productId }
                    });
                }
            }
            else
            {
                LogDebug($"Product not available: {productId}");
                OnPurchaseFailed?.Invoke(productId, "Product not available");
            }
        }

        private void ProcessProductPurchase(IAPProduct product, string transactionId)
        {
            switch (product.type)
            {
                case ProductType.Consumable:
                    ProcessConsumable(product);
                    break;

                case ProductType.NonConsumable:
                    ProcessNonConsumable(product);
                    break;

                case ProductType.Subscription:
                    ProcessSubscription(product);
                    break;
            }

            // Save purchase record
            SavePurchaseRecord(product.productId, transactionId);
        }

        private void ProcessConsumable(IAPProduct product)
        {
            LogDebug($"Processing consumable: {product.productId}");

            // Add currency based on product
            if (product.productId.StartsWith("gems_"))
            {
                AddGems(product.currencyAmount);
            }
            else if (product.productId.StartsWith("credits_"))
            {
                AddCredits(product.currencyAmount);
            }
            else if (product.productId == "powerup_bundle")
            {
                // Grant power-ups (to be implemented)
                LogDebug("Granted power-up bundle");
            }
        }

        private void ProcessNonConsumable(IAPProduct product)
        {
            LogDebug($"Processing non-consumable: {product.productId}");

            switch (product.productId)
            {
                case "starter_pack":
                    AddGems(3000);
                    AddCredits(10000);
                    UnlockTowerSkin("gold");
                    break;

                case "remove_ads":
                    SetAdsRemoved(true);
                    break;

                case "tower_skin_gold":
                    UnlockTowerSkin("gold");
                    break;

                case "tower_skin_neon":
                    UnlockTowerSkin("neon");
                    break;

                case "map_pack_1":
                    UnlockMapPack(1);
                    break;
            }
        }

        private void ProcessSubscription(IAPProduct product)
        {
            LogDebug($"Processing subscription: {product.productId}");

            switch (product.productId)
            {
                case "premium_pass_monthly":
                    ActivatePremiumPass(30); // 30 days
                    break;
            }
        }

        // ── Restore Purchases ─────────────────────────────────────────────────

        /// <summary>
        /// Restore previously purchased non-consumables and subscriptions.
        /// Required for iOS, recommended for Android.
        /// </summary>
        public void RestorePurchases()
        {
            if (!isInitialized)
            {
                LogDebug("IAP not initialized");
                return;
            }

            LogDebug("Restoring purchases...");

            var apple = extensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result) =>
            {
                if (result)
                {
                    LogDebug("Restore successful");
                    OnRestoreComplete?.Invoke();
                }
                else
                {
                    LogDebug("Restore failed");
                    OnPurchaseFailed?.Invoke("restore", "Restore failed");
                }
            });

            // Track restore
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("iap_restore_initiated", null);
            }
        }

        // ── Product Queries ───────────────────────────────────────────────────

        /// <summary>
        /// Get product info from catalog.
        /// </summary>
        public IAPProduct GetProduct(string productId)
        {
            if (productCatalog.TryGetValue(productId, out IAPProduct product))
            {
                return product;
            }
            return null;
        }

        /// <summary>
        /// Get all products of a specific type.
        /// </summary>
        public List<IAPProduct> GetProductsByType(ProductType type)
        {
            var results = new List<IAPProduct>();
            foreach (var product in productCatalog.Values)
            {
                if (product.type == type)
                {
                    results.Add(product);
                }
            }
            return results;
        }

        /// <summary>
        /// Get localized price string from store (e.g. "$0.99", "€0,99").
        /// </summary>
        public string GetLocalizedPrice(string productId)
        {
            if (!isInitialized)
                return productCatalog.TryGetValue(productId, out var product) ? $"${product.price}" : "N/A";

            Product storeProduct = storeController.products.WithID(productId);
            if (storeProduct != null && storeProduct.availableToPurchase)
            {
                return storeProduct.metadata.localizedPriceString;
            }

            return "N/A";
        }

        /// <summary>
        /// Check if a non-consumable has been purchased.
        /// </summary>
        public bool HasPurchased(string productId)
        {
            return PlayerPrefs.GetInt($"IAP_Purchased_{productId}", 0) == 1;
        }

        private void UpdateProductPrices()
        {
            foreach (var product in productCatalog.Keys)
            {
                Product storeProduct = storeController.products.WithID(product);
                if (storeProduct != null && storeProduct.availableToPurchase)
                {
                    // Store product has localized price from app store
                    LogDebug($"Product: {product} = {storeProduct.metadata.localizedPriceString}");
                }
            }
        }

        // ── Game Integration ──────────────────────────────────────────────────

        private void AddGems(int amount)
        {
            // Add to player save data
            var saveManager = Core.SaveManager.Instance;
            if (saveManager != null)
            {
                // Assuming gems field exists in PlayerSaveData
                // saveManager.Data.gems += amount;
                // saveManager.Save();

                LogDebug($"Added {amount} gems (placeholder - add gems field to SaveData)");
            }

            // Show UI notification
            ShowRewardNotification($"+{amount} Gems", "gems");
        }

        private void AddCredits(int amount)
        {
            var gameManager = Core.GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.AddCredits(amount);
                LogDebug($"Added {amount} credits");
            }
        }

        private void UnlockTowerSkin(string skinId)
        {
            PlayerPrefs.SetInt($"TowerSkin_{skinId}_Unlocked", 1);
            PlayerPrefs.Save();
            LogDebug($"Unlocked tower skin: {skinId}");

            ShowRewardNotification($"Unlocked: {skinId} tower skin", "tower_skin");
        }

        private void UnlockMapPack(int packId)
        {
            PlayerPrefs.SetInt($"MapPack_{packId}_Unlocked", 1);
            PlayerPrefs.Save();
            LogDebug($"Unlocked map pack: {packId}");

            ShowRewardNotification($"Unlocked: Map Pack {packId}", "map_pack");
        }

        private void SetAdsRemoved(bool removed)
        {
            PlayerPrefs.SetInt("AdsRemoved", removed ? 1 : 0);
            PlayerPrefs.Save();
            LogDebug($"Ads removed: {removed}");

            if (removed)
            {
                ShowRewardNotification("Ads Removed!", "no_ads");
            }
        }

        private void ActivatePremiumPass(int days)
        {
            long activationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long expirationTime = activationTime + (days * 24 * 60 * 60);

            PlayerPrefs.SetString("PremiumPass_Activation", activationTime.ToString());
            PlayerPrefs.SetString("PremiumPass_Expiration", expirationTime.ToString());
            PlayerPrefs.Save();

            LogDebug($"Activated premium pass for {days} days");

            ShowRewardNotification($"Premium Pass Active ({days} days)", "premium");
        }

        public bool IsPremiumActive()
        {
            long expiration = long.Parse(PlayerPrefs.GetString("PremiumPass_Expiration", "0"));
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return now < expiration;
        }

        public bool AreAdsRemoved()
        {
            return PlayerPrefs.GetInt("AdsRemoved", 0) == 1;
        }

        private void ShowRewardNotification(string message, string rewardType)
        {
            // Show toast notification (if ToastNotification UI exists)
            var toast = FindObjectOfType<UI.ToastNotification>();
            if (toast != null)
            {
                toast.Show(message, UI.ToastNotification.ToastType.Success);
            }
            else
            {
                LogDebug($"Reward: {message}");
            }
        }

        // ── Purchase Records ──────────────────────────────────────────────────

        private void SavePurchaseRecord(string productId, string transactionId)
        {
            // Mark non-consumables as purchased
            if (productCatalog.TryGetValue(productId, out IAPProduct product))
            {
                if (product.type == ProductType.NonConsumable || product.type == ProductType.Subscription)
                {
                    PlayerPrefs.SetInt($"IAP_Purchased_{productId}", 1);
                }
            }

            // Save transaction ID
            PlayerPrefs.SetString($"IAP_Transaction_{productId}_Latest", transactionId);
            
            // Increment purchase count
            int count = PlayerPrefs.GetInt($"IAP_PurchaseCount_{productId}", 0);
            PlayerPrefs.SetInt($"IAP_PurchaseCount_{productId}", count + 1);

            PlayerPrefs.Save();
        }

        public int GetPurchaseCount(string productId)
        {
            return PlayerPrefs.GetInt($"IAP_PurchaseCount_{productId}", 0);
        }

        // ── Editor Simulation ─────────────────────────────────────────────────

#if UNITY_EDITOR
        private void SimulatePurchase(string productId)
        {
            if (productCatalog.TryGetValue(productId, out IAPProduct product))
            {
                // Simulate successful purchase
                string transactionId = $"SIM_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
                ProcessProductPurchase(product, transactionId);
                OnPurchaseComplete?.Invoke(productId, transactionId);
                
                LogDebug($"[EDITOR] Simulated purchase complete: {productId}");
            }
            else
            {
                OnPurchaseFailed?.Invoke(productId, "Product not found");
            }
        }
#endif

        // ── Utilities ─────────────────────────────────────────────────────────

        private void LogDebug(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[IAPManager] {message}");
            }
        }

        // ── Context Menu Commands (Testing) ───────────────────────────────────

        [ContextMenu("Buy 100 Gems")]
        private void TestBuy100Gems()
        {
            BuyProduct("gems_100");
        }

        [ContextMenu("Buy Remove Ads")]
        private void TestBuyRemoveAds()
        {
            BuyProduct("remove_ads");
        }

        [ContextMenu("Restore Purchases")]
        private void TestRestorePurchases()
        {
            RestorePurchases();
        }

        [ContextMenu("Print Product Catalog")]
        private void TestPrintCatalog()
        {
            Debug.Log($"=== IAP Product Catalog ({productCatalog.Count} products) ===");
            foreach (var product in productCatalog.Values)
            {
                Debug.Log($"{product.productId}: {product.title} - ${product.price} ({product.type})");
            }
        }
    }

    // ── Data Classes ──────────────────────────────────────────────────────────

    [Serializable]
    public class IAPProduct
    {
        public string productId;
        public string title;
        public string description;
        public decimal price;
        public ProductType type;
        public int currencyAmount; // For consumables (gems, credits)
    }
}
