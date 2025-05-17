using System;
using System.Collections.Generic;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShootingGallery.Core;
namespace ShootingGallery
{
    public class Crosshair : Entity, IDisposable
    {
        // Event for shooting
        public event EventHandler<ShootEventArgs> OnShoot;

        // Event args for shooting
        public class ShootEventArgs : EventArgs
        {
            public Vector2 Position { get; }
            public bool IsRandomShot { get; }
            
            public ShootEventArgs(Vector2 position, bool isRandomShot = false)
            {
                Position = position;
                IsRandomShot = isRandomShot;
            }
        }

        private const int crosshairRadius = 25;
        private const float ArmDistanceFromCenter = 40f;
        private const float RandomShotCooldown = 1.5f; // Time between random shots in seconds

        private Sprite _sprite;
        private int _mutationLevel = 0;
        private float _randomShotTimer = 0f;
        private Random _random;
        private List<Vector2> _armPositions;
        private List<float> _armRotations;
        private bool _canShoot = true;
        private MouseState _lastMouseState;

        public Crosshair() : base()
        {
            this._sprite = AssetManager.LoadAsset<Sprite>("crosshair_sprite.xml");
            _random = new Random();
            _armPositions = new List<Vector2>();
            _armRotations = new List<float>();
            
            // Initialize with 1 arm (the main one)
            _armPositions.Add(Vector2.Zero);
            _armRotations.Add(0f);
            
            _lastMouseState = Mouse.GetState();
        }

        public void SetMutationLevel(int level)
        {
            _mutationLevel = Math.Clamp(level, 0, 3); // Max 4 arms (1 original + 3 mutations)
            
            // Reset arm positions
            _armPositions.Clear();
            _armRotations.Clear();
            
            // Always have the main arm
            _armPositions.Add(Vector2.Zero);
            _armRotations.Add(0f);
            
            // Add additional arms based on mutation level
            for (int i = 0; i < _mutationLevel; i++)
            {
                // Calculate position of the arm (distributed in a circle around the main crosshair)
                float angle = (float)(i * (2 * Math.PI / _mutationLevel));
                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle) * ArmDistanceFromCenter,
                    (float)Math.Sin(angle) * ArmDistanceFromCenter
                );
                
                _armPositions.Add(offset);
                _armRotations.Add(angle);
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
                OnShoot?.Invoke(this, new ShootEventArgs(mousePosition));
            }
            
            // Handle random shots from mutated arms
            if (_mutationLevel > 0)
            {
                _randomShotTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                if (_randomShotTimer <= 0)
                {
                    // Get a random arm (except the main one)
                    int randomArmIndex = _random.Next(1, _armPositions.Count);
                    
                    // Calculate a random position to shoot at
                    Vector2 randomShotPosition = GetRandomShotPosition();
                    
                    // Trigger the shooting event
                    OnShoot?.Invoke(this, new ShootEventArgs(randomShotPosition, true));
                    
                    // Reset the timer with some randomness
                    _randomShotTimer = RandomShotCooldown - (_mutationLevel * 0.2f) + (float)(_random.NextDouble() * 0.5f);
                }
            }
            
            _lastMouseState = currentMouseState;
        }
          private Vector2 GetRandomShotPosition()
        {
            // Create a random shot position within the game window
            return new Vector2(
                _random.Next(50, ScreenManager.ScreenWidth - 50),
                _random.Next(50, ScreenManager.ScreenHeight - 50)
            );
        }

        public override void Render(SpriteBatch _spriteBatch)
        {
            // Draw the main crosshair
            _sprite.Draw(_spriteBatch, _position, Color.White, 0f, SpriteEffects.None, 0f);
            
            // Draw the mutated arms
            Color mutatedColor = new Color(0, 255, 0, 200); // Glowing green for radiation effect
            
            for (int i = 1; i < _armPositions.Count; i++) // Start from 1 to skip the main arm
            {
                Vector2 armPosition = _position + _armPositions[i];
                _sprite.Draw(_spriteBatch, armPosition, mutatedColor, _armRotations[i], 0.8f, SpriteEffects.None, 0f);
            }
        }

        public void Dispose()
        {
            AssetManager.UnloadAsset<Sprite>(_sprite.Name);
        }
    }
}
