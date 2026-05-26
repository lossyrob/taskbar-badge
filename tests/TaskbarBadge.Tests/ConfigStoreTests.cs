using TaskbarBadge.Core;

namespace TaskbarBadge.Tests;

public sealed class ConfigStoreTests
{
    [Fact]
    public void LoadOrDefaultCreatesDefaultConfigWhenMissing()
    {
        var directory = CreateTempDirectory();
        var store = new ConfigStore(Path.Combine(directory, "config.json"));

        var config = store.LoadOrDefault();

        Assert.True(File.Exists(store.ConfigPath));
        Assert.False(string.IsNullOrWhiteSpace(config.Label));
        Assert.Equal("#0078D4", config.BackgroundColor);
    }

    [Fact]
    public void LoadOrDefaultBacksUpMalformedJson()
    {
        var directory = CreateTempDirectory();
        var path = Path.Combine(directory, "config.json");
        File.WriteAllText(path, "{ not json");
        var store = new ConfigStore(path);

        var config = store.LoadOrDefault();

        Assert.Equal(BadgeConfig.Default.BackgroundColor, config.BackgroundColor);
        Assert.Contains(Directory.EnumerateFiles(directory), file => file.Contains(".invalid-", StringComparison.Ordinal));
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "TaskbarBadge.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
