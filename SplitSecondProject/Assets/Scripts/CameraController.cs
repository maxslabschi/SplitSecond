using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player;
    public Vector3 normalOffset = new Vector3(0, 1.5f, 0);
    public Vector3 sneakingOffset = new Vector3(0, 1f, 0); // Lowered for sneaking
    public float mouseSensitivity = 100f;

    [Header("Dynamic Effects")]
    public float baseFOV = 75f;
    public float maxFOV = 100f;
    public float fovSpeedMultiplier = 0.25f;
    public float maxTiltAngle = 2.5f; // Subtle tilt for strafing
    public float tiltSpeed = 8f;

    private Camera cam;
    private float verticalRotation = 0f;
    private float currentTilt = 0f;

    private PlayerMovement playerMovement;

    private void Start()
    {
        cam = GetComponent<Camera>();
        playerMovement = player.GetComponent<PlayerMovement>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        FollowPlayer();
        HandleMouseLook();
        AdjustFOV();
        ApplyDynamicTilting();
    }

    private void FollowPlayer()
    {
        // Adjust the camera offset based on the player's state
        Vector3 targetOffset = playerMovement.IsSneaking ? sneakingOffset : normalOffset;
        Vector3 targetPosition = player.position + targetOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
    }

    private void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the player horizontally
        player.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void AdjustFOV()
    {
        // Dynamically adjust FOV based on player speed
        float speedFactor = (playerMovement.CurrentSpeed - playerMovement.walkSpeed) * fovSpeedMultiplier;
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedFactor);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 5f);
    }

    private void ApplyDynamicTilting()
    {
        // Calculate camera tilt based on horizontal input
        float horizontalInput = Input.GetAxis("Horizontal");
        float targetTilt = horizontalInput * maxTiltAngle;

        // Smoothly apply tilt
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, currentTilt);
    }
}
