# Changelog

All notable changes to Robot Tower Defense will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned Features
- Endless mode implementation
- Cloud save support
- Leaderboards and online rankings
- Additional tower types (Artillery Bot)
- Additional enemy types (Cloaker)
- Challenge mode with special modifiers
- Daily missions system

---

## [1.0.0] - 2026-03-12

### 🎉 Initial Release - Production Ready

#### ✨ Added

**Core Systems**
- Complete game manager with state machine and event system
- Wave manager with dynamic difficulty scaling (30 waves per map)
- Save/load system with JSON serialization and backup protection
- Object pooling system for performance optimization
- Scene bootstrapper for dependency management
- Input manager supporting touch and mouse controls

**Tower System**
- 11 unique tower types with distinct abilities:
  * Laser Turret (instant hit)
  * Plasma Cannon (energy projectile)
  * Rocket Launcher (splash damage)
  * Sniper Bot (long-range, critical hits)
  * Flamethrower (cone AoE, burn DOT)
  * Tesla Coil (chain lightning)
  * Freeze Turret (slow effect)
  * Shock Tower (stun effect)
  * Buff Station (damage boost aura)
  * Minelayer (trap placement)
  * Repair Station (tower HP regen)
- 3-tier upgrade system for each tower
- 5 targeting priority modes (First, Last, Strongest, Weakest, Closest)
- Tower placement system with visual feedback and validation
- Tower info panel with real-time stats

**Enemy System**
- 11 enemy types with varied behaviors and resistances
- 3 boss encounters with special abilities
- Status effects: slow, stun, burn, shield
- Dynamic HP and speed scaling based on wave progression
- NavMesh-based pathfinding
- Enemy data-driven design with ScriptableObjects

**Map System**
- 5 campaign maps with unique layouts and themes
- 150 total waves across all maps (30 per map)
- MapRegistry for extensible map library
- Dynamic wave composition based on map difficulty
- Path validation and placement grid system

**Progression System**
- 65 achievements with bronze/silver/gold tiers:
  * Tower mastery achievements
  * Wave progression milestones
  * Perfect defense challenges
  * Economy achievements
  * Efficiency achievements
  * Boss hunter achievements
  * Strategic challenges
  * Collector achievements
- Tech tree with 15 permanent upgrades:
  * Tower enhancements (damage, range, attack speed)
  * Economic upgrades (starting credits, interest)
  * Defensive upgrades (starting lives, regeneration)
  * Special abilities (cooldown reduction, critical chance)
  * Utility upgrades (fast forward, auto-sell refund)
- Tech points earned from wave completion and achievements
- Achievement toast notification system

**Tutorial System**
- 9-step interactive tutorial for first-time players
- Auto-start for new players with 1.5s delay
- Manual and auto-advance progression modes
- Spotlight/dimmed overlay system for UI highlighting
- Hand pointer animations for interactive elements
- Game state integration (Tutorial mode)
- Save persistence for completion status

**Performance System**
- 3 quality presets (Low/Medium/High) with comprehensive settings
- Auto-detection of optimal quality based on device capabilities
- Frame rate management (30/60 FPS configurable)
- Battery save mode (auto-switches to 30 FPS when battery < 20%)
- Real-time performance metrics tracking:
  * Current FPS
  * Average FPS (5-second rolling history)
  * Minimum/maximum FPS
  * Frame time measurement
- Memory optimization:
  * Low memory warning handler
  * Garbage collection on scene transitions
  * Force GC public API
- Granular performance controls:
  * VSync toggle
  * Shadows on/off
  * Particles on/off
  * Post-processing on/off
  * Individual quality settings per preset

**UI System**
- 10 polished UI screens:
  * Main menu with play/settings/achievements/tech tree
  * Game HUD with credits, lives, wave counter
  * Pause menu with resume/restart/settings
  * Settings panel with audio/graphics/performance controls
  * Tower selection panel with cost and info
  * Tower info panel with upgrade/sell options
  * Wave result screen with performance metrics
  * Achievement screen with unlock tracking
  * Tech tree screen with upgrade visualization
  * Tutorial overlay system
- TextMeshPro integration for modern text rendering
- Responsive UI with safe area handling
- Achievement toast notifications
- Performance stats display (optional, real-time)

**Audio System**
- Audio manager with music and SFX channels
- Volume controls (master, SFX, music)
- Positional audio for spatial awareness
- Audio pooling for performance
- Vibration support (configurable)

**VFX System**
- VFX manager with particle pooling
- Tower muzzle flash effects
- Enemy death effects
- Projectile trail effects
- Tower upgrade effects

**Editor Tools**
- Android build configuration tool
  * Automated build settings configuration
  * One-click APK/AAB builds
  * Icon size reference guide
- Game data creator for ScriptableObjects
- Scene hierarchy builder for rapid scene setup
- Project validator for asset integrity checking
- Asset processor for import optimization
- Dev test tools for debugging:
  * Skip to specific wave
  * Add credits
  * Complete all achievements (testing)
  * Reset save data

**Android Support**
- IL2CPP scripting backend
- ARM64 and ARMv7 architecture support
- Minimum SDK: Android 7.0 (API 24)
- Target SDK: Android 14 (API 34)
- Vulkan and OpenGL ES 3 graphics APIs
- Multithreaded rendering
- GPU skinning
- Code stripping for size optimization
- Managed code stripping (Medium level)
- Mesh optimization

#### 📝 Documentation
- Comprehensive README.md with:
  * Project overview and features
  * System architecture documentation
  * Setup instructions
  * Android build guide
  * Project structure reference
  * Core systems documentation
  * Development workflow guide
  * Performance optimization tips
  * Testing guidelines
  * Contributing guidelines
- Complete GAME_DESIGN_DOCUMENT.md
- CHANGELOG.md for version tracking

#### 🔧 Technical
- Unity 2022.3 LTS
- C# 9.0+ with modern language features
- Singleton pattern for managers
- Event-driven architecture
- Data-driven design with ScriptableObjects
- JSON serialization for save data
- Factory pattern for tower/enemy instantiation
- Object pooling for performance
- NavMesh-based pathfinding

#### 🎯 Performance
- 60 FPS target on flagship devices
- 30 FPS target on low-end devices
- Object pooling for all frequent spawns
- Efficient memory management
- Battery optimization
- Frame time monitoring
- Adaptive quality system
- Memory warning handling

---

## Version Naming Convention

- **Major (X.0.0)**: Complete feature sets, major milestones
- **Minor (1.X.0)**: New features, maps, towers, enemies
- **Patch (1.0.X)**: Bug fixes, balance tweaks, optimizations

---

## Support

For issues, feature requests, or contributions, please visit:
[GitHub Issues](https://github.com/tydawkinsbuynsale-cell/tower-defense/issues)

---

[Unreleased]: https://github.com/tydawkinsbuynsale-cell/tower-defense/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/tydawkinsbuynsale-cell/tower-defense/releases/tag/v1.0.0
