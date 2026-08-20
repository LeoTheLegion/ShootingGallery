using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Raises <see cref="Clicked"/> on the left-mouse-button press edge (release -> press),
    /// with the click position in screen space.
    /// </summary>
    public class MouseClickComponent : EntityComponent
    {
        public event Action<Vector2> Clicked;

        private MouseState _lastMouseState = Mouse.GetState();

        public override void Update(GameTime gameTime)
        {
            MouseState current = Mouse.GetState();
            if (current.LeftButton == ButtonState.Pressed &&
                _lastMouseState.LeftButton == ButtonState.Released)
            {
                Clicked?.Invoke(current.Position.ToVector2());
            }

            _lastMouseState = current;
        }
    }
}
