# Testing Guide

**Robot Tower Defense**

Complete guide to testing, validation, and quality assurance for the Robot Tower Defense project.

---

## 🎯 Overview

This guide covers three testing approaches:

1. **Editor Integration Tests** - Automated validation of systems and assets
2. **Runtime Validation** - In-game testing during play mode
3. **Device Testing** - Real-world testing on Android devices

---

## 🔧 Editor Integration Tests

### Location
**Tools → Robot TD → Integration Tests**

### What It Tests
- Class existence (reflection-based)
- Asset counts (TowerData, EnemyData, MapData)
- Data structure integrity (SaveManager fields)
- System integration points

### How to Use

1. Open Unity Editor
2. Click **Tools → Robot TD → Integration Tests**
3. Click **"Run All Tests"** or individual test buttons
4. Review results in the scrollable text area
5. Fix any errors reported

### Expected Results

```
✓ PASSED: GameManager
✓ PASSED: 11 TowerData assets found
✗ FAILED: Only 3/14 EnemyData assets found
⚠ WARNING: MapData count low
```

### Test Categories

#### Core Systems
- **GameManager** - Validates singleton exists
- **WaveManager** - Checks wave system + EndlessMode integration
- **SaveManager** - Validates save data structure and fields

#### Game Content
- **Tower Data** - Expects 11 TowerData assets (one per type)
- **Enemy Data** - Expects 14 EnemyData assets (11 + 3 bosses)
- **Map Data** - Expects 5 MapData assets and 5 scenes

#### Progression Systems
- **Achievement System** - Validates AchievementManager class
- **Tech Tree** - Checks TechTree and TechUpgrade enum

#### Performance & Polish
- **Performance Manager** - Validates quality preset system
- **Tutorial System** - Checks TutorialManager structure

### When to Run

- After making changes to core systems
- Before committing code
- Before building a release
- When debugging integration issues
- As part of CI/CD pipeline (future)

### Troubleshooting

**"Type not found"**
- Class may be in wrong namespace
- Check using statement in test file
- Verify class name spelling

**"Asset count mismatch"**
- Create missing ScriptableObject assets
- Check Assets/Data/ folder structure
- Verify asset search filter is correct

**"Field not found"**
- Check PlayerSaveData structure
- Verify field name spelling
- Ensure field is public or [SerializeField]

---

## 🎮 Runtime Validation

### Location
**Attach RuntimeValidator.cs to GameObject in scene**

### What It Tests
- Manager initialization
- Game state during gameplay
- Performance metrics (FPS, memory)
- Save/load system
- System integration (towers, achievements, etc.)

### How to Use

#### Setup
1. Create new GameObject in test scene
2. Add **RuntimeValidator** component
3. Assign UI references:
   - Validator Panel (Canvas panel)
   - Status Text (TextMeshProUGUI)
   - Run Tests Button
   - Close Button
4. Set toggle key (default: F12)

#### During Play Mode
1. Press **F12** to show validator
2. Tests run automatically on open
3. Click **"Run Tests"** to refresh
4. Review real-time system status

### Debug Commands

Right-click RuntimeValidator component for quick commands:

- **Add 1000 Credits** - Test economy
- **Skip to Wave 10** - Test wave progression
- **Complete Tutorial** - Bypass tutorial
- **Reset Save** - Test save/load
- **Force GC** - Test garbage collection
- **Toggle Quality** - Cycle quality presets

### What to Look For

#### Healthy State
```
━━━ Core Managers ━━━
✓ GameManager
✓ WaveManager
✓ SaveManager
...all managers initialized

━━━ Performance ━━━
Quality: Medium
Current FPS: 60.2
Average FPS: 59.8
Min FPS: 58.1
Frame Time: 16.6ms
```

#### Problem State
```
━━━ Core Managers ━━━
✗ GameManager NOT FOUND
✓ WaveManager
...

━━━ Performance ━━━
Current FPS: 28.3  ⚠ LOW!
Min FPS: 15.2      ⚠ VERY LOW!
```

### When to Use

- During active development
- When debugging gameplay issues
- To monitor performance
- To verify save/load works
- Before device testing

---

## 📱 Device Testing

### Required Devices

**Minimum (Low-End):**
- 2GB RAM
- 4 CPU cores
- Android 5.0 (API 21)
- **Example:** Samsung Galaxy J7 (2016)

