using CodeContext.Interfaces;

namespace CodeContext.Services;

/// <summary>
/// Standard console implementation of IConsoleWriter.
/// </summary>
public class ConsoleWriter : IConsoleWriter
{
    /// <inheritdoc/>
    public void WriteLine(string message) => Console.WriteLine(message);

    /// <inheritdoc/>
    public void Write(string message) => Console.Write(message);

    /// <inheritdoc/>
    public string? ReadLine() => Console.ReadLine();
}
