using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Esprima;
using Esprima.Ast;

namespace CefDotnetApp.AgentCore.CodeAnalysis.JavaScript
{
    /// <summary>
    /// JavaScript code analyzer
    /// Provides advanced analysis features for JavaScript code
    /// </summary>
    public class JavaScriptAnalyzer
    {
        private readonly JintParser _parser;

        public JavaScriptAnalyzer()
        {
            _parser = new JintParser();
        }

        /// <summary>
        /// Analyze JavaScript file and return AST wrapper
        /// </summary>
        public JsTreeWrapper? AnalyzeFile(string filePath, out string error)
        {
            error = string.Empty;
            try
            {
                if (!File.Exists(filePath))
                {
                    error = $"File not found: {filePath}";
                    return null;
                }

                var code = File.ReadAllText(filePath);
                return _parser.Parse(code, out error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Analyze JavaScript file and return AST wrapper (without error info)
        /// </summary>
        public JsTreeWrapper? AnalyzeFile(string filePath)
        {
            return AnalyzeFile(filePath, out _);
        }

        /// <summary>
        /// Analyze JavaScript code and return AST wrapper
        /// </summary>
        public JsTreeWrapper? AnalyzeCode(string code, out string error)
        {
            return _parser.Parse(code, out error);
        }

        /// <summary>
        /// Analyze JavaScript code and return AST wrapper (without error info)
        /// </summary>
        public JsTreeWrapper? AnalyzeCode(string code)
        {
            return _parser.Parse(code);
        }

        /// <summary>
        /// Verify JavaScript code syntax
        /// </summary>
        public bool VerifyCode(string code, out string error)
        {
            return _parser.TryParse(code, out error);
        }

        /// <summary>
        /// Get all functions from JavaScript code
        /// </summary>
        public List<JsFunctionInfo> GetFunctions(string code)
        {
            var tree = _parser.Parse(code);
            return tree?.GetFunctions() ?? new List<JsFunctionInfo>();
        }

        /// <summary>
        /// Get all classes from JavaScript code
        /// </summary>
        public List<JsClassInfo> GetClasses(string code)
        {
            var tree = _parser.Parse(code);
            return tree?.GetClasses() ?? new List<JsClassInfo>();
        }

        /// <summary>
        /// Get all variables from JavaScript code
        /// </summary>
        public List<JsVariableInfo> GetVariables(string code)
        {
            var tree = _parser.Parse(code);
            return tree?.GetVariables() ?? new List<JsVariableInfo>();
        }

        /// <summary>
        /// Get all imports from JavaScript code
        /// </summary>
        public List<JsImportInfo> GetImports(string code)
        {
            var tree = _parser.Parse(code);
            return tree?.GetImports() ?? new List<JsImportInfo>();
        }

        /// <summary>
        /// Find all nodes of a specific type in JavaScript code
        /// </summary>
        public List<Node> FindNodesByType(string code, string nodeType)
        {
            var tree = _parser.Parse(code);
            return tree?.FindNodesByType(nodeType) ?? new List<Node>();
        }

        /// <summary>
        /// Find symbol references in JavaScript code
        /// </summary>
        public List<SymbolReference> FindSymbolReferences(string code, string symbolName)
        {
            var tree = _parser.Parse(code);
            if (tree == null)
                return new List<SymbolReference>();

            var references = new List<SymbolReference>();
            FindSymbolReferencesRecursive(tree.Program, symbolName, references);
            return references;
        }

        private void FindSymbolReferencesRecursive(Node node, string symbolName, List<SymbolReference> references)
        {
            if (node is Identifier id && id.Name == symbolName)
            {
                references.Add(new SymbolReference
                {
                    Name = symbolName,
                    Location = id.Location,
                    Type = "Identifier"
                });
            }

            foreach (var child in node.ChildNodes)
            {
                FindSymbolReferencesRecursive(child, symbolName, references);
            }
        }

        /// <summary>
        /// Extract code metrics from JavaScript code
        /// </summary>
        public CodeMetrics GetCodeMetrics(string code)
        {
            var tree = _parser.Parse(code);
            if (tree == null)
                return new CodeMetrics();

            var metrics = new CodeMetrics
            {
                TotalLines = code.Split('\n').Length,
                FunctionCount = tree.GetFunctions().Count,
                ClassCount = tree.GetClasses().Count,
                VariableCount = tree.GetVariables().Count,
                ImportCount = tree.GetImports().Count
            };

            return metrics;
        }
    }

    /// <summary>
    /// Symbol reference information
    /// </summary>
    public class SymbolReference
    {
        public string Name { get; set; } = string.Empty;
        public Location? Location { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Code metrics information
    /// </summary>
    public class CodeMetrics
    {
        public int TotalLines { get; set; }
        public int FunctionCount { get; set; }
        public int ClassCount { get; set; }
        public int VariableCount { get; set; }
        public int ImportCount { get; set; }
    }
}
