using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using CodeContext.Mcp;
using CodeContext.Services;

namespace CodeContext;

/// <summary>
/// Main program entry point.
/// Supports both CLI mode and MCP server mode.
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Check if running in MCP server mode
        if (args.Contains("--mcp") || args.Contains("--server"))
        {
            await RunMcpServer(args);
        }
        else
        {
            // Run in CLI mode
            ProgramCli.RunCli(args);
        }
    }

    /// <summary>
    /// Runs the MCP server mode.
    /// </summary>
    private static async Task RunMcpServer(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Log to stderr (keeps stdout clean for JSON-RPC)
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        // Register our services
        builder.Services.AddSingleton<Interfaces.IConsoleWriter, ConsoleWriter>();

        // Add MCP server with stdio transport and our tools
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        var host = builder.Build();

        // Log startup message to stderr
        Console.Error.WriteLine("CodeContext MCP Server starting...");
        Console.Error.WriteLine("Available tools:");
        Console.Error.WriteLine("  - GetCodeContext: Get optimized code context for a task");
        Console.Error.WriteLine("  - GetProjectStructure: Get project directory structure");
        Console.Error.WriteLine("  - ListProjectFiles: List all project files with metadata");
        Console.Error.WriteLine("  - GetFileContent: Get content of specific files");
        Console.Error.WriteLine();

        await host.RunAsync();
    }
}
