using System;
using System.Collections.Generic;
using System.Linq;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShootingGallery.Core;
namespace ShootingGallery
{
    /// <summary>
    /// A mutated crosshair arm. Rendered as a child entity of the <see cref="Crosshair"/> so it
    /// inherits the crosshair's position (world = parent + local offset) and follows it for free.
    /// </summary>
    public class CrosshairArm : Entity
    {
        private Sprite _sprite;

        // Glowing green + 0.8 scale matches the previous hand-drawn arm render.
        private static readonly Color ArmColor = new Color(0, 255, 0, 200);
        private const float ArmScale = 0.8f;

        public override void OnStart()
        {
            base.OnStart();

            _sprite = AssetManager.LoadAsset<Sprite>("crosshair_sprite.xml");
            var spriteComponent = AddComponent(new SpriteComponent(_sprite));
            spriteComponent.Color = ArmColor;
            Scale = new Vector2(ArmScale);
            RegisterForInstancedRendering(_sprite);
            SetZLayer(10);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_sprite != null)
            {
                AssetManager.UnloadAsset<Sprite>(_sprite.Name);
                _sprite = null;
            }
        }
    }

    public class Crosshair : Entity, IDisposable
    {
        // Event for shooting
        public event EventHandler<ShootEventArgs> OnShoot;

        // Event args for shooting
        public class ShootEventArgs : EventArgs
        {
            public Vector2 Position { get; }
            public bool IsRandomShot { get; }
            public int? ArmIndex { get; } // Null for main, otherwise mutated arm index

            public ShootEventArgs(Vector2 position, bool isRandomShot = false, int? armIndex = null)
            {
                Position = position;
                IsRandomShot = isRandomShot;
                ArmIndex = armIndex;
            }
        }

        private const int crosshairRadius = 25;
        private const float ArmDistanceFromCenter = 40f;

        private Sprite _sprite;
        private int _mutationLevel = 0;
        private Random _random;
        private List<CrosshairArm> _arms = new List<CrosshairArm>();
        private bool _canShoot = true;
        private MouseState _lastMouseState;

        public Crosshair() : base()
        {
            this._sprite = AssetManager.LoadAsset<Sprite>("crosshair_sprite.xml");
            _random = new Random();
            _lastMouseState = Mouse.GetState();
        }

        public override void OnStart()
        {
            base.OnStart();

            // v0.14.0: main crosshair renders via SpriteComponent; arms are child entities.
            var spriteComponent = AddComponent(new SpriteComponent(_sprite));
            RegisterForInstancedRendering(_sprite);
            SetZLayer(10);
        }

        public void SetMutationLevel(int level)
        {
            _mutationLevel = Math.Clamp(level, 0, 3); // Max 4 arms (1 original + 3 mutations)

            // Remove existing arms (destroyed entities are cleaned up by the system next frame)
            foreach (var arm in _arms)
            {
                arm.Destroy();
            }
            _arms.Clear();

            if (EntitySystem == null)
            {
                return;
            }

            // Add additional arms based on mutation level, distributed in a circle around the main crosshair
            for (int i = 0; i < _mutationLevel; i++)
            {
                float angle = (float)(i * (2 * Math.PI / _mutationLevel));
                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle) * ArmDistanceFromCenter,
                    (float)Math.Sin(angle) * ArmDistanceFromCenter
                );

                var arm = EntitySystem.CreateEntity<CrosshairArm>();
                arm.LocalPosition = offset;
                arm.LocalRotation = angle;
                AddChild(arm);
                _arms.Add(arm);
            }
        }
        public override void Update(GameTime gameTime)
        {
            // Update the position of the crosshair to follow the mouse
            MouseState currentMouseState = Mouse.GetState();
            Vector2 mousePosition = currentMouseState.Position.ToVector2();
            this._position = mousePosition - new Vector2(crosshairRadius, crosshairRadius);

            // Handle shooting with the main crosshair
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                _lastMouseState.LeftButton == ButtonState.Released &&
                _canShoot)
            {
                Console.WriteLine("Shot fired at: " + mousePosition);
                OnShoot?.Invoke(this, new ShootEventArgs(mousePosition));
            }

            _lastMouseState = currentMouseState;
        }

        // Method to trigger a random shot from a mutated arm
        // This will be called by GameManager when the player hits a target
        public void TriggerRandomShot()
        {
            // Only proceed if we have mutated arms
            if (_mutationLevel > 0)
            {
                // Pick a single mutated arm (not the main one)
                int mutatedArmCount = _arms.Count;
                if (mutatedArmCount > 0)
                {
                    int randomMutatedArm = _random.Next(1, mutatedArmCount + 1); // 1..N
                    Vector2 randomShotPosition = GetRandomShotPosition();
                    
                    // Only shoot if we found a valid target (not the indicator position)
                    if (randomShotPosition.X >= 0 && randomShotPosition.Y >= 0)
                    {
                        // Trigger the shooting event, passing the arm index
                        OnShoot?.Invoke(this, new ShootEventArgs(randomShotPosition, true, randomMutatedArm));
                        Console.WriteLine($"Random shot triggered from mutated arm {randomMutatedArm} at position {randomShotPosition}");
                    }
                    else
                    {
                        Console.WriteLine("No fully grown regular targets available for random shot - skipping");
                    }
                }
            }
        }
          private Vector2 GetRandomShotPosition()
        {
            // Query live targets directly — destroyed targets are inactive and excluded
            var targets = EntitySystem.FindByType<BaseTarget>();

            // Filter to only get fully grown regular targets
            var fullyGrownRegularTargets = targets
                .Where(t => t is RegularTarget && t.IsFullyGrown)
                .ToList();

            if (fullyGrownRegularTargets.Count > 0)
            {
                int idx = _random.Next(fullyGrownRegularTargets.Count);
                return fullyGrownRegularTargets[idx].Position;
            }

            // No fully grown targets available, so return null indicator position
            // This will be handled in TriggerRandomShot to prevent shooting
            return new Vector2(-1, -1);
        }

        public void Dispose()
        {
            AssetManager.UnloadAsset<Sprite>(_sprite.Name);
        }
    }
}
