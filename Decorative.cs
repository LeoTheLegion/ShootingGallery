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
        private string _assetName;
        public Decorative(string assetName,Vector2 position) : base()
        {
            this._assetName = assetName;
            this._position = position; 
        }

        public override void LoadContent(ContentManager content)
        {
            _sprite = content.Load<Texture2D>(_assetName);
        }

        public override void Update(ref GameTime gameTime)
        {

        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            _spriteBatch.Draw(_sprite, _position, Color.White);
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
