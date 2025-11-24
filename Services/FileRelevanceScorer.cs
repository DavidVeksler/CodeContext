using System.Text.RegularExpressions;
using CodeContext.Utils;

namespace CodeContext.Services;

/// <summary>
/// Scores files based on relevance to a query or task description.
/// Uses keyword matching, path analysis, and file characteristics.
/// </summary>
public class FileRelevanceScorer
{
    private readonly string _projectPath;

    public FileRelevanceScorer(string projectPath)
    {
        _projectPath = Guard.NotNullOrEmpty(projectPath, nameof(projectPath));
    }

    /// <summary>
    /// Represents a scored file with relevance information.
    /// </summary>
    public record ScoredFile(
        string FilePath,
        string Content,
        double RelevanceScore,
        int TokenCount,
        Dictionary<string, double> ScoreBreakdown
    );

    /// <summary>
    /// Scores a file based on query/task relevance.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="content">File content.</param>
    /// <param name="query">Query or task description.</param>
    /// <returns>Scored file with relevance score between 0 and 1.</returns>
    public ScoredFile ScoreFile(string filePath, string content, string query)
    {
        var breakdown = new Dictionary<string, double>();
        var keywords = ExtractKeywords(query);

        // 1. File name relevance (30% weight)
        var nameScore = ScoreFileName(filePath, keywords);
        breakdown["fileName"] = nameScore;

        // 2. Path relevance (20% weight)
        var pathScore = ScoreFilePath(filePath, keywords);
        breakdown["filePath"] = pathScore;

        // 3. Content relevance (40% weight)
        var contentScore = ScoreContent(content, keywords);
        breakdown["content"] = contentScore;

        // 4. File importance indicators (10% weight)
        var importanceScore = ScoreImportance(filePath);
        breakdown["importance"] = importanceScore;

        // Calculate weighted total score
        var totalScore =
            (nameScore * 0.30) +
            (pathScore * 0.20) +
            (contentScore * 0.40) +
            (importanceScore * 0.10);

        var tokenCount = TokenCounter.EstimateTokensForFile(filePath, content);

        return new ScoredFile(filePath, content, totalScore, tokenCount, breakdown);
    }

    /// <summary>
    /// Extracts keywords from a query string.
    /// </summary>
    private static List<string> ExtractKeywords(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<string>();

        // Remove common words and split into keywords
        var commonWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
            "of", "with", "by", "from", "as", "is", "was", "are", "were", "be",
            "been", "being", "have", "has", "had", "do", "does", "did", "will",
            "would", "could", "should", "may", "might", "can", "this", "that",
            "these", "those", "i", "you", "we", "they", "it", "my", "your"
        };

        return Regex.Split(query.ToLowerInvariant(), @"\W+")
            .Where(w => w.Length > 2 && !commonWords.Contains(w))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Scores based on file name matching.
    /// </summary>
    private static double ScoreFileName(string filePath, List<string> keywords)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();
        if (keywords.Count == 0)
            return 0.5; // Neutral score if no keywords

        var matchCount = keywords.Count(keyword => fileName.Contains(keyword));
        return Math.Min(1.0, matchCount / (double)keywords.Count * 1.5);
    }

    /// <summary>
    /// Scores based on file path matching (directory names, etc).
    /// </summary>
    private static double ScoreFilePath(string filePath, List<string> keywords)
    {
        var pathLower = filePath.ToLowerInvariant();
        if (keywords.Count == 0)
            return 0.5;

        var matchCount = keywords.Count(keyword => pathLower.Contains(keyword));
        return Math.Min(1.0, matchCount / (double)keywords.Count);
    }

    /// <summary>
    /// Scores based on content relevance.
    /// </summary>
    private static double ScoreContent(string content, List<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(content) || keywords.Count == 0)
            return 0.3; // Low default score

        var contentLower = content.ToLowerInvariant();
        var totalMatches = keywords.Sum(keyword =>
        {
            var count = Regex.Matches(contentLower, Regex.Escape(keyword)).Count;
            return Math.Min(count, 10); // Cap at 10 matches per keyword to avoid skew
        });

        // Normalize by content length and keyword count
        var density = totalMatches / (double)(content.Length / 100 + 1);
        return Math.Min(1.0, density * keywords.Count);
    }

    /// <summary>
    /// Scores based on file importance indicators.
    /// </summary>
    private static double ScoreImportance(string filePath)
    {
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        var score = 0.5; // Base score

        // Boost for important file types
        if (fileName.Contains("readme"))
            score += 0.3;
        if (fileName.Contains("config") || fileName.Contains("settings"))
            score += 0.2;
        if (fileName.Contains("main") || fileName.Contains("program") || fileName.Contains("app"))
            score += 0.2;
        if (fileName.Contains("index") || fileName.Contains("router"))
            score += 0.15;
        if (fileName.Contains("test") || fileName.Contains("spec"))
            score += 0.1; // Tests are useful but secondary

        // Penalize very long files (might be generated/verbose)
        // This would need actual file size, using path length as proxy
        if (filePath.Length > 100)
            score -= 0.1;

        return Math.Clamp(score, 0.0, 1.0);
    }
}
