using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Scenes;
using CoreEssentials.Utils;
using Microsoft.Xna.Framework;
using ShootingGallery.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShootingGallery
{
    public class GameManager : Entity
    {
        // Event for game over
        public event EventHandler<GameOverEventArgs> OnGameOver;        // Event args for game over
        public class GameOverEventArgs : EventArgs
        {
            public int FinalScore { get; }

            public GameOverEventArgs(int finalScore)
            {
                FinalScore = finalScore;
            }
        }
        // Game configuration (static readonly so it captures the XML-loaded values
        // from GameConstants, which must be runtime fields rather than compile-time consts)
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
        private readonly bool[,] _occupiedCells; // true = occupied, false = free
        private readonly Dictionary<Vector2, (int Row, int Col)> _targetPositionToCell; // Maps positions to grid cells

        // Game state
        private double _timer;
        private int _score = 0;
        private float _timeMultiplier;
        private double _targetSpawnTimer;
        public bool isGameOver => _timer <= 0;        // UI elements
        private ShootingGallery.Core.TextEntity _scoreUI;
        private ShootingGallery.Core.TextEntity _timerUI;
        private ShootingGallery.Core.TextEntity _multiplierUI;

        // Linked entities
        private RadiationManager _radiationManager;
        private Crosshair _crosshair;

        // Getters
        public double GetGameTime() => _timer;
        public int GetScore() => _score;
        public float GetTimeMultiplier() => _timeMultiplier;

        // Setters
        public void AddScore(int points) => _score += (int)(points * _timeMultiplier);
        public void SetScoreUI(ShootingGallery.Core.TextEntity scoreUI) => _scoreUI = scoreUI;
        public void SetTimerUI(ShootingGallery.Core.TextEntity timerUI) => _timerUI = timerUI;
        public void SetMultiplierUI(ShootingGallery.Core.TextEntity multiplierUI) => _multiplierUI = multiplierUI;
        public void SetRadiationManager(RadiationManager radiationManager) => _radiationManager = radiationManager;
        public void SetCrosshair(Crosshair crosshair)
        {
            _crosshair = crosshair;

            if (_crosshair != null)
            {
                Console.WriteLine("Crosshair is not null, subscribing to shoot event.");
                _crosshair.OnShoot += HandleCrosshairShoot;
            }
        }

        public GameManager()
        {
            _timer = ROUND_TIME;
            _score = 0;
            _timeMultiplier = TIME_MULTIPLIER_START;
            _targetSpawnTimer = 0;

            // Initialize grid
            _occupiedCells = new bool[GRID_ROWS, GRID_COLS];
            _targetPositionToCell = new Dictionary<Vector2, (int, int)>();
        }
        public override void OnStart()
        {
            base.OnStart();

            // Register target templates once (Ball.cs pattern — definitions live in Content/*.xml)
            EntitySystem.RegisterTemplate("Regular", "target_regular.xml");
            EntitySystem.RegisterTemplate("Radioactive", "target_radioactive.xml");
            EntitySystem.RegisterTemplate("Bomb", "target_bomb.xml");

            // Populate the entire grid with targets at startup
            PopulateGrid();
        }
        // v0.14.0: spawn a target from its registered entity template (Ball.cs pattern).
        // The template XML defines the type, tags, and SpriteComponent Color/Origin; the
        // sprite asset itself is assigned in the target's OnStart.
        private BaseTarget CreateTargetAt(Vector2 position)
        {
            float roll = GameRandom.NextFloat();
            string templateName;
            if (roll < BOMB_CHANCE)
                templateName = "Bomb";
            else if (roll < BOMB_CHANCE + RADIOACTIVE_CHANCE)
                templateName = "Radioactive";
            else
                templateName = "Regular";

            return (BaseTarget)EntitySystem.Instantiate(templateName, position);
        }

        private void WireTargetEvents(BaseTarget target, Vector2 position, int row, int col)
        {
            _targetPositionToCell[position] = (row, col);

            target.OnScore += (sender, args) => AddScore(args.Score);

            target.OnRadiationChange += (sender, args) =>
            {
                if (_radiationManager != null)
                    _radiationManager.AddRadiation(args.RadiationAmount);
            };

            target.OnGameOver += (sender, args) => OnGameOver?.Invoke(this, new GameOverEventArgs(_score));

            // v0.14.0: free the grid cell when the target dies instead of scanning for dead targets every frame
            target.OnDestroyed += (sender, deadPosition) =>
            {
                if (_targetPositionToCell.TryGetValue(deadPosition, out var cell))
                {
                    _occupiedCells[cell.Row, cell.Col] = false;
                    _targetPositionToCell.Remove(deadPosition);
                }
            };
        }

        private void HandleCrosshairShoot(object sender, Crosshair.ShootEventArgs e)
        {
            // Log the shot for debugging
            Console.WriteLine($"Shot detected at position: {e.Position}");

            // v0.14.0: spatial query — a target is only hittable if its center is within
            // targetRadius * _scale (<= TARGET_RADIUS) of the shot, so this radius is the exact
            // candidate set. FindNearby uses the spatial grid (O(1) avg) and excludes destroyed targets.
            bool targetHit = false;
            float maxHitRadius = GameConstants.TARGET_RADIUS;
            foreach (var target in EntitySystem.FindNearby<BaseTarget>(e.Position, maxHitRadius))
            {
                Console.WriteLine($"Checking target at position: {target.Position}, distance: {Vector2.Distance(target.Position, e.Position)}");

                target.HandleShot(e.Position);

                if (target.IsDestroyed)
                {
                    targetHit = true;
                }
            }

            // If any target was hit and this was a player shot (not a random shot),
            // trigger a random shot from a mutated arm
            if (targetHit && !e.IsRandomShot && _crosshair != null)
            {
                _crosshair.TriggerRandomShot();
            }
        }

        public override void Update(GameTime gameTime)
        {
            ProcessGameplay(gameTime);
            UpdateTargetSpawning(gameTime);
        }
        private void ProcessGameplay(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update timer
            _timer -= elapsedSeconds;

            // Update time multiplier (decreases over time)
            _timeMultiplier = MathHelper.Max(
                _timeMultiplier - (TIME_MULTIPLIER_DECAY * elapsedSeconds),
                TIME_MULTIPLIER_MIN
            );

            // Update UI
            _scoreUI.SetText("Score: " + _score.ToString());
            _timerUI.SetText("Time: " + Math.Ceiling(_timer).ToString());
            _multiplierUI.SetText($"Multiplier: x{_timeMultiplier:F1}");

            if (_timer <= 0)
            {
                _timer = 0;
                // Game Over logic
                OnGameOver?.Invoke(this, new GameOverEventArgs(_score));
            }
        }

        private (Vector2 Position, int Row, int Col)? GetAvailableGridCell()
        {
            // If no cells available, return null
            if (!HasAvailableCell())
            {
                Console.WriteLine("No grid cells available");
                return null;
            }

            // Calculate grid cell size based on screen dimensions
            float cellWidth = ScreenManager.ScreenWidth / (float)GRID_COLS;
            float cellHeight = ScreenManager.ScreenHeight / (float)GRID_ROWS;

            Console.WriteLine($"Grid cell size: {cellWidth}x{cellHeight}");

            // Find a random unoccupied cell
            for (int attempts = 0; attempts < 100; attempts++)
            {
                int row = GameRandom.Next(GRID_ROWS);
                int col = GameRandom.Next(GRID_COLS);

                if (!_occupiedCells[row, col])
                    return OccupyCell(row, col, cellWidth, cellHeight);
            }

            // If we tried 100 times and couldn't find a spot, do a direct search
            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    if (!_occupiedCells[row, col])
                        return OccupyCell(row, col, cellWidth, cellHeight);
                }
            }

            // Should never get here if HasAvailableCell was true
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
            // Mark cell as occupied
            _occupiedCells[row, col] = true;

            // Calculate position (center of cell with offset for target center)
            Vector2 position = new Vector2(
                col * cellWidth + (cellWidth / 2),
                row * cellHeight + (cellHeight / 2)
            );

            Console.WriteLine($"Target positioned at grid ({row},{col}) -> screen position {position}");
            return (position, row, col);
        }
        private void SpawnRandomTarget()
        {
            var cellInfo = GetAvailableGridCell();

            // If no available cell, don't spawn
            if (cellInfo == null)
            {
                Console.WriteLine("No available grid cells for new target!");
                return;
            }

            var (position, row, col) = cellInfo.Value;

            BaseTarget newTarget = CreateTargetAt(position);
            Console.WriteLine($"Created target at position {position}");
            WireTargetEvents(newTarget, position, row, col);
        }
        public void RestartRound()
        {
            _timer = ROUND_TIME;
            _score = 0;
            _timeMultiplier = TIME_MULTIPLIER_START;

            // Clear existing targets (OnDestroyed frees their grid cells)
            foreach (var target in EntitySystem.FindByType<BaseTarget>())
            {
                target.Destroy();
            }

            // Reset grid state
            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    _occupiedCells[row, col] = false;
                }
            }
            _targetPositionToCell.Clear();

            // Reset radiation
            if (_radiationManager != null)
            {
                _radiationManager.Reset();
            }

            // Repopulate the entire grid
            PopulateGrid();
        }

        // Fills the whole GRID_ROWS x GRID_COLS grid with targets, marking each cell occupied.
        // Shared by OnStart (initial fill) and RestartRound (refill) so the two can't drift apart.
        private void PopulateGrid()
        {
            float cellWidth = ScreenManager.ScreenWidth / (float)GRID_COLS;
            float cellHeight = ScreenManager.ScreenHeight / (float)GRID_ROWS;

            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    Vector2 position = new Vector2(
                        col * cellWidth + (cellWidth / 2),
                        row * cellHeight + (cellHeight / 2)
                    );

                    _occupiedCells[row, col] = true;

                    BaseTarget newTarget = CreateTargetAt(position);
                    WireTargetEvents(newTarget, position, row, col);
                }
            }
        }

        private void UpdateTargetSpawning(GameTime gameTime)
        {
            _targetSpawnTimer -= gameTime.ElapsedGameTime.TotalSeconds; if (_targetSpawnTimer <= 0)
            {                SpawnRandomTarget();
                // Faster spawning - extremely aggressive spawn rates for shorter game
                _targetSpawnTimer = (TARGET_SPAWN_DELAY / 2.0) + (GameRandom.NextFloat() * GameConstants.TARGET_SPAWN_RANDOM_FACTOR);

                // Spawn more targets as time goes on (increased spawn rate with shorter thresholds)
                if (_timer < GameConstants.SPAWN_ACCEL_THRESHOLD_1) _targetSpawnTimer *= GameConstants.SPAWN_ACCEL_MULTIPLIER_1;  // After 15 seconds
                if (_timer < GameConstants.SPAWN_ACCEL_THRESHOLD_2) _targetSpawnTimer *= GameConstants.SPAWN_ACCEL_MULTIPLIER_2;  // After 30 seconds
                if (_timer < GameConstants.SPAWN_ACCEL_THRESHOLD_3) _targetSpawnTimer *= GameConstants.SPAWN_ACCEL_MULTIPLIER_3;  // After 45 seconds
            }

            // v0.14.0: grid cell cleanup happens in the OnDestroyed event — no per-frame scan needed
        }
    }
}
