# Changelog

All notable changes to Robot Tower Defense will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### � Version 2.0 - COMPLETE

**All V2.0 Features Successfully Implemented!**
- iOS platform support with mobile-optimized UI ✅
- Multiplayer co-op mode with real-time collaboration ✅
- Custom map editor with community sharing ✅
  - Grid-based tile editing ✅
  - Custom wave configuration ✅
  - Test play system ✅
  - Map storage and library ✅
  - Automatic thumbnail generation ✅
  - Community browser with ratings & likes ✅
- Steam platform integration ✅
  - Achievement synchronization (70+ achievements) ✅
  - Stats tracking ✅
  - Leaderboard integration ✅

### 🚀 Coming Soon - V3.0 Features

**Version 3.0 Roadmap** (In Planning)
- **Seasonal Events System**:
  - Limited-time challenges with exclusive rewards
  - Holiday-themed events (Halloween, Christmas, etc.)
  - Special event currencies and cosmetics
  - Event leaderboards and milestones
- **Tournament Mode**:
  - Competitive ladder system with ranked matches
  - Weekly tournaments with prize pools
  - Matchmaking by skill rating
  - Tournament history and replay system
- **Advanced Customization**:
  - Tower skin system (30+ cosmetic skins)
  - Map theme packs (sci-fi, fantasy, post-apocalyptic)
  - UI theme customization (dark mode, colorblind modes)
  - Player profile customization (avatars, banners, titles)
- **Expanded Content Pack**:
  - 5 new tower types:
    * Laser Grid: Area denial with sweeping beams
    * EMP Tower: Disables enemy abilities temporarily
    * Mine Layer: Deploys explosive mines on paths
    * Support Drone: Buffs nearby towers with auras
    * Weather Control: Environmental damage (lightning, frost)
  - 8 new enemy types:
    * Burrower: Underground movement, bypasses towers
    * Phaser: Teleports short distances randomly
    * Replicator: Splits into copies when damaged
    * Vampire: Heals by draining tower health
    * Berserker: Speed/damage increases when low HP
    * Mimic: Changes type to counter tower types
    * Engineer: Repairs nearby damaged enemies
    * Siege Engine: Long-range attacks on towers
  - 4 new campaign maps:
    * Space Station: Zero-gravity mechanics, multi-level paths
    * Underwater Lab: Pressure zones, bubble shields
    * Arctic Base: Freezing weather effects, ice hazards
    * Volcanic Forge: Lava flows, heat damage over time
- **Cross-Platform Progression**:
  - Unified account system across all platforms
  - Cloud save sync for Android, iOS, Steam, Web
  - Cross-platform friend lists and leaderboards
  - Platform-exclusive rewards for each store
- **Advanced AI Systems**:
  - Dynamic difficulty adjustment based on player skill
  - Adaptive enemy strategies (learns from player behavior)
  - Smart enemy routing (avoids heavily defended paths)
  - Machine learning-powered challenge generation
- **Accessibility Features**:
  - Colorblind modes (Deuteranopia, Protanopia, Tritanopia)
  - Screen reader support (menu navigation, stats)
  - Remappable controls and customizable touch zones
  - Text size scaling and high contrast UI options
  - One-handed mode for mobile devices
- **Analytics Dashboard**:
  - In-game stats viewer (kills, accuracy, efficiency)
  - Performance tracking over time (graphs, trends)
  - Tower effectiveness analysis
  - Map completion statistics
  - Personal records and milestones
- **Clan/Guild System**:
  - Create or join clans (max 50 members)
  - Clan chat and message boards
  - Clan wars (team vs team competitions)
  - Shared clan progression and rewards
  - Clan leaderboards and achievements
- **Live Operations Tools**:
  - Remote configuration without updates
  - A/B testing for features and balance
  - Feature flags for gradual rollouts
  - Real-time analytics dashboard
  - Automated event scheduling

---

## [Version 2.1] - Custom Map Editor

### Added

**Custom Map Editor System** 🗺️
- **MapEditorManager**: Complete map creation and editing system
  - Grid-based tile editing with undo/redo (50-step history)
  - Configurable grid sizes (default 20x15, customizable)
  - Multiple editor modes: Tile, Path, Spawn, Base, Obstacle, Decoration
  - Auto-save functionality (60-second intervals)
  - Keyboard shortcuts (Ctrl+Z/Y for undo/redo, Ctrl+S for save)
- **Custom Map Data Structure**:
  - Comprehensive map metadata (name, author, description, creation date)
  - 2D tile grid with 7 tile types:
    - Buildable (towers can be placed)
    - Path (enemy route)
    - Obstacle (blocked terrain)
    - Water (visual decoration)
    - SpawnPoint (enemy spawner)
    - Base (player objective)
    - Decoration (visual only)
  - Spawn point configuration with wave scheduling
  - Base position tracking
  - Obstacle and decoration placement data
  - Custom wave configuration system
  - Map statistics (play count, ratings, likes)
- **Tile Editing Tools**:
  - Single tile placement
  - Brush tool with adjustable size (1x1 to 5x5)
  - Flood fill tool for quick region painting
  - Path drawing system with drag-to-draw functionality
  - Spawn point and base placement tools
  - Support for obstacles and decorations
- **Path Drawing System**:
  - Interactive path drawing with mouse drag
  - 4-directional adjacency validation
  - Path preview during drawing
  - Cancel path drawing (right-click)
  - Automatic path tile list tracking
- **Undo/Redo System**:
  - 50-step undo history
  - Full state cloning for each undo step
  - Redo stack cleared on new actions
  - Visual buttons with enabled/disabled states
  - Stack size management to prevent memory overflow
- **Map Validation System**:
  - Comprehensive validation before save/publish
  - Error detection:
    - Map name validation (min 3 characters)
    - Spawn point requirement check
    - Base placement requirement
    - Path connectivity verification (spawn to base)
  - Warning system:
    - No wave configuration warning
    - Insufficient buildable space detection
  - Suggestion engine:
    - Starting credits recommendations
    - Wave count suggestions
  - A* pathfinding for connectivity validation
  - Validation result UI with color-coded messages
- **Map Properties Configuration**:
  - Map name and description fields
  - Starting credits (default: 500, customizable)
  - Starting lives (default: 20, customizable)
  - Recommended difficulty slider (1-5: Tutorial/Easy/Normal/Hard/Expert)
  - Estimated play time metadata
  - Author information (linked to authenticated player)
- **MapEditor UI**:
  - Professional tool palette with visual feedback
  - Mode selection buttons (Tile/Path/Spawn/Base/Obstacle/Decoration)
  - Tile type selector (Buildable/Path/Obstacle/Water)
  - Brush size slider (1-5)
  - Undo/Redo buttons with state indicators
  - Unsaved changes indicator (visual dot)
  - Coordinate display for hovered tile
  - Current mode display
  - Properties panel for map configuration
  - Validation panel with error/warning/suggestion display
  - Confirmation dialogs for destructive actions
  - Status message system with auto-hide
  - Test play button with validation and scene loading ✅
- **Test Play System** ✅:
  - Test play button in MapEditorUI for instant map testing
  - Pre-test validation to ensure map is playable
  - Automatic map save before test play
  - Custom map loading in MapManager via LoadCustomMap()
  - Conversion of tile grid to waypoints for enemy pathing
  - Placement grid generation from custom map data
  - Return to editor functionality ✅
  - Test play analytics tracking (map_editor_test_play event)
  - Unsaved changes warning before test play
  - Error display if validation fails
  - Static property system for cross-scene custom map transfer
  - Test play mode flag to distinguish from normal gameplay
