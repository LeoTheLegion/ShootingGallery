using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Scenes;
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
        // Game constants       
        private const double ROUND_TIME = GameConstants.ROUND_TIME;      
        private const float TIME_MULTIPLIER_START = GameConstants.TIME_MULTIPLIER_START;
        private const float TIME_MULTIPLIER_MIN = GameConstants.TIME_MULTIPLIER_MIN;
        private const float TIME_MULTIPLIER_DECAY = GameConstants.TIME_MULTIPLIER_DECAY;
        private const double TARGET_SPAWN_DELAY = GameConstants.TARGET_SPAWN_DELAY;
        private const float BOMB_CHANCE = GameConstants.BOMB_CHANCE;
        private const float RADIOACTIVE_CHANCE = GameConstants.RADIOACTIVE_CHANCE;        // Target grid configuration
        private const int GRID_ROWS = GameConstants.GRID_ROWS;
        private const int GRID_COLS = GameConstants.GRID_COLS;
        private const int TARGET_SIZE = 64; // Size of target sprite
        private bool[,] _occupiedCells; // true = occupied, false = free
        private Dictionary<Vector2, (int Row, int Col)> _targetPositionToCell; // Maps positions to grid cells

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

        // Target tracking
        private List<BaseTarget> _activeTargets = new List<BaseTarget>();
        private Random _random = new Random();

        // Getters
        public double GetGameTime() => _timer;
        public int GetScore() => _score;
        public float GetTimeMultiplier() => _timeMultiplier;

        private void UpdateCrosshairTargets()
        {
            if (_crosshair != null)
            {
                _crosshair.SetCurrentTargets(_activeTargets);
            }
        }

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

            // Populate the entire grid with targets at startup
            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    // Calculate position for this cell
                    float cellWidth = ScreenManager.ScreenWidth / (float)GRID_COLS;
                    float cellHeight = ScreenManager.ScreenHeight / (float)GRID_ROWS;

                    Vector2 position = new Vector2(
                        col * cellWidth + (cellWidth / 2),
                        row * cellHeight + (cellHeight / 2)
                    );

                    // Mark cell as occupied
                    _occupiedCells[row, col] = true;

                    // Create a target (mostly regular targets with some special ones)
                    BaseTarget newTarget;
                    double roll = _random.NextDouble();

                    if (roll < BOMB_CHANCE)
                    {
                        newTarget = EntitySystem.CreateEntity<BombTarget>(position);
                        Console.WriteLine($"Created BOMB at grid ({row},{col}): {position}");
                    }
                    else if (roll < BOMB_CHANCE + RADIOACTIVE_CHANCE)
                    {
                        newTarget = EntitySystem.CreateEntity<RadioactiveTarget>(position);
                        Console.WriteLine($"Created RADIOACTIVE at grid ({row},{col}): {position}");
                    }
                    else
                    {
                        newTarget = EntitySystem.CreateEntity<RegularTarget>(position);
                        Console.WriteLine($"Created REGULAR at grid ({row},{col}): {position}");
                    }

                    // Store mapping from position to grid cell
                    _targetPositionToCell[position] = (row, col);

                    // Subscribe to target events
                    newTarget.OnScore += (sender, args) =>
                    {
                        AddScore(args.Score);
                    };

                    newTarget.OnRadiationChange += (sender, args) =>
                    {
                        if (_radiationManager != null)
                        {
                            _radiationManager.AddRadiation(args.RadiationAmount);
                        }
                    };

                    newTarget.OnGameOver += (sender, args) =>
                    {
                        // Game over from hitting a bomb
                        OnGameOver?.Invoke(this, new GameOverEventArgs(_score));
                    };

                    _activeTargets.Add(newTarget);
                }
            }

            // Update crosshair targets list
            UpdateCrosshairTargets();
        }
        private void HandleCrosshairShoot(object sender, Crosshair.ShootEventArgs e)
        {
            // Log the shot for debugging
            Console.WriteLine($"Shot detected at position: {e.Position}");

            // Check for target hits
            bool targetHit = false;
            foreach (var target in _activeTargets)
            {
                // Log the target being checked
                Console.WriteLine($"Checking target at position: {target.Position}, distance: {Vector2.Distance(target.Position, e.Position)}");

                // Store the target's IsDestroyed state before handling the shot
                bool wasDestroyed = target.IsDestroyed;

                // Handle the shot
                target.HandleShot(e.Position);

                // If the target wasn't destroyed before and is now, it means we hit it
                if (!wasDestroyed && target.IsDestroyed)
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

            // Keep crosshair's target list updated
            UpdateCrosshairTargets();
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
            // Calculate grid cell size based on screen dimensions
            float cellWidth = ScreenManager.ScreenWidth / (float)GRID_COLS;
            float cellHeight = ScreenManager.ScreenHeight / (float)GRID_ROWS;

            Console.WriteLine($"Grid cell size: {cellWidth}x{cellHeight}");

            // Check if any cells are available
            bool anyAvailable = false;
            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    if (!_occupiedCells[row, col])
                    {
                        anyAvailable = true;
                        break;
                    }
                }
                if (anyAvailable) break;
            }

            // If no cells available, return null
            if (!anyAvailable)
            {
                Console.WriteLine("No grid cells available");
                return null;
            }

            // Find a random unoccupied cell
            int attempts = 0;
            while (attempts < 100) // Prevent infinite loop
            {
                int row = _random.Next(0, GRID_ROWS);
                int col = _random.Next(0, GRID_COLS);

                if (!_occupiedCells[row, col])
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

                attempts++;
            }

            // If we tried 100 times and couldn't find a spot, do a direct search
            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    if (!_occupiedCells[row, col])
                    {
                        _occupiedCells[row, col] = true;

                        // Calculate position (center of cell with offset for target center)
                        Vector2 position = new Vector2(
                            col * cellWidth + (cellWidth / 2),
                            row * cellHeight + (cellHeight / 2)
                        );

                        Console.WriteLine($"Target positioned at grid ({row},{col}) -> screen position {position}");
                        return (position, row, col);
                    }
                }
            }

            // Should never get here if anyAvailable was true
            return null;
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

            BaseTarget newTarget;
            double roll = _random.NextDouble();

            if (roll < BOMB_CHANCE)
            {
                newTarget = EntitySystem.CreateEntity<BombTarget>(position);
                Console.WriteLine($"Created BOMB at position {position}");
            }
            else if (roll < BOMB_CHANCE + RADIOACTIVE_CHANCE)
            {
                newTarget = EntitySystem.CreateEntity<RadioactiveTarget>(position);
                Console.WriteLine($"Created RADIOACTIVE at position {position}");
            }
            else
            {
                newTarget = EntitySystem.CreateEntity<RegularTarget>(position);
                Console.WriteLine($"Created REGULAR at position {position}");
            }

            // Store mapping from position to grid cell
            _targetPositionToCell[position] = (row, col);

            // Subscribe to target events
            newTarget.OnScore += (sender, args) =>
            {
                AddScore(args.Score);
            };

            newTarget.OnRadiationChange += (sender, args) =>
            {
                if (_radiationManager != null)
                {
                    _radiationManager.AddRadiation(args.RadiationAmount);
                }
            };

            newTarget.OnGameOver += (sender, args) =>
            {
                // Game over from hitting a bomb
                OnGameOver?.Invoke(this, new GameOverEventArgs(_score));
            };

            _activeTargets.Add(newTarget);
        }
        public void RestartRound()
        {
            _timer = ROUND_TIME;
            _score = 0;
            _timeMultiplier = TIME_MULTIPLIER_START;

            // Clear existing targets
            foreach (var target in _activeTargets)
            {
                target.Destroy();
            }
            _activeTargets.Clear();

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
            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    // Calculate position for this cell
                    float cellWidth = ScreenManager.ScreenWidth / (float)GRID_COLS;
                    float cellHeight = ScreenManager.ScreenHeight / (float)GRID_ROWS;

                    Vector2 position = new Vector2(
                        col * cellWidth + (cellWidth / 2),
                        row * cellHeight + (cellHeight / 2)
                    );

                    // Mark cell as occupied
                    _occupiedCells[row, col] = true;

                    // Create a target (mostly regular targets with some special ones)
                    BaseTarget newTarget;
                    double roll = _random.NextDouble();

                    if (roll < BOMB_CHANCE)
                    {
                        newTarget = EntitySystem.CreateEntity<BombTarget>(position);
                    }
                    else if (roll < BOMB_CHANCE + RADIOACTIVE_CHANCE)
                    {
                        newTarget = EntitySystem.CreateEntity<RadioactiveTarget>(position);
                    }
                    else
                    {
                        newTarget = EntitySystem.CreateEntity<RegularTarget>(position);
                    }

                    // Store mapping from position to grid cell
                    _targetPositionToCell[position] = (row, col);

                    // Subscribe to target events
                    newTarget.OnScore += (sender, args) =>
                    {
                        AddScore(args.Score);
                    };

                    newTarget.OnRadiationChange += (sender, args) =>
                    {
                        if (_radiationManager != null)
                        {
                            _radiationManager.AddRadiation(args.RadiationAmount);
                        }
                    };

                    newTarget.OnGameOver += (sender, args) =>
                    {
                        // Game over from hitting a bomb
                        OnGameOver?.Invoke(this, new GameOverEventArgs(_score));
                    };

                    _activeTargets.Add(newTarget);
                }
            }

            // Update crosshair targets list
            UpdateCrosshairTargets();
        }

        private void UpdateTargetSpawning(GameTime gameTime)
        {
            _targetSpawnTimer -= gameTime.ElapsedGameTime.TotalSeconds; if (_targetSpawnTimer <= 0)
            {                SpawnRandomTarget();
                UpdateCrosshairTargets();                // Faster spawning - extremely aggressive spawn rates for shorter game
                _targetSpawnTimer = (TARGET_SPAWN_DELAY / 2.0) + (_random.NextDouble() * GameConstants.TARGET_SPAWN_RANDOM_FACTOR);

                // Spawn more targets as time goes on (increased spawn rate with shorter thresholds)
                if (_timer < GameConstants.SPAWN_ACCEL_THRESHOLD_1) _targetSpawnTimer *= GameConstants.SPAWN_ACCEL_MULTIPLIER_1;  // After 15 seconds
                if (_timer < GameConstants.SPAWN_ACCEL_THRESHOLD_2) _targetSpawnTimer *= GameConstants.SPAWN_ACCEL_MULTIPLIER_2;  // After 30 seconds
                if (_timer < GameConstants.SPAWN_ACCEL_THRESHOLD_3) _targetSpawnTimer *= GameConstants.SPAWN_ACCEL_MULTIPLIER_3;  // After 45 seconds
            }

            // Clean up destroyed targets and their grid positions
            var destroyedTargets = _activeTargets.Where(t => t == null || t.IsDestroyed).ToList();
            foreach (var target in destroyedTargets)
            {
                if (target != null)
                {
                    Vector2 position = target.Position;
                    Console.WriteLine($"Target destroyed at position {position}");

                    // Free up the grid cell
                    if (_targetPositionToCell.TryGetValue(position, out var cell))
                    {
                        _occupiedCells[cell.Row, cell.Col] = false;
                        _targetPositionToCell.Remove(position);
                        Console.WriteLine($"Freed up grid cell ({cell.Row}, {cell.Col})");
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Could not find grid cell for position {position}");
                    }
                }
            }
            _activeTargets.RemoveAll(t => t == null || t.IsDestroyed);

            if (destroyedTargets.Any())
            {
                UpdateCrosshairTargets();
            }
        }
    }
}
