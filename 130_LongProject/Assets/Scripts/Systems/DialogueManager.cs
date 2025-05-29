using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

/// <summary>
/// Simplified Dialogue Manager with Unified Typewriter System
/// 
/// Two UI panels:
/// 1. SIMPLE MESSAGE PANEL - For interaction prompts and notifications
/// 2. FULL DIALOGUE PANEL - For NPC conversations
/// 
/// Both use the same unified typewriter effect for consistency.
/// Simple messages are immediately hidden when starting conversations.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("Simple Message Panel")]
    public GameObject simpleMessagePanel;
    public TMP_Text simpleMessageText;
    public Image simpleMessageBackground;
    
    [Header("Full Dialogue Panel")]
    public GameObject dialoguePanel;
    public TMP_Text npcNameText;
    public TMP_Text dialogueText;
    public Image npcPortrait;
    public Button closeButton;
    public Image dialogueBackground;
    
    [Header("Dialogue Choice Buttons")]
    public GameObject choiceButtonsPanel;
    public Button questButton;
    public Button loreButton;
    public Button tradeButton;
    public Button generalButton;
    public Button goodbyeButton;
    
    [Header("Choice Button Text")]
    public TMP_Text questButtonText;
    public TMP_Text loreButtonText;
    public TMP_Text tradeButtonText;
    public TMP_Text generalButtonText;
    public TMP_Text goodbyeButtonText;
    
    [Header("Animation Settings")]
    public float typewriterSpeed = 0.05f;
    public bool useTypewriterEffect = true;
    public float simpleMessageDisplayTime = 3f;
    
    [Header("UI Management")]
    public bool hideInventoryDuringDialogue = true;

    private static DialogueManager instance;
    private NPCBehavior currentNPC;
    private bool isInFullDialogue = false;
    
    // Unified typewriter system
    private bool isTyping = false;
    private string currentText = "";
    private TMP_Text currentTextComponent = null;
    private Coroutine typingCoroutine;
    
    // UI Management
    private InventoryUI inventoryUI;
    private bool inventoryWasVisible = true;
    
    // Events for other systems to hook into
    public static event Action OnDialogueStart;
    public static event Action OnDialogueEnd;

    void Awake()
    {
        instance = this;
        SetupUI();
    }
    
    void Start()
    {
        HideAllPanels();
        
        // Find InventoryUI component for hiding during dialogue
        if (hideInventoryDuringDialogue)
        {
            inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI == null)
            {
                Debug.LogWarning("[DialogueManager] hideInventoryDuringDialogue is enabled but no InventoryUI found in scene");
            }
        }
    }
    
    private void SetupUI()
    {
        // Setup button click events
        if (questButton) questButton.onClick.AddListener(() => OnChoiceSelected(DialogueChoice.Quest));
        if (loreButton) loreButton.onClick.AddListener(() => OnChoiceSelected(DialogueChoice.Lore));
        if (tradeButton) tradeButton.onClick.AddListener(() => OnChoiceSelected(DialogueChoice.Trade));
        if (generalButton) generalButton.onClick.AddListener(() => OnChoiceSelected(DialogueChoice.General));
        if (goodbyeButton) goodbyeButton.onClick.AddListener(() => OnChoiceSelected(DialogueChoice.Goodbye));
        if (closeButton) closeButton.onClick.AddListener(() => OnChoiceSelected(DialogueChoice.Goodbye));
        
        // Setup default button text
        if (questButtonText) questButtonText.text = "Quests";
        if (loreButtonText) loreButtonText.text = "Tell me about this place";
        if (tradeButtonText) tradeButtonText.text = "Trade";
        if (generalButtonText) generalButtonText.text = "Chat";
        if (goodbyeButtonText) goodbyeButtonText.text = "Goodbye";
    }

    // ===== PUBLIC STATIC METHODS =====
    
    public static void ShowDialogue(NPCBehavior npc)
    {
        if (instance != null && npc != null && npc.profile != null)
        {
            // If already in dialogue with a different NPC, clean up first
            if (instance.isInFullDialogue && instance.currentNPC != null && instance.currentNPC != npc)
            {
                Debug.Log($"[DialogueManager] Switching from {instance.currentNPC.profile.npcName} to {npc.profile.npcName}");
                instance.CleanupCurrentDialogue();
            }
            
            // Immediately hide any simple messages
            instance.HideSimpleMessage();
            
            instance.currentNPC = npc;
            instance.isInFullDialogue = true;
            instance.HideInventoryUI();
            instance.SetupDialogueUI(npc.profile);
            instance.ShowGreeting();
            instance.dialoguePanel.SetActive(true);
            
            // Subscribe to inventory changes to update quest button state
            instance.SubscribeToInventoryChanges();
            
            OnDialogueStart?.Invoke();
            
            Debug.Log($"[DialogueManager] Started dialogue with {npc.profile.npcName}");
        }
    }

    public static void Show(string message)
    {
        if (instance != null)
        {
            instance.ShowSimpleMessage(message);
        }
    }
    
    public static void ShowItemMessage(string message)
    {
        if (instance != null)
        {
            instance.ShowSimpleMessage(message);
        }
    }
    
    public static void Hide()
    {
        if (instance != null)
        {
            instance.HideAllPanels();
        }
    }
    
    public static bool IsInFullDialogue()
    {
        return instance != null && instance.isInFullDialogue;
    }

    /// <summary>
    /// Check if a specific NPC is currently in dialogue
    /// </summary>
    /// <param name="npc">The NPC to check</param>
    /// <returns>True if this specific NPC is currently in dialogue</returns>
    public static bool IsNPCInDialogue(NPCBehavior npc)
    {
        return instance != null && instance.isInFullDialogue && instance.currentNPC == npc;
    }

    // ===== SIMPLE MESSAGE METHODS =====
    
    private void ShowSimpleMessage(string message)
    {
        // Don't show simple messages during full dialogue
        if (isInFullDialogue) return;
        
        Debug.Log($"[DialogueManager] Showing simple message: '{message}'");
        
        // Cancel any pending auto-hide
        CancelInvoke(nameof(HideSimpleMessage));
        
        // Show the simple message panel
        if (simpleMessagePanel) simpleMessagePanel.SetActive(true);
        
        // Start typewriter effect
        StartTypewriter(message, simpleMessageText);
        
        // Schedule auto-hide
        if (simpleMessageDisplayTime > 0)
        {
            Invoke(nameof(HideSimpleMessage), simpleMessageDisplayTime);
        }
    }
    
    private void HideSimpleMessage()
    {
        CancelInvoke(nameof(HideSimpleMessage));
        
        if (simpleMessagePanel)
        {
            simpleMessagePanel.SetActive(false);
        }
        
        // Stop typewriter if it's working on simple message
        if (currentTextComponent == simpleMessageText)
        {
            StopTypewriter();
        }
    }

    // ===== FULL DIALOGUE METHODS =====
    
    private void SetupDialogueUI(NPCProfile profile)
    {
        // Set NPC name
        if (npcNameText) 
        {
            npcNameText.text = profile.npcName;
            npcNameText.gameObject.SetActive(true);
        }
        
        // Set portrait
        if (npcPortrait)
        {
            if (profile.portrait)
            {
                npcPortrait.sprite = profile.portrait;
                npcPortrait.gameObject.SetActive(true);
            }
            else
            {
                npcPortrait.gameObject.SetActive(false);
            }
        }
        
        // Setup choice buttons
        SetupChoiceButtons(profile);
        
        // Refresh quest button state to reflect current quest status
        if (profile.hasQuests)
        {
            RefreshQuestButtonState();
        }
    }
    
    private void SetupChoiceButtons(NPCProfile profile)
    {
        if (questButton) 
        {
            questButton.gameObject.SetActive(profile.hasQuests);
            // Don't reset quest button text here - RefreshQuestButtonState will set the correct text
        }
        if (loreButton) loreButton.gameObject.SetActive(profile.hasLore);
        if (tradeButton) tradeButton.gameObject.SetActive(profile.hasTrade);
        if (generalButton) generalButton.gameObject.SetActive(profile.hasGeneralChat);
        if (goodbyeButton) goodbyeButton.gameObject.SetActive(true);
    }
    
    private void ShowGreeting()
    {
        if (currentNPC?.profile != null)
        {
            string greeting = GetReputationBasedGreeting();
            StartTypewriter(greeting, dialogueText);
            ShowChoicesPanel(true);
        }
    }
    
    private string GetReputationBasedGreeting()
    {
        if (currentNPC?.repManager != null)
        {
            int rep = currentNPC.repManager.GetReputation();
            var profile = currentNPC.profile;
            
            if (rep < profile.reputationThreshold && !string.IsNullOrEmpty(profile.lowRepDialogue))
                return profile.lowRepDialogue;
            else if (rep >= profile.reputationThreshold && !string.IsNullOrEmpty(profile.highRepDialogue))
                return profile.highRepDialogue;
        }
        
        return currentNPC.profile.greetingDialogue;
    }
    
    private void OnChoiceSelected(DialogueChoice choice)
    {
        if (currentNPC?.profile == null) return;
        
        string responseText = "";
        
        switch (choice)
        {
            case DialogueChoice.Quest:
                responseText = HandleQuestDialogue();
                break;
                
            case DialogueChoice.Lore:
                responseText = currentNPC.profile.loreDialogue;
                if (string.IsNullOrEmpty(responseText))
                    responseText = currentNPC.profile.noLoreDialogue;
                break;
                
            case DialogueChoice.Trade:
                responseText = currentNPC.profile.tradeDialogue;
                if (string.IsNullOrEmpty(responseText))
                    responseText = currentNPC.profile.noTradeDialogue;
                break;
                
            case DialogueChoice.General:
                responseText = currentNPC.profile.generalDialogue;
                break;
                
            case DialogueChoice.Goodbye:
                HideDialogue();
                return;
        }
        
        StartTypewriter(responseText, dialogueText);
        ShowChoicesPanel(true);
    }
    
    private string HandleQuestDialogue()
    {
        if (currentNPC == null) return "Error: No NPC found.";

        string questDialogue = currentNPC.GetQuestDialogue();
        bool canOfferQuest = currentNPC.CanOfferQuest();
        bool hasActiveQuest = currentNPC.HasActiveQuest();
        
        // Get player inventory for quest checks
        InventorySystem playerInventory = FindObjectOfType<InventorySystem>();
        QuestManager questManager = FindObjectOfType<QuestManager>();
        
        if (canOfferQuest)
        {
            // NPC can offer a new quest - show "Accept Quest" button after displaying quest details
            questButtonText.text = "Accept Quest";
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(() => {
                currentNPC.OfferQuest();
                StartTypewriter(currentNPC.GetQuestAcceptDialogue(), dialogueText);
                
                // Refresh quest button state after accepting
                RefreshQuestButtonState();
            });
        }
        else if (hasActiveQuest && playerInventory != null)
        {
            // Check quest item status
            var itemCheck = currentNPC.CheckQuestItemStatus(playerInventory);
            
            if (itemCheck.hasAllItems)
            {
                // Player has all required items - show completion button
                questButtonText.text = "Complete Quest";
                questButton.onClick.RemoveAllListeners();
                questButton.onClick.AddListener(() => {
                    HandleQuestCompletion();
                });
            }
            else
            {
                // Player doesn't have required items - show status check button
                questButtonText.text = "Check Quest Status";
                questButton.onClick.RemoveAllListeners();
                questButton.onClick.AddListener(() => {
                    var statusCheck = currentNPC.CheckQuestItemStatus(playerInventory);
                    StartTypewriter(statusCheck.message, dialogueText);
                    
                    // Refresh button state after showing status - might become "Complete Quest"
                    RefreshQuestButtonState();
                });
            }
        }
        else if (hasActiveQuest)
        {
            // Active quest but no inventory access
            questButtonText.text = "Quest Progress";
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(() => {
                StartTypewriter("You have an active quest with me.", dialogueText);
            });
        }
        else
        {
            // No quest available or already completed
            questButtonText.text = "Quests";
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(() => {
                string questDialogue = HandleQuestDialogue(); // This sets up the "Accept Quest" button
                StartTypewriter(questDialogue, dialogueText); // Display the quest text
            });
        }

        return questDialogue;
    }
    
    private void HandleQuestCompletion()
    {
        // Safety checks
        if (!isInFullDialogue)
        {
            Debug.LogWarning("[DialogueManager] HandleQuestCompletion called but not in full dialogue");
            return;
        }
        
        if (currentNPC == null)
        {
            Debug.LogError("[DialogueManager] HandleQuestCompletion called but currentNPC is null");
            return;
        }
        
        if (currentNPC.profile?.assignedQuest == null)
        {
            Debug.LogWarning($"[DialogueManager] {currentNPC.profile?.npcName} has no assigned quest for completion");
            StartTypewriter("I don't have any quest for you to complete.", dialogueText);
            return;
        }
        
        Debug.Log($"[DialogueManager] Handling quest completion for {currentNPC.profile.npcName}: {currentNPC.profile.assignedQuest.questName}");
        
        InventorySystem playerInventory = FindObjectOfType<InventorySystem>();
        QuestManager questManager = FindObjectOfType<QuestManager>();
        
        if (playerInventory == null || questManager == null)
        {
            StartTypewriter("Error: Cannot complete quest at this time.", dialogueText);
            return;
        }
        
        // Verify items are still available
        var itemCheck = currentNPC.CheckQuestItemStatus(playerInventory);
        if (!itemCheck.hasAllItems)
        {
            StartTypewriter(itemCheck.message, dialogueText);
            RefreshQuestButtonState(); // Update button state
            return;
        }
        
        // Consume quest items if required
        if (currentNPC.profile.assignedQuest.consumeItems)
        {
            questManager.ConsumeQuestItems(currentNPC.profile.assignedQuest, playerInventory);
        }
        
        // Mark quest task as complete and complete the quest
        if (currentNPC.runtimeQuest != null)
        {
            currentNPC.runtimeQuest.MarkTaskAsComplete();
        }
        
        // Complete the quest (this handles rewards and reputation)
        currentNPC.CompleteQuest();
        
        // Show completion dialogue
        string completionMessage = currentNPC.profile.assignedQuest.completionMessage ?? 
                                 currentNPC.questCompletedDialogue;
        StartTypewriter(completionMessage, dialogueText);
        
        // Refresh button states
        RefreshQuestButtonState();
        
        Debug.Log($"[DialogueManager] Quest completed: {currentNPC.profile.assignedQuest.questName}");
    }
    
    // Method to refresh just the quest button state without affecting other buttons
    private void RefreshQuestButtonState()
    {
        // Additional safety checks
        if (!isInFullDialogue)
        {
            Debug.LogWarning("[DialogueManager] RefreshQuestButtonState called but not in full dialogue");
            return;
        }
        
        if (currentNPC?.profile == null)
        {
            Debug.LogWarning("[DialogueManager] RefreshQuestButtonState called but currentNPC is null or has no profile");
            return;
        }
        
        if (questButton == null || questButtonText == null)
        {
            Debug.LogWarning("[DialogueManager] Quest button or text component is null");
            return;
        }
        
        Debug.Log($"[DialogueManager] Refreshing quest button state for {currentNPC.profile.npcName}");
        
        bool canOfferQuest = currentNPC.CanOfferQuest();
        bool hasActiveQuest = currentNPC.HasActiveQuest();
        
        Debug.Log($"[DialogueManager] Quest state - CanOffer: {canOfferQuest}, HasActive: {hasActiveQuest}");
        
        // Get player inventory for quest checks
        InventorySystem playerInventory = FindObjectOfType<InventorySystem>();
        
        if (hasActiveQuest && playerInventory != null)
        {
            // Priority: Handle active quests first
            var itemCheck = currentNPC.CheckQuestItemStatus(playerInventory);
            
            if (itemCheck.hasAllItems)
            {
                // Player has all required items - show completion button
                questButtonText.text = "Complete Quest";
                questButton.onClick.RemoveAllListeners();
                questButton.onClick.AddListener(() => {
                    HandleQuestCompletion();
                });
            }
            else
            {
                // Player doesn't have required items - show status check button
                questButtonText.text = "Check Quest Status";
                questButton.onClick.RemoveAllListeners();
                questButton.onClick.AddListener(() => {
                    var statusCheck = currentNPC.CheckQuestItemStatus(playerInventory);
                    StartTypewriter(statusCheck.message, dialogueText);
                    
                    // Refresh button state after showing status - might become "Complete Quest"
                    RefreshQuestButtonState();
                });
            }
        }
        else if (hasActiveQuest)
        {
            // Active quest but no inventory access
            questButtonText.text = "Quest Progress";
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(() => {
                StartTypewriter("You have an active quest with me.", dialogueText);
            });
        }
        else
        {
            // No active quest - always start with "Quests" to let player hear quest details first
            questButtonText.text = "Quests";
            questButton.onClick.RemoveAllListeners();
            questButton.onClick.AddListener(() => {
                string questDialogue = HandleQuestDialogue(); // This sets up the "Accept Quest" button
                StartTypewriter(questDialogue, dialogueText); // Display the quest text
            });
        }
        
        Debug.Log($"[DialogueManager] Refreshed quest button state: '{questButtonText.text}'");
    }
    
    private void ShowChoicesPanel(bool show)
    {
        if (choiceButtonsPanel)
            choiceButtonsPanel.SetActive(show);
    }
    
    private void HideDialogue()
    {
        if (dialoguePanel)
            dialoguePanel.SetActive(false);
        
        StopTypewriter();
        
        bool wasInFullDialogue = isInFullDialogue;
        
        // Unsubscribe from inventory changes
        UnsubscribeFromInventoryChanges();
        
        currentNPC = null;
        isInFullDialogue = false;
        
        if (wasInFullDialogue)
        {
            ShowInventoryUI();
            OnDialogueEnd?.Invoke();
        }
    }

    // ===== UNIFIED TYPEWRITER SYSTEM =====
    
    private void StartTypewriter(string text, TMP_Text textComponent)
    {
        if (textComponent == null || string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("[DialogueManager] Invalid text component or empty text for typewriter");
            return;
        }
        
        // Stop any current typewriter
        StopTypewriter();
        
        currentText = text;
        currentTextComponent = textComponent;
        
        if (useTypewriterEffect && typewriterSpeed > 0)
        {
            Debug.Log($"[DialogueManager] Starting typewriter for: '{text}'");
            typingCoroutine = StartCoroutine(TypewriterEffect());
        }
        else
        {
            // Set text immediately if typewriter is disabled
            textComponent.text = text;
            isTyping = false;
        }
    }
    
    private void StopTypewriter()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        // Complete the text immediately
        if (isTyping && currentTextComponent != null && !string.IsNullOrEmpty(currentText))
        {
            currentTextComponent.text = currentText;
        }
        
        isTyping = false;
        currentTextComponent = null;
        currentText = "";
    }
    
    private System.Collections.IEnumerator TypewriterEffect()
    {
        isTyping = true;
        currentTextComponent.text = "";
        
        for (int i = 0; i <= currentText.Length; i++)
        {
            if (currentTextComponent == null) break; // Safety check
            
            currentTextComponent.text = currentText.Substring(0, i);
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
        Debug.Log($"[DialogueManager] Typewriter completed: '{currentTextComponent.text}'");
    }

    // ===== UTILITY METHODS =====
    
    private void HideAllPanels()
    {
        StopTypewriter();
        HideDialogue();
        HideSimpleMessage();
    }
    
    private void HideInventoryUI()
    {
        if (inventoryUI != null && hideInventoryDuringDialogue)
        {
            inventoryWasVisible = inventoryUI.gameObject.activeSelf;
            inventoryUI.gameObject.SetActive(false);
        }
    }
    
    private void ShowInventoryUI()
    {
        if (inventoryUI != null && hideInventoryDuringDialogue && inventoryWasVisible)
        {
            inventoryUI.gameObject.SetActive(true);
        }
    }

    // ===== INPUT HANDLING =====
    
    void Update()
    {
        // Skip typewriter effect with space or left click
        if (isTyping && (Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())))
        {
            StopTypewriter();
            return;
        }
        
        // Close dialogue with Escape key (only during full dialogue)
        if (isInFullDialogue && Input.GetKeyDown(KeyCode.Escape))
        {
            HideDialogue();
        }
    }
    
    private bool IsPointerOverUIElement()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    // Subscribe to inventory changes while in dialogue
    private void SubscribeToInventoryChanges()
    {
        InventorySystem playerInventory = FindObjectOfType<InventorySystem>();
        if (playerInventory != null)
        {
            // Remove listener first to prevent double subscription
            playerInventory.OnInventoryChanged.RemoveListener(OnInventoryChangedDuringDialogue);
            playerInventory.OnInventoryChanged.AddListener(OnInventoryChangedDuringDialogue);
            Debug.Log($"[DialogueManager] Subscribed to inventory changes for {currentNPC?.profile?.npcName}");
        }
        else
        {
            Debug.LogWarning("[DialogueManager] No InventorySystem found for event subscription");
        }
    }
    
    // Unsubscribe from inventory changes
    private void UnsubscribeFromInventoryChanges()
    {
        InventorySystem playerInventory = FindObjectOfType<InventorySystem>();
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged.RemoveListener(OnInventoryChangedDuringDialogue);
            Debug.Log($"[DialogueManager] Unsubscribed from inventory changes for {currentNPC?.profile?.npcName}");
        }
    }
    
    // Called when inventory changes during dialogue
    private void OnInventoryChangedDuringDialogue()
    {
        if (isInFullDialogue && currentNPC != null)
        {
            RefreshQuestButtonState();
            Debug.Log($"[DialogueManager] Quest button refreshed due to inventory change (Current NPC: {currentNPC.profile?.npcName})");
        }
        else
        {
            Debug.LogWarning("[DialogueManager] Inventory change event called but not in proper dialogue state");
        }
    }

    // Clean up current dialogue state when switching NPCs
    private void CleanupCurrentDialogue()
    {
        Debug.Log($"[DialogueManager] Cleaning up dialogue state for {currentNPC?.profile?.npcName}");
        
        // Stop any ongoing typewriter effect
        StopTypewriter();
        
        // Unsubscribe from inventory changes for the current NPC
        UnsubscribeFromInventoryChanges();
        
        // Clear any pending actions or coroutines
        CancelInvoke();
        
        // Reset quest button state to prevent contamination
        if (questButton != null)
        {
            questButton.onClick.RemoveAllListeners();
            if (questButtonText != null)
                questButtonText.text = "Quests";
        }
        
        // Clear dialogue text
        if (dialogueText != null)
            dialogueText.text = "";
        
        Debug.Log("[DialogueManager] Dialogue cleanup completed");
    }
}

public enum DialogueChoice
{
    Quest,
    Lore,
    Trade,
    General,
    Goodbye
}
