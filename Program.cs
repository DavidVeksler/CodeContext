using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CodeContext;

Console.OutputEncoding = Encoding.UTF8;

try
{
    var config = LoadConfig();
    var path = GetValidPath(args.FirstOrDefault() ?? config.DefaultInputPath);

    var inputFolderName = GetInputFolderName(path);
    var prefixedDefaultFileName = $"{inputFolderName}_{config.DefaultOutputFileName}";
    var defaultFullOutputPath = Path.Combine(path, prefixedDefaultFileName);
    var outputTarget = GetValidOutputPath(args.ElementAtOrDefault(1), defaultFullOutputPath);

    var sw = Stopwatch.StartNew();
    var content = BuildContent(path, config);
    var stats = CalculateStats(path, content, sw.Elapsed);

    string actualOutputPath = WriteOutput(outputTarget, content, config.OutputFormat, prefixedDefaultFileName);
    Console.WriteLine($"\n✅ Output written to {actualOutputPath}");
    Console.WriteLine(stats);
}
catch (DirectoryNotFoundException ex)
{
    Console.WriteLine($"❌ Directory Error: {ex.Message}");
    Environment.Exit(1);
}
catch (IOException ex)
{
    Console.WriteLine($"❌ I/O Error: {ex.Message}");
    Environment.Exit(2);
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"❌ Access Denied: {ex.Message}");
    Environment.Exit(3);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Unexpected Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"   Details: {ex.InnerException.Message}");
    }
    Environment.Exit(4);
}

/// <summary>
/// Loads configuration from config.json file if it exists, otherwise returns default configuration.
/// </summary>
/// <returns>The loaded or default configuration.</returns>
static Config LoadConfig()
{
    try
    {
        var configJson = File.Exists("config.json") ? File.ReadAllText("config.json") : "{}";
        return JsonSerializer.Deserialize<Config>(configJson) ?? new Config();
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"⚠️ Warning: Invalid config.json format ({ex.Message}). Using defaults.");
        return new Config();
    }
}

/// <summary>
/// Gets and validates the directory path to be indexed.
/// </summary>
/// <param name="defaultPath">The default path to use if user doesn't provide one.</param>
/// <returns>The validated full path.</returns>
/// <exception cref="DirectoryNotFoundException">Thrown when the specified directory doesn't exist.</exception>
static string GetValidPath(string defaultPath)
{
    var path = MyAppsContext.GetUserInput($"Enter the path to index (default: {defaultPath}): ");
    var finalPath = string.IsNullOrWhiteSpace(path) ? defaultPath : path;
    var fullPath = Path.GetFullPath(finalPath);

    if (!Directory.Exists(fullPath))
    {
        throw new DirectoryNotFoundException($"Directory not found: {fullPath}");
    }

    return fullPath;
}

/// <summary>
/// Gets and validates the output path for the generated context file.
/// </summary>
/// <param name="outputArgFromUser">Optional output path from command-line arguments.</param>
/// <param name="defaultFullOutputPathIfNoArgAndNoInput">Default output path if none provided.</param>
/// <returns>The validated full output path.</returns>
static string GetValidOutputPath(string? outputArgFromUser, string defaultFullOutputPathIfNoArgAndNoInput)
{
    if (!string.IsNullOrWhiteSpace(outputArgFromUser))
    {
        return Path.GetFullPath(outputArgFromUser);
    }

    var userInput = MyAppsContext.GetUserInput($"Enter output file/directory (default: {defaultFullOutputPathIfNoArgAndNoInput}): ");
    return string.IsNullOrWhiteSpace(userInput)
        ? defaultFullOutputPathIfNoArgAndNoInput
        : Path.GetFullPath(userInput);
}

