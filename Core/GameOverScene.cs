using System;
using System.Collections;
using System.Collections.Generic;
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

        // Layout and buttons come from Content/game_over.xml. Button clicks are
        // resolved by Command name; the cause/score/mutation labels are then
        // overridden below from this run's state.
        var commands = new Dictionary<string, Action>
        {
            ["RestartGame"] = () => SceneManager.LoadScene(new GameScene()),
            ["MainMenu"] = () => SceneManager.LoadScene(new StartMenuScene()),
        };
        var ui = SceneLoader.LoadScene(entitySystem, "game_over", commands);

        // Cause of game over
        ((TextEntity)ui["cause"]).SetText(_bombHit
            ? "You hit a radioactive bomb!"
            : "Your 1 minute of shooting is up!");

        // Final score
        ((TextEntity)ui["score"]).SetText($"Final Score: {_finalScore}");

        // Mutation summary (message + color depend on mutation level)
        (string mutationMessage, Color mutationColor) = _mutationLevel switch
        {
            <= 0 => ("You escaped without mutations!", Color.White),
            1 => ("You grew one extra arm from radiation exposure!", Color.LightGreen),
            2 => ("You mutated with two extra arms!", Color.Green),
            _ => ("Your mutations have rendered you unrecognizable!", Color.LimeGreen),
        };
        var mutationText = (TextEntity)ui["mutation"];
        mutationText.SetText(mutationMessage);
        mutationText.SetColor(mutationColor);

        yield return null;
    }
}