- **Return to Editor System** ✅:
  - Return to Editor button in GameHUD (test play mode only)
  - Test play mode indicator in HUD ("TEST PLAY MODE" badge)
  - Return to Editor buttons in victory and defeat panels
  - Analytics tracking for editor returns with game results
  - Automatic cleanup of test play state
  - Main menu buttons hidden in test play mode
  - MapEditorManager.ReturnToEditor() static method
  - Scene transition back to MapEditor scene
  - Session data tracking (final wave, score, result)
  - Audio feedback on return to editor
- **Save/Load System**:
  - JSON serialization for custom maps
  - Local storage via PlayerPrefs
  - Map ID generation (GUID-based)
  - Version tracking for future compatibility
  - Clone functionality for map duplication
  - Auto-save with configurable interval
  - Unsaved changes detection
  - Backup/restore capability (via SaveManager integration)
- **CustomMapStorage Manager**:
  - Complete local file system storage for custom maps
  - Save/Load operations with JSON serialization
  - Map library management (up to 100 local maps)
  - Automatic backup system (max 5 backups per map)
  - Backup/restore functionality with timestamp tracking
  - Storage limit enforcement (configurable max maps)
  - File size tracking and storage usage monitoring
  - Map metadata caching for fast browsing
  - Thumbnail generation and storage
  - Export/Import functionality for map sharing
  - Search functionality by name and author
  - Filter by difficulty level
  - Sort options: Date modified/created, name, play count, rating
  - Duplicate map functionality (Save As)
  - Delete with backup protection
  - Directory structure: CustomMaps/, Thumbnails/, Backups/
  - Event system: OnMapSaved, OnMapLoaded, OnMapDeleted, OnLibraryRefreshed
- **Map Library Browser UI**:
  - Professional map library interface with grid/list views
  - Map card display with thumbnails, difficulty, stats
  - Search bar for finding maps by name/author
  - Sort dropdown (8 sort options)
  - Difficulty filter dropdown
  - Storage usage indicator (map count + total size)
  - Selected map details panel:
    - Full map information
    - Thumbnail preview
    - Grid size and difficulty
    - Play count and rating statistics
    - Creation/modification dates
    - File size information
  - Map actions:
    - Load Map (play in campaign)
    - Edit Map (open in editor)
    - Delete Map (with confirmation)
    - Export Map (save to file)
    - Duplicate Map (clone with new name)
  - Confirmation dialogs for destructive actions
  - Status message system
  - Auto-refresh on map changes
  - Empty state message
  - LeanTween animations for smooth transitions
- **Map Card UI Component**:
  - Compact card view for map list
  - Thumbnail display with placeholder fallback
  - Map name and author display
  - Grid size badge
  - Difficulty indicator with color coding
  - Star rating display (0-5 stars)
  - Play count statistic
  - Date modified (smart formatting: "2h ago", "3d ago", "Jan 15")
  - File size display
  - Selection outline
  - Hover animation (scale up 1.05x)
  - Click to select and view details
- **Thumbnail System**:
  - PNG format thumbnail storage
  - **Automatic thumbnail generation** ✅:
    - MapThumbnailGenerator singleton component
    - Orthographic camera render capture (256x256 PNG)
    - Auto-calculated ortho size based on map dimensions
    - Top-down grid view capture on map save
    - Analytics tracking for generation success/failure
    - Configurable resolution (64-1024px), background color, layer mask
  - Manual thumbnail upload support
  - Lazy loading for performance
  - Placeholder image fallback
  - Thumbnail deletion on map delete
- **Mouse Input Handling**:
  - Left-click for placement
  - Left-drag for continuous painting
  - Right-click for removal/cancel
  - Grid raycasting for tile selection
  - Coordinate display on hover
  - Mode-specific input behavior
- **Visual Grid System**:
  - Tile prefab instantiation for visual feedback
  - Parent-child hierarchy for organization
  - Tile visual updates on placement
  - Color coding by tile type
  - Spawn point and base prefab support
  - Path marker visualization
- **Custom Wave Configuration** ✅:
  - **WaveConfigurationUI**: Complete wave editor panel
    - Wave list with add/remove/edit functionality
    - Wave card display showing enemy count, rewards
    - Wave editor panel with configurable properties:
      - Credits reward per wave
      - Time between enemy groups
      - Boss wave toggle with boss type selection
    - Enemy group configuration:
      - 12 enemy types supported: Scout, Soldier, Tank, Elite, Flying, Healer, Splitter, Teleporter, HeavyAssault, SwarmMother, ShieldCommander, Cloaker
      - Enemy type dropdown selector
      - Enemy count (1-100 per group)
      - Spawn interval (0.1-10 seconds)
      - Spawn point index selection
      - Add/remove enemy groups
    - Quick Setup Tools:
      - Generate default waves (5-30 waves)
      - Difficulty presets: Tutorial, Easy, Normal, Hard, Expert
      - Automatic wave scaling and composition
      - Boss wave generation (every 5 waves)
      - Enemy type progression based on wave number
    - Validation system with error messages
    - Real-time wave count display
    - Auto-save integration with map save system
- **WaveManager Integration** ✅:
  - Custom wave loading from CustomMapData
  - GenerateFromCustomWave() method for test play
  - Priority system: Custom waves → WaveSetData → Procedural generation
  - Enemy type parsing from string IDs
  - Custom spawn timing from wave configuration
  - Credits reward tracking from custom waves
  - Boss spawn integration from wave data
  - Enemy count and remaining enemies tracking
- **CustomMapData Extensions** ✅:
  - CustomWaveData class with wave configuration:
    - Wave number sequencing
    - Enemy groups list (EnemySpawnGroup collection)
    - Time between groups
    - Credits reward per wave
    - Boss type (optional)
  - EnemySpawnGroup class structure:
    - Enemy type string identifier
    - Enemy count per group
    - Spawn interval timing
    - Spawn point index
  - Waves list property for storing all custom waves
  - JSON serialization support for save/load
- **Wave Configuration Analytics** ✅:
  - `wave_config_opened`: Track wave editor sessions
  - `wave_config_wave_added`: New wave creation
  - `wave_config_wave_edited`: Wave modifications
  - `wave_config_wave_deleted`: Wave deletion
  - `wave_config_enemy_group_added`: Enemy group additions
  - `wave_config_enemy_group_deleted`: Group deletions
  - `wave_config_generated`: Auto-generation usage
  - `wave_config_saved`: Wave configuration saves
  - Analytics parameters: map_id, wave_count, difficulty, existing_waves
- **Wave Configuration UI Integration** ✅:
  - **MapEditorUI Extensions**:
    - "Configure Waves" button added to main editor panel
    - WaveConfigurationUI component reference
    - OnConfigureWavesClicked() handler with validation
    - Map load status checking before opening wave config
    - Button wiring in SetupButtonListeners()
  - **Enhanced Map Validation**:
    - Comprehensive wave configuration validation
    - Wave number consistency checks
    - Enemy group validation (count, type, spawn intervals)
    - Total enemy count per wave validation (warns if >100, suggests if <5)
    - Credits reward validation
    - Wave count recommendations (suggests 5+, warns if >50)
    - Performance warnings for large enemy counts
    - Enemy type validation (checks for empty types)
    - Spawn interval validation (warns if <0.1s)
  - **Test Play Enhancements**:
    - Wave configuration status logging
    - Custom vs procedural wave detection
    - Debug messages for wave count during test play
  - **UI/UX Improvements**:
    - Status messages for wave config access
    - Map initialization checks
    - Reference validation with user feedback
    - Seamless integration with existing editor workflow