/// <summary>
/// Extracts a clean folder name from the input path for output file naming.
/// </summary>
/// <param name="path">The input path.</param>
/// <returns>A sanitized folder name.</returns>
static string GetInputFolderName(string path)
{
    var inputFolderName = new DirectoryInfo(path).Name;

    if (string.IsNullOrEmpty(inputFolderName) || inputFolderName == ".")
    {
        inputFolderName = new DirectoryInfo(Environment.CurrentDirectory).Name;

        if (path.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
            path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
        {
            var root = Path.GetPathRoot(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(root))
            {
                inputFolderName = root
                    .Replace(Path.DirectorySeparatorChar.ToString(), "")
                    .Replace(Path.AltDirectorySeparatorChar.ToString(), "")
                    .Replace(":", "");

                if (string.IsNullOrEmpty(inputFolderName))
                {
                    inputFolderName = "root";
                }
            }
        }
    }

    return inputFolderName;
}

/// <summary>
/// Builds the complete content output including structure and file contents based on configuration.
/// </summary>
/// <param name="path">The directory path to process.</param>
/// <param name="config">The configuration specifying what to include.</param>
/// <returns>The complete output content.</returns>
/// <exception cref="InvalidOperationException">Thrown when an error occurs during processing.</exception>
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

/// <summary>
/// Calculates and formats statistics about the processing operation.
/// </summary>
/// <param name="path">The directory that was processed.</param>
/// <param name="content">The generated content.</param>
/// <param name="timeTaken">Time elapsed during processing.</param>
/// <returns>Formatted statistics string.</returns>
static string CalculateStats(string path, string content, TimeSpan timeTaken)
{
    try
    {
        var fileCount = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
        var lineCount = content.Count(c => c == '\n');

        return $"""

        📊 Stats:
        📁 Files processed: {fileCount}
        📝 Total lines: {lineCount}
        ⏱️ Time taken: {timeTaken.TotalSeconds:F2}s
        💾 Output size: {content.Length} characters
        """;
    }
    catch (Exception)
    {
        return "\n📊 Stats: Unable to calculate statistics";
    }
}

/// <summary>
/// Writes the generated content to the specified output location.
/// </summary>
/// <param name="outputTarget">Target path (file or directory).</param>
/// <param name="content">Content to write.</param>
/// <param name="format">Output format (text or json).</param>
/// <param name="effectiveOutputFileName">Filename to use if outputTarget is a directory.</param>
/// <returns>The actual path where the file was written.</returns>
/// <exception cref="IOException">Thrown when an error occurs during file writing.</exception>
static string WriteOutput(string outputTarget, string content, string format, string effectiveOutputFileName)
{
    Console.WriteLine("\n💾 Writing output...");
    string resolvedFilePath;

    try
    {
        if (Directory.Exists(outputTarget))
        {
            resolvedFilePath = Path.Combine(outputTarget, effectiveOutputFileName);
        }
        else
        {
            resolvedFilePath = outputTarget;
            var outputDirectory = Path.GetDirectoryName(resolvedFilePath);

            if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
        }

        var formattedContent = format.ToLower() == "json"
            ? JsonSerializer.Serialize(new { content, timestamp = DateTime.Now }, new JsonSerializerOptions { WriteIndented = true })
            : content;

        File.WriteAllText(resolvedFilePath, formattedContent);
        return resolvedFilePath;
    }
    catch (UnauthorizedAccessException ex)
    {
        throw new IOException($"Access denied writing to {outputTarget}", ex);
    }
    catch (Exception ex)
    {
        throw new IOException($"Failed to write output to {outputTarget}", ex);
    }
}

/// <summary>
/// Application configuration record.
/// </summary>
/// <param name="DefaultInputPath">Default directory path to scan.</param>
/// <param name="DefaultOutputFileName">Default output file name.</param>
/// <param name="OutputFormat">Output format (text or json).</param>
/// <param name="IncludeStructure">Whether to include directory structure in output.</param>
/// <param name="IncludeContents">Whether to include file contents in output.</param>
record Config
{
    public string DefaultInputPath { get; init; } = ".";
    public string DefaultOutputFileName { get; init; } = "context.txt";
    public string OutputFormat { get; init; } = "text";
    public bool IncludeStructure { get; init; } = true;
    public bool IncludeContents { get; init; } = true;
}
