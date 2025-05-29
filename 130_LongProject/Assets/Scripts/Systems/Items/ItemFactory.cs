using UnityEngine;

// This is a helper class to create test items at runtime
public class ItemFactory : MonoBehaviour
{
    [Header("Item Prefab")]
    public GameObject itemPrefab;
    
    [Header("Test Items")]
    public ItemData[] testItems;
    
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float spawnRadius = 3f;
    
    public void SpawnRandomItem()
    {
        if (testItems == null || testItems.Length == 0 || itemPrefab == null)
            return;
            
        // Select a random test item
        ItemData randomItem = testItems[Random.Range(0, testItems.Length)];
        
        // Calculate random position
        Vector3 randomPos = spawnPoint != null 
            ? spawnPoint.position + Random.insideUnitSphere * spawnRadius 
            : transform.position + Random.insideUnitSphere * spawnRadius;
        
        randomPos.y = 0.5f; // Adjust height above ground
        
        // Create the item
        GameObject itemObj = Instantiate(itemPrefab, randomPos, Quaternion.identity);
        InteractableItem interactable = itemObj.GetComponent<InteractableItem>();
        
        if (interactable != null)
        {
            interactable.itemData = randomItem;
            itemObj.name = randomItem.itemName;
        }
        
        Debug.Log($"Spawned test item: {randomItem.itemName}");
    }
    
    public void SpawnSpecificItem(string itemName)
    {
        if (testItems == null || testItems.Length == 0 || itemPrefab == null)
            return;
            
        // Find the item by name
        ItemData itemToSpawn = null;
        foreach (var item in testItems)
        {
            if (item.itemName == itemName)
            {
                itemToSpawn = item;
                break;
            }
        }
        
        if (itemToSpawn == null)
        {
            Debug.LogWarning($"No item found with name: {itemName}");
            return;
        }
        
        // Calculate position
        Vector3 pos = spawnPoint != null 
            ? spawnPoint.position 
            : transform.position + Vector3.forward * 2f;
        
        pos.y = 0.5f; // Adjust height above ground
        
        // Create the item
        GameObject itemObj = Instantiate(itemPrefab, pos, Quaternion.identity);
        InteractableItem interactable = itemObj.GetComponent<InteractableItem>();
        
        if (interactable != null)
        {
            interactable.itemData = itemToSpawn;
            itemObj.name = itemToSpawn.itemName;
        }
        
        Debug.Log($"Spawned item: {itemToSpawn.itemName}");
    }
    
    // Example: Use this method to spawn an item associated with a specific quest
    public void SpawnQuestItem(string questName)
    {
        if (testItems == null || testItems.Length == 0 || itemPrefab == null)
            return;
            
        // Find an item linked to this quest
        ItemData questItem = null;
        foreach (var item in testItems)
        {
            if (item.associatedQuestName == questName)
            {
                questItem = item;
                break;
            }
        }
        
        if (questItem == null)
        {
            Debug.LogWarning($"No quest item found for quest: {questName}");
            return;
        }
        
        // Calculate position
        Vector3 pos = spawnPoint != null 
            ? spawnPoint.position 
            : transform.position + Vector3.forward * 2f;
        
        pos.y = 0.5f; // Adjust height above ground
        
        // Create the item
        GameObject itemObj = Instantiate(itemPrefab, pos, Quaternion.identity);
        InteractableItem interactable = itemObj.GetComponent<InteractableItem>();
        
        if (interactable != null)
        {
            interactable.itemData = questItem;
            itemObj.name = questItem.itemName + " (Quest)";
        }
        
        Debug.Log($"Spawned quest item: {questItem.itemName} for quest: {questName}");
    }
} 