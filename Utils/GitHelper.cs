namespace CodeContext.Utils;

/// <summary>
/// Provides Git repository utilities using pure functional recursion.
/// </summary>
public static class GitHelper
{
    /// <summary>
    /// Finds the root directory of the Git repository containing the specified path.
    /// Uses tail-recursive search through parent directories.
    /// </summary>
    /// <param name="startPath">The path to start searching from.</param>
    /// <returns>The Git repository root path, or null if not in a Git repository.</returns>
    public static string? FindRepositoryRoot(string? startPath) =>
        string.IsNullOrEmpty(startPath) || !Directory.Exists(startPath)
            ? null
            : FindRepositoryRootRecursive(startPath);

    /// <summary>
    /// Determines if the specified path is within a Git repository.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is within a Git repository; otherwise, false.</returns>
    public static bool IsInRepository(string? path) =>
        FindRepositoryRoot(path) != null;

    /// <summary>
    /// Pure recursive function to find git repository root.
    /// Walks up directory tree until .git folder is found or root is reached.
    /// </summary>
    private static string? FindRepositoryRootRecursive(string currentPath) =>
        HasGitDirectory(currentPath)
            ? currentPath
            : Path.GetDirectoryName(currentPath) switch
            {
                null => null,
                var parent when string.IsNullOrEmpty(parent) => null,
                var parent => FindRepositoryRootRecursive(parent)
            };

    /// <summary>
    /// I/O operation: checks if a directory contains a .git subdirectory.
    /// </summary>
    private static bool HasGitDirectory(string path) =>
        Directory.Exists(Path.Combine(path, ".git"));
}
