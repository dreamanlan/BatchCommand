using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgentCore.CodeAnalysis;

namespace CefDotnetApp.AgentCore.Tools
{
    // TreeSitter API testing and exploration utilities
    public class TestTreeSitter
    {
        // Helper method to safely get type name without triggering infinite recursion
        private static string GetSafeTypeName(Type type)
        {
            try {
                if (type == null) return "null";

                // Avoid accessing complex properties that might trigger loading
                try {
                    var name = type.Name;
                    return name ?? "Unknown";
                }
                catch {
                    return "Error";
                }
            }
            catch {
                return "Error";
            }
        }

        // Helper method to safely get parameter list
        private static string GetSafeParameterList(System.Reflection.ParameterInfo[] parameters)
        {
            try {
                if (parameters == null || parameters.Length == 0) {
                    return "";
                }

                var paramList = new List<string>();
                foreach (var p in parameters) {
                    try {
                        string typeName = "Unknown";
                        string paramName = "param";

                        try {
                            typeName = GetSafeTypeName(p.ParameterType);
                        }
                        catch {
                            typeName = "Error";
                        }

                        try {
                            paramName = p.Name ?? "param";
                        }
                        catch {
                            paramName = "param";
                        }

                        paramList.Add($"{typeName} {paramName}");
                    }
                    catch {
                        paramList.Add("Error param");
                    }
                }
                return string.Join(", ", paramList);
            }
            catch {
                return "Error";
            }
        }