- **Wave Configuration UI Setup Guide** ✅:
  - **Documentation File**: WAVE_CONFIG_UI_SETUP.md
  - **Wave Card Prefab Specification**:
    - Complete hierarchy and component breakdown
    - RectTransform anchor configurations
    - Required child objects: WaveNumberText, EnemyCountText, RewardText, EditButton, DeleteButton
    - Button styling and interaction specifications
    - Visual design guidelines with measurements
  - **Enemy Group Prefab Specification**:
    - Full prefab structure documentation
    - Component requirements: EnemyTypeDropdown, CountInput, SpawnIntervalInput, DeleteButton
    - TMP_Dropdown setup instructions
    - Input field validation specifications
    - Layout and positioning details
  - **Scene Setup Instructions**:
    - Complete MapEditor scene hierarchy
    - WaveConfigurationUI GameObject structure
    - 40+ serialized field assignments documented
    - Layout Group configurations (Vertical Layout, Content Size Fitter)
    - ScrollRect setup recommendations
    - MapEditorUI integration steps
  - **Testing Checklist**:
    - 15+ test cases for wave card functionality
    - 7+ test cases for enemy group prefab
    - 8+ full system integration tests
    - Verification steps for all features
  - **Styling Guidelines**:
    - Color scheme specifications (6 theme colors)
    - Font size standards (4 categories)
    - Spacing guidelines (4 levels)
    - Consistent UI/UX patterns
  - **Troubleshooting Section**:
    - Common error resolutions
    - Object naming verification steps
    - Layout debugging guidance
    - Dropdown setup fixes
  - **Implementation Support**:
    - Step-by-step Unity editor instructions
    - Component configuration details
    - Visual examples and ASCII diagrams
    - Best practices and recommendations
- **Automatic Thumbnail Generation** ✅:
  - **MapThumbnailGenerator Component**:
    - Singleton design pattern for global access
    - Configurable thumbnail resolution (default 256x256, range 64-1024px)
    - Automatic orthographic camera setup for top-down capture
    - Auto-calculated ortho size based on map dimensions
    - 4x MSAA anti-aliasing for high-quality thumbnails
    - Configurable background color and layer mask
  - **Render Pipeline**:
    - RenderTexture-based capture system
    - Orthographic camera positioned above map center
    - 90-degree downward rotation for perfect top-down view
    - Dynamic camera positioning based on grid dimensions
    - PNG encoding for optimal file size/quality balance
  - **Integration with MapEditorManager**:
    - Automatic thumbnail generation on map save
    - Seamless integration with SaveCurrentMap() workflow
    - Lazy initialization of thumbnail generator instance
    - Grid bounds calculation using tileSize and gridOffset
    - Error handling and fallback behavior
  - **Analytics Tracking**:
    - map_editor_generate_thumbnail event
    - Success/failure tracking with map_id and map_name
    - Debug logging for troubleshooting
  - **File System Management**:
    - Thumbnails saved to Application.persistentDataPath/Thumbnails/
    - Automatic directory creation if missing
    - PNG format with {mapId}.png naming convention
    - Thumbnail deletion utility method
    - Thumbnail existence checking
  - **Configuration API**:
    - SetThumbnailSize(width, height) for custom resolutions
    - SetBackgroundColor(color) for custom backgrounds
    - SetCaptureLayerMask(mask) for selective rendering
    - GetThumbnailPath(mapId) for file path retrieval
    - DeleteThumbnail(mapId) for cleanup operations
  - **Technical Details**:
    - DontDestroyOnLoad for persistence across scenes
    - Proper resource cleanup (RenderTexture release, camera destruction)
    - Exception handling with detailed error logging
    - Null checks and validation throughout
- **Community Features** ✅:
  - **MapSharingManager Component**:
    - Singleton design pattern for global access
    - Integration with AuthenticationManager for user identification
    - Configurable cloud backend endpoint
    - Max 50 maps per user limit (configurable)
    - 5-minute browser cache for performance
    - Comprehensive error handling and user feedback
  - **Map Publishing System**:
    - PublishMap() uploads map data, thumbnail, and metadata
    - Automatic difficulty calculation (1-5 based on wave count)
    - Tag support for categorization
    - Author attribution (user ID and name)
    - Metadata includes: grid size, wave count, dates, stats
    - Analytics tracking: map_published event
  - **Map Download System**:
    - DownloadMap() fetches maps by ID
    - Automatic local storage after download
    - Thumbnail download and caching
    - Play count increment on download
    - Analytics tracking: map_downloaded event
  - **Community Browser**:
    - LoadCommunityMaps() with filtering support
    - Page-based results (20 maps per page, configurable)
    - Cache system with 5-minute timeout
    - RefreshCommunityBrowser() for manual refresh
    - Search by: map name, author name, description
    - Filter by: tags, difficulty range, minimum rating
    - Sort by: most recent, most played, highest rated, most liked, alphabetical
    - Analytics tracking: community_maps_loaded event
  - **Rating System**:
    - RateMap() for 1-5 star ratings
    - GetUserRating() to retrieve user's rating
    - Local storage of user ratings (PlayerPrefs)
    - Cloud sync of rating data
    - Average rating and rating count per map
    - Analytics tracking: map_rated event
  - **Like System**:
    - ToggleLikeMap() for liking/unliking maps
    - HasUserLiked() to check like status
    - Local storage of user likes (PlayerPrefs)
    - Cloud sync of like data
    - Like count per map
    - Analytics tracking: map_liked event
  - **Play Count Tracking**:
    - IncrementPlayCount() called when maps are played
    - Server-side tracking for accurate statistics
    - Used for "most played" sorting
    - Analytics tracking: community_map_played event
  - **Search & Filter System**:
    - MapSearchFilter class with comprehensive options
    - Text search across name, author, description
    - Tag-based filtering with multiple tags support
    - Difficulty range filtering (min/max)
    - Minimum rating filter
    - Pagination support (page index and size)
    - 5 sort options: MostRecent, MostPlayed, HighestRated, MostLiked, Alphabetical
  - **Data Structures**:
    - CommunityMapMetadata: Complete map metadata class
    - MapSearchFilter: Search and filter configuration
    - MapSortOption: Enum for sort options
    - UserRatingsData/UserLikesData: Serialization classes
  - **Editor Simulation Mode**:
    - UNITY_EDITOR conditional compilation
    - Sample data generation for testing
    - Network delay simulation (1s)
    - Local storage fallback for downloads
    - Full feature testing without backend
  - **Production Backend Support**:
    - Placeholder methods for REST API integration
    - UnityWebRequest-ready architecture
    - Multipart form data upload support
    - GET/POST endpoint structure defined
    - TODO markers for actual implementation
  - **Analytics Events** (6 new):
    - map_sharing_initialized: System startup
    - map_published: Map upload with metadata
    - map_downloaded: Map download tracking
    - community_maps_loaded: Browser usage
    - map_rated: Rating submissions
    - map_liked: Like interactions
    - community_map_played: Play count tracking
