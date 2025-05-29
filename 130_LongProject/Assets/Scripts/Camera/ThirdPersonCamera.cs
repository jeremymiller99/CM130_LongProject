using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float zoomSpeed = 2f;

    [SerializeField] private float height = 2f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float rotationSensitivity = 2f;

    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    
    [Header("Camera Collision")]
    [SerializeField] private LayerMask collisionLayers = -1; // What the camera should collide with
    [SerializeField] private float collisionRadius = 0.3f; // Camera collision sphere radius
    [SerializeField] private float collisionOffset = 0.1f; // Small offset from walls
    [SerializeField] private float collisionSmoothTime = 0.1f; // How fast camera adjusts to collisions

    private float currentRotationX;
    private float currentRotationY;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    
    // Camera collision variables
    private float currentDistance;
    private float targetDistance;
    private float distanceVelocity;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("No target assigned to ThirdPersonCamera!");
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (target != null)
        {
            currentDistance = distance;
            targetDistance = distance;
            transform.position = CalculateCameraPosition();
            transform.LookAt(target);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Mouse rotation
        float mouseX = Input.GetAxis("Mouse X") * rotationSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSensitivity;

        currentRotationX = Mathf.Lerp(currentRotationX, currentRotationX + mouseX, smoothSpeed * Time.deltaTime);
        currentRotationY = Mathf.Clamp(
            Mathf.Lerp(currentRotationY, currentRotationY - mouseY, smoothSpeed * Time.deltaTime),
            minVerticalAngle,
            maxVerticalAngle
        );

        // Scroll wheel zoom
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        distance -= scrollInput * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        targetDistance = distance;

        // Handle camera collision
        HandleCameraCollision();

        // Smooth follow with collision-adjusted distance
        targetPosition = CalculateCameraPositionWithCollision();
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            1f / smoothSpeed
        );

        Vector3 lookAtPosition = target.position + Vector3.up * height * 0.5f;
        transform.LookAt(lookAtPosition);
    }
    
    private void HandleCameraCollision()
    {
        Vector3 targetPos = target.position + Vector3.up * height;
        Vector3 cameraDirection = GetCameraDirection();
        
        // Raycast from target towards camera position
        RaycastHit hit;
        if (Physics.SphereCast(targetPos, collisionRadius, cameraDirection, out hit, targetDistance, collisionLayers))
        {
            // Camera hit something, adjust distance
            float adjustedDistance = hit.distance - collisionOffset;
            targetDistance = Mathf.Clamp(adjustedDistance, minDistance, distance);
        }
        else
        {
            // No collision, use intended distance
            targetDistance = distance;
        }
        
        // Smoothly adjust current distance to target distance
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, collisionSmoothTime);
    }
    
    private Vector3 GetCameraDirection()
    {
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        return rotation * Vector3.back;
    }

    private Vector3 CalculateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        Vector3 direction = rotation * Vector3.back;
        return target.position + Vector3.up * height + direction * distance;
    }
    
    private Vector3 CalculateCameraPositionWithCollision()
    {
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        Vector3 direction = rotation * Vector3.back;
        return target.position + Vector3.up * height + direction * currentDistance;
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        Vector3 targetPos = target.position + Vector3.up * height;
        Vector3 cameraDirection = GetCameraDirection();
        
        // Draw collision detection sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPos + cameraDirection * currentDistance, collisionRadius);
        
        // Draw camera ray
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(targetPos, cameraDirection * targetDistance);
        
        // Draw intended camera position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPos + cameraDirection * distance, 0.1f);
    }
}
