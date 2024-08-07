using System.Text;
using DotNet.Globbing;

namespace CodeContext
{

    class MyAppsContext
    {
        internal static HashSet<Glob> ignorePatterns = new HashSet<Glob>();
        private static readonly string[] DefaultIgnorePatterns = new[]
        {
        // Version control
        ".git", ".svn", ".hg", ".bzr", "_darcs", "CVS",
        // IDE files
        ".vs", ".idea", "*.suo", "*.user", "*.userosscache", "*.sln.docstates",
        // Build results
        "bin", "obj", "Debug", "Release", "x64", "x86", 
        // NuGet packages
        "packages", "*.nupkg",
        // Node.js
        "node_modules", "npm-debug.log",
        // Python
        "__pycache__", "*.pyc", "*.pyo", "*.pyd", ".Python", "env", "venv",
        // Backup files
        "*~", "*.bak", "*.swp", "*.tmp",
        // OS generated files
        ".DS_Store", ".DS_Store?", "._*", ".Spotlight-V100", ".Trashes", "ehthumbs.db", "Thumbs.db",
        // Log files
        "*.log",
        // Compiled source
        "*.com", "*.class", "*.dll", "*.exe", "*.o", "*.so",
        // Packages
        "*.7z", "*.dmg", "*.gz", "*.iso", "*.jar", "*.rar", "*.tar", "*.zip",
        // Databases
        "*.sqlite"
    };


        internal static string GetUserInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        internal static void LoadIgnorePatterns(string ignorePath)
        {
            foreach (var pattern in DefaultIgnorePatterns)
            {
                ignorePatterns.Add(Glob.Parse(pattern));
            }

            if (File.Exists(ignorePath))
            {
                foreach (var line in File.ReadAllLines(ignorePath))
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                        ignorePatterns.Add(Glob.Parse(line));
                }
            }
        }

        static bool ShouldIgnore(string path, string basePath)
        {
            var relativePath = Path.GetRelativePath(basePath, path);
            return ignorePatterns.Any(pattern => pattern.IsMatch(relativePath));
        }

        internal static string GetProjectStructure(string path, int indent = 0)
        {
            var sb = new StringBuilder();
            var di = new DirectoryInfo(path);
            var indentation = new string(' ', indent * 2);

            foreach (var item in di.GetFileSystemInfos().OrderBy(f => f.Name))
            {
                if (!ShouldIgnore(item.FullName, path))
                {
                    sb.AppendLine($"{indentation}{item.Name}{(item is DirectoryInfo ? "/" : "")}");

                    if (item is DirectoryInfo)
                        sb.Append(GetProjectStructure(item.FullName, indent + 1));
                }
            }

            return sb.ToString();
        }

        internal static string GetFileContents(string path)
        {
            var sb = new StringBuilder();

            foreach (var filePath in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                if (!ShouldIgnore(filePath, path))
                {
                    sb.AppendLine(filePath);
                    sb.AppendLine(new string('-', 100));
                    sb.AppendLine(File.ReadAllText(filePath));
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
