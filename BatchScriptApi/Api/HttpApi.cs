using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using System.Text;
using System.Collections;
using BatchCommand.Utils;

namespace BatchCommand.Api
{
    /// <summary>
    /// HTTP api set (moved from AgentCore; service core in
    /// BatchCommand.Utils.HttpClientOperations). Sync calls plus async
    /// variants whose results arrive through HostBridge.DeliverCallback
    /// (handle_&lt;msg&gt; dsl callbacks on the host main thread).
    /// </summary>
    public static class HttpApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("fetch", "fetch(url) or fetch(url, headers) - alias of http_get", new ExpressionFactoryHelper<HttpGetExp>());
            BatchCommand.BatchScript.Register("http_get", "http_get(url) or http_get(url, headers)", new ExpressionFactoryHelper<HttpGetExp>());
            BatchCommand.BatchScript.Register("http_post", "http_post(url, content) or http_post(url, content, contentType) or http_post(url, content, contentType, headers)", new ExpressionFactoryHelper<HttpPostExp>());
            BatchCommand.BatchScript.Register("download_file", "download_file(url, savePath) or download_file(url, savePath, headers)", new ExpressionFactoryHelper<DownloadFileExp>());
            BatchCommand.BatchScript.Register("http_get_bytes", "http_get_bytes(url) or http_get_bytes(url, headers) - GET and return byte array", new ExpressionFactoryHelper<HttpGetBytesExp>());
            BatchCommand.BatchScript.Register("http_put", "http_put(url, content) or http_put(url, content, contentType) or http_put(url, content, contentType, headers)", new ExpressionFactoryHelper<HttpPutExp>());
            BatchCommand.BatchScript.Register("http_delete", "http_delete(url) or http_delete(url, headers)", new ExpressionFactoryHelper<HttpDeleteExp>());
            BatchCommand.BatchScript.Register("http_post_form", "http_post_form(url, formData) or http_post_form(url, formData, headers) - formData is a hashtable of form fields", new ExpressionFactoryHelper<HttpPostFormExp>());
            BatchCommand.BatchScript.Register("http_upload_file", "http_upload_file(url, filePath) or http_upload_file(url, filePath, fieldName) or http_upload_file(url, filePath, fieldName, formData) or http_upload_file(url, filePath, fieldName, formData, headers)", new ExpressionFactoryHelper<HttpUploadFileExp>());
            BatchCommand.BatchScript.Register("set_http_default_header", "set_http_default_header(name, value) - set a default request header for all http operations", new ExpressionFactoryHelper<SetHttpDefaultHeaderExp>());
            BatchCommand.BatchScript.Register("remove_http_default_header", "remove_http_default_header(name) - remove a default request header", new ExpressionFactoryHelper<RemoveHttpDefaultHeaderExp>());
            BatchCommand.BatchScript.Register("http_get_callback", "http_get_callback(url, tag) or http_get_callback(url, headers, tag) - async GET, result via http_get_callback CEF message (url, tag, result)", new ExpressionFactoryHelper<HttpGetCallbackExp>());
            BatchCommand.BatchScript.Register("http_get_bytes_callback", "http_get_bytes_callback(url, tag) or http_get_bytes_callback(url, headers, tag) - async binary GET, result is base64 of response body via http_get_bytes_callback CEF message (url, tag, result)", new ExpressionFactoryHelper<HttpGetBytesCallbackExp>());
            BatchCommand.BatchScript.Register("http_post_callback", "http_post_callback(url, content, tag) or (url, content, contentType, tag) or (url, content, contentType, headers, tag) - async POST, result via http_post_callback CEF message (url, tag, result)", new ExpressionFactoryHelper<HttpPostCallbackExp>());
            BatchCommand.BatchScript.Register("http_post_form_callback", "http_post_form_callback(url, formData, tag) or (url, formData, headers, tag) - async POST form, result via http_post_form_callback CEF message (url, tag, result)", new ExpressionFactoryHelper<HttpPostFormCallbackExp>());
            BatchCommand.BatchScript.Register("http_put_callback", "http_put_callback(url, content, tag) or (url, content, contentType, tag) or (url, content, contentType, headers, tag) - async PUT, result via http_put_callback CEF message (url, tag, result)", new ExpressionFactoryHelper<HttpPutCallbackExp>());
            BatchCommand.BatchScript.Register("http_delete_callback", "http_delete_callback(url, tag) or (url, headers, tag) - async DELETE, result via http_delete_callback CEF message (url, tag, result)", new ExpressionFactoryHelper<HttpDeleteCallbackExp>());
            BatchCommand.BatchScript.Register("http_upload_file_callback", "http_upload_file_callback(url, filePath, tag) or (url, filePath, fieldName, tag) or (url, filePath, fieldName, formData, tag) or (url, filePath, fieldName, formData, headers, tag) - async upload, result via http_upload_file_callback CEF message (url, tag, result)", new ExpressionFactoryHelper<HttpUploadFileCallbackExp>());
            BatchCommand.BatchScript.Register("download_file_callback", "download_file_callback(url, savePath, tag) or (url, savePath, headers, tag) - async download, result 'ok' or 'error: ...' via download_file_callback CEF message (url, tag, result)", new ExpressionFactoryHelper<DownloadFileCallbackExp>());
            BatchCommand.BatchScript.Register("set_http_user_agent", "set_http_user_agent(user_agent) - set User-Agent header for http_get/http_post/download_file", new ExpressionFactoryHelper<SetHttpUserAgentExp>());
            BatchCommand.BatchScript.Register("get_http_user_agent", "get_http_user_agent() - get current User-Agent header string", new ExpressionFactoryHelper<GetHttpUserAgentExp>());
        }
    }

    // HTTP Operations
    static class HttpHeaderHelper
    {
        public static Dictionary<string, string>? ExtractStringHeaders(BoxedValue bv)
        {
            var obj = bv.GetObject();
            if (obj is IDictionary<BoxedValue, BoxedValue> bvdict) {
                var d = new Dictionary<string, string>();
                foreach (var kv in bvdict) {
                    d[kv.Key.ToString()] = kv.Value.ToString();
                }
                return d.Count > 0 ? d : null;
            }
            if (obj is IDictionary<string, object?> sdict) {
                var d = new Dictionary<string, string>();
                foreach (var kv in sdict) {
                    d[kv.Key] = kv.Value?.ToString() ?? string.Empty;
                }
                return d.Count > 0 ? d : null;
            }
            return null;
        }
    }

    sealed class HttpGetExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: http_get(url) or http_get(url, headers)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 1) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[1]);
                        if (headers == null) {
                            ApiErrorInfo.AppendLine("Expected: http_get(url, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    string result = HttpClientOperations.Shared.Get(url, headers);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"httpget error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class HttpPostExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                ApiErrorInfo.AppendLine("Expected: http_post(url, content) or http_post(url, content, contentType) or http_post(url, content, contentType, headers)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    string content = operands[1].AsString;
                    string contentType = "application/json";
                    if (operands.Count > 2) {
                        if (operands[2].IsString) {
                            contentType = operands[2].ToString();
                        }
                        else {
                            ApiErrorInfo.AppendLine("Expected: http_post(url, content, contentType) or http_post(url, content, contentType, headers), contentType must be string");
                            return BoxedValue.NullObject;
                        }
                    }
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 3) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[3]);
                        if (headers == null) {
                            ApiErrorInfo.AppendLine("Expected: http_post(url, content, contentType, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    string result = HttpClientOperations.Shared.Post(url, content, contentType, headers);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"httppost error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class DownloadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: download_file(url, savePath) or download_file(url, savePath, headers)");
                return BoxedValue.From(false);
            }

            {
                try {
                    string url = operands[0].AsString;
                    string savePath = operands[1].AsString;
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 2) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[2]);
                        if (headers == null) {
                            ApiErrorInfo.AppendLine("Expected: download_file(url, savePath, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    bool result = HttpClientOperations.Shared.DownloadFile(url, savePath, headers);
                    return BoxedValue.From(result);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"downloadfile error: {ex.Message}");
                }
            }
            return BoxedValue.From(false);
        }
    }

    sealed class HttpGetBytesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: http_get_bytes(url) or http_get_bytes(url, headers)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 1) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[1]);
                        if (headers == null) {
                            ApiErrorInfo.AppendLine("Expected: http_get_bytes(url, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    byte[] result = HttpClientOperations.Shared.GetBytes(url, headers);
                    return BoxedValue.FromObject(result);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"http_get_bytes error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class HttpPutExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 4) {
                ApiErrorInfo.AppendLine("Expected: http_put(url, content) or http_put(url, content, contentType) or http_put(url, content, contentType, headers)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    string content = operands[1].AsString;
                    string contentType = "application/json";
                    if (operands.Count > 2) {
                        if (operands[2].IsString) {
                            contentType = operands[2].ToString();
                        }
                        else {
                            ApiErrorInfo.AppendLine("Expected: http_put(url, content, contentType) or http_put(url, content, contentType, headers), contentType must be string");
                            return BoxedValue.NullObject;
                        }
                    }
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 3) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[3]);
                        if (headers == null) {
                            ApiErrorInfo.AppendLine("Expected: http_put(url, content, contentType, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    string result = HttpClientOperations.Shared.Put(url, content, contentType, headers);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"http_put error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class HttpDeleteExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1 || operands.Count > 2) {
                ApiErrorInfo.AppendLine("Expected: http_delete(url) or http_delete(url, headers)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 1) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[1]);
                        if (headers == null) {
                            ApiErrorInfo.AppendLine("Expected: http_delete(url, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    string result = HttpClientOperations.Shared.Delete(url, headers);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"http_delete error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class HttpPostFormExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: http_post_form(url, formData) or http_post_form(url, formData, headers)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    var formData = HttpHeaderHelper.ExtractStringHeaders(operands[1]);
                    if (formData == null) {
                        ApiErrorInfo.AppendLine("Expected: http_post_form(url, formData[, headers]), formData must be hashtable");
                        return BoxedValue.NullObject;
                    }
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 2) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[2]);
                        if (headers == null) {
                            ApiErrorInfo.AppendLine("Expected: http_post_form(url, formData, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    string result = HttpClientOperations.Shared.PostForm(url, formData, headers);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"http_post_form error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class HttpUploadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 5) {
                ApiErrorInfo.AppendLine("Expected: http_upload_file(url, filePath) or http_upload_file(url, filePath, fieldName) or http_upload_file(url, filePath, fieldName, formData) or http_upload_file(url, filePath, fieldName, formData, headers)");
                return BoxedValue.NullObject;
            }

            {
                try {
                    string url = operands[0].AsString;
                    string filePath = operands[1].AsString;
                    string fieldName = "file";
                    if (operands.Count > 2) {
                        if (operands[2].IsString) {
                            fieldName = operands[2].ToString();
                        }
                        else {
                            ApiErrorInfo.AppendLine("Expected: http_upload_file(url, filePath, fieldName, ...), fieldName must be string");
                            return BoxedValue.NullObject;
                        }
                    }
                    Dictionary<string, string>? formData = null;
                    if (operands.Count > 3) {
                        formData = HttpHeaderHelper.ExtractStringHeaders(operands[3]);
                        if (formData == null) {
                            ApiErrorInfo.AppendLine("Expected: http_upload_file(url, filePath, fieldName, formData, ...), formData must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    Dictionary<string, string>? headers = null;
                    if (operands.Count > 4) {
                        headers = HttpHeaderHelper.ExtractStringHeaders(operands[4]);
                        if (headers == null) {
                            ApiErrorInfo.AppendLine("Expected: http_upload_file(url, filePath, fieldName, formData, headers), headers must be hashtable");
                            return BoxedValue.NullObject;
                        }
                    }
                    string result = HttpClientOperations.Shared.UploadFile(url, filePath, fieldName, formData, headers);
                    return BoxedValue.FromString(result);
                }
                catch (Exception ex) {
                    ApiErrorInfo.AppendLine($"http_upload_file error: {ex.Message}");
                }
            }
            return BoxedValue.NullObject;
        }
    }

    sealed class SetHttpDefaultHeaderExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 2) {
                ApiErrorInfo.AppendLine("Expected: set_http_default_header(name, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            HttpClientOperations.Shared.SetDefaultHeader(operands[0].AsString, operands[1].AsString);
            return BoxedValue.FromString("ok");
        }
    }

    sealed class RemoveHttpDefaultHeaderExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: remove_http_default_header(name)");
                return BoxedValue.FromString("error: missing parameters");
            }
            HttpClientOperations.Shared.RemoveDefaultHeader(operands[0].AsString);
            return BoxedValue.FromString("ok");
        }
    }

    // HTTP async callback expressions. tag is always the last operand. Each
    // returns "ok" immediately; result arrives via a same-named CEF message
    // with 3 string args: (url, tag, result).
    sealed class HttpGetCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: http_get_callback(url, tag) or http_get_callback(url, headers, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            try {
                string url = operands[0].AsString;
                string tag = operands[operands.Count - 1].AsString;
                Dictionary<string, string>? headers = null;
                if (operands.Count == 3) {
                    headers = HttpHeaderHelper.ExtractStringHeaders(operands[1]);
                    if (headers == null) {
                        ApiErrorInfo.AppendLine("Expected: http_get_callback(url, headers, tag), headers must be hashtable");
                        return BoxedValue.FromString("error: invalid headers");
                    }
                }
                HttpClientOperations.Shared.GetWithCallback(url, headers, tag);
                return BoxedValue.FromString("ok");
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"http_get_callback error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    // Async binary GET. result in the http_get_bytes_callback CEF message is the
    // response body encoded as a base64 string (or "error: ..." on failure).
    sealed class HttpGetBytesCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: http_get_bytes_callback(url, tag) or http_get_bytes_callback(url, headers, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            try {
                string url = operands[0].AsString;
                string tag = operands[operands.Count - 1].AsString;
                Dictionary<string, string>? headers = null;
                if (operands.Count == 3) {
                    headers = HttpHeaderHelper.ExtractStringHeaders(operands[1]);
                    if (headers == null) {
                        ApiErrorInfo.AppendLine("Expected: http_get_bytes_callback(url, headers, tag), headers must be hashtable");
                        return BoxedValue.FromString("error: invalid headers");
                    }
                }
                HttpClientOperations.Shared.GetBytesWithCallback(url, headers, tag);
                return BoxedValue.FromString("ok");
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"http_get_bytes_callback error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    sealed class HttpPostCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                ApiErrorInfo.AppendLine("Expected: http_post_callback(url, content, tag) or http_post_callback(url, content, contentType, tag) or http_post_callback(url, content, contentType, headers, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            try {
                string url = operands[0].AsString;
                string content = operands[1].AsString;
                string tag = operands[operands.Count - 1].AsString;
                string contentType = "application/json";
                if (operands.Count >= 4) {
                    if (operands[2].IsString) {
                        contentType = operands[2].ToString();
                    }
                    else {
                        ApiErrorInfo.AppendLine("Expected: http_post_callback(url, content, contentType, ...), contentType must be string");
                        return BoxedValue.FromString("error: invalid contentType");
                    }
                }
                Dictionary<string, string>? headers = null;
                if (operands.Count == 5) {
                    headers = HttpHeaderHelper.ExtractStringHeaders(operands[3]);
                    if (headers == null) {
                        ApiErrorInfo.AppendLine("Expected: http_post_callback(url, content, contentType, headers, tag), headers must be hashtable");
                        return BoxedValue.FromString("error: invalid headers");
                    }
                }
                HttpClientOperations.Shared.PostWithCallback(url, content, contentType, headers, tag);
                return BoxedValue.FromString("ok");
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"http_post_callback error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    sealed class HttpPostFormCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 4) {
                ApiErrorInfo.AppendLine("Expected: http_post_form_callback(url, formData, tag) or http_post_form_callback(url, formData, headers, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            try {
                string url = operands[0].AsString;
                string tag = operands[operands.Count - 1].AsString;
                var formData = HttpHeaderHelper.ExtractStringHeaders(operands[1]);
                if (formData == null) {
                    ApiErrorInfo.AppendLine("Expected: http_post_form_callback(url, formData, ...), formData must be hashtable");
                    return BoxedValue.FromString("error: invalid formData");
                }
                Dictionary<string, string>? headers = null;
                if (operands.Count == 4) {
                    headers = HttpHeaderHelper.ExtractStringHeaders(operands[2]);
                    if (headers == null) {
                        ApiErrorInfo.AppendLine("Expected: http_post_form_callback(url, formData, headers, tag), headers must be hashtable");
                        return BoxedValue.FromString("error: invalid headers");
                    }
                }
                HttpClientOperations.Shared.PostFormWithCallback(url, formData, headers, tag);
                return BoxedValue.FromString("ok");
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"http_post_form_callback error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    sealed class HttpPutCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 5) {
                ApiErrorInfo.AppendLine("Expected: http_put_callback(url, content, tag) or http_put_callback(url, content, contentType, tag) or http_put_callback(url, content, contentType, headers, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            try {
                string url = operands[0].AsString;
                string content = operands[1].AsString;
                string tag = operands[operands.Count - 1].AsString;
                string contentType = "application/json";
                if (operands.Count >= 4) {
                    if (operands[2].IsString) {
                        contentType = operands[2].ToString();
                    }
                    else {
                        ApiErrorInfo.AppendLine("Expected: http_put_callback(url, content, contentType, ...), contentType must be string");
                        return BoxedValue.FromString("error: invalid contentType");
                    }
                }
                Dictionary<string, string>? headers = null;
                if (operands.Count == 5) {
                    headers = HttpHeaderHelper.ExtractStringHeaders(operands[3]);
                    if (headers == null) {
                        ApiErrorInfo.AppendLine("Expected: http_put_callback(url, content, contentType, headers, tag), headers must be hashtable");
                        return BoxedValue.FromString("error: invalid headers");
                    }
                }
                HttpClientOperations.Shared.PutWithCallback(url, content, contentType, headers, tag);
                return BoxedValue.FromString("ok");
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"http_put_callback error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    sealed class HttpDeleteCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2 || operands.Count > 3) {
                ApiErrorInfo.AppendLine("Expected: http_delete_callback(url, tag) or http_delete_callback(url, headers, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            try {
                string url = operands[0].AsString;
                string tag = operands[operands.Count - 1].AsString;
                Dictionary<string, string>? headers = null;
                if (operands.Count == 3) {
                    headers = HttpHeaderHelper.ExtractStringHeaders(operands[1]);
                    if (headers == null) {
                        ApiErrorInfo.AppendLine("Expected: http_delete_callback(url, headers, tag), headers must be hashtable");
                        return BoxedValue.FromString("error: invalid headers");
                    }
                }
                HttpClientOperations.Shared.DeleteWithCallback(url, headers, tag);
                return BoxedValue.FromString("ok");
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"http_delete_callback error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    sealed class HttpUploadFileCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 6) {
                ApiErrorInfo.AppendLine("Expected: http_upload_file_callback(url, filePath, tag) or (url, filePath, fieldName, tag) or (url, filePath, fieldName, formData, tag) or (url, filePath, fieldName, formData, headers, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            try {
                string url = operands[0].AsString;
                string filePath = operands[1].AsString;
                string tag = operands[operands.Count - 1].AsString;
                string fieldName = "file";
                if (operands.Count >= 4) {
                    if (operands[2].IsString) {
                        fieldName = operands[2].ToString();
                    }
                    else {
                        ApiErrorInfo.AppendLine("Expected: http_upload_file_callback(url, filePath, fieldName, ...), fieldName must be string");
                        return BoxedValue.FromString("error: invalid fieldName");
                    }
                }
                Dictionary<string, string>? formData = null;
                if (operands.Count >= 5) {
                    formData = HttpHeaderHelper.ExtractStringHeaders(operands[3]);
                    if (formData == null) {
                        ApiErrorInfo.AppendLine("Expected: http_upload_file_callback(url, filePath, fieldName, formData, ...), formData must be hashtable");
                        return BoxedValue.FromString("error: invalid formData");
                    }
                }
                Dictionary<string, string>? headers = null;
                if (operands.Count == 6) {
                    headers = HttpHeaderHelper.ExtractStringHeaders(operands[4]);
                    if (headers == null) {
                        ApiErrorInfo.AppendLine("Expected: http_upload_file_callback(url, filePath, fieldName, formData, headers, tag), headers must be hashtable");
                        return BoxedValue.FromString("error: invalid headers");
                    }
                }
                HttpClientOperations.Shared.UploadFileWithCallback(url, filePath, fieldName, formData, headers, tag);
                return BoxedValue.FromString("ok");
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"http_upload_file_callback error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    sealed class DownloadFileCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3 || operands.Count > 4) {
                ApiErrorInfo.AppendLine("Expected: download_file_callback(url, savePath, tag) or download_file_callback(url, savePath, headers, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            try {
                string url = operands[0].AsString;
                string savePath = operands[1].AsString;
                string tag = operands[operands.Count - 1].AsString;
                Dictionary<string, string>? headers = null;
                if (operands.Count == 4) {
                    headers = HttpHeaderHelper.ExtractStringHeaders(operands[2]);
                    if (headers == null) {
                        ApiErrorInfo.AppendLine("Expected: download_file_callback(url, savePath, headers, tag), headers must be hashtable");
                        return BoxedValue.FromString("error: invalid headers");
                    }
                }
                HttpClientOperations.Shared.DownloadFileWithCallback(url, savePath, headers, tag);
                return BoxedValue.FromString("ok");
            }
            catch (Exception ex) {
                ApiErrorInfo.AppendLine($"download_file_callback error: {ex.Message}");
                return BoxedValue.FromString($"error: {ex.Message}");
            }
        }
    }

    sealed class SetHttpUserAgentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count != 1) {
                ApiErrorInfo.AppendLine("Expected: set_http_user_agent(user_agent)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string ua = operands[0].AsString;
            HttpClientOperations.Shared.SetUserAgent(ua);
            return BoxedValue.FromString("ok");
        }
    }

    sealed class GetHttpUserAgentExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(HttpClientOperations.Shared.GetUserAgent());
        }
    }
}
