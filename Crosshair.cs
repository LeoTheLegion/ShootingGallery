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
    public class Crosshair : WorldSpaceEntity
    {
        private const int crosshairRadius = 25;

        public Crosshair() : base()
        {
            this._sprite = (Sprite)Resources.Load("crosshairs");
        }

        public override void Update(ref GameTime gameTime)
        {

        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(_sprite.GetTexture2D(),
                Mouse.GetState().Position.ToVector2() - new Vector2(crosshairRadius, crosshairRadius),
                Color.White);
        }

    }
}
