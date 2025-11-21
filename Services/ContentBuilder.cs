using System.Text;
using CodeContext.Configuration;

namespace CodeContext.Services;

/// <summary>
/// Builds project context content based on configuration.
/// </summary>
public class ContentBuilder
{
    private readonly ProjectScanner _scanner;

    public ContentBuilder(ProjectScanner scanner)
    {
        _scanner = scanner;
    }

    /// <summary>
    /// Builds the complete content output including structure and file contents.
    /// </summary>
    /// <param name="projectPath">The directory path to process.</param>
    /// <param name="config">The configuration specifying what to include.</param>
    /// <returns>The complete output content.</returns>
    public string Build(string projectPath, AppConfig config)
    {
        var content = new StringBuilder();

        if (config.IncludeStructure)
        {
            content.AppendLine("Project Structure:")
                   .AppendLine(_scanner.GetProjectStructure(projectPath));
        }

        if (config.IncludeContents)
        {
            content.AppendLine("\nFile Contents:")
                   .AppendLine(_scanner.GetFileContents(projectPath));
        }

        return content.ToString();
    }
}
