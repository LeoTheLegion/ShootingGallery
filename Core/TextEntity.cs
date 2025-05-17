using System;
using CoreEssentials.Assets;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.UI;

namespace ShootingGallery.Core;

public class TextEntity: Entity
{
    private Canvas _canvas;
    private FontAsset _fontAsset;
    private string _text;
    private Color _textColor = Color.White;
    private float _scale = 1.0f;

    private Label _label;

    public TextEntity(Vector2 position, string text)
    {
        this._position = position;
        _canvas = new Canvas();

        _fontAsset = AssetManager.LoadAsset<FontAsset>("galleryFont");
        _text = text;
    }    
    public override void OnStart()
    {
        base.OnStart();

        var label = new Label
        {
            Text = _text,
            TextColor = _textColor
        };

        _label = label;
        UpdateScale();
        
        _canvas.AddWidget(label);
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

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
        AssetManager.UnloadAsset<FontAsset>(_fontAsset.Name);
    }

    public void SetText(string v)
    {
        _text = v;
        _label.Text = v;
    }
    
    public void SetColor(Color color)
    {
        _textColor = color;
        if (_label != null)
        {
            _label.TextColor = color;
        }
    }
    
    public void SetScale(float scale)
    {
        _scale = scale;
        UpdateScale();
    }
    
    private void UpdateScale()
    {
        if (_label != null)
        {
            _label.Scale = new Vector2(_scale);
        }
    }
}
