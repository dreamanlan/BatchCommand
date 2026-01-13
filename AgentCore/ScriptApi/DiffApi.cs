using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // applydiff(targetPath, diffPath) - apply unified diff patch to target file
    sealed class ApplyDiffExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string targetPath = operands[0].AsString;
                string diffPath = operands[1].AsString;

                var result = Core.AgentCore.Instance.DiffOps.ApplyDiff(targetPath, diffPath);

                var resultObj = new Dictionary<string, object>
                {
                    { "success", result.Success },
                    { "error", result.Error ?? string.Empty },
                    { "linesAdded", result.LinesAdded },
                    { "linesRemoved", result.LinesRemoved }
                };

                if (result.HunkResults != null)
                {
                    var hunks = new List<object>();
                    foreach (var hunk in result.HunkResults)
                    {
                        hunks.Add(new Dictionary<string, object>
                        {
                            { "oldStartLine", hunk.OldStartLine },
                            { "newStartLine", hunk.NewStartLine },
                            { "success", hunk.Success },
                            { "error", hunk.Error ?? string.Empty },
                            { "linesAdded", hunk.LinesAdded },
                            { "linesRemoved", hunk.LinesRemoved }
                        });
                    }
                    resultObj["hunks"] = hunks;
                }

                return BoxedValue.FromObject(resultObj);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"applydiff error: {ex.Message}");
                var errorObj = new Dictionary<string, object>
                {
                    { "success", false },
                    { "error", ex.Message },
                    { "linesAdded", 0 },
                    { "linesRemoved", 0 }
                };
                return BoxedValue.FromObject(errorObj);
            }
        }
    }

    // applydifffull(targetPath, diffPath) - apply diff with full features (using LibGit2Sharp)
    sealed class ApplyDiffFullExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string targetPath = operands[0].AsString;
                string diffPath = operands[1].AsString;
                bool createBackup = operands.Count > 2 ? Convert.ToBoolean(operands[2].GetObject()) : true;

                // Use full-featured diff implementation with LibGit2Sharp
                var result = Core.AgentCore.Instance.DiffOps.ApplyDiffFull(targetPath, diffPath, createBackup);

                var resultObj = new Dictionary<string, object>
                {
                    { "success", result.Success },
                    { "error", result.Error ?? string.Empty },
                    { "linesAdded", result.LinesAdded },
                    { "linesRemoved", result.LinesRemoved },
                    { "library", result.Library ?? "Unknown" }
                };

                if (result.HunkResults != null)
                {
                    var hunks = new List<object>();
                    foreach (var hunk in result.HunkResults)
                    {
                        hunks.Add(new Dictionary<string, object>
                        {
                            { "oldStartLine", hunk.OldStartLine },
                            { "newStartLine", hunk.NewStartLine },
                            { "success", hunk.Success },
                            { "error", hunk.Error ?? string.Empty },
                            { "linesAdded", hunk.LinesAdded },
                            { "linesRemoved", hunk.LinesRemoved }
                        });
                    }
                    resultObj["hunks"] = hunks;
                }

                return BoxedValue.FromObject(resultObj);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"applydifffull error: {ex.Message}");
                var errorObj = new Dictionary<string, object>
                {
                    { "success", false },
                    { "error", ex.Message },
                    { "linesAdded", 0 },
                    { "linesRemoved", 0 },
                    { "library", "Error" }
                };
                return BoxedValue.FromObject(errorObj);
            }
        }
    }

    // applydifflibgit2(targetPath, diffContent, createBackup) - apply diff using LibGit2Sharp native capabilities
    sealed class ApplyDiffLibGit2Exp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string targetPath = operands[0].AsString;
                string diffContent = operands[1].AsString;
                bool createBackup = operands.Count > 2 ? Convert.ToBoolean(operands[2].GetObject()) : true;

                // Use LibGit2Sharp native implementation
                var result = Core.AgentCore.Instance.DiffOps.ApplyDiffUsingLibGit2Native(targetPath, diffContent, createBackup);

                var resultObj = new Dictionary<string, object>
                {
                    { "success", result.Success },
                    { "error", result.Error ?? string.Empty },
                    { "linesAdded", result.LinesAdded },
                    { "linesRemoved", result.LinesRemoved },
                    { "library", result.Library ?? "Unknown" }
                };

                if (result.HunkResults != null)
                {
                    var hunks = new List<object>();
                    foreach (var hunk in result.HunkResults)
                    {
                        hunks.Add(new Dictionary<string, object>
                        {
                            { "oldStartLine", hunk.OldStartLine },
                            { "newStartLine", hunk.NewStartLine },
                            { "success", hunk.Success },
                            { "error", hunk.Error ?? string.Empty },
                            { "linesAdded", hunk.LinesAdded },
                            { "linesRemoved", hunk.LinesRemoved }
                        });
                    }
                    resultObj["hunks"] = hunks;
                }

                return BoxedValue.FromObject(resultObj);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"applydifflibgit2 error: {ex.Message}");
                var errorObj = new Dictionary<string, object>
                {
                    { "success", false },
                    { "error", ex.Message },
                    { "linesAdded", 0 },
                    { "linesRemoved", 0 },
                    { "library", "Error" }
                };
                return BoxedValue.FromObject(errorObj);
            }
        }
    }
}
