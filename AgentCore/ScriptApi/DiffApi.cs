using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // apply_diff(targetPath, diffPath[, isContent[, exactMatch]]) - apply unified diff patch to target file
    sealed class ApplyDiffExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: apply_diff(targetPath, diffPathOrContent[, isContent[, exactMatch]]), aliased as apply_unified_diff|applydiff");
                return BoxedValue.NullObject;
            }

            try {
                string targetPath = operands[0].AsString;
                string diffPathOrContent = operands[1].AsString;
                bool isContent = operands.Count > 2 ? Convert.ToBoolean(operands[2].GetObject()) : false;
                bool exactMatch = operands.Count > 3 ? Convert.ToBoolean(operands[3].GetObject()) : false;

                var result = Core.AgentCore.Instance.DiffOps.ApplyDiff(targetPath, diffPathOrContent, isContent, exactMatch);

                var resultObj = new Dictionary<string, object>
                {
                    { "success", result.Success },
                    { "error", result.Error ?? string.Empty },
                    { "linesAdded", result.LinesAdded },
                    { "linesRemoved", result.LinesRemoved }
                };

                if (result.HunkResults != null) {
                    var hunks = new List<object>();
                    foreach (var hunk in result.HunkResults) {
                        var hunkDict = new Dictionary<string, object>
                        {
                            { "oldStartLine", hunk.OldStartLine },
                            { "newStartLine", hunk.NewStartLine },
                            { "success", hunk.Success },
                            { "error", hunk.Error ?? string.Empty },
                            { "linesAdded", hunk.LinesAdded },
                            { "linesRemoved", hunk.LinesRemoved }
                        };
                        if (hunk.CorrectedStartLine > 0) {
                            hunkDict["correctedStartLine"] = hunk.CorrectedStartLine;
                        }
                        hunks.Add(hunkDict);
                    }
                    resultObj["hunks"] = hunks;
                }

                return BoxedValue.FromObject(resultObj);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"applydiff error: {ex.Message}, Expected: apply_diff(targetPath, diffPathOrContent[, isContent[, exactMatch]]), aliased as apply_unified_diff|applydiff");
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

}
