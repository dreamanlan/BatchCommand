using System.Collections.Generic;
using CefDotnetApp.AgentCore.Models;

namespace AgentCore.CodeAnalysis
{
    // Language-specific AST node type configuration for GenericTreeSitterParser
    public class LanguageProfile
    {
        // TreeSitter language id (passed to DotNetParserAdapter)
        public string LanguageId { get; }

        // Corresponding ProgrammingLanguage enum value
        public ProgrammingLanguage Language { get; }

        // Node types that represent function/method definitions
        public HashSet<string> FunctionNodeTypes { get; }

        // Node types that represent class/type definitions
        public HashSet<string> TypeNodeTypes { get; }

        // Node types that represent struct definitions
        public HashSet<string> StructNodeTypes { get; }

        // Node types that represent enum definitions
        public HashSet<string> EnumNodeTypes { get; }

        // Node types that represent parameter lists
        public HashSet<string> ParameterListNodeTypes { get; }

        // Node types that represent individual parameters
        public HashSet<string> ParameterNodeTypes { get; }

        // Node types that represent import/using statements
        public HashSet<string> ImportNodeTypes { get; }

        public LanguageProfile(
            string languageId,
            ProgrammingLanguage language,
            IEnumerable<string> functionNodeTypes,
            IEnumerable<string> typeNodeTypes,
            IEnumerable<string>? structNodeTypes = null,
            IEnumerable<string>? enumNodeTypes = null,
            IEnumerable<string>? parameterListNodeTypes = null,
            IEnumerable<string>? parameterNodeTypes = null,
            IEnumerable<string>? importNodeTypes = null)
        {
            LanguageId = languageId;
            Language = language;
            FunctionNodeTypes = new HashSet<string>(functionNodeTypes);
            TypeNodeTypes = new HashSet<string>(typeNodeTypes);
            StructNodeTypes = new HashSet<string>(structNodeTypes ?? new string[0]);
            EnumNodeTypes = new HashSet<string>(enumNodeTypes ?? new string[0]);
            ParameterListNodeTypes = new HashSet<string>(parameterListNodeTypes ?? new[] { "parameters", "formal_parameters", "parameter_list" });
            ParameterNodeTypes = new HashSet<string>(parameterNodeTypes ?? new[] { "parameter", "required_parameter", "optional_parameter" });
            ImportNodeTypes = new HashSet<string>(importNodeTypes ?? new string[0]);
        }

