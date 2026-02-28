using DotnetStoryScript;
using DotnetStoryScript.DslExpression;

namespace AgentPlugin.Abstractions
{
    /// <summary>
    /// Abstraction for DSL script engine operations (load and call DSL functions).
    /// </summary>
    public interface IDslEngine
    {
        void Register(string name, string doc, IExpressionFactory factory);
        void Register(string name, string doc, bool addToUserApiDoc, IExpressionFactory factory);
        string LoadDslFunc(string func, string code, IList<string> paramNames, bool update);
        string CallDslFunc(string func, List<string> args);
        string ExecuteMetaDslScript(string script);
    }
}
