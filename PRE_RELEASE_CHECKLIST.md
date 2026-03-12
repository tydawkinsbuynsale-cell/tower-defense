# Pre-Release Checklist

**Robot Tower Defense v1.0.0**

Use this checklist to ensure all systems are working correctly before deployment.

---

## 🔧 Editor Validation (Tools → Robot TD → Integration Tests)

### Core Systems
- [ ] GameManager exists and initializes
- [ ] WaveManager exists and works
- [ ] SaveManager structure is valid
- [ ] EndlessMode is properly integrated

### Game Content
- [ ] 11 TowerData assets exist (one per tower type)
- [ ] 14 EnemyData assets exist (11 standard + 3 bosses)
- [ ] 5 MapData assets exist
- [ ] 5 map scenes exist and load

### Progression Systems
- [ ] AchievementManager is functional
- [ ] TechTree system works
- [ ] Save/load persistence works
- [ ] Achievement unlocks trigger correctly

### Performance & Polish
- [ ] PerformanceManager quality presets work
- [ ] TutorialManager completes all 9 steps
- [ ] FPS tracking is accurate
- [ ] Battery save mode activates at 20%

---

## 🎮 Runtime Testing (In Play Mode)

### Basic Gameplay Loop
- [ ] Game boots to main menu
- [ ] Can start a new game
- [ ] Tutorial starts on first launch
- [ ] Can place towers
- [ ] Can upgrade towers
- [ ] Can sell towers
- [ ] Waves spawn enemies correctly
- [ ] Enemies follow paths
- [ ] Towers target and shoot enemies
- [ ] Projectiles hit enemies
- [ ] Enemies die when health reaches 0
- [ ] Credits awarded on enemy kill
- [ ] Lives lost when enemy reaches end
- [ ] Wave completes when all enemies dead
- [ ] Can manually start next wave
- [ ] Victory triggers when all waves complete
- [ ] Game over triggers when lives reach 0

### UI/UX
- [ ] Main menu buttons work
- [ ] Settings menu opens and saves
- [ ] Graphics quality dropdown works
- [ ] Volume sliders affect audio
- [ ] Pause menu works
- [ ] Resume from pause works
- [ ] Restart from pause works
- [ ] Quit to menu works
- [ ] Tower info panel displays correct stats
- [ ] Wave counter displays correctly
- [ ] FPS counter is accurate (if enabled)
- [ ] Tutorial tooltips appear at right time
- [ ] Achievement notifications display

### Map System
- [ ] Can select maps from menu
- [ ] All 5 maps load correctly
- [ ] Each map has valid path waypoints
- [ ] Camera controls work (pan/zoom)
- [ ] Camera stays within bounds
- [ ] Tower placement zones highlighted correctly
- [ ] Invalid placement prevents tower creation

### Save/Load System
- [ ] Progress saves automatically
- [ ] Credits persist between sessions
- [ ] Tech points persist
- [ ] Achievements persist
- [ ] Tutorial completion persists
- [ ] Settings persist (graphics, audio)
- [ ] High score persists
- [ ] Can reset save from settings

### Tech Tree
- [ ] Tech tree UI displays correctly
- [ ] Can spend tech points
- [ ] Upgrades unlock correctly
- [ ] Locked upgrades show requirements
- [ ] Maxed upgrades show completion
- [ ] Descriptions show correct values
- [ ] Tech points earned per wave

### Achievement System
- [ ] Achievements unlock correctly
- [ ] Notifications display on unlock
- [ ] Achievement UI shows progress
- [ ] Secret achievements hidden
- [ ] All achievement IDs unique
- [ ] Completion percentage correct

### Audio
- [ ] Background music plays
- [ ] Music loops correctly
- [ ] Tower fire SFX plays
- [ ] Explosion SFX plays
- [ ] UI click SFX plays
- [ ] Volume sliders affect correct channels
- [ ] Mute toggle works
- [ ] Audio persists between scenes

### VFX
- [ ] Muzzle flashes display
- [ ] Explosion effects trigger
- [ ] Hit effects show on impact
- [ ] Particle effects don' t leak
- [ ] VFX optimized for mobile

### Endless Mode
- [ ] Can unlock endless mode
- [ ] Endless waves scale correctly
- [ ] Difficulty increases per wave
- [ ] Milestones trigger every 5 waves
- [ ] Bonus credits awarded at milestones
- [ ] Score posts to leaderboard (when integrated)
- [ ] Can exit endless mode

