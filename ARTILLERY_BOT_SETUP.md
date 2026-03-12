# Artillery Bot Setup Guide

This guide walks through setting up the **Artillery Bot** tower in Unity Editor. Artillery Bot is a long-range siege tower that fires shells in a parabolic arc, dealing splash damage on impact.

## Overview

**Artillery Bot Features:**
- Long-range targeting (18 units, furthest in game)
- Parabolic arc trajectory (fires over obstacles)
- Splash damage with falloff (2.5 unit radius)
- Minimum range limitation (3 units, can't hit close enemies)
- Slow fire rate (2.5s cooldown) balanced by high damage
- Visual barrel aiming with elevation
- Realistic shell physics

**Strategic Role:**
- Area denial against grouped enemies
- Long-range map coverage
- Pairs well with slow/freeze towers
- Weak against fast or close-range enemies

---

## Quick Setup Checklist

- [ ] Create TowerData ScriptableObject
- [ ] Create Artillery Bot tower prefab
- [ ] Create Artillery Shell projectile prefab
- [ ] Create explosion VFX prefab
- [ ] Add UI button to tower shop
- [ ] Create tower icon sprite
- [ ] Assign audio clips
- [ ] Test in-game

---

## 1. Create TowerData ScriptableObject

### Step 1: Create the Asset

1. In Unity Project window, navigate to `Assets/Resources/Data/Towers/`
2. Right-click → **Create → RobotTD → Tower Data**
3. Name it: `ArtilleryBot`

### Step 2: Configure Settings

Select the ArtilleryBot asset and configure in Inspector:

#### Info Section
```
Tower Name: Artillery Bot
Description: Long-range siege tower. Fires shells in a high arc that deal splash damage on impact. Cannot hit close enemies.
Icon: [Your sprite asset]
Tower Type: ArtilleryBot
```

#### Base Stats
```
Cost: 300
Base Damage: 150.0
Base Range: 18.0
Base Fire Rate: 0.4  (one shot every 2.5 seconds)
Rotation Speed: 3.0  (slower barrel turning)
```

#### Upgrades
```
Max Level: 3
Upgrade Costs: [200, 400, 800]
Damage Upgrade Percent: 0.30  (30% increase per level)
Range Upgrade Percent: 0.10   (10% increase per level)
Fire Rate Upgrade Percent: 0.10  (10% faster per level)
```

#### Targeting
```
Target Priority: First
Can Target Flying: Yes
Can Target Ground: Yes
```

#### Special Properties
```
Slow Percent: 0
Slow Duration: 0
Splash Radius: 2.5
Splash Damage Percent: 0.5  (50% damage to splash targets)
Chain Count: 0
Chain Range: 0
Dot Damage: 0
Dot Duration: 0
```

#### Visuals
```
Projectile Prefab: [Link to ArtilleryShell prefab - see step 3]
Muzzle Flash Prefab: [Optional smoke/fire effect]
Tower Color: RGB(100, 100, 120) - dark grey
Projectile Color: RGB(255, 200, 100) - orange-yellow
```

#### Audio
```
Fire Sound: [Artillery firing sound clip]
Upgrade Sound: [Mechanical upgrade sound]
Place Sound: [Heavy placement thud]
```

---

## 2. Create Artillery Bot Tower Prefab

### Step 1: Create GameObject Hierarchy

1. In Hierarchy, right-click → **3D Object → Cylinder** (or use custom mesh)
2. Rename to: `ArtilleryBot`
3. Add child objects:
   ```
   ArtilleryBot (root)
   ├─ Base (rotating platform)
   ├─ Barrel (gun that aims up/down)
   │  └─ FirePoint (empty, where shells spawn)
   ├─ RangeIndicator (circle mesh)
   └─ VFX (particle effects)
   ```

### Step 2: Add Components

Select the root **ArtilleryBot** GameObject:

1. Add Component → **ArtilleryBot** (script)
2. Configure in Inspector:

```
Artillery Specific:
  Splash Radius: 2.5
  Arc Height: 5.0
  Impact Effect: [Link to explosion prefab - step 4]
  Min Range: 3.0

Visual Effects:
  Barrel Transform: [Drag Barrel child object]
  Barrel Rotation Speed: 3.0
  Muzzle Flash: [Optional particle system]
  Fire Sound: [Link same as TowerData]
  Impact Sound: [Explosion sound clip]
```

3. Add Component → **Tower** (base class requirements)
4. Link TowerData reference: Drag `ArtilleryBot` TowerData asset

### Step 3: Position Objects

- **Base**: Scale (1.5, 0.5, 1.5) - wide flat platform
- **Barrel**: Position (0, 0.8, 0), Scale (0.3, 1.2, 0.3) - cylinder pointing up
- **FirePoint**: Position (0, 1.5, 0) - at tip of barrel
- **RangeIndicator**: Scale (36, 0, 36) - range * 2 for diameter

### Step 4: Save Prefab

1. Drag ArtilleryBot from Hierarchy to `Assets/Prefabs/Towers/`
2. Delete from scene

---

## 3. Create Artillery Shell Projectile Prefab

### Step 1: Create GameObject

1. In Hierarchy, right-click → **3D Object → Sphere**
2. Rename to: `ArtilleryShell`
3. Scale to (0.4, 0.4, 0.4) - small shell

### Step 2: Add Components

1. Add Component → **ArtilleryProjectile** (script)
2. Configure:

```
Artillery Shell Settings:
  Lifetime: 8.0
  Trail Effect: [Optional particle trail]
  Shell Model: [Link to sphere child or custom mesh]
  Rotation Speed: 360.0  (spins during flight)
```

3. Add Component → **Rigidbody**
   - Set Is Kinematic: **Yes**
   - Disable Gravity

4. Add Component → **Sphere Collider**
   - Set Is Trigger: **Yes**
   - Radius: 0.5

### Step 3: Add Visual Effects

- Add particle system child for smoke trail
- Set trail to follow projectile
- Adjust color to match TowerData settings

### Step 4: Save Prefab

1. Drag ArtilleryShell to `Assets/Prefabs/Projectiles/`
2. Link this prefab to TowerData → Projectile Prefab field
3. Delete from scene

---

## 4. Create Explosion VFX Prefab

### Step 1: Create Particle System

1. In Hierarchy, right-click → **Effects → Particle System**
2. Rename to: `ArtilleryExplosion`

### Step 2: Configure Particles

```
Main Module:
  Duration: 1.0
  Start Lifetime: 0.5-1.0
  Start Speed: 5-10
  Start Size: 1.0-2.0
  Start Color: Gradient (orange → yellow → grey)
  Gravity Modifier: 0.5
  Max Particles: 50

Emission:
  Rate over Time: 0
  Bursts: 1 burst at 0.00, 30-50 particles

Shape:
  Shape: Sphere
  Radius: 0.5
  
Color over Lifetime:
  Gradient: Full opacity → Fade out

Size over Lifetime:
  Curve: Start small → Peak → Shrink
```

### Step 3: Add Secondary Effects

Add child particle systems for:
- **Smoke**: Grey, slower, larger particles, longer lifetime
- **Sparks**: Small, fast, orange particles
- **Shockwave**: Single ring that expands rapidly

### Step 4: Save Prefab

1. Drag ArtilleryExplosion to `Assets/Prefabs/VFX/`
2. Link to ArtilleryBot script → Impact Effect field
3. Delete from scene

---

## 5. Add UI Button to Tower Shop

### Step 1: Locate Tower Shop UI

Open scene: `Assets/Scenes/Game.unity`

Navigate in Hierarchy:
```
Canvas
└─ TowerShop
   └─ TowerButtons
```

### Step 2: Duplicate Existing Button

1. Right-click on an existing tower button → Duplicate
2. Rename to: `ArtilleryBotButton`
3. Reposition in layout

### Step 3: Configure Button

Select ArtilleryBotButton:

1. **Image** component:
   - Source Image: [Artillery Bot icon sprite]
   
2. **TowerButton** component:
   - Tower Type: `ArtilleryBot`
   - Tower Data: [Link ArtilleryBot TowerData asset]
   - Cost Text: Should auto-update to "300"

### Step 4: Test Button

1. Enter Play mode
2. Click Artillery Bot button
3. Should highlight and show range indicator

---

## 6. Create Tower Icon Sprite

### Option A: Use Built-in Icon

1. Create simple icon in image editor (64x64 or 128x128)
2. Save as PNG: `artillery_bot_icon.png`
3. Import to Unity: `Assets/Sprites/UI/TowerIcons/`
4. Set Texture Type: **Sprite (2D and UI)**
5. Link to TowerData → Icon field

### Option B: In-Game Render

1. Create Artillery Bot tower in scene
2. Position camera for good angle
3. Use Unity's built-in screenshot or camera render texture
4. Process and import as sprite

### Recommended Icon Design

- **Subject**: View of barrel from 45° angle
- **Background**: Dark grey or transparent
- **Accent**: Orange/yellow highlight on barrel
- **Badge**: Artillery shell icon in corner (optional)

---

## 7. Assign Audio Clips

### Required Audio Clips

1. **Fire Sound** (Artillery firing)
   - Deep bass "BOOM" sound
   - 0.5-1.0 second duration
   - Suggested: Cannon or mortar fire sound

2. **Impact Sound** (Explosion)
   - Sharp explosion crack
   - 0.3-0.7 second duration
   - Suggested: Grenade or bomb explosion

3. **Upgrade Sound** (Tower upgrade)
   - Mechanical whirring/clanking
   - 1.0-1.5 second duration
   - Can reuse from other towers

4. **Place Sound** (Tower placement)
   - Heavy metallic thud
   - 0.3-0.5 second duration
   - Deeper than other towers

### Import and Assign

1. Import audio files to `Assets/Audio/Towers/ArtilleryBot/`
2. Set Audio Clips settings:
   - Load Type: **Decompress on Load** (for short sounds)
   - Compression Format: **Vorbis**
   
3. Assign to TowerData and ArtilleryBot script

---

## 8. Testing

### Test 1: Basic Functionality

1. Start Play mode in Game scene
2. Place Artillery Bot on map
3. **Verify**:
   - Tower appears with correct model
   - Range indicator shows ~18 units
   - Enemies enter range
   - Barrel aims at enemy
   - Shell fires in arc
   - Shell impacts with explosion
   - Enemies take splash damage

### Test 2: Minimum Range

1. Place Artillery Bot
2. Spawn enemy close to tower (< 3 units)
3. **Verify**:
   - Tower does NOT target close enemy
   - Tower only fires when enemies are 3+ units away

### Test 3: Splash Damage

1. Spawn group of enemies close together
2. Fire artillery at group
3. **Verify**:
   - All enemies in 2.5 unit radius take damage
   - Damage falls off based on distance from impact
   - Enemies at edge take ~50% of center damage

### Test 4: Upgrades

1. Place Artillery Bot
2. Upgrade to level 2, then 3
3. **Verify**:
   - Cost deducted correctly (200, 400)
   - Damage increases ~30% per level
   - Range increases ~10% per level
   - Fire rate increases ~10% per level
   - Visual changes (optional glow/effects)

### Test 5: Arc Trajectory

1. Place Artillery Bot at different heights
2. Target enemies at various distances
3. **Verify**:
   - Shell arcs higher for longer distances
   - Arc clears ground obstacles
   - Flight time increases with distance (0.5-2.5s)
   - Trajectory looks natural and smooth

### Test 6: Mission Integration

1. Accept "Place Towers" mission
2. Place Artillery Bot
3. **Verify**: Mission progress increases

1. Accept "Use Artillery Bot" mission (if available)
2. Use Artillery Bot in game
3. **Verify**: Mission tracks usage

### Test 7: Challenge Mode Integration

1. Start challenge with "Limited Arsenal" modifier
2. **Verify**: Artillery Bot included in allowed towers (if applicable)

1. Start challenge with tower cost modifier (+50%)
2. **Verify**: Artillery Bot costs 450 (300 * 1.5)

---

## Troubleshooting

### Issue: Shell doesn't fire

**Check:**
- Projectile Prefab linked in TowerData
- FirePoint position is correct (at barrel tip)
- ArtilleryProjectile script attached to projectile prefab
- Enemy is within range AND beyond minimum range (3+ units)

### Issue: No splash damage

**Check:**
- Splash Radius set to 2.5 in TowerData
- ArtilleryBot.DealSplashDamage() method being called
- Enemies have colliders
- Layer settings allow projectile to hit enemies

### Issue: Shell doesn't arc

**Check:**
- Arc Height set to 5.0 in ArtilleryBot script
- ArtilleryProjectile using parabolic trajectory formula
- Flight duration calculated correctly
- Not using base Projectile class (must use ArtilleryProjectile)

### Issue: Targets too-close enemies

**Check:**
- Min Range set to 3.0 in ArtilleryBot script
- FindTarget() override is working
- No other Tower base class targeting overriding it

### Issue: Barrel doesn't aim

**Check:**
- Barrel Transform linked in ArtilleryBot script
- Barrel is separate child GameObject (not root)
- Barrel Rotation Speed > 0
- Barrel pivot point at base (not center)

### Issue: Explosion doesn't show

**Check:**
- Impact Effect prefab linked in ArtilleryBot script
- Explosion prefab has ParticleSystem component
- Particle system set to "Play On Awake"
- Explosion lifetime > particle duration

---

## Balance Tuning

### If Artillery Bot is Too Strong:

- **Reduce Range**: Lower to 15-16 units
- **Increase Cost**: Raise to 350-400 credits
- **Reduce Damage**: Lower to 120-130 base damage
- **Increase Fire Rate Cooldown**: Slow to 3.0-3.5 seconds
- **Reduce Splash Radius**: Lower to 2.0 units
- **Increase Minimum Range**: Raise to 4-5 units

### If Artillery Bot is Too Weak:

- **Increase Damage**: Raise to 180-200 base damage
- **Reduce Cost**: Lower to 250-280 credits
- **Increase Splash Radius**: Raise to 3.0 units
- **Increase Splash Damage Percent**: Raise to 60-70%
- **Reduce Fire Rate Cooldown**: Speed to 2.0 seconds
- **Reduce Minimum Range**: Lower to 2.0 units

### Recommended Starting Config:

**Balanced for mid-game deployment:**
- Cost: 300 credits (save for waves 8-12)
- Damage: 150 + 30% per level
- Range: 18 units + 10% per level
- Fire Rate: 0.4/sec (2.5s cooldown)
- Splash: 2.5 radius, 50% damage

---

## Advanced: Custom Improvements

### Add Camera Shake on Impact

In `CameraController.cs`, add:

```csharp
public void Shake(float intensity)
{
    StartCoroutine(ShakeCoroutine(intensity));
}

private IEnumerator ShakeCoroutine(float intensity)
{
    Vector3 originalPos = transform.localPosition;
    float elapsed = 0f;
    float duration = 0.3f;
    
    while (elapsed < duration)
    {
        float x = Random.Range(-1f, 1f) * intensity;
        float y = Random.Range(-1f, 1f) * intensity;
        
        transform.localPosition = originalPos + new Vector3(x, y, 0);
        
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    transform.localPosition = originalPos;
}
```

### Add Crater Decal on Impact

1. Create decal prefab with crater texture
2. In ArtilleryProjectile.OnImpact():
   ```csharp
   GameObject crater = Instantiate(craterDecalPrefab, targetPosition, Quaternion.identity);
   Destroy(crater, 10f); // Remove after 10 seconds
   ```

### Add Wind-Up Animation

1. In ArtilleryBot, add barrel recoil:
   ```csharp
   StartCoroutine(BarrelRecoil());
   
   IEnumerator BarrelRecoil()
   {
       Vector3 originalPos = barrelTransform.localPosition;
       barrelTransform.localPosition += Vector3.back * 0.3f;
       yield return new WaitForSeconds(0.2f);
       barrelTransform.localPosition = originalPos;
   }
   ```

### Add Prediction Line

1. In ArtilleryBot, draw line showing predicted arc:
   ```csharp
   LineRenderer lineRenderer;
   
   void ShowTrajectoryPreview()
   {
       for (int i = 0; i < 20; i++)
       {
           float t = i / 20f;
           Vector3 point = CalculateArcPoint(t);
           lineRenderer.SetPosition(i, point);
       }
   }
   ```

---

## Summary

Artillery Bot adds a unique long-range siege tower with:
- ✅ Arc projectile physics
- ✅ Splash damage with falloff
- ✅ Minimum range tactical limitation
- ✅ Visual barrel aiming
- ✅ Integration with all game systems

**Next Steps:**
1. Complete Unity asset setup (30-60 minutes)
2. Test all scenarios
3. Balance damage/cost values
4. Create promotional screenshots
5. Update README with new tower count

**Files Created:**
- `Assets/Scripts/Towers/ArtilleryBot.cs` (tower logic)
- `Assets/Scripts/Projectiles/ArtilleryProjectile.cs` (projectile physics)
- This setup guide (documentation)

**Files Modified:**
- `Assets/Scripts/Towers/TowerData.cs` (added enum value)
- `CHANGELOG.md` (added Artillery Bot section)

Artillery Bot is now ready for Unity Editor setup and in-game testing! 🎯💣
