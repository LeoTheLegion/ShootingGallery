using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShootingGallery.Core
{
    /// <summary>
    /// App-level spawner for floating popups. Owns the knowledge of what a popup entity is
    /// (the "popup" template in Content/popup.xml) so components never need to know about
    /// their own host entity — they just call <see cref="Spawn"/> with per-popup data.
    /// The caller passes its own <see cref="EntitySystem"/> (components get it from the
    /// <c>EntityComponent.EntitySystem</c> property added in CE 0.18.0).
    /// </summary>
    public static class PopupSpawner
    {
        private const string TemplateName = "Popup";
        private const string TemplateAsset = "popup.xml";

        // CE has no HasTemplate API; each scene gets a fresh EntitySystem, so track which
        // system instance already knows the template and (re)register when it changes.
        private static EntitySystem _registeredFor;

        /// <summary>Spawns a standard white popup at normal scale.</summary>
        public static void Spawn(EntitySystem es, Vector2 position, float time, string text)
            => Spawn(es, position, time, text, Color.White, 1.0f, false);

        /// <summary>Spawns a popup with explicit color, scale, and radiation pulse.</summary>
        public static void Spawn(EntitySystem es, Vector2 position, float time, string text,
            Color color, float scale, bool radiationEffect)
        {
            if (es == null)
                return;

            if (!ReferenceEquals(_registeredFor, es))
            {
                es.RegisterTemplate(TemplateName, TemplateAsset);
                _registeredFor = es;
            }

            var entity = es.Instantiate(TemplateName, position);
            var component = entity.GetComponent<FloatingPopUpComponent>();
            if (component != null)
            {
                component.Text = text;
                component.TextColor = color;
                component.Scale = scale;
                component.Duration = time;
                component.RadiationEffect = radiationEffect;
            }
        }
    }
}
