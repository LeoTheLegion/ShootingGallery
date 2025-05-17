
using CoreEssentials;
using CoreEssentials.SceneManagement;
using Microsoft.Xna.Framework;
using ShootingGallery.Core;

using var game = new MainGame();

game.Window.Title = "Shooting Gallery";
game.Graphics.PreferredBackBufferWidth = 1280;
game.Graphics.PreferredBackBufferHeight = 720;
game.Graphics.ApplyChanges();

// Initialize ScreenManager
ScreenManager.Initialize(game);

// Create a loading screen with custom colors
LoadingScene loadingScene = new LoadingScene(
    "Loading Character Demo...", 
    Color.Black, 
    Color.LightBlue, 
    Color.White
);

// Set the loading scene for the SceneManager to use during transitions
game.SceneManager.SetLoadingScene(loadingScene);

var scene = new StartMenuScene();

game.SceneManager.LoadScene(scene);

game.Run();
