NPC Dialogue Tool - Ollama Integration
Overview
This Unity-based tool allows you to create NPCs (Non-Playable Characters) that can interact with players through dialogue. The dialogue is generated by integrating with the Ollama API using language models like Llama3. The tool dynamically generates conversations based on the player's input and the NPC's personality.

Features
Dynamic Dialogue: NPCs can generate dynamic responses based on the player's input and NPC configurations using the Ollama language model.
Customizable NPCs: Define NPC names, personalities, and backgrounds through the NPCSetupTool.
UI Integration: Includes a player input panel, dialogue display, and a send button for interaction.
Ollama Integration: Sends player input to the Ollama server, receives a generated response, and displays it in the Unity UI.
Requirements
Unity Version: 2020 or later
Ollama Server: Running locally on http://localhost:11434
Ollama Model: Configured to use the llama3 model or your custom model
Installation and Setup
1. Clone the Project
Clone this repository to your Unity project folder.

bash
Copy code
git clone <repository_url>
2. Set Up the Ollama API
Make sure the Ollama server is running locally. The server should be accessible at http://localhost:11434/api/generate.

3. Create an NPC
Open the NPC Setup Tool:

In Unity, navigate to Tools > NPC Setup Tool.
Configure the NPC:

Assign the 3D model for your NPC in the Model FBX field.
Enter the NPC's name, personality, and background in the provided fields.
Set Default Fonts (Optional):

If you want a specific font for the dialogue text, assign it in the Default Font field.
Generate the NPC:

Click the Setup NPC button. This will create the NPC in the scene with the required components: PicoDialogue, Collider, Rigidbody, and the dialogue UI.
4. Testing NPC Interaction
Approach the NPC:

The player can approach the NPC and press E to open the dialogue panel.
Send a Message:

Type a message in the input field and click Send.
NPC Response:

The tool will send the message to the Ollama server, generate a response, and display it on the dialogue panel.
Scripts
PicoDialogue.cs
Handles NPC interactions, player input, and communication with the Ollama server.
Sends requests with player input to the Ollama API, receives the generated response, and displays it in the UI.
Automatically assigns default fonts to the UI components if none are set.
NPCSetupTool.cs
Provides an Editor window to set up NPCs in the Unity scene.
Assigns NPC configurations like name, personality, and background.
Automatically adds the required components to the NPC GameObject, including a PicoDialogue script and UI elements.
llama_query.py
A Python script for querying the Ollama API (used as a reference for integrating the API with Unity).
Supports querying Llama3 and other models like GPT-3 or GPT-4.
Usage Example
Configure an NPC:

Name: PICO
Personality: An adventurous and curious AI
Background: A robotic companion exploring the world
Interact with the NPC:

Approach the NPC and type a message like: "How are you?"
The NPC will respond based on its personality and background using the Llama model.
Troubleshooting
Common Issues:
400 Bad Request:

Ensure the correct API URL (http://localhost:11434/api/generate) is set in PicoDialogue.cs.
Verify the JSON structure matches what the Ollama API expects.
NullReferenceException:

Ensure all UI components (InputField, Text, Placeholder) are assigned in the Inspector or properly initialized in the NPC setup script.
No Connection to Ollama:

Verify that the Ollama server is running and accessible on localhost:11434.
Use a browser or curl to confirm that the Ollama server is working:
bash
Copy code
curl -X POST http://localhost:11434/api/generate -d '{"model": "llama3", "prompt": "Hello"}'
Model Not Found:

Ensure that the model used (llama3, or your custom model) is available on the Ollama server.
Logs:
Use Unity's Console to check for errors during runtime.
Log the response from Ollama using the www.downloadHandler.text in PicoDialogue.cs to debug API issues.
Contributing
Feel free to contribute by submitting pull requests or reporting issues in the repository.
