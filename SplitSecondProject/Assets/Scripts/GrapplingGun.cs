using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrappleGun : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float maxGrappleDistance = 30f;
    public float grappleForce = 25f;
    public float upwardForceMultiplier = 1.5f;
    public float minGrappleSpeed = 10f;
    public float maxGrappleSpeed = 40f;
    public LayerMask grappleLayer;
    public float gravityWhileGrappling = -5f;
    
    [Header("Swing Settings")]
    public float swingForce = 40f;
    public float swingDampening = 3f;
    public float momentumMultiplier = 1.3f;
    public float minSwingAngle = 20f;
    public float directionSmoothTime = 0.1f;
    
    [Header("References")]
    public Transform player;
    public Camera playerCamera;

    private LineRenderer lineRenderer;
    private Vector3 grapplePoint;
    private bool isGrappling;
    private CharacterController characterController;
    private Vector3 swingVelocity;
    private Vector3 lastFrameVelocity;
    private PlayerMovement playerMovement;
    private float currentGrappleSpeed;
    private Vector3 currentVelocityDirection;
    private Vector3 velocityDirectionVelocity;

    // Variables for better mid-air transition handling
    private float timeSinceLastGrapple;
    private const float MIN_GRAPPLE_INTERVAL = 0.1f; // Minimum time between grapples
    private bool isTransitioningGrapple;
    private float transitionTimer;
    private const float TRANSITION_DURATION = 0.15f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        characterController = player.GetComponent<CharacterController>();
        playerMovement = player.GetComponent<PlayerMovement>();
        lineRenderer.positionCount = 0;
        ResetGrappleState();
    }

    void ResetGrappleState()
    {
        swingVelocity = Vector3.zero;
        currentGrappleSpeed = minGrappleSpeed;
        velocityDirectionVelocity = Vector3.zero;
        isTransitioningGrapple = false;
        transitionTimer = 0f;
    }

    void Update()
    {
        timeSinceLastGrapple += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && timeSinceLastGrapple >= MIN_GRAPPLE_INTERVAL)
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndGrapple();
        }

        if (isGrappling)
        {
            UpdateGrapple();
        }

        if (isTransitioningGrapple)
        {
            transitionTimer += Time.deltaTime;
            if (transitionTimer >= TRANSITION_DURATION)
            {
                isTransitioningGrapple = false;
            }
        }
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, maxGrappleDistance, grappleLayer))
        {
            // If already grappling, handle transition
            if (isGrappling)
            {
                isTransitioningGrapple = true;
                transitionTimer = 0f;
                
                // Preserve some of the current momentum for smooth transition
                Vector3 currentVel = lastFrameVelocity;
                currentVel.y = Mathf.Max(currentVel.y, 0); // Prevent downward momentum transfer
                lastFrameVelocity = currentVel * 0.5f; // Reduce carried momentum
            }
            else
            {
                ResetGrappleState();
            }

            isGrappling = true;
            grapplePoint = hit.point;
            lineRenderer.positionCount = 2;
            
            // Initialize movement direction
            Vector3 directionToGrapple = (grapplePoint - player.position).normalized;
            currentVelocityDirection = directionToGrapple;
            
            timeSinceLastGrapple = 0f;
        }
    }

    void UpdateGrapple()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, grapplePoint);

        Vector3 directionToGrapple = grapplePoint - player.position;
        float distance = directionToGrapple.magnitude;

        if (distance > 1f)
        {
            Vector3 normalizedDirection = directionToGrapple.normalized;
            
            // Calculate pull force
            float pullForce = Mathf.Lerp(grappleForce, grappleForce * 0.5f, 1 - (distance / maxGrappleDistance));
            
            // Upward boost calculation
            if (player.position.y < grapplePoint.y)
            {
                float heightDifference = grapplePoint.y - player.position.y;
                float verticalBoost = Mathf.Clamp01(heightDifference / maxGrappleDistance);
                pullForce *= (1 + verticalBoost * upwardForceMultiplier);
            }

            // Calculate swing
            Vector3 cross = Vector3.Cross(normalizedDirection, Vector3.up);
            float horizontalInput = Input.GetAxis("Horizontal");
            float swingAngle = Vector3.Angle(directionToGrapple, Vector3.up);
            float swingMultiplier = Mathf.Max(0, (swingAngle - minSwingAngle) / 90f);

            Vector3 swingForceVector = cross * horizontalInput * swingForce * swingMultiplier;
            swingVelocity = Vector3.Lerp(swingVelocity + (swingForceVector * Time.deltaTime), Vector3.zero, Time.deltaTime * swingDampening);
            
            // Calculate movement direction
            Vector3 pullVelocity = normalizedDirection * pullForce;
            Vector3 desiredDirection = (pullVelocity + swingVelocity).normalized;
            
            // Smooth direction changes
            currentVelocityDirection = Vector3.SmoothDamp(
                currentVelocityDirection, 
                desiredDirection, 
                ref velocityDirectionVelocity, 
                isTransitioningGrapple ? directionSmoothTime * 2f : directionSmoothTime
            );
            
            // Calculate speed
            currentGrappleSpeed = Mathf.MoveTowards(currentGrappleSpeed, maxGrappleSpeed, Time.deltaTime * pullForce);
            Vector3 finalVelocity = currentVelocityDirection * currentGrappleSpeed;

            // Gravity handling:
            // Apply gravity only if we're not transitioning
            if (!isTransitioningGrapple)
            {
                // Reset gravity if transitioning between grapples
                if (timeSinceLastGrapple < MIN_GRAPPLE_INTERVAL)
                {
                    finalVelocity.y += gravityWhileGrappling * Time.deltaTime;
                }
            }

            // Move character
            characterController.Move(finalVelocity * Time.deltaTime);
            lastFrameVelocity = finalVelocity;
        }
        else
        {
            EndGrapple();
        }
    }


    void EndGrapple()
    {
        if (!isGrappling) return;

        isGrappling = false;
        lineRenderer.positionCount = 0;
        
        if (playerMovement != null && !isTransitioningGrapple)
        {
            Vector3 preservedVelocity = lastFrameVelocity * momentumMultiplier;
            
            // Maintain horizontal momentum
            float horizontalSpeed = new Vector2(preservedVelocity.x, preservedVelocity.z).magnitude;
            if (horizontalSpeed > 0.1f)
            {
                Vector3 horizontalDir = new Vector3(preservedVelocity.x, 0, preservedVelocity.z).normalized;
                preservedVelocity = horizontalDir * horizontalSpeed + Vector3.up * preservedVelocity.y;
            }
            
            // Ensure reasonable vertical velocity
            preservedVelocity.y = Mathf.Clamp(preservedVelocity.y, -maxGrappleSpeed * 0.3f, maxGrappleSpeed * 0.5f);
            
            playerMovement.SetExternalVelocity(preservedVelocity);
        }
    }
}