using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

namespace ShootingGallery.Core;

/// <summary>
/// The round's balance configuration, held as a plain data component on a tagged entity in the
/// scene (see Content/Scenes/game_scene.xml, tag "GameConfig"). Every value is settable and declared
/// explicitly in the scene XML &lt;Properties&gt;, so there is no hidden static load or fallback —
/// designers tune balance by editing one block of declarative data. The director resolves it by
/// tag on bootstrap; spawned targets resolve it by tag on attach.
///
/// All numeric values are float/int (not double) because CE's scene &lt;Property&gt; parser
/// (SerializationUtils.ParseValue) only supports int, float, bool, string, Vector2, Color and enums.
/// </summary>
public class GameConfigComponent : EntityComponent
{
    // ---- Game timing ----------------------------------------------------------
    public float RoundTime { get; set; } = 60f;                 // game length in seconds
    public float TimeMultiplierStart { get; set; } = 10f;       // starting score multiplier
    public float TimeMultiplierMin { get; set; } = 1f;          // minimum score multiplier
    public float TimeMultiplierDecay { get; set; } = 0.3f;      // multiplier decay per second

    // ---- Target spawning ------------------------------------------------------
    public float TargetSpawnDelay { get; set; } = 0.25f;        // base spawn delay (seconds)
    public float TargetSpawnRandomFactor { get; set; } = 0.2f;  // random factor for spawn timing

    // ---- Target growth --------------------------------------------------------
    public float TargetDefaultScale { get; set; } = 0.3f;       // starting scale for targets
    public float TimeToFullSize { get; set; } = 0.75f;          // time to reach full size (seconds)

    // ---- Target type chances --------------------------------------------------
    public float BombChance { get; set; } = 0.2f;               // chance a spawn is a bomb
    public float RadioactiveChance { get; set; } = 0.15f;       // chance a spawn is radioactive

    // ---- Bomb fade ------------------------------------------------------------
    public float BombFadeStartTime { get; set; } = 5f;          // time before the bomb starts fading
    public float BombFadeDuration { get; set; } = 3f;           // duration of the fade effect

    // ---- Target size / grid ---------------------------------------------------
    public int TargetRadius { get; set; } = 45;                 // hit radius for targets
    public int GridRows { get; set; } = 5;                      // rows in the target grid
    public int GridCols { get; set; } = 5;                      // columns in the target grid

    // ---- Scoring --------------------------------------------------------------
    public int ScoreRegularSmall { get; set; } = 5;
    public int ScoreRegularMedium { get; set; } = 10;
    public int ScoreRegularLarge { get; set; } = 15;
    public int ScoreRadioactiveSmall { get; set; } = 10;
    public int ScoreRadioactiveMedium { get; set; } = 20;
    public int ScoreRadioactiveLarge { get; set; } = 30;

    // ---- Radiation ------------------------------------------------------------
    public float RadiationRegular { get; set; } = 1f;           // radiation from regular targets
    public float RadiationRadioactive { get; set; } = 3f;       // radiation from radioactive targets

    // ---- Target growth thresholds --------------------------------------------
    public float TargetGrowthMedium { get; set; } = 0.6f;       // scale threshold for medium scoring
    public float TargetGrowthLarge { get; set; } = 1.0f;        // scale threshold for large scoring

    // ---- Spawn acceleration (time remaining) ---------------------------------
    public float SpawnAccelThreshold1 { get; set; } = 45f;      // first acceleration threshold
    public float SpawnAccelThreshold2 { get; set; } = 30f;      // second acceleration threshold
    public float SpawnAccelThreshold3 { get; set; } = 15f;      // third acceleration threshold

    public float SpawnAccelMultiplier1 { get; set; } = 0.6f;    // first acceleration multiplier
    public float SpawnAccelMultiplier2 { get; set; } = 0.5f;    // second acceleration multiplier
    public float SpawnAccelMultiplier3 { get; set; } = 0.4f;    // third acceleration multiplier
}
