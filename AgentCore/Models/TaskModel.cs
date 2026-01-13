using System;
using System.Collections.Generic;

namespace CefDotnetApp.AgentCore.Models
{
    public enum AgentTaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }

    public enum TaskPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    public class TaskModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public AgentTaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public List<string> Dependencies { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public string Result { get; set; }
        public string Error { get; set; }

        public TaskModel()
        {
            Id = Guid.NewGuid().ToString();
            Status = AgentTaskStatus.Pending;
            Priority = TaskPriority.Normal;
            CreatedTime = DateTime.Now;
            Dependencies = new List<string>();
            Metadata = new Dictionary<string, object>();
        }

        public TaskModel(string title, string description = null) : this()
        {
            Title = title;
            Description = description;
        }
    }
}
