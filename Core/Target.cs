using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ShootingGallery
{
    public class Target : Entity, IDisposable
    {
        // Event for score reporting
        public event EventHandler<ScoreEventArgs> OnScore;

        // Event arguments for score events
        public class ScoreEventArgs : EventArgs
        {
            public int Score { get; }
            public Vector2 Position { get; }

            public ScoreEventArgs(int score, Vector2 position)
            {
                Score = score;
                Position = position;
            }
        }

        private const int targetRadius = 45;
        private const float _DEFAULTSCALE = .3f;
        private float _scale;
        private const double _TIMETOFULLSIZE = 3.0;
        private double _time;

        private Random rand;

        private Sprite _sprite;

        public Target(Vector2 targetPosition) : base()
        {
            this._position = targetPosition;
            this._scale = _DEFAULTSCALE;
            this._time = 0f;
            this.rand = new Random();
        }

        public override void OnStart()
        {
            base.OnStart();
            this._sprite = AssetManager.LoadAsset<Sprite>("target_sprite.xml");
            MoveRandomly();
        }

        public override void Update(GameTime gameTime)
        {
            var mState = Mouse.GetState();

            if (mState.LeftButton == ButtonState.Pressed)
            {
                float mouseTargetDist = Vector2.Distance(_position, mState.Position.ToVector2());

                if (mouseTargetDist < targetRadius * _scale)
                {
                    int score = CalculateScore();

                    ReportScore(score);
                    MoveRandomly();
                    Reset();
                }
            }

            _time += gameTime.ElapsedGameTime.TotalSeconds;

            _scale = (float)Math.MinMagnitude(_time / _TIMETOFULLSIZE, 1);
        }

        private int CalculateScore()
        {
            int score;

            if (_scale < .4f)
                score = 10;
            else if (_scale < 0.8f)
                score = 5;
            else
                score = 1;
            return score;
        }
        private void ReportScore(int score)
        {

            this.EntitySystem.CreateEntity<FloatingPopUpText>(
                this._position,
                2f,
                score.ToString()
                );

            // Trigger the OnScore event
            OnScore?.Invoke(this, new ScoreEventArgs(score, this._position));
        }

        private void MoveRandomly()
        {

            _position.X = rand.Next(targetRadius, 1280 - targetRadius);
            _position.Y = rand.Next(targetRadius, 720 - targetRadius);
        }
        private void Reset()
        {
            _scale = _DEFAULTSCALE;
            _time = 0;
        }       
        public override void Render(SpriteBatch _spriteBatch)
        {
            _sprite.Draw(_spriteBatch, _position, Color.White, 0f, Vector2.One * _scale, SpriteEffects.None, 0);
        }

        public void Dispose()
        {
            AssetManager.UnloadAsset<Sprite>(_sprite.Name);
        }

    }
}
