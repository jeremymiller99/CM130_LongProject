using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class DebugHelper : MonoBehaviour
{
    [Header("UI References")]
    public Button addItemButton;
    public Button removeItemButton;
    public Button printInventoryButton;
    public Button testQuestButton;
    public Button testNPCSwitchButton; // New button for NPC switching test
    public Button testQuestStateButton; // New button for quest state persistence test
    public Button testQuestFlowButton; // New button for quest dialogue flow test
    public TMP_Dropdown itemDropdown;
    public TMP_Text statusText;
    
    [Header("Test Items")]
    public ItemData[] testItems;
    
    [Header("Test Quest")]
    public QuestData testQuest;
    
    private InventorySystem playerInventory;
    private QuestManager questManager;
    
    void Start()
    {
        // Find systems
        playerInventory = FindObjectOfType<InventorySystem>();
        questManager = FindObjectOfType<QuestManager>();
        
        // Setup UI
        if (addItemButton) addItemButton.onClick.AddListener(AddSelectedItem);
        if (removeItemButton) removeItemButton.onClick.AddListener(RemoveSelectedItem);
        if (printInventoryButton) printInventoryButton.onClick.AddListener(PrintInventory);
        if (testQuestButton) testQuestButton.onClick.AddListener(TestQuestItemCheck);
        if (testNPCSwitchButton) testNPCSwitchButton.onClick.AddListener(TestNPCSwitching);
        if (testQuestStateButton) testQuestStateButton.onClick.AddListener(TestQuestStatePersistence);
        if (testQuestFlowButton) testQuestFlowButton.onClick.AddListener(TestQuestDialogueFlow);
        
        // Populate dropdown
        PopulateItemDropdown();
        
        UpdateStatus("Debug Helper Ready");
    }
    
    void PopulateItemDropdown()
    {
        if (itemDropdown == null || testItems == null) return;
        
        itemDropdown.ClearOptions();
        foreach (var item in testItems)
        {
            if (item != null)
                itemDropdown.options.Add(new TMP_Dropdown.OptionData(item.itemName));
        }
        itemDropdown.RefreshShownValue();
    }
    
    void AddSelectedItem()
    {
        if (playerInventory == null || testItems == null) return;
        
        int selectedIndex = itemDropdown.value;
        if (selectedIndex >= 0 && selectedIndex < testItems.Length)
        {
            ItemData selectedItem = testItems[selectedIndex];
            if (selectedItem != null)
            {
                bool success = playerInventory.AddItem(selectedItem);
                UpdateStatus(success ? $"Added {selectedItem.itemName}" : "Failed to add item (inventory full?)");
            }
        }
    }
    
    void RemoveSelectedItem()
    {
        if (playerInventory == null || testItems == null) return;
        
        int selectedIndex = itemDropdown.value;
        if (selectedIndex >= 0 && selectedIndex < testItems.Length)
        {
            ItemData selectedItem = testItems[selectedIndex];
            if (selectedItem != null)
            {
                bool success = playerInventory.RemoveItem(selectedItem);
                UpdateStatus(success ? $"Removed {selectedItem.itemName}" : "Item not found in inventory");
            }
        }
    }
    
    void PrintInventory()
    {
        if (playerInventory == null) return;
        
        playerInventory.DebugPrintInventory();
        UpdateStatus("Inventory printed to console");
    }
    
    void TestQuestItemCheck()
    {
        if (playerInventory == null || questManager == null)
        {
            UpdateStatus("Missing required systems");
            return;
        }
        
        // Test with NPCs instead of direct quest
        NPCBehavior[] npcs = FindObjectsOfType<NPCBehavior>();
        
        foreach (var npc in npcs)
        {
            if (npc.profile?.assignedQuest != null && npc.HasActiveQuest())
            {
                var result = npc.CheckQuestItemStatus(playerInventory);
                Debug.Log($"Quest Status for {npc.profile.npcName}: {result.message}");
                Debug.Log($"Has all items: {result.hasAllItems}");
                
                if (result.missingItems.Length > 0)
                {
                    Debug.Log($"Missing: {string.Join(", ", result.missingItems.Select(item => item.itemName))}");
                }
                
                if (result.foundItems.Length > 0)
                {
                    Debug.Log($"Found: {string.Join(", ", result.foundItems.Select(item => item.itemName))}");
                }
                
                UpdateStatus($"Quest check for {npc.profile.npcName}: {(result.hasAllItems ? "Complete" : "Incomplete")}");
                return; // Test first found quest
            }
        }
        
        // Fallback to direct quest test if no NPCs have active quests
        if (testQuest != null)
        {
            var result = questManager.CheckQuestItemRequirements(testQuest, playerInventory);
            
            string status = $"Quest Check: {(result.hasAllItems ? "COMPLETE" : "INCOMPLETE")}\n{result.message}";
            UpdateStatus(status);
            
            Debug.Log($"[DebugHelper] Quest item check result: {result.hasAllItems}");
            Debug.Log($"[DebugHelper] Message: {result.message}");
        }
        else
        {
            UpdateStatus("No active quests found");
        }
    }
    
    // Test NPC switching to verify no bugs occur
    void TestNPCSwitching()
    {
        NPCBehavior[] npcs = FindObjectsOfType<NPCBehavior>();
        
        if (npcs.Length >= 2)
        {
            Debug.Log("[DebugHelper] Testing NPC switching...");
            UpdateStatus("Testing NPC switching...");
            
            // Talk to first NPC
            DialogueManager.ShowDialogue(npcs[0]);
            Debug.Log($"Switched to {npcs[0].profile?.npcName}");
            
            // Wait a moment then switch to second NPC
            StartCoroutine(SwitchToSecondNPC(npcs[1]));
        }
        else
        {
            UpdateStatus("Need at least 2 NPCs to test switching");
            Debug.LogWarning("[DebugHelper] Need at least 2 NPCs to test switching");
        }
    }
    
    // Test quest state preservation across NPC switches
    void TestQuestStatePersistence()
    {
        NPCBehavior[] npcs = FindObjectsOfType<NPCBehavior>();
        
        if (npcs.Length >= 2)
        {
            Debug.Log("[DebugHelper] Testing quest state persistence...");
            UpdateStatus("Testing quest state persistence...");
            
            // Find an NPC with a quest
            NPCBehavior questNPC = null;
            NPCBehavior otherNPC = null;
            
            foreach (var npc in npcs)
            {
                if (npc.profile?.hasQuests == true && questNPC == null)
                {
                    questNPC = npc;
                }
                else if (otherNPC == null)
                {
                    otherNPC = npc;
                }
            }
            
            if (questNPC != null && otherNPC != null)
            {
                StartCoroutine(TestQuestStateSequence(questNPC, otherNPC));
            }
            else
            {
                UpdateStatus("Need NPCs with quests to test state persistence");
            }
        }
        else
        {
            UpdateStatus("Need at least 2 NPCs to test quest state persistence");
        }
    }
    
    // Test quest dialogue flow for new quests
    void TestQuestDialogueFlow()
    {
        NPCBehavior[] npcs = FindObjectsOfType<NPCBehavior>();
        
        // Find an NPC that can offer a quest
        NPCBehavior questNPC = null;
        foreach (var npc in npcs)
        {
            if (npc.profile?.hasQuests == true && npc.CanOfferQuest())
            {
                questNPC = npc;
                break;
            }
        }
        
        if (questNPC != null)
        {
            Debug.Log("[DebugHelper] Testing quest dialogue flow...");
            UpdateStatus("Testing quest dialogue flow...");
            
            Debug.Log($"[DebugHelper] Opening dialogue with {questNPC.profile.npcName}");
            DialogueManager.ShowDialogue(questNPC);
            
            Debug.Log("[DebugHelper] Quest dialogue flow test started - check that button shows 'Quests' initially");
            UpdateStatus("Quest dialogue flow test - button should show 'Quests' first");
        }
        else
        {
            UpdateStatus("No NPCs found that can offer new quests");
            Debug.LogWarning("[DebugHelper] No NPCs found that can offer new quests for dialogue flow test");
        }
    }
    
    private System.Collections.IEnumerator SwitchToSecondNPC(NPCBehavior secondNPC)
    {
        yield return new WaitForSeconds(1f);
        
        // Switch to second NPC
        DialogueManager.ShowDialogue(secondNPC);
        Debug.Log($"Switched to {secondNPC.profile?.npcName}");
        
        yield return new WaitForSeconds(1f);
        
        // Close dialogue
        DialogueManager.Hide();
        Debug.Log("NPC switching test completed");
        UpdateStatus("NPC switching test completed - check console for details");
    }
    
    private System.Collections.IEnumerator TestQuestStateSequence(NPCBehavior questNPC, NPCBehavior otherNPC)
    {
        Debug.Log($"[DebugHelper] Step 1: Opening dialogue with quest NPC {questNPC.profile?.npcName}");
        DialogueManager.ShowDialogue(questNPC);
        
        yield return new WaitForSeconds(2f);
        
        Debug.Log($"[DebugHelper] Step 2: Switching to other NPC {otherNPC.profile?.npcName}");
        DialogueManager.ShowDialogue(otherNPC);
        
        yield return new WaitForSeconds(2f);
        
        Debug.Log($"[DebugHelper] Step 3: Returning to quest NPC {questNPC.profile?.npcName}");
        DialogueManager.ShowDialogue(questNPC);
        
        yield return new WaitForSeconds(2f);
        
        Debug.Log("[DebugHelper] Quest state persistence test completed");
        DialogueManager.Hide();
        UpdateStatus("Quest state persistence test completed - check console for quest button states");
    }
    
    void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
        Debug.Log($"[DebugHelper] {message}");
    }
    
    // Keyboard shortcuts for quick testing
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) AddSelectedItem();
        if (Input.GetKeyDown(KeyCode.F2)) RemoveSelectedItem();
        if (Input.GetKeyDown(KeyCode.F3)) PrintInventory();
        if (Input.GetKeyDown(KeyCode.F4)) TestQuestItemCheck();
        if (Input.GetKeyDown(KeyCode.F5)) TestNPCSwitching(); // New shortcut
        if (Input.GetKeyDown(KeyCode.F6)) TestQuestStatePersistence(); // New shortcut for quest state test
        if (Input.GetKeyDown(KeyCode.F7)) TestQuestDialogueFlow(); // New shortcut for quest dialogue flow test
        
        // Quick add first few items
        if (Input.GetKeyDown(KeyCode.Alpha1) && Input.GetKey(KeyCode.LeftShift))
        {
            if (testItems != null && testItems.Length > 0 && testItems[0] != null)
                playerInventory?.AddItem(testItems[0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && Input.GetKey(KeyCode.LeftShift))
        {
            if (testItems != null && testItems.Length > 1 && testItems[1] != null)
                playerInventory?.AddItem(testItems[1]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && Input.GetKey(KeyCode.LeftShift))
        {
            if (testItems != null && testItems.Length > 2 && testItems[2] != null)
                playerInventory?.AddItem(testItems[2]);
        }
    }
} 