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

    // file_last_write_time(path) - get file last write time as DateTime
    sealed class GetFileLastWriteTimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: file_last_write_time(path)");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_last_write_time: file not found: {path}");
                    return BoxedValue.NullObject;
                }

                var t = System.IO.File.GetLastWriteTime(path);
                return BoxedValue.FromDateTime(t);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_last_write_time error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // file_last_access_time(path) - get file last access time as DateTime
    sealed class GetFileLastAccessTimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: file_last_access_time(path)");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_last_access_time: file not found: {path}");
                    return BoxedValue.NullObject;
                }

                var t = System.IO.File.GetLastAccessTime(path);
                return BoxedValue.FromDateTime(t);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_last_access_time error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // file_create_time(path) - get file create time as DateTime
    sealed class GetFileCreateTimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: file_create_time(path)");
                return BoxedValue.NullObject;
            }

            try {
                string path = operands[0].AsString;
                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_create_time: file not found: {path}");
                    return BoxedValue.NullObject;
                }

                var t = System.IO.File.GetCreationTime(path);
                return BoxedValue.FromDateTime(t);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"file_create_time error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // set_file_attributes(path, int_attrs) - set file attributes from int value
    sealed class SetFileAttributesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_file_attributes(path, int_attrs)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                int attrs = operands[1].GetInt();
                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"set_file_attributes: file not found: {path}");
                    return BoxedValue.From(false);
                }

                System.IO.File.SetAttributes(path, (System.IO.FileAttributes)attrs);
                return BoxedValue.From(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"set_file_attributes error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // get_file_attributes(path) - get file attributes as int
    sealed class GetFileAttributesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_file_attributes(path)");
                return BoxedValue.From(-1);
            }

            try {
                string path = operands[0].AsString;
                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"get_file_attributes: file not found: {path}");
                    return BoxedValue.From(-1);
                }

                var attrs = System.IO.File.GetAttributes(path);
                return BoxedValue.From((int)attrs);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"get_file_attributes error: {ex.Message}");
                return BoxedValue.From(-1);
            }
        }
    }

    // set_file_last_write_time(path, date_time) - set file last write time
    sealed class SetFileLastWriteTimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_file_last_write_time(path, date_time)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                DateTime dt = operands[1].GetDateTime();
                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"set_file_last_write_time: file not found: {path}");
                    return BoxedValue.From(false);
                }

                System.IO.File.SetLastWriteTime(path, dt);
                return BoxedValue.From(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"set_file_last_write_time error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // set_file_last_access_time(path, date_time) - set file last access time
    sealed class SetFileLastAccessTimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_file_last_access_time(path, date_time)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                DateTime dt = operands[1].GetDateTime();
                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"set_file_last_access_time: file not found: {path}");
                    return BoxedValue.From(false);
                }

                System.IO.File.SetLastAccessTime(path, dt);
                return BoxedValue.From(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"set_file_last_access_time error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // set_file_create_time(path, date_time) - set file creation time
    sealed class SetFileCreateTimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_file_create_time(path, date_time)");
                return BoxedValue.From(false);
            }

            try {
                string path = operands[0].AsString;
                DateTime dt = operands[1].GetDateTime();
                if (!System.IO.File.Exists(path)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"set_file_create_time: file not found: {path}");
                    return BoxedValue.From(false);
                }

                System.IO.File.SetCreationTime(path, dt);
                return BoxedValue.From(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"set_file_create_time error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // get_diff_time_days(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalDays, truncated to int
    sealed class GetDiffTimeDaysExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_diff_time_days(date_time_1, date_time_2)");
                return BoxedValue.From(0);
            }

            try {
                DateTime dt1 = operands[0].GetDateTime();
                DateTime dt2 = operands[1].GetDateTime();
                int days = (int)(dt2 - dt1).TotalDays;
                return BoxedValue.From(days);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"get_diff_time_days error: {ex.Message}");
                return BoxedValue.From(0);
            }
        }
    }

    // get_diff_time_seconds(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalSeconds truncated to int
    sealed class GetDiffTimeSecondsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_diff_time_seconds(date_time_1, date_time_2)");
                return BoxedValue.From(0);
            }

            try {
                DateTime dt1 = operands[0].GetDateTime();
                DateTime dt2 = operands[1].GetDateTime();
                int seconds = (int)(dt2 - dt1).TotalSeconds;
                return BoxedValue.From(seconds);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"get_diff_time_seconds error: {ex.Message}");
                return BoxedValue.From(0);
            }
        }
    }

    // get_diff_time_ms(date_time_1, date_time_2) - (date_time_2 - date_time_1).TotalMilliseconds truncated to long
    sealed class GetDiffTimeMsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_diff_time_ms(date_time_1, date_time_2)");
                return BoxedValue.From(0L);
            }

            try {
                DateTime dt1 = operands[0].GetDateTime();
                DateTime dt2 = operands[1].GetDateTime();
                long ms = (long)(dt2 - dt1).TotalMilliseconds;
                return BoxedValue.From(ms);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"get_diff_time_ms error: {ex.Message}");
                return BoxedValue.From(0L);
            }
        }
    }
}
