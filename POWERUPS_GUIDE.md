# Power-Ups System Guide

Complete guide for the Robot Tower Defense power-up system with temporary gameplay boosts, inventory management, and monetization integration.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Setup](#setup)
- [Power-Up Types](#power-up-types)
- [Integration](#integration)
- [Testing](#testing)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

The Power-Ups system provides temporary gameplay advantages through consumable items. Players can purchase power-ups via IAP, earn them from rewarded ads, or receive them as mission rewards.

**Files:**
- `Assets/Scripts/Core/PowerUpManager.cs` (780 lines)
- `Assets/Scripts/UI/PowerUpButton.cs` (220 lines)
- `Assets/Scripts/UI/PowerUpPanel.cs` (180 lines)

### Key Characteristics

- **Five Power-Up Types**: Damage boost, speed boost,credit boost, shield, time freeze
- **Inventory System**: Stack up to 99 of each type
- **Duration-Based**: Temporary effects (30-60s)
- **Visual Feedback**: Active indicators, cooldown timers
- **Monetization Ready**: IAP bundles and rewarded ad integration
- **Analytics Tracking**: All power-up events logged

---

## Features

### Core Functionality

✅ **Power-Up Types (5 total)**
- **Damage Boost**: 2x tower damage for 30s
- **Speed Boost**: 1.5x tower fire rate for 30s
- **Credit Boost**: 2x credits earned for 30s
- **Shield**: Protects base from damage for 60s
- **Time Freeze**: Slows all enemies to 10% speed for 30s

✅ **Inventory Management**
- Stack up to 99 of each power-up type
- Persistent storage via PlayerPrefs
- Add/remove power-ups programmatically
- Query current counts and totals

✅ **Activation System**
- One-click activation from inventory
- Active power-ups tracked with timers
- Expiration callbacks for cleanup
- Extend duration if activated while active

✅ **IAP Integration**
- Power-up Bundle purchase (13 power-ups)
- Grants 3x Damage, 3x Speed, 3x Credit, 2x Shield, 2x Time Freeze
- Automatic inventory addition on purchase

✅ **Ad Integration**
- Earn free power-ups from rewarded ads
- Watch ad → Get 1 random power-up
- Integrates with AdManager seamlessly

---

## Architecture

### System Flow

```
┌──────────────┐
│   IAP Shop   │ ──► Buy Power-Up Bundle ($2.99)
│ or Rewarded  │     └─► IAPManager.ProcessPurchase()
│     Ad       │         └─► PowerUpManager.GrantPowerUpBundle()
└──────────────┘             └─► AddPowerUp() x13

         │
         ▼
┌──────────────────┐
│ Power-Up         │
│ Inventory        │ ─────► PlayerPrefs Storage
│ (99 max stack)   │        (persistent across sessions)
└────────┬─────────┘
         │
         │ Player clicks "Activate"
         ▼
┌──────────────────┐
│ PowerUpButton    │ ──► PowerUpManager.ActivatePowerUp()
│ (UI)             │     └─► RemovePowerUp() from inventory
└────────┬─────────┘     └─► ActivatePowerUpEffect()
         │                   └─► ApplyDamageBoost(), etc.
         ▼                   └─► Start expiration timer
┌──────────────────┐
│ Active Power-Ups │ ──► Update() tracks remaining time
│ (Duration Timer) │     └─► OnPowerUpTimeUpdated event
└────────┬─────────┘     └─► UI shows countdown
         │
         │ Timer expires after 30-60s
         ▼
┌──────────────────┐
│ Deactivate       │ ──► DeactivatePowerUpEffect()
│ Effect           │     └─► RemoveDamageBoost(), etc.
└──────────────────┘     └─► OnPowerUpExpired event
```

### Key Components

1. **PowerUpManager** (Singleton)
   - Manages inventory (add/remove/query)
   - Activates power-up effects
   - Tracks active power-ups with timers
   - Applies/removes gameplay effects
   - Integrates with IAP and Ads

2. **PowerUpButton**
   - Individual power-up UI component
   - Shows icon, count, active status
   - Activation button with cooldown fill
   - Timer display for active power-ups

3. **PowerUpPanel**
   - Container for all PowerUpButtons
   - Toggle expand/collapse
   - Total power-up count display
   - Dynamic button creation

---

## Setup

### 1. Add PowerUpManager to Scene

Add to persistent scene (Bootstrap or GameManager):

```csharp
GameObject powerUpObj = new GameObject("PowerUpManager");
powerUpObj.AddComponent<PowerUpManager>();
DontDestroyOnLoad(powerUpObj);
```

**Inspector Configuration:**

**Power-Up Settings:**
- **Enable Power Ups**: ✅ true
- **Default Duration**: 30 (seconds)
- **Max Stack Size**: 99 (per power-up type)

**Effect Multipliers:**
- **Damage Boost Multiplier**: 2.0 (2x damage)
- **Speed Boost Multiplier**: 1.5 (1.5x fire rate)
- **Credit Boost Multiplier**: 2.0 (2x credits)
- **Shield Duration**: 60 (seconds)
- **Time Freeze Slowdown**: 0.1 (10% enemy speed)

### 2. Add PowerUpPanel to HUD

1. Create UI Panel in Canvas
2. Add `PowerUpPanel` component
3. Configure references:
   - Panel (GameObject to show/hide)
   - Power Up Button Container (Vertical/Horizontal Layout Group)
   - Power Up Button Prefab
   - Toggle Button (to expand/collapse)
   - Total Count Text (TextMeshProUGUI)

**Settings:**
- **Start Expanded**: ❌ false (collapsed by default)
- **Show In Gameplay**: ✅ true (visible during game)

### 3. Create PowerUpButton Prefab

1. Create UI Button with:
   - Button component (click to activate)
   - Icon Image (power-up sprite)
   - Count Text (shows inventory count)
   -Name Text (power-up name)
   - Cooldown Fill Image (radial timer)
   - Active Indicator (glow/border when active)
   - Timer Text (countdown in seconds)

2. Add `PowerUpButton` component
3. Assign all UI references
4. Save as prefab in Resources/Prefabs/UI

### 4. Integrate with IAPManager

Power-up bundle already integrated in IAPManager:

```csharp
// IAPManager.cs - ProcessConsumable()
case "powerup_bundle":
    PowerUpManager.Instance?.GrantPowerUpBundle();
    break;
```

No additional code needed!

### 5. Integrate with AdManager

Add rewarded ad option for free power-ups:

```csharp
// ShopUI or Daily Rewards
RewardedAdButton adButton = GetComponent<RewardedAdButton>();
adButton.SetReward(RewardType.PowerUp, 1);

// When ad completes successfully:
PowerUpManager.Instance.GrantFreePowerUp(PowerUpType.DamageBoost);
```

---

## Power-Up Types

### Damage Boost

**Effect:** 2x tower damage
**Duration:** 30 seconds
**Use Case:** Boss waves, tough enemies

**Implementation:**
```csharp
// Activate
PowerUpManager.Instance.ActivatePowerUp(PowerUpType.DamageBoost);

// Check if active
bool isActive = PowerUpManager.Instance.IsDamageBoostActive();

// Apply to towers (in Tower.cs)
if (PowerUpManager.Instance.IsDamageBoostActive())
{
    finalDamage *= PowerUpManager.Instance.GetMultiplier(PowerUpType.DamageBoost);
}
```

---

### Speed Boost

**Effect:** 1.5x tower fire rate
**Duration:** 30 seconds
**Use Case:** Fast enemy waves, swarms

**Implementation:**
```csharp
// Activate
PowerUpManager.Instance.ActivatePowerUp(PowerUpType.SpeedBoost);

// Check if active
bool isActive = PowerUpManager.Instance.IsSpeedBoostActive();

// Apply to towers (in Tower.cs)
if (PowerUpManager.Instance.IsSpeedBoostActive())
{
    fireRate *= PowerUpManager.Instance.GetMultiplier(PowerUpType.SpeedBoost);
}
```

---

### Credit Boost

**Effect:** 2x credits earned
**Duration:** 30 seconds
**Use Case:** Economy waves, farming credits

**Implementation:**
```csharp
// Activate
PowerUpManager.Instance.ActivatePowerUp(PowerUpType.CreditBoost);

// Apply to credit rewards (in Enemy.cs or WaveManager.cs)
int baseCredits = 100;
int finalCredits = PowerUpManager.Instance.ApplyCreditBoost(baseCredits);
// Returns 200 if Credit Boost active, 100 otherwise

GameManager.Instance.AddCredits(finalCredits);
```

---

### Shield

**Effect:** Protects base from damage
**Duration:** 60 seconds  
**Use Case:** Overwhelming waves, emergency protection

**Implementation:**
```csharp
// Activate
PowerUpManager.Instance.ActivatePowerUp(PowerUpType.Shield);

// Check if active (in GameManager.cs when base takes damage)
if (PowerUpManager.Instance.IsShieldActive())
{
    // Block damage, don't reduce health
    Debug.Log("Shield absorbed damage!");
    return;
}

// Normal damage processing
health -= damage;
```

---

### Time Freeze

**Effect:** Slows all enemies to 10% speed
**Duration:** 30 seconds
**Use Case:** Emergencies, need more time to build

**Implementation:**
```csharp
// Activate
PowerUpManager.Instance.ActivatePowerUp(PowerUpType.TimeFreeze);

// Apply to enemies (in Enemy.cs)
if (PowerUpManager.Instance.IsTimeFreezeActive())
{
    moveSpeed *= 0.1f; // 10% normal speed
}
```

---

## Integration

### IAP Shop Integration

Power-up bundle already configured in IAPManager:

**Product:** `powerup_bundle`
**Price:** $2.99
**Contents:** 3x Damage, 3x Speed, 3x Credit, 2x Shield, 2x Time Freeze

```csharp
// In ShopUI, power-up bundle button already exists
// IAPManager processes purchase and calls:
PowerUpManager.Instance.GrantPowerUpBundle();
```

---

### Rewarded Ad Integration

Offer free power-ups for watching ads:

```csharp
// Game Over Screen - Earn power-up instead of continue
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private RewardedAdButton earnPowerUpButton;

    private void Start()
    {
        earnPowerUpButton.SetReward(RewardType.PowerUp, 1);
    }

    // Called after successful ad
    public void OnPowerUpEarned()
    {
        // Grant random power-up
        PowerUpType randomType = (PowerUpType)Random.Range(0, 5);
        PowerUpManager.Instance.GrantFreePowerUp(randomType);
    }
}
```

---

### Tower Integration

Apply power-up effects to towers:

```csharp
public class Tower : MonoBehaviour
{
    private void CalculateDamage()
    {
        float finalDamage = baseDamage;

        // Apply damage boost
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsDamageBoostActive())
        {
            finalDamage *= PowerUpManager.Instance.GetMultiplier(PowerUpType.DamageBoost);
        }

        return finalDamage;
    }

    private void CalculateFireRate()
    {
        float finalFireRate = baseFireRate;

        // Apply speed boost
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsSpeedBoostActive())
        {
            finalFireRate *= PowerUpManager.Instance.GetMultiplier(PowerUpType.SpeedBoost);
        }

        return finalFireRate;
    }
}
```

---

### Enemy Integration

Apply time freeze to enemies:

```csharp
public class Enemy : MonoBehaviour
{
    private void CalculateMoveSpeed()
    {
        float finalSpeed = baseMoveSpeed;

        // Apply time freeze
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsTimeFreezeActive())
        {
            finalSpeed *= 0.1f; // 10% speed
        }

        return finalSpeed;
    }
}
```

---

### Game Manager Integration

Apply shield and credit boost:

```csharp
public class GameManager : MonoBehaviour
{
    public void TakeDamage(int damage)
    {
        // Check shield
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.IsShieldActive())
        {
            Debug.Log("Shield blocked damage!");
            return; // No damage taken
        }

        // Normal damage
        health -= damage;
    }

    public void AddCredits(int amount)
    {
        // Apply credit boost
        if (PowerUpManager.Instance != null)
        {
            amount = PowerUpManager.Instance.ApplyCreditBoost(amount);
        }

        credits += amount;
    }
}
```

---

## Testing

### Editor Testing

**Context Menu Commands:**
```csharp
// Right-click PowerUpManager in Inspector
Add All Power-Ups (x5)    → Adds 5 of each type to inventory
Activate Damage Boost     → Adds 1 and activates Damage Boost
Activate All Power-Ups    → Activates all 5 power-up types
Print Inventory           → Logs inventory and active power-ups
Clear All Power-Ups       → Resets inventory to 0
```

**Manual Testing:**
1. Add power-ups via context menu
2. Enter Play Mode
3. Click power-up buttons in UI
4. Verify effects apply (damage boost, speed boost, etc.)
5. Watch timers count down
6. Verify effects expire after duration

---

### Device Testing

**Test Scenarios:**

#### 1. Purchase Power-Up Bundle

1. Open shop
2. Purchase "powerup_bundle" ($2.99)
3. Verify 13 power-ups added to inventory
4. Check counts: 3 Damage, 3 Speed, 3 Credit, 2 Shield, 2 Time Freeze

**Expected:**
- IAP purchase succeeds
- PowerUpManager.GrantPowerUpBundle() called
- Inventory updated and saved
- Toast notification shown

---

#### 2. Activate Power-Up

1. Ensure power-up in inventory (count > 0)
2. Click power-up button
3. Verify effect active (e.g., towers deal 2x damage)
4. Watch timer count down
5. Verify effect expires after 30s

**Expected:**
- Count decrements by 1
- Active indicator shows
- Timer displays remaining time
- Effect applies to gameplay
- Effect removes on expiration

---

#### 3. Earn Free Power-Up (Rewarded Ad)

1. Click "Watch Ad for Power-Up"
2. Complete rewarded ad
3. Verify power-up added to inventory

**Expected:**
- Ad shows successfully
- PowerUpManager.GrantFreePowerUp() called
- Inventory increments by 1
- Toast notification shown

---

#### 4. Stack Multiple Power-Ups

1. Activate Damage Boost
2. Verify active (30s remaining)
3. Activate another Damage Boost immediately
4. Verify duration extends (60s total)

**Expected:**
- First activation: 30s countdown
- Second activation: Duration extends to 60s
- Inventory decrements by 2 total
- Effect remains active throughout

---

## Best Practices

### 1. Show Clear Value Proposition

```csharp
// ✅ GOOD: Show effect details
nameText.text = "Damage Boost";
descText.text = "2x tower damage for 30s";

// ❌ BAD: Vague description
descText.text = "Boosts damage";
```

### 2. Provide Multiple Acquisition Methods

```csharp
// Earn from:
- IAP purchase (power-up bundle)
- Rewarded ads (watch for free)
- Mission rewards (daily/weekly)
- Achievement unlocks
- Level-up rewards
```

### 3. Balance Power-Up Effects

```csharp
// Avoid making power-ups mandatory
// Power-ups should enhance, not replace strategy

// ✅ GOOD: 2x damage boost
// Makes game easier but winnable without

// ❌ BAD: 100x damage boost
// Game too easy with power-up, impossible without
```

### 4. Track Usage Analytics

```csharp
// Track which power-ups are most popular
Analytics.TrackEvent("powerup_activated", new Dictionary<string, object>
{
    { "type", "DamageBoost" },
    { "wave_number", currentWave },
    { "context", "boss_wave" }
});

// Analyze: Which power-ups need buffs/nerfs?
```

### 5. Save Inventory Persistently

```csharp
// PowerUpManager automatically saves to PlayerPrefs
// But consider cloud save for cross-device sync

CloudSaveManager.Instance.Data.powerUpInventory = inventory;
```

### 6. Visual Feedback for Active Effects

```csharp
// Show visual indicators when power-ups active:
- Tower glow (damage boost)
- Speed lines (speed boost)
- Coin particles (credit boost)
- Shield bubble (shield)
- Purple tint (time freeze)
```

---

## Troubleshooting

### Problem: Power-Up Not Activating

**Symptoms:** Click button, nothing happens

**Solutions:**
1. Check inventory count > 0
2. Verify PowerUpManager instance exists
3. Look for errors in Console
4. Ensure power-up not already active
5. Check `enablePowerUps` setting is true

---

### Problem: Effect Not Applying

**Symptoms:** Power-up active but no gameplay change

**Solutions:**
1. Verify Tower/Enemy integration code added
2. Check `IsDamageBoostActive()` returns true
3. Look for null reference exceptions
4. Ensure GameManager integration complete
5. Test with context menu "Activate All"

---

### Problem: Inventory Not Persisting

**Symptoms:** Power-ups reset on app restart

**Solutions:**
1. Verify PlayerPrefs.Save() called
2. Check platform supports PlayerPrefs
3. Look for PlayerPrefs keys: `PowerUp_DamageBoost`, etc.
4. Clear PlayerPrefs and test fresh install
5. Consider using CloudSaveManager for cross-device sync

---

### Problem: Timer Not Counting Down

**Symptoms:** Active indicator shows but timer frozen

**Solutions:**
1. Check Update() loop running
2. Verify TimeScale not 0 (paused)
3. Ensure expiration coroutine started
4. Look for coroutine stop/interruption
5. Check OnPowerUpTimeUpdated event subscribed

---

## API Reference

### PowerUpManager Public Methods

| Method | Description |
|--------|-------------|
| `AddPowerUp(type, amount)` | Add power-up to inventory |
| `RemovePowerUp(type, amount)` | Remove from inventory |
| `HasPowerUp(type, amount)` | Check if in inventory |
| `GetPowerUpCount(type)` | Get count of specific type |
| `GetTotalPowerUpCount()` | Get all power-ups count |
| `ActivatePowerUp(type)` | Activate from inventory |
| `IsActive(type)` | Check if currently active |
| `GetRemainingTime(type)` | Get seconds remaining |
| `GetDuration(type)` | Get total duration |
| `GetMultiplier(type)` | Get effect multiplier |
| `ApplyCreditBoost(amount)` | Apply credit boost if active |
| `GrantPowerUpBundle()` | Grant IAP bundle (13 power-ups) |
| `GrantFreePowerUp(type)` | Grant from rewarded ad |

### Events

| Event | Parameters | Description |
|-------|------------|-------------|
| `OnInventoryChanged` | type, newCount | Inventory updated |
| `OnPowerUpActivated` | type, duration | Power-up activated |
| `OnPowerUpExpired` | type | Power-up expired |
| `OnPowerUpTimeUpdated` | type, remainingTime | Timer updated |

---

**Built for Robot Tower Defense | Version 1.6**
