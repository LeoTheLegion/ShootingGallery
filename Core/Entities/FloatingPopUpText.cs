using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Tweening;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShootingGallery.Core;

namespace ShootingGallery
{
    /// <summary>
    /// Drifting, fading score popup. Orchestrates LabelComponent (rendering) and
    /// TweenComponent (drift) with a per-frame opacity fade.
    /// </summary>
    public class FloatingPopUpText : Entity
    {
        private const float Distance = 10f;

        private LabelComponent _label;
        private TweenVector2 _drift;

        private readonly string _text;
        private readonly Color _color;
        private readonly float _scale;
        private readonly float _totalTime;
        private float _timeLeft;
        private readonly bool _isRadiationEffect;

        // Standard floating text
        public FloatingPopUpText(Vector2 position, float time, string text)
            : this(position, time, text, Color.White, 1.0f)
        {
        }

        // Floating text with custom color and scale; radiationEffect enables the pulsing style
        public FloatingPopUpText(Vector2 position, float time, string text, Color color, float scale = 1.0f, bool radiationEffect = false)
        {
            this._position = position;
            _text = text;
            _color = color;
            _scale = scale;
            _totalTime = _timeLeft = time;
            _isRadiationEffect = radiationEffect;
        }

        // Floating text with radiation effect
        public FloatingPopUpText(Vector2 position, float time, string text, bool isRadiationEffect)
            : this(position, time, text, isRadiationEffect ? new Color(0, 255, 0) : Color.White)
        {
        }

        public override void OnStart()
        {
            base.OnStart();

            // Components are added in OnStart (CE lifecycle: Awake -> Start).
            // Popups are standalone (no parent), so each carries its own screen-space canvas;
            // the label below resolves this entity's canvas on attach.
            AddComponent(new CanvasComponent(true));
            _label = AddComponent(new LabelComponent(_text));
            _label.TextColor = _color;
            _label.Scale = new Vector2(_scale);

            var tween = AddComponent(new TweenComponent());
            // Linear upward drift over the lifetime
            _drift = tween.TweenToVector2(_position, _position + new Vector2(0, -Distance), _totalTime);

            // Entity system handles lifetime instead of a manual countdown
            DestroyAfter(TimeSpan.FromSeconds(_totalTime));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime); // advances the drift tween

            // Position comes from the drift tween
            this._position = _drift.GetValue();

            if (_isRadiationEffect)
            {
                float pulse = (float)Math.Sin(_timeLeft * 10) * 0.2f + 0.8f;
                _label.Opacity = pulse * Smooth(_totalTime - _timeLeft, _totalTime, 4f);
                _label.Scale = new Vector2(pulse);
            }
            else
            {
                _label.Opacity = Smooth(_totalTime - _timeLeft, _totalTime, 4f);
            }

            _timeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
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
