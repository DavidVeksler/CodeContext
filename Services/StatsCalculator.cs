namespace CodeContext.Services;

/// <summary>
/// Calculates statistics about processed projects.
/// </summary>
public class StatsCalculator
{
    /// <summary>
    /// Calculates and formats statistics about the processing operation.
    /// </summary>
    /// <param name="projectPath">The directory that was processed.</param>
    /// <param name="content">The generated content.</param>
    /// <param name="elapsed">Time elapsed during processing.</param>
    /// <returns>Formatted statistics string.</returns>
    public string Calculate(string projectPath, string content, TimeSpan elapsed)
    {
        try
        {
            var fileCount = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories).Length;
            var lineCount = content.Count(c => c == '\n');

            return $"""

            📊 Stats:
            📁 Files processed: {fileCount}
            📝 Total lines: {lineCount}
            ⏱️ Time taken: {elapsed.TotalSeconds:F2}s
            💾 Output size: {content.Length} characters
            """;
        }
        catch
        {
            return "\n📊 Stats: Unable to calculate statistics";
        }
    }
}
