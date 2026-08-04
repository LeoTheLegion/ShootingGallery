using System;
using System.Collections;
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

        var screen_size = ScreenManager.ScreenSize;
        var screenCenter = ScreenManager.ScreenCenter;        // Create title text - positioned at the top of the screen
        var titleText = entitySystem.CreateEntity<TextEntity>(
            UIConstants.GetCenteredPosition(UIConstants.TITLE_Y),
            "URANIUM REVOLVER");
        titleText.SetColor(Color.LimeGreen);
        titleText.SetScale(2.5f);

        // Create subtitle text
        var subtitleText = entitySystem.CreateEntity<TextEntity>(
            UIConstants.GetCenteredPosition(UIConstants.SUBTITLE_Y),
            "A Radioactive Shooting Gallery");
        subtitleText.SetColor(Color.Yellow);
        subtitleText.SetScale(1.5f);

        // Create game description - placed in the middle of the screen
        var descriptionText1 = entitySystem.CreateEntity<TextEntity>(
            UIConstants.GetCenteredPosition(UIConstants.DESCRIPTION_START_Y),
            "Shoot targets to score points and accumulate radiation");
        descriptionText1.SetColor(Color.White);

        var descriptionText2 = entitySystem.CreateEntity<TextEntity>(
            UIConstants.GetCenteredPosition(UIConstants.DESCRIPTION_START_Y + UIConstants.DESCRIPTION_SPACING),
            "Mutations will give you extra arms to shoot with");
        descriptionText2.SetColor(Color.LightGreen);

        var descriptionText3 = entitySystem.CreateEntity<TextEntity>(
            UIConstants.GetCenteredPosition(UIConstants.DESCRIPTION_START_Y + UIConstants.DESCRIPTION_SPACING * 2),
            "Avoid radioactive bombs or face instant death!");
        descriptionText3.SetColor(Color.Red);

        // Add a radiation warning message
        var warningText = entitySystem.CreateEntity<TextEntity>(
            UIConstants.GetCenteredPosition(UIConstants.WARNING_Y),
            "WARNING: Extreme Radiation Exposure!");
        warningText.SetColor(Color.Orange);
        warningText.SetScale(1.2f);
        
        // Position the start button - in the lower middle of the screen
        var startButton = entitySystem.CreateEntity<ButtonEntity>(
            UIConstants.GetCenteredPosition(UIConstants.START_BUTTON_Y),
            "START MUTATION",
            () =>
            {
                // Load the game scene
                SceneManager.LoadScene(new GameScene());
            });        // Create a credits text - at the bottom of the screen
        var creditsText = entitySystem.CreateEntity<TextEntity>(
            UIConstants.GetBottomPosition(UIConstants.CREDITS_OFFSET),
            "Created with Uranium Power - Warning: May Cause Mutations");
        creditsText.SetColor(new Color(100, 255, 100));
        
        // Add floating radiation text effects on both sides
        entitySystem.CreateEntity<FloatingPopUpText>(
            UIConstants.GetCenteredPositionWithOffset(screenCenter.Y, -300),
            5f,
            "☢️ RADIATION ☢️",
            Color.LimeGreen,
            1.5f
        );
        
        entitySystem.CreateEntity<FloatingPopUpText>(
            UIConstants.GetCenteredPositionWithOffset(screenCenter.Y, 300),
            5f,
            "☢️ RADIATION ☢️",
            Color.LimeGreen,
            1.5f
        );

        yield return null;
    }
}
