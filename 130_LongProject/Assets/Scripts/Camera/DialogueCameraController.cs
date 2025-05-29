using UnityEngine;
using System.Collections;

public class DialogueCameraController : MonoBehaviour
{
    [Header("Dialogue Camera Settings")]
    public float dialogueDistance = 3f;
    public float dialogueHeight = 1.5f;
    public float transitionSpeed = 2f;
    public float focusAngle = 5f; // Much gentler downward angle
    
    [Header("Dialogue Camera Positioning")]
    public float standardDialogueDistance = 4f; // Fixed distance for consistent framing
    public float standardDialogueHeight = 1.6f; // Slightly lower for more natural angle
    public float npcFocusOffset = 0.1f; // Focus more equally between both subjects
    
    [Header("Collision Avoidance")]
    public LayerMask obstructionLayers = -1; // What can obstruct the dialogue camera
    public float collisionCheckRadius = 0.2f; // Radius for obstruction checking
    public int positionAttempts = 8; // Number of positions to try around the subjects
    
    [Header("Animation Settings")]
    public float maxAnimationWaitTime = 2f; // Maximum time to wait for animation to complete
    
    [Header("References")]
    public ThirdPersonCamera thirdPersonCamera;
    
    private bool isInDialogueMode = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform currentNPCTarget;
    private Transform playerTransform;
    private Transform cameraRig; // Reference to the camera rig (parent)
    
    // Smooth transition variables
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isTransitioning = false;
    
