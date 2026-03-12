# Endless Mode Setup Guide

This guide covers the implementation and Unity Editor setup for the Endless Mode system.

## 📋 Overview

The Endless Mode system automatically activates after completing any campaign map, generating infinitely scaling waves with progressive difficulty. The system includes:

- **Core Logic**: EndlessMode.cs (auto-activation, wave generation, scoring)
- **UI Integration**: GameHUD enhancements, Main Menu panel, info display
- **Progression**: SaveManager tracking, leaderboard submission
- **Balance**: Configurable scaling parameters for difficulty curves

---

## ⚙️ Core System (Already Complete)

### EndlessMode.cs
Located at: `Assets/Scripts/Core/EndlessMode.cs`

**Status**: ✅ Fully implemented (195 lines)

The core system handles:
- Automatic activation when WaveManager completes all designed waves
- Infinite wave generation with scaling difficulty
- Milestone rewards every 5 waves
- Score tracking and leaderboard submission
- SaveManager integration for personal bests

**Key Configuration Parameters** (Inspector):
```
[Header("Scaling")]
healthScalePerWave = 0.25       // +25% HP each wave
speedScalePerWave = 0.05        // +5% speed each wave
spawnRateScalePerWave = 0.08    // +8% spawn rate each wave
baseEnemiesPerWave = 10         // Starting enemy count
enemiesIncreasePerWave = 3      // Additional enemies per wave

[Header("Milestones")]
milestoneInterval = 5           // Bonus every N waves
milestoneBaseCreditBonus = 200  // Base bonus amount

[Header("Timings")]
secondsBetweenWaves = 8f        // Delay between endless waves
```

**Events** (Static):
```csharp
EndlessMode.OnEndlessWaveStarted += (int wave) => { };
EndlessMode.OnMilestoneReached += (int wave, long bonus) => { };
```

---

## 🎮 UI Integration (NEW)

### 1. GameHUD Enhancements

**File**: `Assets/Scripts/UI/GameHUD.cs`

**Changes Made**:
- Subscribes to EndlessMode events in Start()
- Unsubscribes in OnDestroy()
- Displays gold-colored "Endless Wave X" text during endless mode
- Shows "Endless Mode Activated!" toast on wave 1
- Displays milestone notifications with bonus amounts

**No Inspector Changes Required** - The existing waveText field is reused.

---

### 2. EndlessModeUI Panel (Main Menu)

**File**: `Assets/Scripts/UI/EndlessModeUI.cs` (NEW - 140 lines)

This panel displays endless mode information in the main menu:
- Description and how-to-play guide
- Personal best stats (highest wave, highest score)
- Scaling information
- Milestone rewards breakdown
- Leaderboard access button

#### Unity Setup Steps:

1. **Create Panel GameObject**
   - In Main Menu scene, under Canvas
   - Right-click Canvas → UI → Panel
   - Name: "EndlessModePanel"
   - Rect Transform: Stretch to full screen
   - Set Active: False (panel starts hidden)

2. **Add EndlessModeUI Component**
   - Select EndlessModePanel
   - Add Component → EndlessModeUI

3. **Create UI Layout** (Example structure):

   ```
   EndlessModePanel
   ├── BackgroundImage (Panel background)
   ├── TitleText (TextMeshProUGUI) - "Endless Mode"
   ├── DescriptionText (TextMeshProUGUI) - Main description
   ├── StatsContainer (Vertical Layout Group)
   │   ├── HighScoreText (TextMeshProUGUI)
   │   └── BestWaveText (TextMeshProUGUI)
   ├── InfoContainer (Horizontal Layout Group)
   │   ├── ScalingInfoText (TextMeshProUGUI)
   │   └── MilestonesInfoText (TextMeshProUGUI)
   ├── ViewLeaderboardButton (Button)
   │   └── Text - "View Leaderboard"
   └── BackButton (Button)
       └── Text - "Back"
   ```

