namespace CodeContext.Utils;

/// <summary>
/// Provides guard clauses for parameter validation to reduce boilerplate.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Ensures that a reference is not null.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if not null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static T NotNull<T>(T? value, string paramName) where T : class
    {
        if (value == null)
        {
            throw new ArgumentNullException(paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures that a string is not null or empty.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if not null or empty.</returns>
    /// <exception cref="ArgumentException">Thrown when value is null or empty.</exception>
    public static string NotNullOrEmpty(string? value, string paramName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures that a directory exists.
    /// </summary>
    /// <param name="path">The directory path to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The path if the directory exists.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory doesn't exist.</exception>
    public static string DirectoryExists(string path, string paramName)
    {
        NotNullOrEmpty(path, paramName);

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }
        return path;
    }

    /// <summary>
    /// Ensures that a file exists.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The path if the file exists.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist.</exception>
    public static string FileExists(string path, string paramName)
    {
        NotNullOrEmpty(path, paramName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }
        return path;
    }
}
