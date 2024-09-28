using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    // Singleton instance
    public static AIManager Instance;

    // Dictionary to store all AI agents
    private Dictionary<string, AIAgent> aiAgents;

    void Awake()
    {
        // Ensure the AIManager is a singleton
        if (Instance == null)
        {
            Instance = this;
            aiAgents = new Dictionary<string, AIAgent>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Add an AI agent to the manager
    public void RegisterAIAgent(AIAgent aiAgent)
    {
        if (!aiAgents.ContainsKey(aiAgent.AgentId))
        {
            aiAgents.Add(aiAgent.AgentId, aiAgent);
            Debug.Log($"AI Agent {aiAgent.AgentName} registered successfully.");
        }
    }

    // Retrieve an AI agent by ID
    public AIAgent GetAIAgent(string agentId)
    {
        if (aiAgents.ContainsKey(agentId))
        {
            return aiAgents[agentId];
        }
        Debug.LogError($"AI Agent with ID {agentId} not found.");
        return null;
    }

    // Save all AI agent data
    public void SaveAllData()
    {
        foreach (var agent in aiAgents.Values)
        {
            SaveSystem.SaveAgentData(agent);
        }
    }

    // Load all AI agent data
    public void LoadAllData()
    {
        foreach (var agent in aiAgents.Values)
        {
            SaveSystem.LoadAgentData(agent);
        }
    }
}
