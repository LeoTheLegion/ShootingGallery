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
    public class Crosshair : Entity
    {
        private Texture2D crosshairsSprite;
        private const int crosshairRadius = 25;

        public override void LoadContent(ContentManager content)
        {
            crosshairsSprite = content.Load<Texture2D>("crosshairs");
        }
        public override void Update(ref GameTime gameTime)
        {

        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(crosshairsSprite,
                Mouse.GetState().Position.ToVector2() - new Vector2(crosshairRadius, crosshairRadius),
                Color.White);
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
