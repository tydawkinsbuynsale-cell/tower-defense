# Scene Setup Guide

**Robot Tower Defense - Runtime Validator Setup**

This guide explains how to set up the RuntimeValidator component in your test scenes.

---

## 🎯 Purpose

The RuntimeValidator provides in-game testing during play mode. It displays real-time system status, performance metrics, and offers debug commands.

---

## 📋 Setup Instructions

### Step 1: Create Validator UI

1. **Create Canvas**
   - Right-click Hierarchy → UI → Canvas
   - Name: `ValidatorCanvas`
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920×1080

2. **Create Validator Panel**
   - Right-click Canvas → UI → Panel
   - Name: `ValidatorPanel`
   - Anchor: Top-Right
   - Width: 600, Height: 800
   - Position: X: -320, Y: -420
   - Background: Semi-transparent black (0, 0, 0, 200)

3. **Create Status Text**
   - Right-click ValidatorPanel → UI → Text - TextMeshPro
   - Name: `StatusText`
   - Anchor: Stretch (fill panel)
   - Font Size: 14
   - Color: White
   - Alignment: Top-Left
   - Overflow: Scroll
   - Rich Text: Enabled

4. **Create Run Tests Button**
   - Right-click ValidatorPanel → UI → Button - TextMeshPro
   - Name: `RunTestsButton`
   - Anchor: Bottom-Center
   - Width: 250, Height: 50
   - Position: Y: 40
   - Text: "Run Tests"

5. **Create Close Button**
   - Right-click ValidatorPanel → UI → Button - TextMeshPro
   - Name: `CloseButton`
   - Anchor: Top-Right corner
   - Width: 50, Height: 50
   - Position: X: -30, Y: -30
   - Text: "✕"

### Step 2: Add RuntimeValidator Component

1. **Create GameObject**
   - Right-click Hierarchy → Create Empty
   - Name: `RuntimeValidator`

2. **Add Component**
   - Select RuntimeValidator GameObject
   - Add Component → Scripts → Core → RuntimeValidator

3. **Assign References**
   - **Validator Panel:** Drag ValidatorPanel here
   - **Status Text:** Drag StatusText here
   - **Run Tests Button:** Drag RunTestsButton here
   - **Close Button:** Drag CloseButton here
   - **Toggle Key:** F12 (default)
   - **Test Interval:** 2.0 (seconds)

### Step 3: Test Setup

1. **Enter Play Mode**
2. **Press F12** - Panel should appear
3. **Tests run automatically** - Check for any errors
4. **Click "Run Tests"** - Tests should re-run
5. **Click "✕"** or **Press F12** - Panel should hide

---

## 🎮 Usage During Development

### Keyboard Shortcut
- **F12:** Toggle validator panel on/off

### What You'll See

```
=== RUNTIME VALIDATION ===
Time: 14:23:45

━━━ Core Managers ━━━
✓ GameManager
✓ WaveManager
✓ SaveManager
✓ TowerPlacementManager
✓ AudioManager
✓ PerformanceManager
✓ TutorialManager
✓ AchievementManager
✓ TechTree
✓ EndlessMode

━━━ Game State ━━━
State: Playing
Credits: 500
Lives: 20
Score: 1250
Paused: False
Game Over: False
Current Wave: 5/15
Wave Active: True
Enemies Alive: 8

━━━ Systems Integration ━━━
Towers Placed: 4
Has Placed: True
Has Upgraded: True
Achievements: 3/25
Tech Points: 12
Endless Active: False

━━━ Performance ━━━
Quality: Medium
Current FPS: 60.2
Average FPS: 59.8
Min FPS: 58.1
Max FPS: 60.5
Frame Time: 16.6ms
Battery Save: False
Memory: 432.5 MB
Time Scale: 1
Target FPS: 60

━━━ Save/Load ━━━
Waves Completed: 15
Highest Wave: 12
High Score: 5240
Tutorial Done: True
Graphics Quality: 1
Master Volume: 0.80
Tech Points: 12
Achievements Unlocked: 3
Tech Upgrades: 5

=== TESTS COMPLETE ===
```

### Debug Commands (Right-Click Component)

**Context Menu Options:**
- **Add 1000 Credits** - Test economy balance
- **Skip to Wave 10** - Test late-game scenarios
- **Complete Tutorial** - Bypass tutorial
- **Unlock All Achievements** - Test achievement system
- **Reset Save** - Test first-time experience
- **Force GC** - Test garbage collection
- **Toggle Quality** - Cycle quality presets

---

## 🔧 Scenes to Set Up

### Recommended Scenes

1. **Testing Scene** (Create New)
   - **Purpose:** Clean scene for isolated testing
   - **Setup:** Only essential systems
   - **Include:** RuntimeValidator (full setup)
   - **Maps:** Use simple test map

