# Daily Missions System Guide

Complete implementation guide for the Daily Missions system in Robot Tower Defense.

## 📋 Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Mission Types](#mission-types)
- [Creating Missions](#creating-missions)
- [UI Setup](#ui-setup)
- [Integration](#integration)
- [Testing](#testing)
- [Best Practices](#best-practices)

---

## Overview

Daily Missions provide daily engagement through 3 rotating objectives that reward players with credits and tech points. Players complete missions through normal gameplay, creating natural progression incentives.

### Features

✅ **3 Daily Missions** — Rotate every 24 hours automatically  
✅ **20+ Mission Types** — Combat, economy, tower, wave, and special objectives  
✅ **Difficulty Tiers** — Easy, Medium, Hard with scaled rewards  
✅ **Auto Progress Tracking** — Missions track automatically during gameplay  
✅ **Persistent Progress** — Tracks across sessions with PlayerPrefs  
✅ **Weighted Rotation** — Mission probability based on rotation weight  
✅ **Level Gating** — Unlock harder missions as player progresses  
✅ **Analytics Integration** — Full event tracking for engagement metrics  
✅ **Reward System** — Credits and tech points on completion  

### System Components

**Core:**
- `MissionData.cs` — ScriptableObject defining mission configurations
- `MissionManager.cs` — Singleton managing mission lifecycle and rotation
- `MissionType` enum — 20+ mission type definitions

**UI:**
- `DailyMissionsUI.cs` — Main panel with 3 mission cards and timer
- `MissionCardUI.cs` — Individual mission card display

**Integration:**
- `AnalyticsEvents.cs` — Mission event tracking constants
- `GameManager.cs` — Mission progress hooks for economy
- `TowerPlacementManager.cs` — Tower placement/upgrade tracking
- `WaveManager.cs` — Wave completion tracking

---

## Quick Start

### 5-Minute Setup

**1. Add MissionManager to Bootstrap Scene**

```csharp
// In your SceneBootstrapper or persistent scene:
GameObject missionObj = new GameObject("MissionManager");
missionObj.AddComponent<MissionManager>();
```

**2. Create Your First Mission**

Right-click in Project window:
- **Create → Robot TD → Mission Data**
- Name it "Mission_KillEnemies"

Configure in Inspector:
```
Mission ID: mission_kill_50
Mission Name: Robot Slayer
Description: Defeat 50 enemies
Mission Type: KillEnemies
Difficulty: Easy
Target Value: 50
Credit Reward: 200
Tech Point Reward: 5
Rotation Weight: 10
Minimum Player Level: 1
```

**3. Register Missions with Manager**

Select `MissionManager` GameObject:
- Expand **Available Missions** array
- Add your mission ScriptableObject(s)
- Set **Missions Per Day**: 3

**4. Test Missions**

Play mode → MissionManager auto-rotates 3 missions → Play game → Missions track automatically

---

## Architecture

### Mission Lifecycle Flow

```
Game Launch
     ↓
MissionManager.Awake()
     ↓
Load Saved Progress
     ↓
Check If Day Changed → Rotate Missions (if needed)
     ↓
Player Plays Game
     ↓
Gameplay Events → UpdateMissionProgress()
     ↓
Mission Complete → Trigger Completion Event
     ↓
Player Claims Reward → Award Credits/Tech Points
     ↓
Next Day → Auto-Rotate Missions
```

### Progress Tracking System

Missions automatically track progress through game event hooks:

1. **Combat Events** → `WaveManager.OnEnemyKilled`
2. **Tower Events** → `TowerPlacementManager.OnTowerPlaced`, `Tower.Upgrade()`
3. **Economy Events** → `GameManager.AddCredits()`, `GameManager.SpendCredits()`
4. **Wave Events** → `WaveManager.OnWaveCompleted`
5. **Victory Events** → `GameManager.TriggerVictory()`

---

## Mission Types

### Combat Missions

| Type | Description | Example Target | Difficulty |
|------|-------------|----------------|------------|
| **KillEnemies** | Kill X enemies total | 50 enemies | Easy |
| **KillEnemiesWithTower** | Kill X enemies using specific tower | 30 with Laser Turret | Medium |
| **KillBosses** | Kill X boss enemies | 2 bosses | Hard |
| **DealDamage** | Deal X total damage | 10,000 damage | Medium |

### Tower Missions

| Type | Description | Example Target | Difficulty |
|------|-------------|----------------|------------|
| **PlaceTowers** | Place X towers in a match | 10 towers | Easy |
| **UpgradeTowers** | Upgrade towers X times | 5 upgrades | Easy |
| **UseToower** | Place X of specific tower type | 3 Sniper Bots | Medium |
| **MaxUpgradeTower** | Fully upgrade X towers | 2 max level towers | Hard |

### Economy Missions

| Type | Description | Example Target | Difficulty |
|------|-------------|----------------|------------|
| **EarnCredits** | Earn X credits in single match | 2000 credits | Medium |
| **SpendCredits** | Spend X credits in single match | 1500 credits | Easy |
| **EndWithCredits** | End match with X+ credits remaining | 500+ credits | Medium |

### Wave Missions

| Type | Description | Example Target | Difficulty |
|------|-------------|----------------|------------|
| **CompleteWaves** | Complete X waves (any game) | 15 waves | Easy |
| **CompleteWavesFlawless** | Complete X waves without losing life | 10 flawless waves | Hard |
| **SurviveWave** | Survive to wave X | Wave 25 | Medium |

### Map Missions

| Type | Description | Example Target | Difficulty |
|------|-------------|----------------|------------|
| **CompleteMap** | Complete specific map | "Factory Floor" | Medium |
| **CompleteAnyMap** | Complete any map | Any map | Easy |
| **WinWithoutLosingLife** | Complete map with all lives | Full health victory | Hard |

### Special Missions

| Type | Description | Example Target | Difficulty |
|------|-------------|----------------|------------|
| **CompleteChallenge** | Complete any challenge mode | 1 challenge | Hard |
| **UseOnlyTowerTypes** | Win using only X tower types | 3 types max | Hard |
| **WinWithTowerLimit** | Win with X or fewer towers placed | 12 towers max | Medium |
| **SpeedRun** | Complete map in under X minutes | 10 minutes | Hard |

---

## Creating Missions

### ScriptableObject Creation

1. **Create Asset:**
   - Right-click in Project → **Create → Robot TD → Mission Data**

2. **Configure Basic Info:**
```
Mission ID: unique_identifier (e.g., "mission_001")
Mission Name: Display name (e.g., "Tower Master")
Description: Player-facing text (use {target} and {parameter} placeholders)
Icon: Optional sprite for UI
```

3. **Set Mission Type and Target:**
```
Mission Type: Select from enum (KillEnemies, PlaceTowers, etc.)
Target Value: Progress goal (e.g., 50 for "kill 50 enemies")
Target Parameter: Optional parameter (e.g., "LaserTurret" for tower-specific)
```

4. **Configure Difficulty and Rewards:**
```
Difficulty: Easy / Medium / Hard
Credit Reward: Credits awarded on completion (100-500)
Tech Point Reward: Tech points awarded (5-25)
```

5. **Set Rotation Properties:**
```
Rotation Weight: Probability (1-20, default: 10, higher = more likely)
Minimum Player Level: Unlock requirement (1-50)
```

### Example Mission Configurations

**Easy Combat Mission**
```yaml
ID: mission_kill_30
Name: Enemy Eliminator
Description: Defeat {target} enemies
Type: KillEnemies
Target: 30
Difficulty: Easy
Rewards: 150 credits, 5 tech points
Weight: 15 (common)
Min Level: 1
```

**Medium Tower Mission**
```yaml
ID: mission_upgrade_5
Name: Tower Enhancement
Description: Upgrade towers {target} times
Type: UpgradeTowers
Target: 5
Difficulty: Medium
Rewards: 250 credits, 10 tech points
Weight: 10 (normal)
Min Level: 5
```

**Hard Economy Mission**
```yaml
ID: mission_earn_3000
Name: Credit Collector
Description: Earn {target} credits in one game
Type: EarnCredits
Target: 3000
Difficulty: Hard
Rewards: 400 credits, 20 tech points
Weight: 5 (rare)
Min Level: 10
```

**Special Challenge Mission**
```yaml
ID: mission_flawless_10
Name: Perfect Defense
Description: Complete {target} waves without losing a life
Type: CompleteWavesFlawless
Target: 10
Difficulty: Hard
Rewards: 500 credits, 25 tech points
Weight: 3 (very rare)
Min Level: 15
```

### Mission Balancing Guidelines

**Easy Missions (Difficulty 1):**
- Target: Achievable in 1-2 normal games
- Rewards: 100-200 credits, 5-8 tech points
- Weight: 15-20 (common rotation)
- Examples: Kill 30 enemies, Place 8 towers

**Medium Missions (Difficulty 2):**
- Target: Requires 2-3 focused games
- Rewards: 200-350 credits, 10-15 tech points
- Weight: 8-12 (normal rotation)
- Examples: Upgrade 5 towers, Complete 15 waves

**Hard Missions (Difficulty 3):**
- Target: Requires skill or multiple attempts
- Rewards: 350-500 credits, 20-30 tech points
- Weight: 3-7 (rare rotation)
- Examples: Win without losing life, Complete challenge

---

## UI Setup

### DailyMissionsUI Component

**Required GameObject Hierarchy:**
```
DailyMissionsPanel (Canvas)
├── DailyMissionsUI (Component)
├── Background (Image)
├── Header
│   ├── Title Text
│   ├── Timer Text
│   └── Completion Text
├── MissionCardContainer (Vertical Layout Group)
│   └── (Cards spawn here)
├── CloseButton
└── RefreshButton (Editor only)
```

**Inspector Configuration:**
- **Panel**: Reference to root GameObject
- **Mission Card Container**: Transform for card spawning
- **Mission Card Prefab**: MissionCardUI prefab reference
- **Timer Text**: Countdown to next rotation
- **Completion Text**: "X/3 completed" display
- **Card Spawn Delay**: 0.1s for staggered animation

### MissionCardUI Prefab

**Required Components:**
```
MissionCard (GameObject)
├── MissionCardUI (Component)
├── Background (Image - colored by difficulty)
├── Icon (Image)
├── Title (TextMeshProUGUI)
├── Description (TextMeshProUGUI)
├── ProgressBar (Slider)
├── ProgressText (Text - "25/50")
├── RewardText (Text - "$ 200  ⚡ 5")
├── ClaimButton
├── CompletedBadge (GameObject - optional)
└── DifficultyStars (3 star GameObjects)
```

**Difficulty Colors:**
- Easy: Green (0.2, 0.8, 0.2)
- Medium: Orange (0.8, 0.6, 0.2)
- Hard: Red (0.8, 0.2, 0.2)

---

## Integration

### Automatic Progress Tracking

Most mission types track automatically through existing game events:

**Already Integrated:**
- ✅ KillEnemies - Tracks via `WaveManager.OnEnemyKilled`
- ✅ Kill Bosses - Tracks via enemy.IsBoss check
- ✅ PlaceTowers - Tracks via `TowerPlacementManager.PlaceTower()`
- ✅ UpgradeTowers - Tracks via `Tower.Upgrade()`
- ✅ EarnCredits - Tracks via `GameManager.AddCredits()`
- ✅ SpendCredits - Tracks via `GameManager.SpendCredits()`
- ✅ CompleteWaves - Tracks via `WaveManager.OnWaveCompleted`
- ✅ CompleteAnyMap - Tracks via `GameManager.TriggerVictory()`
- ✅ WinWithoutLosingLife - Checks lives on victory
- ✅ EndWithCredits - Checks credits on victory

### Manual Progress Tracking

For mission types requiring custom logic, call:

```csharp
if (MissionManager.Instance != null)
{
    MissionManager.Instance.UpdateMissionProgress(
        MissionType.YourMissionType,
        amount: 1,
        parameter: "OptionalParameter"
    );
}
```

**Examples:**

```csharp
// Track damage dealt
MissionManager.Instance.UpdateMissionProgress(MissionType.DealDamage, damageAmount);

// Track specific tower usage
MissionManager.Instance.UpdateMissionProgress(
    MissionType.KillEnemiesWithTower, 
    1, 
    "LaserTurret"
);

// Track challenge completion
MissionManager.Instance.UpdateMissionProgress(MissionType.CompleteChallenge, 1);
```

### Opening Missions UI

From Main Menu or in-game:

```csharp
// Get reference
DailyMissionsUI missionsUI = DailyMissionsUI.Instance;

// Open panel
if (missionsUI != null)
{
    missionsUI.Open();
}

// Or toggle
missionsUI?.Toggle();
```

---

## Testing

### Test Checklist

1. **Mission Rotation**
   - ✅ 3 missions assigned on first launch
   - ✅ Missions rotate after 24 hours
   - ✅ Timer displays correct time until rotation
   - ✅ No duplicate missions in one rotation

2. **Progress Tracking**
   - ✅ KillEnemies increments on enemy death
   - ✅ PlaceTowers increments on tower placement
   - ✅ UpgradeTowers increments on upgrade
   - ✅ EarnCredits accumulates correctly
   - ✅ CompleteWaves tracks across games

3. **UI Display**
   - ✅ Mission cards spawn with animations
   - ✅ Progress bar updates in real-time
   - ✅ Difficulty stars show correctly
   - ✅ Claim button enables on completion
   - ✅ Completion badge appears when done

4. **Reward System**
   - ✅ Claim button awards credits/tech points
   - ✅ Button disables after claiming
   - ✅ Rewards tracked in SaveManager
   - ✅ Toast notification shows on claim

5. **Persistence**
   - ✅ Progress saves across sessions
   - ✅ Completed missions stay completed
   - ✅ Rotation state persists

### Debug Commands (Editor Only)

MissionManager Context Menu options:
- **Force Rotate Missions** — Immediately rotate to new missions
- **Complete All Current Missions** — Instant complete for testing
- **Reset All Progress** — Clear saved data

### Testing Best Practices

1. **Test mission types individually** — Verify each type tracks correctly
2. **Test across game sessions** — Confirm persistence works
3. **Test with multiple missions** — Ensure no conflicts
4. **Test edge cases** — 0 progress, exactly at target, over target
5. **Test rotation timing** — Use system clock to verify 24h rotation

---

##  Best Practices

### Mission Design

**DO:**
- ✅ Balance difficulty across all 3 daily missions (1 easy, 1 medium, 1 hard)
- ✅ Vary mission types each rotation (combat + tower + economy)
- ✅ Use achievable targets for easy missions
- ✅ Make hard missions challenging but fair
- ✅ Test missions with real gameplay before deploying

**DON'T:**
- ❌ Create missions requiring 10+ hours to complete
- ❌ Make all 3 missions hard difficulty
- ❌ Use mission types that don't track automatically without implementing tracking
- ❌ Set rotation weight too low (<3) or too high (>20)
- ❌ Forget to set appropriate minimum player level

### Reward Balancing

**Credits:**
- Easy: 100-200
- Medium: 200-350
- Hard: 350-500

**Tech Points:**
- Easy: 5-8
- Medium: 10-15
- Hard: 20-30

**Total Daily Rewards (3 missions):**
- Target: ~600-900 credits, 30-50 tech points

### Performance Tips

- Mission progress updates are lightweight (< 0.1ms)
- Rotation happens once per day (negligible impact)
- Use PlayerPrefs for persistence (fast, simple)
- Progress tracking piggybacks on existing events (no overhead)

### Analytics Insights

Track these metrics for mission engagement:
- **Completion Rate** — % of players completing all 3 daily missions
- **Mission Type Popularity** — Which types get completed most
- **Avg Time to Complete** — How long missions take
- **Return Rate** — Daily active users after mission launch
- **Reward Claim Rate** — % of completed missions claimed

---

## Advanced Features

### Custom Mission Types

To add new mission types:

1. Add to `MissionType` enum in `MissionData.cs`
2. Implement tracking logic in appropriate gameplay system
3. Call `MissionManager.UpdateMissionProgress()` with new type
4. Test thoroughly before deploying

Example:
```csharp
// In your gameplay code
if (MissionManager.Instance != null)
{
    MissionManager.Instance.UpdateMissionProgress(
        MissionType.YourNewType,
        progressAmount,
        optionalParameter
    );
}
```

### Multi-Target Missions

For missions requiring multiple conditions:

```csharp
// Check multiple parameters
if (MissionManager.Instance != null)
{
    var mission = MissionManager.Instance.GetMissionData("mission_id");
    
    if (mission != null && 
        mission.Type == MissionType.UseOnlyTowerTypes &&
        IsTowerTypeAllowed(towerType))
    {
        // Proceed with mission
    }
}
```

### Weekly Missions (Future Enhancement)

To implement weekly missions:
1. Add `MissionRotationType` enum (Daily, Weekly)
2. Modify `DailyMissionSet` to support weekly rotation
3. Add separate UI tab for weekly missions
4. Increase target values and rewards for weekly

---

## Troubleshooting

### Missions not tracking

**Issue:** Mission progress not updating during gameplay

**Solutions:**
- Verify `MissionManager` exists in scene
- Check game events are firing (OnEnemyKilled, OnTowerPlaced, etc.)
- Ensure mission type matches the gameplay action
- Check target parameter if mission uses one

### Missions not rotating

**Issue:** Same 3 missions appear every day

**Solutions:**
- Check system time is advancing
- Verify PlayerPrefs not locked/corrupted
- Ensure enough missions in library (need 3+ total)
- Check rotation weight > 0 on missions

### Rewards not granted

**Issue:** Claim button doesn't award rewards

**Solutions:**
- Verify `SaveManager.Instance` exists
- Check rewards configured in mission ScriptableObject
- Ensure claim button calls `MissionManager.ClaimReward()`
- Check console for error messages

### UI not updating

**Issue:** Progress bar/text not refreshing

**Solutions:**
- Verify `OnMissionProgressUpdated` event subscribed
- Check `MissionCardUI.UpdateProgress()` being called
- Ensure progress value passed correctly to UI
- Verify TextMeshProUGUI components assigned

---

## Summary

Daily Missions provide:
- 📅 **Daily Engagement** — New missions every 24 hours
- 🎯 **Clear Objectives** — 20+ mission types with specific goals
- 🏆 **Rewarding Progression** — Credits and tech points for completion
- 📊 **Analytics Tracking** — Full engagement metrics
- 🔄 **Automatic Rotation** — No manual intervention needed
- 💾 **Persistent Progress** — Tracks across sessions
- 🎨 **Polished UI** — Animated cards with difficulty indicators

**Development Time:** ~1-2 days  
**Files Created:** 5 C# scripts, mission ScriptableObjects  
**Integration Points:** 6 existing systems  
**Testing Time:** 2-4 hours  

---

**Last Updated:** March 12, 2026  
**Version:** 1.0.0  
**Status:** Production Ready
