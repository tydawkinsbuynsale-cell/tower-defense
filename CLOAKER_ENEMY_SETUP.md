# Cloaker Enemy Setup Guide

This guide walks through setting up the **Cloaker** enemy in Unity Editor. The Cloaker is a stealth enemy that can turn invisible, creating unique tactical challenges for tower defense gameplay.

## Overview

**Cloaker Features:**
- Starts cloaked (mostly invisible)
- Uncloaks when taking damage
- Re-cloaks after not taking damage for a duration
- Only detectable by towers within detection range when cloaked
- Visual transparency transitions (shimmer effect)
- Medium health, medium-fast speed
- Audio/visual feedback for cloaking transitions

**Strategic Role:**
- Forces tight tower placement for detection coverage
- Countered by close-range towers
- Rewards strategic tower positioning
- Creates tension and unpredictability
- Tests player awareness and reaction

---

## Quick Setup Checklist

- [ ] Create EnemyData ScriptableObject
- [ ] Create Cloaker enemy prefab
- [ ] Configure cloaking visual effects
- [ ] Set up particle effects (cloak/uncloak)
- [ ] Assign audio clips
- [ ] Add to wave compositions
- [ ] Test detection mechanics
- [ ] Balance health/speed values

---

## 1. Create EnemyData ScriptableObject

### Step 1: Create the Asset

1. In Unity Project window, navigate to `Assets/Resources/Data/Enemies/`
2. Right-click → **Create → RobotTD → Enemy Data**
3. Name it: `Cloaker`

### Step 2: Configure Settings

Select the Cloaker asset and configure in Inspector:

#### Info Section
```
Enemy Name: Cloaker
Description: Stealth unit that turns invisible. Uncloaks when damaged. Can only be targeted by nearby towers when cloaked.
Icon: [Your sprite asset]
Category: Elite  (or Scout, depending on your design)
```

#### Base Stats
```
Base Health: 150.0
Base Move Speed: 2.5  (medium-fast)
Base Reward: 40
Score Value: 20
Live Damage: 1  (lives lost when reaching end)
```

#### Resistances
```
Physical Resistance: 0.0
Energy Resistance: 0.1  (10% - slightly resistant to lasers)
Fire Resistance: 0.0
Electric Resistance: 0.0
Plasma Resistance: 0.0
```

#### Special Abilities
```
Can Fly: No
Can Cloak: Yes  ✓ (important!)
Has Shield: No
Shield Health: 0
Can Heal: No
Heal Amount: 0
Heal Cooldown: 0
Can Split: No
Split Count: 0
Split Enemy Prefab: None
```

#### Visuals
```
Base Color: RGB(100, 100, 150) - stealth blue-grey
Damage Flash Color: RGB(255, 100, 100) - red
Model Scale: 1.0
Animator Controller: [Standard enemy animator]
```

#### Audio
```
Spawn Sound: [Stealth activation sound]
Hit Sound: [Metallic impact]
Death Sound: [Electronic shutdown]
Ability Sound: [Cloaking shimmer sound]
```

---

## 2. Create Cloaker Enemy Prefab

### Step 1: Create GameObject Hierarchy

1. In Hierarchy, right-click → **3D Object → Capsule** (or use custom mesh)
2. Rename to: `Cloaker`
3. Add child objects:
   ```
   Cloaker (root)
   ├─ Model (visual mesh)
   ├─ HealthBar (UI)
   ├─ CloakEffect (particle system)
   ├─ UncloakEffect (particle system)
   └─ DetectionRadius (gizmo helper)
   ```

### Step 2: Add Components

Select the root **Cloaker** GameObject:

1. Add Component → **Capsule Collider**
   - Is Trigger: **Yes**
   - Radius: 0.5
   - Height: 2.0

2. Add Component → **Rigidbody**
   - Is Kinematic: **Yes**
   - Use Gravity: **No**

3. Add Component → **CloakerEnemy** (script)

4. Configure in Inspector:

```
Enemy Data: [Link Cloaker EnemyData asset]

Model Transform: [Drag Model child object]
Health Bar: [Link HealthBar UI component]
Death Effect Prefab: [Standard enemy death VFX]
Body Renderers: [Drag all MeshRenderer components]

Cloaker Specific:
  Cloak Transition Duration: 0.5  (seconds)
  Uncloak Duration: 2.0  (stay visible 2s after damage)
  Recloak Cooldown: 3.0  (wait 3s before can recloak)
  Cloaked Alpha: 0.15  (15% opacity when cloaked)
  Cloak Effect: [Link CloakEffect particle system]
  Uncloak Effect: [Link UncloakEffect particle system]
  Cloak Sound: [Cloaking audio clip]
  Uncloak Sound: [Uncloaking audio clip]
```

