using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float airControlMultiplier = 8f;

    [Header("Player Height Settings")]
    public float standingHeight = 2f;
    public float slidingHeight = 1f;
    public float heightTransitionSpeed = 8f;

    [Header("Wall Jump Settings")]
    public float wallJumpForce = 5f;
    public float wallDetectionDistance = 1f;
    public LayerMask wallLayer;
    public float wallJumpFriction = 5f;
    [SerializeField] private float cooldownTime;
    private float nextJumpTime;

    [Header("Sliding Settings")]
    public float slideSpeed = 14f;
    public float maxSlideTime = 1.0f;
    public float slideSpeedboost = 1f;

    [Header("Respawn Settings")]
    public float fallThreshold = -10f;
    public Transform respawnPoint;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isWallJumping;
    private bool isSliding;
    private float currentSpeed;
    private float targetHeight;
    private Vector3 wallJumpDirection;
    private float slideTimer;

    public bool IsCoolingDown => Time.time > nextJumpTime;
    public void StartCooldown() => nextJumpTime = Time.time + cooldownTime;

    public float CurrentSpeed => currentSpeed;
    public bool IsSliding => isSliding;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        targetHeight = standingHeight;
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        HandleMovement();
        ApplyWallJumpFriction();
        AdjustHeight();
        CheckFallAndRespawn();
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f) velocity.y = -2f;

        if (isGrounded)
        {
            isWallJumping = false;
            wallJumpDirection = Vector3.zero;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        if (Input.GetKeyDown(KeyCode.C) && !isSliding && isGrounded && move.magnitude > 0.1f && currentSpeed == sprintSpeed) StartSlide();
        if ((Input.GetKeyUp(KeyCode.C) && isSliding) || (isSliding && slideTimer <= 0f)) EndSlide();

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            controller.Move(move.normalized * slideSpeed * Time.deltaTime * slideSpeedboost);
        }
        else
        {
            if (isGrounded)
            {
                if (Input.GetKey(KeyCode.LeftShift) && move.magnitude > 0.1f)
                {
                    currentSpeed = sprintSpeed;
                    targetHeight = standingHeight;
                }
                else
                {
                    currentSpeed = walkSpeed;
                    targetHeight = standingHeight;
                }
                controller.Move(move * currentSpeed * Time.deltaTime);
            }
            else
            {
                Vector3 airMove = move * airControlMultiplier;
                controller.Move(airMove * Time.deltaTime);
            }
        }

        if (IsCoolingDown && !isGrounded && Input.GetButtonDown("Jump") && IsNearWall())
        {
            PerformWallJump();
            StartCooldown();
        }
        else if (Input.GetButtonDown("Jump") && isGrounded && !isSliding) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = maxSlideTime;
        targetHeight = slidingHeight;
        velocity.y = -2f;
    }

    void EndSlide()
    {
        isSliding = false;
        targetHeight = standingHeight;
    }

    bool IsNearWall()
    {
        return Physics.Raycast(transform.position, transform.forward, wallDetectionDistance, wallLayer) ||
               Physics.Raycast(transform.position, transform.right, wallDetectionDistance, wallLayer) ||
               Physics.Raycast(transform.position, -transform.right, wallDetectionDistance, wallLayer);
    }

    void PerformWallJump()
    {
        Vector3 wallNormal = GetWallNormal();
        if (wallNormal != Vector3.zero)
        {
            wallJumpDirection = wallNormal * wallJumpForce;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isWallJumping = true;
        }
    }

    Vector3 GetWallNormal()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, wallDetectionDistance, wallLayer)) return hit.normal;
        if (Physics.Raycast(transform.position, transform.right, out hit, wallDetectionDistance, wallLayer)) return hit.normal;
        if (Physics.Raycast(transform.position, -transform.right, out hit, wallDetectionDistance, wallLayer)) return hit.normal;
        return Vector3.zero;
    }

    void ApplyWallJumpFriction()
    {
        if (isWallJumping)
        {
            wallJumpDirection = Vector3.Lerp(wallJumpDirection, Vector3.zero, Time.deltaTime * wallJumpFriction);
            controller.Move(wallJumpDirection * Time.deltaTime);
            if (wallJumpDirection.magnitude < 0.1f)
            {
                wallJumpDirection = Vector3.zero;
                isWallJumping = false;
            }
        }
    }

    void AdjustHeight()
    {
        float newHeight = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * heightTransitionSpeed);
        controller.height = newHeight;
        controller.center = new Vector3(0, newHeight / 2f, 0);
    }

    void CheckFallAndRespawn()
    {
        if (transform.position.y < fallThreshold) Respawn();
    }

    void Respawn()
    {
        if (respawnPoint != null)
        {
            controller.enabled = false;
            transform.position = respawnPoint.position;
            controller.enabled = true;
            velocity = Vector3.zero;
            wallJumpDirection = Vector3.zero;
            isWallJumping = false;
            Debug.Log("Player respawned.");
        }
        else
        {
            Debug.LogError("Respawn Point not set!");
        }
    }
}
