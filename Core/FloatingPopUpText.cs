using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ShootingGallery
{
    public class FloatingPopUpText : Entity
    {
        private Vector2 _velocity;
        private float _timeLeft, _totalTime;
        private float _transparencyChangeRate;
        private float _transparency;
        private float _scale = 1.0f;
        private bool _isRadiationEffect = false;
        private Color _textColor = Color.White;

        private Canvas _canvas;
        private ILabel _label;

        private const float Distance = 10f;
        private string _text;

        // Standard floating text
        public FloatingPopUpText(Vector2 position, float time, string text)
        {
            this._position = position;
            this._text = text;

            Vector2 end = position + new Vector2(0, -Distance);
            this._velocity = (end - position) / time;
            this._timeLeft = this._totalTime = time;

            this._transparency = 1f;
            this._transparencyChangeRate = this._transparency / time;
            
            _canvas = new Canvas();

            // v0.14.0: entity system handles lifetime instead of manual countdown
            DestroyAfter(TimeSpan.FromSeconds(time));
        }
        
        // Floating text with custom color and scale
        public FloatingPopUpText(Vector2 position, float time, string text, Color color, float scale = 1.0f) 
            : this(position, time, text)
        {
            this._textColor = color;
            this._scale = scale;
            
            // Set radiation effect if it's green
            if (color.G > 200 && color.R < 100 && color.B < 100)
            {
                this._isRadiationEffect = true;
            }
        }
        
        // Floating text with radiation effect
        public FloatingPopUpText(Vector2 position, float time, string text, bool isRadiationEffect) 
            : this(position, time, text)
        {
            this._isRadiationEffect = isRadiationEffect;
            if (isRadiationEffect)
            {
                this._textColor = new Color(0, 255, 0);
            }
        }

        public override void OnStart()
        {
            base.OnStart();

            var label = WidgetFactory.CreateLabel(_text);
            label.TextColor = _textColor;
            // Workaround: position label at canvas origin via IWidget.Position instead of alignment properties
            ((IWidget)label).Position = Vector2.Zero;

            _label = label;

            _canvas.AddWidget(label);
        }

        public override void Update(GameTime gameTime) 
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            this._position += _velocity * deltaTime;

            // Apply standard fade or pulsing effect for radiation
            if (_isRadiationEffect)
            {
                float pulse = (float)Math.Sin(_timeLeft * 10) * 0.2f + 0.8f;
                this._transparency = pulse * smoothFunction(_totalTime - _timeLeft, _totalTime, 4f);

                _label.Scale = new Vector2(pulse);
            }
            else
            {
                this._transparency = smoothFunction(_totalTime - _timeLeft, _totalTime, 4f);
            }

            this._timeLeft -= deltaTime;

            _label.Opacity = _transparency;
            _canvas.SetPosition(this._position);
            _canvas.Update(gameTime);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _canvas.CleanUp();
        }

        private float smoothFunction(float currentTime , float totalTime, float strength)
        {
            double num = Math.Pow(-totalTime + 2 * currentTime, strength);
            double dom = Math.Pow(totalTime, strength);
            return (float)(-(num / dom) + 1);
        }
    }
}
