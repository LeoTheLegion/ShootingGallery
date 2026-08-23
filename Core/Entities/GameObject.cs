using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace ShootingGallery.Core;

/// <summary>
/// A generic, data-driven container entity. It carries no gameplay logic of its own;
/// instead it lets a scene XML declare reusable structural entities — most importantly a
/// screen-space UI canvas root (via <see cref="CanvasComponent"/>) that other GUI entities
/// can be parented under. This mirrors Unity's "GameObject + Canvas" pattern and avoids
/// creating one-off entity classes just to hold a particular component combination.
/// </summary>
public class GameObject : Entity
{
    private readonly bool _isScreenSpace;

    /// <param name="isScreenSpace">True (default) for a screen-space HUD canvas; false for world space.</param>
    public GameObject(bool isScreenSpace = true)
    {
        _isScreenSpace = isScreenSpace;
    }

    public override void OnStart()
    {
        base.OnStart();
        // Attach the canvas as soon as this entity starts so that GUI entities parented under it
        // can resolve this canvas when their own components attach.
        AddComponent(new CanvasComponent(_isScreenSpace));
    }
}
