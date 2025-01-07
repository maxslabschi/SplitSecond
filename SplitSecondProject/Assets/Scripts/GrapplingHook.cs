using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform gunTip, camera, player;
    public LayerMask grappleableMask;
    public float maxDistance = 20f;
    public float grappleSpeed = 10f;

    private Vector3 grapplePoint;
    private bool isGrappling;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click for grappling
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }

        if (isGrappling)
        {
            Vector3 grappleDirection = (grapplePoint - player.position).normalized;
            player.GetComponent<CharacterController>().Move(grappleDirection * grappleSpeed * Time.deltaTime);
        }
    }

    private void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, grappleableMask))
        {
            grapplePoint = hit.point;
            isGrappling = true;
            lineRenderer.SetPosition(0, gunTip.position);
            lineRenderer.SetPosition(1, grapplePoint);
            lineRenderer.enabled = true;
        }
    }

    private void StopGrapple()
    {
        isGrappling = false;
        lineRenderer.enabled = false;
    }
}