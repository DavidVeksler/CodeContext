using CodeContext.Configuration;
using CodeContext.Services;

namespace CodeContext;

/// <summary>
/// Legacy compatibility wrapper for FileFilterService.
/// Use FileFilterService directly for new code.
/// </summary>
[Obsolete("Use FileFilterService instead for better testability and maintainability.")]
public class FileChecker
{
    private static readonly Lazy<FileFilterService> _instance = new(() =>
        new FileFilterService(new FilterConfiguration()));

    /// <summary>
    /// Determines if a file or directory should be skipped during processing.
    /// </summary>
    /// <param name="info">The file or directory information.</param>
    /// <param name="rootPath">The root path of the project being scanned.</param>
    /// <returns>True if the file/directory should be skipped; otherwise, false.</returns>
    public static bool ShouldSkip(FileSystemInfo info, string rootPath)
    {
        return _instance.Value.ShouldSkip(info, rootPath);
    }
}