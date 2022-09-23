using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery.Core
{
    public class Sprite : Asset
    {
        private Texture2D _texture2d;
        private string _assetName;

        public Sprite(string assetName)
        {
            _assetName = assetName;
        }

        public override void LoadContent(ContentManager _content)
        {
            _texture2d = _content.Load<Texture2D>(_assetName);
        }

        public Texture2D GetTexture2D() { return _texture2d; }
    }
}
