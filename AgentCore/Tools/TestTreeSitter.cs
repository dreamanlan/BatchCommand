using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeSitter;
using AgentCore.CodeAnalysis.TreeSitter.Adapters;

namespace CefDotnetApp.AgentCore.Tools
{
    /// <summary>
    /// TreeSitter API testing and exploration utilities using TreeSitter.DotNet
    /// </summary>
    public class TestTreeSitter
    {
        /// <summary>
        /// Explore TreeSitter.DotNet C language API
        /// </summary>
        public static string ExploreTreeSitterCApi()
        {
            var result = new StringBuilder();
            result.AppendLine("=== TreeSitter.DotNet C Language API ===");
            result.AppendLine();

            try
            {
                var lang = new Language("c");
                result.AppendLine($"Language: {lang.Name}");
                result.AppendLine($"ABI Version: {lang.AbiVersion}");
                result.AppendLine($"State Count: {lang.StateCount}");
                result.AppendLine();

                // Test parsing
                using var parser = new Parser(lang);
                string code = "int add(int a, int b) { return a + b; }\nvoid hello() { printf(\"hello\"); }";
                using var tree = parser.Parse(code);
                var root = tree?.RootNode;

                result.AppendLine($"Parsed: root type={root?.Type}, children={root?.Children.Count()}");
                foreach (var child in root?.NamedChildren ?? Enumerable.Empty<Node>())
                {
                    result.AppendLine($"  {child.Type}: {child.GetChildForField("name")?.Text ?? child.Text.Substring(0, Math.Min(50, child.Text.Length))}");
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"Error: {ex.Message}");
            }

            return result.ToString();
        }

        /// <summary>
        /// Explore TreeSitter.DotNet C++ language API
        /// </summary>
        public static string ExploreTreeSitterCppApi()
        {
            var result = new StringBuilder();
            result.AppendLine("=== TreeSitter.DotNet C++ Language API ===");
            result.AppendLine();

            try
            {
                var lang = new Language("cpp");
                result.AppendLine($"Language: {lang.Name}");
                result.AppendLine($"ABI Version: {lang.AbiVersion}");
                result.AppendLine($"State Count: {lang.StateCount}");
                result.AppendLine();

                // Test parsing
                using var parser = new Parser(lang);
                string code = "class Foo { public: int bar(int x) { return x * 2; } };";
                using var tree = parser.Parse(code);
                var root = tree?.RootNode;

                result.AppendLine($"Parsed: root type={root?.Type}, children={root?.Children.Count()}");
                foreach (var child in root?.NamedChildren ?? Enumerable.Empty<Node>())
                {
                    result.AppendLine($"  {child.Type}: {child.Text.Substring(0, Math.Min(60, child.Text.Length))}...");
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"Error: {ex.Message}");
            }

            return result.ToString();
        }

        /// <summary>
        /// Explore all TreeSitter.DotNet supported languages
        /// </summary>
        public static string ExploreAllTreeSitterApis()
        {
            var result = new StringBuilder();
            result.AppendLine("=== TreeSitter.DotNet Complete API Report ===");
            result.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            result.AppendLine();

            // List all supported languages
            result.AppendLine("--- Supported Languages ---");
            var languages = DotNetParserAdapter.SupportedLanguages.OrderBy(l => l).ToList();
            result.AppendLine($"Total aliases: {languages.Count}");
            result.AppendLine();

            // Test each unique language
            var tested = new HashSet<string>();
            foreach (var lang in languages)
            {
                if (tested.Contains(lang))
                    continue;
                tested.Add(lang);

                try
                {
                    var adapter = new DotNetParserAdapter(lang);
                    result.AppendLine($"  [{lang}] -> '{adapter.Language}': OK");
                }
                catch (Exception ex)
                {
                    result.AppendLine($"  [{lang}]: FAILED - {ex.Message}");
                }
            }

            result.AppendLine();

            // Individual language reports
            result.AppendLine(ExploreTreeSitterCApi());
            result.AppendLine(ExploreTreeSitterCppApi());

            return result.ToString();
        }

        /// <summary>
        /// Test creating a C parser via TreeSitter.DotNet
        /// </summary>
        public static string TestCreateCParser()
        {
            var result = new StringBuilder();
            result.AppendLine("=== Test: Create C Parser (TreeSitter.DotNet) ===");

            try
            {
                var adapter = new DotNetParserAdapter("c");
                result.AppendLine($"Parser created for language: {adapter.Language}");

                var tree = adapter.Parse("int main() { return 0; }");
                result.AppendLine($"Parse successful, root type: {tree.RootNode.Type}");
                result.AppendLine($"Root children: {tree.RootNode.ChildCount}");
            }
            catch (Exception ex)
            {
                result.AppendLine($"Error: {ex.Message}");
                result.AppendLine($"Stack: {ex.StackTrace}");
            }

            return result.ToString();
        }

        /// <summary>
        /// Test creating a C++ parser via TreeSitter.DotNet
        /// </summary>
        public static string TestCreateCppParser()
        {
            var result = new StringBuilder();
            result.AppendLine("=== Test: Create C++ Parser (TreeSitter.DotNet) ===");

            try
            {
                var adapter = new DotNetParserAdapter("cpp");
                result.AppendLine($"Parser created for language: {adapter.Language}");

                var tree = adapter.Parse("class Foo { void bar(); };");
                result.AppendLine($"Parse successful, root type: {tree.RootNode.Type}");
                result.AppendLine($"Root children: {tree.RootNode.ChildCount}");
            }
            catch (Exception ex)
            {
                result.AppendLine($"Error: {ex.Message}");
                result.AppendLine($"Stack: {ex.StackTrace}");
            }

            return result.ToString();
        }

        /// <summary>
        /// Test GetNodeText via TreeSitter.DotNet adapter
        /// </summary>
        public static string TestGetNodeText()
        {
            var result = new StringBuilder();
            result.AppendLine("=== Test: GetNodeText (TreeSitter.DotNet) ===");

            try
            {
                var adapter = new DotNetParserAdapter("c");
                string code = "int add(int a, int b) { return a + b; }";
                var tree = adapter.Parse(code);
                var root = tree.RootNode;

                result.AppendLine($"Source: {code}");
                result.AppendLine($"Root type: {root.Type}");
                result.AppendLine($"Root text: {root.GetText(code)}");
                result.AppendLine();

                // Traverse and print node texts
                foreach (var child in root.Children)
                {
                    result.AppendLine($"  {child.Type}: \"{child.GetText(code)}\"");
                    foreach (var grandChild in child.Children)
                    {
                        result.AppendLine($"    {grandChild.Type}: \"{grandChild.GetText(code)}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"Error: {ex.Message}");
                result.AppendLine($"Stack: {ex.StackTrace}");
            }

            return result.ToString();
        }
    }
}
