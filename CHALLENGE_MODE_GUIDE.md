# Challenge Mode Guide

Complete implementation guide for the Challenge Mode system in Robot Tower Defense.

## 📋 Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Challenge Modifiers](#challenge-modifiers)
- [Creating Challenges](#creating-challenges)
- [UI Setup](#ui-setup)
- [Integration](#integration)
- [Testing](#testing)
- [Best Practices](#best-practices)

---

## Overview

Challenge Mode adds replayability and competitive gameplay by applying special modifiers to existing maps. Players face increased difficulty with unique constraints and earn bonus rewards for completion.

### Features

✅ **20+ Challenge Modifiers** — Speed rush, tower limits, economic hardship, and more  
✅ **Daily/Weekly Rotation** — Fresh challenges that change automatically  
✅ **Permanent Challenges** — Always-available challenges for practice  
✅ **Difficulty Tiers** — Easy, Medium, Hard, Extreme with score multipliers  
✅ **Progress Tracking** — Best scores, completion status, attempt counts  
✅ **Rewards System** — Credits and tech points for first completion  
✅ **Achievement Integration** — Unlock achievements for challenge milestones  
✅ **Analytics Tracking** — Full event tracking for engagement metrics  
✅ **Leaderboard Support** — Separate leaderboards for challenge scores  

### System Components

**Core:**
- `ChallengeData.cs` — ScriptableObject defining challenge configurations
- `ChallengeManager.cs` — Singleton managing challenge lifecycle and modifiers
- `ChallengeModifier` enum — 20+ modifier types

**UI:**
- `ChallengeSelectorUI.cs` — Main browsing/selection panel with tabs
- `ChallengeCardUI.cs` — Individual challenge card display
- `ChallengeResultUI.cs` — Completion results screen

**Integration:**
- `GameManager.cs` — Challenge mode support methods
- `AnalyticsManager.cs` — Challenge event tracking
- `AchievementManager.cs` — Challenge achievement checks

---

## Quick Start

### 5-Minute Setup

**1. Add ChallengeManager to Bootstrap Scene**

```csharp
// In your SceneBootstrapper or GameManager scene setup:
GameObject challengeObj = new GameObject("ChallengeManager");
challengeObj.AddComponent<ChallengeManager>();
```

**2. Create Your First Challenge**

Right-click in Project window:
- **Create → Robot TD → Challenge Data**
- Name it "Challenge_SpeedRush"

Configure in Inspector:
```
Challenge ID: challenge_speed_rush
Challenge Name: Speed Rush
Description: Enemies move 50% faster!
Map ID: map_factory
Modifiers: [SpeedRush]
Difficulty: Medium
Credit Reward: 500
Tech Point Reward: 10
Rotation Type: Daily
```

**3. Register Challenge with Manager**

Select `ChallengeManager` GameObject:
- Expand **Challenge Library** array
- Add your challenge ScriptableObject

**4. Test Challenge**

Play mode → Open Challenge Selector → Click your challenge card

---

## Architecture

### Challenge Data Flow

```
Player Selects Challenge
           ↓
ChallengeManager.StartChallenge(challengeData)
           ↓
Apply Modifiers to Game Systems
           ↓
Load Map Scene
           ↓
Gameplay with Modified Rules
           ↓
Victory/Defeat
           ↓
ChallengeManager.EndChallenge()
           ↓
Award Rewards & Update Progress
```

### Modifier Application

Modifiers are applied in `ChallengeManager.ApplyModifiers()`:

1. **Enemy Modifiers** → Affect WaveManager spawning
2. **Tower Modifiers** → Affect TowerPlacementManager and Tower stats
3. **Economy Modifiers** → Affect GameManager credits/rewards
4. **Wave Modifiers** → Affect WaveManager timing

---

## Challenge Modifiers

### Enemy Modifiers

| Modifier | Effect | Difficulty Impact |
|----------|--------|-------------------|
| **SpeedRush** | Enemies move 50% faster | Medium |
| **ArmoredAssault** | Enemies have 100% more HP | Hard |
| **SwarmMode** | 50% more enemies per wave | Hard |
| **BossRush** | Boss every 5 waves instead of 10 | Medium |
| **RegeneratingFoes** | Enemies slowly regenerate health | Extreme |

### Tower Modifiers

| Modifier | Effect | Difficulty Impact |
|----------|--------|-------------------|
| **LimitedArsenal** | Only 3 random tower types available | Hard |
| **BudgetCrisis** | Towers cost 50% more | Medium |
| **TowerLimit** | Maximum 10 towers on map | Medium |
| **NoUpgrades** | Towers cannot be upgraded | Hard |
| **WeakenedTowers** | Towers deal 30% less damage | Medium |
| **SlowBuild** | 3-second delay before tower activates | Easy |

### Economy Modifiers

| Modifier | Effect | Difficulty Impact |
|----------|--------|-------------------|
| **EconomicHardship** | 50% less credits per kill | Medium |
| **StartingDebt** | Start with 50% normal credits | Easy |
| **ExpensiveUpgrades** | Upgrades cost 100% more | Medium |

### Wave Modifiers

| Modifier | Effect | Difficulty Impact |
|----------|--------|-------------------|
| **FastForward** | Waves auto-start with 3s delay | Easy |
| **NoBreaks** | Zero time between waves | Medium |
| **RandomWaves** | Enemy types randomized each wave | Medium |

### Special Modifiers

| Modifier | Effect | Difficulty Impact |
|----------|--------|-------------------|
| **FogOfWar** | Reduced vision range | Hard |
| **TimeAttack** | Must complete in 15 minutes | Hard |
| **PerfectDefense** | Only 1 life | Extreme |

---

## Creating Challenges

### ScriptableObject Creation

1. **Create Asset:**
   - Right-click in Project → **Create → Robot TD → Challenge Data**

2. **Configure Basic Info:**
```
Challenge ID: unique_identifier (e.g., "challenge_001")
Challenge Name: Display name
Description: Player-facing description (2-4 sentences)
Icon: Optional sprite for UI
```

3. **Set Gameplay Parameters:**
```
Map ID: Which map to use (e.g., "map_factory")
Modifiers: Array of modifiers to apply
Difficulty: Easy / Medium / Hard / Extreme
```

4. **Configure Rewards:**
```
Credit Reward: Credits awarded on first completion
Tech Point Reward: Tech points awarded on first completion
Achievement ID: Optional linked achievement
```

5. **Set Rotation:**
```
Rotation Type: Daily / Weekly / Permanent
Rotation Index: Order in rotation schedule (0-based)
```

### Auto-Generate Example Challenges (Recommended)

For quick setup, use the built-in editor tool to create 13 pre-configured challenges:

**Menu:** `Robot TD → Generate Example Challenges`

This creates:
- **4 Daily Challenges** (Easy-Medium): Speed Rush, Budget Warriors, Rapid Fire, Armored Horde
- **4 Weekly Challenges** (Hard): Limited Arsenal, Economic Crisis, Tower Limit, Weakened Defense
- **5 Permanent Challenges** (Easy-Extreme): Perfect Defense, Speed Master, Minimalist, Boss Rush, Economy Master

All assets are created in `Assets/Resources/Data/Challenges/` and automatically configured with appropriate:
- Modifiers and difficulty tiers
- Reward amounts (300-2200 credits, 5-55 tech points)
- Rotation types and indices
- Achievement IDs for extreme challenges

**Clear All:** Use `Robot TD → Clear All Challenges` to remove generated assets and start fresh.

### Example Challenges

**Easy Challenge - "Starting Line"**
```yaml
ID: challenge_starting_line
Modifiers: [FastForward]
Difficulty: Easy
Map: Training Grounds
Rewards: 200 credits, 5 tech points
Description: Waves start automatically with reduced delay. A gentle introduction to challenge mode.
```

**Medium Challenge - "Speed Rush"**
```yaml
ID: challenge_speed_rush
Modifiers: [SpeedRush, StartingDebt]
Difficulty: Medium
Map: Factory Floor
Rewards: 500 credits, 10 tech points
Description: Enemies move 50% faster and you start with reduced credits. Quick reflexes required!
```

**Hard Challenge - "Limited Resources"**
```yaml
ID: challenge_limited_resources
Modifiers: [LimitedArsenal, TowerLimit, EconomicHardship]
Difficulty: Hard
Map: Circuit City
Rewards: 1000 credits, 20 tech points
Description: Only 3 tower types available, max 10 towers, and reduced income. Plan carefully!
```

**Extreme Challenge - "Perfect Defense"**
```yaml
ID: challenge_perfect_defense
Modifiers: [PerfectDefense, ArmoredAssault, NoUpgrades]
Difficulty: Extreme
Map: Command Center
Rewards: 2500 credits, 50 tech points
Description: One life only. Enemies have double HP. No upgrades. For true masters only.
```

---

## UI Setup

### Challenge Selector Scene

**Hierarchy Structure:**
```
Canvas
├── ChallengeSelectorPanel (ChallengeSelectorUI.cs)
│   ├── HeaderPanel
│   │   ├── TitleText (TMP_Text)
│   │   ├── CloseButton (Button)
│   │   └── RefreshButton (Button)
│   ├── TabPanel
│   │   ├── DailyTabButton (Button)
│   │   ├── WeeklyTabButton (Button)
│   │   └── PermanentTabButton (Button)
│   ├── ContentPanel
│   │   ├── DailyScrollView
│   │   │   └── DailyContainer (for spawn)
│   │   ├── WeeklyScrollView
│   │   │   └── WeeklyContainer (for spawn)
│   │   └── PermanentScrollView
│   │       └── PermanentContainer (for spawn)
│   └── TimerPanel
│       ├── DailyTimerText (TMP_Text)
│       └── WeeklyTimerText (TMP_Text)
```

### Challenge Card Prefab

Create a prefab for individual challenge cards:

```
ChallengeCard (ChallengeCardUI.cs)
├── Background (Image)
├── IconImage (Image)
├── NameText (TMP_Text)
├── DescriptionText (TMP_Text)
├── DifficultyPanel
│   ├── DifficultyText (TMP_Text)
│   └── StarsImage (Image)
├── ModifiersContainer (for modifier icons)
├── RewardsPanel
│   └── RewardsText (TMP_Text)
├── ProgressPanel
│   ├── CompletedBadge (Image)
│   └── BestScoreText (TMP_Text)
└── PlayButton (Button)
```

### In-Game Integration

Add to your Game HUD:

```csharp
// In GameHUD.cs, add challenge info display
if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsChallengeActive)
{
    var challenge = ChallengeManager.Instance.CurrentChallenge;
    challengeNameText.text = challenge.ChallengeName;
    
    // Display active modifiers
    string modifiers = string.Join(", ", challenge.Modifiers);
    modifiersText.text = modifiers;
}
```

---

## Integration

### WaveManager Integration

To fully apply challenge modifiers, extend WaveManager:

```csharp
// In WaveManager.cs, add challenge modifier support
public void SetChallengeModifiers(float healthMult, float speedMult, float countMult)
{
    challengeHealthMultiplier = healthMult;
    challengeSpeedMultiplier = speedMult;
    challengeCountMultiplier = countMult;
}

// In enemy spawning code:
private void SpawnEnemy(EnemyType type)
{
    Enemy enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
    
    // Apply challenge modifiers
    if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsChallengeActive)
    {
        var challenge = ChallengeManager.Instance.CurrentChallenge;
        
        if (challenge.HasModifier(ChallengeModifier.ArmoredAssault))
            enemy.maxHealth *= 2f;
        
        if (challenge.HasModifier(ChallengeModifier.SpeedRush))
            enemy.moveSpeed *= 1.5f;
    }
}
```

### TowerPlacementManager Integration

```csharp
// In TowerPlacementManager.cs
public bool CanPlaceTower(TowerType type)
{
    // Check challenge tower limit
    if (ChallengeManager.Instance != null && !ChallengeManager.Instance.CanPlaceTower())
    {
        ShowError("Tower limit reached!");
        return false;
    }
    
    // Check challenge limited arsenal
    if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsChallengeActive)
    {
        var challenge = ChallengeManager.Instance.CurrentChallenge;
        if (challenge.HasModifier(ChallengeModifier.LimitedArsenal))
        {
            if (!allowedTowerTypes.Contains(type))
            {
                ShowError("This tower type is not available in this challenge!");
                return false;
            }
        }
    }
    
    return true;
}

public int GetTowerCost(TowerType type, int level)
{
    int baseCost = GetBaseCost(type, level);
    
    // Apply challenge cost modifier
    if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsChallengeActive)
    {
        var challenge = ChallengeManager.Instance.CurrentChallenge;
        if (challenge.HasModifier(ChallengeModifier.BudgetCrisis))
        {
            baseCost = Mathf.RoundToInt(baseCost * 1.5f);
        }
    }
    
    return baseCost;
}
```

### Tower.cs Integration

```csharp
// In Tower.cs, check upgrade restrictions
public bool CanUpgrade()
{
    if (currentLevel >= maxLevel)
        return false;
    
    // Check challenge no-upgrade modifier
    if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsChallengeActive)
    {
        var challenge = ChallengeManager.Instance.CurrentChallenge;
        if (challenge.HasModifier(ChallengeModifier.NoUpgrades))
        {
            return false;
        }
    }
    
    return true;
}

public float GetDamage()
{
    float damage = baseDamage * damageMultiplier;
    
    // Apply challenge damage reduction
    if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsChallengeActive)
    {
        var challenge = ChallengeManager.Instance.CurrentChallenge;
        if (challenge.HasModifier(ChallengeModifier.WeakenedTowers))
        {
            damage *= 0.7f;
        }
    }
    
    return damage;
}
```

---

## Testing

### Manual Testing Checklist

**Basic Functionality:**
- [ ] ChallengeManager spawns and initializes
- [ ] Can open Challenge Selector UI
- [ ] Daily/Weekly/Permanent tabs display correctly
- [ ] Challenge cards display with correct info
- [ ] Click challenge card starts the challenge
- [ ] Game loads with challenge active

**Modifier Testing:**
Per modifier type:
- [ ] SpeedRush: Enemies move faster visually
- [ ] TowerLimit: Cannot place beyond 10 towers
- [ ] BudgetCrisis: Tower costs are increased
- [ ] NoUpgrades: Upgrade button is disabled
- [ ] PerfectDefense: Start with 1 life only
- [ ] EconomicHardship: Kill rewards reduced

**Completion Flow:**
- [ ] Victory triggers challenge completion
- [ ] Rewards granted on first completion
- [ ] Best score saved
- [ ] Completion badge appears on card
- [ ] Can replay completed challenges
- [ ] No duplicate rewards on repeat

**Rotation System:**
- [ ] Daily challenges rotate after 24 hours
- [ ] Weekly challenges rotate after 7 days
- [ ] Timer displays correct countdown
- [ ] Rotation persists across sessions

**Analytics:**
- [ ] challenge_started event fires
- [ ] challenge_completed event fires on victory
- [ ] challenge_failed event fires on defeat
- [ ] Parameters logged correctly

### Debug Tools

Add to ChallengeManager for testing:

```csharp
[ContextMenu("Force Daily Rotation")]
private void ForceDailyRotation()
{
    RotateDaily();
    Debug.Log("Daily challenges rotated for testing");
}

[ContextMenu("Complete Current Challenge")]
private void DebugCompleteChallenge()
{
    if (IsChallengeActive)
    {
        CompleteChallenge(CurrentChallenge);
        Debug.Log("Challenge marked complete for testing");
    }
}

[ContextMenu("Reset All Progress")]
private void ResetAllProgress()
{
    progressData.Clear();
    SaveProgress();
    Debug.Log("All challenge progress reset");
}

[ContextMenu("Print Active Challenges")]
private void PrintActiveChallenges()
{
    Debug.Log($"Daily: {activeDailyChallenges.Count}");
    Debug.Log($"Weekly: {activeWeeklyChallenges.Count}");
}
```

### Unit Testing Approach

```csharp
[Test]
public void ChallengeModifier_SpeedRush_IncreasesEnemySpeed()
{
    // Arrange
    var challenge = CreateTestChallenge(ChallengeModifier.SpeedRush);
    var enemy = SpawnTestEnemy();
    float normalSpeed = enemy.moveSpeed;
    
    // Act
    ChallengeManager.Instance.StartChallenge(challenge);
    var modifiedEnemy = SpawnTestEnemy();
    
    // Assert
    Assert.Greater(modifiedEnemy.moveSpeed, normalSpeed);
}

[Test]
public void ChallengeRewards_FirstCompletion_GrantsRewards()
{
    // Arrange
    var challenge = CreateTestChallenge();
    int initialCredits = GameManager.Instance.Credits;
    
    // Act
    ChallengeManager.Instance.StartChallenge(challenge);
    GameManager.Instance.TriggerVictory();
    
    // Assert
    Assert.Greater(GameManager.Instance.Credits, initialCredits);
}
```

---

## Best Practices

### Challenge Design

**Balance Guidelines:**
- **Easy:** 1-2 modifiers, minor impact
- **Medium:** 2-3 modifiers, moderate impact
- **Hard:** 3-4 modifiers, significant challenge
- **Extreme:** 4+ modifiers or single extreme modifier (PerfectDefense)

**Reward Scaling:**
```
Easy:     200-400 credits,   5-10 tech points
Medium:   500-800 credits,  10-20 tech points
Hard:    1000-1500 credits, 20-30 tech points
Extreme: 2000-3000 credits, 40-50 tech points
```

**Avoid Unfair Combinations:**
- ❌ PerfectDefense + ArmoredAssault + SwarmMode (nearly impossible)
- ❌ NoUpgrades + WeakenedTowers + BudgetCrisis (too restrictive)
- ✅ SpeedRush + FastForward (challenging but fair)
- ✅ LimitedArsenal + EconomicHardship (strategic challenge)

### Performance Optimization

**Challenge Card Pooling:**
```csharp
// Instead of Instantiate/Destroy on every refresh:
private ObjectPool<ChallengeCardUI> cardPool;

private void InitializeCardPool()
{
    cardPool = new ObjectPool<ChallengeCardUI>(
        () => Instantiate(challengeCardPrefab).GetComponent<ChallengeCardUI>(),
        card => card.gameObject.SetActive(true),
        card => card.gameObject.SetActive(false),
        card => Destroy(card.gameObject),
        defaultCapacity: 10
    );
}
```

**Lazy Loading:**
```csharp
// Load challenge UI only when opened
public void OpenChallengeSelector()
{
    if (challengeSelectorUI == null)
    {
        // Load from Resources or Addressables
        GameObject prefab = Resources.Load<GameObject>("UI/ChallengeSelectorUI");
        challengeSelectorUI = Instantiate(prefab).GetComponent<ChallengeSelectorUI>();
    }
    
    challengeSelectorUI.Open();
}
```

### UX Recommendations

**Clarity:**
- Show modifier effects clearly (icons + tooltips)
- Display active modifiers during gameplay
- Highlight which stats are affected

**Feedback:**
- Play sound effect on challenge start
- Show visual effect when modifier applies
- Toast notification for milestone achievements

**Accessibility:**
- Color-blind friendly difficulty colors
- Text size options for descriptions
- Screen reader support for UI elements

---

## Advanced Topics

### Custom Modifier Implementation

To add a new modifier:

1. Add to `ChallengeModifier` enum:
```csharp
public enum ChallengeModifier
{
    // ... existing modifiers
    
    MyCustomModifier,  // Your new modifier
}
```

2. Add application logic in `ChallengeManager.ApplyModifiers()`:
```csharp
case ChallengeModifier.MyCustomModifier:
    ApplyMyCustomModifier();
    break;
```

3. Implement the modifier method:
```csharp
private void ApplyMyCustomModifier()
{
    // Your modifier logic
    Debug.Log("[ChallengeManager] Applied MyCustomModifier");
}
```

4. Add icon mapping in `ChallengeCardUI.GetModifierIcon()`:
```csharp
Core.ChallengeModifier.MyCustomModifier => "🔥",
```

### Leaderboard Integration

```csharp
// In ChallengeManager.EndChallenge()
if (victory && Core.Online.LeaderboardManager.Instance != null)
{
    long finalScore = Core.GameManager.Instance?.Score ?? 0;
    float multiplier = challenge.GetDifficultyMultiplier();
    long adjustedScore = Mathf.RoundToInt(finalScore * multiplier);
    
    // Post to challenge-specific leaderboard
    LeaderboardManager.Instance.SubmitScore(
        leaderboardId: $"challenge_{challenge.ChallengeId}",
        score: adjustedScore,
        metadata: new Dictionary<string, object>
        {
            { "wave", WaveManager.Instance?.CurrentWave ?? 0 },
            { "difficulty", challenge.Difficulty.ToString() }
        }
    );
}
```

### Cloud Sync

```csharp
// Save challenge progress to cloud
public async Task SyncChallengeProgress()
{
    var cloudData = new ChallengeSaveData
    {
        progressData = this.progressData,
        lastDailyRotation = this.lastDailyRotation,
        lastWeeklyRotation = this.lastWeeklyRotation
    };
    
    await CloudSaveManager.SaveAsync("challenge_progress", cloudData);
}

// Load from cloud
public async Task LoadChallengeProgress()
{
    var cloudData = await CloudSaveManager.LoadAsync<ChallengeSaveData>("challenge_progress");
    if (cloudData != null)
    {
        this.progressData = cloudData.progressData;
        this.lastDailyRotation = cloudData.lastDailyRotation;
        this.lastWeeklyRotation = cloudData.lastWeeklyRotation;
    }
}
```

---

## Troubleshooting

### Common Issues

**Issue: Modifiers not applying**
- ✓ Check ChallengeManager.IsChallengeActive is true
- ✓ Verify modifier switch case is implemented
- ✓ Ensure challenge data has modifiers assigned

**Issue: Rewards not granted**
- ✓ Check first completion flag in progress data
- ✓ Verify GameManager and SaveManager are available
- ✓ Ensure EndChallenge() is called with victory=true

**Issue: Rotation not working**
- ✓ Check PlayerPrefs for last rotation dates
- ✓ Verify system DateTime access
- ✓ Test force rotation with context menu

**Issue: UI not displaying challenges**
- ✓ Verify challenges assigned to ChallengeManager array
- ✓ Check rotation type matches active tab
- ✓ Ensure challenge card prefab is assigned

---

## Migration Guide

### From Existing Game

If adding to existing tower defense:

1. **Add ChallengeManager** to bootstrap scene
2. **Create challenge data assets** (start with 5-10)
3. **Add UI** scenes and prefabs
4. **Integrate modifiers** into existing systems:
   - GameManager: SetStartingCredits(), SetLives()
   - WaveManager: Apply HP/speed/count multipliers
   - TowerPlacementManager: Check CanPlaceTower()
   - Tower: Check CanUpgrade(), apply damage modifiers
5. **Test thoroughly** with each modifier type
6. **Add to main menu** as new game mode option

---

## Summary

Challenge Mode is now fully implemented with:
- ✅ 20+ modifiers for varied gameplay
- ✅ Daily/weekly/permanent rotation system
- ✅ Full UI for browsing and selection
- ✅ Rewards and achievement integration
- ✅ Analytics tracking
- ✅ Progress persistence

**Next Steps:**
1. Create 10-15 challenge ScriptableObjects
2. Build UI prefabs to match your game's art style
3. Integrate modifiers into core gameplay systems
4. Test challenge completion flow end-to-end
5. Balance rewards based on player feedback

For questions or issues, refer to the testing section or check debug logs with `enableDebugLogs = true` on ChallengeManager.
