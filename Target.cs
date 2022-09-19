using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        private static Texture2D targetSprite;

        private Vector2 targetPosition;
        private const int targetRadius = 45;

        public Target(Vector2 targetPosition)
        {
            this.targetPosition = targetPosition;
        }

        public override void LoadContent(ContentManager content)
        {
            if(targetSprite == null)
                targetSprite = content.Load<Texture2D>("target");
        }            

        public override void Update(ref GameTime gameTime)
        {
            var mState = Mouse.GetState();

            if (mState.LeftButton == ButtonState.Pressed)
            {
                float mouseTargetDist = Vector2.Distance(targetPosition, mState.Position.ToVector2());

                if (mouseTargetDist < targetRadius)
                {
                    GameManager.AddScore(1);

                    Random rand = new Random();

                    targetPosition.X = rand.Next(targetRadius, Game1.WIDTH - targetRadius);
                    targetPosition.Y = rand.Next(targetRadius, Game1.HEIGHT - targetRadius);
                }
            }
        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(targetSprite,
                targetPosition - new Vector2(targetRadius, targetRadius)
                , Color.White);
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
