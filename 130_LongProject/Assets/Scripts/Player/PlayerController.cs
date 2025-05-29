using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(InventorySystem))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 7.5f;

    [Header("Jump Tuning")]
    [SerializeField] private float fallMultiplier = 4f;
    [SerializeField] private float lowJumpMultiplier = 6f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation Settings")]
    [SerializeField] private float animationSmoothTime = 0.1f;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 3f; // Keep for reference, but not used in raycast

    // Components
    private Rigidbody rb;
    private Animator animator;
    private InventorySystem inventory;

    // State tracking
    private bool isGrounded;
    private bool isRunning;
    private bool isWaving;
    private Vector3 moveDirection;
    private float currentSpeed;
    private float weightSpeedMultiplier = 1f;

    // Selected inventory slot
    private int currentSlot = 0;

    // Interaction tracking - this replaces the raycast system
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private IInteractable currentInteractable = null;

    // Animation parameter hashes (for performance)
    private int speedHash;
    private int isGroundedHash;
    private int interactHash;
    private int waveHash;

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        inventory = GetComponent<InventorySystem>();

        // Set up rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Cache animation parameter hashes
        speedHash = Animator.StringToHash("Speed");
        isGroundedHash = Animator.StringToHash("IsGrounded");
        interactHash = Animator.StringToHash("Interact");
        waveHash = Animator.StringToHash("Wave");
        
        // Subscribe to inventory weight changes
        if (inventory != null)
        {
            inventory.OnInventoryWeightChanged.AddListener(OnWeightChanged);
            Debug.Log("[PlayerController] Initialized with InventorySystem");
        }
        else
        {
            Debug.LogError("[PlayerController] No InventorySystem found!");
        }
        
        Debug.Log("[PlayerController] Using trigger-based interaction system");
    }

    void Update()
    {
        HandleInput();
        UpdateAnimations();
        HandleInventorySelection();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInput()
    {
        // Check for running
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Interact input (assuming E key)
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("[PlayerController] Interact key (E) pressed");
            TriggerInteract();
        }

        // Wave input (assuming G key)
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("[PlayerController] Wave key (G) pressed");
            isWaving = true;
            animator.SetTrigger(waveHash);
            
            // If we have a current interactable, trigger wave interaction
            if (currentInteractable != null)
            {
                Debug.Log($"[PlayerController] Waving at {currentInteractable.GetGameObject().name}");
                currentInteractable.Interact(this);
            }
            
            StartCoroutine(ResetWavingState());
        }
        
        // Use item input (assuming F key)
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("[PlayerController] Use item key (F) pressed");
            UseCurrentItem();
        }
    }
    
    private void HandleInventorySelection()
    {
        // Number keys 1-5 for inventory slots
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                currentSlot = i;
                ItemData selectedItem = inventory.GetItemAt(currentSlot);
                
                Debug.Log($"[PlayerController] Selected inventory slot {i+1}");
                
                if (selectedItem != null)
                {
                    Debug.Log($"[PlayerController] Selected item: {selectedItem.itemName}");
                    DialogueManager.ShowItemMessage($"Selected: {selectedItem.itemName}");
                }
                else
                {
                    Debug.Log("[PlayerController] Selected empty slot");
                    DialogueManager.ShowItemMessage("Empty slot");
                }
                break;
            }
        }
    }

    private void HandleMovement()
    {
        // Normal movement input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate camera-relative movement direction
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = (camForward * vertical + camRight * horizontal).normalized;

        // Determine current speed based on state and weight
        float targetSpeed = (isRunning ? runSpeed : moveSpeed) * weightSpeedMultiplier;
        currentSpeed = moveDirection.magnitude * targetSpeed;

        // Rotate player when moving
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // Apply movement
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    private void UpdateAnimations()
    {
        // Normal speed blending for idle/walk/run
        float animSpeed = currentSpeed / runSpeed;
        animator.SetFloat(speedHash, animSpeed, animationSmoothTime, Time.deltaTime);

        // Update grounded state
        animator.SetBool(isGroundedHash, isGrounded);
    }

    // NEW TRIGGER-BASED INTERACTION SYSTEM
    private void TriggerInteract()
    {
        Debug.Log("[PlayerController] TriggerInteract called");
        animator.SetTrigger(interactHash);

        if (currentInteractable != null)
        {
            // Check if dialogue is already active to prevent re-triggering
            if (IsDialogueActive())
            {
                Debug.Log("[PlayerController] Dialogue is already active, ignoring interaction");
                return;
            }
            
            Debug.Log($"[PlayerController] Interacting with {currentInteractable.GetGameObject().name}");
            currentInteractable.Interact(this);
        }
        else
        {
            Debug.Log("[PlayerController] No interactable found");
        }
    }

    // Helper method to check if dialogue is currently active
    private bool IsDialogueActive()
    {
        // Use the new method that distinguishes between simple messages and full dialogue
        return DialogueManager.IsInFullDialogue();
    }

    // Methods to manage nearby interactables
    public void RegisterInteractable(IInteractable interactable)
    {
        if (!nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
            Debug.Log($"[PlayerController] Registered interactable: {interactable.GetGameObject().name} (Priority: {interactable.GetPriority()})");
            
            // Sort by priority (higher priority first)
            nearbyInteractables.Sort((a, b) => b.GetPriority().CompareTo(a.GetPriority()));
            
            // Update current interactable to highest priority one
            UpdateCurrentInteractable();
        }
    }

    public void UnregisterInteractable(IInteractable interactable)
    {
        if (nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Remove(interactable);
            Debug.Log($"[PlayerController] Unregistered interactable: {interactable.GetGameObject().name}");
            
            // Update current interactable
            UpdateCurrentInteractable();
        }
    }

    private void UpdateCurrentInteractable()
    {
        IInteractable newCurrent = nearbyInteractables.Count > 0 ? nearbyInteractables[0] : null;
        
        if (currentInteractable != newCurrent)
        {
            SetCurrentInteractable(newCurrent);
        }
    }

    private void SetCurrentInteractable(IInteractable interactable)
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnLoseFocus();
        }

        currentInteractable = interactable;

        if (currentInteractable != null)
        {
            Debug.Log($"[PlayerController] Set current interactable to: {currentInteractable.GetGameObject().name} (Priority: {currentInteractable.GetPriority()})");
            currentInteractable.OnGainFocus();
        }
        else
        {
            Debug.Log("[PlayerController] No current interactable");
        }
    }
    
    private void UseCurrentItem()
    {
        Debug.Log($"[PlayerController] Attempting to use item in slot {currentSlot}");
        
        ItemData currentItem = inventory.GetItemAt(currentSlot);
        if (currentItem != null)
        {
            // For now, just drop the item in front of the player
            Debug.Log($"[PlayerController] Using item: {currentItem.itemName}");
            DialogueManager.ShowItemMessage($"Used: {currentItem.itemName}");
            
            // In the future, you could add specific item use effects here
            
            // Remove from inventory
            inventory.RemoveItemAt(currentSlot);
        }
        else
        {
            Debug.Log("[PlayerController] No item in selected slot");
            DialogueManager.ShowItemMessage("No item selected");
        }
    }

    private void TriggerWave()
    {
        animator.SetTrigger(waveHash);
        isWaving = true;
        StartCoroutine(ResetWavingState());
    }

    private IEnumerator ResetWavingState()
    {
        yield return new WaitForSeconds(1f); // Adjust this time to match your wave animation length
        isWaving = false;
    }

    public bool IsWaving()
    {
        return isWaving;
    }
    
    private void OnWeightChanged(float newWeight)
    {
        // Update the weight-based speed multiplier
        weightSpeedMultiplier = inventory.GetWeightSpeedMultiplier();
        Debug.Log($"[PlayerController] Inventory weight changed: {newWeight}kg | Speed multiplier: {weightSpeedMultiplier:F2}");
    }

    // Animation Events - Called from animation clips
    public void OnInteractComplete()
    {
        Debug.Log("[PlayerController] Interact animation completed");
    }

    public void OnWaveComplete()
    {
        Debug.Log("[PlayerController] Wave animation completed");
    }

    // Ground detection
    private void OnTriggerEnter(Collider other)
    {
        if ((groundLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            isGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((groundLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            isGrounded = false;
        }
    }

    // For debugging, visualize the interaction range
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        // Show current interactable
        if (currentInteractable != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentInteractable.GetGameObject().transform.position);
        }
    }
}