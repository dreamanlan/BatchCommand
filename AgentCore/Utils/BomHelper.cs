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
        /// Returns a UTF8Encoding whose BOM-emit flag matches the existing file at fullPath.
        /// If the file does not exist, defaultBom decides whether the encoding emits BOM.
        /// </summary>
        public static UTF8Encoding GetUtf8EncodingPreservingBom(string fullPath, bool defaultBom)
        {
            if (File.Exists(fullPath)) {
                return new UTF8Encoding(HasUtf8Bom(fullPath));
            }
            return new UTF8Encoding(defaultBom);
        }
    }
}
