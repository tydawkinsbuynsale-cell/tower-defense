# Robot Tower Defense

A premium mobile tower defense game featuring futuristic robotic warfare. Strategically place intelligent combat robots to defend against waves of hostile machines.

**Platform:** Android (Primary) | iOS (Secondary)  
**Engine:** Unity 2022.3 LTS  
**Language:** C# 9.0+  
**Status:** Production-Ready

---

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [System Architecture](#system-architecture)
- [Setup Instructions](#setup-instructions)
- [Building for Android](#building-for-android)
- [Project Structure](#project-structure)
- [Core Systems](#core-systems)
- [Development Workflow](#development-workflow)
- [Performance Optimization](#performance-optimization)
- [Testing](#testing)
- [Contributing](#contributing)

---

## 🎮 Project Overview

### Features

**✅ Complete Tower System**
- 12 unique tower types with distinct abilities
- 3-tier upgrade system for each tower
- 5 targeting priority modes (First, Last, Strongest, Weakest, Closest)
- Advanced placement system with visual feedback

**✅ Enemy System**
- 12 enemy types with varied behaviors
- 3 boss encounters with special abilities
- Dynamic difficulty scaling across 30 waves per map
- Status effects: slow, stun, burn, shield
- Stealth mechanics with cloaking/detection system

**✅ Map System**
- 6 campaign maps with 180 waves total
- Dynamic wave composition based on map difficulty
- MapRegistry for extensible map library
- Path validation and placement grid system

**✅ Progression System**
- 65 achievements with bronze/silver/gold tiers
- Tech tree with 15 permanent upgrades
- Save/load system with backup protection
- Tech points earned from wave completion

**✅ UI System**
- 10 polished UI screens (Main Menu, Game HUD, Pause, Settings, etc.)
- Achievement toast notifications
- Tower info panels with real-time stats
- Wave result screen with performance metrics

**✅ Tutorial System**
- 9-step interactive tutorial for first-time players
- Auto-start for new players
- Manual and auto-advance progression modes
- Spotlight/dimmed overlay system for UI highlighting

**✅ Performance System**
- 3 quality presets (Low/Medium/High) with auto-detection
- Frame rate management (30/60 FPS)
- Battery save mode (auto-switches to 30 FPS when low)
- Real-time performance metrics tracking

**✅ Audio System**
- Music and SFX with volume controls
- Positional audio for spatial awareness
- Audio pooling for performance

**✅ Editor Tools**
- 6 custom editor tools for rapid development
- Android build configuration tool
- Scene hierarchy builder
- Asset processor and validator

**✅ Online Systems**
- Authentication system (Anonymous, Email, Device ID)
- Cloud save synchronization
- Leaderboards (global and friend-only)
- Multi-backend support (Unity Gaming Services, PlayFab, Custom)

**✅ Mission Systems**
- Daily missions (3 missions, 24-hour rotation)
- Weekly missions (3 missions, 7-day rotation)
- Challenge mode with special modifiers
- Boss rush mode
- Tab-based mission UI

**✅ Social Features**
- Friend management (send/accept requests, remove friends)
- Friend leaderboards (view friends-only scores)
- Player search functionality
- Score and achievement sharing
- Max 100 friends, 50 pending requests

**✅ Custom Map Editor System**
- Grid-based map creation tool (configurable size, default 20x15)
- 7 tile types: Buildable, Path, Obstacle, Water, SpawnPoint, Base, Decoration
- Multiple editor modes: Tile, Path, Spawn, Base, Obstacle, Decoration
- Interactive tools: Single tile, Brush (1x1 to 5x5), Flood fill
- Path drawing system with drag-to-draw functionality
- Undo/Redo system (50-step history)
- Auto-save functionality (60-second intervals)
- Comprehensive map validation:
  - Path connectivity verification (spawn to base)
  - Spawn point and base requirements
  - Buildable space analysis
  - Error/warning/suggestion system
- Map properties configuration:
  - Name, description, author metadata
  - Starting credits/lives customization
  - Difficulty rating (1-5)
  - Estimated play time
- Custom wave configuration:
  - Wave-by-wave enemy composition editor
  - 12 enemy types: Scout, Soldier, Tank, Elite, Flying, Healer, Splitter, Teleporter, HeavyAssault, SwarmMother, ShieldCommander, Cloaker
  - Enemy group configuration (count, spawn interval, spawn point)
  - Boss wave setup with 3 boss types
  - Credits reward per wave
  - Quick generation tool (5-30 waves, 5 difficulty presets)
  - Automatic wave scaling and progression
  - Comprehensive validation system
- Test Play system:
  - In-editor map testing
  - Custom wave playback
  - Return to editor from game
  - Session data tracking
- Local storage system:
  - Save/Load up to 100 custom maps
  - JSON file format with automatic thumbnail generation
  - Thumbnails: 256x256 PNG top-down captures
  - Automatic backup system (5 backups per map)
  - Export/Import maps for sharing
  - Map library browser with search/filter
  - Sort by date, name, play count, rating
  - Storage usage tracking
- Community features:
  - Publish maps to cloud (max 50/user)
  - Community browser with search/filter
  - Rating system (1-5 stars)
  - Like/unlike maps
  - Play count tracking
  - Tag-based categorization
  - Sort by: recent, played, rated, liked, name
  - Download and play community maps
- Keyboard shortcuts: Ctrl+Z/Y (undo/redo), Ctrl+S (save)
- Analytics integration: 50 events (20 editor + 8 wave config + 1 thumbnail + 6 community + 3 Steam + 5 events + 7 tournament)

**✅ Seasonal Events System**
- Limited-time events (Halloween, Christmas, etc.)
- Event-specific challenges (10 types)
- Event currency system (earn & spend)
- Exclusive cosmetic rewards (skins, avatars, titles)
- Automatic event activation based on dates
- Progress tracking and persistence
- 7 event types: Halloween, Christmas, NewYear, Easter, Summer, Anniversary, Special

**✅ Steam Integration**
- Achievement synchronization with Steam (70+ achievements)
- Stats tracking (kills, waves, playtime, etc.)
- Leaderboard integration (high scores, waves, kills)
- Automatic sync on achievement unlock
- Editor simulation mode for testing
- Steamworks.NET SDK support

**✅ Monetization Systems**
- In-App Purchases (13 products: gems, credits, skins, subscriptions)
- Ad monetization (interstitial, rewarded, banner)
- Power-Ups system (5 types with IAP/Ad integration)
- Shop UI with multiple tabs

---

## 🏗️ System Architecture

### Core Managers (Singleton Pattern)

```
GameManager          - Central hub for game state, economy, and events
WaveManager          - Wave spawning, progression, and enemy coordination
TowerPlacementManager - Tower placement validation and management
SaveManager          - Persistent data with JSON serialization
PerformanceManager   - Quality presets, FPS management, memory optimization
TutorialManager      - First-time player onboarding
AchievementManager   - Achievement tracking and unlocking
TechTree             - Permanent progression system
AudioManager         - Audio playback and mixing
VFXManager           - Visual effects pooling
ObjectPooler         - Generic object pooling system
InputManager         - Touch/mouse input handling
CameraController     - Camera pan, zoom, and boundaries
```

### Data-Driven Design

- **ScriptableObjects** for all game data:
  - `TowerData` - Tower stats, cost, upgrade paths
  - `EnemyData` - Enemy stats, behaviors, resistances
  - `WaveSetData` - Wave composition templates
  - `MapData` - Map configuration, wave sets, difficulty
- **JSON Serialization** for save data
- **Factory Pattern** for tower/enemy instantiation

### Event-Driven Architecture

```csharp
// GameManager Events
public static event System.Action<int> OnCreditsChanged;
public static event System.Action<int> OnLivesChanged;
public static event System.Action<int> OnScoreChanged;
public static event System.Action<GameState> OnGameStateChanged;
public static event System.Action OnGamePaused;
public static event System.Action OnGameResumed;

// WaveManager Events
public static event System.Action<int> OnWaveStarted;
public static event System.Action<int> OnWaveCompleted;
public static event System.Action<int> OnAllWavesCompleted;

// AchievementManager Events
public static event System.Action<Achievement> OnAchievementUnlocked;
```

---

## 🛠️ Setup Instructions

### Prerequisites

1. **Unity 2022.3 LTS** (or newer)
2. **Android Build Support** module installed
3. **Git** for version control
4. **Visual Studio 2022** or **JetBrains Rider** (recommended)

### Initial Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/tydawkinsbuynsale-cell/tower-defense.git
   cd tower-defense
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Add" → Select the `RobotTowerDefense` folder
   - Wait for Unity to import assets (first import may take 5-10 minutes)

3. **Configure Project Settings**
   - Open `Edit → Project Settings`
   - Verify **Player Settings** → **Scripting Backend**: IL2CPP
   - Verify **Quality Settings** presets exist (Low/Medium/High)

4. **Test in Editor**
   - Open `Scenes/MainMenu`
   - Press Play
   - Verify all UI elements load correctly

---

## 📱 Building for Android

### Quick Build (Automated)

1. Open `Tools → Robot TD → Android Build Config`
2. Configure settings:
   - **Bundle ID**: `com.yourstudio.robottowerdefense`
   - **Version**: `1.0.0`
   - **Min SDK**: Android 7.0 (API 24)
   - **Target SDK**: Android 14 (API 34)
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARM64 ✅, ARMv7 ✅
3. Click **Apply Settings**
4. Click **Build AAB** for Play Store submission

### Manual Build

1. **Player Settings** (`Edit → Project Settings → Player`)
   ```
   Company Name: Your Studio
   Product Name: Robot Tower Defense
   Bundle Identifier: com.yourstudio.robottowerdefense
   Version: 1.0.0
   Bundle Version Code: 1
   ```

2. **Android Settings**
   ```
   Minimum API Level: Android 7.0 (API 24)
   Target API Level: Android 14 (API 34)
   Scripting Backend: IL2CPP
   Target Architectures: ARM64, ARMv7
   ```

3. **Graphics Settings**
   ```
   Graphics APIs: Vulkan, OpenGL ES 3
   Multithreaded Rendering: ✅
   GPU Skinning: ✅
   ```

4. **Optimization**
   ```
   Strip Engine Code: ✅
   Managed Stripping Level: Medium
   Optimize Mesh Data: ✅
   ```

5. **Build**
   - `File → Build Settings → Android → Build`
   - Select output location
   - Wait for build completion (~10-30 minutes for first IL2CPP build)

### Required Icons

**Android Adaptive Icons:**
- Foreground: 432×432 px (transparent PNG with 72px safe zone)
- Background: 432×432 px (solid color or simple pattern)

**Legacy Icons:**
- xxxhdpi: 192×192 px
- xxhdpi: 144×144 px
- xhdpi: 96×96 px
- hdpi: 72×72 px
- mdpi: 48×48 px

**Place in:** `Assets/Art/Icons/`

---

## 📂 Project Structure

```
RobotTowerDefense/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # Game managers and core systems
│   │   │   ├── GameManager.cs
│   │   │   ├── WaveManager.cs
│   │   │   ├── SaveManager.cs
│   │   │   ├── PerformanceManager.cs
│   │   │   ├── TutorialManager.cs
│   │   │   ├── ObjectPooler.cs
│   │   │   ├── SceneBootstrapper.cs
│   │   │   └── EndlessMode.cs
│   │   │
│   │   ├── Towers/            # Tower system
│   │   │   ├── Tower.cs       # Base tower component
│   │   │   ├── TowerData.cs   # ScriptableObject for tower stats
│   │   │   ├── TowerTypes.cs  # Tower type definitions
│   │   │   └── TowerPlacementManager.cs
│   │   │
│   │   ├── Enemies/           # Enemy system
│   │   │   ├── Enemy.cs       # Base enemy component
│   │   │   ├── EnemyData.cs   # ScriptableObject for enemy stats
│   │   │   └── EnemyTypes.cs  # Enemy type definitions
│   │   │
│   │   ├── Projectiles/       # Projectile system
│   │   │   └── Projectile.cs
│   │   │
│   │   ├── Map/               # Map and navigation
│   │   │   ├── MapManager.cs
│   │   │   ├── CameraController.cs
│   │   │   └── InputManager.cs
│   │   │
│   │   ├── Progression/       # Progression systems
│   │   │   ├── AchievementManager.cs
│   │   │   └── TechTree.cs
│   │   │
│   │   ├── UI/                # User interface
│   │   │   ├── MainMenuUI.cs
│   │   │   ├── GameHUD.cs
│   │   │   ├── PauseMenuUI.cs
│   │   │   ├── SettingsUI.cs
│   │   │   ├── TowerButton.cs
│   │   │   ├── TowerInfoUI.cs
│   │   │   └── WaveResultUI.cs
│   │   │
│   │   ├── Audio/             # Audio system
│   │   │   └── AudioManager.cs
│   │   │
│   │   ├── VFX/               # Visual effects
│   │   │   └── VFXManager.cs
│   │   │
│   │   └── Editor/            # Unity Editor tools
│   │       ├── AndroidBuildConfig.cs
│   │       ├── GameDataCreator.cs
│   │       ├── SceneHierarchyBuilder.cs
│   │       └── ProjectValidator.cs
│   │
│   ├── Prefabs/               # Prefabs for towers, enemies, UI
│   ├── ScriptableObjects/     # Data assets (towers, enemies, waves)
│   ├── Scenes/                # Unity scenes
│   ├── Materials/             # Shaders and materials
│   ├── Art/                   # Sprites, textures, icons
│   └── Audio/                 # Music and SFX
│
├── GAME_DESIGN_DOCUMENT.md    # Complete game design spec
└── README.md                  # This file
```

---

## 🔧 Core Systems

### 1. Tower System

**File:** `Assets/Scripts/Towers/Tower.cs`

```csharp
// Tower component handles targeting, firing, and upgrades
public class Tower : MonoBehaviour
{
    public TowerData data;           // Stats from ScriptableObject
    public int currentLevel;         // 0-3 (base + 3 upgrades)
    public TargetPriority priority;  // First/Last/Strongest/etc.
    
    // Upgrade tower
    public bool TryUpgrade() { }
    
    // Sell tower (returns credits)
    public void Sell() { }
    
    // Change targeting mode
    public void SetTargetPriority(TargetPriority priority) { }
}
```

**Available Tower Types:**
- Laser Turret (instant hit, high accuracy)
- Plasma Cannon (energy projectile)
- Rocket Launcher (splash damage)
- Sniper Bot (long range, critical hits)
- Flamethrower (cone AoE, burn DOT)
- Tesla Coil (chain lightning)
- Freeze Turret (slow effect)
- Shock Tower (stun effect)
- Buff Station (damage boost to nearby towers)
- Minelayer (trap placement)
- Repair Station (tower HP regeneration)
- Artillery Bot (long range arc projectile, splash damage)

### 2. Wave System

**File:** `Assets/Scripts/Core/WaveManager.cs`

```csharp
// Manages wave progression and enemy spawning
public class WaveManager : MonoBehaviour
{
    public int CurrentWave { get; }
    public bool IsWaveActive { get; }
    
    // Start next wave
    public void StartWave() { }
    
    // Called when enemy dies
    public void OnEnemyKilled(Enemy enemy) { }
    
    // Called when enemy reaches end
    public void OnEnemyReachedEnd(Enemy enemy) { }
}
```

**Wave Composition:**
- Defined in `WaveSetData` ScriptableObjects
- Dynamic difficulty scaling (HP multipliers, speed modifiers)
- Boss waves at designated intervals
- 30 waves per campaign map

### 3. Save/Load System

**File:** `Assets/Scripts/Core/SaveManager.cs`

```csharp
// Persistent save/load with JSON serialization
public class SaveManager : MonoBehaviour
{
    public PlayerSaveData Data { get; }
    
    // Save current progress
    public void Save() { }
    
    // Load from disk
    public void Load() { }
    
    // Delete save file
    public void DeleteSave() { }
    
    // Create backup
    public void CreateBackup() { }
}

[System.Serializable]
public class PlayerSaveData
{
    // Settings
    public float masterVolume = 0.8f;
    public float sfxVolume = 0.7f;
    public float musicVolume = 0.6f;
    public bool vibrationEnabled = true;
    public int graphicsQuality = 2;  // 0=Low, 1=Medium, 2=High
    
    // Progress
    public int totalWavesCompleted;
    public int highestWaveReached;
    public int techPoints;
    public bool tutorialCompleted;
    
    // Achievements (65 total)
    public List<string> unlockedAchievements = new List<string>();
    
    // Tech Tree (15 nodes)
    public Dictionary<string, int> techLevels = new Dictionary<string, int>();
}
```

**Save Location:**
- Windows: `%AppData%/../LocalLow/YourStudio/RobotTowerDefense/save.json`
- Android: `Application.persistentDataPath/save.json`

### 4. Achievement System

**File:** `Assets/Scripts/Progression/AchievementManager.cs`

**65 Achievements:**
- **Tower Mastery** (11 achievements): Kill X enemies with each tower type
- **Wave Progression** (6 achievements): Complete wave milestones (10, 20, 30, 50, 100, 200)
- **Perfect Defense** (5 achievements): Complete maps without losing lives
- **Tech Master** (3 achievements): Unlock all tech tree nodes
- **Economy** (8 achievements): Accumulate/spend credits milestones
- **Efficiency** (6 achievements): Complete waves under time limits
- **Combo** (4 achievements): Multi-kill achievements
- **Boss Hunter** (3 achievements): Defeat all boss types
- **Strategic** (10 achievements): Specific tactical challenges
- **Collector** (9 achievements): Meta progression achievements

```csharp
// Example achievement tracking
public void OnEnemyKilled(Enemy enemy, Tower tower)
{
    // Track tower-specific kills
    TrackKillsByStat(tower.data.towerType, 1);
    
    // Check if achievement unlocked
    CheckAchievement($"tower_{tower.data.towerType}_kills_100");
}
```

### 5. Tech Tree

**File:** `Assets/Scripts/Progression/TechTree.cs`

**15 Permanent Upgrades:**
- **Tower Enhancements:** Damage +10%, Range +5%, Attack Speed +8%
- **Economic:** Starting Credits +50, Interest Rate +1%, Wave Bonus +20
- **Defensive:** Starting Lives +2, Life Regen (1 per 10 waves)
- **Special:** Ability Cooldown -10%, Critical Chance +5%, Splash Radius +15%
- **Utility:** Fast Forward Speed +1x, Auto-sell refund +10%

```csharp
// Upgrade a tech node
if (TechTree.Instance.TryUpgrade(TechUpgrade.TowerDamage))
{
    // Upgrade successful, tech points deducted
    Debug.Log("Tower damage increased!");
}
```

### 6. Performance System

**File:** `Assets/Scripts/Core/PerformanceManager.cs`

**Quality Presets:**

| Setting | Low | Medium | High |
|---------|-----|--------|------|
| Target FPS | 30 | 60 | 60 |
| Shadows | Off | Hard Only | All |
| Particles | Reduced | Standard | High |
| Textures | Half-res | Full | Full |
| Anti-aliasing | Off | 2x MSAA | 4x MSAA |
| Post-processing | Off | Off | On |

**Auto-Detection:**
```csharp
// Analyzes device on startup
// 6+ CPU cores, 4GB+ RAM → High
// 4+ CPU cores, 2GB+ RAM → Medium
// Otherwise → Low
```

**Battery Save Mode:**
- Automatically enables when battery < 20%
- Reduces target FPS from 60 → 30
- Disables when charging

**Real-time Metrics:**
```csharp
float currentFPS = PerformanceManager.Instance.CurrentFPS;
float avgFPS = PerformanceManager.Instance.GetAverageFPS();
string report = PerformanceManager.Instance.GetPerformanceReport();
```

### 7. Tutorial System

**File:** `Assets/Scripts/Core/TutorialManager.cs`

**9 Tutorial Steps:**
1. Welcome & mission briefing (4s auto-advance)
2. Credits display explanation (3.5s)
3. Lives display explanation (4s)
4. Place first tower (manual advance, hand pointer)
5. Start first wave (manual advance, hand pointer)
6. Watch wave complete (auto after wave ends)
7. Build more towers (waits for 2+ towers)
8. Upgrade a tower (waits for upgrade)
9. Final tips & completion (6s)

**Features:**
- Auto-start for first-time players (1.5s delay)
- Spotlight/dimmed overlay system
- Hand pointer animations
- Manual and auto-advance modes
- Game state integration (Tutorial game state)
- Save persistence (tutorialCompleted flag)

---

## 💼 Development Workflow

### Adding a New Tower

1. **Create TowerData ScriptableObject**
   ```
   Assets → Create → Robot TD → Tower Data
   ```
   
2. **Configure Stats**
   - Set cost, damage, range, fire rate
   - Configure upgrade tiers
   - Assign projectile prefab
   
3. **Add to TowerTypes Enum**
   ```csharp
   public enum TowerType
   {
       // ... existing types
       YourNewTower
   }
   ```

4. **Create Prefab**
   - Add Tower component
   - Assign TowerData reference
   - Add visual model
   - Configure colliders

5. **Test in Play Mode**
   - Place tower in test scene
   - Verify targeting and firing
   - Test all upgrade tiers

### Adding a New Enemy

1. **Create EnemyData ScriptableObject**
   ```
   Assets → Create → Robot TD → Enemy Data
   ```

2. **Configure Stats**
   - Set HP, speed, armor
   - Configure resistances
   - Set credit reward

3. **Add to EnemyTypes Enum**
   ```csharp
   public enum EnemyType
   {
       // ... existing types
       YourNewEnemy
   }
   ```

4. **Create Prefab**
   - Add Enemy component
   - Assign EnemyData reference
   - Add Animator (optional)
   - Configure NavMeshAgent

5. **Add to Wave Composition**
   - Edit WaveSetData assets
   - Add to appropriate waves

### Adding a New Map

1. **Create Scene**
   ```
   File → New Scene → Save as "Map_YourMapName"
   ```

2. **Build Map Layout**
   - Add path waypoints
   - Configure placement grid
   - Add decorations

3. **Create MapData ScriptableObject**
   ```
   Assets → Create → Robot TD → Map Data
   ```

4. **Configure Map**
   - Assign WaveSetData
   - Set difficulty modifiers
   - Define starting resources

5. **Register in MapRegistry**
   ```csharp
   MapRegistry.RegisterMap("map_yourname", mapData);
   ```

### Creating an Achievement

```csharp
// In AchievementManager.cs, add to InitializeAchievements()
achievements.Add(new Achievement
{
    id = "your_achievement_id",
    title = "Achievement Title",
    description = "Complete this specific challenge",
    tier = AchievementTier.Bronze,  // or Silver, Gold
    techPointReward = 5,
    requirement = 100  // e.g., 100 kills
});
```

### Adding a Tech Tree Node

```csharp
// In TechTree.cs, add to BuildTree()
techNodes[TechUpgrade.YourUpgrade] = new TechNode
{
    upgrade = TechUpgrade.YourUpgrade,
    displayName = "Upgrade Name",
    description = "What this upgrade does",
    maxLevel = 5,
    baseCost = 10,
    costScaling = 1.5f  // Each level costs 1.5x more
};
```

---

## ⚡ Performance Optimization

### Object Pooling

All frequently spawned objects use pooling:

```csharp
// Get from pool
GameObject projectile = ObjectPooler.Instance.SpawnFromPool("Projectile", position, rotation);

// Return to pool
ObjectPooler.Instance.ReturnToPool("Projectile", projectile);
```

**Pooled Objects:**
- Projectiles
- Enemies
- VFX particles
- UI elements (achievement toasts)

### Memory Management

```csharp
// Force garbage collection on scene transition
if (PerformanceManager.Instance != null)
{
    PerformanceManager.Instance.ForceGarbageCollection();
}

// Unload unused assets
Resources.UnloadUnusedAssets();
```

### Mobile Optimization Tips

1. **Texture Compression:** Use ETC2 for Android
2. **Mesh Optimization:** Enable "Optimize Mesh Data"
3. **Audio Compression:** Use Vorbis for music, ADPCM for SFX
4. **Lightmap baking:** Bake lighting for static objects
5. **Occlusion culling:** Enable for complex maps
6. **GPU Instancing:** Enable on materials with many instances

---

## 🧪 Testing

### Test Modes

**Development Build:**
```bash
# Enable in AndroidBuildConfig
Development Build: ✅
Deep Profiling Support: ✅
Script Debugging: ✅
```

**Editor Test Tools:**
```
Tools → Robot TD → Dev Test Tools
- Skip to wave X
- Add credits
- Complete all achievements (testing)
- Reset save data
```

### Performance Testing

```csharp
// View performance metrics in-game
if (Input.GetKeyDown(KeyCode.P))
{
    Debug.Log(PerformanceManager.Instance.GetPerformanceReport());
}
```

**Metrics to Monitor:**
- FPS (target: 60 on flagship, 30 on low-end)
- Frame time (target: <16.7ms for 60fps)
- Memory usage (target: <500MB)
- Battery drain (target: <10% per hour)

### Save/Load Testing

```csharp
// Test save integrity
SaveManager.Instance.Save();
SaveManager.Instance.CreateBackup();
SaveManager.Instance.Load();

// Verify data persistence
Debug.Log($"Waves completed: {SaveManager.Instance.Data.totalWavesCompleted}");
```

---

## 🤝 Contributing

### Code Style

- **Naming:** PascalCase for public, camelCase for private
- **Regions:** Use `#region` for organization
- **Comments:** XML docs for public APIs
- **Namespaces:** All scripts in `RobotTD.*` namespace

Example:
```csharp
using UnityEngine;

namespace RobotTD.Core
{
    /// <summary>
    /// Manages game state and core systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Fields
        
        [SerializeField] private int startingCredits = 500;
        private bool isPaused;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Initialization
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Add credits to player's balance.
        /// </summary>
        public void AddCredits(int amount)
        {
            // Implementation
        }
        
        #endregion
    }
}
```

### Commit Messages

Use conventional commits:
```
feat: Add new Tesla Tower with chain lightning
fix: Correct wave spawn timing issue
perf: Optimize enemy pathfinding with caching
docs: Update README with new build instructions
refactor: Simplify achievement tracking logic
test: Add unit tests for save system
```

### Pull Request Process

1. Create feature branch: `feature/your-feature-name`
2. Implement changes with tests
3. Update documentation if needed
4. Submit PR with clear description
5. Address code review feedback

---

## 📄 License

Copyright © 2026 Your Studio. All rights reserved.

---

## 📞 Support

**Issues:** [GitHub Issues](https://github.com/tydawkinsbuynsale-cell/tower-defense/issues)  
**Documentation:** See `GAME_DESIGN_DOCUMENT.md` for detailed game design  
**Unity Forums:** Link to your forum thread

---

## 🎯 Roadmap

### Version 1.1 ✅ COMPLETE
- [x] Endless mode implementation ✅
- [x] New tower: Artillery Bot (long-range siege) ✅
- [x] New enemy: Cloaker (stealth unit with detection mechanics) ✅  
- [x] Cloud save support ✅
- [x] Leaderboards ✅

### Version 1.2 ✅ COMPLETE
- [x] Challenge mode (special modifiers) ✅
- [x] Daily missions ✅
- [x] Boss rush mode ✅
- [x] New map: Mega Factory (endgame challenge) ✅

### Version 1.3 ✅ COMPLETE
- [x] Authentication system (Anonymous, Email/Password, Device ID) ✅
- [x] Multi-backend auth support (Unity Gaming Services, PlayFab, Custom) ✅
- [x] Session management and persistence ✅
- [x] Integration with cloud save and leaderboards ✅

### Version 1.4 ✅ COMPLETE
- [x] In-App Purchase system (Unity IAP integration) ✅
- [x] 13-product catalog (gems, credits, skins, maps, subscriptions) ✅
- [x] Shop UI with tab-based navigation ✅
- [x] Editor simulation mode for testing ✅

### Version 1.5 ✅ COMPLETE
- [x] Ad monetization system (Unity Ads integration) ✅
- [x] Interstitial, rewarded, and banner ads ✅
- [x] Frequency controls and cooldowns ✅
- [x] IAP integration (respects "Remove Ads" purchase) ✅

### Version 1.6 ✅ COMPLETE
- [x] Power-Ups system (temporary gameplay boosts) ✅
- [x] 5 power-up types (Damage, Speed, Credit, Shield, Time Freeze) ✅
- [x] Inventory and activation UI ✅
- [x] IAP and Ad integration (bundle purchase, earn free) ✅

### Version 1.7 ✅ COMPLETE
- [x] iOS platform support (iPhone and iPad) ✅
- [x] Xcode project build configuration tool ✅
- [x] Notch and safe area handler ✅
- [x] Touch gesture recognition and haptic feedback ✅
- [x] Platform-specific ad placements (iOS Game IDs) ✅

### Version 1.8 ✅ COMPLETE
- [x] Weekly Missions system (7-day rotating challenges) ✅
- [x] Tabbed mission UI (Daily/Weekly) ✅
- [x] Social Features system (friend management) ✅
- [x] Friend leaderboards (view friends-only scores) ✅
- [x] Score and achievement sharing ✅
- [x] Player search functionality ✅

### Version 2.0 ✅ COMPLETE
- [x] iOS platform support with mobile-optimized UI ✅
- [x] Multiplayer co-op mode with real-time collaboration ✅
- [x] Custom map editor with community sharing ✅
- [x] Steam platform integration with achievements ✅

### Version 3.0 (In Progress)
- [x] Seasonal Events system (limited-time challenges, exclusive rewards) ✅
- [x] Tournament Mode (competitive ladder, ranked matches, weekly tournaments) ✅
- [ ] Advanced Customization (tower skins, map themes, UI themes)
- [ ] Expanded Content Pack:
  - [ ] 5 new tower types (Laser Grid, EMP Tower, Mine Layer, Support Drone, Weather Control)
  - [ ] 8 new enemy types (Burrower, Phaser, Replicator, Vampire, Berserker, etc.)
  - [ ] 4 new campaign maps (Space Station, Underwater Lab, Arctic Base, Volcanic Forge)
- [ ] Cross-Platform Progression (play anywhere, progress synced)
- [ ] Advanced AI Systems (dynamic difficulty, adaptive strategies)
- [ ] Accessibility Features (colorblind modes, screen reader support, remappable controls)
- [ ] Analytics Dashboard (in-game stats viewer, performance tracking)
- [ ] Clan/Guild System (team up, clan wars, shared progression)
- [ ] Live Ops Tools (remote config, A/B testing, feature flags)

---

**Built with ❤️ using Unity 2022.3 LTS**
