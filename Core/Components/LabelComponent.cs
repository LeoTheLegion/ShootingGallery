using System;
using System.Collections.Generic;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Renders a Myra label that follows the owning entity, reusing a shared parent canvas.
    /// Unlike CE's built-in LabelComponent (which only applies its properties in OnAttach), this
    /// thin wrapper exposes live setters — Text / TextColor / Scale / Opacity write straight to the
    /// underlying widget — so HUD values that change every frame (score, timer, radiation) keep updating.
    /// It never owns a canvas: it borrows the nearest <see cref="CanvasComponent"/> in the entity
    /// hierarchy (via <see cref="CanvasComponent.RequireCanvas"/>) and adds its widget to that canvas.
    /// </summary>
    public class LabelComponent : EntityComponent, IConfigurableComponent
    {
        // Scene XML property names this component understands.
        internal const string TextProp = "Text";
        internal const string ColorProp = "Color";
        internal const string ScaleProp = "Scale";

        private ILabel _label;
        private CanvasComponent _canvasComponent;

        public LabelComponent() : this(string.Empty)
        {
        }

        public LabelComponent(string text)
        {
            _text = text;
        }

        // Backing fields so properties can be set before the component attaches; applied in OnAttach.
        private string _text;
        private Color _color = Color.White;
        private float _scale = 1f;
        private float _opacity = 1f;

        public string Text
        {
            get => _label != null ? _label.Text : _text;
            set
            {
                _text = value;
                if (_label != null)
                    _label.Text = value;
            }
        }

        public Color TextColor
        {
            get => _color;
            set
            {
                _color = value;
                if (_label != null)
                    _label.TextColor = value;
            }
        }

        public float Scale
        {
            get => _scale;
            // Myra's Scale setter is a Vector2; keep a uniform scalar for callers.
            set
            {
                _scale = value;
                if (_label != null)
                    _label.Scale = new Vector2(value);
            }
        }

        public float Opacity
        {
            get => _opacity;
            // Myra's Opacity setter throws on out-of-range values
            set
            {
                _opacity = Math.Clamp(value, 0f, 1f);
                if (_label != null)
                    _label.Opacity = _opacity;
            }
        }

        /// <summary>Applies scene XML properties (Text/Color/Scale) before the component attaches.</summary>
        public void Configure(Dictionary<string, string> props)
        {
            if (props == null)
                return;

            if (props.TryGetValue(TextProp, out var text)) _text = text;
            if (props.TryGetValue(ColorProp, out var color)) _color = PropParsers.ParseColor(color);
            if (props.TryGetValue(ScaleProp, out var scale)) _scale = PropParsers.ParseFloat(scale);
        }

        public override void OnAttach()
        {
            // Resolve the nearest canvas in this entity's hierarchy (this entity or an ancestor).
            _canvasComponent = CanvasComponent.RequireCanvas(Owner);

            _label = WidgetFactory.CreateLabel(_text);
            _label.TextColor = _color;
            _label.Scale = new Vector2(_scale);
            _label.Opacity = _opacity;

            _canvasComponent.Canvas.AddWidget(_label);
        }

        public override void OnDetach()
        {
            if (_label != null && _canvasComponent != null)
                _canvasComponent.Canvas.RemoveWidget(_label);

            _label = null;
            _canvasComponent = null;
        }

        public override void Update(GameTime gameTime)
        {
            if (_label == null || _canvasComponent == null || Owner == null)
                return;

            // Keep the label at the entity's position relative to the canvas entity, mirroring CE.
            var canvasEntity = _canvasComponent.Owner;
            if (canvasEntity != null)
                _label.Position = Owner.Position - canvasEntity.Position;
        }
    }
}
