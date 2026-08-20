using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ShootingGallery
{
    public class RadiationManager : Entity
    {
        // Event for mutation changes
        public event EventHandler<MutationEventArgs> OnMutationChange;

        // Event arguments for mutation events
        public class MutationEventArgs : EventArgs
        {
            public int MutationLevel { get; }
            
            public MutationEventArgs(int mutationLevel)
            {
                MutationLevel = mutationLevel;
            }
        }

        // Constants
        private const float MAX_RADIATION = 100f;
        private const float RADIATION_PER_TARGET = 5f; // Base radiation per target hit
        private const int MAX_MUTATION_LEVEL = 3; // Maximum number of extra arms (max is 4 arms total)
        
        // Thresholds for mutations (percentage of max radiation)
        private readonly float[] MUTATION_THRESHOLDS = {
            0.25f,  // 25% - First mutation
            0.5f,   // 50% - Second mutation  
            0.75f   // 75% - Third mutation
        };
          // Current state
        private float _currentRadiation = 0f;
        private int _currentMutationLevel = 0;
        private ShootingGallery.Core.TextEntity _radiationUI;
        
        public int GetMutationLevel() => _currentMutationLevel;
        public float GetRadiationPercentage() => _currentRadiation / MAX_RADIATION;
        
        public void SetRadiationUI(ShootingGallery.Core.TextEntity radiationUI) => _radiationUI = radiationUI;
        
        public RadiationManager()
        {
            _currentRadiation = 0f;
            _currentMutationLevel = 0;
        }
        
        public override void Update(GameTime gameTime)
        {
            if (_radiationUI != null)
            {
                _radiationUI.SetText($"Radiation: {(int)(_currentRadiation / MAX_RADIATION * 100)}%");
            }
            
            // Check if we need to mutate
            CheckForMutation();
        }
        
        public void AddRadiation(float amount)
        {
            _currentRadiation = MathHelper.Clamp(_currentRadiation + amount, 0, MAX_RADIATION);
        }
        
        public void AddRadiationFromTarget(float targetMultiplier = 1f)
        {
            AddRadiation(RADIATION_PER_TARGET * targetMultiplier);
        }
        
        private void CheckForMutation()
        {
            float radiationPercentage = GetRadiationPercentage();
            int newMutationLevel = 0;
            
            // Determine new mutation level based on radiation percentage
            for (int i = 0; i < MUTATION_THRESHOLDS.Length; i++)
            {
                if (radiationPercentage >= MUTATION_THRESHOLDS[i])
                {
                    newMutationLevel = i + 1;
                }
                else
                {
                    break;
                }
            }
            
            // If mutation level changed, trigger the event
            if (newMutationLevel != _currentMutationLevel)
            {
                _currentMutationLevel = newMutationLevel;
                OnMutationChange?.Invoke(this, new MutationEventArgs(_currentMutationLevel));
            }
        }
        
        public void Reset()
        {
            _currentRadiation = 0f;
            _currentMutationLevel = 0;
            
            // Notify of mutation reset
            OnMutationChange?.Invoke(this, new MutationEventArgs(_currentMutationLevel));
        }
    }
}
