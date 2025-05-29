using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    
    [Header("Options")]
    public bool destroyOnPickup = true;
    public float pickupDistance = 2f;
    public int priority = 1; // Items have lower priority than NPCs by default
    
    private bool isCurrentInteractable = false;
    private PlayerController playerInRange = null;
    
    private void Start()
    {
        // Ensure the collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
        Debug.Log($"[InteractableItem] Initialized: {gameObject.name} with item: {(itemData ? itemData.itemName : "NULL")}");
    }
    
    public void Interact(PlayerController player)
    {
        Debug.Log($"[InteractableItem] Interact called on {gameObject.name} with item: {(itemData ? itemData.itemName : "NULL")}");
        
        if (itemData == null)
        {
            Debug.LogError($"[InteractableItem] No ItemData assigned to {gameObject.name}");
            return;
        }
        
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        
        if (inventory != null)
        {
            Debug.Log($"[InteractableItem] Found inventory on player, attempting to add {itemData.itemName}");
            if (inventory.AddItem(itemData))
            {
                Debug.Log($"[InteractableItem] Successfully added {itemData.itemName} to inventory");
                // Play effects if they exist
                if (itemData.pickupEffect != null)
                {
                    Instantiate(itemData.pickupEffect, transform.position, Quaternion.identity);
                }
                
                if (itemData.pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(itemData.pickupSound, transform.position);
                }
                
                // Unregister from player before destroying
                if (playerInRange != null)
                {
                    playerInRange.UnregisterInteractable(this);
                }
                
                // Remove from world if configured
                if (destroyOnPickup)
                {
                    Debug.Log($"[InteractableItem] Destroying {gameObject.name} after pickup");
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log($"[InteractableItem] Inventory full, could not add {itemData.itemName}");
                // Inventory full message
                DialogueManager.ShowItemMessage("Inventory full!");
            }
        }
        else
        {
            Debug.LogError($"[InteractableItem] No InventorySystem found on player!");
        }
    }
    
    public void OnGainFocus()
    {
        isCurrentInteractable = true;
        Debug.Log($"[InteractableItem] {gameObject.name} gained focus");
        if (itemData != null)
        {
            DialogueManager.ShowItemMessage($"Press E to pick up {itemData.itemName}");
        }
    }
    
    public void OnLoseFocus()
    {
        isCurrentInteractable = false;
        Debug.Log($"[InteractableItem] {gameObject.name} lost focus");
        DialogueManager.Hide();
    }
    
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    
    public int GetPriority()
    {
        return priority;
    }
    
    // Register with player when they enter trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[InteractableItem] Player entered trigger for {gameObject.name}");
            playerInRange = other.GetComponent<PlayerController>();
            if (playerInRange != null)
            {
                playerInRange.RegisterInteractable(this);
            }
        }
    }
    
    // Unregister from player when they exit trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[InteractableItem] Player exited trigger for {gameObject.name}");
            if (playerInRange != null)
            {
                playerInRange.UnregisterInteractable(this);
                playerInRange = null;
            }
        }
    }
} 