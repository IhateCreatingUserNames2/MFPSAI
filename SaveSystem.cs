using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    // Path to save the data locally
    private static string path = Application.persistentDataPath + "/agentData/";

    public static void SaveAgentData(AIAgent agent)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string filePath = path + agent.AgentId + ".dat";

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Create);

        AIAgentData data = new AIAgentData(agent);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static void LoadAgentData(AIAgent agent)
    {
        string filePath = path + agent.AgentId + ".dat";

        if (File.Exists(filePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(filePath, FileMode.Open);

            AIAgentData data = formatter.Deserialize(stream) as AIAgentData;
            stream.Close();

            if (data != null)
            {
                agent.UserConversations = data.userConversations;
                agent.Embeddings = data.embeddings;
            }
        }
        else
        {
            Debug.LogWarning($"No saved data for agent {agent.AgentId} found.");
        }
    }
}

// Serializable class to hold the agent's data
[System.Serializable]
public class AIAgentData
{
    public Dictionary<string, List<string>> userConversations;
    public Dictionary<string, float[]> embeddings;

    public AIAgentData(AIAgent agent)
    {
        userConversations = agent.UserConversations;
        embeddings = agent.Embeddings;
    }
}
