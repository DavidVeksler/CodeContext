namespace CodeContext.Services;

/// <summary>
/// Functional statistics calculator with separated I/O and formatting logic.
/// </summary>
public class StatsCalculator
{
    /// <summary>
    /// Calculates and formats statistics about the processing operation.
    /// Separates I/O (file counting) from pure calculations.
    /// </summary>
    /// <param name="projectPath">The directory that was processed.</param>
    /// <param name="content">The generated content.</param>
    /// <param name="elapsed">Time elapsed during processing.</param>
    /// <returns>Formatted statistics string.</returns>
    public string Calculate(string projectPath, string content, TimeSpan elapsed)
    {
        var stats = GatherStats(projectPath, content, elapsed);
        return FormatStats(stats);
    }

    /// <summary>
    /// I/O operation: gathers statistics from file system and content.
    /// Returns null on error for functional error handling.
    /// </summary>
    private static ProjectStats? GatherStats(string projectPath, string content, TimeSpan elapsed)
    {
        try
        {
            var fileCount = CountFiles(projectPath);
            var lineCount = CountLines(content);

            return new ProjectStats(fileCount, lineCount, elapsed, content.Length);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// I/O operation: counts all files in directory tree.
    /// </summary>
    private static int CountFiles(string directoryPath) =>
        Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).Length;

    /// <summary>
    /// Pure function: counts newline characters in content.
    /// </summary>
    private static int CountLines(string content) =>
        content.Count(c => c == '\n');

    /// <summary>
    /// Pure function: formats statistics into display string.
    /// </summary>
    private static string FormatStats(ProjectStats? stats) =>
        stats switch
        {
            null => "\n📊 Stats: Unable to calculate statistics",
            var s => $"""

            📊 Stats:
            📁 Files processed: {s.FileCount}
            📝 Total lines: {s.LineCount}
            ⏱️ Time taken: {s.Elapsed.TotalSeconds:F2}s
            💾 Output size: {s.ContentLength} characters
            """
        };

    /// <summary>
    /// Immutable record holding project statistics.
    /// </summary>
    private sealed record ProjectStats(
        int FileCount,
        int LineCount,
        TimeSpan Elapsed,
        int ContentLength);
}
