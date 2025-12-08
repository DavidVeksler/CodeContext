using System.ComponentModel;
using System.Text;
using CodeContext.Configuration;
using CodeContext.Interfaces;
using CodeContext.Services;
using CodeContext.Utils;
using ModelContextProtocol.Server;

namespace CodeContext.Mcp;

/// <summary>
/// MCP server tools for CodeContext functionality.
/// Provides intelligent code context generation with token budget optimization.
/// </summary>
[McpServerToolType]
public class CodeContextTools
{
    private readonly IConsoleWriter _console;

    public CodeContextTools(IConsoleWriter console)
    {
        _console = console;
    }

    /// <summary>
    /// Gets optimized code context for a specific task within a token budget.
    /// </summary>
    [McpServerTool]
    [Description("Get optimized code context for a task. Intelligently selects most relevant files within token budget.")]
    public string GetCodeContext(
        [Description("Path to the project directory to analyze")] string projectPath,
        [Description("Description of the task (e.g., 'fix authentication bug', 'add payment feature')")] string taskDescription,
        [Description("Maximum number of tokens to use (default: 50000)")] int tokenBudget = 50000,
        [Description("Include project structure in output (default: true)")] bool includeStructure = true,
        [Description("Selection strategy: GreedyByScore, ValueOptimized, or Balanced (default: ValueOptimized)")]
        string strategy = "ValueOptimized")
    {
        try
        {
            // Validate inputs
            Guard.DirectoryExists(projectPath, nameof(projectPath));

            // Initialize services
            var filterConfig = new FilterConfiguration();
            var gitIgnoreParser = GitHelper.FindRepositoryRoot(projectPath) switch
            {
                null => GitIgnoreParser.Empty,
                var gitRoot => GitIgnoreParser.FromFile(Path.Combine(gitRoot, ".gitignore"))
            };

            var fileChecker = new FileFilterService(filterConfig, gitIgnoreParser);
            var scanner = new ProjectScanner(fileChecker, _console);
            var scorer = new FileRelevanceScorer(projectPath);
            var optimizer = new TokenBudgetOptimizer();

            // Parse strategy
            var strategyEnum = Enum.TryParse<TokenBudgetOptimizer.SelectionStrategy>(strategy, true, out var s)
                ? s
                : TokenBudgetOptimizer.SelectionStrategy.ValueOptimized;

            // Scan and score files (synchronous I/O, no Task.Run needed)
            var files = GetAllProjectFiles(scanner, projectPath);
            var scoredFiles = files
                .Select(f => scorer.ScoreFile(f.path, f.content, taskDescription))
                .ToList();

            // Optimize selection
            var result = optimizer.OptimizeSelection(
                scoredFiles,
                tokenBudget,
                strategyEnum,
                includeStructure);

            // Build output
            var output = new StringBuilder();

            output.AppendLine("# Code Context");
            output.AppendLine($"Project: {Path.GetFileName(projectPath)}");
            output.AppendLine($"Task: {taskDescription}");
            output.AppendLine();

            output.AppendLine(TokenBudgetOptimizer.GenerateSummary(result));
            output.AppendLine();
            output.AppendLine(new string('=', 80));
            output.AppendLine();

            // Include structure if requested
            if (includeStructure)
            {
                output.AppendLine("## Project Structure");
                output.AppendLine();
                var structure = scanner.GetProjectStructure(projectPath);
                output.AppendLine(structure);
                output.AppendLine();
                output.AppendLine(new string('=', 80));
                output.AppendLine();
            }

            // Include selected files
            output.AppendLine("## Selected Files");
            output.AppendLine();

            foreach (var file in result.SelectedFiles.OrderByDescending(f => f.RelevanceScore))
            {
                output.AppendLine($"### {file.FilePath}");
                output.AppendLine($"Relevance: {file.RelevanceScore:F3} | Tokens: {file.TokenCount:N0}");
                output.AppendLine(new string('-', 80));
                output.AppendLine(file.Content);
                output.AppendLine();
            }

            return output.ToString();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets the project structure (directory tree).
    /// </summary>
    [McpServerTool]
    [Description("Get the hierarchical directory structure of a project")]
    public string GetProjectStructure(
        [Description("Path to the project directory")] string projectPath)
    {
        try
        {
            Guard.DirectoryExists(projectPath, nameof(projectPath));

            var filterConfig = new FilterConfiguration();
            var gitIgnoreParser = GitHelper.FindRepositoryRoot(projectPath) switch
            {
                null => GitIgnoreParser.Empty,
                var gitRoot => GitIgnoreParser.FromFile(Path.Combine(gitRoot, ".gitignore"))
            };

            var fileChecker = new FileFilterService(filterConfig, gitIgnoreParser);
            var scanner = new ProjectScanner(fileChecker, _console);

            return scanner.GetProjectStructure(projectPath);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Lists all files in a project with metadata.
    /// </summary>
    [McpServerTool]
    [Description("List all files in a project with token counts and basic metadata")]
    public string ListProjectFiles(
        [Description("Path to the project directory")] string projectPath,
        [Description("Optional query to filter/rank files")] string? query = null)
    {
        try
        {
            Guard.DirectoryExists(projectPath, nameof(projectPath));

            var filterConfig = new FilterConfiguration();
            var gitIgnoreParser = GitHelper.FindRepositoryRoot(projectPath) switch
            {
                null => GitIgnoreParser.Empty,
                var gitRoot => GitIgnoreParser.FromFile(Path.Combine(gitRoot, ".gitignore"))
            };

            var fileChecker = new FileFilterService(filterConfig, gitIgnoreParser);
            var scanner = new ProjectScanner(fileChecker, _console);

            // Synchronous I/O, no Task.Run needed
            var files = GetAllProjectFiles(scanner, projectPath);

            var output = new StringBuilder();
            output.AppendLine($"# Project Files: {Path.GetFileName(projectPath)}");
            output.AppendLine();

            if (!string.IsNullOrWhiteSpace(query))
            {
                // Score and sort by relevance
                var scorer = new FileRelevanceScorer(projectPath);
                var scored = files
                    .Select(f => scorer.ScoreFile(f.path, f.content, query))
                    .OrderByDescending(f => f.RelevanceScore)
                    .ToList();

                output.AppendLine($"Filtered by: {query}");
                output.AppendLine($"Total files: {scored.Count}");
                output.AppendLine();
                output.AppendLine("Path | Relevance | Tokens");
                output.AppendLine(new string('-', 80));

                foreach (var file in scored)
                {
                    output.AppendLine($"{file.FilePath} | {file.RelevanceScore:F3} | {file.TokenCount:N0}");
                }
            }
            else
            {
                // Just list all files
                output.AppendLine($"Total files: {files.Count}");
                output.AppendLine();
                output.AppendLine("Path | Tokens");
                output.AppendLine(new string('-', 80));

                foreach (var (path, content) in files)
                {
                    var tokens = TokenCounter.EstimateTokensForFile(path, content);
                    output.AppendLine($"{path} | {tokens:N0}");
                }
            }

            return output.ToString();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets the content of specific files.
    /// </summary>
    [McpServerTool]
    [Description("Get the content of specific files by path")]
    public string GetFileContent(
        [Description("Path to the project directory")] string projectPath,
        [Description("Comma-separated list of file paths relative to project root")] string filePaths)
    {
        try
        {
            Guard.DirectoryExists(projectPath, nameof(projectPath));

            var paths = filePaths.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var output = new StringBuilder();

            output.AppendLine("# File Contents");
            output.AppendLine();

            foreach (var relativePath in paths)
            {
                // Validate path to prevent path traversal attacks
                if (!PathSecurity.TryValidatePathWithinRoot(projectPath, relativePath, out var fullPath))
                {
                    output.AppendLine($"## {relativePath}");
                    output.AppendLine("❌ Security error: Path traversal detected");
                    output.AppendLine();
                    continue;
                }

                if (!File.Exists(fullPath))
                {
                    output.AppendLine($"## {relativePath}");
                    output.AppendLine("❌ File not found");
                    output.AppendLine();
                    continue;
                }

                var content = File.ReadAllText(fullPath);
                var tokens = TokenCounter.EstimateTokensForFile(relativePath, content);

                output.AppendLine($"## {relativePath}");
                output.AppendLine($"Tokens: {tokens:N0}");
                output.AppendLine(new string('-', 80));
                output.AppendLine(content);
                output.AppendLine();
            }

            return output.ToString();
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Helper method to get all project files with content.
    /// </summary>
    private static List<(string path, string content)> GetAllProjectFiles(
        ProjectScanner scanner,
        string projectPath)
    {
        var files = new List<(string path, string content)>();
        var context = GitHelper.FindRepositoryRoot(projectPath) ?? projectPath;

        CollectFiles(scanner, projectPath, context, files);

        return files;
    }

    private static void CollectFiles(
        ProjectScanner scanner,
        string currentPath,
        string rootPath,
        List<(string path, string content)> files)
    {
        try
        {
            var fileCheckerField = scanner.GetType()
                .GetField("_fileChecker", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fileChecker = fileCheckerField?.GetValue(scanner) as IFileChecker;

            var entries = Directory.EnumerateFileSystemEntries(currentPath)
                .Where(e => fileChecker == null || !fileChecker.ShouldSkip(new FileInfo(e), rootPath))
                .ToList();

            foreach (var entry in entries)
            {
                if (Directory.Exists(entry))
                {
                    CollectFiles(scanner, entry, rootPath, files);
                }
                else if (File.Exists(entry))
                {
                    try
                    {
                        var content = File.ReadAllText(entry);
                        var relativePath = Path.GetRelativePath(rootPath, entry);
                        files.Add((relativePath, content));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files with permission issues
                    }
                    catch (IOException)
                    {
                        // Skip files that are locked or in use
                    }
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories with permission issues
        }
        catch (DirectoryNotFoundException)
        {
            // Skip if directory was deleted during scan
        }
    }
}
