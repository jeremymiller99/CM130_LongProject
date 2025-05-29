using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the activation/deactivation of a time menu game object based on mouse hover over a UI panel.
/// Attach this script to the UI panel that should detect mouse hover events.
/// </summary>
public class TimeMenuHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Time Menu Settings")]
    [SerializeField] 
    [Tooltip("The time menu game object to activate/deactivate on hover")]
    private GameObject timeMenuGameObject;

    [SerializeField]
    [Tooltip("Find time menu by name if not assigned directly")]
    private string timeMenuName = "time menu";

    [Header("Optional Settings")]
    [SerializeField]
    [Tooltip("Delay before activating the time menu (in seconds)")]
    private float activationDelay = 0f;

    [SerializeField]
    [Tooltip("Delay before deactivating the time menu (in seconds)")]
    private float deactivationDelay = 0f;

    private Coroutine currentHoverCoroutine;

    void Start()
    {
        // If timeMenuGameObject is not assigned, try to find it by name
        if (timeMenuGameObject == null && !string.IsNullOrEmpty(timeMenuName))
        {
            timeMenuGameObject = GameObject.Find(timeMenuName);
            
            if (timeMenuGameObject == null)
            {
                Debug.LogWarning($"TimeMenuHoverController: Could not find game object named '{timeMenuName}'. Please assign the time menu game object manually.");
            }
        }

        // Ensure the time menu starts as inactive
        if (timeMenuGameObject != null)
        {
            timeMenuGameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called when the pointer enters the UI panel
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (timeMenuGameObject == null) return;

        // Stop any existing coroutine
        if (currentHoverCoroutine != null)
        {
            StopCoroutine(currentHoverCoroutine);
        }

        // Start activation coroutine
        currentHoverCoroutine = StartCoroutine(ActivateTimeMenuCoroutine());
    }

    /// <summary>
    /// Called when the pointer exits the UI panel
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (timeMenuGameObject == null) return;

        // Stop any existing coroutine
        if (currentHoverCoroutine != null)
        {
            StopCoroutine(currentHoverCoroutine);
        }

        // Start deactivation coroutine
        currentHoverCoroutine = StartCoroutine(DeactivateTimeMenuCoroutine());
    }

    private System.Collections.IEnumerator ActivateTimeMenuCoroutine()
    {
        if (activationDelay > 0)
        {
            yield return new WaitForSeconds(activationDelay);
        }

        if (timeMenuGameObject != null)
        {
            timeMenuGameObject.SetActive(true);
            Debug.Log("Time menu activated");
        }
    }

    private System.Collections.IEnumerator DeactivateTimeMenuCoroutine()
    {
        if (deactivationDelay > 0)
        {
            yield return new WaitForSeconds(deactivationDelay);
        }

        if (timeMenuGameObject != null)
        {
            timeMenuGameObject.SetActive(false);
            Debug.Log("Time menu deactivated");
        }
    }

    /// <summary>
    /// Public method to manually set the time menu game object
    /// </summary>
    public void SetTimeMenuGameObject(GameObject timeMenu)
    {
        timeMenuGameObject = timeMenu;
    }

    /// <summary>
    /// Public method to get the current time menu game object
    /// </summary>
    public GameObject GetTimeMenuGameObject()
    {
        return timeMenuGameObject;
    }

    void OnValidate()
    {
        // Validation in editor to help with setup
        if (timeMenuGameObject == null && !string.IsNullOrEmpty(timeMenuName))
        {
            var foundObject = GameObject.Find(timeMenuName);
            if (foundObject != null)
            {
                timeMenuGameObject = foundObject;
            }
        }
    }
} 