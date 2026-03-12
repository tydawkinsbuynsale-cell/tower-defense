# Ads System Guide

Complete guide for the Robot Tower Defense ad monetization system with Unity Ads integration, frequency controls, and IAP integration.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Setup](#setup)
- [Ad Types](#ad-types)
- [Integration](#integration)
- [Testing](#testing)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

The Ads system provides monetization through Unity Ads with intelligent frequency controls and seamless IAP integration. Supports interstitial ads, rewarded ads, and banner ads with player-friendly cooldowns.

**Files:**
- `Assets/Scripts/Monetization/AdManager.cs` (850 lines)
- `Assets/Scripts/UI/RewardedAdButton.cs` (280 lines)

### Key Characteristics

- **Unity Ads Integration**: Native support for iOS and Android
- **Three Ad Types**: Interstitial (full-screen), Rewarded (watch for bonus), Banner (persistent)
- **Frequency Controls**: Cooldowns and gameplay-based triggers to avoid spamming
- **IAP Integration**: Respects "Remove Ads" purchase automatically
- **Editor Simulation**: Test ads without SDK in Unity Editor
- **Analytics Integration**: Track all ad events for revenue optimization

---

## Features

### Core Functionality

✅ **Ad Types**
- **Interstitial Ads**: Full-screen ads shown between gameplay sessions
- **Rewarded Ads**: Opt-in ads that grant bonuses (gems, credits, continues)
- **Banner Ads**: Small persistent ads at screen edges

✅ **Frequency Controls**
- Cooldown timers prevent ad spam (3 min interstitial, 1 min rewarded)
- Gameplay-based triggers (show after X games, not mid-session)
- Query API to check if ads can be shown

✅ **IAP Integration**
- Automatically respects "Remove Ads" IAP purchase
- Interstitial and banner ads disabled if purchased
- Rewarded ads still available (optional bonus for premium users)

✅ **Editor Simulation**
- Test all ad flows without Unity Ads SDK
- Configurable simulated ad duration
- Full event callbacks for UI testing

---

## Architecture

### System Flow

```
┌──────────────┐
│  GameManager │
│ (Game Over)  │
└──────┬───────┘
       │ ShowInterstitialAd()
       ▼
┌──────────────────┐
│   AdManager      │ ──► Check IAPManager.AreAdsRemoved()
│ (Ad Logic)       │     └─► If true: Skip ad
└──────┬───────────┘     └─► If false: Continue
       │
       ├─► Check cooldown (3 min since last ad?)
       ├─► Check gameplay count (3 games played?)
       │
       ▼
┌──────────────────┐
│  Unity Ads SDK   │
│ (Ad Display)     │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐      ┌──────────────────┐
│ OnAdComplete     │      │ AnalyticsManager │
│ (Callback)       │──────►│ (Track Events)   │
└──────────────────┘      └──────────────────┘
```

### Key Components

1. **AdManager** (Singleton)
   - Initializes Unity Ads SDK
   - Manages ad loading and showing
   - Enforces frequency controls
   - Integrates with IAPManager

2. **RewardedAdButton**
   - UI component for rewarded ads
   - Displays reward preview
   - Shows cooldown timer
   - Grants rewards on completion

---

## Setup

### 1. Install Unity Ads Package

**Package Manager:**
```
Window → Package Manager → Unity Registry → Advertisement Legacy → Install
```

Or add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.unity.ads": "4.4.2"
  }
}
```

### 2. Configure Unity Ads Service

**Unity Services:**
1. Window → General → Services
2. Create/Link Unity Project
3. Enable "Ads"
4. Copy Game IDs (iOS and Android)

**Unity Ads Dashboard:**
1. Go to dashboard.unity3d.com
2. Navigate to Monetization → Ad Units
3. Create placements:
   - `Interstitial_Android` (Interstitial)
   - `Rewarded_Android` (Rewarded Video)
   - `Banner_Android` (Banner)
4. Copy placement IDs

### 3. Add AdManager to Scene

Add to persistent scene (Bootstrap or MainMenu):

```csharp
GameObject adObj = new GameObject("AdManager");
adObj.AddComponent<AdManager>();
DontDestroyOnLoad(adObj);
```

**Inspector Configuration:**

**Unity Ads Setup:**
- **Android Game ID**: `5678901` (from Unity Ads dashboard)
- **iOS Game ID**: `5678900` (from Unity Ads dashboard)
- **Test Mode**: ✅ true (development), ❌ false (production)
- **Enable Ads**: ✅ true (enable monetization)

**Ad Placement IDs:**
- **Interstitial Placement ID**: `Interstitial_Android`
- **Rewarded Placement ID**: `Rewarded_Android`
- **Banner Placement ID**: `Banner_Android`

**Frequency Controls:**
- **Interstitial Cooldown**: 180 (3 minutes between ads)
- **Interstitial Gameplay Count**: 3 (show after 3 games)
- **Rewarded Cooldown**: 60 (1 minute between rewarded ads)

**Banner Settings:**
- **Show Banner On Start**: ❌ false (typically)
- **Banner Position**: Bottom Center

**Editor Simulation:**
- **Simulate Ads In Editor**: ✅ true (testing)
- **Simulated Ad Duration**: 2.0 (2 seconds)

### 4. Add RewardedAdButton to UI

1. Create button in Game Over / Shop / Daily Rewards UI
2. Add `RewardedAdButton` component
3. Configure:
   - Watch Ad Button (Button reference)
   - Reward Text (TextMeshProUGUI)
   - Cooldown Text (TextMeshProUGUI)
   - Reward Type (Gems, Credits, etc.)
   - Reward Amount (50 gems, 1000 credits, etc.)

---

## Ad Types

### Interstitial Ads

**Purpose:** Monetize between gameplay sessions without interrupting action.

**When to Show:**
- After game over (defeat or victory)
- After completing daily mission
- When returning from pause menu
- Between map transitions

**Frequency Controls:**
- 3-minute cooldown (configurable)
- Only after 3 completed games (configurable)
- Never during active gameplay

**Usage:**
```csharp
// After game over
AdManager.Instance.ShowInterstitialAd(() =>
{
    // Ad complete or skipped - continue to results screen
    ShowResultsScreen();
});
```

**IAP Integration:**
- Automatically skipped if "Remove Ads" purchased
- Check: `IAPManager.Instance.AreAdsRemoved()`

---

### Rewarded Ads

**Purpose:** Offer optional bonuses to players who watch ads.

**Common Rewards:**
- **50-100 Gems**: Premium currency bonus
- **1000-5000 Credits**: Soft currency bonus
- **Continue Game**: Revive after defeat
- **Double Rewards**: 2x rewards after victory
- **Extra Life**: Additional health during gameplay

**When to Offer:**
- Game over screen (continue option)
- Victory screen (double rewards)
- Shop UI (earn gems button)
- Daily rewards (bonus claim)

**Frequency Controls:**
- 1-minute cooldown (configurable)
- Available even if "Remove Ads" purchased (optional bonus)

**Usage:**
```csharp
// Offer continue after game over
AdManager.Instance.ShowRewardedAd("continue", (success) =>
{
    if (success)
    {
        // Grant continue - restore player health
        GameManager.Instance.ContinueGame();
    }
    else
    {
        // Ad not completed - show game over screen
        ShowGameOverScreen();
    }
});
```

**RewardedAdButton Component:**
```csharp
// Automatic UI with cooldown display
RewardedAdButton button = GetComponent<RewardedAdButton>();
button.SetReward(RewardType.Gems, 50);

// Button handles:
// - Ad availability check
// - Cooldown display
// - Reward granting
// - Analytics tracking
```

---

### Banner Ads

**Purpose:** Persistent low-value monetization without interrupting gameplay.

**When to Show:**
- Main menu screen
- Shop UI
- Leaderboard screen
- Settings screen

**When NOT to Show:**
- During active gameplay (obstructs view)
- On game over / victory screens (interstitials better)
- If "Remove Ads" purchased

**Usage:**
```csharp
// Show banner on main menu
private void OnEnable()
{
    AdManager.Instance.ShowBanner();
}

private void OnDisable()
{
    AdManager.Instance.HideBanner();
}
```

**Banner Positions:**
- Top Left/Center/Right
- Bottom Left/Center/Right
- Center (not recommended)

---

## Integration

### Game Over Screen Integration

**Show Interstitial After Defeat:**
```csharp
public class GameOverUI : MonoBehaviour
{
    public void OnGameOver()
    {
        // Track gameplay session for frequency
        AdManager.Instance.OnGameplaySessionComplete();

        // Show interstitial if cooldown passed
        AdManager.Instance.ShowInterstitialAd(() =>
        {
            // Continue to results after ad (or if skipped)
            ShowResults();
        });
    }
}
```

**Offer Rewarded Continue:**
```csharp
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private RewardedAdButton continueButton;

    private void Start()
    {
        // Configure continue reward
        continueButton.SetReward(RewardType.ContinueGame, 1);

        // Button automatically handles ad display and cooldown
    }

    // Called by RewardedAdButton after successful ad
    public void OnContinueGranted()
    {
        GameManager.Instance.ContinueGame();
        gameObject.SetActive(false);
    }
}
```

---

### Victory Screen Integration

**Offer Double Rewards:**
```csharp
public class VictoryUI : MonoBehaviour
{
    [SerializeField] private RewardedAdButton doubleRewardsButton;
    [SerializeField] private TextMeshProUGUI rewardsText;

    private int baseReward = 1000;

    private void Start()
    {
        doubleRewardsButton.SetReward(RewardType.DoubleReward, 1);
        rewardsText.text = $"Rewards: {baseReward} credits";
    }

    public void OnDoubleRewardsGranted()
    {
        int bonusReward = baseReward;
        GameManager.Instance.AddCredits(bonusReward);
        
        rewardsText.text = $"Rewards: {baseReward * 2} credits (2x Bonus!)";
        
        Debug.Log($"Double rewards granted - Bonus: {bonusReward}");
    }
}
```

---

### Shop Integration

**Earn Free Gems:**
```csharp
public class ShopUI : MonoBehaviour
{
    [SerializeField] private RewardedAdButton earnGemsButton;

    private void Start()
    {
        earnGemsButton.SetReward(RewardType.Gems, 50);

        // Button automatically integrates with AdManager
        // Grants 50 gems on successful ad completion
    }
}
```

---

### Main Menu Integration

**Show Banner Ad:**
```csharp
public class MainMenuUI : MonoBehaviour
{
    private void OnEnable()
    {
        // Show banner when menu opens
        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowBanner();
        }
    }

    private void OnDisable()
    {
        // Hide banner when menu closes
        if (AdManager.Instance != null)
        {
            AdManager.Instance.HideBanner();
        }
    }
}
```

---

### Analytics Integration

All ad events automatically tracked:

**Events Tracked:**
- `ads_initialized`: Unity Ads ready
- `ad_shown`: Ad started (type, placement)
- `ad_clicked`: Ad clicked
- `ad_completed`: Ad finished (completion state)
- `ad_reward_earned`: Rewarded ad completed (reward type, amount)
- `ad_load_failed`: Ad failed to load (error)
- `ad_show_failed`: Ad failed to show (error)

**Example:**
```csharp
// Automatically tracked by AdManager
Analytics.AnalyticsManager.Instance.TrackEvent("ad_reward_earned", new Dictionary<string, object>
{
    { "reward_type", "gems" },
    { "reward_amount", 50 },
    { "placement", "shop_ui" }
});
```

---

## Testing

### Editor Testing (Simulation Mode)

1. Enable "Simulate Ads In Editor" in AdManager Inspector
2. Set "Simulated Ad Duration" (default 2 seconds)
3. Enter Play Mode
4. Call ad methods - simulated instantly

**Context Menu Testing:**
```csharp
// Right-click AdManager in Inspector
Show Interstitial Ad → Simulates interstitial with 2s delay
Show Rewarded Ad     → Simulates rewarded ad with reward grant
Show Banner          → Logs banner show (no visual)
Hide Banner          → Logs banner hide
Print Ad Status      → Shows cooldowns, initialization, IAP status
```

**Simulation Behavior:**
- Ads complete after `simulatedAdDuration` seconds
- Rewarded ads always grant rewards
- Cooldowns enforced normally
- Analytics events tracked
- No Unity Ads SDK required

---

### Device Testing (Test Mode)

**Android Testing:**
1. Set "Test Mode" to true in AdManager Inspector
2. Build APK and install on device
3. Ads shown are test ads (no revenue)
4. Verify all ad types display correctly

**iOS Testing:**
1. Set "Test Mode" to true
2. Build to Xcode and deploy to device
3. Test ads displayed with test watermark
4. Verify all placements work

**Test Checklist:**
- ✅ Interstitial shows after game over
- ✅ Interstitial respects cooldown (3 min)
- ✅ Interstitial respects gameplay count (3 games)
- ✅ Rewarded ad grants reward on completion
- ✅ Rewarded ad respects cooldown (1 min)
- ✅ Banner shows on main menu
- ✅ Banner hides during gameplay
- ✅ All ads skip if "Remove Ads" purchased

---

### Production Testing

**Before Launch:**
1. Set "Test Mode" to **false**
2. Verify Game IDs are production IDs (not test IDs)
3. Test on real device with production build
4. Verify ads are real (no test watermark)
5. Verify revenue tracking in Unity Ads dashboard

**Soft Launch Testing:**
1. Release to small region (e.g., Canada)
2. Monitor ad fill rate (% of ad requests filled)
3. Monitor eCPM (earnings per 1000 impressions)
4. Adjust frequency if needed (increase/decrease cooldowns)

---

## Best Practices

### 1. Never Interrupt Active Gameplay

```csharp
// ✅ GOOD: Show after game over
private void OnGameOver()
{
    AdManager.Instance.ShowInterstitialAd(() =>
    {
        ShowResultsScreen();
    });
}

