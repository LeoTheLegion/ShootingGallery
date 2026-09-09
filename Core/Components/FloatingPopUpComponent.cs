using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Tweening;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Drifting, fading score popup. The entity is a plain GameObjectEntity declared in the
    /// "popup" template (Content/popup.xml); this component just drives its CanvasComponent
    /// and LabelComponent with per-popup values (text, color, scale, duration) that each spawn
    /// site pokes right after InstantiateTemplate — before the first Update — so the deferred
    /// setup in <see cref="EnsureInitialized"/> always sees them.
    /// </summary>
    public class FloatingPopUpComponent : EntityComponent
    {
        private const float Distance = 10f;

        // Per-popup values (poked by each spawn site right after InstantiateTemplate)
        public string Text { get; set; } = "";
        public Color TextColor { get; set; } = Color.White;
        public float Scale { get; set; } = 1.0f;
        public float Duration { get; set; } = 2f;
        public bool RadiationEffect { get; set; }

        private LabelComponent _label;
        private TweenVector2 _drift;
        private float _timeLeft;
        private bool _initialized;

        public override void Update(GameTime gameTime)
        {
            if (Owner == null || Owner.Destroyed)
                return;

            EnsureInitialized();

            // Position comes from the drift tween
            Owner.Position = _drift.GetValue();

            if (RadiationEffect)
            {
                float pulse = (float)Math.Sin(_timeLeft * 10) * 0.2f + 0.8f;
                _label.Opacity = pulse * Smooth(Duration - _timeLeft, Duration, 4f);
                _label.Scale = new Vector2(pulse);
            }
            else
            {
                _label.Opacity = Smooth(Duration - _timeLeft, Duration, 4f);
            }

            _timeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;
            _initialized = true;

            // By the first Update all template components are attached (the label attaches
            // after this component, so it can only be resolved here).
            _label = Owner.GetComponent<LabelComponent>();

            _label.Text = Text;
            _label.TextColor = TextColor;
            _label.Scale = new Vector2(Scale);

            var tween = Owner.GetComponent<TweenComponent>();
            // Linear upward drift over the lifetime
            _drift = tween.TweenToVector2(Owner.Position, Owner.Position + new Vector2(0, -Distance), Duration);

            _timeLeft = Duration;
            // Entity system handles lifetime instead of a manual countdown
            Owner.DestroyAfter(TimeSpan.FromSeconds(Duration));
        }

        // Eased fade-out over the popup's life. Returns a value guaranteed to be within [0,1]:
        // CE's LabelComponent passes opacity straight to Myra, which throws on out-of-range.
        private static float Smooth(float currentTime, float totalTime, float strength)
        {
            if (totalTime <= 0f)
                return 0f;

            float t = Math.Clamp(currentTime / totalTime, 0f, 1f); // 0 at spawn -> 1 at end of life
            float eased = (float)Math.Pow(1f - t, strength);        // 1 (visible) -> 0 (gone), never NaN
            return Math.Clamp(eased, 0f, 1f);
        }
    }
}
