using CodeContext.Interfaces;
using CodeContext.Utils;

namespace CodeContext.Services;

/// <summary>
/// Resolves and validates input and output paths with user interaction.
/// </summary>
public class PathResolver
{
    private readonly IConsoleWriter _console;

    public PathResolver(IConsoleWriter console)
    {
        _console = console;
    }

    /// <summary>
    /// Gets and validates the input directory path.
    /// </summary>
    /// <param name="defaultPath">The default path to use if user doesn't provide one.</param>
    /// <returns>The validated full path to the input directory.</returns>
    public string GetInputPath(string defaultPath)
    {
        var userPath = _console.ReadLine() ?? string.Empty;
        var selectedPath = string.IsNullOrWhiteSpace(userPath) ? defaultPath : userPath;
        var fullPath = Path.GetFullPath(selectedPath);

        return Guard.DirectoryExists(fullPath, nameof(fullPath));
    }

    /// <summary>
    /// Gets and validates the output file path.
    /// </summary>
    /// <param name="commandLineArg">Optional output path from command-line arguments.</param>
    /// <param name="defaultPath">Default output path if none provided.</param>
    /// <returns>The validated full output path.</returns>
    public string GetOutputPath(string? commandLineArg, string defaultPath)
    {
        if (!string.IsNullOrWhiteSpace(commandLineArg))
        {
            return Path.GetFullPath(commandLineArg);
        }

        var userInput = _console.ReadLine() ?? string.Empty;
        return string.IsNullOrWhiteSpace(userInput)
            ? defaultPath
            : Path.GetFullPath(userInput);
    }

    /// <summary>
    /// Extracts a clean folder name from the input path for output file naming.
    /// </summary>
    /// <param name="path">The input path.</param>
    /// <returns>A sanitized folder name.</returns>
    public static string GetFolderName(string path)
    {
        var folderName = new DirectoryInfo(path).Name;

        if (!string.IsNullOrEmpty(folderName) && folderName != ".")
        {
            return folderName;
        }

        folderName = new DirectoryInfo(Environment.CurrentDirectory).Name;

        if (IsPathSeparatorTerminated(path))
        {
            return ExtractRootFolderName(path) ?? "root";
        }

        return folderName;
    }

    private static bool IsPathSeparatorTerminated(string path)
    {
        return path.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
               path.EndsWith(Path.AltDirectorySeparatorChar.ToString());
    }

    private static string? ExtractRootFolderName(string path)
    {
        var root = Path.GetPathRoot(Path.GetFullPath(path));
        if (string.IsNullOrEmpty(root))
        {
            return null;
        }

        var cleanedRoot = root
            .Replace(Path.DirectorySeparatorChar.ToString(), "")
            .Replace(Path.AltDirectorySeparatorChar.ToString(), "")
            .Replace(":", "");

        return string.IsNullOrEmpty(cleanedRoot) ? null : cleanedRoot;
    }
}
