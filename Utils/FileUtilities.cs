namespace CodeContext.Utils;

/// <summary>
/// Functional utility methods for file operations.
/// Separates I/O operations from pure byte analysis logic.
/// </summary>
public static class FileUtilities
{
    private static readonly byte[] Utf8Bom = { 0xEF, 0xBB, 0xBF };

    /// <summary>
    /// Determines if a file is binary based on its content (I/O operation).
    /// Returns false on any error to fail-safe to text processing.
    /// </summary>
    /// <param name="filePath">Path to the file to check.</param>
    /// <param name="chunkSize">Number of bytes to read for analysis.</param>
    /// <param name="binaryThreshold">Threshold ratio (0.0-1.0) of non-printable bytes to consider a file binary.</param>
    /// <returns>True if the file appears to be binary; otherwise, false.</returns>
    public static bool IsBinaryFile(string filePath, int chunkSize = 4096, double binaryThreshold = 0.3)
    {
        Guard.NotNullOrEmpty(filePath, nameof(filePath));

        return File.Exists(filePath) && CheckFileBinaryContent(filePath, chunkSize, binaryThreshold);
    }

    /// <summary>
    /// I/O operation: reads file and checks if content is binary.
    /// </summary>
    private static bool CheckFileBinaryContent(string filePath, int chunkSize, double threshold)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return IsStreamBinary(stream, chunkSize, threshold);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Pure function: analyzes stream content to determine if binary.
    /// Checks for UTF-8 BOM first, then analyzes byte patterns.
    /// </summary>
    private static bool IsStreamBinary(FileStream stream, int chunkSize, double threshold) =>
        stream.Length > 0 && !HasUtf8Bom(stream) && HasBinaryContent(stream, chunkSize, threshold);

    /// <summary>
    /// Pure function: checks if stream starts with UTF-8 BOM.
    /// Resets stream position after checking.
    /// </summary>
    private static bool HasUtf8Bom(FileStream stream)
    {
        if (stream.Length < Utf8Bom.Length)
        {
            return false;
        }

        var bom = new byte[Utf8Bom.Length];
        stream.Read(bom, 0, Utf8Bom.Length);
        stream.Position = 0;

        return bom.SequenceEqual(Utf8Bom);
    }

    /// <summary>
    /// Pure function: checks if stream content exceeds binary threshold.
    /// </summary>
    private static bool HasBinaryContent(FileStream stream, int chunkSize, double threshold)
    {
        var buffer = new byte[chunkSize];
        var bytesRead = stream.Read(buffer, 0, chunkSize);

        return bytesRead > 0 && CalculateBinaryRatio(buffer, bytesRead) > threshold;
    }

    /// <summary>
    /// Pure function: calculates ratio of binary bytes in buffer.
    /// </summary>
    private static double CalculateBinaryRatio(byte[] buffer, int length) =>
        (double)buffer.Take(length).Count(IsBinaryByte) / length;

    /// <summary>
    /// Pure predicate: determines if a byte is non-printable (binary).
    /// Bytes outside printable ASCII range (32-127) except common control chars (7-14).
    /// </summary>
    private static bool IsBinaryByte(byte b) =>
        b is (< 7 or > 14) and (< 32 or > 127);
}
