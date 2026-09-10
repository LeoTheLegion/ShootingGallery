
using CoreEssentials;
using ShootingGallery.Core;

using var game = new MainGame();

game.Window.Title = "Shooting Gallery";
game.Graphics.PreferredBackBufferWidth = 1280;
game.Graphics.PreferredBackBufferHeight = 720;
game.Graphics.ApplyChanges();

// Boot purely from data files (CE 0.20.0 scene-as-data). The loading screen and every scene are
// strict-format XML assets staged into Content/ — no C# LoadingScene or scene subclass. The scene
// manifest gates all name-based loads; the first <GameScenes> entry (start_menu.xml) is startup.
game.SceneManager.SetManifestAsset("Scenes/scenes.xml");
game.SceneManager.SetLoadingScene("Scenes/loading.xml");
game.SceneManager.LoadScene("Scenes/start_menu.xml");

game.Run();
