using System;
using System.Collections;
using System.Collections.Generic;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;
using ShootingGallery;

namespace ShootingGallery.Core;

public class GameScene : Scene
{
    protected override GameSystem[] LoadGameSystems()
    {
        return new GameSystem[]{
            new EntitySystem(),
        };
    }

    protected override IEnumerator OnStartCoroutine()
    {
        Debug.StickyLog.IsVisible = false;

        var entitySystem = GetGameSystem<EntitySystem>();
        var screenCenter = ScreenManager.ScreenCenter;

        // HUD labels are data-driven in Content/game_scene.xml; grab typed references
        // by Id so we can wire live updates below. No buttons in this scene, so no commands.
        var hud = SceneLoader.LoadScene(entitySystem, "game_scene", new Dictionary<string, Action>());
        var scoreText = (TextEntity)hud["score"];
        var timerText = (TextEntity)hud["timer"];
        var multiplierText = (TextEntity)hud["multiplier"];
        var radiationText = (TextEntity)hud["radiation"];
        var mutationText = (TextEntity)hud["mutation"];

        // Create the player's crosshair
        var crosshair = entitySystem.CreateEntity<Crosshair>();

        // Create the radiation manager
        var radiationManager = entitySystem.CreateEntity<RadiationManager>();
        radiationManager.SetRadiationUI(radiationText);

        // Subscribe to radiation mutation events
        radiationManager.OnMutationChange += (sender, args) =>
        {
            // Update the crosshair with new mutation level
            crosshair.SetMutationLevel(args.MutationLevel);

            // Update mutation text
            mutationText.SetText($"Arms: {args.MutationLevel + 1}");

            // Display mutation message
            if (args.MutationLevel > 0)
            {
                string message = $"MUTATION LEVEL {args.MutationLevel}!";
                Color messageColor = new Color(0, 255, 0);

                entitySystem.CreateEntity<FloatingPopUpText>(
                    new Vector2(screenCenter.X, 150),
                    3f,
                    message,
                    messageColor,
                    1.5f
                );

                // Create a smaller floating text with details
                string detailMessage = $"You've grown {args.MutationLevel} extra arm{(args.MutationLevel > 1 ? "s" : "")}!";
                entitySystem.CreateEntity<FloatingPopUpText>(
                    new Vector2(screenCenter.X, 200),
                    4f,
                    detailMessage,
                    messageColor,
                    1.2f
                );
            }
        };

        // Create a game manager entity
        var gameManager = entitySystem.CreateEntity<GameManager>();

        // Configure the game manager
        gameManager.SetScoreUI(scoreText);
        gameManager.SetTimerUI(timerText);
        gameManager.SetMultiplierUI(multiplierText);
        gameManager.SetRadiationManager(radiationManager);
        gameManager.SetCrosshair(crosshair);

        // Subscribe to crosshair shoot events
        crosshair.OnShoot += (sender, args) =>
        {
            // Add a visual effect for shots
            entitySystem.CreateEntity<FloatingPopUpText>(
                args.Position,
                0.5f,
                "×",
                args.IsRandomShot ? Color.LimeGreen : Color.White,
                args.IsRandomShot ? 1.5f : 1.0f
            );
        };

        // Subscribe to the game over event
        gameManager.OnGameOver += (sender, args) =>
        {
            // Display game over message
            entitySystem.CreateEntity<FloatingPopUpText>(
                new Vector2(screenCenter.X, screenCenter.Y - 50),
                5f,
                "GAME OVER!",
                Color.Red,
                2.0f
            );

            // Switch to GameOverScene and pass the final score and mutation level
            SceneManager.LoadScene(new GameOverScene(args.FinalScore, radiationManager.GetMutationLevel()));
        };
        yield return null;
    }
}
