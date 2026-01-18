using System;
using System.Linq;
using System.Text;
using Esprima;
using Esprima.Ast;
using CefDotnetApp.AgentCore.CodeAnalysis.JavaScript;

namespace AgentCore.Tools
{
    public class TestEsprima
    {
        /// <summary>
        /// Test JavaScript properties (getter/setter) parsing
        /// </summary>
        public static string TestJavaScriptProperties()
        {
            try {
                var sb = new StringBuilder();
                sb.AppendLine("=== Testing JavaScript Properties (Getter/Setter) ===");
                sb.AppendLine();

                // Embedded test JavaScript code
                string jsCode = @"class TestProperties {
    // Fields
    publicField = 10;
    static staticField = 20;

    // Constructor
    constructor() {
        this.constructorField = 30;
    }

    // Getter (property-like)
    get myProperty() {
        return this.publicField * 2;
    }

    // Setter (property-like)
    set myProperty(value) {
        this.publicField = value / 2;
    }

    // Static getter
    static get staticProperty() {
        return TestProperties.staticField * 3;
    }

    // Regular method
    myMethod() {
        return this.publicField;
    }

    // Static method
    static staticMethod() {
        return TestProperties.staticField;
    }
}";

                sb.AppendLine("Source Code:");
                sb.AppendLine("─────────────────────────────────────────────────");
                sb.AppendLine(jsCode);
                sb.AppendLine("─────────────────────────────────────────────────");
                sb.AppendLine();

                // Parse with Jint parser
                var jintParser = new AgentCore.CodeAnalysis.JintCodeParser();
                var parsedFile = jintParser.ParseText(jsCode, "test_js_properties.js");

                sb.AppendLine("Extracted Symbols:");
                sb.AppendLine();

                // Debug: Show raw method kinds from Esprima
                if (parsedFile.NativeSyntaxTree is JsTreeWrapper tree)
                {
                    var classes = tree.GetClasses();
                    if (classes.Count > 0)
                    {
                        sb.AppendLine("=== DEBUG: Raw Method Kinds from Esprima ===");
                        foreach (var cls in classes)
                        {
                            sb.AppendLine($"Class: {cls.Name}");
                            if (cls.MethodDetails != null)
                            {
                                foreach (var method in cls.MethodDetails)
                                {
                                    sb.AppendLine($"  - {method.Name}: Kind = '{method.Kind}'");
                                }
                            }
                        }
                        sb.AppendLine();
                    }
                }


                // Check classes
                if (parsedFile.Types.Count > 0) {
                    sb.AppendLine($"✓ Found {parsedFile.Types.Count} class(es):");
                    foreach (var type in parsedFile.Types) {
                        sb.AppendLine($"  • Class: {type.Name}");
                        sb.AppendLine($"    Line: {type.Location?.StartLine ?? 0}");
                        sb.AppendLine();

                        // Check fields
                        if (type.Fields != null && type.Fields.Count > 0) {
                            sb.AppendLine($"    Fields ({type.Fields.Count}):");
                            foreach (var field in type.Fields) {
                                string modifier = field.IsStatic ? "static " : "";
                                sb.AppendLine($"      - {modifier}{field.Name} (Line {field.Location?.StartLine ?? 0})");
                            }
                            sb.AppendLine();
                        } else {
                            sb.AppendLine("    ✗ No fields found");
                            sb.AppendLine();
                        }

                        // Check properties (getter/setter)
                        if (type.Properties != null && type.Properties.Count > 0) {
                            sb.AppendLine($"    Properties ({type.Properties.Count}):");
                            foreach (var prop in type.Properties) {
                                string modifier = prop.IsStatic ? "static " : "";
                                string accessors = "";
                                if (prop.HasGetter && prop.HasSetter) {
                                    accessors = "{ get; set; }";
                                } else if (prop.HasGetter) {
                                    accessors = "{ get; }";
                                } else if (prop.HasSetter) {
                                    accessors = "{ set; }";
                                }
                                sb.AppendLine($"      - {modifier}{prop.Name} {accessors} (Line {prop.Location?.StartLine ?? 0})");
                            }
                            sb.AppendLine();
                        } else {
                            sb.AppendLine("    ✗ No properties found");
                            sb.AppendLine();
                        }

                        // Check constructors
                        if (type.Constructors != null && type.Constructors.Count > 0) {
                            sb.AppendLine($"    Constructors ({type.Constructors.Count}):");
                            foreach (var ctor in type.Constructors) {
                                sb.AppendLine($"      - constructor() (Line {ctor.Location?.StartLine ?? 0})");
                            }
                            sb.AppendLine();
                        } else {
                            sb.AppendLine("    ✗ No constructors found");
                            sb.AppendLine();
                        }

                        // Check methods
                        if (type.Methods != null && type.Methods.Count > 0) {
                            sb.AppendLine($"    Methods ({type.Methods.Count}):");
                            foreach (var method in type.Methods) {
                                string modifier = method.IsStatic ? "static " : "";
                                sb.AppendLine($"      - {modifier}{method.Name}() (Line {method.Location?.StartLine ?? 0})");
                            }
                            sb.AppendLine();
                        } else {
                            sb.AppendLine("    ✗ No methods found");
                            sb.AppendLine();
                        }
                    }
                } else {
                    sb.AppendLine("✗ No classes found");
                }

                sb.AppendLine();
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine("Expected Results:");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine("• Class: TestProperties");
                sb.AppendLine("  - Fields: publicField, staticField, constructorField");
                sb.AppendLine("  - Properties: myProperty (get/set), staticProperty (get)");
                sb.AppendLine("  - Constructors: constructor");
                sb.AppendLine("  - Methods: myMethod, staticMethod");
                sb.AppendLine();
                sb.AppendLine("Verification:");
                sb.AppendLine("• Properties should show getter/setter info");
                sb.AppendLine("• Static properties should be marked as static");
                sb.AppendLine("• Properties should be separate from methods");

                return sb.ToString();
            }
            catch (Exception ex) {
                return $"✗ Error testing JavaScript properties: {ex.Message}\nStack: {ex.StackTrace}";
            }
        }
    }
}
