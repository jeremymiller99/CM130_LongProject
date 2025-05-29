using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum NPCState
{
    Idle,
    Walking,
    Talking
}

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class NPCBehavior : MonoBehaviour, IInteractable
{
    [Header("NPC Profile")]
    public NPCProfile profile;
    public ReputationManager repManager;
    
    [Header("Movement & Animation")]
    public float walkSpeed = 1.5f;
    public float walkRadius = 10f;
    public float stateChangeInterval = 5f;
    [Range(0f, 1f)] public float walkChance = 0.3f;
    
    [Header("Animation Settings")]
    public string idleAnimation = "Idle";
    public string walkAnimation = "Walk";
    public string talkAnimation = "Talk";
    public string waveAnimation = "Wave";
    
    [Header("Item Exchange")]
    // Removed independent trading items - all items are now quest-related
    public string questItemsNeededDialogue = "You need to bring me the quest items.";
    public string questItemsReadyDialogue = "I see you have the items I need!";
    public string questCompletedDialogue = "Thank you for completing this quest!";
    
    [Header("Interaction Settings")]
    public int priority = 2; // NPCs have higher priority than items by default
    public float waveInteractionRange = 5f; // Separate range for wave interactions
    public float interactionRadius = 3f; // Radius for regular interactions
    
    // Components
    private Animator animator;
    private NavMeshAgent agent;
    private Collider interactionCollider; // Reference to the collider
    
    // Interaction tracking
    private bool isCurrentInteractable = false;
    private PlayerController playerInRange = null;
    private bool isInWaveRange = false; // Track if player is in wave range
    private bool isInInteractionRange = false; // Track if player is in regular interaction range
    
    // State tracking
    private NPCState currentState = NPCState.Idle;
    private NPCState previousState = NPCState.Idle;
    private Vector3 startPosition;
    private bool isInDialogue = false;
    
    // Animation hashes
    private int idleHash;
    private int walkHash;
    private int talkHash;
    private int waveHash;
    private int speedHash;

    #region Quest System Integration
    
    public QuestInstance runtimeQuest;
    private QuestManager questManager;
    
    private void InitializeQuest()
    {
        if (profile?.assignedQuest == null) return;
        
        questManager = FindObjectOfType<QuestManager>();
        if (questManager == null)
        {
            Debug.LogError($"[NPCBehavior] {gameObject.name} - No QuestManager found!");
            return;
        }
        
        runtimeQuest = new QuestInstance { data = profile.assignedQuest };
        runtimeQuest.state = QuestState.Inactive;
        questManager.RegisterQuest(runtimeQuest);
        
        Debug.Log($"[NPCBehavior] {gameObject.name} initialized quest: {profile.assignedQuest.questName}");
    }
    
    public bool CanOfferQuest()
    {
        if (runtimeQuest == null || profile.assignedQuest == null) return false;
        return runtimeQuest.IsAvailable() && !runtimeQuest.IsActive() && 
               repManager.GetReputation() >= profile.assignedQuest.requiredReputation;
    }
    
    public bool CanCompleteQuest()
    {
        if (runtimeQuest == null || profile.assignedQuest == null) return false;
        return runtimeQuest.IsActive() && runtimeQuest.taskCompleted;
    }

    public bool HasActiveQuest()
    {
        return runtimeQuest != null && runtimeQuest.IsActive();
    }
    
    // New method to check quest item requirements without completing
    public QuestItemCheckResult CheckQuestItemStatus(InventorySystem inventory)
    {
        if (runtimeQuest == null || profile.assignedQuest == null || !runtimeQuest.IsActive())
        {
            return new QuestItemCheckResult { hasAllItems = false, message = "No active quest." };
        }
        
        return questManager.CheckQuestItemRequirements(profile.assignedQuest, inventory);
    }
    
    public void OfferQuest()
    {
        if (!CanOfferQuest()) return;
        runtimeQuest.StartQuest(questManager.GetCurrentDay(), questManager.GetCurrentTimeZone());
        Debug.Log($"Offered quest: {profile.assignedQuest.questName}");
    }
    
    public void CompleteQuest()
    {
        if (!CanCompleteQuest()) return;
        
        // Give rewards before completing
        InventorySystem playerInventory = FindObjectOfType<InventorySystem>();
        if (playerInventory != null)
        {
            questManager.GiveQuestRewards(profile.assignedQuest, playerInventory);
        }
        
        // Complete the quest
        runtimeQuest.CompleteQuest(repManager);
        Debug.Log($"Completed quest: {profile.assignedQuest.questName}");
    }
    
    public string GetQuestDialogue()
    {
        if (runtimeQuest == null || profile.assignedQuest == null)
            return profile.noQuestDialogue ?? "I have no quests for you right now.";

        if (!runtimeQuest.IsAvailable())
            return profile.noQuestDialogue ?? "I have a task for you, but it's not the right time yet.";

        if (runtimeQuest.IsActive())
        {
            // Check if quest requirements are met
            InventorySystem playerInventory = FindObjectOfType<InventorySystem>();
            if (playerInventory != null)
            {
                var itemCheck = CheckQuestItemStatus(playerInventory);
                if (itemCheck.hasAllItems)
                {
                    if (runtimeQuest.taskCompleted)
                        return profile.questCompletedDialogue ?? questCompletedDialogue;
                    else
                        return questItemsReadyDialogue;
                }
                else
                {
                    return itemCheck.message;
                }
            }
            
            return profile.questInProgressDialogue ?? "Have you completed the task I gave you?";
        }

        if (repManager.GetReputation() < profile.assignedQuest.requiredReputation)
            return profile.insufficientRepDialogue ?? "I don't know you well enough to trust you with this task.";

        return profile.questDialogue ?? "I have a task that needs doing. Would you help me?";
    }
    
    public string GetQuestAcceptDialogue()
    {
        return profile.questAcceptDialogue ?? "Thank you for accepting this task. I'll be waiting for your return.";
    }
    
    // Check if quest can be completed with current inventory
    public bool CanCompleteQuestNow(InventorySystem inventory)
    {
        if (!HasActiveQuest() || inventory == null) return false;
        
        var itemCheck = CheckQuestItemStatus(inventory);
        return itemCheck.hasAllItems;
    }
    
    #endregion

    private void Start()
    {
        if (repManager == null)
            repManager = FindObjectOfType<ReputationManager>();

        // Get components
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        interactionCollider = GetComponent<Collider>();

        if (profile == null)
        {
            Debug.LogError($"[NPCBehavior] {gameObject.name} has no profile assigned!");
            return;
        }
        
        // Cache animation hashes
        idleHash = Animator.StringToHash(idleAnimation);
        walkHash = Animator.StringToHash(walkAnimation);
        talkHash = Animator.StringToHash(talkAnimation);
        waveHash = Animator.StringToHash(waveAnimation);
        speedHash = Animator.StringToHash("Speed");
        
        // Store starting position
        startPosition = transform.position;
        
        // Configure NavMeshAgent
        if (IsAgentValid())
        {
            agent.speed = walkSpeed;
            agent.angularSpeed = 180f;
            agent.stoppingDistance = 0.5f;
        }
        
        Debug.Log($"[NPCBehavior] {gameObject.name} initialized with profile: {profile.npcName}");
        
        if (profile.assignedQuest != null)
        {
            Debug.Log($"[NPCBehavior] {gameObject.name} linked to quest: {profile.assignedQuest.questName}");
        }
        
        // Set up interaction collider
        if (interactionCollider != null)
        {
            // Adjust collider size based on interaction radius
            if (interactionCollider is CapsuleCollider capsule)
            {
                capsule.radius = interactionRadius;
                Debug.Log($"[NPCBehavior] {gameObject.name} - Set capsule collider radius to {interactionRadius}");
            }
            else if (interactionCollider is SphereCollider sphere)
            {
                sphere.radius = interactionRadius;
                Debug.Log($"[NPCBehavior] {gameObject.name} - Set sphere collider radius to {interactionRadius}");
            }
            else if (interactionCollider is BoxCollider box)
            {
                box.size = new Vector3(interactionRadius * 2, box.size.y, interactionRadius * 2);
                Debug.Log($"[NPCBehavior] {gameObject.name} - Set box collider size to {box.size}");
            }
        }
        else
        {
            Debug.LogWarning($"[NPCBehavior] {gameObject.name} has no collider component!");
        }
        
        // Start simple behavior loop
        StartCoroutine(SimpleBehaviorLoop());
        
        InitializeQuest();
    }
    
    void Update()
    {
        UpdateAnimations();
        UpdateMovement();
        CheckDialogueState();
        CheckWaveRange(); // Add wave range check
    }
    
    #region Simple 3-State Behavior System
    
    private IEnumerator SimpleBehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(stateChangeInterval);
            
            // Don't change states if in dialogue
            if (isInDialogue)
                continue;
                
            // Simple random behavior
            if (Random.value < walkChance)
            {
                SetState(NPCState.Walking);
                SetRandomDestination();
            }
            else
            {
                SetState(NPCState.Idle);
            }
        }
    }
    
    private void SetRandomDestination()
    {
        Vector3 randomPoint = GetRandomPointInRadius(startPosition, walkRadius);
        SetAgentDestination(randomPoint);
    }
    
    private Vector3 GetRandomPointInRadius(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;
        
        NavMeshHit hit;
        Vector3 finalPosition = center;
        
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        
        return finalPosition;
    }
    
    #endregion
    
    #region Animation and Movement Updates
    
    private void UpdateAnimations()
    {
        if (animator == null) 
        {
            Debug.LogWarning($"[NPCBehavior] {gameObject.name} - Animator is null!");
            return;
        }
        
        // Set speed parameter for blend trees
        float currentSpeed = GetAgentSpeed();
        
        // Check if Speed parameter exists before setting it
        if (HasAnimatorParameter("Speed"))
        {
            animator.SetFloat(speedHash, currentSpeed);
        }
        
        // Let NPCAnimationController handle the state-specific animations if it exists
        NPCAnimationController animController = GetComponent<NPCAnimationController>();
        if (animController != null)
        {
            // NPCAnimationController will handle animations
            return;
        }
        
        // Fallback: Handle animations directly if no NPCAnimationController
        SetAnimationBooleans();
    }
    
    private void SetAnimationBooleans()
    {
        // Reset all booleans first
        if (HasAnimatorParameter("IsWalking")) animator.SetBool("IsWalking", false);
        if (HasAnimatorParameter("IsTalking")) animator.SetBool("IsTalking", false);
        if (HasAnimatorParameter("IsWaving")) animator.SetBool("IsWaving", false);
        
        // Set state-specific booleans
        switch (currentState)
        {
            case NPCState.Idle:
                // Idle is default when all booleans are false
                break;
            case NPCState.Walking:
                if (HasAnimatorParameter("IsWalking"))
                    animator.SetBool("IsWalking", true);
                break;
            case NPCState.Talking:
                if (HasAnimatorParameter("IsTalking"))
                    animator.SetBool("IsTalking", true);
                if (HasAnimatorParameter("TriggerTalk"))
                    animator.SetTrigger("TriggerTalk");
                break;
        }
    }
    
    private bool HasAnimatorParameter(string parameterName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName)
                return true;
        }
        return false;
    }
    
    private void PlayAnimation(int animationHash)
    {
        if (animator == null)
        {
            Debug.LogWarning($"[NPCBehavior] {gameObject.name} - Cannot play animation, animator is null!");
            return;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[NPCBehavior] {gameObject.name} - Cannot play animation, no animator controller assigned!");
            return;
        }
        
        if (animator.HasState(0, animationHash))
        {
            animator.Play(animationHash);
            Debug.Log($"[NPCBehavior] {gameObject.name} - Playing animation with hash: {animationHash}");
        }
        else
        {
            Debug.LogWarning($"[NPCBehavior] {gameObject.name} - Animation hash {animationHash} not found in animator controller!");
        }
    }
    
    private void UpdateMovement()
    {
        if (!IsAgentValid()) return;
        
        // If walking and reached destination, go idle
        if (currentState == NPCState.Walking)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                SetState(NPCState.Idle);
            }
        }
        
        // Stop movement if in dialogue
        if (isInDialogue)
        {
            SetAgentStopped(true);
        }
        else if (currentState == NPCState.Walking)
        {
            SetAgentStopped(false);
        }
        else
        {
            SetAgentStopped(true);
        }
    }
    
    #endregion
    
    #region Dialogue System Integration
    
    private void CheckDialogueState()
    {
        // Check if THIS SPECIFIC NPC is in dialogue, not just any NPC
        bool shouldBeInDialogue = DialogueManager.IsNPCInDialogue(this);
        
        if (shouldBeInDialogue && !isInDialogue)
        {
            EnterDialogueState();
        }
        else if (!shouldBeInDialogue && isInDialogue)
        {
            ExitDialogueState();
        }
    }
    
    public void EnterDialogueState()
    {
        isInDialogue = true;
        SetState(NPCState.Talking);
        SetAgentStopped(true);
    }
    
    public void ExitDialogueState()
    {
        isInDialogue = false;
        SetState(NPCState.Idle);
    }
    
    #endregion
    
    #region State Management
    
    public void SetState(NPCState newState)
    {
        if (currentState == newState) return;
        
        previousState = currentState;
        currentState = newState;
        
        Debug.Log($"[NPCBehavior] {gameObject.name} changed state from {previousState} to {currentState}");
    }
    
    public NPCState GetCurrentState() => currentState;
    public bool IsInDialogue() => isInDialogue;
    
    #endregion
    
    #region IInteractable Implementation
    
    public void Interact(PlayerController player)
    {
        // Check if player is waving
        if (player.IsWaving())
        {
            // Only respond to wave if player is in wave range
            if (isInWaveRange)
            {
                Debug.Log($"[NPCBehavior] {gameObject.name} - Player is waving and in wave range, responding to wave");
                RespondToWave();
            }
            else
            {
                Debug.Log($"[NPCBehavior] {gameObject.name} - Player is waving but too far for wave interaction");
            }
            return;
        }

        // Regular interaction - only if in regular interaction range
        if (!isInInteractionRange)
        {
            Debug.Log($"[NPCBehavior] {gameObject.name} - Player is too far for regular interaction");
            return;
        }

        InventorySystem inventory = player.GetComponent<InventorySystem>();
        
        // Check for quest item completion if there's an active quest
        if (inventory != null && HasActiveQuest() && CanCompleteQuestNow(inventory))
        {
            // Handle quest completion through dialogue
            DialogueManager.ShowDialogue(this);
            Debug.Log($"[NPCBehavior] Started quest completion dialogue with {profile.npcName}");
            return;
        }
        
        // Start regular dialogue using the DialogueManager's static method
        if (profile != null)
        {
            DialogueManager.ShowDialogue(this);
            Debug.Log($"[NPCBehavior] Started dialogue with {profile.npcName}");
        }
        else
        {
            Debug.LogWarning($"[NPCBehavior] {gameObject.name} has no profile for dialogue!");
        }
    }
    
    public void OnGainFocus()
    {
        isCurrentInteractable = true;
        
        // Show interaction prompt using DialogueManager
        if (profile != null && !DialogueManager.IsInFullDialogue())
        {
            DialogueManager.Show($"Press E to talk to {profile.npcName}");
        }
        
        Debug.Log($"[NPCBehavior] {gameObject.name} gained interaction focus");
    }
    
    public void OnLoseFocus()
    {
        isCurrentInteractable = false;
        
        // Hide interaction prompt if not in full dialogue
        if (!DialogueManager.IsInFullDialogue())
        {
            DialogueManager.Hide();
        }
        
        Debug.Log($"[NPCBehavior] {gameObject.name} lost interaction focus");
    }
    
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    
    public int GetPriority()
    {
        return priority;
    }
    
    #endregion
    
    #region Trigger Detection
    
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            playerInRange = player;
            
            // Check distances for both ranges
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            isInInteractionRange = distanceToPlayer <= interactionRadius;
            isInWaveRange = distanceToPlayer <= waveInteractionRange;
            
            // Only register for regular interaction if in regular range
            if (isInInteractionRange)
            {
                player.RegisterInteractable(this);
                Debug.Log($"[NPCBehavior] Player entered interaction range of {gameObject.name}");
            }
            
            if (isInWaveRange)
            {
                Debug.Log($"[NPCBehavior] Player entered wave range of {gameObject.name}");
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && player == playerInRange)
        {
            player.UnregisterInteractable(this);
            playerInRange = null;
            isInWaveRange = false;
            isInInteractionRange = false;
            Debug.Log($"[NPCBehavior] Player exited interaction range of {gameObject.name}");
        }
    }
    
    private void CheckWaveRange()
    {
        if (playerInRange != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerInRange.transform.position);
            bool wasInWaveRange = isInWaveRange;
            bool wasInInteractionRange = isInInteractionRange;
            
            isInWaveRange = distanceToPlayer <= waveInteractionRange;
            isInInteractionRange = distanceToPlayer <= interactionRadius;

            // Handle regular interaction range changes
            if (isInInteractionRange && !wasInInteractionRange)
            {
                playerInRange.RegisterInteractable(this);
                Debug.Log($"[NPCBehavior] Player entered interaction range of {gameObject.name}");
            }
            else if (!isInInteractionRange && wasInInteractionRange)
            {
                playerInRange.UnregisterInteractable(this);
                Debug.Log($"[NPCBehavior] Player exited interaction range of {gameObject.name}");
            }

            // Handle wave range changes
            if (isInWaveRange && !wasInWaveRange)
            {
                Debug.Log($"[NPCBehavior] Player entered wave range of {gameObject.name}");
            }
            else if (!isInWaveRange && wasInWaveRange)
            {
                Debug.Log($"[NPCBehavior] Player exited wave range of {gameObject.name}");
            }
        }
    }
    
    #endregion
    
    #region NavMesh Utilities
    
    private bool IsAgentValid()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }
    
    private void SetAgentStopped(bool stopped)
    {
        if (IsAgentValid())
        {
            agent.isStopped = stopped;
        }
    }
    
    private void SetAgentDestination(Vector3 destination)
    {
        if (IsAgentValid())
        {
            agent.SetDestination(destination);
        }
    }
    
    private float GetAgentSpeed()
    {
        if (IsAgentValid())
        {
            return agent.velocity.magnitude;
        }
        return 0f;
    }
    
    #endregion
    
    #region Wave System
    
    private void RespondToWave()
    {
        Debug.Log($"[NPCBehavior] {gameObject.name} - RespondToWave called");
        
        // Only respond if not in dialogue
        if (isInDialogue)
        {
            Debug.Log($"[NPCBehavior] {gameObject.name} - Cannot wave, NPC is in dialogue");
            return;
        }
        
        // Set temporary wave state
        SetState(NPCState.Talking);
        Debug.Log($"[NPCBehavior] {gameObject.name} - Set state to Talking for wave");
        
        // Trigger wave animation using NPCAnimationController if available
        NPCAnimationController animController = GetComponent<NPCAnimationController>();
        if (animController != null)
        {
            Debug.Log($"[NPCBehavior] {gameObject.name} - Using NPCAnimationController for wave");
            animController.TriggerWave();
        }
        else
        {
            Debug.Log($"[NPCBehavior] {gameObject.name} - No NPCAnimationController found, using direct animator control");
            // Fallback to direct animator control
            if (HasAnimatorParameter("TriggerWave"))
            {
                animator.SetTrigger("TriggerWave");
                Debug.Log($"[NPCBehavior] {gameObject.name} - Triggered wave animation directly");
            }
            else
            {
                Debug.LogWarning($"[NPCBehavior] {gameObject.name} - No TriggerWave parameter found in animator!");
            }
        }
        
        // Show wave response dialogue
        if (profile != null)
        {
            Debug.Log($"[NPCBehavior] {gameObject.name} - Showing wave response dialogue");
            DialogueManager.Show($"{profile.npcName} waves back!");
        }
        
        // Return to idle state after a short delay
        Debug.Log($"[NPCBehavior] {gameObject.name} - Starting return to idle coroutine");
        StartCoroutine(ReturnToIdleAfterWave());
    }
    
    private IEnumerator ReturnToIdleAfterWave()
    {
        Debug.Log($"[NPCBehavior] {gameObject.name} - Waiting 2 seconds before returning to idle");
        yield return new WaitForSeconds(2f);
        SetState(NPCState.Idle);
        Debug.Log($"[NPCBehavior] {gameObject.name} - Returned to idle state after wave");
    }
    
    #endregion

    // For debugging, visualize the interaction and wave ranges
    private void OnDrawGizmos()
    {
        // Draw regular interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        
        // Draw wave interaction range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, waveInteractionRange);
        
        // Show connection to player if in range
        if (playerInRange != null)
        {
            if (isInWaveRange)
            {
                Gizmos.color = Color.green;
            }
            else if (isInInteractionRange)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawLine(transform.position, playerInRange.transform.position);
        }
    }
}