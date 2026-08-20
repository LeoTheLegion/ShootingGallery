using System;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShootingGallery.Core;

/// <summary>
/// A positioned button. All Canvas/Myra plumbing lives in ButtonComponent.
/// </summary>
public class ButtonEntity : Entity
{
    public ButtonEntity(Vector2 position, string text, Action onClick)
    {
        this._position = position;

        var button = AddComponent(new ButtonComponent(text));
        button.Clicked += () => onClick?.Invoke();
    }

    public override void Render(SpriteBatch _spriteBatch)
    {
        base.Render(_spriteBatch);
        Debug.Primitives.DrawCircle(_spriteBatch, _position, 5, Color.Red);
    }
}
