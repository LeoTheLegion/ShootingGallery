using CoreEssentials;
using Microsoft.Xna.Framework;
using System;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Static class to access screen dimensions from anywhere in the game
    /// </summary>
    public static class ScreenManager
    {
        private static MainGame _game;

        public static void Initialize(MainGame game)
        {
            _game = game;
        }

        public static int ScreenWidth
        {
            get
            {
                if (_game == null)
                {
                    throw new InvalidOperationException("ScreenManager has not been initialized.");
                }
                return _game.Graphics.PreferredBackBufferWidth;
            }
        }

        public static int ScreenHeight
        {
            get
            {
                if (_game == null)
                {
                    throw new InvalidOperationException("ScreenManager has not been initialized.");
                }
                return _game.Graphics.PreferredBackBufferHeight;
            }
        }

        public static Vector2 ScreenSize
        {
            get
            {
                return new Vector2(ScreenWidth, ScreenHeight);
            }
        }

        public static Vector2 ScreenCenter
        {
            get
            {
                return ScreenSize / 2;
            }
        }
    }
}
