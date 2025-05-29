using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxSlots = 5;
    
    [Header("Weight System")]
    public float maxWeightBeforePenalty = 10f;
    public float maxMovementPenalty = 0.5f; // Maximum 50% movement reduction
    
    [Header("Events")]
    public UnityEvent<ItemData> OnItemAdded;
    public UnityEvent<ItemData> OnItemRemoved;
    public UnityEvent<float> OnInventoryWeightChanged;
    public UnityEvent OnInventoryChanged; // New event for any inventory change
    
    // The actual inventory slots
    private ItemData[] inventorySlots;
    
    // Track total weight
    private float currentTotalWeight = 0f;
    
    private void Awake()
    {
        inventorySlots = new ItemData[maxSlots];
        Debug.Log($"[InventorySystem] Initialized with {maxSlots} slots");
    }
    
    private void Start()
    {
        // Notify initial weight
        OnInventoryWeightChanged?.Invoke(currentTotalWeight);
        OnInventoryChanged?.Invoke(); // Trigger initial UI update
        Debug.Log($"[InventorySystem] Initial weight: {currentTotalWeight}");
    }
    
    // Add an item to inventory
    public bool AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("[InventorySystem] Attempted to add NULL item");
            return false;
        }
        
        Debug.Log($"[InventorySystem] Attempting to add item: {item.itemName}");
        
        // Find first empty slot
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
            {
                inventorySlots[i] = item;
                
                // Update weight
                currentTotalWeight += item.weight;
                OnInventoryWeightChanged?.Invoke(currentTotalWeight);
                
                // Notify listeners
                OnItemAdded?.Invoke(item);
                OnInventoryChanged?.Invoke(); // Trigger UI update
                
                Debug.Log($"[InventorySystem] Added item: {item.itemName} to slot {i} (Weight: {item.weight}, Total: {currentTotalWeight})");
                return true;
            }
        }
        
        // Inventory full
        Debug.Log($"[InventorySystem] Inventory full, cannot add item: {item.itemName}");
        return false;
    }
    
    // Remove a specific item
    public bool RemoveItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("[InventorySystem] Attempted to remove NULL item");
            return false;
        }
        
        Debug.Log($"[InventorySystem] Attempting to remove item: {item.itemName}");
        
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == item)
            {
                // Update weight
                currentTotalWeight -= item.weight;
                OnInventoryWeightChanged?.Invoke(currentTotalWeight);
                
                // Notify listeners
                OnItemRemoved?.Invoke(item);
                
                // Clear slot
                inventorySlots[i] = null;
                
                OnInventoryChanged?.Invoke(); // Trigger UI update
                
                Debug.Log($"[InventorySystem] Removed item: {item.itemName} from slot {i} (New weight total: {currentTotalWeight})");
                return true;
            }
        }
        
        Debug.Log($"[InventorySystem] Item not found in inventory: {item.itemName}");
        return false;
    }
    
    // Remove an item from a specific slot
    public ItemData RemoveItemAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
        {
            Debug.LogError($"[InventorySystem] Invalid slot index: {slotIndex}");
            return null;
        }
            
        ItemData itemToRemove = inventorySlots[slotIndex];
        
        if (itemToRemove != null)
        {
            // Update weight
            currentTotalWeight -= itemToRemove.weight;
            OnInventoryWeightChanged?.Invoke(currentTotalWeight);
            
            // Notify listeners
            OnItemRemoved?.Invoke(itemToRemove);
            
            // Clear slot
            inventorySlots[slotIndex] = null;
            
            OnInventoryChanged?.Invoke(); // Trigger UI update
            
            Debug.Log($"[InventorySystem] Removed item from slot {slotIndex}: {itemToRemove.itemName} (New weight total: {currentTotalWeight})");
        }
        else
        {
            Debug.Log($"[InventorySystem] No item in slot {slotIndex} to remove");
        }
        
        return itemToRemove;
    }
    
    // Get item in a specific slot
    public ItemData GetItemAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Length)
        {
            Debug.LogError($"[InventorySystem] Invalid slot index for GetItemAt: {slotIndex}");
            return null;
        }
            
        return inventorySlots[slotIndex];
    }
    
    // Get all items (for UI display)
    public ItemData[] GetAllItems()
    {
        return inventorySlots;
    }
    
    // Get first item matching a quest name
    public ItemData GetQuestItem(string questName)
    {
        if (string.IsNullOrEmpty(questName))
        {
            Debug.LogWarning("[InventorySystem] Searching for item with empty quest name");
            return null;
        }
        
        foreach (var item in inventorySlots)
        {
            if (item != null && item.associatedQuestName == questName)
            {
                Debug.Log($"[InventorySystem] Found quest item for quest: {questName} - {item.itemName}");
                return item;
            }
        }
        Debug.Log($"[InventorySystem] No quest item found for quest: {questName}");
        return null;
    }
    
    // Get all items matching specific quest items
    public List<ItemData> GetQuestItems(ItemData[] requiredItems)
    {
        List<ItemData> foundItems = new List<ItemData>();
        
        if (requiredItems == null) return foundItems;
        
        foreach (ItemData requiredItem in requiredItems)
        {
            if (requiredItem != null && HasItem(requiredItem))
            {
                foundItems.Add(requiredItem);
            }
        }
        
        return foundItems;
    }
    
    // Check if inventory has all required quest items
    public bool HasAllQuestItems(ItemData[] requiredItems)
    {
        if (requiredItems == null || requiredItems.Length == 0) return true;
        
        foreach (ItemData requiredItem in requiredItems)
        {
            if (requiredItem != null && !HasItem(requiredItem))
            {
                return false;
            }
        }
        
        return true;
    }
    
    // Calculate movement speed multiplier based on current weight
    public float GetWeightSpeedMultiplier()
    {
        if (currentTotalWeight <= maxWeightBeforePenalty)
        {
            Debug.Log($"[InventorySystem] Weight ({currentTotalWeight}) under limit, no speed penalty");
            return 1f;
        }
        
        // Calculate penalty (1.0 to 0.5 based on weight)
        float overweightRatio = Mathf.Clamp01((currentTotalWeight - maxWeightBeforePenalty) / maxWeightBeforePenalty);
        float multiplier = 1f - (overweightRatio * maxMovementPenalty);
        Debug.Log($"[InventorySystem] Weight penalty applied: {currentTotalWeight}kg gives {multiplier:F2}x speed");
        return multiplier;
    }
    
    // Check if inventory has space
    public bool HasSpace()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
            {
                return true;
            }
        }
        Debug.Log("[InventorySystem] Inventory is full");
        return false;
    }
    
    // Check if inventory contains an item
    public bool HasItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogError("[InventorySystem] Checking for NULL item");
            return false;
        }
        
        foreach (var slot in inventorySlots)
        {
            if (slot == item)
            {
                Debug.Log($"[InventorySystem] Item found in inventory: {item.itemName}");
                return true;
            }
        }
        Debug.Log($"[InventorySystem] Item not found in inventory: {item.itemName}");
        return false;
    }
    
    // Get count of empty slots
    public int GetEmptySlotCount()
    {
        return inventorySlots.Count(slot => slot == null);
    }
    
    // Get current total weight
    public float GetCurrentWeight()
    {
        return currentTotalWeight;
    }
    
    // Force UI update (useful for ensuring consistency)
    public void ForceUIUpdate()
    {
        OnInventoryChanged?.Invoke();
        OnInventoryWeightChanged?.Invoke(currentTotalWeight);
        Debug.Log("[InventorySystem] Forced UI update");
    }
    
    // Debug function to print inventory contents
    public void DebugPrintInventory()
    {
        string contents = "[InventorySystem] Current inventory contents:";
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
                contents += $"\n  Slot {i}: {inventorySlots[i].itemName} (Weight: {inventorySlots[i].weight})";
            else
                contents += $"\n  Slot {i}: Empty";
        }
        contents += $"\n  Total Weight: {currentTotalWeight}";
        Debug.Log(contents);
    }
} 