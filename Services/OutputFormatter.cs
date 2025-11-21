using System.Text.Json;
using CodeContext.Interfaces;

namespace CodeContext.Services;

/// <summary>
/// Formats and writes output to files in various formats.
/// </summary>
public class OutputFormatter
{
    private readonly IConsoleWriter _console;

    public OutputFormatter(IConsoleWriter console)
    {
        _console = console;
    }

    /// <summary>
    /// Writes content to the specified output location.
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
        EnsureDirectoryExists(resolvedPath);

        var formattedContent = FormatContent(content, format);
        File.WriteAllText(resolvedPath, formattedContent);

        return resolvedPath;
    }

    private static string ResolveOutputPath(string outputTarget, string defaultFileName)
    {
        return Directory.Exists(outputTarget)
            ? Path.Combine(outputTarget, defaultFileName)
            : outputTarget;
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static string FormatContent(string content, string format)
    {
        return format.ToLower() == "json"
            ? JsonSerializer.Serialize(
                new { content, timestamp = DateTime.Now },
                new JsonSerializerOptions { WriteIndented = true })
            : content;
    }
}
