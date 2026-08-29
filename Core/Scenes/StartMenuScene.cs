using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
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
        // (CE GameObjectEntity + AnchorComponent + Label/Button components). The START
        // click is bound declaratively in XML (<Bind> to MenuCommandsComponent.StartGame);
        // here we only add two decorative radiation popups.
        LoadEntitiesFromXml("start_menu.xml", entitySystem);

        var center = World.Center;
        PopupSpawner.Spawn(entitySystem, new Vector2(center.X - 300, center.Y), 5f, "☢️ RADIATION ☢️", Color.LimeGreen, 1.5f, true);
        PopupSpawner.Spawn(entitySystem, new Vector2(center.X + 300, center.Y), 5f, "☢️ RADIATION ☢️", Color.LimeGreen, 1.5f, true);

        yield return null;
    }
}
