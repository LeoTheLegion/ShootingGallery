using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShootingGallery
{
    public static class GameManager
    {
        private static double timer = 10;
        private static int score = 0;
        public static bool isGameOver => timer <= 0;

        public static void ReduceGameTime(double x) { 
            if(timer <= 0)
            {
                timer = 0;
                return;
            }
            timer -= x;
        }

        public static double GetGameTime() => timer;

        public static int GetScore() => score;
        public static void AddScore(int x) => score += x;
    }
}
