using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AgentCore.Tools
{
    /// <summary>
    /// Diagnostic tool to inspect TreeSitterSharp language DLLs
    /// </summary>
    public static class TreeSitterLanguageDiagnostics
    {
        /// <summary>
        /// Inspect TreeSitterSharp.C assembly
        /// </summary>
        public static string InspectCLanguageAssembly()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== TreeSitterSharp.C Assembly Inspection ===");
            sb.AppendLine();

            try
            {
                var assembly = Assembly.Load("TreeSitterSharp.C");
                sb.AppendLine($"Assembly: {assembly.FullName}");
                sb.AppendLine($"Location: {assembly.Location}");
                sb.AppendLine();

                var types = assembly.GetTypes();
                sb.AppendLine($"Total types: {types.Length}");
                sb.AppendLine();

                foreach (var type in types.OrderBy(t => t.FullName))
                {
                    sb.AppendLine($"Type: {type.FullName}");
                    sb.AppendLine($"  IsPublic: {type.IsPublic}");
                    sb.AppendLine($"  IsClass: {type.IsClass}");
                    sb.AppendLine($"  IsInterface: {type.IsInterface}");

                    if (type.BaseType != null)
                    {
                        sb.AppendLine($"  BaseType: {type.BaseType.FullName}");
                    }

                    // List static properties
                    var staticProps = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
                    if (staticProps.Length > 0)
                    {
                        sb.AppendLine("  Static Properties:");
                        foreach (var prop in staticProps)
                        {
                            sb.AppendLine($"    {prop.PropertyType.Name} {prop.Name}");
                        }
                    }

                    // List constructors
                    var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                    if (ctors.Length > 0)
                    {
                        sb.AppendLine("  Constructors:");
                        foreach (var ctor in ctors)
                        {
                            var parameters = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                            sb.AppendLine($"    {type.Name}({parameters})");
                        }
                    }

                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error: {ex.Message}");
                sb.AppendLine($"Stack: {ex.StackTrace}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Inspect TreeSitterSharp.Cpp assembly
        /// </summary>
        public static string InspectCppLanguageAssembly()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== TreeSitterSharp.Cpp Assembly Inspection ===");
            sb.AppendLine();

            try
            {
                var assembly = Assembly.Load("TreeSitterSharp.Cpp");
                sb.AppendLine($"Assembly: {assembly.FullName}");
                sb.AppendLine($"Location: {assembly.Location}");
                sb.AppendLine();

                var types = assembly.GetTypes();
                sb.AppendLine($"Total types: {types.Length}");
                sb.AppendLine();

                foreach (var type in types.OrderBy(t => t.FullName))
                {
                    sb.AppendLine($"Type: {type.FullName}");
                    sb.AppendLine($"  IsPublic: {type.IsPublic}");
                    sb.AppendLine($"  IsClass: {type.IsClass}");
                    sb.AppendLine($"  IsInterface: {type.IsInterface}");

                    if (type.BaseType != null)
                    {
                        sb.AppendLine($"  BaseType: {type.BaseType.FullName}");
                    }

                    // List static properties
                    var staticProps = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
                    if (staticProps.Length > 0)
                    {
                        sb.AppendLine("  Static Properties:");
                        foreach (var prop in staticProps)
                        {
                            sb.AppendLine($"    {prop.PropertyType.Name} {prop.Name}");
                        }
                    }

                    // List constructors
                    var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                    if (ctors.Length > 0)
                    {
                        sb.AppendLine("  Constructors:");
                        foreach (var ctor in ctors)
                        {
                            var parameters = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                            sb.AppendLine($"    {type.Name}({parameters})");
                        }
                    }

                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error: {ex.Message}");
                sb.AppendLine($"Stack: {ex.StackTrace}");
            }

            return sb.ToString();
        }
    }
}
