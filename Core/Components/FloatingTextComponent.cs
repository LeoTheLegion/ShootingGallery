using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Tweening;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShootingGallery.Core
{
    /// <summary>
    /// A label that drifts upward, fades out, and destroys its owner after a lifetime.
    /// Combines LabelComponent (rendering) with a TweenComponent (drift) and an opacity fade.
    /// </summary>
    public class FloatingTextComponent : EntityComponent
    {
        private const float Distance = 10f;

        private readonly LabelComponent _label;
        private readonly TweenComponent _tween;
        private TweenVector2 _drift;

        private float _timeLeft;
        private readonly float _totalTime;
        private readonly bool _radiationEffect;

        public FloatingTextComponent(string text, float time, Color color, float scale = 1.0f, bool radiationEffect = false)
        {
            _totalTime = time;
            _timeLeft = time;
            _radiationEffect = radiationEffect;

            _label = new LabelComponent(text);
            _label.TextColor = color;
            _label.Scale = scale;

            _tween = new TweenComponent();
        }

        public override void OnAttach()
        {
            Owner.AddComponent(_label);
            Owner.AddComponent(_tween);

            // Linear upward drift over the lifetime
            _drift = _tween.TweenToVector2(Owner.Position, Owner.Position + new Vector2(0, -Distance), _totalTime);

            // Entity system handles lifetime instead of a manual countdown
            Owner.DestroyAfter(TimeSpan.FromSeconds(_totalTime));
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Position comes from the drift tween
            Owner.Position = _drift.GetValue();

            if (_radiationEffect)
            {
                float pulse = (float)Math.Sin(_timeLeft * 10) * 0.2f + 0.8f;
                _label.Opacity = pulse * Smooth(_totalTime - _timeLeft, _totalTime, 4f);
                _label.Scale = pulse;
            }
            else
            {
                _label.Opacity = Smooth(_totalTime - _timeLeft, _totalTime, 4f);
            }

            _timeLeft -= deltaTime;
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
