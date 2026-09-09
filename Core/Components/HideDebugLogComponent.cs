using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

namespace ShootingGallery.Core;

/// <summary>
/// Hides the on-screen debug sticky log. CE's MainGame calls Debug.StickyLog.LoadGUI() during
/// LoadContent (i.e. inside game.Run()), which creates the log grid visible — so setting
/// IsVisible before Run() is a no-op (the grid doesn't exist yet and the setter guards on it).
/// This component attaches after that point and hides the log for the whole session: the grid is
/// a global GUI element that scene transitions don't recreate, so hiding it once is enough.
/// Attached to the start menu's HUD root in Content/start_menu.xml.
/// </summary>
public class HideDebugLogComponent : EntityComponent
{
    public override void OnAttach()
    {
        base.OnAttach();
        Debug.StickyLog.IsVisible = false;
    }
}
