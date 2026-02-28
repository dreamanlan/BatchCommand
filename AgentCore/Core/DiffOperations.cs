using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CefDotnetApp.AgentCore.Utils;

using AgentPlugin.Abstractions;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Diff operations for applying patches to files
    /// </summary>
    public class DiffOperations
    {
        private readonly string _basePath;
        private readonly string _appDir;
        private readonly bool _isMac;

        public DiffOperations(string basePath, string appDir, bool isMac)
        {
            _basePath = basePath;
            _appDir = appDir;
            _isMac = isMac;
        }

        /// <summary>
        /// Apply unified diff patch to target file
        /// </summary>
        public DiffResult ApplyDiff(string targetPath, string diffPath, bool exactMatch = false)
        {
            try {
                string fullTargetPath = PathHelper.EnsureAbsolutePath(targetPath, _basePath);
                string fullDiffPath = PathHelper.EnsureAbsolutePath(diffPath, _basePath);

                if (!File.Exists(fullDiffPath))
                    return new DiffResult { Success = false, Error = "Diff file not found", Library = "Basic" };

                string diffContent = File.ReadAllText(fullDiffPath, Encoding.UTF8);
                string[] targetLines = File.Exists(fullTargetPath)
                    ? File.ReadAllLines(fullTargetPath, Encoding.UTF8)
                    : new string[0];

                var hunks = ParseUnifiedDiff(diffContent);
                if (hunks.Count == 0)
                    return new DiffResult { Success = false, Error = "No valid hunks found in diff (only supports unified diff)", Library = "Basic" };

                // Apply hunks from bottom to top to avoid line number shifting
                var sortedHunks = hunks.OrderByDescending(h => h.OldStartLine).ToList();
                List<string> resultLines = new List<string>(targetLines);
                List<DiffHunkResult> hunkResults = new List<DiffHunkResult>();

                foreach (var hunk in sortedHunks) {
                    var hunkResult = ApplyHunk(resultLines, hunk, exactMatch);
                    hunkResults.Add(hunkResult);
                    if (!hunkResult.Success) {
                        return new DiffResult {
                            Success = false,
                            Error = $"Failed to apply hunk at line {hunk.OldStartLine + 1}: {hunkResult.Error}",
                            HunkResults = hunkResults,
                            Library = "Basic"
                        };
                    }
                }

                // Report line number corrections
                ReportLineNumberCorrections(hunkResults);

                // Write the result
                File.WriteAllLines(fullTargetPath, resultLines, Encoding.UTF8);

                return new DiffResult {
                    Success = true,
                    HunkResults = hunkResults,
                    LinesAdded = hunkResults.Sum(h => h.LinesAdded),
                    LinesRemoved = hunkResults.Sum(h => h.LinesRemoved),
                    Library = "Basic"
                };
            }
            catch (Exception ex) {
                return new DiffResult { Success = false, Error = ex.Message, Library = "Basic" };
            }
        }
        /// <summary>
        /// Apply unified diff content directly to target file
        /// </summary>
        public DiffResult ApplyDiff(string targetPath, string diffContent, bool isContent, bool exactMatch = false)
        {
            if (!isContent) {
                if (File.Exists(diffContent)) {
                    // If isContent is false, treat as file path (backward compatibility)
                    return ApplyDiff(targetPath, diffContent, exactMatch);
                }
                isContent = true;
            }

            try {
                string fullTargetPath = PathHelper.EnsureAbsolutePath(targetPath, _basePath);

                string[] targetLines = File.Exists(fullTargetPath)
                    ? File.ReadAllLines(fullTargetPath, Encoding.UTF8)
                    : new string[0];

                var hunks = ParseUnifiedDiff(diffContent);
                if (hunks.Count == 0)
                    return new DiffResult { Success = false, Error = "No valid hunks found in diff (only supports unified diff)", Library = "Basic" };

                // Apply hunks from bottom to top to avoid line number shifting
                var sortedHunks = hunks.OrderByDescending(h => h.OldStartLine).ToList();
                List<string> resultLines = new List<string>(targetLines);
                List<DiffHunkResult> hunkResults = new List<DiffHunkResult>();

                foreach (var hunk in sortedHunks) {
                    var hunkResult = ApplyHunk(resultLines, hunk, exactMatch);
                    hunkResults.Add(hunkResult);
                    if (!hunkResult.Success) {
                        return new DiffResult {
                            Success = false,
                            Error = $"Failed to apply hunk at line {hunk.OldStartLine + 1}: {hunkResult.Error}",
                            HunkResults = hunkResults,
                            Library = "Basic"
                        };
                    }
                }

                // Report line number corrections
                ReportLineNumberCorrections(hunkResults);

                // Write the result
                File.WriteAllLines(fullTargetPath, resultLines, Encoding.UTF8);

                return new DiffResult {
                    Success = true,
                    HunkResults = hunkResults,
                    LinesAdded = hunkResults.Sum(h => h.LinesAdded),
                    LinesRemoved = hunkResults.Sum(h => h.LinesRemoved),
                    Library = "Basic"
                };
            }
            catch (Exception ex) {
                return new DiffResult { Success = false, Error = ex.Message, Library = "Basic" };
            }
        }

        /// <summary>
        /// Report line number corrections to error reporter as informational messages
        /// </summary>
        private static void ReportLineNumberCorrections(List<DiffHunkResult> hunkResults)
        {
            foreach (var hr in hunkResults) {
                if (hr.Success && hr.CorrectedStartLine > 0 && hr.CorrectedStartLine != hr.OldStartLine) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(
                        $"[diff info] Hunk line number corrected: specified line {hr.OldStartLine} -> actual line {hr.CorrectedStartLine}");
                }
            }
        }

        /// <summary>
        /// Parse unified diff format and extract hunks
        /// </summary>
        private static List<DiffHunk> ParseUnifiedDiff(string diffContent)
        {
            var hunks = new List<DiffHunk>();
            var lines = diffContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i];

                // Match hunk header: @@ -old_start,old_count +new_start,new_count @@
                var match = Regex.Match(line, @"^@@\s+-?(\d+)(?:,(\d+))?\s+\+?(\d+)(?:,(\d+))?\s+@@");
                if (match.Success) {
                    var hunk = new DiffHunk {
                        OldStartLine = int.Parse(match.Groups[1].Value) - 1,  // Convert to 0-based
                        OldLineCount = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1,
                        NewStartLine = int.Parse(match.Groups[3].Value) - 1,  // Convert to 0-based
                        NewLineCount = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 1,
                        Lines = new List<DiffLine>()
                    };

                    // Collect hunk lines
                    i++;
                    while (i < lines.Length) {
                        string hunkLine = lines[i];
                        if (hunkLine.StartsWith("@@") || hunkLine.StartsWith("diff ") || hunkLine.StartsWith("index "))
                            break;

                        if (hunkLine.Length == 0) {
                            // Empty line - need to determine if it's end of hunk or empty context line
                            bool isEndOfHunk = false;
                            if (i + 1 < lines.Length) {
                                string nextLine = lines[i + 1].TrimStart();
                                if (nextLine.StartsWith("@@") ||      // Next hunk
                                    nextLine.StartsWith("diff ") ||   // Next file
                                    nextLine.StartsWith("index ") ||  // Git index line
                                    nextLine.StartsWith("---") ||     // File marker
                                    nextLine.StartsWith("+++"))       // File marker
                                {
                                    isEndOfHunk = true;
                                }
                            }
                            else {
                                // Last line of diff - end of hunk
                                isEndOfHunk = true;
                            }

                            if (isEndOfHunk) {
                                // Empty line marks end of hunk
                                break;
                            }
                            else {
                                // Treat as empty context line (for compatibility with non-standard diffs)
                                hunk.Lines.Add(new DiffLine {
                                    Prefix = ' ',
                                    Content = string.Empty
                                });
                                i++;
                                continue;
                            }
                        }

                        char prefix = hunkLine[0];
                        string content = hunkLine.Length > 1 ? hunkLine.Substring(1) : string.Empty;

                        hunk.Lines.Add(new DiffLine {
                            Prefix = prefix,
                            Content = content
                        });

                        i++;
                    }

                    hunks.Add(hunk);
                    i--; // Back up one line
                }
            }

            return hunks;
        }
        private static DiffHunkResult ApplyHunk(List<string> lines, DiffHunk hunk, bool exactMatch = false)
        {
            var result = new DiffHunkResult {
                OldStartLine = hunk.OldStartLine + 1,  // Convert to 1-based for display
                NewStartLine = hunk.NewStartLine + 1   // Convert to 1-based for display
            };

            try {
                int startIndex = hunk.OldStartLine;  // Already 0-based

                // Find match using context + removed lines
                int matchedIndex = FindContextMatch(lines, startIndex, hunk, exactMatch);
                if (matchedIndex < 0) {
                    result.Success = false;
                    result.Error = "Context mismatch";
                    return result;
                }

                // Record corrected line number if it differs from the specified one
                int corrected1Based = matchedIndex + 1;
                if (corrected1Based != result.OldStartLine) {
                    result.CorrectedStartLine = corrected1Based;
                }

                // Determine if indent adjustment is needed for non-exact match
                IndentAdjustParams indentParams = default;
                bool needIndentAdjust = false;
                if (!exactMatch) {
                    // Find first non-empty original line (context or removed) in hunk
                    var origLines = hunk.Lines.Where(l => l.Prefix == ' ' || l.Prefix == '-').ToList();
                    DiffLine? firstNonEmptyOrig = null;
                    int origOffset = 0;
                    for (int oi = 0; oi < origLines.Count; oi++) {
                        if (!string.IsNullOrWhiteSpace(origLines[oi].Content)) {
                            firstNonEmptyOrig = origLines[oi];
                            origOffset = oi;
                            break;
                        }
                    }
                    if (firstNonEmptyOrig != null && matchedIndex + origOffset < lines.Count) {
                        string fileLine = lines[matchedIndex + origOffset];
                        string diffOrigLine = firstNonEmptyOrig.Content;
                        // Only adjust if not an exact match on this line
                        if (fileLine != diffOrigLine) {
                            needIndentAdjust = true;
                            // Collect replacement lines (context + added) for detecting replacement indent style
                            var replLines = hunk.Lines.Where(l => l.Prefix == ' ' || l.Prefix == '+').Select(l => l.Content);
                            indentParams = CalcIndentAdjustment(lines, replLines, fileLine, diffOrigLine);
                        }
                    }
                }

                // Build new content by processing diff lines
                var newLines = new List<string>();
                int linesRemoved = 0;
                int linesAdded = 0;

                foreach (var diffLine in hunk.Lines) {
                    if (diffLine.Prefix == ' ' || diffLine.Prefix == '+') {
                        string lineContent = diffLine.Content;
                        if (needIndentAdjust) {
                            lineContent = ApplyIndentAdjustment(lineContent, indentParams);
                        }
                        newLines.Add(lineContent);
                        if (diffLine.Prefix == '+')
                            linesAdded++;
                    }
                    else if (diffLine.Prefix == '-') {
                        linesRemoved++;
                    }
                }

                // Calculate how many lines to remove (context + removed lines)
                int oldCount = hunk.Lines.Count(l => l.Prefix == ' ' || l.Prefix == '-');

                // Boundary check
                if (matchedIndex < 0 || matchedIndex + oldCount > lines.Count) {
                    result.Success = false;
                    result.Error = $"Hunk extends beyond file end: need {matchedIndex + oldCount} lines but file has {lines.Count}";
                    return result;
                }

                // Apply changes
                lines.RemoveRange(matchedIndex, oldCount);
                lines.InsertRange(matchedIndex, newLines);

                result.Success = true;
                result.LinesRemoved = linesRemoved;
                result.LinesAdded = linesAdded;
                return result;
            }
            catch (Exception ex) {
                result.Success = false;
                result.Error = ex.Message;
                return result;
            }
        }
        /// <summary>
        /// Find the best match for original lines (context + removed) in the file
        /// </summary>
        private static int FindContextMatch(List<string> lines, int expectedIndex, DiffHunk hunk, bool exactMatch = false)
        {
            // Build the list of original lines that should exist in the file
            var originalLines = hunk.Lines
                .Where(l => l.Prefix == ' ' || l.Prefix == '-')
                .Select(l => l.Content)
                .ToList();

            if (originalLines.Count == 0) {
                // For pure insertion (no context, no removed lines)
                // Insert at expected position, or at file end if beyond range
                if (expectedIndex < 0)
                    return 0;
                if (expectedIndex > lines.Count)
                    return lines.Count; // Insert at end
                return expectedIndex;
            }

            // Try match at expected position
            if (TryMatchAt(lines, expectedIndex, originalLines, exactMatch))
                return expectedIndex;

            // In exact match mode, only try the expected position with strict comparison
            if (exactMatch)
                return -1;

            // Fuzzy search with sliding window (nearby first, then expand)
            int nearRange = Math.Min(20, lines.Count / 2);
            for (int offset = 1; offset <= nearRange; offset++) {
                // Try below
                if (TryMatchAt(lines, expectedIndex + offset, originalLines, false))
                    return expectedIndex + offset;

                // Try above
                if (TryMatchAt(lines, expectedIndex - offset, originalLines, false))
                    return expectedIndex - offset;
            }

            // Full file search as last resort
            for (int i = 0; i < lines.Count; i++) {
                // Skip already searched range
                if (i >= Math.Max(0, expectedIndex - nearRange) && i <= Math.Min(lines.Count - 1, expectedIndex + nearRange))
                    continue;
                if (TryMatchAt(lines, i, originalLines, false))
                    return i;
            }

            return -1;
        }
        /// <summary>
        /// Try to match original lines (context + removed) at a specific position.
        /// First tries exact match, then falls back to normalized whitespace match.
        /// </summary>
        private static bool TryMatchAt(List<string> lines, int startIndex, List<string> originalLines, bool exactMatchOnly = false)
        {
            if (originalLines.Count == 0)
                return true; // No lines to match

            if (startIndex < 0 || startIndex >= lines.Count || startIndex + originalLines.Count > lines.Count)
                return false;

            // First try exact match
            bool isExact = true;
            for (int i = 0; i < originalLines.Count; i++) {
                if (lines[startIndex + i] != originalLines[i]) {
                    isExact = false;
                    break;
                }
            }
            if (isExact)
                return true;

            // In exact match mode, do not try fuzzy matching
            if (exactMatchOnly)
                return false;

            // Second try: trim only (remove leading/trailing whitespace)
            bool trimMatch = true;
            for (int i = 0; i < originalLines.Count; i++) {
                var fileLine = lines[startIndex + i].Trim();
                var diffLine = originalLines[i].Trim();
                if (fileLine != diffLine) {
                    trimMatch = false;
                    break;
                }
            }
            if (trimMatch)
                return true;

            // Third try: normalize all whitespace (replace consecutive whitespace with single space)
            for (int i = 0; i < originalLines.Count; i++) {
                var fileLine = NormalizeWhitespace(lines[startIndex + i]);
                var diffLine = NormalizeWhitespace(originalLines[i]);
                if (fileLine != diffLine)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get leading whitespace of a line
        /// </summary>
        private static string GetLeadingWhitespace(string line)
        {
            int i = 0;
            while (i < line.Length && char.IsWhiteSpace(line[i]))
                i++;
            return line.Substring(0, i);
        }

        /// <summary>
        /// Find the first non-empty line from a list of lines and return its index.
        /// Returns -1 if all lines are empty.
        /// </summary>
        private static int FindFirstNonEmptyLineIndex(IList<string> lines)
        {
            for (int i = 0; i < lines.Count; i++) {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Detect indent style from a set of lines.
        /// Returns (useTabs, spacesPerLevel).
        /// useTabs=true means tab indentation, spacesPerLevel=0 in that case.
        /// useTabs=false means space indentation, spacesPerLevel is the GCD of all non-zero indent lengths.
        /// If no indentation detected, returns (false, 0).
        /// </summary>
        /// <summary>
        /// Detect indent style from lines.
        /// Returns (useTabs, spacesPerLevel, altSpacesPerLevel):
        ///   useTabs=true  => primary is tabs, altSpacesPerLevel = GCD of space-indented lines (minority)
        ///   useTabs=false => primary is spaces, altSpacesPerLevel = -1 if tabs also present, 0 otherwise
        /// </summary>
        private static (bool useTabs, int spacesPerLevel, int altSpacesPerLevel) DetectIndentStyle(IEnumerable<string> lines)
        {
            int tabCount = 0;
            int spaceCount = 0;
            int spaceGcd = 0;
            foreach (string line in lines) {
                if (string.IsNullOrEmpty(line))
                    continue;
                string ws = GetLeadingWhitespace(line);
                if (ws.Length == 0)
                    continue;
                if (ws[0] == '\t') {
                    tabCount++;
                }
                else if (ws[0] == ' ') {
                    spaceCount++;
                    spaceGcd = Gcd(spaceGcd, ws.Length);
                }
            }
            if (tabCount > 0 && tabCount >= spaceCount) {
                // Primary: tabs; alt: space GCD if any space lines exist
                return (true, 0, spaceGcd);
            }
            if (spaceCount > 0 && spaceGcd > 0) {
                // Primary: spaces; alt: mark -1 if tabs also present
                return (false, spaceGcd, tabCount > 0 ? -1 : 0);
            }
            return (false, 0, 0);
        }

        /// <summary>
        /// Calculate the GCD of two non-negative integers.
        /// </summary>
        private static int Gcd(int a, int b)
        {
            while (b != 0) {
                int t = b;
                b = a % b;
                a = t;
            }
            return a;
        }

        /// <summary>
        /// Count indent level of a line based on the given indent style.
        /// For tabs: count leading tab chars.
        /// For spaces: leading space count / spacesPerLevel.
        /// Returns (level, remainder) where remainder is extra spaces that don't form a full level.
        /// </summary>
        private static (int level, int remainder) CountIndentLevel(string line, bool useTabs, int spacesPerLevel)
        {
            if (string.IsNullOrEmpty(line))
                return (0, 0);
            string ws = GetLeadingWhitespace(line);
            if (ws.Length == 0)
                return (0, 0);
            if (useTabs) {
                int tabs = 0;
                while (tabs < ws.Length && ws[tabs] == '\t')
                    tabs++;
                return (tabs, 0);
            }
            else {
                if (spacesPerLevel <= 0)
                    return (0, ws.Length);
                int spaces = 0;
                while (spaces < ws.Length && ws[spaces] == ' ')
                    spaces++;
                return (spaces / spacesPerLevel, spaces % spacesPerLevel);
            }
        }

        /// <summary>
        /// Build indent string for a given level using file's indent style.
        /// </summary>
        private static string BuildIndent(int level, int remainderSpaces, bool fileUseTabs, int fileSpacesPerLevel)
        {
            if (level < 0) level = 0;
            if (fileUseTabs)
                return new string('\t', level) + (remainderSpaces > 0 ? new string(' ', remainderSpaces) : "");
            int spl = fileSpacesPerLevel > 0 ? fileSpacesPerLevel : 4;
            return new string(' ', level * spl + remainderSpaces);
        }

        /// <summary>
        /// Indent adjustment parameters for style-aware indent conversion.
        /// </summary>
        private struct IndentAdjustParams
        {
            public bool FileUseTabs;
            public int FileSpacesPerLevel;
            public int FileAltSpacesPerLevel; // minority style info from file
            public bool FileHasAltTabs;       // true when file primary is spaces but also has tab lines
            public bool ReplUseTabs;
            public int ReplSpacesPerLevel;
            public int LevelOffset; // file first line level - search first line level
        }

        /// <summary>
        /// Calculate indent adjustment parameters for style-aware indent conversion.
        /// fileLines: all lines from the original file (for detecting file indent style).
        /// replLines: replacement lines (for detecting replacement indent style).
        /// fileLine: the first non-empty matched line in file.
        /// searchLine: the first non-empty search/diff line.
        /// </summary>
        private static IndentAdjustParams CalcIndentAdjustment(IEnumerable<string> fileLines, IEnumerable<string> replLines, string fileLine, string searchLine)
        {
            var (fileUseTabs, fileSpl, fileAltSpl) = DetectIndentStyle(fileLines);
            var (replUseTabs, replSpl, _) = DetectIndentStyle(replLines);
            // Determine if file has both styles
            bool fileHasAltTabs = !fileUseTabs && fileAltSpl == -1;
            int fileAltSpaces = fileUseTabs ? fileAltSpl : 0; // only meaningful when primary is tabs
            // Determine which style to use for parsing fileLine's indent
            bool fileLineUseTabs = fileUseTabs;
            int fileLineSpl = fileSpl;
            if (!string.IsNullOrEmpty(fileLine)) {
                string flWs = GetLeadingWhitespace(fileLine);
                if (flWs.Length > 0) {
                    if (flWs[0] == '\t' && !fileUseTabs && fileHasAltTabs) {
                        // File primary is spaces, but this line uses tabs (minority)
                        fileLineUseTabs = true;
                        fileLineSpl = 0;
                    }
                    else if (flWs[0] == ' ' && fileUseTabs && fileAltSpaces > 0) {
                        // File primary is tabs, but this line uses spaces (minority)
                        fileLineUseTabs = false;
                        fileLineSpl = fileAltSpaces;
                    }
                }
            }
            // Calculate level offset from first non-empty matched line
            var (fileLevel, _) = CountIndentLevel(fileLine, fileLineUseTabs, fileLineSpl);
            var (searchLevel, _) = CountIndentLevel(searchLine, replUseTabs, replSpl > 0 ? replSpl : fileSpl);
            return new IndentAdjustParams {
                FileUseTabs = fileUseTabs,
                FileSpacesPerLevel = fileSpl,
                FileAltSpacesPerLevel = fileAltSpaces,
                FileHasAltTabs = fileHasAltTabs,
                ReplUseTabs = replUseTabs,
                ReplSpacesPerLevel = replSpl,
                LevelOffset = fileLevel - searchLevel
            };
        }

        /// <summary>
        /// Apply indent adjustment to a single line using style-aware conversion.
        /// Calculates the line's indent level (based on replacement style), adds levelOffset,
        /// then rebuilds the indent using file's style.
        /// </summary>
        private static string ApplyIndentAdjustment(string line, IndentAdjustParams p)
        {
            if (string.IsNullOrEmpty(line))
                return line;
            // If both styles are unknown (no indentation detected), fall back to no-op
            if (p.FileSpacesPerLevel == 0 && !p.FileUseTabs && p.ReplSpacesPerLevel == 0 && !p.ReplUseTabs && p.LevelOffset == 0)
                return line;
            string ws = GetLeadingWhitespace(line);
            string content = line.Substring(ws.Length);
            if (content.Length == 0)
                return line;
            // Use replacement style to count this line's indent level
            int replSpl = p.ReplSpacesPerLevel > 0 ? p.ReplSpacesPerLevel : p.FileSpacesPerLevel;
            bool replTabs = p.ReplUseTabs;
            var (level, remainder) = CountIndentLevel(line, replTabs, replSpl);
            int targetLevel = level + p.LevelOffset;
            if (targetLevel < 0) targetLevel = 0;
            string newIndent = BuildIndent(targetLevel, remainder, p.FileUseTabs, p.FileSpacesPerLevel);
            return newIndent + content;
        }

        // Unicode word char: letter (\p{L}), digit (\p{N}), or underscore
        internal static bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        internal static string NormalizeWhitespace(string line)
        {
            if (string.IsNullOrEmpty(line))
                return string.Empty;

            string trimmed = line.Trim();
            if (trimmed.Length == 0)
                return string.Empty;

            // Replace consecutive whitespace smartly:
            // - If both sides are identifier chars [a-zA-Z0-9_], replace with single space
            // - Otherwise (operator, bracket, punctuation on either side), replace with empty string
            var sb = new StringBuilder(trimmed.Length);
            int i = 0;
            while (i < trimmed.Length) {
                if (char.IsWhiteSpace(trimmed[i])) {
                    // Skip all consecutive whitespace
                    int wsStart = i;
                    while (i < trimmed.Length && char.IsWhiteSpace(trimmed[i]))
                        i++;
                    // Check left and right characters
                    bool leftIsIdent = wsStart > 0 && IsIdentifierChar(trimmed[wsStart - 1]);
                    bool rightIsIdent = i < trimmed.Length && IsIdentifierChar(trimmed[i]);
                    if (leftIsIdent && rightIsIdent) {
                        sb.Append(' ');
                    }
                    // else: collapse to nothing
                }
                else {
                    sb.Append(trimmed[i]);
                    i++;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Replace multi-line text with normalized whitespace matching
        /// This method is designed to handle LLM-generated replacements that may fail due to invisible character mismatches
        /// </summary>
        /// <param name="targetContent">Target string content to search in</param>
        /// <param name="searchText">Multi-line text to search for (will be normalized for matching)</param>
        /// <param name="replacementText">Multi-line text to replace with</param>
        /// <param name="replaceAll">Whether to replace all matches (default: true)</param>
        /// <param name="normalizeWhitespace">Whether to normalize whitespace for matching (default: true)</param>
        /// <returns>ReplaceResult with success status, details, and the modified content</returns>
        public static ReplaceResult ReplaceFullLinesText(string targetContent, string searchText, string replacementText, bool replaceAll = true, bool normalizeWhitespace = true)
        {
            try {
                if (string.IsNullOrEmpty(targetContent)) {
                    return new ReplaceResult {
                        Success = false,
                        Error = "Target content is null or empty"
                    };
                }

                // Split target content into lines
                string[] targetLines = targetContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Split search text into lines
                string[] searchLines = searchText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Remove empty lines at start and end of search text
                int searchStart = 0;
                int searchEnd = searchLines.Length - 1;
                while (searchStart < searchLines.Length && string.IsNullOrWhiteSpace(searchLines[searchStart]))
                    searchStart++;
                while (searchEnd >= 0 && string.IsNullOrWhiteSpace(searchLines[searchEnd]))
                    searchEnd--;

                if (searchStart > searchEnd) {
                    return new ReplaceResult {
                        Success = false,
                        Error = "Search text is empty or contains only whitespace"
                    };
                }

                // Get effective search lines
                var effectiveSearchLines = new List<string>();
                for (int i = searchStart; i <= searchEnd; i++) {
                    effectiveSearchLines.Add(searchLines[i]);
                }

                // Find all matching lines in target file
                var allMatches = new List<MatchInfo>();

                for (int i = 0; i <= targetLines.Length - effectiveSearchLines.Count; i++) {
                    bool matched = true;

                    for (int j = 0; j < effectiveSearchLines.Count; j++) {
                        string targetLine = targetLines[i + j];
                        string searchLine = effectiveSearchLines[j];

                        if (normalizeWhitespace) {
                            // Try normalized matching (similar to TryMatchAt logic)
                            if (!IsLineMatch(targetLine, searchLine)) {
                                matched = false;
                                break;
                            }
                        }
                        else {
                            // Exact match
                            if (targetLine != searchLine) {
                                matched = false;
                                break;
                            }
                        }
                    }

                    if (matched) {
                        allMatches.Add(new MatchInfo {
                            StartLine = i + 1,  // Convert to 1-based
                            EndLine = i + effectiveSearchLines.Count  // Convert to 1-based
                        });

                        if (!replaceAll) {
                            break;  // Only find first match if not replacing all
                        }

                        // Skip past this match to avoid overlapping matches
                        i += effectiveSearchLines.Count - 1;
                    }
                }

                if (allMatches.Count == 0) {
                    return new ReplaceResult {
                        Success = false,
                        Error = "Search text not found in target file (no match found even with normalized whitespace)",
                        ReplaceCount = 0,
                        AllMatches = new List<MatchInfo>()
                    };
                }

                // Prepare replacement lines
                string[] replacementLines = replacementText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Replace from bottom to top to avoid line number shifting
                var resultLines = new List<string>(targetLines);

                for (int matchIdx = allMatches.Count - 1; matchIdx >= 0; matchIdx--) {
                    var match = allMatches[matchIdx];
                    int startLine = match.StartLine - 1;  // Convert back to 0-based
                    int endLine = match.EndLine - 1;      // Convert back to 0-based

                    // Determine if indent adjustment is needed
                    // Check if first non-empty matched line is an exact match with search line
                    var actualReplacementLines = replacementLines;
                    int searchNonEmptyIdx = FindFirstNonEmptyLineIndex(effectiveSearchLines);
                    if (searchNonEmptyIdx >= 0) {
                        string fileFirstLine = targetLines[startLine + searchNonEmptyIdx];
                        string searchFirstLine = effectiveSearchLines[searchNonEmptyIdx];
                        // If not exact match, apply style-aware indent adjustment
                        if (fileFirstLine != searchFirstLine) {
                            var indentParams = CalcIndentAdjustment(targetLines, replacementLines, fileFirstLine, searchFirstLine);
                            actualReplacementLines = new string[replacementLines.Length];
                            for (int ri = 0; ri < replacementLines.Length; ri++) {
                                actualReplacementLines[ri] = ApplyIndentAdjustment(replacementLines[ri], indentParams);
                            }
                        }
                    }

                    // Remove matched lines
                    resultLines.RemoveRange(startLine, endLine - startLine + 1);

                    // Insert replacement lines
                    resultLines.InsertRange(startLine, actualReplacementLines);
                }

                // Build result content string
                string resultContent = string.Join("\n", resultLines);

                return new ReplaceResult {
                    Success = true,
                    LinesRemoved = effectiveSearchLines.Count * allMatches.Count,
                    LinesAdded = replacementLines.Length * allMatches.Count,
                    ResultContent = resultContent,
                    ReplaceCount = allMatches.Count,
                    AllMatches = allMatches
                };
            }
            catch (Exception ex) {
                return new ReplaceResult {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Check if two lines match with normalized whitespace comparison
        /// Uses the same logic as TryMatchAt for consistency
        /// </summary>
        private static bool IsLineMatch(string targetLine, string searchLine)
        {
            // First try exact match
            if (targetLine == searchLine)
                return true;

            // Second try: trim only
            if (targetLine.Trim() == searchLine.Trim())
                return true;

            // Third try: normalize all whitespace
            return NormalizeWhitespace(targetLine) == NormalizeWhitespace(searchLine);
        }

        /// <summary>
        /// Replace multi-line text in a file with normalized whitespace matching
        /// This is a file-based wrapper around ReplaceFullLinesText
        /// </summary>
        /// <param name="targetPath">Path to the target file</param>
        /// <param name="searchText">Multi-line text to search for</param>
        /// <param name="replacementText">Multi-line text to replace with</param>
        /// <param name="replaceAll">Whether to replace all matches (default: true)</param>
        /// <param name="normalizeWhitespace">Whether to normalize whitespace for matching (default: true)</param>
        /// <returns>True if replacement succeeded, false otherwise</returns>
        public bool ReplaceFullLinesTextInFile(string targetPath, string searchText, string replacementText, bool replaceAll = true, bool normalizeWhitespace = true)
        {
            try {
                string fullTargetPath = PathHelper.EnsureAbsolutePath(targetPath, _basePath);

                if (!File.Exists(fullTargetPath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Target file not found: {fullTargetPath}");
                    return false;
                }

                // Read file content
                string targetContent = File.ReadAllText(fullTargetPath, Encoding.UTF8);

                // Call ReplaceFullLinesText to perform the replacement
                var result = ReplaceFullLinesText(targetContent, searchText, replacementText, replaceAll, normalizeWhitespace);
                if (!result.Success) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Replace failed: {result.Error}");
                    return false;
                }

                // Write result back to file
                File.WriteAllText(fullTargetPath, result.ResultContent, Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"ReplaceFullLinesTextInFile error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Insert text after matched multi-line text with normalized whitespace matching
        /// </summary>
        /// <param name="targetContent">Target string content to search in</param>
        /// <param name="searchText">Multi-line text to search for</param>
        /// <param name="insertText">Multi-line text to insert after the match</param>
        /// <param name="insertAll">Whether to insert after all matches (default: true)</param>
        /// <param name="normalizeWhitespace">Whether to normalize whitespace for matching (default: true)</param>
        /// <returns>ReplaceResult with success status, details, and the modified content</returns>
        public static ReplaceResult InsertAfterFullLinesText(string targetContent, string searchText, string insertText, bool insertAll = true, bool normalizeWhitespace = true)
        {
            try {
                if (string.IsNullOrEmpty(targetContent)) {
                    return new ReplaceResult {
                        Success = false,
                        Error = "Target content is null or empty"
                    };
                }

                // Split target content into lines
                string[] targetLines = targetContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Split search text into lines
                string[] searchLines = searchText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Remove empty lines at start and end of search text
                int searchStart = 0;
                int searchEnd = searchLines.Length - 1;
                while (searchStart < searchLines.Length && string.IsNullOrWhiteSpace(searchLines[searchStart]))
                    searchStart++;
                while (searchEnd >= 0 && string.IsNullOrWhiteSpace(searchLines[searchEnd]))
                    searchEnd--;

                if (searchStart > searchEnd) {
                    return new ReplaceResult {
                        Success = false,
                        Error = "Search text is empty or contains only whitespace"
                    };
                }

                // Get effective search lines
                var effectiveSearchLines = new List<string>();
                for (int i = searchStart; i <= searchEnd; i++) {
                    effectiveSearchLines.Add(searchLines[i]);
                }

                // Find all matching lines in target file
                var allMatches = new List<MatchInfo>();

                for (int i = 0; i <= targetLines.Length - effectiveSearchLines.Count; i++) {
                    bool matched = true;

                    for (int j = 0; j < effectiveSearchLines.Count; j++) {
                        string targetLine = targetLines[i + j];
                        string searchLine = effectiveSearchLines[j];

                        if (normalizeWhitespace) {
                            if (!IsLineMatch(targetLine, searchLine)) {
                                matched = false;
                                break;
                            }
                        }
                        else {
                            if (targetLine != searchLine) {
                                matched = false;
                                break;
                            }
                        }
                    }

                    if (matched) {
                        allMatches.Add(new MatchInfo {
                            StartLine = i + 1,
                            EndLine = i + effectiveSearchLines.Count
                        });

                        if (!insertAll) {
                            break;
                        }

                        i += effectiveSearchLines.Count - 1;
                    }
                }

                if (allMatches.Count == 0) {
                    return new ReplaceResult {
                        Success = false,
                        Error = "Search text not found in target content",
                        ReplaceCount = 0,
                        AllMatches = new List<MatchInfo>()
                    };
                }

                // Prepare insert lines
                string[] insertLines = insertText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Insert from bottom to top to avoid line number shifting
                var resultLines = new List<string>(targetLines);

                for (int matchIdx = allMatches.Count - 1; matchIdx >= 0; matchIdx--) {
                    var match = allMatches[matchIdx];
                    int insertPosition = match.EndLine; // Insert after EndLine (0-based: EndLine is already the position after last matched line)

                    // Insert lines at position
                    resultLines.InsertRange(insertPosition, insertLines);
                }

                // Build result content string
                string resultContent = string.Join("\n", resultLines);

                return new ReplaceResult {
                    Success = true,
                    LinesRemoved = 0,
                    LinesAdded = insertLines.Length * allMatches.Count,
                    ResultContent = resultContent,
                    ReplaceCount = allMatches.Count,
                    AllMatches = allMatches
                };
            }
            catch (Exception ex) {
                return new ReplaceResult {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Insert text before matched multi-line text with normalized whitespace matching
        /// </summary>
        /// <param name="targetContent">Target string content to search in</param>
        /// <param name="searchText">Multi-line text to search for</param>
        /// <param name="insertText">Multi-line text to insert before the match</param>
        /// <param name="insertAll">Whether to insert before all matches (default: true)</param>
        /// <param name="normalizeWhitespace">Whether to normalize whitespace for matching (default: true)</param>
        /// <returns>ReplaceResult with success status, details, and the modified content</returns>
        public static ReplaceResult InsertBeforeFullLinesText(string targetContent, string searchText, string insertText, bool insertAll = true, bool normalizeWhitespace = true)
        {
            try {
                if (string.IsNullOrEmpty(targetContent)) {
                    return new ReplaceResult {
                        Success = false,
                        Error = "Target content is null or empty"
                    };
                }

                // Split target content into lines
                string[] targetLines = targetContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Split search text into lines
                string[] searchLines = searchText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Remove empty lines at start and end of search text
                int searchStart = 0;
                int searchEnd = searchLines.Length - 1;
                while (searchStart < searchLines.Length && string.IsNullOrWhiteSpace(searchLines[searchStart]))
                    searchStart++;
                while (searchEnd >= 0 && string.IsNullOrWhiteSpace(searchLines[searchEnd]))
                    searchEnd--;

                if (searchStart > searchEnd) {
                    return new ReplaceResult {
                        Success = false,
                        Error = "Search text is empty or contains only whitespace"
                    };
                }

                // Get effective search lines
                var effectiveSearchLines = new List<string>();
                for (int i = searchStart; i <= searchEnd; i++) {
                    effectiveSearchLines.Add(searchLines[i]);
                }

                // Find all matching lines in target file
                var allMatches = new List<MatchInfo>();

                for (int i = 0; i <= targetLines.Length - effectiveSearchLines.Count; i++) {
                    bool matched = true;

                    for (int j = 0; j < effectiveSearchLines.Count; j++) {
                        string targetLine = targetLines[i + j];
                        string searchLine = effectiveSearchLines[j];

                        if (normalizeWhitespace) {
                            if (!IsLineMatch(targetLine, searchLine)) {
                                matched = false;
                                break;
                            }
                        }
                        else {
                            if (targetLine != searchLine) {
                                matched = false;
                                break;
                            }
                        }
                    }

                    if (matched) {
                        allMatches.Add(new MatchInfo {
                            StartLine = i + 1,
                            EndLine = i + effectiveSearchLines.Count
                        });

                        if (!insertAll) {
                            break;
                        }

                        i += effectiveSearchLines.Count - 1;
                    }
                }

                if (allMatches.Count == 0) {
                    return new ReplaceResult {
                        Success = false,
                        Error = "Search text not found in target content",
                        ReplaceCount = 0,
                        AllMatches = new List<MatchInfo>()
                    };
                }

                // Prepare insert lines
                string[] insertLines = insertText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                // Insert from bottom to top to avoid line number shifting
                var resultLines = new List<string>(targetLines);

                for (int matchIdx = allMatches.Count - 1; matchIdx >= 0; matchIdx--) {
                    var match = allMatches[matchIdx];
                    int insertPosition = match.StartLine - 1; // Insert before StartLine (convert to 0-based)

                    // Insert lines at position
                    resultLines.InsertRange(insertPosition, insertLines);
                }

                // Build result content string
                string resultContent = string.Join("\n", resultLines);

                return new ReplaceResult {
                    Success = true,
                    LinesRemoved = 0,
                    LinesAdded = insertLines.Length * allMatches.Count,
                    ResultContent = resultContent,
                    ReplaceCount = allMatches.Count,
                    AllMatches = allMatches
                };
            }
            catch (Exception ex) {
                return new ReplaceResult {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Insert text after matched multi-line text in a file
        /// This is a file-based wrapper around InsertAfterFullLinesText
        /// </summary>
        /// <param name="targetPath">Path to the target file</param>
        /// <param name="searchText">Multi-line text to search for</param>
        /// <param name="insertText">Multi-line text to insert after the match</param>
        /// <param name="insertAll">Whether to insert after all matches (default: true)</param>
        /// <param name="normalizeWhitespace">Whether to normalize whitespace for matching (default: true)</param>
        /// <returns>True if insertion succeeded, false otherwise</returns>
        public bool InsertAfterFullLinesTextInFile(string targetPath, string searchText, string insertText, bool insertAll = true, bool normalizeWhitespace = true)
        {
            try {
                string fullTargetPath = PathHelper.EnsureAbsolutePath(targetPath, _basePath);

                if (!File.Exists(fullTargetPath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Target file not found: {fullTargetPath}");
                    return false;
                }

                // Read file content
                string targetContent = File.ReadAllText(fullTargetPath, Encoding.UTF8);

                // Call InsertAfterFullLinesText to perform the insertion
                var result = InsertAfterFullLinesText(targetContent, searchText, insertText, insertAll, normalizeWhitespace);
                if (!result.Success) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Insert after failed: {result.Error}");
                    return false;
                }

                // Write result back to file
                File.WriteAllText(fullTargetPath, result.ResultContent, Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"InsertAfterFullLinesTextInFile error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Insert text before matched multi-line text in a file
        /// This is a file-based wrapper around InsertBeforeFullLinesText
        /// </summary>
        /// <param name="targetPath">Path to the target file</param>
        /// <param name="searchText">Multi-line text to search for</param>
        /// <param name="insertText">Multi-line text to insert before the match</param>
        /// <param name="insertAll">Whether to insert before all matches (default: true)</param>
        /// <param name="normalizeWhitespace">Whether to normalize whitespace for matching (default: true)</param>
        /// <returns>True if insertion succeeded, false otherwise</returns>
        public bool InsertBeforeFullLinesTextInFile(string targetPath, string searchText, string insertText, bool insertAll = true, bool normalizeWhitespace = true)
        {
            try {
                string fullTargetPath = PathHelper.EnsureAbsolutePath(targetPath, _basePath);

                if (!File.Exists(fullTargetPath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Target file not found: {fullTargetPath}");
                    return false;
                }

                // Read file content
                string targetContent = File.ReadAllText(fullTargetPath, Encoding.UTF8);

                // Call InsertBeforeFullLinesText to perform the insertion
                var result = InsertBeforeFullLinesText(targetContent, searchText, insertText, insertAll, normalizeWhitespace);
                if (!result.Success) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Insert before failed: {result.Error}");
                    return false;
                }

                // Write result back to file
                File.WriteAllText(fullTargetPath, result.ResultContent, Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"InsertBeforeFullLinesTextInFile error: {ex.Message}");
                return false;
            }
        }

    }

    // Supporting classes for diff operations

    public class DiffHunk
    {
        public int OldStartLine { get; set; }
        public int OldLineCount { get; set; }
        public int NewStartLine { get; set; }
        public int NewLineCount { get; set; }
        public List<DiffLine> Lines { get; set; } = new List<DiffLine>();
    }

    public class DiffLine
    {
        public char Prefix { get; set; }  // ' ' context, '-' remove, '+' add
        public string Content { get; set; } = string.Empty;
    }

    public class DiffResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public List<DiffHunkResult> HunkResults { get; set; } = new List<DiffHunkResult>();
        public int LinesAdded { get; set; }
        public int LinesRemoved { get; set; }
        public string Library { get; set; } = string.Empty;
    }

    public class DiffHunkResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public int OldStartLine { get; set; }
        public int NewStartLine { get; set; }
        public int CorrectedStartLine { get; set; } // Actual matched line (1-based), 0 means no correction
        public int LinesAdded { get; set; }
        public int LinesRemoved { get; set; }
    }

    public class ReplaceResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public int LinesAdded { get; set; }
        public int LinesRemoved { get; set; }
        public string ResultContent { get; set; } = string.Empty;
        public int ReplaceCount { get; set; }
        public List<MatchInfo> AllMatches { get; set; } = new List<MatchInfo>();
    }

    public class MatchInfo
    {
        public int StartLine { get; set; }
        public int EndLine { get; set; }
    }

}
