using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Raises <see cref="Clicked"/> on the left-mouse-button press edge, with the click
    /// position in screen space. Rides on CE's Input system (updated by MainGame each frame),
    /// so no per-frame MouseState polling is needed here.
    /// </summary>
    public class MouseClickComponent : EntityComponent
    {
        public event Action<Vector2> Clicked;

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButton.Left)
                Clicked?.Invoke(e.Position);
        }

        public override void OnAttach()
        {
            base.OnAttach();
            Input.Mouse.MouseDown += OnMouseDown;
        }

        public override void OnDetach()
        {
            Input.Mouse.MouseDown -= OnMouseDown;
            base.OnDetach();
        }
    }
}
