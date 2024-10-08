# CodeContext

CodeContext is an app for Mac & Windows to provide code context to Language Learning Models (LLMs). 
It scans project directories, generates a structured representation of the project, and extracts relevant file contents while intelligently filtering out unnecessary files and directories.

![screenshot](https://github.com/DavidVeksler/CodeContext/blob/master/screenshot.png?raw=true)

Update:  a more comprehensive tool is [code2prompt](https://github.com/mufeedvh/code2prompt).  
I found that CodeContext is more user friendly,  faster, and automatically includes only user code (based on both extension and file contents), but you may have better luck.

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
