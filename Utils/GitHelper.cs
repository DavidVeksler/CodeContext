namespace CodeContext.Utils;

/// <summary>
/// Provides Git repository utilities.
/// </summary>
public static class GitHelper
{
    /// <summary>
    /// Finds the root directory of the Git repository containing the specified path.
    /// </summary>
    /// <param name="startPath">The path to start searching from.</param>
    /// <returns>The Git repository root path, or null if not in a Git repository.</returns>
    public static string? FindRepositoryRoot(string? startPath)
    {
        if (string.IsNullOrEmpty(startPath) || !Directory.Exists(startPath))
        {
            return null;
        }

        var currentPath = startPath;
        while (!string.IsNullOrEmpty(currentPath))
        {
            if (Directory.Exists(Path.Combine(currentPath, ".git")))
            {
                return currentPath;
            }
            currentPath = Path.GetDirectoryName(currentPath);
        }

        return null;
    }

    /// <summary>
    /// Determines if the specified path is within a Git repository.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is within a Git repository; otherwise, false.</returns>
    public static bool IsInRepository(string? path)
    {
        return FindRepositoryRoot(path) != null;
    }
}
