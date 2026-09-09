using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

/// <summary>
/// Applies this run's result to the game-over screen. Name-based scene loads can't carry
/// parameters, so GameDirectorComponent publishes the outcome via the static LastResult handoff
/// right before transitioning here; this component reads it and pokes the cause / score / mutation
/// labels (resolved by id) that Content/game_over.xml declares as placeholders. The poking is
/// deferred to the first Update because DataDrivenScene attaches components pre-order (parents
/// before children), so the child label entities don't exist yet at OnAttach time.
/// </summary>
public class GameOverSummaryComponent : EntityComponent
{
    /// <summary>The most recent round's outcome, written by GameDirectorComponent before the
    /// transition to the game-over scene.</summary>
    public static GameOverResult LastResult { get; set; } = new GameOverResult(0, 0, false);

    /// <summary>A finished round's result: final score, mutation level and whether a bomb was hit.</summary>
    public class GameOverResult
    {
        public int FinalScore { get; }
        public int MutationLevel { get; }
        public bool BombHit { get; }

        public GameOverResult(int finalScore, int mutationLevel, bool bombHit)
        {
            FinalScore = finalScore;
            MutationLevel = mutationLevel;
            BombHit = bombHit;
        }
    }

    private bool _applied;

    public override void Update(GameTime gameTime)
    {
        if (_applied || Owner == null || Owner.Destroyed)
            return;

        var es = EntitySystem;
        if (es == null)
            return;

        // Defer until the child labels exist (components attach parents-before-children).
        if (es.FindById("cause") == null || es.FindById("score") == null || es.FindById("mutation") == null)
            return;

        var result = LastResult;

        var causeText = es.FindById("cause")?.GetComponent<LabelComponent>();
        if (causeText != null)
            causeText.Text = result.BombHit ? "You hit a radioactive bomb!" : "Your 1 minute of shooting is up!";

        var scoreText = es.FindById("score")?.GetComponent<LabelComponent>();
        if (scoreText != null)
            scoreText.Text = $"Final Score: {result.FinalScore}";

        // Mutation summary (message + color depend on mutation level)
        (string mutationMessage, Color mutationColor) = result.MutationLevel switch
        {
            <= 0 => ("You escaped without mutations!", Color.White),
            1 => ("You grew one extra arm from radiation exposure!", Color.LightGreen),
            2 => ("You mutated with two extra arms!", Color.Green),
            _ => ("Your mutations have rendered you unrecognizable!", Color.LimeGreen),
        };

        var mutationText = es.FindById("mutation")?.GetComponent<LabelComponent>();
        if (mutationText != null)
        {
            mutationText.Text = mutationMessage;
            mutationText.TextColor = mutationColor;
        }

        _applied = true;
    }
}
