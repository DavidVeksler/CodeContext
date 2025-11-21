using System.Text;
using CodeContext.Interfaces;

namespace CodeContext.Services;

/// <summary>
/// Service for scanning and analyzing project directories.
/// </summary>
public class ProjectScanner
{
    private readonly IFileChecker _fileChecker;
    private readonly IConsoleWriter _console;
    private string? _gitRepoRoot;

    /// <summary>
    /// Initializes a new instance of the ProjectScanner class.
    /// </summary>
    /// <param name="fileChecker">The file checker to use for filtering.</param>
    /// <param name="console">The console writer for output.</param>
    public ProjectScanner(IFileChecker fileChecker, IConsoleWriter console)
    {
        _fileChecker = fileChecker ?? throw new ArgumentNullException(nameof(fileChecker));
        _console = console ?? throw new ArgumentNullException(nameof(console));
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
    /// <param name="path">The directory path to scan.</param>
    /// <param name="indent">Current indentation level (used for recursion).</param>
    /// <returns>A string representation of the directory structure.</returns>
    public string GetProjectStructure(string path, int indent = 0)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        _gitRepoRoot ??= FindGitRepoRoot(path);

        if (indent == 0)
        {
            _console.WriteLine("📁 Analyzing directory structure...");
        }

        var rootPath = _gitRepoRoot ?? path;
        var entries = Directory.EnumerateFileSystemEntries(path)
            .OrderBy(e => e)
            .Where(e => !_fileChecker.ShouldSkip(new FileInfo(e), rootPath))
            .ToList();

        var sb = new StringBuilder();

        for (int i = 0; i < entries.Count; i++)
        {
            WriteProgress(i + 1, entries.Count);
            var entry = entries[i];

            if (Directory.Exists(entry))
            {
                var dir = new DirectoryInfo(entry);
                sb.AppendLine($"{new string(' ', indent * 2)}[{dir.Name}/]");
                sb.Append(GetProjectStructure(entry, indent + 1));
            }
            else
            {
                var file = new FileInfo(entry);
                sb.AppendLine($"{new string(' ', indent * 2)}[{file.Extension}] {file.Name}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Retrieves the contents of all non-filtered files in the directory tree.
    /// </summary>
    /// <param name="path">The directory path to scan.</param>
    /// <returns>A string containing all file contents with separators.</returns>
    public string GetFileContents(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        _gitRepoRoot ??= FindGitRepoRoot(path);
        _console.WriteLine("\n📄 Processing files...");

        var rootPath = _gitRepoRoot ?? path;
        var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Where(f => !_fileChecker.ShouldSkip(new FileInfo(f), rootPath))
            .ToList();

        var results = new List<string>();
        for (int i = 0; i < files.Count; i++)
        {
            WriteProgress(i + 1, files.Count);
            var file = files[i];

            try
            {
                var content = File.ReadAllText(file);
                results.Add($"{file}\n{new string('-', 100)}\n{content}");
            }
            catch (Exception ex)
            {
                _console.WriteLine($"\n⚠️ Warning: Could not read file {file}: {ex.Message}");
            }
        }

        return string.Join("\n\n", results);
    }

    /// <summary>
    /// Gets the root path of the git repository containing the specified path.
    /// </summary>
    public string? GitRepoRoot => _gitRepoRoot;

    private string? FindGitRepoRoot(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return null;
        }

        var currentPath = path;
        while (!string.IsNullOrEmpty(currentPath))
        {
            if (Directory.Exists(Path.Combine(currentPath, ".git")))
            {
                return currentPath;
            }
            currentPath = Path.GetDirectoryName(currentPath);
        }

        return null;
    }

    private void WriteProgress(int current, int total)
    {
        var percent = (int)((current / (double)total) * 100);
        _console.Write($"\r⏳ Progress: {percent}% ({current}/{total})");
    }
}
