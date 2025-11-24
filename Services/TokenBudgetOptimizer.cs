using System.Collections.Immutable;
using CodeContext.Utils;
using static CodeContext.Services.FileRelevanceScorer;

namespace CodeContext.Services;

/// <summary>
/// Optimizes file selection based on token budget constraints.
/// Implements multiple strategies to maximize relevance within token limits.
/// </summary>
public class TokenBudgetOptimizer
{
    public enum SelectionStrategy
    {
        /// <summary>
        /// Greedy selection: pick highest-scoring files until budget exhausted.
        /// </summary>
        GreedyByScore,

        /// <summary>
        /// Value-optimized: maximize relevance/token ratio (bang for buck).
        /// </summary>
        ValueOptimized,

        /// <summary>
        /// Balanced: mix of high-value and comprehensive coverage.
        /// </summary>
        Balanced
    }

    /// <summary>
    /// Result of budget optimization.
    /// </summary>
    public record OptimizationResult(
        ImmutableArray<ScoredFile> SelectedFiles,
        ImmutableArray<ScoredFile> ExcludedFiles,
        int TotalTokens,
        int TokenBudget,
        double AverageRelevanceScore,
        SelectionStrategy Strategy
    );

    /// <summary>
    /// Optimizes file selection within a token budget.
    /// </summary>
    /// <param name="scoredFiles">Files with relevance scores.</param>
    /// <param name="tokenBudget">Maximum tokens allowed.</param>
    /// <param name="strategy">Selection strategy to use.</param>
    /// <param name="includeStructure">Whether to reserve tokens for project structure.</param>
    /// <returns>Optimized file selection.</returns>
    public OptimizationResult OptimizeSelection(
        IEnumerable<ScoredFile> scoredFiles,
        int tokenBudget,
        SelectionStrategy strategy = SelectionStrategy.ValueOptimized,
        bool includeStructure = true)
    {
        Guard.NotNull(scoredFiles, nameof(scoredFiles));

        if (tokenBudget <= 0)
        {
            return new OptimizationResult(
                ImmutableArray<ScoredFile>.Empty,
                scoredFiles.ToImmutableArray(),
                0,
                tokenBudget,
                0.0,
                strategy
            );
        }

        // Reserve tokens for project structure if requested
        var reservedTokens = includeStructure ? 2000 : 100; // Structure + overhead
        var availableBudget = Math.Max(0, tokenBudget - reservedTokens);

        var selected = strategy switch
        {
            SelectionStrategy.GreedyByScore => SelectGreedyByScore(scoredFiles, availableBudget),
            SelectionStrategy.ValueOptimized => SelectValueOptimized(scoredFiles, availableBudget),
            SelectionStrategy.Balanced => SelectBalanced(scoredFiles, availableBudget),
            _ => throw new ArgumentException($"Unknown strategy: {strategy}")
        };

        var selectedArray = selected.ToImmutableArray();
        var excludedArray = scoredFiles.Except(selected).ToImmutableArray();
        var totalTokens = selectedArray.Sum(f => f.TokenCount) + reservedTokens;
        var avgScore = selectedArray.Any()
            ? selectedArray.Average(f => f.RelevanceScore)
            : 0.0;

        return new OptimizationResult(
            selectedArray,
            excludedArray,
            totalTokens,
            tokenBudget,
            avgScore,
            strategy
        );
    }

    /// <summary>
    /// Greedy selection: pick highest-scoring files first.
    /// </summary>
    private static IEnumerable<ScoredFile> SelectGreedyByScore(
        IEnumerable<ScoredFile> files,
        int budget)
    {
        var selected = new List<ScoredFile>();
        var remainingBudget = budget;

        foreach (var file in files.OrderByDescending(f => f.RelevanceScore))
        {
            if (file.TokenCount <= remainingBudget)
            {
                selected.Add(file);
                remainingBudget -= file.TokenCount;
            }

            if (remainingBudget <= 0)
                break;
        }

        return selected;
    }

    /// <summary>
    /// Value-optimized selection: maximize relevance per token.
    /// </summary>
    private static IEnumerable<ScoredFile> SelectValueOptimized(
        IEnumerable<ScoredFile> files,
        int budget)
    {
        var selected = new List<ScoredFile>();
        var remainingBudget = budget;

        // Calculate value ratio: relevance / tokens
        var valueRanked = files
            .Select(f => new
            {
                File = f,
                ValueRatio = f.TokenCount > 0 ? f.RelevanceScore / f.TokenCount : 0
            })
            .OrderByDescending(x => x.ValueRatio)
            .ToList();

        foreach (var item in valueRanked)
        {
            if (item.File.TokenCount <= remainingBudget)
            {
                selected.Add(item.File);
                remainingBudget -= item.File.TokenCount;
            }

            if (remainingBudget <= 0)
                break;
        }

        return selected;
    }

    /// <summary>
    /// Balanced selection: prioritize high-value files, then fill with high-score files.
    /// </summary>
    private static IEnumerable<ScoredFile> SelectBalanced(
        IEnumerable<ScoredFile> files,
        int budget)
    {
        var selected = new List<ScoredFile>();
        var filesList = files.ToList();
        var remainingBudget = budget;

        // Phase 1: Select top 50% budget with highest value ratio
        var phase1Budget = budget / 2;
        var valueOptimized = SelectValueOptimized(filesList, phase1Budget).ToList();
        selected.AddRange(valueOptimized);
        remainingBudget -= valueOptimized.Sum(f => f.TokenCount);

        // Phase 2: Fill remaining with highest-scoring files not yet selected
        var remaining = filesList.Except(selected);
        var greedy = SelectGreedyByScore(remaining, remainingBudget);
        selected.AddRange(greedy);

        return selected;
    }

    /// <summary>
    /// Generates a summary of the optimization result.
    /// </summary>
    public static string GenerateSummary(OptimizationResult result)
    {
        var utilizationPercent = result.TokenBudget > 0
            ? (result.TotalTokens / (double)result.TokenBudget * 100)
            : 0;

        var summary = $@"Token Budget Optimization Summary
Strategy: {result.Strategy}
Token Budget: {result.TokenBudget:N0}
Tokens Used: {result.TotalTokens:N0} ({utilizationPercent:F1}%)
Files Selected: {result.SelectedFiles.Length}
Files Excluded: {result.ExcludedFiles.Length}
Average Relevance Score: {result.AverageRelevanceScore:F3}

Top Selected Files:";

        var topFiles = result.SelectedFiles
            .OrderByDescending(f => f.RelevanceScore)
            .Take(10)
            .Select(f => $"  • {Path.GetFileName(f.FilePath)} (score: {f.RelevanceScore:F3}, tokens: {f.TokenCount:N0})")
            .ToList();

        if (topFiles.Any())
        {
            summary += "\n" + string.Join("\n", topFiles);
        }

        if (result.ExcludedFiles.Length > 0)
        {
            summary += $"\n\nExcluded {result.ExcludedFiles.Length} files due to token budget constraints.";
        }

        return summary;
    }
}
