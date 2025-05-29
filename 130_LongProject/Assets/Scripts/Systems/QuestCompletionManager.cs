using UnityEngine;

/// <summary>
/// Handles automatic quest completion based on various triggers
/// This replaces the missing QuestTrigger and QuestTaskTarget functionality
/// </summary>
public class QuestCompletionManager : MonoBehaviour
{
    private QuestManager questManager;
    private InventorySystem inventorySystem;
    
    void Start()
    {
        // Find required components
        questManager = FindObjectOfType<QuestManager>();
        inventorySystem = FindObjectOfType<InventorySystem>();
        
        if (questManager == null)
        {
            Debug.LogError("[QuestCompletionManager] No QuestManager found!");
            return;
        }
        
        if (inventorySystem == null)
        {
            Debug.LogError("[QuestCompletionManager] No InventorySystem found!");
            return;
        }
        
        // Subscribe to inventory events
        inventorySystem.OnItemAdded.AddListener(OnItemAdded);
        
        Debug.Log("[QuestCompletionManager] Initialized and listening for quest completion triggers");
    }
    
    /// <summary>
    /// Called when an item is added to inventory - check if it completes any quests
    /// </summary>
    private void OnItemAdded(ItemData item)
    {
        if (item == null || string.IsNullOrEmpty(item.associatedQuestName)) return;
        
        Debug.Log($"[QuestCompletionManager] Item {item.itemName} added with quest association: {item.associatedQuestName}");
        
        // Check if this item completes its associated quest
        CheckItemQuestCompletion(item.associatedQuestName);
    }
    
    /// <summary>
    /// Check if collecting this quest item should complete the quest
    /// </summary>
    private void CheckItemQuestCompletion(string questName)
    {
        // Mark the quest task as complete
        questManager.MarkTaskAsComplete(questName);
        
        // Show completion message
        DialogueManager.ShowItemMessage($"Quest item collected! Speak to the quest giver to complete the quest.");
        
        Debug.Log($"[QuestCompletionManager] Marked quest '{questName}' task as complete due to item collection");
    }
    
    /// <summary>
    /// Public method for NPCs to trigger quest completion when they receive items
    /// </summary>
    public static void TriggerQuestCompletionForItemExchange(string questName, ItemData item)
    {
        QuestCompletionManager instance = FindObjectOfType<QuestCompletionManager>();
        if (instance != null && instance.questManager != null)
        {
            instance.questManager.MarkTaskAsComplete(questName);
            Debug.Log($"[QuestCompletionManager] Marked quest '{questName}' as complete due to item exchange with NPC");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (inventorySystem != null)
        {
            inventorySystem.OnItemAdded.RemoveListener(OnItemAdded);
        }
    }
} 