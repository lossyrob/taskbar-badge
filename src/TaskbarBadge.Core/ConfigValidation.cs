using System.Globalization;

namespace TaskbarBadge.Core;

public static class ConfigValidation
{
    public static BadgeConfig Normalize(BadgeConfig config)
    {
        return config with
        {
            Label = string.IsNullOrWhiteSpace(config.Label) ? Environment.MachineName : config.Label.Trim(),
            BackgroundColor = NormalizeColor(config.BackgroundColor, "#0078D4"),
            TextColor = NormalizeColor(config.TextColor, "#FFFFFF"),
            Width = Clamp(config.Width, 80, 600),
            Height = Clamp(config.Height, 20, 96),
            FontSize = Clamp(config.FontSize, 8, 48),
            Opacity = Clamp(config.Opacity, 0.2, 1),
            AlongTaskbarOffset = Clamp(config.AlongTaskbarOffset, -2000, 4000),
            CrossTaskbarOffset = Clamp(config.CrossTaskbarOffset, -400, 400),
            OverlayLeft = NormalizeNullableCoordinate(config.OverlayLeft),
            OverlayTop = NormalizeNullableCoordinate(config.OverlayTop)
        };
    }

    private static double Clamp(double value, double min, double max)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return min;
        }

        return Math.Min(Math.Max(value, min), max);
    }

    private static string NormalizeColor(string? color, string fallback)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return fallback;
        }

        var value = color.Trim();
        if (!value.StartsWith('#'))
        {
            value = $"#{value}";
        }

        if (value.Length != 7)
        {
            return fallback;
        }

        return int.TryParse(value[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _)
            ? value.ToUpperInvariant()
            : fallback;
    }

    private static double? NormalizeNullableCoordinate(double? value)
    {
        if (value is null || double.IsNaN(value.Value) || double.IsInfinity(value.Value))
        {
            return null;
        }

        return Clamp(value.Value, -10000, 10000);
    }
}
