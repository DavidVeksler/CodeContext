using CodeContext.Configuration;
using CodeContext.Services;

namespace CodeContext;

/// <summary>
/// Legacy compatibility wrapper for ProjectScanner.
/// Use ProjectScanner directly for new code.
/// </summary>
[Obsolete("Use ProjectScanner instead for better testability and maintainability.")]
public class MyAppsContext
{
    private static readonly Lazy<ProjectScanner> _instance = new(() =>
    {
        var config = new FilterConfiguration();
        var fileChecker = new FileFilterService(config);
        var console = new ConsoleWriter();
        return new ProjectScanner(fileChecker, console);
    });

    /// <summary>
    /// Gets the git repository root path.
    /// </summary>
    public static string? GitRepoRoot => _instance.Value.GitRepoRoot;

    /// <summary>
    /// Gets user input with a prompt.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <returns>The user's input.</returns>
    public static string GetUserInput(string prompt)
    {
        return _instance.Value.GetUserInput(prompt);
    }

    /// <summary>
    /// Generates a hierarchical structure representation of the project directory.
    /// </summary>
    /// <param name="path">The directory path to scan.</param>
    /// <param name="indent">Current indentation level (used for recursion).</param>
    /// <returns>A string representation of the directory structure.</returns>
    public static string GetProjectStructure(string path, int indent = 0)
    {
        return _instance.Value.GetProjectStructure(path, indent);
    }

    /// <summary>
    /// Retrieves the contents of all non-filtered files in the directory tree.
    /// </summary>
    /// <param name="path">The directory path to scan.</param>
    /// <returns>A string containing all file contents with separators.</returns>
    public static string GetFileContents(string path)
    {
        return _instance.Value.GetFileContents(path);
    }
}