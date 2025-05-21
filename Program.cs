using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CodeContext;

Console.OutputEncoding = Encoding.UTF8;

try
{
    var config = LoadConfig();
    var path = GetValidPath(args.FirstOrDefault() ?? config.DefaultInputPath);

    // 1. Get input folder name
    var inputFolderName = new DirectoryInfo(path).Name;
    if (string.IsNullOrEmpty(inputFolderName) || inputFolderName == ".") // Handle cases like "." or "C:\"
    {
        // For "." use current directory name, for root drives, use a generic name or drive letter
        inputFolderName = new DirectoryInfo(Environment.CurrentDirectory).Name;
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
        {
            // If path was like "C:/", DirectoryInfo(path).Name might be "C:".
            // Let's try to get a more descriptive name if it's a root drive.
            var root = Path.GetPathRoot(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(root))
            {
                inputFolderName = root.Replace(Path.DirectorySeparatorChar.ToString(), "").Replace(Path.AltDirectorySeparatorChar.ToString(), "").Replace(":", "");
                if (string.IsNullOrEmpty(inputFolderName)) inputFolderName = "root";
            }
        }
    }


    // 2. Construct prefixed default file name
    var prefixedDefaultFileName = $"{inputFolderName}_{config.DefaultOutputFileName}";

    // 3. Default output is INSIDE the input path folder with the prefixed name
    var defaultFullOutputPath = Path.Combine(path, prefixedDefaultFileName);

    // 4. Get final output path (could be a file or directory specified by user, or the default)
    var outputTarget = GetValidOutputPath(args.ElementAtOrDefault(1), defaultFullOutputPath);

    var sw = Stopwatch.StartNew();
    var content = BuildContent(path, config);
    var stats = CalculateStats(path, content, sw.Elapsed);

    // 5. Pass prefixedDefaultFileName to WriteOutput
    string actualOutputPath = WriteOutput(outputTarget, content, config.OutputFormat, prefixedDefaultFileName);
    Console.WriteLine($"\n✅ Output written to {actualOutputPath}"); // 6. Use actual output path
    Console.WriteLine(stats);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Environment.Exit(1);
}

static Config LoadConfig() =>
    JsonSerializer.Deserialize<Config>(
        File.Exists("config.json") ? File.ReadAllText("config.json") : "{}"
    ) ?? new();

static string GetValidPath(string defaultPath)
{
    var path = MyAppsContext.GetUserInput($"Enter the path to index (default: {defaultPath}): ");
    var finalPath = string.IsNullOrWhiteSpace(path) ? defaultPath : path;
    var fullPath = Path.GetFullPath(finalPath); // Resolve to full path for consistency

    return Directory.Exists(fullPath)
        ? fullPath
        : throw new DirectoryNotFoundException($"Invalid directory path: {fullPath}");
}

// Modified to accept user's argument and the fully resolved default path
static string GetValidOutputPath(string? outputArgFromUser, string defaultFullOutputPathIfNoArgAndNoInput)
{
    // If an argument is provided, use it directly.
    if (!string.IsNullOrWhiteSpace(outputArgFromUser))
    {
        return Path.GetFullPath(outputArgFromUser); // Resolve to full path
    }
    // Otherwise, prompt the user, showing the calculated default.
    var userInput = MyAppsContext.GetUserInput($"Enter output file/directory (default: {defaultFullOutputPathIfNoArgAndNoInput}): ");
    return string.IsNullOrWhiteSpace(userInput)
        ? defaultFullOutputPathIfNoArgAndNoInput
        : Path.GetFullPath(userInput); // Resolve to full path
}


static string BuildContent(string path, Config config)
{
    try
    {
        var sb = new StringBuilder();

        if (config.IncludeStructure)
        {
            sb.AppendLine("Project Structure:")
              .AppendLine(MyAppsContext.GetProjectStructure(path));
        }

        if (config.IncludeContents)
        {
            sb.AppendLine("\nFile Contents:")
              .AppendLine(MyAppsContext.GetFileContents(path));
        }

        return sb.ToString();
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Error processing project at {path}", ex);
    }
}

static string CalculateStats(string path, string content, TimeSpan timeTaken) =>
    $"""

    📊 Stats:
    📁 Files processed: {Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length}
    📝 Total lines: {content.Count(c => c == '\n')}
    ⏱️ Time taken: {timeTaken.TotalSeconds:F2}s
    💾 Output size: {content.Length} characters
    """;

// Modified to accept the effective output filename and return the actual path written
static string WriteOutput(string outputTarget, string content, string format, string effectiveOutputFileName)
{
    Console.WriteLine("\n💾 Writing output...");
    string resolvedFilePath = "";
    try
    {
        // If outputTarget is an existing directory, combine it with the effectiveOutputFileName.
        // Otherwise, assume outputTarget is the full file path.
        if (Directory.Exists(outputTarget))
        {
            resolvedFilePath = Path.Combine(outputTarget, effectiveOutputFileName);
        }
        else
        {
            resolvedFilePath = outputTarget;
            // Ensure the directory for the output file exists
            var outputDirectory = Path.GetDirectoryName(resolvedFilePath);
            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
        }

        var formattedContent = format.ToLower() == "json"
            ? JsonSerializer.Serialize(new { content, timestamp = DateTime.Now })
            : content;
        File.WriteAllText(resolvedFilePath, formattedContent);
        return resolvedFilePath; // Return the actual path
    }
    catch (Exception ex)
    {
        // Try to provide a more specific path in the error if resolvedFilePath was determined
        string errorPath = string.IsNullOrEmpty(resolvedFilePath) ? outputTarget : resolvedFilePath;
        throw new IOException($"Error writing output to {errorPath}", ex);
    }
}

record Config
{
    public string DefaultInputPath { get; init; } = ".";
    public string DefaultOutputFileName { get; init; } = "context.txt"; // Base name
    public string OutputFormat { get; init; } = "text";
    public bool IncludeStructure { get; init; } = true;
    public bool IncludeContents { get; init; } = true;
}