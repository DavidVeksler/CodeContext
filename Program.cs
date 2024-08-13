using System.Diagnostics;
using System.Text;
using CodeContext;

Console.OutputEncoding = Encoding.UTF8;
var path = args.FirstOrDefault() ?? MyAppsContext.GetUserInput("Enter the path to index: ");
var defaultOutput = Path.Combine(Path.GetDirectoryName(path), "context.txt");
var output = args.ElementAtOrDefault(1) ?? MyAppsContext.GetUserInput($"Enter output file (default: {defaultOutput}): ");
output = string.IsNullOrWhiteSpace(output) ? defaultOutput : output;

var sw = Stopwatch.StartNew();
var structure = MyAppsContext.GetProjectStructure(path);
var contents = MyAppsContext.GetFileContents(path);

var content = new StringBuilder()
    .AppendLine("Project Structure:")
    .AppendLine(structure)
    .AppendLine("\nFile Contents:")
    .AppendLine(contents)
    .ToString();

var fileCount = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
var lineCount = content.Count(c => c == '\n');
var timeTaken = sw.Elapsed;

var stats = $"""
             📊 Stats:
             📁 Files processed: {fileCount}
             📝 Total lines: {lineCount}
             ⏱️ Time taken: {timeTaken.TotalSeconds:F2}s
             💾 Output size: {content.Length} characters
             """;

var outputPath = Directory.Exists(output) ? Path.Combine(output, "context.txt") : output;
File.WriteAllText(outputPath, content);
Console.WriteLine($"✅ Output written to {outputPath}");
Console.WriteLine(stats);