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

        return string.Join("\n", Directory.EnumerateFileSystemEntries(path)
            .OrderBy(e => e)
            .Where(e => !FileChecker.ShouldSkip(new FileInfo(e), _gitRepoRootPath))
            .Select(e =>
            {
                var info = new FileInfo(e);
                var indentation = new string(' ', indent * 2);
                var result =
                    $"{indentation}{info.Name}{(info.Attributes.HasFlag(FileAttributes.Directory) ? "/" : "")}";
                return info.Attributes.HasFlag(FileAttributes.Directory)
                    ? result + "\n" + GetProjectStructure(e, indent + 1)
                    : result;
            }));
    }

    public static string GetFileContents(string path)
    {
        if (_gitRepoRootPath == null)
            _gitRepoRootPath = FindGitRepoRoot(path);

        return string.Join("\n\n", Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Where(f => !FileChecker.ShouldSkip(new FileInfo(f), _gitRepoRootPath))
            .Select(f => $"{f}\n{new string('-', 100)}\n{File.ReadAllText(f)}"));
    }

    private static string FindGitRepoRoot(string path)
    {
        return Directory.Exists(Path.Combine(path, ".git"))
            ? path
            : string.IsNullOrEmpty(path)
                ? null
                : FindGitRepoRoot(Path.GetDirectoryName(path));
    }
}