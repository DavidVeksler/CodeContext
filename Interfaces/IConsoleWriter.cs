namespace CodeContext.Interfaces;

/// <summary>
/// Provides an abstraction for console output operations.
/// </summary>
public interface IConsoleWriter
{
    /// <summary>
    /// Writes a line of text to the console.
    /// </summary>
    /// <param name="message">The message to write.</param>
    void WriteLine(string message);

    /// <summary>
    /// Writes text to the console without a line break.
    /// </summary>
    /// <param name="message">The message to write.</param>
    void Write(string message);

    /// <summary>
    /// Reads a line of input from the console.
    /// </summary>
    /// <returns>The input string.</returns>
    string? ReadLine();
}