4. **Assign Inspector References** on EndlessModeUI:
   - **Title Text**: Drag TitleText object
   - **Description Text**: Drag DescriptionText object
   - **High Score Text**: Drag HighScoreText object
   - **Best Wave Text**: Drag BestWaveText object
   - **View Leaderboard Button**: Drag ViewLeaderboardButton
   - **Back Button**: Drag BackButton
   - **Scaling Info Text**: Drag ScalingInfoText
   - **Milestones Info Text**: Drag MilestonesInfoText
   - **Leaderboard Panel**: (Optional) Drag LeaderboardUI panel if in scene

5. **Text Styling Recommendations**:
   - **Title**: Font size 48-60, bold, centered
   - **Description**: Font size 18-22, left-aligned, word wrap enabled
   - **Stats**: Font size 20-24, bold, colored text (gold for scores)
   - **Info Boxes**: Font size 16-18, use RichText for colors
   - **Colors**: Use `<color=#FFD700>` for highlights (gold)

---

### 3. MainMenuUI Integration

**File**: `Assets/Scripts/UI/MainMenuUI.cs`

**Changes Made**:
- Added `endlessModePanel` SerializeField
- Added `endlessModeButton` SerializeField
- Button click listener opens EndlessModePanel
- Panel visibility managed in ShowPanel() method

#### Unity Setup Steps:

1. **Add Endless Mode Button** (Main Panel):
   - Select Main Menu → MainPanel
   - Create a Button (duplicate existing Play button for consistency)
   - Name: "EndlessModeButton"
   - Position: Between Play and Tech Tree buttons
   - Button Text: "Endless Mode" or "♾️ Endless"
   - Icon (optional): Infinity symbol or wave icon

2. **Assign Inspector References** on MainMenuUI:
   - **Endless Mode Button**: Drag EndlessModeButton
   - **Endless Mode Panel**: Drag EndlessModePanel

3. **Styling**:
   - Match existing button style for visual consistency
   - Consider gold/orange color to distinguish from campaign play
   - Add hover/press animations if other buttons have them

---

## 💾 SaveManager Integration

**File**: `Assets/Scripts/Core/SaveManager.cs`

**Changes Made** (PlayerSaveData):
```csharp
// Endless mode
public int endlessHighWave = 0;
public long endlessHighScore = 0;
public int endlessGamesPlayed = 0;
```

**No Inspector Changes Required** - These are data fields.

**EndlessMode.cs** saves to these fields automatically in `PostEndlessScore()`.

---

## 🔊 Audio Integration

The system uses existing AudioManager SFX:
- **Milestone Reached**: `Audio.SFX.UISuccess`
- **Endless Activated**: Toast notification sound (automatic)

**No additional audio setup required** - existing sounds are reused.

---

## 📊 Leaderboard Integration

The system integrates with the existing LeaderboardManager:

**EndlessMode.cs** calls:
```csharp
LeaderboardManager.Instance?.SubmitEndlessScore(EndlessWaveNumber, totalScore);
```

**Leaderboard ID**: `"endless_high_score"` (already exists in LeaderboardUI)

**No additional setup required** if leaderboard system is already configured.

---

## ⚖️ Difficulty Balancing

### Scaling Calculations

**Wave 1**: Baseline (100% HP, 100% speed, 10 enemies)
**Wave 10**:
- HP: 100% + (10 × 25%) = **350% HP**
- Speed: 100% + (10 × 5%) = **150% speed**
- Enemies: 10 + (10 × 3) = **40 enemies**

**Wave 20**:
- HP: **600% HP**
- Speed: **200% speed**
- Enemies: **70 enemies**

**Wave 50**:
- HP: **1350% HP**
- Speed: **350% speed**
- Enemies: **160 enemies**

### Milestone Rewards

Formula: `milestoneBaseCreditBonus * (wave / milestoneInterval)`

| Wave | Milestone | Credits Bonus |
|------|-----------|---------------|
| 5    | 1st       | 200           |
| 10   | 2nd       | 400           |
| 15   | 3rd       | 600           |
| 20   | 4th       | 800           |
| 50   | 10th      | 2000          |

### Tuning Recommendations

**If Too Easy**:
- Increase `healthScalePerWave` to 0.30 (+30% HP)
- Increase `speedScalePerWave` to 0.07 (+7% speed)
- Decrease `milestoneBaseCreditBonus` to 150

