using System;
using System.Collections.Generic;
using System.Linq;
using CefDotnetApp.AgentCore.Models;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    public class TaskManagement : ITaskManagement
    {
        private readonly Dictionary<string, TaskModel> _tasks;
        private readonly object _lockObject = new object();

        public TaskManagement()
        {
            _tasks = new Dictionary<string, TaskModel>();
        }

        public TaskModel CreateTask(string title, string description = null, TaskPriority priority = TaskPriority.Normal)
        {
            lock (_lockObject)
            {
                var task = new TaskModel(title, description)
                {
                    Priority = priority
                };

                _tasks[task.Id] = task;
                return task;
            }
        }

        public TaskModel GetTask(string taskId)
        {
            lock (_lockObject)
            {
                return _tasks.ContainsKey(taskId) ? _tasks[taskId] : null;
            }
        }

        public List<TaskModel> GetAllTasks()
        {
            lock (_lockObject)
            {
                return _tasks.Values.ToList();
            }
        }

        public List<TaskModel> GetTasksByStatus(AgentTaskStatus status)
        {
            lock (_lockObject)
            {
                return _tasks.Values.Where(t => t.Status == status).ToList();
            }
        }

        public List<TaskModel> GetTasksByPriority(TaskPriority priority)
        {
            lock (_lockObject)
            {
                return _tasks.Values.Where(t => t.Priority == priority).ToList();
            }
        }

        public bool UpdateTaskStatus(string taskId, AgentTaskStatus status, string result = null, string error = null)
        {
            lock (_lockObject)
            {
                if (!_tasks.ContainsKey(taskId))
                    return false;

                var task = _tasks[taskId];
                task.Status = status;

                if (status == AgentTaskStatus.InProgress && !task.StartTime.HasValue)
                {
                    task.StartTime = DateTime.Now;
                }
                else if (status == AgentTaskStatus.Completed || status == AgentTaskStatus.Failed || status == AgentTaskStatus.Cancelled)
                {
                    task.CompletedTime = DateTime.Now;
                }

                if (result != null)
                    task.Result = result;

                if (error != null)
                    task.Error = error;

                return true;
            }
        }

        public bool UpdateTaskPriority(string taskId, TaskPriority priority)
        {
            lock (_lockObject)
            {
                if (!_tasks.ContainsKey(taskId))
                    return false;

                _tasks[taskId].Priority = priority;
                return true;
            }
        }

        public bool AddTaskDependency(string taskId, string dependencyTaskId)
        {
            lock (_lockObject)
            {
                if (!_tasks.ContainsKey(taskId) || !_tasks.ContainsKey(dependencyTaskId))
                    return false;

                var task = _tasks[taskId];
                if (!task.Dependencies.Contains(dependencyTaskId))
                {
                    task.Dependencies.Add(dependencyTaskId);
                }

                return true;
            }
        }

        public bool RemoveTaskDependency(string taskId, string dependencyTaskId)
        {
            lock (_lockObject)
            {
                if (!_tasks.ContainsKey(taskId))
                    return false;

                var task = _tasks[taskId];
                return task.Dependencies.Remove(dependencyTaskId);
            }
        }

        public bool CanStartTask(string taskId)
        {
            lock (_lockObject)
            {
                if (!_tasks.ContainsKey(taskId))
                    return false;

                var task = _tasks[taskId];

                foreach (var depId in task.Dependencies)
                {
                    if (!_tasks.ContainsKey(depId))
                        continue;

                    var depTask = _tasks[depId];
                    if (depTask.Status != AgentTaskStatus.Completed)
                        return false;
                }

                return true;
            }
        }

        public bool DeleteTask(string taskId)
        {
            lock (_lockObject)
            {
                return _tasks.Remove(taskId);
            }
        }

        public bool SetTaskMetadata(string taskId, string key, object value)
        {
            lock (_lockObject)
            {
                if (!_tasks.ContainsKey(taskId))
                    return false;

                _tasks[taskId].Metadata[key] = value;
                return true;
            }
        }

        public object GetTaskMetadata(string taskId, string key)
        {
            lock (_lockObject)
            {
                if (!_tasks.ContainsKey(taskId))
                    return null;

                var task = _tasks[taskId];
                return task.Metadata.ContainsKey(key) ? task.Metadata[key] : null;
            }
        }

        public void ClearCompletedTasks()
        {
            lock (_lockObject)
            {
                var completedTaskIds = _tasks.Values
                    .Where(t => t.Status == AgentTaskStatus.Completed ||
                               t.Status == AgentTaskStatus.Failed ||
                               t.Status == AgentTaskStatus.Cancelled)
                    .Select(t => t.Id)
                    .ToList();

                foreach (var taskId in completedTaskIds)
                {
                    _tasks.Remove(taskId);
                }
            }
        }

        public void ClearAllTasks()
        {
            lock (_lockObject)
            {
                _tasks.Clear();
            }
        }

        public int GetTaskCount()
        {
            lock (_lockObject)
            {
                return _tasks.Count;
            }
        }

        public Dictionary<AgentTaskStatus, int> GetTaskStatistics()
        {
            lock (_lockObject)
            {
                var stats = new Dictionary<AgentTaskStatus, int>();

                foreach (AgentTaskStatus status in Enum.GetValues(typeof(AgentTaskStatus)))
                {
                    stats[status] = _tasks.Values.Count(t => t.Status == status);
                }

                return stats;
            }
        }
    }
}
