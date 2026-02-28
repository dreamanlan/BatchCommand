using System;
using System.Collections.Generic;

namespace CefDotnetApp.AgentCore.Models
{
    // Supported programming languages
    public enum ProgrammingLanguage
    {
        CSharp,
        JavaScript,
        C,
        Cpp,
        TypeScript,
        Tsx,
        Bash,
        Python,
        Rust,
        Go,
        Java,
        Swift,
        PHP,
        Ruby,
        Scala,
        Haskell,
        Julia,
        OCaml,
        Agda,
        Verilog,
        QL,
        Razor,
        JsDoc,
        EmbeddedTemplate,
        // Data/markup languages (no function/type model, use find_nodes APIs)
        Html,
        Css,
        Json,
        Toml
    }

    // Code element types (common across languages)
    public enum CodeElementType
    {
        Class,
        Interface,
        Struct,
        Enum,
        Function,
        Method,
        Property,
        Field,
        Variable,
        Parameter,
        Namespace,
        Module,
        Unknown
    }

    // Unified code element (language-agnostic)
    public class CodeElement
    {
        public ProgrammingLanguage Language { get; set; }
        public CodeElementType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public CodeLocation? Location { get; set; }
        public string FullText { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; }
        public object? NativeObject { get; set; }
        public List<CodeElement> Children { get; set; }

        public CodeElement()
        {
            Metadata = new Dictionary<string, object>();
            Children = new List<CodeElement>();
        }

        public override string ToString()
        {
            return $"[{Language}] {Type}: {Name}";
        }
    }

    // Unified function/method info
    public class FunctionInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? Namespace { get; set; }
        public string ReturnType { get; set; } = string.Empty;
        public List<ParameterInfo> Parameters { get; set; }
        public List<TemplateParameterInfo> TemplateParameters { get; set; }
        public CodeLocation? Location { get; set; }
        public string? Modifiers { get; set; }
        public string FullText { get; set; } = string.Empty;
        public ProgrammingLanguage Language { get; set; }
        public object? NativeObject { get; set; }

        // Extended properties for code analysis
        public string? DocumentationComment { get; set; }
        public int LineCount { get; set; }
        public int CyclomaticComplexity { get; set; }
        public bool IsAsync { get; set; }
        public bool IsStatic { get; set; }
        public bool IsPublic { get; set; }

        public FunctionInfo()
        {
            Parameters = new List<ParameterInfo>();
            TemplateParameters = new List<TemplateParameterInfo>();
        }

