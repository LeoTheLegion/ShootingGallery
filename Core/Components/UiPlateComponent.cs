using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core
{
    /// <summary>
    /// A static world-space UI back-plate (button plate, HUD panel frame). The entity is a plain
    /// GameObjectEntity declared in a scene carrying a single SpriteComponent (which loads its own
    /// sprite from SpriteAsset); this component registers that sprite for instanced rendering and
    /// pins it to a z-layer so it sorts deterministically: above the unregistered background pass,
    /// below the GUI text overlay. Mirrors how targets (z 0) / crosshair (z 10) / effects (z 20)
    /// render — see EffectComponent for the same registration pattern.
    /// </summary>
    public class UiPlateComponent : EntityComponent
    {
        /// <summary>Z-layer for the plate. Default 1 sits just above the background pass.</summary>
        public int ZLayer { get; set; } = 1;

        /// <summary>Optional tint applied to the plate sprite (default white = no tint).</summary>
        public Color Tint { get; set; } = Color.White;

        /// <summary>Uniform scale for the plate sprite. Default 1 (native size). Not an entity-level
        /// &lt;Scale&gt; element (the scene parser doesn't accept one) — driven here instead.</summary>
        public float Scale { get; set; } = 1f;

        private bool _initialized;

        public override void Update(GameTime gameTime)
        {
            if (Owner == null || Owner.Destroyed)
                return;

            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;
            _initialized = true;

            // The SpriteComponent is declared before this one in the scene, so it's attached by now.
            var sprite = Owner.GetComponent<SpriteComponent>();
            if (sprite == null || sprite.Sprite == null)
                return;

            // Without a registered batch texture the entity system routes this to the back-most
            // (no-texture) pass — the same pass as the background, where relative order is not
            // guaranteed. Registering + pinning a z-layer puts the plate in the front z-layer pass,
            // deterministically above the background and below the GUI text overlay.
            Owner.RegisterForInstancedRendering(sprite.Sprite);
            Owner.SetZLayer(ZLayer);

            Owner.Scale = new Vector2(Scale);
            sprite.Color = Tint;
        }
    }
}