### Step 3: Configure Model

- **Model Scale**: (0.8, 1.5, 0.8) - slender, agile appearance
- **Material**: Use material with shader that supports transparency
  - Rendering Mode: **Fade** or **Transparent**
  - Color: Dark blue-grey
  - Emission: Subtle cyan glow (for cloaked visibility)

### Step 4: Save Prefab

1. Drag Cloaker from Hierarchy to `Assets/Prefabs/Enemies/`
2. Delete from scene

---

## 3. Create Cloaking Visual Effects

### Cloak Effect (Particle System)

Create particle system for when enemy cloaks:

```
Main Module:
  Duration: 0.5
  Looping: No
  Start Lifetime: 0.3-0.5
  Start Speed: 2-4
  Start Size: 0.3-0.6
  Start Color: Gradient (cyan → transparent)
  Gravity Modifier: 0
  Simulation Space: World
  Max Particles: 30

Emission:
  Rate over Time: 0
  Bursts: 1 burst at 0.00, 20-30 particles

Shape:
  Shape: Sphere
  Radius: 0.8
  Emit from: Volume

Color over Lifetime:
  Gradient: Full opacity → Fade out

Size over Lifetime:
  Curve: Start small → Grow → Shrink

Renderer:
  Render Mode: Billboard
  Material: Additive particle material (glowing)
```

### Uncloak Effect (Particle System)

Create particle system for when enemy uncloaks:

```
Main Module:
  Duration: 0.5
  Looping: No
  Start Lifetime: 0.3-0.5
  Start Speed: 1-3
  Start Size: 0.4-0.7
  Start Color: Gradient (white → cyan)
  Gravity Modifier: 0
  Max Particles: 40

Emission:
  Rate over Time: 0
  Bursts: 1 burst at 0.00, 30-40 particles

Shape:
  Shape: Sphere
  Radius: 1.0
  Emit from: Volume

Color over Lifetime:
  Gradient: Full bright → Fade to blue

Size over Lifetime:
  Curve: Burst outward

Texture Sheet Animation:
  Mode: Grid
  Tiles: 2x2 (shimmer texture)
  Animation: Random Between Two Constants
```

### Material Setup (Important!)

For the Cloaker model material to work with transparency:

1. **Create Material**: `Assets/Materials/CloakerMaterial.mat`
2. **Shader**: Standard or URP/Lit
3. **Rendering Mode**: Fade or Transparent
4. **Settings**:
   - Albedo: Dark blue-grey (RGB 80, 80, 120)
   - Metallic: 0.5
   - Smoothness: 0.7
   - Emission: Enabled, Cyan color, Intensity 0.3

5. **Script will handle alpha transitions automatically**

---

## 4. Assign Audio Clips

### Required Audio Clips

1. **Cloak Sound** (Stealth activation)
   - Subtle "whoosh" with electronic shimmer
   - 0.3-0.5 second duration
   - Low volume (0.3-0.5)
   - Suggested: Phase-shift or invisibility sound

2. **Uncloak Sound** (Becomes visible)
   - Sharp "flash" or "crack" sound
   - 0.2-0.4 second duration
   - Medium volume (0.5-0.7)
   - Suggested: Energy discharge sound

3. **Spawn Sound** (Enemy appears)
   - Stealth deployment sound
   - 0.5-0.8 second duration
   - Suggested: Portal or teleport-in sound

4. **Hit Sound** (Takes damage)
   - Metallic impact
   - 0.1-0.3 second duration
   - Suggested: Light armor hit

5. **Death Sound** (Destroyed)
   - Electronic shutdown
   - 0.5-1.0 second duration
   - Suggested: Robot power-down

### Import and Assign

1. Import audio files to `Assets/Audio/Enemies/Cloaker/`
2. Set Audio Clips settings:
   - Load Type: **Decompress on Load** (for short sounds)
   - Compression Format: **Vorbis**
   - Quality: 70-80%
   
3. Assign to:
   - EnemyData fields (spawn, hit, death, ability)
   - CloakerEnemy component fields (cloak, uncloak)

---

## 5. Add to Wave Compositions

### Step 1: Locate Wave Data

