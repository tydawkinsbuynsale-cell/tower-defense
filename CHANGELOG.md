# Changelog

All notable changes to Robot Tower Defense will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### 🎯 Coming Soon - V2.0 Features

**Version 2.0 Roadmap**
- iOS platform support with mobile-optimized UI
- Multiplayer co-op mode with real-time collaboration
- Custom map editor with sharing capabilities
- Steam platform release with achievements

---

## [Version 1.3] - Authentication System

### Added

**Authentication System** 🔐
- **AuthenticationManager**: Unified player authentication with session management
  - Multi-backend support: Unity Gaming Services, PlayFab, Custom HTTP server
  - Backend-agnostic API with preprocessor directives
  - Singleton pattern with DontDestroyOnLoad persistence
  - Session persistence across app restarts
  - Token management and automatic refresh
  - Account linking support (upgrade guest to permanent)
- **Authentication Methods (3 types)**:
  - **Anonymous/Guest**: Instant sign-in, no credentials required
    - Device-tied account with automatic ID generation
    - Upgradeable to permanent account later
    - Perfect for quick onboarding
  - **Email/Password**: Traditional account authentication
    - Secure credential handling via backend
    - Password minimum 6 characters
    - Email format validation
    - Account recovery support (backend-dependent)
  - **Device ID**: Silent authentication
    - Uses SystemInfo.deviceUniqueIdentifier
    - Falls back to GUID if unavailable
    - Persistent across app restarts
    - Cross-platform device tracking
- **Session Management**:
  - Automatic session persistence in PlayerPrefs
  - Token validation on app start
  - Auto-restore previous session if valid
  - Token expiration handling (24-hour tokens typical)
  - Manual token refresh API
  - Full session cleanup on sign-out
- **Event System**:
  - OnAuthenticationStateChanged(bool isAuth)
  - OnSignInSuccess(string playerId, string playerName)
  - OnSignInFailed(string error)
  - OnSignOut()
  - OnError(string error)
- **Integration with Online Systems**:
  - **CloudSaveManager**: Waits for authentication before syncing
    - Uses auth token for API authorization
    - Auto-pulls cloud data on sign-in
    - Supports offline mode if not authenticated
  - **LeaderboardManager**: Uses authenticated player info
    - Syncs player ID from AuthenticationManager
    - Falls back to local PlayerPrefs if not authenticated
    - Scores tied to authenticated accounts
- **UI Components**:
  - **LoginUI**: Sign-in interface with tab switching
    - Anonymous and email/password forms
    - Input validation (email format, password length)
    - Loading state with animated spinner
    - Error display panel with dismiss button
    - Enter key submission support
    - Success message with auto-close
  - **AccountUI**: Player account status display
    - Player name and ID display (shortened)
    - Auth status indicator with color coding
      - Green: Authenticated (full account)
      - Yellow: Guest Account (anonymous)
      - Gray: Not Signed In
    - Sign-out button
    - Link account button (for guest accounts)
    - Auto-refresh on auth state change
- **Configuration Options**:
  - Auto sign-in on start (default: true)
  - Allow guest mode (default: true)
  - Persist session (default: true)
  - Preferred backend selection
  - Custom backend URL configuration
  - Verbose logging toggle
- **Security Features**:
  - Token expiration validation
  - Secure token storage in PlayerPrefs
  - HTTPS required for custom backends
  - Backend-handled password security
  - Device ID fallback with GUID generation
- **Complete Documentation**: [AUTHENTICATION_GUIDE.md](AUTHENTICATION_GUIDE.md)
  - Architecture overview and state machine diagrams
  - Backend setup guides (Unity/PlayFab/Custom)
  - Authentication method explanations
  - UI component setup instructions
  - Integration examples with CloudSave and Leaderboards
  - Testing scenarios and debug tools
  - Best practices and security guidelines
  - Comprehensive troubleshooting guide

---

## [Version 1.4] - In-App Purchases System

### Added

**In-App Purchases (IAP)** 💳
- **IAPManager**: Complete Unity IAP integration for monetization
  - Unity IAP SDK integration with IStoreListener implementation
  - Multi-platform support: iOS App Store, Google Play Store, and more
  - Singleton pattern with DontDestroyOnLoad persistence
  - 13-product catalog across all product types
  - Automatic receipt validation (local + optional server)
  - Purchase idempotency with transaction ID tracking
  - Editor simulation mode for testing without real payments
- **Product Catalog (13 Products)**:
  - **Consumables (7 products)**: Repeatable purchases
    - Gem bundles: 100 ($0.99), 500 ($4.99), 1200 ($9.99), 3000 ($19.99)
    - Credit packs: 5000 ($0.99), 25000 ($4.99)
    - Power-up bundle ($2.99): 5x all power-ups
  - **Non-Consumables (5 products)**: One-time permanent purchases
    - Starter Pack ($4.99): 3000 gems + 10000 credits + gold tower skin
    - Remove Ads ($2.99): Permanently disable advertisements
    - Tower Skin Gold ($1.99): Exclusive gold tower appearance
    - Tower Skin Neon ($1.99): Exclusive neon tower appearance
    - Map Pack 1 ($3.99): 3 additional challenge maps
  - **Subscriptions (1 product)**: Recurring billing
    - Premium Pass Monthly ($4.99/mo): 100 daily gems + exclusive rewards