**If Too Hard**:
- Decrease `healthScalePerWave` to 0.20 (+20% HP)
- Decrease `speedScalePerWave` to 0.03 (+3% speed)
- Increase `milestoneBaseCreditBonus` to 250
- Decrease `enemiesIncreasePerWave` to 2

**For Faster Gameplay**:
- Decrease `secondsBetweenWaves` to 5-6 seconds

**For Strategic Depth**:
- Increase `secondsBetweenWaves` to 10-12 seconds
- Gives more time to plan tower placement

---

## 🧪 Testing Procedures

### 1. Quick Test (Activation)
1. Launch any campaign map
2. Complete all designed waves
3. ✅ Verify: "Endless Mode Activated!" toast appears
4. ✅ Verify: Wave text changes to gold "Endless Wave 1"
5. ✅ Verify: Enemies spawn automatically after delay

### 2. Scaling Test
1. Reach Endless Wave 5
2. Note enemy health/speed
3. Reach Endless Wave 10
4. ✅ Verify: Enemies noticeably tougher (more HP, faster)
5. ✅ Verify: More enemies per wave

### 3. Milestone Test
1. Survive to Wave 5
2. ✅ Verify: Milestone notification appears
3. ✅ Verify: Credits awarded
4. ✅ Verify: Success sound plays

### 4. UI Test (Main Menu)
1. Open Main Menu
2. Click "Endless Mode" button
3. ✅ Verify: EndlessModeUI panel opens
4. ✅ Verify: Personal best stats display
5. ✅ Verify: "View Leaderboard" button works
6. ✅ Verify: "Back" button closes panel

### 5. Save Test
1. Play endless mode, reach wave 10
2. Game Over
3. ✅ Verify: High wave saved (check EndlessModeUI panel)
4. Play again, reach wave 15
5. ✅ Verify: New high wave saved (15 > 10)

### 6. Leaderboard Test
1. Complete an endless run
2. Game Over
3. ✅ Verify: Score submitted to leaderboard
4. Open Leaderboards
5. ✅ Verify: "Endless" tab visible
6. ✅ Verify: Your score appears

---

## 🐛 Troubleshooting

### Issue: Endless Mode Doesn't Activate

**Symptoms**: After completing campaign, nothing happens

