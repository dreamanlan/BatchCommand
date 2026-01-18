using System;
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
            BatchCommand.BatchScript.Register("inspect_c_language_dll", "inspect_c_language_dll() => string", new ExpressionFactoryHelper<InspectCLanguageDllExp>());
            BatchCommand.BatchScript.Register("inspect_cpp_language_dll", "inspect_cpp_language_dll() => string", new ExpressionFactoryHelper<InspectCppLanguageDllExp>());
            BatchCommand.BatchScript.Register("explore_treesitter_c_api", "explore_treesitter_c_api() => string", new ExpressionFactoryHelper<ExploreTreeSitterCApiExp>());
            BatchCommand.BatchScript.Register("explore_treesitter_cpp_api", "explore_treesitter_cpp_api() => string", new ExpressionFactoryHelper<ExploreTreeSitterCppApiExp>());
            BatchCommand.BatchScript.Register("test_create_c_parser", "test_create_c_parser() => string", new ExpressionFactoryHelper<TestCreateCParserExp>());
            BatchCommand.BatchScript.Register("test_create_cpp_parser", "test_create_cpp_parser() => string", new ExpressionFactoryHelper<TestCreateCppParserExp>());
            BatchCommand.BatchScript.Register("test_get_node_text", "test_get_node_text() => string", new ExpressionFactoryHelper<TestGetNodeTextExp>());
            BatchCommand.BatchScript.Register("explore_all_treesitter_apis", "explore_all_treesitter_apis() => string", new ExpressionFactoryHelper<ExploreAllTreeSitterApisExp>());
            BatchCommand.BatchScript.Register("save_treesitter_report", "save_treesitter_report(output_path) => bool", new ExpressionFactoryHelper<SaveTreeSitterReportExp>());
            BatchCommand.BatchScript.Register("test_js_properties", "test_js_properties()", new ExpressionFactoryHelper<TestEsprimaPropertiesExp>());

        }
    }

    sealed class InspectCLanguageDllExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var result = TreeSitterLanguageDiagnostics.InspectCLanguageAssembly();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                var error = $"Error inspecting C language DLL: {ex.Message}\n{ex.StackTrace}";
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class InspectCppLanguageDllExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var result = TreeSitterLanguageDiagnostics.InspectCppLanguageAssembly();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                var error = $"Error inspecting C++ language DLL: {ex.Message}\n{ex.StackTrace}";
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class ExploreTreeSitterCApiExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.ExploreTreeSitterCApi();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                var error = $"Error exploring TreeSitterSharp.C API: {ex.Message}\n{ex.StackTrace}";
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class ExploreTreeSitterCppApiExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.ExploreTreeSitterCppApi();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                var error = $"Error exploring TreeSitterSharp.Cpp API: {ex.Message}\n{ex.StackTrace}";
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class TestCreateCParserExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.TestCreateCParser();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                var error = $"Error testing C parser creation: {ex.Message}\n{ex.StackTrace}";
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class TestCreateCppParserExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.TestCreateCppParser();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                var error = $"Error testing C++ parser creation: {ex.Message}\n{ex.StackTrace}";
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class ExploreAllTreeSitterApisExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.ExploreAllTreeSitterApis();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                var error = $"Error exploring all TreeSitter APIs: {ex.Message}\n{ex.StackTrace}";
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class SaveTreeSitterReportExp : SimpleExpressionBase

    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                if (operands.Count < 1)
                {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("Error: save_treesitter_report requires output_path parameter");

                    return BoxedValue.From(false);
                }

                var outputPath = operands[0].AsString;
                if (string.IsNullOrEmpty(outputPath))
                {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine("Error: output_path cannot be empty");
                    return BoxedValue.From(false);
                }

                var report = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.ExploreAllTreeSitterApis();

                // Ensure directory exists
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(outputPath, report);
                return BoxedValue.From(true);
            }
            catch (Exception ex)
            {
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error saving TreeSitter report: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    sealed class TestGetNodeTextExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var result = CefDotnetApp.AgentCore.CodeAnalysis.CodeAnalysisApi.TestGetNodeText();
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                var error = $"Error testing GetNodeText: {ex.Message}\n{ex.StackTrace}";
                DotNetLib.NativeApi.AppendApiErrorInfoFormatLine(error);
                return BoxedValue.FromString(error);
            }
        }
    }

    sealed class TestEsprimaPropertiesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            string result = TestEsprima.TestJavaScriptProperties();
            return BoxedValue.FromString(result);
        }
    }
}
