using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class QuestManager : MonoBehaviour
{
    public ReputationManager repManager;
    public DayNightCycleManager timeManager;
    private List<QuestInstance> allQuests = new List<QuestInstance>();

    void Start()
    {
        if (timeManager == null)
            timeManager = FindObjectOfType<DayNightCycleManager>();
            
        if (timeManager == null)
        {
            Debug.LogError("[QuestManager] No DayNightCycleManager found in scene!");
            return;
        }

        if (repManager == null)
            repManager = FindObjectOfType<ReputationManager>();

        timeManager.OnMorning.AddListener(CheckQuests);
        timeManager.OnAfternoon.AddListener(CheckQuests);
        timeManager.OnEvening.AddListener(CheckQuests);
        timeManager.OnNight.AddListener(CheckQuests);
        
        CheckQuests();
    }

    public void RegisterQuest(QuestInstance quest)
    {
        if (quest == null)
        {
            Debug.LogError("[QuestManager] Attempted to register null quest!");
            return;
        }
        
        allQuests.Add(quest);
        
        if (timeManager != null)
        {
            quest.TryActivate(GetCurrentDay(), GetCurrentTimeZone());
        }
        else
        {
            Debug.LogWarning($"[QuestManager] TimeManager not initialized, quest {quest.data.questName} will be checked on next time event");
        }
    }

    public void CheckQuests()
    {
        if (timeManager == null)
        {
            Debug.LogWarning("[QuestManager] Cannot check quests - TimeManager is null");
            return;
        }
        
        int day = GetCurrentDay();
        string zone = GetCurrentTimeZone();

        foreach (var quest in allQuests)
            quest.TryActivate(day, zone);
    }

    public int GetCurrentDay() => timeManager != null ? timeManager.GetCurrentDay() : 1;
    public string GetCurrentTimeZone() => timeManager != null ? timeManager.GetCurrentTimeZone() : "Morning";

    public void MarkTaskAsComplete(string questName)
    {
        foreach (QuestInstance quest in allQuests)
        {
            if (quest.data.questName == questName && quest.state == QuestState.AvailableActive)
            {
                quest.MarkTaskAsComplete();
                return;
            }
        }
    }
    
    // Enhanced method to check multiple required items
    public QuestItemCheckResult CheckQuestItemRequirements(string questName, InventorySystem inventory)
    {
        foreach (QuestInstance quest in allQuests)
        {
            if (quest.data.questName == questName && quest.state == QuestState.AvailableActive)
            {
                return CheckQuestItemRequirements(quest.data, inventory);
            }
        }
        return new QuestItemCheckResult { hasAllItems = false, message = "Quest not found or not active." };
    }
    
    // Check items for a specific quest data
    public QuestItemCheckResult CheckQuestItemRequirements(QuestData questData, InventorySystem inventory)
    {
        if (questData.requiredItems == null || questData.requiredItems.Length == 0)
        {
            return new QuestItemCheckResult { hasAllItems = true, message = questData.hasItemsMessage };
        }
        
        List<ItemData> missingItems = new List<ItemData>();
        List<ItemData> foundItems = new List<ItemData>();
        
        foreach (ItemData requiredItem in questData.requiredItems)
        {
            if (requiredItem != null)
            {
                if (inventory.HasItem(requiredItem))
                {
                    foundItems.Add(requiredItem);
                }
                else
                {
                    missingItems.Add(requiredItem);
                }
            }
        }
        
        if (missingItems.Count == 0)
        {
            // Player has all required items
            return new QuestItemCheckResult 
            { 
                hasAllItems = true, 
                message = questData.hasItemsMessage,
                foundItems = foundItems.ToArray()
            };
        }
        else
        {
            // Player is missing some items
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("You need the following items:");
            foreach (ItemData missingItem in missingItems)
            {
                messageBuilder.AppendLine($"- {missingItem.itemName}");
            }
            
            if (foundItems.Count > 0)
            {
                messageBuilder.AppendLine("\nYou have:");
                foreach (ItemData foundItem in foundItems)
                {
                    messageBuilder.AppendLine($"- {foundItem.itemName}");
                }
            }
            
            return new QuestItemCheckResult 
            { 
                hasAllItems = false, 
                message = messageBuilder.ToString(),
                missingItems = missingItems.ToArray(),
                foundItems = foundItems.ToArray()
            };
        }
    }
    
    // Consume required items for quest completion
    public bool ConsumeQuestItems(string questName, InventorySystem inventory)
    {
        foreach (QuestInstance quest in allQuests)
        {
            if (quest.data.questName == questName && quest.state == QuestState.AvailableActive)
            {
                return ConsumeQuestItems(quest.data, inventory);
            }
        }
        return false;
    }
    
    public bool ConsumeQuestItems(QuestData questData, InventorySystem inventory)
    {
        if (!questData.consumeItems || questData.requiredItems == null) return true;
        
        // First verify all items are present
        var checkResult = CheckQuestItemRequirements(questData, inventory);
        if (!checkResult.hasAllItems) return false;
        
        // Remove each required item
        foreach (ItemData requiredItem in questData.requiredItems)
        {
            if (requiredItem != null)
            {
                inventory.RemoveItem(requiredItem);
                Debug.Log($"[QuestManager] Consumed quest item: {requiredItem.itemName}");
            }
        }
        
        return true;
    }
    
    // Give quest rewards
    public void GiveQuestRewards(QuestData questData, InventorySystem inventory)
    {
        if (questData.rewardItems == null) return;
        
        foreach (ItemData rewardItem in questData.rewardItems)
        {
            if (rewardItem != null && inventory.HasSpace())
            {
                inventory.AddItem(rewardItem);
                Debug.Log($"[QuestManager] Gave quest reward: {rewardItem.itemName}");
            }
        }
    }
    
    public List<QuestInstance> GetAllQuests()
    {
        return allQuests;
    }
}

// Data structure for quest item check results
[System.Serializable]
public class QuestItemCheckResult
{
    public bool hasAllItems;
    public string message;
    public ItemData[] missingItems;
    public ItemData[] foundItems;
}