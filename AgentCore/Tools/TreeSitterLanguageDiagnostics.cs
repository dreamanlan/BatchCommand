using System;
using System.Linq;
using System.Text;
using TreeSitter;
using AgentCore.CodeAnalysis.TreeSitter.Adapters;

namespace AgentCore.Tools
{
    /// <summary>
    /// Diagnostic tool to inspect TreeSitter.DotNet language support
    /// </summary>
    public static class TreeSitterLanguageDiagnostics
    {
        /// <summary>
        /// Inspect C language support via TreeSitter.DotNet
        /// </summary>
        public static string InspectCLanguageAssembly()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== TreeSitter.DotNet C Language Inspection ===");
            sb.AppendLine();

            try
            {
                var lang = new Language("c");
                sb.AppendLine($"Language: {lang.Name}");
                sb.AppendLine($"ABI Version: {lang.AbiVersion}");
                sb.AppendLine($"State Count: {lang.StateCount}");
                sb.AppendLine();

                // Test parse
                using var parser = new Parser(lang);
                using var tree = parser.Parse("int main() { return 0; }");
                sb.AppendLine($"Test parse successful, root type: {tree?.RootNode.Type ?? "null"}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error: {ex.Message}");
                sb.AppendLine($"Stack: {ex.StackTrace}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Inspect C++ language support via TreeSitter.DotNet
        /// </summary>
        public static string InspectCppLanguageAssembly()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== TreeSitter.DotNet C++ Language Inspection ===");
            sb.AppendLine();

            try
            {
                var lang = new Language("cpp");
                sb.AppendLine($"Language: {lang.Name}");
                sb.AppendLine($"ABI Version: {lang.AbiVersion}");
                sb.AppendLine($"State Count: {lang.StateCount}");
                sb.AppendLine();

                // Test parse
                using var parser = new Parser(lang);
                using var tree = parser.Parse("class Foo { public: void bar(); };");
                sb.AppendLine($"Test parse successful, root type: {tree?.RootNode.Type ?? "null"}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error: {ex.Message}");
                sb.AppendLine($"Stack: {ex.StackTrace}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// List all supported languages by TreeSitter.DotNet
        /// </summary>
        public static string ListSupportedLanguages()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== TreeSitter.DotNet Supported Languages ===");
            sb.AppendLine();

            foreach (var lang in DotNetParserAdapter.SupportedLanguages.OrderBy(l => l))
            {
                bool supported = DotNetParserAdapter.IsLanguageSupported(lang);
                sb.AppendLine($"  {lang}: {(supported ? "OK" : "NOT AVAILABLE")}");
            }

            return sb.ToString();
        }
    }
}
