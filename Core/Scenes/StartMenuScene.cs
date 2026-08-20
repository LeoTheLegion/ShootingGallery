using System;
using System.Collections;
using System.Collections.Generic;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

public class StartMenuScene : Scene
{
    protected override GameSystem[] LoadGameSystems()
    {
        return new GameSystem[]{
            new EntitySystem(),
        };
    }
    protected override IEnumerator OnStartCoroutine()
    {
        var entitySystem = GetGameSystem<EntitySystem>();

        // Layout, text, and colors live in Scenes/start_menu.xml. Only the click
        // behavior is wired in code, selected by the Command name in the XML.
        var commands = new Dictionary<string, Action>
        {
            ["StartGame"] = () => SceneManager.LoadScene(new GameScene()),
        };

        SceneLoader.LoadScene(entitySystem, "start_menu", commands);

        yield return null;
    }
}
