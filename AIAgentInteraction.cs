using UnityEngine;

public class AIAgentInteraction
{
    public void InteractWithAIAgent(string agentId, string userId, string message)
    {
        AIAgent agent = AIManager.Instance.GetAIAgent(agentId);
        if (agent != null)
        {
            // Store conversation log
            agent.AddToConversation(userId, message);

            // Generate a response (This should hook into your AI backend)
            string aiResponse = GetAIResponse(agent, message);

            // Store the AI response
            agent.AddToConversation(userId, aiResponse);

            Debug.Log($"AI {agent.AgentName} responded: {aiResponse}");

            // Optionally send response back to the chat system here
        }
    }

    // Example of AI response (this would come from your Llama/embedding model)
    private string GetAIResponse(AIAgent agent, string message)
    {
        return $"AI Response to '{message}' from {agent.AgentName}";
    }
}
