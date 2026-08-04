using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

public class GameOverScene : Scene
{
    private int _finalScore;
    private bool _bombHit;
    private int _mutationLevel;

    public GameOverScene()
    {
        _finalScore = 0;
        _bombHit = false;
        _mutationLevel = 0;
    }

    public GameOverScene(int finalScore, int mutationLevel = 0, bool bombHit = false)
    {
        _finalScore = finalScore;
        _mutationLevel = mutationLevel;
        _bombHit = bombHit;
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

        var screen_size = ScreenManager.ScreenSize;
        var screenCenter = ScreenManager.ScreenCenter;        // Create a text entity for the game over message - centered at the top
        var gameOverText = entitySystem.CreateEntity<TextEntity>(
            UIConstants.GetCenteredPosition(UIConstants.TITLE_Y),
            "GAME OVER");
        gameOverText.SetColor(Color.Red);
        gameOverText.SetScale(2f);        // Create a text entity for the cause of game over
        string causeMessage = _bombHit
            ? "You hit a radioactive bomb!"
            : "Your 1 minute of shooting is up!";

        var causeText = entitySystem.CreateEntity<TextEntity>(
            UIConstants.GetCenteredPosition(UIConstants.CAUSE_Y),
            causeMessage);
        causeText.SetColor(Color.White);

        // Create a text entity for the score - centered below game over text
        var scoreText = entitySystem.CreateEntity<TextEntity>(
            UIConstants.GetCenteredPosition(UIConstants.SCORE_Y),
            $"Final Score: {_finalScore}");
        scoreText.SetColor(Color.Yellow);
        scoreText.SetScale(1.5f);

        // Add a radiation poisoning message
        string mutationMessage;
        Color mutationColor;

        if (_mutationLevel <= 0)
        {
            mutationMessage = "You escaped without mutations!";
            mutationColor = Color.White;
        }
        else if (_mutationLevel == 1)
        {
            mutationMessage = "You grew one extra arm from radiation exposure!";
            mutationColor = Color.LightGreen;
        }
        else if (_mutationLevel == 2)
        {
            mutationMessage = "You mutated with two extra arms!";
            mutationColor = Color.Green;
        }
        else
        {
            mutationMessage = "Your mutations have rendered you unrecognizable!";
            mutationColor = Color.LimeGreen;
        }
        var radiationText = entitySystem.CreateEntity<TextEntity>(
          UIConstants.GetCenteredPosition(UIConstants.MUTATION_Y),
          mutationMessage);
        radiationText.SetColor(mutationColor);

        // Create a button to restart the game - centered below score
        var restartButton = entitySystem.CreateEntity<ButtonEntity>(
            UIConstants.GetCenteredPosition(UIConstants.RESTART_BUTTON_Y),
            "Play Again", () =>
            {
                // Load the game scene (not start menu)
                SceneManager.LoadScene(new GameScene());
            });

        // Create a button to go to the main menu
        var menuButton = entitySystem.CreateEntity<ButtonEntity>(
            UIConstants.GetCenteredPosition(UIConstants.MENU_BUTTON_Y),
            "Main Menu", () =>
            {
                // Load the start menu scene
                SceneManager.LoadScene(new StartMenuScene());
            });

        yield return null;
    }
}
