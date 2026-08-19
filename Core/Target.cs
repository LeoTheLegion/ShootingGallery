using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Scenes;
using CoreEssentials.Tweening;
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
        public event EventHandler<Vector2> OnDestroyed;

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
        }        // Configuration (static readonly so it captures the XML-loaded values
        // from GameConstants, which must be runtime fields rather than compile-time consts)
        protected static readonly int targetRadius = GameConstants.TARGET_RADIUS;
        protected static readonly float DefaultScale = GameConstants.TARGET_DEFAULT_SCALE;
        protected static readonly double TimeToFullSize = GameConstants.TARGET_TIME_TO_FULL_SIZE;
        
        // State
        protected float _scale;
        protected double _time;
        protected Random _random;
        protected Sprite _sprite;
        protected SpriteComponent _spriteComponent;
        protected TweenComponent _tweenComponent;
        protected TweenFloat _growthTween;
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
        public TargetType Type { get; protected set; }        public bool IsDestroyed => _isDestroyed;
        
        // Add a property to check if the target is fully grown (scale is at max)
        public bool IsFullyGrown => _scale >= GameConstants.TARGET_GROWTH_LARGE;
        
        protected Scene Scene { get; private set; }
        
        // Store a reference to the scene
        public void SetScene(Scene scene)
        {
            this.Scene = scene;        }

        // Add a public getter for the position
        public new Vector2 Position => _position;

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

            // v0.14.0: render via SpriteComponent so targets join z-layer texture batching.
            // The base Entity.Render() draws the component using Owner.Position/Rotation/Scale,
            // so no custom Render override is needed. Tint lives on the component's Color.
            _spriteComponent = AddComponent(new SpriteComponent(_sprite));
            _spriteComponent.Color = _tintColor;
            RegisterForInstancedRendering(_sprite);
            SetZLayer(0);
            Scale = new Vector2(_scale);

            // v0.14.0: drive growth with a TweenComponent (linear 0 -> 1 over TimeToFullSize,
            // matching the previous _time-based formula). base.Update() advances the tween.
            _tweenComponent = AddComponent(new TweenComponent());
            _growthTween = _tweenComponent.TweenToFloat(0f, 1f, (float)TimeToFullSize);

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
            base.Update(gameTime); // advances the growth tween
            _time += gameTime.ElapsedGameTime.TotalSeconds;
            _scale = _growthTween.GetValue();
            Scale = new Vector2(_scale);
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
        
        public virtual void Dispose()
        {
            AssetManager.UnloadAsset<Sprite>(_sprite.Name);
        }
        
        public new void Destroy()
        {
            base.Destroy();
            _isDestroyed = true;
            OnDestroyed?.Invoke(this, this._position);
            Dispose();
        }
    }

    // Standard Target (regular points)
    public class RegularTarget : BaseTarget
    {
        public RegularTarget(Vector2 targetPosition) : base(targetPosition, "target")
        {
            Type = TargetType.Standard;
        }        protected override void ProcessHit()
        {
            int score = CalculateScore();
            ReportScore(score);
            ReportRadiationChange(GameConstants.RADIATION_REGULAR); // Regular radiation amount
            
            // Destroy this target instead of moving it randomly
            Destroy();
        }private int CalculateScore()
        {
            // Score based on size (fully grown = more points)
            if (_scale >= GameConstants.TARGET_GROWTH_LARGE)
                return GameConstants.SCORE_REGULAR_LARGE; // Fully grown targets are worth more
            else if (_scale >= GameConstants.TARGET_GROWTH_MEDIUM)
                return GameConstants.SCORE_REGULAR_MEDIUM;
            else
                return GameConstants.SCORE_REGULAR_SMALL;
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
            ReportRadiationChange(GameConstants.RADIATION_RADIOACTIVE); // Triple radiation amount
            
            // Destroy this target instead of moving it randomly
            Destroy();
        }private int CalculateScore()
        {
            // Higher scores for radioactive targets, especially when fully grown
            if (_scale >= GameConstants.TARGET_GROWTH_LARGE)
                return GameConstants.SCORE_RADIOACTIVE_LARGE; // Fully grown radioactive targets worth a lot
            else if (_scale >= GameConstants.TARGET_GROWTH_MEDIUM)
                return GameConstants.SCORE_RADIOACTIVE_MEDIUM;
            else
                return GameConstants.SCORE_RADIOACTIVE_SMALL;
        }
    }    // Bomb Target (game over when hit)
    public class BombTarget : BaseTarget
    {
        // Configuration for the auto-fade behavior (static readonly to capture XML-loaded values)
        private static readonly double FadeStartTime = GameConstants.BOMB_FADE_START_TIME;
        private static readonly double FadeDuration = GameConstants.BOMB_FADE_DURATION;
        
        // State for tracking lifetime and alpha
        private double _lifeTime = 0; // How long this bomb has existed
        private float _alpha = 1.0f; // Transparency (1.0f = fully visible, 0.0f = invisible)
        
        public BombTarget(Vector2 targetPosition) : base(targetPosition, "target")
        {
            Type = TargetType.Bomb;
            _tintColor = new Color((byte)255, (byte)0, (byte)0, (byte)255); // Red tint for bomb
        }
        
        public override void Update(GameTime gameTime)
        {
            // Call the base Update method first
            base.Update(gameTime);
            
            // Increment lifetime
            _lifeTime += gameTime.ElapsedGameTime.TotalSeconds;
            
            // Start fading once we reach FadeStartTime
            if (_lifeTime > FadeStartTime)
            {
                // Calculate how far through the fade we are (0.0 to 1.0)
                float fadeProgress = (float)((_lifeTime - FadeStartTime) / FadeDuration);
                
                // Update alpha value (clamped between 0 and 1)
                _alpha = Math.Max(0, 1.0f - fadeProgress);
                
                // Update tint color with new alpha (explicitly using byte for all parameters)
                byte alpha = (byte)Math.Round(_alpha * 255);
                _tintColor = new Color((byte)255, (byte)0, (byte)0, alpha);
                _spriteComponent.Color = _tintColor;

                // When fully faded out, destroy the bomb
                if (_alpha <= 0)
                {
                    Console.WriteLine("Bomb has faded away completely and will be destroyed");
                    Destroy();
                }
            }
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