- **Steam Platform Integration** ✅:
  - **SteamIntegrationManager Component**:
    - Singleton design pattern with DontDestroyOnLoad persistence
    - Steamworks.NET SDK integration (conditional compilation with STEAM_ENABLED)
    - Editor simulation mode for testing without Steam SDK
    - Auto-sync for achievements and stats (configurable intervals)
    - 30-second stats update interval (configurable)
    - Comprehensive error handling and logging
  - **Achievement Synchronization**:
    - Automatic sync with AchievementManager on unlock events
    - UnlockSteamAchievement() for manual achievement unlocking
    - SyncAchievementsToSteam() for first-time sync or post-reinstall
    - IsSteamAchievementUnlocked() for checking unlock status
    - ClearSteamAchievement() for testing (Editor only)
    - Achievement name conversion: AchievementId → "ACH_{ENUM_NAME_UPPERCASE}"
    - Event subscription to AchievementManager.OnAchievementUnlocked
    - Duplicate prevention with syncedAchievements HashSet
  - **Stats Tracking System**:
    - SetStat(string, int) for integer stat values
    - GetStat(string) for retrieving stat values
    - IncrementStat(string, int) for incrementing stats
    - SyncStatsToSteam() with automatic interval-based sync
    - Local caching with Dictionary<string, int>
    - OnStatUpdated event for UI updates
  - **Leaderboard Integration**:
    - UploadLeaderboardScore(string, int) for generic uploads
    - UploadHighScore(int) shortcut for main leaderboard
    - UploadHighestWave(int) for wave progression
    - UploadTotalKills(int) for kill count tracking
    - Configurable leaderboard names (HighScores, HighestWave, MostKills)
    - OnLeaderboardScoreUploaded event with rank information
    - Placeholder structure ready for Steamworks callback implementation
  - **Initialization System**:
    - SteamAPI.RestartAppIfNecessary() check with App ID
    - SteamAPI.Init() with error handling
    - Steam user ID and username retrieval
    - OnSteamInitialized event for dependent systems
    - Graceful degradation for non-Steam builds
  - **Runtime Management**:
    - SteamAPI.RunCallbacks() in Update loop (required for Steamworks.NET)
    - Automatic stats sync before application quit
    - SteamAPI.Shutdown() on application exit
    - Exception handling for all Steam API calls
  - **Simulation Mode** (Editor Testing):
    - Full feature testing without Steamworks.NET SDK
    - Conditional compilation with #if STEAM_ENABLED
    - Simulated Steam user ID and username
    - Local achievement and stat tracking
    - Event firing for UI testing
    - Network delay simulation for async operations
  - **Production Configuration**:
    - Scripting Define Symbol: STEAM_ENABLED
    - steam_appid.txt configuration requirement
    - Steamworks.NET SDK installation from Unity Asset Store or GitHub
    - Steam Partner dashboard achievement ID mapping
    - App ID configuration (replace placeholder 480)
  - **Public API**:
    - GetSteamUserId() - Steam user identifier
    - GetSteamUsername() - Steam persona name
    - IsSteamRunning() - Check Steam API status
    - IsInitialized() - Check manager readiness
  - **Events System**:
    - OnSteamInitialized(bool) - Initialization result
    - OnAchievementUnlocked(AchievementId) - Achievement sync confirmation
    - OnStatUpdated(string, int) - Stat change notification
    - OnLeaderboardScoreUploaded(string, bool, int, int) - Upload result with rank
    - OnSteamError(string) - Error notifications
  - **Analytics Events** (3 new):
    - steam_initialized: SDK startup with user info
    - steam_achievement_unlocked: Achievement sync tracking
    - steam_leaderboard_upload: Score upload tracking
  - **Setup Documentation**:
    - Comprehensive XML documentation in source code
    - Step-by-step setup instructions in class header
    - Achievement naming convention documentation
    - Public API summary with usage examples
- **Analytics Integration** (20 new events):
  - `map_editor_opened`: Track editor sessions
  - `map_editor_new_map`: New map creation
  - `map_editor_load_map`: Existing map loading
  - `map_editor_save_map`: Map save events
  - `map_editor_place_spawn`: Spawn point placement
  - `map_editor_place_base`: Base placement
  - `map_editor_draw_path`: Path drawing actions
  - `map_editor_validate`: Validation attempts
  - `map_editor_test_play`: Test play sessions
  - `map_editor_publish`: Map publishing
  - `custom_map_played`: Community map plays
  - `custom_map_rated`: Player ratings
  - `custom_map_downloaded`: Map downloads
  - `custom_map_shared`: Social sharing
  - `map_storage_deleted`: Map deletion tracking
  - `map_storage_exported`: Map export events
  - `map_storage_imported`: Map import tracking
  - `map_library_opened`: Library browser usage
  - `map_backup_created`: Backup creation events
  - `map_backup_restored`: Backup restore tracking
- **Analytics Parameters** (14 new params):
  - `map_id`, `map_name`, `map_author`: Map identification
  - `grid_width`, `grid_height`: Map dimensions
  - `is_valid`, `validation_errors`: Validation state
  - `play_count`, `rating`: Community metrics
  - `tile_count`, `spawn_count`, `path_length`: Map statistics
  - `file_size`: Storage metrics
  - `total_maps`: Library size
  - `storage_size`: Total storage used
- **Editor Features**:
  - Context menu commands for testing
  - Debug logging toggle
  - Test map generation
  - Map validation command
  - Grid offset configuration
  - Tile size customization
- **Integration Points**:
  - AuthenticationManager: Author identification
  - AnalyticsManager: Event tracking
  - CustomMapStorage: File system persistence and library management
  - SaveManager: Backup integration
  - MapSelector: Custom map loading (future)
  - CloudSaveManager: Cloud storage (future)
- **Best Practices**:
  - Singleton pattern for MapEditorManager
  - Event-driven architecture for UI updates
  - State cloning for undo/redo
  - Validation before save/publish
  - Auto-save to prevent data loss
  - Confirmation dialogs for destructive actions

---

## [Version 1.8] - Weekly Missions & Social Features

### Added

**Weekly Missions System** 📅
- **Mission Rotation System**: 7-day rotating weekly missions
  - Separate weekly mission pool independent from daily missions
  - 3 weekly missions active simultaneously (vs 3 daily missions)
  - 7-day rotation cycle (Monday to Monday)
  - Progressive difficulty scaling comparable to daily missions
  - Mission types: Tower placement, enemy elimination, wave survival, resource management
- **WeeklyMissionSet Class**: Tracks weekly rotation state
  - Start timestamp for current week
  - Mission ID array for active weekly missions
  - Completion tracking separate from daily missions
  - Automatic rollover on week expiration
- **MissionRotationType Enum**: Daily vs Weekly classification
  - Used to differentiate mission types in MissionData
  - Enables dual-tracking in MissionManager
  - Separate analytics events per rotation type
- **Dual Mission UI**: Tabbed interface for Daily/Weekly missions
  - DailyMissionsUI enhanced with tab switching
  - Daily tab: Shows 3 daily missions with 24-hour timer
  - Weekly tab: Shows 3 weekly missions with day counter (e.g., "3d 15h")
  - Tab visual feedback (color coding, active state)
  - Smooth transitions between tabs
  - Shared mission card prefab for consistency