// ❌ BAD: Show during wave
private void OnWaveStart()
{
    AdManager.Instance.ShowInterstitialAd(() => {}); // Ruins UX!
}
```

### 2. Always Provide Value for Rewarded Ads

```csharp
// ✅ GOOD: Clear value proposition
button.SetReward(RewardType.Gems, 50);
rewardText.text = "Watch Ad\n+50 Gems";

// ❌ BAD: Unclear or low value
button.SetReward(RewardType.Credits, 10); // Too little
rewardText.text = "Watch Ad"; // Doesn't show reward
```

### 3. Respect "Remove Ads" Purchase

```csharp
// AdManager automatically checks, but for custom logic:
if (IAPManager.Instance.AreAdsRemoved())
{
    // Hide interstitial/banner ad buttons
    hideInterstitialButton.gameObject.SetActive(false);
    
    // Keep rewarded ad buttons visible (optional bonus)
    rewardedButton.gameObject.SetActive(true);
}
```

### 4. Use Cooldowns to Avoid Spam

```csharp
// Built into AdManager, but check before calling:
if (AdManager.Instance.CanShowInterstitial())
{
    AdManager.Instance.ShowInterstitialAd(OnAdComplete);
}
else
{
    // Too soon - skip ad this time
    OnAdComplete();
}
```

### 5. Track Gameplay Sessions for Frequency

```csharp
// Call after each game
private void OnGameComplete()
{
    AdManager.Instance.OnGameplaySessionComplete();
    
    // AdManager tracks count and shows interstitial after 3 games
}
```

### 6. Handle Ad Failures Gracefully

```csharp
AdManager.Instance.ShowRewardedAd("gems", (success) =>
{
    if (success)
    {
        GrantGems(50);
        ShowToast("Earned 50 gems!", ToastType.Success);
    }
    else
    {
        // Ad not available or cancelled - don't punish player
        ShowToast("Ad not available. Try again later.", ToastType.Info);
        
        // DO NOT: Remove features or block progress
    }
});
```

### 7. Monitor Ad Performance

```csharp
// Track conversion rates
Analytics.AnalyticsManager.Instance.TrackEvent("rewarded_ad_offered", new Dictionary<string, object>
{
    { "context", "game_over_continue" }
});

