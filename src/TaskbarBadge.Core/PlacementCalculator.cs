namespace TaskbarBadge.Core;

public static class PlacementCalculator
{
    public static DesktopRect Calculate(BadgeConfig config, TaskbarSnapshot taskbar)
    {
        var normalized = ConfigValidation.Normalize(config);
        var bounds = taskbar.Bounds;

        return taskbar.Edge switch
        {
            TaskbarEdge.Top or TaskbarEdge.Bottom => CalculateHorizontalTaskbar(normalized, bounds),
            TaskbarEdge.Left or TaskbarEdge.Right => CalculateVerticalTaskbar(normalized, bounds),
            _ => CalculateHorizontalTaskbar(normalized, bounds)
        };
    }

    public static DesktopRect RepositionRelativeToTaskbar(
        DesktopRect badge,
        DesktopRect previousTaskbar,
        DesktopRect currentTaskbar)
    {
        return badge with
        {
            Left = RepositionCoordinate(
                badge.Left,
                badge.Width,
                previousTaskbar.Left,
                previousTaskbar.Width,
                currentTaskbar.Left,
                currentTaskbar.Width),
            Top = RepositionCoordinate(
                badge.Top,
                badge.Height,
                previousTaskbar.Top,
                previousTaskbar.Height,
                currentTaskbar.Top,
                currentTaskbar.Height)
        };
    }

    private static DesktopRect CalculateHorizontalTaskbar(BadgeConfig config, DesktopRect bounds)
    {
        var left = config.Anchor switch
        {
            BadgeAnchor.Start => bounds.Left + config.AlongTaskbarOffset,
            BadgeAnchor.Center => bounds.Left + ((bounds.Width - config.Width) / 2) + config.AlongTaskbarOffset,
            BadgeAnchor.End => bounds.Right - config.Width - config.AlongTaskbarOffset,
            _ => bounds.Right - config.Width - config.AlongTaskbarOffset
        };

        var top = bounds.Top + ((bounds.Height - config.Height) / 2) + config.CrossTaskbarOffset;
        return new DesktopRect(left, top, config.Width, config.Height);
    }

    private static DesktopRect CalculateVerticalTaskbar(BadgeConfig config, DesktopRect bounds)
    {
        var top = config.Anchor switch
        {
            BadgeAnchor.Start => bounds.Top + config.AlongTaskbarOffset,
            BadgeAnchor.Center => bounds.Top + ((bounds.Height - config.Height) / 2) + config.AlongTaskbarOffset,
            BadgeAnchor.End => bounds.Bottom - config.Height - config.AlongTaskbarOffset,
            _ => bounds.Bottom - config.Height - config.AlongTaskbarOffset
        };

        var left = bounds.Left + ((bounds.Width - config.Width) / 2) + config.CrossTaskbarOffset;
        return new DesktopRect(left, top, config.Width, config.Height);
    }

    private static double RepositionCoordinate(
        double itemStart,
        double itemExtent,
        double previousStart,
        double previousExtent,
        double currentStart,
        double currentExtent)
    {
        var previousRange = previousExtent - itemExtent;
        var currentRange = currentExtent - itemExtent;

        if (Math.Abs(previousRange) < 0.001)
        {
            return currentStart + (currentRange / 2);
        }

        var relativePosition = (itemStart - previousStart) / previousRange;
        return currentStart + (relativePosition * currentRange);
    }
}
