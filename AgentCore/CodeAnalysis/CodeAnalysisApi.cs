using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using System.Linq;
using AgentCore.CodeAnalysis;
using CefDotnetApp.AgentCore.Models;

namespace CefDotnetApp.AgentCore.CodeAnalysis
{
    // DSL API wrapper for code analysis functionality
    public class CodeAnalysisApi
    {
        // Parse a C# file and return parsed file object
        public static object? ParseCSharpFile(string filePath)
        {
            try {
                var parsedFile = RoslynParser.ParseFileComplete(filePath);
                return parsedFile;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error parsing file {filePath}: {ex.Message}");
                return null;
            }
        }

        // Find a class in a parsed file or by parsing a file
        public static object? FindClass(object fileOrPath, string className)
        {
            try {
                RoslynParsedFile? parsedFile = null;

                if (fileOrPath is string filePath) {
                    parsedFile = RoslynParser.ParseFileComplete(filePath);
                }
                else if (fileOrPath is RoslynParsedFile pf) {
                    parsedFile = pf;
                }
                else {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("[CodeAnalysisApi] Invalid input type for FindClass");
                    return null;
                }

                return parsedFile?.Classes.FirstOrDefault(c => c.Name == className);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error finding class {className}: {ex.Message}");
                return null;
            }
        }

        // Find a method in a class
        public static object? FindMethod(object classInfo, string methodName)
        {
            try {
                if (classInfo is RoslynClassInfo ci) {
                    return ci.Methods.FirstOrDefault(m => m.Name == methodName);
                }

                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("[CodeAnalysisApi] Invalid input type for FindMethod");
                return null;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error finding method {methodName}: {ex.Message}");
                return null;
            }
        }

        // Get location information from a code element
        public static object? GetLocation(object codeElement)
        {
            try {
                if (codeElement is RoslynClassInfo ci) {
                    return ci.Location;
                }
                else if (codeElement is RoslynMethodInfo mi) {
                    return mi.Location;
                }

                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("[CodeAnalysisApi] Invalid input type for GetLocation");
                return null;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error getting location: {ex.Message}");
                return null;
            }
        }

        // Get all C# files in a directory
        public static object? GetCSharpFiles(string directory, bool recursive = true)
        {
            try {
                var files = RoslynParser.FindCSharpFiles(directory, recursive);
                return files;
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error getting C# files from {directory}: {ex.Message}");
                return new List<string>();
            }
        }

        // Find file containing a specific class
        public static string? FindFileWithClass(string directory, string className)
        {
            try {
                return RoslynParser.FindFileContainingClass(directory, className);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error finding file with class {className}: {ex.Message}");
                return null;
            }
        }

        // Get class name from a class info object
        public static string? GetClassName(object classInfo)
        {
            if (classInfo is RoslynClassInfo ci) {
                return ci.Name;
            }
            return null;
        }

        // Get method name from a method info object
        public static string? GetMethodName(object methodInfo)
        {
            if (methodInfo is RoslynMethodInfo mi) {
                return mi.Name;
            }
            return null;
        }

        // Get all methods from a class
        public static object GetMethods(object classInfo)
        {
            if (classInfo is RoslynClassInfo ci) {
                return ci.Methods;
            }
            return new List<RoslynMethodInfo>();
        }

        // Get method full text
        public static string? GetMethodText(object methodInfo)
        {
            if (methodInfo is RoslynMethodInfo mi) {
                return mi.FullText;
            }
            return null;
        }

        // Print class information (for debugging)
        public static void PrintClassInfo(object classInfo)
        {
            if (classInfo is RoslynClassInfo ci) {
                var logger = CefDotnetApp.AgentCore.Core.AgentCore.Instance.Logger;
                logger.Info($"Class: {ci.FullName}");
                logger.Info($"  Modifiers: {ci.Modifiers}");
                logger.Info($"  Location: {ci.Location}");
                logger.Info($"  Methods: {ci.Methods.Count}");
                foreach (var method in ci.Methods) {
                    logger.Info($"    - {method.Name}");
                }
            }
        }

