using System.Text.Json;
using CodeContext.Configuration;
using CodeContext.Interfaces;

namespace CodeContext.Services;

/// <summary>
/// Functional configuration loader with separated I/O and parsing logic.
/// </summary>
public class ConfigLoader
{
    private const string ConfigFileName = "config.json";
    private readonly IConsoleWriter _console;

    public ConfigLoader(IConsoleWriter console)
    {
        _console = console;
    }

    /// <summary>
    /// Loads configuration from config.json if it exists, otherwise returns default configuration.
    /// I/O operation with functional error handling.
    /// </summary>
    public AppConfig Load() =>
        ReadConfigFile(ConfigFileName)
            .Match(
                onSuccess: ParseConfig,
                onError: HandleParseError);

    /// <summary>
    /// I/O operation: reads config file or returns empty JSON.
    /// </summary>
    private static Result<string> ReadConfigFile(string fileName)
    {
        try
        {
            var json = File.Exists(fileName) ? File.ReadAllText(fileName) : "{}";
            return Result<string>.Success(json);
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex.Message);
        }
    }

    /// <summary>
    /// Pure function: parses JSON string into AppConfig.
    /// </summary>
    private AppConfig ParseConfig(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch (JsonException ex)
        {
            _console.WriteLine($"⚠️ Warning: Invalid config.json format ({ex.Message}). Using defaults.");
            return new AppConfig();
        }
    }

    /// <summary>
    /// Error handler: returns default config and logs error.
    /// </summary>
    private AppConfig HandleParseError(string error)
    {
        _console.WriteLine($"⚠️ Warning: Could not read config.json ({error}). Using defaults.");
        return new AppConfig();
    }

    /// <summary>
    /// Simple Result type for functional error handling.
    /// </summary>
    private abstract record Result<T>
    {
        private Result() { }

        public sealed record Success(T Value) : Result<T>;
        public sealed record Error(string Message) : Result<T>;

        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<string, TResult> onError) =>
            this switch
            {
                Success s => onSuccess(s.Value),
                Error e => onError(e.Message),
                _ => throw new InvalidOperationException()
            };
    }
}
