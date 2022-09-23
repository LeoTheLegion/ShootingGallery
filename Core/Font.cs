using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery.Core
{
    public class Font : Asset
    {
        private string _assetName;
        private SpriteFont _spriteFont;
        public Font(string assetName)
        {
            _assetName = assetName;
        }

        public override void LoadContent(ContentManager _content)
        {
            _spriteFont = _content.Load<SpriteFont>(_assetName);
        }

        public SpriteFont GetSpriteFont() { return _spriteFont; }
    }
}
