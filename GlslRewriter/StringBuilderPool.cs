using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlslRewriter
{
    internal static class Literal
    {
        internal static string GetIndentString(int indent)
        {
            PrepareIndent(indent);
            return s_IndentString.Substring(0, indent);
        }
        internal static string GetSpaceString(int indent)
        {
            PrepareSpace(indent * c_IndentSpaceString.Length);
            return s_SpaceString.Substring(0, indent * c_IndentSpaceString.Length);
        }

        private static void PrepareIndent(int indent)
        {
            if (s_IndentString.Length < indent) {
                int len = c_InitCount;
                while (len < indent) {
                    len *= 2;
                }
                s_IndentString = new string('\t', indent);
            }
        }
        private static void PrepareSpace(int indent)
        {
            if (s_SpaceString.Length < indent) {
                int len = c_InitCount;
                while (len < indent) {
                    len *= 2;
                }
                while (s_SpaceBuilder.Length < len) {
                    s_SpaceBuilder.Append(c_IndentSpaceString);
                }
                s_SpaceString = s_SpaceBuilder.ToString();
            }
        }

        private static string s_IndentString = string.Empty;
        private static string s_SpaceString = string.Empty;
        private static StringBuilder s_SpaceBuilder = new StringBuilder();

        private const int c_InitCount = 256;
        private const string c_IndentSpaceString = "| ";
    }
    internal static class StringBuilderExtension
    {
        public static void Append(this StringBuilder sb, string fmt, params object[] args)
        {
            if (args.Length == 0) {
                sb.Append(fmt);
            }
            else {
                sb.AppendFormat(fmt, args);
            }
        }
        public static void AppendLine(this StringBuilder sb, string fmt, params object[] args)
        {
            if (args.Length == 0) {
                sb.AppendLine(fmt);
            }
            else {
                sb.AppendFormat(fmt, args);
                sb.AppendLine();
            }
        }
    }
    internal sealed class StringBuilderPool
    {
        public bool IsDebugMode { get; set; } = false;

        public StringBuilder Alloc()
        {
            StringBuilder sb;
            if (m_Queue.Count > 0) {
                sb = m_Queue.Dequeue();
                if (IsDebugMode) {
                    m_Records.Remove(sb);
                }
                if (sb.Length > 0)
                    sb.Length = 0;
            }
            else {
                sb = new StringBuilder();
            }
            return sb;
        }
        public void Recycle(StringBuilder sb)
        {
            if (IsDebugMode) {
                string trace = Environment.StackTrace;
                if (m_Records.TryGetValue(sb, out var rec)) {
                    Console.WriteLine("[===pool conflict===]");
                    Console.WriteLine("===this===");
                    Console.WriteLine(trace);
                    Console.WriteLine("===other===");
                    Console.WriteLine(rec);
                    return;
                }
                else {
                    m_Records.Add(sb, trace);
                }
            }
            m_Queue.Enqueue(sb);
        }

        private Queue<StringBuilder> m_Queue = new Queue<StringBuilder>();
        private Dictionary<StringBuilder, string> m_Records = new Dictionary<StringBuilder, string>();
    }
}
