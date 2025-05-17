using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.SceneManagement;
using Microsoft.Xna.Framework;
using ShootingGallery.Core;
using System;
using System.Collections.Generic;

namespace ShootingGallery
{
    public class GameManager : Entity
    {
        // Event for game over
        public event EventHandler<GameOverEventArgs> OnGameOver;

        // Event args for game over
        public class GameOverEventArgs : EventArgs
        {
            public int FinalScore { get; }

            public GameOverEventArgs(int finalScore)
            {
                FinalScore = finalScore;
            }
        }

        // Game constants
        private const double ROUND_TIME = 300; // 5 minutes in seconds
        private const float TIME_MULTIPLIER_START = 10.0f; // Starting multiplier
        private const float TIME_MULTIPLIER_MIN = 1.0f; // Minimum multiplier
        private const float TIME_MULTIPLIER_DECAY = 0.2f; // Decay per second
        private const int TARGET_SPAWN_DELAY = 3; // Seconds between target spawns
        private const float BOMB_CHANCE = 0.2f; // 20% chance for a bomb
        private const float RADIOACTIVE_CHANCE = 0.3f; // 30% chance for radioactive target

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
        }

        public override void OnStart()
        {
            base.OnStart();

            // Initialize with a few targets
            for (int i = 0; i < 3; i++)
            {
                SpawnRandomTarget();
            }
        }
        private void HandleCrosshairShoot(object sender, Crosshair.ShootEventArgs e)
        {
            // Log the shot for debugging
            Console.WriteLine($"Shot detected at position: {e.Position}");

            // Check for target hits
            foreach (var target in _activeTargets)
            {
                // Log the target being checked
                Console.WriteLine($"Checking target at position: {target.Position}, distance: {Vector2.Distance(target.Position, e.Position)}");
                target.HandleShot(e.Position);
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

        private void UpdateTargetSpawning(GameTime gameTime)
        {
            _targetSpawnTimer -= gameTime.ElapsedGameTime.TotalSeconds;

            if (_targetSpawnTimer <= 0)
            {
                SpawnRandomTarget();
                UpdateCrosshairTargets(); // Update after spawning

                // Calculate next spawn time (with some randomness)
                _targetSpawnTimer = TARGET_SPAWN_DELAY + (_random.NextDouble() * 2);

                // Spawn more targets as time goes on
                if (_timer < ROUND_TIME * 0.75)
                {
                    _targetSpawnTimer *= 0.9;
                }
                if (_timer < ROUND_TIME * 0.5)
                {
                    _targetSpawnTimer *= 0.8;
                }
                if (_timer < ROUND_TIME * 0.25)
                {
                    _targetSpawnTimer *= 0.7;
                }
            }

            // Clean up destroyed targets
            bool targetsRemoved = _activeTargets.RemoveAll(t => t == null || t.IsDestroyed) > 0;
            if (targetsRemoved)
            {
                UpdateCrosshairTargets(); // Update if any targets were removed
            }
        }
        private void SpawnRandomTarget()
        {
            Vector2 randomPosition = new Vector2(
                _random.Next(50, ScreenManager.ScreenWidth - 50),
                _random.Next(50, ScreenManager.ScreenHeight - 50)
            );

            BaseTarget newTarget;
            double roll = _random.NextDouble();

            if (roll < BOMB_CHANCE)
            {
                // Spawn a bomb
                newTarget = EntitySystem.CreateEntity<BombTarget>(randomPosition);
            }
            else if (roll < BOMB_CHANCE + RADIOACTIVE_CHANCE)
            {
                // Spawn a radioactive target
                newTarget = EntitySystem.CreateEntity<RadioactiveTarget>(randomPosition);
            }
            else
            {
                // Spawn a regular target
                newTarget = EntitySystem.CreateEntity<RegularTarget>(randomPosition);
            }

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

            // Reset radiation
            if (_radiationManager != null)
            {
                _radiationManager.Reset();
            }

            // Spawn new targets
            for (int i = 0; i < 3; i++)
            {
                SpawnRandomTarget();
            }
        }
    }
}
