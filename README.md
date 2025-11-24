# CodeContext

CodeContext is a cross-platform CLI tool and MCP (Model Context Protocol) server for Mac, Windows, and Linux that provides intelligent code context to Language Learning Models (LLMs) and agentic coding tools.

It scans project directories, generates a structured representation of the project, and extracts relevant file contents while intelligently filtering out unnecessary files and directories. Now with **token budget optimization** and **relevance-based file selection** for agentic coding workflows!

![screenshot](https://github.com/DavidVeksler/CodeContext/blob/master/screenshot.png?raw=true)

Update: A more comprehensive tool is [code2prompt](https://github.com/mufeedvh/code2prompt).
I found that CodeContext is more user-friendly, faster, and automatically includes only user code (based on both extension and file contents), but you may have better luck with alternatives.

## Features

### Core Features
- **Hierarchical Project Structure**: Generates a clear tree view of your project
- **Smart Content Extraction**: Extracts contents of relevant source files
- **Intelligent Filtering**: Automatically filters out binaries, dependencies, build outputs, and more
- **Git-Aware**: Respects .gitignore rules
- **Binary File Detection**: Automatically detects and skips binary files
- **Generated Code Detection**: Excludes auto-generated code
- **Highly Customizable**: Configure ignored extensions, directories, and file size limits
- **Multiple Output Formats**: Supports plain text and JSON output
- **Well-Architected**: Clean separation of concerns with interfaces for testability

### 🆕 Agentic Coding Features
- **MCP Server Mode**: Native integration with Claude Code, Cline, and other MCP-compatible agents
- **Token Budget Optimization**: Intelligently selects most relevant files within token constraints
- **Relevance Scoring**: Automatically ranks files based on task description
- **Multiple Selection Strategies**: GreedyByScore, ValueOptimized, and Balanced algorithms
- **Dynamic Context Generation**: Task-specific context rather than dumping entire codebase

## Architecture

The project follows SOLID principles with a modular architecture:

- **`Configuration/`**: Filter configuration settings and app configuration
- **`Interfaces/`**: Abstraction interfaces (IFileChecker, IConsoleWriter)
- **`Services/`**: Core business logic
  - File filtering and scanning (FileFilterService, ProjectScanner)
  - Token counting and budget optimization (TokenCounter, TokenBudgetOptimizer)
  - Relevance scoring (FileRelevanceScorer)
  - Git integration (GitIgnoreParser, GitHelper)
  - Output formatting and content building
- **`Mcp/`**: Model Context Protocol server tools
  - MCP tool implementations for agentic coding integration
- **`Utils/`**: Utility functions (FileUtilities, Guard)

This design makes the codebase maintainable, testable, and extensible while supporting both CLI and MCP server modes.

## Getting Started

### Prerequisites

- .NET 9.0 or later

#### macOS

Install .NET SDK if you haven't already:
```bash
brew install --cask dotnet-sdk
```

#### Windows

Download and install the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

#### Linux

Follow the [official .NET installation guide](https://learn.microsoft.com/en-us/dotnet/core/install/linux) for your distribution.

### Installation

1. Clone the repository:
```bash
git clone https://github.com/DavidVeksler/CodeContext.git
cd CodeContext
```

2. Build the project:
```bash
dotnet build
```

3. (Optional) Publish for your platform:
```bash
# Self-contained executable
dotnet publish -c Release -r win-x64 --self-contained  # Windows
dotnet publish -c Release -r osx-x64 --self-contained  # macOS
dotnet publish -c Release -r linux-x64 --self-contained  # Linux
```

### Usage

Run the application with:
```bash
dotnet run [path_to_index] [output_file]
```

Arguments:
- `path_to_index`: The directory to analyze (optional, will prompt if not provided)
- `output_file`: The file to write the output (optional, defaults to `{foldername}_context.txt` in the indexed directory)

If no arguments are provided, the application will prompt for input interactively.

### Example

```bash
# Interactive mode
dotnet run

# With arguments
dotnet run ./MyProject ./output/context.txt

# Using published executable
./CodeContext ./MyProject ./output/context.txt
```

## 🚀 MCP Server Mode (New!)

CodeContext now supports **Model Context Protocol (MCP)**, enabling native integration with agentic coding tools like Claude Code, Cline, and other MCP-compatible clients.

### What is MCP Server Mode?

MCP server mode provides:
- **Intelligent context generation** based on task descriptions
- **Token budget optimization** - automatically selects most relevant files within token limits
- **Dynamic queries** - agents can request exactly the context they need
- **Multiple strategies** - optimize for relevance, value, or balanced coverage

### Setup with Claude Code

1. Build CodeContext:
```bash
dotnet build
```

2. Add to your Claude Code MCP configuration (`~/.config/claude/mcp.json` or project `.claude/mcp.json`):
```json
{
  "mcpServers": {
    "codecontext": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/absolute/path/to/CodeContext/CodeContext.csproj",
        "--",
        "--mcp"
      ]
    }
  }
}
```

3. Restart Claude Code - CodeContext will now be available as an MCP tool!

### Available MCP Tools

#### 1. GetCodeContext
Get optimized code context for a specific task within a token budget.

**Parameters:**
- `projectPath` (required): Path to project directory
- `taskDescription` (required): Description of task (e.g., "fix authentication bug", "add payment feature")
- `tokenBudget` (optional, default: 50000): Maximum tokens to use
- `includeStructure` (optional, default: true): Include project structure
- `strategy` (optional, default: "ValueOptimized"): Selection strategy
  - `GreedyByScore`: Pick highest-scoring files first
  - `ValueOptimized`: Maximize relevance per token (best bang for buck)
  - `Balanced`: Mix of high-value and comprehensive coverage

**Example:**
```
Agent: Use GetCodeContext with projectPath="/path/to/project",
       taskDescription="implement user authentication",
       tokenBudget=30000,
       strategy="ValueOptimized"
```

#### 2. GetProjectStructure
Get hierarchical directory tree of the project.

**Parameters:**
- `projectPath` (required): Path to project directory

#### 3. ListProjectFiles
List all files with token counts and optional relevance filtering.

**Parameters:**
- `projectPath` (required): Path to project directory
- `query` (optional): Query to filter/rank files by relevance

#### 4. GetFileContent
Get content of specific files.

**Parameters:**
- `projectPath` (required): Path to project directory
- `filePaths` (required): Comma-separated list of relative file paths

### How Token Budget Optimization Works

1. **Relevance Scoring**: Files are scored based on:
   - File name matching task keywords (30% weight)
   - File path matching keywords (20% weight)
   - Content matching keywords (40% weight)
   - File importance indicators (10% weight)

2. **Selection Strategies**:
   - **ValueOptimized** (recommended): Maximizes relevance/token ratio - gives you the best context per token
   - **GreedyByScore**: Picks highest-scoring files until budget is exhausted
   - **Balanced**: Combines both approaches for comprehensive yet efficient coverage

3. **Result**: You get the most relevant files for your task within your token budget!

### Example Workflow

```bash
# Agent asks: "Help me fix the login authentication bug"

# CodeContext MCP server:
# 1. Scans project files
# 2. Scores files for relevance to "login authentication bug"
# 3. Selects optimal files within token budget (e.g., 50K tokens)
# 4. Returns context with:
#    - auth/login.ts (score: 0.95, 2K tokens)
#    - auth/session.ts (score: 0.87, 1.5K tokens)
#    - middleware/auth.ts (score: 0.79, 1K tokens)
#    - tests/auth.test.ts (score: 0.72, 3K tokens)
#    - ... (up to budget)
```

### Benefits for Agentic Coding

- **Token Efficiency**: Don't waste tokens on irrelevant files
- **Task-Specific Context**: Get exactly what you need for each task
- **Automatic Relevance Ranking**: No manual file selection needed
- **Scalable**: Works with large codebases by intelligently sampling
- **Multiple Strategies**: Choose optimization approach per task

## Configuration

Create a `config.json` file in the application directory to customize settings:

```json
{
  "DefaultInputPath": ".",
  "DefaultOutputFileName": "context.txt",
  "OutputFormat": "text",
  "IncludeStructure": true,
  "IncludeContents": true
}
```

### Advanced Configuration

Customize filtering behavior by modifying the `FilterConfiguration` class:

- **`IgnoredExtensions`**: File extensions to ignore (e.g., `.exe`, `.dll`, `.png`)
- **`IgnoredDirectories`**: Directories to ignore (e.g., `node_modules`, `bin`, `obj`)
- **`IgnoredFiles`**: Specific files to ignore (e.g., `.gitignore`, `package-lock.json`)
- **`MaxFileSizeBytes`**: Maximum file size to process (default: 100KB)
- **`BinaryThreshold`**: Threshold for binary file detection (default: 0.3)

## Output Formats

### Text Format (default)
Plain text output with file paths, separators, and content.

### JSON Format
Structured JSON with content and timestamp:
```json
{
  "content": "...",
  "timestamp": "2025-11-21T10:30:00"
}
```

## Error Handling

The application provides clear error messages with appropriate exit codes:
- `1`: Directory not found
- `2`: I/O error
- `3`: Access denied
- `4`: Unexpected error

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development

The codebase uses:
- **C# 12** with modern language features
- **Nullable reference types** for better null safety
- **XML documentation comments** on all public APIs
- **Dependency injection** patterns for testability

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Acknowledgments

Built with ❤️ for the developer community to make working with LLMs more efficient.
