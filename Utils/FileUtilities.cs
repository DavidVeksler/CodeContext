namespace CodeContext.Utils;

/// <summary>
/// Utility methods for file operations.
/// </summary>
public static class FileUtilities
{
    /// <summary>
    /// Determines if a file is binary based on its content.
    /// </summary>
    /// <param name="filePath">Path to the file to check.</param>
    /// <param name="chunkSize">Number of bytes to read for analysis.</param>
    /// <param name="binaryThreshold">Threshold ratio (0.0-1.0) of non-printable bytes to consider a file binary.</param>
    /// <returns>True if the file appears to be binary; otherwise, false.</returns>
    public static bool IsBinaryFile(string filePath, int chunkSize = 4096, double binaryThreshold = 0.3)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            return false;
        }

        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            if (stream.Length == 0)
            {
                return false;
            }

            // Check for UTF-8 BOM
            if (HasUtf8Bom(stream))
            {
                return false;
            }

            return CheckBinaryContent(stream, chunkSize, binaryThreshold);
        }
        catch (Exception)
        {
            // If we can't read the file (permissions, etc.), assume it's not binary
            return false;
        }
    }

    private static bool HasUtf8Bom(FileStream stream)
    {
        if (stream.Length < 3)
        {
            return false;
        }

        var bom = new byte[3];
        stream.Read(bom, 0, 3);
        stream.Position = 0;
        return bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF;
    }

    private static bool CheckBinaryContent(FileStream stream, int chunkSize, double threshold)
    {
        var buffer = new byte[chunkSize];
        var bytesRead = stream.Read(buffer, 0, chunkSize);

        if (bytesRead == 0)
        {
            return false;
        }

        var nonPrintableCount = buffer.Take(bytesRead).Count(IsBinaryByte);
        return (double)nonPrintableCount / bytesRead > threshold;
    }

    private static bool IsBinaryByte(byte b)
    {
        return b is (< 7 or > 14) and (< 32 or > 127);
    }
}
