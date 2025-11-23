using System.Collections.Immutable;
using CodeContext.Interfaces;
using CodeContext.Utils;

namespace CodeContext.Services;

/// <summary>
/// Functional service for scanning and analyzing project directories.
/// Uses immutable data structures and separates I/O from pure logic.
/// </summary>
public class ProjectScanner
{
    private readonly IFileChecker _fileChecker;
    private readonly IConsoleWriter _console;

    public ProjectScanner(IFileChecker fileChecker, IConsoleWriter console)
    {
        _fileChecker = Guard.NotNull(fileChecker, nameof(fileChecker));
        _console = Guard.NotNull(console, nameof(console));
    }

    /// <summary>
    /// Gets user input with a prompt (pure I/O operation).
    /// </summary>
    /// <param name="prompt">The prompt to display.</param>
    /// <returns>The user's input.</returns>
    public string GetUserInput(string prompt)
    {
        _console.Write(prompt);
        return _console.ReadLine() ?? string.Empty;
    }

    /// <summary>
    /// Represents the context for a scan operation (immutable).
    /// </summary>
    private sealed record ScanContext(string RootPath, string GitRepoRoot)
    {
        public static ScanContext Create(string projectPath) =>
            new(projectPath, GitHelper.FindRepositoryRoot(projectPath) ?? projectPath);
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

        if (indent == 0)
        {
            _console.WriteLine("📁 Analyzing directory structure...");
        }

        var context = ScanContext.Create(projectPath);
        var lines = GetProjectStructureLines(projectPath, context, indent).ToImmutableArray();

        // Report progress (side effect isolated to specific calls)
        if (indent == 0 && lines.Length > 0)
        {
            _console.Write($"\r⏳ Progress: 100% ({lines.Length}/{lines.Length})");
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Recursive function to generate directory structure lines (performs I/O).
    /// Uses lazy evaluation via yield return.
    /// </summary>
    private IEnumerable<string> GetProjectStructureLines(string directoryPath, ScanContext context, int indent)
    {
        var entries = GetFilteredEntries(directoryPath, context.GitRepoRoot);

        foreach (var entry in entries)
        {
            if (Directory.Exists(entry))
            {
                var dir = new DirectoryInfo(entry);
                yield return FormatDirectoryEntry(dir.Name, indent);

                // Recursively yield subdirectory contents
                foreach (var line in GetProjectStructureLines(entry, context, indent + 1))
                {
                    yield return line;
                }
            }
            else
            {
                var file = new FileInfo(entry);
                yield return FormatFileEntry(file.Name, file.Extension, indent);
            }
        }
    }

    /// <summary>
    /// I/O operation: gets filtered and sorted directory entries.
    /// Includes console logging side effects on errors.
    /// </summary>
    private IEnumerable<string> GetFilteredEntries(string directoryPath, string rootPath)
    {
        try
        {
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = false
            };

            return Directory.EnumerateFileSystemEntries(directoryPath, "*", options)
                .OrderBy(e => e)
                .Where(e => ShouldIncludeEntry(e, rootPath));
        }
        catch (Exception ex)
        {
            _console.WriteLine($"\n⚠️ Warning: Could not enumerate directory {directoryPath}: {ex.Message}");
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Predicate to determine if an entry should be included (may perform I/O).
    /// </summary>
    private bool ShouldIncludeEntry(string entryPath, string rootPath)
    {
        try
        {
            return !_fileChecker.ShouldSkip(new FileInfo(entryPath), rootPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Pure function to format a directory entry.
    /// </summary>
    private static string FormatDirectoryEntry(string name, int indent) =>
        $"{new string(' ', indent * 2)}[{name}/]";

    /// <summary>
    /// Pure function to format a file entry.
    /// </summary>
    private static string FormatFileEntry(string name, string extension, int indent) =>
        $"{new string(' ', indent * 2)}[{extension}] {name}";

    /// <summary>
    /// Retrieves the contents of all non-filtered files in the directory tree.
    /// </summary>
    /// <param name="projectPath">The directory path to scan.</param>
    /// <returns>A string containing all file contents with separators.</returns>
    public string GetFileContents(string projectPath)
    {
        Guard.DirectoryExists(projectPath, nameof(projectPath));
        _console.WriteLine("\n📄 Processing files...");

        var context = ScanContext.Create(projectPath);
        var files = EnumerateFilesRecursively(projectPath, context.GitRepoRoot).ToImmutableArray();

        var fileContents = files
            .Select((file, index) =>
            {
                WriteProgress(index + 1, files.Length);
                return ReadFileWithSeparator(file);
            })
            .Where(content => content != null)
            .ToImmutableArray();

        return string.Join("\n\n", fileContents!);
    }

    /// <summary>
    /// Recursive I/O operation to enumerate all files in directory tree.
    /// Uses lazy evaluation via yield return.
    /// </summary>
    private IEnumerable<string> EnumerateFilesRecursively(string directory, string rootPath)
    {
        var options = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = false
        };

        // Yield files in current directory
        foreach (var file in GetFilteredFiles(directory, rootPath, options))
        {
            yield return file;
        }

        // Recursively yield files from subdirectories
        foreach (var subDir in GetFilteredDirectories(directory, rootPath, options))
        {
            foreach (var file in EnumerateFilesRecursively(subDir, rootPath))
            {
                yield return file;
            }
        }
    }

    /// <summary>
    /// I/O operation: gets filtered files from a directory.
    /// Includes console logging side effects on errors.
    /// </summary>
    private IEnumerable<string> GetFilteredFiles(string directory, string rootPath, EnumerationOptions options)
    {
        try
        {
            return Directory.EnumerateFiles(directory, "*", options)
                .Where(file => ShouldIncludeFile(file, rootPath));
        }
        catch (Exception ex)
        {
            _console.WriteLine($"\n⚠️ Warning: Could not enumerate directory {directory}: {ex.Message}");
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// I/O operation: gets filtered subdirectories from a directory.
    /// </summary>
    private IEnumerable<string> GetFilteredDirectories(string directory, string rootPath, EnumerationOptions options)
    {
        try
        {
            return Directory.EnumerateDirectories(directory, "*", options)
                .Where(dir => ShouldIncludeDirectory(dir, rootPath));
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// Predicate to check if a file should be included (may perform I/O).
    /// </summary>
    private bool ShouldIncludeFile(string filePath, string rootPath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            return !_fileChecker.ShouldSkip(fileInfo, rootPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Predicate to check if a directory should be included (may perform I/O).
    /// </summary>
    private bool ShouldIncludeDirectory(string dirPath, string rootPath)
    {
        try
        {
            var dirInfo = new DirectoryInfo(dirPath);
            return !_fileChecker.ShouldSkip(dirInfo, rootPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// I/O function to read file with formatted separator.
    /// Returns null on error for filtering.
    /// </summary>
    private string? ReadFileWithSeparator(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            return $"{filePath}\n{new string('-', 100)}\n{content}";
        }
        catch (Exception ex)
        {
            _console.WriteLine($"\n⚠️ Warning: Could not read file {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Side effect: writes progress to console.
    /// </summary>
    private void WriteProgress(int current, int total)
    {
        var percent = (int)((current / (double)total) * 100);
        _console.Write($"\r⏳ Progress: {percent}% ({current}/{total})");
    }
}
