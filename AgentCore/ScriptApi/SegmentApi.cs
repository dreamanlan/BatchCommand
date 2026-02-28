using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // tokenize(text) - segment mixed Chinese/English text into a list of tokens
    sealed class TokenizeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: tokenize(text)");
                return BoxedValue.FromObject(new List<BoxedValue>());
            }

            {
                try {
                    string text = operands[0].AsString;
                    var tokens = Core.AgentCore.Instance.MixedSegmenter.Segment(text);
                    var list = new List<BoxedValue>(tokens.Count);
                    foreach (var t in tokens)
                        list.Add(BoxedValue.FromString(t));
                    return BoxedValue.FromObject(list);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"tokenize error: {ex.Message}");
                }
            }
            return BoxedValue.FromObject(new List<BoxedValue>());
        }
    }

    // set_help_semantic_search(type) - set help search mode: 0=BagOfWords, 1=TfIdf, 2=Embedding
    sealed class SetHelpSemanticSearchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_help_semantic_search(type)");
                return BoxedValue.FromString("[error] missing argument");
            }

            {
                int type = operands[0].GetInt();
                Core.AgentCore.Instance.HelpSearchMode = (Core.HelpSearchType)type;
                return BoxedValue.FromString("ok");
            }
        }
    }

    // set_help_reranker(enable) - enable or disable reranker for help search: 1=enable, 0=disable
    sealed class SetHelpRerankerExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_help_reranker(enable)");
                return BoxedValue.FromString("[error] missing argument");
            }

            {
                Core.AgentCore.Instance.HelpUseReranker = operands[0].GetInt() != 0;
                return BoxedValue.FromString("ok");
            }
        }
    }

    public static class SegmentApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("tokenize", "tokenize(text) => list", new ExpressionFactoryHelper<TokenizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_help_semantic_search", "set_help_semantic_search(type) - 0=BagOfWords,1=TfIdf,2=Embedding", new ExpressionFactoryHelper<SetHelpSemanticSearchExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_help_reranker", "set_help_reranker(enable) - 1=enable,0=disable", new ExpressionFactoryHelper<SetHelpRerankerExp>());
        }
    }
}
