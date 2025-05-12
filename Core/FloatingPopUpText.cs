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

        private Canvas _canvas;

        private const float Distance = 10f;
        private string _text;

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

        public override void OnStart()
        {
            base.OnStart();

            var label = new Label
            {
                Text = _text,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            _canvas.AddWidget(label);
        }

        public override void Update(GameTime gameTime) 
        {

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            this._position += _velocity * deltaTime;

            this._transparency = smoothFunction(_totalTime - _timeLeft, _totalTime, 4f);

            this._timeLeft -= deltaTime;

            if (_timeLeft <= 0)
                this.Destroy();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _canvas.CleanUp();
        }

        public override void Render(SpriteBatch _spriteBatch)
        {
        }

        private float smoothFunction(float currentTime , float totalTime, float strength)
        {
            double num = Math.Pow(-totalTime + 2 * currentTime, strength);
            double dom = Math.Pow(totalTime, strength);
            return (float)(-(num / dom) + 1);
        }
    }
}
