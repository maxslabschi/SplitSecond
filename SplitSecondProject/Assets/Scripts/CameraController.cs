using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player;
    public Vector3 normalOffset = new Vector3(0, 1.5f, 0);
    public Vector3 slidingOffset = new Vector3(0, 1.0f, 0);
    public float mouseSensitivity = 100f;

    [Header("Dynamic Effects")]
    public float baseFOV = 75f;
    public float maxFOV = 100f;
    public float fovSpeedMultiplier = 0.25f;
    public float maxTiltAngle = 2.5f;   // Left-right tilt for strafing
    public float tiltSpeed = 8f;

    [Header("Slide Camera Tilt")]
    public float slideTiltAngle = 15f;
    public float slideTiltSpeed = 5f;

    private Camera cam;
    private float verticalRotation = 0f;
    private float currentTilt = 0f;
    private float currentSlideTilt = 0f;

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
        ApplySlidingTilt();
    }

    private void FollowPlayer()
    {
        // If sliding, use a different offset
        Vector3 targetOffset = playerMovement.IsSliding ? slidingOffset : normalOffset;

        // Smoothly move camera toward that offset
        Vector3 targetPosition = player.position + targetOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
    }

    private void HandleMouseLook()
    {
        // Get raw mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the player horizontally
        player.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
    }

    private void AdjustFOV()
    {
        float speedFactor = (playerMovement.CurrentSpeed - playerMovement.walkSpeed) * fovSpeedMultiplier;
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedFactor);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 5f);
    }

    private void ApplyDynamicTilting()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float targetTilt = horizontalInput * maxTiltAngle;

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
    }

    private void ApplySlidingTilt()
    {
        float targetSlideTilt = playerMovement.IsSliding ? slideTiltAngle : 0f;

        currentSlideTilt = Mathf.Lerp(currentSlideTilt, targetSlideTilt, Time.deltaTime * slideTiltSpeed);

        float finalPitch = verticalRotation + currentSlideTilt;
        float finalRoll = currentTilt;

        // Apply to camera
        transform.localRotation = Quaternion.Euler(finalPitch, 0f, finalRoll);
    }
}
