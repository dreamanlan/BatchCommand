using System;
using System.Collections.Generic;
using System.Linq;
using CefDotnetApp.AgentCore.Models;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    public class LLMInteraction : ILLMInteraction
    {
        private readonly Dictionary<string, LLMConversation> _conversations;
        private readonly object _lockObject = new object();
        private string _currentConversationId;

        public LLMInteraction()
        {
            _conversations = new Dictionary<string, LLMConversation>();
        }

        public LLMConversation CreateConversation(string title = null)
        {
            lock (_lockObject)
            {
                var conversation = new LLMConversation
                {
                    Title = title ?? $"Conversation {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                };

                _conversations[conversation.Id] = conversation;
                _currentConversationId = conversation.Id;

                return conversation;
            }
        }

        public LLMConversation GetConversation(string conversationId)
        {
            lock (_lockObject)
            {
                return _conversations.ContainsKey(conversationId) ? _conversations[conversationId] : null;
            }
        }

        public LLMConversation GetCurrentConversation()
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentConversationId))
                    return null;

                return GetConversation(_currentConversationId);
            }
        }

        public bool SetCurrentConversation(string conversationId)
        {
            lock (_lockObject)
            {
                if (!_conversations.ContainsKey(conversationId))
                    return false;

                _currentConversationId = conversationId;
                return true;
            }
        }

        public List<LLMConversation> GetAllConversations()
        {
            lock (_lockObject)
            {
                return _conversations.Values.OrderByDescending(c => c.LastUpdateTime).ToList();
            }
        }

        public bool AddMessage(string conversationId, LLMRole role, string content)
        {
            lock (_lockObject)
            {
                if (!_conversations.ContainsKey(conversationId))
                    return false;

                var conversation = _conversations[conversationId];
                conversation.AddMessage(role, content);

                return true;
            }
        }

        public bool AddMessageToCurrent(LLMRole role, string content)
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentConversationId))
                {
                    CreateConversation();
                }

                return AddMessage(_currentConversationId, role, content);
            }
        }

        public bool AddSystemMessage(string conversationId, string content)
        {
            return AddMessage(conversationId, LLMRole.System, content);
        }

        public bool AddUserMessage(string conversationId, string content)
        {
            return AddMessage(conversationId, LLMRole.User, content);
        }

        public bool AddAssistantMessage(string conversationId, string content)
        {
            return AddMessage(conversationId, LLMRole.Assistant, content);
        }

        public List<LLMMessage> GetMessages(string conversationId, int maxCount = 0)
        {
            lock (_lockObject)
            {
                if (!_conversations.ContainsKey(conversationId))
                    return new List<LLMMessage>();

                var conversation = _conversations[conversationId];
                var messages = conversation.Messages;

                if (maxCount > 0 && messages.Count > maxCount)
                {
                    return messages.Skip(messages.Count - maxCount).ToList();
                }

                return new List<LLMMessage>(messages);
            }
        }

        public List<LLMMessage> GetCurrentMessages(int maxCount = 0)
        {
            lock (_lockObject)
            {
                if (string.IsNullOrEmpty(_currentConversationId))
                    return new List<LLMMessage>();

                return GetMessages(_currentConversationId, maxCount);
            }
        }

        public bool ClearMessages(string conversationId)
        {
            lock (_lockObject)
            {
                if (!_conversations.ContainsKey(conversationId))
                    return false;

                var conversation = _conversations[conversationId];
                conversation.Messages.Clear();
                conversation.LastUpdateTime = DateTime.Now;

                return true;
            }
        }

        public bool DeleteConversation(string conversationId)
        {
            lock (_lockObject)
            {
                if (_currentConversationId == conversationId)
                    _currentConversationId = null;

                return _conversations.Remove(conversationId);
            }
        }

        public bool UpdateConversationTitle(string conversationId, string title)
        {
            lock (_lockObject)
            {
                if (!_conversations.ContainsKey(conversationId))
                    return false;

                _conversations[conversationId].Title = title;
                return true;
            }
        }

        public bool SetConversationMetadata(string conversationId, string key, object value)
        {
            lock (_lockObject)
            {
                if (!_conversations.ContainsKey(conversationId))
                    return false;

                _conversations[conversationId].Metadata[key] = value;
                return true;
            }
        }

        public object GetConversationMetadata(string conversationId, string key)
        {
            lock (_lockObject)
            {
                if (!_conversations.ContainsKey(conversationId))
                    return null;

                var conversation = _conversations[conversationId];
                return conversation.Metadata.ContainsKey(key) ? conversation.Metadata[key] : null;
            }
        }

        public string ExportConversation(string conversationId, bool includeMetadata = false)
        {
            lock (_lockObject)
            {
                if (!_conversations.ContainsKey(conversationId))
                    return null;

                var conversation = _conversations[conversationId];
                var lines = new List<string>();

                lines.Add($"# {conversation.Title}");
                lines.Add($"Created: {conversation.CreatedTime:yyyy-MM-dd HH:mm:ss}");
                lines.Add($"Last Updated: {conversation.LastUpdateTime:yyyy-MM-dd HH:mm:ss}");
                lines.Add("");

                foreach (var message in conversation.Messages)
                {
                    lines.Add($"## {message.Role} ({message.Timestamp:yyyy-MM-dd HH:mm:ss})");
                    lines.Add(message.Content);
                    lines.Add("");
                }

                if (includeMetadata && conversation.Metadata.Count > 0)
                {
                    lines.Add("## Metadata");
                    foreach (var kvp in conversation.Metadata)
                    {
                        lines.Add($"- {kvp.Key}: {kvp.Value}");
                    }
                }

                return string.Join(Environment.NewLine, lines);
            }
        }

        public void ClearAllConversations()
        {
            lock (_lockObject)
            {
                _conversations.Clear();
                _currentConversationId = null;
            }
        }

        public int GetConversationCount()
        {
            lock (_lockObject)
            {
                return _conversations.Count;
            }
        }
    }
}
