using System;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShootingGallery.Core;

public class ButtonEntity : Entity
{
    private Canvas _canvas;
    private Action onClick;
    private string text;

    public ButtonEntity(Vector2 position, string text, Action onClick)
    {
        this._position = position;
        _canvas = new Canvas();
        this.onClick = onClick;
        this.text = text;
    }

    public override void OnStart()
    {
        base.OnStart();

        var button = WidgetFactory.CreateTextButton(text);

        button.Clicked += (b) =>
        {
            onClick?.Invoke();
        };

        _canvas.AddWidget(button);
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Center the button by positioning the canvas directly at the specified position
        // No additional offset needed as the button has centered alignment
        _canvas.SetPosition(this._position);

        _canvas.Update(gameTime);
    }

    public override void Render(SpriteBatch _spriteBatch)
    {
        base.Render(_spriteBatch);

        Debug.Primitives.DrawCircle(_spriteBatch, _position, 5, Color.Red);
    }

    public override void OnDestroy()
    {
        _canvas.CleanUp();
    }
}