Navigate to `Assets/Resources/Data/Waves/` and open existing WaveData assets

### Step 2: Add Cloaker to Waves

Cloakers should appear in:
- **Mid-game waves** (waves 10-20): Small groups (2-3)
- **Late-game waves** (waves 20-30): Larger groups (4-8)
- **Mixed compositions**: Paired with standard enemies

### Example Wave Composition

Wave 12:
```
Enemies:
- 10x Soldier Bot
- 3x Cloaker  ← New addition
- 5x Scout Bot
- 1x Tank Bot

Spawn Rate: 1.5 seconds between spawns
Health Multiplier: 1.8x
Speed Multiplier: 1.05x
```

Wave 18:
```
Enemies:
- 8x Elite Bot
- 6x Cloaker  ← Increased count
- 4x Tank Bot
- 2x Healer Bot

Spawn Rate: 1.2 seconds
Health Multiplier: 2.5x
Speed Multiplier: 1.15x
```

### Strategy Notes for Wave Design

- **Don't over-use**: Cloakers in every wave becomes repetitive
- **Pair with tanks**: Cloakers slip past while tanks absorb fire
- **Mix with healers**: Cloaked healers are extra dangerous
- **Progressive difficulty**: More cloakers in later waves
- **Boss waves**: 1-2 cloakers during boss fights add chaos

---

## 6. Testing

### Test 1: Basic Cloaking

1. Start Play mode in Game scene
2. Spawn Cloaker enemy
3. **Verify**:
   - Starts at 15% opacity (mostly invisible)
   - Cloak effect particles play
   - Health bar hidden when cloaked
   - Enemy moves normally along path

### Test 2: Uncloak on Damage

1. Place tower in range
2. Wait for tower to shoot Cloaker
3. **Verify**:
   - Cloaker becomes fully visible when hit
   - Uncloak effect particles play
   - Uncloak sound plays
   - Health bar appears
   - Stays visible for 2 seconds

### Test 3: Re-cloak Mechanic

1. Damage Cloaker, then stop shooting
2. Wait 5 seconds (2s uncloak + 3s cooldown)
3. **Verify**:
   - Cloaker re-cloaks after cooldown
   - Cloak effect plays again
   - Returns to 15% opacity
   - Health bar hides again

### Test 4: Detection Range

1. Place tower more than 4 units from enemy path
2. Spawn cloaked Cloaker
3. **Verify**:
   - Tower does NOT target cloaked Cloaker (out of detection range)

1. Place tower within 4 units of path
2. Spawn cloaked Cloaker
3. **Verify**:
   - Tower DOES target cloaked Cloaker (within detection range)
   - Tower fires and uncloaks enemy

### Test 5: Multiple Cloakers

1. Spawn 5 Cloakers in a group
2. **Verify**:
   - All cloak independently
   - Damage to one doesn't affect others
   - Towers prioritize based on targeting mode
   - Performance is acceptable (no lag)

### Test 6: Mixed Waves

1. Start wave with Cloakers + Tanks + Soldiers
2. **Verify**:
   - Towers target visible enemies first
   - Cloakers slip through when towers are busy
   - Strategic tower placement matters
   - Game feels challenging but fair

### Test 7: Mission Integration

1. Accept "Kill Enemies" mission
2. Kill Cloakers
3. **Verify**: Mission progress increases normally

1. Accept "Deal Damage" mission
2. Damage Cloakers
3. **Verify**: Damage tracking works correctly

---

## Troubleshooting

### Issue: Cloaker doesn't become transparent

**Check:**
- Material rendering mode set to Fade/Transparent
- Body Renderers array populated in CloakerEnemy component
- Material shader supports alpha blending
- CloakedAlpha value between 0-1 (try 0.15)

### Issue: Towers always target cloaked enemies

**Check:**
- Tower.cs modifications applied correctly
- CloakerEnemy.CanTarget() static method working
- DetectionRange value reasonable (4 units default)
- Tower.GetDetectionRange() returning correct value

### Issue: Cloaker never re-cloaks

**Check:**
- UncloakDuration set correctly (2 seconds)
- RecloakCooldown set correctly (3 seconds)
- Enemy not continuously taking damage
- Cloak transition code executing (no errors in console)

### Issue: Visual glitching during transitions

**Check:**
- Transition duration not too short (min 0.3s)
- Material properties correct (alpha, rendering mode)
- No conflicting shaders or materials
- Body renderers array has correct references

### Issue: Cloak effects don't play

