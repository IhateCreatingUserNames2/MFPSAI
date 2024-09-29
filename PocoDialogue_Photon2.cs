using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Photon.Chat;
using Photon.Pun;
using ExitGames.Client.Photon;  // Required for Photon Chat

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

public class PicoDialogue : MonoBehaviour, IChatClientListener
{
    private string ollamaURL = "http://localhost:11434/api/generate";
    public string model = "llama3.2:1b";  // Replace with your actual model name

    // UI elements to display dialogue
    public Canvas dialogueCanvas;
    public Text dialogueText;
    public GameObject playerInputPanel;
    public InputField playerInputField;
    public Button sendButton;

    // Flag to check if the player is nearby
    private bool isPlayerNear = false;

    // NPC configurations
    public string npcName = "PICO";
    public string npcPersonality = "An adventurous and curious AI.";
    public string npcBackground = "PICO is a robotic companion exploring the world.";

    // Chat client for Photon
    private ChatClient chatClient;
    private string userId = "NPC_PICO";  // Unique ID for the NPC in the chat

    private string[] channelsToSubscribe = new string[] { "ALL", "TEAM" };

    void Start()
    {
        // Initialize Photon Chat Client
        ChatAppSettings chatAppSettings = new ChatAppSettings
        {
            AppIdChat = "Photon Chat ID",  // Use your real AppIdChat
            AppVersion = "1.0",         // Version of the app
            FixedRegion = "us",         // Adjust this to your region (e.g., us, eu)
            Protocol = ConnectionProtocol.Udp  // Use UDP or TCP based on your network
        };

        // Use AuthenticationValues from Photon.Chat
        Photon.Chat.AuthenticationValues authValues = new Photon.Chat.AuthenticationValues(userId);

        // Initialize and connect the ChatClient
        chatClient = new ChatClient(this);
        bool isConnected = chatClient.Connect(chatAppSettings.AppIdChat, chatAppSettings.AppVersion, authValues);

        // Log the connection attempt
        Debug.Log("Connecting to Photon Chat: " + isConnected);

        // Hide the dialogue canvas and player input panel at the start
        if (dialogueCanvas != null)
        {
            dialogueCanvas.enabled = false;
        }

        if (playerInputPanel != null)
        {
            playerInputPanel.SetActive(false);
        }

        // Add listener to the send button
        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }

        // Ensure fonts are assigned
        AssignFonts();

