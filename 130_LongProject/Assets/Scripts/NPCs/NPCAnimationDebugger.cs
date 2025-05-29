using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

/// <summary>
/// Debug helper for NPC animation issues
/// Add this component to NPCs with animation problems to get detailed debug info
/// </summary>
[RequireComponent(typeof(Animator))]
public class NPCAnimationDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public bool showAnimatorInfo = true;
    public bool showStateInfo = true;
    public bool showParameterInfo = true;
    
    [Header("Manual Testing")]
    [SerializeField] private string testAnimationName = "";
    [SerializeField] private bool testIdleState = false;
    [SerializeField] private bool testWalkState = false;
    [SerializeField] private bool testTalkState = false;
    
    private Animator animator;
    private NPCBehavior npcBehavior;
    private NPCAnimationController animController;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        npcBehavior = GetComponent<NPCBehavior>();
        animController = GetComponent<NPCAnimationController>();
        
        if (enableDebugLogs)
        {
            DebugAnimatorSetup();
        }
    }
    
    void Update()
    {
        // Manual testing buttons
        if (testIdleState)
        {
            testIdleState = false;
            if (npcBehavior != null) npcBehavior.SetState(NPCState.Idle);
        }
        
        if (testWalkState)
        {
            testWalkState = false;
            if (npcBehavior != null) npcBehavior.SetState(NPCState.Walking);
        }
        
        if (testTalkState)
        {
            testTalkState = false;
            if (npcBehavior != null) npcBehavior.SetState(NPCState.Talking);
        }
        
        if (!string.IsNullOrEmpty(testAnimationName))
        {
            if (animator != null)
            {
                animator.Play(testAnimationName);
                Debug.Log($"[NPCAnimationDebugger] Attempting to play: {testAnimationName}");
            }
            testAnimationName = "";
        }
    }
    
    [ContextMenu("Debug Animator Setup")]
    public void DebugAnimatorSetup()
    {
        Debug.Log($"=== ANIMATION DEBUG FOR {gameObject.name} ===");
        
        // Check Animator Component
        if (animator == null)
        {
            Debug.LogError("❌ NO ANIMATOR COMPONENT!");
            return;
        }
        else
        {
            Debug.Log("✅ Animator component found");
        }
        
        // Check Animator Controller
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("❌ NO ANIMATOR CONTROLLER ASSIGNED!");
            return;
        }
        else
        {
            Debug.Log($"✅ Animator Controller: {animator.runtimeAnimatorController.name}");
        }
        
        if (showAnimatorInfo)
        {
            DebugAnimatorController();
        }
        
        if (showStateInfo)
        {
            DebugAnimatorStates();
        }
        
        if (showParameterInfo)
        {
            DebugAnimatorParameters();
        }
        
        DebugNPCComponents();
        DebugCurrentAnimatorState();
    }
    
    private void DebugAnimatorController()
    {
        Debug.Log("--- ANIMATOR CONTROLLER INFO ---");
        Debug.Log($"Controller Name: {animator.runtimeAnimatorController.name}");
        Debug.Log($"Layer Count: {animator.layerCount}");
        Debug.Log($"Parameter Count: {animator.parameterCount}");
    }
    
    private void DebugAnimatorStates()
    {
        Debug.Log("--- ANIMATOR STATES ---");
        
#if UNITY_EDITOR
        // Editor-only: Access to detailed state information
        if (animator.runtimeAnimatorController is AnimatorController controller)
        {
            if (controller.layers.Length > 0)
            {
                var stateMachine = controller.layers[0].stateMachine;
                Debug.Log($"States in Layer 0:");
                
                foreach (var state in stateMachine.states)
                {
                    string motionInfo = state.state.motion != null ? state.state.motion.name : "NO MOTION";
                    Debug.Log($"  - {state.state.name} (Motion: {motionInfo})");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Cannot access detailed state info - not an AnimatorController");
        }
#else
        // Runtime: Limited state information available
        Debug.Log("⚠️ Detailed state info only available in Editor");
        Debug.Log("Current state info available at runtime:");
        DebugCurrentAnimatorState();
#endif
    }
    
    private void DebugCurrentAnimatorState()
    {
        Debug.Log("--- CURRENT ANIMATOR STATE (Runtime) ---");
        
        for (int i = 0; i < animator.layerCount; i++)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(i);
            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(i);
            
            Debug.Log($"Layer {i}:");
            Debug.Log($"  - Current State Hash: {stateInfo.shortNameHash}");
            Debug.Log($"  - State Name Hash: {stateInfo.fullPathHash}");
            Debug.Log($"  - Normalized Time: {stateInfo.normalizedTime:F2}");
            Debug.Log($"  - Is Looping: {stateInfo.loop}");
            
            if (clipInfos.Length > 0)
            {
                Debug.Log($"  - Current Clip: {clipInfos[0].clip.name}");
                Debug.Log($"  - Clip Weight: {clipInfos[0].weight:F2}");
            }
            else
            {
                Debug.Log($"  - No clips playing");
            }
        }
    }
    
    private void DebugAnimatorParameters()
    {
        Debug.Log("--- ANIMATOR PARAMETERS ---");
        
        if (animator.parameters.Length == 0)
        {
            Debug.LogWarning("❌ No parameters found in animator controller!");
            return;
        }
        
        foreach (var param in animator.parameters)
        {
            string currentValue = "";
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    currentValue = animator.GetFloat(param.name).ToString("F2");
                    break;
                case AnimatorControllerParameterType.Bool:
                    currentValue = animator.GetBool(param.name).ToString();
                    break;
                case AnimatorControllerParameterType.Int:
                    currentValue = animator.GetInteger(param.name).ToString();
                    break;
                case AnimatorControllerParameterType.Trigger:
                    currentValue = "Trigger";
                    break;
            }
            Debug.Log($"  - {param.name} ({param.type}): {currentValue}");
        }
    }
    
    private void DebugNPCComponents()
    {
        Debug.Log("--- NPC COMPONENTS ---");
        
        if (npcBehavior != null)
        {
            Debug.Log($"✅ NPCBehavior found - Current State: {npcBehavior.GetCurrentState()}");
        }
        else
        {
            Debug.LogWarning("❌ NPCBehavior not found!");
        }
        
        if (animController != null)
        {
            Debug.Log($"✅ NPCAnimationController found");
        }
        else
        {
            Debug.LogWarning("⚠️ NPCAnimationController not found (optional)");
        }
    }
    
    [ContextMenu("Test All States")]
    public void TestAllStates()
    {
        if (npcBehavior == null) return;
        
        StartCoroutine(TestStatesSequence());
    }
    
    [ContextMenu("Test Animation Parameters")]
    public void TestAnimationParameters()
    {
        if (animator == null) return;
        
        Debug.Log("=== TESTING ANIMATION PARAMETERS ===");
        
        // Test Speed parameter
        if (HasParameter("Speed"))
        {
            animator.SetFloat("Speed", 1.0f);
            Debug.Log("✅ Set Speed to 1.0");
        }
        else
        {
            Debug.LogWarning("❌ Speed parameter not found");
        }
        
        // Test IsWalking
        if (HasParameter("IsWalking"))
        {
            animator.SetBool("IsWalking", true);
            Debug.Log("✅ Set IsWalking to true");
        }
        else
        {
            Debug.LogWarning("❌ IsWalking parameter not found");
        }
        
        // Test IsTalking
        if (HasParameter("IsTalking"))
        {
            animator.SetBool("IsTalking", true);
            Debug.Log("✅ Set IsTalking to true");
        }
        else
        {
            Debug.LogWarning("❌ IsTalking parameter not found");
        }
    }
    
    private bool HasParameter(string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    
    private System.Collections.IEnumerator TestStatesSequence()
    {
        Debug.Log("Testing Idle state...");
        npcBehavior.SetState(NPCState.Idle);
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Testing Walking state...");
        npcBehavior.SetState(NPCState.Walking);
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Testing Talking state...");
        npcBehavior.SetState(NPCState.Talking);
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Returning to Idle state...");
        npcBehavior.SetState(NPCState.Idle);
    }
    
    [ContextMenu("Debug Animation Clips")]
    public void DebugAnimationClips()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("❌ No animator or controller found!");
            return;
        }

        Debug.Log("=== ANIMATION CLIPS DEBUG ===");
        
        // Check current state and clip
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
        
        Debug.Log($"Current State Hash: {currentState.shortNameHash}");
        Debug.Log($"Current State Normalized Time: {currentState.normalizedTime:F2}");
        Debug.Log($"Is Playing: {!animator.IsInTransition(0)}");
        
        if (clipInfos.Length > 0)
        {
            foreach (var clipInfo in clipInfos)
            {
                Debug.Log($"✅ Active Clip: {clipInfo.clip.name}");
                Debug.Log($"   - Length: {clipInfo.clip.length:F2}s");
                Debug.Log($"   - Frame Rate: {clipInfo.clip.frameRate}");
                Debug.Log($"   - Is Looping: {clipInfo.clip.isLooping}");
                Debug.Log($"   - Clip Weight: {clipInfo.weight:F2}");
                Debug.Log($"   - Is Legacy: {clipInfo.clip.legacy}");
                Debug.Log($"   - Is Human Motion: {clipInfo.clip.humanMotion}");
            }
        }
        else
        {
            Debug.LogError("❌ NO ANIMATION CLIPS FOUND IN CURRENT STATE!");
            Debug.LogError("This means the animator states don't have animation clips assigned!");
        }
        
        // Check if animator is enabled and properly configured
        Debug.Log($"Animator Enabled: {animator.enabled}");
        Debug.Log($"Animator Culling Mode: {animator.cullingMode}");
        Debug.Log($"Animator Update Mode: {animator.updateMode}");
        Debug.Log($"Animator Speed: {animator.speed}");
        
        // Check avatar and rig setup
        if (animator.avatar != null)
        {
            Debug.Log($"✅ Avatar: {animator.avatar.name}");
            Debug.Log($"   - Is Valid: {animator.avatar.isValid}");
            Debug.Log($"   - Is Human: {animator.avatar.isHuman}");
        }
        else
        {
            Debug.LogWarning("⚠️ No Avatar assigned to animator!");
        }
    }
    
    [ContextMenu("Force Play Animations")]
    public void ForcePlayAnimations()
    {
        if (animator == null)
        {
            Debug.LogError("❌ No animator found!");
            return;
        }
        
        Debug.Log("=== FORCE PLAYING ANIMATIONS ===");
        
        // Try to force play animations by name
        string[] testAnimations = { "Idle", "Walk", "Walking", "Talk", "Talking" };
        
        foreach (string animName in testAnimations)
        {
            try
            {
                animator.Play(animName);
                Debug.Log($"✅ Successfully played: {animName}");
                return; // If one works, stop trying
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"❌ Could not play {animName}: {e.Message}");
            }
        }
        
        Debug.LogError("❌ Could not force play any test animations!");
    }
    
    [ContextMenu("Check Animation Setup Issues")]
    public void CheckAnimationSetupIssues()
    {
        Debug.Log("=== CHECKING COMMON ANIMATION ISSUES ===");
        
        if (animator == null)
        {
            Debug.LogError("❌ CRITICAL: No Animator component!");
            return;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("❌ CRITICAL: No Animator Controller assigned!");
            return;
        }
        
        // Check if animator is properly enabled
        if (!animator.enabled)
        {
            Debug.LogError("❌ ISSUE: Animator component is disabled!");
        }
        
        // Check animator speed
        if (animator.speed == 0)
        {
            Debug.LogError("❌ ISSUE: Animator speed is 0 - animations won't play!");
        }
        
        // Check culling mode
        if (animator.cullingMode == AnimatorCullingMode.CullCompletely)
        {
            Debug.LogWarning("⚠️ POTENTIAL ISSUE: Animator culling mode is 'Cull Completely' - animations may not play when not visible!");
        }
        
        // Check if we have animation clips in current state
        AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfos.Length == 0)
        {
            Debug.LogError("❌ MAJOR ISSUE: Current animator state has NO animation clips assigned!");
            Debug.LogError("   SOLUTION: Open Animator window, select each state (Idle, Walking, Talking) and assign Motion clips");
        }
        
        // Check avatar setup for humanoid animations
        if (animator.avatar == null)
        {
            Debug.LogWarning("⚠️ No Avatar assigned - required for Humanoid animations");
        }
        else if (!animator.avatar.isValid)
        {
            Debug.LogError("❌ Avatar is invalid!");
        }
        
        // Check for common parameter issues
        bool hasSpeed = HasParameter("Speed");
        bool hasIsWalking = HasParameter("IsWalking");
        bool hasIsTalking = HasParameter("IsTalking");
        
        if (!hasSpeed) Debug.LogWarning("⚠️ Missing 'Speed' parameter");
        if (!hasIsWalking) Debug.LogWarning("⚠️ Missing 'IsWalking' parameter");
        if (!hasIsTalking) Debug.LogWarning("⚠️ Missing 'IsTalking' parameter");
        
        if (hasSpeed && hasIsWalking && hasIsTalking)
        {
            Debug.Log("✅ All required parameters found");
        }
        
        Debug.Log("=== ANIMATION SETUP CHECK COMPLETE ===");
    }
} 