**Recommended (Mid-Range):**
- 4GB RAM
- 6 CPU cores
- Android 8.0 (API 26)
- **Example:** Samsung Galaxy A50

**Optimal (High-End):**
- 8GB+ RAM
- 8+ CPU cores
- Android 10+ (API 29+)
- **Example:** Samsung Galaxy S21

### Installation Methods

#### Method 1: Direct APK Install
```bash
# Build APK in Unity (File → Build Settings → Build)
# Connect device via USB
# Enable USB debugging on device
adb install "path/to/RobotTD.apk"
```

#### Method 2: Internal Testing Track
1. Build AAB in Unity
2. Upload to Play Console
3. Add your email to internal testers
4. Install via Play Store link

### Performance Benchmarks

#### Low-End Device (Low Quality)
- **Target FPS:** 30 stable
- **Memory:** < 512 MB
- **Battery:** < 10% drain per hour
- **Wave 10 test:** No drops below 25 FPS
- **Wave 20 test:** Playable

#### Mid-Range Device (Medium Quality)
- **Target FPS:** 60 stable
- **Memory:** < 768 MB
- **Battery:** < 15% drain per hour
- **Wave 10 test:** Stable 60 FPS
- **Wave 20 test:** No drops below 50 FPS

#### High-End Device (High Quality)
- **Target FPS:** 60 stable
- **Memory:** < 1 GB
- **Battery:** < 20% drain per hour
- **Wave 30 test:** Stable 60 FPS
- **All effects enabled:** No stuttering

### Testing Checklist

#### First Launch
- [ ] Splash screen displays
- [ ] Main menu loads
- [ ] Tutorial auto-starts
- [ ] Touch input works
- [ ] UI scales correctly
- [ ] No crashes

#### Core Gameplay
- [ ] Can place towers
- [ ] Towers fire at enemies
- [ ] Enemies follow path
- [ ] Damage calculation correct
- [ ] Currency system works
- [ ] Lives decrease correctly
- [ ] Wave progression works

#### Save/Load
- [ ] Progress saves automatically
- [ ] Close app mid-game
- [ ] Reopen app
- [ ] Progress restored correctly
- [ ] Settings persisted

#### Performance
- [ ] Monitor FPS during gameplay
- [ ] No stuttering during wave spawns
- [ ] No lag when many units on screen
- [ ] Battery drain acceptable
- [ ] Device doesn't overheat
- [ ] Memory usage stable

#### Edge Cases
- [ ] Phone call interruption
- [ ] Background app and resume
- [ ] Low battery behavior
- [ ] Low storage behavior
- [ ] Network loss (if online features)
- [ ] Screen rotation (should be locked)
- [ ] Different aspect ratios

### Profiling on Device

#### Using Unity Profiler
1. Build Development Build (check box in Build Settings)
2. Enable Autoconnect Profiler
3. Install and run on device
4. Open Unity Profiler (Window → Analysis → Profiler)
5. Monitor CPU, memory, rendering

#### Using Android Profiler
```bash
# Monitor FPS
adb shell dumpsys gfxinfo com.yourstudio.robottd

# Monitor memory
adb shell dumpsys meminfo com.yourstudio.robottd

# Monitor battery
adb shell dumpsys battery

# Capture logcat
adb logcat -s Unity
```

#### Key Metrics

**CPU:**
- Main thread < 16ms (for 60 FPS)
- Render thread < 16ms
- No GC spikes > 50ms

**Memory:**
- Heap < 400 MB
- Native < 600 MB
- GC frequency < 1/second
- No leaks over time

**Rendering:**
- Draw calls < 100 (mobile)
- Batches < 50
- Triangles < 100k
- Texture memory < 300 MB

---

## 🧪 Automated Testing (Future)

### Unit Tests (C# Test Framework)

**Setup:**
```csharp
// Assets/Tests/EditMode/
// Example: TowerDataTests.cs
using NUnit.Framework;
using RobotTD.Towers;

public class TowerDataTests
{
    [Test]
    public void TowerData_DamageCalculation_IsCorrect()
    {
        var tower = ScriptableObject.CreateInstance<TowerData>();
        tower.baseDamage = 10f;
        tower.damagePerLevel = 2f;
        
        Assert.AreEqual(14f, tower.GetDamageAt Level(2));
    }
}
```

