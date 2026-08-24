using System;
using System.Collections;
using System.Collections.Generic;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
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
        var worldCenter = World.Center;

        // HUD labels are data-driven in Content/game_scene.xml (CE GameObjectEntity +
        // AnchorComponent + LabelComponent); grab them by Id to wire live updates below.
        LoadEntitiesFromXml("game_scene.xml", entitySystem);
        var scoreText = entitySystem.FindById("score")?.GetComponent<LabelComponent>();
        var timerText = entitySystem.FindById("timer")?.GetComponent<LabelComponent>();
        var multiplierText = entitySystem.FindById("multiplier")?.GetComponent<LabelComponent>();
        var radiationText = entitySystem.FindById("radiation")?.GetComponent<LabelComponent>();
        var mutationText = entitySystem.FindById("mutation")?.GetComponent<LabelComponent>();

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
            mutationText.Text = $"Arms: {args.MutationLevel + 1}";

            // Display mutation message
            if (args.MutationLevel > 0)
            {
                string message = $"MUTATION LEVEL {args.MutationLevel}!";
                Color messageColor = new Color(0, 255, 0);

                entitySystem.CreateEntity<FloatingPopUpText>(
                    new Vector2(worldCenter.X, 150),
                    3f,
                    message,
                    messageColor,
                    1.5f,
                    true
                );

                // Create a smaller floating text with details
                string detailMessage = $"You've grown {args.MutationLevel} extra arm{(args.MutationLevel > 1 ? "s" : "")}!";
                entitySystem.CreateEntity<FloatingPopUpText>(
                    new Vector2(worldCenter.X, 200),
                    4f,
                    detailMessage,
                    messageColor,
                    1.2f,
                    true
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
            // Add a visual effect for shots (random shots pulse like radiation)
            entitySystem.CreateEntity<FloatingPopUpText>(
                args.Position,
                0.5f,
                "×",
                args.IsRandomShot ? Color.LimeGreen : Color.White,
                args.IsRandomShot ? 1.5f : 1.0f,
                args.IsRandomShot
            );
        };

        // Subscribe to the game over event
        gameManager.OnGameOver += (sender, args) =>
        {
            // Display game over message (pass all ctor args explicitly — CE's CreateEntity<T>
            // forwards this array to Activator.CreateInstance, which cannot fill optional params)
            entitySystem.CreateEntity<FloatingPopUpText>(
                new Vector2(worldCenter.X, worldCenter.Y - 50),
                5f,
                "GAME OVER!",
                Color.Red,
                2.0f,
                false
            );

            // Switch to GameOverScene and pass the final score and mutation level
            SceneManager.LoadScene(new GameOverScene(args.FinalScore, radiationManager.GetMutationLevel()));
        };
        yield return null;
    }
}
