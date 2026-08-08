using System.IO;
using System.Text;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Provides file read methods that open with FileShare.ReadWrite,
    /// so reading does not fail when another process holds the file for writing.
    /// </summary>
    internal static class SafeFileReader
    {
        public static string ReadAllText(string path, Encoding? encoding = null)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return sr.ReadToEnd();
        }

        public static string[] ReadAllLines(string path, Encoding? encoding = null)
        {
            var lines = new List<string>();
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            while (!sr.EndOfStream) {
                lines.Add(sr.ReadLine()!);
            }
            return lines.ToArray();
        }

        public static byte[] ReadAllBytes(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var buffer = new byte[fs.Length];
            int totalRead = 0;
            while (totalRead < buffer.Length) {
                int read = fs.Read(buffer, totalRead, buffer.Length - totalRead);
                if (read == 0) break;
                totalRead += read;
            }
            if (totalRead < buffer.Length) {
                var trimmed = new byte[totalRead];
                Buffer.BlockCopy(buffer, 0, trimmed, 0, totalRead);
                return trimmed;
            }
            return buffer;
        }

        public static FileStream OpenRead(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}