        /// <summary>
        /// Explore TreeSitterSharp.C assembly types and methods
        /// </summary>
        public static string ExploreTreeSitterCApi()
        {
            try {
                var result = new StringBuilder();
                result.AppendLine("=== TreeSitterSharp.C API Exploration ===");
                result.AppendLine();

                // Load the assembly by finding it in loaded assemblies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                System.Reflection.Assembly assembly = null;

                foreach (var asm in assemblies) {
                    if (asm.GetName().Name == "TreeSitterSharp.C") {
                        assembly = asm;
                        break;
                    }
                }

                if (assembly == null) {
                    // Try to load it
                    try {
                        assembly = System.Reflection.Assembly.Load("TreeSitterSharp.C");
                    }
                    catch {
                        return "ERROR: TreeSitterSharp.C assembly not found or could not be loaded";
                    }
                }

                result.AppendLine($"Assembly: {assembly.FullName}");
                result.AppendLine();

                // Get all public types
                var types = assembly.GetExportedTypes();
                result.AppendLine($"Total exported types: {types.Length}");
                result.AppendLine();

                // Limit to first 10 types to avoid hanging
                int maxTypes = Math.Min(types.Length, 10);
                result.AppendLine($"Processing first {maxTypes} types...");
                result.AppendLine();

                for (int i = 0; i < maxTypes; i++) {
                    var type = types[i];
                    try {
                        result.AppendLine($"Type: {type.FullName}");
                        result.AppendLine($"  IsClass: {type.IsClass}, IsInterface: {type.IsInterface}, IsAbstract: {type.IsAbstract}");

                        // Static properties
                        try {
                            var staticProps = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                            if (staticProps.Length > 0) {
                                result.AppendLine("  Static Properties:");
                                foreach (var prop in staticProps) {
                                    try {
                                        var typeName = GetSafeTypeName(prop.PropertyType);
                                        result.AppendLine($"    - {typeName} {prop.Name}");
                                    }
                                    catch (Exception ex) {
                                        result.AppendLine($"    - [Error: {ex.Message}] {prop.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            result.AppendLine($"  Static Properties: [Error: {ex.Message}]");
                        }

                        // Static methods
                        try {
                            var staticMethods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly);
                            if (staticMethods.Length > 0) {
                                result.AppendLine("  Static Methods:");
                                foreach (var method in staticMethods) {
                                    try {
                                        var parameters = GetSafeParameterList(method.GetParameters());
                                        var returnType = GetSafeTypeName(method.ReturnType);
                                        result.AppendLine($"    - {returnType} {method.Name}({parameters})");
                                    }
                                    catch (Exception ex) {
                                        result.AppendLine($"    - [Error: {ex.Message}] {method.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            result.AppendLine($"  Static Methods: [Error: {ex.Message}]");
                        }

                        // Instance properties
                        try {
                            var instanceProps = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (instanceProps.Length > 0) {
                                result.AppendLine("  Instance Properties:");
                                foreach (var prop in instanceProps) {
                                    try {
                                        var typeName = GetSafeTypeName(prop.PropertyType);
                                        result.AppendLine($"    - {typeName} {prop.Name}");
                                    }
                                    catch (Exception ex) {
                                        result.AppendLine($"    - [Error: {ex.Message}] {prop.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            result.AppendLine($"  Instance Properties: [Error: {ex.Message}]");
                        }

                        // Instance methods
                        try {
                            var instanceMethods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
                            if (instanceMethods.Length > 0) {
                                result.AppendLine("  Instance Methods:");
                                foreach (var method in instanceMethods) {
                                    try {
                                        var parameters = GetSafeParameterList(method.GetParameters());
                                        var returnType = GetSafeTypeName(method.ReturnType);
                                        result.AppendLine($"    - {returnType} {method.Name}({parameters})");
                                    }
                                    catch (Exception ex) {
                                        result.AppendLine($"    - [Error: {ex.Message}] {method.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            result.AppendLine($"  Instance Methods: [Error: {ex.Message}]");
                        }

                        // Constructors
                        try {
                            var ctors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (ctors.Length > 0) {
                                result.AppendLine("  Constructors:");
                                foreach (var ctor in ctors) {
                                    try {
                                        var parameters = GetSafeParameterList(ctor.GetParameters());
                                        result.AppendLine($"    - {type.Name}({parameters})");
                                    }
                                    catch (Exception ex) {
                                        result.AppendLine($"    - [Error: {ex.Message}] {type.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            result.AppendLine($"  Constructors: [Error: {ex.Message}]");
                        }

                        result.AppendLine();
                    }
                    catch (Exception ex) {
                        result.AppendLine($"Type {type.FullName}: [Error: {ex.Message}]");
                        result.AppendLine();
                    }
                }

                return result.ToString();
            }
            catch (Exception ex) {
                return $"Error exploring TreeSitterSharp.C API: {ex.Message}\nStack: {ex.StackTrace}";
            }
        }

        /// <summary>
        /// Explore TreeSitterSharp.Cpp assembly types and methods
        /// </summary>
        public static string ExploreTreeSitterCppApi()
        {
            try {
                var result = new StringBuilder();
                result.AppendLine("=== TreeSitterSharp.Cpp API Exploration ===");
                result.AppendLine();

                // Load the assembly by finding it in loaded assemblies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                System.Reflection.Assembly assembly = null;

                foreach (var asm in assemblies) {
                    if (asm.GetName().Name == "TreeSitterSharp.Cpp") {
                        assembly = asm;
                        break;
                    }
                }

                if (assembly == null) {
                    // Try to load it
                    try {
                        assembly = System.Reflection.Assembly.Load("TreeSitterSharp.Cpp");
                    }
                    catch {
                        return "ERROR: TreeSitterSharp.Cpp assembly not found or could not be loaded";
                    }
                }
                result.AppendLine($"Assembly: {assembly.FullName}");
                result.AppendLine();

                // Get all public types
                var types = assembly.GetExportedTypes();
                result.AppendLine($"Total exported types: {types.Length}");
                result.AppendLine();

                // Limit to first 10 types to avoid hanging
                int maxTypes = Math.Min(types.Length, 10);
                result.AppendLine($"Processing first {maxTypes} types...");
                result.AppendLine();

                for (int i = 0; i < maxTypes; i++) {
                    var type = types[i];
                    try {
                        result.AppendLine($"Type: {type.FullName}");
                        result.AppendLine($"  IsClass: {type.IsClass}, IsInterface: {type.IsInterface}, IsAbstract: {type.IsAbstract}");

                        // Static properties
                        try {
                            var staticProps = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                            if (staticProps.Length > 0) {
                                result.AppendLine("  Static Properties:");
                                foreach (var prop in staticProps) {
                                    try {
                                        var typeName = GetSafeTypeName(prop.PropertyType);
                                        result.AppendLine($"    - {typeName} {prop.Name}");
                                    }
                                    catch (Exception ex) {
                                        result.AppendLine($"    - [Error: {ex.Message}] {prop.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            result.AppendLine($"  Static Properties: [Error: {ex.Message}]");
                        }

                        // Static methods
                        try {
                            var staticMethods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly);
                            if (staticMethods.Length > 0) {
                                result.AppendLine("  Static Methods:");
                                foreach (var method in staticMethods) {
                                    try {
                                        var parameters = GetSafeParameterList(method.GetParameters());
                                        var returnType = GetSafeTypeName(method.ReturnType);
                                        result.AppendLine($"    - {returnType} {method.Name}({parameters})");
                                    }
                                    catch (Exception ex) {
                                        result.AppendLine($"    - [Error: {ex.Message}] {method.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            result.AppendLine($"  Static Methods: [Error: {ex.Message}]");
                        }

                        // Instance properties
                        try {
                            var instanceProps = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (instanceProps.Length > 0) {
                                result.AppendLine("  Instance Properties:");
                                foreach (var prop in instanceProps) {
                                    try {
                                        var typeName = GetSafeTypeName(prop.PropertyType);
                                        result.AppendLine($"    - {typeName} {prop.Name}");
                                    }
                                    catch (Exception ex) {
                                        result.AppendLine($"    - [Error: {ex.Message}] {prop.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            result.AppendLine($"  Instance Properties: [Error: {ex.Message}]");
                        }

                        // Instance methods
                        try {
                            var instanceMethods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
                            if (instanceMethods.Length > 0) {
                                result.AppendLine("  Instance Methods:");
                                foreach (var method in instanceMethods) {
                                    try {
                                        var parameters = GetSafeParameterList(method.GetParameters());
                                        var returnType = GetSafeTypeName(method.ReturnType);
                                        result.AppendLine($"    - {returnType} {method.Name}({parameters})");
                                    }
                                    catch (Exception ex) {
                                        result.AppendLine($"    - [Error: {ex.Message}] {method.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            result.AppendLine($"  Instance Methods: [Error: {ex.Message}]");
                        }

                        // Constructors
                        try {
                            var ctors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (ctors.Length > 0) {
                                result.AppendLine("  Constructors:");
                                foreach (var ctor in ctors) {
                                    try {
                                        var parameters = GetSafeParameterList(ctor.GetParameters());
                                        result.AppendLine($"    - {type.Name}({parameters})");
                                    }
                                    catch (Exception ex) {
                                        result.AppendLine($"    - [Error: {ex.Message}] {type.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            result.AppendLine($"  Constructors: [Error: {ex.Message}]");
                        }

                        result.AppendLine();
                    }
                    catch (Exception ex) {
                        result.AppendLine($"Type {type.FullName}: [Error: {ex.Message}]");
                        result.AppendLine();
                    }
                }

                return result.ToString();
            }
            catch (Exception ex) {
                return $"Error exploring TreeSitterSharp.Cpp API: {ex.Message}\nStack: {ex.StackTrace}";
            }
        }

        /// <summary>
        /// Explore all TreeSitter APIs and generate comprehensive report
        /// </summary>
        public static string ExploreAllTreeSitterApis()
        {
            var report = new StringBuilder();

            report.AppendLine("╔═══════════════════════════════════════════════════════════════╗");
            report.AppendLine("║         TreeSitter API Comprehensive Exploration Report       ║");
            report.AppendLine("╚═══════════════════════════════════════════════════════════════╝");
            report.AppendLine();
            report.AppendLine($"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine("═══════════════════════════════════════════════════════════════");
            report.AppendLine();

            // 1. Explore TreeSitterSharp.C
            report.AppendLine("┌─────────────────────────────────────────────────────────────┐");
            report.AppendLine("│ 1. TreeSitterSharp.C API Exploration                        │");
            report.AppendLine("└─────────────────────────────────────────────────────────────┘");
            report.AppendLine();
            try {
                var cApiResult = ExploreTreeSitterCApi();
                report.AppendLine(cApiResult);
            }
            catch (Exception ex) {
                report.AppendLine($"Error exploring C API: {ex.Message}");
            }
            report.AppendLine();
            report.AppendLine("═══════════════════════════════════════════════════════════════");
            report.AppendLine();

            // 2. Explore TreeSitterSharp.Cpp
            report.AppendLine("┌─────────────────────────────────────────────────────────────┐");
            report.AppendLine("│ 2. TreeSitterSharp.Cpp API Exploration                      │");
            report.AppendLine("└─────────────────────────────────────────────────────────────┘");
            report.AppendLine();
            try {
                var cppApiResult = ExploreTreeSitterCppApi();
                report.AppendLine(cppApiResult);
            }
            catch (Exception ex) {
                report.AppendLine($"Error exploring C++ API: {ex.Message}");
            }
            report.AppendLine();
            report.AppendLine("═══════════════════════════════════════════════════════════════");
            report.AppendLine();

            // 3. Test C Parser Creation
            report.AppendLine("┌─────────────────────────────────────────────────────────────┐");
            report.AppendLine("│ 3. C Parser Creation Test                                   │");
            report.AppendLine("└─────────────────────────────────────────────────────────────┘");
            report.AppendLine();
            try {
                var testResult = TestCreateCParser();
                report.AppendLine(testResult);
            }
            catch (Exception ex) {
                report.AppendLine($"Error testing C parser: {ex.Message}");
            }
            report.AppendLine();
            report.AppendLine("═══════════════════════════════════════════════════════════════");
            report.AppendLine();

            // Summary
            report.AppendLine("┌─────────────────────────────────────────────────────────────┐");
            report.AppendLine("│ Summary                                                     │");
            report.AppendLine("└─────────────────────────────────────────────────────────────┘");
            report.AppendLine();
            report.AppendLine("Exploration completed. Review the above sections for details.");
            report.AppendLine();
            report.AppendLine("═══════════════════════════════════════════════════════════════");

            return report.ToString();
        }

        /// <summary>
        /// Test creating a C parser
        /// </summary>
        public static string TestCreateCParser()
        {
            try {
                var result = new StringBuilder();
                result.AppendLine("=== Testing TreeSitter C Parser Creation ===");
                result.AppendLine();

                // Test 1: Create CParser directly
                result.AppendLine("Test 1: Creating CParser instance...");
                var parser = new TreeSitterSharp.C.CParser();
                result.AppendLine($"✓ CParser created successfully");
                result.AppendLine($"  Parser type: {parser.GetType().FullName}");
                result.AppendLine($"  Language: {parser.Language}");
                result.AppendLine();

                // Test 2: Parse simple C code
                result.AppendLine("Test 2: Parsing simple C code...");
                string testCode = @"
int add(int a, int b) {
    return a + b;
}

struct Point {
    int x;
    int y;
};
";
                var codeBytes = System.Text.Encoding.UTF8.GetBytes(testCode);
                var tree = parser.Parse(codeBytes.AsSpan());
                result.AppendLine($"✓ Code parsed successfully");
                result.AppendLine($"  Tree type: {tree.GetType().FullName}");
                result.AppendLine($"  Root node type: {tree.Root.NodeType}");
                result.AppendLine($"  Root children count: {tree.Root.ChildCount}");
                result.AppendLine();

                result.AppendLine("✓ All tests passed!");
                return result.ToString();
            }
            catch (Exception ex) {
                return $"✗ Error testing C parser creation: {ex.Message}\nStack: {ex.StackTrace}";
            }
        }

        /// <summary>
        /// Test creating a C++ parser
        /// </summary>
        public static string TestCreateCppParser()
        {
            try {
                var result = new StringBuilder();
                result.AppendLine("=== Testing TreeSitter C++ Parser Creation ===");
                result.AppendLine();

                // Test 1: Create CppParser directly
                result.AppendLine("Test 1: Creating CppParser instance...");
                var parser = new TreeSitterSharp.Cpp.CppParser();
                result.AppendLine($"✓ CppParser created successfully");
                result.AppendLine($"  Parser type: {parser.GetType().FullName}");
                result.AppendLine($"  Language: {parser.Language}");
                result.AppendLine();

                // Test 2: Parse simple C++ code
                result.AppendLine("Test 2: Parsing simple C++ code...");
                string testCode = @"
class Calculator {
public:
    int add(int a, int b) {
        return a + b;
    }
};

namespace Math {
    template<typename T>
    T multiply(T a, T b) {
        return a * b;
    }
}
";
                var codeBytes = System.Text.Encoding.UTF8.GetBytes(testCode);
                var tree = parser.Parse(codeBytes.AsSpan());
                result.AppendLine($"✓ Code parsed successfully");
                result.AppendLine($"  Tree type: {tree.GetType().FullName}");
                result.AppendLine($"  Root node type: {tree.Root.NodeType}");
                result.AppendLine($"  Root children count: {tree.Root.ChildCount}");
                result.AppendLine();

                result.AppendLine("✓ All tests passed!");
                return result.ToString();
            }
            catch (Exception ex) {
                return $"✗ Error testing C++ parser creation: {ex.Message}\nStack: {ex.StackTrace}";
            }
        }

        public static string TestGetNodeText()
        {
            try {
                var result = new StringBuilder();
                result.AppendLine("=== Testing GetNodeText - Actual Code Extraction ===");
                result.AppendLine();

                // Test C Parser
                result.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                result.AppendLine("Test 1: C Parser - GetNodeText Verification");
                result.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                result.AppendLine();

                string cCode = @"int add(int a, int b) {
    return a + b;
}

struct Point {
    int x;
    int y;
};";

                var cParser = new TreeSitterCParser();
                var cParsedFile = cParser.ParseText(cCode, "test.c");

                result.AppendLine("Source Code:");
                result.AppendLine("─────────────────────────────────────────────────");
                result.AppendLine(cCode);
                result.AppendLine("─────────────────────────────────────────────────");
                result.AppendLine();

                // Debug: Show tree structure
                result.AppendLine("Debug Info:");
                result.AppendLine($"  • Has NativeSyntaxTree: {cParsedFile.NativeSyntaxTree != null}");
                if (cParsedFile.NativeSyntaxTree != null) {
                    var tree = cParsedFile.NativeSyntaxTree as TreeSitterSharp.C.CSyntaxTree;
                    if (tree != null) {
                        result.AppendLine($"  • Root node type: {tree.Root?.NodeType ?? "null"}");
                        bool hasChildren = tree.Root != null && tree.Root.NamedChildren.Any();
                        result.AppendLine($"  • Root has children: {hasChildren}");
                        if (tree.Root != null) {
                            result.AppendLine($"  • Root children count: {tree.Root.NamedChildren.Count()}");
                            foreach (var child in tree.Root.NamedChildren.Take(5)) {
                                result.AppendLine($"    - Child type: {child.NodeType}");

                                // Debug: Try to extract function info manually
                                if (child.NodeType == "function_definition") {
                                    result.AppendLine($"      [DEBUG] Found function_definition node");
                                    result.AppendLine($"      [DEBUG] Children of function_definition:");
                                    foreach (var funcChild in child.NamedChildren) {
                                        result.AppendLine($"        - {funcChild.NodeType}");

                                        // Look for function_declarator
                                        if (funcChild.NodeType == "function_declarator") {
                                            result.AppendLine($"          [DEBUG] Found function_declarator!");
                                            result.AppendLine($"          [DEBUG] Children of function_declarator:");
                                            foreach (var declChild in funcChild.NamedChildren) {
                                                result.AppendLine($"            - {declChild.NodeType}");
                                                if (declChild.NodeType == "identifier") {
                                                    // Extract text using StartByte and EndByte
                                                    int start = (int)declChild.StartByte;
                                                    int end = (int)declChild.EndByte;
                                                    string funcName = cCode.Substring(start, end - start);
                                                    result.AppendLine($"              [DEBUG] Function name: '{funcName}'");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                result.AppendLine();

                result.AppendLine("Extracted Symbols:");
                result.AppendLine();

                // Check functions
                if (cParsedFile.Functions.Count > 0) {
                    result.AppendLine($"✓ Found {cParsedFile.Functions.Count} function(s):");
                    foreach (var func in cParsedFile.Functions) {
                        result.AppendLine($"  • Function: {func.Name}");
                        if (func.TemplateParameters != null && func.TemplateParameters.Count > 0) {
                            result.AppendLine($"    Template: <{string.Join(", ", func.TemplateParameters)}>");
                        }
                        result.AppendLine($"    Return Type: {func.ReturnType}");
                        result.AppendLine($"    Parameters: {string.Join(", ", func.Parameters.Select(p => $"{p.Type} {p.Name}"))}");
                        result.AppendLine($"    Line: {func.Location?.StartLine ?? 0}");
                        result.AppendLine();
                    }
                } else {
                    result.AppendLine("✗ No functions found");
                }

                // Check types
                if (cParsedFile.Types.Count > 0) {
                    result.AppendLine($"✓ Found {cParsedFile.Types.Count} type(s):");
                    foreach (var type in cParsedFile.Types) {
                        result.AppendLine($"  • Type: {type.Name} ({type.Type})");
                        if (type.Fields != null && type.Fields.Count > 0) {
                            result.AppendLine($"    Fields:");
                            foreach (var field in type.Fields) {
                                result.AppendLine($"      - {field.Type} {field.Name}");
                            }
                        }
                        result.AppendLine($"    Line: {type.Location?.StartLine ?? 0}");
                        result.AppendLine();
                    }
                } else {
                    result.AppendLine("✗ No types found");
                }

                result.AppendLine();

                // Test C++ Parser
                result.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                result.AppendLine("Test 2: C++ Parser - GetNodeText Verification");
                result.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                result.AppendLine();

                string cppCode = @"class Calculator {
public:
    int add(int a, int b) {
        return a + b;
    }

    int multiply(int x, int y) {
        return x * y;
    }
};

namespace Math {
    double divide(double a, double b) {
        return a / b;
    }
}

template<typename T>
T max(T a, T b) {
    return a > b ? a : b;
}

template<typename T, int N>
class Array {
public:
    T data[N];
};";

                var cppParser = new TreeSitterCppParser();
                var cppParsedFile = cppParser.ParseText(cppCode, "test.cpp");

                result.AppendLine("Source Code:");
                result.AppendLine("─────────────────────────────────────────────────");
                result.AppendLine(cppCode);
                result.AppendLine("─────────────────────────────────────────────────");
                result.AppendLine();

                // Debug: Show C++ tree structure
                result.AppendLine("Debug Info:");
                result.AppendLine($"  • Has NativeSyntaxTree: {cppParsedFile.NativeSyntaxTree != null}");
                if (cppParsedFile.NativeSyntaxTree != null) {
                    var tree = cppParsedFile.NativeSyntaxTree as TreeSitterSharp.Cpp.CppSyntaxTree;
                    if (tree != null && tree.Root != null) {
                        result.AppendLine($"  • Root node type: {tree.Root.NodeType}");
                        result.AppendLine($"  • Root children count: {tree.Root.NamedChildren.Count()}");
                        foreach (var child in tree.Root.NamedChildren.Take(5)) {
                            result.AppendLine($"    - Child type: {child.NodeType}");

                            // Debug: Show class_specifier structure
                            if (child.NodeType == "class_specifier") {
                                result.AppendLine($"      [DEBUG] Found class_specifier node");
                                result.AppendLine($"      [DEBUG] Children of class_specifier:");
                                foreach (var classChild in child.NamedChildren) {
                                    result.AppendLine($"        - {classChild.NodeType}");

                                    // Look for field_declaration_list
                                    if (classChild.NodeType == "field_declaration_list") {
                                        result.AppendLine($"          [DEBUG] Found field_declaration_list!");
                                        result.AppendLine($"          [DEBUG] Children of field_declaration_list:");
                                        foreach (var fieldChild in classChild.NamedChildren.Take(10)) {
                                            result.AppendLine($"            - {fieldChild.NodeType}");

                                            // Show children of each field
                                            if (fieldChild.NamedChildren.Any()) {
                                                result.AppendLine($"              [DEBUG] Children:");
                                                foreach (var subChild in fieldChild.NamedChildren.Take(5)) {
                                                    result.AppendLine($"                - {subChild.NodeType}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                result.AppendLine();

                result.AppendLine("Extracted Symbols:");
                result.AppendLine();

                // Check classes
                if (cppParsedFile.Types.Count > 0) {
                    result.AppendLine($"✓ Found {cppParsedFile.Types.Count} type(s):");
                    foreach (var type in cppParsedFile.Types) {
                        result.AppendLine($"  • Type: {type.Name} ({type.Type})");
                        if (!string.IsNullOrEmpty(type.Namespace)) {
                            result.AppendLine($"    Namespace: {type.Namespace}");
                        }
                        if (type.TemplateParameters != null && type.TemplateParameters.Count > 0) {
                            result.AppendLine($"    Template: <{string.Join(", ", type.TemplateParameters)}>");
                        }
                        if (type.Fields != null && type.Fields.Count > 0) {
                            result.AppendLine($"    Fields:");
                            foreach (var field in type.Fields) {
                                result.AppendLine($"      - {field.Type} {field.Name}");
                            }
                        }
                        result.AppendLine($"    Line: {type.Location?.StartLine ?? 0}");
                        result.AppendLine();
                    }
                } else {
                    result.AppendLine("✗ No types found");
                }

                // Check functions
                if (cppParsedFile.Functions.Count > 0) {
                    result.AppendLine($"✓ Found {cppParsedFile.Functions.Count} function(s):");
                    foreach (var func in cppParsedFile.Functions) {
                        result.AppendLine($"  • Function: {func.Name}");
                        if (!string.IsNullOrEmpty(func.Namespace)) {
                            result.AppendLine($"    Namespace: {func.Namespace}");
                        }
                        if (func.TemplateParameters != null && func.TemplateParameters.Count > 0) {
                            result.AppendLine($"    Template: <{string.Join(", ", func.TemplateParameters)}>");
                        }
                        result.AppendLine($"    Return Type: {func.ReturnType}");
                        result.AppendLine($"    Parameters: {string.Join(", ", func.Parameters.Select(p => $"{p.Type} {p.Name}"))}");
                        result.AppendLine($"    Line: {func.Location?.StartLine ?? 0}");
                        result.AppendLine();
                    }
                } else {
                    result.AppendLine("✗ No functions found");
                }

                result.AppendLine();
                result.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                result.AppendLine("✓ GetNodeText Test Completed!");
                result.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                result.AppendLine();
                result.AppendLine("Verification:");
                result.AppendLine("• If you see actual function/type names (not 'function_definition' or 'class_specifier'),");
                result.AppendLine("  then GetNodeText is working correctly!");
                result.AppendLine("• If you see node type names, GetNodeText needs fixing.");

                return result.ToString();
            }
            catch (Exception ex) {
                return $"✗ Error testing GetNodeText: {ex.Message}\nStack: {ex.StackTrace}";
            }
        }
    }
}