**Run:**
- Window → General → Test Runner
- EditMode tab
- Run All

### Play Mode Tests

**Setup:**
```csharp
// Assets/Tests/PlayMode/
// Example: GameManagerTests.cs
using UnityEngine.TestTools;
using System.Collections;

public class GameManagerTests
{
    [UnityTest]
    public IEnumerator GameManager_StartsWithCorrectCredits()
    {
        GameManager.Instance.StartNewGame();
        yield return null;
        
        Assert.AreEqual(300, GameManager.Instance.Credits);
    }
}
```

**Run:**
- Window → General → Test Runner
- PlayMode tab
- Run All

### CI/CD Pipeline (GitHub Actions Example)

```yaml
# .github/workflows/test.yml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: game-ci/unity-test-runner@v2
        with:
          unityVersion: 2022.3.10f1
          testMode: all
      - uses: actions/upload-artifact@v2
        with:
          name: Test Results
          path: artifacts
```

---

## 📊 Test Coverage Goals

### Current State (v1.0.0)
- **Editor Integration Tests:** 10 test categories ✅
- **Runtime Validator:** Manual testing tool ✅
- **Unit Tests:** 0% coverage ❌
- **Play Mode Tests:** 0% coverage ❌

### Future Goals

**v1.1.0:**
- [ ] Add 20+ unit tests (core systems)
- [ ] Add 10+ play mode tests (gameplay)
- [ ] Set up CI/CD pipeline
- [ ] Automated build testing

**v1.2.0:**
- [ ] Achieve 50% code coverage
- [ ] Add integration tests for multiplayer (if added)
- [ ] Performance regression testing
- [ ] Automated device testing (Firebase Test Lab)

**v2.0.0:**
- [ ] Achieve 70% code coverage
- [ ] End-to-end test automation
- [ ] Load testing (stress tests)
- [ ] Security testing (save file validation)

---

## 🐛 Bug Reporting Template

When you find a bug during testing, document it:

### Bug Report Format

```markdown
**Title:** [Brief description]

**Severity:** [Critical / High / Medium / Low]

**Category:** [Gameplay / UI / Performance / Audio / Save/Load / Other]

**Steps to Reproduce:**
1. Start new game
2. Place tower at position X
3. Start wave
4. ...

**Expected Behavior:**
Tower should fire at enemies

**Actual Behavior:**
Tower doesn't fire

**Screenshots/Video:**
[Attach here]

**Device Info:**
- Device: Samsung Galaxy A50
- Android Version: 10
- RAM: 4GB
- Build: v1.0.0 (APK)

**Logs:**
```
[Error] NullReferenceException at Tower.FindTarget()
```

**Workaround:**
None found

**Additional Notes:**
Only happens with Laser Tower type
```

---

## ✅ Definition of Done

A feature is "done" when:

1. **Code Complete**
   - All code written and committed
   - No compiler errors or warnings
   - Code follows style guide

2. **Tests Passing**
   - Editor integration tests pass
   - Runtime validator shows no errors
   - Unit tests pass (if applicable)

3. **Documented**
   - Code comments added
   - README updated  (if needed)
   - CHANGELOG entry written

4. **Reviewed**
   - Code reviewed by another developer
   - Design reviewed by team
   - No critical issues found

5. **Device Tested**
   - Tested on low-end device
   - Tested on mid-range device
   - Performance acceptable

6. **No Regressions**
   - Existing features still work
   - No new crashes introduced
   - No performance degradation

---

## 📚 Additional Resources

### Unity Testing Documentation
- [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [Performance Testing](https://docs.unity3d.com/Manual/profiler-profiling-applications.html)
- [Mobile Optimization](https://docs.unity3d.com/Manual/MobileOptimisation.html)

### Tools
- **Unity Profiler** - Performance profiling
- **Android Profiler** - Device-level profiling
- **Firebase Test Lab** - Automated device testing
- **GameBench** - Mobile game performance benchmarking

### Testing Best Practices
- Test early, test often
- Automate repetitive tests
- Test on real devices, not just emulators
- Profile on low-end devices
- Monitor crash reports post-launch
- Keep test documentation updated

---

**Last Updated:** 2024
**Version:** 1.0.0
