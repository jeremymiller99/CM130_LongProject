using UnityEngine;

[CreateAssetMenu(menuName = "NPC System/NPC Profile")]
public class NPCProfile : ScriptableObject
{
    [Header("Basic Info")]
    public string npcName;
    public Sprite portrait; // Character portrait for dialogue UI
    
    [Header("Dialogue Categories")]
    [TextArea] public string greetingDialogue = "Hello there!";
    
    [Header("Quest Dialogue")]
    [TextArea] public string questDialogue = "I have something important for you to do.";
    [TextArea] public string questAcceptDialogue = "Thank you for accepting this task!";
    [TextArea] public string questCompletedDialogue = "Thank you for completing my quest!";
    [TextArea] public string noQuestDialogue = "I don't have any tasks for you right now.";
    [TextArea] public string insufficientRepDialogue = "I don't know you well enough to trust you with this task.";
    [TextArea] public string questInProgressDialogue = "Have you completed the task I gave you?";
    
    [Header("Lore & Information")]
    [TextArea] public string loreDialogue = "Let me tell you about this place...";
    [TextArea] public string noLoreDialogue = "I don't have much to say about that.";
    
    [Header("Trade Dialogue")]
    [TextArea] public string tradeDialogue = "Would you like to buy or sell something?";
    [TextArea] public string noTradeDialogue = "I'm not trading anything right now.";
    
    [Header("General Conversation")]
    [TextArea] public string generalDialogue = "How can I help you?";

    [Header("Reputation Reactions")]
    public string lowRepDialogue;
    public string highRepDialogue;
    public int reputationThreshold = 25;

    [Header("Quest Link")]
    public QuestData assignedQuest;
    
    [Header("Dialogue Options Configuration")]
    public bool hasQuests = true;
    public bool hasLore = true;
    public bool hasTrade = false;
    public bool hasGeneralChat = true;
}