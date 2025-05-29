using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Specialized hover controller that integrates with the existing DebugTimeUI system.
/// This shows/hides the entire DebugTimeUI panel when hovering over a UI element.
/// </summary>
public class DebugTimeUIHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Debug Time UI Settings")]
    [SerializeField]
    [Tooltip("The DebugTimeUI component to show/hide on hover")]
    private DebugTimeUI debugTimeUI;

    [SerializeField]
    [Tooltip("Alternative: Find DebugTimeUI by GameObject name")]
    private string debugTimeUIObjectName = "DebugTimeUI";

    [Header("Hover Behavior")]
    [SerializeField]
    [Tooltip("Show the time UI when hovering")]
    private bool showOnHover = true;

    [SerializeField]
    [Tooltip("Delay before showing the UI (in seconds)")]
    private float showDelay = 0.2f;

    [SerializeField]
    [Tooltip("Delay before hiding the UI (in seconds)")]
    private float hideDelay = 0.5f;

    [Header("UI State Management")]
    [SerializeField]
    [Tooltip("Keep the UI visible if it was already active")]
    private bool respectInitialState = true;

    private bool wasInitiallyActive;
    private Coroutine currentCoroutine;
    private GameObject timeUIGameObject;

    void Start()
    {
        // Find the DebugTimeUI component if not assigned
        if (debugTimeUI == null)
        {
            if (!string.IsNullOrEmpty(debugTimeUIObjectName))
            {
                var foundObject = GameObject.Find(debugTimeUIObjectName);
                if (foundObject != null)
                {
                    debugTimeUI = foundObject.GetComponent<DebugTimeUI>();
                }
            }

            // If still not found, try to find any DebugTimeUI in the scene
            if (debugTimeUI == null)
            {
                debugTimeUI = FindObjectOfType<DebugTimeUI>();
            }
        }

        if (debugTimeUI != null)
        {
            timeUIGameObject = debugTimeUI.gameObject;
            wasInitiallyActive = timeUIGameObject.activeInHierarchy;

            // Hide the UI initially if showOnHover is true and it should start hidden
            if (showOnHover && !respectInitialState)
            {
                timeUIGameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("DebugTimeUIHoverController: Could not find DebugTimeUI component. " +
                           "Please assign it manually or ensure it exists in the scene.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (debugTimeUI == null || timeUIGameObject == null) return;

        // Stop any existing coroutine
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        if (showOnHover)
        {
            currentCoroutine = StartCoroutine(ShowUICoroutine());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (debugTimeUI == null || timeUIGameObject == null) return;

        // Stop any existing coroutine
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        if (showOnHover)
        {
            // Only hide if it wasn't initially active or if we don't respect initial state
            if (!respectInitialState || !wasInitiallyActive)
            {
                currentCoroutine = StartCoroutine(HideUICoroutine());
            }
        }
    }

    private System.Collections.IEnumerator ShowUICoroutine()
    {
        if (showDelay > 0)
        {
            yield return new WaitForSeconds(showDelay);
        }

        if (timeUIGameObject != null)
        {
            timeUIGameObject.SetActive(true);
            Debug.Log("Debug Time UI shown on hover");
        }
    }

    private System.Collections.IEnumerator HideUICoroutine()
    {
        if (hideDelay > 0)
        {
            yield return new WaitForSeconds(hideDelay);
        }

        if (timeUIGameObject != null)
        {
            timeUIGameObject.SetActive(false);
            Debug.Log("Debug Time UI hidden after hover exit");
        }
    }

    /// <summary>
    /// Manually set the DebugTimeUI component
    /// </summary>
    public void SetDebugTimeUI(DebugTimeUI timeUI)
    {
        debugTimeUI = timeUI;
        if (timeUI != null)
        {
            timeUIGameObject = timeUI.gameObject;
            wasInitiallyActive = timeUIGameObject.activeInHierarchy;
        }
    }

    /// <summary>
    /// Get the current DebugTimeUI component
    /// </summary>
    public DebugTimeUI GetDebugTimeUI()
    {
        return debugTimeUI;
    }

    /// <summary>
    /// Manually show the time UI
    /// </summary>
    public void ShowTimeUI()
    {
        if (timeUIGameObject != null)
        {
            timeUIGameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Manually hide the time UI
    /// </summary>
    public void HideTimeUI()
    {
        if (timeUIGameObject != null)
        {
            timeUIGameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Toggle the time UI visibility
    /// </summary>
    public void ToggleTimeUI()
    {
        if (timeUIGameObject != null)
        {
            timeUIGameObject.SetActive(!timeUIGameObject.activeInHierarchy);
        }
    }

    void OnValidate()
    {
        // Validation in editor
        if (debugTimeUI == null && !string.IsNullOrEmpty(debugTimeUIObjectName))
        {
            var foundObject = GameObject.Find(debugTimeUIObjectName);
            if (foundObject != null)
            {
                debugTimeUI = foundObject.GetComponent<DebugTimeUI>();
            }
        }
    }
} 