using UnityEngine;

/// <summary>
/// Controls mouse cursor visibility and lock state based on key input.
/// Pressing Tab or Esc will toggle the cursor between locked/hidden and visible/free.
/// </summary>
public class CursorController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField]
    [Tooltip("Key to toggle cursor visibility (default: Tab)")]
    private KeyCode toggleKey1 = KeyCode.Tab;

    [SerializeField]
    [Tooltip("Alternative key to toggle cursor visibility (default: Escape)")]
    private KeyCode toggleKey2 = KeyCode.Escape;

    [Header("Cursor Behavior")]
    [SerializeField]
    [Tooltip("Should the cursor start locked and hidden?")]
    private bool startLocked = true;

    [SerializeField]
    [Tooltip("Cursor lock mode when hidden (Locked = center of screen, Confined = within game window)")]
    private CursorLockMode hiddenLockMode = CursorLockMode.Locked;

    [SerializeField]
    [Tooltip("Cursor lock mode when visible (None = free movement)")]
    private CursorLockMode visibleLockMode = CursorLockMode.None;

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Log cursor state changes to console")]
    private bool debugMode = false;

    // Current state
    private bool isCursorVisible;

    // Events for other scripts to listen to
    public System.Action<bool> OnCursorStateChanged;

    void Start()
    {
        // Set initial cursor state
        if (startLocked)
        {
            SetCursorState(false);
        }
        else
        {
            SetCursorState(true);
        }
    }

    void Update()
    {
        // Check for toggle input
        if (Input.GetKeyDown(toggleKey1) || Input.GetKeyDown(toggleKey2))
        {
            ToggleCursor();
        }
    }

    /// <summary>
    /// Toggle cursor between visible and hidden states
    /// </summary>
    public void ToggleCursor()
    {
        SetCursorState(!isCursorVisible);
    }

    /// <summary>
    /// Set cursor visibility and lock state
    /// </summary>
    /// <param name="visible">True to show cursor, false to hide</param>
    public void SetCursorState(bool visible)
    {
        isCursorVisible = visible;

        if (visible)
        {
            // Show cursor and allow free movement
            Cursor.visible = true;
            Cursor.lockState = visibleLockMode;
            
            if (debugMode)
                Debug.Log("Cursor: Visible and unlocked");
        }
        else
        {
            // Hide cursor and lock to center or confine to window
            Cursor.visible = false;
            Cursor.lockState = hiddenLockMode;
            
            if (debugMode)
                Debug.Log($"Cursor: Hidden and {hiddenLockMode}");
        }

        // Notify other scripts of the state change
        OnCursorStateChanged?.Invoke(isCursorVisible);
    }

    /// <summary>
    /// Force cursor to be visible (useful for UI interactions)
    /// </summary>
    public void ShowCursor()
    {
        SetCursorState(true);
    }

    /// <summary>
    /// Force cursor to be hidden (useful for gameplay)
    /// </summary>
    public void HideCursor()
    {
        SetCursorState(false);
    }

    /// <summary>
    /// Get current cursor visibility state
    /// </summary>
    public bool IsCursorVisible()
    {
        return isCursorVisible;
    }

    /// <summary>
    /// Temporarily show cursor for a specified duration
    /// </summary>
    /// <param name="duration">How long to show cursor in seconds</param>
    public void ShowCursorTemporarily(float duration)
    {
        if (!isCursorVisible)
        {
            ShowCursor();
            StartCoroutine(HideCursorAfterDelay(duration));
        }
    }

    private System.Collections.IEnumerator HideCursorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideCursor();
    }

    // Handle application focus to ensure cursor behaves correctly
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !isCursorVisible)
        {
            // Reapply hidden state when regaining focus
            Cursor.visible = false;
            Cursor.lockState = hiddenLockMode;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && !isCursorVisible)
        {
            // Reapply hidden state when unpausing
            Cursor.visible = false;
            Cursor.lockState = hiddenLockMode;
        }
    }

    // Editor-only validation
    void OnValidate()
    {
        // Ensure the lock modes make sense
        if (visibleLockMode == CursorLockMode.Locked)
        {
            Debug.LogWarning("CursorController: Visible lock mode is set to Locked, which may not be desired for UI interaction.");
        }
    }
} 