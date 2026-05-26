using TaskbarBadge.Core;

namespace TaskbarBadge.Tests;

public sealed class ConfigValidationTests
{
    [Fact]
    public void NormalizeClampsNumericValues()
    {
        var config = ConfigValidation.Normalize(new BadgeConfig
        {
            Width = 10,
            Height = 999,
            FontSize = double.NaN,
            Opacity = 7
        });

        Assert.Equal(80, config.Width);
        Assert.Equal(96, config.Height);
        Assert.Equal(8, config.FontSize);
        Assert.Equal(1, config.Opacity);
    }

    [Fact]
    public void NormalizeAcceptsHexColorsWithoutHash()
    {
        var config = ConfigValidation.Normalize(new BadgeConfig
        {
            BackgroundColor = "ff00aa",
            TextColor = "123456"
        });

        Assert.Equal("#FF00AA", config.BackgroundColor);
        Assert.Equal("#123456", config.TextColor);
    }

    [Fact]
    public void DefaultConfigKeepsOverlayHidden()
    {
        Assert.False(BadgeConfig.Default.ShowBadge);
    }
}