- **Purchase Flow**:
  - One-click purchase initiation: `BuyProduct(productId)`
  - Automatic store integration via Unity IAP
  - Receipt processing and validation
  - Reward granting by product type:
    - Consumables: AddGems(), AddCredits() via SaveManager/GameManager
    - Non-Consumables: UnlockTowerSkin(), SetAdsRemoved(), UnlockMapPack()
    - Subscriptions: ActivatePremiumPass() with expiration tracking
  - Success/failure callbacks with detailed error messages
  - Transaction ID logging for support and fraud prevention
- **Restore Purchases**:
  - iOS App Store compliance feature (required by Apple)
  - Restores all non-consumable purchases on new device
  - Available for Android (optional but recommended)
  - Integration with IAppleExtensions.RestoreTransactions()
  - Event: OnRestoreComplete callback
- **Shop UI System**:
  - **ShopUI**: Tab-based shopping interface
    - Three tabs: Consumables, Permanent, Subscriptions
    - Product list with dynamic spawning from catalog
    - Loading state indicator during initialization
    - Success/error message display with auto-clear
    - Restore Purchases button (iOS compliance)
    - Analytics tracking: shop_opened event
  - **ShopProductCard**: Individual product display component
    - Product title, description, and localized price
    - Purchase button with state management: "Buy" → "Processing..." → "Purchased"
    - "Purchased" badge for owned non-consumables
    - Product type color coding: Green (consumable), Blue (non-consumable), Orange (subscription)
    - Button auto-disable after non-consumable purchase
    - Refresh on purchase status change
- **Integration with Game Systems**:
  - **SaveManager**: Gems storage and persistence
  - **GameManager**: Credits management via AddCredits()
  - **PlayerPrefs**: Non-consumable ownership tracking
    - Tower skins unlocked (gold, neon)
    - Map packs unlocked
    - Ads removed flag
    - Premium pass status and expiration timestamp
- **Analytics Integration**:
  - All purchase events tracked via AnalyticsManager:
    - iap_initialized: IAP system ready
    - iap_purchase_initiated: User clicked "Buy"
    - iap_purchase_success: Purchase completed (with product_id, price, transaction_id)
    - iap_purchase_failed: Purchase failed (with product_id, error reason)
    - iap_restore_initiated: Restore purchases clicked
  - Conversion funnel tracking for monetization optimization
  - Transaction metadata for analytics dashboards
- **Localization Support**:
  - Store-provided localized pricing strings
  - GetLocalizedPrice(): Returns "$0.99" (US), "€0,99" (EU), "¥120" (JP), etc.
  - Currency symbols and formatting per region
- **Testing & Debugging**:
  - Editor simulation mode: Test all purchases without real payments
  - Context menu commands for quick testing:
    - Buy 100 Gems (simulate gems_100 purchase)
    - Buy Remove Ads (simulate remove_ads purchase)
    - Restore Purchases (test restore flow)
    - Print Product Catalog (log all 13 products)
  - Verbose logging toggle for development
  - Purchase validation checks before granting
- **Query API**:
  - GetProduct(productId): Retrieve product info
  - GetProductsByType(type): Filter by Consumable/NonConsumable/Subscription
  - HasPurchased(productId): Check non-consumable ownership
  - GetPurchaseCount(productId): Track purchase frequency
  - IsPremiumActive(): Check subscription validity with expiration
  - AreAdsRemoved(): Check ad removal status
- **Event System**:
  - OnInitialized()
  - OnInitializeFailed(string error)
  - OnPurchaseComplete(string productId, string transactionId)
  - OnPurchaseFailed(string productId, string reason)
  - OnRestoreComplete()
- **Configuration Options**:
  - Enable/disable IAP system (for regions without monetization)
  - Editor simulation toggle for development testing
  - Verbose logging for debugging purchase flows
  - Custom product definitions extendable for future products
- **Security Features**:
  - Receipt validation via Unity IAP
  - Transaction ID deduplication to prevent double-grants
  - Purchase records encrypted in PlayerPrefs
  - Server-side validation ready (optional backend integration)
  - Restore purchases prevents fraud via server verification
- **Complete Documentation**: [IAP_GUIDE.md](IAP_GUIDE.md)
  - Unity IAP setup and package installation
  - Product catalog explanation with pricing strategy
  - Store configuration guides (App Store Connect, Google Play Console)
  - Purchase flow implementation details
  - Shop UI setup instructions
  - Integration examples with SaveManager and GameManager
  - Testing procedures: Editor simulation, sandbox testing, production
  - iOS restore purchases requirement and implementation
  - Best practices for monetization
  - Comprehensive troubleshooting guide

