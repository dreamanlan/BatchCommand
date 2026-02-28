using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Utils;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    /// <summary>
    /// find_file(query[, max_count])
    /// Cross-platform file search. Returns formatted text for LLM.
    /// Windows: Everything SDK (indexed, fast).
    /// macOS: mdfind (Spotlight, indexed).
    /// Linux: locate (if available) or find (fallback).
    /// Default max_count=10, max=100.
    /// </summary>
    sealed class FindFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("find_file requires (query[, max_count])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string query = operands[0].AsString;
            uint maxCount = operands.Count > 1 ? operands[1].GetUInt() : c_DefMaxResults;
            if (maxCount > c_MaxResults) maxCount = c_MaxResults;
            if (string.IsNullOrEmpty(query))
                return BoxedValue.FromString("error: empty query");
            try {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return SearchWindows(query, maxCount);
                return SearchOther(query, maxCount);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"find_file error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
            }
        }

        private static BoxedValue SearchWindows(string query, uint maxCount)
        {
            if (!EverythingSDK.EverythingExists())
                return BoxedValue.FromString("[error] Everything service is not running. Use everything_ensure() first.");
            lock (EverythingSDK.Lock) {
                EverythingSDK.Everything_SetSearchW(query);
                EverythingSDK.Everything_SetOffset(0);
                EverythingSDK.Everything_SetMax(maxCount);
                EverythingSDK.Everything_SetRequestFlags(
                    (uint)(EverythingSDK.EVERYTHING_REQUEST_FILE_NAME |
                           EverythingSDK.EVERYTHING_REQUEST_PATH |
                           EverythingSDK.EVERYTHING_REQUEST_SIZE |
                           EverythingSDK.EVERYTHING_REQUEST_DATE_MODIFIED));
                if (!EverythingSDK.Everything_QueryW(true))
                    return BoxedValue.FromString($"[error] Everything query failed, error code: {EverythingSDK.Everything_GetLastError()}");
                uint tot = EverythingSDK.Everything_GetTotResults();
                uint num = EverythingSDK.Everything_GetNumResults();
                var sb = new StringBuilder();
                sb.AppendLine($"Found {num} results (total: {tot}) for: {query}");
                sb.AppendLine(new string('-', 40));
                for (uint i = 0; i < num; ++i) {
                    var pathBuf = new StringBuilder(EverythingSDK.PATH_CAPACITY);
                    EverythingSDK.Everything_GetResultFullPathNameW(i, pathBuf, (uint)EverythingSDK.PATH_CAPACITY);
                    EverythingSDK.Everything_GetResultSize(i, out long size);
                    EverythingSDK.Everything_GetResultDateModified(i, out long time);
                    var dt = new DateTime(1601, 1, 1, 8, 0, 0, DateTimeKind.Utc) + new TimeSpan(time);
                    string sizeStr = FormatSize(size);
                    sb.AppendLine($"[{i + 1}] {pathBuf}  ({sizeStr}, {dt:yyyy-MM-dd HH:mm:ss})");
                }
                return BoxedValue.FromString(sb.ToString().TrimEnd());
            }
        }

        private static BoxedValue SearchOther(string query, uint maxCount)
        {
            var paths = PlatformFileSearch.Search(query, maxCount);
            if (paths.Count == 0)
                return BoxedValue.FromString($"No results found for: {query}");
            var sb = new StringBuilder();
            string platform = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Spotlight" : "locate/find";
            sb.AppendLine($"Found {paths.Count} results (via {platform}) for: {query}");
            sb.AppendLine(new string('-', 40));
            for (int i = 0; i < paths.Count; ++i) {
                var (size, modified) = PlatformFileSearch.GetFileInfo(paths[i]);
                string sizeStr = size >= 0 ? FormatSize(size) : "?";
                string timeStr = modified != DateTime.MinValue ? modified.ToString("yyyy-MM-dd HH:mm:ss") : "?";
                sb.AppendLine($"[{i + 1}] {paths[i]}  ({sizeStr}, {timeStr})");
            }
            return BoxedValue.FromString(sb.ToString().TrimEnd());
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 0) return "?";
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }

        private const uint c_DefMaxResults = 10;
        private const uint c_MaxResults = 100;
    }

    /// <summary>
    /// find_file_raw(query[, max_count])
    /// Cross-platform file search. Returns List of [fullPath, size, modifiedTime].
    /// Use 'to_string' to convert to a string.
    /// </summary>
    sealed class FindFileRawExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("find_file_raw requires (query[, max_count])");
                return BoxedValue.FromObject(s_EmptyList);
            }
            string query = operands[0].AsString;
            uint maxCount = operands.Count > 1 ? operands[1].GetUInt() : c_DefMaxResults;
            if (maxCount > c_MaxResults) maxCount = c_MaxResults;
            if (string.IsNullOrEmpty(query))
                return BoxedValue.FromObject(s_EmptyList);
            try {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return SearchWindowsRaw(query, maxCount);
                return SearchOtherRaw(query, maxCount);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"find_file_raw error: {ex.Message}");
                return BoxedValue.FromObject(s_EmptyList);
            }
        }

        private static BoxedValue SearchWindowsRaw(string query, uint maxCount)
        {
            if (!EverythingSDK.EverythingExists())
                return BoxedValue.FromObject(s_EmptyList);
            lock (EverythingSDK.Lock) {
                EverythingSDK.Everything_SetSearchW(query);
                EverythingSDK.Everything_SetOffset(0);
                EverythingSDK.Everything_SetMax(maxCount);
                EverythingSDK.Everything_SetRequestFlags(
                    (uint)(EverythingSDK.EVERYTHING_REQUEST_FILE_NAME |
                           EverythingSDK.EVERYTHING_REQUEST_PATH |
                           EverythingSDK.EVERYTHING_REQUEST_SIZE |
                           EverythingSDK.EVERYTHING_REQUEST_DATE_MODIFIED));
                if (!EverythingSDK.Everything_QueryW(true))
                    return BoxedValue.FromObject(s_EmptyList);
                uint num = EverythingSDK.Everything_GetNumResults();
                var list = new List<object[]>();
                for (uint i = 0; i < num; ++i) {
                    var sb = new StringBuilder(EverythingSDK.PATH_CAPACITY);
                    EverythingSDK.Everything_GetResultFullPathNameW(i, sb, (uint)EverythingSDK.PATH_CAPACITY);
                    EverythingSDK.Everything_GetResultSize(i, out long size);
                    EverythingSDK.Everything_GetResultDateModified(i, out long time);
                    var dt = new DateTime(1601, 1, 1, 8, 0, 0, DateTimeKind.Utc) + new TimeSpan(time);
                    list.Add(new object[] { sb.ToString(), size, dt.ToString("yyyy-MM-dd HH:mm:ss") });
                }
                return BoxedValue.FromObject(list);
            }
        }

        private static BoxedValue SearchOtherRaw(string query, uint maxCount)
        {
            var paths = PlatformFileSearch.Search(query, maxCount);
            var list = new List<object[]>();
            foreach (var path in paths) {
                var (size, modified) = PlatformFileSearch.GetFileInfo(path);
                string timeStr = modified != DateTime.MinValue ? modified.ToString("yyyy-MM-dd HH:mm:ss") : "";
                list.Add(new object[] { path, size, timeStr });
            }
            return BoxedValue.FromObject(list);
        }

        private static readonly List<object[]> s_EmptyList = new List<object[]>();
        private const uint c_DefMaxResults = 10;
        private const uint c_MaxResults = 100;
    }

    /// <summary>
    /// Registers cross-platform file search DSL APIs.
    /// </summary>
    public static class FindFileApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("find_file",
                "find_file(query[, max_count]) - cross-platform file search (Windows: Everything, macOS: Spotlight, Linux: locate/find). Returns formatted text",
                new ExpressionFactoryHelper<FindFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_file_raw",
                "find_file_raw(query[, max_count]) - cross-platform file search, returns List of [path, size, time]. Use 'to_string' to convert",
                new ExpressionFactoryHelper<FindFileRawExp>());
        }
    }
}
