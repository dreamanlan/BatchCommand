using System;
using System.Collections.Generic;
using System.Linq;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    public class ContextManagement : IContextManagement
    {
        private readonly Dictionary<string, WorkspaceContext> _workspaces;
        private readonly Dictionary<string, SessionContext> _sessions;
        private readonly object _lockObject = new object();
        private string _currentWorkspaceId;
        private string _currentSessionId;

        public ContextManagement()
        {
            _workspaces = new Dictionary<string, WorkspaceContext>();
            _sessions = new Dictionary<string, SessionContext>();
        }

        public WorkspaceContext CreateWorkspace(string name, string rootPath)
        {
            lock (_lockObject)
            {
                var workspace = new WorkspaceContext
                {
                    Name = name,
                    RootPath = rootPath
                };

                _workspaces[workspace.Id] = workspace;
                _currentWorkspaceId = workspace.Id;

                return workspace;
            }
        }

        public WorkspaceContext GetWorkspace(string workspaceId)
        {
            lock (_lockObject)
            {
                return _workspaces.ContainsKey(workspaceId) ? _workspaces[workspaceId] : null;
            }
        }

        public WorkspaceContext GetCurrentWorkspace()
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentWorkspaceId))
                    return null;

                return GetWorkspace(_currentWorkspaceId);
            }
        }

        public bool SetCurrentWorkspace(string workspaceId)
        {
            lock (_lockObject)
            {
                if (!_workspaces.ContainsKey(workspaceId))
                    return false;

                _currentWorkspaceId = workspaceId;
                return true;
            }
        }

        public List<WorkspaceContext> GetAllWorkspaces()
        {
            lock (_lockObject)
            {
                return _workspaces.Values.ToList();
            }
        }

        public bool DeleteWorkspace(string workspaceId)
        {
            lock (_lockObject)
            {
                if (_currentWorkspaceId == workspaceId)
                    _currentWorkspaceId = null;

                return _workspaces.Remove(workspaceId);
            }
        }

        public SessionContext CreateSession(string title = null)
        {
            lock (_lockObject)
            {
                var session = new SessionContext
                {
                    Title = title ?? $"Session {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    WorkspaceId = _currentWorkspaceId
                };

                _sessions[session.Id] = session;
                _currentSessionId = session.Id;

                return session;
            }
        }

        public SessionContext GetSession(string sessionId)
        {
            lock (_lockObject)
            {
                return _sessions.ContainsKey(sessionId) ? _sessions[sessionId] : null;
            }
        }

        public SessionContext GetCurrentSession()
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentSessionId))
                    return null;

                return GetSession(_currentSessionId);
            }
        }

        public bool SetCurrentSession(string sessionId)
        {
            lock (_lockObject)
            {
                if (!_sessions.ContainsKey(sessionId))
                    return false;

                _currentSessionId = sessionId;
                return true;
            }
        }

        public List<SessionContext> GetAllSessions()
        {
            lock (_lockObject)
            {
                return _sessions.Values.OrderByDescending(s => s.LastUpdateTime).ToList();
            }
        }

        public bool DeleteSession(string sessionId)
        {
            lock (_lockObject)
            {
                if (_currentSessionId == sessionId)
                    _currentSessionId = null;

                return _sessions.Remove(sessionId);
            }
        }

        public bool SetContextVariable(string key, object value, ContextScope scope = ContextScope.Session)
        {
            lock (_lockObject)
            {
                if (scope == ContextScope.Session)
                {
                    if (string.IsNullOrEmpty(_currentSessionId))
                        return false;

                    var session = _sessions[_currentSessionId];
                    session.Variables[key] = value;
                    session.LastUpdateTime = DateTime.Now;
                    return true;
                }
                else if (scope == ContextScope.Workspace)
                {
                    if (string.IsNullOrEmpty(_currentWorkspaceId))
                        return false;

                    var workspace = _workspaces[_currentWorkspaceId];
                    workspace.Variables[key] = value;
                    workspace.LastUpdateTime = DateTime.Now;
                    return true;
                }

                return false;
            }
        }

        public object GetContextVariable(string key, ContextScope scope = ContextScope.Session)
        {
            lock (_lockObject)
            {
                if (scope == ContextScope.Session)
                {
                    if (string.IsNullOrEmpty(_currentSessionId))
                        return null;

                    var session = _sessions[_currentSessionId];
                    return session.Variables.ContainsKey(key) ? session.Variables[key] : null;
                }
                else if (scope == ContextScope.Workspace)
                {
                    if (string.IsNullOrEmpty(_currentWorkspaceId))
                        return null;

                    var workspace = _workspaces[_currentWorkspaceId];
                    return workspace.Variables.ContainsKey(key) ? workspace.Variables[key] : null;
                }

                return null;
            }
        }

        public bool AddOpenFile(string filePath)
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentWorkspaceId))
                    return false;

                var workspace = _workspaces[_currentWorkspaceId];
                if (!workspace.OpenFiles.Contains(filePath))
                {
                    workspace.OpenFiles.Add(filePath);
                    workspace.LastUpdateTime = DateTime.Now;
                }

                return true;
            }
        }

        public bool RemoveOpenFile(string filePath)
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentWorkspaceId))
                    return false;

                var workspace = _workspaces[_currentWorkspaceId];
                bool removed = workspace.OpenFiles.Remove(filePath);
                if (removed)
                    workspace.LastUpdateTime = DateTime.Now;

                return removed;
            }
        }

        public List<string> GetOpenFiles()
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentWorkspaceId))
                    return new List<string>();

                var workspace = _workspaces[_currentWorkspaceId];
                return new List<string>(workspace.OpenFiles);
            }
        }

        public bool AddRecentFile(string filePath)
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentWorkspaceId))
                    return false;

                var workspace = _workspaces[_currentWorkspaceId];
                workspace.RecentFiles.Remove(filePath);
                workspace.RecentFiles.Insert(0, filePath);

                if (workspace.RecentFiles.Count > 50)
                    workspace.RecentFiles.RemoveAt(workspace.RecentFiles.Count - 1);

                workspace.LastUpdateTime = DateTime.Now;
                return true;
            }
        }

        public List<string> GetRecentFiles(int maxCount = 10)
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentWorkspaceId))
                    return new List<string>();

                var workspace = _workspaces[_currentWorkspaceId];
                return workspace.RecentFiles.Take(maxCount).ToList();
            }
        }

        public void ClearAllContexts()
        {
            lock (_lockObject)
            {
                _workspaces.Clear();
                _sessions.Clear();
                _currentWorkspaceId = null;
                _currentSessionId = null;
            }
        }
    }

    public class WorkspaceContext
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RootPath { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public List<string> OpenFiles { get; set; }
        public List<string> RecentFiles { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public WorkspaceContext()
        {
            Id = Guid.NewGuid().ToString();
            CreatedTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            Variables = new Dictionary<string, object>();
            OpenFiles = new List<string>();
            RecentFiles = new List<string>();
            Metadata = new Dictionary<string, object>();
        }
    }

    public class SessionContext
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string WorkspaceId { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public SessionContext()
        {
            Id = Guid.NewGuid().ToString();
            CreatedTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            Variables = new Dictionary<string, object>();
            Metadata = new Dictionary<string, object>();
        }
    }

    public enum ContextScope
    {
        Session,
        Workspace
    }
}