### Tutorial System
- [ ] Tutorial starts on first launch
- [ ] Step 1: Welcome message displays
- [ ] Step 2: Tower selection works
- [ ] Step 3: Tower placement works
- [ ] Step 4: Wave start works
- [ ] Step 5: Enemy targeting explained
- [ ] Step 6: Tower upgrade works
- [ ] Step 7: Tower sell works
- [ ] Step 8: Tech tree introduction
- [ ] Step 9: Tutorial completion triggers
- [ ] Can skip tutorial (if implemented)
- [ ] Tutorial doesn't restart on replay

### Performance
- [ ] Low quality: 30 FPS stable
- [ ] Medium quality: 60 FPS stable
- [ ] High quality: 60 FPS stable
- [ ] Auto-detect chooses correct preset
- [ ] Battery save activates at 20%
- [ ] No memory leaks during long sessions
- [ ] FPS doesn't drop during large waves
- [ ] GC doesn't cause stuttering
- [ ] Scene transitions smooth

---

## 📱 Android Device Testing

### Low-End Device (2GB RAM, 4 cores)
- [ ] Game boots successfully
- [ ] Auto-detects Low quality
- [ ] 30 FPS maintained
- [ ] No crashes during gameplay
- [ ] Memory usage < 512 MB
- [ ] Battery drain < 10%/hour
- [ ] Touch input responsive
- [ ] UI scales correctly

### Mid-Range Device (4GB RAM, 6 cores)
- [ ] Game boots successfully
- [ ] Auto-detects Medium quality
- [ ] 60 FPS maintained
- [ ] No crashes during gameplay
- [ ] Memory usage < 768 MB
- [ ] Battery drain < 15%/hour
- [ ] Touch input responsive
- [ ] UI scales correctly

### High-End Device (8GB+ RAM, 8+ cores)
- [ ] Game boots successfully
- [ ] Auto-detects High quality
- [ ] 60 FPS maintained
- [ ] All effects display correctly
- [ ] Memory usage < 1 GB
- [ ] Battery drain < 20%/hour
- [ ] Touch input responsive
- [ ] UI scales correctly
- [ ] Post-processing works

### Device Compatibility
- [ ] Landscape orientation forced
- [ ] Notch/cutout handled correctly
- [ ] Different aspect ratios supported (16:9, 18:9, 19.5:9, 21:9)
- [ ] Different screen resolutions work (720p, 1080p, 1440p)
- [ ] Android 5.0+ supported
- [ ] ARMv7 and ARM64 builds work
- [ ] Google Play Services integrated (if using leaderboards)

---

## 🏗️ Build Validation

### APK Build
- [ ] APK builds without errors
- [ ] APK size < 150 MB
- [ ] All scenes included in build
- [ ] All assets bundled correctly
- [ ] Version number correct in manifest
- [ ] Bundle ID correct (`com.yourstudio.robottd`)
- [ ] Min SDK version set (API 21 / Android 5.0)
- [ ] Target SDK version set (API 33 / Android 13)
- [ ] Icons display correctly in launcher
- [ ] APK installs on device
- [ ] APK runs on device

### AAB Build (App Bundle)
- [ ] AAB builds without errors
- [ ] AAB size < 150 MB
- [ ] All scenes included
- [ ] All assets bundled correctly
- [ ] Version number correct
- [ ] Bundle ID correct
- [ ] Signing configured correctly
- [ ] AAB accepted by Play Console

### Code Stripping
- [ ] IL2CPP enabled for release
- [ ] Code stripping level: Medium or High
- [ ] No missing types at runtime
- [ ] Reflection works correctly (SaveManager, etc.)

---

## 📋 Assets Checklist

### Icons & Graphics
- [ ] App icon 432×432 (adaptive)
- [ ] App icon 192×192 (legacy)
- [ ] App icon 144×144 (legacy)
- [ ] App icon 96×96 (legacy)
- [ ] App icon 72×72 (legacy)
- [ ] App icon 48×48 (legacy)
- [ ] Feature graphic 1024×500 (Play Store)
- [ ] Screenshots (at least 2, up to 8)
- [ ] Background for adaptive icon
- [ ] Foreground for adaptive icon

### Tower Sprites/Models
- [ ] Basic Gun Tower
- [ ] Sniper Tower
- [ ] Missile Tower
- [ ] Flamethrower Tower
- [ ] Tesla Tower
- [ ] Laser Tower
- [ ] Ice Tower
- [ ] Poison Tower
- [ ] Artillery Tower
- [ ] Shield Generator Tower
- [ ] Radar Tower

