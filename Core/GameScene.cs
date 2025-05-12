using System;
using System.Collections;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.SceneManagement;
using Microsoft.Xna.Framework;

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

        var entitySystem = GetGameSystem<EntitySystem>(); var screen_size = new Vector2(1280, 720);
        var screenCenter = screen_size / 2;

        // Create a score text entity - positioned at top left
        var scoreText = entitySystem.CreateEntity<TextEntity>(new Vector2(100, 30), "Score: 0");

        // Create a timer text entity - positioned at top right
        var timerText = entitySystem.CreateEntity<TextEntity>(new Vector2(screen_size.X - 100, 30), "Time: 10");        
        // Create a game manager entity
        var gameManager = entitySystem.CreateEntity<GameManager>();

        // Set the score and timer UI in the game manager
        gameManager.SetScoreUI(scoreText);
        gameManager.SetTimerUI(timerText);        // Subscribe to the game over event
        gameManager.OnGameOver += (sender, args) =>
        {
            // Switch to GameOverScene and pass the final score
            SceneManager.LoadScene(new GameOverScene(args.FinalScore));
        };

        //create a TargetEntity
        var target = entitySystem.CreateEntity<Target>(new Vector2(200, 200));

        target.OnScore += (sender, args) =>
        {
            gameManager.AddScore(args.Score);
        };

        yield return null;
    }
}
