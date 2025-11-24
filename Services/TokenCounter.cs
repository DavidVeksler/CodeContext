using System.Text.RegularExpressions;

namespace CodeContext.Services;

/// <summary>
/// Service for estimating token counts in text.
/// Uses approximation: ~4 characters per token for code, ~3.5 for natural language.
/// </summary>
public static class TokenCounter
{
    private const double CharsPerTokenCode = 4.0;
    private const double CharsPerTokenText = 3.5;

    /// <summary>
    /// Estimates the number of tokens in a string (for code content).
    /// </summary>
    /// <param name="text">The text to count tokens for.</param>
    /// <returns>Estimated token count.</returns>
    public static int EstimateTokens(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        // Simple approximation: 4 chars ≈ 1 token for code
        return (int)Math.Ceiling(text.Length / CharsPerTokenCode);
    }

    /// <summary>
    /// Estimates tokens for natural language text.
    /// </summary>
    public static int EstimateTokensNaturalLanguage(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        return (int)Math.Ceiling(text.Length / CharsPerTokenText);
    }

    /// <summary>
    /// Counts tokens in a file path representation (structure view).
    /// </summary>
    public static int EstimateTokensForFilePath(string filePath)
    {
        // File paths are typically short, use exact word counting
        var parts = filePath.Split('/', '\\', '.');
        return parts.Length + 2; // Add overhead for formatting
    }

    /// <summary>
    /// Estimates total tokens for a file including its path and content.
    /// </summary>
    public static int EstimateTokensForFile(string filePath, string content)
    {
        const int separatorTokens = 10; // For "---- file.cs ----" separators

        return EstimateTokensForFilePath(filePath)
            + EstimateTokens(content)
            + separatorTokens;
    }

    /// <summary>
    /// Calculates tokens for structured context output.
    /// </summary>
    public static int EstimateTokensForStructuredOutput(
        string projectStructure,
        IEnumerable<(string path, string content)> files)
    {
        var structureTokens = EstimateTokensNaturalLanguage(projectStructure);
        var fileTokens = files.Sum(f => EstimateTokensForFile(f.path, f.content));

        const int overhead = 100; // Headers, formatting, metadata

        return structureTokens + fileTokens + overhead;
    }
}
