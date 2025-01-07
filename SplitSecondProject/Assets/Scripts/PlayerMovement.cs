using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float airControlMultiplier = 8f; // Air control sensitivity for strafing

    [Header("Player Height Settings")]
    public float standingHeight = 2f;
    public float slidingHeight = 1f;  // Height when sliding
    public float heightTransitionSpeed = 8f;

    [Header("Wall Jump Settings")]
    public float wallJumpForce = 5f;
    public float wallDetectionDistance = 1f;
    public LayerMask wallLayer;
    public float wallJumpFriction = 5f; // Friction applied to the wall jump direction

    [Header("Sliding Settings")]
    public float slideSpeed = 14f;      // Movement speed during slide
    public float maxSlideTime = 1.0f;   // How long the slide lasts (in seconds)

    [Header("Respawn Settings")]
    public float fallThreshold = -10f;
    public Transform respawnPoint;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isWallJumping = false;
    private bool isSliding = false;     // Sliding state

    private float currentSpeed;
    private float targetHeight;
    private Vector3 wallJumpDirection = Vector3.zero;

    // Sliding timer
    private float slideTimer;

    // Public properties so other scripts (like the camera) can access
    public float CurrentSpeed => currentSpeed;
    public bool IsSliding => isSliding;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        targetHeight = standingHeight;
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        HandleMovement();
        ApplyWallJumpFriction();
        AdjustHeight();
        CheckFallAndRespawn();
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        // If grounded, reset vertical velocity to keep the player on the ground
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // Reset wall jump states when grounded
        if (isGrounded)
        {
            isWallJumping = false;
            wallJumpDirection = Vector3.zero;
        }

        // Read movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        if (Input.GetKeyDown(KeyCode.C) && !isSliding && isGrounded && move.magnitude > 0.1f && currentSpeed == sprintSpeed)
        {
            StartSlide();
        }

        if ((Input.GetKeyUp(KeyCode.C) && isSliding) || (isSliding && slideTimer <= 0f))
        {
            EndSlide();
        }

        // If sliding, override movement
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            controller.Move(move.normalized * slideSpeed * Time.deltaTime);
        }
        else
        {
            // Normal ground movement
            if (isGrounded)
            {
                // Sprint if holding left shift and moving
                if (Input.GetKey(KeyCode.LeftShift) && move.magnitude > 0.1f)
                {
                    currentSpeed = sprintSpeed;
                    targetHeight = standingHeight;
                }
                else
                {
                    // Otherwise walk
                    currentSpeed = walkSpeed;
                    targetHeight = standingHeight;
                }

                controller.Move(move * currentSpeed * Time.deltaTime);
            }
            else
            {
                // Air control (reduced responsiveness in the air)
                Vector3 airMove = move * airControlMultiplier;
                controller.Move(airMove * Time.deltaTime);
            }
        }

        // Wall Jump check
        if (!isGrounded && Input.GetButtonDown("Jump") && IsNearWall())
        {
            PerformWallJump();
        }
        else if (Input.GetButtonDown("Jump") && isGrounded && !isSliding)
        {
            // Normal jump (only if not sliding)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void StartSlide()
    {
        isSliding = true;
        slideTimer = maxSlideTime;

        // Change player height
        targetHeight = slidingHeight;

        // Force the player a bit downward so they stay on the ground
        velocity.y = -2f;
    }

    private void EndSlide()
    {
        isSliding = false;
        targetHeight = standingHeight;
    }

    private bool IsNearWall()
    {
        // Check for walls in front, right, left
        return Physics.Raycast(transform.position, transform.forward, wallDetectionDistance, wallLayer) ||
               Physics.Raycast(transform.position, transform.right, wallDetectionDistance, wallLayer) ||
               Physics.Raycast(transform.position, -transform.right, wallDetectionDistance, wallLayer);
    }

    private void PerformWallJump()
    {
        Vector3 wallNormal = GetWallNormal();
        if (wallNormal != Vector3.zero)
        {
            // Push the player away from the wall
            wallJumpDirection = wallNormal * wallJumpForce;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isWallJumping = true;
        }
    }

    private Vector3 GetWallNormal()
    {
        RaycastHit hit;
        // Check forward
        if (Physics.Raycast(transform.position, transform.forward, out hit, wallDetectionDistance, wallLayer))
            return hit.normal;
        // Check right
        if (Physics.Raycast(transform.position, transform.right, out hit, wallDetectionDistance, wallLayer))
            return hit.normal;
        // Check left
        if (Physics.Raycast(transform.position, -transform.right, out hit, wallDetectionDistance, wallLayer))
            return hit.normal;

        return Vector3.zero;
    }

    private void ApplyWallJumpFriction()
    {
        if (isWallJumping)
        {
            // Gradually reduce wall jump direction velocity
            wallJumpDirection = Vector3.Lerp(wallJumpDirection, Vector3.zero, Time.deltaTime * wallJumpFriction);

            // Apply that velocity to the character
            controller.Move(wallJumpDirection * Time.deltaTime);

            // Stop wall jumping if negligible
            if (wallJumpDirection.magnitude < 0.1f)
            {
                wallJumpDirection = Vector3.zero;
                isWallJumping = false;
            }
        }
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
            wallJumpDirection = Vector3.zero;
            isWallJumping = false;
            Debug.Log("Player respawned at the respawn point.");
        }
        else
        {
            Debug.LogError("Respawn Point is not set! Assign a respawn point in the Inspector.");
        }
    }
}
