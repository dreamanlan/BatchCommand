using System.IO;
using System.Text;

namespace CefDotnetApp.AgentCore.Utils
{
    /// <summary>
    /// UTF-8 BOM detection and BOM-preserving encoding helpers.
    /// Used by file modification APIs to keep the original file's BOM state on overwrite.
    /// </summary>
    public static class BomHelper
    {
        /// <summary>
        /// Returns true if the file at fullPath starts with UTF-8 BOM (EF BB BF).
        /// Returns false if file is shorter than 3 bytes, missing BOM, or not accessible.
        /// </summary>
        public static bool HasUtf8Bom(string fullPath)
        {
            try {
                using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (fs.Length < 3)
                    return false;
                byte[] head = new byte[3];
                int read = fs.Read(head, 0, 3);
                return read == 3 && head[0] == 0xEF && head[1] == 0xBB && head[2] == 0xBF;
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Returns an Encoding that matches the existing file's encoding and BOM state.
        /// Detection order: BOM-based (UTF-32/UTF-16/UTF-8) first, then UTF-8 validity check.
        /// If the file does not exist, or the encoding cannot be determined, falls back to
        /// UTF-8 with BOM controlled by defaultBom.
        /// </summary>
        public static Encoding GetEncodingPreservingBom(string fullPath, bool defaultBom = true)
        {
            if (!File.Exists(fullPath)) {
                return new UTF8Encoding(defaultBom);
            }

            try {
                using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (fs.Length == 0) {
                    return new UTF8Encoding(defaultBom);
                }

                // Read up to 4 bytes for BOM detection.
                int bomLen = (int)Math.Min(4, fs.Length);
                byte[] head = new byte[bomLen];
                int read = fs.Read(head, 0, bomLen);

                // Detect by BOM (order matters: UTF-32 LE shares FF FE with UTF-16 LE).
                if (read >= 4 && head[0] == 0x00 && head[1] == 0x00 && head[2] == 0xFE && head[3] == 0xFF) {
                    return new UTF32Encoding(bigEndian: true, byteOrderMark: true);
                }
                if (read >= 4 && head[0] == 0xFF && head[1] == 0xFE && head[2] == 0x00 && head[3] == 0x00) {
                    return new UTF32Encoding(bigEndian: false, byteOrderMark: true);
                }
                if (read >= 3 && head[0] == 0xEF && head[1] == 0xBB && head[2] == 0xBF) {
                    return new UTF8Encoding(true);
                }
                if (read >= 2 && head[0] == 0xFE && head[1] == 0xFF) {
                    return new UnicodeEncoding(bigEndian: true, byteOrderMark: true);
                }
                if (read >= 2 && head[0] == 0xFF && head[1] == 0xFE) {
                    return new UnicodeEncoding(bigEndian: false, byteOrderMark: true);
                }

                // No BOM: verify UTF-8 validity over the first 64KB.
                long sampleLen = Math.Min(fs.Length, 64 * 1024);
                byte[] sample = new byte[sampleLen];
                Array.Copy(head, 0, sample, 0, read);
                int extra = fs.Read(sample, read, (int)sampleLen - read);
                int total = read + extra;

                var validator = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                try {
                    validator.GetString(sample, 0, total);
                    return new UTF8Encoding(false);
                }
                catch {
                    return new UTF8Encoding(defaultBom);
                }
            }
            catch {
                return new UTF8Encoding(defaultBom);
            }
        }
    }
}
