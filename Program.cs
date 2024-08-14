using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CodeContext;

Console.OutputEncoding = Encoding.UTF8;

try
{
    var config = LoadConfig();
    var path = GetValidPath(args.FirstOrDefault() ?? config.DefaultInputPath);
    var defaultOutput = Path.Combine(Path.GetDirectoryName(path) ?? ".", config.DefaultOutputFileName);
    var output = GetValidOutputPath(args.ElementAtOrDefault(1) ?? defaultOutput);

    var sw = Stopwatch.StartNew();
    var (structure, contents) = GetProjectInfo(path);
    var content = BuildContent(structure, contents);
    var stats = CalculateStats(path, content, sw.Elapsed);

    WriteOutput(output, content, config.OutputFormat);
    Console.WriteLine($"✅ Output written to {output}");
    Console.WriteLine(stats);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Environment.Exit(1);
}

static Config LoadConfig()
{
    const string configPath = "config.json";
    try
    {
        return File.Exists(configPath)
            ? JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath)) ?? new Config()
            : new Config();
    }
    catch (JsonException ex)
    {
        throw new InvalidOperationException("Error parsing config file", ex);
    }
}

static string GetValidPath(string defaultPath)
{
    var path = MyAppsContext.GetUserInput($"Enter the path to index (default: {defaultPath}): ");
    var finalPath = string.IsNullOrWhiteSpace(path) ? defaultPath : path;
    return Directory.Exists(finalPath)
        ? finalPath
        : throw new DirectoryNotFoundException($"Invalid directory path: {finalPath}");
}

static string GetValidOutputPath(string defaultOutput)
{
    var output = MyAppsContext.GetUserInput($"Enter output file (default: {defaultOutput}): ");
    return string.IsNullOrWhiteSpace(output) ? defaultOutput : output;
}

static (string structure, string contents) GetProjectInfo(string path)
{
    try
    {
        return (MyAppsContext.GetProjectStructure(path), MyAppsContext.GetFileContents(path));
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Error processing project at {path}", ex);
    }
}

static string BuildContent(string structure, string contents) =>
    new StringBuilder()
        .AppendLine("Project Structure:")
        .AppendLine(structure)
        .AppendLine("\nFile Contents:")
        .AppendLine(contents)
        .ToString();

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
    catch (Exception ex)
    {
        throw new InvalidOperationException("Error calculating stats", ex);
    }
}

static void WriteOutput(string output, string content, string format)
{
    try
    {
        var outputPath = Directory.Exists(output) ? Path.Combine(output, "context.txt") : output;
        var formattedContent = format.ToLower() switch
        {
            "json" => JsonSerializer.Serialize(new { content, timestamp = DateTime.Now }),
            _ => content
        };
        File.WriteAllText(outputPath, formattedContent);
    }
    catch (Exception ex)
    {
        throw new IOException($"Error writing output to {output}", ex);
    }
}

record Config
{
    public string DefaultInputPath { get; init; } = ".";
    public string DefaultOutputFileName { get; init; } = "context.txt";
    public string OutputFormat { get; init; } = "text";
}