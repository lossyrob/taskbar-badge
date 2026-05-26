namespace TaskbarBadge.Core;

public enum BadgeAnchor
{
    Start,
    Center,
    End
}

public sealed record BadgeConfig
{
    public string Label { get; init; } = Environment.MachineName;

    public string BackgroundColor { get; init; } = "#0078D4";

    public string TextColor { get; init; } = "#FFFFFF";

    public double Width { get; init; } = 220;

    public double Height { get; init; } = 32;

    public double FontSize { get; init; } = 14;

    public double Opacity { get; init; } = 0.92;

    public BadgeAnchor Anchor { get; init; } = BadgeAnchor.End;

    public double AlongTaskbarOffset { get; init; } = 320;

    public double CrossTaskbarOffset { get; init; } = 0;

    public double? OverlayLeft { get; init; }

    public double? OverlayTop { get; init; }

    public bool LockOverlayPosition { get; init; }

    public bool ShowBadge { get; init; }

    public bool RunAtStartup { get; init; } = true;

    public static BadgeConfig Default => ConfigValidation.Normalize(new BadgeConfig());
}
