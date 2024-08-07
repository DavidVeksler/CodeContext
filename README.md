# CodeContext

CodeContext is an efficient C# application designed to provide code context to Language Learning Models (LLMs). It scans project directories, generates a structured representation of the project, and extracts relevant file contents while intelligently filtering out unnecessary files and directories.

## Features

- Generates a hierarchical project structure
- Extracts contents of relevant files
- Intelligent file and directory filtering
- Git-aware: respects .gitignore rules
- Handles binary files and large files
- Excludes generated code
- Customizable ignored extensions, directories, and files

## Getting Started

### Prerequisites

- .NET 6.0 or later

#### macOS

1. Install .NET SDK if you haven't already:
brew install --cask dotnet-sdk

2. Clone the repository:
git clone https://github.com/yourusername/CodeContext.git

3. Navigate to the project directory:
cd CodeContext

4. Build the project:
dotnet build

### Installation

1. Clone the repository:
git clone https://github.com/DavidVeksler/CodeContext/CodeContext.git

2. Navigate to the project directory:
cd CodeContext

3. Build the project:
dotnet build

1. 
### Usage

Run the application with:
dotnet run [path_to_index] [output_file]

- `path_to_index`: The directory to analyze (optional, will prompt if not provided)
- `output_file`: The file to write the output (optional, defaults to `context.txt` in the parent directory of the indexed path)

If no arguments are provided, the application will prompt for input.

## Configuration

Customize ignored files, directories, and extensions by modifying the `FileChecker` class:

- `IgnoredExtensions`: File extensions to ignore
- `IgnoredDirectories`: Directories to ignore
- `IgnoredFiles`: Specific files to ignore
- `MaxFileSizeBytes`: Maximum file size to process (default: 100KB)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE) file for details.