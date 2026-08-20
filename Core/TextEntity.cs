using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShootingGallery.Core;

/// <summary>
/// A positioned text label. All Canvas/Myra plumbing lives in LabelComponent.
/// </summary>
public class TextEntity : Entity
{
    private LabelComponent _label;
    private string _text;

    public TextEntity(Vector2 position, string text)
    {
        this._position = position;
        _text = text;
    }

    public override void OnStart()
    {
        base.OnStart();
        _label = AddComponent(new LabelComponent(_text));
    }

    public override void Render(SpriteBatch _spriteBatch)
    {
        base.Render(_spriteBatch);
        Debug.Primitives.DrawCircle(_spriteBatch, _position, 5, Color.Red);
    }

    public void SetText(string v)
    {
        _text = v;
        _label.Text = v;
    }

    public void SetColor(Color color) => _label.TextColor = color;

    public void SetScale(float scale) => _label.Scale = scale;
}
