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
}
