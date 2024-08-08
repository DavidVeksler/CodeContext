namespace CodeContext;

public static class FileUtils
{
    public static bool IsBinaryFile(string filePath)
    {
        const int chunkSize = 4096;
        const double binaryThreshold = 0.3;
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        if (stream.Length == 0)
            return false;

        // Check for UTF-8 BOM
        if (HasUtf8Bom(stream))
            return false;

        return CheckBinaryContent(stream, chunkSize, binaryThreshold);
    }

    private static bool HasUtf8Bom(FileStream stream)
    {
        var bom = new byte[3];
        stream.Read(bom, 0, 3);
        stream.Position = 0;
        return bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF;
    }

    private static bool CheckBinaryContent(FileStream stream, int chunkSize, double threshold)
    {
        var buffer = new byte[chunkSize];
        var bytesRead = stream.Read(buffer, 0, chunkSize);
        var nonPrintableCount = buffer.Take(bytesRead).Count(IsBinaryByte);
        return (double)nonPrintableCount / bytesRead > threshold;
    }

    private static bool IsBinaryByte(byte b)
    {
        return b is (< 7 or > 14) and (< 32 or > 127);
    }
}