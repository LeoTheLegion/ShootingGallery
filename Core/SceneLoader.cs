using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

/// <summary>
/// Loads a scene from an XML definition file and instantiates its entities.
/// Positions use resolution-independent anchors so layouts stay centered at any window size.
/// GUI is fully data-driven: a GameObject declares its components (Canvas/Label/Button) via
/// &lt;Property Name="Component" /&gt; entries, and button clicks are resolved from a Command-name
/// lookup to the component's Clicked event.
/// </summary>
public static class SceneLoader
{
    /// <summary>
    /// Loads the named scene (e.g. "start_menu") from the Content folder and adds its
    /// entities to the given system.
    /// </summary>
    /// <param name="system">The EntitySystem to add entities to.</param>
    /// <param name="sceneName">Scene file name without extension (e.g. "start_menu").</param>
    /// <param name="commands">Map of Command names (from XML) to the delegates that handle them.</param>
    /// <returns>A lookup of each entity's <c>Id</c> attribute to the created entity, for wiring typed references.</returns>
    public static Dictionary<string, Entity> LoadScene(EntitySystem system, string sceneName, Dictionary<string, Action> commands)
    {
        // Load through CE's asset system (refcounted, content-manager-backed),
        // consistent with how sprites/fonts are pulled. A missing file throws from XMLAsset.Load.
        var xmlAsset = AssetManager.LoadAsset<XMLAsset>(sceneName + ".xml");
        string xml = xmlAsset.XMLContent;
        if (string.IsNullOrWhiteSpace(xml))
            throw new FileNotFoundException($"Scene definition '{sceneName}.xml' is empty.");

        var root = XDocument.Parse(xml).Root;
        if (root == null || root.Name.LocalName != "Scene")
            throw new FormatException($"'{sceneName}.xml' must have a <Scene> root element.");

        var byId = new Dictionary<string, Entity>(StringComparer.Ordinal);

        // The XML hierarchy maps directly onto the entity hierarchy: entities nested under a
        // parent element become its children (e.g. GUI nested under a GameObject canvas root).
        foreach (var entityElem in root.Elements("Entity"))
            RegisterEntity(system, entityElem, null, commands, byId);

        return byId;
    }

    /// <summary>Creates the entity for an &lt;Entity&gt; element and recursively creates its nested children.</summary>
    private static void RegisterEntity(EntitySystem system, XElement entityElem, Entity parent, Dictionary<string, Action> commands, Dictionary<string, Entity> byId)
    {
        var position = ResolvePosition(entityElem);
        var props = ParseProperties(entityElem);
        string type = GetEntityType(entityElem);
        Entity entity = parent == null
            ? CreateEntity(system, type, position, props)
            : CreateEntity(system, type, position, props, commands, parent);

        string id = entityElem.Attribute("Id")?.Value;
        if (!string.IsNullOrWhiteSpace(id))
            byId[id] = entity;

        foreach (var childElem in entityElem.Elements("Entity"))
            RegisterEntity(system, childElem, entity, commands, byId);
    }

    /// <summary>Creates a top-level (parentless) entity.</summary>
    private static Entity CreateEntity(EntitySystem system, string type, Vector2 position, Dictionary<string, string> props)
    {
        switch (type)
        {
            case "GameObject":
                // Top-level GameObjects are structural (e.g. a canvas root); no click wiring.
                return system.CreateEntity<GameObject>(ResolveComponents(props), props, null);
            case "FloatingPopUpText":
                // Transient popups carry their own screen-space canvas (they are also spawned at
                // runtime far from the scene), so they are started standalone and not parented.
                return CreateFloatingPopUp(system, position, props);
            default:
                throw new NotSupportedException($"SceneLoader: unknown entity type '{type}'.");
        }
    }

    /// <summary>Creates an entity nested under a parent in the XML.</summary>
    private static Entity CreateEntity(EntitySystem system, string type, Vector2 position, Dictionary<string, string> props, Dictionary<string, Action> commands, Entity parent)
    {
        switch (type)
        {
            case "GameObject":
                // Data-driven container, label, or button: create unstarted, position it, parent it
                // under the canvas root, then start so widget components can resolve the ancestor
                // CanvasComponent when they attach. A Command property is resolved to the click
                // handler wired into any attached component's Clicked event.
                var gameObj = (GameObject)system.CreateEntityUnstarted(typeof(GameObject), ResolveComponents(props), props, ResolveCommand(props, commands));
                gameObj.LocalPosition = position;
                parent.AddChild(gameObj);
                gameObj.OnStart();
                return gameObj;
            default:
                throw new NotSupportedException($"SceneLoader: unknown entity type '{type}' under a parent.");
        }
    }

