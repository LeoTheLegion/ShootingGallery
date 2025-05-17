using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;
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
        private Label _label;

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

            var label = new Label
            {
                Text = _text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextColor = _textColor
            };

            _label = label;
            
            // Apply scale to font size and label scale
            if (_scale != 1.0f)
            {
                _label.Scale = new Vector2(_scale);
            }

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
                
                // Make the text pulse in size if it's a radiation effect
                float scalePulse = (float)Math.Sin(_timeLeft * 8) * 0.1f + 1.0f;
                _label.Scale = new Vector2(_scale * scalePulse, _scale * scalePulse);
            }
            else
            {
                this._transparency = smoothFunction(_totalTime - _timeLeft, _totalTime, 4f);
            }

            this._timeLeft -= deltaTime;

            _canvas.SetPosition(this._position);
            _canvas.Update(gameTime);

            _label.Opacity = _transparency;

            if (_timeLeft <= 0)
                this.Destroy();
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
