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

        // Group files by their type/purpose
        var files = entries.Where(e => !Directory.Exists(e))
            .Select(f => new FileInfo(f))
            .ToList();

        var projectFiles = files.Where(f => f.Extension == ".csproj" || f.Extension == ".sln");
        var configFiles = files.Where(f =>
            f.Name.StartsWith("appsettings") ||
            f.Name.EndsWith(".json") ||
            f.Name.EndsWith(".yml") ||
            f.Name.EndsWith(".config"));
        var sourceFiles = files.Where(f =>
            f.Extension == ".cs" ||
            f.Extension == ".ts" ||
            f.Extension == ".js" ||
            f.Extension == ".py");
        var otherFiles = files.Except(projectFiles)
                             .Except(configFiles)
                             .Except(sourceFiles);

        // Add project overview if it's the root
        if (indent == 0)
        {
            var projFiles = projectFiles.ToList();
            if (projFiles.Any())
            {
                sb.AppendLine("📦 Project Files:");
                foreach (var proj in projFiles)
                {
                    WriteProgress(files.IndexOf(proj) + 1, files.Count);
                    sb.AppendLine($"{new string(' ', (indent + 1) * 2)}[{proj.Extension}] {proj.Name}");
                }
            }

            var configs = configFiles.ToList();
            if (configs.Any())
            {
                sb.AppendLine("\n⚙️ Configuration Files:");
                foreach (var config in configs)
                {
                    WriteProgress(files.IndexOf(config) + 1, files.Count);
                    // Extract environment for appsettings files
                    var env = config.Name.Contains("appsettings.")
                        ? $"({config.Name.Split('.')[1]}) "
                        : "";
                    sb.AppendLine($"{new string(' ', (indent + 1) * 2)}[{config.Extension}] {config.Name} {env}");
                }
            }
        }

        // Add source files
        var srcFiles = sourceFiles.ToList();
        if (srcFiles.Any())
        {
            if (indent == 0) sb.AppendLine("\n💻 Source Files:");
            foreach (var src in srcFiles)
            {
                WriteProgress(files.IndexOf(src) + 1, files.Count);
                var filePurpose = GetFilePurpose(src.Name);
                sb.AppendLine($"{new string(' ', (indent + 1) * 2)}[{src.Extension}] {src.Name} {filePurpose}");
            }
        }

        // Add remaining files
        var remaining = otherFiles.ToList();
        if (remaining.Any())
        {
            if (indent == 0) sb.AppendLine("\n📄 Other Files:");
            foreach (var file in remaining)
            {
                WriteProgress(files.IndexOf(file) + 1, files.Count);
                sb.AppendLine($"{new string(' ', (indent + 1) * 2)}{file.Name}");
            }
        }

        // Process subdirectories
        var directories = entries.Where(e => Directory.Exists(e))
            .Select(d => new DirectoryInfo(d))
            .ToList();

        if (directories.Any())
        {
            if (indent == 0) sb.AppendLine("\n📂 Directories:");
            foreach (var dir in directories)
            {
                var dirPurpose = GetDirectoryPurpose(dir.Name);
                sb.AppendLine($"{new string(' ', (indent + 1) * 2)}[{dir.Name}/] {dirPurpose}")
                  .Append(GetProjectStructure(dir.FullName, indent + 1));
            }
        }

        return sb.ToString();
    }

    private static string GetFilePurpose(string fileName) => fileName.ToLower() switch
    {
        var n when n.EndsWith("controller.cs") => "(API Endpoint)",
        var n when n.EndsWith("service.cs") => "(Business Logic)",
        var n when n.EndsWith("repository.cs") => "(Data Access)",
        var n when n.EndsWith("model.cs") => "(Data Model)",
        var n when n.EndsWith("request.cs") => "(API Request)",
        var n when n.EndsWith("response.cs") => "(API Response)",
        var n when n.EndsWith("dto.cs") => "(Data Transfer)",
        var n when n.EndsWith("mapper.cs") => "(Object Mapping)",
        var n when n == "program.cs" => "(Application Entry)",
        var n when n == "startup.cs" => "(App Configuration)",
        _ => ""
    };

    private static string GetDirectoryPurpose(string dirName) => dirName.ToLower() switch
    {
        "controllers" => "(API Endpoints)",
        "services" => "(Business Logic)",
        "models" => "(Data Models)",
        "views" => "(UI Templates)",
        "repositories" => "(Data Access)",
        "tests" => "(Test Cases)",
        "migrations" => "(DB Migrations)",
        "scripts" => "(Utility Scripts)",
        "docs" => "(Documentation)",
        "config" => "(Configuration)",
        "wwwroot" => "(Static Files)",
        _ => ""
    };

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