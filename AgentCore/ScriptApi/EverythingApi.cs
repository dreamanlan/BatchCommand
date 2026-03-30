using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using Microsoft.Win32;
using ScriptableFramework;
using System.Diagnostics;
using System.Threading;
using CefDotnetApp.AgentCore.Utils;

#pragma warning disable CA1416
namespace CefDotnetApp.AgentCore.ScriptApi
{
    /// <summary>
    /// everything_exists()
    /// Returns true if Everything service is running.
    /// </summary>
    sealed class EverythingExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.From(EverythingSDK.EverythingExists());
        }
    }

    /// <summary>
    /// everything_reset()
    /// Resets Everything search state.
    /// </summary>
    sealed class EverythingResetExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (EverythingSDK.EverythingExists()) {
                lock (EverythingSDK.Lock) {
                    EverythingSDK.Everything_Reset();
                }
            }
            return BoxedValue.NullObject;
        }
    }

    /// <summary>
    /// everything_set_default()
    /// Sets default search parameters: no path match, no case, no whole word, no regex, sort by path asc.
    /// </summary>
    sealed class EverythingSetDefaultExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (EverythingSDK.EverythingExists()) {
                lock (EverythingSDK.Lock) {
                    EverythingSDK.Everything_SetMatchPath(false);
                    EverythingSDK.Everything_SetMatchCase(false);
                    EverythingSDK.Everything_SetMatchWholeWord(false);
                    EverythingSDK.Everything_SetRegex(false);
                    EverythingSDK.Everything_SetSort((uint)EverythingSDK.EVERYTHING_SORT_PATH_ASCENDING);
                }
            }
            return BoxedValue.NullObject;
        }
    }

    /// <summary>
    /// everything_match_path([bool])
    /// Get or set match path option.
    /// </summary>
    sealed class EverythingMatchPathExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                bool val = operands[0].GetBool();
                if (EverythingSDK.EverythingExists()) {
                    lock (EverythingSDK.Lock) {
                        EverythingSDK.Everything_SetMatchPath(val);
                    }
                    return true;
                }
                return false;
            }
            else if (EverythingSDK.EverythingExists()) {
                lock (EverythingSDK.Lock) {
                    return EverythingSDK.Everything_GetMatchPath();
                }
            }
            return false;
        }
    }

    /// <summary>
    /// everything_match_case([bool])
    /// Get or set case sensitive option.
    /// </summary>
    sealed class EverythingMatchCaseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                bool val = operands[0].GetBool();
                if (EverythingSDK.EverythingExists()) {
                    lock (EverythingSDK.Lock) {
                        EverythingSDK.Everything_SetMatchCase(val);
                    }
                    return true;
                }
                return false;
            }
            else if (EverythingSDK.EverythingExists()) {
                lock (EverythingSDK.Lock) {
                    return EverythingSDK.Everything_GetMatchCase();
                }
            }
            return false;
        }
    }

    /// <summary>
    /// everything_match_whole_word([bool])
    /// Get or set whole word match option.
    /// </summary>
    sealed class EverythingMatchWholeWordExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                bool val = operands[0].GetBool();
                if (EverythingSDK.EverythingExists()) {
                    lock (EverythingSDK.Lock) {
                        EverythingSDK.Everything_SetMatchWholeWord(val);
                    }
                    return true;
                }
                return false;
            }
            else if (EverythingSDK.EverythingExists()) {
                lock (EverythingSDK.Lock) {
                    return EverythingSDK.Everything_GetMatchWholeWord();
                }
            }
            return false;
        }
    }

    /// <summary>
    /// everything_regex([bool])
    /// Get or set regex mode.
    /// </summary>
    sealed class EverythingRegexExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                bool val = operands[0].GetBool();
                if (EverythingSDK.EverythingExists()) {
                    lock (EverythingSDK.Lock) {
                        EverythingSDK.Everything_SetRegex(val);
                    }
                    return true;
                }
                return false;
            }
            else if (EverythingSDK.EverythingExists()) {
                lock (EverythingSDK.Lock) {
                    return EverythingSDK.Everything_GetRegex();
                }
            }
            return false;
        }
    }

    /// <summary>
    /// everything_sort([type, asc])
    /// Get or set sort mode.
    /// type: "path" / "size" / "time" / "name" / numeric sort constant.
    /// asc: true for ascending, false for descending (default true).
    /// </summary>
    sealed class EverythingSortExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count >= 1) {
                string type = operands[0].AsString;
                bool asc = true;
                if (operands.Count >= 2)
                    asc = operands[1].GetBool();
                if (EverythingSDK.EverythingExists()) {
                    lock (EverythingSDK.Lock) {
                        if (type == "name") {
                            EverythingSDK.Everything_SetSort(asc ? (uint)EverythingSDK.EVERYTHING_SORT_NAME_ASCENDING : (uint)EverythingSDK.EVERYTHING_SORT_NAME_DESCENDING);
                        }
                        else if (type == "path") {
                            EverythingSDK.Everything_SetSort(asc ? (uint)EverythingSDK.EVERYTHING_SORT_PATH_ASCENDING : (uint)EverythingSDK.EVERYTHING_SORT_PATH_DESCENDING);
                        }
                        else if (type == "size") {
                            EverythingSDK.Everything_SetSort(asc ? (uint)EverythingSDK.EVERYTHING_SORT_SIZE_ASCENDING : (uint)EverythingSDK.EVERYTHING_SORT_SIZE_DESCENDING);
                        }
                        else if (type == "time") {
                            EverythingSDK.Everything_SetSort(asc ? (uint)EverythingSDK.EVERYTHING_SORT_DATE_MODIFIED_ASCENDING : (uint)EverythingSDK.EVERYTHING_SORT_DATE_MODIFIED_DESCENDING);
                        }
                        else {
                            uint sort = 0;
                            if (null != type)
                                uint.TryParse(type, out sort);
                            else
                                sort = operands[0].GetUInt();
                            EverythingSDK.Everything_SetSort(sort);
                            return BoxedValue.FromString(sort.ToString());
                        }
                    }
                    return BoxedValue.FromString(type + "," + (asc ? "asc" : "desc"));
                }
                return BoxedValue.FromString(string.Empty);
            }
            else if (EverythingSDK.EverythingExists()) {
                lock (EverythingSDK.Lock) {
                    uint sort = EverythingSDK.Everything_GetSort();
                    switch (sort) {
                        case (uint)EverythingSDK.EVERYTHING_SORT_NAME_ASCENDING: return "name,asc";
                        case (uint)EverythingSDK.EVERYTHING_SORT_NAME_DESCENDING: return "name,desc";
                        case (uint)EverythingSDK.EVERYTHING_SORT_PATH_ASCENDING: return "path,asc";
                        case (uint)EverythingSDK.EVERYTHING_SORT_PATH_DESCENDING: return "path,desc";
                        case (uint)EverythingSDK.EVERYTHING_SORT_SIZE_ASCENDING: return "size,asc";
                        case (uint)EverythingSDK.EVERYTHING_SORT_SIZE_DESCENDING: return "size,desc";
                        case (uint)EverythingSDK.EVERYTHING_SORT_DATE_MODIFIED_ASCENDING: return "time,asc";
                        case (uint)EverythingSDK.EVERYTHING_SORT_DATE_MODIFIED_DESCENDING: return "time,desc";
                        default: return sort.ToString();
                    }
                }
            }
            return BoxedValue.FromString(string.Empty);
        }
    }

    /// <summary>
    /// everything_search(query[, max_count])
    /// Search files via Everything, returns formatted text for LLM.
    /// Default max_count=10, max=100.
    /// </summary>
    sealed class EverythingSearchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: everything_search(query[, max_count])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string query = operands[0].AsString;
            uint maxCount = operands.Count > 1 ? operands[1].GetUInt() : c_DefMaxResults;
            if (maxCount > c_MaxResults) maxCount = c_MaxResults;
            if (string.IsNullOrEmpty(query))
                return BoxedValue.FromString("error: empty query");
            if (!EverythingSDK.EverythingExists())
                return BoxedValue.FromString("[error] Everything service is not running.");
            try {
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
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"everything_search error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
            }
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
    /// everything_search_raw(query[, max_count])
    /// Search files via Everything, returns List of [fullPath, size, modifiedTime].
    /// Use 'to_string' to convert to a string.
    /// </summary>
    sealed class EverythingSearchRawExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: everything_search_raw(query[, max_count])");
                return BoxedValue.FromObject(s_EmptyList);
            }
            string query = operands[0].AsString;
            uint maxCount = operands.Count > 1 ? operands[1].GetUInt() : c_DefMaxResults;
            if (maxCount > c_MaxResults) maxCount = c_MaxResults;
            if (string.IsNullOrEmpty(query) || !EverythingSDK.EverythingExists())
                return BoxedValue.FromObject(s_EmptyList);
            try {
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
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"everything_search_raw error: {ex.Message}");
                return BoxedValue.FromObject(s_EmptyList);
            }
        }

        private static readonly List<object[]> s_EmptyList = new List<object[]>();
        private const uint c_DefMaxResults = 10;
        private const uint c_MaxResults = 100;
    }

    /// <summary>
    /// everything_ensure()
    /// Ensure Everything service is running.
    /// Checks process -> finds path (registry then SDK fallback) -> starts if needed -> returns path.
    /// Returns: full path of Everything.exe, or "[error] not found" if not found.
    /// </summary>
    sealed class EverythingEnsureExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                // If cached path exists and process is running, return immediately
                if (!string.IsNullOrEmpty(s_EverythingFullPath) && IsEverythingRunning())
                    return BoxedValue.FromString(s_EverythingFullPath);

                // Find path if not cached
                if (string.IsNullOrEmpty(s_EverythingFullPath))
                    FindEverythingPath();

                if (string.IsNullOrEmpty(s_EverythingFullPath))
                    return BoxedValue.FromString("[error] not found");

                // Process is running, just return path
                if (IsEverythingRunning())
                    return BoxedValue.FromString(s_EverythingFullPath);

                // Start Everything with -startup flag (silent, minimized to tray)
                Process.Start(new ProcessStartInfo {
                    FileName = s_EverythingFullPath,
                    Arguments = "-startup",
                    UseShellExecute = false
                });

                // Wait for Everything service to become ready
                for (int i = 0; i < c_MaxWaitRetries; ++i) {
                    Thread.Sleep(c_WaitIntervalMs);
                    if (EverythingSDK.EverythingExists())
                        return BoxedValue.FromString(s_EverythingFullPath);
                }
                // Started but service not ready yet
                return BoxedValue.FromString(s_EverythingFullPath);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"everything_ensure error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
            }
        }

        private static bool IsEverythingRunning()
        {
            var procs = Process.GetProcessesByName("Everything");
            bool running = procs.Length > 0;
            foreach (var p in procs)
                p.Dispose();
            return running;
        }

        private static void FindEverythingPath()
        {
            // Try registry first
            var dir = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\voidtools\Everything",
                "InstallLocation", string.Empty) as string;
            if (!string.IsNullOrEmpty(dir)) {
                var fullPath = Path.Combine(dir, c_Key);
                if (File.Exists(fullPath)) {
                    s_EverythingFullPath = fullPath;
                    return;
                }
            }
            // Fallback: use Everything SDK to search Everything.exe
            if (EverythingSDK.EverythingExists()) {
                lock (EverythingSDK.Lock) {
                    EverythingSDK.Everything_SetMatchPath(false);
                    EverythingSDK.Everything_SetMatchCase(false);
                    EverythingSDK.Everything_SetMatchWholeWord(true);
                    EverythingSDK.Everything_SetRegex(false);
                    EverythingSDK.Everything_SetSort((uint)EverythingSDK.EVERYTHING_SORT_PATH_ASCENDING);
                    EverythingSDK.Everything_SetSearchW(c_Key);
                    EverythingSDK.Everything_SetOffset(0);
                    EverythingSDK.Everything_SetMax(100);
                    EverythingSDK.Everything_SetRequestFlags(
                        (uint)(EverythingSDK.EVERYTHING_REQUEST_FILE_NAME | EverythingSDK.EVERYTHING_REQUEST_PATH));
                    if (EverythingSDK.Everything_QueryW(true)) {
                        uint num = EverythingSDK.Everything_GetNumResults();
                        for (uint i = 0; i < num; ++i) {
                            string? fileName = Marshal.PtrToStringUni(EverythingSDK.Everything_GetResultFileNameW(i));
                            var sb = new StringBuilder(EverythingSDK.PATH_CAPACITY);
                            EverythingSDK.Everything_GetResultFullPathNameW(i, sb, (uint)EverythingSDK.PATH_CAPACITY);
                            if (fileName != null && string.Compare(fileName, c_Key, true) == 0) {
                                var fullPath = sb.ToString();
                                if (File.Exists(fullPath)) {
                                    s_EverythingFullPath = fullPath;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string s_EverythingFullPath = string.Empty;
        private const string c_Key = "Everything.exe";
        private const int c_MaxWaitRetries = 10;
        private const int c_WaitIntervalMs = 500;
    }

    /// <summary>
    /// Registers all Everything DSL APIs.
    /// </summary>
    public static class EverythingApi
    {
        public static void RegisterApis()
        {
            // Everything SDK is Windows-only; skip registration on other platforms
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Windows))
                return;

            AgentFrameworkService.Instance.DslEngine!.Register("everything_exists",
                "everything_exists() - check if Everything service is running",
                new ExpressionFactoryHelper<EverythingExistsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("everything_ensure",
                "everything_ensure() - ensure Everything is running (find path, start if needed), returns path",
                new ExpressionFactoryHelper<EverythingEnsureExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("everything_reset",
                "everything_reset() - reset Everything search state",
                new ExpressionFactoryHelper<EverythingResetExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("everything_set_default",
                "everything_set_default() - set default search parameters",
                new ExpressionFactoryHelper<EverythingSetDefaultExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("everything_match_path",
                "everything_match_path([bool]) - get/set path matching",
                new ExpressionFactoryHelper<EverythingMatchPathExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("everything_match_case",
                "everything_match_case([bool]) - get/set case sensitive matching",
                new ExpressionFactoryHelper<EverythingMatchCaseExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("everything_match_whole_word",
                "everything_match_whole_word([bool]) - get/set whole word matching",
                new ExpressionFactoryHelper<EverythingMatchWholeWordExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("everything_regex",
                "everything_regex([bool]) - get/set regex mode",
                new ExpressionFactoryHelper<EverythingRegexExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("everything_sort",
                "everything_sort([type, asc]) - get/set sort mode. type: name/path/size/time or numeric constant",
                new ExpressionFactoryHelper<EverythingSortExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("everything_search",
                "everything_search(query[, max_count]) - search files, returns formatted text (default 100 results, max 1000)",
                new ExpressionFactoryHelper<EverythingSearchExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("everything_search_raw",
                "everything_search_raw(query[, max_count]) - search files, returns List of [path, size, time]. use 'to_string' to convert",
                new ExpressionFactoryHelper<EverythingSearchRawExp>());
        }
    }
}
#pragma warning restore CA1416
