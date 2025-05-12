using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
namespace ShootingGallery
{
    public class Crosshair : Entity, IDisposable
    {
        private const int crosshairRadius = 25;

        private Sprite _sprite;

        public Crosshair() : base()
        {
            this._sprite = AssetManager.LoadAsset<Sprite>("crosshair_sprite.xml");
        }

        public override void Update(GameTime gameTime)
        {
            // Update the position of the crosshair to follow the mouse
            Vector2 mousePosition = Mouse.GetState().Position.ToVector2();
            this._position = mousePosition - new Vector2(crosshairRadius, crosshairRadius);
        }

        public override void Render(SpriteBatch _spriteBatch)
        {
             _sprite.Draw(_spriteBatch, _position, Color.White, 0f, SpriteEffects.None, 0f);
        }

        public void Dispose()
        {
            AssetManager.UnloadAsset<Sprite>(_sprite.Name);
        }

    }
}
