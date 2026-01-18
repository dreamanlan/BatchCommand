using System;
using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native;
using Esprima;
using Esprima.Ast;

namespace CefDotnetApp.AgentCore.CodeAnalysis.JavaScript
{
    /// <summary>
    /// Wrapper for JavaScript AST parsed by Jint/Esprima
    /// Provides unified interface for JavaScript code analysis
    /// </summary>
    public class JsTreeWrapper
    {
        public Esprima.Ast.Program Program { get; }
        public string SourceCode { get; }
        public string Language => "javascript";

        public JsTreeWrapper(Esprima.Ast.Program program, string sourceCode)
        {
            Program = program ?? throw new ArgumentNullException(nameof(program));
            SourceCode = sourceCode ?? string.Empty;
        }

        /// <summary>
        /// Get all function declarations in the code
        /// </summary>
        public List<JsFunctionInfo> GetFunctions()
        {
            var functions = new List<JsFunctionInfo>();
            CollectFunctions(Program, functions);
            return functions;
        }

        /// <summary>
        /// Get all class declarations in the code
        /// </summary>
        public List<JsClassInfo> GetClasses()
        {
            var classes = new List<JsClassInfo>();
            CollectClasses(Program, classes);
            return classes;
        }

        /// <summary>
        /// Get all variable declarations in the code
        /// </summary>
        public List<JsVariableInfo> GetVariables()
        {
            var variables = new List<JsVariableInfo>();
            CollectVariables(Program, variables);
            return variables;
        }

        /// <summary>
        /// Get all import statements in the code
        /// </summary>
        public List<JsImportInfo> GetImports()
        {
            var imports = new List<JsImportInfo>();
            foreach (var node in Program.Body)
            {
                if (node is ImportDeclaration importDecl)
                {
                    imports.Add(new JsImportInfo(importDecl, SourceCode));
                }
            }
            return imports;
        }

        /// <summary>
        /// Find all nodes of a specific type
        /// </summary>
        public List<Node> FindNodesByType(string nodeType)
        {
            var nodes = new List<Node>();
            FindNodesByTypeRecursive(Program, nodeType, nodes);
            return nodes;
        }

        private void CollectFunctions(Node node, List<JsFunctionInfo> functions)
        {
            // Skip MethodDefinition nodes - these are class methods collected by JsClassInfo
            // But continue traversing other nodes (including class declarations)
            if (node is MethodDefinition)
            {
                return;
            }

            // Skip PropertyDefinition nodes - these are class fields collected by JsClassInfo
            // Even if they contain function expressions, they belong to the class
            if (node.Type == Nodes.PropertyDefinition ||
                node.Type.ToString() == "PropertyDefinition" ||
                node.Type.ToString() == "FieldDefinition")
            {
                return;
            }

            if (node is FunctionDeclaration funcDecl)
            {
                functions.Add(new JsFunctionInfo(funcDecl, SourceCode));
            }
            else if (node is FunctionExpression funcExpr)
            {
                functions.Add(new JsFunctionInfo(funcExpr, SourceCode));
            }
            else if (node is ArrowFunctionExpression arrowFunc)
            {
                functions.Add(new JsFunctionInfo(arrowFunc, SourceCode));
            }

            foreach (var child in node.ChildNodes)
            {
                CollectFunctions(child, functions);
            }
        }

        private void CollectClasses(Node node, List<JsClassInfo> classes)
        {
            if (node is ClassDeclaration classDecl)
            {
                classes.Add(new JsClassInfo(classDecl, SourceCode));
            }
            else if (node is ClassExpression classExpr)
            {
                classes.Add(new JsClassInfo(classExpr, SourceCode));
            }

            foreach (var child in node.ChildNodes)
            {
                CollectClasses(child, classes);
            }
        }

        private void CollectVariables(Node node, List<JsVariableInfo> variables)
        {
            if (node is VariableDeclaration varDecl)
            {
                foreach (var declarator in varDecl.Declarations)
                {
                    variables.Add(new JsVariableInfo(declarator, varDecl.Kind, SourceCode));
                }
            }

            foreach (var child in node.ChildNodes)
            {
                CollectVariables(child, variables);
            }
        }

        private void FindNodesByTypeRecursive(Node node, string nodeType, List<Node> nodes)
        {
            if (node.Type.ToString().Equals(nodeType, StringComparison.OrdinalIgnoreCase))
            {
                nodes.Add(node);
            }

            foreach (var child in node.ChildNodes)
            {
                FindNodesByTypeRecursive(child, nodeType, nodes);
            }
        }
    }

    /// <summary>
    /// Information about a JavaScript function
    /// </summary>
    public class JsFunctionInfo
    {
        public string Name { get; }
        public string Type { get; }
        public List<string> Parameters { get; }
        public Location Location { get; }
        public string SourceCode { get; }

