using System.Diagnostics;
using System.Text;
using CodeContext.Configuration;
using CodeContext.Services;
using CodeContext.Utils;

Console.OutputEncoding = Encoding.UTF8;

try
{
    // Initialize dependencies using functional composition
    var console = new ConsoleWriter();
    var configLoader = new ConfigLoader(console);
    var pathResolver = new PathResolver(console);
    var filterConfig = new FilterConfiguration();
    var statsCalculator = new StatsCalculator();

    // Load configuration
    var config = configLoader.Load();

    // Get and validate input path
    var defaultInputPath = args.FirstOrDefault() ?? config.DefaultInputPath;
    console.Write($"Enter the path to index (default: {defaultInputPath}): ");
    var projectPath = pathResolver.GetInputPath(defaultInputPath);

    // Load GitIgnore patterns for the project (I/O boundary clearly defined)
    var gitIgnoreParser = GitHelper.FindRepositoryRoot(projectPath) switch
    {
        null => GitIgnoreParser.Empty,
        var gitRoot => GitIgnoreParser.FromFile(Path.Combine(gitRoot, ".gitignore"))
    };

    // Initialize file checker with immutable GitIgnore parser
    var fileChecker = new FileFilterService(filterConfig, gitIgnoreParser);
    var scanner = new ProjectScanner(fileChecker, console);
    var contentBuilder = new ContentBuilder(scanner);
    var outputFormatter = new OutputFormatter(console);

    // Determine output path
    var folderName = PathResolver.GetFolderName(projectPath);
    var defaultFileName = $"{folderName}_{config.DefaultOutputFileName}";
    var defaultOutputPath = Path.Combine(projectPath, defaultFileName);
    var outputArg = args.ElementAtOrDefault(1);
    console.Write($"Enter output file/directory (default: {defaultOutputPath}): ");
    var outputPath = pathResolver.GetOutputPath(outputArg, defaultOutputPath);

    // Build content
    var stopwatch = Stopwatch.StartNew();
    var content = contentBuilder.Build(projectPath, config);
    var stats = statsCalculator.Calculate(projectPath, content, stopwatch.Elapsed);

    // Write output
    var actualOutputPath = outputFormatter.WriteToFile(outputPath, content, config.OutputFormat, defaultFileName);
    console.WriteLine($"\n✅ Output written to {actualOutputPath}");
    console.WriteLine(stats);
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