    void Start()
    {
        // Find the player if not set
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) playerTransform = player.transform;
        }
        
        // Find ThirdPersonCamera if not set, and get the camera rig
        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();
        }
        
        if (thirdPersonCamera != null)
        {
            cameraRig = thirdPersonCamera.transform; // The rig is where ThirdPersonCamera is attached
            Debug.Log($"[DialogueCameraController] Found camera rig: {cameraRig.name}");
        }
        else
        {
            // Fallback: assume this script is on the camera and parent is the rig
            cameraRig = transform.parent;
            Debug.LogWarning("[DialogueCameraController] ThirdPersonCamera not found, using parent as camera rig");
        }
        
        // Subscribe to dialogue events
        DialogueManager.OnDialogueStart += OnDialogueStart;
        DialogueManager.OnDialogueEnd += OnDialogueEnd;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from dialogue events
        DialogueManager.OnDialogueStart -= OnDialogueStart;
        DialogueManager.OnDialogueEnd -= OnDialogueEnd;
    }
    
    void Update()
    {
        if (isTransitioning)
        {
            HandleCameraTransition();
        }
        
        // Failsafe: If dialogue panel is not active but we're still in dialogue mode, reset
        if (isInDialogueMode && !DialogueManager.IsInFullDialogue())
        {
            Debug.LogWarning("[DialogueCameraController] Dialogue mode active but no full dialogue detected, forcing reset");
            ForceResetToNormalMode();
        }
    }
    
    private void OnDialogueStart()
    {
        // Only start camera focus for full NPC dialogues, not simple item messages
        if (!DialogueManager.IsInFullDialogue())
        {
            Debug.Log("[DialogueCameraController] Not a full dialogue, skipping camera focus");
            return;
        }
        
        // Find the current NPC the player is talking to
        FindCurrentNPC();
        
        if (currentNPCTarget != null && playerTransform != null)
        {
            // Start coroutine to wait for animation completion before starting dialogue mode
            StartCoroutine(WaitForAnimationThenStartDialogue());
        }
        else
        {
            Debug.Log("[DialogueCameraController] No NPC target found for dialogue camera focus");
        }
    }
    
    private IEnumerator WaitForAnimationThenStartDialogue()
    {
        if (playerTransform == null) yield break;
        
        // Get the player's Animator
        Animator playerAnimator = playerTransform.GetComponent<Animator>();
        
        if (playerAnimator != null)
        {
            Debug.Log("[DialogueCameraController] Waiting for player interaction animation to complete");
            
            float waitStartTime = Time.time;
            
            // Wait for the interact animation state to start
            yield return null; // Wait one frame for animation to potentially start
            
            // Check if we're in an interaction animation state
            AnimatorStateInfo currentState = playerAnimator.GetCurrentAnimatorStateInfo(1); // UpperBody layer
            bool wasInInteractState = currentState.IsName("Interact");
            
            if (wasInInteractState)
            {
                // Wait for the interaction animation to complete
                while (Time.time - waitStartTime < maxAnimationWaitTime)
                {
                    currentState = playerAnimator.GetCurrentAnimatorStateInfo(1); // UpperBody layer
                    
                    // Check if we're still in interact state and if animation is near completion
                    if (currentState.IsName("Interact"))
                    {
                        // Wait for animation to be near completion (95% done)
                        if (currentState.normalizedTime >= 0.95f)
                        {
                            Debug.Log("[DialogueCameraController] Interact animation near completion, starting dialogue");
                            break;
                        }
                    }
                    else
                    {
                        // Animation has transitioned out of interact state
                        Debug.Log("[DialogueCameraController] Player animation completed, starting dialogue");
                        break;
                    }
                    
                    yield return null;
                }
                
                if (Time.time - waitStartTime >= maxAnimationWaitTime)
                {
                    Debug.LogWarning("[DialogueCameraController] Animation wait timed out, starting dialogue anyway");
                }
            }
            else
            {
                Debug.Log("[DialogueCameraController] No interaction animation detected, starting dialogue immediately");
            }
        }
        else
        {
            Debug.Log("[DialogueCameraController] No player animator found, starting dialogue immediately");
        }
        
        // Now start the dialogue mode
        StartDialogueMode();
    }
    
    private void OnDialogueEnd()
    {
        EndDialogueMode();
    }
    
    private void FindCurrentNPC()
    {
        // Find the closest NPC within interaction range
        NPCBehavior[] npcs = FindObjectsOfType<NPCBehavior>();
        NPCBehavior closestNPC = null;
        float closestDistance = float.MaxValue;
        
        if (playerTransform == null) return;
        
        foreach (NPCBehavior npc in npcs)
        {
            float distance = Vector3.Distance(playerTransform.position, npc.transform.position);
            if (distance < 5f && distance < closestDistance) // 5f is a reasonable interaction range
            {
                closestDistance = distance;
                closestNPC = npc;
            }
        }
        
        currentNPCTarget = closestNPC?.transform;
    }
    
    private void StartDialogueMode()
    {
        if (isInDialogueMode || currentNPCTarget == null) return;
        
        Debug.Log("[DialogueCameraController] Starting dialogue mode");
        
        // Store original camera RIG state (not just the camera)
        if (cameraRig != null)
        {
            originalPosition = cameraRig.position;
            originalRotation = cameraRig.rotation;
            Debug.Log($"[DialogueCameraController] Stored original rig position: {originalPosition}, rotation: {originalRotation.eulerAngles}");
        }
        else
        {
            // Fallback to camera position
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            Debug.Log($"[DialogueCameraController] Stored original camera position: {originalPosition}, rotation: {originalRotation.eulerAngles}");
        }
        
        isInDialogueMode = true;
        
        // Disable third person camera controls AFTER storing state
        if (thirdPersonCamera)
        {
            thirdPersonCamera.enabled = false;
            Debug.Log("[DialogueCameraController] Disabled third person camera");
        }
        
        // Calculate dialogue camera position (for the actual camera, not the rig)
        CalculateDialoguePosition();
        
        // Start transition
        isTransitioning = true;
        
        // Disable player movement during dialogue
        SetPlayerMovementEnabled(false);
        
        Debug.Log("[DialogueCameraController] Dialogue mode started, transitioning to dialogue position");
    }
    
    private void EndDialogueMode()
    {
        if (!isInDialogueMode) return;
        
        Debug.Log("[DialogueCameraController] Starting to end dialogue mode");
        
        // If we have a camera rig, restore its position/rotation
        if (cameraRig != null)
        {
            targetPosition = originalPosition;
            targetRotation = originalRotation;
            Debug.Log($"[DialogueCameraController] Restoring to rig position: {targetPosition}, rotation: {targetRotation.eulerAngles}");
        }
        else
        {
            // Fallback to camera position
            targetPosition = originalPosition;
            targetRotation = originalRotation;
            Debug.Log($"[DialogueCameraController] Restoring to camera position: {targetPosition}, rotation: {targetRotation.eulerAngles}");
        }
        
        isTransitioning = true; // Make sure we're transitioning back
        
        // Re-enable player movement immediately (player can move while camera transitions back)
        SetPlayerMovementEnabled(true);
        
        // Start coroutine to handle the transition and re-enable third person camera
        StartCoroutine(TransitionBackToThirdPersonCamera());
        
        currentNPCTarget = null;
        
        Debug.Log("[DialogueCameraController] Dialogue mode ending, transitioning back to third person camera");
    }
    
    private IEnumerator TransitionBackToThirdPersonCamera()
    {
        Debug.Log("[DialogueCameraController] Starting transition back to third person camera");
        
        // Wait for transition to complete with a timeout to prevent infinite waiting
        float timeout = 5f; // 5 second timeout
        float elapsedTime = 0f;
        
        while (isTransitioning && elapsedTime < timeout)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (elapsedTime >= timeout)
        {
            Debug.LogWarning("[DialogueCameraController] Camera transition timed out, forcing completion");
            isTransitioning = false;
            
            // Force restore position based on what we're controlling
            if (cameraRig != null)
            {
                cameraRig.position = targetPosition;
                cameraRig.rotation = targetRotation;
            }
            else
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
            }
        }
        
        // Re-enable third person camera
        if (thirdPersonCamera)
        {
            thirdPersonCamera.enabled = true;
            Debug.Log("[DialogueCameraController] Third person camera re-enabled");
        }
        
        // Mark dialogue mode as fully ended
        isInDialogueMode = false;
        
        Debug.Log("[DialogueCameraController] Successfully transitioned back to third person camera");
    }
    
    private void CalculateDialoguePosition()
    {
        if (currentNPCTarget == null || playerTransform == null) return;
        
        // Get the center point between player and NPC at dialogue height
        Vector3 playerPos = playerTransform.position + Vector3.up * standardDialogueHeight;
        Vector3 npcPos = currentNPCTarget.position + Vector3.up * standardDialogueHeight;
        
        // Focus point slightly favoring the NPC
        Vector3 focusPoint = Vector3.Lerp(playerPos, npcPos, 0.5f + npcFocusOffset);
        
        // Find the best camera position with obstruction avoidance
        Vector3 bestCameraPos = FindBestCameraPosition(playerPos, npcPos, focusPoint);
        
        targetPosition = bestCameraPos;
        
        // Always look at the focus point with the specified angle
        Vector3 lookDirection = (focusPoint - targetPosition).normalized;
        targetRotation = Quaternion.LookRotation(lookDirection);
        
        // Apply the focus angle (slight downward tilt)
        targetRotation *= Quaternion.Euler(focusAngle, 0, 0);
        
        Debug.Log($"[DialogueCameraController] Calculated dialogue position: {targetPosition}, looking at: {focusPoint}");
    }
    
    private Vector3 FindBestCameraPosition(Vector3 playerPos, Vector3 npcPos, Vector3 focusPoint)
    {
        // Calculate the direction between player and NPC
        Vector3 playerToNpc = (npcPos - playerPos).normalized;
        
        // Create a perpendicular direction (to the side)
        Vector3 sideDirection = Vector3.Cross(playerToNpc, Vector3.up).normalized;
        
        // Try different positions around the conversation
        Vector3[] candidatePositions = new Vector3[positionAttempts];
        
        for (int i = 0; i < positionAttempts; i++)
        {
            // Calculate angle around the conversation (360 degrees / attempts)
            float angle = (360f / positionAttempts) * i;
            float radians = angle * Mathf.Deg2Rad;
            
            // Create position at standard distance using the angle
            Vector3 offset = new Vector3(
                Mathf.Cos(radians) * standardDialogueDistance,
                0f,
                Mathf.Sin(radians) * standardDialogueDistance
            );
            
            candidatePositions[i] = focusPoint + offset;
        }
        
        // Find the best position (no obstructions and good angle)
        Vector3 bestPosition = candidatePositions[0]; // Fallback
        float bestScore = -1f;
        
        foreach (Vector3 candidatePos in candidatePositions)
        {
            float score = EvaluateCameraPosition(candidatePos, playerPos, npcPos, focusPoint);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestPosition = candidatePos;
            }
        }
        
        Debug.Log($"[DialogueCameraController] Selected camera position with score: {bestScore}");
        return bestPosition;
    }
    
    private float EvaluateCameraPosition(Vector3 cameraPos, Vector3 playerPos, Vector3 npcPos, Vector3 focusPoint)
    {
        float score = 1f; // Start with perfect score
        
        // Check for obstructions between camera and focus point
        if (Physics.CheckSphere(cameraPos, collisionCheckRadius, obstructionLayers))
        {
            score -= 0.8f; // Heavy penalty for camera being inside an obstruction
        }
        
        // Check line of sight to both player and NPC
        if (Physics.Linecast(cameraPos, playerPos, obstructionLayers))
        {
            score -= 0.3f; // Penalty for player obstruction
        }
        
        if (Physics.Linecast(cameraPos, npcPos, obstructionLayers))
        {
            score -= 0.3f; // Penalty for NPC obstruction
        }
        
        // Check line of sight to focus point
        if (Physics.Linecast(cameraPos, focusPoint, obstructionLayers))
        {
            score -= 0.4f; // Penalty for focus point obstruction
        }
        
        // Prefer positions that are not too close to the ground
        if (cameraPos.y < playerPos.y - 0.5f)
        {
            score -= 0.2f; // Penalty for being too low
        }
        
        // Bonus for good conversation angles (side-by-side view)
        Vector3 toPlayer = (playerPos - cameraPos).normalized;
        Vector3 toNpc = (npcPos - cameraPos).normalized;
        float angleBetween = Vector3.Angle(toPlayer, toNpc);
        
        // Prefer wider angles for natural conversation view (45-90 degrees)
        if (angleBetween >= 45f && angleBetween <= 90f)
        {
            score += 0.3f; // Higher bonus for good side-by-side angles
        }
        else if (angleBetween >= 30f && angleBetween <= 120f)
        {
            score += 0.1f; // Small bonus for acceptable angles
        }
        
        // Bonus for being positioned to the side rather than directly behind/in front
        Vector3 playerToNpc = (npcPos - playerPos).normalized;
        Vector3 cameraDirection = (cameraPos - focusPoint).normalized;
        float sideAngle = Vector3.Dot(playerToNpc, cameraDirection);
        
        // Prefer camera positions that are more to the side (perpendicular to player-NPC line)
        if (Mathf.Abs(sideAngle) < 0.5f) // More perpendicular = better side view
        {
            score += 0.2f; // Bonus for side positioning
        }
        
        return Mathf.Clamp01(score); // Keep score between 0 and 1
    }
    
    private void HandleCameraTransition()
    {
        if (!isTransitioning) return;
        
        // Determine which transform to manipulate
        Transform targetTransform = cameraRig != null ? cameraRig : transform;
        
        // Smoothly move to target position and rotation
        targetTransform.position = Vector3.Lerp(targetTransform.position, targetPosition, transitionSpeed * Time.deltaTime);
        targetTransform.rotation = Quaternion.Lerp(targetTransform.rotation, targetRotation, transitionSpeed * Time.deltaTime);
        
        // Check if transition is complete
        float positionDifference = Vector3.Distance(targetTransform.position, targetPosition);
        float rotationDifference = Quaternion.Angle(targetTransform.rotation, targetRotation);
        
        // More lenient thresholds for smoother completion
        if (positionDifference < 0.05f && rotationDifference < 0.5f)
        {
            // Snap to final position/rotation
            targetTransform.position = targetPosition;
            targetTransform.rotation = targetRotation;
            isTransitioning = false;
            
            string transitionType = isInDialogueMode ? "dialogue position" : "original position";
            Debug.Log($"[DialogueCameraController] Camera transition to {transitionType} completed. Position: {targetTransform.position}, Rotation: {targetTransform.rotation.eulerAngles}");
        }
    }
    
    private void SetPlayerMovementEnabled(bool enabled)
    {
        // Disable/enable player movement during dialogue
        PlayerController playerController = playerTransform?.GetComponent<PlayerController>();
        if (playerController)
        {
            playerController.enabled = enabled;
        }
        
        // Handle cursor visibility and lock state for dialogue
        if (!enabled) // Disabling player movement (starting dialogue)
        {
            // Unlock and show cursor for dialogue UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Stop player physics movement
            Rigidbody playerRb = playerTransform?.GetComponent<Rigidbody>();
            if (playerRb)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.isKinematic = true;
            }
            
            Debug.Log("[DialogueCameraController] Enabled cursor for dialogue interaction");
        }
        else // Re-enabling player movement (ending dialogue)
        {
            // Restore cursor to locked state for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Re-enable player physics
            Rigidbody playerRb = playerTransform?.GetComponent<Rigidbody>();
            if (playerRb)
            {
                playerRb.isKinematic = false;
            }
            
            Debug.Log("[DialogueCameraController] Restored cursor lock for gameplay");
        }
    }
    
    private void ForceResetToNormalMode()
    {
        Debug.Log("[DialogueCameraController] Force resetting to normal camera mode");
        
        isInDialogueMode = false;
        isTransitioning = false;
        
        // Re-enable third person camera
        if (thirdPersonCamera)
        {
            thirdPersonCamera.enabled = true;
        }
        
        // Re-enable player movement
        SetPlayerMovementEnabled(true);
        
        currentNPCTarget = null;
        
        Debug.Log("[DialogueCameraController] Force reset completed");
    }
    
    void OnDrawGizmos()
    {
        if (isInDialogueMode && currentNPCTarget != null && playerTransform != null)
        {
            Vector3 playerPos = playerTransform.position + Vector3.up * standardDialogueHeight;
            Vector3 npcPos = currentNPCTarget.position + Vector3.up * standardDialogueHeight;
            Vector3 focusPoint = Vector3.Lerp(playerPos, npcPos, 0.5f + npcFocusOffset);
            
            // Draw the subjects
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(playerPos, 0.3f); // Player
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(npcPos, 0.3f); // NPC
            
            // Draw focus point
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(focusPoint, 0.2f);
            
            // Draw current camera position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
            
            // Draw line from camera to focus point
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, focusPoint);
            
            // Draw candidate positions (if in dialogue mode)
            Gizmos.color = Color.cyan;
            for (int i = 0; i < positionAttempts; i++)
            {
                float angle = (360f / positionAttempts) * i;
                float radians = angle * Mathf.Deg2Rad;
                
                Vector3 offset = new Vector3(
                    Mathf.Cos(radians) * standardDialogueDistance,
                    0f,
                    Mathf.Sin(radians) * standardDialogueDistance
                );
                
                Vector3 candidatePos = focusPoint + offset;
                Gizmos.DrawWireCube(candidatePos, Vector3.one * 0.1f);
            }
            
            // Draw dialogue area
            Gizmos.color = Color.gray;
            DrawWireCircle(focusPoint, standardDialogueDistance);
        }
    }
    
    // Helper method for drawing circles in Gizmos
    private void DrawWireCircle(Vector3 center, float radius)
    {
        int segments = 32;
        Vector3 prevPoint = center + Vector3.forward * radius;
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
} 