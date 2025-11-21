using System.Text.Json;
using CodeContext.Configuration;

namespace CodeContext.Services;

/// <summary>
/// Loads application configuration from config.json.
/// </summary>
public class ConfigLoader
{
    private readonly IConsoleWriter _console;

    public ConfigLoader(IConsoleWriter console)
    {
        _console = console;
    }

    /// <summary>
    /// Loads configuration from config.json if it exists, otherwise returns default configuration.
    /// </summary>
    public AppConfig Load()
    {
        try
        {
            var configJson = File.Exists("config.json") ? File.ReadAllText("config.json") : "{}";
            return JsonSerializer.Deserialize<AppConfig>(configJson) ?? new AppConfig();
        }
        catch (JsonException ex)
        {
            _console.WriteLine($"⚠️ Warning: Invalid config.json format ({ex.Message}). Using defaults.");
            return new AppConfig();
        }
    }
}