### Enemy Sprites/Models
- [ ] Scout (light, fast)
- [ ] Soldier (standard)
- [ ] Tank (heavy, slow)
- [ ] Drone (flying)
- [ ] Healer (support)
- [ ] Rusher (very fast)
- [ ] Splitter (clones on death)
- [ ] Stealth (invisible)
- [ ] Shielded (has shields)
- [ ] Regenerating (heals over time)
- [ ] Emp (disables towers)
- [ ] Boss 1 (large robot)
- [ ] Boss 2 (mega tank)
- [ ] Boss 3 (flying fortress)

### UI Graphics
- [ ] Main menu background
- [ ] Game HUD background
- [ ] Button states (normal, hover, pressed, disabled)
- [ ] Panel backgrounds (semi-transparent)
- [ ] Tower info panel
- [ ] Achievement icons
- [ ] Tech tree icons
- [ ] Health bar sprite
- [ ] Currency icon (credits)
- [ ] Life icon (hearts)
- [ ] Wave indicator

### Audio Assets
- [ ] Main menu music
- [ ] Gameplay music (3+ tracks)
- [ ] Boss music
- [ ] Victory music
- [ ] Game over music
- [ ] Tower fire SFX (11 types)
- [ ] Explosion SFX (3 sizes)
- [ ] UI click SFX
- [ ] UI hover SFX
- [ ] Enemy death SFX (3 types)
- [ ] Wave start SFX
- [ ] Wave complete SFX
- [ ] Achievement unlock SFX
- [ ] Credit collect SFX
- [ ] Life lost SFX

### VFX Assets
- [ ] Muzzle flash effect (tower fire)
- [ ] Explosion effect (small, medium, large)
- [ ] Hit spark effect
- [ ] Shield effect
- [ ] Heal effect
- [ ] Ice effect (slow)
- [ ] Poison effect (DOT)
- [ ] Laser beam effect
- [ ] Tesla arc effect
- [ ] Missile trail effect

---

## 📱 Play Store Preparation

### Store Listing
- [ ] App title (max 50 chars)
- [ ] Short description (max 80 chars)
- [ ] Full description (max 4000 chars)
- [ ] Feature graphic (1024×500)
- [ ] Screenshots (2-8 images)
- [ ] Video trailer (optional)
- [ ] Category selected (Games → Strategy)
- [ ] Content rating questionnaire completed
- [ ] Privacy policy URL
- [ ] Contact email
- [ ] Website URL (optional)

### Release Management
- [ ] Version code incremented
- [ ] Version name set (1.0.0)
- [ ] Release notes written
- [ ] Internal testing track published
- [ ] Closed testing track published (optional)
- [ ] Open testing track published (optional)
- [ ] Production track ready

### Compliance
- [ ] Privacy policy created and hosted
- [ ] COPPA compliant (if targeting children)
- [ ] GDPR compliant (if EU users)
- [ ] No copyrighted assets used
- [ ] All third-party licenses included
- [ ] Analytics opt-out available
- [ ] Data collection disclosed

---

## 🐛 Known Issues Log

Document any known issues that don't block release:

| Issue | Severity | Workaround | Target Fix Version |
|-------|----------|------------|-------------------|
| _Example: FPS drops on wave 30+ with 50+ towers_ | _Low_ | _Don't place too many towers_ | _v1.1.0_ |

---

## ✅ Final Sign-Off

**Before clicking "Release to Production":**

- [ ] All critical tests passing
- [ ] No game-breaking bugs
- [ ] All assets finalized
- [ ] Performance acceptable on target devices
- [ ] Privacy policy published
- [ ] Store listing complete
- [ ] APK/AAB signed with production keystore
- [ ] Version number correct
- [ ] Release notes written
- [ ] Team has reviewed and approved

**Sign-off:**
- **Developer:** ________________ Date: ________
- **QA:** ________________ Date: ________
- **PM:** ________________ Date: ________

---

## 📈 Post-Launch Monitoring

**First 24 Hours:**
- [ ] Monitor crash reports (Play Console)
- [ ] Check user reviews
- [ ] Verify analytics working
- [ ] Monitor download count
- [ ] Check for payment issues (if IAP)

**First Week:**
- [ ] Review crash-free rate (target >99%)
- [ ] Check retention metrics (D1, D7)
- [ ] Monitor average session length
- [ ] Track completion rates
- [ ] Identify pain points from reviews

**First Month:**
- [ ] Plan hotfix if needed
- [ ] Plan content updates (v1.1.0)
- [ ] Analyze performance metrics
- [ ] Collect feature requests
- [ ] Build roadmap for v1.2.0

---

**Last Updated:** 2024
**For Version:** 1.0.0
