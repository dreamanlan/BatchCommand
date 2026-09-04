using System;
using System.Collections.Generic;

namespace AgentCore.Core
{
    /// <summary>
    /// Simplified context management that only handles variable storage.
    /// One instance holds exactly one set of key/value pairs. The global
    /// context is a single shared instance, and every AgentInstance owns
    /// a private one, so there is no scope parameter anymore.
    /// </summary>
    public class DslContextManagement
    {
        private readonly Dictionary<string, object> _variables;
        private readonly object _lockObject = new object();

        public DslContextManagement()
        {
            _variables = new Dictionary<string, object>();
        }

        public bool SetContextVariable(string key, object value)
        {
            lock (_lockObject) {
                _variables[key] = value;
                return true;
            }
        }

        public object? GetContextVariable(string key)
        {
            lock (_lockObject) {
                return _variables.TryGetValue(key, out var value) ? value : null;
            }
        }

        public bool RemoveContextVariable(string key)
        {
            lock (_lockObject) {
                return _variables.Remove(key);
            }
        }

        public void ClearVariables()
        {
            lock (_lockObject) {
                _variables.Clear();
            }
        }

        public Dictionary<string, object> GetAllVariables()
        {
            lock (_lockObject) {
                return new Dictionary<string, object>(_variables);
            }
        }
    }
}
