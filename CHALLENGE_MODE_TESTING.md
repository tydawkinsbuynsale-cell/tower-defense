# Challenge Mode Testing Guide

Complete testing checklist for validating all Challenge Mode modifiers and integrations.

## 📋 Pre-Test Setup

### 1. Generate Example Challenges
1. Open Unity Editor
2. Go to menu: **Robot TD → Generate Example Challenges**
3. Verify 13 challenge assets created in `Assets/Resources/Data/Challenges/`
4. Check console for success message

### 2. Register Challenges with Manager
1. Locate `ChallengeManager` GameObject in scene (should be in Bootstrap or persistent scene)
2. In Inspector, expand **Challenge Library** array
3. Drag all 13 generated challenge assets into the array
4. Save scene

### 3. Verify UI Setup
1. Ensure `ChallengeSelectorUI` prefab exists and is referenced
2. Ensure `ChallengeCardUI` prefab exists for card spawning
3. Ensure `ChallengeResultUI` prefab is linked to GameManager
4. Test opening Challenge Selector from main menu

---

## 🧪 Modifier Testing Checklist

### Enemy Modifiers

#### ✅ SpeedRush (Enemies move 50% faster)
**Challenge:** Daily 01 - Speed Rush

**Test Steps:**
1. Start challenge "Speed Rush"
2. Observe enemy movement speed against normal gameplay
3. Verify enemies reach destination ~33% faster (1.5x speed)
4. Check if fast enemies overwhelm towers as expected

**Expected Result:**
- Enemies visibly move faster than normal
- WaveManager applies `challengeSpeedMultiplier = 1.5f`
- Console: `[WaveManager] Challenge multipliers set: Health=1.0, Speed=1.5, Count=1.0`

---

#### ✅ ArmoredAssault (Enemies have 100% more HP)
**Challenge:** Daily 04 - Armored Horde

**Test Steps:**
1. Start challenge "Armored Horde"
2. Place basic tower and count shots to kill first enemy
3. Compare to normal mode (should take ~2x as many hits)
4. Check enemy health bars (should deplete slower)

**Expected Result:**
- Enemies take significantly more damage to kill
- WaveManager applies `challengeHealthMultiplier = 2.0f`
- Console: `[WaveManager] Challenge multipliers set: Health=2.0, Speed=1.0, Count=1.0`

---

#### ✅ SwarmMode (50% more enemies per wave)
**Challenge:** Weekly 01 - Limited Arsenal (includes SwarmMode)

**Test Steps:**
1. Start challenge "Limited Arsenal"
2. Start wave 1 and count enemies spawned
3. Compare to normal wave 1 (should be 1.5x count, rounded)
4. Verify later waves also have increased enemy counts

**Expected Result:**
- More enemies spawn per wave (e.g., 10 → 15)
- WaveManager `GetChallengeModifiedEnemyCount()` returns increased count
- Console: `[WaveManager] Challenge multipliers set: Health=1.0, Speed=1.0, Count=1.5`

---

### Tower Modifiers

#### ✅ BudgetCrisis (Towers cost 50% more)
**Challenge:** Weekly 02 - Economic Crisis

**Test Steps:**
1. Start challenge "Economic Crisis"
2. Open tower build menu
3. Check cost displayed on tower buttons
4. Verify costs are 1.5x normal (e.g., 100 → 150)
5. Attempt to buy tower with exact modified cost
6. Verify credits deducted correctly

**Expected Result:**
- Tower button shows increased cost (e.g., "$150" instead of "$100")
- TowerButton.GetModifiedCost() returns 1.5x base cost
- TowerPlacementManager.GetModifiedTowerCost() applies multiplier
- Credits deducted match modified cost
- Console: `[ChallengeManager] Active tower cost multiplier: 1.5`

---

#### ✅ TowerLimit (Maximum 10 towers)
**Challenge:** Weekly 03 - Tower Limit

**Test Steps:**
1. Start challenge "Tower Limit"
2. Place 10 towers on map
3. Attempt to place 11th tower
4. Verify placement blocked with debug message
5. Sell one tower
6. Verify can now place tower again

**Expected Result:**
- First 10 towers place successfully
- 11th tower placement blocked
- Console: `"Challenge tower limit reached!"`
- After selling, placement works again
- ChallengeManager.CanPlaceTower() returns false when at limit

---

#### ✅ WeakenedTowers (Towers deal 30% less damage)
**Challenge:** Weekly 04 - Weakened Defense

**Test Steps:**
1. Start challenge "Weakened Defense"
2. Place a tower with known damage (e.g., Laser Turret with 20 damage)
3. Shoot enemy and observe damage numbers
4. Verify damage is ~70% of normal (e.g., 20 → 14)
5. Upgrade tower and verify damage still weakened

**Expected Result:**
- Damage numbers show 0.7x base damage
- Tower.GetDamageMultiplier() returns baseMultiplier * 0.7
- Console: `[Tower] Damage multiplier: 0.7 (challenge modifier applied)`
- Upgraded towers also affected (maintains 30% reduction)

---

#### ✅ LimitedArsenal (Only 3 tower types available)
**Challenge:** Weekly 01 - Limited Arsenal