- **Weekly Mission Analytics** (6 new events):
  - `weekly_missions_rotated`: Fired every Monday midnight
  - `weekly_mission_progress`: Tracked with mission_id, progress, target
  - `weekly_mission_completed`: Logged with rewards and completion time
  - `weekly_mission_reward_claimed`: Tracks reward distribution
  - `all_weekly_missions_complete`: Bonus reward eligibility tracking
  - `weekly_bonus_reward_claimed`: Tracks bonus payouts
- **Backend Integration**:
  - SaveManager stores weekly mission state separately
  - JSON serialization for WeeklyMissionSet
  - Cloud sync support (if CloudSaveManager enabled)
  - Offline-first design with local persistence
- **Enhanced MissionManager**:
  - `CheckAndRotateWeeklyMissions()`: Validates weekly expiration
  - `RotateWeeklyMissions()`: Generates new weekly set
  - `GetTimeUntilWeeklyRotation()`: Returns TimeSpan for UI countdown
  - `GetActiveWeeklyMissions()`: Returns current weekly mission list
  - Separate event triggers for weekly mission lifecycle
- **Configuration**:
  - Weekly rotation time: Monday 00:00 UTC (configurable)
  - Mission count per week: 3 (matches daily count)
  - Bonus reward for completing all weeklies
  - Weekly missions persist across sessions

**Social Features System** 👥
- **SocialManager**: Complete friend management system
  - Multi-backend support: Unity Gaming Services, PlayFab, Custom HTTP API
  - Friend list management with max 100 friends
  - Pending request tracking (max 50 requests)
  - Player search functionality
  - Score and achievement sharing
  - Event-driven architecture for UI updates
  - Editor simulation mode for testing without backend
- **Friend Management**:
  - Send/receive friend requests with player ID or name
  - Accept/decline friend requests
  - Remove friends from list
  - Block/unblock functionality (backend-dependent)
  - Friend status tracking (online/offline, last seen)
  - Friend count limits (100 friends, 50 pending requests)
- **Friend Leaderboards**:
  - LeaderboardManager extended with friend filtering
  - `FetchFriendLeaderboard()`: Query scores from friends only
  - Friend ID list integration with SocialManager
  - Efficient friend-only score filtering
  - Offline mode: Filter local scores by friend IDs
  - Backend implementations for UGS, PlayFab, Custom API
  - Friend leaderboard caching (5-minute cache lifetime)
- **Score & Achievement Sharing**:
  - Share high scores with friends
  - Share unlocked achievements
  - Broadcast to specific friends or all friends
  - Custom message support
  - Share feed/timeline (UI component ready)
  - Backend notification system integration
- **Player Search**:
  - Search players by name or player ID
  - Autocomplete suggestions (backend-dependent)
  - Search result filtering (exclude existing friends)
  - Friend status display in search results
  - Quick "Add Friend" from search results
- **UI Components**:
  - **FriendsUI**: Tab-based interface
    - Friends tab: Scrollable friend list with online status
    - Requests tab: Pending friend requests (incoming)
    - Search tab: Player search with add friend button
    - Tab switching with visual feedback
    - Refresh button for manual list updates
    - Friend count display (e.g., "Friends: 42/100")
  - **FriendCardUI**: Individual friend list entry
    - Player name and online status indicator
    - Last seen timestamp ("2h ago", "3d ago")
    - View profile button (future: full profile view)
    - Remove friend button with confirmation
    - Online indicator color (green = online, gray = offline)
  - **FriendRequestCardUI**: Pending request item
    - Requester name and time ago
    - Accept/Decline buttons
    - Auto-disable buttons after action (prevent double-click)
  - **PlayerSearchResultCardUI**: Search result item
    - Player name and online status
    - "Add Friend" button (or "Already Friends" indicator)
    - Request sent feedback ("Request Sent" button state)
  - **LeaderboardUI Enhanced**:
    - Global/Friends scope toggle buttons
    - Active scope visual feedback (color coding)
    - Title updates based on scope ("Endless Mode (Friends)")
    - Smooth scope switching without full reload
    - Analytics tracking for friend leaderboard views
- **Social Analytics** (10 new events):
  - `friend_request_sent`: Tracks outgoing requests
  - `friend_request_received`: Tracks incoming requests
  - `friend_request_accepted`: Successful friend additions
  - `friend_request_declined`: Rejected requests
  - `friend_removed`: Unfriend actions
  - `player_searched`: Search queries (with search term)
  - `score_shared`: Score sharing events with friend count
  - `achievement_shared_social`: Achievement sharing
  - `friend_leaderboard_viewed`: Friend leaderboard opens
  - `friends_list_viewed`: Friends UI opens
- **Backend API Endpoints** (Custom HTTP):
  - `POST /api/social/friends/request`: Send friend request
  - `POST /api/social/friends/accept`: Accept request
  - `POST /api/social/friends/decline`: Decline request
  - `POST /api/social/friends/remove`: Remove friend
  - `GET /api/social/friends/{player_id}`: Get friend list
  - `GET /api/social/friends/requests/{player_id}`: Get pending requests
  - `POST /api/leaderboards/{id}/friends`: Friend leaderboard query
  - `GET /api/social/search?query=name`: Player search
- **Integration Points**:
  - AuthenticationManager: Uses authenticated player ID
  - LeaderboardManager: Friend filtering for leaderboards
  - AnalyticsManager: Tracks all social interactions
  - SaveManager: Local friend data caching
  - MessageBox: Friend removal confirmations
- **Configuration Options**:
  - Enable/disable friend features
  - Max friends limit (default: 100)
  - Max pending requests (default: 50)
  - Friend leaderboard cache duration (default: 5 min)
  - Auto-accept friend requests (optional)
  - Privacy settings (who can send requests)
- **Security & Privacy**:
  - Friend request spam prevention (rate limiting)
  - Block list support (backend-dependent)
  - Privacy controls for leaderboard visibility
  - Secure friend ID validation
  - Request validation on backend
- **Complete Documentation**: [SOCIAL_FEATURES_GUIDE.md](SOCIAL_FEATURES_GUIDE.md)
  - Architecture overview and component relationships
  - Backend setup guides (Unity/PlayFab/Custom)
  - Friend system usage examples
  - Friend leaderboard integration
  - Score/achievement sharing workflows
  - UI setup instructions with prefab requirements
  - Testing scenarios with editor simulation
  - Analytics event reference
  - Comprehensive troubleshooting guide
  - Security best practices

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

## [Version 1.5] - Ad Monetization System

### Added

**Ad Monetization** 📺
- **AdManager**: Complete Unity Ads integration for mobile monetization
  - Unity Ads SDK integration with IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
  - Multi-platform support: Android and iOS with platform-specific Game IDs
  - Test mode for development and production mode for release
  - Singleton pattern with DontDestroyOnLoad persistence
  - Automatic ad loading and pre-caching for instant display
  - Editor simulation mode for testing without Unity Ads SDK
- **Ad Types (3 placements)**:
  - **Interstitial Ads**: Full-screen ads shown between gameplay sessions
    - Shown after game over, mission complete, or map transitions
    - Frequency-controlled with cooldown (default 3 minutes)
    - Gameplay-based triggers (show after X completed games, default 3)
    - Never interrupts active gameplay
  - **Rewarded Ads**: Opt-in ads that grant player bonuses
    - Rewards: Gems, credits, continue game, double rewards, extra life
    - 1-minute cooldown (configurable)
    - Available even if "Remove Ads" purchased (optional bonus)
    - Clear value proposition shown before ad
  - **Banner Ads**: Small persistent ads for menu screens
    - Configurable position (top/bottom/center, left/center/right)
    - Show on main menu, shop, leaderboards
    - Hide during active gameplay
    - Low-value but constant revenue stream
