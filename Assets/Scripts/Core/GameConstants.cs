namespace RobotTD.Core
{
    /// <summary>
    /// Central location for all game constants and balance values.
    /// Makes balancing easier - change values here instead of hunting through code.
    /// </summary>
    public static class GameConstants
    {
        // ═══════════════════════════════════════════════════════════════════
        // ECONOMY
        // ═══════════════════════════════════════════════════════════════════

        public const int STARTING_CREDITS_DEFAULT = 500;
        public const int STARTING_LIVES_DEFAULT = 20;
        public const int WAVE_COMPLETION_BONUS_BASE = 100;
        public const float INTEREST_RATE = 0.05f;               // 5% interest on banked credits
        public const float TOWER_SELL_VALUE_PERCENT = 0.5f;     // Get back 50% of total value

        // ═══════════════════════════════════════════════════════════════════
        // DIFFICULTY SCALING
        // ═══════════════════════════════════════════════════════════════════

        public const float HEALTH_SCALE_PER_WAVE = 0.15f;       // 15% more HP per wave
        public const float SPEED_SCALE_PER_WAVE = 0.02f;        // 2% faster per wave
        public const float DAMAGE_SCALE_PER_WAVE = 0.1f;        // 10% more damage per wave (if enemies attack)

        // Boss waves
        public const int BOSS_WAVE_INTERVAL = 5;                // Boss every 5 waves
        public const float BOSS_HEALTH_MULTIPLIER = 10f;        // Bosses have 10x health
        public const float BOSS_REWARD_MULTIPLIER = 5f;         // Bosses give 5x rewards

        // ═══════════════════════════════════════════════════════════════════
        // TOWER BALANCE
        // ═══════════════════════════════════════════════════════════════════

        // Upgrade tiers
        public const float UPGRADE_TIER1_BONUS = 0.20f;         // +20% stats
        public const float UPGRADE_TIER2_BONUS = 0.40f;         // +40% stats
        public const float UPGRADE_TIER3_BONUS = 0.60f;         // +60% stats

        public const float UPGRADE_TIER1_COST_MULT = 0.50f;     // 50% of base cost
        public const float UPGRADE_TIER2_COST_MULT = 0.75f;     // 75% of base cost
        public const float UPGRADE_TIER3_COST_MULT = 1.00f;     // 100% of base cost

        // Damage type effectiveness
        public const float DAMAGE_STRONG_MULTIPLIER = 1.5f;     // 50% more damage
        public const float DAMAGE_WEAK_MULTIPLIER = 0.5f;       // 50% less damage
        public const float DAMAGE_RESIST_MULTIPLIER = 0.75f;    // 25% less damage

        // ═══════════════════════════════════════════════════════════════════
        // STATUS EFFECTS
        // ═══════════════════════════════════════════════════════════════════

        public const float SLOW_EFFECT_WEAK = 0.3f;             // 30% slow
        public const float SLOW_EFFECT_MEDIUM = 0.5f;           // 50% slow
        public const float SLOW_EFFECT_STRONG = 0.7f;           // 70% slow
        public const float SLOW_DURATION_DEFAULT = 2f;          // Seconds

        public const float STUN_DURATION_DEFAULT = 0.5f;        // Seconds
        public const float BURN_TICK_RATE = 0.25f;              // 4 ticks per second
        public const int BURN_MAX_STACKS = 3;                   // Maximum burn stacks

        // ═══════════════════════════════════════════════════════════════════
        // ENEMY BEHAVIOR
        // ═══════════════════════════════════════════════════════════════════

        public const float ENEMY_WAYPOINT_REACH_DISTANCE = 0.1f;    // How close to waypoint
        public const float ENEMY_HEALTH_BAR_HIDE_DURATION = 1f;     // Fade out after damaged
        public const float BOSS_ENRAGE_HEALTH_PERCENT = 0.25f;      // Enrage at 25% HP
        public const float BOSS_ENRAGE_SPEED_BOOST = 0.5f;          // 50% speed increase

        // Flying enemies
        public const float FLYING_HEIGHT = 3f;                      // Height above ground
        public const float FLYING_BOB_AMPLITUDE = 0.3f;             // Up/down motion
        public const float FLYING_BOB_FREQUENCY = 2f;               // Oscillation speed

        // ═══════════════════════════════════════════════════════════════════
        // PROGRESSION
        // ═══════════════════════════════════════════════════════════════════

        public const int XP_PER_ENEMY_KILL = 5;
        public const int XP_PER_WAVE_COMPLETE = 50;
        public const int XP_PER_MAP_COMPLETE = 200;
        public const int XP_FOR_LEVEL_BASE = 100;                   // XP needed for level 2
        public const float XP_LEVEL_SCALING = 1.5f;                 // Each level needs 50% more XP

        public const int TECH_POINTS_PER_LEVEL = 1;
        public const int TECH_POINTS_PER_MAP = 3;
        public const int TECH_UPGRADE_COST = 3;                     // Tech points per upgrade tier

        // ═══════════════════════════════════════════════════════════════════
        // TECH TREE BONUSES
        // ═══════════════════════════════════════════════════════════════════

        public const float FIREPOWER_BONUS_PER_LEVEL = 0.05f;       // 5% damage per level
        public const float EFFICIENCY_BONUS_PER_LEVEL = 0.05f;      // 5% cost reduction per level
        public const int RESILIENCE_BONUS_PER_LEVEL = 2;            // +2 lives per level
        public const float TACTICS_BONUS_PER_LEVEL = 0.1f;          // 10% more credits per level
        public const float RAPID_DEPLOY_BONUS_PER_LEVEL = 0.1f;     // 10% faster building per level
        public const float RECYCLING_BONUS_PER_LEVEL = 0.1f;        // 10% better sell value per level

        // ═══════════════════════════════════════════════════════════════════
        // PERFORMANCE & OPTIMIZATION
        // ═══════════════════════════════════════════════════════════════════

        public const int MAX_ENEMIES_PER_WAVE = 50;
        public const int MAX_ACTIVE_ENEMIES = 100;
        public const int MAX_ACTIVE_PROJECTILES = 150;
        public const int POOL_SIZE_PROJECTILES = 50;
        public const int POOL_SIZE_ENEMIES = 30;
        public const int POOL_SIZE_VFX = 20;

        public const float OBJECT_CULL_DISTANCE = 50f;              // Cull objects beyond this distance
        public const float UPDATE_RATE_SLOW = 0.2f;                 // Update every 0.2s for non-critical
        public const int MAX_PATH_UPDATES_PER_FRAME = 5;            // Spread pathfinding over frames

        // ═══════════════════════════════════════════════════════════════════
        // UI & VISUAL
        // ═══════════════════════════════════════════════════════════════════

        public const float UI_ANIMATION_SPEED = 0.3f;
        public const float DAMAGE_NUMBER_LIFETIME = 1f;
        public const float DAMAGE_NUMBER_FLOAT_SPEED = 2f;
        public const float HEALTH_BAR_UPDATE_SPEED = 0.1f;

        public const float CAMERA_SHAKE_INTENSITY = 0.2f;
        public const float CAMERA_SHAKE_DURATION = 0.15f;

        // ═══════════════════════════════════════════════════════════════════
        // AUDIO
        // ═══════════════════════════════════════════════════════════════════

        public const float AUDIO_MASTER_VOLUME_DEFAULT = 1f;
        public const float AUDIO_SFX_VOLUME_DEFAULT = 0.8f;
        public const float AUDIO_MUSIC_VOLUME_DEFAULT = 0.6f;
        public const float AUDIO_FADE_DURATION = 1.5f;
        public const int AUDIO_SFX_POOL_SIZE = 12;

        // ═══════════════════════════════════════════════════════════════════
        // GAME MODES
        // ═══════════════════════════════════════════════════════════════════

        public const int ENDLESS_MODE_START_WAVE = 1;
        public const float ENDLESS_MODE_SCALING_MULTIPLIER = 1.2f;  // Faster scaling in endless
        public const int ENDLESS_MODE_CHECKPOINT_INTERVAL = 10;     // Save every 10 waves

        // ═══════════════════════════════════════════════════════════════════
        // LAYERS & TAGS (Unity layer/tag IDs)
        // ═══════════════════════════════════════════════════════════════════

        public const string LAYER_ENEMY = "Enemy";
        public const string LAYER_TOWER = "Tower";
        public const string LAYER_GROUND = "Ground";
        public const string LAYER_PATH = "Path";
        
        public const string TAG_WAYPOINT = "Waypoint";
        public const string TAG_SPAWN_POINT = "SpawnPoint";
        public const string TAG_END_POINT = "EndPoint";

        // ═══════════════════════════════════════════════════════════════════
        // MOBILE SETTINGS
        // ═══════════════════════════════════════════════════════════════════

        public const int TARGET_FRAME_RATE = 60;
        public const int MIN_FRAME_RATE = 30;
        public const float MOBILE_INPUT_DEADZONE = 0.1f;
        public const float PINCH_ZOOM_SENSITIVITY = 0.01f;
        public const float DRAG_PAN_SENSITIVITY = 0.5f;

        // ═══════════════════════════════════════════════════════════════════
        // DEBUG
        // ═══════════════════════════════════════════════════════════════════

        public const bool DEBUG_SHOW_PATH = true;
        public const bool DEBUG_SHOW_RANGE_INDICATORS = true;
        public const bool DEBUG_SHOW_GRID = false;
        public const bool DEBUG_LOG_WAVE_GENERATION = false;
    }
}