---

## [Version 1.2] - Boss Rush & Mega Factory

### Added

### 🎯 Coming Soon - Analytics & Online Features

**Analytics & Telemetry**
- **AnalyticsManager**: Comprehensive event tracking system
  - Session management with unique IDs and timeout handling
  - Automatic new user detection and first launch tracking
  - Real-time event tracking with custom parameters
  - Performance metrics sampling (FPS, memory, frame time)
  - Error and crash logging with stack traces
  - Backend integration ready (Unity Analytics, Firebase, custom server)
- **AnalyticsEvents**: 40+ predefined event types
  - Session events (start, end)
  - Gameplay events (game start/end, waves, towers, enemies) 
  - Progression events (achievements, tech tree, tutorial)
  - Performance events (quality changes, battery save, FPS tracking)
  - Monetization events (IAP ready for future)
- **Analytics Integration**: Tracking throughout all systems
  - GameManager: game start/end, victory/defeat
  - WaveManager: wave start/complete with metrics
  - TowerPlacementManager: tower placed with position/cost
  - AchievementManager: achievement unlocks
  - PerformanceManager: quality preset changes
- **AnalyticsDashboard** (Editor Tool): **Tools → Robot TD → Analytics Dashboard**
  - Realtime event stream viewer
  - Event count aggregation with graphs
  - Session info display
  - CSV export functionality
- **Complete Documentation**: [ANALYTICS_GUIDE.md](ANALYTICS_GUIDE.md)

**Leaderboard System**
- **LeaderboardManager**: Competitive scoring with offline-first design
  - Multiple leaderboard support (endless, daily, weekly)
  - Player identity management with auto-generated IDs and names
  - Local score storage with PlayerPrefs persistence
  - 5-minute score caching to reduce API calls
  - Backend-agnostic architecture (Unity Gaming Services, PlayFab, custom HTTP)
  - Rich query API (top scores, player rank, nearby scores)
  - Analytics integration for engagement tracking
