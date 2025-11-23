using System.Collections.Immutable;
using CodeContext.Configuration;

namespace CodeContext.Services;

/// <summary>
/// Builds project context content using functional composition.
/// Separates content generation from assembly.
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
    /// Uses functional composition to build content sections.
    /// </summary>
    /// <param name="projectPath">The directory path to process.</param>
    /// <param name="config">The configuration specifying what to include.</param>
    /// <returns>The complete output content.</returns>
    public string Build(string projectPath, AppConfig config) =>
        string.Join("\n", BuildContentSections(projectPath, config));

    /// <summary>
    /// Pure function that generates content sections based on configuration.
    /// Uses declarative approach with LINQ and immutable collections.
    /// </summary>
    private IEnumerable<string> BuildContentSections(string projectPath, AppConfig config)
    {
        var sections = ImmutableArray.CreateBuilder<ContentSection>();

        if (config.IncludeStructure)
        {
            sections.Add(new ContentSection(
                "Project Structure:",
                () => _scanner.GetProjectStructure(projectPath)));
        }

        if (config.IncludeContents)
        {
            sections.Add(new ContentSection(
                "\nFile Contents:",
                () => _scanner.GetFileContents(projectPath)));
        }

        return sections.ToImmutable().SelectMany(section => section.Render());
    }

    /// <summary>
    /// Immutable record representing a content section with lazy evaluation.
    /// </summary>
    private sealed record ContentSection(string Header, Func<string> ContentGenerator)
    {
        /// <summary>
        /// Renders the section by evaluating the content generator.
        /// </summary>
        public IEnumerable<string> Render()
        {
            yield return Header;
            yield return ContentGenerator();
        }
    }
}