// Later, after ad:
Analytics.AnalyticsManager.Instance.TrackEvent("rewarded_ad_accepted", new Dictionary<string, object>
{
    { "context", "game_over_continue" },
    { "success", true }
});

// Analyze: rewarded_ad_accepted / rewarded_ad_offered = conversion rate
```

---

## Troubleshooting

### Problem: Ads Not Initializing

**Symptoms:** `IsInitialized` always false, no ads show

**Solutions:**
1. Verify Unity Ads package installed (com.unity.ads)
2. Check Game ID is correct in Inspector
3. Enable Unity Services (Window → Services)
4. Check internet connection on device
5. Look for initialization errors in Console
6. Verify `#if UNITY_ADS` directive (add Scripting Define Symbol if needed)

---

### Problem: Ads Not Showing

**Symptoms:** Ad methods called but nothing happens

**Solutions:**
1. Check "Remove Ads" IAP status: `IAPManager.Instance.AreAdsRemoved()`
2. Verify cooldown passed: `AdManager.Instance.CanShowInterstitial()`
3. Check gameplay count: `AdManager.Instance.GetGameplayCountUntilAd()`
4. Ensure ad loaded: Check logs for "Ad loaded" messages
5. Test in Test Mode first before production
6. Check placement IDs match Unity Ads dashboard

