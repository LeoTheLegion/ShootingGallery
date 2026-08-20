using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShootingGallery.Core;

namespace ShootingGallery
{
    /// <summary>
    /// Drifting, fading score popup. All Canvas/tween/fade plumbing lives in FloatingTextComponent.
    /// </summary>
    public class FloatingPopUpText : Entity
    {
        // Standard floating text
        public FloatingPopUpText(Vector2 position, float time, string text)
            : this(position, time, text, Color.White, 1.0f)
        {
        }

        // Floating text with custom color and scale
        public FloatingPopUpText(Vector2 position, float time, string text, Color color, float scale = 1.0f)
        {
            this._position = position;

            // Set radiation effect if it's green
            bool isRadiation = color.G > 200 && color.R < 100 && color.B < 100;

            AddComponent(new FloatingTextComponent(text, time, color, scale, isRadiation));
        }

        // Floating text with radiation effect
        public FloatingPopUpText(Vector2 position, float time, string text, bool isRadiationEffect)
            : this(position, time, text, isRadiationEffect ? new Color(0, 255, 0) : Color.White)
        {
        }
    }
}
