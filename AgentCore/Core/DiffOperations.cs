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
    public class DiffOperations : IDiffOperations
    {
        private readonly string _basePath;

        public DiffOperations(string basePath)
        {
            _basePath = basePath ?? Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Apply unified diff patch to target file
        /// </summary>
        public DiffResult ApplyDiff(string targetPath, string diffPath)
        {
            try
            {
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

                foreach (var hunk in sortedHunks)
                {
                    var hunkResult = ApplyHunk(resultLines, hunk);
                    hunkResults.Add(hunkResult);
                    if (!hunkResult.Success)
                    {
                        return new DiffResult
                        {
                            Success = false,
                            Error = $"Failed to apply hunk at line {hunk.OldStartLine}: {hunkResult.Error}",
                            HunkResults = hunkResults,
                            Library = "Basic"
                        };
                    }
                }

                // Write the result
                File.WriteAllLines(fullTargetPath, resultLines, Encoding.UTF8);

                return new DiffResult
                {
                    Success = true,
                    HunkResults = hunkResults,
                    LinesAdded = hunkResults.Sum(h => h.LinesAdded),
                    LinesRemoved = hunkResults.Sum(h => h.LinesRemoved),
                    Library = "Basic"
                };
            }
            catch (Exception ex)
            {
                return new DiffResult { Success = false, Error = ex.Message, Library = "Basic" };
            }
        }

        /// <summary>
        /// Apply diff with full-featured support using LibGit2Sharp
        /// </summary>
        public DiffResult ApplyDiffFull(string targetPath, string diffPath, bool createBackup = true)
        {
            try
            {
                string fullTargetPath = PathHelper.EnsureAbsolutePath(targetPath, _basePath);
                string fullDiffPath = PathHelper.EnsureAbsolutePath(diffPath, _basePath);

                if (!File.Exists(fullDiffPath))
                    return new DiffResult { Success = false, Error = "Diff file not found", Library = "LibGit2Sharp" };

                // Create backup if requested
                if (createBackup && File.Exists(fullTargetPath))
                {
                    string backupPath = fullTargetPath + ".backup";
                    File.Copy(fullTargetPath, backupPath, true);
                }

                // Read diff content
                string diffContent = File.ReadAllText(fullDiffPath, Encoding.UTF8);

                // Try to use LibGit2Sharp for advanced processing
                return ApplyDiffUsingLibGit2(fullTargetPath, diffContent, createBackup);
            }
            catch (Exception ex)
            {
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
            try
            {
                string fullTargetPath = PathHelper.EnsureAbsolutePath(targetPath, _basePath);

                if (!File.Exists(fullTargetPath))
                {
                    return new DiffResult
                    {
                        Success = false,
                        Error = $"Target file not found: {fullTargetPath}",
                        Library = "LibGit2Sharp (Native)"
                    };
                }

                // Create backup if requested
                if (createBackup)
                {
                    string backupPath = fullTargetPath + ".backup";
                    File.Copy(fullTargetPath, backupPath, true);
                    Core.AgentCore.Instance.Logger.Info($"Backup created at: {backupPath}");
                }

                // Create temporary directory for Git repository
                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                string tempFilePath = Path.Combine(tempDir, Path.GetFileName(fullTargetPath));
                File.Copy(fullTargetPath, tempFilePath, true);

                Core.AgentCore.Instance.Logger.Info($"Using LibGit2Sharp native API at: {tempDir}");

                try
                {
                    // Initialize Git repository using LibGit2Sharp
                    string repoPath = Repository.Init(tempDir);
                    using (var repo = new Repository(repoPath))
                    {
                        Core.AgentCore.Instance.Logger.Info($"Git repository initialized at: {repoPath}");

                        // Stage and commit the original file
                        Commands.Stage(repo, "*");
                        Signature author = new Signature("Agent", "agent@local", DateTimeOffset.Now);
                        Commit commit = repo.Commit("Original state", author, author);
                        Core.AgentCore.Instance.Logger.Info($"Initial commit created: {commit.Id}");

                        // Write diff to temporary file
                        string diffFilePath = Path.Combine(tempDir, "patch.diff");
                        File.WriteAllText(diffFilePath, diffContent, Encoding.UTF8);

                        // Note: LibGit2Sharp doesn't provide a direct "apply patch" API.
                        // We'll use git apply command instead (see below).
                        Core.AgentCore.Instance.Logger.Info($"Patch written to temporary file: {diffFilePath}");

                    }

                    // Use git apply command through LibGit2Sharp's wrapper or system
                    // Since LibGit2Sharp doesn't expose git-apply, we need to use system git
                    string gitPath = FindGitExecutable();
                    if (string.IsNullOrEmpty(gitPath))
                    {
                        return new DiffResult
                        {
                            Success = false,
                            Error = "Git executable not found. Please ensure git is installed and in PATH.",
                            Library = "LibGit2Sharp (Native)"
                        };
                    }

                    // Apply patch using git apply in the temp directory
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = gitPath,
                        Arguments = "apply patch.diff",
                        WorkingDirectory = tempDir,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = System.Diagnostics.Process.Start(psi))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            Core.AgentCore.Instance.Logger.Error($"git apply failed: {error}");
                            return new DiffResult
                            {
                                Success = false,
                                Error = $"Failed to apply patch: {error}",
                                Library = "LibGit2Sharp (Native)"
                            };
                        }

                        Core.AgentCore.Instance.Logger.Info("Patch applied successfully");
                    }

                    // Read the modified file and apply back to target
                    if (File.Exists(tempFilePath))
                    {
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

                        return new DiffResult
                        {
                            Success = true,
                            LinesAdded = linesAdded,
                            LinesRemoved = linesRemoved,
                            Library = "LibGit2Sharp (Native)"
                        };
                    }
                    else
                    {
                        return new DiffResult
                        {
                            Success = false,
                            Error = "Modified file not found after patch application",
                            Library = "LibGit2Sharp (Native)"
                        };
                    }
                }
                finally
                {
                    // Clean up temporary directory
                    try
                    {
                        if (Directory.Exists(tempDir))
                        {
                            Directory.Delete(tempDir, true);
                            Core.AgentCore.Instance.Logger.Info($"Temporary directory cleaned up: {tempDir}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.AgentCore.Instance.Logger.Warning($"Failed to clean up temp directory: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"ApplyDiffUsingLibGit2Native error: {ex.Message}");
                return new DiffResult
                {
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
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    if (process != null)
                    {
                        process.WaitForExit(1000);
                        if (process.ExitCode == 0)
                        {
                            return "git";
                        }
                    }
                }
            }
            catch
            {
                // Git not found
            }

            return null;
        }

        /// <summary>
        /// Calculate diff between two arrays of lines
        /// </summary>
        private (int added, int removed) CalculateDiff(string[] original, string[] modified)
        {
            // Simple calculation: use LibGit2Sharp's diff capabilities
            int added = 0;
            int removed = 0;

            // This is a simplified calculation. For a more accurate result,
            // we could use LibGit2Sharp's diff API if both files were in a repository.
            // For now, use a basic line-by-line comparison.
            int maxLength = Math.Max(original.Length, modified.Length);
            for (int i = 0; i < maxLength; i++)
            {
                if (i >= original.Length)
                {
                    added++;
                }
                else if (i >= modified.Length)
                {
                    removed++;
                }
                else if (original[i] != modified[i])
                {
                    // Count as both removed and added (changed)
                    removed++;
                    added++;
                }
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
            while (i < lines.Length)
            {
                string line = lines[i];

                // Look for diff header
                if (line.StartsWith("diff ") || line.StartsWith("index "))
                {
                    var patch = new Patch();

                    // Parse file paths
                    if (line.StartsWith("diff "))
                    {
                        var match = Regex.Match(line, @"diff\s+--git\s+(a/.+)\s+(b/.+)");
                        if (match.Success)
                        {
                            patch.OldPath = match.Groups[1].Value.Substring(2);
                            patch.NewPath = match.Groups[2].Value.Substring(2);
                            patch.TargetPath = patch.NewPath;
                        }
                    }

                    // Skip to hunk headers
                    while (i < lines.Length && !lines[i].StartsWith("@@"))
                    {
                        if (lines[i].StartsWith("new file"))
                            patch.NewFile = true;
                        else if (lines[i].StartsWith("deleted file"))
                            patch.DeletedFile = true;
                        i++;
                    }

                    // Parse hunks
                    while (i < lines.Length && lines[i].StartsWith("@@"))
                    {
                        var hunk = ParseHunk(lines, ref i);
                        if (hunk != null)
                            patch.Hunks.Add(hunk);
                    }

                    if (patch.Hunks.Count > 0)
                        patches.Add(patch);
                }
                else
                {
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
            while (index < lines.Length)
            {
                string line = lines[index];
                if (line.StartsWith("@@") || line.StartsWith("diff ") || line.StartsWith("index "))
                    break;

                if (line.Length > 0)
                {
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
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
                var allHunkResults = new List<DiffHunkResult>();
                int totalAdded = 0;
                int totalRemoved = 0;

                // Apply hunks from bottom to top
                foreach (var hunk in patch.Hunks.OrderByDescending(h => h.OldStart))
                {
                    var result = ApplyEnhancedHunk(lines, hunk);
                    allHunkResults.Add(result);

                    if (!result.Success)
                    {
                        return new DiffResult
                        {
                            Success = false,
                            Error = $"Failed to apply hunk at line {hunk.OldStart + 1}: {result.Error}",
                            Library = "LibGit2Sharp"
                        };
                    }

                    totalAdded += result.LinesAdded;
                    totalRemoved += result.LinesRemoved;
                }

                File.WriteAllLines(filePath, lines, Encoding.UTF8);

                return new DiffResult
                {
                    Success = true,
                    HunkResults = allHunkResults,
                    LinesAdded = totalAdded,
                    LinesRemoved = totalRemoved,
                    Library = "LibGit2Sharp"
                };
            }
            catch (Exception ex)
            {
                return new DiffResult { Success = false, Error = ex.Message, Library = "LibGit2Sharp" };
            }
        }

        /// <summary>
        /// Apply enhanced hunk with better context matching
        /// </summary>
        private DiffHunkResult ApplyEnhancedHunk(List<string> lines, Hunk hunk)
        {
            var result = new DiffHunkResult
            {
                OldStartLine = hunk.OldStart + 1,
                NewStartLine = hunk.NewStart + 1
            };

            try
            {
                // Enhanced context matching
                int matchedIndex = FindEnhancedContextMatch(lines, hunk);
                if (matchedIndex < 0)
                {
                    result.Success = false;
                    result.Error = "Context mismatch - cannot find suitable location";
                    return result;
                }

                // Apply changes
                var newLines = new List<string>();
                int linesRemoved = 0;
                int linesAdded = 0;

                for (int i = 0; i < hunk.Lines.Count; i++)
                {
                    var diffLine = hunk.Lines[i];

                    if (diffLine.Prefix == ' ')
                    {
                        // Context line
                        if (matchedIndex + i < lines.Count)
                        {
                            newLines.Add(lines[matchedIndex + i]);
                        }
                    }
                    else if (diffLine.Prefix == '-')
                    {
                        // Remove line
                        linesRemoved++;
                    }
                    else if (diffLine.Prefix == '+')
                    {
                        // Add line
                        newLines.Add(diffLine.Content);
                        linesAdded++;
                    }
                }

                // Replace old lines with new lines
                int oldCount = hunk.Lines.Count(l => l.Prefix != '+');
                if (matchedIndex + oldCount <= lines.Count)
                {
                    lines.RemoveRange(matchedIndex, oldCount);
                    lines.InsertRange(matchedIndex, newLines);
                }

                result.Success = true;
                result.LinesRemoved = linesRemoved;
                result.LinesAdded = linesAdded;
                return result;
            }
            catch (Exception ex)
            {
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

            if (contextLines.Count == 0)
            {
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

            for (int offset = -searchRange; offset <= searchRange; offset++)
            {
                int testIndex = hunk.OldStart + offset;
                if (testIndex < 0 || testIndex >= lines.Count)
                    continue;

                float score = CalculateMatchScore(lines, testIndex, contextLines, hunk.Lines.Count);
                if (score > bestScore)
                {
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
        private bool TryExactMatch(List<string> lines, int position, List<DiffLine> contextLines, out int matchedIndex)
        {
            matchedIndex = -1;

            if (position < 0 || position >= lines.Count)
                return false;

            // Map context lines to their positions in the hunk
            var contextPositions = new List<int>();
            int totalLines = contextLines.Count; // This is approximate, need better tracking

            // Simple implementation: check if context lines match near position
            int matched = 0;
            int searchWindow = Math.Min(5, lines.Count - position);

            for (int i = 0; i < contextLines.Count && i < searchWindow; i++)
            {
                if (position + i < lines.Count && lines[position + i] == contextLines[i].Content)
                    matched++;
            }

            if (matched == contextLines.Count)
            {
                matchedIndex = position;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculate match score for context
        /// </summary>
        private float CalculateMatchScore(List<string> lines, int position, List<DiffLine> contextLines, int totalHunkLines)
        {
            if (contextLines.Count == 0)
                return 0;

            int matches = 0;
            int searchWindow = Math.Min(10, lines.Count - position);

            for (int i = 0; i < contextLines.Count && i < searchWindow; i++)
            {
                if (position + i < lines.Count)
                {
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
        private int FindContextAnywhere(List<string> lines, List<DiffLine> contextLines)
        {
            if (contextLines.Count == 0)
                return 0;

            // Look for first context line
            string firstContext = contextLines[0].Content;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] == firstContext)
                {
                    // Check if following lines match
                    bool allMatch = true;
                    for (int j = 1; j < contextLines.Count; j++)
                    {
                        if (i + j >= lines.Count || lines[i + j] != contextLines[j].Content)
                        {
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
        private DiffHunkResult ApplyNewFilePatch(string filePath, Patch patch)
        {
            try
            {
                var lines = new List<string>();
                int totalAdded = 0;

                foreach (var hunk in patch.Hunks)
                {
                    foreach (var line in hunk.Lines)
                    {
                        if (line.Prefix == ' ' || line.Prefix == '+')
                        {
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

                return new DiffHunkResult
                {
                    Success = true,
                    OldStartLine = 0,
                    NewStartLine = 1,
                    LinesAdded = totalAdded,
                    LinesRemoved = 0
                };
            }
            catch (Exception ex)
            {
                return new DiffHunkResult
                {
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
            try
            {
                // Parse unified diff using enhanced parser
                var patches = ParseEnhancedDiff(diffContent);
                if (patches.Count == 0)
                {
                    // Fallback to basic implementation if LibGit2Sharp parsing fails
                    return ApplyDiffFallback(targetPath, diffContent);
                }

                List<DiffHunkResult> allHunkResults = new List<DiffHunkResult>();
                int totalAdded = 0;
                int totalRemoved = 0;

                // Apply each patch
                foreach (var patch in patches)
                {
                    string targetFile = patch.TargetPath;
                    if (string.IsNullOrEmpty(targetFile))
                        targetFile = targetPath;

                    if (!File.Exists(targetFile) && patch.NewFile)
                    {
                        // Create new file
                        var fileResult = ApplyNewFilePatch(targetFile, patch);
                        allHunkResults.Add(fileResult);
                        if (!fileResult.Success)
                        {
                            return new DiffResult
                            {
                                Success = false,
                                Error = $"Failed to create new file: {fileResult.Error}",
                                Library = "LibGit2Sharp"
                            };
                        }
                        totalAdded += fileResult.LinesAdded;
                        continue;
                    }

                    if (!File.Exists(targetFile))
                    {
                        return new DiffResult
                        {
                            Success = false,
                            Error = $"Target file not found: {targetFile}",
                            Library = "LibGit2Sharp"
                        };
                    }

                    if (patch.DeletedFile)
                    {
                        // Delete file
                        File.Delete(targetFile);
                        totalRemoved += File.ReadAllLines(targetFile).Length;
                        continue;
                    }

                    // Apply patch to existing file
                    var result = ApplyPatchToFile(targetFile, patch);
                    allHunkResults.AddRange(result.HunkResults);

                    if (!result.Success)
                    {
                        return new DiffResult
                        {
                            Success = false,
                            Error = result.Error,
                            HunkResults = allHunkResults,
                            Library = "LibGit2Sharp"
                        };
                    }

                    totalAdded += result.LinesAdded;
                    totalRemoved += result.LinesRemoved;
                }

                return new DiffResult
                {
                    Success = true,
                    HunkResults = allHunkResults,
                    LinesAdded = totalAdded,
                    LinesRemoved = totalRemoved,
                    Library = "LibGit2Sharp"
                };
            }
            catch (Exception ex)
            {
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
            try
            {
                File.WriteAllText(tempDiff, diffContent, Encoding.UTF8);

                // Use the temporary diff file path relative to base path
                string relativeDiff = Path.GetRelativePath(_basePath, tempDiff);
                return ApplyDiff(targetPath, relativeDiff);
            }
            finally
            {
                if (File.Exists(tempDiff))
                    File.Delete(tempDiff);
            }
        }

        /// <summary>
        /// Parse unified diff format and extract hunks
        /// </summary>
        private List<DiffHunk> ParseUnifiedDiff(string diffContent)
        {
            var hunks = new List<DiffHunk>();
            var lines = diffContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Match hunk header: @@ -old_start,old_count +new_start,new_count @@
                var match = Regex.Match(line, @"^@@\s+-?(\d+)(?:,(\d+))?\s+\+?(\d+)(?:,(\d+))?\s+@@");
                if (match.Success)
                {
                    var hunk = new DiffHunk
                    {
                        OldStartLine = int.Parse(match.Groups[1].Value),
                        OldLineCount = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1,
                        NewStartLine = int.Parse(match.Groups[3].Value),
                        NewLineCount = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 1,
                        Lines = new List<DiffLine>()
                    };

                    // Collect hunk lines
                    i++;
                    while (i < lines.Length)
                    {
                        string hunkLine = lines[i];
                        if (hunkLine.StartsWith("@@") || hunkLine.StartsWith("diff ") || hunkLine.StartsWith("index "))
                            break;

                        if (hunkLine.Length == 0)
                        {
                            i++;
                            continue;
                        }

                        char prefix = hunkLine[0];
                        string content = hunkLine.Length > 1 ? hunkLine.Substring(1) : string.Empty;

                        hunk.Lines.Add(new DiffLine
                        {
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

        /// <summary>
        /// Apply a single hunk to the file lines
        /// </summary>
        private DiffHunkResult ApplyHunk(List<string> lines, DiffHunk hunk)
        {
            var result = new DiffHunkResult
            {
                OldStartLine = hunk.OldStartLine,
                NewStartLine = hunk.NewStartLine
            };

            try
            {
                // Convert to 0-based index
                int startIndex = hunk.OldStartLine - 1;

                // Validate context
                var contextLines = new List<string>();
                for (int i = 0; i < hunk.Lines.Count; i++)
                {
                    if (hunk.Lines[i].Prefix == ' ')
                    {
                        contextLines.Add(hunk.Lines[i].Content);
                    }
                }

                // Find matching context (fuzzy matching)
                int matchedIndex = FindContextMatch(lines, startIndex, contextLines);
                if (matchedIndex < 0)
                {
                    result.Success = false;
                    result.Error = "Context mismatch";
                    return result;
                }

                // Apply changes
                var newLines = new List<string>();
                int linesRemoved = 0;
                int linesAdded = 0;

                for (int i = 0; i < hunk.Lines.Count; i++)
                {
                    var diffLine = hunk.Lines[i];

                    if (diffLine.Prefix == ' ')
                    {
                        // Context line - keep it
                        if (matchedIndex + i < lines.Count)
                        {
                            newLines.Add(lines[matchedIndex + i]);
                        }
                    }
                    else if (diffLine.Prefix == '-')
                    {
                        // Remove line
                        linesRemoved++;
                    }
                    else if (diffLine.Prefix == '+')
                    {
                        // Add line
                        newLines.Add(diffLine.Content);
                        linesAdded++;
                    }
                }

                // Replace old lines with new lines
                int oldCount = hunk.Lines.Count(l => l.Prefix != '+');
                lines.RemoveRange(matchedIndex, oldCount);
                lines.InsertRange(matchedIndex, newLines);

                result.Success = true;
                result.LinesRemoved = linesRemoved;
                result.LinesAdded = linesAdded;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Find the best match for context lines in the file
        /// </summary>
        private int FindContextMatch(List<string> lines, int expectedIndex, List<string> contextLines)
        {
            if (contextLines.Count == 0)
            {
                if (expectedIndex >= 0 && expectedIndex < lines.Count)
                    return expectedIndex;
                return 0;
            }

            // First try exact match at expected index
            if (expectedIndex >= 0 && expectedIndex < lines.Count)
            {
                bool exactMatch = true;
                for (int i = 0; i < contextLines.Count; i++)
                {
                    int lineIndex = GetContextLineIndex(expectedIndex, i, contextLines.Count);
                    if (lineIndex >= lines.Count || lines[lineIndex] != contextLines[i])
                    {
                        exactMatch = false;
                        break;
                    }
                }
                if (exactMatch)
                    return expectedIndex;
            }

            // Fuzzy search nearby
            int searchRange = Math.Min(10, lines.Count / 2);
            int bestMatch = -1;
            int bestMatchScore = 0;

            for (int offset = -searchRange; offset <= searchRange; offset++)
            {
                int testIndex = expectedIndex + offset;
                if (testIndex < 0 || testIndex >= lines.Count)
                    continue;

                int matchScore = 0;
                for (int i = 0; i < contextLines.Count; i++)
                {
                    int lineIndex = GetContextLineIndex(testIndex, i, contextLines.Count);
                    if (lineIndex >= lines.Count)
                        break;

                    if (lines[lineIndex] == contextLines[i])
                        matchScore++;
                }

                if (matchScore > bestMatchScore)
                {
                    bestMatchScore = matchScore;
                    bestMatch = testIndex;
                }
            }

            // Require at least 50% context match
            if (bestMatchScore * 2 >= contextLines.Count)
                return bestMatch;

            return -1;
        }

        private int GetContextLineIndex(int startIndex, int contextIndex, int totalContext)
        {
            // Simple heuristic: context lines are distributed throughout the hunk
            return startIndex + (int)((double)contextIndex / totalContext * 5);
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
