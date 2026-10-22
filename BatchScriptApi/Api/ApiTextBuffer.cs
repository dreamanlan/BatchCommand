using System.Text;

namespace BatchCommand.Api
{
    /// <summary>
    /// Shared text buffer implementation behind ApiTextInfo: each of its
    /// per-thread buffers (Error, Tip) is one instance of this class.
    /// All append/query/clear logic lives here once, so both collectors expose
    /// the same member set and external code calls it directly through the
    /// ApiTextInfo.Error / ApiTextInfo.Tip properties.
    /// </summary>
    public sealed class ApiTextBuffer
    {
        private readonly StringBuilder m_Text = new StringBuilder();

        public StringBuilder Text {
            get {
                return m_Text;
            }
        }

        public bool HasContent {
            get {
                return m_Text.Length > 0;
            }
        }

        public string GetText()
        {
            return m_Text.ToString();
        }

        public void Clear()
        {
            m_Text.Clear();
        }

        public void Append(string msg)
        {
            m_Text.Append(msg);
        }

        public void AppendLine(string msg)
        {
            m_Text.AppendLine(msg);
        }

        public void AppendFormat(string fmt, params object[] args)
        {
            if (args.Length == 0)
                m_Text.Append(fmt);
            else
                m_Text.AppendFormat(fmt, args);
        }

        public void AppendFormatLine(string fmt, params object[] args)
        {
            if (args.Length == 0)
                m_Text.AppendLine(fmt);
            else {
                m_Text.AppendFormat(fmt, args);
                m_Text.AppendLine();
            }
        }
    }
}
