using Newtonsoft.Json;
using OpenAI_API;

namespace CCIPICL_ChatAssistant.Models
{
    public class ThreadResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public ThreadData Data { get; set; }
    }

    public class ThreadData
    {
        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }
    }
    public class RunResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long CreatedAt { get; set; }
        public string AssistantId { get; set; }
        public string ThreadId { get; set; }
        public string Status { get; set; }
        public long? StartedAt { get; set; }
        public long? ExpiresAt { get; set; }
        public long? CancelledAt { get; set; }
        public long? FailedAt { get; set; }
        public long? CompletedAt { get; set; }
        public string LastError { get; set; }
        public string Model { get; set; }
        public string Instructions { get; set; }
        public List<Tool> Tools { get; set; } = new List<Tool>();
        public List<string> FileIds { get; set; } = new List<string>();
        public Metadata Metadata { get; set; }
        public Usage Usage { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
    }
    public class ThreadMessagesResponse
    {
        public string Object { get; set; }
        public List<Message> Messages { get; set; } = new List<Message>();
    }

    public class Tool
    {
        public string Type { get; set; }
    }
    public class Metadata
    {
        // Example of possible fields - adjust based on actual use
        public string RequestId { get; set; } // Unique identifier for the request
        public string UserContext { get; set; } // Information about the user or session, if applicable

        // If the metadata can contain arbitrary key-value pairs,
        // you might consider using a dictionary:
        public Dictionary<string, string> AdditionalInfo { get; set; } = new Dictionary<string, string>();
    }

    public class Usage
    {
        public int TokenCount { get; set; } // Number of tokens processed in the request
        public double ComputeTime { get; set; } // Compute time in seconds, if available
        public int APIRequestsCount { get; set; } // Number of API requests made, if tracked

        // You can expand this class with more properties as you identify them
        // in the API's responses or documentation.
    }


    public class Message
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public string Role { get; set; }
        public Content Content { get; set; }
        public long CreatedAt { get; set; }
    }

    public class Content
    {
        public string Text { get; set; }
    }
}

