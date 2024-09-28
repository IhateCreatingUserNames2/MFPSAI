using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class NPCSetupTool : EditorWindow
{
    private GameObject modelFBX;
    private Font defaultFont;
    private string npcName = "PICO";
    private string npcPersonality = "An adventurous and curious AI.";
    private string npcBackground = "PICO is a robotic companion exploring the world.";

    [MenuItem("Tools/NPC Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<NPCSetupTool>("NPC Setup Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("NPC Setup Tool", EditorStyles.boldLabel);

        modelFBX = (GameObject)EditorGUILayout.ObjectField("Model FBX", modelFBX, typeof(GameObject), false);
        defaultFont = (Font)EditorGUILayout.ObjectField("Default Font", defaultFont, typeof(Font), false);

        GUILayout.Space(10);
        GUILayout.Label("NPC Configurations", EditorStyles.boldLabel);
        npcName = EditorGUILayout.TextField("NPC Name", npcName);
        npcPersonality = EditorGUILayout.TextField("Personality", npcPersonality);
        npcBackground = EditorGUILayout.TextField("Background", npcBackground);

        GUILayout.Space(20);

        if (GUILayout.Button("Setup NPC"))
        {
            if (modelFBX == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign the Model FBX.", "OK");
                return;
            }

            SetupNPC();
        }
    }

    void SetupNPC()
    {
        // Instantiate the model
        GameObject npc = Instantiate(modelFBX);
        npc.name = npcName;

        // Add necessary components
        if (npc.GetComponent<PicoDialogue>() == null)
        {
            PicoDialogue picoDialogue = npc.AddComponent<PicoDialogue>();

            // Assign NPC configurations
            picoDialogue.npcName = npcName;
            picoDialogue.npcPersonality = npcPersonality;
            picoDialogue.npcBackground = npcBackground;
        }

        // Ensure the model has a Collider
        if (npc.GetComponent<Collider>() == null)
        {
            Collider collider = npc.AddComponent<CapsuleCollider>();
            collider.isTrigger = true;
        }

        // Ensure the model has a Rigidbody (required for trigger events)
        if (npc.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = npc.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        // Set up the Dialogue Canvas at the root level
        GameObject dialogueCanvas = CreateDialogueCanvas();

        // Set up the Player Input Panel
        GameObject playerInputPanel = CreatePlayerInputPanel(dialogueCanvas.transform);

        // Assign UI elements to the PicoDialogue script
        PicoDialogue picoDialogueComponent = npc.GetComponent<PicoDialogue>();
        picoDialogueComponent.dialogueCanvas = dialogueCanvas.GetComponent<Canvas>();
        picoDialogueComponent.dialogueText = dialogueCanvas.transform.Find("DialogueText").GetComponent<Text>();
        picoDialogueComponent.playerInputPanel = playerInputPanel;
        picoDialogueComponent.playerInputField = playerInputPanel.transform.Find("PlayerInputField").GetComponent<InputField>();
        picoDialogueComponent.sendButton = playerInputPanel.transform.Find("SendButton").GetComponent<Button>();

        // Set default font if assigned
        if (defaultFont != null)
        {
            Text[] texts = dialogueCanvas.GetComponentsInChildren<Text>(true);
            foreach (Text txt in texts)
            {
                txt.font = defaultFont;
            }
        }

        // Assign the 'AiAgent' tag to the NPC
        npc.tag = "AiAgent";

        // Save the new NPC as a prefab
        string path = EditorUtility.SaveFilePanelInProject("Save NPC Prefab", npc.name + "_NPC.prefab", "prefab", "Please enter a file name to save the NPC prefab.");
        if (path.Length > 0)
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(npc, path, InteractionMode.UserAction);
            EditorUtility.DisplayDialog("Success", "NPC prefab created successfully at:\n" + path, "OK");
        }
        else
        {
            // If the user cancels saving, destroy the created NPC and UI
            DestroyImmediate(npc);
            DestroyImmediate(dialogueCanvas);
        }
    }

    GameObject CreateDialogueCanvas()
    {
        // Create a new Canvas at the root level
        GameObject canvasGO = new GameObject("DialogueCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Dialogue Text
        GameObject textGO = new GameObject("DialogueText");
        textGO.transform.SetParent(canvasGO.transform, false);
        Text dialogueText = textGO.AddComponent<Text>();
        dialogueText.text = "";
        dialogueText.alignment = TextAnchor.MiddleCenter;
        dialogueText.fontSize = 24;
        dialogueText.color = Color.white;

        RectTransform textRect = dialogueText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.8f);
        textRect.anchorMax = new Vector2(0.9f, 0.95f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Ensure a valid font is assigned
        if (defaultFont != null)
        {
            dialogueText.font = defaultFont;
        }
        else
        {
            dialogueText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return canvasGO;
    }

    GameObject CreatePlayerInputPanel(Transform parent)
    {
        // Create Player Input Panel
        GameObject panelGO = new GameObject("PlayerInputPanel");
        panelGO.transform.SetParent(parent, false);
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.3f, 0.05f);
        panelRect.anchorMax = new Vector2(0.7f, 0.2f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);

        // Create InputField
        GameObject inputFieldGO = new GameObject("PlayerInputField");
        inputFieldGO.transform.SetParent(panelGO.transform, false);
        RectTransform inputFieldRect = inputFieldGO.AddComponent<RectTransform>();
        inputFieldRect.anchorMin = new Vector2(0.05f, 0.2f);
        inputFieldRect.anchorMax = new Vector2(0.95f, 0.8f);
        inputFieldRect.offsetMin = Vector2.zero;
        inputFieldRect.offsetMax = Vector2.zero;
        InputField inputField = inputFieldGO.AddComponent<InputField>();
        Image inputFieldImage = inputFieldGO.AddComponent<Image>();
        inputFieldImage.color = Color.white;

        // Create Text component for InputField
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(inputFieldGO.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        Text inputText = textGO.AddComponent<Text>();
        inputText.text = "";
        inputText.alignment = TextAnchor.MiddleLeft;
        inputText.fontSize = 18;
        inputText.color = Color.black;

        // Assign Text component to InputField
        inputField.textComponent = inputText;

        // Ensure a valid font is assigned
        if (defaultFont != null)
        {
            inputText.font = defaultFont;
        }
        else
        {
            inputText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // Create Placeholder Text
        GameObject placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(inputFieldGO.transform, false);
        RectTransform placeholderRect = placeholderGO.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        Text placeholderText = placeholderGO.AddComponent<Text>();
        placeholderText.text = "Type your message...";
        placeholderText.alignment = TextAnchor.MiddleLeft;
        placeholderText.fontSize = 18;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1);

        // Assign Placeholder to InputField
        inputField.placeholder = placeholderText;

        // Ensure a valid font is assigned to Placeholder
        if (defaultFont != null)
        {
            placeholderText.font = defaultFont;
        }
        else
        {
            placeholderText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // Create Send Button
        GameObject buttonGO = new GameObject("SendButton");
        buttonGO.transform.SetParent(panelGO.transform, false);
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.35f, 0f);
        buttonRect.anchorMax = new Vector2(0.65f, 0.2f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        Button sendButton = buttonGO.AddComponent<Button>();
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.75f, 0.75f, 0.75f, 1f);

        // Create Text for Button
        GameObject buttonTextGO = new GameObject("Text");
        buttonTextGO.transform.SetParent(buttonGO.transform, false);
        RectTransform buttonTextRect = buttonTextGO.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        Text buttonText = buttonTextGO.AddComponent<Text>();
        buttonText.text = "Send";
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontSize = 18;
        buttonText.color = Color.black;

        // Assign targetGraphic and onClick event
        sendButton.targetGraphic = buttonImage;
        sendButton.onClick.RemoveAllListeners();
        // The actual function will be assigned in the PicoDialogue script

        // Ensure a valid font is assigned to Button Text
        if (defaultFont != null)
        {
            buttonText.font = defaultFont;
        }
        else
        {
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return panelGO;
    }
}
