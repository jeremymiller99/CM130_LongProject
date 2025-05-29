using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public InventorySystem playerInventory;
    public GameObject slotPrefab;
    public Transform slotContainer;
    
    [Header("UI Elements")]
    public TMP_Text weightText;
    public Image weightBar;
    
    private GameObject[] slotObjects;
    private Image[] slotImages; // Cache slot images for better performance
    
    void Start()
    {
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerController>().GetComponent<InventorySystem>();
            
        // Initialize inventory slots
        InitializeSlots();
        
        // Subscribe to inventory events - using the new comprehensive event
        playerInventory.OnInventoryChanged.AddListener(UpdateUI);
        playerInventory.OnInventoryWeightChanged.AddListener(UpdateWeightDisplay);
        
        // Also keep the individual events for specific updates if needed
        playerInventory.OnItemAdded.AddListener(OnItemAdded);
        playerInventory.OnItemRemoved.AddListener(OnItemRemoved);
        
        // Initial UI update
        UpdateUI();
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged.RemoveListener(UpdateUI);
            playerInventory.OnInventoryWeightChanged.RemoveListener(UpdateWeightDisplay);
            playerInventory.OnItemAdded.RemoveListener(OnItemAdded);
            playerInventory.OnItemRemoved.RemoveListener(OnItemRemoved);
        }
    }
    
    private void InitializeSlots()
    {
        if (slotPrefab == null || slotContainer == null)
        {
            Debug.LogError("[InventoryUI] Slot prefab or container is null!");
            return;
        }
            
        slotObjects = new GameObject[playerInventory.maxSlots];
        slotImages = new Image[playerInventory.maxSlots];
        
        // Clear existing slots
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);
        
        // Create new slots
        for (int i = 0; i < playerInventory.maxSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            slotObj.name = $"Slot_{i}";
            
            // Add slot number to know which key to press
            TMP_Text slotNumber = slotObj.GetComponentInChildren<TMP_Text>();
            if (slotNumber != null)
                slotNumber.text = (i + 1).ToString();
            
            // Cache the image component for this slot
            Image itemIcon = FindItemIcon(slotObj.transform);
            slotImages[i] = itemIcon;
            
            if (itemIcon == null)
            {
                Debug.LogError($"[InventoryUI] No Image component found in slot {i}! Make sure your slot prefab has an Image component for item icons.");
            }
                
            slotObjects[i] = slotObj;
        }
        
        Debug.Log($"[InventoryUI] Initialized {playerInventory.maxSlots} inventory slots");
    }
    
    private Image FindItemIcon(Transform slotTransform)
    {
        // Try to find the ItemIcon component more robustly
        Transform iconTransform = slotTransform.Find("ItemIcon");
        if (iconTransform != null)
        {
            return iconTransform.GetComponent<Image>();
        }
        
        // Try finding any Image component in children (excluding the slot background)
        Image[] images = slotTransform.GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            // Skip the main slot image (usually the background)
            if (img.transform != slotTransform)
            {
                return img;
            }
        }
        
        return null;
    }
    
    private void UpdateUI()
    {
        if (playerInventory == null || slotObjects == null) return;
        
        ItemData[] items = playerInventory.GetAllItems();
        
        // Update each slot with its corresponding item
        for (int i = 0; i < slotObjects.Length && i < items.Length; i++)
        {
            UpdateSlot(i, items[i]);
        }
        
        // Update weight display
        UpdateWeightDisplay(playerInventory.GetCurrentWeight());
        
        Debug.Log("[InventoryUI] Updated all inventory slots");
    }
    
    private void UpdateSlot(int slotIndex, ItemData item)
    {
        if (slotIndex < 0 || slotIndex >= slotImages.Length) return;
        
        Image itemIcon = slotImages[slotIndex];
        if (itemIcon == null) return;
        
        if (item != null)
        {
            if (item.icon != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.enabled = true;
                itemIcon.color = Color.white; // Ensure full opacity
                Debug.Log($"[InventoryUI] Updated slot {slotIndex} with item: {item.itemName}");
            }
            else
            {
                Debug.LogWarning($"[InventoryUI] Item {item.itemName} has no icon assigned!");
                itemIcon.enabled = false;
            }
            
            // Update tooltip if applicable
            UpdateTooltip(slotIndex, item);
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            UpdateTooltip(slotIndex, null);
            Debug.Log($"[InventoryUI] Cleared slot {slotIndex}");
        }
    }
    
    private void UpdateTooltip(int slotIndex, ItemData item)
    {
        if (slotIndex >= slotObjects.Length) return;
        
        Tooltip tooltip = slotObjects[slotIndex].GetComponent<Tooltip>();
        if (tooltip != null)
        {
            if (item != null)
            {
                tooltip.tooltipText = $"{item.itemName}\n{item.description}";
                tooltip.enabled = true;
            }
            else
            {
                tooltip.tooltipText = "";
                tooltip.enabled = false;
            }
        }
    }
    
    private void UpdateWeightDisplay(float currentWeight)
    {
        // Update weight text
        if (weightText != null)
        {
            weightText.text = $"{currentWeight:F1} / {playerInventory.maxWeightBeforePenalty:F1}";
        }
        
        // Update weight bar if present
        if (weightBar != null)
        {
            weightBar.fillAmount = currentWeight / (playerInventory.maxWeightBeforePenalty * 2);
            
            // Change color based on weight (green->yellow->red)
            float ratio = currentWeight / playerInventory.maxWeightBeforePenalty;
            weightBar.color = Color.Lerp(Color.green, Color.red, ratio);
        }
    }
    
    // Event handlers for specific item operations
    private void OnItemAdded(ItemData item)
    {
        Debug.Log($"[InventoryUI] Item added: {item.itemName}");
        // Force a UI update to ensure consistency
        UpdateUI();
    }
    
    private void OnItemRemoved(ItemData item)
    {
        Debug.Log($"[InventoryUI] Item removed: {item.itemName}");
        // Force a UI update to ensure consistency
        UpdateUI();
    }
    
    // Public method to manually refresh the UI (useful for debugging)
    public void RefreshUI()
    {
        UpdateUI();
        Debug.Log("[InventoryUI] Manually refreshed UI");
    }
    
    // Helper script for tooltips - you may already have one
    [System.Serializable]
    public class Tooltip : MonoBehaviour
    {
        public string tooltipText;
    }
} 