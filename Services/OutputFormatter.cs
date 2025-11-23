using System.Text.Json;
using CodeContext.Interfaces;

namespace CodeContext.Services;

/// <summary>
/// Functional output formatter with separated I/O and formatting logic.
/// </summary>
public class OutputFormatter
{
    private readonly IConsoleWriter _console;

    public OutputFormatter(IConsoleWriter console)
    {
        _console = console;
    }

    /// <summary>
    /// Writes content to the specified output location (I/O operation).
    /// Composed from pure formatting and impure I/O functions.
    /// </summary>
    /// <param name="outputTarget">Target path (file or directory).</param>
    /// <param name="content">Content to write.</param>
    /// <param name="format">Output format (text or json).</param>
    /// <param name="defaultFileName">Filename to use if outputTarget is a directory.</param>
    /// <returns>The actual path where the file was written.</returns>
    public string WriteToFile(string outputTarget, string content, string format, string defaultFileName)
    {
        _console.WriteLine("\n💾 Writing output...");

        var resolvedPath = ResolveOutputPath(outputTarget, defaultFileName);
        var formattedContent = FormatContent(content, format, DateTime.Now);

        WriteFile(resolvedPath, formattedContent);

        return resolvedPath;
    }

    /// <summary>
    /// I/O operation: resolves output path based on target type.
    /// Checks if directory exists before combining paths.
    /// </summary>
    private static string ResolveOutputPath(string outputTarget, string defaultFileName) =>
        Directory.Exists(outputTarget)
            ? Path.Combine(outputTarget, defaultFileName)
            : outputTarget;

    /// <summary>
    /// I/O operation: ensures directory exists and writes file.
    /// </summary>
    private static void WriteFile(string filePath, string content)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, content);
    }

    /// <summary>
    /// Pure function: formats content based on output format.
    /// </summary>
    private static string FormatContent(string content, string format, DateTime timestamp) =>
        format.Equals("json", StringComparison.OrdinalIgnoreCase)
            ? SerializeToJson(content, timestamp)
            : content;

    /// <summary>
    /// Pure function: serializes content to JSON with provided timestamp.
    /// Deterministic - same inputs always produce same output.
    /// </summary>
    private static string SerializeToJson(string content, DateTime timestamp) =>
        JsonSerializer.Serialize(
            new { content, timestamp },
            new JsonSerializerOptions { WriteIndented = true });
}
