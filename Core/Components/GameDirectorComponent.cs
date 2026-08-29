using System;
using System.Collections.Generic;
using System.Linq;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShootingGallery.Core;

/// <summary>
/// The round "director": timer, score, time multiplier, target grid, spawning, shot
/// resolution, radiation/mutation, and game-over detection — all as one component on a
/// hidden GameObjectEntity declared in the scene XML. Replaces the old GameManager and
/// RadiationManager entities. All references (labels, crosshair) are resolved lazily on the
/// first Update via FindById/tag lookups, so the scene file is pure data.
/// </summary>
public class GameDirectorComponent : EntityComponent
{
    /// <summary>Raised when the round ends (timer expired or a bomb was hit).</summary>
    public event EventHandler<GameOverEventArgs> OnGameOver;

    public class GameOverEventArgs : EventArgs
    {
        public int FinalScore { get; }
        public int MutationLevel { get; }

        public GameOverEventArgs(int finalScore, int mutationLevel)
        {
            FinalScore = finalScore;
            MutationLevel = mutationLevel;
        }
    }

    // Round configuration (static readonly so it captures the XML-loaded values from GameConstants)
    private static readonly double ROUND_TIME = GameConstants.ROUND_TIME;
    private static readonly float TIME_MULTIPLIER_START = GameConstants.TIME_MULTIPLIER_START;
    private static readonly float TIME_MULTIPLIER_MIN = GameConstants.TIME_MULTIPLIER_MIN;
    private static readonly float TIME_MULTIPLIER_DECAY = GameConstants.TIME_MULTIPLIER_DECAY;
    private static readonly double TARGET_SPAWN_DELAY = GameConstants.TARGET_SPAWN_DELAY;
    private static readonly float BOMB_CHANCE = GameConstants.BOMB_CHANCE;
    private static readonly float RADIOACTIVE_CHANCE = GameConstants.RADIOACTIVE_CHANCE;

    // Target grid configuration
    private static readonly int GRID_ROWS = GameConstants.GRID_ROWS;
    private static readonly int GRID_COLS = GameConstants.GRID_COLS;

    // Radiation / mutation configuration
    private const float MAX_RADIATION = 100f;
    private readonly float[] MUTATION_THRESHOLDS = { 0.25f, 0.5f, 0.75f };

    // Unity-style serialized prefab references ([SerializeField] GameObject equivalent):
    // assigned from Content/game_scene.xml <Properties>, like dragging prefabs into the
    // inspector. The defaults keep the component runnable even without XML assignment.
    public string RegularTargetTemplate { get; set; } = "target_regular.xml";
    public string RadioactiveTargetTemplate { get; set; } = "target_radioactive.xml";
    public string BombTargetTemplate { get; set; } = "target_bomb.xml";

    // Grid state
    private readonly bool[,] _occupiedCells;
    private readonly Dictionary<Vector2, (int Row, int Col)> _targetPositionToCell = new();

    // Round state
    private double _timer;
    private int _score;
    private float _timeMultiplier;
    private double _targetSpawnTimer;

    // Radiation / mutation state
    private float _currentRadiation;
    private int _currentMutationLevel;

    // References resolved lazily on first Update
    private bool _bootstrapped;
    private LabelComponent _scoreUI;
    private LabelComponent _timerUI;
    private LabelComponent _multiplierUI;
    private LabelComponent _radiationUI;
    private LabelComponent _mutationUI;
    private CrosshairComponent _crosshair;

    public GameDirectorComponent()
    {
        _occupiedCells = new bool[GRID_ROWS, GRID_COLS];
        _timer = ROUND_TIME;
        _score = 0;
        _timeMultiplier = TIME_MULTIPLIER_START;
        _targetSpawnTimer = 0;
    }

    public override void Update(GameTime gameTime)
    {
        if (Owner == null || Owner.Destroyed)
            return;

        if (!_bootstrapped)
        {
            Bootstrap();
            if (!_bootstrapped)
                return; // references not ready yet (shouldn't happen)
        }

        ProcessGameplay(gameTime);
        UpdateTargetSpawning(gameTime);
    }

    /// <summary>
    /// One-time wiring: register target templates, resolve label/crosshair references by id/tag,
    /// subscribe to shots, and fill the grid. Deferred to the first Update so every scene entity
    /// already exists (scene XML is fully loaded before any component Update runs).
    /// </summary>
    private void Bootstrap()
    {
        var es = EntitySystem;
        if (es == null)
            return;

        // Register the serialized target templates once (assets assigned in scene XML)
        es.RegisterTemplate("Regular", RegularTargetTemplate);
        es.RegisterTemplate("Radioactive", RadioactiveTargetTemplate);
        es.RegisterTemplate("Bomb", BombTargetTemplate);

        // Resolve HUD labels by id
        _scoreUI = es.FindById("score")?.GetComponent<LabelComponent>();
        _timerUI = es.FindById("timer")?.GetComponent<LabelComponent>();
        _multiplierUI = es.FindById("multiplier")?.GetComponent<LabelComponent>();
        _radiationUI = es.FindById("radiation")?.GetComponent<LabelComponent>();
        _mutationUI = es.FindById("mutation")?.GetComponent<LabelComponent>();

        // Resolve the crosshair by tag
        foreach (var entity in es.GetEntitiesByTag("Crosshair"))
        {
            _crosshair = entity.GetComponent<CrosshairComponent>();
            if (_crosshair != null)
            {
                _crosshair.OnShoot += HandleShot;
                break;
            }
        }

        // Fill the entire grid at round start
        PopulateGrid();

        _bootstrapped = true;
    }

