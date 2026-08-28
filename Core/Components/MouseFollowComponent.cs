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
        /// <summary>
        /// The offset subtracted from the mouse position. Exposed as a settable property so it
        /// can be configured from XML (<c>&lt;Property Name="Offset" Value="25,25" /&gt;</c>).
        /// </summary>
        public Vector2 Offset { get; set; }

        /// <summary>Parameterless constructor required for XML component creation.</summary>
        public MouseFollowComponent()
        {
        }

        public MouseFollowComponent(Vector2 offset)
        {
            Offset = offset;
        }

        public override void Update(GameTime gameTime)
        {
            Owner.Position = Mouse.GetState().Position.ToVector2() - Offset;
        }
    }
}
