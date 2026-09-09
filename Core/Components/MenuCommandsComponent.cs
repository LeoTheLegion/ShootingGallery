using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

namespace ShootingGallery.Core;

/// <summary>
/// Declarative &lt;Bind&gt; command target for the menu scenes. Attached to each scene's HUD
/// root entity in Content/start_menu.xml and Content/game_over.xml (auto-discovered by name —
/// no registration needed); buttons bind their Clicked events to these public methods, so there
/// is no FindById + subscribe boilerplate anywhere. Transitions are name-based (CE 0.20.0
/// scene-as-data), resolved through this component's own Game reference.
/// </summary>
public class MenuCommandsComponent : EntityComponent
{
    // These must be public INSTANCE methods (not static): CE's declarative Bind wiring
    // resolves command names against the component instance at runtime.
#pragma warning disable S1192 // Must stay instance methods — they are <Bind> targets in the scene XML
    /// <summary>Bound by the START MUTATION / Play Again buttons: starts a new round.</summary>
    public void StartGame() => LoadScene("game_scene.xml");

    /// <summary>Bound by the Main Menu button: returns to the start menu.</summary>
    public void BackToMenu() => LoadScene("start_menu.xml");
#pragma warning restore S1192

    private void LoadScene(string sceneAssetName) => Game?.SceneManager.LoadScene(sceneAssetName);
}