        Debug.Log("PicoDialogue system initialized.");
    }

    void Update()
    {
        // Detect interaction when the player presses "E" and is nearby
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed, player is near the NPC.");
            OpenPlayerInputUI();
        }

        // Continuously service Photon Chat client
        if (chatClient != null)
        {
            chatClient.Service();  // This keeps the connection alive and services the chat
        }
    }

    // Open player input UI when interacting with the NPC
    void OpenPlayerInputUI()
    {
        Debug.Log("Opening Player Input UI...");

        if (dialogueCanvas != null)
        {
            dialogueCanvas.enabled = true;

            if (playerInputPanel != null)
            {
                playerInputPanel.SetActive(true);
                playerInputField.text = "";
                playerInputField.ActivateInputField();
            }
            else
            {
                Debug.LogError("Player Input Panel is not assigned in the inspector.");
            }
        }
        else
        {
            Debug.LogError("Dialogue Canvas is not assigned in the inspector.");
        }
    }

    // Callback when the player sends a message via the input panel
    public void OnSendButtonClicked()
    {
        if (playerInputField != null)
        {
            string playerInput = playerInputField.text;
            playerInputPanel.SetActive(false);
            StartCoroutine(GetLlamaResponse(playerInput));  // Fetches response from Ollama
        }
        else
        {
            Debug.LogError("Player Input Field is not assigned.");
        }
    }

    // Coroutine to send player input to Ollama and get a response
    IEnumerator GetLlamaResponse(string playerInput)
    {
        Debug.Log("Sending input to Ollama: " + playerInput);

        // Create prompt based on player input and NPC configurations
        string prompt = $"The following is a conversation between a player and an NPC.\n" +
                        $"NPC Name: {npcName}\n" +
                        $"Personality: {npcPersonality}\n" +
                        $"Background: {npcBackground}\n\n" +
                        $"Player: {playerInput}\n" +
                        $"{npcName}:";

        // Create request for Ollama
        OllamaRequest request = new OllamaRequest
        {
            model = model,
            prompt = prompt,
            max_tokens = 150,
            temperature = 0.7f
        };

        string jsonData = JsonUtility.ToJson(request);
        byte[] postData = Encoding.UTF8.GetBytes(jsonData);

        // Send POST request to Ollama
        UnityWebRequest www = new UnityWebRequest(ollamaURL, "POST");
        www.uploadHandler = new UploadHandlerRaw(postData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        // Wait for Ollama's response
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            ShowDialogue("Sorry, I'm having trouble communicating right now.");
            Debug.LogError("Error communicating with Ollama: " + www.error);
            yield break;
        }

        // Process Ollama response
        string response = www.downloadHandler.text;
        string finalResponse = "";

        string[] responseChunks = response.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string chunk in responseChunks)
        {
            if (!string.IsNullOrEmpty(chunk.Trim()))
            {
                try
                {
                    ResponseData parsedResponse = JsonUtility.FromJson<ResponseData>(chunk);
                    finalResponse += parsedResponse.response;

                    if (parsedResponse.done)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing JSON chunk: " + ex.Message);
                }
            }
        }

        // Show the final response in the dialogue UI and in chat
        ShowDialogue(finalResponse);
        SendMessageToChat(finalResponse);
    }

    // Function to send the response to the chat system
    // Function to send the response to the chat system
    // Function to send the response to the in-game chat system (All chat only)
    // Function to send the response to the in-game chat system (All chat only)
    // Function to send the response to the in-game chat system (All chat only)
    void SendMessageToChat(string message)
    {
        if (bl_RoomChatBase.Instance != null)
        {
            bl_RoomChat roomChat = bl_RoomChatBase.Instance as bl_RoomChat;

            if (roomChat != null)
            {
                // Check if the message sender is not the local player to avoid repetition
                if (userId != PhotonNetwork.LocalPlayer.UserId)
                {
                    // Use the room chat's method to send the message to the All channel
                    roomChat.SetChatLocally($"[ALL] {npcName}: {message}");   // Locally show the message
                    roomChat.SetChat($"[ALL] {npcName}: {message}");           // Send the message to the network
                }
                else
                {
                    Debug.Log("Skipping message re-send to avoid player repetition.");
                }
            }
            else
            {
                Debug.LogError("Room chat instance is null.");
            }
        }
        else
        {
            Debug.LogError("Chat system is not initialized.");
        }
    }






    // Function to display the dialogue above NPC
    void ShowDialogue(string message)
    {
        if (dialogueCanvas != null && dialogueText != null)
        {
            dialogueText.text = message;
            dialogueCanvas.enabled = true;
            Invoke("HideDialogue", 5f);  // Hide dialogue after 5 seconds
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

    // Photon Chat Callbacks
    public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
    {
        Debug.Log($"Photon Debug: {level} - {message}");
    }

    public void OnDisconnected()
    {
        Debug.Log("Chat Disconnected.");
        StartCoroutine(ReconnectToChat());
    }

    IEnumerator ReconnectToChat()
    {
        int attempt = 1;
        while (!chatClient.CanChat)
        {
            yield return new WaitForSeconds(2f);  // Delay before retrying connection

            Debug.Log($"Reconnecting to chat. Attempt {attempt}...");
            chatClient.Connect("your-app-id", "1.0", new Photon.Chat.AuthenticationValues(userId));

            attempt++;
            if (attempt > 5)
            {
                Debug.LogError("Failed to reconnect to chat after 5 attempts.");
                yield break;
            }
        }
    }

    public void OnConnected()
    {
        Debug.Log("Chat Connected.");
        chatClient.Subscribe(channelsToSubscribe);  // Subscribe to necessary channels
    }

    public void OnChatStateChange(ChatState state) { }
    public void OnGetMessages(string channelName, string[] senders, object[] messages) { }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("Subscribed to channels: " + string.Join(", ", channels));
    }
    public void OnUnsubscribed(string[] channels) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnUserSubscribed(string channel, string user) { }
    public void OnUserUnsubscribed(string channel, string user) { }

    // Assign default fonts if missing
    void AssignFonts()
    {
        Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        if (dialogueText != null && dialogueText.font == null)
        {
            dialogueText.font = defaultFont;
        }

        if (playerInputField != null && playerInputField.textComponent != null && playerInputField.textComponent.font == null)
        {
            playerInputField.textComponent.font = defaultFont;
        }
    }

    // Response Data Class for Ollama
    [Serializable]
    public class ResponseData
    {
        public string response;
        public bool done;
    }

    // Handle the player entering the NPC interaction zone
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered interaction zone.");
            isPlayerNear = true;
        }
    }

    // Handle the player leaving the NPC interaction zone
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left interaction zone.");
            isPlayerNear = false;
        }
    }
}
