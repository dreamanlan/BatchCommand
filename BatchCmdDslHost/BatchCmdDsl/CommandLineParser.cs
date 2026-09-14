using System;
using System.Collections.Generic;
using System.Text;

namespace BatchCmdDsl
{
    /// <summary>
    /// Parsed command line result: switches (key/value pairs) and positional arguments.
    /// </summary>
    public sealed class CommandLineOptions
    {
        private readonly Dictionary<string, List<string>> _switches = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _args = new List<string>();

        public IReadOnlyDictionary<string, List<string>> Switches => _switches;
        public IReadOnlyList<string> Args => _args;

        public bool HasSwitch(string name)
        {
            return _switches.ContainsKey(name);
        }

        /// <summary>
        /// Get the first value of a switch. A bare switch without value has value "true".
        /// </summary>
        public string? GetSwitchValue(string name)
        {
            if (_switches.TryGetValue(name, out var values) && values.Count > 0) {
                return values[0];
            }
            return null;
        }

        public IReadOnlyList<string> GetSwitchValues(string name)
        {
            if (_switches.TryGetValue(name, out var values)) {
                return values;
            }
            return Array.Empty<string>();
        }

        internal void AddSwitchValue(string name, string value)
        {
            if (string.IsNullOrEmpty(name)) {
                return;
            }
            if (!_switches.TryGetValue(name, out var values)) {
                values = new List<string>();
                _switches.Add(name, values);
            }
            values.Add(value ?? string.Empty);
        }

        internal void AddArg(string arg)
        {
            _args.Add(arg ?? string.Empty);
        }

        internal void RemoveFirstArg()
        {
            if (_args.Count > 0) {
                _args.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Generic command line parser.
    /// Supported switch forms: --key=value, --key value, -key value, -key:value, /key:value, /key value.
    /// A bare switch without value is stored as "true". A bare "--" token ends switch parsing.
    /// Note: a separate value starting with '-' or '/' (e.g. a negative number) is not consumed
    /// as a switch value, use --key=value or quote it after "--" instead.
    /// </summary>
    public static class CommandLineParser
    {
        /// <summary>
        /// Parse a raw command line string which contains the program name as the first token.
        /// The first token is skipped.
        /// </summary>
        public static CommandLineOptions Parse(string commandLine)
        {
            var argv = SplitCommandLine(commandLine);
            // Skip the program name (argv[0]) in a raw command line
            if (argv.Count > 0) {
                argv.RemoveAt(0);
            }
            return Parse(argv);
        }

        /// <summary>
        /// Parse an argument list without the program name.
        /// </summary>
        public static CommandLineOptions Parse(IReadOnlyList<string> argv)
        {
            var options = new CommandLineOptions();
            if (null == argv) {
                return options;
            }
            bool endOfSwitches = false;
            for (int i = 0; i < argv.Count; i++) {
                string token = argv[i];
                if (!endOfSwitches && token == "--") {
                    endOfSwitches = true;
                    continue;
                }
                if (!endOfSwitches && IsSwitch(token, out int nameOffset)) {
                    string body = token.Substring(nameOffset);
                    string name = body;
                    string? inlineValue = null;
                    int sep = body.IndexOfAny(new char[] { '=', ':' });
                    if (sep >= 0) {
                        name = body.Substring(0, sep);
                        inlineValue = body.Substring(sep + 1);
                    }
                    if (name.Length == 0) {
                        // token like "-" or "-=x", keep it as a positional arg
                        options.AddArg(token);
                        continue;
                    }
                    if (null != inlineValue) {
                        options.AddSwitchValue(name, inlineValue);
                    }
                    else if (i + 1 < argv.Count && argv[i + 1] != "--" && !IsSwitch(argv[i + 1], out _)) {
                        ++i;
                        options.AddSwitchValue(name, argv[i]);
                    }
                    else {
                        options.AddSwitchValue(name, "true");
                    }
                }
                else {
                    options.AddArg(token);
                }
            }
            return options;
        }

        private static bool IsSwitch(string token, out int nameOffset)
        {
            nameOffset = 0;
            if (string.IsNullOrEmpty(token)) {
                return false;
            }
            if (token.StartsWith("--")) {
                nameOffset = 2;
                return token.Length > 2;
            }
            if (token.StartsWith("-") || token.StartsWith("/")) {
                nameOffset = 1;
                return token.Length > 1;
            }
            return false;
        }

        /// <summary>
        /// Split a raw command line with Windows command line rules
        /// (same as CommandLineToArgvW: 2n backslashes before a quote produce n backslashes,
        /// 2n+1 backslashes before a quote produce n backslashes plus a literal quote).
        /// </summary>
        private static List<string> SplitCommandLine(string commandLine)
        {
            var args = new List<string>();
            if (string.IsNullOrEmpty(commandLine)) {
                return args;
            }
            var current = new StringBuilder();
            int backslashes = 0;
            bool inQuotes = false;
            bool hasToken = false;
            foreach (char c in commandLine) {
                if (c == '\\') {
                    ++backslashes;
                    continue;
                }
                if (c == '"') {
                    current.Append('\\', backslashes / 2);
                    if (backslashes % 2 == 1) {
                        current.Append('"');
                    }
                    else {
                        inQuotes = !inQuotes;
                    }
                    backslashes = 0;
                    hasToken = true;
                    continue;
                }
                if (backslashes > 0) {
                    current.Append('\\', backslashes);
                    backslashes = 0;
                }
                if (!inQuotes && char.IsWhiteSpace(c)) {
                    if (hasToken || current.Length > 0) {
                        args.Add(current.ToString());
                        current.Clear();
                        hasToken = false;
                    }
                    continue;
                }
                current.Append(c);
                hasToken = true;
            }
            if (backslashes > 0) {
                current.Append('\\', backslashes);
            }
            if (hasToken || current.Length > 0) {
                args.Add(current.ToString());
            }
            return args;
        }
    }
}
