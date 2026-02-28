using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using System.IO;
using AgentCore.Tools;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// Script API for exploring TreeSitter library APIs
    /// </summary>
    public static class TreeSitterExplorerApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("inspect_c_language_dll", "inspect_c_language_dll() => string", new ExpressionFactoryHelper<InspectCLanguageDllExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("inspect_cpp_language_dll", "inspect_cpp_language_dll() => string", new ExpressionFactoryHelper<InspectCppLanguageDllExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("explore_treesitter_c_api", "explore_treesitter_c_api() => string", new ExpressionFactoryHelper<ExploreTreeSitterCApiExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("explore_treesitter_cpp_api", "explore_treesitter_cpp_api() => string", new ExpressionFactoryHelper<ExploreTreeSitterCppApiExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("test_create_c_parser", "test_create_c_parser() => string", new ExpressionFactoryHelper<TestCreateCParserExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("test_create_cpp_parser", "test_create_cpp_parser() => string", new ExpressionFactoryHelper<TestCreateCppParserExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("test_get_node_text", "test_get_node_text() => string", new ExpressionFactoryHelper<TestGetNodeTextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("explore_all_treesitter_apis", "explore_all_treesitter_apis() => string", new ExpressionFactoryHelper<ExploreAllTreeSitterApisExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("save_treesitter_report", "save_treesitter_report(output_path) => bool", new ExpressionFactoryHelper<SaveTreeSitterReportExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("test_js_properties", "test_js_properties()", new ExpressionFactoryHelper<TestEsprimaPropertiesExp>());

        }
    }

    sealed class InspectCLanguageDllExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: inspect_c_language_dll() => string");
                return BoxedValue.NullObject;
            }
            try {
                var result = TreeSitterLanguageDiagnostics.InspectCLanguageAssembly();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                var error = $"Error inspecting C language DLL: {ex.Message}\n{ex.StackTrace}";
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class InspectCppLanguageDllExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: inspect_cpp_language_dll() => string");
                return BoxedValue.NullObject;
            }
            try {
                var result = TreeSitterLanguageDiagnostics.InspectCppLanguageAssembly();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                var error = $"Error inspecting C++ language DLL: {ex.Message}\n{ex.StackTrace}";
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class ExploreTreeSitterCApiExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: explore_treesitter_c_api() => string");
                return BoxedValue.NullObject;
            }
            try {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.ExploreTreeSitterCApi();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                var error = $"Error exploring TreeSitter.DotNet C API: {ex.Message}\n{ex.StackTrace}";
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class ExploreTreeSitterCppApiExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: explore_treesitter_cpp_api() => string");
                return BoxedValue.NullObject;
            }
            try {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.ExploreTreeSitterCppApi();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                var error = $"Error exploring TreeSitter.DotNet C++ API: {ex.Message}\n{ex.StackTrace}";
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class TestCreateCParserExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: test_create_c_parser() => string");
                return BoxedValue.NullObject;
            }
            try {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.TestCreateCParser();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                var error = $"Error testing C parser creation: {ex.Message}\n{ex.StackTrace}";
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class TestCreateCppParserExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: test_create_cpp_parser() => string");
                return BoxedValue.NullObject;
            }
            try {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.TestCreateCppParser();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                var error = $"Error testing C++ parser creation: {ex.Message}\n{ex.StackTrace}";
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class ExploreAllTreeSitterApisExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: explore_all_treesitter_apis() => string");
                return BoxedValue.NullObject;
            }
            try {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.ExploreAllTreeSitterApis();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                var error = $"Error exploring all TreeSitter APIs: {ex.Message}\n{ex.StackTrace}";
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class SaveTreeSitterReportExp : SimpleExpressionBase

    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: save_treesitter_report(output_path) => bool");

                    return BoxedValue.From(false);
                }

                var outputPath = operands[0].AsString;
                if (string.IsNullOrEmpty(outputPath)) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Error: output_path cannot be empty");
                    return BoxedValue.From(false);
                }

                var report = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.ExploreAllTreeSitterApis();

                // Ensure directory exists
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(outputPath, report);
                return BoxedValue.From(true);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"Error saving TreeSitter report: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    sealed class TestGetNodeTextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: test_get_node_text() => string");
                return BoxedValue.NullObject;
            }
            try {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.TestGetNodeText();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex) {
                var error = $"Error testing GetNodeText: {ex.Message}\n{ex.StackTrace}";
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class TestEsprimaPropertiesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 0) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: test_js_properties()");
                return BoxedValue.NullObject;
            }
            string result = TestEsprima.TestJavaScriptProperties();
            return BoxedValue.FromString(result);
        }
    }
}
