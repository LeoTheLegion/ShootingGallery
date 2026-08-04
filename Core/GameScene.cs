using System;
using System.Collections;
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
        var screen_size = ScreenManager.ScreenSize;
        var screenCenter = ScreenManager.ScreenCenter;

        // Create UI elements
        // Title UI
        var titleText = entitySystem.CreateEntity<TextEntity>(
            new Vector2(screenCenter.X, 30),
            "URANIUM REVOLVER");
        titleText.SetColor(Color.LimeGreen);
        titleText.SetScale(1.5f);

        // Score UI
        var scoreText = entitySystem.CreateEntity<TextEntity>(
            new Vector2(20, 20),
            "Score: 0");
        scoreText.SetColor(Color.White);
        scoreText.SetScale(1.2f);

        // Timer UI
        var timerText = entitySystem.CreateEntity<TextEntity>(
            new Vector2(screen_size.X - 150, 20),
            "Time: 300");
        timerText.SetColor(Color.Yellow);
        timerText.SetScale(1.2f);

        // Multiplier UI
        var multiplierText = entitySystem.CreateEntity<TextEntity>(
            new Vector2(20, 50),
            "Multiplier: x10.0");
        multiplierText.SetColor(Color.Orange);
        multiplierText.SetScale(1.0f);

        // Radiation UI
        var radiationText = entitySystem.CreateEntity<TextEntity>(
            new Vector2(screen_size.X - 150, 50),
            "Radiation: 0%");
        radiationText.SetColor(Color.LimeGreen);
        radiationText.SetScale(1.0f);

        // Arm count UI
        var mutationText = entitySystem.CreateEntity<TextEntity>(
            new Vector2(20, 80),
            "Arms: 1");
        mutationText.SetColor(Color.LimeGreen);
        mutationText.SetScale(1.0f);

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
