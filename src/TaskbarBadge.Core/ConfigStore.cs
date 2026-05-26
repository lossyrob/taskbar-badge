using System.Text.Json;

namespace TaskbarBadge.Core;

public sealed class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public ConfigStore(string configPath)
    {
        ConfigPath = configPath;
    }

    public string ConfigPath { get; }

    public string ConfigDirectory => Path.GetDirectoryName(ConfigPath)
        ?? throw new InvalidOperationException("Config path must include a directory.");

    public static ConfigStore CreateDefault()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TaskbarBadge");

        return new ConfigStore(Path.Combine(directory, "config.json"));
    }

    public BadgeConfig LoadOrDefault()
    {
        if (!File.Exists(ConfigPath))
        {
            var defaults = BadgeConfig.Default;
            Save(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<BadgeConfig>(json, JsonOptions);
            return ConfigValidation.Normalize(config ?? BadgeConfig.Default);
        }
        catch (JsonException)
        {
            var backupPath = $"{ConfigPath}.invalid-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
            File.Move(ConfigPath, backupPath, overwrite: true);
            var defaults = BadgeConfig.Default;
            Save(defaults);
            return defaults;
        }
    }

    public void Save(BadgeConfig config)
    {
        Directory.CreateDirectory(ConfigDirectory);
        var normalized = ConfigValidation.Normalize(config);
        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }
}
