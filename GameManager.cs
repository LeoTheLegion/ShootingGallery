using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ShootingGallery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ShootingGallery
{
    public static class GameManager
    {
        private const double ROUNDTIME = 10;
        private static double timer;
        private static int score = 0;
        private static Text _scoreUI, _timerUI, _gameOverMessageUI; // should be events handled
        private static Entity _target;

        public static bool isGameOver => timer <= 0;


        public static double GetGameTime() => timer;

        public static int GetScore() => score;
        public static void AddScore(int x) => score += x;

        public static void Init(ref Text scoreUI, ref Text timerUI, ref Text gameOverMessageUI, ref Entity target)
        {
            _scoreUI = scoreUI;
            _timerUI = timerUI;
            _target = target;
            _gameOverMessageUI = gameOverMessageUI;
            RestartRound();
        }

        public static void Update(ref GameTime gameTime)
        {
            _scoreUI.SetText("Score: " + score.ToString());
            _timerUI.SetText("Time: " + Math.Ceiling(timer).ToString());

            if (!isGameOver)
            {
                _target.SetActive(true);
                _gameOverMessageUI.SetActive(false);

                timer -= gameTime.ElapsedGameTime.TotalSeconds;

                if (timer < 0)
                    timer = 0;
            }
            else
            {
                _target.SetActive(false);
                _gameOverMessageUI.SetActive(true);

                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    RestartRound();

            }
        }
        private static void RestartRound()
        {
            timer = ROUNDTIME;
            score = 0;
        }

    }
}
