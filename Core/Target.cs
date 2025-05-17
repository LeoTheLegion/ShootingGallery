using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.SceneManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShootingGallery.Core;
using System;

namespace ShootingGallery
{    // Base Target class with common functionality
    public abstract class BaseTarget : Entity, IDisposable
    {
        // Events
        public event EventHandler<ScoreEventArgs> OnScore;
        public event EventHandler<GameOverEventArgs> OnGameOver;
        public event EventHandler<RadiationEventArgs> OnRadiationChange;

        // Event arguments
        public class ScoreEventArgs : EventArgs
        {
            public int Score { get; }
            public Vector2 Position { get; }

            public ScoreEventArgs(int score, Vector2 position)
            {
                Score = score;
                Position = position;
            }
        }

        public class GameOverEventArgs : EventArgs { }

        public class RadiationEventArgs : EventArgs
        {
            public float RadiationAmount { get; }
            
            public RadiationEventArgs(float radiationAmount)
            {
                RadiationAmount = radiationAmount;
            }
        }

        // Constants
        protected const int targetRadius = 45;
        protected const float DefaultScale = 0.3f;
        protected const double TimeToFullSize = 3.0;
        
        // State
        protected float _scale;
        protected double _time;
        protected Random _random;
        protected Sprite _sprite;
        protected string _spriteName;
        protected Color _tintColor = Color.White;
        protected bool _isDestroyed = false;
        
        // Target type identifiers
        public enum TargetType
        {
            Standard,
            Radioactive,
            Bomb
        }
        public TargetType Type { get; protected set; }
        public bool IsDestroyed => _isDestroyed;
        
        protected Scene Scene { get; private set; }
        
        // Store a reference to the scene
        public void SetScene(Scene scene)
        {
            this.Scene = scene;
        }

        // Add a public getter for the position
        public Vector2 Position => _position;

        protected BaseTarget(Vector2 targetPosition, string spriteName) : base()
        {
            this._position = targetPosition;
            this._scale = DefaultScale;
            this._time = 0f;
            this._random = new Random();            this._spriteName = spriteName;
            
            Type = TargetType.Standard; // Default
        }

        public override void OnStart()
        {
            base.OnStart();
            this._sprite = AssetManager.LoadAsset<Sprite>("target_sprite.xml");
            
            // No longer move randomly on start - use the position provided in constructor
            Console.WriteLine($"Target spawned at position: {_position}");
        }

        public virtual void HandleShot(Vector2 shotPosition)
        {
            float distanceToShot = Vector2.Distance(_position, shotPosition);
            Console.WriteLine($"HandleShot: Target at {_position}, Shot at {shotPosition}, Distance: {distanceToShot}, HitRadius: {targetRadius * _scale}");

            if (distanceToShot < targetRadius * _scale)
            {
                Console.WriteLine("Hit detected! Processing...");
                ProcessHit();
            }
        }
        
        protected abstract void ProcessHit();

        protected void ReportScore(int score)
        {
            this.EntitySystem.CreateEntity<FloatingPopUpText>(
                this._position,
                2f,
                score.ToString()
            );

            // Trigger the OnScore event
            OnScore?.Invoke(this, new ScoreEventArgs(score, this._position));
        }
        
        protected void ReportGameOver()
        {
            // Trigger game over
            OnGameOver?.Invoke(this, new GameOverEventArgs());
        }
        
        protected void ReportRadiationChange(float amount)
        {
            // Trigger radiation change
            OnRadiationChange?.Invoke(this, new RadiationEventArgs(amount));
        }

        public override void Update(GameTime gameTime)
        {
            _time += gameTime.ElapsedGameTime.TotalSeconds;
            _scale = (float)Math.MinMagnitude(_time / TimeToFullSize, 1);
        }
        
        // Add a method for EntitySystem-compatible Update
        public void Update(ref GameTime gameTime)
        {
            Update(gameTime);
        }

        protected void MoveRandomly()
        {
            _position.X = _random.Next(targetRadius, ScreenManager.ScreenWidth - targetRadius);
            _position.Y = _random.Next(targetRadius, ScreenManager.ScreenHeight - targetRadius);
        }
        protected void Reset()
        {
            _scale = DefaultScale;
            _time = 0;
        }
        
        public override void Render(SpriteBatch _spriteBatch)
        {
            _sprite.Draw(_spriteBatch, _position, _tintColor, 0f, Vector2.One * _scale, SpriteEffects.None, 0);
        }
        
        // Add a method for EntitySystem-compatible Render
        public void Render(ref SpriteBatch _spriteBatch)
        {
            Render(_spriteBatch);
        }

        public virtual void Dispose()
        {
            AssetManager.UnloadAsset<Sprite>(_sprite.Name);
        }
        
        public new void Destroy()
        {            base.Destroy();
            _isDestroyed = true;
            Dispose();
        }
    }

    // Standard Target (regular points)
    public class RegularTarget : BaseTarget
    {
        public RegularTarget(Vector2 targetPosition) : base(targetPosition, "target")
        {
            Type = TargetType.Standard;
        }

        protected override void ProcessHit()
        {
            int score = CalculateScore();
            ReportScore(score);
            ReportRadiationChange(1.0f); // Regular radiation amount
            
            // Destroy this target instead of moving it randomly
            Destroy();
        }
        
        private int CalculateScore()
        {
            // Score based on size (smaller = harder to hit = more points)
            if (_scale < .4f)
                return 10;
            else if (_scale < 0.8f)
                return 5;            else
                return 1;
        }
    }

    // Radioactive Target (higher radiation, higher score)
    public class RadioactiveTarget : BaseTarget
    {
        public RadioactiveTarget(Vector2 targetPosition) : base(targetPosition, "target")
        {
            Type = TargetType.Radioactive;
            _tintColor = new Color(0, 255, 0); // Green tint for radioactive
        }
        
        protected override void ProcessHit()
        {
            int score = CalculateScore();
            ReportScore(score);
            ReportRadiationChange(3.0f); // Triple radiation amount
            
            // Destroy this target instead of moving it randomly
            Destroy();
        }
        
        private int CalculateScore()
        {
            // Higher scores for radioactive targets
            if (_scale < .4f)
                return 20;
            else if (_scale < 0.8f)
                return 10;
            else
                return 5;
        }
    }
    // Bomb Target (game over when hit)
    public class BombTarget : BaseTarget
    {
        public BombTarget(Vector2 targetPosition) : base(targetPosition, "target")
        {
            Type = TargetType.Bomb;
            _tintColor = new Color(255, 0, 0); // Red tint for bomb
        }
        
        protected override void ProcessHit()
        {
            // Game over!
            ReportGameOver();
        }
    }
    
    // Legacy Target class for backward compatibility
    public class Target : RegularTarget
    {
        public Target(Vector2 targetPosition) : base(targetPosition)
        {
        }
    }
}
