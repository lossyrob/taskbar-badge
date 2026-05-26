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
}
