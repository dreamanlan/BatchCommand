using System;
using System.Collections.Generic;
using System.Linq;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // findsymbol(path, symbolName, symbolType) - find symbol definition location
    sealed class FindSymbolExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                string symbolName = operands[1].AsString;
                string symbolType = operands.Count > 2 ? operands[2].AsString : "";

                var location = Core.AgentCore.Instance.FileOps.FindSymbolDefinition(path, symbolName, symbolType);
                if (location == null)
                    return BoxedValue.NullObject;

                var result = new Dictionary<string, object>
                {
                    { "line", location.LineNumber },
                    { "column", location.Column },
                    { "length", location.Length },
                    { "context", location.Context },
                    { "symbol", location.SymbolName }
                };

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"findsymbol error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // findusages(path, symbolName) - find all symbol usages
    sealed class FindUsagesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                string symbolName = operands[1].AsString;

                var usages = Core.AgentCore.Instance.FileOps.FindSymbolUsages(path, symbolName);
                var result = new List<object>();

                foreach (var usage in usages)
                {
                    var item = new Dictionary<string, object>
                    {
                        { "line", usage.LineNumber },
                        { "column", usage.Column },
                        { "length", usage.Length },
                        { "context", usage.Context },
                        { "symbol", usage.SymbolName }
                    };
                    result.Add(item);
                }

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"findusages error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // getfunctions(path) - get all functions in file
    sealed class GetFunctionsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                var functions = Core.AgentCore.Instance.FileOps.GetAllFunctions(path);
                var result = new List<object>();

                foreach (var func in functions)
                {
                    var item = new Dictionary<string, object>
                    {
                        { "name", func.Name },
                        { "returnType", func.ReturnType },
                        { "modifiers", func.Modifiers },
                        { "startLine", func.StartLine },
                        { "startColumn", func.StartColumn },
                        { "endLine", func.EndLine }
                    };
                    result.Add(item);
                }

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"getfunctions error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // getclasses(path) - get all classes in file
    sealed class GetClassesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                var classes = Core.AgentCore.Instance.FileOps.GetAllClasses(path);
                var result = new List<object>();

                foreach (var cls in classes)
                {
                    var item = new Dictionary<string, object>
                    {
                        { "name", cls.Name },
                        { "type", cls.Type },
                        { "startLine", cls.StartLine },
                        { "startColumn", cls.StartColumn },
                        { "endLine", cls.EndLine }
                    };
                    result.Add(item);
                }

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"getclasses error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // findfunction(path, functionName) - find function and its range
    sealed class FindFunctionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                string functionName = operands[1].AsString;

                var range = Core.AgentCore.Instance.FileOps.FindFunctionRange(path, functionName);
                if (range == null)
                    return BoxedValue.NullObject;

                var result = new Dictionary<string, object>
                {
                    { "name", range.Name },
                    { "returnType", range.ReturnType },
                    { "modifiers", range.Modifiers },
                    { "startLine", range.StartLine },
                    { "endLine", range.EndLine }
                };

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"findfunction error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // findclass(path, className) - find class and its range
    sealed class FindClassExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                string className = operands[1].AsString;

                var range = Core.AgentCore.Instance.FileOps.FindClassRange(path, className);
                if (range == null)
                    return BoxedValue.NullObject;

                var result = new Dictionary<string, object>
                {
                    { "name", range.Name },
                    { "type", range.Type },
                    { "startLine", range.StartLine },
                    { "endLine", range.EndLine }
                };

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"findclass error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // getimports(path) - get all import statements
    sealed class GetImportsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string path = operands[0].AsString;
                var imports = Core.AgentCore.Instance.FileOps.GetImportStatements(path);
                var result = new List<object>();

                foreach (var imp in imports)
                {
                    result.Add(imp);
                }

                return BoxedValue.FromObject(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"getimports error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // addimport(path, importStatement) - add import statement
    sealed class AddImportExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string path = operands[0].AsString;
                string importStatement = operands[1].AsString;

                bool result = Core.AgentCore.Instance.FileOps.AddImportStatement(path, importStatement);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"addimport error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }
}
