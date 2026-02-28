using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // get_file_size(path) - get file size in bytes
    sealed class GetFileSizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_file_size(path), aliased as file_size");
                return BoxedValue.From(-1L);
            }

            try {
                string path = operands[0].AsString;
                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"get_file_size: file not found: {path}");
                    return BoxedValue.From(-1L);
                }

                var fileInfo = new System.IO.FileInfo(path);
                long size = fileInfo.Length;
                return BoxedValue.From(size);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"get_file_size error: {ex.Message}");
                return BoxedValue.From(-1L);
            }
        }
    }
}
