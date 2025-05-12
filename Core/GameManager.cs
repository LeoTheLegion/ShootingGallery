using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.SceneManagement;
using Microsoft.Xna.Framework;
using ShootingGallery.Core;
using System;

namespace ShootingGallery
{
    public class GameManager : Entity
    {
        // Event for game over
        public event EventHandler<GameOverEventArgs> OnGameOver;

        // Event args for game over
        public class GameOverEventArgs : EventArgs
        {
            public int FinalScore { get; }
            
            public GameOverEventArgs(int finalScore)
            {
                FinalScore = finalScore;
            }
        }

        private const double ROUNDTIME = 10;
        private  double timer;
        private  int score = 0;
        public  bool isGameOver => timer <= 0;

        private  TextEntity _scoreUI;
        private  TextEntity _timerUI;


        public  double GetGameTime() => timer;

        public  int GetScore() => score;
        public  void AddScore(int x) => score += x;

        public  void SetScoreUI(TextEntity scoreUI) => _scoreUI = scoreUI;
        public  void SetTimerUI(TextEntity timerUI) => _timerUI = timerUI;

        public GameManager()
        {
            timer = ROUNDTIME;
            score = 0;
        }

        public override void Update(GameTime gameTime)
        {
            ProcessGameplay(gameTime);
        }

        private  void ProcessGameplay(GameTime gameTime)
        {
            _scoreUI.SetText("Score: " + score.ToString());
            _timerUI.SetText("Time: " + Math.Ceiling(timer).ToString());

            timer -= gameTime.ElapsedGameTime.TotalSeconds;

            if (timer < 0){
                timer = 0;
                // Game Over logic
                OnGameOver?.Invoke(this, new GameOverEventArgs(score));
            }
        }

        public  void RestartRound()
        {
            timer = ROUNDTIME;
            score = 0;
        }


    }
}