**Test Steps:**
1. Start challenge "Limited Arsenal"
2. Check tower build menu
3. Verify only 3 tower types are selectable/visible
4. Verify other tower buttons are grayed out or hidden
5. Attempt to place allowed towers (should work)
6. Restart challenge, verify different 3 towers selected randomly

**Expected Result:**
- Only 3 tower types available for placement
- Other towers disabled in UI
- ChallengeManager stores allowed tower list
- Random selection each challenge attempt

**Note:** This modifier requires additional UI implementation to hide/disable tower buttons. Mark as "Pending UI Integration" if not yet implemented.

---

### Economy Modifiers

#### ✅ EconomicHardship (50% less credits per kill)
**Challenge:** Weekly 02 - Economic Crisis

**Test Steps:**
1. Start challenge "Economic Crisis"
2. Kill first enemy and note credit reward
3. Compare to normal mode (should be 0.5x normal)
4. Verify wave completion bonus also reduced by 50%
5. Check total credits earned over multiple waves

**Expected Result:**
- Enemy kill credits reduced by 50% (e.g., 10 → 5)
- Wave bonuses reduced by 50%
- GameManager.AddCredits() applies ChallengeManager.GetEconomyMultiplier()
- Console: `[GameManager] Credits added: 5 (after challenge modifier: 0.5x)`

---

#### ✅ StartingDebt (Start with 50% credits)
**Challenge:** Daily 02 - Budget Warriors

**Test Steps:**
1. Start challenge "Budget Warriors"
2. Check starting credits immediately on game start
3. Compare to normal starting credits (e.g., 500 → 250)
4. Verify can only afford cheaper towers at start

**Expected Result:**
- Starting credits reduced to 50% of normal
- GameManager.SetStartingCredits() called by ChallengeManager
- UI credits display shows reduced amount
- Console: `[ChallengeManager] Starting credits set to: 250 (50% of normal)`

---

### Wave Modifiers

#### ✅ FastForward (Waves auto-start with reduced delay)
**Challenge:** Daily 02 - Budget Warriors

**Test Steps:**
1. Start challenge with FastForward modifier
2. Complete wave 1
3. Measure time until wave 2 auto-starts
4. Compare to normal delay (should be ~50% of baseDelay)
5. Verify no manual start required

**Expected Result:**
- Wave auto-starts after short delay (~2.5s instead of 5s)
- WaveManager.GetWaveDelay() returns timeBetweenWaves * 0.5
- Console: `[WaveManager] Wave delay: 2.5s (FastForward modifier)`

---

#### ✅ NoBreaks (Zero delay between waves)
**Challenge:** Daily 03 - Rapid Fire

**Test Steps:**
1. Start challenge "Rapid Fire"
2. Complete wave 1
3. Observe wave 2 starts almost immediately (0.1s delay)
4. Verify continuous pressure with no breathing room
5. Check multiple waves

**Expected Result:**
- Waves start nearly instantly after previous wave completes
- WaveManager.GetWaveDelay() returns 0.1f
- Console: `[WaveManager] Wave delay: 0.1s (NoBreaks modifier)`
- Intense, continuous gameplay with no rest periods

---

### Special Modifiers

#### ✅ PerfectDefense (One life only)
**Challenge:** Permanent 01 - Perfect Defense

**Test Steps:**
1. Start challenge "Perfect Defense"
2. Check starting lives (should be 1)
3. Let one enemy reach end
4. Verify immediate game over
5. Check GameOver UI displays

**Expected Result:**
- Lives display shows "1 ❤️"
- GameManager.SetLives(1) called by ChallengeManager
- Single enemy leak triggers instant defeat
- Console: `[ChallengeManager] Lives set to: 1 (PerfectDefense modifier)`

---

## 🔄 Multi-Modifier Testing

### Combined Enemy + Tower Modifiers
**Challenge:** Permanent 02 - Speed Master

**Test:** SpeedRush + NoBreaks + SwarmMode together

**Expected Result:**
- Fast enemies (1.5x speed)
- More enemies per wave (1.5x count)
- No delay between waves (0.1s)
- Extremely challenging, requires perfect tower placement
- All modifiers stack correctly

---

### Combined Economy + Tower Modifiers
**Challenge:** Permanent 05 - Economy Master

**Test:** EconomicHardship + StartingDebt + BudgetCrisis

**Expected Result:**
- Start with 50% credits
- Towers cost 1.5x normal
- Enemy kills give 0.5x credits
- Severe economic pressure throughout game
- All economic modifiers compound difficulty

---

## 🎯 Analytics Verification

### Challenge Events
1. Start any challenge
2. Check Analytics Dashboard (Tools → Robot TD → Analytics Dashboard)
3. Verify "challenge_started" event logged with:
   - challenge_id
   - difficulty_tier
   - modifiers (comma-separated)

4. Complete challenge (win or lose)
5. Verify "challenge_completed" event logged with:
   - challenge_id
   - success (true/false)
   - final_score
   - waves_completed
   - completion_time
   - attempts

---

## 💾 Persistence Testing

