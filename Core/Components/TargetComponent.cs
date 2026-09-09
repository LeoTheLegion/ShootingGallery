using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Tweening;
using Microsoft.Xna.Framework;
using ShootingGallery.Core;

namespace ShootingGallery.Core;

/// <summary>
/// All target gameplay (growth, hit testing, scoring, radiation, bomb fade) as a single
/// composable component. The entity itself is a plain GameObjectEntity declared in the
/// target template XML; this component owns the sprite and all behavior.
/// Sprite/tween setup is deferred to the first Update because template instantiation
/// applies XML-settable properties (e.g. <see cref="Type"/>) only after OnAttach.
/// </summary>
public class TargetComponent : EntityComponent
{
    /// <summary>The kind of target this component behaves as.</summary>
    public enum TargetType { Standard, Radioactive, Bomb }

    // Which target behavior to run (set from template XML).
    public TargetType Type { get; set; } = TargetType.Standard;

    // Events (consumed by the director component)
    public event EventHandler<TargetScoreEventArgs> OnScore;
    public event EventHandler OnGameOver;
    public event EventHandler<TargetRadiationEventArgs> OnRadiationChange;
    public event EventHandler<Vector2> OnDestroyed;

    public class TargetScoreEventArgs : EventArgs
    {
        public int Score { get; }
        public Vector2 Position { get; }
        public TargetScoreEventArgs(int score, Vector2 position)
        {
            Score = score;
            Position = position;
        }
    }

    public class TargetRadiationEventArgs : EventArgs
    {
        public float RadiationAmount { get; }
        public TargetRadiationEventArgs(float radiationAmount)
        {
            RadiationAmount = radiationAmount;
        }
    }

    // Configuration (static readonly so it captures the XML-loaded values from GameConstants)
    private static readonly int TargetRadius = GameConstants.TargetRadius;
    private static readonly float DefaultScale = GameConstants.TargetDefaultScale;
    private static readonly double TimeToFullSize = GameConstants.TargetTimeToFullSize;
    private static readonly double BombFadeStartTime = GameConstants.BombFadeStartTime;
    private static readonly double BombFadeDuration = GameConstants.BombFadeDuration;

    // State
    private bool _initialized;
    private float _scale = DefaultScale;
    private SpriteComponent _spriteComponent;
    private TweenFloat _growthTween;
    private Color _tintColor = Color.White;
    private double _lifeTime;
    private bool _isDestroyed;

    public bool IsDestroyed => _isDestroyed;

    /// <summary>True once the growth tween has reached full size (scale >= large threshold).</summary>
    public bool IsFullyGrown => _scale >= GameConstants.TargetGrowthLarge;

    /// <summary>
    /// Component creation (sprite + tween) happens here, outside the entity's per-frame
    /// component enumeration — adding a component from within Update would mutate the
    /// collection being enumerated and throw. Only the tint depends on the XML-set
    /// <see cref="Type"/> property, so that part is deferred to the first Update.
    /// </summary>
    public override void OnAttach()
    {
        base.OnAttach();

        // The SpriteComponent and TweenComponent are declared in the target prefab XML (declared
        // before this component), so we grab them here rather than adding components to the entity
        // — adding a component from within OnAttach would mutate the collection CE is enumerating.
        var sprite = AssetManager.LoadAsset<Sprite>("target_sprite.xml");
        _spriteComponent = Owner.GetComponent<SpriteComponent>();
        if (_spriteComponent != null && _spriteComponent.Sprite == null)
            _spriteComponent.Sprite = sprite;

        Owner.RegisterForInstancedRendering(sprite);
        Owner.SetZLayer(0);

        var tweenComponent = Owner.GetComponent<TweenComponent>();
        if (tweenComponent != null)
            _growthTween = tweenComponent.TweenToFloat(0f, 1f, (float)TimeToFullSize);
    }

    public override void Update(GameTime gameTime)
    {
        if (Owner == null || Owner.Destroyed)
            return;

        EnsureInitialized();

        // Growth: tween 0 -> 1 drives the scale (same as the old entity's Update)
        if (_growthTween != null)
        {
            _scale = _growthTween.GetValue();
            Owner.Scale = new Vector2(_scale);
        }

        // Bomb lifetime fade
        if (Type == TargetType.Bomb)
        {
            _lifeTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (_lifeTime > BombFadeStartTime)
            {
                float fadeProgress = (float)((_lifeTime - BombFadeStartTime) / BombFadeDuration);
                float alpha = Math.Max(0, 1.0f - fadeProgress);
                byte alphaByte = (byte)Math.Round(alpha * 255);
                _tintColor = new Color((byte)255, (byte)0, (byte)0, alphaByte);
                if (_spriteComponent != null)
                    _spriteComponent.Color = _tintColor;

                if (alpha <= 0)
                    Destroy();
            }
        }
    }

    /// <summary>
    /// One-time tint setup. Deferred from OnAttach because template XML applies the
    /// <see cref="Type"/> property only after attach.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_initialized)
            return;
        _initialized = true;

        switch (Type)
        {
            case TargetType.Radioactive:
                _tintColor = new Color(0, 255, 0);
                break;
            case TargetType.Bomb:
                _tintColor = new Color(255, 0, 0, 255);
                break;
        }

        if (_spriteComponent != null)
            _spriteComponent.Color = _tintColor;

        Owner.Scale = new Vector2(_scale);
    }

    /// <summary>
    /// Called by the director when a shot lands. Hit if the shot is within
    /// TargetRadius * current scale of the target center.
    /// </summary>
    public void HandleShot(Vector2 shotPosition)
    {
        float distanceToShot = Vector2.Distance(Owner.Position, shotPosition);

        if (distanceToShot < TargetRadius * _scale)
            ProcessHit();
    }

    private void ProcessHit()
    {
        switch (Type)
        {
            case TargetType.Standard:
                ReportScore(CalculateScore(GameConstants.ScoreRegularLarge,
                                           GameConstants.ScoreRegularMedium,
                                           GameConstants.ScoreRegularSmall));
                OnRadiationChange?.Invoke(this, new TargetRadiationEventArgs(GameConstants.RadiationRegular));
                Destroy();
                break;

            case TargetType.Radioactive:
                ReportScore(CalculateScore(GameConstants.ScoreRadioactiveLarge,
                                           GameConstants.ScoreRadioactiveMedium,
                                           GameConstants.ScoreRadioactiveSmall));
                OnRadiationChange?.Invoke(this, new TargetRadiationEventArgs(GameConstants.RadiationRadioactive));
                Destroy();
                break;

            case TargetType.Bomb:
                OnGameOver?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    private int CalculateScore(int large, int medium, int small)
    {
        if (_scale >= GameConstants.TargetGrowthLarge)
            return large;
        if (_scale >= GameConstants.TargetGrowthMedium)
            return medium;
        return small;
    }

    private void ReportScore(int score)
    {
        // Unity-style: instantiate the "Popup" prefab; its XML defaults (duration 2s, white,
        // scale 1) apply — only the text varies per hit.
        var popup = InstantiatePrefab("Popup", Owner.Position)?.GetComponent<FloatingPopUpComponent>();
        if (popup != null)
            popup.Text = score.ToString();

        OnScore?.Invoke(this, new TargetScoreEventArgs(score, Owner.Position));
    }

    /// <summary>Destroys the owning entity and fires OnDestroyed with its final position.</summary>
    public void Destroy()
    {
        if (_isDestroyed)
            return;

        _isDestroyed = true;
        Vector2 position = Owner.Position;
        Owner.Destroy();
        OnDestroyed?.Invoke(this, position);
    }
}
