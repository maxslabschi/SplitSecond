using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float sneakSpeed = 3f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Player Height Settings")]
    public float standingHeight = 2f;
    public float sneakingHeight = 1f; // Lowered for tighter spaces
    public float heightTransitionSpeed = 8f;

    [Header("Respawn Settings")]
    public float fallThreshold = -10f;
    public Transform respawnPoint;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isSneaking = false;

    private float currentSpeed;
    private float targetHeight;

    public bool IsSneaking => isSneaking;
    public float CurrentSpeed => currentSpeed;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        targetHeight = standingHeight;
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        HandleMovement();
        AdjustHeight();
        CheckFallAndRespawn();
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Ensure the player stays grounded
        }

        // Movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Sneaking
        if (Input.GetKey(KeyCode.C))
        {
            isSneaking = true;
            currentSpeed = sneakSpeed;
            targetHeight = sneakingHeight;
        }
        // Walking or Sprinting
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            isSneaking = false;
            currentSpeed = sprintSpeed;
            targetHeight = standingHeight;
        }
        else
        {
            isSneaking = false;
            currentSpeed = walkSpeed;
            targetHeight = standingHeight;
        }

        // Apply movement
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void AdjustHeight()
    {
        // Smoothly transition to the target height
        float newHeight = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * heightTransitionSpeed);
        controller.height = newHeight;
        controller.center = new Vector3(0, newHeight / 2f, 0);
    }

    private void CheckFallAndRespawn()
    {
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        if (respawnPoint != null)
        {
            controller.enabled = false;
            transform.position = respawnPoint.position;
            controller.enabled = true;
            velocity = Vector3.zero;
            Debug.Log("Player respawned at the respawn point.");
        }
        else
        {
            Debug.LogError("Respawn Point is not set! Assign a respawn point in the Inspector.");
        }
    }
}
