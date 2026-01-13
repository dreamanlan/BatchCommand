using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    public class TemplateEngine : ITemplateEngine
    {
        private readonly Dictionary<string, string> _templates;
        private readonly object _lockObject = new object();
        private readonly string _basePath;

        public TemplateEngine(string basePath)
        {
            _templates = new Dictionary<string, string>();
            _basePath = basePath ?? Directory.GetCurrentDirectory();
        }

        public bool RegisterTemplate(string name, string template)
        {
            lock (_lockObject)
            {
                _templates[name] = template;
                return true;
            }
        }

        public bool LoadTemplateFromFile(string name, string filePath)
        {
            try
            {
                var fullPath = Path.IsPathRooted(filePath) ? filePath : Path.Combine(_basePath, filePath);
                var template = File.ReadAllText(fullPath, Encoding.UTF8);
                return RegisterTemplate(name, template);
            }
            catch
            {
                return false;
            }
        }

        public string GetTemplate(string name)
        {
            lock (_lockObject)
            {
                return _templates.ContainsKey(name) ? _templates[name] : null;
            }
        }

        public bool RemoveTemplate(string name)
        {
            lock (_lockObject)
            {
                return _templates.Remove(name);
            }
        }

        public List<string> GetAllTemplateNames()
        {
            lock (_lockObject)
            {
                return new List<string>(_templates.Keys);
            }
        }

        public string Render(string templateName, Dictionary<string, object> variables)
        {
            lock (_lockObject)
            {
                if (!_templates.ContainsKey(templateName))
                    return null;

                var template = _templates[templateName];
                return RenderTemplate(template, variables);
            }
        }

        public string RenderTemplate(string template, Dictionary<string, object> variables)
        {
            if (string.IsNullOrEmpty(template))
                return template;

            var result = template;

            // Replace simple variables: {{variableName}}
            var simplePattern = @"\{\{([a-zA-Z_][a-zA-Z0-9_]*)\}\}";
            result = Regex.Replace(result, simplePattern, match =>
            {
                var varName = match.Groups[1].Value;
                if (variables.ContainsKey(varName))
                {
                    return variables[varName]?.ToString() ?? string.Empty;
                }
                return match.Value;
            });

            // Replace conditional blocks: {{#if condition}}...{{/if}}
            var ifPattern = @"\{\{#if\s+([a-zA-Z_][a-zA-Z0-9_]*)\}\}(.*?)\{\{/if\}\}";
            result = Regex.Replace(result, ifPattern, match =>
            {
                var varName = match.Groups[1].Value;
                var content = match.Groups[2].Value;

                if (variables.ContainsKey(varName))
                {
                    var value = variables[varName];
                    if (value is bool boolValue && boolValue)
                        return content;
                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                        return content;
                }
                return string.Empty;
            }, RegexOptions.Singleline);

            // Replace loop blocks: {{#each items}}...{{/each}}
            var eachPattern = @"\{\{#each\s+([a-zA-Z_][a-zA-Z0-9_]*)\}\}(.*?)\{\{/each\}\}";
            result = Regex.Replace(result, eachPattern, match =>
            {
                var varName = match.Groups[1].Value;
                var content = match.Groups[2].Value;

                if (variables.ContainsKey(varName))
                {
                    var value = variables[varName];
                    if (value is System.Collections.IEnumerable enumerable)
                    {
                        var sb = new StringBuilder();
                        int index = 0;
                        foreach (var item in enumerable)
                        {
                            var itemVars = new Dictionary<string, object>(variables)
                            {
                                ["item"] = item,
                                ["index"] = index
                            };
                            sb.Append(RenderTemplate(content, itemVars));
                            index++;
                        }
                        return sb.ToString();
                    }
                }
                return string.Empty;
            }, RegexOptions.Singleline);

            // Replace item references in loops: {{item.property}}
            var itemPattern = @"\{\{item\.([a-zA-Z_][a-zA-Z0-9_]*)\}\}";
            result = Regex.Replace(result, itemPattern, match =>
            {
                var propName = match.Groups[1].Value;
                if (variables.ContainsKey("item"))
                {
                    var item = variables["item"];
                    if (item != null)
                    {
                        var prop = item.GetType().GetProperty(propName);
                        if (prop != null)
                        {
                            return prop.GetValue(item)?.ToString() ?? string.Empty;
                        }
                    }
                }
                return match.Value;
            });

            return result;
        }

        public string RenderFromFile(string templatePath, Dictionary<string, object> variables)
        {
            try
            {
                var fullPath = Path.IsPathRooted(templatePath) ? templatePath : Path.Combine(_basePath, templatePath);
                var template = File.ReadAllText(fullPath, Encoding.UTF8);
                return RenderTemplate(template, variables);
            }
            catch
            {
                return null;
            }
        }

        public bool SaveRenderedTemplate(string templateName, Dictionary<string, object> variables, string outputPath)
        {
            try
            {
                var rendered = Render(templateName, variables);
                if (rendered == null)
                    return false;

                var fullPath = Path.IsPathRooted(outputPath) ? outputPath : Path.Combine(_basePath, outputPath);
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(fullPath, rendered, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ClearAllTemplates()
        {
            lock (_lockObject)
            {
                _templates.Clear();
            }
        }

        public static string CreateClassTemplate(string className, string namespaceName, List<string> properties)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"using System;");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");

            foreach (var prop in properties)
            {
                sb.AppendLine($"        public {prop} {{ get; set; }}");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string CreateFunctionTemplate(string functionName, string returnType, List<string> parameters, string body = null)
        {
            var sb = new StringBuilder();
            var paramStr = string.Join(", ", parameters);

            sb.AppendLine($"public {returnType} {functionName}({paramStr})");
            sb.AppendLine("{");
            if (!string.IsNullOrEmpty(body))
            {
                sb.AppendLine($"    {body}");
            }
            else
            {
                if (returnType != "void")
                {
                    sb.AppendLine($"    // TODO: Implement {functionName}");
                    sb.AppendLine($"    throw new NotImplementedException();");
                }
            }
            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string CreateApiEndpointTemplate(string methodName, string httpMethod, string route, string requestType, string responseType)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"[Http{httpMethod}(\"{route}\")]");
            sb.AppendLine($"public async Task<ActionResult<{responseType}>> {methodName}([FromBody] {requestType} request)");
            sb.AppendLine("{");
            sb.AppendLine("    try");
            sb.AppendLine("    {");
            sb.AppendLine($"        // TODO: Implement {methodName}");
            sb.AppendLine($"        var response = new {responseType}();");
            sb.AppendLine("        return Ok(response);");
            sb.AppendLine("    }");
            sb.AppendLine("    catch (Exception ex)");
            sb.AppendLine("    {");
            sb.AppendLine("        return StatusCode(500, ex.Message);");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