2. **GameplayScene** (Optional)
   - **Purpose:** Test during actual gameplay
   - **Setup:** Full game scene
   - **Include:** RuntimeValidator (hidden by default)
   - **Use Case:** Performance testing during real gameplay

3. **MainMenu** (Optional)
   - **Purpose:** Test menu systems
   - **Setup:** Menu only
   - **Include:** RuntimeValidator (if needed)
   - **Use Case:** Validate save load on startup

### Testing Scene Template

```
TestingScene
├── Managers (DontDestroyOnLoad)
│   ├── GameManager
│   ├── WaveManager
│   ├── AudioManager
│   ├── PerformanceManager
│   └── ...
├── Map
│   ├── Background
│   ├── Path (with waypoints)
│   └── PlacementZones
├── UI
│   ├── GameHUD
│   └── ValidatorCanvas ← RuntimeValidator UI
└── RuntimeValidator ← Component with references
```

---

## 📱 Mobile Testing

### Enable On-Screen Toggle Button

If F12 doesn't work on device, add a button:

1. **Create Toggle Button**
   - Right-click GameHUD → UI → Button
   - Name: `DebugToggleButton`
   - Anchor: Bottom-Right corner
   - Width: 100, Height: 100
   - Text: "DEBUG"
   - Color: Semi-transparent

2. **Add Click Handler**
   ```csharp
   // In RuntimeValidator.cs, make Show() public
   public void ToggleVisibility()
   {
       if (isVisible)
           Hide();
       else
           Show();
   }
   ```

3. **Connect Button**
   - Select DebugToggleButton
   - Add OnClick event
   - Drag RuntimeValidator GameObject
   - Select RuntimeValidator → ToggleVisibility()

### Disable for Release Builds

```csharp
// In RuntimeValidator.cs Awake()
void Awake()
{
    #if !DEVELOPMENT_BUILD
    gameObject.SetActive(false);
    return;
    #endif
}
```

---

## 🎨 UI Customization

### Change Colors

**Panel Background:**
```
Color: (0, 0, 0, 200) ← Black, 200 alpha
```

**Success Text:**
```
Rich text: <color=green>✓ Passed</color>
```

**Warning Text:**
```
Rich text: <color=yellow>⚠ Warning</color>
```

**Error Text:**
```
Rich text: <color=red>✗ Failed</color>
```

### Change Position

**Top-Left:**
```
Anchor: Top-Left
Pivot: (0, 1)
Position: (320, -20)
```

**Bottom-Left:**
```
Anchor: Bottom-Left
Pivot: (0, 0)
Position: (320, 20)
```

**Full-Screen:**
```
Anchor: Stretch
Margins: (20, 20, 20, 20)
```

### Change Toggle Key

In Inspector:
```
Toggle Key: F12 (default)
           : BackQuote (`)
           : Keypad0
           : Any KeyCode enum value
```

---

## 🐛 Troubleshooting

### "Panel doesn't appear"
- Check validatorPanel reference is assigned
- Check Canvas is enabled
- Check ValidatorPanel is active
- Check toggle key isn't conflicting

### "Tests show errors"
- Expected if systems not initialized yet
- Wait for Awake/Start to complete
- Check manager singleton initialization
- Verify scene has required managers

### "FPS shows 0"
- Wait a few seconds for FPS tracking
- PerformanceManager needs time to collect samples
- Check PerformanceManager is in scene

### "Touch doesn't work on device"
- Ensure EventSystem exists in scene
- Check Canvas has GraphicRaycaster
- Verify buttons have Image component
- Test with on-screen toggle button

### "Text is cut off"
- Increase ValidatorPanel height
- Enable scrolling on StatusText
- Reduce font size
- Use Overflow: Scroll

---

## ✅ Verification Checklist

**Before committing scene:**

- [ ] RuntimeValidator GameObject exists
- [ ] ValidatorPanel created with proper size
- [ ] StatusText configured (TMP, white, top-left)
- [ ] RunTestsButton created and connected
- [ ] CloseButton created and connected
- [ ] All references assigned in Inspector
- [ ] Toggle key set (F12)
- [ ] Tested in Play Mode (F12 works)
- [ ] Tests run and display results
- [ ] Close button works
- [ ] No console errors

---

## 📚 Related Files

- **RuntimeValidator.cs** - Main component script
- **IntegrationTests.cs** - Editor-time tests
- **TESTING_GUIDE.md** - Complete testing documentation
- **PRE_RELEASE_CHECKLIST.md** - Release validation checklist

---

## 🚀 Quick Start (Copy-Paste)

Fastest setup for experienced users:

1. Create Canvas → Panel (600×800, top-right)
2. Add TMP text (stretch, scroll)
3. Add 2 buttons (Run Tests, Close)
4. Create empty GameObject, add RuntimeValidator
5. Assign 4 references in Inspector
6. Press Play, press F12, done!

---

**Last Updated:** 2024
**Version:** 1.0.0
