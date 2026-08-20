using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
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

        private float _timeLeft;
        private readonly float _totalTime;
        private readonly bool _isRadiationEffect;

        // Standard floating text
        public FloatingPopUpText(Vector2 position, float time, string text)
            : this(position, time, text, Color.White, 1.0f)
        {
        }

        // Floating text with custom color and scale
        public FloatingPopUpText(Vector2 position, float time, string text, Color color, float scale = 1.0f)
        {
            this._position = position;
            _totalTime = _timeLeft = time;

            // Set radiation effect if it's green
            _isRadiationEffect = color.G > 200 && color.R < 100 && color.B < 100;

            _label = AddComponent(new LabelComponent(text));
            _label.TextColor = color;
            _label.Scale = scale;

            // Entity system handles lifetime instead of a manual countdown
            DestroyAfter(TimeSpan.FromSeconds(time));
        }

        // Floating text with radiation effect
        public FloatingPopUpText(Vector2 position, float time, string text, bool isRadiationEffect)
            : this(position, time, text, isRadiationEffect ? new Color(0, 255, 0) : Color.White)
        {
        }

        public override void OnStart()
        {
            base.OnStart();
            var tween = AddComponent(new TweenComponent());
            // Linear upward drift over the lifetime
            _drift = tween.TweenToVector2(_position, _position + new Vector2(0, -Distance), _totalTime);
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
                _label.Scale = pulse;
            }
            else
            {
                _label.Opacity = Smooth(_totalTime - _timeLeft, _totalTime, 4f);
            }

            _timeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        // Eased fade curve; can drift slightly outside [0,1] (LabelComponent clamps)
        private static float Smooth(float currentTime, float totalTime, float strength)
        {
            double num = Math.Pow(-totalTime + 2 * currentTime, strength);
            double dom = Math.Pow(totalTime, strength);
            return (float)(-(num / dom) + 1);
        }
    }
}
