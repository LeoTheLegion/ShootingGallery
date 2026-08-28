using System;
using System.Collections;
using System.Collections.Generic;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

public class GameOverScene : Scene
{
    private readonly int _finalScore;
    private readonly bool _bombHit;
    private readonly int _mutationLevel;

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

        // Layout, text and buttons are all data in Content/game_over.xml (CE GameObjectEntity +
        // AnchorComponent + Label/Button components). The two button clicks are bound
        // declaratively in XML (<Bind> to MenuCommandsComponent); we only override the
        // cause/score/mutation labels from this run's state below.
        LoadEntitiesFromXml("game_over.xml", entitySystem);

        var causeText = entitySystem.FindById("cause")?.GetComponent<LabelComponent>();
        if (causeText != null)
            causeText.Text = _bombHit ? "You hit a radioactive bomb!" : "Your 1 minute of shooting is up!";

        var scoreText = entitySystem.FindById("score")?.GetComponent<LabelComponent>();
        if (scoreText != null)
            scoreText.Text = $"Final Score: {_finalScore}";

        // Mutation summary (message + color depend on mutation level)
        (string mutationMessage, Color mutationColor) = _mutationLevel switch
        {
            <= 0 => ("You escaped without mutations!", Color.White),
            1 => ("You grew one extra arm from radiation exposure!", Color.LightGreen),
            2 => ("You mutated with two extra arms!", Color.Green),
            _ => ("Your mutations have rendered you unrecognizable!", Color.LimeGreen),
        };
        var mutationText = entitySystem.FindById("mutation")?.GetComponent<LabelComponent>();
        if (mutationText != null)
        {
            mutationText.Text = mutationMessage;
            mutationText.TextColor = mutationColor;
        }

        yield return null;
    }
}
