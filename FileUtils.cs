using System;
using System.IO;
using System.Linq;
using System.Text;

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
            byte[] bom = new byte[3];
            stream.Read(bom, 0, 3);
            stream.Position = 0;
            return bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF;
        }

        private static bool CheckBinaryContent(FileStream stream, int chunkSize, double threshold)
        {
            var buffer = new byte[chunkSize];
            int bytesRead = stream.Read(buffer, 0, chunkSize);
            int nonPrintableCount = buffer.Take(bytesRead).Count(IsBinaryByte);
            return (double)nonPrintableCount / bytesRead > threshold;
        }

        private static bool IsBinaryByte(byte b) =>
            (b < 7 || b > 14) && (b < 32 || b > 127);
    }
}