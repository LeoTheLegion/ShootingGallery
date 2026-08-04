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
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.TextColor = _textColor;

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
                
                // Note: ILabel doesn't expose Scale in v0.13.1 — visual pulse effect removed
            }
            else
            {
                this._transparency = smoothFunction(_totalTime - _timeLeft, _totalTime, 4f);
            }

            this._timeLeft -= deltaTime;

            // Note: ILabel doesn't expose Opacity in v0.13.1 — transparency fade removed.
            // Canvas.Visible can be used as a coarse alternative for hide/show.
            _canvas.SetPosition(this._position);
            _canvas.Update(gameTime);

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
