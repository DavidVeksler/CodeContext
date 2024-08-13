using System.Diagnostics;
using System.Text;
using CodeContext;

Console.OutputEncoding = Encoding.UTF8;


var path = args.Length > 0 ? args[0] : MyAppsContext.GetUserInput("Enter the path to index: ");
var defaultOutput = Path.Combine(Path.GetDirectoryName(path), "context.txt");
var output = args.Length > 1 ? args[1] : MyAppsContext.GetUserInput($"Enter output file (default: {defaultOutput}): ");
output = string.IsNullOrWhiteSpace(output) ? defaultOutput : output;

var sw = Stopwatch.StartNew();

var structure = MyAppsContext.GetProjectStructure(path);
var contents = MyAppsContext.GetFileContents(path);

var contentWithLineNumbers = new StringBuilder();
using (var reader = new StringReader(contents))
{
    for (var i = 1; reader.ReadLine() is { } line; i++)
        contentWithLineNumbers.AppendFormat("{0,4}: {1}\n", i, line);
}

var content = new StringBuilder()
    .AppendLine("Project Structure:")
    .AppendLine(structure)
    .AppendLine("\nFile Contents:")
    .Append(contentWithLineNumbers)
    .ToString();

var fileCount = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
var lineCount = content.Count(c => c == '\n');
var timeTaken = sw.Elapsed;

var stats = $"📊 Stats:\n" +
            $"📁 Files processed: {fileCount}\n" +
            $"📝 Total lines: {lineCount}\n" +
            $"⏱️ Time taken: {timeTaken.TotalSeconds:F2}s\n" +
            $"💾 Output size: {content.Length} characters";

var outputPath = Directory.Exists(output) ? Path.Combine(output, "context.txt") : output;
File.WriteAllText(outputPath, content);

Console.WriteLine($"✅ Output written to {outputPath}");
Console.WriteLine(stats);