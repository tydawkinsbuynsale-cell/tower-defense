namespace RobotTD.Analytics
{
    /// <summary>
    /// Centralized definition of all analytics event names.
    /// Use these constants instead of hardcoded strings to prevent typos.
    /// </summary>
    public static class AnalyticsEvents
    {
        // ── Session Events ────────────────────────────────────────────────────
        public const string SESSION_START = "session_start";
        public const string SESSION_END = "session_end";

        // ── Gameplay Events ───────────────────────────────────────────────────
        public const string GAME_START = "game_start";
        public const string GAME_END = "game_end";
        public const string WAVE_COMPLETE = "wave_complete";
        public const string WAVE_STARTED = "wave_started";
        public const string WAVE_FAILED = "wave_failed";

        // ── Tower Events ──────────────────────────────────────────────────────
        public const string TOWER_PLACED = "tower_placed";
        public const string TOWER_UPGRADED = "tower_upgraded";
        public const string TOWER_SOLD = "tower_sold";
        public const string TOWER_TARGETED_ENEMY = "tower_targeted_enemy";

        // ── Enemy Events ──────────────────────────────────────────────────────
        public const string ENEMY_SPAWNED = "enemy_spawned";
        public const string ENEMY_KILLED = "enemy_killed";
        public const string ENEMY_REACHED_END = "enemy_reached_end";
        public const string BOSS_SPAWNED = "boss_spawned";
        public const string BOSS_DEFEATED = "boss_defeated";

        // ── Progression Events ────────────────────────────────────────────────
        public const string ACHIEVEMENT_UNLOCKED = "achievement_unlocked";
        public const string TECH_UPGRADED = "tech_upgraded";
        public const string LEVEL_UP = "level_up";
        public const string MILESTONE_REACHED = "milestone_reached";

        // ── Tutorial Events ───────────────────────────────────────────────────
        public const string TUTORIAL_START = "tutorial_start";
        public const string TUTORIAL_STEP = "tutorial_step";
        public const string TUTORIAL_COMPLETE = "tutorial_complete";
        public const string TUTORIAL_SKIPPED = "tutorial_skipped";

        // ── UI Events ─────────────────────────────────────────────────────────
        public const string MENU_OPENED = "menu_opened";
        public const string BUTTON_CLICKED = "button_clicked";
        public const string SCREEN_VIEW = "screen_view";
        public const string SETTINGS_CHANGED = "settings_changed";

        // ── Monetization Events (for future IAP) ──────────────────────────────
        public const string PURCHASE_INITIATED = "purchase_initiated";
        public const string PURCHASE_COMPLETE = "purchase_complete";
        public const string PURCHASE_FAILED = "purchase_failed";
        public const string AD_WATCHED = "ad_watched";
        public const string AD_SKIPPED = "ad_skipped";

        // ── Performance Events ────────────────────────────────────────────────
        public const string PERFORMANCE_SAMPLE = "performance_sample";
        public const string QUALITY_CHANGED = "quality_changed";
        public const string BATTERY_SAVE_ACTIVATED = "battery_save_activated";
        public const string LOW_MEMORY_WARNING = "low_memory_warning";
        public const string FPS_DROP_DETECTED = "fps_drop_detected";

        // ── Error Events ──────────────────────────────────────────────────────
        public const string ERROR_LOGGED = "error_logged";
        public const string CRASH = "crash";
        public const string EXCEPTION = "exception";

        // ── Social Events ─────────────────────────────────────────────────────
        public const string SCORE_SHARED = "score_shared";
        public const string LEADERBOARD_VIEWED = "leaderboard_viewed";
        public const string ACHIEVEMENT_SHARED = "achievement_shared";

        // ── Endless Mode Events ───────────────────────────────────────────────
        public const string ENDLESS_MODE_START = "endless_mode_start";
        public const string ENDLESS_MODE_END = "endless_mode_end";
        public const string ENDLESS_MILESTONE = "endless_milestone";
        public const string ENDLESS_HIGH_SCORE = "endless_high_score";

        // ── Map Events ────────────────────────────────────────────────────────
        public const string MAP_SELECTED = "map_selected";
        public const string MAP_UNLOCKED = "map_unlocked";
        public const string MAP_FIRST_CLEAR = "map_first_clear";

        // ── Economy Events ────────────────────────────────────────────────────
        public const string CREDITS_EARNED = "credits_earned";
        public const string CREDITS_SPENT = "credits_spent";
        public const string TECH_POINTS_EARNED = "tech_points_earned";
        public const string TECH_POINTS_SPENT = "tech_points_spent";

        // ── Retention Events ──────────────────────────────────────────────────
        public const string DAILY_LOGIN = "daily_login";
        public const string RETURN_AFTER_ABSENCE = "return_after_absence";
        public const string FIRST_SESSION = "first_session";
        
        // ── Challenge Mode Events ─────────────────────────────────────────────
        public const string CHALLENGE_STARTED = "challenge_started";
        public const string CHALLENGE_COMPLETED = "challenge_completed";
        public const string CHALLENGE_FAILED = "challenge_failed";
        public const string DAILY_CHALLENGE_COMPLETE = "daily_challenge_complete";
        public const string WEEKLY_CHALLENGE_COMPLETE = "weekly_challenge_complete";
        
        // ── Daily Mission Events ──────────────────────────────────────────────
        public const string MISSIONS_ROTATED = "missions_rotated";
        public const string MISSION_STARTED = "mission_started";
        public const string MISSION_PROGRESS = "mission_progress";
        public const string MISSION_COMPLETED = "mission_completed";
        public const string MISSION_REWARD_CLAIMED = "mission_reward_claimed";
        public const string ALL_MISSIONS_COMPLETE = "all_missions_complete";
        
        // ── Weekly Mission Events ─────────────────────────────────────────────
        public const string WEEKLY_MISSIONS_ROTATED = "weekly_missions_rotated";
        public const string WEEKLY_MISSION_STARTED = "weekly_mission_started";
        public const string WEEKLY_MISSION_PROGRESS = "weekly_mission_progress";
        public const string WEEKLY_MISSION_COMPLETED = "weekly_mission_completed";
        public const string WEEKLY_MISSION_REWARD_CLAIMED = "weekly_mission_reward_claimed";
        public const string ALL_WEEKLY_MISSIONS_COMPLETE = "all_weekly_missions_complete";
        public const string WEEKLY_BONUS_REWARD_CLAIMED = "weekly_bonus_reward_claimed";
        
        // ── Social Features Events ────────────────────────────────────────────
        public const string FRIEND_REQUEST_SENT = "friend_request_sent";
        public const string FRIEND_REQUEST_RECEIVED = "friend_request_received";
        public const string FRIEND_REQUEST_ACCEPTED = "friend_request_accepted";
        public const string FRIEND_REQUEST_DECLINED = "friend_request_declined";
        public const string FRIEND_REMOVED = "friend_removed";
        public const string PLAYER_SEARCHED = "player_searched";
        public const string SCORE_SHARED = "score_shared";
        public const string ACHIEVEMENT_SHARED_SOCIAL = "achievement_shared_social";
        public const string FRIEND_LEADERBOARD_VIEWED = "friend_leaderboard_viewed";
        public const string FRIENDS_LIST_VIEWED = "friends_list_viewed";
        
        // ── Map Editor Events ─────────────────────────────────────────────────
        public const string MAP_EDITOR_OPENED = "map_editor_opened";
        public const string MAP_EDITOR_NEW_MAP = "map_editor_newmap";
        public const string MAP_EDITOR_LOAD_MAP = "map_editor_load_map";
        public const string MAP_EDITOR_SAVE_MAP = "map_editor_save_map";
        public const string MAP_EDITOR_PLACE_SPAWN = "map_editor_place_spawn";
        public const string MAP_EDITOR_PLACE_BASE = "map_editor_place_base";
        public const string MAP_EDITOR_DRAW_PATH = "map_editor_draw_path";
        public const string MAP_EDITOR_VALIDATE = "map_editor_validate";
        public const string MAP_EDITOR_TEST_PLAY = "map_editor_test_play";
        public const string MAP_EDITOR_PUBLISH = "map_editor_publish";
        public const string CUSTOM_MAP_PLAYED = "custom_map_played";
        public const string CUSTOM_MAP_RATED = "custom_map_rated";
        public const string CUSTOM_MAP_DOWNLOADED = "custom_map_downloaded";
        public const string CUSTOM_MAP_SHARED = "custom_map_shared";
        
        // ── Map Storage Events ────────────────────────────────────────────────
        public const string MAP_STORAGE_DELETED = "map_storage_deleted";
        public const string MAP_STORAGE_EXPORTED = "map_storage_exported";
        public const string MAP_STORAGE_IMPORTED = "map_storage_imported";
        public const string MAP_LIBRARY_OPENED = "map_library_opened";
        public const string MAP_BACKUP_CREATED = "map_backup_created";
        public const string MAP_BACKUP_RESTORED = "map_backup_restored";
    }

    /// <summary>
    /// Parameter keys used across analytics events.
    /// </summary>
    public static class AnalyticsParameters
    {
        // Common
        public const string SESSION_ID = "session_id";
        public const string TIMESTAMP = "timestamp";
        public const string PLATFORM = "platform";
        public const string DEVICE_MODEL = "device_model";

        // Gameplay
        public const string MAP_NAME = "map_name";
        public const string WAVE_NUMBER = "wave_number";
        public const string DIFFICULTY = "difficulty";
        public const string SCORE = "score";
        public const string CREDITS = "credits";
        public const string LIVES = "lives";

        // Towers
        public const string TOWER_TYPE = "tower_type";
        public const string TOWER_LEVEL = "tower_level";
        public const string TOWER_COUNT = "tower_count";
        public const string COST = "cost";

        // Enemies
        public const string ENEMY_TYPE = "enemy_type";
        public const string ENEMY_COUNT = "enemy_count";
        public const string BOSS_TYPE = "boss_type";

        // Progression
        public const string ACHIEVEMENT_ID = "achievement_id";
        public const string TECH_NAME = "tech_name";
        public const string LEVEL = "level";

        // Performance
        public const string FPS = "fps";
        public const string FRAME_TIME = "frame_time";
        public const string MEMORY = "memory";
        public const string QUALITY_LEVEL = "quality_level";

        // Monetization
        public const string PRODUCT_ID = "product_id";
        public const string PRICE = "price";
        public const string CURRENCY = "currency";
        public const string TRANSACTION_ID = "transaction_id";

        // Result
        public const string RESULT = "result";
        public const string REASON = "reason";
        public const string DURATION = "duration";
        
        // Missions
        public const string MISSION_ID = "mission_id";
        public const string MISSION_TYPE = "mission_type";
        public const string MISSION_DIFFICULTY = "mission_difficulty";
        public const string MISSION_PROGRESS = "mission_progress";
        public const string TARGET_VALUE = "target_value";
        public const string REWARD_AMOUNT = "reward_amount";
        
        // Social
        public const string FRIEND_ID = "friend_id";
        public const string FRIEND_NAME = "friend_name";
        public const string FRIEND_COUNT = "friend_count";
        public const string REQUEST_ID = "request_id";
        public const string SEARCH_QUERY = "search_query";
        public const string SHARE_PLATFORM = "share_platform";
        public const string LEADERBOARD_ID = "leaderboard_id";
        public const string LEADERBOARD_SCOPE = "leaderboard_scope";
        
        // Map Editor
        public const string MAP_ID = "map_id";
        public const string MAP_NAME = "map_name";
        public const string MAP_AUTHOR = "map_author";
        public const string GRID_WIDTH = "grid_width";
        public const string GRID_HEIGHT = "grid_height";
        public const string IS_VALID = "is_valid";
        public const string VALIDATION_ERRORS = "validation_errors";
        public const string PLAY_COUNT = "play_count";
        public const string RATING = "rating";
        public const string TILE_COUNT = "tile_count";
        public const string SPAWN_COUNT = "spawn_count";
        public const string PATH_LENGTH = "path_length";
        public const string FILE_SIZE = "file_size";
        public const string TOTAL_MAPS = "total_maps";
        public const string STORAGE_SIZE = "storage_size";
    }

    /// <summary>
    /// Common parameter values for consistency.
    /// </summary>
    public static class AnalyticsValues
    {
        // Game results
        public const string VICTORY = "victory";
        public const string DEFEAT = "defeat";
        public const string QUIT = "quit";

        // Quality presets
        public const string QUALITY_LOW = "low";
        public const string QUALITY_MEDIUM = "medium";
        public const string QUALITY_HIGH = "high";
        public const string QUALITY_CUSTOM = "custom";

        // Change reasons
        public const string REASON_MANUAL = "manual";
        public const string REASON_AUTO = "auto";
        public const string REASON_BATTERY = "battery";
        public const string REASON_PERFORMANCE = "performance";
    }
}