    private void ProcessGameplay(GameTime gameTime)
    {
        float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Timer + multiplier decay
        _timer -= elapsedSeconds;
        _timeMultiplier = MathHelper.Max(
            _timeMultiplier - (TIME_MULTIPLIER_DECAY * elapsedSeconds),
            TIME_MULTIPLIER_MIN
        );

        // UI updates
        if (_scoreUI != null)
            _scoreUI.Text = "Score: " + _score.ToString();
        if (_timerUI != null)
            _timerUI.Text = "Time: " + Math.Ceiling(_timer).ToString();
        if (_multiplierUI != null)
            _multiplierUI.Text = $"Multiplier: x{_timeMultiplier:F1}";

        // Radiation UI + mutation check
        if (_radiationUI != null)
            _radiationUI.Text = $"Radiation: {(int)(_currentRadiation / MAX_RADIATION * 100)}%";
        CheckForMutation();

        if (_timer <= 0)
        {
            _timer = 0;
            OnGameOver?.Invoke(this, new GameOverEventArgs(_score, _currentMutationLevel));
        }
    }

    private void UpdateTargetSpawning(GameTime gameTime)
    {
        _targetSpawnTimer -= gameTime.ElapsedGameTime.TotalSeconds;
        if (_targetSpawnTimer <= 0)
        {
            SpawnRandomTarget();

            // Aggressive spawn rate, accelerating as the round progresses
            _targetSpawnTimer = (TARGET_SPAWN_DELAY / 2.0) + (GameRandom.NextFloat() * GameConstants.TARGET_SPAWN_RANDOM_FACTOR);
            if (_timer < GameConstants.SPAWN_ACCEL_THRESHOLD_1) _targetSpawnTimer *= GameConstants.SPAWN_ACCEL_MULTIPLIER_1;
            if (_timer < GameConstants.SPAWN_ACCEL_THRESHOLD_2) _targetSpawnTimer *= GameConstants.SPAWN_ACCEL_MULTIPLIER_2;
            if (_timer < GameConstants.SPAWN_ACCEL_THRESHOLD_3) _targetSpawnTimer *= GameConstants.SPAWN_ACCEL_MULTIPLIER_3;
        }
    }

    // ---- Radiation / mutation -------------------------------------------------

    private void AddRadiation(float amount)
    {
        _currentRadiation = MathHelper.Clamp(_currentRadiation + amount, 0, MAX_RADIATION);
    }

    private void CheckForMutation()
    {
        float radiationPercentage = _currentRadiation / MAX_RADIATION;
        int newMutationLevel = 0;

        for (int i = 0; i < MUTATION_THRESHOLDS.Length; i++)
        {
            if (radiationPercentage >= MUTATION_THRESHOLDS[i])
                newMutationLevel = i + 1;
            else
                break;
        }

        if (newMutationLevel != _currentMutationLevel)
        {
            _currentMutationLevel = newMutationLevel;
            OnMutationChanged();
        }
    }

    private void OnMutationChanged()
    {
        // Grow the crosshair's mutated arms
        _crosshair?.SetMutationLevel(_currentMutationLevel);

        // Update the mutation HUD label
        if (_mutationUI != null)
            _mutationUI.Text = $"Arms: {_currentMutationLevel + 1}";

        // Celebrate with floating popups (purely presentational)
        if (_currentMutationLevel > 0)
        {
            Vector2 worldCenter = World.Center;
            Color messageColor = new Color(0, 255, 0);

            SpawnPopup(new Vector2(worldCenter.X, 150), $"MUTATION LEVEL {_currentMutationLevel}!", 3f, messageColor, 1.5f, true);

            string detailMessage = $"You've grown {_currentMutationLevel} extra arm{(_currentMutationLevel > 1 ? "s" : "")}!";
            SpawnPopup(new Vector2(worldCenter.X, 200), detailMessage, 4f, messageColor, 1.2f, true);
        }
    }

    /// <summary>
    /// Spawns a floating popup: instantiates the "Popup" prefab and applies per-popup values.
    /// Values not passed here fall back to the prefab defaults in Content/popup.xml.
    /// </summary>
    private void SpawnPopup(Vector2 position, string text, float duration, Color color, float scale, bool radiationEffect)
    {
        var popup = InstantiateTemplate("Popup", position)?.GetComponent<FloatingPopUpComponent>();
        if (popup == null)
            return;

        popup.Text = text;
        popup.Duration = duration;
        popup.TextColor = color;
        popup.Scale = scale;
        popup.RadiationEffect = radiationEffect;
    }