### Progress Tracking
1. Complete a challenge successfully
2. Exit to main menu
3. Reopen Challenge Selector
4. Verify challenge shows:
   - ✅ Completion badge
   - Best score displayed
   - "Completed" status

5. Restart same challenge
6. Verify attempt counter incremented
7. Get higher score
8. Verify best score updated

### Rotation System
1. Check Daily challenge timer in UI
2. Manually advance system time 24 hours (OS settings)
3. Relaunch game
4. Verify Daily challenges rotated to next index
5. Verify Weekly challenges unchanged (need 7 days)

---

## 🐛 Common Issues & Fixes

### Issue: Modifiers not applying
**Solution:** 
- Verify ChallengeManager.ApplyModifiers() called on game start
- Check console for modifier application logs
- Ensure challenge is active: ChallengeManager.IsChallengeActive returns true

### Issue: Tower costs not changing
**Solution:**
- Verify TowerButton calls GetModifiedCost()
- Ensure TowerPlacementManager.GetModifiedTowerCost() implemented
- Check ChallengeManager.GetTowerCostMultiplier() returns correct value

### Issue: Enemy stats unchanged
**Solution:**
- Verify WaveManager.SetChallengeMultipliers() called by ChallengeManager
- Check WaveManager.SpawnEnemy() applies multipliers to enemy stats
- Ensure enemy.maxHealth and enemy.moveSpeed modified on spawn

### Issue: UI shows wrong costs
**Solution:**
- Ensure TowerButton.RefreshAffordability() queries modified costs
- Verify UpdateCostDisplay() uses GetModifiedCost()
- Check canvasGroup alpha updates based on affordability

---

## ✅ Final Validation Checklist

Before marking Challenge Mode complete:

- [ ] All 13 example challenges load without errors
- [ ] All enemy modifiers affect gameplay correctly
- [ ] All tower modifiers affect costs/limits/damage
- [ ] All economy modifiers affect credit rewards
- [ ] All wave modifiers affect timing correctly
- [ ] Multi-modifier challenges work (modifiers stack)
- [ ] Tower limit enforcement prevents over-placement
- [ ] Challenge completion rewards granted (first time only)
- [ ] Progress tracking persists across sessions
- [ ] Analytics events fire for challenge start/complete
- [ ] UI displays correct modified values (costs, lives, etc.)
- [ ] Challenge rotation system works for Daily/Weekly
- [ ] ChallengeResultUI shows correct victory/defeat state
- [ ] No console errors during challenge gameplay
- [ ] Performance acceptable with all modifiers active

---

## 📊 Performance Notes

Expected FPS impact:
- Normal gameplay: 60 FPS target
- Challenge Mode (single modifier): <5% impact
- Challenge Mode (3+ modifiers): <10% impact
- SwarmMode (1.5x enemies): ~15% FPS reduction expected

If performance issues occur:
1. Check ObjectPooler expansion for increased enemy counts
2. Verify VFXManager not over-spawning particles
3. Review PerformanceManager auto-quality adjustments
4. Consider enemy spawn rate limiting for SwarmMode

---

## 🎓 Tips for Testers

1. **Test incrementally:** Validate one modifier type at a time before testing combinations
2. **Use debug logs:** Enable verbose logging in ChallengeManager for troubleshooting
3. **Compare to baseline:** Always test same scenario in normal mode first
4. **Document edge cases:** Note any unexpected behaviors for later fixes
5. **Test persistence:** Exit and relaunch between tests to verify save/load
6. **Check all maps:** Verify challenges work on all 5 available maps
7. **Test difficulty scaling:** Easy should feel achievable, Extreme should feel brutal

---

## 🔧 Developer Testing Commands

Add these to ChallengeManager for testing (remove before release):

```csharp
#if UNITY_EDITOR
[ContextMenu("Force Complete Current Challenge")]
private void DEBUG_ForceComplete()
{
    if (IsChallengeActive)
        CompleteChallenge(true, 10000, 25);
}

[ContextMenu("Reset All Challenge Progress")]
private void DEBUG_ResetProgress()
{
    progressData.Clear();
    SaveProgress();
    Debug.Log("[ChallengeManager] All progress reset");
}

[ContextMenu("Log Active Modifiers")]
private void DEBUG_LogModifiers()
{
    if (!IsChallengeActive)
    {
        Debug.Log("[ChallengeManager] No active challenge");
        return;
    }
    
    Debug.Log($"[ChallengeManager] Active Challenge: {CurrentChallenge.ChallengeName}");
    Debug.Log($"[ChallengeManager] Modifiers: {string.Join(", ", CurrentChallenge.Modifiers)}");
    Debug.Log($"[ChallengeManager] Cost Mult: {activeTowerCostMultiplier}");
    Debug.Log($"[ChallengeManager] Damage Mult: {activeTowerDamageMultiplier}");
    Debug.Log($"[ChallengeManager] Economy Mult: {activeEconomyMultiplier}");
}
#endif
```

---

**Testing Status:** Ready for Unity play mode testing  
**Last Updated:** March 12, 2026  
**Tester:** _____________  
**Date Tested:** _____________
