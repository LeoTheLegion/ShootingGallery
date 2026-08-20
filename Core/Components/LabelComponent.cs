using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Renders a Myra label that follows the owning entity. Owns the Canvas lifecycle:
    /// per-frame SetPosition/Update and CleanUp on detach.
    /// </summary>
    public class LabelComponent : EntityComponent
    {
        private readonly Canvas _canvas;
        private readonly ILabel _label;
        private readonly FontAsset _fontAsset;

        public LabelComponent(string text, string fontAssetName = "galleryFont")
        {
            _fontAsset = AssetManager.LoadAsset<FontAsset>(fontAssetName);
            _canvas = new Canvas();

            _label = WidgetFactory.CreateLabel(text);
            // Position label at canvas origin via IWidget.Position instead of alignment properties
            ((IWidget)_label).Position = Vector2.Zero;
            _canvas.AddWidget(_label);
        }

        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        public Color TextColor
        {
            get => _label.TextColor;
            set => _label.TextColor = value;
        }

        public float Scale
        {
            get => _label.Scale.X;
            set => _label.Scale = new Vector2(value);
        }

        public float Opacity
        {
            get => _label.Opacity;
            // Myra's Opacity setter throws on out-of-range values
            set => _label.Opacity = Math.Clamp(value, 0f, 1f);
        }

        public override void Update(GameTime gameTime)
        {
            _canvas.SetPosition(Owner.Position);
            _canvas.Update(gameTime);
        }

        public override void OnDetach()
        {
            _canvas.CleanUp();
            AssetManager.UnloadAsset<FontAsset>(_fontAsset.Name);
        }
    }
}
