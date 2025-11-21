using CodeContext.Utils;

namespace CodeContext;

/// <summary>
/// Legacy compatibility wrapper for FileUtilities.
/// Use FileUtilities instead for new code.
/// </summary>
[Obsolete("Use FileUtilities in CodeContext.Utils namespace instead.")]
public static class FileUtils
{
    /// <summary>
    /// Determines if a file is binary based on its content.
    /// </summary>
    /// <param name="filePath">Path to the file to check.</param>
    /// <returns>True if the file appears to be binary; otherwise, false.</returns>
    public static bool IsBinaryFile(string filePath)
    {
        return FileUtilities.IsBinaryFile(filePath);
    }
}