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
