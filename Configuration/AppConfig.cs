namespace CodeContext.Configuration;

/// <summary>
/// Application configuration loaded from config.json.
/// </summary>
public record AppConfig
{
    public string DefaultInputPath { get; init; } = ".";
    public string DefaultOutputFileName { get; init; } = "context.txt";
    public string OutputFormat { get; init; } = "text";
    public bool IncludeStructure { get; init; } = true;
    public bool IncludeContents { get; init; } = true;
}
