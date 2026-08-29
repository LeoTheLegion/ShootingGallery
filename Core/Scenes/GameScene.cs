using System.Collections;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

/// <summary>
/// The gameplay scene. Everything is data-driven: the HUD, crosshair and round director are
/// all declared in Content/game_scene.xml as plain GameObjectEntities composed of components.
/// This scene only loads the XML and handles the game-over transition to the GameOverScene;
/// components reach their EntitySystem through CE 0.18.0's EntityComponent.EntitySystem.
/// </summary>
public class GameScene : Scene
{
    protected override GameSystem[] LoadGameSystems()
    {
        return new GameSystem[]
        {
            new EntitySystem(),
        };
    }

    protected override IEnumerator OnStartCoroutine()
    {
        Debug.StickyLog.IsVisible = false;

        var entitySystem = GetGameSystem<EntitySystem>();

        // Load the fully data-driven scene (HUD + crosshair + director).
        LoadEntitiesFromXml("game_scene.xml", entitySystem);

        var worldCenter = World.Center;

        // The only remaining code wiring: game-over popup + scene transition.
        var director = entitySystem.FindById("director")?.GetComponent<GameDirectorComponent>();
        if (director != null)
        {
            director.OnGameOver += (sender, args) =>
            {
                PopupSpawner.Spawn(
                    entitySystem,
                    new Vector2(worldCenter.X, worldCenter.Y - 50),
                    text: "GAME OVER!",
                    duration: 5f,
                    color: Color.Red,
                    scale: 2.0f
                );

                SceneManager.LoadScene(new GameOverScene(args.FinalScore, args.MutationLevel));
            };
        }

        yield return null;
    }
}
