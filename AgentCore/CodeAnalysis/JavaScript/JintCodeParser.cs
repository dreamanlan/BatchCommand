using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CefDotnetApp.AgentCore.CodeAnalysis.JavaScript;
using CefDotnetApp.AgentCore.Models;
using Esprima;
using Esprima.Ast;

namespace AgentCore.CodeAnalysis
{
    /// <summary>
    /// Jint-based JavaScript parser implementing unified ICodeParser interface
    /// </summary>
    public class JintCodeParser : ICodeParser
    {
        private readonly JavaScriptAnalyzer _analyzer;

        public ProgrammingLanguage Language => ProgrammingLanguage.JavaScript;

        public JintCodeParser()
        {
            _analyzer = new JavaScriptAnalyzer();
        }

        public ParsedCodeFile Parse(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var tree = _analyzer.AnalyzeFile(filePath, out string error);
            var result = ConvertToUnifiedFormat(tree, filePath);

            // Set syntax error info if parsing failed
            if (tree == null && !string.IsNullOrEmpty(error))
            {
                result.HasSyntaxErrors = true;
                result.SyntaxErrors.Add(error);
            }

            return result;
        }

        public ParsedCodeFile ParseText(string code, string? filePath = null)
        {
            var tree = _analyzer.AnalyzeCode(code, out string error);
            var result = ConvertToUnifiedFormat(tree, filePath ?? "<text>");

            // Set syntax error info if parsing failed
            if (tree == null && !string.IsNullOrEmpty(error))
            {
                result.HasSyntaxErrors = true;
                result.SyntaxErrors.Add(error);
            }

            return result;
        }

        public List<FunctionInfo> FindFunctions(ParsedCodeFile parsed, string? namePattern = null, bool ignoreCase = true)
        {
            var result = new List<FunctionInfo>();

            // Include global functions
            if (string.IsNullOrEmpty(namePattern))
            {
                result.AddRange(parsed.Functions);
            }
            else
            {
                result.AddRange(parsed.Functions.Where(f => f.Name != null && MatchesPattern(f.Name, namePattern, ignoreCase)));
            }

            // Include class methods, properties and constructors
            foreach (var type in parsed.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (string.IsNullOrEmpty(namePattern) || MatchesPattern(method.Name, namePattern, ignoreCase))
                    {
                        result.Add(method);
                    }
                }

                foreach (var property in type.Properties)
                {
                    if (string.IsNullOrEmpty(namePattern) || MatchesPattern(property.Name, namePattern, ignoreCase))
                    {
                        result.Add(ConvertPropertyToFunctionInfo(property));
                    }
                }

                foreach (var constructor in type.Constructors)
                {
                    if (string.IsNullOrEmpty(namePattern) || MatchesPattern(constructor.Name, namePattern, ignoreCase))
                    {
                        result.Add(constructor);
                    }
                }
            }

            return result;
        }

        private FunctionInfo ConvertPropertyToFunctionInfo(PropertyInfo property)
        {
            var funcInfo = new FunctionInfo
            {
                Name = property.Name,
                ReturnType = property.Type,
                Location = property.Location,
                Modifiers = property.Modifiers,
                Language = property.Language,
                NativeObject = property.NativeObject,
                DocumentationComment = property.DocumentationComment,
                IsStatic = property.IsStatic,
                IsPublic = property.IsPublic
            };

            return funcInfo;
        }

        public List<TypeInfo> FindTypes(ParsedCodeFile parsed, string? namePattern = null, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(namePattern))
            {
                return parsed.Types;
            }

