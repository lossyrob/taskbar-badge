namespace TaskbarBadge.Core;

public enum TaskbarEdge
{
    Left,
    Top,
    Right,
    Bottom
}

public readonly record struct DesktopRect(double Left, double Top, double Width, double Height)
{
    public double Right => Left + Width;

    public double Bottom => Top + Height;
}

public sealed record TaskbarSnapshot(DesktopRect Bounds, TaskbarEdge Edge);
