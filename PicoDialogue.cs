using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class OllamaRequest
{
    public string model;
    public string prompt;
    public int max_tokens;
    public float temperature;
}

[Serializable]
public class OllamaResponse
{
    public string RESPONSE;
    public bool DONE;
}

public class PicoDialogue : MonoBehaviour
{
    // URL for the Ollama server
    private string ollamaURL = "http://localhost:11434/api/generate";

    // The model to use
    [Tooltip("Ensure the model name matches exactly with the model available on Ollama.")]
    public string model = "llama3.2:1b"; // Replace with your actual model name

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
        Debug.Log("PicoDialogue Start()");
        Debug.Log("Dialogue Canvas Assigned: " + (dialogueCanvas != null));
        Debug.Log("Dialogue Text Assigned: " + (dialogueText != null));
        Debug.Log("Player Input Panel Assigned: " + (playerInputPanel != null));
        Debug.Log("Player Input Field Assigned: " + (playerInputField != null));
        Debug.Log("Send Button Assigned: " + (sendButton != null));

        // Hide the dialogue canvas at the start
        if (dialogueCanvas != null)
        {
            dialogueCanvas.enabled = false;
        }
        else
        {
            Debug.LogError("Dialogue Canvas is not assigned in the Inspector.");
        }

        // Hide the player input panel at the start
        if (playerInputPanel != null)
        {
            playerInputPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Player Input Panel is not assigned in the Inspector.");
        }

        // Add listener to the send button
        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }
        else
        {
            Debug.LogError("Send Button is not assigned in the Inspector.");
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
        Debug.Log("OpenPlayerInputUI called");

        if (dialogueCanvas != null)
        {
            dialogueCanvas.enabled = true; // Make sure the canvas is active

            if (playerInputPanel != null)
            {
                playerInputPanel.SetActive(true);
                if (playerInputField != null)
                {
                    playerInputField.text = "";  // Clear the input field
                    playerInputField.ActivateInputField();
                }
                else
                {
                    Debug.LogError("Player Input Field is not assigned.");
                }
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
        if (playerInputField != null)
        {
            string playerInput = playerInputField.text;
            playerInputPanel.SetActive(false);

            // Start the coroutine to get the response
            StartCoroutine(GetLlamaResponse(playerInput));
        }
        else
        {
            Debug.LogError("Player Input Field is not assigned.");
        }
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

        // Create the request object
        OllamaRequest request = new OllamaRequest
        {
            model = model,
            prompt = prompt,
            max_tokens = 150,       // Ensure this matches Ollama's API requirements
            temperature = 0.7f
        };

        // Serialize the request to JSON
        string jsonData = JsonUtility.ToJson(request);
        Debug.Log("JSON Data Sent to Ollama: " + jsonData);

        // Convert JSON data to bytes
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        // Create a new UnityWebRequest for the POST request
        UnityWebRequest www = new UnityWebRequest(ollamaURL, "POST");
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for a response
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error communicating with Ollama: " + www.error);
            Debug.Log("Server Response: " + www.downloadHandler.text);
            ShowDialogue("Sorry, I'm having trouble communicating right now.");
            yield break;
        }

        // Initialize the final response string
        string finalResponse = "";

        // Get the complete response text
        string response = www.downloadHandler.text;

        // Split the response into individual JSON objects
        string[] responseChunks = response.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

        Debug.Log("Total JSON Chunks Received: " + responseChunks.Length);

        foreach (string chunk in responseChunks)
        {
            if (!string.IsNullOrEmpty(chunk.Trim()))
            {
                try
                {
                    // Parse the JSON response
                    ResponseData parsedResponse = JsonUtility.FromJson<ResponseData>(chunk);

                    if (parsedResponse != null)
                    {
                        Debug.Log($"Parsed Response: {parsedResponse.response}, Done: {parsedResponse.done}");
                        finalResponse += parsedResponse.response;

                        // If "done": true, exit the loop
                        if (parsedResponse.done)
                        {
                            Debug.Log("Received DONE signal from Ollama.");
                            break;
                        }
                    }
                    else
                    {
                        Debug.LogError("ParsedResponse is null.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing JSON chunk: " + ex.Message);
                }
            }
        }

        // Display the final accumulated response
        ShowDialogue(finalResponse);
    }


    // Class to handle the response data
    [Serializable]
    public class OllamaResponse
    {
        public string RESPONSE;
        public bool DONE;
    }
    public class ResponseData
    {
        public string response; // Must match "response" in JSON
        public bool done;        // Must match "done" in JSON
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
        else
        {
            Debug.LogError("Dialogue Canvas or Dialogue Text is not assigned.");
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
            Debug.Log("Assigned default font to Dialogue Text.");
        }

        if (playerInputField != null)
        {
            if (playerInputField.textComponent != null && playerInputField.textComponent.font == null)
            {
                playerInputField.textComponent.font = defaultFont;
                Debug.Log("Assigned default font to Player Input Field Text.");
            }

            if (playerInputField.placeholder != null)
            {
                Text placeholderText = playerInputField.placeholder as Text;
                if (placeholderText != null && placeholderText.font == null)
                {
                    placeholderText.font = defaultFont;
                    Debug.Log("Assigned default font to Player Input Field Placeholder.");
                }
            }
        }
    }
}
