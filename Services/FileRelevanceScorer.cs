using System.Text.RegularExpressions;
using CodeContext.Utils;

namespace CodeContext.Services;

/// <summary>
/// Scores files based on relevance to a query or task description.
/// Uses keyword matching, path analysis, and file characteristics.
/// </summary>
public class FileRelevanceScorer(string projectPath)
{
    // Scoring weights for different factors
    private const double FileNameWeight = 0.30;
    private const double FilePathWeight = 0.20;
    private const double ContentWeight = 0.40;
    private const double ImportanceWeight = 0.10;

    // Scoring parameters
    private const double NeutralScore = 0.5;
    private const double LowDefaultScore = 0.3;
    private const int MaxMatchesPerKeyword = 10;
    private const int ContentLengthNormalizer = 100;

    // File importance boost values
    private const double ReadmeBoost = 0.3;
    private const double ConfigBoost = 0.2;
    private const double MainFileBoost = 0.2;
    private const double IndexFileBoost = 0.15;
    private const double TestFileBoost = 0.1;

    private readonly string _projectPath = Guard.NotNullOrEmpty(projectPath, nameof(projectPath));

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
        Dictionary<string, double> breakdown = [];
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
        var importanceScore = ScoreImportance(filePath, content.Length);
        breakdown["importance"] = importanceScore;

        // Calculate weighted total score
        var totalScore =
            (nameScore * FileNameWeight) +
            (pathScore * FilePathWeight) +
            (contentScore * ContentWeight) +
            (importanceScore * ImportanceWeight);

        var tokenCount = TokenCounter.EstimateTokensForFile(filePath, content);

        return new ScoredFile(filePath, content, totalScore, tokenCount, breakdown);
    }

    /// <summary>
    /// Extracts keywords from a query string.
    /// </summary>
    private static List<string> ExtractKeywords(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        // Remove common words and split into keywords
        HashSet<string> commonWords = new(StringComparer.OrdinalIgnoreCase)
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
        if (keywords.Count == 0)
            return NeutralScore;

        var fileName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();
        var matchCount = keywords.Count(keyword => fileName.Contains(keyword));
        return Math.Min(1.0, matchCount / (double)keywords.Count * 1.5);
    }

    /// <summary>
    /// Scores based on file path matching (directory names, etc).
    /// </summary>
    private static double ScoreFilePath(string filePath, List<string> keywords)
    {
        if (keywords.Count == 0)
            return NeutralScore;

        var pathLower = filePath.ToLowerInvariant();
        var matchCount = keywords.Count(keyword => pathLower.Contains(keyword));
        return Math.Min(1.0, matchCount / (double)keywords.Count);
    }

    /// <summary>
    /// Scores based on content relevance.
    /// Optimized to avoid repeated ToLowerInvariant() calls on large content.
    /// </summary>
    private static double ScoreContent(string content, List<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(content) || keywords.Count == 0)
            return LowDefaultScore;

        // Cache lowercase conversion once to avoid repeated allocations
        var contentLower = content.ToLowerInvariant();

        var totalMatches = keywords.Sum(keyword =>
        {
            var count = Regex.Matches(contentLower, Regex.Escape(keyword)).Count;
            return Math.Min(count, MaxMatchesPerKeyword);
        });

        // Normalize by content length and keyword count
        var density = totalMatches / (double)(content.Length / ContentLengthNormalizer + 1);
        return Math.Min(1.0, density * keywords.Count);
    }

    /// <summary>
    /// Scores based on file importance indicators.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="fileSize">Size of the file content in bytes.</param>
    private static double ScoreImportance(string filePath, int fileSize)
    {
        const int VeryLargeFileThreshold = 50000; // 50KB
        const double LargeFilePenalty = 0.1;

        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        var score = NeutralScore;

        // Boost for important file types
        if (fileName.Contains("readme"))
            score += ReadmeBoost;
        if (fileName.Contains("config") || fileName.Contains("settings"))
            score += ConfigBoost;
        if (fileName.Contains("main") || fileName.Contains("program") || fileName.Contains("app"))
            score += MainFileBoost;
        if (fileName.Contains("index") || fileName.Contains("router"))
            score += IndexFileBoost;
        if (fileName.Contains("test") || fileName.Contains("spec"))
            score += TestFileBoost;

        // Penalize very large files (might be generated/verbose)
        if (fileSize > VeryLargeFileThreshold)
            score -= LargeFilePenalty;

        return Math.Clamp(score, 0.0, 1.0);
    }
}
