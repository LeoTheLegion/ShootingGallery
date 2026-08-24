using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

public class StartMenuScene : Scene
{
    protected override GameSystem[] LoadGameSystems()
    {
        return new GameSystem[]{
            new EntitySystem(),
        };
    }
    protected override IEnumerator OnStartCoroutine()
    {
        var entitySystem = GetGameSystem<EntitySystem>();

        // Layout, text, colors and the button are all data in Content/start_menu.xml
        // (CE GameObjectEntity + AnchorComponent + Label/Button components). Only the
        // click behavior is wired here, plus two decorative radiation popups.
        LoadEntitiesFromXml("start_menu.xml", entitySystem);

        var center = ScreenManager.ScreenCenter;
        entitySystem.CreateEntity<FloatingPopUpText>(new Vector2(center.X - 300, center.Y), 5f, "☢️ RADIATION ☢️", Color.LimeGreen, 1.5f, true);
        entitySystem.CreateEntity<FloatingPopUpText>(new Vector2(center.X + 300, center.Y), 5f, "☢️ RADIATION ☢️", Color.LimeGreen, 1.5f, true);

        var startButton = entitySystem.FindById("startButton")?.GetComponent<ButtonComponent>();
        if (startButton != null)
            startButton.Clicked += () => SceneManager.LoadScene(new GameScene());

        yield return null;
    }
}
