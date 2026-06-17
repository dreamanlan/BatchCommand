using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AgentCore.CodeAnalysis.TreeSitter.Adapters;
using AgentCore.CodeAnalysis.TreeSitter.Interfaces;
using CefDotnetApp.AgentCore.Models;

namespace AgentCore.CodeAnalysis
{
    // List-returning variants of the Find* APIs.
    // Each item in the returned list corresponds to one logical target
    // (function/type/variable/class/node...). On any error or empty result,
    // an empty list is returned (no error string item).
    public partial class UnifiedCodeAnalysisApi
    {
        // ====================================================================
        // B1. Function Search APIs (List)
        // ====================================================================
        public List<CodeItem> FindFunctionsAsList(string filePath, ProgrammingLanguage language, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            var functions = FindFunctions(parsed, namePattern);
            return BuildFunctionItems(functions, filePath);
        }

        public List<CodeItem> FindFunctionsInCodeAsList(string code, ProgrammingLanguage language, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            var functions = FindFunctions(parsed, namePattern);
            return BuildFunctionItems(functions, fileName);
        }

        private List<CodeItem> BuildFunctionItems(List<FunctionInfo> functions, string filePath)
        {
            var list = new List<CodeItem>();
            foreach (var func in functions) {
                var locationStr = FormatLocation(func.Location);
                var signatureStr = FormatFunctionSignature(func);
                var sb = new StringBuilder();
                sb.AppendLine($"Function: {func.Name}");
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {locationStr}");
                sb.AppendLine($"Signature: {signatureStr}");
                list.Add(new CodeItem {
                    Kind = "Function",
                    Name = func.Name ?? string.Empty,
                    FilePath = filePath,
                    Location = locationStr,
                    StartLine = func.Location?.StartLine ?? 0,
                    EndLine = func.Location?.EndLine ?? 0,
                    Signature = signatureStr,

                    Type = func.ReturnType ?? string.Empty,
                    Modifiers = func.Modifiers ?? string.Empty,
                    Text = sb.ToString().TrimEnd('\r', '\n')
                });
            }
            return list;
        }

        // ====================================================================
        // B2. Type Search APIs (List)
        // ====================================================================
        public List<CodeItem> FindTypesAsList(string filePath, ProgrammingLanguage language, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            var types = FindTypes(parsed, namePattern);
            return BuildTypeItems(types, filePath);
        }

        public List<CodeItem> FindTypesInCodeAsList(string code, ProgrammingLanguage language, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            var types = FindTypes(parsed, namePattern);
            return BuildTypeItems(types, fileName);
        }

        private List<CodeItem> BuildTypeItems(List<TypeInfo> types, string filePath)
        {
            var list = new List<CodeItem>();
            foreach (var type in types) {
                var locationStr = FormatLocation(type.Location);
                var baseTypesStr = type.BaseTypes.Count > 0 ? string.Join(", ", type.BaseTypes) : string.Empty;
                var sb = new StringBuilder();
                sb.AppendLine($"{type.Type}: {type.Name}");
                if (type.IsNested) {
                    sb.AppendLine($"  Nested: Yes");
                    sb.AppendLine($"  Parent Type: {type.ParentTypeName}");
                    sb.AppendLine($"  Full Type Name: {type.FullTypeName}");
                }
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {locationStr}");
                if (!string.IsNullOrEmpty(baseTypesStr))
                    sb.AppendLine($"Base Types: {baseTypesStr}");
                list.Add(new CodeItem {
                    Kind = type.Type.ToString(),
                    Name = type.Name ?? string.Empty,
                    FilePath = filePath,
                    Location = locationStr,
                    StartLine = type.Location?.StartLine ?? 0,
                    EndLine = type.Location?.EndLine ?? 0,
                    Type = baseTypesStr,

                    ParentClass = type.IsNested ? (type.ParentTypeName ?? string.Empty) : string.Empty,
                    Modifiers = type.Modifiers ?? string.Empty,
                    Text = sb.ToString().TrimEnd('\r', '\n')
                });
            }
            return list;
        }

        // ====================================================================
        // B3. Variable Search APIs (List)
        // ====================================================================
        public List<CodeItem> FindVariablesAsList(string filePath, ProgrammingLanguage language, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildVariableItems(parsed, namePattern, filePath);
        }