        // All predefined language profiles
        public static readonly Dictionary<ProgrammingLanguage, LanguageProfile> Profiles = new Dictionary<ProgrammingLanguage, LanguageProfile>
        {
            [ProgrammingLanguage.TypeScript] = new LanguageProfile(
                "typescript",
                ProgrammingLanguage.TypeScript,
                functionNodeTypes: new[] { "function_declaration", "method_definition", "arrow_function" },
                typeNodeTypes: new[] { "class_declaration", "interface_declaration", "type_alias_declaration" },
                enumNodeTypes: new[] { "enum_declaration" },
                importNodeTypes: new[] { "import_statement" }
            ),

            [ProgrammingLanguage.Tsx] = new LanguageProfile(
                "tsx",
                ProgrammingLanguage.Tsx,
                functionNodeTypes: new[] { "function_declaration", "method_definition", "arrow_function" },
                typeNodeTypes: new[] { "class_declaration", "interface_declaration", "type_alias_declaration" },
                enumNodeTypes: new[] { "enum_declaration" },
                importNodeTypes: new[] { "import_statement" }
            ),

            [ProgrammingLanguage.Python] = new LanguageProfile(
                "python",
                ProgrammingLanguage.Python,
                functionNodeTypes: new[] { "function_definition" },
                typeNodeTypes: new[] { "class_definition" },
                parameterListNodeTypes: new[] { "parameters" },
                parameterNodeTypes: new[] { "identifier", "default_parameter", "typed_parameter", "typed_default_parameter" },
                importNodeTypes: new[] { "import_statement", "import_from_statement" }
            ),

            [ProgrammingLanguage.Rust] = new LanguageProfile(
                "rust",
                ProgrammingLanguage.Rust,
                functionNodeTypes: new[] { "function_item" },
                typeNodeTypes: new[] { "impl_item", "trait_item" },
                structNodeTypes: new[] { "struct_item" },
                enumNodeTypes: new[] { "enum_item" },
                parameterListNodeTypes: new[] { "parameters" },
                parameterNodeTypes: new[] { "parameter" },
                importNodeTypes: new[] { "use_declaration" }
            ),

            [ProgrammingLanguage.Go] = new LanguageProfile(
                "go",
                ProgrammingLanguage.Go,
                functionNodeTypes: new[] { "function_declaration", "method_declaration" },
                typeNodeTypes: new[] { "type_declaration" },
                structNodeTypes: new[] { "struct_type" },
                parameterListNodeTypes: new[] { "parameter_list" },
                parameterNodeTypes: new[] { "parameter_declaration" },
                importNodeTypes: new[] { "import_declaration" }
            ),

            [ProgrammingLanguage.Java] = new LanguageProfile(
                "java",
                ProgrammingLanguage.Java,
                functionNodeTypes: new[] { "method_declaration", "constructor_declaration" },
                typeNodeTypes: new[] { "class_declaration", "interface_declaration" },
                enumNodeTypes: new[] { "enum_declaration" },
                parameterListNodeTypes: new[] { "formal_parameters" },
                parameterNodeTypes: new[] { "formal_parameter", "spread_parameter" },
                importNodeTypes: new[] { "import_declaration" }
            ),

            [ProgrammingLanguage.Swift] = new LanguageProfile(
                "swift",
                ProgrammingLanguage.Swift,
                functionNodeTypes: new[] { "function_declaration" },
                typeNodeTypes: new[] { "class_declaration", "protocol_declaration" },
                structNodeTypes: new[] { "struct_declaration" },
                enumNodeTypes: new[] { "enum_declaration" },
                importNodeTypes: new[] { "import_declaration" }
            ),

            [ProgrammingLanguage.PHP] = new LanguageProfile(
                "php",
                ProgrammingLanguage.PHP,
                functionNodeTypes: new[] { "function_definition", "method_declaration" },
                typeNodeTypes: new[] { "class_declaration", "interface_declaration", "trait_declaration" },
                enumNodeTypes: new[] { "enum_declaration" },
                parameterListNodeTypes: new[] { "formal_parameters" },
                parameterNodeTypes: new[] { "simple_parameter", "variadic_parameter" },
                importNodeTypes: new[] { "namespace_use_declaration" }
            ),

            [ProgrammingLanguage.Bash] = new LanguageProfile(
                "bash",
                ProgrammingLanguage.Bash,
                functionNodeTypes: new[] { "function_definition" },
                typeNodeTypes: new string[0],
                parameterListNodeTypes: new string[0],
                parameterNodeTypes: new string[0]
            ),

            [ProgrammingLanguage.Ruby] = new LanguageProfile(
                "ruby",
                ProgrammingLanguage.Ruby,
                functionNodeTypes: new[] { "method", "singleton_method" },
                typeNodeTypes: new[] { "class", "module" },
                parameterListNodeTypes: new[] { "method_parameters" },
                parameterNodeTypes: new[] { "identifier", "optional_parameter", "splat_parameter", "keyword_parameter" },
                importNodeTypes: new[] { "call" } // require/require_relative
            ),

            [ProgrammingLanguage.Scala] = new LanguageProfile(
                "scala",
                ProgrammingLanguage.Scala,
                functionNodeTypes: new[] { "function_definition" },
                typeNodeTypes: new[] { "class_definition", "trait_definition", "object_definition" },
                enumNodeTypes: new[] { "enum_definition" },
                parameterListNodeTypes: new[] { "parameters" },
                parameterNodeTypes: new[] { "parameter" },
                importNodeTypes: new[] { "import_declaration" }
            ),

            [ProgrammingLanguage.Haskell] = new LanguageProfile(
                "haskell",
                ProgrammingLanguage.Haskell,
                functionNodeTypes: new[] { "function", "bind" },
                typeNodeTypes: new[] { "type_alias", "newtype", "adt" },
                parameterListNodeTypes: new string[0],
                parameterNodeTypes: new string[0],
                importNodeTypes: new[] { "import" }
            ),

            [ProgrammingLanguage.Julia] = new LanguageProfile(
                "julia",
                ProgrammingLanguage.Julia,
                functionNodeTypes: new[] { "function_definition", "short_function_definition" },
                typeNodeTypes: new[] { "struct_definition", "abstract_definition" },
                parameterListNodeTypes: new[] { "parameter_list" },
                parameterNodeTypes: new[] { "identifier", "typed_parameter", "optional_parameter" },
                importNodeTypes: new[] { "import_statement", "using_statement" }
            ),

            [ProgrammingLanguage.OCaml] = new LanguageProfile(
                "ocaml",
                ProgrammingLanguage.OCaml,
                functionNodeTypes: new[] { "let_binding", "value_definition" },
                typeNodeTypes: new[] { "type_definition", "class_definition", "module_definition" },
                parameterListNodeTypes: new[] { "parameter" },
                parameterNodeTypes: new[] { "parameter" },
                importNodeTypes: new[] { "open_statement" }
            ),

            [ProgrammingLanguage.Agda] = new LanguageProfile(
                "agda",
                ProgrammingLanguage.Agda,
                functionNodeTypes: new[] { "function_clause" },
                typeNodeTypes: new[] { "data", "record", "module" },
                parameterListNodeTypes: new string[0],
                parameterNodeTypes: new string[0],
                importNodeTypes: new[] { "open", "import" }
            ),
        };
    }
}
