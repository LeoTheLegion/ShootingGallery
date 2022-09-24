using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShootingGallery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery
{
    public class FloatingPopUpText : Text
    {
        private Vector2 _velocity;
        private float _timeLeft, _totalTime;
        private float _transparencyChangeRate;
        private float _transparency;

        public FloatingPopUpText(string assetName, string text, Vector2 position, Vector2 end, float time) : base(assetName, text, position)
        {
            this._velocity = (end - position) / time;
            this._timeLeft = this._totalTime = time;
            this._transparency = 1f;
            this._transparencyChangeRate = this._transparency / time;
            
        }

        public override void Update(ref GameTime gameTime) 
        {
            base.Update(ref gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            this._position += _velocity * deltaTime;

            this._transparency = smoothFunction(_totalTime - _timeLeft, _totalTime, 4f);

            this._timeLeft -= deltaTime;

            if (_timeLeft <= 0)
                this.Destroy();
        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            _spriteBatch.DrawString(_gameFont.GetSpriteFont(), _text, _position, _color * _transparency);
        }

        private float smoothFunction(float currentTime , float totalTime, float strength)
        {
            double num = Math.Pow(-totalTime + 2 * currentTime, strength);
            double dom = Math.Pow(totalTime, strength);
            return (float)(-(num / dom) + 1);
        }
    }
}
