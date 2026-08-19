using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

/// <summary>
/// Loads a scene from an XML definition file and instantiates its entities.
/// Positions use resolution-independent anchors so layouts stay centered at any window size.
/// Entity configuration is applied through the existing entity constructors/methods, and
/// <see cref="ButtonEntity"/> clicks are resolved to delegates via a Command-name lookup.
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
        foreach (var entityElem in root.Elements("Entity"))
        {
            string type = entityElem.Attribute("Type")?.Value ?? throw new FormatException("Entity missing 'Type' attribute.");
            string id = entityElem.Attribute("Id")?.Value;
            var position = ResolvePosition(entityElem);
            var props = ParseProperties(entityElem);
            var entity = CreateEntity(system, type, position, props, commands);
            if (entity != null && !string.IsNullOrWhiteSpace(id))
                byId[id] = entity;
        }
        return byId;
    }

    private static Entity CreateEntity(EntitySystem system, string type, Vector2 position, Dictionary<string, string> props, Dictionary<string, Action> commands)
    {
        switch (type)
        {
            case "TextEntity":
            {
                var entity = system.CreateEntity<TextEntity>(position, props.GetValueOrDefault("Text", string.Empty));
                if (props.TryGetValue("Color", out var color)) entity.SetColor(ParseColor(color));
                if (props.TryGetValue("Scale", out var scale)) entity.SetScale(ParseFloat(scale));
                return entity;
            }
            case "ButtonEntity":
            {
                Action onClick = null;
                if (props.TryGetValue("Command", out var cmd) && commands.TryGetValue(cmd, out var action))
                    onClick = action;
                return system.CreateEntity<ButtonEntity>(position, props.GetValueOrDefault("Text", string.Empty), onClick);
            }
            case "FloatingPopUpText":
            {
                float time = ParseFloat(props.GetValueOrDefault("Time", "5"));
                Color color = props.TryGetValue("Color", out var c) ? ParseColor(c) : Color.White;
                float scale = ParseFloat(props.GetValueOrDefault("Scale", "1.0"));
                return system.CreateEntity<FloatingPopUpText>(position, time, props.GetValueOrDefault("Text", string.Empty), color, scale);
            }
            default:
                throw new NotSupportedException($"SceneLoader: unknown entity type '{type}'.");
        }
    }

    /// <summary>
    /// Converts anchor + offset attributes into a concrete screen position.
    /// HAnchor: Center (default) | Left | Right. VAnchor: Top (default) | Bottom | Middle.
    /// </summary>
    private static Vector2 ResolvePosition(XElement element)
    {
        string hAnchor = (element.Attribute("HAnchor")?.Value ?? "Center").ToLowerInvariant();
        string vAnchor = (element.Attribute("VAnchor")?.Value ?? "Top").ToLowerInvariant();
        float x = ParseFloat(element.Attribute("X")?.Value ?? "0");
        float y = ParseFloat(element.Attribute("Y")?.Value ?? "0");

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

    private static float ParseFloat(string value)
        => float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);

    /// <summary>Parses a named color (e.g. "LimeGreen") or an "R,G,B[,A]" string.</summary>
    private static Color ParseColor(string value)
    {
        var field = typeof(Color).GetField(value, BindingFlags.Static | BindingFlags.Public);
        if (field != null)
            return (Color)field.GetValue(null)!;

        var parts = value.Split(',');
        if (parts.Length >= 3)
        {
            int r = int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            int g = int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
            int b = int.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
            int a = parts.Length >= 4 ? int.Parse(parts[3].Trim(), CultureInfo.InvariantCulture) : 255;
            return new Color(r, g, b, a);
        }
        return Color.White;
    }
}
