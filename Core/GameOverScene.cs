using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.SceneManagement;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

public class GameOverScene : Scene
{
    private int _finalScore;

    public GameOverScene()
    {
        _finalScore = 0; // Default value
    }

    public GameOverScene(int finalScore)
    {
        _finalScore = finalScore;
    }

    protected override GameSystem[] LoadGameSystems()
    {
        return new GameSystem[]{
            new EntitySystem(),
        };
    }

    protected override IEnumerator OnStartCoroutine()
    {
        var entitySystem = GetGameSystem<EntitySystem>();

        var screen_size = new Vector2(1280, 720);
        var screenCenter = screen_size / 2;

        // Create a text entity for the game over message - centered at the top
        var gameOverText = entitySystem.CreateEntity<TextEntity>(
            new Vector2(screenCenter.X, screenCenter.Y - 200),
            "Game Over");
        // Create a text entity for the score - centered below game over text
        var scoreText = entitySystem.CreateEntity<TextEntity>(
            new Vector2(screenCenter.X, screenCenter.Y - 100),
            $"Score: {_finalScore}");
        // Create a button to restart the game - centered below score
        var restartButton = entitySystem.CreateEntity<ButtonEntity>(
            new Vector2(screenCenter.X, screenCenter.Y),
            "Restart", () =>
            {
                // Load the game scene (not start menu)
                SceneManager.LoadScene(new GameScene());
            });

        yield return null;
    }

}
