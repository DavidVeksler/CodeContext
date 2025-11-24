namespace CodeContext.Utils;

/// <summary>
/// Security utilities for path validation to prevent path traversal attacks.
/// </summary>
public static class PathSecurity
{
    /// <summary>
    /// Validates that a resolved path is within the allowed root directory.
    /// Prevents path traversal attacks using ".." or absolute paths.
    /// </summary>
    /// <param name="rootPath">The root directory that paths must be within.</param>
    /// <param name="relativePath">The relative path to validate.</param>
    /// <returns>The validated full path if safe, null if path traversal detected.</returns>
    /// <exception cref="SecurityException">Thrown when path traversal is detected.</exception>
    public static string ValidatePathWithinRoot(string rootPath, string relativePath)
    {
        Guard.NotNullOrEmpty(rootPath, nameof(rootPath));
        Guard.NotNullOrEmpty(relativePath, nameof(relativePath));

        // Get absolute paths for comparison
        var absoluteRoot = Path.GetFullPath(rootPath);
        var combinedPath = Path.Combine(absoluteRoot, relativePath);
        var absoluteCombined = Path.GetFullPath(combinedPath);

        // Ensure the resolved path is within the root directory
        if (!absoluteCombined.StartsWith(absoluteRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityException(
                $"Path traversal detected: '{relativePath}' resolves outside root directory. " +
                $"Root: {absoluteRoot}, Resolved: {absoluteCombined}");
        }

        return absoluteCombined;
    }

    /// <summary>
    /// Tries to validate a path, returning false if path traversal is detected.
    /// </summary>
    public static bool TryValidatePathWithinRoot(string rootPath, string relativePath, out string? validatedPath)
    {
        try
        {
            validatedPath = ValidatePathWithinRoot(rootPath, relativePath);
            return true;
        }
        catch (SecurityException)
        {
            validatedPath = null;
            return false;
        }
    }
}

/// <summary>
/// Exception thrown when a security violation is detected.
/// </summary>
public class SecurityException : Exception
{
    public SecurityException(string message) : base(message) { }
    public SecurityException(string message, Exception innerException) : base(message, innerException) { }
}
