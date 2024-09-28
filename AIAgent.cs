using System.Collections.Generic;
using UnityEngine;

public class AIAgent
{
    public string AgentId { get; private set; }
    public string AgentName { get; private set; }

    // Chat history with different users
    public Dictionary<string, List<string>> UserConversations;

    // Embeddings for the AI agent (e.g., custom data for memory)
    public Dictionary<string, float[]> Embeddings;

    public AIAgent(string id, string name)
    {
        AgentId = id;
        AgentName = name;
        UserConversations = new Dictionary<string, List<string>>();
        Embeddings = new Dictionary<string, float[]>();
    }

    // Add a message to the conversation log for a specific user
    public void AddToConversation(string userId, string message)
    {
        if (!UserConversations.ContainsKey(userId))
        {
            UserConversations[userId] = new List<string>();
        }
        UserConversations[userId].Add(message);
    }

    // Retrieve conversation log for a specific user
    public List<string> GetConversation(string userId)
    {
        if (UserConversations.ContainsKey(userId))
        {
            return UserConversations[userId];
        }
        return new List<string>();
    }

    // Add an embedding for the AI agent
    public void AddEmbedding(string key, float[] embedding)
    {
        if (!Embeddings.ContainsKey(key))
        {
            Embeddings[key] = embedding;
        }
    }

    public float[] GetEmbedding(string key)
    {
        if (Embeddings.ContainsKey(key))
        {
            return Embeddings[key];
        }
        return null;
    }
}
