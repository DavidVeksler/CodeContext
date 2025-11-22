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

        List<string> entries;
        try
        {
            var enumerationOptions = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = false
            };

            entries = Directory.EnumerateFileSystemEntries(projectPath, "*", enumerationOptions)
                .OrderBy(e => e)
                .Where(e =>
                {
                    try
                    {
                        return !_fileChecker.ShouldSkip(new FileInfo(e), rootPath);
                    }
                    catch
                    {
                        // Skip entries that can't be accessed
                        return false;
                    }
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _console.WriteLine($"\n⚠️ Warning: Could not enumerate directory {projectPath}: {ex.Message}");
            return string.Empty;
        }

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
        var files = new List<string>();

        // Manually enumerate files recursively to respect filters
        EnumerateFilesRecursively(projectPath, rootPath, files);

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
    /// Recursively enumerates files while respecting filter rules.
    /// </summary>
    private void EnumerateFilesRecursively(string directory, string rootPath, List<string> files)
    {
        try
        {
            var enumerationOptions = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = false
            };

            // Get files in current directory
            foreach (var file in Directory.EnumerateFiles(directory, "*", enumerationOptions))
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    if (!_fileChecker.ShouldSkip(fileInfo, rootPath))
                    {
                        files.Add(file);
                    }
                }
                catch
                {
                    // Skip files that can't be accessed
                }
            }

            // Recursively process subdirectories
            foreach (var subDir in Directory.EnumerateDirectories(directory, "*", enumerationOptions))
            {
                try
                {
                    var dirInfo = new DirectoryInfo(subDir);
                    if (!_fileChecker.ShouldSkip(dirInfo, rootPath))
                    {
                        EnumerateFilesRecursively(subDir, rootPath, files);
                    }
                }
                catch
                {
                    // Skip directories that can't be accessed
                }
            }
        }
        catch (Exception ex)
        {
            _console.WriteLine($"\n⚠️ Warning: Could not enumerate directory {directory}: {ex.Message}");
        }
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
