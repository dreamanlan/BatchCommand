using System;
using System.Collections.Generic;

namespace CefDotnetApp.AgentCore.Models
{
    // Represents location information in source code
    public class CodeLocation
    {
        public string FilePath { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }

        public CodeLocation(string filePath, int startLine, int endLine, int startColumn, int endColumn)
        {
            FilePath = filePath;
            StartLine = startLine;
            EndLine = endLine;
            StartColumn = startColumn;
            EndColumn = endColumn;
        }

        public override string ToString()
        {
            return $"{FilePath}:{StartLine}:{StartColumn}-{EndLine}:{EndColumn}";
        }
    }

    // Represents a method in the code (Roslyn-based detailed info)
    public class RoslynMethodInfo
    {
        public string Name { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public List<RoslynParameterInfo> Parameters { get; set; }
        public CodeLocation? Location { get; set; }
        public string? Modifiers { get; set; }
        public string FullText { get; set; } = string.Empty;
        public Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax? SyntaxNode { get; set; }

        public RoslynMethodInfo()
        {
            Parameters = new List<RoslynParameterInfo>();
        }

        public override string ToString()
        {
            return $"{Modifiers} {ReturnType} {Name}({string.Join(", ", Parameters)})";
        }
    }

    // Represents a method parameter
    public class RoslynParameterInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }

    // Represents a property in the code
    public class RoslynPropertyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Modifiers { get; set; }
        public CodeLocation? Location { get; set; }
        public bool HasGetter { get; set; }
        public bool HasSetter { get; set; }
        public bool IsAutoProperty { get; set; }
        public Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax? SyntaxNode { get; set; }

        public override string ToString()
        {
            return $"{Modifiers} {Type} {Name}";
        }
    }

    // Represents a field in the code
    public class RoslynFieldInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Modifiers { get; set; }
        public CodeLocation? Location { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsConst { get; set; }
        public Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax? SyntaxNode { get; set; }

        public override string ToString()
        {
            return $"{Modifiers} {Type} {Name}";
        }
    }

    // Represents an event in the code
    public class RoslynEventInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Modifiers { get; set; }
        public CodeLocation? Location { get; set; }
        public Microsoft.CodeAnalysis.CSharp.Syntax.EventDeclarationSyntax? SyntaxNode { get; set; }

        public override string ToString()
        {
            return $"{Modifiers} event {Type} {Name}";
        }
    }

    // Represents a constructor in the code
    public class RoslynConstructorInfo
    {
        public string Name { get; set; } = string.Empty;
        public List<RoslynParameterInfo> Parameters { get; set; }
        public CodeLocation? Location { get; set; }
        public string? Modifiers { get; set; }
        public string FullText { get; set; } = string.Empty;
        public Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax? SyntaxNode { get; set; }

        public RoslynConstructorInfo()
        {
            Parameters = new List<RoslynParameterInfo>();
        }

        public override string ToString()
        {
            return $"{Modifiers} {Name}({string.Join(", ", Parameters)})";
        }
    }

    // Represents a class in the code (Roslyn-based detailed info)
    public class RoslynClassInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? Namespace { get; set; }
        public List<RoslynMethodInfo> Methods { get; set; }
        public List<RoslynPropertyInfo> Properties { get; set; }
        public List<RoslynFieldInfo> Fields { get; set; }
        public List<RoslynConstructorInfo> Constructors { get; set; }
        public List<RoslynEventInfo> Events { get; set; }
        public List<string> BaseTypes { get; set; }
        public CodeLocation? Location { get; set; }
        public string? Modifiers { get; set; }
        public Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax? SyntaxNode { get; set; }

        // Nested type information
        public bool IsNested { get; set; }
        public string? ParentTypeName { get; set; }
        public string? FullTypeName { get; set; }

        public RoslynClassInfo()
        {
            Methods = new List<RoslynMethodInfo>();
            Properties = new List<RoslynPropertyInfo>();
            Fields = new List<RoslynFieldInfo>();
            Constructors = new List<RoslynConstructorInfo>();
            Events = new List<RoslynEventInfo>();
            BaseTypes = new List<string>();
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
            return $"{Modifiers} class {FullName}";
        }
    }

    // Represents a parsed C# file (Roslyn-based)
    public class RoslynParsedFile
    {
        public string FilePath { get; set; } = string.Empty;
        public List<RoslynClassInfo> Classes { get; set; }
        public List<RoslynInterfaceInfo> Interfaces { get; set; }
        public List<RoslynStructInfo> Structs { get; set; }
        public List<RoslynEnumInfo> Enums { get; set; }
        public List<string> Usings { get; set; }
        public string? Namespace { get; set; }
        public Microsoft.CodeAnalysis.SyntaxTree? SyntaxTree { get; set; }

        public RoslynParsedFile(string filePath)
        {
            FilePath = filePath;
            Classes = new List<RoslynClassInfo>();
            Interfaces = new List<RoslynInterfaceInfo>();
            Structs = new List<RoslynStructInfo>();
            Enums = new List<RoslynEnumInfo>();
            Usings = new List<string>();
        }
    }

    // Represents an interface in the code
    public class RoslynInterfaceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? Namespace { get; set; }
        public List<RoslynMethodInfo> Methods { get; set; }
        public List<RoslynPropertyInfo> Properties { get; set; }
        public List<string> BaseInterfaces { get; set; }
        public CodeLocation? Location { get; set; }
        public string? Modifiers { get; set; }
        public Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax? SyntaxNode { get; set; }

        // Nested type information
        public bool IsNested { get; set; }
        public string? ParentTypeName { get; set; }
        public string? FullTypeName { get; set; }

        public RoslynInterfaceInfo()
        {
            Methods = new List<RoslynMethodInfo>();
            Properties = new List<RoslynPropertyInfo>();
            BaseInterfaces = new List<string>();
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
            return $"{Modifiers} interface {FullName}";
        }
    }

    // Represents a struct in the code
    public class RoslynStructInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? Namespace { get; set; }
        public List<RoslynMethodInfo> Methods { get; set; }
        public List<RoslynPropertyInfo> Properties { get; set; }
        public List<RoslynFieldInfo> Fields { get; set; }
        public List<RoslynConstructorInfo> Constructors { get; set; }
        public CodeLocation? Location { get; set; }
        public string? Modifiers { get; set; }
        public Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax? SyntaxNode { get; set; }

        // Nested type information
        public bool IsNested { get; set; }
        public string? ParentTypeName { get; set; }
        public string? FullTypeName { get; set; }

        public RoslynStructInfo()
        {
            Methods = new List<RoslynMethodInfo>();
            Properties = new List<RoslynPropertyInfo>();
            Fields = new List<RoslynFieldInfo>();
            Constructors = new List<RoslynConstructorInfo>();
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
            return $"{Modifiers} struct {FullName}";
        }
    }

    // Represents an enum in the code
    public class RoslynEnumInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? Namespace { get; set; }
        public List<RoslynEnumMemberInfo> Members { get; set; }
        public CodeLocation? Location { get; set; }
        public string? Modifiers { get; set; }
        public Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax? SyntaxNode { get; set; }

        // Nested type information
        public bool IsNested { get; set; }
        public string? ParentTypeName { get; set; }
        public string? FullTypeName { get; set; }

        public RoslynEnumInfo()
        {
            Members = new List<RoslynEnumMemberInfo>();
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
            return $"{Modifiers} enum {FullName}";
        }
    }

    // Represents an enum member
    public class RoslynEnumMemberInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Value) ? Name : $"{Name} = {Value}";
        }
    }
}