---

### Problem: Rewarded Ads Not Granting Rewards

**Symptoms:** Ad completes but no reward given

**Solutions:**
1. Check callback receives `success = true`
2. Verify `OnUnityAdsShowComplete` with `COMPLETED` state
3. Check reward granting logic in RewardedAdButton
4. Look for null reference errors in SaveManager/GameManager
5. Ensure SaveManager.Data.gems field exists (if using gems)

---

### Problem: Banner Always Visible

**Symptoms:** Banner shows during gameplay

**Solutions:**
1. Call `HideBanner()` when entering gameplay scene
2. Add banner hide to GameManager.StartGame()
3. Check UI layer ordering (banner should be below game UI)
4. Consider only showing banner on menus, not in-game

---

### Problem: Too Many Ads (Player Complaints)

**Symptoms:** Low retention, negative reviews about ads

**Solutions:**
1. Increase interstitial cooldown (180s → 300s)
2. Increase gameplay count (3 → 5 games)
3. Add more "Remove Ads" IAP promotions
4. Only show interstitials on defeats, not victories
5. Remove banner ads from gameplay entirely
6. Monitor ads-per-session metric (recommended: < 3 per 30 min)

---

### Problem: Low Ad Revenue

**Symptoms:** Fill rate or eCPM too low

**Solutions:**
1. Enable mediation in Unity Ads dashboard (AdMob, AppLovin)
2. Increase ad frequency (more impressions = more revenue)
3. Add more rewarded ad placements (shop, daily rewards)
4. Optimize ad placement timing (show when engagement highest)
5. Target high-value regions (US, UK, CA, AU)
6. Monitor fill rate by region and adjust targeting

