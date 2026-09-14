using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace BatchCommand.Api
{
    // Shared utility apis used by multiple hosts (AgentCore, CefDotnetApp,
    // ...). Implementations only: BatchScriptApiRegistrar registers the names.
    // Errors are recorded through ApiErrorInfo (thread static, fetched
    // uniformly by the host after dsl execution) and a null/default value is
    // returned, so execution continues.
    // Note: read_file/to_json/strlen (StringLengthExp) moved in from AgentCore
    // OtherOperationsApi.cs, and string_contains*/combine_path/new_string_builder/
    // string_builder_tostring use the AgentCore-copied implementations in
    // FrameworkApiAlias.cs - see the notes at the bottom of this file.

    // append_line(stringbuilder, val) - append value with newline
    public sealed class AppendLineExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: append_line(stringbuilder, val)");
                return BoxedValue.NullObject;
            }
            var sb = operands[0].As<StringBuilder>();
            if (null == sb) {
                ApiErrorInfo.AppendLine("append_line: first argument is not a string builder");
                return BoxedValue.NullObject;
            }
            var tmp = new StringBuilder();
            BatchCommand.Utils.DslHelper.ConvertToString(operands[1], tmp, 0, true);
            sb.AppendLine(tmp.ToString());
            return BoxedValue.FromObject(sb);
        }
    }

    // string_contains/not_contains/contains_any/not_contains_any and
    // combine_path/new_string_builder/string_builder_tostring previously had
    // local ports here; the single implementation now lives in
    // FrameworkApiAlias.cs (copied from AgentCore), registered under all the
    // alias names by FrameworkApiAlias.RegisterApis.
}
