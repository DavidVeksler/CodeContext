using System.Text;
using CodeContext.Interfaces;
using CodeContext.Utils;

namespace CodeContext.Services;

/// <summary>
/// Service for scanning and analyzing project directories.
/// </summary>
public class ProjectScanner
{
    private readonly IFileChecker _fileChecker;
    private readonly IConsoleWriter _console;
    private string? _gitRepoRoot;

    public ProjectScanner(IFileChecker fileChecker, IConsoleWriter console)
    {
        _fileChecker = Guard.NotNull(fileChecker, nameof(fileChecker));
        _console = Guard.NotNull(console, nameof(console));
    }

    /// <summary>
    /// Gets user input with a prompt.
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <returns>The user's input.</returns>
    public string GetUserInput(string prompt)
    {
        _console.Write(prompt);
        return _console.ReadLine() ?? string.Empty;
    }

    /// <summary>
    /// Generates a hierarchical structure representation of the project directory.
    /// </summary>
    /// <param name="projectPath">The directory path to scan.</param>
    /// <param name="indent">Current indentation level (used for recursion).</param>
    /// <returns>A string representation of the directory structure.</returns>
    public string GetProjectStructure(string projectPath, int indent = 0)
    {
        Guard.DirectoryExists(projectPath, nameof(projectPath));
        _gitRepoRoot ??= GitHelper.FindRepositoryRoot(projectPath);

        if (indent == 0)
        {
            _console.WriteLine("📁 Analyzing directory structure...");
        }

        var rootPath = _gitRepoRoot ?? projectPath;
        var entries = Directory.EnumerateFileSystemEntries(projectPath)
            .OrderBy(e => e)
            .Where(e => !_fileChecker.ShouldSkip(new FileInfo(e), rootPath))
            .ToList();

        var structure = new StringBuilder();

        for (int i = 0; i < entries.Count; i++)
        {
            WriteProgress(i + 1, entries.Count);
            var entry = entries[i];

            if (Directory.Exists(entry))
            {
                var dir = new DirectoryInfo(entry);
                structure.AppendLine($"{new string(' ', indent * 2)}[{dir.Name}/]");
                structure.Append(GetProjectStructure(entry, indent + 1));
            }
            else
            {
                var file = new FileInfo(entry);
                structure.AppendLine($"{new string(' ', indent * 2)}[{file.Extension}] {file.Name}");
            }
        }

        return structure.ToString();
    }

    /// <summary>
    /// Retrieves the contents of all non-filtered files in the directory tree.
    /// </summary>
    /// <param name="projectPath">The directory path to scan.</param>
    /// <returns>A string containing all file contents with separators.</returns>
    public string GetFileContents(string projectPath)
    {
        Guard.DirectoryExists(projectPath, nameof(projectPath));
        _gitRepoRoot ??= GitHelper.FindRepositoryRoot(projectPath);
        _console.WriteLine("\n📄 Processing files...");

        var rootPath = _gitRepoRoot ?? projectPath;
        var files = Directory.EnumerateFiles(projectPath, "*", SearchOption.AllDirectories)
            .Where(f => !_fileChecker.ShouldSkip(new FileInfo(f), rootPath))
            .ToList();

        var fileContents = new List<string>();
        for (int i = 0; i < files.Count; i++)
        {
            WriteProgress(i + 1, files.Count);
            var file = files[i];

            try
            {
                var content = File.ReadAllText(file);
                fileContents.Add($"{file}\n{new string('-', 100)}\n{content}");
            }
            catch (Exception ex)
            {
                _console.WriteLine($"\n⚠️ Warning: Could not read file {file}: {ex.Message}");
            }
        }

        return string.Join("\n\n", fileContents);
    }

    /// <summary>
    /// Gets the root path of the git repository containing the scanned path.
    /// </summary>
    public string? GitRepoRoot => _gitRepoRoot;

    private void WriteProgress(int current, int total)
    {
        var percent = (int)((current / (double)total) * 100);
        _console.Write($"\r⏳ Progress: {percent}% ({current}/{total})");
    }
}
