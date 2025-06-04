using UnityEngine;
using System.Collections;

public class LevelStartCutscene : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private float cutsceneDuration = 3f;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private bool autoStart = true;
    
    [Header("Player Movement")]
    [SerializeField] private Vector3 playerStartPosition = Vector3.zero;
    [SerializeField] private bool rotatePlayerToMoveDirection = true;
    
    [Header("Dialogue")]
    [SerializeField] private string[] dialogueLines = {
        "Welcome to your new adventure...",
        "Take your time to explore and discover.",
        "The world awaits your choices."
    };
    [SerializeField] private float dialogueDisplayTime = 2f;
    [SerializeField] private bool showDialogueSequentially = true;
    
    [Header("Fade Settings")]
    [SerializeField] private bool startWithFadeIn = true;
    [SerializeField] private bool endWithFadeOut = false;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Color fadeColor = Color.black;
    
    // References
    private PlayerController playerController;
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;
    
    // State tracking
    private bool cutsceneActive = false;
    private bool cutsceneCompleted = false;
    
    // Movement tracking
    private Vector3 moveDirection;
    
    // Fade overlay
    private GameObject fadeOverlay;
    private UnityEngine.UI.Image fadeImage;
    
    void Start()
    {
        if (autoStart)
        {
            StartCoroutine(InitializeCutscene());
        }
    }
    
    private IEnumerator InitializeCutscene()
    {
        // Wait a frame to ensure all components are initialized
        yield return null;
        
        // Find required components
        FindComponents();
        
        if (playerController == null)
        {
            Debug.LogError("[LevelStartCutscene] PlayerController not found! Cannot start cutscene.");
            yield break;
        }
        
        // Setup fade overlay if needed
        if (startWithFadeIn)
        {
            SetupFadeOverlay();
            SetFadeAlpha(1f);
        }
        
        // Start the cutscene
        StartCutscene();
    }
    
    public void StartCutscene()
    {
        if (cutsceneActive || cutsceneCompleted)
        {
            return;
        }
        
        cutsceneActive = true;
        StartCoroutine(PlayCutscene());
    }
    
    private void FindComponents()
    {
        // Find player controller
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("[LevelStartCutscene] No PlayerController found in scene!");
            return;
        }
        
        // Get player components
        playerRigidbody = playerController.GetComponent<Rigidbody>();
        playerAnimator = playerController.GetComponent<Animator>();
    }
    
    private IEnumerator PlayCutscene()
    {
        Debug.Log("[LevelStartCutscene] Starting level opening cutscene");
        
        // Disable player input (but leave camera alone)
        DisablePlayerControl();
        
        // Set initial positions and setup movement
        SetupInitialPositions();
        
        // Start movement and dialogue immediately
        Coroutine movementCoroutine = StartCoroutine(PlayMovement());
        Coroutine dialogueCoroutine = StartCoroutine(PlayDialogue());
        
        // Start fade in simultaneously with movement (if enabled)
        if (startWithFadeIn)
        {
            StartCoroutine(FadeIn());
        }
        
        // Wait for movement and dialogue to complete
        yield return movementCoroutine;
        yield return dialogueCoroutine;
        
        // Fade out if enabled
        if (endWithFadeOut)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // Wait a moment before returning control
        yield return new WaitForSeconds(0.5f);
        
        // Return control to player
        RestorePlayerControl();
        
        cutsceneActive = false;
        cutsceneCompleted = true;
        
        Debug.Log("[LevelStartCutscene] Cutscene completed, control returned to player");
        
        // Clean up fade overlay
        if (fadeOverlay != null)
        {
            Destroy(fadeOverlay);
        }
    }
    
    private void SetupInitialPositions()
    {
        // Set player position if specified
        if (playerStartPosition != Vector3.zero)
        {
            playerController.transform.position = playerStartPosition;
        }
        
        // Store movement direction
        moveDirection = playerController.transform.forward;
        
        // Rotate player to face movement direction if enabled
        if (rotatePlayerToMoveDirection)
        {
            playerController.transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
    
    private IEnumerator PlayMovement()
    {
        // Set walking animation
        if (playerAnimator != null)
        {
            int speedHash = Animator.StringToHash("Speed");
            playerAnimator.SetFloat(speedHash, 0.5f); // Set to walking speed
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < cutsceneDuration)
        {
            // Move player forward continuously
            Vector3 movement = moveDirection * walkSpeed * Time.deltaTime;
            playerController.transform.position += movement;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Stop walking animation
        if (playerAnimator != null)
        {
            int speedHash = Animator.StringToHash("Speed");
            playerAnimator.SetFloat(speedHash, 0f);
        }
    }
    
    private IEnumerator PlayDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            yield break;
        }
        
        if (showDialogueSequentially)
        {
            // Show each dialogue line one after another
            foreach (string line in dialogueLines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    DialogueManager.Show(line);
                    yield return new WaitForSeconds(dialogueDisplayTime);
                }
            }
        }
        else
        {
            // Show all dialogue lines as one continuous sequence
            string fullDialogue = string.Join(" ", dialogueLines);
            DialogueManager.Show(fullDialogue);
            yield return new WaitForSeconds(dialogueDisplayTime * dialogueLines.Length);
        }
        
        // Hide dialogue when done
        DialogueManager.Hide();
    }
    
    private void DisablePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Stop any existing velocity
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }
    
    private void RestorePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }
    
    private void SetupFadeOverlay()
    {
        // Create canvas for fade overlay
        GameObject canvasGO = new GameObject("CutsceneFadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Ensure it's on top
        
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create fade overlay image
        fadeOverlay = new GameObject("FadeOverlay");
        fadeOverlay.transform.SetParent(canvasGO.transform, false);
        
        fadeImage = fadeOverlay.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = fadeColor;
        
        // Make it cover the entire screen
        RectTransform rectTransform = fadeOverlay.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }
    
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            float normalizedTime = elapsedTime / fadeDuration;
            // Use smoothstep for a more gradual fade that feels slower
            float smoothTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            float alpha = Mathf.Lerp(1f, 0f, smoothTime);
            SetFadeAlpha(alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        SetFadeAlpha(0f);
    }
    
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            SetFadeAlpha(alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        SetFadeAlpha(1f);
    }
    
    // Public methods for manual control
    public bool IsCutsceneActive()
    {
        return cutsceneActive;
    }
    
    public bool IsCutsceneCompleted()
    {
        return cutsceneCompleted;
    }
    
    public void SkipCutscene()
    {
        if (cutsceneActive && !cutsceneCompleted)
        {
            StopAllCoroutines();
            
            // Hide dialogue
            DialogueManager.Hide();
            
            // Restore control
            RestorePlayerControl();
            
            cutsceneActive = false;
            cutsceneCompleted = true;
            
            // Clean up fade overlay
            if (fadeOverlay != null)
            {
                Destroy(fadeOverlay.transform.parent.gameObject); // Destroy the canvas
            }
            
            Debug.Log("[LevelStartCutscene] Cutscene skipped");
        }
    }
    
    // Editor helper methods
    void OnDrawGizmosSelected()
    {
        // Get consistent player position for all gizmo drawing
        Vector3 playerPos;
        if (Application.isPlaying && playerController != null)
        {
            // Use actual player position when playing
            playerPos = playerController.transform.position;
        }
        else if (playerStartPosition != Vector3.zero)
        {
            // Use specified start position when not playing
            playerPos = playerStartPosition;
        }
        else
        {
            // Fall back to this object's position
            playerPos = transform.position;
        }
        
        // Draw player forward direction and movement path
        if (Application.isPlaying && cutsceneActive)
        {
            // Draw current movement
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerPos, moveDirection * 2f);
        }
        else
        {
            // Draw estimated path from current position
            Vector3 forwardDir = transform.forward;
            
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerPos, forwardDir * (walkSpeed * cutsceneDuration));
            Gizmos.DrawWireSphere(playerPos, 0.3f);
        }
    }
} 