        public List<CodeItem> FindVariablesInCodeAsList(string code, ProgrammingLanguage language, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildVariableItems(parsed, namePattern, fileName);
        }

        private List<CodeItem> BuildVariableItems(ParsedCodeFile parsed, string? namePattern, string filePath)
        {
            var list = new List<CodeItem>();
            foreach (var type in parsed.Types) {
                foreach (var field in type.Fields) {
                    if (string.IsNullOrEmpty(namePattern) || MatchesPattern(field.Name ?? string.Empty, namePattern)) {
                        var item = new CodeItem {
                            Kind = "Variable",
                            Name = field.Name ?? string.Empty,
                            FilePath = filePath,
                            Location = FormatLocation(field.Location),
                            StartLine = field.Location?.StartLine ?? 0,
                            EndLine = field.Location?.EndLine ?? 0,
                            Type = field.Type ?? string.Empty,

                            ParentClass = type.Name ?? string.Empty,
                            Modifiers = field.Modifiers ?? string.Empty,
                            Scope = $"Field (in {type.Type.ToString().ToLower()} {type.Name})",
                            Text = $"Variable: {field.Name} ({field.Type}) in {type.Name}"
                        };
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public List<CodeItem> FindGlobalVariablesAsList(string filePath, ProgrammingLanguage language, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildGlobalVariableItems(parsed, namePattern, filePath);
        }

        public List<CodeItem> FindGlobalVariablesInCodeAsList(string code, ProgrammingLanguage language, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildGlobalVariableItems(parsed, namePattern, fileName);
        }

        private List<CodeItem> BuildGlobalVariableItems(ParsedCodeFile parsed, string? namePattern, string filePath)
        {
            var list = new List<CodeItem>();
            foreach (var type in parsed.Types) {
                foreach (var field in type.Fields) {
                    if (field.IsStatic && (string.IsNullOrEmpty(namePattern) || MatchesPattern(field.Name ?? string.Empty, namePattern))) {
                        var item = new CodeItem {
                            Kind = "GlobalVariable",
                            Name = field.Name ?? string.Empty,
                            FilePath = filePath,
                            Location = FormatLocation(field.Location),
                            StartLine = field.Location?.StartLine ?? 0,
                            EndLine = field.Location?.EndLine ?? 0,
                            Type = field.Type ?? string.Empty,

                            ParentClass = type.Name ?? string.Empty,
                            Modifiers = field.Modifiers ?? string.Empty,
                            Scope = $"Global (static field in {type.Name})",
                            Text = $"GlobalVariable: {field.Name} ({field.Type}) in {type.Name}"
                        };
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public List<CodeItem> FindParametersAsList(string filePath, ProgrammingLanguage language, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildParameterItems(parsed, namePattern, filePath);
        }

        public List<CodeItem> FindParametersInCodeAsList(string code, ProgrammingLanguage language, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildParameterItems(parsed, namePattern, fileName);
        }

        private List<CodeItem> BuildParameterItems(ParsedCodeFile parsed, string? namePattern, string filePath)
        {
            var list = new List<CodeItem>();
            foreach (var func in parsed.Functions) {
                foreach (var param in func.Parameters) {
                    if (string.IsNullOrEmpty(namePattern) || MatchesPattern(param.Name ?? string.Empty, namePattern)) {
                        var item = new CodeItem {
                            Kind = "Parameter",
                            Name = param.Name ?? string.Empty,
                            FilePath = filePath,
                            Type = param.Type ?? string.Empty,
                            Scope = "Function",
                            Extra = func.Name ?? string.Empty,
                            Text = $"Parameter: {param.Name} ({param.Type}) of function {func.Name}"
                        };
                        list.Add(item);
                    }
                }
            }
            foreach (var type in parsed.Types) {
                foreach (var method in type.Methods) {
                    foreach (var param in method.Parameters) {
                        if (string.IsNullOrEmpty(namePattern) || MatchesPattern(param.Name ?? string.Empty, namePattern)) {
                            var item = new CodeItem {
                                Kind = "Parameter",
                                Name = param.Name ?? string.Empty,
                                FilePath = filePath,
                                Type = param.Type ?? string.Empty,
                                ParentClass = type.Name ?? string.Empty,
                                Scope = "Method",
                                Extra = $"{type.Name}.{method.Name}",
                                Text = $"Parameter: {param.Name} ({param.Type}) of method {type.Name}.{method.Name}"
                            };
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }



        // ====================================================================
        // B4. Class Member Search APIs (List, one item per member)
        // ====================================================================

        // ----- per-member item builders (reused across ClassMembers / Fields / Properties / Methods / Events / Constructors) -----
        private CodeItem BuildOneFieldItem(FieldInfo field, string parentName, string filePath)
        {
            var locationStr = FormatLocation(field.Location);
            var typeStr = field.Type ?? string.Empty;
            var modifiersStr = field.Modifiers ?? string.Empty;
            var briefText = $"Field: {(string.IsNullOrEmpty(modifiersStr) ? "" : modifiersStr + " ")}{typeStr} {field.Name}".Trim();
            return new CodeItem {
                Kind = "Field",
                Name = field.Name ?? string.Empty,
                FilePath = filePath,
                Location = locationStr,
                StartLine = field.Location?.StartLine ?? 0,
                EndLine = field.Location?.EndLine ?? 0,
                Type = typeStr,

                ParentClass = parentName ?? string.Empty,
                Modifiers = modifiersStr,
                Text = briefText
            };
        }

        private CodeItem BuildOnePropertyItem(PropertyInfo prop, string parentName, string filePath)
        {
            var locationStr = FormatLocation(prop.Location);
            var typeStr = prop.Type ?? string.Empty;
            var modifiersStr = prop.Modifiers ?? string.Empty;
            var scopeParts = new List<string>();
            if (prop.HasGetter) scopeParts.Add("get");
            if (prop.HasSetter) scopeParts.Add("set");
            var scopeStr = string.Join(",", scopeParts);
            var accessorPart = scopeParts.Count > 0 ? $" {{ {scopeStr} }}" : string.Empty;
            var briefText = $"Property: {(string.IsNullOrEmpty(modifiersStr) ? "" : modifiersStr + " ")}{typeStr} {prop.Name}{accessorPart}".Trim();
            return new CodeItem {
                Kind = "Property",
                Name = prop.Name ?? string.Empty,
                FilePath = filePath,
                Location = locationStr,
                StartLine = prop.Location?.StartLine ?? 0,
                EndLine = prop.Location?.EndLine ?? 0,
                Type = typeStr,

                ParentClass = parentName ?? string.Empty,
                Modifiers = modifiersStr,
                Scope = scopeStr,
                Text = briefText
            };
        }

        private CodeItem BuildOneMethodItem(FunctionInfo method, string parentName, string filePath)
        {
            var locationStr = FormatLocation(method.Location);
            var signatureStr = FormatFunctionSignature(method);
            var returnTypeStr = method.ReturnType ?? string.Empty;
            var modifiersStr = method.Modifiers ?? string.Empty;
            var briefText = $"Method: {signatureStr}";
            return new CodeItem {
                Kind = "Method",
                Name = method.Name ?? string.Empty,
                FilePath = filePath,
                Location = locationStr,
                StartLine = method.Location?.StartLine ?? 0,
                EndLine = method.Location?.EndLine ?? 0,
                Signature = signatureStr,

                Type = returnTypeStr,
                ParentClass = parentName ?? string.Empty,
                Modifiers = modifiersStr,
                Text = briefText
            };
        }

        private CodeItem BuildOneEventItem(EventInfo evt, string parentName, string filePath)
        {
            var locationStr = FormatLocation(evt.Location);
            var typeStr = evt.Type ?? string.Empty;
            var modifiersStr = evt.Modifiers ?? string.Empty;
            var briefText = $"Event: {(string.IsNullOrEmpty(modifiersStr) ? "" : modifiersStr + " ")}{typeStr} {evt.Name}".Trim();
            return new CodeItem {
                Kind = "Event",
                Name = evt.Name ?? string.Empty,
                FilePath = filePath,
                Location = locationStr,
                StartLine = evt.Location?.StartLine ?? 0,
                EndLine = evt.Location?.EndLine ?? 0,
                Type = typeStr,

                ParentClass = parentName ?? string.Empty,
                Modifiers = modifiersStr,
                Text = briefText
            };
        }

        private CodeItem BuildOneConstructorItem(FunctionInfo ctor, string parentName, string filePath)
        {
            var locationStr = FormatLocation(ctor.Location);
            var signatureStr = FormatFunctionSignature(ctor);
            var modifiersStr = ctor.Modifiers ?? string.Empty;
            var briefText = $"Constructor: {signatureStr}";
            return new CodeItem {
                Kind = "Constructor",
                Name = parentName ?? string.Empty,
                FilePath = filePath,
                Location = locationStr,
                StartLine = ctor.Location?.StartLine ?? 0,
                EndLine = ctor.Location?.EndLine ?? 0,
                Signature = signatureStr,

                ParentClass = parentName ?? string.Empty,
                Modifiers = modifiersStr,
                Text = briefText
            };
        }

        // ----- B4-1. ClassMembers (per-member mixed Kinds: Field / Property / Method / Event / Constructor) -----
        public List<CodeItem> FindClassMembersAsList(string filePath, ProgrammingLanguage language, string className)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildClassMemberItemsPerClass(parsed, className, filePath);
        }

        public List<CodeItem> FindClassMembersInCodeAsList(string code, ProgrammingLanguage language, string className, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildClassMemberItemsPerClass(parsed, className, fileName);
        }

        private List<CodeItem> BuildClassMemberItemsPerClass(ParsedCodeFile parsed, string className, string filePath)
        {
            var list = new List<CodeItem>();
            var matchingTypes = FindAllMatchingTypes(parsed, className);
            foreach (var type in matchingTypes) {
                var parentName = type.Name ?? string.Empty;
                foreach (var field in type.Fields)
                    list.Add(BuildOneFieldItem(field, parentName, filePath));
                foreach (var prop in type.Properties)
                    list.Add(BuildOnePropertyItem(prop, parentName, filePath));
                foreach (var method in type.Methods)
                    list.Add(BuildOneMethodItem(method, parentName, filePath));
                foreach (var evt in type.Events)
                    list.Add(BuildOneEventItem(evt, parentName, filePath));
                foreach (var ctor in type.Constructors)
                    list.Add(BuildOneConstructorItem(ctor, parentName, filePath));
            }
            return list;
        }

        // ----- B4-2. Fields -----
        public List<CodeItem> FindFieldsAsList(string filePath, ProgrammingLanguage language, string className, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildFieldsItemsPerClass(parsed, className, namePattern, filePath);
        }

        public List<CodeItem> FindFieldsInCodeAsList(string code, ProgrammingLanguage language, string className, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildFieldsItemsPerClass(parsed, className, namePattern, fileName);
        }

        private List<CodeItem> BuildFieldsItemsPerClass(ParsedCodeFile parsed, string className, string? namePattern, string filePath)
        {
            var list = new List<CodeItem>();
            var matchingTypes = FindAllMatchingTypes(parsed, className);
            foreach (var type in matchingTypes) {
                var parentName = type.Name ?? string.Empty;
                var fields = string.IsNullOrEmpty(namePattern)
                    ? type.Fields
                    : type.Fields.Where(f => MatchesPattern(f.Name, namePattern)).ToList();
                foreach (var field in fields)
                    list.Add(BuildOneFieldItem(field, parentName, filePath));
            }
            return list;
        }

        // ----- B4-3. Properties -----
        public List<CodeItem> FindPropertiesAsList(string filePath, ProgrammingLanguage language, string className, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildPropertiesItemsPerClass(parsed, className, namePattern, filePath);
        }

        public List<CodeItem> FindPropertiesInCodeAsList(string code, ProgrammingLanguage language, string className, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildPropertiesItemsPerClass(parsed, className, namePattern, fileName);
        }

        private List<CodeItem> BuildPropertiesItemsPerClass(ParsedCodeFile parsed, string className, string? namePattern, string filePath)
        {
            var list = new List<CodeItem>();
            var matchingTypes = FindAllMatchingTypes(parsed, className);
            foreach (var type in matchingTypes) {
                var parentName = type.Name ?? string.Empty;
                var properties = string.IsNullOrEmpty(namePattern)
                    ? type.Properties
                    : type.Properties.Where(p => MatchesPattern(p.Name, namePattern)).ToList();
                foreach (var prop in properties)
                    list.Add(BuildOnePropertyItem(prop, parentName, filePath));
            }
            return list;
        }

        // ----- B4-4. Methods -----
        public List<CodeItem> FindMethodsAsList(string filePath, ProgrammingLanguage language, string className, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildMethodsItemsPerClass(parsed, className, namePattern, filePath);
        }

        public List<CodeItem> FindMethodsInCodeAsList(string code, ProgrammingLanguage language, string className, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildMethodsItemsPerClass(parsed, className, namePattern, fileName);
        }

        private List<CodeItem> BuildMethodsItemsPerClass(ParsedCodeFile parsed, string className, string? namePattern, string filePath)
        {
            var list = new List<CodeItem>();
            var matchingTypes = FindAllMatchingTypes(parsed, className);
            foreach (var type in matchingTypes) {
                var parentName = type.Name ?? string.Empty;
                var methods = string.IsNullOrEmpty(namePattern)
                    ? type.Methods
                    : type.Methods.Where(m => MatchesPattern(m.Name, namePattern)).ToList();
                foreach (var method in methods)
                    list.Add(BuildOneMethodItem(method, parentName, filePath));
            }
            return list;
        }

        // ----- B4-5. Events -----
        public List<CodeItem> FindEventsAsList(string filePath, ProgrammingLanguage language, string className, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildEventsItemsPerClass(parsed, className, namePattern, filePath);
        }

        public List<CodeItem> FindEventsInCodeAsList(string code, ProgrammingLanguage language, string className, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildEventsItemsPerClass(parsed, className, namePattern, fileName);
        }

        private List<CodeItem> BuildEventsItemsPerClass(ParsedCodeFile parsed, string className, string? namePattern, string filePath)
        {
            var list = new List<CodeItem>();
            var matchingTypes = FindAllMatchingTypes(parsed, className);
            foreach (var type in matchingTypes) {
                var parentName = type.Name ?? string.Empty;
                var events = string.IsNullOrEmpty(namePattern)
                    ? type.Events
                    : type.Events.Where(e => MatchesPattern(e.Name, namePattern)).ToList();
                foreach (var evt in events)
                    list.Add(BuildOneEventItem(evt, parentName, filePath));
            }
            return list;
        }

        // ----- B4-6. Constructors -----
        public List<CodeItem> FindConstructorsAsList(string filePath, ProgrammingLanguage language, string className)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildConstructorsItemsPerClass(parsed, className, filePath);
        }

        public List<CodeItem> FindConstructorsInCodeAsList(string code, ProgrammingLanguage language, string className, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            return BuildConstructorsItemsPerClass(parsed, className, fileName);
        }

        private List<CodeItem> BuildConstructorsItemsPerClass(ParsedCodeFile parsed, string className, string filePath)
        {
            var list = new List<CodeItem>();
            var matchingTypes = FindAllMatchingTypes(parsed, className);
            foreach (var type in matchingTypes) {
                var parentName = type.Name ?? string.Empty;
                foreach (var ctor in type.Constructors)
                    list.Add(BuildOneConstructorItem(ctor, parentName, filePath));
            }
            return list;
        }


        // ====================================================================
        // F. Interface / G. Struct / H. Enum Search APIs (List)
        // ====================================================================
        public List<CodeItem> FindInterfacesAsList(string filePath, ProgrammingLanguage language, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            var interfaces = string.IsNullOrEmpty(namePattern)
                ? parsed.Interfaces
                : parsed.Interfaces.Where(i => MatchesPattern(i.Name, namePattern)).ToList();
            return BuildInterfaceItems(interfaces, filePath);
        }

        public List<CodeItem> FindInterfacesInCodeAsList(string code, ProgrammingLanguage language, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            var interfaces = string.IsNullOrEmpty(namePattern)
                ? parsed.Interfaces
                : parsed.Interfaces.Where(i => MatchesPattern(i.Name, namePattern)).ToList();
            return BuildInterfaceItems(interfaces, fileName);
        }

        private List<CodeItem> BuildInterfaceItems(List<TypeInfo> interfaces, string filePath)
        {
            var list = new List<CodeItem>();
            foreach (var iface in interfaces) {
                var locationStr = FormatLocation(iface.Location);
                var baseTypesStr = iface.BaseTypes.Count > 0 ? string.Join(", ", iface.BaseTypes) : string.Empty;
                var sb = new StringBuilder();
                sb.AppendLine($"Interface: {iface.Name}");
                if (iface.IsNested) {
                    sb.AppendLine($"  Nested: Yes");
                    sb.AppendLine($"  Parent Type: {iface.ParentTypeName}");
                    sb.AppendLine($"  Full Type Name: {iface.FullTypeName}");
                }
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {locationStr}");
                if (!string.IsNullOrEmpty(baseTypesStr))
                    sb.AppendLine($"Base Interfaces: {baseTypesStr}");
                sb.AppendLine($"Methods: {iface.Methods.Count}");
                sb.AppendLine($"Properties: {iface.Properties.Count}");
                list.Add(new CodeItem {
                    Kind = "Interface",
                    Name = iface.Name ?? string.Empty,
                    FilePath = filePath,
                    Location = locationStr,
                    StartLine = iface.Location?.StartLine ?? 0,
                    EndLine = iface.Location?.EndLine ?? 0,
                    Type = baseTypesStr,

                    ParentClass = iface.IsNested ? (iface.ParentTypeName ?? string.Empty) : string.Empty,
                    Modifiers = iface.Modifiers ?? string.Empty,
                    Text = sb.ToString().TrimEnd('\r', '\n')
                });
            }
            return list;
        }

        public List<CodeItem> FindStructsAsList(string filePath, ProgrammingLanguage language, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            var structs = string.IsNullOrEmpty(namePattern)
                ? parsed.Structs
                : parsed.Structs.Where(s => MatchesPattern(s.Name, namePattern)).ToList();
            return BuildStructItems(structs, filePath);
        }

        public List<CodeItem> FindStructsInCodeAsList(string code, ProgrammingLanguage language, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            var structs = string.IsNullOrEmpty(namePattern)
                ? parsed.Structs
                : parsed.Structs.Where(s => MatchesPattern(s.Name, namePattern)).ToList();
            return BuildStructItems(structs, fileName);
        }

        private List<CodeItem> BuildStructItems(List<TypeInfo> structs, string filePath)
        {
            var list = new List<CodeItem>();
            foreach (var structType in structs) {
                var locationStr = FormatLocation(structType.Location);
                var sb = new StringBuilder();
                sb.AppendLine($"Struct: {structType.Name}");
                if (structType.IsNested) {
                    sb.AppendLine($"  Nested: Yes");
                    sb.AppendLine($"  Parent Type: {structType.ParentTypeName}");
                    sb.AppendLine($"  Full Type Name: {structType.FullTypeName}");
                }
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {locationStr}");
                sb.AppendLine($"Fields: {structType.Fields.Count}");
                sb.AppendLine($"Methods: {structType.Methods.Count}");
                list.Add(new CodeItem {
                    Kind = "Struct",
                    Name = structType.Name ?? string.Empty,
                    FilePath = filePath,
                    Location = locationStr,
                    StartLine = structType.Location?.StartLine ?? 0,
                    EndLine = structType.Location?.EndLine ?? 0,
                    ParentClass = structType.IsNested ? (structType.ParentTypeName ?? string.Empty) : string.Empty,

                    Modifiers = structType.Modifiers ?? string.Empty,
                    Text = sb.ToString().TrimEnd('\r', '\n')
                });
            }
            return list;
        }

        public List<CodeItem> FindEnumsAsList(string filePath, ProgrammingLanguage language, string? namePattern = null)
        {
            var parsed = ParseFile(filePath, language);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            var enums = string.IsNullOrEmpty(namePattern)
                ? parsed.Enums
                : parsed.Enums.Where(e => MatchesPattern(e.Name, namePattern)).ToList();
            return BuildEnumItems(enums, filePath);
        }

        public List<CodeItem> FindEnumsInCodeAsList(string code, ProgrammingLanguage language, string? namePattern = null, string fileName = "<text>")
        {
            var parsed = ParseText(code, language, fileName);
            if (CheckParsedFileSyntaxErrors(parsed) != null) return new List<CodeItem>();

            var enums = string.IsNullOrEmpty(namePattern)
                ? parsed.Enums
                : parsed.Enums.Where(e => MatchesPattern(e.Name, namePattern)).ToList();
            return BuildEnumItems(enums, fileName);
        }

        private List<CodeItem> BuildEnumItems(List<EnumInfo> enums, string filePath)
        {
            var list = new List<CodeItem>();
            foreach (var enumType in enums) {
                var locationStr = FormatLocation(enumType.Location);
                var sb = new StringBuilder();
                sb.AppendLine($"Enum: {enumType.Name}");
                if (enumType.IsNested) {
                    sb.AppendLine($"  Nested: Yes");
                    sb.AppendLine($"  Parent Type: {enumType.ParentTypeName}");
                    sb.AppendLine($"  Full Type Name: {enumType.FullTypeName}");
                }
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Location: {locationStr}");
                sb.AppendLine($"Members: {enumType.Members.Count}");
                list.Add(new CodeItem {
                    Kind = "Enum",
                    Name = enumType.Name ?? string.Empty,
                    FilePath = filePath,
                    Location = locationStr,
                    StartLine = enumType.Location?.StartLine ?? 0,
                    EndLine = enumType.Location?.EndLine ?? 0,
                    ParentClass = enumType.IsNested ? (enumType.ParentTypeName ?? string.Empty) : string.Empty,

                    Extra = $"Members={enumType.Members.Count}",
                    Text = sb.ToString().TrimEnd('\r', '\n')
                });
            }
            return list;
        }

        // ====================================================================
        // I. AST Node Search APIs (List)
        // ====================================================================
        public List<CodeItem> FindNodesAsList(string filePath, ProgrammingLanguage language, string pattern)
        {
            if (!File.Exists(filePath))
                return new List<CodeItem>();

            var code = File.ReadAllText(filePath);
            return FindNodesAsListInternal(code, language, pattern, filePath);
        }

        public List<CodeItem> FindNodesInCodeAsList(string code, ProgrammingLanguage language, string pattern, string fileName = "<text>")
        {
            return FindNodesAsListInternal(code, language, pattern, fileName);
        }

        private List<CodeItem> FindNodesAsListInternal(string code, ProgrammingLanguage language, string pattern, string filePath)
        {
            ITreeSitterTree tree;
            try {
                tree = ParseTreeSitter(code, language);
            }
            catch {
                return new List<CodeItem>();
            }

            var matches = new List<(ITreeSitterNode node, string? nameFieldValue)>();
            CollectMatchingNodes(tree.RootNode, code, pattern, matches);

            var sourceLines = code.Split('\n');
            var list = new List<CodeItem>();
            foreach (var (node, nameValue) in matches) {
                int startLine = node.StartPoint.Row + 1;
                int startCol = node.StartPoint.Column;
                int endLine = node.EndPoint.Row + 1;
                int endCol = node.EndPoint.Column;
                int lineCount = endLine - startLine + 1;

                int namedCount = 0;
                int totalCount = 0;
                foreach (var child in node.Children) {
                    totalCount++;
                    if (child.IsNamed)
                        namedCount++;
                }

                string textPreview = string.Empty;
                int lineIndex = startLine - 1;
                if (lineIndex >= 0 && lineIndex < sourceLines.Length) {
                    var lineText = sourceLines[lineIndex].TrimEnd('\r').Trim();
                    if (lineText.Length > 120)
                        lineText = lineText.Substring(0, 120) + "...";
                    if (lineCount > 1)
                        lineText += " ...";
                    textPreview = lineText;
                }

                var parent = node.Parent;
                var item = new CodeItem {
                    Kind = node.Type ?? string.Empty,
                    Name = nameValue ?? string.Empty,
                    FilePath = filePath,
                    Location = $"[{startLine}:{startCol}-{endLine}:{endCol}] ({lineCount} line{(lineCount != 1 ? "s" : "")})",
                    StartLine = startLine,
                    EndLine = endLine,
                    Extra = $"Parent={(parent != null ? parent.Type : "(root)")}; Children={namedCount} named, {totalCount} total",

                    Text = textPreview
                };
                list.Add(item);
            }
            return list;
        }


    }
}