        // Print method information (for debugging)
        public static void PrintMethodInfo(object methodInfo)
        {
            if (methodInfo is RoslynMethodInfo mi) {
                var logger = CefDotnetApp.AgentCore.Core.AgentCore.Instance.Logger;
                logger.Info($"Method: {mi.Name}");
                logger.Info($"  Return Type: {mi.ReturnType}");
                logger.Info($"  Modifiers: {mi.Modifiers}");
                logger.Info($"  Location: {mi.Location}");
                logger.Info($"  Parameters: {mi.Parameters.Count}");
                foreach (var param in mi.Parameters) {
                    logger.Info($"    - {param.Type} {param.Name}");
                }
            }
        }

        // ========== Phase 2: Smart Code Editing APIs ==========

        /// <summary>
        /// Add a new method to an existing class
        /// </summary>
        public static bool AddMethodToClass(string filePath, string className, string methodCode)
        {
            try {
                return SmartCodeEditor.AddMethodToClass(filePath, className, methodCode);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error adding method: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Replace an existing method in a class
        /// </summary>
        public static bool ReplaceMethod(string filePath, string className, string methodName, string newMethodCode)
        {
            try {
                return SmartCodeEditor.ReplaceMethod(filePath, className, methodName, newMethodCode);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error replacing method: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Insert a method after another method in a class
        /// </summary>
        public static bool InsertMethodAfter(string filePath, string className, string afterMethodName, string newMethodCode)
        {
            try {
                return SmartCodeEditor.InsertMethodAfter(filePath, className, afterMethodName, newMethodCode);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error inserting method: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a method from a class
        /// </summary>
        public static bool DeleteMethod(string filePath, string className, string methodName)
        {
            try {
                return SmartCodeEditor.DeleteMethod(filePath, className, methodName);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error deleting method: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Add a new class to a file
        /// </summary>
        public static bool AddClassToFile(string filePath, string classCode)
        {
            try {
                return SmartCodeEditor.AddClassToFile(filePath, classCode);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error adding class: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create a new C# file with the given code
        /// </summary>
        public static bool CreateNewFile(string filePath, string code)
        {
            try {
                return SmartCodeEditor.CreateNewFile(filePath, code);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error creating file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verify that code compiles without errors
        /// </summary>
        public static bool VerifyCodeCompiles(string code)
        {
            try {
                return SmartCodeEditor.VerifyCodeCompiles(code);
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[CodeAnalysisApi] Error verifying code: {ex.Message}");
                return false;
            }
        }

        // ========== TreeSitter API Explorer ==========
        // Note: TreeSitter test methods have been moved to Tools/TestTreeSitter.cs

        /// <summary>
        /// Explore TreeSitter.DotNet C language API
        /// </summary>
        public static string ExploreTreeSitterCApi()
        {
            return CefDotnetApp.AgentCore.Tools.TestTreeSitter.ExploreTreeSitterCApi();
        }

        /// <summary>
        /// Explore TreeSitter.DotNet C++ language API
        /// </summary>
        public static string ExploreTreeSitterCppApi()
        {
            return CefDotnetApp.AgentCore.Tools.TestTreeSitter.ExploreTreeSitterCppApi();
        }

        /// <summary>
        /// Explore all TreeSitter APIs and generate comprehensive report
        /// </summary>
        public static string ExploreAllTreeSitterApis()
        {
            return CefDotnetApp.AgentCore.Tools.TestTreeSitter.ExploreAllTreeSitterApis();
        }

        /// <summary>
        /// Test creating a C parser
        /// </summary>
        public static string TestCreateCParser()
        {
            return CefDotnetApp.AgentCore.Tools.TestTreeSitter.TestCreateCParser();
        }

        /// <summary>
        /// Test creating a C++ parser
        /// </summary>
        public static string TestCreateCppParser()
        {
            return CefDotnetApp.AgentCore.Tools.TestTreeSitter.TestCreateCppParser();
        }

        public static string TestGetNodeText()
        {
            return CefDotnetApp.AgentCore.Tools.TestTreeSitter.TestGetNodeText();
        }
    }
}
