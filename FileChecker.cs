using System.Text.RegularExpressions;
using CodeContext;

public class FileChecker
{
    private static readonly HashSet<string> IgnoredExtensions = new(StringComparer.OrdinalIgnoreCase)
{
    // Executable and library files
    ".exe", ".dll", ".pdb", ".bin", ".obj", ".lib", ".so", ".dylib", ".a", ".o",

    // Image files
    ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".svg", ".webp", ".tiff", ".tif", ".raw", ".psd", ".ai", ".eps", ".ps",

    // Audio and video files
    ".mp3", ".mp4", ".wav", ".avi", ".mov", ".flv", ".wmv", ".m4a", ".m4v", ".mkv", ".webm", ".ogg",

    // Compressed files
    ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".tgz",

    // Database files
    ".db", ".sqlite", ".mdf", ".ldf", ".bak", ".mdb", ".accdb",

    // Document files
    ".docx", ".xlsx", ".pptx", ".pdf", ".doc", ".xls", ".ppt", ".rtf", ".odt", ".ods", ".odp",

    // Log and temporary files
    ".log", ".cache", ".tmp", ".temp",

    // Minified and source map files
    ".min.js", ".min.css", ".map", ".lock",

    // Design files
    ".sketch", ".fig", ".xd",

    // Deployment and settings files
    ".pub", ".pubxml", ".publishsettings", ".settings", ".suo", ".user", ".userosscache",

    // Version control files
    ".vspscc", ".vssscc", ".pidb", ".scc",

    // System files
    ".DS_Store", ".localized", ".manifest",

    // Project-specific files
    ".csproj.user", ".sln.docstates", ".suo", ".user", ".vssscc",

    // Compiler and build output
    ".pdb", ".ilk", ".msi", ".idb", ".pch", ".res",

    // Font files
    ".eot", ".ttf", ".woff", ".woff2",

    // 3D model files
    ".fbx", ".obj", ".3ds", ".max",

    // Unity-specific files
    ".unity", ".unitypackage", ".asset",

    // Certificate files
    ".pfx", ".cer", ".crt",

    // Package manager files
    ".nupkg", ".snupkg",

    // Java-specific files
    ".class", ".jar",

    // Python-specific files
    ".pyc", ".pyo",

    // Node.js-specific files
    ".node",

    // Ruby-specific files
    ".gem",

    // Rust-specific files
    ".rlib",

    // Go-specific files
    ".a",

    // Swift-specific files
    ".swiftmodule",

    // Docker-specific files
    ".dockerignore",

    // Kubernetes-specific files
    ".kubeconfig",

    // Machine learning model files
    ".h5", ".pkl", ".onnx",

    // Executable scripts (to be cautious)
    ".bat", ".sh", ".cmd", ".ps1"
};

private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
{
    // Version control systems
    ".git", ".svn", ".hg", ".bzr", ".cvs",

    // IDE and editor-specific
    ".vs", ".idea", ".vscode", ".atom", ".sublime-project",

    // Build output
    "bin", "obj", "Debug", "Release", "x64", "x86", "AnyCPU",

    // Package management
    "packages", "node_modules", "bower_components", "jspm_packages",

    // Python-specific
    "__pycache__", "venv", "env", "virtualenv", ".venv", ".env", ".pytest_cache",

    // Ruby-specific
    ".bundle", "vendor/bundle",

    // Java-specific
    "target", ".gradle", "build",

    // JavaScript/TypeScript-specific
    "dist", "out", "build", ".next", ".nuxt", ".cache",

    // Testing and coverage
    "coverage", "test-results", "reports", ".nyc_output",

    // Logs and temporary files
    "logs", "temp", "tmp", ".temp", ".tmp",

    // Content and media
    "uploads", "media", "static", "public", "assets",

    // Third-party and dependencies
    "vendor", "third-party", "external", "lib", "libs",

    // WordPress-specific
    "wp-content", "wp-includes", "wp-admin",

    // Mobile development
    "Pods", "DerivedData",

    // Containerization
    ".docker",

    // CI/CD
    ".github", ".gitlab", ".circleci", ".jenkins",

    // Documentation
    "docs", "_site", ".docusaurus",

    // Caching
    ".cache", ".sass-cache", ".parcel-cache",

    // Compiled languages
    "__pycache__", ".mypy_cache", ".rpt2_cache", ".rts2_cache_cjs", ".rts2_cache_es", ".rts2_cache_umd",

    // OS-specific
    ".DS_Store", "Thumbs.db",

    // Dependency lock files directory
    ".pnpm-store",

    // Serverless frameworks
    ".serverless",

    // Terraform
    ".terraform",

    // Yarn
    ".yarn",

    // Expo (React Native)
    ".expo",

    // Electron
    "out",

    // Flutter/Dart
    ".dart_tool", ".flutter-plugins", ".flutter-plugins-dependencies",

    // Kubernetes
    ".kube",

    // Ansible
    ".ansible",

    // Chef
    ".chef",

    // Vagrant
    ".vagrant",

    // Unity
    "Library", "Temp", "Obj", "Builds", "Logs",

    // Unreal Engine
    "Binaries", "Build", "Saved", "Intermediate",

    // Godot Engine
    ".import", "export_presets.cfg",

    // R language
    ".Rproj.user", ".Rhistory", ".RData",

    // Jupyter Notebooks
    ".ipynb_checkpoints",

    // LaTeX
    "build", "out",

    // Rust
    "target",

    // Go
    "vendor",

    // Elixir
    "_build", ".elixir_ls",

    // Helm Charts
    "charts",

    // Pipenv
    ".venv"
};

