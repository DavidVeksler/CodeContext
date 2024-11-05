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
    var content = BuildContent(path, config);
    var stats = CalculateStats(path, content, sw.Elapsed);

    WriteOutput(output, content, config.OutputFormat);
    Console.WriteLine($"\n✅ Output written to {output}");
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
    return Directory.Exists(finalPath)
        ? finalPath
        : throw new DirectoryNotFoundException($"Invalid directory path: {finalPath}");
}

static string GetValidOutputPath(string defaultOutput)
{
    var output = MyAppsContext.GetUserInput($"Enter output file (default: {defaultOutput}): ");
    return string.IsNullOrWhiteSpace(output) ? defaultOutput : output;
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

static void WriteOutput(string output, string content, string format)
{
    Console.WriteLine("\n💾 Writing output...");
    try
    {
        var outputPath = Directory.Exists(output) ? Path.Combine(output, "context.txt") : output;
        var formattedContent = format.ToLower() == "json"
            ? JsonSerializer.Serialize(new { content, timestamp = DateTime.Now })
            : content;
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
    public bool IncludeStructure { get; init; } = true;
    public bool IncludeContents { get; init; } = true;
}