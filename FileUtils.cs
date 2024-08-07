
namespace CodeContext
{
    public static class FileUtils
    {
        private static readonly byte[] BinarySignatures = {
            0x00, 0xFF, 0xFE, 0xEF, 0xBB, 0xBF, 0x1F, 0x8B, 0x08, 0x89, 0x50, 0x4E, 0x47
        };

        public static bool IsBinaryFile(string filePath)
        {
            const int chunkSize = 4096;
            const double binaryThreshold = 0.2;
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return stream.Length != 0 && (HasBinarySignature(stream) ||
                                          stream.ReadByte() == 0 ||
                                          CheckBinaryContent(stream, chunkSize, binaryThreshold));
        }

        private static bool HasBinarySignature(FileStream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            stream.Position = 0;
            return BinarySignatures.Any(sig => buffer.Contains(sig));
        }

        private static bool CheckBinaryContent(FileStream stream, int chunkSize, double threshold)
        {
            var buffer = new byte[chunkSize];
            int bytesRead = stream.Read(buffer, 0, chunkSize);
            int nonPrintableCount = buffer.Take(bytesRead).Count(IsBinaryByte);
            return (double)nonPrintableCount / bytesRead > threshold;
        }

        private static bool IsBinaryByte(byte b) =>
            b <= 8 || (b >= 14 && b <= 31) || b >= 127;
    }
}