- **Leaderboard UI Components**:
  - **LeaderboardUI**: Main display panel with tab switching
    - Top scores display with rank, name, score
    - Player highlight (gold) and top 3 tint (blue)
    - Loading and error states
    - Refresh and tab navigation
  - **LeaderboardEntryUI**: Individual score row prefab
    - Rank formatting (1st, 2nd, 3rd, #10)
    - Medal icons for top 3 (gold/silver/bronze)
    - Metadata display (wave number, challenge date)
  - **PlayerNameDialog**: First launch name input
    - Auto-show on first launch
    - Random name generator (prefix+suffix+number)
    - Name validation and sanitization
    - Character count display
- **EndlessMode Integration**:
  - Automatic score submission on game over/victory
  - Wave number included in metadata
  - Combined score (base + endless bonus)
- **Complete Documentation**: [LEADERBOARD_GUIDE.md](LEADERBOARD_GUIDE.md)
  - Quick setup (5 minutes)
  - Backend integration guides (Unity/PlayFab/Custom)
  - UI configuration
  - Testing procedures
  - Best practices

**Cloud Save System** ☁️
- **CloudSaveManager**: Cross-device progress sync with intelligent conflict resolution
  - Offline-first architecture (local save always works, sync when possible)
  - Multi-backend support: Unity Cloud Save, PlayFab, Custom HTTP server
  - Backend-agnostic design using preprocessor directives
  - Automatic sync: On app start, app pause, periodic intervals (5 min default)
  - Manual sync controls: Push, Pull, Full Sync
  - Non-blocking async operations (never blocks gameplay)
  - Singleton pattern with DontDestroyOnLoad persistence
- **Conflict Resolution Strategies**:
  - **Most Recent** (default): Use save with newest timestamp
  - **Highest Progress**: Use save with most XP/progress (prevents data loss)
  - **Merge**: Intelligently combine both saves (take best of all values)
    - Numeric values: Take maximum (XP, kills, playtime, stats)
    - Lists: Union (unlocked maps, achievements)
    - Per-map scores: Take highest from either device
    - Tech tree: Highest upgrade level from either device
  - **Prefer Local**: Always use local save (testing/debugging)
  - **Prefer Cloud**: Always use cloud save (restoring after device reset)
- **Data Sync Coverage**:
  - All PlayerSaveData fields synced: progression, stats, achievements
  - Maps: Unlocked status, best scores, stars earned
  - Tech tree: All upgrade levels preserved
  - Game modes: Endless high waves/scores, Boss Rush best runs
  - Settings: Volume, graphics quality, control preferences
  - Daily/weekly stats: Missions, challenges, playtime
- **Sync Management**:
  - Auto-sync timer with configurable interval (default 5 minutes)
  - Sync on app start/resume (pull cloud progress)
  - Sync on app pause/quit (push local progress)
  - HasUnsyncedChanges() check for local modifications
  - TimeSinceLastSync() for UI status display
  - Data hash comparison for efficient change detection
- **Event System**:
  - OnSyncCompleted(bool success): Fires when sync finishes
  - OnSyncError(string error): Fires on network/auth failures
  - OnConflictDetected(ConflictInfo): Fires when conflict found
  - Enables UI feedback without tight coupling
- **SaveManager Integration**:
  - CloudSaveManager automatically notified of local saves
  - Auto-sync timer handles periodic push to cloud
  - No manual hookup required (passive notification)
  - Local backup system preserved for data safety
- **Backend Implementations** (Template/Placeholder):
  - Unity Cloud Save: Async/await pattern with Unity Services
  - PlayFab: Client API with UpdateUserData/GetUserData
  - Custom HTTP: REST API with UnityWebRequest (GET/POST)
  - All backends require authentication token
  - Easily extensible for additional backends
- **Development Features**:
  - Verbose logging toggle for debugging
  - Context menu commands for testing (Force Push/Pull)
  - Backend dashboard verification support
  - Data validation before deserialize
- **Complete Documentation**: [CLOUD_SAVE_GUIDE.md](CLOUD_SAVE_GUIDE.md)
  - Architecture overview with flow diagrams
  - Backend setup guides (Unity/PlayFab/Custom)
  - Conflict resolution strategy explanations with examples
  - API reference and usage examples
  - UI integration sample code
  - Testing scenarios (cross-device, offline, conflicts)
  - Best practices and troubleshooting

**Challenge Mode System** 🎮
- **ChallengeData**: ScriptableObject-based challenge configuration
  - 20+ challenge modifiers (Speed Rush, Tower Limit, Budget Crisis, Perfect Defense, etc.)
  - 4 difficulty tiers (Easy, Medium, Hard, Extreme) with score multipliers
  - Reward system (credits + tech points)
  - Rotation types (Daily, Weekly, Permanent)
- **ChallengeManager**: Core challenge lifecycle management
  - Challenge selection and activation
  - Modifier application to game systems
  - Progress tracking (completion status, best scores, attempt counts)
  - Daily/weekly rotation system with automatic scheduling
  - Tower limit enforcement
  - Rewards on first completion
  - Analytics integration
- **Challenge UI Components**:
  - **ChallengeSelectorUI**: Main browsing panel with tabs
    - Daily/Weekly/Permanent challenge tabs
    - Automatic rotation timers
    - Challenge card spawning and pooling
  - **ChallengeCardUI**: Individual challenge display
    - Difficulty visualization with stars and colors
    - Modifier icons with tooltips
    - Completion badges and best scores
    - Reward display
  - **ChallengeResultUI**: Completion screen
    - Victory/defeat display
    - Final score with difficulty multiplier
    - First completion rewards notification
    - Retry/Next Challenge/Menu options
- **Game Integration**:
  - GameManager: Static events for challenge hooks, starting credits/lives override
  - AnalyticsManager: challenge_started, challenge_completed event tracking
  - AchievementManager: Challenge milestone achievements
  - Analytics Events: 5 new challenge-specific event types
- **Gameplay Modifier Integration** (Full Implementation):
  - WaveManager: Enemy stat modifiers (SpeedRush, ArmoredAssault, SwarmMode)
    - Challenge multipliers for health, speed, and enemy count
    - SetChallengeMultipliers(), ResetChallengeMultipliers(), GetChallengeModifiedEnemyCount()
    - Wave delay modifiers (FastForward 50% delay, NoBreaks 0.1s delay)
  - Tower: Damage modifier integration (WeakenedTowers)
    - GetDamageMultiplier() queries ChallengeManager for active challenges
  - TowerPlacementManager: Cost and limit modifiers
    - GetModifiedTowerCost() applies BudgetCrisis multiplier
    - CanPlaceTower() enforces TowerLimit modifier
    - Tower count tracking (placement/sell events)
  - TowerButton: Dynamic cost display with challenge modifiers
    - Real-time affordability checks with modified costs
    - UI cost text updates to show challenge prices
  - GameManager: Economy modifier integration (EconomicHardship)
    - AddCredits() applies challenge economy multiplier to all rewards
  - ChallengeManager: Public query API
    - GetTowerCostMultiplier(), GetTowerDamageMultiplier(), GetEconomyMultiplier()
    - HasActiveModifier() for conditional logic checks
- **Editor Tools**:
  - **ChallengeDataGenerator**: Auto-creation of 13 example challenges
    - Menu: Robot TD → Generate Example Challenges
    - 4 Daily challenges (Easy-Medium)
    - 4 Weekly challenges (Hard)
    - 5 Permanent challenges (Easy-Extreme)
    - Automatic configuration with balanced rewards
    - Clear All Challenges utility
- **Complete Documentation**: [CHALLENGE_MODE_GUIDE.md](CHALLENGE_MODE_GUIDE.md)
  - Quick start (5 minutes)
  - 20+ modifier descriptions
  - Challenge creation guide (manual + auto-generation)
  - UI setup instructions
  - Integration examples
  - Testing procedures
  - Best practices and balance guidelines

**Daily Missions System** 📅
- **MissionData**: ScriptableObject-based mission configuration
  - 20+ mission types (Combat, Tower, Economy, Wave, Map, Special)
  - 3 difficulty tiers (Easy, Medium, Hard) with scaled rewards
  - Weighted rotation system for mission selection
  - Level gating for progressive unlocks
  - Target values and optional parameters for flexibility
- **MissionManager**: Core mission lifecycle management
  - Automatic daily rotation (24-hour cycle)
  - Progress tracking across all mission types
  - Reward system (credits + tech points)
  - Persistent storage with PlayerPrefs
  - Auto-subscription to game events for tracking
  - Mission completion detection and rewards
  - Analytics integration
- **Mission UI Components**:
  - **DailyMissionsUI**: Main panel with rotation timer
    - 3 mission cards with staggered animations
    - Countdown timer to next rotation
    - Completion summary (X/3 completed)
    - Refresh button for testing
  - **MissionCardUI**: Individual mission display
    - Progress bar with real-time updates
    - Difficulty stars and color coding
    - Reward display (credits + tech points)
    - Claim button with animations
    - Completion badge
- **Auto Progress Tracking**:
  - Combat missions: Enemy kills, boss kills, damage dealt
  - Tower missions: Placement, upgrades, specific tower types
  - Economy missions: Credits earned/spent, ending balance
  - Wave missions: Completion, flawless waves, survival
  - Map missions: Victory, flawless victory, specific maps
- **Game Integration**:
  - GameManager: Economy tracking (AddCredits, SpendCredits, victory rewards)
  - TowerPlacementManager: Tower placement and type tracking
  - Tower: Upgrade tracking, max level detection
  - WaveManager: Wave completion, enemy kill tracking
  - AnalyticsEvents: 6 new mission event types
- **Complete Documentation**: [DAILY_MISSIONS_GUIDE.md](DAILY_MISSIONS_GUIDE.md)
  - Quick start (5 minutes)
  - 20+ mission type descriptions
  - Mission creation guide with examples
  - Difficulty and reward balancing guidelines
  - UI setup instructions
  - Integration examples
  - Testing procedures
  - Troubleshooting guide

**Artillery Bot Tower** 💣
- **New Tower Type**: Long-range siege tower with arc projectiles
  - Fires shells in high parabolic arc over obstacles
  - Deals splash damage on impact with damage falloff
  - Minimum range limitation (can't hit close enemies)
  - Highest range of all towers (~15-18 units)
  - Slow fire rate (~2-3 seconds) balanced by high damage
  - Visual arc trajectory and barrel aiming
- **ArtilleryBot** (Tower Class):
  - Extends Tower base class with arc firing logic
  - Minimum range targeting (3+ units from tower)
  - Barrel rotation with elevation angle calculation
  - Splash damage helper method for projectiles
  - Muzzle flash and fire sound effects
  - Range gizmos showing min/max range and splash radius
- **ArtilleryProjectile**:
  - Position-based targeting (doesn't track moving targets)
  - Parabolic trajectory physics with realistic arc
  - Flight duration calculated from distance (0.5-2.5s)
  - Shell rotation during flight for visual effect
  - Explosion on impact with optional camera shake
  - Gizmo visualization of trajectory prediction
  - Damage falloff based on distance from impact
- **Game Balance**:
  - Cost: ~300 credits (expensive, mid-game tower)
  - Damage: ~150 base (high single-instance damage)
  - Range: ~18 units (extreme, covers large map areas)
  - Fire Rate: 0.4 attacks/sec (~2.5s cooldown)
  - Splash Radius: 2.5 units with 50% falloff
  - Minimum Range: 3 units (requires other towers for close defense)
- **Strategic Role**:
  - Long-range area denial for grouped enemy clusters
  - Pairs well with slow/freeze towers for static targets
  - Weak against fast-moving or close-range threats
  - Tests Challenge Mode (LimitedArsenal, tower cost modifiers)
  - Provides content for Daily Missions (UseTowerType, PlaceTowers)
- **Integration**:
  - Added ArtilleryBot to TowerType enum
  - New tower and projectile classes
  - Compatible with all existing systems (upgrades, missions, challenges)
  - Requires Unity asset setup (ScriptableObject, prefabs, UI button)
- **Documentation**: Setup guide in this changelog (see below)

**Cloaker Enemy** 👻
- **New Enemy Type**: Stealth unit with invisibility mechanics
  - Turns invisible (cloaked) - only 15% opacity
  - Uncloaks when taking damage - becomes fully visible
  - Re-cloaks after cooldown period (no damage for 5 seconds)
  - Only targetable by towers within detection range when cloaked
  - Creates strategic tower placement challenges
  - Medium health (150 HP), medium-fast speed (2.5 units/sec)
- **CloakerEnemy** (Enemy Class):
  - Extends Enemy base class with stealth mechanics
  - Smooth visual transitions (0.5s cloak/uncloak)
  - Material transparency control with alpha blending
  - Particle effects for cloak/uncloak events
  - Audio feedback for state changes
  - Uncloak duration timer (stays visible 2s after damage)
  - Recloak cooldown system (3s delay before can recloak)
  - Health bar visibility tied to cloak state
- **Detection System**:
  - Global detection range: 4 units (default for all towers)
  - Towers can only target cloaked enemies within detection range
  - Tower.GetDetectionRange() virtual method for custom detection
  - CloakerEnemy.CanTarget() static helper for tower checks
  - Supports enhanced detection for specialized towers
- **Tower Targeting Integration**:
  - Modified Tower.IsValidTarget() to check cloaking status
  - Updated all targeting methods (First, Last, Strongest, Weakest, Closest)
  - Cloaked enemies skipped unless within detection range
  - Seamless integration with existing tower AI
- **Visual System**:
  - Transparency transitions using material alpha
  - Standard shader rendering mode switching (Opaque ↔ Transparent)
  - Original color preservation for smooth transitions
  - Particle effects for cloak/uncloak events
  - Gizmos showing detection radius in editor
- **Game Balance**:
  - Health: 150 HP (medium durability)
  - Speed: 2.5 units/sec (medium-fast)
  - Reward: 40 credits (higher than basic enemies)
  - Detection Range: 4 units (requires close tower placement)
  - Uncloak Duration: 2 seconds (window to deal damage)
  - Recloak Cooldown: 3 seconds (strategic timing)
  - Cloaked Alpha: 15% (mostly invisible but faintly visible)
- **Strategic Role**:
  - Forces tight tower placement for detection coverage
  - Countered by close-range towers and strategic positioning
  - Pairs well with tanks and healers (slips past defenses)
  - Tests player awareness and map control
  - Rewards forward-thinking tower placement
  - Can be specialized with detection towers (future enhancement)
- **Integration**:
  - Added CloakerEnemy class to EnemyTypes.cs (260 lines)
  - Enhanced Tower.cs with detection mechanics
  - Compatible with all game systems (achievements, missions, challenges)
  - Requires Unity asset setup (ScriptableObject, prefabs, VFX)
  - EnemyData already has canCloak boolean (system-ready)
- **Documentation**: [CLOAKER_ENEMY_SETUP.md](CLOAKER_ENEMY_SETUP.md)
  - Complete Unity Editor setup guide (600+ lines)
  - Material and shader configuration
  - Particle effect creation
  - Audio integration
  - Wave composition guidelines
  - Testing procedures with detection mechanics
  - Balancing recommendations
  - Troubleshooting guide
  - Advanced features (detection towers, true sight)

**Endless Mode System** ♾️
- **EndlessMode**: Infinite wave generation with progressive scaling
  - Auto-activates after completing campaign maps
  - Infinitely escalating enemy waves with difficulty scaling
  - Separate endless score tracking (distinct from campaign)
  - Milestone reward system (bonuses every 5 waves)
  - Leaderboard integration for competitive gameplay
  - Personal best tracking (highest wave, highest score)
- **EndlessMode.cs** (Core System, 195 lines):
  - Singleton pattern with static event system
  - OnEndlessWaveStarted(int wave) - fired when each endless wave begins
  - OnMilestoneReached(int wave, long bonus) - fired at milestone intervals
  - Automatic activation via WaveManager.OnAllWavesCompleted event
  - EndlessLoop() coroutine for continuous wave generation
  - Configurable scaling parameters (HP, speed, spawn rate, enemy count)
  - Milestone credit bonuses with exponential scaling
  - PostEndlessScore() for leaderboard submission and save data
- **Scaling System**:
  - Enemy Health: +25% per wave (healthScalePerWave)
  - Enemy Speed: +5% per wave (speedScalePerWave)
  - Spawn Rate: +8% per wave (spawnRateScalePerWave)
  - Enemy Count: Base 10 + 3 per wave (baseEnemiesPerWave, enemiesIncreasePerWave)
  - Wave Delay: 8 seconds between waves (configurable)
- **Milestone Rewards**:
  - Triggered every 5 waves (milestoneInterval)
  - Bonus Credits: 200 * (wave / 5) scaling formula
  - Achievement hooks via AchievementManager.CheckEndlessMilestone()
  - Toast notifications with celebration sounds
  - Leaderboard updates at each milestone
- **WaveManager Integration**:
  - SetEndlessMode(bool) - toggles endless mode flag
  - SpawnEndlessWave(effectiveWave, count, hpMult, spdMult) - spawns scaled enemies
  - GenerateEndlessComposition() - creates dynamic enemy mix
  - IsEndlessMode property for conditional logic
- **SaveManager Integration**:
  - PlayerSaveData.endlessHighWave - highest wave reached
  - PlayerSaveData.endlessHighScore - best endless score
  - PlayerSaveData.endlessGamesPlayed - total endless sessions
  - Persistent tracking across sessions
  - New record detection and logging
- **UI Integration**:
  - **GameHUD.cs**: In-game endless wave display
    - Gold-colored "Endless Wave X" text when in endless mode
    - "Endless Mode Activated!" toast on wave 1
    - Real-time milestone notifications with bonus amounts
    - "Endless Mode" button text indicator during active waves
  - **EndlessModeUI.cs**: Main menu info panel (140 lines)
    - Endless mode description and how-to-play guide
    - Personal best display (highest wave, highest score)
    - Enemy scaling information viewer
    - Milestone rewards breakdown
    - Leaderboard access button
    - "Not Yet Played" state for new players
  - **MainMenuUI.cs**: Endless mode button integration
    - New "Endless Mode" button in main panel
    - Direct access to EndlessModeUI panel
    - Audio feedback on button press
- **Leaderboard Integration**:
  - LeaderboardManager.SubmitEndlessScore(wave, score) submission
  - "endless_high_score" leaderboard ID
  - Wave number included in score metadata
  - LeaderboardUI.ShowEndlessLeaderboard() display method
  - Endless mode tab in leaderboard UI
- **Game Balance**:
  - Starting Difficulty: Equivalent to wave 1 of campaign
  - Wave 10: ~250% health, ~50% faster speed, ~13 enemies
  - Wave 20: ~500% health, ~100% faster speed, ~16 enemies
  - Wave 50: ~1250% health, ~250% faster speed, ~25 enemies
  - Milestone bonuses scale exponentially (200, 400, 600, 800...)
  - Designed for 30-60 minute average survival time
- **Strategic Considerations**:
  - Long-term tower investment becomes critical
  - Economy management with milestone bonuses
  - Tower composition must handle all enemy types
  - Map control and coverage increasingly important
  - Challenge Mode integration for difficulty modifiers
  - Achievement system support for endless milestones

**Boss Rush Mode** 👹
- **BossRushMode**: Sequential boss battle game mode with escalating difficulty
  - Face one boss at a time in endless gauntlet
  - Each boss significantly stronger than previous
  - 30-second prep phase between bosses for tower upgrades
  - Rewards scale with boss number defeated
  - Leaderboard integration for competitive play
  - Personal best tracking (bosses defeated, high score)
- **BossRushMode.cs** (Core System, 320 lines):
  - Singleton pattern with static event system
  - OnBossEncounterStarted(int boss, string name) - fired when boss spawns
  - OnBossDefeated(int boss, int credits) - fired when boss eliminated
  - OnPrepPhaseStarted() - fired at start of break time
  - Sequential boss queue with configurable boss prefabs
  - BossRushLoop() coroutine for continuous encounters
  - PrepPhase() coroutine with countdown timer
  - Configurable scaling parameters (HP, speed, rewards)
  - PostBossRushScore() for leaderboard submission and save data
- **Boss Types** (Already Existed):
  - **BossEnemy** base class: Health regeneration (1%/sec), enrage at 25% HP (+50% speed)
  - **SwarmMotherBoss**: Spawns drone minions every 2 seconds (max 8)
  - **ShieldCommanderBoss**: Provides shields to nearby enemies (8 unit radius)
  - **Tank Destroyer**: High damage, armored (future enhancement)
  - **Repair Master**: Advanced healing (future enhancement)
  - **Artillery Juggernaut**: Long-range attacks (future enhancement)
- **Scaling System**:
  - Boss Health: +50% per boss (very aggressive scaling)
  - Boss Speed: +10% per boss  - Rewards: 500 base + (boss# * 100) credits per defeat
  - Boss 1: 100% HP, 100% speed, 500 credits
  - Boss 5: 300% HP, 150% speed, 900 credits
  - Boss 10: 550% HP, 200% speed, 1,400 credits
- **SaveManager Integration**:
  - PlayerSaveData.bossRushBestRun - most bosses defeated
  - PlayerSaveData.bossRushHighScore - best score achieved
  - PlayerSaveData.bossRushGamesPlayed - total sessions
  - Persistent tracking across sessions
  - New record detection and logging
- **UI Integration**:
  - **GameHUD.cs**: In-game boss rush display
    - Red-colored "BOSS X: [Name]" text during encounters
    - "Preparation Phase" cyan text during breaks
    - Boss encounter warnings with toast notifications
    - Boss defeated celebrations with credit totals
    - Prep phase countdown timer on button text
  - **BossRushUI.cs**: Main menu info panel (180 lines)
    - Boss rush mode description and how-to-play guide
    - Personal best display (bosses defeated, high score)
    - Boss type information with scaling details
    - Reward breakdown and progression
    - Leaderboard access button
    - Start Boss Rush button (scene loading hook)
    - "Not Yet Played" state for new players
  - **MainMenuUI.cs**: Boss rush button integration
    - New "Boss Rush" button in main panel
    - Direct access to BossRushUI panel
    - Audio feedback on button press
- **Leaderboard Integration**:
  - LeaderboardManager.SubmitBossRushScore(bosses, score) submission
  - "boss_rush_score" leaderboard ID
  - Bosses defeated count in score metadata
  - LeaderboardUI.ShowBossRushLeaderboard() display method (future)
  - Boss rush tab in leaderboard UI (future enhancement)
- **Game Balance**:
  - Starting Resources: 1000 credits, 20 lives (more generous than campaign)
  - Boss HP scaling is aggressive to create challenge curve
  - Prep time crucial for economy management and upgrades
  - Late-game bosses (10+) require optimized tower compositions
  - Milestone rewards help sustain long runs
- **Strategic Considerations**:
  - Tower composition must handle all boss abilities
  - Economy management critical - use prep time wisely
  - Boss-specific counters (range for Swarm Mother, burst for Shield Commander)
  - Long-term tower investment becomes essential
  - Map positioning and coverage increasingly important
  - Achievement system support for boss milestones
- **Integration Notes**:
  - Requires boss spawn point configuration per map
  - Boss prefabs must be assigned in Inspector
  - Compatible with Challenge Mode (future: boss rush challenges)
  - Works with existing WaveManager infrastructure
  - Uses ObjectPooler for efficient boss spawning

**Mega Factory Map** 🏭
- **Map 6: Mega Factory**: Ultimate endgame challenge map (Difficulty: 5/5)
  - Post-release content for elite players who completed main campaign
  - 30 waves of extreme difficulty with aggressive scaling
  - Complex multi-loop industrial path requiring strategic tower placement
  - Starting resources: 700 credits, 15 lives (fewer lives, higher challenge)
  - Difficulty multiplier: 1.6x (significantly harder than campaign maps)
  - Dark industrial atmosphere with heavy fog effects
  - No next map - final ultimate challenge
- **Wave Composition**:
  - No easy introduction - starts hard immediately with mixed enemy types
  - Multiple boss waves: 10, 15, 20, 25, 30 (5 boss encounters total)
  - Wave 10: 2x Shield Commander + Elite support
  - Wave 15: 2x Swarm Mother + Massive flying assault
  - Wave 20: 3x Titan + Heavy armor support
  - Wave 25: Multi-boss combo (Shield Commander + Swarm Mother)
  - Wave 30: "Mega Factory Apocalypse" - 12 total bosses + 100+ support enemies
- **Scaling Parameters**:
  - Health: 1.2x + 0.25x per wave (much more aggressive than campaign)
  - Speed: 1.1x + 0.04x per wave (fastest enemies in game)
  - Spawn rate: 0.5s between enemies (relentless pressure)
  - Preparation time: 6s (less time to strategize)
  - Rewards: 1.2x + 0.08x per wave (compensates for difficulty)
- **Enemy Counts** (Late Game):
  - Waves 21-24: 30+ Elites, 25+ Tanks, 20+ Flying, full enemy roster
  - Wave 25: 2 Commanders + 2 Mothers + 55 support units
  - Wave 30 (Final): 5 Titans + 4 Commanders + 3 Mothers + 100 elites
  - Ultra endgame test of tower mastery and strategic planning
- **Rewards**:
  - Wave completion bonuses: 100 + (20 × wave number)
  - Boss waves grant 3-6 tech points each
  - Wave 30 grants 15 tech points + 2000 credit bonus
  - Designed for players who want maximum challenge
- **Strategic Requirements**:
  - Requires mastery of all tower types and synergies
  - Early-game economy management critical with higher starting credits
  - Must handle all enemy types simultaneously (no gaps allowed)
  - Boss waves require burst damage, crowd control, AND sustained DPS
  - Map path has multiple loops - requires full coverage
- **Integration**:
  - Created via MapContentCreator: Tools → Robot TD → Create Map Content
  - Map 6 button in editor tool
  - Unlocks after completing Map 5 (Final Assault)
  - Generates Map06_MegaFactory.asset and WaveSet_MegaFactory.asset
  - Automatically added to MapRegistry
  - Compatible with all game modes (Campaign, Challenge, etc.)
- **Challenge Integration**:
  - Works with Challenge Mode modifiers for even harder difficulty
  - Boss Rush mode excludes regular maps (separate boss gauntlet)
  - Endless Mode can activate after completing Mega Factory
  - Achievement system supports Mega Factory milestones
  - Leaderboard tracking for completion time and score

### Planned Features
- Cloud save support with conflict resolution
- Weekly missions (extended version of daily missions)
- Social features (friend leaderboards, sharing)

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
