using System;
using System.IO;

namespace CefDotnetApp.AgentCore.Utils
{
    public static class PathHelper
    {
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return Path.GetFullPath(path).Replace('\\', '/');
        }

        public static string CombinePaths(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            return Path.Combine(paths);
        }

        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(toPath))
                return toPath;

            Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        public static string AppendDirectorySeparatorChar(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }

        public static bool IsAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return Path.IsPathRooted(path);
        }

        public static string EnsureAbsolutePath(string path, string basePath)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (IsAbsolutePath(path))
                return path;

            return Path.Combine(basePath, path);
        }
    }
}
