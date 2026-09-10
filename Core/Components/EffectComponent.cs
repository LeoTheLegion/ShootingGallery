using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core
{
    /// <summary>
    /// A short-lived world-space visual effect (hit flash, bomb detonation). The entity is a plain
    /// GameObjectEntity declared in an effect template (Content/Prefabs/*_effect.xml) carrying a single
    /// SpriteComponent; this component drives it: scale eases from StartScale to EndScale while the
    /// alpha fades to zero over Duration, then the entity destroys itself.
    ///
    /// Spawn sites instantiate the prefab and poke StartScale/EndScale/Tint/Duration before the first
    /// Update (the deferred setup in EnsureInitialized always sees them), exactly like the popup pattern.
    /// </summary>
    public class EffectComponent : EntityComponent
    {
        // Per-effect values (poked by each spawn site right after InstantiatePrefab, before first Update).
        public float StartScale { get; set; } = 0.5f;
        public float EndScale { get; set; } = 1.4f;
        public Color Tint { get; set; } = Color.White;
        public float Duration { get; set; } = 0.3f;

        private SpriteComponent _sprite;
        private float _elapsed;
        private bool _initialized;

        public override void Update(GameTime gameTime)
        {
            if (Owner == null || Owner.Destroyed)
                return;

            EnsureInitialized();
            _elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

            float t = Duration <= 0f ? 1f : Math.Clamp(_elapsed / Duration, 0f, 1f);
            // Ease-out: fast at the start of the burst, settling as it fades.
            float eased = 1f - (float)Math.Pow(1f - t, 2f);

            float scale = StartScale + (EndScale - StartScale) * eased;
            Owner.Scale = new Vector2(scale);

            byte alpha = (byte)Math.Round(Math.Clamp(1f - eased, 0f, 1f) * 255f);
            _sprite.Color = new Color(Tint.R, Tint.G, Tint.B, alpha);
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;
            _initialized = true;

            // The SpriteComponent is declared before this one in the prefab, so it's attached by now.
            _sprite = Owner.GetComponent<SpriteComponent>();
            if (_sprite == null)
            {
                // No sprite to drive — just let the lifetime expire.
                Owner.DestroyAfter(TimeSpan.FromSeconds(Duration));
                return;
            }

            Owner.Scale = new Vector2(StartScale);
            _sprite.Color = Tint;

            // Entity system owns the lifetime (mirrors FloatingPopUpComponent).
            Owner.DestroyAfter(TimeSpan.FromSeconds(Duration));
        }
    }
}
