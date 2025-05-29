using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPCSetupHelper : MonoBehaviour
{
    [Header("Auto-Setup Configuration")]
    [Tooltip("Click to automatically add and configure all required NPC components")]
    public bool autoSetupOnStart = false;
    
    [Header("Animation Settings")]
    public RuntimeAnimatorController animatorController;
    public bool useRootMotion = false;
    
    [Header("Movement Settings")]
    public float walkSpeed = 1.5f;
    public float walkRadius = 10f;
    public float stateChangeInterval = 5f;
    [Range(0f, 1f)] public float walkChance = 0.3f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupNPCComponents();
        }
    }
    
    [ContextMenu("Setup NPC Components")]
    public void SetupNPCComponents()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSetupHelper] Setting up NPC components for {gameObject.name}");
        }
        
        // Ensure required components exist
        SetupRequiredComponents();
        
        // Configure components
        ConfigureComponents();
        
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSetupHelper] Setup complete for {gameObject.name}");
        }
    }
    
    private void SetupRequiredComponents()
    {
        // Add NPCBehavior if it doesn't exist
        if (GetComponent<NPCBehavior>() == null)
        {
            gameObject.AddComponent<NPCBehavior>();
            if (showDebugInfo) Debug.Log($"[NPCSetupHelper] Added NPCBehavior to {gameObject.name}");
        }
        
        // Add NPCAnimationController if it doesn't exist
        if (GetComponent<NPCAnimationController>() == null)
        {
            gameObject.AddComponent<NPCAnimationController>();
            if (showDebugInfo) Debug.Log($"[NPCSetupHelper] Added NPCAnimationController to {gameObject.name}");
        }
        
        // Add Animator if it doesn't exist
        if (GetComponent<Animator>() == null)
        {
            gameObject.AddComponent<Animator>();
            if (showDebugInfo) Debug.Log($"[NPCSetupHelper] Added Animator to {gameObject.name}");
        }
        
        // Add NavMeshAgent if it doesn't exist
        if (GetComponent<NavMeshAgent>() == null)
        {
            gameObject.AddComponent<NavMeshAgent>();
            if (showDebugInfo) Debug.Log($"[NPCSetupHelper] Added NavMeshAgent to {gameObject.name}");
        }
        
        // Ensure there's a collider for interactions
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Add a capsule collider as default
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.radius = 2f; // Interaction radius
            capsule.height = 2f;
            if (showDebugInfo) Debug.Log($"[NPCSetupHelper] Added CapsuleCollider to {gameObject.name}");
        }
        else if (!col.isTrigger)
        {
            if (showDebugInfo) Debug.LogWarning($"[NPCSetupHelper] {gameObject.name} has a non-trigger collider. NPCs should have trigger colliders for interactions.");
        }
    }
    
    private void ConfigureComponents()
    {
        // Configure Animator
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            if (animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
                if (showDebugInfo) Debug.Log($"[NPCSetupHelper] Assigned animator controller to {gameObject.name}");
            }
            animator.applyRootMotion = useRootMotion;
        }
        
        // Configure NavMeshAgent
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.angularSpeed = 180f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;
            agent.autoBraking = true;
            if (showDebugInfo) Debug.Log($"[NPCSetupHelper] Configured NavMeshAgent for {gameObject.name}");
        }
        
        // Configure NPCBehavior
        NPCBehavior npcBehavior = GetComponent<NPCBehavior>();
        if (npcBehavior != null)
        {
            npcBehavior.walkSpeed = walkSpeed;
            npcBehavior.walkRadius = walkRadius;
            npcBehavior.stateChangeInterval = stateChangeInterval;
            npcBehavior.walkChance = walkChance;
            
            if (showDebugInfo) Debug.Log($"[NPCSetupHelper] Configured NPCBehavior for {gameObject.name}");
        }
        
        // Configure NPCAnimationController
        NPCAnimationController animController = GetComponent<NPCAnimationController>();
        if (animController != null)
        {
            animController.useRootMotion = useRootMotion;
            animController.debugAnimations = showDebugInfo;
            if (showDebugInfo) Debug.Log($"[NPCSetupHelper] Configured NPCAnimationController for {gameObject.name}");
        }
    }
    
    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        Debug.Log($"[NPCSetupHelper] Validating setup for {gameObject.name}:");
        
        // Check required components
        bool hasNPCBehavior = GetComponent<NPCBehavior>() != null;
        bool hasAnimController = GetComponent<NPCAnimationController>() != null;
        bool hasAnimator = GetComponent<Animator>() != null;
        bool hasNavMeshAgent = GetComponent<NavMeshAgent>() != null;
        bool hasCollider = GetComponent<Collider>() != null;
        
        Debug.Log($"✅ NPCBehavior: {hasNPCBehavior}");
        Debug.Log($"✅ NPCAnimationController: {hasAnimController}");
        Debug.Log($"✅ Animator: {hasAnimator}");
        Debug.Log($"✅ NavMeshAgent: {hasNavMeshAgent}");
        Debug.Log($"✅ Collider: {hasCollider}");
        
        // Check animator controller
        Animator animator = GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            Debug.Log($"✅ Animator Controller assigned: {animator.runtimeAnimatorController.name}");
        }
        else
        {
            Debug.LogWarning($"❌ No Animator Controller assigned to {gameObject.name}");
        }
        
        // Check NavMeshAgent on NavMesh
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                Debug.Log($"✅ NavMeshAgent is on NavMesh");
            }
            else
            {
                Debug.LogWarning($"❌ NavMeshAgent is not on NavMesh. Make sure NavMesh is baked and NPC is positioned correctly.");
            }
        }
        
        // Check collider setup
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (col.isTrigger)
            {
                Debug.Log($"✅ Collider is set as trigger for interactions");
            }
            else
            {
                Debug.LogWarning($"❌ Collider should be set as trigger for NPC interactions");
            }
        }
        
        Debug.Log($"[NPCSetupHelper] Validation complete for {gameObject.name}");
    }
    
    [ContextMenu("Create Basic Animator Controller")]
    public void CreateBasicAnimatorController()
    {
#if UNITY_EDITOR
        // Create a basic animator controller asset
        string path = $"Assets/Animators/{gameObject.name}_Controller.controller";
        
        // Ensure directory exists
        string directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        UnityEditor.Animations.AnimatorController controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);
        
        // Add basic parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("TriggerTalk", AnimatorControllerParameterType.Trigger);
        
        // Create states
        var rootStateMachine = controller.layers[0].stateMachine;
        var idleState = rootStateMachine.AddState("Idle");
        var walkState = rootStateMachine.AddState("Walking");
        var talkState = rootStateMachine.AddState("Talking");
        
        // Set default state
        rootStateMachine.defaultState = idleState;
        
        // Create transitions
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsWalking");
        idleToWalk.hasExitTime = false;
        
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "IsWalking");
        walkToIdle.hasExitTime = false;
        
        var idleToTalk = idleState.AddTransition(talkState);
        idleToTalk.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsTalking");
        idleToTalk.hasExitTime = false;
        
        var talkToIdle = talkState.AddTransition(idleState);
        talkToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "IsTalking");
        talkToIdle.hasExitTime = false;
        
        // Assign to this NPC
        animatorController = controller;
        GetComponent<Animator>().runtimeAnimatorController = controller;
        
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSetupHelper] Created basic animator controller at: {path}");
        }
#else
        Debug.LogWarning("[NPCSetupHelper] CreateBasicAnimatorController only works in the Unity Editor");
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NPCSetupHelper))]
public class NPCSetupHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10);
        
        NPCSetupHelper helper = (NPCSetupHelper)target;
        
        if (GUILayout.Button("Setup NPC Components", GUILayout.Height(30)))
        {
            helper.SetupNPCComponents();
        }
        
        if (GUILayout.Button("Validate Setup"))
        {
            helper.ValidateSetup();
        }
        
        if (GUILayout.Button("Create Basic Animator Controller"))
        {
            helper.CreateBasicAnimatorController();
        }
    }
}
#endif 