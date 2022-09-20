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
    internal class Text : UI
    {
        protected SpriteFont _gameFont;
        protected string _assetName;
        protected string _text;

        public Text(string assetName, string text, Vector2 position)
        {
            this._assetName = assetName;
            this._position = position;
            this._text = text;
        }

        public override void LoadContent(ContentManager content)
        {
            this._gameFont = content.Load<SpriteFont>(this._assetName);
        }

        public override void Update(ref GameTime gameTime)
        {

        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            _spriteBatch.DrawString(_gameFont, _text, _position, Color.White);
        }

        public virtual void SetText(string x) => this._text = x;

        public override void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
