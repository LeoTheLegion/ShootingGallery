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

        // One-time template registration (until CE auto-registers templates — upstream issue #84).
        entitySystem.RegisterTemplate("Popup", "popup.xml");

        // Layout, text, colors and the button are all data in Content/start_menu.xml
        // (CE GameObjectEntity + AnchorComponent + Label/Button components). The START
        // click is bound declaratively in XML via a Bind element targeting
        // MenuCommandsComponent.StartGame; here we only add two decorative radiation popups.
        LoadEntitiesFromXml("start_menu.xml", entitySystem);

        var center = World.Center;
        SpawnRadiationPopup(entitySystem, new Vector2(center.X - 300, center.Y));
        SpawnRadiationPopup(entitySystem, new Vector2(center.X + 300, center.Y));

        yield return null;
    }

    /// <summary>Decorative radiation popup — Unity-style prefab instantiate + field pokes.</summary>
    private static void SpawnRadiationPopup(EntitySystem entitySystem, Vector2 position)
    {
        var popup = entitySystem.Instantiate("Popup", position)?.GetComponent<FloatingPopUpComponent>();
        if (popup == null)
            return;

        popup.Text = "☢️ RADIATION ☢️";
        popup.Duration = 5f;
        popup.TextColor = Color.LimeGreen;
        popup.Scale = 1.5f;
        popup.RadiationEffect = true;
    }
}