- **Frequency Controls**:
  - Interstitial cooldown: 3 minutes default (prevent spam)
  - Rewarded cooldown: 1 minute default (reasonable re-watch interval)
  - Gameplay count: Show interstitial after 3 games (not every game)
  - Query API: `CanShowInterstitial()`, `CanShowRewarded()`
  - Cooldown helpers: `GetInterstitialCooldownRemaining()`, `GetRewardedCooldownRemaining()`
  - Gameplay counter: `OnGameplaySessionComplete()` tracks session count
- **IAP Integration**:
  - Automatically checks `IAPManager.AreAdsRemoved()`
  - Interstitial ads skipped if "Remove Ads" purchased
  - Banner ads hidden if "Remove Ads" purchased
  - Rewarded ads still available for premium users (optional bonus)
  - Seamless integration without code duplication
- **RewardedAdButton UI Component**:
  - Reusable UI component for rewarded ad placements
  - Displays reward preview: "Watch Ad\n+50 Gems"
  - Dynamic reward configuration: SetReward(RewardType, amount)
  - Cooldown timer display with countdown text
  - Cooldown fill bar (radial or linear)
  - Auto-hide if "Remove Ads" purchased (configurable)
  - Reward types: Gems, Credits, ContinueGame, DoubleReward, ExtraLife
  - Automatic reward granting on ad completion
  - Toast notifications for success/failure
- **Ad Flow Management**:
  - OnUnityAdsAdLoaded: Cache loaded ads for instant display
  - OnUnityAdsShowStart: Pause game (Time.timeScale = 0, AudioListener.pause = true)
  - OnUnityAdsShowComplete: Resume game, grant rewards, reload next ad
  - OnUnityAdsShowFailure: Graceful failure handling, resume game
  - Retry logic: Auto-reload failed ads after 5-second delay
  - Transaction tracking: Prevent duplicate reward grants
- **Analytics Integration**:
  - All ad events tracked via AnalyticsManager:
    - ads_initialized: Unity Ads ready
    - ad_shown: Ad started (type, placement)
    - ad_clicked: User clicked ad
    - ad_completed: Ad finished (completion state)
    - ad_reward_earned: Rewarded ad completed (reward type, amount)
    - ad_load_failed: Ad failed to load (error, message)
    - ad_show_failed: Ad failed to display (error, message)
    - rewarded_ad_reward_claimed: Reward granted to player
  - Conversion funnel tracking for optimization
  - Revenue metrics: Impressions, clicks, eCPM
- **Editor Simulation**:
  - Simulate all ad types without Unity Ads SDK
  - Configurable simulated ad duration (default 2 seconds)
  - Full event callbacks (OnAdShown, OnAdClosed, OnRewardEarned)
  - Test reward granting logic without real ads
  - Context menu commands:
    - Show Interstitial Ad
    - Show Rewarded Ad
    - Show Banner
    - Hide Banner
    - Print Ad Status (cooldowns, initialization, IAP status)
- **Platform Configuration**:
  - Separate Game IDs for Android and iOS
  - Platform-specific placement IDs
  - Test mode toggle (development vs production)
  - Enable/disable ads globally (for regions without monetization)
  - Banner position per platform (Android often bottom, iOS safer at top)
- **Event System**:
  - OnAdsInitialized(): Ads ready, start showing
  - OnAdShown(placementId): Ad started, pause gameplay
  - OnAdClosed(placementId): Ad finished, resume gameplay
  - OnAdFailed(placementId, error): Handle failures gracefully
  - OnRewardEarned(rewardType): Notify UI of reward grant
- **Best Practices Built-In**:
  - Never show interstitials during active gameplay
  - Always show value for rewarded ads
  - Respect "Remove Ads" IAP purchase
  - Cooldowns prevent ad fatigue
  - Graceful failure handling (never block gameplay)
  - Analytics for revenue optimization
- **Complete Documentation**: [ADS_GUIDE.md](ADS_GUIDE.md)
  - Unity Ads setup and SDK installation
  - Game ID and placement ID configuration
  - Ad type explanations (when to use each)
  - Frequency control tuning guidelines
  - IAP integration architecture
  - RewardedAdButton UI setup
  - Integration examples (game over, victory, shop, main menu)
  - Editor simulation testing procedures
  - Device testing checklist
  - Production launch checklist
  - Best practices for player-friendly monetization
  - Comprehensive troubleshooting guide
  - Ad performance optimization tips

---

## [Version 1.6] - Power-Ups System

### Added

**Power-Ups & Consumables** ⚡
- **PowerUpManager**: Temporary gameplay boosts with inventory management
  - Five power-up types: Damage Boost, Speed Boost, Credit Boost, Shield, Time Freeze
  - Inventory system: Stack up to 99 of each type
  - Duration-based effects: 30-60 second temporary boosts
  - Persistent storage via PlayerPrefs
  - Active power-up tracking with expiration timers
  - Effect stacking: Extend duration if activated while active
  - Singleton pattern with DontDestroyOnLoad persistence
- **Power-Up Types (5)**:
  - **Damage Boost**: 2x tower damage for 30 seconds
    - Applies to all towers automatically
    - Perfect for boss waves or tough enemies
    - Multiplier configurable in Inspector (default 2.0)
  - **Speed Boost**: 1.5x tower fire rate for 30 seconds
    - Increases attack speed for all towers
    - Great for dealing with fast enemy swarms
    - Multiplier configurable (default 1.5)
  - **Credit Boost**: 2x credits earned for 30 seconds
    - Doubles all credit rewards from enemies
    - Ideal for economy-focused waves
    - Multiplier configurable (default 2.0)
  - **Shield**: Protects base from damage for 60 seconds
    - Blocks all incoming damage to base
    - Emergency protection during overwhelming waves
    - Duration configurable (default 60s)
  - **Time Freeze**: Slows all enemies to 10% speed for 30 seconds
    - Significantly slows enemy movement
    - Buys time to build defenses
    - Slowdown configurable (default 0.1)
- **Inventory Management**:
  - AddPowerUp(type, amount): Add to inventory
  - RemovePowerUp(type, amount): Remove from inventory
  - HasPowerUp(type, amount): Check availability
  - GetPowerUpCount(type): Get inventory count
  - GetTotalPowerUpCount(): Total across all types
  - Max stack size: 99 per power-up type
  - PlayerPrefs persistence across sessions
- **Activation System**:
  - ActivatePowerUp(type): Consume from inventory and activate
  - IsActive(type): Check if power-up currently active
  - GetRemainingTime(type): Get seconds remaining
  - Extend duration: Stack activations to extend time
  - Expiration callbacks: OnPowerUpExpired event
  - Automatic effect cleanup on expiration
- **UI Components**:
  - **PowerUpButton**: Individual power-up activation button
    - Icon display with power-up sprite
    - Inventory count text
    - Power-up name display
    - Cooldown fill image (radial timer showing remaining time)
    - Active indicator (glow/border when active)
    - Timer text (countdown in seconds)
    - Disable button when count = 0 or already active
    - SetPowerUpType(): Configure dynamically
  - **PowerUpPanel**: Container for all power-up buttons
    - Toggle expand/collapse functionality
    - Total power-up count display
    - Dynamic button creation for all types
    - Configurable start state (expanded/collapsed)
    - Show/hide in gameplay (configurable)
