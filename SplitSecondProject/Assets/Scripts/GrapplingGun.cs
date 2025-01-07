using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrappleGun : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float maxGrappleDistance = 30f;
    public float grappleSpeed = 10f;
    public LayerMask grappleLayer;     // Objects you can grapple
    public float swingDampening = 5f;  // Higher = more dampened swinging

    [Header("References")]
    public Transform player;
    public Camera playerCamera;

    private LineRenderer lineRenderer;
    private Vector3 grapplePoint;
    private bool isGrappling;
    private CharacterController characterController;
    private Vector3 playerVelocity;
    private Vector3 swingDirection;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        characterController = player.GetComponent<CharacterController>();
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) StartGrapple();
        else if (Input.GetMouseButtonUp(0)) EndGrapple();

        if (isGrappling) UpdateGrapple();
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, maxGrappleDistance, grappleLayer))
        {
            isGrappling = true;
            grapplePoint = hit.point;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }

    void UpdateGrapple()
    {
        // Continually update line positions
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, grapplePoint);

        Vector3 directionToGrapple = grapplePoint - player.position;
        float distance = directionToGrapple.magnitude;

        // Pull or 'swing' the player toward the grapple point
        if (distance > 1f)
        {
            directionToGrapple.Normalize();

            // Basic swinging force
            swingDirection = Vector3.Lerp(swingDirection, directionToGrapple, Time.deltaTime * swingDampening);

            // Move the player
            characterController.Move(swingDirection * grappleSpeed * Time.deltaTime);
        }
        else
        {
            // If close enough, stop grappling
            EndGrapple();
        }
    }

    void EndGrapple()
    {
        isGrappling = false;
        lineRenderer.positionCount = 0;
        swingDirection = Vector3.zero;
    }
}
