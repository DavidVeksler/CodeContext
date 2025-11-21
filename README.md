# CodeContext

CodeContext is a cross-platform CLI tool for Mac, Windows, and Linux that provides code context to Language Learning Models (LLMs).
It scans project directories, generates a structured representation of the project, and extracts relevant file contents while intelligently filtering out unnecessary files and directories.

![screenshot](https://github.com/DavidVeksler/CodeContext/blob/master/screenshot.png?raw=true)

Update: A more comprehensive tool is [code2prompt](https://github.com/mufeedvh/code2prompt).
I found that CodeContext is more user-friendly, faster, and automatically includes only user code (based on both extension and file contents), but you may have better luck with alternatives.

## Features

- **Hierarchical Project Structure**: Generates a clear tree view of your project
- **Smart Content Extraction**: Extracts contents of relevant source files
- **Intelligent Filtering**: Automatically filters out binaries, dependencies, build outputs, and more
- **Git-Aware**: Respects .gitignore rules
- **Binary File Detection**: Automatically detects and skips binary files
- **Generated Code Detection**: Excludes auto-generated code
- **Highly Customizable**: Configure ignored extensions, directories, and file size limits
- **Multiple Output Formats**: Supports plain text and JSON output
- **Well-Architected**: Clean separation of concerns with interfaces for testability

## Architecture

The project follows SOLID principles with a modular architecture:

- **`Configuration/`**: Filter configuration settings
- **`Interfaces/`**: Abstraction interfaces (IFileChecker, IConsoleWriter)
- **`Services/`**: Core business logic (FileFilterService, ProjectScanner, GitIgnoreParser)
- **`Utils/`**: Utility functions (FileUtilities)

This design makes the codebase maintainable, testable, and extensible.

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