- **IAP Integration**:
  - Power-up Bundle product already in IAPManager
  - Price: $2.99 for 13 power-ups
  - Contents: 3x Damage, 3x Speed, 3x Credit, 2x Shield, 2x Time Freeze
  - GrantPowerUpBundle(): Called on IAP purchase
  - Automatic inventory addition and save
  - Toast notification on bundle receipt
- **Ad Integration**:
  - GrantFreePowerUp(type): Called after rewarded ad
  - Watch ad → Earn 1 random power-up
  - Integrates with AdManager seamlessly
  - Toast notification on reward
- **Gameplay Integration**:
  - Tower damage calculation: ApplyDamageBoost()
  - Tower fire rate: ApplySpeedBoost()
  - Enemy speed: ApplyTimeFreeze()
  - Base shield: IsShieldActive() check before damage
  - Credit rewards: ApplyCreditBoost(amount) multiplier
  - Automatic application/removal on activation/expiration
- **Effect Management**:
  - ActivatePowerUpEffect(): Apply gameplay changes
  - DeactivatePowerUpEffect(): Remove gameplay changes
  - Coroutine-based expiration timers
  - Update() loop tracks remaining time
  - OnPowerUpTimeUpdated: Real-time UI updates
- **Analytics Integration**:
  - All power-up events tracked via AnalyticsManager:
    - powerup_added: Power-up added to inventory (type, amount, total)
    - powerup_activated: Power-up consumed (type, duration, wave context)
    - powerup_expired: Power-up effect ended (type)
  - Usage statistics for balancing
  - Popular power-up tracking
  - Context tracking (which waves, which situations)
- **Configuration Options**:
  - Enable/disable power-ups globally
  - Default duration (30s)
  - Max stack size (99)
  - Effect multipliers per type (2x, 1.5x, etc.)
  - Shield duration (60s)
  - Time freeze slowdown (0.1 = 10% speed)
- **Event System**:
  - OnInventoryChanged(type, newCount)
  - OnPowerUpActivated(type, duration)
  - OnPowerUpExpired(type)
  - OnPowerUpTimeUpdated(type, remainingTime)
- **Context Menu Testing**:
  - Add All Power-Ups (x5)
  - Activate Damage Boost
  - Activate All Power-Ups
  - Print Inventory
  - Clear All Power-Ups
- **Complete Documentation**: [POWERUPS_GUIDE.md](POWERUPS_GUIDE.md)
  - Power-up type explanations and use cases
  - Inventory management examples
  - Activation and expiration mechanics
  - IAP and Ad integration guides
  - Tower/Enemy/GameManager integration code
  - UI setup instructions (PowerUpButton, PowerUpPanel)
  - Testing procedures and context menu commands
  - Best practices for balancing
  - Comprehensive troubleshooting guide

---

## [Version 2.0] - Multiplayer Co-op Mode

### Added

**Multiplayer Co-op System** 👥
- **MultiplayerManager**: Core networking infrastructure with Unity Netcode for GameObjects
  - Host-Client architecture: One player hosts, others connect as clients
  - Room code system: 6-character alphanumeric codes for easy room joining
  - Player management: 2-4 players per room, registration/unregistration
  - Network callbacks: OnClientConnected, OnClientDisconnected, OnServerStarted
  - Game state synchronization: SyncGameStateClientRpc() for credits, lives, wave number
  - Tower placement replication: PlaceTowerServerRpc() with server validation
  - Tower sync to all clients: TowerPlacedClientRpc() broadcasts placement
  - Chat system: SendChatMessage() with BroadcastChatMessageClientRpc()
  - Co-op balance multipliers:
    - Enemy health: 1 + (players-1) * 1.5 (2 players = 2.5x, 3 = 4.0x, 4 = 5.5x)
    - Enemy spawn rate: 1 + (players-1) * 1.3 (2 players = 2.3x, 3 = 3.6x, 4 = 4.9x)
  - Shared resources: Optional shared credits and lives (configurable)
  - Ready system: AllPlayersReady() checks before game start
  - Kick player functionality: KickPlayer() with server authority
  - Event system: OnPlayerJoined, OnPlayerLeft, OnMultiplayerStarted, OnMultiplayerEnded, OnRoomCreated, OnChatMessageReceived
  - Singleton pattern with DontDestroyOnLoad
  - Configuration: Max players (4), min players (2), sync interval (0.1s), host port (7777)
- **LobbyManager**: Matchmaking and room discovery system
  - Room creation: CreateRoom() with name, host, max players, public/private options
  - Room joining: JoinRoomByCode() validates code and room capacity
  - Quick Match: StartQuickMatch() auto-finds best room or creates new one
  - Room discovery: RefreshRoomList() queries available public rooms
  - Matchmaking timeout: 30 seconds default with OnQuickMatchFound fallback
  - Room management: LeaveRoom(), GetRoomByCode(), FindBestAvailableRoom()
  - RoomInfo data structure: roomId (GUID), roomCode, hostName, maxPlayers, currentPlayers, isPublic, mapName, difficulty
  - Network transport configuration: UnityTransport setup with IP:port
  - Event subscriptions: OnPlayerJoinedRoom, OnPlayerLeftRoom, OnConnectionFailedHandler
  - Events: OnRoomCreatedSuccess, OnRoomJoinedSuccess, OnQuickMatchFound, OnRoomListUpdated
  - Analytics tracking: Room created, joined, left, quick match started
  - Singleton pattern with lobby state management
- **NetworkedPlayer**: Individual player network synchronization
  - NetworkBehaviour component for automatic state replication
  - Network variables: Cursor position, tower placement state, selected tower type
  - Cursor synchronization: Updated 10 times/second with position interpolation
  - Remote player cursors: Visible cursors for all remote players with color coding
  - Player actions: SetReady(), SendChatMessage(), tower placement, power-up activation
  - Tower placement: TryPlaceTower() requests server validation through MultiplayerManager
  - Power-up system: TryUsePowerUp() with server-side validation and client notification
  - Player info: Name, color (for identification), client ID, permissions (place/upgrade/sell/power-ups)
  - Owner authorization: Only owner can control their player
  - Automatic registration: RegisterNetworkedPlayer() on Start, unregister on Destroy
  - Smooth cursor interpolation: Lerp remote cursors for natural movement
  - Event-driven updates: OnCursorPositionChanged, OnPlacingTowerChanged
  - Helper methods: GetMouseWorldPosition(), IsLocalPlayer(), SetPlayerName(), SetPlayerColor()
  - Context menu testing: Simulate tower placement, power-up usage
- **MultiplayerUI**: Complete lobby interface and room management
  - Four main panels: Lobby, Create Room, Join Room, Room (in-game)
  - Lobby panel:
    - Player name input with persistent save (PlayerPrefs)
    - Create Room button → Room creation panel
    - Join Room button → Room code input panel
    - Quick Match button → Automatic matchmaking
    - Room list with real-time updates: Room code, host name, player count
    - Refresh button for manual room list update
  - Create Room panel:
    - Room name input (defaults to "{PlayerName}'s Room")
    - Max players dropdown (2-4 players)
    - Public/private toggle
    - Confirm/cancel buttons
  - Join Room panel:
    - Room code input (6-character validation)
    - Confirm/cancel buttons
  - Room panel:
    - Room code and name display
    - Player list with colors and ready indicators
    - Ready button (toggles state, changes color green/red)
    - Leave Room button
    - Chat system:
      - Chat message container with scrolling
      - Chat input field with send button
      - Enter key submission support
      - Color-coded messages by player
      - 50 message history limit
    - Status text: "Waiting for players...", "Game starting..."
  - Event subscriptions: OnRoomCreated, OnRoomJoined, OnPlayerJoined/Left, OnMultiplayerStarted, OnChatMessage
  - Automatic UI updates: Player list, room list, ready states
  - Toast notifications: Room created, joined, match found
  - Panel management: Show/hide with state tracking
  - Context menu testing: Simulate room created, player joined, chat message
