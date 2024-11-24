using System.Text;

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

        var sb = new StringBuilder();
        
        // Process all entries
        for (int i = 0; i < entries.Count; i++)
        {
            WriteProgress(i + 1, entries.Count);
            var entry = entries[i];
            
            if (Directory.Exists(entry))
            {
                var dir = new DirectoryInfo(entry);
                sb.AppendLine($"{new string(' ', indent * 2)}[{dir.Name}/]")
                  .Append(GetProjectStructure(entry, indent + 1));
            }
            else
            {
                var file = new FileInfo(entry);
                sb.AppendLine($"{new string(' ', indent * 2)}[{file.Extension}] {file.Name}");
            }
        }

        return sb.ToString();
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