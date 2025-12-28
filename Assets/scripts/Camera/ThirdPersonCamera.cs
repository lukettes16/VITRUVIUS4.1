using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Camera Target")]
    public Transform target;
    public Transform pivot; // Used for vertical rotation if assigned

    [Header("Camera Settings")]
    public float distance = 5f;
    public float height = 1.7f;
    public float horizontalOffset = 0f;
    public float rotationSpeed = 2.6f; // Aumentado un 30% (de 2.0f a 2.6f) para mayor agilidad
    public float joystickSensitivity = 1.0f;
    public float rotationSmoothing = 0.05f;
    public bool invertX = false;
    public bool invertY = false;

    [Header("Rotation Limits")]
    public float minPitch = -20f;
    public float maxPitch = 80f;

    [Header("Collision")]
    public LayerMask collisionLayers;
    public float collisionOffset = 0.2f;
    public float sphereCastRadius = 0.2f;

    [Header("First Person")]
    public bool isFirstPerson = false;
    public float fpHeight = 1.6f;
    public float fpSmoothing = 0.02f;

    // Input from Player scripts
    [HideInInspector] public Vector2 lookInput;

    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private float targetYaw = 0f;
    private float targetPitch = 0f;
    private float targetDistance;
    private Vector3 cameraVelocity = Vector3.zero;
    private float yawVelocity = 0f;
    private float pitchVelocity = 0f;
    
    private bool isInitialized = false;

    void Start()
    {
        InitializeCamera();
    }

    public void InitializeCamera()
    {
        if (target == null)
        {
            target = transform.parent;
        }

        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            targetYaw = angles.y;
            targetPitch = angles.x;
            currentYaw = targetYaw;
            currentPitch = targetPitch;
            targetDistance = distance;
            isInitialized = true;
        }
    }

    void LateUpdate()
    {
        if (!isInitialized || target == null) return;

        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        // Apply sensitivity and inversion
        float xInput = lookInput.x * (invertX ? -1 : 1);
        float yInput = lookInput.y * (invertY ? -1 : 1);

        // Frame-rate independent rotation with smoothing (reduced from 100f to 5f for natural control)
        targetYaw += xInput * joystickSensitivity * rotationSpeed * 5f * Time.unscaledDeltaTime;
        targetPitch -= yInput * joystickSensitivity * rotationSpeed * 5f * Time.unscaledDeltaTime;
        
        // Clamp pitch to avoid uncomfortable angles
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // Smooth damping for fluid 360 rotation
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotationSmoothing);
        currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, rotationSmoothing);
    }

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        
        // Point of focus (Player's head/center)
        float currentHeight = isFirstPerson ? fpHeight : height;
        Vector3 targetFocusPosition = target.position + Vector3.up * currentHeight;
        
        if (!isFirstPerson && horizontalOffset != 0)
        {
            targetFocusPosition += rotation * Vector3.right * horizontalOffset;
        }

        if (isFirstPerson)
        {
            // First Person: Camera at head, follows rotation exactly
            transform.position = Vector3.SmoothDamp(transform.position, targetFocusPosition, ref cameraVelocity, fpSmoothing);
            transform.rotation = rotation;
            return;
        }

        // Third Person: Orbit logic
        Vector3 direction = rotation * -Vector3.forward;
        
        // Check for collisions to avoid clipping through walls
        RaycastHit hit;
        float currentDistance = distance;
        if (Physics.SphereCast(targetFocusPosition, sphereCastRadius, direction, out hit, distance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform != target && !hit.transform.IsChildOf(target))
            {
                currentDistance = Mathf.Max(hit.distance - collisionOffset, 0.2f);
            }
        }

        // Smoothly interpolate distance for professional feel
        targetDistance = Mathf.Lerp(targetDistance, currentDistance, Time.unscaledDeltaTime * 10f);
        Vector3 finalPosition = targetFocusPosition + direction * targetDistance;

        transform.position = finalPosition;
        transform.rotation = rotation;
    }

    public void SetFirstPerson(bool fp)
    {
        isFirstPerson = fp;
        if (fp)
        {
            // Reset distance when entering FP
            targetDistance = 0f;
        }
        else
        {
            targetDistance = distance;
        }
    }
}
