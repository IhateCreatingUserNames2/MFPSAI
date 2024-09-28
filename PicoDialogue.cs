using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class PicoDialogue : MonoBehaviour
{
    // URL for the Ollama server
    private string ollamaURL = "http://localhost:1145/generate";

    // The model to use (ensure this matches your installed model)
    private string model = "llama-3.2-1b";

    // UI elements to display dialogue
    public Canvas dialogueCanvas;
    public Text dialogueText;

    // UI elements for player input
    public GameObject playerInputPanel;
    public InputField playerInputField;
    public Button sendButton;

    // Flag to check if the player is nearby
    private bool isPlayerNear = false;

    // NPC configurations
    public string npcName = "PICO";
    public string npcPersonality = "An adventurous and curious AI.";
    public string npcBackground = "PICO is a robotic companion exploring the world.";

    void Start()
    {
        // Hide the dialogue canvas at the start
        if (dialogueCanvas != null)
        {
            dialogueCanvas.enabled = false;
        }

        // Hide the player input panel at the start
        if (playerInputPanel != null)
        {
            playerInputPanel.SetActive(false);
        }

        // Add listener to the send button
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }
    }

    void Update()
    {
        // Check for player input when near the NPC
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            // Open the player input UI
            OpenPlayerInputUI();
        }

        // Optionally, detect 'G' key for configuration
        if (isPlayerNear && Input.GetKeyDown(KeyCode.G))
        {
            // Implement configuration UI if needed
            // OpenConfigurationUI();
        }
    }

    // Function to open player input UI
    void OpenPlayerInputUI()
    {
        if (playerInputPanel != null)
        {
            playerInputPanel.SetActive(true);
            playerInputField.text = "";
            playerInputField.ActivateInputField();
        }
    }

    // Callback when send button is clicked
    void OnSendButtonClicked()
    {
        string playerInput = playerInputField.text;
        playerInputPanel.SetActive(false);

        // Start the coroutine to get the response
        StartCoroutine(GetLlamaResponse(playerInput));
    }

    IEnumerator GetLlamaResponse(string playerInput)
    {
        // Build the prompt with NPC configurations
        string prompt = $"The following is a conversation between a player and an NPC.\n" +
                        $"NPC Name: {npcName}\n" +
                        $"Personality: {npcPersonality}\n" +
                        $"Background: {npcBackground}\n\n" +
                        $"Player: {playerInput}\n" +
                        $"{npcName}:";

        // Create JSON data with the prompt and model
        string jsonData = $"{{\"model\": \"{model}\", \"prompt\": \"{prompt}\"}}";

        // Convert JSON data to bytes
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // Create a UnityWebRequest with the JSON data
        UnityWebRequest www = new UnityWebRequest(ollamaURL, "POST");
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for a response
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string response = www.downloadHandler.text;

            // Parse the response (assuming it's plain text)
            string npcReply = ParseOllamaResponse(response);

            // Display the dialogue
            ShowDialogue(npcReply);
        }
        else
        {
            Debug.LogError("Error communicating with Ollama: " + www.error);
            ShowDialogue("Sorry, I'm having trouble communicating right now.");
        }
    }

    // Function to parse the response from Ollama
    string ParseOllamaResponse(string response)
    {
        // If Ollama returns plain text, return it directly
        // If Ollama returns JSON, adjust parsing accordingly
        return response.Trim();
    }

    // Function to display dialogue
    void ShowDialogue(string message)
    {
        if (dialogueCanvas != null && dialogueText != null)
        {
            dialogueText.text = message;
            dialogueCanvas.enabled = true;

            // Optionally hide the dialogue after some time
            Invoke("HideDialogue", 5f); // Hides after 5 seconds
        }
    }

    void HideDialogue()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.enabled = false;
        }
    }

    // Detect when the player enters the interaction zone
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            // Optionally, display a prompt like "Press E to talk"
        }
    }

    // Detect when the player leaves the interaction zone
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            // Optionally, hide any interaction prompts
        }
    }
}