    // ---- Shot resolution ------------------------------------------------------

    private void HandleShot(object sender, CrosshairComponent.ShootEventArgs e)
    {
        var es = EntitySystem;
        if (es == null)
            return;

        // Shot feedback popup (random shots pulse like radiation, player shots are white).
        SpawnPopup(e.Position, "×", 0.5f,
            e.IsRandomShot ? Color.LimeGreen : Color.White,
            e.IsRandomShot ? 1.5f : 1.0f,
            e.IsRandomShot);

        // Spatial query: a target is only hittable within TARGET_RADIUS of the shot.
        // A single shot resolves to ONE target — the closest one (so a bomb near the aim
        // point can't end the game when you're shooting a regular target beside it).
        bool targetHit = false;
        float maxHitRadius = GameConstants.TARGET_RADIUS;

        var candidates = es.FindNearby(e.Position, maxHitRadius)
            .Where(entity => entity.GetComponent<TargetComponent>() != null)
            .OrderBy(entity => Vector2.DistanceSquared(entity.Position, e.Position))
            .ToList();

        if (candidates.Count > 0)
        {
            var target = candidates[0].GetComponent<TargetComponent>();
            target.HandleShot(e.Position);
            targetHit = target.IsDestroyed;
        }

        // If a player shot hit something, trigger a random mutated-arm shot
        if (targetHit && !e.IsRandomShot)
            _crosshair?.TriggerRandomShot();
    }

    // ---- Spawning / grid ------------------------------------------------------

    private Entity CreateTargetAt(Vector2 position)
    {
        var es = EntitySystem;
        float roll = GameRandom.NextFloat();
        string templateName;
        if (roll < BOMB_CHANCE)
            templateName = "Bomb";
        else if (roll < BOMB_CHANCE + RADIOACTIVE_CHANCE)
            templateName = "Radioactive";
        else
            templateName = "Regular";

        return es.Instantiate(templateName, position);
    }

    private void WireTarget(Entity targetEntity, Vector2 position, int row, int col)
    {
        _targetPositionToCell[position] = (row, col);

        var target = targetEntity.GetComponent<TargetComponent>();

        target.OnScore += (sender, args) => _score += (int)(args.Score * _timeMultiplier);

        target.OnRadiationChange += (sender, args) => AddRadiation(args.RadiationAmount);

        target.OnGameOver += (sender, args) =>
            OnGameOver?.Invoke(this, new GameOverEventArgs(_score, _currentMutationLevel));

        // Free the grid cell when the target dies (no per-frame scan needed)
        target.OnDestroyed += (sender, deadPosition) =>
        {
            if (_targetPositionToCell.TryGetValue(deadPosition, out var cell))
            {
                _occupiedCells[cell.Row, cell.Col] = false;
                _targetPositionToCell.Remove(deadPosition);
            }
        };
    }

    private (Vector2 Position, int Row, int Col)? GetAvailableGridCell()
    {
        if (!HasAvailableCell())
            return null;

        float cellWidth = World.Width / (float)GRID_COLS;
        float cellHeight = World.Height / (float)GRID_ROWS;

        for (int attempts = 0; attempts < 100; attempts++)
        {
            int row = GameRandom.Next(GRID_ROWS);
            int col = GameRandom.Next(GRID_COLS);
            if (!_occupiedCells[row, col])
                return OccupyCell(row, col, cellWidth, cellHeight);
        }

        for (int row = 0; row < GRID_ROWS; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                if (!_occupiedCells[row, col])
                    return OccupyCell(row, col, cellWidth, cellHeight);
            }
        }

        return null;
    }

    private bool HasAvailableCell()
    {
        for (int row = 0; row < GRID_ROWS; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                if (!_occupiedCells[row, col])
                    return true;
            }
        }
        return false;
    }

    private (Vector2 Position, int Row, int Col) OccupyCell(int row, int col, float cellWidth, float cellHeight)
    {
        _occupiedCells[row, col] = true;
        Vector2 position = new Vector2(
            col * cellWidth + (cellWidth / 2),
            row * cellHeight + (cellHeight / 2)
        );
        return (position, row, col);
    }

    private void SpawnRandomTarget()
    {
        var cellInfo = GetAvailableGridCell();
        if (cellInfo == null)
            return;

        var (position, row, col) = cellInfo.Value;
        Entity newTarget = CreateTargetAt(position);
        WireTarget(newTarget, position, row, col);
    }

    private void PopulateGrid()
    {
        float cellWidth = World.Width / (float)GRID_COLS;
        float cellHeight = World.Height / (float)GRID_ROWS;

        for (int row = 0; row < GRID_ROWS; row++)
        {
            for (int col = 0; col < GRID_COLS; col++)
            {
                Vector2 position = new Vector2(
                    col * cellWidth + (cellWidth / 2),
                    row * cellHeight + (cellHeight / 2)
                );

                _occupiedCells[row, col] = true;
                Entity newTarget = CreateTargetAt(position);
                WireTarget(newTarget, position, row, col);
            }
        }
    }
}
