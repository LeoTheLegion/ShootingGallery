
using CoreEssentials;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;
using ShootingGallery.Core;

using var game = new MainGame();

game.Window.Title = "Shooting Gallery";
game.Graphics.PreferredBackBufferWidth = 1280;
game.Graphics.PreferredBackBufferHeight = 720;
game.Graphics.ApplyChanges();

// The world is 1:1 with the window (no camera); gameplay only ever asks World.
World.Initialize(game.Graphics.PreferredBackBufferWidth, game.Graphics.PreferredBackBufferHeight);

// XML-declared <Bind> commands in the menu scenes resolve through this reference.
MenuCommandsComponent.Initialize(game);

// Create a loading scene with custom colors
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
