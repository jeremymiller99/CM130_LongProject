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

    private float currentRotationX;
    private float currentRotationY;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("No target assigned to ThirdPersonCamera!");
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (target != null)
        {
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

        // Smooth follow
        targetPosition = CalculateCameraPosition();
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            1f / smoothSpeed
        );

        Vector3 lookAtPosition = target.position + Vector3.up * height * 0.5f;
        transform.LookAt(lookAtPosition);
    }

    private Vector3 CalculateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        Vector3 direction = rotation * Vector3.back;
        return target.position + Vector3.up * height + direction * distance;
    }
}
