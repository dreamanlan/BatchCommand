using System.Text.Json.Serialization;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Simple role/content chat message used by OpenAI/Claude/Ollama providers
    /// for local history storage. Explicit POCO instead of anonymous objects,
    /// so System.Text.Json can cache serialization metadata and avoid the
    /// reflection cost paid on every anonymous-type send.
    ///
    /// Image attachments are NOT stored here; they are attached at send time
    /// only, since each vendor has a different wire format for image content.
    /// </summary>
    internal sealed class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";

        public ChatMessage() { }

        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }
}
