using System.Text;

namespace BatchCommand.Api
{
    /// <summary>
    /// Thread-static text collectors for dsl api output, shared by all hosts
    /// (AgentCore, CefDotnetApp, ...). Two per-thread buffers live here:
    /// Error records api errors - the host marks the execution result as
    /// failed (HasError) and appends the error text; Tip records non-error
    /// hints - the text is appended to the normal result without the HasError
    /// flag. Dsl apis record both instead of throwing: execution continues
    /// with a null/default return value. Both buffers are cleared by
    /// DslHost.TryLoadDSL/ExecuteScript per execution. Buffer mechanics live
    /// in the shared ApiTextBuffer; error aggregation semantics
    /// (HasAnyError/BuildErrorText) stay here.
    /// </summary>
    public static class ApiTextInfo
    {
        [ThreadStatic]
        private static ApiTextBuffer? tls_Error;

        [ThreadStatic]
        private static ApiTextBuffer? tls_Tip;

        public static ApiTextBuffer Error {
            get {
                if (null == tls_Error) {
                    tls_Error = new ApiTextBuffer();
                }
                return tls_Error;
            }
        }

        public static ApiTextBuffer Tip {
            get {
                if (null == tls_Tip) {
                    tls_Tip = new ApiTextBuffer();
                }
                return tls_Tip;
            }
        }

        /// <summary>
        /// Unified error check after dsl execution: api errors recorded through
        /// Error plus dsl interpreter errors (BatchScript.GetDslErrors).
        /// </summary>
        public static bool HasAnyError {
            get {
                return Error.HasContent || BatchScript.HasDslErrors;
            }
        }

        /// <summary>
        /// Unified error text after dsl execution: api error info plus dsl
        /// interpreter errors, each block on its own lines. Empty when no error.
        /// </summary>
        public static string BuildErrorText()
        {
            var sb = new StringBuilder();
            if (Error.HasContent) {
                sb.AppendLine();
                sb.AppendLine(Error.GetText());
            }
            if (BatchScript.HasDslErrors) {
                sb.AppendLine();
                sb.AppendLine(BatchScript.GetDslErrors());
            }
            return sb.ToString();
        }
    }
}