        public JsFunctionInfo(FunctionDeclaration funcDecl, string sourceCode)
        {
            Name = funcDecl.Id?.Name ?? "<anonymous>";
            Type = "FunctionDeclaration";
            Parameters = funcDecl.Params.Select(p => GetParameterName(p)).ToList();
            Location = funcDecl.Location;
            SourceCode = sourceCode;
        }

        public JsFunctionInfo(FunctionExpression funcExpr, string sourceCode)
        {
            Name = funcExpr.Id?.Name ?? "<anonymous>";
            Type = "FunctionExpression";
            Parameters = funcExpr.Params.Select(p => GetParameterName(p)).ToList();
            Location = funcExpr.Location;
            SourceCode = sourceCode;
        }

        public JsFunctionInfo(ArrowFunctionExpression arrowFunc, string sourceCode)
        {
            Name = "<arrow>";
            Type = "ArrowFunctionExpression";
            Parameters = arrowFunc.Params.Select(p => GetParameterName(p)).ToList();
            Location = arrowFunc.Location;
            SourceCode = sourceCode;
        }

        private string GetParameterName(Node param)
        {
            if (param is Identifier id)
                return id.Name;
            if (param is RestElement rest && rest.Argument is Identifier restId)
                return "..." + restId.Name;
            return param.Type.ToString();
        }

