using Microsoft.Xna.Framework;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Centralized constants for the Shooting Gallery game.
    /// This class organizes all game constants in one place for easier maintenance.
    /// </summary>
    public static class GameConstants
    {
        // Game timing constants
        public const double ROUND_TIME = 60.0; // 1 minute game length in seconds
        public const float TIME_MULTIPLIER_START = 10.0f; // Starting score multiplier
        public const float TIME_MULTIPLIER_MIN = 1.0f; // Minimum score multiplier
        public const float TIME_MULTIPLIER_DECAY = 0.3f; // Multiplier decay rate per second
        
        // Target spawn constants
        public const double TARGET_SPAWN_DELAY = 0.25; // Base target spawn delay in seconds
        public const double TARGET_SPAWN_RANDOM_FACTOR = 0.2; // Random factor for spawn timing
        
        // Target growth constants
        public const float TARGET_DEFAULT_SCALE = 0.3f; // Starting scale for targets
        public const double TARGET_TIME_TO_FULL_SIZE = 0.75; // Time to reach full size in seconds
        
        // Target chance constants
        public const float BOMB_CHANCE = 0.2f; // 20% chance for a bomb target
        public const float RADIOACTIVE_CHANCE = 0.15f; // 15% chance for a radioactive target
        
        // Target fade constants
        public const double BOMB_FADE_START_TIME = 5.0; // Time before bomb starts fading (seconds)
        public const double BOMB_FADE_DURATION = 3.0; // Duration of fade effect (seconds)
        
        // Target size constants
        public const int TARGET_RADIUS = 45; // Hit radius for targets
        
        // Target grid configuration
        public const int GRID_ROWS = 5; // Number of rows in target grid
        public const int GRID_COLS = 5; // Number of columns in target grid
        
        // Scoring constants
        public const int SCORE_REGULAR_SMALL = 5; // Score for small regular targets
        public const int SCORE_REGULAR_MEDIUM = 10; // Score for medium regular targets
        public const int SCORE_REGULAR_LARGE = 15; // Score for large regular targets
        
        public const int SCORE_RADIOACTIVE_SMALL = 10; // Score for small radioactive targets
        public const int SCORE_RADIOACTIVE_MEDIUM = 20; // Score for medium radioactive targets
        public const int SCORE_RADIOACTIVE_LARGE = 30; // Score for large radioactive targets
        
        // Radiation constants
        public const float RADIATION_REGULAR = 1.0f; // Radiation from regular targets
        public const float RADIATION_RADIOACTIVE = 3.0f; // Radiation from radioactive targets
        
        // Target growth thresholds
        public const float TARGET_GROWTH_MEDIUM = 0.6f; // Threshold for medium target size
        public const float TARGET_GROWTH_LARGE = 1.0f; // Threshold for large target size
        
        // Spawn acceleration thresholds (time remaining)
        public const double SPAWN_ACCEL_THRESHOLD_1 = 45.0; // First acceleration threshold (15 seconds elapsed)
        public const double SPAWN_ACCEL_THRESHOLD_2 = 30.0; // Second acceleration threshold (30 seconds elapsed)
        public const double SPAWN_ACCEL_THRESHOLD_3 = 15.0; // Third acceleration threshold (45 seconds elapsed)
        
        // Spawn acceleration multipliers
        public const double SPAWN_ACCEL_MULTIPLIER_1 = 0.6; // First acceleration multiplier
        public const double SPAWN_ACCEL_MULTIPLIER_2 = 0.5; // Second acceleration multiplier
        public const double SPAWN_ACCEL_MULTIPLIER_3 = 0.4; // Third acceleration multiplier
    }
}
