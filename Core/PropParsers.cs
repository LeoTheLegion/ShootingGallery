using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace ShootingGallery.Core;

/// <summary>
/// Shared parsers for scene XML property values (colors, floats), used by both the
/// <see cref="SceneLoader"/> and data-driven components so parsing rules live in one place.
/// </summary>
public static class PropParsers
{
    public static float ParseFloat(string value)
        => float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);

    /// <summary>Parses a named color (e.g. "LimeGreen") or an "R,G,B[,A]" string.</summary>
    public static Color ParseColor(string value)
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
