using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // ========== Context Management APIs ==========

    // Create workspace
    sealed class CreateWorkspaceExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string name = operands[0].AsString;
                string rootPath = operands[1].AsString;

                var workspace = Core.AgentCore.Instance.ContextManager.CreateWorkspace(name, rootPath);
                return BoxedValue.FromString(workspace.Id);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"CreateWorkspace error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Set context variable
    sealed class SetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string key = operands[0].AsString;
                object value = operands[1].GetObject();
                string scopeStr = operands.Count > 2 ? operands[2].AsString : "session";

                ContextScope scope = scopeStr.ToLower() == "workspace" ? ContextScope.Workspace : ContextScope.Session;
                bool result = Core.AgentCore.Instance.ContextManager.SetContextVariable(key, value, scope);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"SetContextVar error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Get context variable
    sealed class GetContextVarExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string key = operands[0].AsString;
                string scopeStr = operands.Count > 1 ? operands[1].AsString : "session";

                ContextScope scope = scopeStr.ToLower() == "workspace" ? ContextScope.Workspace : ContextScope.Session;
                object value = Core.AgentCore.Instance.ContextManager.GetContextVariable(key, scope);
                return value != null ? BoxedValue.FromObject(value) : BoxedValue.NullObject;
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"GetContextVar error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Add open file
    sealed class AddOpenFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string filePath = operands[0].AsString;
                bool result = Core.AgentCore.Instance.ContextManager.AddOpenFile(filePath);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"AddOpenFile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Get open files
    sealed class GetOpenFilesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                var files = Core.AgentCore.Instance.ContextManager.GetOpenFiles();
                return BoxedValue.FromObject(files);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"GetOpenFiles error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Add recent file
    sealed class AddRecentFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string filePath = operands[0].AsString;
                bool result = Core.AgentCore.Instance.ContextManager.AddRecentFile(filePath);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"AddRecentFile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Get recent files
    sealed class GetRecentFilesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                int maxCount = operands.Count > 0 ? operands[0].GetInt() : 10;
                var files = Core.AgentCore.Instance.ContextManager.GetRecentFiles(maxCount);
                return BoxedValue.FromObject(files);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"GetRecentFiles error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // ========== Template Engine APIs ==========

    // Register template
    sealed class RegisterTemplateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string name = operands[0].AsString;
                string template = operands[1].AsString;

                bool result = Core.AgentCore.Instance.TemplateEngine.RegisterTemplate(name, template);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"RegisterTemplate error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Load template from file
    sealed class LoadTemplateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string name = operands[0].AsString;
                string filePath = operands[1].AsString;

                bool result = Core.AgentCore.Instance.TemplateEngine.LoadTemplateFromFile(name, filePath);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"LoadTemplate error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Render template
    sealed class RenderTemplateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string templateName = operands[0].AsString;
                var varsObj = operands[1].GetObject();

                Dictionary<string, object> variables = new Dictionary<string, object>();
                if (varsObj is Dictionary<string, object> dict)
                {
                    variables = dict;
                }

                string result = Core.AgentCore.Instance.TemplateEngine.Render(templateName, variables);
                return result != null ? BoxedValue.FromString(result) : BoxedValue.NullObject;
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"RenderTemplate error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Render template string
    sealed class RenderTemplateStringExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string template = operands[0].AsString;
                var varsObj = operands[1].GetObject();

                Dictionary<string, object> variables = new Dictionary<string, object>();
                if (varsObj is Dictionary<string, object> dict)
                {
                    variables = dict;
                }

                string result = Core.AgentCore.Instance.TemplateEngine.RenderTemplate(template, variables);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"RenderTemplateString error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Save rendered template
    sealed class SaveRenderedTemplateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.From(false);

            try
            {
                string templateName = operands[0].AsString;
                var varsObj = operands[1].GetObject();
                string outputPath = operands[2].AsString;

                Dictionary<string, object> variables = new Dictionary<string, object>();
                if (varsObj is Dictionary<string, object> dict)
                {
                    variables = dict;
                }

                bool result = Core.AgentCore.Instance.TemplateEngine.SaveRenderedTemplate(templateName, variables, outputPath);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"SaveRenderedTemplate error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Create class template
    sealed class CreateClassTemplateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.NullObject;

            try
            {
                string className = operands[0].AsString;
                string namespaceName = operands[1].AsString;
                var propsObj = operands[2].GetObject();

                List<string> properties = new List<string>();
                if (propsObj is List<object> list)
                {
                    foreach (var item in list)
                    {
                        properties.Add(item.ToString());
                    }
                }

                string result = TemplateEngine.CreateClassTemplate(className, namespaceName, properties);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"CreateClassTemplate error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // Create function template
    sealed class CreateFunctionTemplateExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.NullObject;

            try
            {
                string functionName = operands[0].AsString;
                string returnType = operands[1].AsString;
                var paramsObj = operands[2].GetObject();
                string body = operands.Count > 3 ? operands[3].AsString : null;

                List<string> parameters = new List<string>();
                if (paramsObj is List<object> list)
                {
                    foreach (var item in list)
                    {
                        parameters.Add(item.ToString());
                    }
                }

                string result = TemplateEngine.CreateFunctionTemplate(functionName, returnType, parameters, body);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"CreateFunctionTemplate error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }
}
