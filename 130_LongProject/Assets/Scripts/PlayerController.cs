using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
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

    // Components
    private Rigidbody rb;
    private Animator animator;

    // State tracking
    private bool isGrounded;
    private bool isRunning;
    private Vector3 moveDirection;
    private float currentSpeed;

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

        // Set up rigidbody
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Cache animation parameter hashes
        speedHash = Animator.StringToHash("Speed");
        isGroundedHash = Animator.StringToHash("IsGrounded");
        interactHash = Animator.StringToHash("Interact");
        waveHash = Animator.StringToHash("Wave");
    }

    void Update()
    {
        HandleInput();
        UpdateAnimations();
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
            TriggerInteract();
        }

        // Wave input (assuming G key)
        if (Input.GetKeyDown(KeyCode.G))
        {
            TriggerWave();
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

        // Determine current speed based on state
        float targetSpeed = isRunning ? runSpeed : moveSpeed;
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

    private void TriggerInteract()
    {
        animator.SetTrigger(interactHash);
    }

    private void TriggerWave()
    {
        animator.SetTrigger(waveHash);
    }

    // Animation Events - Called from animation clips
    public void OnInteractComplete()
    {
        // Handle interaction completion if needed
        // This can be called from the interact animation
    }

    public void OnWaveComplete()
    {
        // Handle wave completion if needed
        // This can be called from the wave animation
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
}