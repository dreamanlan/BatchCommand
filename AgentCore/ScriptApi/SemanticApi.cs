using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using System.Linq;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    /// <summary>
    /// DSL Script API for semantic index operations (SQLite + HNSW).
    /// All APIs use a 'collection' parameter as namespace.
    /// </summary>

    // ---- Helper for converting SearchResultItem list to BoxedValue list ----
    static class SearchResultHelper
    {
        public static BoxedValue ToBoxedList(List<SearchResultItem> items)
        {
            var list = new List<BoxedValue>(items.Count);
            foreach (var item in items) {
                list.Add(BoxedValue.FromObject(item));
            }
            return BoxedValue.FromObject(list);
        }

        public static BoxedValue ToBoxedListOrderByTime(List<SearchResultItem> items, bool isAsc)
        {
            List<SearchResultItem> sorted;
            if (isAsc)
                sorted = items.OrderBy(r => r.CreatedAt).ToList();
            else
                sorted = items.OrderByDescending(r => r.CreatedAt).ToList();
            return ToBoxedList(sorted);
        }
    }

    // semantic_init(collection) - start background loading of a collection
    sealed class SemanticInitExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_init(collection)");
                return BoxedValue.From(false);
            }

            {
                try {
                    string collection = operands[0].AsString;
                    Core.AgentCore.Instance.SemanticIndex.InitCollection(collection);
                    return BoxedValue.From(true);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_init error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    // semantic_is_ready(collection) - check if collection is loaded
    sealed class SemanticIsReadyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_is_ready(collection)");
                return BoxedValue.From(false);
            }

            {
                try {
                    string collection = operands[0].AsString;
                    return BoxedValue.From(Core.AgentCore.Instance.SemanticIndex.IsReady(collection));
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_is_ready error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    // semantic_add(collection, content [, metadata]) - add a record, returns id
    sealed class SemanticAddExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_add(collection, content[, metadata])");
                return BoxedValue.FromString("[error] missing arguments");
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string content = operands[1].AsString;
                    string? metadata = operands.Count > 2 ? operands[2].AsString : null;

                    var embedding = Core.AgentCore.Instance.EmbeddingService;
                    if (!embedding.IsReady)
                        return BoxedValue.FromString("[error] embedding model not ready");

                    float[]? vector = embedding.Encode(content);
                    if (vector == null)
                        return BoxedValue.FromString("[error] encode failed");

                    string id = Core.AgentCore.Instance.SemanticIndex.Add(collection, content, vector, metadata);
                    return BoxedValue.FromString(id);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_add error: {ex.Message}");
                    return BoxedValue.FromString($"[error] {ex.Message}");
                }
            }
        }
    }

    // semantic_search(collection, query[, topN]) - search, returns JSON array
    sealed class SemanticSearchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_search(collection, query[, topN])");
                return BoxedValue.FromString("[error] missing arguments");
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    int topN = operands.Count > 2 ? (int)operands[2].GetLong() : 5;

                    var embedding = Core.AgentCore.Instance.EmbeddingService;
                    if (!embedding.IsReady)
                        return BoxedValue.FromString("[error] embedding model not ready");

                    float[]? vector = embedding.Encode(query);
                    if (vector == null)
                        return BoxedValue.FromString("[error] encode failed");

                    string result = Core.AgentCore.Instance.SemanticIndex.SemanticSearch(collection, vector, query, topN);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_search error: {ex.Message}");
                    return BoxedValue.FromString($"[error] {ex.Message}");
                }
            }
        }
    }

    // semantic_search_between(collection, query, startTime[, endTime[, topN]]) - search within time range, returns JSON
    sealed class SemanticSearchBetweenExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_search_between(collection, query, startTime[, endTime[, topN]])");
                return BoxedValue.FromString("[error] missing arguments");
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    long startTime = SemanticIndex.ParseTimeToUnix(operands[2].AsString);
                    long endTime = operands.Count > 3 ? SemanticIndex.ParseTimeToUnix(operands[3].AsString) : 0;
                    int topN = operands.Count > 4 ? (int)operands[4].GetLong() : 5;

                    var embedding = Core.AgentCore.Instance.EmbeddingService;
                    if (!embedding.IsReady)
                        return BoxedValue.FromString("[error] embedding model not ready");

                    float[]? vector = embedding.Encode(query);
                    if (vector == null)
                        return BoxedValue.FromString("[error] encode failed");

                    var items = Core.AgentCore.Instance.SemanticIndex.SemanticSearchBetweenCore(collection, vector, query, topN, startTime, endTime);
                    return BoxedValue.FromString(SemanticIndex.ResultsToJson(items));
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_search_between error: {ex.Message}");
                    return BoxedValue.FromString($"[error] {ex.Message}");
                }
            }
        }
    }

    // keyword_search_between(collection, query, startTime[, endTime[, topN]]) - keyword search within time range, returns JSON
    sealed class KeywordSearchBetweenExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: keyword_search_between(collection, query, startTime[, endTime[, topN]])");
                return BoxedValue.FromString("[error] missing arguments");
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    long startTime = SemanticIndex.ParseTimeToUnix(operands[2].AsString);
                    long endTime = operands.Count > 3 ? SemanticIndex.ParseTimeToUnix(operands[3].AsString) : 0;
                    int topN = operands.Count > 4 ? (int)operands[4].GetLong() : 5;

                    var items = Core.AgentCore.Instance.SemanticIndex.KeywordSearchBetweenCore(collection, query, topN, startTime, endTime);
                    return BoxedValue.FromString(SemanticIndex.ResultsToJson(items));
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"keyword_search_between error: {ex.Message}");
                    return BoxedValue.FromString($"[error] {ex.Message}");
                }
            }
        }
    }

    // semantic_delete(collection) - delete all records in a collection
    sealed class SemanticDeleteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_delete(collection)");
                return BoxedValue.From(false);
            }

            {
                try {
                    string collection = operands[0].AsString;
                    Core.AgentCore.Instance.SemanticIndex.DeleteCollection(collection);
                    return BoxedValue.From(true);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_delete error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    // semantic_count(collection) - count records in a collection
    sealed class SemanticCountExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_count(collection)");
                return BoxedValue.From(0);
            }

            {
                try {
                    string collection = operands[0].AsString;
                    int count = Core.AgentCore.Instance.SemanticIndex.Count(collection);
                    return BoxedValue.From(count);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_count error: {ex.Message}");
                }
            }
            return BoxedValue.From(0);
        }
    }

    // keyword_search(collection, query[, topN]) - BM25-based keyword search, returns JSON array
    sealed class KeywordSearchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: keyword_search(collection, query[, topN])");
                return BoxedValue.FromString("[error] missing arguments");
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    int topN = operands.Count > 2 ? (int)operands[2].GetLong() : 5;
                    string result = Core.AgentCore.Instance.SemanticIndex.KeywordSearch(collection, query, topN);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"keyword_search error: {ex.Message}");
                    return BoxedValue.FromString($"[error] {ex.Message}");
                }
            }
        }
    }

    // semantic_get_recent(collection[, topN]) - get most recent N records ordered chronologically, returns JSON array
    sealed class SemanticGetRecentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_get_recent(collection[, topN])");
                return BoxedValue.FromString("[error] missing arguments");
            }

            {
                try {
                    string collection = operands[0].AsString;
                    int topN = operands.Count > 1 ? (int)operands[1].GetLong() : 20;
                    string result = Core.AgentCore.Instance.SemanticIndex.GetRecent(collection, topN);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_get_recent error: {ex.Message}");
                    return BoxedValue.FromString($"[error] {ex.Message}");
                }
            }
        }
    }

    // semantic_get_recent_as_list(collection[, topN]) - get most recent N records' content as a list of strings
    sealed class SemanticGetRecentAsListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_get_recent_as_list(collection[, topN])");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string collection = operands[0].AsString;
                    int topN = operands.Count > 1 ? (int)operands[1].GetLong() : 20;
                    var items = Core.AgentCore.Instance.SemanticIndex.GetRecentCore(collection, topN);
                    return SearchResultHelper.ToBoxedList(items);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_get_recent_as_list error: {ex.Message}");
                    return BoxedValue.NullObject;
                }
            }
        }
    }

    // ---- as_list variants: return List<BoxedValue> of content strings ----

    // semantic_search_as_list(collection, query[, topN])
    sealed class SemanticSearchAsListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_search_as_list(collection, query[, topN])");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    int topN = operands.Count > 2 ? (int)operands[2].GetLong() : 5;

                    var embedding = Core.AgentCore.Instance.EmbeddingService;
                    if (!embedding.IsReady) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("semantic_search_as_list: embedding model not ready");
                        return BoxedValue.NullObject;
                    }

                    float[]? vector = embedding.Encode(query);
                    if (vector == null) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("semantic_search_as_list: encode failed");
                        return BoxedValue.NullObject;
                    }

                    var items = Core.AgentCore.Instance.SemanticIndex.SemanticSearchCore(collection, vector, query, topN);
                    return SearchResultHelper.ToBoxedList(items);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_search_as_list error: {ex.Message}");
                    return BoxedValue.NullObject;
                }
            }
        }
    }

    // keyword_search_as_list(collection, query[, topN])
    sealed class KeywordSearchAsListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: keyword_search_as_list(collection, query[, topN])");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    int topN = operands.Count > 2 ? (int)operands[2].GetLong() : 5;

                    var items = Core.AgentCore.Instance.SemanticIndex.KeywordSearchCore(collection, query, topN);
                    return SearchResultHelper.ToBoxedList(items);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"keyword_search_as_list error: {ex.Message}");
                    return BoxedValue.NullObject;
                }
            }
        }
    }

    // semantic_search_between_as_list(collection, query, startTime[, endTime[, topN]])
    sealed class SemanticSearchBetweenAsListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_search_between_as_list(collection, query, startTime[, endTime[, topN]])");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    long startTime = SemanticIndex.ParseTimeToUnix(operands[2].AsString);
                    long endTime = operands.Count > 3 ? SemanticIndex.ParseTimeToUnix(operands[3].AsString) : 0;
                    int topN = operands.Count > 4 ? (int)operands[4].GetLong() : 5;

                    var embedding = Core.AgentCore.Instance.EmbeddingService;
                    if (!embedding.IsReady) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("semantic_search_between_as_list: embedding model not ready");
                        return BoxedValue.NullObject;
                    }

                    float[]? vector = embedding.Encode(query);
                    if (vector == null) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("semantic_search_between_as_list: encode failed");
                        return BoxedValue.NullObject;
                    }

                    var items = Core.AgentCore.Instance.SemanticIndex.SemanticSearchBetweenCore(collection, vector, query, topN, startTime, endTime);
                    return SearchResultHelper.ToBoxedList(items);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_search_between_as_list error: {ex.Message}");
                    return BoxedValue.NullObject;
                }
            }
        }
    }

    // keyword_search_between_as_list(collection, query, startTime[, endTime[, topN]])
    sealed class KeywordSearchBetweenAsListExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: keyword_search_between_as_list(collection, query, startTime[, endTime[, topN]])");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    long startTime = SemanticIndex.ParseTimeToUnix(operands[2].AsString);
                    long endTime = operands.Count > 3 ? SemanticIndex.ParseTimeToUnix(operands[3].AsString) : 0;
                    int topN = operands.Count > 4 ? (int)operands[4].GetLong() : 5;

                    var items = Core.AgentCore.Instance.SemanticIndex.KeywordSearchBetweenCore(collection, query, topN, startTime, endTime);
                    return SearchResultHelper.ToBoxedList(items);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"keyword_search_between_as_list error: {ex.Message}");
                    return BoxedValue.NullObject;
                }
            }
        }
    }

    // ---- order_by_time variants: return List<BoxedValue> sorted by time ----

    // semantic_search_order_by_time(collection, query[, topN[, isAsc]])
    sealed class SemanticSearchOrderByTimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_search_order_by_time(collection, query[, topN[, isAsc]])");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    int topN = operands.Count > 2 ? (int)operands[2].GetLong() : 5;
                    bool isAsc = operands.Count > 3 && operands[3].GetBool();

                    var embedding = Core.AgentCore.Instance.EmbeddingService;
                    if (!embedding.IsReady) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("semantic_search_order_by_time: embedding model not ready");
                        return BoxedValue.NullObject;
                    }

                    float[]? vector = embedding.Encode(query);
                    if (vector == null) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("semantic_search_order_by_time: encode failed");
                        return BoxedValue.NullObject;
                    }

                    var items = Core.AgentCore.Instance.SemanticIndex.SemanticSearchCore(collection, vector, query, topN);
                    return SearchResultHelper.ToBoxedListOrderByTime(items, isAsc);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_search_order_by_time error: {ex.Message}");
                    return BoxedValue.NullObject;
                }
            }
        }
    }

    // keyword_search_order_by_time(collection, query[, topN[, isAsc]])
    sealed class KeywordSearchOrderByTimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: keyword_search_order_by_time(collection, query[, topN[, isAsc]])");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    int topN = operands.Count > 2 ? (int)operands[2].GetLong() : 5;
                    bool isAsc = operands.Count > 3 && operands[3].GetBool();

                    var items = Core.AgentCore.Instance.SemanticIndex.KeywordSearchCore(collection, query, topN);
                    return SearchResultHelper.ToBoxedListOrderByTime(items, isAsc);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"keyword_search_order_by_time error: {ex.Message}");
                    return BoxedValue.NullObject;
                }
            }
        }
    }

    // semantic_search_between_order_by_time(collection, query, startTime[, endTime[, topN[, isAsc]]])
    sealed class SemanticSearchBetweenOrderByTimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 6) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: semantic_search_between_order_by_time(collection, query, startTime[, endTime[, topN[, isAsc]]])");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    long startTime = SemanticIndex.ParseTimeToUnix(operands[2].AsString);
                    long endTime = operands.Count > 3 ? SemanticIndex.ParseTimeToUnix(operands[3].AsString) : 0;
                    int topN = operands.Count > 4 ? (int)operands[4].GetLong() : 5;
                    bool isAsc = operands.Count > 5 && operands[5].GetBool();

                    var embedding = Core.AgentCore.Instance.EmbeddingService;
                    if (!embedding.IsReady) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("semantic_search_between_order_by_time: embedding model not ready");
                        return BoxedValue.NullObject;
                    }

                    float[]? vector = embedding.Encode(query);
                    if (vector == null) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("semantic_search_between_order_by_time: encode failed");
                        return BoxedValue.NullObject;
                    }

                    var items = Core.AgentCore.Instance.SemanticIndex.SemanticSearchBetweenCore(collection, vector, query, topN, startTime, endTime);
                    return SearchResultHelper.ToBoxedListOrderByTime(items, isAsc);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"semantic_search_between_order_by_time error: {ex.Message}");
                    return BoxedValue.NullObject;
                }
            }
        }
    }

    // keyword_search_between_order_by_time(collection, query, startTime[, endTime[, topN[, isAsc]]])
    sealed class KeywordSearchBetweenOrderByTimeExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 6) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: keyword_search_between_order_by_time(collection, query, startTime[, endTime[, topN[, isAsc]]])");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string collection = operands[0].AsString;
                    string query = operands[1].AsString;
                    long startTime = SemanticIndex.ParseTimeToUnix(operands[2].AsString);
                    long endTime = operands.Count > 3 ? SemanticIndex.ParseTimeToUnix(operands[3].AsString) : 0;
                    int topN = operands.Count > 4 ? (int)operands[4].GetLong() : 5;
                    bool isAsc = operands.Count > 5 && operands[5].GetBool();

                    var items = Core.AgentCore.Instance.SemanticIndex.KeywordSearchBetweenCore(collection, query, topN, startTime, endTime);
                    return SearchResultHelper.ToBoxedListOrderByTime(items, isAsc);
                }
                catch (Exception ex) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"keyword_search_between_order_by_time error: {ex.Message}");
                    return BoxedValue.NullObject;
                }
            }
        }
    }

    public static class SemanticApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_init", "semantic_init(collection)", new ExpressionFactoryHelper<SemanticInitExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_is_ready", "semantic_is_ready(collection)", new ExpressionFactoryHelper<SemanticIsReadyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_add", "semantic_add(collection, content[, metadata])", new ExpressionFactoryHelper<SemanticAddExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_search", "semantic_search(collection, query[, topN])", new ExpressionFactoryHelper<SemanticSearchExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_delete", "semantic_delete(collection)", new ExpressionFactoryHelper<SemanticDeleteExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_count", "semantic_count(collection)", new ExpressionFactoryHelper<SemanticCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_get_count", "semantic_get_count(collection)", new ExpressionFactoryHelper<SemanticCountExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_get_recent", "semantic_get_recent(collection[, topN])", new ExpressionFactoryHelper<SemanticGetRecentExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("keyword_search", "keyword_search(collection, query[, topN])", new ExpressionFactoryHelper<KeywordSearchExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_get_recent_as_list", "semantic_get_recent_as_list(collection[, topN])", new ExpressionFactoryHelper<SemanticGetRecentAsListExp>());
            // between variants (JSON)
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_search_between", "semantic_search_between(collection, query, startTime[, endTime[, topN]])", new ExpressionFactoryHelper<SemanticSearchBetweenExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("keyword_search_between", "keyword_search_between(collection, query, startTime[, endTime[, topN]])", new ExpressionFactoryHelper<KeywordSearchBetweenExp>());
            // as_list variants
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_search_as_list", "semantic_search_as_list(collection, query[, topN])", new ExpressionFactoryHelper<SemanticSearchAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("keyword_search_as_list", "keyword_search_as_list(collection, query[, topN])", new ExpressionFactoryHelper<KeywordSearchAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_search_between_as_list", "semantic_search_between_as_list(collection, query, startTime[, endTime[, topN]])", new ExpressionFactoryHelper<SemanticSearchBetweenAsListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("keyword_search_between_as_list", "keyword_search_between_as_list(collection, query, startTime[, endTime[, topN]])", new ExpressionFactoryHelper<KeywordSearchBetweenAsListExp>());
            // order_by_time variants
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_search_order_by_time", "semantic_search_order_by_time(collection, query[, topN[, isAsc]]), return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<SemanticSearchOrderByTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("keyword_search_order_by_time", "keyword_search_order_by_time(collection, query[, topN[, isAsc]]), return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<KeywordSearchOrderByTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("semantic_search_between_order_by_time", "semantic_search_between_order_by_time(collection, query, startTime[, endTime[, topN[, isAsc]]]), return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<SemanticSearchBetweenOrderByTimeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("keyword_search_between_order_by_time", "keyword_search_between_order_by_time(collection, query, startTime[, endTime[, topN[, isAsc]]]), return List, use 'to_string' to convert to a string", new ExpressionFactoryHelper<KeywordSearchBetweenOrderByTimeExp>());
        }
    }
}
