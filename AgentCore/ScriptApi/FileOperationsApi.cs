using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;
using CefDotnetApp.AgentCore.Utils;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // readfile(path) - read file content
    sealed class ReadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                string content = Core.AgentCore.Instance.FileOps.ReadFile(path);
                return BoxedValue.FromString(content ?? string.Empty);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"readfile error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // writefile(path, content) - write file content
    sealed class WriteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                string content = operands[1].AsString;
                bool result = Core.AgentCore.Instance.FileOps.WriteFile(path, content);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"writefile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // appendfile(path, content) - append to file
    sealed class AppendFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                string content = operands[1].AsString;
                bool result = Core.AgentCore.Instance.FileOps.AppendFile(path, content);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"appendfile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // deletefile(path) - delete file
    sealed class DeleteFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                bool result = Core.AgentCore.Instance.FileOps.DeleteFile(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"deletefile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // copyfile(sourcePath, destPath, overwrite) - copy file
    sealed class CopyFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string sourcePath = operands[0].AsString;
                string destPath = operands[1].AsString;
                bool overwrite = operands.Count > 2 ? operands[2].GetBool() : false;
                bool result = Core.AgentCore.Instance.FileOps.CopyFile(sourcePath, destPath, overwrite);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"copyfile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // movefile(sourcePath, destPath, overwrite) - move file
    sealed class MoveFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string sourcePath = operands[0].AsString;
                string destPath = operands[1].AsString;
                bool overwrite = operands.Count > 2 ? operands[2].GetBool() : false;
                bool result = Core.AgentCore.Instance.FileOps.MoveFile(sourcePath, destPath, overwrite);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"movefile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // fileexists(path) - check if file exists
    sealed class FileExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                bool result = Core.AgentCore.Instance.FileOps.FileExists(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"fileexists error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // direxists(path) - check if directory exists
    sealed class DirExistsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                bool result = Core.AgentCore.Instance.FileOps.DirectoryExists(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"direxists error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // createdir(path) - create directory
    sealed class CreateDirExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                bool result = Core.AgentCore.Instance.FileOps.CreateDirectory(path);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"createdir error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // deletedir(path, recursive) - delete directory
    sealed class DeleteDirExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                bool recursive = operands.Count > 1 ? operands[1].GetBool() : false;
                bool result = Core.AgentCore.Instance.FileOps.DeleteDirectory(path, recursive);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"deletedir error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // listdir(path, pattern, recursive) - list directory contents
    sealed class ListDirExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                string pattern = operands.Count > 1 ? operands[1].AsString : "*";
                bool recursive = operands.Count > 2 ? operands[2].GetBool() : false;

                var files = Core.AgentCore.Instance.FileOps.ListDirectory(path, pattern, recursive);
                var result = new List<object>();

                foreach (var file in files)
                {
                    result.Add(file.Path);
                }

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"listdir error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }
}