- **Complete Documentation**: [MULTIPLAYER_GUIDE.md](MULTIPLAYER_GUIDE.md)
  - Package installation: Unity Netcode for GameObjects, Unity Transport
  - Unity Netcode setup: NetworkManager, UnityTransport, NetworkedPlayer prefab
  - Architecture overview: Component diagram, class responsibilities
  - Room system flow: Create/join/quick match diagrams
  - Player synchronization: Network variables, RPCs, ownership
  - Game state sync: Tower placement, power-ups, credits/lives/wave
  - Testing guide: Local testing (same machine), network testing (different machines), testing checklist
  - Troubleshooting: Common issues and solutions (connection, sync, chat)
  - Best practices: Server authority, RPC throttling, ownership, scene management
  - UI setup: Complete hierarchy and prefab requirements
  - Integration examples: GameManager, WaveManager, Enemy
  - Performance considerations: Network bandwidth, CPU performance, optimization tips
  - Production deployment: Dedicated server setup, security considerations

**Co-op Balance System** ⚖️
- Dynamic enemy scaling based on player count
- Health multiplier: Exponentially scales with more players
- Spawn rate multiplier: More enemies for larger teams
- Shared vs individual resources: Configurable credits/lives
- Team coordination mechanics: Chat, ready system, synchronized game state
- Fair difficulty: Ensures challenge scales appropriately for team size

**Network Features** 🌐
- Unity Netcode for GameObjects integration
- Unity Transport (UTP) for low-latency networking
- Server authority: All actions validated on host
- Client prediction: Smooth cursor movement and UI updates
- Delta compression: Automatic by Unity Netcode
- Bandwidth optimization: ~0.5 KB/s for 4 players
- DTLS encryption: Secure communication by default

---

## [Version 1.7] - iOS Platform Support

### Added

**iOS Platform Support** 📱
- **iOSBuildConfig**: Comprehensive iOS build configuration tool
  - Editor window for all iOS build settings: **Tools > Robot Tower Defense > iOS Build Configuration**
  - Five configuration tabs: Build Settings, App Icons, Splash Screen, Capabilities, Quick Actions
  - App identity configuration: Bundle ID, version, build number
  - Code signing setup: Automatic or manual provisioning profiles
  - Device compatibility: Universal (iPhone + iPad), minimum iOS 13.0
  - Orientation settings: Landscape Left/Right (portrait disabled)
  - Optimization presets: Strip engine code, Metal API, script call optimization
  - Icon validation: Check all required icon sizes present
  - Privacy descriptions: Camera, microphone, location usage
  - Quick actions: Apply recommended settings, increment build number, validate all settings
  - Export/import build settings as JSON for CI/CD
  - One-click Xcode project generation
- **iOSInputHandler**: iOS-optimized touch input and gesture recognition
  - Multi-touch gesture detection:
    - **Tap**: Quick touch for tower placement, UI interaction
    - **Double Tap**: Two rapid taps for quick actions
    - **Long Press**: Hold 0.5s for context menus, tower info
    - **Swipe**: Directional camera control (up/down/left/right)
    - **Pinch**: Two-finger zoom for camera (future feature)
  - Gesture configuration: Configurable thresholds for swipe distance, tap duration, double-tap interval
  - Haptic feedback integration:
    - Light: UI buttons, tap confirmation
    - Medium: Tower placement, enemy death
    - Heavy: Wave complete, game over
    - Success/Warning/Error: Contextual feedback
  - Platform-specific APIs:
    - Enable multi-touch on iOS
    - iOS device-specific settings
    - Safe touch area validation
  - Helper methods: Screen to world position conversion, swipe direction detection
  - Event system: OnTap, OnDoubleTap, OnSwipe, OnLongPress, OnPinch
  - Settings integration: Enable/disable haptics via PlayerPrefs
  - Context menu testing: Simulate gestures in editor
- **iOSSafeAreaHandler**: Automatic notch and safe area support
  - Automatic UI adjustment for iPhone X and newer
  - Respects notch, home indicator, rounded corners
  - Per-edge configuration: Select which edges respect safe area
  - Canvas auto-detection: Automatically applied to all canvases at runtime
  - Real-time updates: Responds to orientation changes
  - Debug visualization: Show safe area boundaries in editor
  - Static utility methods:
    - GetSafeAreaNormalized(): Get safe area as percentage
    - GetSafeAreaInsets(): Get pixel insets from screen edges
    - HasNotch(): Detect if device has notch
    - GetDeviceModel(): iOS device generation info
  - Auto-apply component: Attach to Canvas for automatic safe area setup
  - Editor simulation: Simulate iPhone X notch for testing
- **Platform-Specific Ad Integration**:
  - Separate iOS and Android placement IDs in AdManager
  - iOS Game ID configuration: Automatic platform detection
  - iOS-optimized placement IDs:
    - `Interstitial_iOS`
    - `Rewarded_iOS`
    - `Banner_iOS`
  - Helper methods: GetInterstitialPlacementId(), GetRewardedPlacementId(), GetBannerPlacementId()
  - Automatic platform selection at runtime
  - Banner position defaults to top on iOS (avoids home indicator)
- **iOS Build Workflow**:
  - Step-by-step Xcode project generation
  - Automatic scene inclusion from build settings
  - Build validation: Check bundle ID, version, orientation, icons
  - Xcode integration: Opens project automatically after build
  - Fast iteration: Incremental builds for rapid testing
  - Device/simulator selection in Xcode toolbar
- **Complete Documentation**: [IOS_GUIDE.md](IOS_GUIDE.md)
  - Prerequisites: macOS, Xcode 14+, Unity iOS module, Apple Developer account
  - iOS build configuration walkthrough (all tabs)
  - App icon requirements and generator tool links
  - Splash screen setup (Launch Storyboard)
  - Xcode project generation and signing
  - Device and simulator testing
  - Safe area implementation guide
  - Touch gesture integration examples
  - Haptic feedback best practices
  - Unity Ads iOS setup (dashboard and placements)
  - In-App Purchases iOS setup (App Store Connect)
  - Game Center integration (leaderboards, achievements)
  - TestFlight deployment (internal and external testing)
  - App Store submission checklist
  - Pre-submission validation
  - Troubleshooting common build/runtime issues
  - Performance optimization for iOS devices
  - App Store rejection reasons and solutions
  - CI/CD command line build scripts
  - Useful resources and links

### Changed
- **AdManager**: Refactored to support platform-specific placement IDs
  - Separated Android and iOS placement ID configuration
  - Added GetInterstitialPlacementId(), GetRewardedPlacementId(), GetBannerPlacementId() helper methods
  - Automatic platform detection via preprocessor directives
  - All ad calls now use platform-specific placements

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
