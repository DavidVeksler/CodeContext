using CodeContext.Interfaces;
using CodeContext.Utils;

namespace CodeContext.Services;

/// <summary>
/// Resolves and validates input and output paths with user interaction.
/// Separates pure path operations from I/O side effects.
/// </summary>
public class PathResolver
{
    private readonly IConsoleWriter _console;

    public PathResolver(IConsoleWriter console)
    {
        _console = console;
    }

    /// <summary>
    /// Gets and validates the input directory path (I/O operation).
    /// </summary>
    /// <param name="defaultPath">The default path to use if user doesn't provide one.</param>
    /// <returns>The validated full path to the input directory.</returns>
    public string GetInputPath(string defaultPath)
    {
        var userPath = _console.ReadLine() ?? string.Empty;
        var selectedPath = SelectPath(userPath, defaultPath);
        var fullPath = Path.GetFullPath(selectedPath);

        return Guard.DirectoryExists(fullPath, nameof(fullPath));
    }

    /// <summary>
    /// Gets and validates the output file path (I/O operation).
    /// </summary>
    /// <param name="commandLineArg">Optional output path from command-line arguments.</param>
    /// <param name="defaultPath">Default output path if none provided.</param>
    /// <returns>The validated full output path.</returns>
    public string GetOutputPath(string? commandLineArg, string defaultPath)
    {
        var selectedPath = commandLineArg switch
        {
            not null when !string.IsNullOrWhiteSpace(commandLineArg) => commandLineArg,
            _ => SelectPath(_console.ReadLine() ?? string.Empty, defaultPath)
        };

        return Path.GetFullPath(selectedPath);
    }

    /// <summary>
    /// Pure function: selects between user input and default path.
    /// </summary>
    private static string SelectPath(string userInput, string defaultPath) =>
        string.IsNullOrWhiteSpace(userInput) ? defaultPath : userInput;

    /// <summary>
    /// Pure function: extracts a clean folder name from the input path for output file naming.
    /// Uses functional composition to handle edge cases.
    /// </summary>
    /// <param name="path">The input path.</param>
    /// <returns>A sanitized folder name.</returns>
    public static string GetFolderName(string path) =>
        TryGetDirectoryName(path)
        ?? TryGetCurrentDirectoryName()
        ?? (IsPathSeparatorTerminated(path) ? ExtractRootFolderName(path) ?? "root" : "root");

    /// <summary>
    /// Pure function: attempts to get directory name from path.
    /// Returns null if invalid or current directory marker.
    /// </summary>
    private static string? TryGetDirectoryName(string path)
    {
        var name = new DirectoryInfo(path).Name;
        return !string.IsNullOrEmpty(name) && name != "." ? name : null;
    }

    /// <summary>
    /// I/O function: gets current directory name.
    /// </summary>
    private static string? TryGetCurrentDirectoryName() =>
        new DirectoryInfo(Environment.CurrentDirectory).Name;

    /// <summary>
    /// Pure predicate: checks if path ends with directory separator.
    /// </summary>
    private static bool IsPathSeparatorTerminated(string path) =>
        path.EndsWith(Path.DirectorySeparatorChar) ||
        path.EndsWith(Path.AltDirectorySeparatorChar);

    /// <summary>
    /// Pure function: extracts root folder name from path.
    /// Removes path separators and drive colons.
    /// </summary>
    private static string? ExtractRootFolderName(string path) =>
        Path.GetPathRoot(Path.GetFullPath(path)) switch
        {
            null or "" => null,
            var root => CleanRootPath(root) switch
            {
                "" => null,
                var cleaned => cleaned
            }
        };

    /// <summary>
    /// Pure function: removes separators and colons from root path.
    /// </summary>
    private static string CleanRootPath(string root) =>
        root.Replace(Path.DirectorySeparatorChar.ToString(), string.Empty)
            .Replace(Path.AltDirectorySeparatorChar.ToString(), string.Empty)
            .Replace(":", string.Empty);
}
