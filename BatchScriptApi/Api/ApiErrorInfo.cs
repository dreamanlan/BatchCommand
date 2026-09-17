using System.Text;

namespace BatchCommand.Api
{
    /// <summary>
    /// Thread-static collector for api error messages, shared by all hosts
    /// (AgentCore, CefDotnetApp, ...). Dsl apis record errors here instead of
    /// throwing: execution continues with a null/default return value, and the
    /// host fetches everything uniformly after dsl execution via
    /// BuildErrorText/HasAnyError (api errors + dsl interpreter errors).
    /// The buffer is cleared by DslHost.TryLoadDSL/ExecuteScript per execution.
    /// </summary>
    public static class ApiErrorInfo
    {
        [ThreadStatic]
        private static StringBuilder? tls_Info;

        public static StringBuilder Info {
            get {
                if (null == tls_Info) {
                    tls_Info = new StringBuilder();
                }
                return tls_Info;
            }
        }

        public static bool HasInfo {
            get {
                return null != tls_Info && tls_Info.Length > 0;
            }
        }

        public static string GetInfo()
        {
            return null != tls_Info ? tls_Info.ToString() : string.Empty;
        }

        public static void Clear()
        {
            tls_Info?.Clear();
        }

        public static void Append(string msg)
        {
            Info.Append(msg);
        }

        public static void AppendLine(string msg)
        {
            Info.AppendLine(msg);
        }

        public static void AppendFormat(string fmt, params object[] args)
        {
            if (args.Length == 0)
                Info.Append(fmt);
            else
                Info.AppendFormat(fmt, args);
        }

        public static void AppendFormatLine(string fmt, params object[] args)
        {
            if (args.Length == 0)
                Info.AppendLine(fmt);
            else {
                Info.AppendFormat(fmt, args);
                Info.AppendLine();
            }
        }

        /// <summary>
        /// Unified error check after dsl execution: api errors recorded through
        /// this class plus dsl interpreter errors (BatchScript.GetDslErrors).
        /// </summary>
        public static bool HasAnyError {
            get {
                return HasInfo || BatchScript.HasDslErrors;
            }
        }

        /// <summary>
        /// Unified error text after dsl execution: api error info plus dsl
        /// interpreter errors, each block on its own lines. Empty when no error.
        /// </summary>
        public static string BuildErrorText()
        {
            var sb = new StringBuilder();
            if (HasInfo) {
                sb.AppendLine();
                sb.AppendLine(GetInfo());
            }
            if (BatchScript.HasDslErrors) {
                sb.AppendLine();
                sb.AppendLine(BatchScript.GetDslErrors());
            }
            return sb.ToString();
        }
    }
}