    private static readonly HashSet<string> IgnoredFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bzrignore", ".coveragerc", ".editorconfig", ".env", ".env.development",
        ".env.production", ".env.local", ".env.test", ".eslintrc", ".gitattributes",
        "thumbs.db", "desktop.ini", ".DS_Store", "npm-debug.log", "yarn-error.log",
        "package-lock.json", "yarn.lock", "composer.lock", ".gitignore"
    };

    private static List<string> gitIgnorePatterns;
    private const long MaxFileSizeBytes = 100 * 1024; // 100KB

    public static bool ShouldSkip(FileSystemInfo info, string rootPath)
    {
        // Check if any parent directory is in the ignored list
        string relativePath = Path.GetRelativePath(rootPath, info.FullName);
        string[] pathParts = relativePath.Split(Path.DirectorySeparatorChar);

        for (int i = 0; i < pathParts.Length; i++)
        {
            string currentPart = pathParts[i];
            if (IgnoredDirectories.Contains(currentPart))
                return true;
        }

        if (info.Attributes.HasFlag(FileAttributes.Directory))
            return false; // We've already checked if it's an ignored directory

        // Check for ignored files
        if (IgnoredFiles.Contains(info.Name))
            return true;

        // Improved extension checking
        string fileName = info.Name;
        string extension = Path.GetExtension(fileName);
        if (IgnoredExtensions.Contains(extension))
            return true;

        // Check for compound extensions like .min.css
        int lastDotIndex = fileName.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            int secondLastDotIndex = fileName.LastIndexOf('.', lastDotIndex - 1);
            if (secondLastDotIndex >= 0)
            {
                string compoundExtension = fileName.Substring(secondLastDotIndex);
                if (IgnoredExtensions.Contains(compoundExtension))
                    return true;
            }
        }

        if (info is FileInfo fileInfo && fileInfo.Length > MaxFileSizeBytes)
            return true;

        if (IsInGitRepository(rootPath))
        {
            if (gitIgnorePatterns == null)
                LoadGitIgnore(rootPath);

            if (IsIgnoredByGitIgnore(info.FullName, rootPath))
                return true;
        }

        return FileUtils.IsBinaryFile(info.FullName) || 
               IgnoredDirectories.Any(dir => info.FullName.Contains($"{Path.DirectorySeparatorChar}{dir}{Path.DirectorySeparatorChar}") || IsGeneratedCode(info.FullName));
    }


    private static bool IsGeneratedCode(string filePath)
    {
        const int linesToCheck = 10;
        var lines = File.ReadLines(filePath).Take(linesToCheck);
        return lines.Any(line => line.Contains("<auto-generated />"));
    }

    private static bool IsInGitRepository(string path)
    {
        while (!string.IsNullOrEmpty(path))
        {
            if (Directory.Exists(Path.Combine(path, ".git")))
                return true;
            path = Path.GetDirectoryName(path);
        }
        return false;
    }

    private static void LoadGitIgnore(string rootPath)
    {
        gitIgnorePatterns = new List<string>();
        string gitIgnorePath = Path.Combine(rootPath, ".gitignore");
        if (File.Exists(gitIgnorePath))
        {
            gitIgnorePatterns.AddRange(File.ReadAllLines(gitIgnorePath)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#')));
        }
    }

    private static bool IsIgnoredByGitIgnore(string filePath, string rootPath)
    {
        string relativePath = Path.GetRelativePath(rootPath, filePath);
        return gitIgnorePatterns.Any(pattern => IsMatch(relativePath, pattern));
    }

    private static bool IsMatch(string path, string pattern)
    {
        pattern = pattern.Replace(".", "\\.").Replace("*", ".*").Replace("?", ".");
        return Regex.IsMatch(path, $"^{pattern}$", RegexOptions.IgnoreCase);
    }
}