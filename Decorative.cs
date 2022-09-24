using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ShootingGallery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery
{
    public class Decorative : WorldSpaceEntity
    {
        public Decorative(string assetName,Vector2 position) : base()
        {
            this._position = position;
            this._sprite = (Sprite)Resources.Load(assetName);
        }

        public override void Update(ref GameTime gameTime)
        {

        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(_sprite.GetTexture2D(), _position, Color.White);
        }

    }
}
