using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Makes the owning entity follow the mouse cursor, with an optional offset.
    /// Driven automatically by the entity's base Update (component auto-iteration).
    /// </summary>
    public class MouseFollowComponent : EntityComponent
    {
        private readonly Vector2 _offset;

        public MouseFollowComponent(Vector2 offset = default)
        {
            _offset = offset;
        }

        public override void Update(GameTime gameTime)
        {
            Owner.Position = Mouse.GetState().Position.ToVector2() - _offset;
        }
    }

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
