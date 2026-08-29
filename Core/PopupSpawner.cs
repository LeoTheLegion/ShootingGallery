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

        /// <summary>
        /// Instantiates the popup prefab and applies per-instance overrides — Unity-style:
        /// baseline values live in Content/popup.xml, callers only pass what differs.
        /// Unspecified parameters keep the template's prefab defaults.
        /// </summary>
        public static void Spawn(EntitySystem es, Vector2 position, string text = null,
            float? duration = null, Color? color = null, float? scale = null, bool? radiationEffect = null)
        {
            if (es == null)
                return;

            if (!ReferenceEquals(_registeredFor, es))
            {
                es.RegisterTemplate(TemplateName, TemplateAsset);
                _registeredFor = es;
            }

            var component = es.Instantiate(TemplateName, position).GetComponent<FloatingPopUpComponent>();
            if (component == null)
                return;

            // Only override what the caller specified — everything else stays at the prefab default.
            if (text != null) component.Text = text;
            if (duration.HasValue) component.Duration = duration.Value;
            if (color.HasValue) component.TextColor = color.Value;
            if (scale.HasValue) component.Scale = scale.Value;
            if (radiationEffect.HasValue) component.RadiationEffect = radiationEffect.Value;
        }
    }
}
