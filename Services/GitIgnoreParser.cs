using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace CodeContext.Services;

/// <summary>
/// Immutable gitignore pattern matcher using functional programming principles.
/// Separates I/O operations from pure pattern matching logic.
/// </summary>
public sealed record GitIgnoreParser
{
    private readonly ImmutableArray<string> _patterns;
    private readonly Lazy<ImmutableDictionary<string, Regex>> _regexCache;

    private GitIgnoreParser(ImmutableArray<string> patterns)
    {
        _patterns = patterns;
        _regexCache = new Lazy<ImmutableDictionary<string, Regex>>(() =>
            CreateRegexCache(patterns));
    }

    /// <summary>
    /// Creates an empty GitIgnoreParser with no patterns.
    /// </summary>
    public static GitIgnoreParser Empty { get; } = new GitIgnoreParser(ImmutableArray<string>.Empty);

    /// <summary>
    /// Creates a GitIgnoreParser from a collection of patterns (pure function).
    /// </summary>
    /// <param name="patterns">The gitignore patterns to use.</param>
    /// <returns>A new immutable GitIgnoreParser instance.</returns>
    public static GitIgnoreParser FromPatterns(IEnumerable<string> patterns)
    {
        var validPatterns = patterns
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
            .ToImmutableArray();

        return validPatterns.IsEmpty ? Empty : new GitIgnoreParser(validPatterns);
    }

    /// <summary>
    /// Reads gitignore patterns from a file (I/O operation).
    /// Returns Empty parser if file doesn't exist or can't be read.
    /// </summary>
    /// <param name="gitIgnorePath">Path to the .gitignore file.</param>
    /// <returns>A new GitIgnoreParser with patterns from the file.</returns>
    public static GitIgnoreParser FromFile(string gitIgnorePath)
    {
        if (!File.Exists(gitIgnorePath))
        {
            return Empty;
        }

        try
        {
            var lines = File.ReadAllLines(gitIgnorePath);
            return FromPatterns(lines);
        }
        catch
        {
            return Empty;
        }
    }

    /// <summary>
    /// Checks if a relative path matches any gitignore patterns (pure function).
    /// </summary>
    /// <param name="relativePath">The relative path to check.</param>
    /// <returns>True if the path should be ignored; otherwise, false.</returns>
    public bool IsIgnored(string relativePath) =>
        _patterns.Any(pattern => IsMatch(relativePath, pattern));

    /// <summary>
    /// Checks if there are any loaded patterns.
    /// </summary>
    public bool HasPatterns => !_patterns.IsEmpty;

    /// <summary>
    /// Gets the number of patterns.
    /// </summary>
    public int PatternCount => _patterns.Length;

    private bool IsMatch(string path, string pattern)
    {
        var cache = _regexCache.Value;
        if (!cache.TryGetValue(pattern, out var regex))
        {
            // This shouldn't happen as cache is pre-computed, but handle defensively
            var regexPattern = ConvertGitIgnorePatternToRegex(pattern);
            regex = new Regex($"^{regexPattern}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        return regex.IsMatch(path);
    }

    private static ImmutableDictionary<string, Regex> CreateRegexCache(ImmutableArray<string> patterns) =>
        patterns.ToImmutableDictionary(
            pattern => pattern,
            pattern => new Regex(
                $"^{ConvertGitIgnorePatternToRegex(pattern)}$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled));

    private static string ConvertGitIgnorePatternToRegex(string pattern) =>
        pattern
            .Replace(".", "\\.")
            .Replace("*", ".*")
            .Replace("?", ".");
}
