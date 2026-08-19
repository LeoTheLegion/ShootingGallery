using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

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
        // Game timing
        public static double ROUND_TIME = 60.0; // 1 minute game length in seconds
        public static float TIME_MULTIPLIER_START = 10.0f; // Starting score multiplier
        public static float TIME_MULTIPLIER_MIN = 1.0f; // Minimum score multiplier
        public static float TIME_MULTIPLIER_DECAY = 0.3f; // Multiplier decay rate per second

        // Target spawning
        public static double TARGET_SPAWN_DELAY = 0.25; // Base target spawn delay in seconds
        public static double TARGET_SPAWN_RANDOM_FACTOR = 0.2; // Random factor for spawn timing

        // Target growth
        public static float TARGET_DEFAULT_SCALE = 0.3f; // Starting scale for targets
        public static double TARGET_TIME_TO_FULL_SIZE = 0.75; // Time to reach full size in seconds

        // Target type chances
        public static float BOMB_CHANCE = 0.2f; // 20% chance for a bomb target
        public static float RADIOACTIVE_CHANCE = 0.15f; // 15% chance for a radioactive target

        // Target fade
        public static double BOMB_FADE_START_TIME = 5.0; // Time before bomb starts fading (seconds)
        public static double BOMB_FADE_DURATION = 3.0; // Duration of fade effect (seconds)

        // Target size
        public static int TARGET_RADIUS = 45; // Hit radius for targets

        // Target grid
        public static int GRID_ROWS = 5; // Number of rows in target grid
        public static int GRID_COLS = 5; // Number of columns in target grid

        // Scoring
        public static int SCORE_REGULAR_SMALL = 5; // Score for small regular targets
        public static int SCORE_REGULAR_MEDIUM = 10; // Score for medium regular targets
        public static int SCORE_REGULAR_LARGE = 15; // Score for large regular targets
        public static int SCORE_RADIOACTIVE_SMALL = 10; // Score for small radioactive targets
        public static int SCORE_RADIOACTIVE_MEDIUM = 20; // Score for medium radioactive targets
        public static int SCORE_RADIOACTIVE_LARGE = 30; // Score for large radioactive targets

        // Radiation
        public static float RADIATION_REGULAR = 1.0f; // Radiation from regular targets
        public static float RADIATION_RADIOACTIVE = 3.0f; // Radiation from radioactive targets

        // Target growth thresholds
        public static float TARGET_GROWTH_MEDIUM = 0.6f; // Threshold for medium target size
        public static float TARGET_GROWTH_LARGE = 1.0f; // Threshold for large target size

        // Spawn acceleration thresholds (time remaining)
        public static double SPAWN_ACCEL_THRESHOLD_1 = 45.0; // First acceleration threshold (15 seconds elapsed)
        public static double SPAWN_ACCEL_THRESHOLD_2 = 30.0; // Second acceleration threshold (30 seconds elapsed)
        public static double SPAWN_ACCEL_THRESHOLD_3 = 15.0; // Third acceleration threshold (45 seconds elapsed)

        // Spawn acceleration multipliers
        public static double SPAWN_ACCEL_MULTIPLIER_1 = 0.6; // First acceleration multiplier
        public static double SPAWN_ACCEL_MULTIPLIER_2 = 0.5; // Second acceleration multiplier
        public static double SPAWN_ACCEL_MULTIPLIER_3 = 0.4; // Third acceleration multiplier

        private static bool _loaded;

        // Load configuration on first access so dependent static readonly fields
        // (in GameManager/Target) capture the tuned values, not the defaults.
        static GameConstants()
        {
            Load();
        }

        /// <summary>
        /// Loads balance values from Content/game_config.xml, overriding the defaults.
        /// Safe to call multiple times; a missing file or malformed entry keeps the default.
        /// </summary>
        public static void Load()
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Content", "game_config.xml");
                if (!File.Exists(path))
                {
                    _loaded = true;
                    return;
                }

                var root = XDocument.Load(path).Root;
                if (root == null || root.Name.LocalName != "GameConfig")
                    return;

                int applied = 0;
                foreach (var prop in root.Elements("Property"))
                {
                    string name = prop.Attribute("Name")?.Value;
                    string value = prop.Attribute("Value")?.Value;
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                        continue;

                    var field = typeof(GameConstants).GetField(name, BindingFlags.Public | BindingFlags.Static);
                    if (field == null)
                    {
                        Console.WriteLine($"[GameConstants] Unknown config key '{name}' ignored.");
                        continue;
                    }

                    field.SetValue(null, ParseValue(field.FieldType, value));
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

        private static object ParseValue(Type targetType, string value)
        {
            if (targetType == typeof(int)) return int.Parse(value, CultureInfo.InvariantCulture);
            if (targetType == typeof(float)) return float.Parse(value, CultureInfo.InvariantCulture);
            if (targetType == typeof(double)) return double.Parse(value, CultureInfo.InvariantCulture);
            throw new NotSupportedException($"Unsupported config value type: {targetType.Name}.");
        }
    }
}
