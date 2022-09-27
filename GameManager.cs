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
        private static Text _scoreUI, _timerUI, _gameOverMessageUI, _startMessageUI; // should be events handled
        private static Button _restartButton, _startButtonUI;
        private static Entity _target;
        private static bool started;
        public static bool isGameOver => timer <= 0;


        public static double GetGameTime() => timer;

        public static int GetScore() => score;
        public static void AddScore(int x) => score += x;

        internal static void Init(ref Text scoreUI, ref Text timerUI,
            ref Text gameoverMessageUI, ref Button restartButtonUI,
            ref Text startMessageUI, ref Button startButtonUI,
            ref Entity target)
        {
            _scoreUI = scoreUI;
            _timerUI = timerUI;
            _target = target;
            _gameOverMessageUI = gameoverMessageUI;
            _restartButton = restartButtonUI.SetOnPress(() => GameManager.RestartRound());

            _startMessageUI = startMessageUI;
            _startButtonUI = startButtonUI.SetOnPress(() => GameManager.StartGame());

            started = false;
        }

        public static void StartGame()
        {
            started = true;
            RestartRound();
        }

        public static void Update(ref GameTime gameTime)
        {
            if (!started)
                ProcessStartMenu();
            else if(isGameOver)
                ProcessGameOver();
            else
                ProcessGameplay(gameTime);
        }

        private static void ProcessGameOver()
        {
            _startMessageUI.SetActive(false);
            _startButtonUI.SetActive(false);
            _scoreUI.SetActive(true);
            _timerUI.SetActive(true);

            _scoreUI.SetText("Score: " + score.ToString());
            _timerUI.SetText("Time: " + Math.Ceiling(timer).ToString());

            _target.SetActive(false);
            _gameOverMessageUI.SetActive(true);
            _restartButton.SetActive(true);

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                RestartRound();
        }

        private static void ProcessGameplay(GameTime gameTime)
        {
            _startMessageUI.SetActive(false);
            _startButtonUI.SetActive(false);
            _scoreUI.SetActive(true);
            _timerUI.SetActive(true);

            _scoreUI.SetText("Score: " + score.ToString());
            _timerUI.SetText("Time: " + Math.Ceiling(timer).ToString());

            _target.SetActive(true);
            _gameOverMessageUI.SetActive(false);
            _restartButton.SetActive(false);

            timer -= gameTime.ElapsedGameTime.TotalSeconds;

            if (timer < 0)
                timer = 0;
        }

        private static void ProcessStartMenu()
        {
            _target.SetActive(false);
            _gameOverMessageUI.SetActive(false);
            _restartButton.SetActive(false);
            _scoreUI.SetActive(false);
            _timerUI.SetActive(false);

            _startMessageUI.SetActive(true);
            _startButtonUI.SetActive(true);
        }

        public static void RestartRound()
        {
            timer = ROUNDTIME;
            score = 0;
        }


    }
}
