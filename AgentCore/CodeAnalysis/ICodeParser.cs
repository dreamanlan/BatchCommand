using System;
using System.Collections.Generic;
using CefDotnetApp.AgentCore.Models;

namespace AgentCore.CodeAnalysis
{
    // Unified code parser interface
    public interface ICodeParser
    {
        ProgrammingLanguage Language { get; }

        ParsedCodeFile Parse(string filePath);

        ParsedCodeFile ParseText(string code, string filePath = null);

        List<FunctionInfo> FindFunctions(ParsedCodeFile parsed, string namePattern = null, bool ignoreCase = true);

        List<TypeInfo> FindTypes(ParsedCodeFile parsed, string namePattern = null, bool ignoreCase = true);

        FunctionInfo FindFunction(ParsedCodeFile parsed, string functionName, bool ignoreCase = true);

        TypeInfo FindType(ParsedCodeFile parsed, string typeName, bool ignoreCase = true);
    }
}
