using System;

using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using BatchCommand.Utils;

namespace BatchCommand.Api
{
    // apply_diff(targetPath, diffPath[, isContent[, exactMatch]]) - apply unified diff patch to target file
    public sealed class ApplyDiffExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                ApiErrorInfo.AppendLine("Expected: apply_diff(targetPath, diffPathOrContent[, isContent[, exactMatch]]), aliased as apply_unified_diff|applydiff");
                return BoxedValue.NullObject;
            }

            try {
                string targetPath = operands[0].AsString;
                string diffPathOrContent = operands[1].AsString;
                bool isContent = operands.Count > 2 ? Convert.ToBoolean(operands[2].GetObject()) : false;
                bool exactMatch = operands.Count > 3 ? Convert.ToBoolean(operands[3].GetObject()) : false;

                var host = DslHost.Current;
                var diffOps = new DiffOperations(host?.BasePath ?? string.Empty, host?.AppDir ?? string.Empty, host?.IsMac ?? false);
                var result = diffOps.ApplyDiff(targetPath, diffPathOrContent, isContent, exactMatch);

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
                            { "correction", hunk.Correction ?? string.Empty },
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
                ApiErrorInfo.AppendLine($"applydiff error: {ex.Message}, Expected: apply_diff(targetPath, diffPathOrContent[, isContent[, exactMatch]]), aliased as apply_unified_diff|applydiff");
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
