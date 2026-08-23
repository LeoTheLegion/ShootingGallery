using System;
using System.Collections.Generic;
using System.Reflection;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

/// <summary>
/// A generic, data-driven container entity. It carries no gameplay logic of its own; instead a
/// scene XML declares which components to attach (e.g. a screen-space <c>CanvasComponent</c>,
/// making this the UI root that other GUI entities are parented under, a <c>LabelComponent</c>
/// turning it into a text label, or a CE <c>ButtonComponent</c> with a <c>Command</c> property
/// turning it into a clickable button). This mirrors Unity's "GameObject + components" pattern
/// and avoids creating one-off entity classes just to hold a particular component combination.
/// </summary>
public class GameObject : Entity
{
    private readonly Type[] _componentTypes;
    private readonly Dictionary<string, string> _props;
    private readonly Action _onClick;

    /// <param name="componentTypes">Component types with a parameterless or all-optional-parameter constructor.</param>
    public GameObject(params Type[] componentTypes) : this(componentTypes, null, null)
    {
    }

    /// <param name="componentTypes">Component types with a parameterless or all-optional-parameter constructor.</param>
    /// <param name="props">Scene XML properties, applied to each declared component before it is
    /// attached (via <see cref="IConfigurableComponent"/> or by matching public property names).</param>
    /// <param name="onClick">Optional click handler, resolved from a <c>Command</c> property by the
    /// scene loader and subscribed to any attached component exposing a public <c>Clicked</c> event.</param>
    public GameObject(Type[] componentTypes, Dictionary<string, string> props, Action onClick)
    {
        _componentTypes = componentTypes ?? Array.Empty<Type>();
        _props = props;
        _onClick = onClick;
    }

    public override void OnStart()
    {
        base.OnStart();
        // Attach the declared components as soon as this entity starts so that GUI entities
        // parented under it can resolve them (e.g. its canvas) when their own components attach.
        foreach (var type in _componentTypes)
        {
            var component = CreateComponent(type);
            if (_props != null)
                Configure(component, _props);
            AddComponent(component);
        }

        // Unity-style listener wiring: subscribe the resolved command to any attached component's
        // public Clicked event (e.g. CE's ButtonComponent).
        if (_onClick != null && _props != null && _props.TryGetValue("Command", out _))
            foreach (var component in Components)
                SubscribeClicked(component, _onClick);
    }

    /// <summary>
    /// Instantiates a component. Prefers a parameterless constructor; otherwise falls back to a
    /// public constructor whose parameters are all optional (e.g. CE's
    /// <c>CanvasComponent(bool isScreenSpace = true)</c>, which has no parameterless ctor).
    /// </summary>
    private static EntityComponent CreateComponent(Type type)
    {
        var parameterless = type.GetConstructor(Type.EmptyTypes);
        if (parameterless != null)
            return (EntityComponent)parameterless.Invoke(null);

        foreach (var candidate in type.GetConstructors())
        {
            var parameters = candidate.GetParameters();
            var args = new object[parameters.Length];
            bool allOptional = true;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].HasDefaultValue)
                {
                    allOptional = false;
                    break;
                }
                args[i] = parameters[i].DefaultValue;
            }

            if (allOptional)
                return (EntityComponent)candidate.Invoke(args);
        }

        throw new NotSupportedException($"GameObject: component '{type.Name}' needs a parameterless or all-optional-parameter constructor.");
    }

    /// <summary>Applies scene XML properties to a component before it attaches.</summary>
    private static void Configure(EntityComponent component, Dictionary<string, string> props)
    {
        if (component is IConfigurableComponent configurable)
        {
            configurable.Configure(props);
            return;
        }

        // Fallback for third-party components (e.g. CE's ButtonComponent): match XML property names
        // to the component's public writable properties, converting the string values.
        var type = component.GetType();
        foreach (var prop in props)
        {
            if (string.Equals(prop.Key, "Component", StringComparison.OrdinalIgnoreCase))
                continue;

            var member = type.GetProperty(prop.Key);
            if (member == null || !member.CanWrite || !member.PropertyType.IsPublic)
                continue;

            try
            {
                member.SetValue(component, ConvertValue(prop.Value, member.PropertyType));
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException)
            {
                Console.WriteLine($"GameObject: could not apply property '{prop.Key}' ({prop.Value}) to {type.Name}.");
            }
        }
    }

    private static object ConvertValue(string value, Type target)
    {
        if (target == typeof(Color))
            return PropParsers.ParseColor(value);
        if (target.IsPrimitive || target == typeof(string))
            return Convert.ChangeType(value, target, System.Globalization.CultureInfo.InvariantCulture);
        throw new InvalidCastException($"Unsupported property type {target.Name} for data-driven configuration.");
    }

    private static void SubscribeClicked(EntityComponent component, Action onClick)
    {
        var eventInfo = component.GetType().GetEvent("Clicked");
        if (eventInfo == null || eventInfo.EventHandlerType != typeof(Action))
            return;

        eventInfo.AddEventHandler(component, onClick);
    }
}
