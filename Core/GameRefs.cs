using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace ShootingGallery.Core;

/// <summary>
/// Static bridge that lets components reach the live <see cref="EntitySystem"/>. Components
/// have no public path to their owning entity's system (CE issue #80), so scene code publishes
/// this reference before entities start updating. It is the single seam that lets fully
/// data-driven components spawn, query, and destroy entities without custom Entity subclasses.
/// </summary>
public static class GameRefs
{
    public static EntitySystem EntitySystem { get; set; }
}