---

## API Reference

### AdManager Public Methods

| Method | Description |
|--------|-------------|
| `ShowInterstitialAd(Action onComplete)` | Show full-screen interstitial ad |
| `ShowRewardedAd(string rewardType, Action<bool> onComplete)` | Show rewarded ad, grant bonus on success |
| `ShowBanner()` | Display persistent banner ad |
| `HideBanner()` | Hide banner ad |
| `OnGameplaySessionComplete()` | Track game completion for frequency |
| `CanShowInterstitial()` | Check if interstitial available (cooldown passed) |
| `CanShowRewarded()` | Check if rewarded ad available |
| `GetInterstitialCooldownRemaining()` | Get seconds until next interstitial |
| `GetRewardedCooldownRemaining()` | Get seconds until next rewarded ad |
| `GetGameplayCountUntilAd()` | Get games remaining before interstitial |

### Events

| Event | Parameters | Description |
|-------|------------|-------------|
| `OnAdsInitialized` | none | Unity Ads initialized successfully |
| `OnAdShown` | string placementId | Ad started displaying |
| `OnAdClosed` | string placementId | Ad closed (completed or skipped) |
| `OnAdFailed` | string placementId, string error | Ad failed to load or show |
| `OnRewardEarned` | string rewardType | Rewarded ad completed, reward granted |

---

## Additional Resources

- [Unity Ads Documentation](https://docs.unity.com/ads/)
- [Unity Ads Dashboard](https://dashboard.unity3d.com/)
- [Mediation Setup](https://docs.unity.com/mediation/)
- [Best Practices Guide](https://docs.unity.com/ads/BestPractices.html)

---

**Built for Robot Tower Defense | Version 1.5**
