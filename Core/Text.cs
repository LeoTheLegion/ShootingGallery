using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery.Core
{
    public class Text : UI
    {
        protected Font _gameFont;
        protected string _assetName;
        protected string _text;
        protected Color _color;

        public Text(string assetName, string text, Vector2 position)
        {
            this._assetName = assetName;
            this._position = position;
            this._text = text;
            this._gameFont = (Font)Resources.Load(assetName);
            this._color = Color.White;
        }

        public override void Update(ref GameTime gameTime)
        {

        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            _spriteBatch.DrawString(_gameFont.GetSpriteFont(), _text, _position, _color);
        }

        public virtual void SetText(string x) => this._text = x;


    }
}
