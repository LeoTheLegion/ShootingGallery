using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery.Core
{
    public class Button : UI
    {
        private Vector2 _hoverOffset;
        private Sprite _normal;
        private Sprite _hover;
        private Font _font;
        private string _text;
        private Rectangle _bounds
        {
            get
            {
                return _hover.GetTexture2D().Bounds;
            }
        }
        private bool _ishovering;

        public delegate void onPress();
        private onPress _onPress;

        public Button(string normal, string hover, string font, Vector2 pos, string text) : base()
        {
            this._normal = (Sprite)Resources.Load(normal);
            this._hover = (Sprite)Resources.Load(hover);
            this._font = (Font)Resources.Load(font);
            this._text = text;
            this._position = pos;
            this._ishovering = false;
            this._hoverOffset = new Vector2(0, 4);
        }

        public Button SetOnPress(onPress d){
            this._onPress = d;
            return this; 
        }

        public override void Update(ref GameTime gameTime)
        {
            //throw new NotImplementedException();
            var mState = Mouse.GetState();

            var rect_position = _bounds;
            rect_position.Offset(this._position);

            this._ishovering = rect_position.Contains(mState.Position);

            if(this._ishovering && mState.LeftButton == ButtonState.Pressed)
            {
                _onPress?.Invoke();
            }
        }

        public override void Render(ref SpriteBatch _spriteBatch)
        {
            Vector2 text_midPoint = _font.GetSpriteFont().MeasureString(this._text)/2;

            Rectangle bounds = this._bounds;

            Vector2 button_midPoint = new Vector2(bounds.Width/2, bounds.Height/2);

            if (this._ishovering)
            {
                _spriteBatch.Draw(_hover.GetTexture2D(), this._position, bounds, Color.White);

                Vector2 text_offset = button_midPoint - text_midPoint + _hoverOffset;

                _spriteBatch.DrawString(_font.GetSpriteFont(), _text, this._position + text_offset, Color.White);
            }
                
            else
            {
                _spriteBatch.Draw(_normal.GetTexture2D(), this._position, bounds, Color.White);

                Vector2 text_offset = button_midPoint - text_midPoint;

                _spriteBatch.DrawString(_font.GetSpriteFont(), _text, this._position + text_offset, Color.White);
            }
        }
    }
}
