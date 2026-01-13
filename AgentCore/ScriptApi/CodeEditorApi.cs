using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // replacefile(path, oldString, newString, allOccurrences) - replace string in file
    sealed class ReplaceFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                string oldString = operands[1].AsString;
                string newString = operands[2].AsString;
                bool allOccurrences = operands.Count > 3 ? operands[3].GetBool() : false;

                bool result = Core.AgentCore.Instance.FileOps.ReplaceInFile(path, oldString, newString, allOccurrences);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"replacefile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // replacerange(path, startLine, endLine, newContent) - replace line range
    sealed class ReplaceRangeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                int startLine = operands[1].GetInt();
                int endLine = operands[2].GetInt();
                string newContent = operands[3].AsString;

                bool result = Core.AgentCore.Instance.FileOps.ReplaceLines(path, startLine, endLine, newContent);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"replacerange error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // insertafter(path, searchPattern, content, allOccurrences) - insert content after pattern
    sealed class InsertAfterExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                string searchPattern = operands[1].AsString;
                string content = operands[2].AsString;
                bool allOccurrences = operands.Count > 3 ? operands[3].GetBool() : false;

                bool result = Core.AgentCore.Instance.FileOps.InsertAfter(path, searchPattern, content, allOccurrences);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"insertafter error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // insertbefore(path, searchPattern, content, allOccurrences) - insert content before pattern
    sealed class InsertBeforeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                string searchPattern = operands[1].AsString;
                string content = operands[2].AsString;
                bool allOccurrences = operands.Count > 3 ? operands[3].GetBool() : false;

                bool result = Core.AgentCore.Instance.FileOps.InsertBefore(path, searchPattern, content, allOccurrences);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"insertbefore error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // deletelines(path, startLine, endLine) - delete line range
    sealed class DeleteLinesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                int startLine = operands[1].GetInt();
                int endLine = operands[2].GetInt();

                bool result = Core.AgentCore.Instance.FileOps.DeleteLines(path, startLine, endLine);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"deletelines error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // searchfile(path, pattern, useRegex) - search pattern in file
    sealed class SearchFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                string pattern = operands[1].AsString;
                bool useRegex = operands.Count > 2 ? operands[2].GetBool() : false;

                var lines = Core.AgentCore.Instance.FileOps.SearchInFile(path, pattern, useRegex);
                var result = new List<object>();

                foreach (var line in lines)
                {
                    result.Add(line);
                }

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"searchfile error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // readlines(path, startLine, endLine) - read line range from file
    sealed class ReadLinesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                int startLine = operands.Count > 1 ? operands[1].GetInt() : 1;
                int endLine = operands.Count > 2 ? operands[2].GetInt() : -1;

                if (endLine == -1)
                {
                    // Read all lines
                    string[] lines = Core.AgentCore.Instance.FileOps.ReadFileLines(path);
                    var result = new List<object>();
                    foreach (var line in lines)
                    {
                        result.Add(line);
                    }
                    return BoxedValue.FromObject(result);
                }
                else
                {
                    string[] lines = Core.AgentCore.Instance.FileOps.ReadLinesRange(path, startLine, endLine);
                    var result = new List<object>();
                    foreach (var line in lines)
                    {
                        result.Add(line);
                    }
                    return BoxedValue.FromObject(result);
                }
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"readlines error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // getlinecount(path) - get line count of file
    sealed class GetLineCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(0);

            try
            {
                string path = operands[0].AsString;
                int count = Core.AgentCore.Instance.FileOps.GetLineCount(path);
                return BoxedValue.From(count);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"getlinecount error: {ex.Message}");
                return BoxedValue.From(0);
            }
        }
    }
}
