using AgentPlugin.Abstractions;
using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using AgentCore.CodeAnalysis;
using CefDotnetApp.AgentCore.Models;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // List-returning DSL APIs (each item corresponds to one matching target).
    // On error or no result, an empty list is returned.
    public static partial class UnifiedCodeAnalysisScriptApi
    {
        private static readonly List<string> s_EmptyStringList = new List<string>();

        public static void RegisterAsListApis()
        {
            // Function search
            AgentFrameworkService.Instance.DslEngine!.Register("find_functions_as_list", "find_functions_as_list(filePath, language, [nameRegexPattern]) return List of CodeItem(Kind=Function, Name/FilePath/Location/Signature/Type/Modifiers/ParentClass fields, supports LINQ such as .where($$.Name.StartsWith(\"On\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindFunctionsAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_functions_in_code_as_list", "find_functions_in_code_as_list(code, language, [nameRegexPattern], [fileName]) return List of CodeItem(Kind=Function, Name/FilePath/Location/Signature/Type/Modifiers/ParentClass fields, supports LINQ such as .where($$.Name.StartsWith(\"On\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindFunctionsInCodeAsListExp>());

            // Type search
            AgentFrameworkService.Instance.DslEngine!.Register("find_types_as_list", "find_types_as_list(filePath, language, [nameRegexPattern]) return List of CodeItem(Kind=Type, Name/FilePath/Location/Modifiers/Extra(base type info) fields, supports LINQ such as .where($$.Name.EndsWith(\"Manager\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindTypesAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_types_in_code_as_list", "find_types_in_code_as_list(code, language, [nameRegexPattern], [fileName]) return List of CodeItem(Kind=Type, Name/FilePath/Location/Modifiers/Extra(base type info) fields, supports LINQ such as .where($$.Name.EndsWith(\"Manager\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindTypesInCodeAsListExp>());

            // Variable search
            AgentFrameworkService.Instance.DslEngine!.Register("find_variables_as_list", "find_variables_as_list(filePath, language, [nameRegexPattern]) return List of CodeItem(Kind=Variable, Name/FilePath/Location/Type/Modifiers/ParentClass/Scope fields, flattened across all type declarations, supports LINQ such as .where($$.ParentClass == \"Foo\")), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindVariablesAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_variables_in_code_as_list", "find_variables_in_code_as_list(code, language, [nameRegexPattern], [fileName]) return List of CodeItem(Kind=Variable, Name/FilePath/Location/Type/Modifiers/ParentClass/Scope fields, flattened across all type declarations, supports LINQ such as .where($$.ParentClass == \"Foo\")), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindVariablesInCodeAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_global_variables_as_list", "find_global_variables_as_list(filePath, language, [nameRegexPattern]) return List of CodeItem(Kind=GlobalVariable, Name/FilePath/Location/Type/Modifiers/Scope=Global fields, static fields only, supports LINQ such as .where($$.Modifiers.Contains(\"const\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindGlobalVariablesAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_global_variables_in_code_as_list", "find_global_variables_in_code_as_list(code, language, [nameRegexPattern], [fileName]) return List of CodeItem(Kind=GlobalVariable, Name/FilePath/Location/Type/Modifiers/Scope=Global fields, static fields only, supports LINQ such as .where($$.Modifiers.Contains(\"const\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindGlobalVariablesInCodeAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_parameters_as_list", "find_parameters_as_list(filePath, language, [nameRegexPattern]) return List of CodeItem(Kind=Parameter, Name/FilePath/Location/Type/Scope(Function/Method)/ParentClass/Extra(owner function or Type.Method) fields, supports LINQ such as .where($$.Type == \"int\")), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindParametersAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_parameters_in_code_as_list", "find_parameters_in_code_as_list(code, language, [nameRegexPattern], [fileName]) return List of CodeItem(Kind=Parameter, Name/FilePath/Location/Type/Scope(Function/Method)/ParentClass/Extra(owner function or Type.Method) fields, supports LINQ such as .where($$.Type == \"int\")), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindParametersInCodeAsListExp>());

            // Class member search (each item corresponds to one matching class)
            AgentFrameworkService.Instance.DslEngine!.Register("find_class_members_as_list", "find_class_members_as_list(filePath, language, classNameRegexPattern) return List of CodeItem (mixed Kind: Field/Property/Method/Event/Constructor, all with ParentClass set; flattened across matching classes, supports LINQ such as .where($$.Kind == \"Method\")), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindClassMembersAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_class_members_in_code_as_list", "find_class_members_in_code_as_list(code, language, classNameRegexPattern, [fileName]) return List of CodeItem (mixed Kind: Field/Property/Method/Event/Constructor, all with ParentClass set; flattened across matching classes, supports LINQ such as .where($$.Kind == \"Method\")), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindClassMembersInCodeAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_fields_as_list", "find_fields_as_list(filePath, language, classNameRegexPattern, [fieldNameRegexPattern]) return List of CodeItem(Kind=Field, Name/FilePath/Location/Type/Modifiers/ParentClass fields, flattened across matching classes, supports LINQ such as .where($$.Type == \"int\")), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindFieldsAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_fields_in_code_as_list", "find_fields_in_code_as_list(code, language, classNameRegexPattern, [fieldNameRegexPattern], [fileName]) return List of CodeItem(Kind=Field, Name/FilePath/Location/Type/Modifiers/ParentClass fields, flattened across matching classes, supports LINQ such as .where($$.Type == \"int\")), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindFieldsInCodeAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_properties_as_list", "find_properties_as_list(filePath, language, classNameRegexPattern, [propertyNameRegexPattern]) return List of CodeItem(Kind=Property, Name/FilePath/Location/Type/Modifiers/ParentClass/Scope(getter/setter info) fields, supports LINQ such as .where($$.Scope.Contains(\"set\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindPropertiesAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_properties_in_code_as_list", "find_properties_in_code_as_list(code, language, classNameRegexPattern, [propertyNameRegexPattern], [fileName]) return List of CodeItem(Kind=Property, Name/FilePath/Location/Type/Modifiers/ParentClass/Scope(getter/setter info) fields, supports LINQ such as .where($$.Scope.Contains(\"set\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindPropertiesInCodeAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_methods_as_list", "find_methods_as_list(filePath, language, classNameRegexPattern, [methodNameRegexPattern]) return List of CodeItem(Kind=Method, Name/FilePath/Location/Signature/Type(return)/Modifiers/ParentClass fields, supports LINQ such as .where($$.Modifiers.Contains(\"static\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindMethodsAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_methods_in_code_as_list", "find_methods_in_code_as_list(code, language, classNameRegexPattern, [methodNameRegexPattern], [fileName]) return List of CodeItem(Kind=Method, Name/FilePath/Location/Signature/Type(return)/Modifiers/ParentClass fields, supports LINQ such as .where($$.Modifiers.Contains(\"static\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindMethodsInCodeAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_events_as_list", "find_events_as_list(filePath, language, classNameRegexPattern, [eventNameRegexPattern]) return List of CodeItem(Kind=Event, Name/FilePath/Location/Type(handler)/Modifiers/ParentClass fields, C# only, supports LINQ such as .where($$.Name.StartsWith(\"On\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindEventsAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_events_in_code_as_list", "find_events_in_code_as_list(code, language, classNameRegexPattern, [eventNameRegexPattern], [fileName]) return List of CodeItem(Kind=Event, Name/FilePath/Location/Type(handler)/Modifiers/ParentClass fields, C# only, supports LINQ such as .where($$.Name.StartsWith(\"On\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindEventsInCodeAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_constructors_as_list", "find_constructors_as_list(filePath, language, classNameRegexPattern) return List of CodeItem(Kind=Constructor, Name/FilePath/Location/Signature/Modifiers/ParentClass fields, supports LINQ such as .where($$.Signature.Contains(\"()\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindConstructorsAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_constructors_in_code_as_list", "find_constructors_in_code_as_list(code, language, classNameRegexPattern, [fileName]) return List of CodeItem(Kind=Constructor, Name/FilePath/Location/Signature/Modifiers/ParentClass fields, supports LINQ such as .where($$.Signature.Contains(\"()\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindConstructorsInCodeAsListExp>());

            // Interface / Struct / Enum search
            AgentFrameworkService.Instance.DslEngine!.Register("find_interfaces_as_list", "find_interfaces_as_list(filePath, language, [nameRegexPattern]) return List of CodeItem(Kind=Interface, Name/FilePath/Location/Modifiers/Extra(base list) fields, supports LINQ such as .where($$.Name.StartsWith(\"I\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindInterfacesAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_interfaces_in_code_as_list", "find_interfaces_in_code_as_list(code, language, [nameRegexPattern], [fileName]) return List of CodeItem(Kind=Interface, Name/FilePath/Location/Modifiers/Extra(base list) fields, supports LINQ such as .where($$.Name.StartsWith(\"I\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindInterfacesInCodeAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_structs_as_list", "find_structs_as_list(filePath, language, [nameRegexPattern]) return List of CodeItem(Kind=Struct, Name/FilePath/Location/Modifiers/Extra(base list) fields, supports LINQ such as .where($$.Modifiers.Contains(\"readonly\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindStructsAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_structs_in_code_as_list", "find_structs_in_code_as_list(code, language, [nameRegexPattern], [fileName]) return List of CodeItem(Kind=Struct, Name/FilePath/Location/Modifiers/Extra(base list) fields, supports LINQ such as .where($$.Modifiers.Contains(\"readonly\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindStructsInCodeAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_enums_as_list", "find_enums_as_list(filePath, language, [nameRegexPattern]) return List of CodeItem(Kind=Enum, Name/FilePath/Location/Modifiers fields, supports LINQ such as .where($$.Modifiers.Contains(\"public\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindEnumsAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_enums_in_code_as_list", "find_enums_in_code_as_list(code, language, [nameRegexPattern], [fileName]) return List of CodeItem(Kind=Enum, Name/FilePath/Location/Modifiers fields, supports LINQ such as .where($$.Modifiers.Contains(\"public\"))), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindEnumsInCodeAsListExp>());

            // AST node search
            AgentFrameworkService.Instance.DslEngine!.Register("find_nodes_as_list", "find_nodes_as_list(filePath, language, nodeRegexPattern) return List of CodeItem(Kind=ast node type, Name/FilePath/Location(line/column range)/Extra(parent and child count)/Text(preview) fields, supports LINQ such as .where($$.Kind == \"identifier\")), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindNodesAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("find_nodes_in_code_as_list", "find_nodes_in_code_as_list(code, language, nodeRegexPattern, [fileName]) return List of CodeItem(Kind=ast node type, Name/FilePath/Location(line/column range)/Extra(parent and child count)/Text(preview) fields, supports LINQ such as .where($$.Kind == \"identifier\")), use 'to_string' to convert to a string", new ExpressionFactoryHelper<FindNodesInCodeAsListExp>());
        }

        // ========== Function Search (List) ==========
        public sealed class FindFunctionsAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindFunctionsAsList(filePath, language, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindFunctionsInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindFunctionsInCodeAsList(code, language, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        // ========== Type Search (List) ==========
        public sealed class FindTypesAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindTypesAsList(filePath, language, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindTypesInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindTypesInCodeAsList(code, language, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        // ========== Variable Search (List) ==========
        public sealed class FindVariablesAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindVariablesAsList(filePath, language, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindVariablesInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindVariablesInCodeAsList(code, language, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindGlobalVariablesAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindGlobalVariablesAsList(filePath, language, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindGlobalVariablesInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindGlobalVariablesInCodeAsList(code, language, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindParametersAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindParametersAsList(filePath, language, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindParametersInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindParametersInCodeAsList(code, language, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        // ========== Class Member Search (List) ==========
        public sealed class FindClassMembersAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    var result = _api.FindClassMembersAsList(filePath, language, className);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindClassMembersInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindClassMembersInCodeAsList(code, language, className, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindFieldsAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindFieldsAsList(filePath, language, className, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindFieldsInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindFieldsInCodeAsList(code, language, className, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindPropertiesAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindPropertiesAsList(filePath, language, className, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindPropertiesInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindPropertiesInCodeAsList(code, language, className, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindMethodsAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindMethodsAsList(filePath, language, className, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindMethodsInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindMethodsInCodeAsList(code, language, className, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindEventsAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindEventsAsList(filePath, language, className, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindEventsInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string? namePattern = operands.Count > 3 ? operands[3].GetString() : null;
                    string fileName = operands.Count > 4 ? operands[4].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindEventsInCodeAsList(code, language, className, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindConstructorsAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    var result = _api.FindConstructorsAsList(filePath, language, className);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindConstructorsInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string className = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindConstructorsInCodeAsList(code, language, className, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        // ========== Interface / Struct / Enum Search (List) ==========
        public sealed class FindInterfacesAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindInterfacesAsList(filePath, language, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindInterfacesInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindInterfacesInCodeAsList(code, language, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindStructsAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindStructsAsList(filePath, language, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindStructsInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindStructsInCodeAsList(code, language, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindEnumsAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    var language = ParseLanguage(langStr);

                    var result = _api.FindEnumsAsList(filePath, language, namePattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindEnumsInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string? namePattern = operands.Count > 2 ? operands[2].GetString() : null;
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindEnumsInCodeAsList(code, language, namePattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        // ========== AST Node Search (List) ==========
        public sealed class FindNodesAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 3)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string filePath = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string pattern = operands[2].GetString();
                    var language = ParseLanguage(langStr);

                    var result = _api.FindNodesAsList(filePath, language, pattern);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }

        public sealed class FindNodesInCodeAsListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 3 || operands.Count > 4)
                    return BoxedValue.FromObject(s_EmptyStringList);

                try {
                    string code = operands[0].GetString();
                    string langStr = operands[1].GetString().ToLower();
                    string pattern = operands[2].GetString();
                    string fileName = operands.Count > 3 ? operands[3].GetString() : "<text>";
                    var language = ParseLanguage(langStr);

                    var result = _api.FindNodesInCodeAsList(code, language, pattern, fileName);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"code_analysis error: {ex.Message}");
                    return BoxedValue.FromObject(s_EmptyStringList);
                }
            }
        }
    }
}
