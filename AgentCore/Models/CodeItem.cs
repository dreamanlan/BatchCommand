using System.Collections.Generic;

namespace CefDotnetApp.AgentCore.Models
{
    /// <summary>
    /// Flat structured item returned by all find_*_as_list APIs.
    /// Different endpoints fill different subsets of fields; unused fields are empty string or null.
    /// Supports LINQ-style access in MetaDSL: list.where($$.Kind == "Method" and $$.ParentClass == "Foo").
    /// </summary>
    public class CodeItem
    {
        /// <summary>
        /// Item category: Function/Class/Struct/Interface/Enum/Field/Property/Method/Event/Constructor/Variable/GlobalVariable/Parameter/Node.
        /// </summary>
        public string Kind { get; set; } = string.Empty;

        /// <summary>Symbol name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Source file path (or "&lt;text&gt;" for *_in_code_*).</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>Formatted location string, e.g. "Line 12, Col 4" or "Lines 12-30".</summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>Start line number (1-based); 0 if unavailable. Use this for numeric LINQ filtering/sorting.</summary>
        public int StartLine { get; set; } = 0;

        /// <summary>End line number (1-based, inclusive); equals StartLine for single-point items, 0 if unavailable.</summary>
        public int EndLine { get; set; } = 0;

        /// <summary>Function/method/constructor signature; empty for non-callable items.</summary>
        public string Signature { get; set; } = string.Empty;

        /// <summary>Declared type for fields/properties/parameters; empty otherwise.</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Parent type name for class members (Field/Property/Method/Event/Constructor/Parameter inside method).</summary>
        public string ParentClass { get; set; } = string.Empty;

        /// <summary>Modifiers such as "public static readonly".</summary>
        public string Modifiers { get; set; } = string.Empty;

        /// <summary>Scope: Field/Global/Parameter/Local/Nested.</summary>
        public string Scope { get; set; } = string.Empty;

        /// <summary>Type-specific extra info: BaseTypes for type, MembersCount for enum, NameValue for node, etc.</summary>
        public string Extra { get; set; } = string.Empty;

        /// <summary>Multi-line fallback rendering used by ToString(); preserves backward-compatible string output.</summary>
        public string Text { get; set; } = string.Empty;

        public override string ToString()
        {
            return string.IsNullOrEmpty(Text) ? $"{Kind}: {Name}" : Text;
        }
    }
}