**Check:**
- Particle systems assigned in CloakerEnemy component
- Particle systems set to Play On Awake: **No**
- Particle effects parented to enemy
- Simulation Space set to World (not Local)

### Issue: Sounds don't play

**Check:**
- Audio clips assigned in component
- Audio clips imported correctly
- Volume not set to 0
- AudioSource.PlayClipAtPoint working (no audio listener issues)

---

## Balance Tuning

### If Cloakers Are Too Strong:

- **Increase Detection Range**: Raise to 5-6 units (easier to detect)
- **Reduce Health**: Lower to 120-130 HP
- **Longer Uncloak Duration**: Keep visible 3-4 seconds
- **Shorter Recloak Cooldown**: Wait 2 seconds before recloak
- **Higher Cloaked Opacity**: Raise to 25-30% (more visible)
- **Reduce Speed**: Slow to 2.0 move speed

### If Cloakers Are Too Weak:

- **Decrease Detection Range**: Lower to 3 units (harder to detect)
- **Increase Health**: Raise to 180-200 HP
- **Shorter Uncloak Duration**: Visible only 1-1.5 seconds
- **Longer Recloak Cooldown**: Wait 4-5 seconds
- **Lower Cloaked Opacity**: Drop to 10% (nearly invisible)
- **Increase Speed**: Speed up to 2.8-3.0

### Recommended Starting Config:

**Balanced for mid-late game:**
- Health: 150 HP
- Speed: 2.5 units/sec
- Detection Range: 4 units
- Uncloak Duration: 2 seconds
- Recloak Cooldown: 3 seconds
- Cloaked Alpha: 15% (0.15)
- Reward: 40 credits

---

## Advanced: Special Tower Types

### Create Detection Tower

For advanced gameplay, create a tower that enhances detection:

```csharp
public class SensorTower : Tower
{
    [Header("Sensor Specific")]
    [SerializeField] private float enhancedDetectionRange = 10f;
    
    protected override float GetDetectionRange()
    {
        // Much larger detection range for cloakers
        return enhancedDetectionRange;
    }
    
    // This tower doesn't attack, just reveals cloakers
    protected override void SpawnProjectile()
    {
        // No projectile - sensor only
    }
}
```

**SensorTower Stats:**
- Cost: 200 credits
- Range: 8 units (normal)
- Detection Range: 10 units (large)
- No damage (support tower)
- Purpose: Reveals cloaked enemies for other towers

### Create True Sight Modifier

For Challenge Mode or special cases:

```csharp
// In Tower.cs, modify GetDetectionRange()
protected override float GetDetectionRange()
{
    // Check for "True Sight" buff/modifier
    if (HasTrueSight())
    {
        return float.MaxValue; // Can see all cloaked enemies
    }
    return base.GetDetectionRange();
}
```

---

## Strategic Gameplay Tips (for Documentation/Tutorial)

### For Players:

1. **Watch for Shimmer**: Cloaked enemies have slight visual tells
2. **Place Towers Close**: Detection range is limited (4 units)
3. **Focus Fire**: Once uncloaked, prioritize killing before re-cloak
4. **Use Slow Towers**: Freeze/slow keeps cloakers visible longer
5. **Cover Choke Points**: Place towers at path narrows for detection

### For Level Design:

1. **Introduce Gradually**: First cloaker at wave 10-12
2. **Teach Detection**: Early waves with 1-2 cloakers near towers
3. **Ramp Difficulty**: More cloakers in later waves
4. **Strategic Placement**: Design paths that require detection coverage
5. **Counter-play**: Provide tower spots at strategic positions

---

## Summary

Cloaker enemy adds stealth mechanics that:
- ✅ Test strategic tower placement
- ✅ Reward close-range tower coverage
- ✅ Create tension and unpredictability
- ✅ Integrate with all existing systems
- ✅ Provide progressive difficulty scaling

**Next Steps:**
1. Complete Unity asset setup (30-45 minutes)
2. Test all stealth mechanics
3. Balance detection range and transparency
4. Add to wave compositions
5. Create tutorial/tooltips for players
6. Update documentation

**Files Created:**
- `Assets/Scripts/Enemies/EnemyTypes.cs` (added CloakerEnemy class)
- `Assets/Scripts/Towers/Tower.cs` (added detection logic)
- This setup guide (documentation)

**Files Modified:**
- `Tower.cs`: Added GetDetectionRange() and cloaking checks

Cloaker is now ready for Unity Editor setup and in-game testing! 🕵️👻