        public override string ToString()
        {
            var paramStr = string.Join(", ", Parameters);
            return string.IsNullOrEmpty(Modifiers)
                ? $"{ReturnType} {Name}({paramStr})"
                : $"{Modifiers} {ReturnType} {Name}({paramStr})";
        }
    }

    // Unified parameter info
    public class ParameterInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? DefaultValue { get; set; }

        public override string ToString()
        {
            var result = $"{Type} {Name}";
            if (!string.IsNullOrEmpty(DefaultValue))
            {
                result += $" = {DefaultValue}";
            }
            return result;
        }
    }

    // Template parameter information (for C++ templates)
    public class TemplateParameterInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // typename, class, int, etc.
        public string? DefaultValue { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DefaultValue))
                return $"{Type} {Name} = {DefaultValue}";
            return string.IsNullOrEmpty(Name) ? Type : $"{Type} {Name}";
        }
    }

    // Unified property info
    public class PropertyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Modifiers { get; set; }
        public CodeLocation? Location { get; set; }
        public bool HasGetter { get; set; }
        public bool HasSetter { get; set; }
        public bool IsAutoProperty { get; set; }
        public bool IsStatic { get; set; }
        public bool IsPublic { get; set; }
        public string? DefaultValue { get; set; }
        public string? DocumentationComment { get; set; }
        public ProgrammingLanguage Language { get; set; }
        public object? NativeObject { get; set; }

        public override string ToString()
        {
            var accessors = new List<string>();
            if (HasGetter) accessors.Add("get");
            if (HasSetter) accessors.Add("set");
            var accessorStr = accessors.Count > 0 ? $" {{ {string.Join("; ", accessors)}; }}" : "";
            return string.IsNullOrEmpty(Modifiers)
                ? $"{Type} {Name}{accessorStr}"
                : $"{Modifiers} {Type} {Name}{accessorStr}";
        }
    }

    // Unified field info
    public class FieldInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Modifiers { get; set; }
        public CodeLocation? Location { get; set; }
        public bool IsStatic { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsConst { get; set; }
        public bool IsPublic { get; set; }
        public string? DefaultValue { get; set; }
        public string? DocumentationComment { get; set; }
        public ProgrammingLanguage Language { get; set; }
        public object? NativeObject { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Modifiers)
                ? $"{Type} {Name}"
                : $"{Modifiers} {Type} {Name}";
        }
    }

    // Unified event info
    public class EventInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Modifiers { get; set; }
        public CodeLocation? Location { get; set; }
        public bool IsStatic { get; set; }
        public bool IsPublic { get; set; }
        public string? DocumentationComment { get; set; }
        public object? NativeObject { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Modifiers)
                ? $"event {Type} {Name}"
                : $"{Modifiers} event {Type} {Name}";
        }
    }

    // Unified class/struct info
    public class TypeInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? Namespace { get; set; }
        public CodeElementType Type { get; set; }
        public List<FunctionInfo> Methods { get; set; }
        public List<PropertyInfo> Properties { get; set; }
        public List<FieldInfo> Fields { get; set; }
        public List<FunctionInfo> Constructors { get; set; }
        public List<EventInfo> Events { get; set; }
        public List<string> BaseTypes { get; set; }
        public List<string> Interfaces { get; set; }
        public List<TemplateParameterInfo> TemplateParameters { get; set; }
        public CodeLocation? Location { get; set; }
        public string? Modifiers { get; set; }
        public ProgrammingLanguage Language { get; set; }
        public object? NativeObject { get; set; }

        // Extended properties for code analysis
        public string? DocumentationComment { get; set; }
        public int LineCount { get; set; }
        public int MemberCount { get; set; }
        public bool IsAbstract { get; set; }
        public bool IsSealed { get; set; }
        public bool IsPublic { get; set; }
        public bool IsStatic { get; set; }
        public bool IsPartial { get; set; }

        // Nested type information
        public bool IsNested { get; set; }
        public string? ParentTypeName { get; set; }
        public string? FullTypeName { get; set; }

        public TypeInfo()
        {
            Methods = new List<FunctionInfo>();
            Properties = new List<PropertyInfo>();
            Fields = new List<FieldInfo>();
            Constructors = new List<FunctionInfo>();
            Events = new List<EventInfo>();
            BaseTypes = new List<string>();
            Interfaces = new List<string>();
            TemplateParameters = new List<TemplateParameterInfo>();
        }

        public string FullName
        {
            get
            {
                var typeName = IsNested ? FullTypeName ?? Name : Name;
                return string.IsNullOrEmpty(Namespace) ? typeName : $"{Namespace}.{typeName}";
            }
        }

        public override string ToString()
        {
            return $"{Modifiers} {Type} {FullName}";
        }
    }

    // Unified parsed file result
    public class ParsedCodeFile
    {
        public string FilePath { get; set; } = string.Empty;
        public ProgrammingLanguage Language { get; set; }
        public List<TypeInfo> Types { get; set; }
        public List<TypeInfo> Interfaces { get; set; }
        public List<TypeInfo> Structs { get; set; }
        public List<EnumInfo> Enums { get; set; }
        public List<FunctionInfo> Functions { get; set; }
        public List<string> Imports { get; set; }
        public string? Namespace { get; set; }
        public object? NativeSyntaxTree { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        // Extended properties for code analysis
        public int TotalLines { get; set; }
        public int CodeLines { get; set; }
        public int CommentLines { get; set; }
        public List<DependencyInfo> Dependencies { get; set; }

        // Syntax error information
        public bool HasSyntaxErrors { get; set; }
        public List<string> SyntaxErrors { get; set; }

        public ParsedCodeFile(string filePath, ProgrammingLanguage language)
        {
            FilePath = filePath;
            Language = language;
            Types = new List<TypeInfo>();
            Interfaces = new List<TypeInfo>();
            Structs = new List<TypeInfo>();
            Enums = new List<EnumInfo>();
            Functions = new List<FunctionInfo>();
            Imports = new List<string>();
            Metadata = new Dictionary<string, object>();
            Dependencies = new List<DependencyInfo>();
            SyntaxErrors = new List<string>();
        }

        public override string ToString()
        {
            return $"[{Language}] {FilePath}: {Types.Count} types, {Functions.Count} functions";
        }
    }

    // Enum information
    public class EnumInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? Namespace { get; set; }
        public string? Modifiers { get; set; }
        public CodeLocation? Location { get; set; }
        public List<EnumMemberInfo> Members { get; set; }
        public string? DocumentationComment { get; set; }
        public bool IsPublic { get; set; }
        public object? NativeObject { get; set; }

        // Nested type information
        public bool IsNested { get; set; }
        public string? ParentTypeName { get; set; }
        public string? FullTypeName { get; set; }

        public EnumInfo()
        {
            Members = new List<EnumMemberInfo>();
        }

        public string FullName
        {
            get
            {
                var typeName = IsNested ? FullTypeName ?? Name : Name;
                return string.IsNullOrEmpty(Namespace) ? typeName : $"{Namespace}.{typeName}";
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Modifiers)
                ? $"enum {FullName}"
                : $"{Modifiers} enum {FullName}";
        }
    }

    // Enum member information
    public class EnumMemberInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? DocumentationComment { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Value) ? Name : $"{Name} = {Value}";
        }
    }

    // Dependency information
    public class DependencyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;  // "using", "type_reference", "method_call", etc.
        public string FullName { get; set; } = string.Empty;
        public CodeLocation? Location { get; set; }

        public override string ToString()
        {
            return $"{Type}: {FullName}";
        }
    }
}
