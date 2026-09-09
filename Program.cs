
using CoreEssentials;
using ShootingGallery.Core;

using var game = new MainGame();

game.Window.Title = "Shooting Gallery";
game.Graphics.PreferredBackBufferWidth = 1280;
game.Graphics.PreferredBackBufferHeight = 720;
game.Graphics.ApplyChanges();

// The world is 1:1 with the window (no camera); gameplay only ever asks World.
World.Initialize(game.Graphics.PreferredBackBufferWidth, game.Graphics.PreferredBackBufferHeight);

// Boot purely from data files (CE 0.20.0 scene-as-data). The loading screen and every scene are
// strict-format XML assets staged into Content/ — no C# LoadingScene or scene subclass. The scene
// manifest gates all name-based loads; the first <GameScenes> entry (start_menu.xml) is startup.
game.SceneManager.SetManifestAsset("scenes.xml");
game.SceneManager.SetLoadingScene("loading.xml");
game.SceneManager.LoadScene("start_menu.xml");

game.Run();
