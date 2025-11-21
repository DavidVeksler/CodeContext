using System.Text.RegularExpressions;

namespace CodeContext.Services;

/// <summary>
/// Handles parsing and matching of .gitignore patterns.
/// </summary>
public class GitIgnoreParser
{
    private readonly List<string> _patterns = new();
    private readonly Dictionary<string, Regex> _regexCache = new();

    /// <summary>
    /// Loads .gitignore patterns from a file.
    /// </summary>
    /// <param name="gitIgnorePath">Path to the .gitignore file.</param>
    public void LoadFromFile(string gitIgnorePath)
    {
        if (!File.Exists(gitIgnorePath))
        {
            return;
        }

        _patterns.Clear();
        _regexCache.Clear();

        var lines = File.ReadAllLines(gitIgnorePath)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'));

        _patterns.AddRange(lines);
    }

    /// <summary>
    /// Checks if a relative path matches any loaded gitignore patterns.
    /// </summary>
    /// <param name="relativePath">The relative path to check.</param>
    /// <returns>True if the path should be ignored; otherwise, false.</returns>
    public bool IsIgnored(string relativePath)
    {
        return _patterns.Any(pattern => IsMatch(relativePath, pattern));
    }

    /// <summary>
    /// Checks if there are any loaded patterns.
    /// </summary>
    public bool HasPatterns => _patterns.Count > 0;

    private bool IsMatch(string path, string pattern)
    {
        if (!_regexCache.TryGetValue(pattern, out var regex))
        {
            var regexPattern = ConvertGitIgnorePatternToRegex(pattern);
            regex = new Regex($"^{regexPattern}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _regexCache[pattern] = regex;
        }

        return regex.IsMatch(path);
    }

    private static string ConvertGitIgnorePatternToRegex(string pattern)
    {
        // Simple conversion - could be enhanced for full gitignore spec
        return pattern
            .Replace(".", "\\.")
            .Replace("*", ".*")
            .Replace("?", ".");
    }
}
