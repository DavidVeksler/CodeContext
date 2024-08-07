using System.Text;

namespace CodeContext;

public class MyAppsContext
{
    private static string _gitRepoRootPath;

    public static string GetUserInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }

    public static string GetProjectStructure(string path, int indent = 0)
    {
        if (_gitRepoRootPath == null)
            _gitRepoRootPath = FindGitRepoRoot(path);

        var sb = new StringBuilder();
        foreach (var entry in Directory.EnumerateFileSystemEntries(path).OrderBy(e => e))
        {
            var info = new FileInfo(entry);
            if (FileChecker.ShouldSkip(info, _gitRepoRootPath)) continue;

            var indentation = new string(' ', indent * 2);
            sb.AppendLine($"{indentation}{info.Name}{(info.Attributes.HasFlag(FileAttributes.Directory) ? "/" : "")}");

            if (info.Attributes.HasFlag(FileAttributes.Directory))
                sb.Append(GetProjectStructure(entry, indent + 1));
        }
        return sb.ToString();
    }

    public static string GetFileContents(string path)
    {
        if (_gitRepoRootPath == null)
            _gitRepoRootPath = FindGitRepoRoot(path);

        var sb = new StringBuilder();
        foreach (var filePath in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            var info = new FileInfo(filePath);
            if (FileChecker.ShouldSkip(info, _gitRepoRootPath)) continue;

            sb.AppendLine(filePath);
            sb.AppendLine(new string('-', 100));
            sb.AppendLine(File.ReadAllText(filePath));
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string FindGitRepoRoot(string path)
    {
        while (!string.IsNullOrEmpty(path))
        {
            if (Directory.Exists(Path.Combine(path, ".git")))
                return path;
            path = Path.GetDirectoryName(path);
        }
        return null; // Not in a Git repository
    }
}