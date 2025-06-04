using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class IntroCutscene : MonoBehaviour
{
    [SerializeField] private GameObject frame1;
    [SerializeField] private GameObject frame2;
    [SerializeField] private GameObject frame3;
    
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("Text Display")]
    [SerializeField] private TMP_Text cutsceneText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text continuePromptText;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float flashSpeed = 0.8f;
    
    [Header("Frame 1 Messages")]
    [SerializeField] private string frame1Name = "Narrator";
    [SerializeField] private string frame1Message = "Welcome to our world...";
    
    [Header("Frame 2 Messages")]
    [SerializeField] private string frame2Name = "Narrator";
    [SerializeField] private string frame2Message = "A place of mystery and wonder...";
    
    [Header("Frame 3 Messages")]
    [SerializeField] private string frame3Name = "Narrator";
    [SerializeField] private string frame3Message = "Your journey begins now.";
    
    private CanvasGroup frame1CanvasGroup;
    private CanvasGroup frame2CanvasGroup;
    private CanvasGroup frame3CanvasGroup;
    
    private int currentFrame = 1;
    private bool isTransitioning = false;
    private Coroutine typingCoroutine;
    private Coroutine flashingCoroutine;
    private bool isTyping = false;
    private bool waitingForInput = false;

    void Start()
    {
        // Get or add CanvasGroup components
        frame1CanvasGroup = GetOrAddCanvasGroup(frame1);
        frame2CanvasGroup = GetOrAddCanvasGroup(frame2);
        frame3CanvasGroup = GetOrAddCanvasGroup(frame3);
        
        // Initialize visibility
        frame1.SetActive(true);
        frame2.SetActive(true);
        frame3.SetActive(true);
        
        frame1CanvasGroup.alpha = 1f;
        frame2CanvasGroup.alpha = 0f;
        frame3CanvasGroup.alpha = 0f;
        
        // Hide continue prompt initially
        if (continuePromptText != null)
        {
            continuePromptText.gameObject.SetActive(false);
        }
        
        // Show first frame message immediately
        ShowText(frame1Name, frame1Message);
    }
    
    private CanvasGroup GetOrAddCanvasGroup(GameObject obj)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }
    
    private void ShowText(string name, string message)
    {
        if (cutsceneText == null)
        {
            Debug.LogWarning("Cutscene text component not assigned!");
            return;
        }
        
        // Set name text
        if (nameText != null)
        {
            nameText.text = name;
        }
        
        // Hide continue prompt while typing
        HideContinuePrompt();
        
        // Stop any current typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // Start typewriter effect
        waitingForInput = false;
        typingCoroutine = StartCoroutine(TypewriterEffect(message));
    }
    
    private IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        cutsceneText.text = "";
        
        for (int i = 0; i <= text.Length; i++)
        {
            cutsceneText.text = text.Substring(0, i);
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
        waitingForInput = true;
        
        // Show continue prompt when text is complete
        ShowContinuePrompt();
    }
    
    private void CompleteText(string fullText)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        cutsceneText.text = fullText;
        isTyping = false;
        waitingForInput = true;
        
        // Show continue prompt when text is completed early
        ShowContinuePrompt();
    }
    
    private void ShowContinuePrompt()
    {
        if (continuePromptText != null)
        {
            continuePromptText.text = "Press SPACE to continue";
            continuePromptText.gameObject.SetActive(true);
            
            // Start flashing effect
            if (flashingCoroutine != null)
            {
                StopCoroutine(flashingCoroutine);
            }
            flashingCoroutine = StartCoroutine(FlashContinuePrompt());
        }
    }
    
    private void HideContinuePrompt()
    {
        if (continuePromptText != null)
        {
            continuePromptText.gameObject.SetActive(false);
        }
        
        // Stop flashing coroutine
        if (flashingCoroutine != null)
        {
            StopCoroutine(flashingCoroutine);
            flashingCoroutine = null;
        }
    }
    
    private IEnumerator FlashContinuePrompt()
    {
        while (waitingForInput && continuePromptText != null)
        {
            continuePromptText.alpha = 1f;
            yield return new WaitForSeconds(flashSpeed);
            continuePromptText.alpha = 0.3f;
            yield return new WaitForSeconds(flashSpeed);
        }
    }

    void Update()
    {
        if (isTransitioning) return;
        
        // Handle space key input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpaceInput();
        }
    }
    
    private void HandleSpaceInput()
    {
        if (isTyping)
        {
            // Complete the current text
            string currentMessage = GetCurrentFrameMessage();
            CompleteText(currentMessage);
        }
        else if (waitingForInput)
        {
            // Move to next frame
            AdvanceToNextFrame();
        }
    }
    
    private string GetCurrentFrameMessage()
    {
        switch (currentFrame)
        {
            case 1: return frame1Message;
            case 2: return frame2Message;
            case 3: return frame3Message;
            default: return "";
        }
    }
    
    private void AdvanceToNextFrame()
    {
        // Hide continue prompt when advancing
        HideContinuePrompt();
        
        switch (currentFrame)
        {
            case 1:
                StartCoroutine(FadeTransition(frame1CanvasGroup, frame2CanvasGroup, 2, frame2Name, frame2Message));
                break;
                
            case 2:
                StartCoroutine(FadeTransition(frame2CanvasGroup, frame3CanvasGroup, 3, frame3Name, frame3Message));
                break;
                
            case 3:
                StartCoroutine(FadeOut(frame3CanvasGroup));
                break;
        }
    }
    
    private IEnumerator FadeTransition(CanvasGroup fromFrame, CanvasGroup toFrame, int nextFrame, string name, string message)
    {
        isTransitioning = true;
        waitingForInput = false;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            
            fromFrame.alpha = Mathf.Lerp(1f, 0f, t);
            toFrame.alpha = Mathf.Lerp(0f, 1f, t);
            
            yield return null;
        }
        
        fromFrame.alpha = 0f;
        toFrame.alpha = 1f;
        
        currentFrame = nextFrame;
        isTransitioning = false;
        
        // Show message for the new frame after transition completes
        ShowText(name, message);
    }
    
    private IEnumerator FadeOut(CanvasGroup frame)
    {
        isTransitioning = true;
        waitingForInput = false;
        float elapsedTime = 0f;
        
        // Clear text before final fade
        if (cutsceneText != null)
        {
            cutsceneText.text = "";
        }
        if (nameText != null)
        {
            nameText.text = "";
        }
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            
            frame.alpha = Mathf.Lerp(1f, 0f, t);
            
            yield return null;
        }
        
        frame.alpha = 0f;
        currentFrame = 4; // Prevents further updates
        isTransitioning = false;
        
        // Load next scene
        SceneManager.LoadScene("Level");
    }
}