        public string GetText()
        {
            if (Location == null || string.IsNullOrEmpty(SourceCode))
                return string.Empty;

            var lines = SourceCode.Split('\n');
            var startLine = Location.Start.Line - 1;
            var endLine = Location.End.Line - 1;

            if (startLine < 0 || endLine >= lines.Length)
                return string.Empty;

            if (startLine == endLine)
            {
                var line = lines[startLine];
                var start = Location.Start.Column;
                var end = Location.End.Column;
                if (start >= 0 && end <= line.Length)
                    return line.Substring(start, end - start);
            }
            else
            {
                var result = new List<string>();
                for (int i = startLine; i <= endLine; i++)
                {
                    if (i == startLine)
                        result.Add(lines[i].Substring(Location.Start.Column));
                    else if (i == endLine)
                        result.Add(lines[i].Substring(0, Location.End.Column));
                    else
                        result.Add(lines[i]);
                }
                return string.Join("\n", result);
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Information about a JavaScript class field
    /// </summary>
    public class JsFieldInfo
    {
        public string Name { get; }
        public Location Location { get; }
        public bool IsStatic { get; }
        public bool HasInitializer { get; }

        public JsFieldInfo(Node fieldNode, string sourceCode)
        {
            // Try to handle PropertyDefinition if available in Esprima
            // For now, extract basic information from the node
            Name = GetFieldName(fieldNode);
            Location = fieldNode.Location;
            IsStatic = GetIsStatic(fieldNode);
            HasInitializer = GetHasInitializer(fieldNode);
        }

        private string GetFieldName(Node fieldNode)
        {
            // Use reflection to get the Key property if it exists
            var keyProp = fieldNode.GetType().GetProperty("Key");
            if (keyProp != null)
            {
                var key = keyProp.GetValue(fieldNode);
                if (key is Identifier id)
                    return id.Name;
                if (key is Literal lit)
                    return lit.Raw ?? string.Empty;
            }
            return "<unknown>";
        }

        private bool GetIsStatic(Node fieldNode)
        {
            var staticProp = fieldNode.GetType().GetProperty("Static");
            if (staticProp != null && staticProp.PropertyType == typeof(bool))
            {
                return (bool)(staticProp.GetValue(fieldNode) ?? false);
            }
            return false;
        }

        private bool GetHasInitializer(Node fieldNode)
        {
            var valueProp = fieldNode.GetType().GetProperty("Value");
            return valueProp != null && valueProp.GetValue(fieldNode) != null;
        }
    }

    /// <summary>
    /// Information about a JavaScript class method
    /// </summary>
    public class JsMethodInfo
    {
        public string Name { get; }
        public List<string> Parameters { get; }
        public Location Location { get; }
        public string Kind { get; }

        public JsMethodInfo(MethodDefinition method, string sourceCode)
        {
            Name = GetMethodName(method);
            Kind = method.Kind.ToString().ToLowerInvariant();
            Location = method.Location;
            Parameters = new List<string>();

            if (method.Value is IFunction func)
            {
                Parameters = func.Params.Select(p => GetParameterName(p)).ToList();
            }
        }

        private string GetMethodName(MethodDefinition method)
        {
            if (method.Key is Identifier id)
                return id.Name;
            if (method.Key is Literal lit)
                return lit.Raw ?? string.Empty;
            return method.Key.Type.ToString();
        }

        private string GetParameterName(Node param)
        {
            if (param is Identifier id)
                return id.Name;
            if (param is RestElement rest && rest.Argument is Identifier restId)
                return "..." + restId.Name;
            return param.Type.ToString();
        }
    }

    /// <summary>
    /// Information about a JavaScript class
    /// </summary>
    public class JsClassInfo
    {
        public string Name { get; }
        public string Type { get; }
        public string SuperClass { get; }
        public List<string> Methods { get; }
        public List<JsMethodInfo> MethodDetails { get; }
        public List<string> Fields { get; }
        public List<JsFieldInfo> FieldDetails { get; }
        public Location Location { get; }
        public string SourceCode { get; }

        public JsClassInfo(ClassDeclaration classDecl, string sourceCode)
        {
            Name = classDecl.Id?.Name ?? "<anonymous>";
            Type = "ClassDeclaration";
            SuperClass = GetSuperClassName(classDecl.SuperClass);
            var methodDefs = classDecl.Body.Body.OfType<MethodDefinition>().ToList();
            Methods = methodDefs.Select(m => GetMethodName(m)).ToList();
            MethodDetails = methodDefs.Select(m => new JsMethodInfo(m, sourceCode)).ToList();

            // Collect class fields (PropertyDefinition nodes)
            var fieldDefs = CollectFields(classDecl.Body.Body);
            Fields = fieldDefs.Select(f => GetFieldName(f)).ToList();
            FieldDetails = fieldDefs.Select(f => new JsFieldInfo(f, sourceCode)).ToList();

            Location = classDecl.Location;
            SourceCode = sourceCode;
        }

        public JsClassInfo(ClassExpression classExpr, string sourceCode)
        {
            Name = classExpr.Id?.Name ?? "<anonymous>";
            Type = "ClassExpression";
            SuperClass = GetSuperClassName(classExpr.SuperClass);
            var methodDefs = classExpr.Body.Body.OfType<MethodDefinition>().ToList();
            Methods = methodDefs.Select(m => GetMethodName(m)).ToList();
            MethodDetails = methodDefs.Select(m => new JsMethodInfo(m, sourceCode)).ToList();

            // Collect class fields (PropertyDefinition nodes)
            var fieldDefs = CollectFields(classExpr.Body.Body);
            Fields = fieldDefs.Select(f => GetFieldName(f)).ToList();
            FieldDetails = fieldDefs.Select(f => new JsFieldInfo(f, sourceCode)).ToList();

            Location = classExpr.Location;
            SourceCode = sourceCode;
        }

        private string GetSuperClassName(Expression? superClass)
        {
            if (superClass is Identifier id)
                return id.Name;
            return string.Empty;
        }

        private string GetMethodName(MethodDefinition method)
        {
            if (method.Key is Identifier id)
                return id.Name;
            if (method.Key is Literal lit)
                return lit.Raw ?? string.Empty;
            return method.Key.Type.ToString();
        }

        private List<Node> CollectFields(NodeList<ClassElement> classBodyNodes)
        {
            var fields = new List<Node>();
            foreach (var node in classBodyNodes)
            {
                // Check if this is a PropertyDefinition node (class field)
                // Esprima may represent it with Type = "PropertyDefinition" or "FieldDefinition"
                if (node.Type == Nodes.PropertyDefinition ||
                    node.Type.ToString() == "PropertyDefinition" ||
                    node.Type.ToString() == "FieldDefinition")
                {
                    fields.Add(node);
                }
            }
            return fields;
        }

        private string GetFieldName(Node fieldNode)
        {
            // Use reflection to get the Key property
            var keyProp = fieldNode.GetType().GetProperty("Key");
            if (keyProp != null)
            {
                var key = keyProp.GetValue(fieldNode);
                if (key is Identifier id)
                    return id.Name;
                if (key is Literal lit)
                    return lit.Raw ?? string.Empty;
            }
            return "<unknown>";
        }
    }

    /// <summary>
    /// Information about a JavaScript variable
    /// </summary>
    public class JsVariableInfo
    {
        public string Name { get; }
        public string Kind { get; }
        public Location Location { get; }
        public string SourceCode { get; }

        public JsVariableInfo(VariableDeclarator declarator, VariableDeclarationKind kind, string sourceCode)
        {
            Name = (declarator.Id as Identifier)?.Name ?? "<unknown>";
            Kind = kind.ToString().ToLower();
            Location = declarator.Location;
            SourceCode = sourceCode;
        }
    }

    /// <summary>
    /// Information about a JavaScript import statement
    /// </summary>
    public class JsImportInfo
    {
        public string Source { get; }
        public List<string> Specifiers { get; }
        public Location Location { get; }
        public string SourceCode { get; }

        public JsImportInfo(ImportDeclaration importDecl, string sourceCode)
        {
            Source = (importDecl.Source as Literal)?.StringValue ?? string.Empty;
            Specifiers = importDecl.Specifiers.Select(s => GetSpecifierName(s)).ToList();
            Location = importDecl.Location;
            SourceCode = sourceCode;
        }

        private string GetSpecifierName(ImportDeclarationSpecifier specifier)
        {
            if (specifier is ImportDefaultSpecifier defaultSpec)
                return defaultSpec.Local.Name;
            if (specifier is ImportNamespaceSpecifier namespaceSpec)
                return "* as " + namespaceSpec.Local.Name;
            if (specifier is ImportSpecifier importSpec)
                return importSpec.Local.Name;
            return specifier.Type.ToString();
        }
    }
}
