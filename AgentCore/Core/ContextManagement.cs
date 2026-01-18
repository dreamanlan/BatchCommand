using System;
using System.Collections.Generic;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Simplified context management that only handles variable storage
    /// </summary>
    public class ContextManagement
    {
        private readonly Dictionary<string, object> _sessionVariables;
        private readonly Dictionary<string, object> _workspaceVariables;
        private readonly object _lockObject = new object();

        public ContextManagement()
        {
            _sessionVariables = new Dictionary<string, object>();
            _workspaceVariables = new Dictionary<string, object>();
        }

        public bool SetContextVariable(string key, object value, ContextScope scope = ContextScope.Session)
        {
            lock (_lockObject) {
                if (scope == ContextScope.Session) {
                    _sessionVariables[key] = value;
                    return true;
                }
                else if (scope == ContextScope.Workspace) {
                    _workspaceVariables[key] = value;
                    return true;
                }

                return false;
            }
        }

        public object GetContextVariable(string key, ContextScope scope = ContextScope.Session)
        {
            lock (_lockObject) {
                if (scope == ContextScope.Session) {
                    return _sessionVariables.ContainsKey(key) ? _sessionVariables[key] : null;
                }
                else if (scope == ContextScope.Workspace) {
                    return _workspaceVariables.ContainsKey(key) ? _workspaceVariables[key] : null;
                }

                return null;
            }
        }

        public bool RemoveContextVariable(string key, ContextScope scope = ContextScope.Session)
        {
            lock (_lockObject) {
                if (scope == ContextScope.Session) {
                    return _sessionVariables.Remove(key);
                }
                else if (scope == ContextScope.Workspace) {
                    return _workspaceVariables.Remove(key);
                }

                return false;
            }
        }

        public void ClearSessionVariables()
        {
            lock (_lockObject) {
                _sessionVariables.Clear();
            }
        }

        public void ClearWorkspaceVariables()
        {
            lock (_lockObject) {
                _workspaceVariables.Clear();
            }
        }

        public void ClearAllVariables()
        {
            lock (_lockObject) {
                _sessionVariables.Clear();
                _workspaceVariables.Clear();
            }
        }

        public Dictionary<string, object> GetAllSessionVariables()
        {
            lock (_lockObject) {
                return new Dictionary<string, object>(_sessionVariables);
            }
        }

        public Dictionary<string, object> GetAllWorkspaceVariables()
        {
            lock (_lockObject) {
                return new Dictionary<string, object>(_workspaceVariables);
            }
        }
    }

    public enum ContextScope
    {
        Session,
        Workspace
    }
}
