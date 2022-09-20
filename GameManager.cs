﻿using Microsoft.Xna.Framework;
using ShootingGallery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ShootingGallery
{
    public static class GameManager
    {
        private static double timer = 10;
        private static int score = 0;
        private static Text _scoreUI, _timerUI;
        private static Entity _target;

        public static bool isGameOver => timer <= 0;

        public static void ReduceGameTime(double x) { 
            timer -= x;
            if (timer < 0)
            {
                timer = 0;
            }
        }

        public static double GetGameTime() => timer;

        public static int GetScore() => score;
        public static void AddScore(int x) => score += x;

        internal static void Init(ref Text score, ref Text timer, ref Entity target)
        {
            _scoreUI = score;
            _timerUI = timer;
            _target = target;
        }

        internal static void Update(ref GameTime gameTime)
        {
            _scoreUI.SetText("Score: " + score.ToString());
            _timerUI.SetText("Time: " + Math.Ceiling(timer).ToString());


            if (!isGameOver)
            {
                ReduceGameTime(gameTime.ElapsedGameTime.TotalSeconds);
            }
            else
            {
                _target.SetActive(false);
            }
        }
    }
}
