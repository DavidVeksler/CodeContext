using DotNet.Globbing;
using System.IO;
using System.Linq;
using System.Text;
using CodeContext;

var path = args.Length > 0 ? args[0] : MyAppsContext.GetUserInput("Enter the path to index: ");
var defaultOutput = Path.Combine(Path.GetDirectoryName(path), "context.txt");
var output = args.Length > 1 ? args[1] : MyAppsContext.GetUserInput($"Enter output file (default: {defaultOutput}): ");
output = string.IsNullOrWhiteSpace(output) ? defaultOutput : output;

var structure = MyAppsContext.GetProjectStructure(path);
var contents = MyAppsContext.GetFileContents(path);

var contentWithLineNumbers = contents.Split('\n')
    .Select((line, index) => $"{index + 1,4}: {line}")
    .Aggregate((a, b) => $"{a}\n{b}");

var content = new StringBuilder()
    .AppendLine("Project Structure:")
    .AppendLine(structure)
    .AppendLine("\nFile Contents:")
    .AppendLine(contentWithLineNumbers)
    .ToString();

var stats = $"Output size: {content.Length} characters, {content.Count(c => c == '\n')} lines";

var outputPath = Directory.Exists(output) ? Path.Combine(output, "context.txt") : output;
File.WriteAllText(outputPath, content);
Console.WriteLine($"Output written to {outputPath}");
Console.WriteLine(stats);