using System;
using System.Collections.Generic;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Utils;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

/// <summary>
/// The player's crosshair as a component: mouse-following, click-to-shoot, and mutated
/// arms (child entities). Rendering is handled by the entity's own SpriteComponent declared
/// in the scene XML; this component only owns the arm sprites.
/// </summary>
public class CrosshairComponent : EntityComponent
{
    /// <summary>Raised when a shot is fired (player click or random mutated-arm shot).</summary>
    public event EventHandler<ShootEventArgs> OnShoot;

    public class ShootEventArgs : EventArgs
    {
        public Vector2 Position { get; }
        public bool IsRandomShot { get; }
        public int? ArmIndex { get; }

        public ShootEventArgs(Vector2 position, bool isRandomShot = false, int? armIndex = null)
        {
            Position = position;
            IsRandomShot = isRandomShot;
            ArmIndex = armIndex;
        }
    }

    // Glowing green + 0.8 scale matches the previous hand-drawn arm render.
    private static readonly Color ArmColor = new Color(0, 255, 0, 200);
    private const float ArmScale = 0.8f;
    private const float ArmDistanceFromCenter = 40f;

    private int _mutationLevel;
    private readonly List<Entity> _arms = new();
    private Sprite _armSprite;

    /// <summary>The current number of extra (mutated) arms.</summary>
    public int MutationLevel => _mutationLevel;

    public override void OnAttach()
    {
        // Own the main crosshair sprite: scene XML can't assign a Sprite object property, so
        // the component creates its own SpriteComponent. This entity is a plain GameObjectEntity.
        var mainSprite = AssetManager.LoadAsset<Sprite>("crosshair_sprite.xml");
        var spriteComponent = Owner.GetComponent<SpriteComponent>();
        if (spriteComponent == null)
            Owner.AddComponent(new SpriteComponent(mainSprite));
        else if (spriteComponent.Sprite == null)
            spriteComponent.Sprite = mainSprite;

        Owner.RegisterForInstancedRendering(mainSprite);
        Owner.SetZLayer(10);

        // Fire OnShoot on a left-mouse click at the cursor position. MouseClickComponent must be
        // declared before this component in the scene XML so it exists by the time we attach.
        var click = Owner.GetComponent<MouseClickComponent>();
        if (click != null)
            click.Clicked += position => OnShoot?.Invoke(this, new ShootEventArgs(position));
    }

    /// <summary>Rebuilds the mutated arms around the crosshair for the given level (0-3).</summary>
    public void SetMutationLevel(int level)
    {
        _mutationLevel = Math.Clamp(level, 0, 3);

        // Remove existing arms (destroyed entities are cleaned up by the system next frame)
        foreach (var arm in _arms)
            arm.Destroy();
        _arms.Clear();

        var es = GameRefs.EntitySystem;
        if (es == null || _mutationLevel <= 0)
            return;

        // Distribute arms evenly around the main crosshair as child entities, so they
        // inherit its position and follow it for free.
        for (int i = 0; i < _mutationLevel; i++)
        {
            float angle = (float)(i * (2 * Math.PI / _mutationLevel));
            Vector2 offset = new Vector2(
                (float)Math.Cos(angle) * ArmDistanceFromCenter,
                (float)Math.Sin(angle) * ArmDistanceFromCenter
            );

            var arm = es.CreateEntity<GameObjectEntity>();
            arm.LocalPosition = offset;
            arm.LocalRotation = angle;
            SetupArmSprite(arm);
            Owner.AddChild(arm);
            _arms.Add(arm);
        }
    }

    /// <summary>Triggers a random shot aimed at a fully-grown standard target.</summary>
    public void TriggerRandomShot()
    {
        if (_mutationLevel <= 0 || _arms.Count == 0)
            return;

        int randomArm = GameRandom.Next(1, _arms.Count + 1); // 1..N
        Vector2 shotPosition = GetRandomShotPosition();

        if (shotPosition.X >= 0 && shotPosition.Y >= 0)
        {
            OnShoot?.Invoke(this, new ShootEventArgs(shotPosition, true, randomArm));
        }
        else
        {
            Console.WriteLine("No fully grown standard targets available for random shot - skipping");
        }
    }

    /// <summary>Finds a random fully-grown standard target; (-1,-1) if none exist.</summary>
    private static Vector2 GetRandomShotPosition()
    {
        var es = GameRefs.EntitySystem;
        if (es == null)
            return new Vector2(-1, -1);

        var candidates = new List<Entity>();
        foreach (var entity in es.GetEntitiesByTag("Target"))
        {
            var target = entity.GetComponent<TargetComponent>();
            if (target != null && !target.IsDestroyed &&
                target.Type == TargetComponent.TargetType.Standard && target.IsFullyGrown)
            {
                candidates.Add(entity);
            }
        }

        if (candidates.Count > 0)
            return candidates[GameRandom.Next(candidates.Count)].Position;

        return new Vector2(-1, -1);
    }

    private void SetupArmSprite(Entity arm)
    {
        if (_armSprite == null)
            _armSprite = AssetManager.LoadAsset<Sprite>("crosshair_sprite.xml");

        var spriteComponent = arm.AddComponent(new SpriteComponent(_armSprite));
        spriteComponent.Color = ArmColor;
        arm.Scale = new Vector2(ArmScale);
        arm.RegisterForInstancedRendering(_armSprite);
        arm.SetZLayer(10);
    }

    public override void OnDetach()
    {
        foreach (var arm in _arms)
            arm.Destroy();
        _arms.Clear();

        if (_armSprite != null)
        {
            AssetManager.UnloadAsset<Sprite>(_armSprite.Name);
            _armSprite = null;
        }
    }
}
