using System;
using System.Collections.Generic;

namespace CefDotnetApp.AgentCore.Models
{
    public enum LLMRole
    {
        System,
        User,
        Assistant
    }

    public class LLMMessage
    {
        public LLMRole Role { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public LLMMessage()
        {
            Timestamp = DateTime.Now;
            Metadata = new Dictionary<string, object>();
        }

        public LLMMessage(LLMRole role, string content) : this()
        {
            Role = role;
            Content = content;
        }
    }

    public class LLMConversation
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<LLMMessage> Messages { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public LLMConversation()
        {
            Id = Guid.NewGuid().ToString();
            Messages = new List<LLMMessage>();
            CreatedTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            Metadata = new Dictionary<string, object>();
        }

        public void AddMessage(LLMRole role, string content)
        {
            Messages.Add(new LLMMessage(role, content));
            LastUpdateTime = DateTime.Now;
        }
    }
}
