using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using BatchCommand;
using BatchCommand.Utils;
using AgentCore.CodeAnalysis;
using AgentCore.Models;

namespace AgentCore.ScriptApi
{
    // Unified code analysis DSL APIs for multi-language support
    public static partial class UnifiedCodeAnalysisScriptApi
    {
        private static UnifiedCodeAnalysisApi _api = new UnifiedCodeAnalysisApi();

        public static void RegisterApis()
        {
            // ========== LLM-Friendly Code Analysis APIs ==========
            // Overview APIs
            BatchCommand.BatchScript.Register("view_file_structure", "view_file_structure(filePath, language)", new ExpressionFactoryHelper<ViewFileStructureExp>());
            BatchCommand.BatchScript.Register("view_code_structure", "view_code_structure(code, language, [fileName])", new ExpressionFactoryHelper<ViewCodeStructureExp>());

            // Function Search APIs (formatted output)
            BatchCommand.BatchScript.Register("find_functions", "find_functions(filePath, language, [nameRegexPattern])", new ExpressionFactoryHelper<FindFunctionsExp>());
            BatchCommand.BatchScript.Register("find_functions_in_code", "find_functions_in_code(code, language, [nameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindFunctionsInCodeExp>());
            BatchCommand.BatchScript.Register("find_function", "find_function(filePath, language, functionNameRegexPattern)", new ExpressionFactoryHelper<FindFunctionExp>());
            BatchCommand.BatchScript.Register("find_function_in_code", "find_function_in_code(code, language, functionNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindFunctionInCodeExp>());

            // Type Search APIs (formatted output)
            BatchCommand.BatchScript.Register("find_types", "find_types(filePath, language, [nameRegexPattern])", new ExpressionFactoryHelper<FindTypesExp>());
            BatchCommand.BatchScript.Register("find_types_in_code", "find_types_in_code(code, language, [nameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindTypesInCodeExp>());
            BatchCommand.BatchScript.Register("find_type", "find_type(filePath, language, typeNameRegexPattern)", new ExpressionFactoryHelper<FindTypeExp>());
            BatchCommand.BatchScript.Register("find_type_in_code", "find_type_in_code(code, language, typeNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindTypeInCodeExp>());

            // Variable Search APIs (formatted output)
            BatchCommand.BatchScript.Register("find_variables", "find_variables(filePath, language, [nameRegexPattern])", new ExpressionFactoryHelper<FindVariablesExp>());
            BatchCommand.BatchScript.Register("find_variables_in_code", "find_variables_in_code(code, language, [nameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindVariablesInCodeExp>());
            BatchCommand.BatchScript.Register("find_variable", "find_variable(filePath, language, variableNameRegexPattern)", new ExpressionFactoryHelper<FindVariableExp>());
            BatchCommand.BatchScript.Register("find_variable_in_code", "find_variable_in_code(code, language, variableNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindVariableInCodeExp>());
            BatchCommand.BatchScript.Register("find_global_variables", "find_global_variables(filePath, language, [nameRegexPattern])", new ExpressionFactoryHelper<FindGlobalVariablesExp>());
            BatchCommand.BatchScript.Register("find_global_variables_in_code", "find_global_variables_in_code(code, language, [nameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindGlobalVariablesInCodeExp>());
            BatchCommand.BatchScript.Register("find_parameters", "find_parameters(filePath, language, [nameRegexPattern])", new ExpressionFactoryHelper<FindParametersExp>());
            BatchCommand.BatchScript.Register("find_parameters_in_code", "find_parameters_in_code(code, language, [nameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindParametersInCodeExp>());

            // Class Member Search APIs (formatted output)
            BatchCommand.BatchScript.Register("find_class_members", "find_class_members(filePath, language, classNameRegexPattern)", new ExpressionFactoryHelper<FindClassMembersExp>());
            BatchCommand.BatchScript.Register("find_class_members_in_code", "find_class_members_in_code(code, language, classNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindClassMembersInCodeExp>());
            BatchCommand.BatchScript.Register("find_fields", "find_fields(filePath, language, classNameRegexPattern, [fieldNameRegexPattern])", new ExpressionFactoryHelper<FindFieldsExp>());
            BatchCommand.BatchScript.Register("find_fields_in_code", "find_fields_in_code(code, language, classNameRegexPattern, [fieldNameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindFieldsInCodeExp>());
            BatchCommand.BatchScript.Register("find_field", "find_field(filePath, language, classNameRegexPattern, fieldNameRegexPattern)", new ExpressionFactoryHelper<FindFieldExp>());
            BatchCommand.BatchScript.Register("find_field_in_code", "find_field_in_code(code, language, classNameRegexPattern, fieldNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindFieldInCodeExp>());
            BatchCommand.BatchScript.Register("find_properties", "find_properties(filePath, language, classNameRegexPattern, [propertyNameRegexPattern])", new ExpressionFactoryHelper<FindPropertiesExp>());
            BatchCommand.BatchScript.Register("find_properties_in_code", "find_properties_in_code(code, language, classNameRegexPattern, [propertyNameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindPropertiesInCodeExp>());
            BatchCommand.BatchScript.Register("find_property", "find_property(filePath, language, classNameRegexPattern, propertyNameRegexPattern)", new ExpressionFactoryHelper<FindPropertyExp>());
            BatchCommand.BatchScript.Register("find_property_in_code", "find_property_in_code(code, language, classNameRegexPattern, propertyNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindPropertyInCodeExp>());
            BatchCommand.BatchScript.Register("find_methods", "find_methods(filePath, language, classNameRegexPattern, [methodNameRegexPattern])", new ExpressionFactoryHelper<FindMethodsExp>());
            BatchCommand.BatchScript.Register("find_methods_in_code", "find_methods_in_code(code, language, classNameRegexPattern, [methodNameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindMethodsInCodeExp>());
            BatchCommand.BatchScript.Register("find_method", "find_method(filePath, language, classNameRegexPattern, methodNameRegexPattern)", new ExpressionFactoryHelper<FindMethodExp>());
            BatchCommand.BatchScript.Register("find_method_in_code", "find_method_in_code(code, language, classNameRegexPattern, methodNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindMethodInCodeExp>());

            // Event Search APIs (formatted output)
            BatchCommand.BatchScript.Register("find_events", "find_events(filePath, language, classNameRegexPattern, [eventNameRegexPattern])", new ExpressionFactoryHelper<FindEventsExp>());
            BatchCommand.BatchScript.Register("find_events_in_code", "find_events_in_code(code, language, classNameRegexPattern, [eventNameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindEventsInCodeExp>());
            BatchCommand.BatchScript.Register("find_event", "find_event(filePath, language, classNameRegexPattern, eventNameRegexPattern)", new ExpressionFactoryHelper<FindEventExp>());
            BatchCommand.BatchScript.Register("find_event_in_code", "find_event_in_code(code, language, classNameRegexPattern, eventNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindEventInCodeExp>());

            // Constructor Search APIs (formatted output)
            BatchCommand.BatchScript.Register("find_constructors", "find_constructors(filePath, language, classNameRegexPattern)", new ExpressionFactoryHelper<FindConstructorsExp>());
            BatchCommand.BatchScript.Register("find_constructors_in_code", "find_constructors_in_code(code, language, classNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindConstructorsInCodeExp>());

            // Interface Search APIs (formatted output)
            BatchCommand.BatchScript.Register("find_interfaces", "find_interfaces(filePath, language, [nameRegexPattern])", new ExpressionFactoryHelper<FindInterfacesExp>());
            BatchCommand.BatchScript.Register("find_interfaces_in_code", "find_interfaces_in_code(code, language, [nameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindInterfacesInCodeExp>());
            BatchCommand.BatchScript.Register("find_interface", "find_interface(filePath, language, interfaceNameRegexPattern)", new ExpressionFactoryHelper<FindInterfaceExp>());
            BatchCommand.BatchScript.Register("find_interface_in_code", "find_interface_in_code(code, language, interfaceNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindInterfaceInCodeExp>());

            // Struct Search APIs (formatted output)
            BatchCommand.BatchScript.Register("find_structs", "find_structs(filePath, language, [nameRegexPattern])", new ExpressionFactoryHelper<FindStructsExp>());
            BatchCommand.BatchScript.Register("find_structs_in_code", "find_structs_in_code(code, language, [nameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindStructsInCodeExp>());
            BatchCommand.BatchScript.Register("find_struct", "find_struct(filePath, language, structNameRegexPattern)", new ExpressionFactoryHelper<FindStructExp>());
            BatchCommand.BatchScript.Register("find_struct_in_code", "find_struct_in_code(code, language, structNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindStructInCodeExp>());

            // Enum Search APIs (formatted output)
            BatchCommand.BatchScript.Register("find_enums", "find_enums(filePath, language, [nameRegexPattern])", new ExpressionFactoryHelper<FindEnumsExp>());
            BatchCommand.BatchScript.Register("find_enums_in_code", "find_enums_in_code(code, language, [nameRegexPattern], [fileName])", new ExpressionFactoryHelper<FindEnumsInCodeExp>());
            BatchCommand.BatchScript.Register("find_enum", "find_enum(filePath, language, enumNameRegexPattern)", new ExpressionFactoryHelper<FindEnumExp>());
            BatchCommand.BatchScript.Register("find_enum_in_code", "find_enum_in_code(code, language, enumNameRegexPattern, [fileName])", new ExpressionFactoryHelper<FindEnumInCodeExp>());

            // AST Node Search APIs (formatted output)
            BatchCommand.BatchScript.Register("find_nodes", "find_nodes(filePath, language, nodeRegexPattern)", new ExpressionFactoryHelper<FindNodesExp>());
            BatchCommand.BatchScript.Register("find_nodes_in_code", "find_nodes_in_code(code, language, nodeRegexPattern, [fileName])", new ExpressionFactoryHelper<FindNodesInCodeExp>());
            BatchCommand.BatchScript.Register("find_node", "find_node(filePath, language, nodeRegexPattern)", new ExpressionFactoryHelper<FindNodeExp>());
            BatchCommand.BatchScript.Register("find_node_in_code", "find_node_in_code(code, language, nodeRegexPattern, [fileName])", new ExpressionFactoryHelper<FindNodeInCodeExp>());

            // Register list-returning variants (each item corresponds to one matching target)
            RegisterAsListApis();
        }

        // Helper: Parse language string (delegates to UnifiedCodeAnalysisApi.ResolveLanguage)
        private static ProgrammingLanguage ParseLanguage(string langStr)
        {
            var lang = UnifiedCodeAnalysisApi.ResolveLanguage(langStr);
            if (lang == null)
                throw new NotSupportedException($"Language '{langStr}' is not supported");
            return lang.Value;
        }

        private static bool MatchesPattern(string name, string pattern)
        {
            try {
                return Regex.IsMatch(name, pattern);
            }
            catch (ArgumentException) {
                return name.Contains(Regex.Escape(pattern));
            }
        }

        // ========== LLM-Friendly Formatting API Implementations ==========

        // View file structure overview
        sealed class ViewFileStructureExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: ViewFileStructure");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    var language = ParseLanguage(langStr);

                    string result = _api.FormatFileStructure(filePath, language);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // View code structure overview
        sealed class ViewCodeStructureExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: ViewCodeStructure");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string fileName = operands.Count > 2 ? operands[2].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FormatCodeStructure(code, language, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find functions (formatted)
        sealed class FindFunctionsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindFunctions");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindFunctions(filePath, language, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find functions in code (formatted)
        sealed class FindFunctionsInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindFunctionsInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindFunctionsInCode(code, language, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find function (formatted)
        sealed class FindFunctionExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindFunction");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string functionName = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindFunction(filePath, language, functionName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find function in code (formatted)
        sealed class FindFunctionInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindFunctionInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string functionName = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindFunctionInCode(code, language, functionName, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find types (formatted)
        sealed class FindTypesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindTypes");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindTypes(filePath, language, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find types in code (formatted)
        sealed class FindTypesInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindTypesInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindTypesInCode(code, language, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find type (formatted)
        sealed class FindTypeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindType");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string typeName = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindType(filePath, language, typeName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find type in code (formatted)
        sealed class FindTypeInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindTypeInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string typeName = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindTypeInCode(code, language, typeName, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }
        // ========== Variable Search APIs () ==========

        // Find variables (formatted)
        public sealed class FindVariablesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindVariables");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindVariables(filePath, language, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find variables in code (formatted)
        public sealed class FindVariablesInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindVariablesInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindVariablesInCode(code, language, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find variable (formatted)
        public sealed class FindVariableExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindVariable");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string variableName = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindVariable(filePath, language, variableName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find variable in code (formatted)
        public sealed class FindVariableInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindVariableInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string variableName = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindVariableInCode(code, language, variableName, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find global variables (formatted)
        public sealed class FindGlobalVariablesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindGlobalVariables");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindGlobalVariables(filePath, language, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find global variables in code (formatted)
        public sealed class FindGlobalVariablesInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindGlobalVariablesInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindGlobalVariablesInCode(code, language, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find parameters (formatted)
        public sealed class FindParametersExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindParameters");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindParameters(filePath, language, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find parameters in code (formatted)
        public sealed class FindParametersInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindParametersInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindParametersInCode(code, language, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // ========== Class Member Search APIs () ==========

        // Find class members (formatted)
        public sealed class FindClassMembersExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindClassMembers");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindClassMembers(filePath, language, className);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find class members in code (formatted)
        public sealed class FindClassMembersInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindClassMembersInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindClassMembersInCode(code, language, className, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find fields (formatted)
        public sealed class FindFieldsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindFields");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindFields(filePath, language, className, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find fields in code (formatted)
        public sealed class FindFieldsInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindFieldsInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindFieldsInCode(code, language, className, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find field (formatted)
        public sealed class FindFieldExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindField");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string fieldName = operands[3].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindField(filePath, language, className, fieldName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find field in code (formatted)
        public sealed class FindFieldInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindFieldInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string fieldName = operands[3].GetString();
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindFieldInCode(code, language, className, fieldName, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find properties (formatted)
        public sealed class FindPropertiesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindProperties");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindProperties(filePath, language, className, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find properties in code (formatted)
        public sealed class FindPropertiesInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindPropertiesInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindPropertiesInCode(code, language, className, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find property (formatted)
        public sealed class FindPropertyExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindProperty");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string propertyName = operands[3].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindProperty(filePath, language, className, propertyName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find property in code (formatted)
        public sealed class FindPropertyInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindPropertyInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string propertyName = operands[3].GetString();
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindPropertyInCode(code, language, className, propertyName, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find methods (formatted)
        public sealed class FindMethodsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindMethods");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindMethods(filePath, language, className, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find methods in code (formatted)
        public sealed class FindMethodsInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindMethodsInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindMethodsInCode(code, language, className, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find method (formatted)
        public sealed class FindMethodExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindMethod");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string methodName = operands[3].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindMethod(filePath, language, className, methodName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find method in code (formatted)
        public sealed class FindMethodInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindMethodInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string methodName = operands[3].GetString();
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindMethodInCode(code, language, className, methodName, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find constructors (formatted)
        public sealed class FindConstructorsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindConstructors");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindConstructors(filePath, language, className);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find constructors in code (formatted)
        public sealed class FindConstructorsInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindConstructorsInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindConstructorsInCode(code, language, className, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find events (formatted)
        public sealed class FindEventsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindEvents");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindEvents(filePath, language, className, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find events in code (formatted)
        public sealed class FindEventsInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindEventsInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindEventsInCode(code, language, className, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find event (formatted)
        public sealed class FindEventExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindEvent");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string eventName = operands[3].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindEvent(filePath, language, className, eventName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find event in code (formatted)
        public sealed class FindEventInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindEventInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string eventName = operands[3].GetString();
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindEventInCode(code, language, className, eventName, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find interfaces (formatted)
        public sealed class FindInterfacesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindInterfaces");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindInterfaces(filePath, language, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find interfaces in code (formatted)
        public sealed class FindInterfacesInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindInterfacesInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindInterfacesInCode(code, language, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find interface (formatted)
        public sealed class FindInterfaceExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindInterface");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string interfaceName = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindInterface(filePath, language, interfaceName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find interface in code (formatted)
        public sealed class FindInterfaceInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindInterfaceInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string interfaceName = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindInterfaceInCode(code, language, interfaceName, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find structs (formatted)
        public sealed class FindStructsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindStructs");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindStructs(filePath, language, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find structs in code (formatted)
        public sealed class FindStructsInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindStructsInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindStructsInCode(code, language, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find struct (formatted)
        public sealed class FindStructExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindStruct");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string structName = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindStruct(filePath, language, structName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find struct in code (formatted)
        public sealed class FindStructInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindStructInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string structName = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindStructInCode(code, language, structName, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find enums (formatted)
        public sealed class FindEnumsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindEnums");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    string result = _api.FindEnums(filePath, language, namePattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find enums in code (formatted)
        public sealed class FindEnumsInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindEnumsInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindEnumsInCode(code, language, namePattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find enum (formatted)
        public sealed class FindEnumExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindEnum");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string enumName = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindEnum(filePath, language, enumName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find enum in code (formatted)
        public sealed class FindEnumInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: FindEnumInCode");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string enumName = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindEnumInCode(code, language, enumName, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // ========== AST Node Search APIs ==========

        // Find all AST nodes matching pattern in file
        public sealed class FindNodesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 3) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: find_nodes(filePath, language, nodeRegexPattern)");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string pattern = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindNodes(filePath, language, pattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find all AST nodes matching pattern in code
        public sealed class FindNodesInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3 || operands.Count > 4) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: find_nodes_in_code(code, language, nodeRegexPattern, [fileName])");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string pattern = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindNodesInCode(code, language, pattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find first AST node matching pattern in file
        public sealed class FindNodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 3) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: find_node(filePath, language, nodeRegexPattern)");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string pattern = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    string result = _api.FindNode(filePath, language, pattern);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }

        // Find first AST node matching pattern in code
        public sealed class FindNodeInCodeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3 || operands.Count > 4) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("Expected: find_node_in_code(code, language, nodeRegexPattern, [fileName])");
                    return BoxedValue.FromString(string.Empty);
                }

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string pattern = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    string result = _api.FindNodeInCode(code, language, pattern, fileName);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromString(string.Empty);
                }
            }
        }
    }
}