            return parsed.Types
                .Where(t => t.Name != null && MatchesPattern(t.Name, namePattern, ignoreCase))
                .ToList();
        }

        public FunctionInfo? FindFunction(ParsedCodeFile parsed, string functionName, bool ignoreCase = true)
        {
            // First, search in global functions
            foreach (var func in parsed.Functions)
            {
                if (MatchesPattern(func.Name, functionName, ignoreCase))
                {
                    return func;
                }
            }

            // If not found, search in class methods, properties and constructors
            foreach (var type in parsed.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (MatchesPattern(method.Name, functionName, ignoreCase))
                    {
                        return method;
                    }
                }

                foreach (var property in type.Properties)
                {
                    if (MatchesPattern(property.Name, functionName, ignoreCase))
                    {
                        return ConvertPropertyToFunctionInfo(property);
                    }
                }

                foreach (var constructor in type.Constructors)
                {
                    if (MatchesPattern(constructor.Name, functionName, ignoreCase))
                    {
                        return constructor;
                    }
                }
            }

            return null;
        }

        public TypeInfo? FindType(ParsedCodeFile parsed, string typeName, bool ignoreCase = true)
        {
            return parsed.Types.FirstOrDefault(t => MatchesPattern(t.Name, typeName, ignoreCase));
        }

        private ParsedCodeFile ConvertToUnifiedFormat(JsTreeWrapper? tree, string filePath)
        {
            var result = new ParsedCodeFile(filePath, ProgrammingLanguage.JavaScript)
            {
                NativeSyntaxTree = tree
            };

            if (tree == null)
                return result;

            // Extract functions
            var functions = tree.GetFunctions();
            foreach (var func in functions)
            {
                var funcInfo = new FunctionInfo
                {
                    Name = func.Name,
                    ReturnType = "any",
                    Language = ProgrammingLanguage.JavaScript,
                    NativeObject = func,
                    Location = CreateLocationFromEsprima(func.Location, filePath),
                    FullText = func.GetText(),
                    Modifiers = func.Type
                };

                if (func.Parameters != null)
                {
                    foreach (var param in func.Parameters)
                    {
                        funcInfo.Parameters.Add(new ParameterInfo
                        {
                            Name = param,
                            Type = "any"
                        });
                    }
                }

                result.Functions.Add(funcInfo);
            }

            // Extract classes
            var classes = tree.GetClasses();
            foreach (var cls in classes)
            {
                var typeInfo = new TypeInfo
                {
                    Name = cls.Name,
                    Type = CodeElementType.Class,
                    Language = ProgrammingLanguage.JavaScript,
                    NativeObject = cls,
                    Location = CreateLocationFromEsprima(cls.Location, filePath)
                };

                if (!string.IsNullOrEmpty(cls.SuperClass))
                {
                    typeInfo.BaseTypes.Add(cls.SuperClass);
                }

                // Process fields
                if (cls.FieldDetails != null)
                {
                    foreach (var field in cls.FieldDetails)
                    {
                        var fieldInfo = new FieldInfo
                        {
                            Name = field.Name,
                            Type = "any",
                            Language = ProgrammingLanguage.JavaScript,
                            Location = CreateLocationFromEsprima(field.Location, filePath),
                            Modifiers = field.IsStatic ? "static" : ""
                        };

                        typeInfo.Fields.Add(fieldInfo);
                    }
                }

                // Process methods, constructors, and properties (getters/setters)
                if (cls.MethodDetails != null)
                {
                    // Group getters and setters by name to create PropertyInfo
                    var propertyDict = new Dictionary<string, PropertyInfo>();

                    foreach (var method in cls.MethodDetails)
                    {
                        // Handle getters and setters as properties
                        if (method.Kind == "get" || method.Kind == "set")
                        {
                            if (!propertyDict.ContainsKey(method.Name))
                            {
                                propertyDict[method.Name] = new PropertyInfo
                                {
                                    Name = method.Name,
                                    Type = "any",
                                    Language = ProgrammingLanguage.JavaScript,
                                    Location = CreateLocationFromEsprima(method.Location, filePath),
                                    HasGetter = false,
                                    HasSetter = false
                                };
                            }

                            var propInfo = propertyDict[method.Name];
                            if (method.Kind == "get")
                            {
                                propInfo.HasGetter = true;
                            }
                            else if (method.Kind == "set")
                            {
                                propInfo.HasSetter = true;
                            }
                        }
                        else
                        {
                            // Handle constructors and regular methods
                            var methodInfo = new FunctionInfo
                            {
                                Name = method.Name,
                                ReturnType = "any",
                                Language = ProgrammingLanguage.JavaScript,
                                Location = CreateLocationFromEsprima(method.Location, filePath),
                                Modifiers = method.Kind
                            };

                            if (method.Parameters != null)
                            {
                                foreach (var param in method.Parameters)
                                {
                                    methodInfo.Parameters.Add(new ParameterInfo
                                    {
                                        Name = param,
                                        Type = "any"
                                    });
                                }
                            }

                            // Distinguish between constructors and regular methods
                            if (method.Kind == "constructor")
                            {
                                typeInfo.Constructors.Add(methodInfo);
                            }
                            else
                            {
                                typeInfo.Methods.Add(methodInfo);
                            }
                        }
                    }

                    // Add all properties to typeInfo
                    foreach (var prop in propertyDict.Values)
                    {
                        typeInfo.Properties.Add(prop);
                    }
                }

                result.Types.Add(typeInfo);
            }

            // Extract variables (store in metadata for JavaScript)
            var variables = tree.GetVariables();
            var variableList = new List<string>();
            foreach (var variable in variables)
            {
                variableList.Add($"{variable.Kind} {variable.Name}");
            }
            result.Metadata["Variables"] = string.Join(", ", variableList);

            // Extract imports
            var imports = tree.GetImports();
            foreach (var import in imports)
            {
                result.Imports.Add(import.Source);
            }

            // Get metrics from tree
            result.TotalLines = tree.SourceCode.Split('\n').Length;
            result.Metadata["FunctionCount"] = functions.Count.ToString();
            result.Metadata["ClassCount"] = classes.Count.ToString();
            result.Metadata["VariableCount"] = variables.Count.ToString();
            result.Metadata["ImportCount"] = imports.Count.ToString();

            return result;
        }

        private CodeLocation CreateLocationFromEsprima(Location? location, string filePath)
        {
            if (location == null || !location.HasValue)
                return new CodeLocation(filePath ?? "<text>", 0, 0, 0, 0);

            var loc = location.Value;
            return new CodeLocation(
                filePath ?? "<text>",
                loc.Start.Line,
                loc.End.Line,
                loc.Start.Column,
                loc.End.Column
            );
        }

        private bool MatchesPattern(string name, string pattern, bool ignoreCase = true)
        {
            try
            {
                var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                return Regex.IsMatch(name, pattern, options);
            }
            catch (ArgumentException)
            {
                var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return name.IndexOf(pattern, comparison) >= 0;
            }
        }    }
}
