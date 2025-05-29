using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class NPCAnimationController : MonoBehaviour
{
    [Header("Animation Parameters")]
    public string speedParameter = "Speed";
    public string isWalkingParameter = "IsWalking";
    public string isTalkingParameter = "IsTalking";
    public string triggerTalkParameter = "TriggerTalk";
    public string triggerWaveParameter = "TriggerWave";
    
    [Header("Animation Clips (Optional - for direct control)")]
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip talkClip;
    public AnimationClip waveClip;
    
    [Header("Animation Settings")]
    public float transitionSpeed = 0.15f;
    public bool useRootMotion = false;
    public bool debugAnimations = false;
    
    [Header("Facial Expressions (Optional)")]
    public SkinnedMeshRenderer faceMeshRenderer;
    public Material[] expressionMaterials; // Happy, Sad, Angry, Neutral, etc.
    private int currentExpressionIndex = 0;
    
    // Components
    private Animator animator;
    private NPCBehavior npcBehavior;
    
    // Animation hashes for performance
    private int speedHash;
    private int isWalkingHash;
    private int isTalkingHash;
    private int triggerTalkHash;
    private int triggerWaveHash;
    
    // State tracking
    private NPCState lastState;
    private bool isTransitioning = false;
    
    void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        npcBehavior = GetComponent<NPCBehavior>();
        
        // Cache parameter hashes
        speedHash = Animator.StringToHash(speedParameter);
        isWalkingHash = Animator.StringToHash(isWalkingParameter);
        isTalkingHash = Animator.StringToHash(isTalkingParameter);
        triggerTalkHash = Animator.StringToHash(triggerTalkParameter);
        triggerWaveHash = Animator.StringToHash(triggerWaveParameter);
        
        // Configure animator
        animator.applyRootMotion = useRootMotion;
        
        lastState = NPCState.Idle;
        
        Debug.Log($"[NPCAnimationController] {gameObject.name} initialized");
    }
    
    void Update()
    {
        if (npcBehavior != null)
        {
            UpdateAnimationState();
        }
    }
    
    private void UpdateAnimationState()
    {
        NPCState currentState = npcBehavior.GetCurrentState();
        float currentSpeed = GetCurrentSpeed();
        
        // Update speed parameter
        if (HasParameter(speedHash))
        {
            animator.SetFloat(speedHash, currentSpeed);
        }
        
        // Handle state changes
        if (currentState != lastState)
        {
            TransitionToState(currentState);
            lastState = currentState;
        }
        
        // Update boolean parameters based on current state
        UpdateBooleanParameters(currentState, currentSpeed);
    }
    
    private float GetCurrentSpeed()
    {
        // Get speed from NavMeshAgent or Rigidbody
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            return agent.velocity.magnitude;
        }
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            return rb.linearVelocity.magnitude;
        }
        
        return 0f;
    }
    
    private void TransitionToState(NPCState newState)
    {
        if (isTransitioning) return;
        
        if (debugAnimations)
        {
            Debug.Log($"[NPCAnimationController] {gameObject.name} transitioning to: {newState}");
        }
        
        // Reset all boolean parameters
        ResetAllBooleans();
        
        // Set parameters for new state
        switch (newState)
        {
            case NPCState.Idle:
                // Idle is handled by default when all booleans are false
                break;
                
            case NPCState.Walking:
                SetBool(isWalkingHash, true);
                break;
                
            case NPCState.Talking:
                SetBool(isTalkingHash, true);
                TriggerParameter(triggerTalkHash);
                StartCoroutine(PlayTalkingAnimation());
                break;
        }
    }
    
    private void UpdateBooleanParameters(NPCState currentState, float speed)
    {
        // Update walking based on actual movement speed
        bool isActuallyWalking = speed > 0.1f && currentState == NPCState.Walking;
        SetBool(isWalkingHash, isActuallyWalking);
    }
    
    private void ResetAllBooleans()
    {
        SetBool(isWalkingHash, false);
        SetBool(isTalkingHash, false);
    }
    
    private void SetBool(int parameterHash, bool value)
    {
        if (HasParameter(parameterHash))
        {
            animator.SetBool(parameterHash, value);
        }
    }
    
    private void TriggerParameter(int parameterHash)
    {
        if (HasParameter(parameterHash))
        {
            animator.SetTrigger(parameterHash);
        }
    }
    
    private bool HasParameter(int parameterHash)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == parameterHash)
                return true;
        }
        return false;
    }
    
    private IEnumerator PlayTalkingAnimation()
    {
        isTransitioning = true;
        
        // Wait for transition to complete
        yield return new WaitForSeconds(transitionSpeed);
        
        // You can add more complex talking animation logic here
        // For now, just mark transition as complete
        isTransitioning = false;
    }
    
    /// <summary>
    /// Trigger the wave animation
    /// </summary>
    public void TriggerWave()
    {
        Debug.Log($"[NPCAnimationController] {gameObject.name} - TriggerWave called");
        
        if (debugAnimations)
        {
            Debug.Log($"[NPCAnimationController] {gameObject.name} - Debug animations enabled, triggering wave");
        }
        
        if (!HasParameter(triggerWaveHash))
        {
            Debug.LogError($"[NPCAnimationController] {gameObject.name} - TriggerWave parameter not found in animator!");
            return;
        }
        
        Debug.Log($"[NPCAnimationController] {gameObject.name} - Triggering wave animation");
        TriggerParameter(triggerWaveHash);
        StartCoroutine(PlayWaveAnimation());
    }
    
    private IEnumerator PlayWaveAnimation()
    {
        Debug.Log($"[NPCAnimationController] {gameObject.name} - Starting wave animation");
        isTransitioning = true;
        
        // Wait for transition to complete
        Debug.Log($"[NPCAnimationController] {gameObject.name} - Waiting for transition ({transitionSpeed}s)");
        yield return new WaitForSeconds(transitionSpeed);
        
        // You can add more complex wave animation logic here
        Debug.Log($"[NPCAnimationController] {gameObject.name} - Wave animation transition complete");
        isTransitioning = false;
    }
    
    // Add animation event method for wave
    public void OnWaveAnimationStart()
    {
        Debug.Log($"[NPCAnimationController] {gameObject.name} - Wave animation started (Animation Event)");
    }
    
    public void OnWaveAnimationEnd()
    {
        Debug.Log($"[NPCAnimationController] {gameObject.name} - Wave animation ended (Animation Event)");
    }
    
    #region Public Interface
    
    /// <summary>
    /// Play a custom animation by name
    /// </summary>
    public void PlayCustomAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }
    
    /// <summary>
    /// Play a custom animation by hash
    /// </summary>
    public void PlayCustomAnimation(int animationHash)
    {
        if (animator != null)
        {
            animator.Play(animationHash);
        }
    }
    
    /// <summary>
    /// Set a custom float parameter
    /// </summary>
    public void SetCustomParameter(string parameterName, float value)
    {
        int hash = Animator.StringToHash(parameterName);
        if (HasParameter(hash))
        {
            animator.SetFloat(hash, value);
        }
    }
    
    /// <summary>
    /// Set a custom bool parameter
    /// </summary>
    public void SetCustomParameter(string parameterName, bool value)
    {
        int hash = Animator.StringToHash(parameterName);
        if (HasParameter(hash))
        {
            animator.SetBool(hash, value);
        }
    }
    
    /// <summary>
    /// Trigger a custom parameter
    /// </summary>
    public void TriggerCustomParameter(string parameterName)
    {
        int hash = Animator.StringToHash(parameterName);
        if (HasParameter(hash))
        {
            animator.SetTrigger(hash);
        }
    }
    
    #endregion
    
    #region Facial Expressions
    
    /// <summary>
    /// Set facial expression by index
    /// </summary>
    public void SetExpression(int expressionIndex)
    {
        if (faceMeshRenderer != null && expressionMaterials != null && 
            expressionIndex >= 0 && expressionIndex < expressionMaterials.Length)
        {
            faceMeshRenderer.material = expressionMaterials[expressionIndex];
            currentExpressionIndex = expressionIndex;
        }
    }
    
    /// <summary>
    /// Set a random facial expression
    /// </summary>
    public void SetRandomExpression()
    {
        if (expressionMaterials != null && expressionMaterials.Length > 0)
        {
            int randomIndex = Random.Range(0, expressionMaterials.Length);
            SetExpression(randomIndex);
        }
    }
    
    /// <summary>
    /// Set neutral expression (index 0)
    /// </summary>
    public void SetNeutralExpression()
    {
        SetExpression(0);
    }
    
    /// <summary>
    /// Set happy expression (index 1)
    /// </summary>
    public void SetHappyExpression()
    {
        SetExpression(1);
    }
    
    /// <summary>
    /// Set sad expression (index 2)
    /// </summary>
    public void SetSadExpression()
    {
        SetExpression(2);
    }
    
    #endregion
    
    #region Animation Events
    
    /// <summary>
    /// Called when talk animation starts (Animation Event)
    /// </summary>
    public void OnTalkAnimationStart()
    {
        if (debugAnimations)
        {
            Debug.Log($"[NPCAnimationController] {gameObject.name} started talking animation");
        }
    }
    
    /// <summary>
    /// Called when talk animation ends (Animation Event)
    /// </summary>
    public void OnTalkAnimationEnd()
    {
        if (debugAnimations)
        {
            Debug.Log($"[NPCAnimationController] {gameObject.name} ended talking animation");
        }
    }
    
    /// <summary>
    /// Called on footstep (Animation Event)
    /// </summary>
    public void OnFootstep()
    {
        // Can be used to trigger footstep sounds or effects
        if (debugAnimations)
        {
            Debug.Log($"[NPCAnimationController] {gameObject.name} footstep");
        }
    }
    
    #endregion
    
    #region Getters
    
    public bool IsTransitioning() => isTransitioning;
    public NPCState GetLastState() => lastState;
    public int GetCurrentExpression() => currentExpressionIndex;
    
    public bool IsAnimationPlaying(string animationName)
    {
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(animationName);
        }
        return false;
    }
    
    #endregion
} 