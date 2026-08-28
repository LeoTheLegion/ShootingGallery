using CoreEssentials;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Scenes;

namespace ShootingGallery.Core;

/// <summary>
/// Declarative &lt;Bind&gt; command target for the menu scenes. Attached to each scene's HUD
/// root entity in Content/start_menu.xml and Content/game_over.xml (auto-discovered by name —
/// no registration needed); buttons bind their Clicked events to these public methods, so the
/// scene classes contain no FindById + subscribe boilerplate.
/// </summary>
public class MenuCommandsComponent : EntityComponent
{
    private static MainGame _game;

    /// <summary>Called once from Program.cs so XML-bound commands can reach the SceneManager.</summary>
    public static void Initialize(MainGame game) => _game = game;

    /// <summary>Bound by the START MUTATION / Play Again buttons: starts a new round.</summary>
    public void StartGame() => LoadScene(new GameScene());

    /// <summary>Bound by the Main Menu button: returns to the start menu.</summary>
    public void BackToMenu() => LoadScene(new StartMenuScene());

    private static void LoadScene(Scene scene)
    {
        if (_game != null)
            _game.SceneManager.LoadScene(scene);
    }
}
