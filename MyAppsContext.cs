namespace CodeContext;
public class MyAppsContext
{
    public static string GitRepoRoot { get; private set; }

    public static string GetUserInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }

    public static string GetProjectStructure(string path, int indent = 0)
    {
        if (GitRepoRoot == null) GitRepoRoot = FindGitRepoRoot(path);
        if (indent == 0) Console.WriteLine("📁 Analyzing directory structure...");

        var entries = Directory.EnumerateFileSystemEntries(path)
            .OrderBy(e => e)
            .Where(e => !FileChecker.ShouldSkip(new FileInfo(e), GitRepoRoot))
            .ToList();

        return string.Join("\n", entries.Select((e, i) =>
        {
            if (indent == 0) WriteProgress(i + 1, entries.Count);

            var info = new FileInfo(e);
            var indentation = new string(' ', indent * 2);
            var result = $"{indentation}{info.Name}{(info.Attributes.HasFlag(FileAttributes.Directory) ? "/" : "")}";

            return info.Attributes.HasFlag(FileAttributes.Directory)
                ? result + "\n" + GetProjectStructure(e, indent + 1)
                : result;
        }));
    }

    public static string GetFileContents(string path)
    {
        if (GitRepoRoot == null) GitRepoRoot = FindGitRepoRoot(path);
        Console.WriteLine("\n📄 Processing files...");

        var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Where(f => !FileChecker.ShouldSkip(new FileInfo(f), GitRepoRoot))
            .ToList();

        return string.Join("\n\n", files.Select((f, i) =>
        {
            WriteProgress(i + 1, files.Count);
            return $"{f}\n{new string('-', 100)}\n{File.ReadAllText(f)}";
        }));
    }

    private static string FindGitRepoRoot(string path) =>
        Directory.Exists(Path.Combine(path, ".git"))
            ? path
            : string.IsNullOrEmpty(path) ? null : FindGitRepoRoot(Path.GetDirectoryName(path));

    private static void WriteProgress(int current, int total)
    {
        var percent = (int)((current / (double)total) * 100);
        Console.Write($"\r⏳ Progress: {percent}% ({current}/{total})");
    }
}