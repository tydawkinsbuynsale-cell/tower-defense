# In-App Purchases (IAP) System Guide

Complete guide for the Robot Tower Defense IAP system with Unity IAP integration, product catalog, and monetization support.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Setup](#setup)
- [Product Catalog](#product-catalog)
- [Integration](#integration)
- [Testing](#testing)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

The IAP system enables monetization through in-app purchases using Unity's IAP service. Supports consumables (gems, credits), non-consumables (skins, maps), and subscriptions (premium pass).

**Files:**
- `Assets/Scripts/Monetization/IAPManager.cs` (720 lines)
- `Assets/Scripts/UI/ShopUI.cs` (320 lines)

### Key Characteristics

- **Unity IAP Integration**: Native support for iOS, Android, and other platforms
- **Product Catalog**: 13+ pre-configured products across all categories
- **Receipt Validation**: Server-side validation ready
- **Restore Functionality**: Required for iOS, available for all platforms
- **Editor Simulation**: Test purchases without real transactions
- **Analytics Integration**: Track all purchase events automatically

---

## Features

### Core Functionality

✅ **Product Types**
- **Consumables**: Gems, credits, power-ups (can be purchased multiple times)
- **Non-Consumables**: Skins, maps, ad removal (one-time purchases)
- **Subscriptions**: Premium pass with recurring billing

✅ **Purchase Flow**
- Initiate purchase with one button click
- Process payment through platform store (App Store, Google Play)
- Validate receipt (local + optional server validation)
- Grant rewards automatically
- Show success/failure feedback

✅ **Restore Purchases**
- Restore non-consumables on new device
- Required for iOS App Store compliance
- Recommended for Android Google Play

✅ **Edge Simulation**
- Test all purchases in Unity Editor without real money
- Simulates successful purchases and reward granting
- Toggle on/off in Inspector

---

## Architecture

### System Flow

```
┌─────────────┐
│   Player    │
│ Opens Shop  │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│    ShopUI       │ ─────► Display products with prices
│ (Product List)  │        (Localized from store)
└──────┬──────────┘
       │ Click "Buy"
       ▼
┌──────────────────┐      ┌──────────────────┐
│   IAPManager     │◄────►│  Unity IAP SDK   │
│ (Purchase Logic) │      │ (Store Interface)│
└──────────────────┘      └──────────────────┘
       │                           │
       │ ProcessPurchase           │ Payment
       ▼                           ▼
┌──────────────────┐      ┌──────────────────┐
│  Grant Rewards   │      │  App Store / GP  │
│  (Gems, Skins)   │      │  (User Payment)  │
└──────────────────┘      └──────────────────┘
       │
       │ Fire OnPurchaseComplete
       ▼
┌──────────────────┐      ┌──────────────────┐
│ AnalyticsManager │      │   SaveManager    │
│ (Track Purchase) │      │ (Persist Rewards)│
└──────────────────┘      └──────────────────┘
```

### Key Components

1. **IAPManager** (Singleton)
   - Initializes Unity IAP
   - Manages product catalog
   - Handles purchase processing
   - Validates and grants rewards

2. **ShopUI**
   - Displays products with prices
   - Handles tab switching (Consumables, Permanent, Subscriptions)
   - Shows purchase status
   - Restore purchases button

3. **ShopProductCard**
   - Individual product display
   - Purchase button with loading state
   - "Purchased" badge for non-consumables
   - Color-coded by product type

---

## Setup

### 1. Install Unity IAP Package

**Package Manager:**
```
Window → Package Manager → Unity Registry → In-App Purchasing → Install
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.unity.purchasing": "4.9.3"
  }
}
```

### 2. Configure Unity IAP Service

**Unity Services:**
1. Window → General → Services
2. Create/Link Unity Project
3. Enable "In-App Purchasing"
4. Configure products in Unity Dashboard (or use catalog in code)

### 3. Add IAPManager to Scene

Add to persistent scene (Bootstrap or MainMenu):

```csharp
GameObject iapObj = new GameObject("IAPManager");
iapObj.AddComponent<IAPManager>();
DontDestroyOnLoad(iapObj);
```

**Inspector Settings:**
- ✅ **Enable IAP**: true (production), false (disable monetization)
- **Verbose Logging**: true (development), false (production)
- ✅ **Simulate Purchases In Editor**: true (testing), false (real purchases in editor)

### 4. Add ShopUI to MainMenu

1. Create Shop Panel in Canvas
2. Add `ShopUI` component
3. Configure references:
   - Shop Panel (parent GameObject)
   - Product List Container (Vertical Layout Group)
   - Product Card Prefab
   - Tab buttons, status text, etc.

### 5. Configure Platform Stores

**iOS (App Store Connect):**
1. Create app in App Store Connect
2. Add products with matching IDs:
   - `gems_100`, `gems_500`, etc.
3. Set prices for each region
4. Enable In-App Purchases capability in Xcode

**Android (Google Play Console):**
1. Create app in Google Play Console
2. Add products in Monetization → Products:
   - Managed products (non-consumables)
   - Consumables
   - Subscriptions
3. Set prices for each country
4. Publish app to internal/alpha testing

---

## Product Catalog

### Consumables - Premium Currency (Gems)

| Product ID | Title | Price | Amount |
|------------|-------|-------|--------|
| `gems_100` | 100 Gems | $0.99 | 100 |
| `gems_500` | 500 Gems | $4.99 | 500 |
| `gems_1200` | 1200 Gems | $9.99 | 1200 |
| `gems_3000` | 3000 Gems | $19.99 | 3000 |

**Value Tiers:**
- $0.99 = 100 gems (1¢/gem baseline)
- $4.99 = 500 gems (20% bonus)
- $9.99 = 1200 gems (20% bonus)
- $19.99 = 3000 gems (50% bonus)

---

### Consumables - In-Game Currency (Credits)

| Product ID | Title | Price | Amount |
|------------|-------|-------|--------|
| `credits_5000` | 5000 Credits | $0.99 | 5000 |
| `credits_25000` | 25000 Credits | $4.99 | 25000 |

---

### Consumables - Power-Ups

| Product ID | Title | Price | Contents |
|------------|-------|-------|----------|
| `powerup_bundle` | Power-up Bundle | $2.99 | 5x all power-ups |

---

### Non-Consumables - Permanent Upgrades

| Product ID | Title | Price | Rewards |
|------------|-------|-------|---------|
| `starter_pack` | Starter Pack | $4.99 | 3000 gems + 10000 credits + gold tower skin |
| `remove_ads` | Remove Ads | $2.99 | Permanently disable ads |
| `tower_skin_gold` | Gold Tower Skin | $1.99 | Exclusive gold tower appearance |
| `tower_skin_neon` | Neon Tower Skin | $1.99 | Exclusive neon tower appearance |
| `map_pack_1` | Bonus Map Pack | $3.99 | 3 additional challenge maps |

---

### Subscriptions

| Product ID | Title | Price | Benefits |
|------------|-------|-------|----------|
| `premium_pass_monthly` | Premium Pass (Monthly) | $4.99/mo | Daily gems + exclusive rewards |

**Premium Pass Benefits:**
- 100 gems per day (3000/month value = $29.99)
- Exclusive tower skins
- Priority customer support
- Ad-free experience
- Early access to new features

---

## Integration

### Game Currency Integration

**Adding Gems (SaveManager):**
```csharp
// IAPManager.cs - AddGems() method
private void AddGems(int amount)
{
    var saveManager = Core.SaveManager.Instance;
    if (saveManager != null)
    {
        // Add gems field to PlayerSaveData:
        // public int gems = 0;
        
        saveManager.Data.gems += amount;
        saveManager.Save();
        
        Debug.Log($"Added {amount} gems - Total: {saveManager.Data.gems}");
    }
}
```

**Adding Credits (GameManager):**
```csharp
// Already integrated - uses GameManager.AddCredits()
private void AddCredits(int amount)
{
    var gameManager = Core.GameManager.Instance;
    if (gameManager != null)
    {
        gameManager.AddCredits(amount);
    }
}
```

---

### Analytics Integration

All purchase events automatically tracked:

**Events Tracked:**
- `iap_initialized`: IAP system ready
- `iap_purchase_initiated`: User clicked "Buy"
- `iap_purchase_success`: Purchase completed
  - Parameters: product_id, product_type, price, transaction_id
- `iap_purchase_failed`: Purchase failed
  - Parameters: product_id, reason
- `iap_restore_initiated`: Restore purchases clicked

**Example:**
```csharp
// Automatically tracked in IAPManager
Analytics.AnalyticsManager.Instance.TrackEvent("iap_purchase_success", new Dictionary<string, object>
{
    { "product_id", "gems_500" },
    { "product_type", "Consumable" },
    { "price", 4.99 },
    { "transaction_id", "1234567890" }
});
```

---

### Authentication Integration

Purchase records tied to authenticated players:

```csharp
// When processing purchase
string playerId = Online.AuthenticationManager.Instance?.PlayerId ?? "guest";

// Save purchase record with player ID
SavePurchaseRecord(productId, transactionId, playerId);
```

This enables:
- Cross-device purchase restoration
- Player-specific purchase history
- Anti-fraud detection
- Customer support lookup

---

### UI Integration Examples

**Open Shop from Main Menu:**
```csharp
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button shopButton;
    
    private void Start()
    {
        shopButton.onClick.AddListener(OpenShop);
    }
    
    private void OpenShop()
    {
        var shopUI = FindObjectOfType<ShopUI>();
        if (shopUI != null)
        {
            shopUI.Show();
        }
    }
}
```

**Check Premium Status:**
```csharp
public class FeatureController : MonoBehaviour
{
    private void Start()
    {
        bool isPremium = IAPManager.Instance.IsPremiumActive();
        
        if (isPremium)
        {
            EnablePremiumFeatures();
        }
    }
}
```

**Check Ads Removed:**
```csharp
public class AdManager : MonoBehaviour
{
    public void ShowAd()
    {
        if (IAPManager.Instance.AreAdsRemoved())
        {
            Debug.Log("Ads removed - skip");
            return;
        }
        
        // Show ad
    }
}
```

---

## Testing

### Editor Testing (Simulation Mode)

1. Enable "Simulate Purchases In Editor" in Inspector
2. Open shop in Play Mode
3. Click "Buy" on any product
4. Purchase simulated instantly (no real transaction)
5. Rewards granted immediately
6. Check Console for logs

**Context Menu Testing:**
```csharp
// Right-click IAPManager in Inspector
Buy 100 Gems           → Simulates gems_100 purchase
Buy Remove Ads         → Simulates remove_ads purchase
Restore Purchases      → Tests restore functionality
Print Product Catalog  → Lists all 13 products
```

---

### Device Testing (Sandbox Mode)

**iOS Sandbox Testing:**
1. Create Sandbox tester account in App Store Connect
2. Sign out of App Store on device
3. Build and install app to device
4. Make purchase - prompted to sign in with Sandbox account
5. Complete purchase (no real charge)
6. Verify rewards granted

**Android Testing:**
1. Upload AAB to Google Play Internal Testing track
2. Add test users via Google Play Console
3. Install app from Internal Testing link
4. Make purchase - prompted for payment
5. Use test card or real payment (refunded automatically)

---

### Test Scenarios

#### 1. First Purchase (Consumable)

1. Open shop
2. Click "Buy 100 Gems"
3. Complete payment
4. Verify gems added to player account
5. Check analytics event fired

**Expected:**
- Purchase processes successfully
- Gems added: `saveManager.Data.gems += 100`
- Analytics: `iap_purchase_success` event
- UI shows success notification

---

#### 2. Non-Consumable Purchase

1. Purchase "Remove Ads"
2. Complete payment
3. Verify ads disabled: `IAPManager.Instance.AreAdsRemoved() == true`
4. Try to purchase again - button should show "Purchased"

**Expected:**
- Purchase records saved to PlayerPrefs
- `HasPurchased("remove_ads")` returns true
- Purchase button disabled with "Purchased" label
- Ads no longer shown in-game

---

#### 3. Restore Purchases

1. Purchase "Gold Tower Skin" on Device A
2. Install app on Device B (or reset local data)
3. Click "Restore Purchases" in shop
4. Verify "Gold Tower Skin" shows as "Purchased"

**Expected:**
- Restore API called
- Non-consumables re-applied
- UI updated to show owned items
- Analytics: `iap_restore_initiated` event

---

#### 4. Subscription Status

1. Purchase "Premium Pass (Monthly)"
2. Close and reopen app
3. Verify premium status: `IAPManager.Instance.IsPremiumActive() == true`
4. Wait for expiration (or manually set past expiration)
5. Verify status becomes false

**Expected:**
- Expiration date saved: `PlayerPrefs("PremiumPass_Expiration")`
- Status checked on app launch
- Premium features disabled after expiration

---

#### 5. Purchase Failure Handling

1. Attempt purchase with insufficient funds
2. Verify error message shown
3. Verify analytics event: `iap_purchase_failed`
4. Verify purchase button re-enabled

**Expected:**
- OnPurchaseFailed event fires
- UI shows error: "Purchase failed: [reason]"
- Button returns to "Buy" state after 2 seconds
- No rewards granted

---

## Best Practices

### 1. Always Validate Receipts (Production)

```csharp
// Add server-side validation
public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
{
    string receipt = args.purchasedProduct.receipt;
    
    // Send receipt to your server for validation
    ValidateReceiptOnServer(receipt, (isValid) =>
    {
        if (isValid)
        {
            // Grant rewards
            ProcessProductPurchase(product, transactionId);
        }
        else
        {
            // Fraud detected - do not grant
            Debug.LogError("Receipt validation failed!");
        }
    });
    
    return PurchaseProcessingResult.Pending; // Complete after validation
}
```

### 2. Gracefully Handle Purchase Failures

```csharp
// Never block gameplay on IAP failures
iapManager.OnPurchaseFailed += (productId, reason) =>
{
    // Show friendly error
    ShowToast("Purchase failed - please try again later", ToastType.Error);
    
    // Log for debugging
    Debug.LogWarning($"IAP failed: {productId} - {reason}");
    
    // DO NOT: Disable game features or block access
};
```

### 3. Provide Restore Button (iOS Requirement)

```csharp
// Always include Restore button in shop for iOS
[SerializeField] private Button restoreButton;

private void Start()
{
    #if UNITY_IOS
    restoreButton.gameObject.SetActive(true);
    #else
    restoreButton.gameObject.SetActive(false); // Optional for Android
    #endif
}
```

### 4. Test All Purchase Flows Before Release

- Test each product type (consumable, non-consumable, subscription)
- Test purchase failure scenarios
- Test restore on fresh install
- Test subscription expiration
- Test with no internet connection
- Test with payment method declined

### 5. Localize Prices

```csharp
// Use store's localized price string
string price = iapManager.GetLocalizedPrice("gems_100");
// Returns "$0.99" (US), "€0,99" (EU), "¥120" (JP), etc.
```

### 6. Track Conversion Funnel

```csharp
// Track shop interactions for optimization
Analytics.AnalyticsManager.Instance.TrackEvent("shop_opened", null);
Analytics.AnalyticsManager.Instance.TrackEvent("product_viewed", new Dictionary<string, object>
{
    { "product_id", "gems_500" }
});
Analytics.AnalyticsManager.Instance.TrackEvent("iap_purchase_initiated", ...);
Analytics.AnalyticsManager.Instance.TrackEvent("iap_purchase_success", ...);
```

---

## Troubleshooting

### Problem: IAP Initialization Fails

**Symptoms:** Shop shows "Loading..." forever

**Solutions:**
1. Verify Unity IAP package installed
2. Check Unity Services enabled and linked
3. Verify internet connection
4. Check Console for error messages
5. Ensure product IDs match store configuration

---

### Problem: Purchase Button Does Nothing

**Symptoms:** Clicking "Buy" has no effect

**Solutions:**
1. Check `IAPManager.IsInitialized` is true
2. Verify product is `availableToPurchase`
3. Check button is not disabled
4. Look for errors in Console
5. Ensure IStoreListener properly implemented

---

### Problem: Rewards Not Granted

**Symptoms:** Purchase succeeds but gems/items not added

**Solutions:**
1. Check `ProcessPurchase()` is called
2. Verify product ID matches catalog
3. Check SaveManager/GameManager integration
4. Look for null reference exceptions
5. Verify `PurchaseProcessingResult.Complete` returned

---

### Problem: Restore Not Working

**Symptoms:** "Restore Purchases" doesn't restore items

**Solutions:**
1. Verify you're signed in with same Apple ID / Google account
2. Check products are non-consumables (consumables can't restore)
3. Ensure `IAppleExtensions.RestoreTransactions()` is called
4. Check receipt validation is not blocking restore
5. Look for errors in Console during restore

---

### Problem: Duplicate Purchases

**Symptoms:** User charged multiple times for same item

**Solutions:**
1. Return `PurchaseProcessingResult.Complete` immediately after granting
2. Don't return `.Pending` unless doing async validation
3. Add purchase idempotency check:
   ```csharp
   if (HasProcessedTransaction(transactionId))
   {
       return PurchaseProcessingResult.Complete; // Already processed
   }
   ```
4. Use transaction ID tracking to prevent duplicates

---

## API Reference

### IAPManager Public Methods

| Method | Description |
|--------|-------------|
| `BuyProduct(string productId)` | Initiate purchase for product |
| `RestorePurchases()` | Restore non-consumables (iOS required) |
| `GetProduct(string productId)` | Get product info from catalog |
| `GetProductsByType(ProductType type)` | Get all products of type |
| `GetLocalizedPrice(string productId)` | Get store price string |
| `HasPurchased(string productId)` | Check if non-consumable owned |
| `IsPremiumActive()` | Check if premium subscription active |
| `AreAdsRemoved()` | Check if ad removal purchased |
| `GetPurchaseCount(string productId)` | Get times product purchased |

### Events

| Event | Parameters | Description |
|-------|------------|-------------|
| `OnInitialized` | none | IAP system ready |
| `OnInitializeFailed` | string error | IAP init failed |
| `OnPurchaseComplete` | string productId, string transactionId | Purchase succeeded |
| `OnPurchaseFailed` | string productId, string reason | Purchase failed |
| `OnRestoreComplete` | none | Restore finished |

---

## Additional Resources

- [Unity IAP Documentation](https://docs.unity.com/iap/)
- [Apple App Store Guidelines](https://developer.apple.com/app-store/review/guidelines/#in-app-purchase)
- [Google Play Billing](https://developer.android.com/google/play/billing)
- [Receipt Validation Guide](https://docs.unity.com/iap/ReceiptValidation.html)

---

**Built for Robot Tower Defense | Version 1.4**
