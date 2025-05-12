using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.SceneManagement;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

public class StartMenuScene : Scene
{
    protected override GameSystem[] LoadGameSystems()
    {
        return new GameSystem[]{
            new EntitySystem(),
        };
    }    protected override IEnumerator OnStartCoroutine()
    {
        var entitySystem = GetGameSystem<EntitySystem>();

        var screen_size = new Vector2(1280, 720);
        var screenCenter = screen_size / 2;

        // Create a text entity for the title - centered horizontally and positioned in the top third
        var titleText = entitySystem.CreateEntity<TextEntity>(
            new Vector2(screenCenter.X, screenCenter.Y - 150),
            "Shooting Gallery");

        // Position the start button below the title and center it horizontally
        var startButton = entitySystem.CreateEntity<ButtonEntity>(
            new Vector2(screenCenter.X, screenCenter.Y + 50),
            "Start Game",
            () =>
            {
                // Load the game scene
                SceneManager.LoadScene(new GameScene());
            });


        yield return null;
    }
}
