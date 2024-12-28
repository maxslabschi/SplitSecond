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
    public float airControlMultiplier = 8f; // Air control sensitivity for strafing

    [Header("Player Height Settings")]
    public float standingHeight = 2f;
    public float sneakingHeight = 1f;
    public float heightTransitionSpeed = 8f;

    [Header("Wall Jump Settings")]
    public float wallJumpForce = 5f;
    public float wallDetectionDistance = 1f;
    public LayerMask wallLayer;
    public float wallJumpFriction = 5f; // Friction applied to the wall jump direction

    [Header("Respawn Settings")]
    public float fallThreshold = -10f;
    public Transform respawnPoint;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isSneaking = false;
    private bool isWallJumping = false;

    private float currentSpeed;
    private float targetHeight;
    private Vector3 wallJumpDirection = Vector3.zero;

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
        ApplyWallJumpFriction();
        AdjustHeight();
        CheckFallAndRespawn();
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f; // Ensure the player stays grounded

            isWallJumping = false; // Reset wall jumping state when grounded
            wallJumpDirection = Vector3.zero; // Reset wall jump direction
        }

        // Movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        if (isGrounded)
        {
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

            // Apply standard movement when grounded
            controller.Move(move * currentSpeed * Time.deltaTime);
        }
        else
        {
            // Allow air control for strafing
            Vector3 airMove = move * airControlMultiplier;
            controller.Move(airMove * Time.deltaTime);
        }

        // Wall Jump Check
        if (!isGrounded && Input.GetButtonDown("Jump") && IsNearWall())
        {
            PerformWallJump();
        }
        else if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Normal Jump
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Apply gravity or wall jump velocity
        controller.Move(velocity * Time.deltaTime);
    }

    private void PerformWallJump()
    {
        // Push the player away from the wall
        Vector3 wallNormal = GetWallNormal();
        if (wallNormal != Vector3.zero)
        {
            // Set velocity for wall jump and apply a directional force
            wallJumpDirection = wallNormal * wallJumpForce;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            isWallJumping = true;
        }
    }

    private bool IsNearWall()
    {
        // Check for walls in front, left, and right
        return Physics.Raycast(transform.position, transform.forward, wallDetectionDistance, wallLayer) ||
               Physics.Raycast(transform.position, transform.right, wallDetectionDistance, wallLayer) ||
               Physics.Raycast(transform.position, -transform.right, wallDetectionDistance, wallLayer);
    }

    private Vector3 GetWallNormal()
    {
        // Raycast to detect walls and return the wall normal
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, wallDetectionDistance, wallLayer))
            return hit.normal;
        if (Physics.Raycast(transform.position, transform.right, out hit, wallDetectionDistance, wallLayer))
            return hit.normal;
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

            // Apply the reduced velocity to the controller
            controller.Move(wallJumpDirection * Time.deltaTime);

            // Stop wall jump if the direction velocity is negligible
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
