namespace CodeContext.Interfaces;

/// <summary>
/// Provides functionality to determine if files or directories should be skipped during processing.
/// </summary>
public interface IFileChecker
{
    /// <summary>
    /// Determines if a file or directory should be skipped during processing.
    /// </summary>
    /// <param name="info">The file or directory information.</param>
    /// <param name="rootPath">The root path of the project being scanned.</param>
    /// <returns>True if the file/directory should be skipped; otherwise, false.</returns>
    bool ShouldSkip(FileSystemInfo info, string rootPath);
}
