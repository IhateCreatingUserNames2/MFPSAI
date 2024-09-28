using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class PicoDialogue : MonoBehaviour
{
    // URL for the Ollama server
    private string ollamaURL = "http://localhost:11434/api/generate";

    // The model to use
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
            sendButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

        // Ensure fonts are assigned
        AssignFonts();
    }

    void Update()
    {
        // Check for player input when near the NPC
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            // Open the player input UI
            OpenPlayerInputUI();
        }
    }

    // Function to open player input UI
    void OpenPlayerInputUI()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.enabled = true; // Make sure the canvas is active

            if (playerInputPanel != null)
            {
                playerInputPanel.SetActive(true);
                playerInputField.text = "";  // Clear the input field
                playerInputField.ActivateInputField();
            }
            else
            {
                Debug.LogError("Player Input Panel is not assigned.");
            }
        }
        else
        {
            Debug.LogError("Dialogue Canvas is not assigned.");
        }
    }



    // Callback when send button is clicked
    public void OnSendButtonClicked()
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

        // Use a static JSON string to debug the issue
        string jsonData = "{\"model\": \"llama3\", \"prompt\": \"Hello, how are you?\", \"max_length\": 150, \"temperature\": 0.7}";

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
            Debug.Log("Server Response: " + www.downloadHandler.text);  // Log the server response
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
        Debug.Log("OnTriggerEnter with " + other.name);
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("Player is near the NPC");
        }
    }

    // Detect when the player leaves the interaction zone
    void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit with " + other.name);
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            Debug.Log("Player left the NPC");
        }
    }

    // Assign default fonts if missing
    void AssignFonts()
    {
        Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        if (dialogueText != null && dialogueText.font == null)
        {
            dialogueText.font = defaultFont;
        }

        if (playerInputField != null)
        {
            if (playerInputField.textComponent != null && playerInputField.textComponent.font == null)
            {
                playerInputField.textComponent.font = defaultFont;
            }

            if (playerInputField.placeholder != null)
            {
                Text placeholderText = playerInputField.placeholder as Text;
                if (placeholderText != null && placeholderText.font == null)
                {
                    placeholderText.font = defaultFont;
                }
            }
        }
    }
}
