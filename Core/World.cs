using Microsoft.Xna.Framework;

namespace ShootingGallery.Core
{
    /// <summary>
    /// The playable world's borders. Gameplay code refers to the world, never to
    /// graphics/window settings. Today the world is 1:1 with the window (no camera);
    /// if that ever changes, only Initialize() needs to change.
    /// </summary>
    public static class World
    {
        private static int _width;
        private static int _height;

        public static void Initialize(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public static int Width => _width;

        public static int Height => _height;

        public static Vector2 Size => new Vector2(_width, _height);

        public static Vector2 Center => Size / 2f;
    }
}
