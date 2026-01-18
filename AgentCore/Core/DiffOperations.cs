using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using CefDotnetApp.AgentCore.Utils;

using CefDotnetApp.Interfaces;

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
        public DiffResult ApplyDiff(string targetPath, string diffPath)
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
                    return new DiffResult { Success = false, Error = "No valid hunks found in diff", Library = "Basic" };

                // Apply hunks from bottom to top to avoid line number shifting
                var sortedHunks = hunks.OrderByDescending(h => h.OldStartLine).ToList();
                List<string> resultLines = new List<string>(targetLines);
                List<DiffHunkResult> hunkResults = new List<DiffHunkResult>();

                foreach (var hunk in sortedHunks) {
                    var hunkResult = ApplyHunk(resultLines, hunk);
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
        public DiffResult ApplyDiff(string targetPath, string diffContent, bool isContent)
        {
            if (!isContent) {
                if (File.Exists(diffContent)) {
                    // If isContent is false, treat as file path (backward compatibility)
                    return ApplyDiff(targetPath, diffContent);
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
                    return new DiffResult { Success = false, Error = "No valid hunks found in diff", Library = "Basic" };

                // Apply hunks from bottom to top to avoid line number shifting
                var sortedHunks = hunks.OrderByDescending(h => h.OldStartLine).ToList();
                List<string> resultLines = new List<string>(targetLines);
                List<DiffHunkResult> hunkResults = new List<DiffHunkResult>();

                foreach (var hunk in sortedHunks) {
                    var hunkResult = ApplyHunk(resultLines, hunk);
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
        /// Apply diff with full-featured support using LibGit2Sharp
        /// </summary>
        public DiffResult ApplyDiffFull(string targetPath, string diffPath, bool createBackup = true)
        {
            try {
                string fullTargetPath = PathHelper.EnsureAbsolutePath(targetPath, _basePath);
                string fullDiffPath = PathHelper.EnsureAbsolutePath(diffPath, _basePath);

                if (!File.Exists(fullDiffPath))
                    return new DiffResult { Success = false, Error = "Diff file not found", Library = "LibGit2Sharp" };

                // Create backup if requested
                if (createBackup && File.Exists(fullTargetPath)) {
                    string backupPath = fullTargetPath + ".backup";
                    File.Copy(fullTargetPath, backupPath, true);
                }

                // Read diff content
                string diffContent = File.ReadAllText(fullDiffPath, Encoding.UTF8);

                // Try to use LibGit2Sharp for advanced processing
                return ApplyDiffUsingLibGit2(fullTargetPath, diffContent, createBackup);
            }
            catch (Exception ex) {
                return new DiffResult { Success = false, Error = ex.Message, Library = "LibGit2Sharp (Error)" };
            }
        }

        /// <summary>
        /// Apply diff using LibGit2Sharp native capabilities
        /// This method creates a temporary Git repository and uses LibGit2Sharp's
        /// Repository APIs to apply the patch in a Git-native way.
        /// </summary>
        /// <param name="targetPath">Path to the target file</param>
        /// <param name="diffContent">Unified diff content to apply</param>
        /// <param name="createBackup">Whether to create a backup of the target file</param>
        /// <returns>DiffResult indicating success or failure</returns>
        public DiffResult ApplyDiffUsingLibGit2Native(string targetPath, string diffContent, bool createBackup = true)
        {
            try {
                string fullTargetPath = PathHelper.EnsureAbsolutePath(targetPath, _basePath);

                if (!File.Exists(fullTargetPath)) {
                    return new DiffResult {
                        Success = false,
                        Error = $"Target file not found: {fullTargetPath}",
                        Library = "LibGit2Sharp (Native)"
                    };
                }

                // Create backup if requested
                if (createBackup) {
                    string backupPath = fullTargetPath + ".backup";
                    File.Copy(fullTargetPath, backupPath, true);
                }

                // Create temporary directory for Git repository
                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                string tempFilePath = Path.Combine(tempDir, Path.GetFileName(fullTargetPath));
                File.Copy(fullTargetPath, tempFilePath, true);

                try {
                    // Initialize Git repository using LibGit2Sharp
                    string repoPath = Repository.Init(tempDir);
                    using (var repo = new Repository(repoPath)) {

                        // Stage and commit the original file
                        Commands.Stage(repo, "*");
                        Signature author = new Signature("Agent", "agent@local", DateTimeOffset.Now);
                        Commit commit = repo.Commit("Original state", author, author);

                        // Write diff to temporary file
                        string diffFilePath = Path.Combine(tempDir, "patch.diff");
                        File.WriteAllText(diffFilePath, diffContent, Encoding.UTF8);

                        // Note: LibGit2Sharp doesn't provide a direct "apply patch" API.
                        // We'll use git apply command instead (see below).
                    }

                    // Use git apply command through LibGit2Sharp's wrapper or system
                    // Since LibGit2Sharp doesn't expose git-apply, we need to use system git
                    string gitPath = FindGitExecutable();
                    if (string.IsNullOrEmpty(gitPath)) {
                        return new DiffResult {
                            Success = false,
                            Error = "Git executable not found. Please ensure git is installed and in PATH.",
                            Library = "LibGit2Sharp (Native)"
                        };
                    }

                    // Apply patch using git apply in the temp directory
                    ProcessStartInfo psi = new ProcessStartInfo {
                        FileName = gitPath,
                        Arguments = "apply patch.diff",
                        WorkingDirectory = tempDir,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = System.Diagnostics.Process.Start(psi)) {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode != 0) {
                            DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"git apply failed: {error}");
                            return new DiffResult {
                                Success = false,
                                Error = $"Failed to apply patch: {error}",
                                Library = "LibGit2Sharp (Native)"
                            };
                        }
                    }

                    // Read the modified file and apply back to target
                    if (File.Exists(tempFilePath)) {
                        string[] modifiedContent = File.ReadAllLines(tempFilePath, Encoding.UTF8);
                        string[] originalContent = File.ReadAllLines(fullTargetPath, Encoding.UTF8);

                        // Calculate changes
                        int linesAdded = 0;
                        int linesRemoved = 0;

                        // Simple diff calculation
                        var diff = CalculateDiff(originalContent, modifiedContent);
                        linesAdded = diff.Item1;
                        linesRemoved = diff.Item2;

                        // Apply the changes
                        File.WriteAllLines(fullTargetPath, modifiedContent, Encoding.UTF8);

                        return new DiffResult {
                            Success = true,
                            LinesAdded = linesAdded,
                            LinesRemoved = linesRemoved,
                            Library = "LibGit2Sharp (Native)"
                        };
                    }
                    else {
                        return new DiffResult {
                            Success = false,
                            Error = "Modified file not found after patch application",
                            Library = "LibGit2Sharp (Native)"
                        };
                    }
                }
                finally {
                    // Clean up temporary directory
                    try {
                        if (Directory.Exists(tempDir)) {
                            Directory.Delete(tempDir, true);
                        }
                    }
                    catch (Exception ex) {
                        DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Failed to clean up temp directory: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"ApplyDiffUsingLibGit2Native error: {ex.Message}");
                return new DiffResult {
                    Success = false,
                    Error = ex.Message,
                    Library = "LibGit2Sharp (Native)"
                };
            }
        }

        /// <summary>
        /// Find git executable in system PATH
        /// </summary>
        private string FindGitExecutable()
        {
            try {
                var psi = new ProcessStartInfo {
                    FileName = "git",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = System.Diagnostics.Process.Start(psi)) {
                    if (process != null) {
                        process.WaitForExit(1000);
                        if (process.ExitCode == 0) {
                            return "git";
                        }
                    }
                }
            }
            catch {
                // Git not found
            }

            return null;
        }

        /// <summary>
        /// Calculate diff between two arrays of lines using simple LCS-based algorithm
        /// </summary>
        private (int added, int removed) CalculateDiff(string[] original, string[] modified)
        {
            // Use a simple diff algorithm based on longest common subsequence
            // This is more accurate than line-by-line comparison
            int added = 0;
            int removed = 0;

            // Simple approach: count lines that exist in one but not the other
            var originalSet = new HashSet<string>(original);
            var modifiedSet = new HashSet<string>(modified);

            // Lines in modified but not in original are added
            foreach (var line in modified) {
                if (!originalSet.Contains(line))
                    added++;
            }

            // Lines in original but not in modified are removed
            foreach (var line in original) {
                if (!modifiedSet.Contains(line))
                    removed++;
            }

            // If counts don't match, use simple length difference as fallback
            if (added == 0 && removed == 0 && original.Length != modified.Length) {
                if (modified.Length > original.Length)
                    added = modified.Length - original.Length;
                else
                    removed = original.Length - modified.Length;
            }

            return (added, removed);
        }

        /// <summary>
        /// Parse enhanced diff format with multiple file support
        /// </summary>
        private List<Patch> ParseEnhancedDiff(string diffContent)
        {
            var patches = new List<Patch>();
            var lines = diffContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            int i = 0;
            while (i < lines.Length) {
                string line = lines[i];

                // Look for diff header
                if (line.StartsWith("diff ") || line.StartsWith("index ")) {
                    var patch = new Patch();

                    // Parse file paths
                    if (line.StartsWith("diff ")) {
                        var match = Regex.Match(line, @"diff\s+--git\s+(a/.+)\s+(b/.+)");
                        if (match.Success) {
                            patch.OldPath = match.Groups[1].Value.Substring(2);
                            patch.NewPath = match.Groups[2].Value.Substring(2);
                            patch.TargetPath = patch.NewPath;
                        }
                    }

                    // Skip to hunk headers
                    while (i < lines.Length && !lines[i].StartsWith("@@")) {
                        if (lines[i].StartsWith("new file"))
                            patch.NewFile = true;
                        else if (lines[i].StartsWith("deleted file"))
                            patch.DeletedFile = true;
                        i++;
                    }

                    // Parse hunks
                    while (i < lines.Length && lines[i].StartsWith("@@")) {
                        var hunk = ParseHunk(lines, ref i);
                        if (hunk != null)
                            patch.Hunks.Add(hunk);
                    }

                    if (patch.Hunks.Count > 0)
                        patches.Add(patch);
                }
                else {
                    i++;
                }
            }

            return patches;
        }

        /// <summary>
        /// Parse a single hunk from diff lines
        /// </summary>
        private Hunk ParseHunk(string[] lines, ref int index)
        {
            if (index >= lines.Length || !lines[index].StartsWith("@@"))
                return null;

            var hunk = new Hunk();
            var match = Regex.Match(lines[index], @"^@@\s+-?(\d+)(?:,(\d+))?\s+\+?(\d+)(?:,(\d+))?\s+@@");
            if (!match.Success)
                return null;

            hunk.OldStart = int.Parse(match.Groups[1].Value) - 1; // 0-based
            hunk.OldCount = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1;
            hunk.NewStart = int.Parse(match.Groups[3].Value) - 1;
            hunk.NewCount = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 1;

            index++;

            // Collect hunk lines
            while (index < lines.Length) {
                string line = lines[index];
                if (line.StartsWith("@@") || line.StartsWith("diff ") || line.StartsWith("index "))
                    break;

                if (line.Length > 0) {
                    char prefix = line[0];
                    string content = line.Length > 1 ? line.Substring(1) : string.Empty;
                    hunk.Lines.Add(new DiffLine { Prefix = prefix, Content = content });
                }

                index++;
            }

            return hunk;
        }

        /// <summary>
        /// Apply patch to a file using LibGit2Sharp-like algorithm
        /// </summary>
        private DiffResult ApplyPatchToFile(string filePath, Patch patch)
        {
            try {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
                var allHunkResults = new List<DiffHunkResult>();
                int totalAdded = 0;
                int totalRemoved = 0;

                // Apply hunks from bottom to top
                foreach (var hunk in patch.Hunks.OrderByDescending(h => h.OldStart)) {
                    var result = ApplyEnhancedHunk(lines, hunk);
                    allHunkResults.Add(result);

                    if (!result.Success) {
                        return new DiffResult {
                            Success = false,
                            Error = $"Failed to apply hunk at line {hunk.OldStart + 1}: {result.Error}",
                            Library = "LibGit2Sharp"
                        };
                    }

                    totalAdded += result.LinesAdded;
                    totalRemoved += result.LinesRemoved;
                }

                File.WriteAllLines(filePath, lines, Encoding.UTF8);

                return new DiffResult {
                    Success = true,
                    HunkResults = allHunkResults,
                    LinesAdded = totalAdded,
                    LinesRemoved = totalRemoved,
                    Library = "LibGit2Sharp"
                };
            }
            catch (Exception ex) {
                return new DiffResult { Success = false, Error = ex.Message, Library = "LibGit2Sharp" };
            }
        }

        /// <summary>
        /// Apply enhanced hunk with better context matching
        /// </summary>
        private DiffHunkResult ApplyEnhancedHunk(List<string> lines, Hunk hunk)
        {
            var result = new DiffHunkResult {
                OldStartLine = hunk.OldStart + 1,
                NewStartLine = hunk.NewStart + 1
            };

            try {
                // Enhanced context matching
                int matchedIndex = FindEnhancedContextMatch(lines, hunk);
                if (matchedIndex < 0) {
                    result.Success = false;
                    result.Error = "Context mismatch - cannot find suitable location";
                    return result;
                }

                // Apply changes - use diff content directly, not position-based indexing
                var newLines = new List<string>();
                int linesRemoved = 0;
                int linesAdded = 0;

                // Process diff lines directly (similar to ApplyHunk)
                foreach (var diffLine in hunk.Lines) {
                    if (diffLine.Prefix == ' ' || diffLine.Prefix == '+') {
                        // Context line or added line - use content from diff
                        newLines.Add(diffLine.Content);
                        if (diffLine.Prefix == '+')
                            linesAdded++;
                    }
                    else if (diffLine.Prefix == '-') {
                        // Removed line - don't add to newLines
                        linesRemoved++;
                    }
                }

                // Replace old lines with new lines
                int oldCount = hunk.Lines.Count(l => l.Prefix != '+');
                if (matchedIndex + oldCount <= lines.Count) {
                    lines.RemoveRange(matchedIndex, oldCount);
                    lines.InsertRange(matchedIndex, newLines);
                }

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
        /// Enhanced context matching with better algorithms
        /// </summary>
        private int FindEnhancedContextMatch(List<string> lines, Hunk hunk)
        {
            var contextLines = hunk.Lines.Where(l => l.Prefix == ' ').ToList();

            if (contextLines.Count == 0) {
                // No context, use exact position
                if (hunk.OldStart >= 0 && hunk.OldStart < lines.Count)
                    return hunk.OldStart;
                return 0;
            }

            // Try exact match at expected position
            if (TryExactMatch(lines, hunk.OldStart, contextLines, out int exactIndex))
                return exactIndex;

            // Enhanced fuzzy search with larger range
            int searchRange = Math.Min(20, lines.Count / 3);
            int bestMatch = -1;
            float bestScore = 0;

            for (int offset = -searchRange; offset <= searchRange; offset++) {
                int testIndex = hunk.OldStart + offset;
                if (testIndex < 0 || testIndex >= lines.Count)
                    continue;

                float score = CalculateMatchScore(lines, testIndex, contextLines, hunk.Lines.Count);
                if (score > bestScore) {
                    bestScore = score;
                    bestMatch = testIndex;
                }
            }

            // Require at least 60% match for acceptance
            if (bestScore >= 0.6f)
                return bestMatch;

            // Last resort: try to find context anywhere in file
            return FindContextAnywhere(lines, contextLines);
        }

        /// <summary>
        /// Try to find exact context match at specific position
        /// </summary>
        private static bool TryExactMatch(List<string> lines, int position, List<DiffLine> contextLines, out int matchedIndex)
        {
            matchedIndex = -1;

            if (position < 0 || position >= lines.Count)
                return false;

            // Check if all context lines match at position
            if (position + contextLines.Count > lines.Count)
                return false;

            int matched = 0;
            for (int i = 0; i < contextLines.Count; i++) {
                if (lines[position + i] == contextLines[i].Content)
                    matched++;
            }

            if (matched == contextLines.Count) {
                matchedIndex = position;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculate match score for context
        /// </summary>
        private static float CalculateMatchScore(List<string> lines, int position, List<DiffLine> contextLines, int totalHunkLines)
        {
            if (contextLines.Count == 0)
                return 0;

            int matches = 0;
            int searchWindow = Math.Min(10, lines.Count - position);

            for (int i = 0; i < contextLines.Count && i < searchWindow; i++) {
                if (position + i < lines.Count) {
                    if (lines[position + i] == contextLines[i].Content)
                        matches++;
                    else if (string.IsNullOrWhiteSpace(lines[position + i]) && string.IsNullOrWhiteSpace(contextLines[i].Content))
                        matches++; // Count whitespace-only lines as match
                }
            }

            return (float)matches / contextLines.Count;
        }

        /// <summary>
        /// Find context anywhere in the file (last resort)
        /// </summary>
        private static int FindContextAnywhere(List<string> lines, List<DiffLine> contextLines)
        {
            if (contextLines.Count == 0)
                return 0;

            // Look for first context line
            string firstContext = contextLines[0].Content;
            for (int i = 0; i < lines.Count; i++) {
                if (lines[i] == firstContext) {
                    // Check if following lines match
                    bool allMatch = true;
                    for (int j = 1; j < contextLines.Count; j++) {
                        if (i + j >= lines.Count || lines[i + j] != contextLines[j].Content) {
                            allMatch = false;
                            break;
                        }
                    }

                    if (allMatch)
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Apply patch to create a new file
        /// </summary>
        private static DiffHunkResult ApplyNewFilePatch(string filePath, Patch patch)
        {
            try {
                var lines = new List<string>();
                int totalAdded = 0;

                foreach (var hunk in patch.Hunks) {
                    foreach (var line in hunk.Lines) {
                        if (line.Prefix == ' ' || line.Prefix == '+') {
                            lines.Add(line.Content);
                            totalAdded++;
                        }
                    }
                }

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllLines(filePath, lines, Encoding.UTF8);

                return new DiffHunkResult {
                    Success = true,
                    OldStartLine = 0,
                    NewStartLine = 1,
                    LinesAdded = totalAdded,
                    LinesRemoved = 0
                };
            }
            catch (Exception ex) {
                return new DiffHunkResult {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Apply diff using LibGit2Sharp for advanced features
        /// </summary>
        private DiffResult ApplyDiffUsingLibGit2(string targetPath, string diffContent, bool createBackup)
        {
            try {
                // Parse unified diff using enhanced parser
                var patches = ParseEnhancedDiff(diffContent);
                if (patches.Count == 0) {
                    // Fallback to basic implementation if LibGit2Sharp parsing fails
                    return ApplyDiffFallback(targetPath, diffContent);
                }

                List<DiffHunkResult> allHunkResults = new List<DiffHunkResult>();
                int totalAdded = 0;
                int totalRemoved = 0;

                // Apply each patch
                foreach (var patch in patches) {
                    string targetFile = patch.TargetPath;
                    if (string.IsNullOrEmpty(targetFile))
                        targetFile = targetPath;

                    if (!File.Exists(targetFile) && patch.NewFile) {
                        // Create new file
                        var fileResult = ApplyNewFilePatch(targetFile, patch);
                        allHunkResults.Add(fileResult);
                        if (!fileResult.Success) {
                            return new DiffResult {
                                Success = false,
                                Error = $"Failed to create new file: {fileResult.Error}",
                                Library = "LibGit2Sharp"
                            };
                        }
                        totalAdded += fileResult.LinesAdded;
                        continue;
                    }

                    if (!File.Exists(targetFile)) {
                        return new DiffResult {
                            Success = false,
                            Error = $"Target file not found: {targetFile}",
                            Library = "LibGit2Sharp"
                        };
                    }

                    if (patch.DeletedFile) {
                        // Delete file - read line count before deletion
                        totalRemoved += File.ReadAllLines(targetFile).Length;
                        File.Delete(targetFile);
                        continue;
                    }

                    // Apply patch to existing file
                    var result = ApplyPatchToFile(targetFile, patch);
                    allHunkResults.AddRange(result.HunkResults);

                    if (!result.Success) {
                        return new DiffResult {
                            Success = false,
                            Error = result.Error,
                            HunkResults = allHunkResults,
                            Library = "LibGit2Sharp"
                        };
                    }

                    totalAdded += result.LinesAdded;
                    totalRemoved += result.LinesRemoved;
                }

                return new DiffResult {
                    Success = true,
                    HunkResults = allHunkResults,
                    LinesAdded = totalAdded,
                    LinesRemoved = totalRemoved,
                    Library = "LibGit2Sharp"
                };
            }
            catch (Exception ex) {
                // Fallback to basic implementation on error
                return ApplyDiffFallback(targetPath, diffContent);
            }
        }

        /// <summary>
        /// Fallback to basic implementation
        /// </summary>
        private DiffResult ApplyDiffFallback(string targetPath, string diffContent)
        {
            // Write diff to temporary file and use basic implementation
            string tempDiff = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".diff");
            try {
                File.WriteAllText(tempDiff, diffContent, Encoding.UTF8);

                // Use the temporary diff file path relative to base path
                string relativeDiff = Path.GetRelativePath(_basePath, tempDiff);
                return ApplyDiff(targetPath, relativeDiff);
            }
            finally {
                if (File.Exists(tempDiff))
                    File.Delete(tempDiff);
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
        private static DiffHunkResult ApplyHunk(List<string> lines, DiffHunk hunk)
        {
            var result = new DiffHunkResult {
                OldStartLine = hunk.OldStartLine + 1,  // Convert to 1-based for display
                NewStartLine = hunk.NewStartLine + 1   // Convert to 1-based for display
            };

            try {
                int startIndex = hunk.OldStartLine;  // Already 0-based

                // Find match using context + removed lines
                int matchedIndex = FindContextMatch(lines, startIndex, hunk);
                if (matchedIndex < 0) {
                    result.Success = false;
                    result.Error = "Context mismatch";
                    return result;
                }

                // Build new content by processing diff lines
                var newLines = new List<string>();
                int linesRemoved = 0;
                int linesAdded = 0;

                foreach (var diffLine in hunk.Lines) {
                    if (diffLine.Prefix == ' ' || diffLine.Prefix == '+') {
                        newLines.Add(diffLine.Content);
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
                if (matchedIndex + oldCount > lines.Count) {
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
        private static int FindContextMatch(List<string> lines, int expectedIndex, DiffHunk hunk)
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

            // Try exact match at expected position
            if (TryMatchAt(lines, expectedIndex, originalLines))
                return expectedIndex;

            // Fuzzy search with sliding window
            int searchRange = Math.Min(10, lines.Count / 2);
            for (int offset = 1; offset <= searchRange; offset++) {
                // Try below
                if (TryMatchAt(lines, expectedIndex + offset, originalLines))
                    return expectedIndex + offset;

                // Try above
                if (TryMatchAt(lines, expectedIndex - offset, originalLines))
                    return expectedIndex - offset;
            }

            return -1;
        }
        /// <summary>
        /// Try to match original lines (context + removed) at a specific position.
        /// First tries exact match, then falls back to normalized whitespace match.
        /// </summary>
        private static bool TryMatchAt(List<string> lines, int startIndex, List<string> originalLines)
        {
            if (originalLines.Count == 0)
                return true; // No lines to match

            if (startIndex < 0 || startIndex + originalLines.Count > lines.Count)
                return false;

            // First try exact match
            bool exactMatch = true;
            for (int i = 0; i < originalLines.Count; i++) {
                if (lines[startIndex + i] != originalLines[i]) {
                    exactMatch = false;
                    break;
                }
            }
            if (exactMatch)
                return true;

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

        private static string NormalizeWhitespace(string line)
        {
            if (string.IsNullOrEmpty(line))
                return string.Empty;

            // Replace all consecutive whitespace characters (space, tab, etc.) with a single space
            // Then trim leading and trailing whitespace
            return Regex.Replace(line.Trim(), @"\s+", " ");
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

                    // Remove matched lines
                    resultLines.RemoveRange(startLine, endLine - startLine + 1);

                    // Insert replacement lines
                    resultLines.InsertRange(startLine, replacementLines);
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
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Target file not found: {fullTargetPath}");
                    return false;
                }

                // Read file content
                string targetContent = File.ReadAllText(fullTargetPath, Encoding.UTF8);

                // Call ReplaceFullLinesText to perform the replacement
                var result = ReplaceFullLinesText(targetContent, searchText, replacementText, replaceAll, normalizeWhitespace);
                if (!result.Success) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Replace failed: {result.Error}");
                    return false;
                }

                // Write result back to file
                File.WriteAllText(fullTargetPath, result.ResultContent, Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"ReplaceFullLinesTextInFile error: {ex.Message}");
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
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Target file not found: {fullTargetPath}");
                    return false;
                }

                // Read file content
                string targetContent = File.ReadAllText(fullTargetPath, Encoding.UTF8);

                // Call InsertAfterFullLinesText to perform the insertion
                var result = InsertAfterFullLinesText(targetContent, searchText, insertText, insertAll, normalizeWhitespace);
                if (!result.Success) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Insert after failed: {result.Error}");
                    return false;
                }

                // Write result back to file
                File.WriteAllText(fullTargetPath, result.ResultContent, Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"InsertAfterFullLinesTextInFile error: {ex.Message}");
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
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Target file not found: {fullTargetPath}");
                    return false;
                }

                // Read file content
                string targetContent = File.ReadAllText(fullTargetPath, Encoding.UTF8);

                // Call InsertBeforeFullLinesText to perform the insertion
                var result = InsertBeforeFullLinesText(targetContent, searchText, insertText, insertAll, normalizeWhitespace);
                if (!result.Success) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Insert before failed: {result.Error}");
                    return false;
                }

                // Write result back to file
                File.WriteAllText(fullTargetPath, result.ResultContent, Encoding.UTF8);

                return true;
            }
            catch (Exception ex) {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"InsertBeforeFullLinesTextInFile error: {ex.Message}");
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
        public List<DiffLine> Lines { get; set; }
    }

    public class DiffLine
    {
        public char Prefix { get; set; }  // ' ' context, '-' remove, '+' add
        public string Content { get; set; }
    }

    public class DiffResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public List<DiffHunkResult> HunkResults { get; set; }
        public int LinesAdded { get; set; }
        public int LinesRemoved { get; set; }
        public string Library { get; set; }
    }

    public class DiffHunkResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int OldStartLine { get; set; }
        public int NewStartLine { get; set; }
        public int LinesAdded { get; set; }
        public int LinesRemoved { get; set; }
    }

    public class ReplaceResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int LinesAdded { get; set; }
        public int LinesRemoved { get; set; }
        public string ResultContent { get; set; }
        public int ReplaceCount { get; set; }
        public List<MatchInfo> AllMatches { get; set; }
    }

    public class MatchInfo
    {
        public int StartLine { get; set; }
        public int EndLine { get; set; }
    }

    // Additional classes for full-featured diff support

    public class Patch
    {
        public string OldPath { get; set; }
        public string NewPath { get; set; }
        public string TargetPath { get; set; }
        public bool NewFile { get; set; }
        public bool DeletedFile { get; set; }
        public List<Hunk> Hunks { get; set; } = new List<Hunk>();
    }

    public class Hunk
    {
        public int OldStart { get; set; }
        public int OldCount { get; set; }
        public int NewStart { get; set; }
        public int NewCount { get; set; }
        public List<DiffLine> Lines { get; set; } = new List<DiffLine>();
    }
}
