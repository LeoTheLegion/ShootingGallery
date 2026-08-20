using Microsoft.Xna.Framework;

namespace ShootingGallery.Core
{
    /// <summary>
    /// Contains UI positioning constants used across different scenes
    /// </summary>
    public static class UIConstants
    {
        // Common title position
        public const int TITLE_Y = 100;
        
        // Start Menu Scene constants
        public const int SUBTITLE_Y = 160;
        public const int DESCRIPTION_START_Y = 240;
        public const int DESCRIPTION_SPACING = 50;
        public const int WARNING_Y = 390;
        public const int START_BUTTON_Y = 490;
        public const int CREDITS_OFFSET = 80;
        
        // Game Over Scene constants
        public const int CAUSE_Y = 200;
        public const int SCORE_Y = 280;
        public const int MUTATION_Y = 360;
        public const int RESTART_BUTTON_Y = 460;
        public const int MENU_BUTTON_Y = 530;
        
        // Get a position relative to the center of the screen
        public static Vector2 GetCenteredPosition(float y)
        {
            return new Vector2(ScreenManager.ScreenCenter.X, y);
        }
        
        // Get a position at the bottom of the screen
        public static Vector2 GetBottomPosition(float offsetFromBottom)
        {
            return new Vector2(ScreenManager.ScreenCenter.X, ScreenManager.ScreenHeight - offsetFromBottom);
        }
        
        // Get a position relative to the center of the screen with horizontal offset
        public static Vector2 GetCenteredPositionWithOffset(float y, float xOffset)
        {
            return new Vector2(ScreenManager.ScreenCenter.X + xOffset, y);
        }
    }
}