**Solutions**:
1. Verify EndlessMode.cs component exists in scene
   - Should be on a persistent GameObject (don't destroy on load)
   - Or attached to GameManager
2. Check WaveManager events subscription
   - EndlessMode subscribes to `WaveManager.OnAllWavesCompleted`
   - Set breakpoint in `HandleNormalModeDone()` to verify
3. Ensure WaveManager has `OnAllWavesCompleted` event firing

---

### Issue: Wave Text Not Updating

**Symptoms**: Wave text still shows "Wave X" instead of "Endless Wave X"

**Solutions**:
1. Verify GameHUD.cs has OnEndlessWaveStarted subscription
   - Check Start() method for `EndlessMode.OnEndlessWaveStarted +=`
2. Check event is actually firing
   - Set breakpoint in GameHUD.OnEndlessWaveStarted()
3. Ensure EndlessMode raises event
   - Check EndlessLoop() coroutine calls `OnEndlessWaveStarted?.Invoke()`

---

### Issue: Milestones Not Triggering

**Symptoms**: Wave 5, 10, 15... pass but no milestone notification

**Solutions**:
1. Check milestone interval setting
   - Default: `milestoneInterval = 5` (every 5 waves)
2. Verify event subscription in GameHUD
   - `EndlessMode.OnMilestoneReached +=`
3. Ensure ToastNotification.Instance exists
   - Should be in scene and accessible

---

### Issue: Personal Bests Not Saving

**Symptoms**: EndlessModeUI shows 0 or "Not Yet Played" after completing run

**Solutions**:
1. Verify PostEndlessScore() is called
   - Called on game over/victory when IsActive
   - Set breakpoint in EndlessMode.PostEndlessScore()
2. Check SaveManager.Instance exists
   - SaveManager must be present in scene
3. Verify SaveManager.Save() is called
   - Check SaveManager.cs Data property has new fields

---

### Issue: Endless Mode Button Not Working

**Symptoms**: Main menu button doesn't open panel

**Solutions**:
1. Verify Inspector references on MainMenuUI
   - endlessModeButton assigned
   - endlessModePanel assigned
2. Check button listener setup
   - Start() method should add click listener
3. Ensure EndlessModePanel GameObject exists
   - Should be child of Canvas
   - Initially inactive

---

### Issue: Scaling Too Extreme

**Symptoms**: Wave 10+ impossible to survive, or Wave 50+ too easy

**Solutions**:
1. Adjust scaling parameters in EndlessMode Inspector
   - Reduce/increase healthScalePerWave
   - Reduce/increase speedScalePerWave
   - Modify enemiesIncreasePerWave
2. Balance milestone credit bonuses
   - More credits = easier to build defenses
3. Test with different tower compositions
   - Ensure all tower types remain viable at higher waves

---

## 📈 Advanced Features (Future Enhancements)

### Endless Mode Modifiers
- Add starting difficulty selection (Easy/Normal/Hard)
- Challenge Mode integration (endless + challenge modifiers)
- Special endless-only enemies at high waves

### Enhanced UI
- Real-time wave timer countdown
- Enemy composition preview (next wave)
- Current wave difficulty rating display
- Personal milestone history viewer

### Achievement Integration
- "Reach Wave 25" achievement
- "Reach Wave 50" achievement
- "Milestone Master" - complete 10 milestones
- "Endless Champion" - top 100 on leaderboard

### New Scaling Options
- Boss waves every 10 waves
- Elite enemies starting at wave 20
- Special rewards for wave 25, 50, 75, 100

---

## 📝 Summary Checklist

### Core System
- [x] EndlessMode.cs implemented (auto-activation, scaling, milestones)
- [x] WaveManager integration (SetEndlessMode, SpawnEndlessWave)
- [x] SaveManager fields (endlessHighWave, endlessHighScore)
- [x] Leaderboard submission (SubmitEndlessScore)

### UI Integration
- [x] GameHUD endless wave display (gold text, toasts, milestones)
- [x] EndlessModeUI panel script (info, stats, leaderboard access)
- [ ] Unity Editor setup for EndlessModeUI panel (REQUIRES MANUAL SETUP)
- [x] MainMenuUI button integration (endlessModeButton, panel logic)
- [ ] Unity Editor setup for main menu button (REQUIRES MANUAL SETUP)

### Testing
- [ ] Activation test (complete campaign → endless starts)
- [ ] Scaling test (waves get progressively harder)
- [ ] Milestone test (bonuses at 5, 10, 15...)
- [ ] UI test (main menu panel displays correctly)
- [ ] Save test (personal bests persist)
- [ ] Leaderboard test (scores submit successfully)

### Documentation
- [x] CHANGELOG.md updated (Endless Mode section added)
- [x] README.md roadmap updated (marked complete)
- [x] Setup guide created (this document)

---

## 🎯 Next Steps

1. **Unity Editor Setup** (15-30 minutes):
   - Create EndlessModePanel in Main Menu scene
   - Assign all Inspector references
   - Add Endless Mode button to main panel
   - Test UI navigation and display

2. **Playtesting** (30-60 minutes):
   - Complete a campaign map
   - Verify endless mode activation
   - Play to wave 10+ to test scaling
   - Verify milestone rewards trigger
   - Check personal bests save correctly

3. **Balance Tuning** (Optional):
   - Adjust scaling parameters based on playtesting
   - Tune milestone credit amounts
   - Set appropriate wave delay timing

4. **Polish** (Optional):
   - Add particle effects for milestone achievements
   - Enhanced UI animations for wave transitions
   - Audio polish (unique endless mode music?)
   - Achievement integration for endless milestones

---

**Setup Complete!** 🎉

The Endless Mode system is fully implemented in code and ready for Unity Editor scene setup. Follow the steps above to complete the UI integration, then playtest to ensure everything works smoothly.

For questions or issues, refer to the Troubleshooting section or check the inline code comments in EndlessMode.cs, GameHUD.cs, and EndlessModeUI.cs.
