using UnityEngine;

[CreateAssetMenu(menuName = "Quest System/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;

    public int requiredDay;
    public string requiredTimeZone;

    public bool autoStart = false;

    [Header("Reputation")]
    public int requiredReputation = 0; // Minimum reputation required to access this quest
    public int repRewardOnTime = 10;
    public int repRewardLate = 5;

    [Header("Item Requirements")]
    public ItemData[] requiredItems; // Changed to support multiple items
    public bool consumeItems = true;

    [Header("Rewards")]
    public ItemData[] rewardItems; // Changed to support multiple reward items
    
    [Header("Quest Status Messages")]
    public string needItemsMessage = "You need the required items to complete this quest.";
    public string hasItemsMessage = "You have all the required items! I can complete this quest.";
    public string completionMessage = "Thank you for completing this quest!";
}