using TaskbarBadge.Core;

namespace TaskbarBadge.Tests;

public sealed class PlacementCalculatorTests
{
    [Fact]
    public void CalculatePlacesBadgeNearEndOfBottomTaskbar()
    {
        var config = BadgeConfig.Default with
        {
            Width = 200,
            Height = 30,
            Anchor = BadgeAnchor.End,
            AlongTaskbarOffset = 300
        };
        var taskbar = new TaskbarSnapshot(new DesktopRect(0, 1040, 1920, 40), TaskbarEdge.Bottom);

        var badge = PlacementCalculator.Calculate(config, taskbar);

        Assert.Equal(1420, badge.Left);
        Assert.Equal(1045, badge.Top);
    }

    [Fact]
    public void CalculatePlacesBadgeInCenterOfTopTaskbar()
    {
        var config = BadgeConfig.Default with
        {
            Width = 100,
            Height = 20,
            Anchor = BadgeAnchor.Center,
            AlongTaskbarOffset = 10
        };
        var taskbar = new TaskbarSnapshot(new DesktopRect(0, 0, 1000, 40), TaskbarEdge.Top);

        var badge = PlacementCalculator.Calculate(config, taskbar);

        Assert.Equal(460, badge.Left);
        Assert.Equal(10, badge.Top);
    }

    [Fact]
    public void CalculatePlacesBadgeOnVerticalTaskbar()
    {
        var config = BadgeConfig.Default with
        {
            Width = 90,
            Height = 24,
            Anchor = BadgeAnchor.Start,
            AlongTaskbarOffset = 16
        };
        var taskbar = new TaskbarSnapshot(new DesktopRect(0, 0, 80, 1200), TaskbarEdge.Left);

        var badge = PlacementCalculator.Calculate(config, taskbar);

        Assert.Equal(-5, badge.Left);
        Assert.Equal(16, badge.Top);
    }
}
