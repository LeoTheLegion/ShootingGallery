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
    public class Target : Entity
    {
        private const int targetRadius = 45;

        public Target(Vector2 targetPosition)
        {
            this._position = targetPosition;
        }

        public override void LoadContent(ContentManager content)
        {
            _sprite = content.Load<Texture2D>("target");
        }            

        public override void Update(ref GameTime gameTime)
        {
            var mState = Mouse.GetState();

            if (mState.LeftButton == ButtonState.Pressed)
            {
                float mouseTargetDist = Vector2.Distance(_position, mState.Position.ToVector2());

                if (mouseTargetDist < targetRadius)
                {
                    GameManager.AddScore(1);

                    Random rand = new Random();

                    _position.X = rand.Next(targetRadius, Game1.WIDTH - targetRadius);
                    _position.Y = rand.Next(targetRadius, Game1.HEIGHT - targetRadius);
                }
            }
        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(_sprite,
                _position - new Vector2(targetRadius, targetRadius)
                , Color.White);
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