    /// <summary>Resolves an XML Command name to its registered delegate, or null when absent/unknown.</summary>
    private static Action ResolveCommand(Dictionary<string, string> props, Dictionary<string, Action> commands)
    {
        if (props.TryGetValue("Command", out var cmd) && commands.TryGetValue(cmd, out var action))
            return action;
        return null;
    }

    private static Entity CreateFloatingPopUp(EntitySystem system, Vector2 position, Dictionary<string, string> props)
    {
        float time = PropParsers.ParseFloat(props.GetValueOrDefault("Time", "5"));
        Color color = props.TryGetValue("Color", out var c) ? PropParsers.ParseColor(c) : Color.White;
        float scale = PropParsers.ParseFloat(props.GetValueOrDefault("Scale", "1.0"));
        bool radiation = props.TryGetValue("RadiationEffect", out var r) && bool.Parse(r);
        return system.CreateEntity<FloatingPopUpText>(position, time, props.GetValueOrDefault("Text", string.Empty), color, scale, radiation);
    }

    /// <summary>Reads the required Type attribute of an &lt;Entity&gt; element.</summary>
    private static string GetEntityType(XElement element)
        => element.Attribute("Type")?.Value ?? throw new FormatException("Entity missing 'Type' attribute.");

    /// <summary>
    /// Resolves the component types declared on a GameObject via its
    /// <c>&lt;Property Name="Component" Value="..." /&gt;</c> entries (e.g. "CanvasComponent").
    /// </summary>
    private static Type[] ResolveComponents(Dictionary<string, string> props)
    {
        var names = props.Where(p => string.Equals(p.Key, "Component", StringComparison.OrdinalIgnoreCase))
            .Select(p => p.Value)
            .ToList();

        return names.Select(ResolveComponentType).ToArray();
    }

    private static Type ResolveComponentType(string name)
    {
        // Local (game-specific) components win first so, e.g., "LabelComponent" resolves to our
        // live-updating wrapper rather than CE's attach-only built-in of the same name.
        var local = Assembly.GetExecutingAssembly().GetType($"ShootingGallery.Core.{name}");
        if (local != null) return local;

        // Fall back to CE's built-in components (CanvasComponent, ButtonComponent, ...).
        var builtIn = typeof(Entity).Assembly.GetType($"CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn.{name}");
        return builtIn ?? throw new NotSupportedException($"SceneLoader: unknown component '{name}' on GameObject.");
    }

    /// <summary>
    /// Converts anchor + offset attributes into a concrete screen position.
    /// HAnchor: Center (default) | Left | Right. VAnchor: Top (default) | Bottom | Middle.
    /// </summary>
    private static Vector2 ResolvePosition(XElement element)
    {
        string hAnchor = (element.Attribute("HAnchor")?.Value ?? "Center").ToLowerInvariant();
        string vAnchor = (element.Attribute("VAnchor")?.Value ?? "Top").ToLowerInvariant();
        float x = PropParsers.ParseFloat(element.Attribute("X")?.Value ?? "0");
        float y = PropParsers.ParseFloat(element.Attribute("Y")?.Value ?? "0");

        float px = hAnchor switch
        {
            "left" => x,
            "right" => ScreenManager.ScreenWidth - x,
            _ => ScreenManager.ScreenCenter.X + x,
        };
        float py = vAnchor switch
        {
            "bottom" => ScreenManager.ScreenHeight - y,
            "middle" => ScreenManager.ScreenCenter.Y + y,
            _ => y,
        };
        return new Vector2(px, py);
    }

    private static Dictionary<string, string> ParseProperties(XElement element)
    {
        var props = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var prop in element.Elements("Property"))
        {
            var name = prop.Attribute("Name")?.Value;
            var value = prop.Attribute("Value")?.Value;
            if (!string.IsNullOrWhiteSpace(name))
                props[name] = value ?? string.Empty;
        }
        return props;
    }

}
