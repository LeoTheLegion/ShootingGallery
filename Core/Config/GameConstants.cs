using System;
using System.Globalization;
using System.Xml.Linq;
using CoreEssentials.Assets;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Centralized, data-driven configuration for the Shooting Gallery game.
    /// Values are seeded with built-in defaults, then overridden at startup from
    /// Content/game_config.xml so designers can tune balance without recompiling.
    /// A missing file or key leaves the default in place.
    /// </summary>
    public static class GameConstants
    {
        // Backing fields are private and mutable so Load() can override them from
        // game_config.xml (keys match the field names); consumers read the PascalCase
        // properties below.

        // Game timing
        private static double ROUND_TIME = 60.0; // 1 minute game length in seconds
        private static float TIME_MULTIPLIER_START = 10.0f; // Starting score multiplier
        private static float TIME_MULTIPLIER_MIN = 1.0f; // Minimum score multiplier
        private static float TIME_MULTIPLIER_DECAY = 0.3f; // Multiplier decay rate per second

        // Target spawning
        private static double TARGET_SPAWN_DELAY = 0.25; // Base target spawn delay in seconds
        private static double TARGET_SPAWN_RANDOM_FACTOR = 0.2; // Random factor for spawn timing

        // Target growth
        private static float TARGET_DEFAULT_SCALE = 0.3f; // Starting scale for targets
        private static double TARGET_TIME_TO_FULL_SIZE = 0.75; // Time to reach full size in seconds

        // Target type chances
        private static float BOMB_CHANCE = 0.2f; // 20% chance for a bomb target
        private static float RADIOACTIVE_CHANCE = 0.15f; // 15% chance for a radioactive target

        // Target fade
        private static double BOMB_FADE_START_TIME = 5.0; // Time before bomb starts fading (seconds)
        private static double BOMB_FADE_DURATION = 3.0; // Duration of fade effect (seconds)

        // Target size
        private static int TARGET_RADIUS = 45; // Hit radius for targets

        // Target grid
        private static int GRID_ROWS = 5; // Number of rows in target grid
        private static int GRID_COLS = 5; // Number of columns in target grid

        // Scoring
        private static int SCORE_REGULAR_SMALL = 5; // Score for small regular targets
        private static int SCORE_REGULAR_MEDIUM = 10; // Score for medium regular targets
        private static int SCORE_REGULAR_LARGE = 15; // Score for large regular targets
        private static int SCORE_RADIOACTIVE_SMALL = 10; // Score for small radioactive targets
        private static int SCORE_RADIOACTIVE_MEDIUM = 20; // Score for medium radioactive targets
        private static int SCORE_RADIOACTIVE_LARGE = 30; // Score for large radioactive targets

        // Radiation
        private static float RADIATION_REGULAR = 1.0f; // Radiation from regular targets
        private static float RADIATION_RADIOACTIVE = 3.0f; // Radiation from radioactive targets

        // Target growth thresholds
        private static float TARGET_GROWTH_MEDIUM = 0.6f; // Threshold for medium target size
        private static float TARGET_GROWTH_LARGE = 1.0f; // Threshold for large target size

        // Spawn acceleration thresholds (time remaining)
        private static double SPAWN_ACCEL_THRESHOLD_1 = 45.0; // First acceleration threshold (15 seconds elapsed)
        private static double SPAWN_ACCEL_THRESHOLD_2 = 30.0; // Second acceleration threshold (30 seconds elapsed)
        private static double SPAWN_ACCEL_THRESHOLD_3 = 15.0; // Third acceleration threshold (45 seconds elapsed)

        // Spawn acceleration multipliers
        private static double SPAWN_ACCEL_MULTIPLIER_1 = 0.6; // First acceleration multiplier
        private static double SPAWN_ACCEL_MULTIPLIER_2 = 0.5; // Second acceleration multiplier
        private static double SPAWN_ACCEL_MULTIPLIER_3 = 0.4; // Third acceleration multiplier

        // Public read-only view of the tunable values (captured by consumers after Load)
        public static double RoundTime => ROUND_TIME;
        public static float TimeMultiplierStart => TIME_MULTIPLIER_START;
        public static float TimeMultiplierMin => TIME_MULTIPLIER_MIN;
        public static float TimeMultiplierDecay => TIME_MULTIPLIER_DECAY;
        public static double TargetSpawnDelay => TARGET_SPAWN_DELAY;
        public static double TargetSpawnRandomFactor => TARGET_SPAWN_RANDOM_FACTOR;
        public static float TargetDefaultScale => TARGET_DEFAULT_SCALE;
        public static double TargetTimeToFullSize => TARGET_TIME_TO_FULL_SIZE;
        public static float BombChance => BOMB_CHANCE;
        public static float RadioactiveChance => RADIOACTIVE_CHANCE;
        public static double BombFadeStartTime => BOMB_FADE_START_TIME;
        public static double BombFadeDuration => BOMB_FADE_DURATION;
        public static int TargetRadius => TARGET_RADIUS;
        public static int GridRows => GRID_ROWS;
        public static int GridCols => GRID_COLS;
        public static int ScoreRegularSmall => SCORE_REGULAR_SMALL;
        public static int ScoreRegularMedium => SCORE_REGULAR_MEDIUM;
        public static int ScoreRegularLarge => SCORE_REGULAR_LARGE;
        public static int ScoreRadioactiveSmall => SCORE_RADIOACTIVE_SMALL;
        public static int ScoreRadioactiveMedium => SCORE_RADIOACTIVE_MEDIUM;
        public static int ScoreRadioactiveLarge => SCORE_RADIOACTIVE_LARGE;
        public static float RadiationRegular => RADIATION_REGULAR;
        public static float RadiationRadioactive => RADIATION_RADIOACTIVE;
        public static float TargetGrowthMedium => TARGET_GROWTH_MEDIUM;
        public static float TargetGrowthLarge => TARGET_GROWTH_LARGE;
        public static double SpawnAccelThreshold1 => SPAWN_ACCEL_THRESHOLD_1;
        public static double SpawnAccelThreshold2 => SPAWN_ACCEL_THRESHOLD_2;
        public static double SpawnAccelThreshold3 => SPAWN_ACCEL_THRESHOLD_3;
        public static double SpawnAccelMultiplier1 => SPAWN_ACCEL_MULTIPLIER_1;
        public static double SpawnAccelMultiplier2 => SPAWN_ACCEL_MULTIPLIER_2;
        public static double SpawnAccelMultiplier3 => SPAWN_ACCEL_MULTIPLIER_3;

        private static bool _loaded;

        // Load configuration on first access so dependent static readonly fields
        // (in GameManager/Target) capture the tuned values, not the defaults.
        static GameConstants()
        {
            Load();
        }

        /// <summary>
        /// Loads balance values from Content/game_config.xml (via CE's XMLAsset/AssetManager),
        /// overriding the defaults. Safe to call multiple times; a missing file or malformed
        /// entry keeps the default.
        /// </summary>
        public static void Load()
        {
            try
            {
                var xmlAsset = AssetManager.LoadAsset<XMLAsset>("game_config.xml");
                string xml = xmlAsset.XMLContent;
                if (string.IsNullOrWhiteSpace(xml))
                {
                    _loaded = true;
                    return;
                }

                var root = XDocument.Parse(xml).Root;
                if (root == null || root.Name.LocalName != "GameConfig")
                    return;

                int applied = 0;
                foreach (var prop in root.Elements("Property"))
                {
                    string name = prop.Attribute("Name")?.Value;
                    string value = prop.Attribute("Value")?.Value;
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                        continue;

                    // Config keys match the backing field names; assignments are explicit so
                    // a renamed or removed field becomes a compile error instead of a silent skip.
                    if (!TryApply(name, value))
                    {
                        Console.WriteLine($"[GameConstants] Unknown config key '{name}' ignored.");
                        continue;
                    }

                    applied++;
                }

                _loaded = true;
                Console.WriteLine($"[GameConstants] Loaded {applied} balance values from game_config.xml.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameConstants] Failed to load game_config.xml, using defaults. {ex.Message}");
            }
        }

        /// <summary>True once configuration has been loaded (or confirmed absent).</summary>
        public static bool IsLoaded => _loaded;

        private static bool TryApply(string name, string value)
        {
            switch (name)
            {
                // Game timing
                case "ROUND_TIME": ROUND_TIME = ParseDouble(value); return true;
                case "TIME_MULTIPLIER_START": TIME_MULTIPLIER_START = ParseFloat(value); return true;
                case "TIME_MULTIPLIER_MIN": TIME_MULTIPLIER_MIN = ParseFloat(value); return true;
                case "TIME_MULTIPLIER_DECAY": TIME_MULTIPLIER_DECAY = ParseFloat(value); return true;

                // Target spawning
                case "TARGET_SPAWN_DELAY": TARGET_SPAWN_DELAY = ParseDouble(value); return true;
                case "TARGET_SPAWN_RANDOM_FACTOR": TARGET_SPAWN_RANDOM_FACTOR = ParseDouble(value); return true;

                // Target growth
                case "TARGET_DEFAULT_SCALE": TARGET_DEFAULT_SCALE = ParseFloat(value); return true;
                case "TARGET_TIME_TO_FULL_SIZE": TARGET_TIME_TO_FULL_SIZE = ParseDouble(value); return true;

                // Target type chances
                case "BOMB_CHANCE": BOMB_CHANCE = ParseFloat(value); return true;
                case "RADIOACTIVE_CHANCE": RADIOACTIVE_CHANCE = ParseFloat(value); return true;

                // Target fade
                case "BOMB_FADE_START_TIME": BOMB_FADE_START_TIME = ParseDouble(value); return true;
                case "BOMB_FADE_DURATION": BOMB_FADE_DURATION = ParseDouble(value); return true;

                // Target size / grid
                case "TARGET_RADIUS": TARGET_RADIUS = ParseInt(value); return true;
                case "GRID_ROWS": GRID_ROWS = ParseInt(value); return true;
                case "GRID_COLS": GRID_COLS = ParseInt(value); return true;

                // Scoring
                case "SCORE_REGULAR_SMALL": SCORE_REGULAR_SMALL = ParseInt(value); return true;
                case "SCORE_REGULAR_MEDIUM": SCORE_REGULAR_MEDIUM = ParseInt(value); return true;
                case "SCORE_REGULAR_LARGE": SCORE_REGULAR_LARGE = ParseInt(value); return true;
                case "SCORE_RADIOACTIVE_SMALL": SCORE_RADIOACTIVE_SMALL = ParseInt(value); return true;
                case "SCORE_RADIOACTIVE_MEDIUM": SCORE_RADIOACTIVE_MEDIUM = ParseInt(value); return true;
                case "SCORE_RADIOACTIVE_LARGE": SCORE_RADIOACTIVE_LARGE = ParseInt(value); return true;

                // Radiation
                case "RADIATION_REGULAR": RADIATION_REGULAR = ParseFloat(value); return true;
                case "RADIATION_RADIOACTIVE": RADIATION_RADIOACTIVE = ParseFloat(value); return true;

                // Target growth thresholds
                case "TARGET_GROWTH_MEDIUM": TARGET_GROWTH_MEDIUM = ParseFloat(value); return true;
                case "TARGET_GROWTH_LARGE": TARGET_GROWTH_LARGE = ParseFloat(value); return true;

                // Spawn acceleration thresholds (time remaining)
                case "SPAWN_ACCEL_THRESHOLD_1": SPAWN_ACCEL_THRESHOLD_1 = ParseDouble(value); return true;
                case "SPAWN_ACCEL_THRESHOLD_2": SPAWN_ACCEL_THRESHOLD_2 = ParseDouble(value); return true;
                case "SPAWN_ACCEL_THRESHOLD_3": SPAWN_ACCEL_THRESHOLD_3 = ParseDouble(value); return true;

                // Spawn acceleration multipliers
                case "SPAWN_ACCEL_MULTIPLIER_1": SPAWN_ACCEL_MULTIPLIER_1 = ParseDouble(value); return true;
                case "SPAWN_ACCEL_MULTIPLIER_2": SPAWN_ACCEL_MULTIPLIER_2 = ParseDouble(value); return true;
                case "SPAWN_ACCEL_MULTIPLIER_3": SPAWN_ACCEL_MULTIPLIER_3 = ParseDouble(value); return true;

                default: return false;
            }
        }

        private static int ParseInt(string value) => int.Parse(value, CultureInfo.InvariantCulture);
        private static float ParseFloat(string value) => float.Parse(value, CultureInfo.InvariantCulture);
        private static double ParseDouble(string value) => double.Parse(value, CultureInfo.InvariantCulture);
    }
}
