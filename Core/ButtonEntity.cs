using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GUI;
using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;

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

        var button = Button.CreateTextButton(text);

        button.Click += (s, e) =>
        {
            onClick?.Invoke();
        };

        _canvas.AddWidget(button);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _canvas.SetPosition(this._position);

        _canvas.Update(gameTime);
    }

    public override void OnDestroy()
    {
        _canvas.CleanUp();
    }
}
