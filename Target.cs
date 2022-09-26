using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShootingGallery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery
{
    public class Target : WorldSpaceEntity
    {
        private const int targetRadius = 45;
        private const float _DEFAULTSCALE = .3f;
        private float _scale;
        private const double _TIMETOFULLSIZE = 3.0;
        private double _time;

        private Random rand;

        public Target(Vector2 targetPosition) : base()
        {
            this._position = targetPosition;
            this._sprite = (Sprite)Resources.Load("target");
            this._scale = _DEFAULTSCALE;
            this._time = 0f;
            this.rand = new Random();
        }

        public override void Update(ref GameTime gameTime)
        {
            var mState = Mouse.GetState();

            if (mState.LeftButton == ButtonState.Pressed)
            {
                float mouseTargetDist = Vector2.Distance(_position, mState.Position.ToVector2());

                if (mouseTargetDist < targetRadius * _scale)
                {
                    int score = CalculateScore();

                    ReportScore(score);
                    MoveRandomly();
                    Reset();
                }
            }

            _time += gameTime.ElapsedGameTime.TotalSeconds;

            _scale = (float)Math.MinMagnitude(_time / _TIMETOFULLSIZE,1);
        }

        private int CalculateScore()
        {
            int score;

            if (_scale < .4f)
                score = 10;
            else if (_scale < 0.8f)
                score = 5;
            else
                score = 1;
            return score;
        }

        private void ReportScore(int score)
        {
            GameManager.AddScore(score);

            new FloatingPopUpText(
                "galleryFont",
                score.ToString(),
                this._position,
                this._position + new Vector2(0, -20),
                2f
                );
        }

        private void MoveRandomly()
        {
            _position.X = rand.Next(targetRadius, Game1.WIDTH - targetRadius);
            _position.Y = rand.Next(targetRadius, Game1.HEIGHT - targetRadius);
        }
        private void Reset()
        {
            _scale = _DEFAULTSCALE;
            _time = 0;
        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(_sprite.GetTexture2D(),
                _position - (new Vector2(targetRadius, targetRadius)*_scale),
                null,
                Color.White,
                0,
                new Vector2(0, 0),
                _scale,
                SpriteEffects.None,
                0f
                );
            ;
        }
    }
}
