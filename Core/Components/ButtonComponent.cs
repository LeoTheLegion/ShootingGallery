using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Renders a Myra text button that follows the owning entity. Owns the Canvas lifecycle:
    /// per-frame SetPosition/Update and CleanUp on detach.
    /// </summary>
    public class ButtonComponent : EntityComponent
    {
        public event Action Clicked;

        private readonly Canvas _canvas;

        public ButtonComponent(string text)
        {
            _canvas = new Canvas();

            var button = WidgetFactory.CreateTextButton(text);
            button.Clicked += (b) => Clicked?.Invoke();
            _canvas.AddWidget(button);
        }

        public override void Update(GameTime gameTime)
        {
            _canvas.SetPosition(Owner.Position);
            _canvas.Update(gameTime);
        }

        public override void OnDetach()
        {
            _canvas.CleanUp();
        }
    }
}
